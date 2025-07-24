using NOL.Domain.Entities;

namespace NOL.Application.Common.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetReviewsByCarIdAsync(int carId);
    Task<IEnumerable<Review>> GetReviewsByUserIdAsync(string userId);
    Task<Review?> GetReviewByUserAndCarAsync(string userId, int carId);
    Task<decimal> GetAverageRatingByCarIdAsync(int carId);
    Task<int> GetTotalReviewsByCarIdAsync(int carId);
    Task<bool> HasUserReviewedCarAsync(string userId, int carId);
    Task<bool> CanUserReviewCarAsync(string userId, int carId);
    Task UpdateCarRatingAsync(int carId);
}
