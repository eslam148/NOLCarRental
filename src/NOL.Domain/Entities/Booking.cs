using NOL.Domain.Enums;

namespace NOL.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public decimal CarRentalCost { get; set; }
    public decimal ExtrasCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Open;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public string UserId { get; set; } = string.Empty;
    public int CarId { get; set; }

    // Navigation Properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Car Car { get; set; } = null!;
    public virtual ICollection<BookingExtra> BookingExtras { get; set; } = new List<BookingExtra>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
} 