using System.ComponentModel.DataAnnotations;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs.Admin;

#region Main DTOs

public class AdminExtraTypePriceDto
{
    public int Id { get; set; }
    public ExtraType ExtraType { get; set; }
    public string ExtraTypeName { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public decimal DailyPrice { get; set; }
    public decimal WeeklyPrice { get; set; }
    public decimal MonthlyPrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Statistics
    public int TotalBookings { get; set; }
    public int ActiveBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }
    public int UsageCount { get; set; }
    public DateTime? LastUsed { get; set; }
    
    // Calculated Fields
    public decimal WeeklySavings => (DailyPrice * 7) - WeeklyPrice;
    public decimal MonthlySavings => (DailyPrice * 30) - MonthlyPrice;
    public decimal WeeklyDiscountPercentage => DailyPrice > 0 ? (WeeklySavings / (DailyPrice * 7)) * 100 : 0;
    public decimal MonthlyDiscountPercentage => DailyPrice > 0 ? (MonthlySavings / (DailyPrice * 30)) * 100 : 0;
}

public class CreateExtraTypePriceDto
{
    [Required]
    public ExtraType ExtraType { get; set; }
    
    [Required]
    [StringLength(100)]
    public string NameAr { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string NameEn { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string DescriptionAr { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string DescriptionEn { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 10000)]
    public decimal DailyPrice { get; set; }
    
    [Required]
    [Range(0.01, 50000)]
    public decimal WeeklyPrice { get; set; }
    
    [Required]
    [Range(0.01, 200000)]
    public decimal MonthlyPrice { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class UpdateExtraTypePriceDto
{
    [StringLength(100)]
    public string? NameAr { get; set; }
    
    [StringLength(100)]
    public string? NameEn { get; set; }
    
    [StringLength(500)]
    public string? DescriptionAr { get; set; }
    
    [StringLength(500)]
    public string? DescriptionEn { get; set; }
    
    [Range(0.01, 10000)]
    public decimal? DailyPrice { get; set; }
    
    [Range(0.01, 50000)]
    public decimal? WeeklyPrice { get; set; }
    
    [Range(0.01, 200000)]
    public decimal? MonthlyPrice { get; set; }
    
    public bool? IsActive { get; set; }
}

#endregion

#region Bulk Operation DTOs

public class BulkOperationResultDto
{
    public int TotalItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public bool IsSuccessful => FailedItems == 0;
    public decimal SuccessRate => TotalItems > 0 ? (decimal)SuccessfulItems / TotalItems * 100 : 0;
}

#endregion

#region Filter DTOs

public class ExtraTypePriceFilterDto
{
    public ExtraType? ExtraType { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinDailyPrice { get; set; }
    public decimal? MaxDailyPrice { get; set; }
    public decimal? MinWeeklyPrice { get; set; }
    public decimal? MaxWeeklyPrice { get; set; }
    public decimal? MinMonthlyPrice { get; set; }
    public decimal? MaxMonthlyPrice { get; set; }
    public string? SearchTerm { get; set; }
    public string? Language { get; set; } = "en";
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
    public string? SortBy { get; set; } = "NameEn";
    public string? SortOrder { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

#endregion

#region Analytics DTOs

public class ExtraTypePriceAnalyticsDto
{
    public int TotalExtraTypePrices { get; set; }
    public int ActiveExtraTypePrices { get; set; }
    public int InactiveExtraTypePrices { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageDailyPrice { get; set; }
    public decimal AverageWeeklyPrice { get; set; }
    public decimal AverageMonthlyPrice { get; set; }
    public int TotalBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
    public List<ExtraTypeStatsDto> ExtraTypeStats { get; set; } = new();
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    public List<PopularExtraTypePriceDto> TopPerforming { get; set; } = new();
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}

public class ExtraTypeStatsDto
{
    public ExtraType ExtraType { get; set; }
    public string ExtraTypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MarketShare { get; set; }
}

public class PopularExtraTypePriceDto
{
    public int Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public ExtraType ExtraType { get; set; }
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal DailyPrice { get; set; }
    public decimal PopularityScore { get; set; }
    public decimal RevenuePerBooking { get; set; }
}

public class ExtraTypePriceUsageStatsDto
{
    public int Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public ExtraType ExtraType { get; set; }
    public int TotalUsage { get; set; }
    public int MonthlyUsage { get; set; }
    public int WeeklyUsage { get; set; }
    public decimal UsageGrowthRate { get; set; }
    public DateTime? LastUsed { get; set; }
    public decimal AverageQuantityPerBooking { get; set; }
}

public class ExtraTypeRevenueDto
{
    public ExtraType ExtraType { get; set; }
    public string ExtraTypeName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal DailyRevenue { get; set; }
    public decimal WeeklyRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int BookingCount { get; set; }
    public decimal AverageRevenuePerBooking { get; set; }
    public decimal RevenueGrowthRate { get; set; }
    public decimal MarketSharePercentage { get; set; }
}

#endregion

#region Pricing DTOs

public class UpdateExtraTypePricingDto
{
    [Required]
    [Range(0.01, 10000)]
    public decimal DailyPrice { get; set; }
    
    [Required]
    [Range(0.01, 50000)]
    public decimal WeeklyPrice { get; set; }
    
    [Required]
    [Range(0.01, 200000)]
    public decimal MonthlyPrice { get; set; }
    
    public string? Reason { get; set; }
}

public class BulkPricingUpdateDto
{
    public int Id { get; set; }
    public decimal? DailyPrice { get; set; }
    public decimal? WeeklyPrice { get; set; }
    public decimal? MonthlyPrice { get; set; }
}

public class ExtraTypePricingHistoryDto
{
    public int Id { get; set; }
    public int ExtraTypePriceId { get; set; }
    public decimal OldDailyPrice { get; set; }
    public decimal NewDailyPrice { get; set; }
    public decimal OldWeeklyPrice { get; set; }
    public decimal NewWeeklyPrice { get; set; }
    public decimal OldMonthlyPrice { get; set; }
    public decimal NewMonthlyPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

#endregion

#region Validation DTOs

public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> ValidationData { get; set; } = new();
}

public class ExtraTypePriceValidationRulesDto
{
    public decimal MinDailyPrice { get; set; } = 0.01m;
    public decimal MaxDailyPrice { get; set; } = 10000m;
    public decimal MinWeeklyPrice { get; set; } = 0.01m;
    public decimal MaxWeeklyPrice { get; set; } = 50000m;
    public decimal MinMonthlyPrice { get; set; } = 0.01m;
    public decimal MaxMonthlyPrice { get; set; } = 200000m;
    public int MaxNameLength { get; set; } = 100;
    public int MaxDescriptionLength { get; set; } = 500;
    public bool RequireUniqueNames { get; set; } = true;
    public List<ExtraType> AllowedExtraTypes { get; set; } = new();
}

#endregion

#region Import/Export DTOs

public class ImportResultDto
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<ImportedExtraTypePriceDto> ImportedItems { get; set; } = new();
}

public class ImportedExtraTypePriceDto
{
    public int RowNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public AdminExtraTypePriceDto? ExtraTypePrice { get; set; }
    public List<string> Errors { get; set; } = new();
}

#endregion

#region Localization DTOs

public class UpdateExtraTypePriceLocalizationDto
{
    [StringLength(100)]
    public string? NameAr { get; set; }
    
    [StringLength(100)]
    public string? NameEn { get; set; }
    
    [StringLength(500)]
    public string? DescriptionAr { get; set; }
    
    [StringLength(500)]
    public string? DescriptionEn { get; set; }
}

#endregion

#region Report DTOs

public class ExtraTypePriceReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ExtraType? ExtraType { get; set; }
    public bool? IsActive { get; set; }
    public string? ReportType { get; set; } = "summary";
    public bool IncludeAnalytics { get; set; } = true;
    public bool IncludeUsageStats { get; set; } = true;
    public bool IncludeRevenue { get; set; } = true;
    public string? Language { get; set; } = "en";
}

public class ExtraTypePriceReportDto
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public ExtraTypePriceReportFilterDto Filter { get; set; } = new();
    public ExtraTypePriceAnalyticsDto Analytics { get; set; } = new();
    public List<AdminExtraTypePriceDto> ExtraTypePrices { get; set; } = new();
    public List<ExtraTypePriceUsageStatsDto> UsageStats { get; set; } = new();
    public List<ExtraTypeRevenueDto> RevenueData { get; set; } = new();
}

public class ExtraTypePricePerformanceReportDto
{
    public DateTime ReportDate { get; set; }
    public List<PopularExtraTypePriceDto> TopPerformers { get; set; } = new();
    public List<PopularExtraTypePriceDto> LowPerformers { get; set; } = new();
    public List<ExtraTypeRevenueDto> RevenueByType { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal RevenueGrowth { get; set; }
    public int TotalBookings { get; set; }
    public decimal BookingGrowth { get; set; }
}

public class MonthlyTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal GrowthRate { get; set; }
}

#endregion
