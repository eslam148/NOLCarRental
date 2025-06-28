using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetCategoriesOrderedAsync()
    {
        return await _dbSet
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }
} 