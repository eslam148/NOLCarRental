using NOL.Domain.Enums;

namespace NOL.Domain.Entities;

public class LoyaltyPointTransaction
{
    public int Id { get; set; }
    public int Points { get; set; }
    public LoyaltyPointTransactionType TransactionType { get; set; }
    public LoyaltyPointEarnReason? EarnReason { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public string UserId { get; set; } = string.Empty;
    public int? BookingId { get; set; }

    // Navigation Properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Booking? Booking { get; set; }
} 