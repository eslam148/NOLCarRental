using NOL.Domain.Enums;

namespace NOL.Application.DTOs;

public class LoyaltyPointTransactionDto
{
    public int Id { get; set; }
    public int Points { get; set; }
    public LoyaltyPointTransactionType TransactionType { get; set; }
    public LoyaltyPointEarnReason? EarnReason { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
    public int? BookingId { get; set; }
    public string? BookingNumber { get; set; }
}

public class LoyaltyPointSummaryDto
{
    public int TotalLoyaltyPoints { get; set; }
    public int AvailableLoyaltyPoints { get; set; }
    public int LifetimePointsEarned { get; set; }
    public int LifetimePointsRedeemed { get; set; }
    public DateTime? LastPointsEarnedDate { get; set; }
    public int PointsExpiringIn30Days { get; set; }
    public List<LoyaltyPointTransactionDto> RecentTransactions { get; set; } = new();
}

public class RedeemPointsDto
{
    public int PointsToRedeem { get; set; }
    public int? BookingId { get; set; }
    public string? Description { get; set; }
}

public class AwardPointsDto
{
    public string UserId { get; set; } = string.Empty;
    public int Points { get; set; }
    public LoyaltyPointEarnReason EarnReason { get; set; }
    public string? Description { get; set; }
    public int? BookingId { get; set; }
    public DateTime? ExpiryDate { get; set; }
} 