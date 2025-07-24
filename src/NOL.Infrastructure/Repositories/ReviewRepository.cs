using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Review>> GetReviewsByCarIdAsync(int carId)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Car)
            .Where(r => r.CarId == carId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Car)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetReviewByUserAndCarAsync(string userId, int carId)
    {
        return await _dbSet
            .Include(r => r.User)
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CarId == carId);
    }

    public async Task<decimal> GetAverageRatingByCarIdAsync(int carId)
    {
        var reviews = await _dbSet
            .Where(r => r.CarId == carId)
            .ToListAsync();

        if (!reviews.Any())
            return 0;

        return Math.Round((decimal)reviews.Average(r => r.Rating), 2);
    }

    public async Task<int> GetTotalReviewsByCarIdAsync(int carId)
    {
        return await _dbSet
            .CountAsync(r => r.CarId == carId);
    }

    public async Task<bool> HasUserReviewedCarAsync(string userId, int carId)
    {
        return await _dbSet
            .AnyAsync(r => r.UserId == userId && r.CarId == carId);
    }

    public async Task<bool> CanUserReviewCarAsync(string userId, int carId)
    {
        // Check if user has completed a booking for this car
        var hasCompletedBooking = await _context.Bookings
            .AnyAsync(b => b.UserId == userId && 
                          b.CarId == carId && 
                          b.Status == BookingStatus.Completed);

        // Check if user hasn't already reviewed this car
        var hasNotReviewed = !await HasUserReviewedCarAsync(userId, carId);

        return hasCompletedBooking && hasNotReviewed;
    }

    public async Task UpdateCarRatingAsync(int carId)
    {
        var car = await _context.Cars.FindAsync(carId);
        if (car != null)
        {
            car.AverageRating = await GetAverageRatingByCarIdAsync(carId);
            car.TotalReviews = await GetTotalReviewsByCarIdAsync(carId);
            car.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }
    }
}
