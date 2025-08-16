using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Branch>> GetActiveBranchesAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive)
            .OrderBy(b => b.NameEn)
            .ToListAsync();
    }

    public async Task<IEnumerable<Branch>> GetActiveBranchesPagedAsync(int page, int pageSize)
    {
        return await _dbSet
            .Where(b => b.IsActive)
            .OrderBy(b => b.NameEn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetActiveBranchesCountAsync()
    {
        return await _dbSet
            .Where(b => b.IsActive)
            .CountAsync();
    }

    public async Task<Branch?> GetActiveBranchByIdAsync(int id)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
    }

    public async Task<IEnumerable<Branch>> GetBranchesByCountryAsync(string country)
    {
        return await _dbSet
            .Where(b => b.IsActive && b.Country.ToLower() == country.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Branch>> GetBranchesByCityAsync(string city)
    {
        return await _dbSet
            .Where(b => b.IsActive && b.City.ToLower() == city.ToLower())
            .ToListAsync();
    }

    public async Task<IEnumerable<Branch>> GetActiveBranchesNearbyAsync(decimal latitude, decimal longitude, double radiusKm)
    {
        // Get all active branches and filter by distance in memory
        // For better performance with large datasets, consider using spatial queries
        var allBranches = await _dbSet
            .Where(b => b.IsActive)
            .ToListAsync();

        // Calculate distance once per branch, filter by radius, and sort by distance (nearest first)
        var nearbyBranches = allBranches
            .Select(b => new {
                Branch = b,
                Distance = CalculateDistance((double)latitude, (double)longitude, (double)b.Latitude, (double)b.Longitude)
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance) // Sort by distance - nearest first
            .Select(x => x.Branch)
            .ToList();

        return nearbyBranches;
    }

    public async Task<int> GetActiveBranchesNearbyCountAsync(decimal latitude, decimal longitude, double radiusKm)
    {
        var nearbyBranches = await GetActiveBranchesNearbyAsync(latitude, longitude, radiusKm);
        return nearbyBranches.Count();
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula to calculate distance between two points on Earth
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c;

        return distance;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}