using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface ILoyaltyPointService
{
    Task<ApiResponse<LoyaltyPointSummaryDto>> GetUserLoyaltyPointSummaryAsync(string userId);
    Task<ApiResponse<List<LoyaltyPointTransactionDto>>> GetUserTransactionsAsync(string userId, int page = 1, int pageSize = 10);
    Task<ApiResponse<LoyaltyPointTransactionDto>> AwardPointsAsync(AwardPointsDto awardDto);
    Task<ApiResponse<LoyaltyPointTransactionDto>> RedeemPointsAsync(string userId, RedeemPointsDto redeemDto);
    Task<ApiResponse<bool>> ProcessBookingPointsAsync(string userId, int bookingId, decimal bookingAmount);
    Task<ApiResponse<bool>> ExpireOldPointsAsync();
    int CalculatePointsForAmount(decimal amount);
    decimal CalculateDiscountForPoints(int points);
} 