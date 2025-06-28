using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class AdvertisementRepository : IAdvertisementRepository
{
    private readonly ApplicationDbContext _context;

    public AdvertisementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Advertisement>> GetActiveAdvertisementsAsync()
    {
        return await _context.Advertisements
            .Include(a => a.Car)
            .Include(a => a.Category)
            .Where(a => a.IsActive && 
                       a.Status == AdvertisementStatus.Active &&
                       a.StartDate <= DateTime.UtcNow &&
                       a.EndDate >= DateTime.UtcNow)
            .OrderBy(a => a.SortOrder)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Advertisement>> GetAdvertisementsByCarIdAsync(int carId)
    {
        return await _context.Advertisements
            .Include(a => a.Car)
            .Include(a => a.Category)
            .Where(a => a.CarId == carId && 
                       a.IsActive && 
                       a.Status == AdvertisementStatus.Active &&
                       a.StartDate <= DateTime.UtcNow &&
                       a.EndDate >= DateTime.UtcNow)
            .OrderBy(a => a.SortOrder)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Advertisement>> GetAdvertisementsByCategoryIdAsync(int categoryId)
    {
        return await _context.Advertisements
            .Include(a => a.Car)
            .Include(a => a.Category)
            .Where(a => a.CategoryId == categoryId && 
                       a.IsActive && 
                       a.Status == AdvertisementStatus.Active &&
                       a.StartDate <= DateTime.UtcNow &&
                       a.EndDate >= DateTime.UtcNow)
            .OrderBy(a => a.SortOrder)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Advertisement>> GetFeaturedAdvertisementsAsync()
    {
        return await _context.Advertisements
            .Include(a => a.Car)
            .Include(a => a.Category)
            .Where(a => a.IsFeatured && 
                       a.IsActive && 
                       a.Status == AdvertisementStatus.Active &&
                       a.StartDate <= DateTime.UtcNow &&
                       a.EndDate >= DateTime.UtcNow)
            .OrderBy(a => a.SortOrder)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Advertisement?> GetAdvertisementByIdAsync(int id)
    {
        return await _context.Advertisements
            .Include(a => a.Car)
            .Include(a => a.Category)
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Advertisement> CreateAdvertisementAsync(Advertisement advertisement)
    {
        _context.Advertisements.Add(advertisement);
        await _context.SaveChangesAsync();
        return advertisement;
    }

    public async Task<Advertisement> UpdateAdvertisementAsync(Advertisement advertisement)
    {
        advertisement.UpdatedAt = DateTime.UtcNow;
        _context.Advertisements.Update(advertisement);
        await _context.SaveChangesAsync();
        return advertisement;
    }

    public async Task<bool> DeleteAdvertisementAsync(int id)
    {
        var advertisement = await _context.Advertisements.FindAsync(id);
        if (advertisement == null)
            return false;

        _context.Advertisements.Remove(advertisement);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementViewCountAsync(int id)
    {
        var advertisement = await _context.Advertisements.FindAsync(id);
        if (advertisement == null)
            return false;

        advertisement.ViewCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementClickCountAsync(int id)
    {
        var advertisement = await _context.Advertisements.FindAsync(id);
        if (advertisement == null)
            return false;

        advertisement.ClickCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Advertisement>> GetExpiredAdvertisementsAsync()
    {
        return await _context.Advertisements
            .Where(a => a.Status == AdvertisementStatus.Active && 
                       a.EndDate < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<bool> UpdateAdvertisementStatusAsync(int id, AdvertisementStatus status)
    {
        var advertisement = await _context.Advertisements.FindAsync(id);
        if (advertisement == null)
            return false;

        advertisement.Status = status;
        advertisement.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
} 