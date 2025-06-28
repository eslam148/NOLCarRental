using NOL.Domain.Enums;

namespace NOL.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? TransactionId { get; set; }
    public string? PaymentGatewayResponse { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int BookingId { get; set; }

    // Navigation Properties
    public virtual Booking Booking { get; set; } = null!;
} 