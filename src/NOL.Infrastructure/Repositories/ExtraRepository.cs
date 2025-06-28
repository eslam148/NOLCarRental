using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class ExtraRepository : Repository<ExtraTypePrice>, IExtraRepository
{
    public ExtraRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExtraTypePrice>> GetExtrasByTypeAsync(ExtraType type)
    {
        return await _dbSet
            .Where(e => e.ExtraType == type)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExtraTypePrice>> GetActiveExtrasAsync()
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .ToListAsync();
    }
} 