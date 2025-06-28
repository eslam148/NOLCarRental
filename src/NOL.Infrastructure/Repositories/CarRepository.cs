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

    public async Task<IEnumerable<Car>> GetCarsAsync(string? sortByCost = null, int page = 1, int pageSize = 10)
    {
        var query = _dbSet.AsQueryable();

        // Include related entities
        query = query
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .Where(c => c.IsActive);

        // Apply cost sorting
        if (!string.IsNullOrEmpty(sortByCost))
        {
            if (sortByCost.ToLower() == "asc")
                query = query.OrderBy(c => c.DailyRate);
            else if (sortByCost.ToLower() == "desc")
                query = query.OrderByDescending(c => c.DailyRate);
        }
        else
        {
            // Default sorting by ID if no sort specified
            query = query.OrderBy(c => c.Id);
        }

        // Apply pagination
        return await query
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