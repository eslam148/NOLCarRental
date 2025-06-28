using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface IAdvertisementRepository
{
    Task<List<Advertisement>> GetActiveAdvertisementsAsync();
    Task<List<Advertisement>> GetAdvertisementsByCarIdAsync(int carId);
    Task<List<Advertisement>> GetAdvertisementsByCategoryIdAsync(int categoryId);
    Task<List<Advertisement>> GetFeaturedAdvertisementsAsync();
    Task<Advertisement?> GetAdvertisementByIdAsync(int id);
    Task<Advertisement> CreateAdvertisementAsync(Advertisement advertisement);
    Task<Advertisement> UpdateAdvertisementAsync(Advertisement advertisement);
    Task<bool> DeleteAdvertisementAsync(int id);
    Task<bool> IncrementViewCountAsync(int id);
    Task<bool> IncrementClickCountAsync(int id);
    Task<List<Advertisement>> GetExpiredAdvertisementsAsync();
    Task<bool> UpdateAdvertisementStatusAsync(int id, AdvertisementStatus status);
} 