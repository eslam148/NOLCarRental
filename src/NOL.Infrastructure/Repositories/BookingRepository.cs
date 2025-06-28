using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Booking>> GetUserBookingsAsync(string userId)
    {
        return await _context.Bookings
            .Include(b => b.Car)
            .ThenInclude(c => c.Category)
            .Include(b => b.Car)
            .ThenInclude(c => c.Branch)
            .Include(b => b.BookingExtras)
            .ThenInclude(be => be.ExtraTypePrice)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Booking?> GetUserBookingByIdAsync(int bookingId, string userId)
    {
        return await _context.Bookings
            .Include(b => b.Car)
            .ThenInclude(c => c.Category)
            .Include(b => b.Car)
            .ThenInclude(c => c.Branch)
            .Include(b => b.BookingExtras)
            .ThenInclude(be => be.ExtraTypePrice)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
    }

    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate)
    {
        var car = await _context.Cars.FindAsync(carId);
        if (car == null || car.Status != CarStatus.Available)
            return false;

        // Check for overlapping bookings
        var hasOverlappingBookings = await _context.Bookings
            .AnyAsync(b => b.CarId == carId && 
                     b.Status != BookingStatus.Canceled &&
                     ((b.StartDate <= startDate && b.EndDate > startDate) ||
                      (b.StartDate < endDate && b.EndDate >= endDate) ||
                      (b.StartDate >= startDate && b.EndDate <= endDate)));

        return !hasOverlappingBookings;
    }

    public async Task<Car?> GetCarByIdAsync(int carId)
    {
        return await _context.Cars.FindAsync(carId);
    }

    public async Task<List<ExtraTypePrice>> GetExtraTypePricesByIdsAsync(List<int> ids)
    {
        return await _context.ExtraTypePrices
            .Where(etp => ids.Contains(etp.Id))
            .ToListAsync();
    }

    public async Task AddBookingExtrasAsync(List<BookingExtra> bookingExtras)
    {
        _context.BookingExtras.AddRange(bookingExtras);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateBookingAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }
} 