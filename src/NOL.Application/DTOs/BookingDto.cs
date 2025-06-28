using NOL.Domain.Enums;

namespace NOL.Application.DTOs;

public class BookingDto
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
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public CarDto Car { get; set; } = null!;
    public List<BookingExtraDto> Extras { get; set; } = new();
}

public class BookingExtraDto
{
    public int Id { get; set; }
    public string ExtraName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateBookingDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CarId { get; set; }
    public List<BookingExtraRequestDto> Extras { get; set; } = new();
    public string? Notes { get; set; }
}

public class BookingExtraRequestDto
{
    public int ExtraTypePriceId { get; set; }
    public int Quantity { get; set; }
} 