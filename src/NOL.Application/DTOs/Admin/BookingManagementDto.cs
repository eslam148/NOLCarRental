using System.ComponentModel.DataAnnotations;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs.Admin;

public class AdminBookingDto : BookingDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CarBrand { get; set; } = string.Empty;
    public string CarModel { get; set; } = string.Empty;
    public string CarPlateNumber { get; set; } = string.Empty;
    public string ReceivingBranchName { get; set; } = string.Empty;
    public string DeliveryBranchName { get; set; } = string.Empty;
    public List<BookingExtraDetailDto> ExtrasDetails { get; set; } = new();
    public List<PaymentDetailDto> PaymentDetails { get; set; } = new();
    public new DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByAdmin { get; set; } = string.Empty;
    public string LastModifiedByAdmin { get; set; } = string.Empty;
}

public class BookingExtraDetailDto
{
    public int Id { get; set; }
    public string ExtraName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class PaymentDetailDto
{
    public int Id { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
}

public class BookingFilterDto
{
    public BookingStatus? Status { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public DateTime? CreatedDateFrom { get; set; }
    public DateTime? CreatedDateTo { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? BookingNumber { get; set; }
    public int? CarId { get; set; }
    public int? BranchId { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UpdateBookingStatusDto
{
    [Required]
    public BookingStatus Status { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(500)]
    public string? CancellationReason { get; set; }
}

public class BookingAnalyticsDto
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    public double AverageBookingDuration { get; set; }
    public List<BookingStatusStatsDto> StatusBreakdown { get; set; } = new();
    public List<MonthlyBookingStatsDto> MonthlyStats { get; set; } = new();
    public List<PopularCarStatsDto> PopularCars { get; set; } = new();
    public List<BranchBookingStatsDto> BranchStats { get; set; } = new();
    public List<PeakTimeStatsDto> PeakTimes { get; set; } = new();
}

public class BookingStatusStatsDto
{
    public BookingStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlyBookingStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public double CancellationRate { get; set; }
}

public class PopularCarStatsDto
{
    public int CarId { get; set; }
    public string CarInfo { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public double UtilizationRate { get; set; }
    public double AverageRating { get; set; }
}

public class BranchBookingStatsDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public int PickupCount { get; set; }
    public int ReturnCount { get; set; }
}

public class PeakTimeStatsDto
{
    public string Period { get; set; } = string.Empty; // "Hour", "DayOfWeek", "Month"
    public string PeriodName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public double Percentage { get; set; }
}

public class BookingReportDto
{
    public DateTime GeneratedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public BookingAnalyticsDto Analytics { get; set; } = new();
    public List<AdminBookingDto> Bookings { get; set; } = new();
    public BookingReportSummaryDto Summary { get; set; } = new();
}

public class BookingReportSummaryDto
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetRevenue { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int PendingBookings { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public decimal AverageBookingValue { get; set; }
    public double AverageBookingDuration { get; set; }
}

public class AdminCreateBookingDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public int CarId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public int ReceivingBranchId { get; set; }
    
    [Required]
    public int DeliveryBranchId { get; set; }
    
    public List<AdminBookingExtraDto> Extras { get; set; } = new();
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public decimal? DiscountAmount { get; set; }
    
    public string? DiscountReason { get; set; }
}

public class AdminBookingExtraDto
{
    [Required]
    public int ExtraTypePriceId { get; set; }

    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }
}

public class ModifyBookingDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ReceivingBranchId { get; set; }
    public int? DeliveryBranchId { get; set; }
    public List<AdminBookingExtraDto>? Extras { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
    
    public decimal? DiscountAmount { get; set; }
    public string? DiscountReason { get; set; }
    
    [StringLength(500)]
    public string? ModificationReason { get; set; }
}

public class BulkBookingOperationDto
{
    [Required]
    public List<int> BookingIds { get; set; } = new();
    
    [Required]
    public string Operation { get; set; } = string.Empty; // "cancel", "confirm", "complete", "updateStatus"
    
    public BookingStatus? NewStatus { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(500)]
    public string? Reason { get; set; }
}
