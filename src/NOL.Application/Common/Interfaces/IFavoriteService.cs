using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.Application.Common.Interfaces;

public interface IFavoriteService
{
    Task<ApiResponse<List<FavoriteDto>>> GetUserFavoritesAsync(string userId);
    Task<ApiResponse<FavoriteDto>> AddToFavoritesAsync(string userId, AddFavoriteDto addFavoriteDto);
    Task<ApiResponse<bool>> RemoveFromFavoritesAsync(string userId, int carId);
    Task<ApiResponse<bool>> IsFavoriteAsync(string userId, int carId);
} 