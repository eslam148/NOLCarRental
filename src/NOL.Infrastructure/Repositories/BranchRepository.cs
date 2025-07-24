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
} 