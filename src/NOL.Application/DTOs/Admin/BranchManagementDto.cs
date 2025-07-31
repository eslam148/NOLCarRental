using System.ComponentModel.DataAnnotations;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs.Admin;

public class AdminBranchDto : BranchDto
{
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByAdmin { get; set; } = string.Empty;
    public string UpdatedByAdmin { get; set; } = string.Empty;
    
    // Statistics
    public int TotalCars { get; set; }
    public int AvailableCars { get; set; }
    public int RentedCars { get; set; }
    public int MaintenanceCars { get; set; }
    public double CarUtilizationRate { get; set; }
    
    // Booking Statistics
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    
    // Revenue Statistics
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
    
    // Staff Information
    public int TotalStaff { get; set; }
    public int ActiveStaff { get; set; }
    public List<BranchStaffDto> Staff { get; set; } = new();
    
    // Performance Metrics
    public double CustomerSatisfactionRate { get; set; }
    public double OnTimeDeliveryRate { get; set; }
    public int MaintenanceRequestsCount { get; set; }
    
    // Recent Activity
    public DateTime? LastBookingDate { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public List<BranchRecentActivityDto> RecentActivities { get; set; } = new();
}

public class AdminCreateBranchDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string NameAr { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string NameEn { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? DescriptionAr { get; set; }
    
    [StringLength(500)]
    public string? DescriptionEn { get; set; }
    
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Country { get; set; } = string.Empty;
    
    [Phone]
    public string? Phone { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Required]
    [Range(-90, 90)]
    public decimal Latitude { get; set; }
    
    [Required]
    [Range(-180, 180)]
    public decimal Longitude { get; set; }
    
    [StringLength(500)]
    public string? WorkingHours { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<string> AssignedStaffIds { get; set; } = new();
    
    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class AdminUpdateBranchDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? NameAr { get; set; }
    
    [StringLength(100, MinimumLength = 2)]
    public string? NameEn { get; set; }
    
    [StringLength(500)]
    public string? DescriptionAr { get; set; }
    
    [StringLength(500)]
    public string? DescriptionEn { get; set; }
    
    [StringLength(200, MinimumLength = 5)]
    public string? Address { get; set; }
    
    [StringLength(50, MinimumLength = 2)]
    public string? City { get; set; }
    
    [StringLength(50, MinimumLength = 2)]
    public string? Country { get; set; }
    
    [Phone]
    public string? Phone { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Range(-90, 90)]
    public decimal? Latitude { get; set; }
    
    [Range(-180, 180)]
    public decimal? Longitude { get; set; }
    
    [StringLength(500)]
    public string? WorkingHours { get; set; }
    
    public bool? IsActive { get; set; }
    
    public List<string>? AssignedStaffIds { get; set; }
    
    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class BranchFilterDto
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public DateTime? CreatedDateFrom { get; set; }
    public DateTime? CreatedDateTo { get; set; }
    public int? MinCars { get; set; }
    public int? MaxCars { get; set; }
    public decimal? MinRevenue { get; set; }
    public decimal? MaxRevenue { get; set; }
    public double? MinUtilizationRate { get; set; }
    public double? MaxUtilizationRate { get; set; }
    public string? SortBy { get; set; } = "name";
    public string? SortOrder { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class BranchStaffDto
{
    public string StaffId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public class BranchRecentActivityDto
{
    public DateTime Date { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
}

public class BranchAnalyticsDto
{
    public int TotalBranches { get; set; }
    public int ActiveBranches { get; set; }
    public int InactiveBranches { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenuePerBranch { get; set; }
    public int TotalCars { get; set; }
    public int TotalBookings { get; set; }
    public double AverageUtilizationRate { get; set; }
    public double AverageCustomerSatisfaction { get; set; }
    public List<BranchPerformanceDto> TopPerformingBranches { get; set; } = new();
    public List<BranchPerformanceDto> LowPerformingBranches { get; set; } = new();
    public List<MonthlyBranchStatsDto> MonthlyStats { get; set; } = new();
    public List<BranchCityStatsDto> CityStats { get; set; } = new();
    public List<BranchCountryStatsDto> CountryStats { get; set; } = new();
}

public class BranchPerformanceDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public int CarCount { get; set; }
    public double UtilizationRate { get; set; }
    public double CustomerSatisfactionRate { get; set; }
    public double PerformanceScore { get; set; }
    public DateTime LastActivityDate { get; set; }
}

public class MonthlyBranchStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int BranchCount { get; set; }
    public int NewBranches { get; set; }
    public int ClosedBranches { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public double AverageUtilizationRate { get; set; }
}

public class BranchCityStatsDto
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int BranchCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int TotalCars { get; set; }
    public double AverageUtilizationRate { get; set; }
}

public class BranchCountryStatsDto
{
    public string Country { get; set; } = string.Empty;
    public int BranchCount { get; set; }
    public int CityCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int TotalCars { get; set; }
    public double AverageUtilizationRate { get; set; }
    public double MarketShare { get; set; }
}

public class BranchCarTransferDto
{
    [Required]
    public List<int> CarIds { get; set; } = new();

    [Required]
    public int FromBranchId { get; set; }

    [Required]
    public int ToBranchId { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public DateTime? ScheduledDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class BranchStaffAssignmentDto
{
    [Required]
    public List<string> StaffIds { get; set; } = new();

    [Required]
    public int BranchId { get; set; }

    [StringLength(500)]
    public string? AssignmentReason { get; set; }

    public DateTime? EffectiveDate { get; set; }
}

public class BranchMaintenanceScheduleDto
{
    [Required]
    public int BranchId { get; set; }

    [Required]
    public string MaintenanceType { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledDate { get; set; }

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string? AssignedTechnician { get; set; }

    public decimal? EstimatedCost { get; set; }

    public int? EstimatedDurationHours { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class BranchRevenueAnalysisDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal DailyRevenue { get; set; }
    public decimal WeeklyRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public decimal RevenueGrowthRate { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int BookingCount { get; set; }
    public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
    public List<MonthlyRevenueDto> MonthlyBreakdown { get; set; } = new();
}



public class BulkBranchOperationDto
{
    [Required]
    public List<int> BranchIds { get; set; } = new();

    [Required]
    public string Operation { get; set; } = string.Empty; // "activate", "deactivate", "delete", "updateCity", "updateCountry"

    public bool? IsActive { get; set; }
    public string? NewCity { get; set; }
    public string? NewCountry { get; set; }
    public List<string>? AssignedStaffIds { get; set; }
}

public class BranchReportDto
{
    public DateTime GeneratedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public BranchAnalyticsDto Analytics { get; set; } = new();
    public List<AdminBranchDto> Branches { get; set; } = new();
    public BranchReportSummaryDto Summary { get; set; } = new();
}

public class BranchReportSummaryDto
{
    public int TotalBranches { get; set; }
    public int ActiveBranches { get; set; }
    public int InactiveBranches { get; set; }
    public int NewBranches { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenuePerBranch { get; set; }
    public int TotalCars { get; set; }
    public int TotalBookings { get; set; }
    public int TotalStaff { get; set; }
    public double AverageUtilizationRate { get; set; }
    public double AverageCustomerSatisfaction { get; set; }
    public string BestPerformingBranch { get; set; } = string.Empty;
    public string WorstPerformingBranch { get; set; } = string.Empty;
    public string MostPopularCity { get; set; } = string.Empty;
    public string MostPopularCountry { get; set; } = string.Empty;
}

public class BranchComparisonDto
{
    public List<int> BranchIds { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> Metrics { get; set; } = new(); // "revenue", "bookings", "utilization", "satisfaction"
}

public class BranchComparisonResultDto
{
    public List<BranchPerformanceDto> BranchPerformances { get; set; } = new();
    public Dictionary<string, List<decimal>> MetricComparisons { get; set; } = new();
    public string BestPerformingBranch { get; set; } = string.Empty;
    public string WorstPerformingBranch { get; set; } = string.Empty;
    public Dictionary<string, decimal> AverageMetrics { get; set; } = new();
}


