using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.Application.Common.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<List<ReviewDto>>> GetReviewsByCarIdAsync(int carId);
    Task<ApiResponse<List<ReviewDto>>> GetReviewsByUserIdAsync(string userId);
    Task<ApiResponse<ReviewDto>> GetReviewByIdAsync(int id);
    Task<ApiResponse<CarRatingDto>> GetCarRatingAsync(int carId);
    Task<ApiResponse<ReviewDto>> CreateReviewAsync(string userId, CreateReviewDto createReviewDto);
    Task<ApiResponse<ReviewDto>> UpdateReviewAsync(int id, string userId, UpdateReviewDto updateReviewDto);
    Task<ApiResponse<bool>> DeleteReviewAsync(int id, string userId);
    Task<ApiResponse<bool>> CanUserReviewCarAsync(string userId, int carId);
}
