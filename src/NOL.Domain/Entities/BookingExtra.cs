namespace NOL.Domain.Entities;

public class BookingExtra
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int BookingId { get; set; }
    public int ExtraTypePriceId { get; set; }

    // Navigation Properties
    public Booking Booking { get; set; } = null!;
    public ExtraTypePrice ExtraTypePrice { get; set; } = null!;
} 