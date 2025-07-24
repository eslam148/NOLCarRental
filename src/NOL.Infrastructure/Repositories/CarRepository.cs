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

    public async Task<IEnumerable<Car>> GetCarsAsync(string? sortByCost = null, int page = 1, int pageSize = 10, string? brand = null)
    {
        var query = _dbSet.AsQueryable();

        // Include related entities
        query = query
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .Include(c=>c.Reviews)
            .Where(c => c.IsActive);

        // Apply brand filtering (search in both Arabic and English brand names)
        if (!string.IsNullOrEmpty(brand))
        {
            query = query.Where(c => 
                c.BrandAr.Contains(brand) || 
                c.BrandEn.Contains(brand));
        }

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

    // Search operations
    public async Task<IEnumerable<Car>> SearchCarsAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        var query = _dbSet
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .Where(c => c.IsActive);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(c =>
                c.BrandAr.ToLower().Contains(lowerSearchTerm) ||
                c.BrandEn.ToLower().Contains(lowerSearchTerm) ||
                c.ModelAr.ToLower().Contains(lowerSearchTerm) ||
                c.ModelEn.ToLower().Contains(lowerSearchTerm) ||
                c.PlateNumber.ToLower().Contains(lowerSearchTerm));
        }

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> IsPlateNumberUniqueAsync(string plateNumber, int? excludeCarId = null)
    {
        var query = _dbSet.Where(c => c.PlateNumber == plateNumber);

        if (excludeCarId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCarId.Value);
        }

        return !await query.AnyAsync();
    }

    // Rate management operations
    public async Task<bool> UpdateCarRatesAsync(int carId, decimal dailyRate, decimal weeklyRate, decimal monthlyRate)
    {
        var car = await _dbSet.FindAsync(carId);
        if (car == null) return false;

        car.DailyRate = dailyRate;
        car.WeeklyRate = weeklyRate;
        car.MonthlyRate = monthlyRate;
        car.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Car>> GetCarsWithRatesAsync(int page = 1, int pageSize = 10)
    {
        return await _dbSet
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .Where(c => c.IsActive)
            .OrderBy(c => c.BrandEn)
            .ThenBy(c => c.ModelEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> BulkUpdateRatesAsync(Dictionary<int, (decimal daily, decimal weekly, decimal monthly)> rateUpdates)
    {
        var carIds = rateUpdates.Keys.ToList();
        var cars = await _dbSet.Where(c => carIds.Contains(c.Id)).ToListAsync();

        foreach (var car in cars)
        {
            if (rateUpdates.TryGetValue(car.Id, out var rates))
            {
                car.DailyRate = rates.daily;
                car.WeeklyRate = rates.weekly;
                car.MonthlyRate = rates.monthly;
                car.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // Status management
    public async Task<bool> UpdateCarStatusAsync(int carId, CarStatus status)
    {
        var car = await _dbSet.FindAsync(carId);
        if (car == null) return false;

        car.Status = status;
        car.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Car>> GetCarsByStatusAsync(CarStatus status)
    {
        return await _dbSet
            .Include(c => c.Category)
            .Include(c => c.Branch)
            .Where(c => c.Status == status && c.IsActive)
            .ToListAsync();
    }

    // Validation operations
    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(c => c.Id == id && c.IsActive);
    }

    public async Task<bool> IsCategoryValidAsync(int categoryId)
    {
        return await _context.Categories.AnyAsync(c => c.Id == categoryId && c.IsActive);
    }

    public async Task<bool> IsBranchValidAsync(int branchId)
    {
        return await _context.Branches.AnyAsync(b => b.Id == branchId && b.IsActive);
    }
}