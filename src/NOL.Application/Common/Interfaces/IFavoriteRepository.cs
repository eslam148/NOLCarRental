using NOL.Domain.Entities;

namespace NOL.Application.Common.Interfaces;

public interface IFavoriteRepository : IRepository<Favorite>
{
    Task<IEnumerable<Favorite>> GetUserFavoritesAsync(string userId);
    Task<IEnumerable<Favorite>> GetUserFavoritesPagedAsync(string userId, int page, int pageSize);
    Task<int> GetUserFavoritesCountAsync(string userId);
    Task<Favorite?> GetFavoriteAsync(string userId, int carId);
    Task<bool> IsFavoriteAsync(string userId, int carId);
    Task RemoveFavoriteAsync(string userId, int carId);
}