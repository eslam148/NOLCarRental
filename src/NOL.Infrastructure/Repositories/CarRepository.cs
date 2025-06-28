using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class CarRepository : Repository<Car>, ICarRepository
{
    public CarRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Car>> GetCarsAsync(CarStatus? status = null, int? categoryId = null, int page = 1, int pageSize = 10)
    {
        var query = _dbSet.AsQueryable();

        // Apply filters
        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (categoryId.HasValue)
            query = query.Where(c => c.CategoryId == categoryId.Value);

        // Include related entities and apply pagination
        return await query
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Car?> GetCarWithIncludesAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Car>> GetCarsWithIncludesAsync()
    {
        return await _dbSet
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .ToListAsync();
    }

    public async Task<IEnumerable<Car>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate)
    {
        // Get cars that are not booked during the specified period
        var bookedCarIds = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed &&
                       ((b.StartDate <= endDate && b.EndDate >= startDate)))
            .Select(b => b.CarId)
            .ToListAsync();

        return await _dbSet
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .Where(c => c.Status == CarStatus.Available && !bookedCarIds.Contains(c.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Car>> GetCarsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .Where(c => c.CategoryId == categoryId && c.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Car>> GetCarsByBranchAsync(int branchId)
    {
        return await _dbSet
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .Where(c => c.BranchId == branchId && c.IsActive)
            .ToListAsync();
    }
} 