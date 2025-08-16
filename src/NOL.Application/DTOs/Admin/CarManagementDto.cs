using System.ComponentModel.DataAnnotations;
using NOL.Application.DTOs.Common;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs.Admin;

public class AdminCarDto : CarDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public double UtilizationRate { get; set; }
    public DateTime? LastBookingDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public List<CarMaintenanceRecordDto> MaintenanceHistory { get; set; } = new();
}

public class AdminCreateCarDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string BrandAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string BrandEn { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string ModelAr { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string ModelEn { get; set; } = string.Empty;

    [Required]
    [Range(1900, 2030)]
    public int Year { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string ColorAr { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string ColorEn { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string PlateNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 20)]
    public int SeatingCapacity { get; set; }

    [Required]
    public TransmissionType TransmissionType { get; set; }

    [Required]
    public FuelType FuelType { get; set; }

    [Required]
    [Range(0.01, 10000)]
    public decimal DailyRate { get; set; }

    [Required]
    [Range(0.01, 50000)]
    public decimal WeeklyRate { get; set; }

    [Required]
    [Range(0.01, 200000)]
    public decimal MonthlyRate { get; set; }

    public CarStatus Status { get; set; } = CarStatus.Available;

    [Url]
    public string? ImageUrl { get; set; }

    [StringLength(1000)]
    public string? DescriptionAr { get; set; }

    [StringLength(1000)]
    public string? DescriptionEn { get; set; }

    [Range(0, 1000000)]
    public int Mileage { get; set; }

    public string? Features { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int BranchId { get; set; }
}

public class AdminUpdateCarDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? BrandAr { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string? BrandEn { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string? ModelAr { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string? ModelEn { get; set; }

    [Range(1900, 2030)]
    public int? Year { get; set; }

    [StringLength(50, MinimumLength = 2)]
    public string? ColorAr { get; set; }

    [StringLength(50, MinimumLength = 2)]
    public string? ColorEn { get; set; }

    [StringLength(20, MinimumLength = 3)]
    public string? PlateNumber { get; set; }

    [Range(1, 20)]
    public int? SeatingCapacity { get; set; }

    public TransmissionType? TransmissionType { get; set; }

    public FuelType? FuelType { get; set; }

    [Range(0.01, 10000)]
    public decimal? DailyRate { get; set; }

    [Range(0.01, 50000)]
    public decimal? WeeklyRate { get; set; }

    [Range(0.01, 200000)]
    public decimal? MonthlyRate { get; set; }

    public CarStatus? Status { get; set; }

    [Url]
    public string? ImageUrl { get; set; }

    [StringLength(1000)]
    public string? DescriptionAr { get; set; }

    [StringLength(1000)]
    public string? DescriptionEn { get; set; }

    [Range(0, 1000000)]
    public int? Mileage { get; set; }

    public string? Features { get; set; }

    public int? CategoryId { get; set; }

    public int? BranchId { get; set; }
}

public class CarMaintenanceRecordDto
{
    public int Id { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime? NextMaintenanceDate { get; set; }
}

public class CarAnalyticsDto
{
    public int CarId { get; set; }
    public string CarInfo { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public double UtilizationRate { get; set; }
    public decimal AverageBookingValue { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<MonthlyCarRevenueDto> MonthlyRevenue { get; set; } = new();
    public List<CarBookingTrendDto> BookingTrends { get; set; } = new();
}

public class MonthlyCarRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public int DaysRented { get; set; }
}

public class CarBookingTrendDto
{
    public DateTime Date { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

public class BulkCarOperationDto
{
    [Required]
    public List<int> CarIds { get; set; } = new();
    
    [Required]
    public string Operation { get; set; } = string.Empty; // "delete", "updateStatus", "updateBranch", "updateCategory"
    
    public CarStatus? NewStatus { get; set; }
    public int? NewBranchId { get; set; }
    public int? NewCategoryId { get; set; }
}

public class CarImportDto
{
    [Required]
    public string BrandAr { get; set; } = string.Empty;
    [Required]
    public string BrandEn { get; set; } = string.Empty;
    [Required]
    public string ModelAr { get; set; } = string.Empty;
    [Required]
    public string ModelEn { get; set; } = string.Empty;
    [Required]
    public int Year { get; set; }
    [Required]
    public string ColorAr { get; set; } = string.Empty;
    [Required]
    public string ColorEn { get; set; } = string.Empty;
    [Required]
    public string PlateNumber { get; set; } = string.Empty;
    [Required]
    public int SeatingCapacity { get; set; }
    [Required]
    public string TransmissionType { get; set; } = string.Empty;
    [Required]
    public string FuelType { get; set; } = string.Empty;
    [Required]
    public decimal DailyRate { get; set; }
    [Required]
    public decimal WeeklyRate { get; set; }
    [Required]
    public decimal MonthlyRate { get; set; }
    public string Status { get; set; } = "Available";
    public string? ImageUrl { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public int Mileage { get; set; }
    public string? Features { get; set; }
    [Required]
    public string CategoryName { get; set; } = string.Empty;
    [Required]
    public string BranchName { get; set; } = string.Empty;
}

public class CarFilterDto : BasePaginationFilterDto
{
    public CarStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    public int? BranchId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public decimal? DailyRateFrom { get; set; }
    public decimal? DailyRateTo { get; set; }
    public TransmissionType? TransmissionType { get; set; }
    public FuelType? FuelType { get; set; }

    public CarFilterDto()
    {
        SortBy = "CreatedAt";
        SortOrder = "desc";
    }
}
