using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface IAdvertisementService
{
    Task<ApiResponse<List<AdvertisementDto>>> GetActiveAdvertisementsAsync();
    Task<ApiResponse<List<AdvertisementDto>>> GetAdvertisementsByCarIdAsync(int carId);
    Task<ApiResponse<List<AdvertisementDto>>> GetAdvertisementsByCategoryIdAsync(int categoryId);
    Task<ApiResponse<List<AdvertisementDto>>> GetFeaturedAdvertisementsAsync();
    Task<ApiResponse<AdvertisementDto>> GetAdvertisementByIdAsync(int id);
    Task<ApiResponse<AdvertisementDto>> CreateAdvertisementAsync(CreateAdvertisementDto createDto, string userId);
    Task<ApiResponse<AdvertisementDto>> UpdateAdvertisementAsync(int id, UpdateAdvertisementDto updateDto);
    Task<ApiResponse<bool>> DeleteAdvertisementAsync(int id);
    Task<ApiResponse<bool>> IncrementViewCountAsync(int id);
    Task<ApiResponse<bool>> IncrementClickCountAsync(int id);
    Task<ApiResponse<bool>> UpdateAdvertisementStatusAsync(int id, AdvertisementStatus status);
} 