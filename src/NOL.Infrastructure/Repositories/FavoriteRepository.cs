using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class FavoriteRepository : Repository<Favorite>, IFavoriteRepository
{
    public FavoriteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Favorite>> GetUserFavoritesAsync(string userId)
    {
        return await _dbSet
            .Include(f => f.Car)
                .ThenInclude(c => c.Category)
            .Include(f => f.Car)
                .ThenInclude(c => c.Branch)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<Favorite?> GetFavoriteAsync(string userId, int carId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.UserId == userId && f.CarId == carId);
    }

    public async Task<bool> IsFavoriteAsync(string userId, int carId)
    {
        return await _dbSet
            .AnyAsync(f => f.UserId == userId && f.CarId == carId);
    }

    public async Task RemoveFavoriteAsync(string userId, int carId)
    {
        var favorite = await GetFavoriteAsync(userId, carId);
        if (favorite != null)
        {
            _dbSet.Remove(favorite);
            await _context.SaveChangesAsync();
        }
    }
} 