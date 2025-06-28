using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface ILoyaltyPointRepository
{
    Task<List<LoyaltyPointTransaction>> GetUserTransactionsAsync(string userId, int page = 1, int pageSize = 10);
    Task<LoyaltyPointTransaction> CreateTransactionAsync(LoyaltyPointTransaction transaction);
    Task<int> GetUserAvailablePointsAsync(string userId);
    Task<int> GetUserTotalPointsAsync(string userId);
    Task<List<LoyaltyPointTransaction>> GetExpiringPointsAsync(string userId, DateTime beforeDate);
    Task<LoyaltyPointTransaction?> GetTransactionByIdAsync(int id);
    Task ExpirePointsAsync(List<int> transactionIds);
    Task<bool> HasUserEarnedPointsForBookingAsync(string userId, int bookingId);
    Task UpdateUserLoyaltyPointsAsync(string userId, int availablePoints, int totalPoints, int lifetimeEarned, int lifetimeRedeemed);
} 