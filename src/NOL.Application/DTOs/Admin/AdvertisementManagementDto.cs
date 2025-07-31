using System.ComponentModel.DataAnnotations;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs.Admin;

public class AdminAdvertisementDto : AdvertisementDto
{
    public string CreatedByAdminName { get; set; } = string.Empty;
    public new DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public new int ViewCount { get; set; }
    public new int ClickCount { get; set; }
    public double ClickThroughRate { get; set; }
    public int ConversionCount { get; set; }
    public double ConversionRate { get; set; }
    public decimal RevenueGenerated { get; set; }
    public double PerformanceScore { get; set; }
    public List<AdvertisementMetricDto> DailyMetrics { get; set; } = new();
}

public class AdminCreateAdvertisementDto
{
    [Required]
    [StringLength(200)]
    public string TitleAr { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string TitleEn { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string DescriptionAr { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string DescriptionEn { get; set; } = string.Empty;
    
    [Required]
    public AdvertisementType Type { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Url]
    public string? ImageUrl { get; set; }
    
    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }
    
    [Range(0, 10000)]
    public decimal? DiscountPrice { get; set; }
    
    public bool IsFeatured { get; set; } = false;
    
    [Range(0, 1000)]
    public int SortOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public int? CarId { get; set; }
    
    public int? CategoryId { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class AdminUpdateAdvertisementDto
{
    [StringLength(200)]
    public string? TitleAr { get; set; }
    
    [StringLength(200)]
    public string? TitleEn { get; set; }
    
    [StringLength(1000)]
    public string? DescriptionAr { get; set; }
    
    [StringLength(1000)]
    public string? DescriptionEn { get; set; }
    
    public AdvertisementType? Type { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [Url]
    public string? ImageUrl { get; set; }
    
    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }
    
    [Range(0, 10000)]
    public decimal? DiscountPrice { get; set; }
    
    public bool? IsFeatured { get; set; }
    
    [Range(0, 1000)]
    public int? SortOrder { get; set; }
    
    public bool? IsActive { get; set; }
    
    public AdvertisementStatus? Status { get; set; }
    
    public int? CarId { get; set; }
    
    public int? CategoryId { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class AdvertisementFilterDto
{
    public AdvertisementType? Type { get; set; }
    public AdvertisementStatus? Status { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
    public int? CarId { get; set; }
    public int? CategoryId { get; set; }
    public string? CreatedByAdminId { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public DateTime? CreatedDateFrom { get; set; }
    public DateTime? CreatedDateTo { get; set; }
    public decimal? MinDiscountPercentage { get; set; }
    public decimal? MaxDiscountPercentage { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AdvertisementMetricDto
{
    public DateTime Date { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    public double ClickThroughRate { get; set; }
    public int ConversionCount { get; set; }
    public double ConversionRate { get; set; }
    public decimal RevenueGenerated { get; set; }
}

public class AdvertisementAnalyticsDto
{
    public int TotalAdvertisements { get; set; }
    public int ActiveAdvertisements { get; set; }
    public int ExpiredAdvertisements { get; set; }
    public int FeaturedAdvertisements { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public double AverageClickThroughRate { get; set; }
    public int TotalConversions { get; set; }
    public double AverageConversionRate { get; set; }
    public decimal TotalRevenueGenerated { get; set; }
    public List<AdvertisementTypeStatsDto> TypeStats { get; set; } = new();
    public List<AdvertisementPerformanceDto> TopPerformingAds { get; set; } = new();
    public List<AdvertisementPerformanceDto> LowPerformingAds { get; set; } = new();
    public List<MonthlyAdvertisementStatsDto> MonthlyStats { get; set; } = new();
}

public class AdvertisementTypeStatsDto
{
    public AdvertisementType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public double ClickThroughRate { get; set; }
    public decimal RevenueGenerated { get; set; }
}

public class AdvertisementPerformanceDto
{
    public int AdvertisementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public AdvertisementType Type { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    public double ClickThroughRate { get; set; }
    public int ConversionCount { get; set; }
    public double ConversionRate { get; set; }
    public decimal RevenueGenerated { get; set; }
    public double PerformanceScore { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class MonthlyAdvertisementStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int AdvertisementCount { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public double ClickThroughRate { get; set; }
    public int TotalConversions { get; set; }
    public double ConversionRate { get; set; }
    public decimal RevenueGenerated { get; set; }
}

public class BulkAdvertisementOperationDto
{
    [Required]
    public List<int> AdvertisementIds { get; set; } = new();
    
    [Required]
    public string Operation { get; set; } = string.Empty; // "activate", "deactivate", "delete", "updateStatus", "feature", "unfeature"
    
    public bool? IsActive { get; set; }
    public AdvertisementStatus? NewStatus { get; set; }
    public bool? IsFeatured { get; set; }
    public DateTime? NewStartDate { get; set; }
    public DateTime? NewEndDate { get; set; }
}

public class AdvertisementScheduleDto
{
    [Required]
    public List<int> AdvertisementIds { get; set; } = new();
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public bool AutoActivate { get; set; } = true;
    public bool AutoDeactivate { get; set; } = true;
}

public class AdvertisementReportDto
{
    public DateTime GeneratedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public AdvertisementAnalyticsDto Analytics { get; set; } = new();
    public List<AdminAdvertisementDto> Advertisements { get; set; } = new();
    public AdvertisementReportSummaryDto Summary { get; set; } = new();
}

public class AdvertisementReportSummaryDto
{
    public int TotalAdvertisements { get; set; }
    public int ActiveAdvertisements { get; set; }
    public int ExpiredAdvertisements { get; set; }
    public int FeaturedAdvertisements { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public double OverallClickThroughRate { get; set; }
    public int TotalConversions { get; set; }
    public double OverallConversionRate { get; set; }
    public decimal TotalRevenueGenerated { get; set; }
    public decimal AverageRevenuePerAd { get; set; }
    public double AveragePerformanceScore { get; set; }
    public string BestPerformingAdType { get; set; } = string.Empty;
    public string MostPopularDiscountType { get; set; } = string.Empty;
}

public class CopyAdvertisementDto
{
    [Required]
    public int SourceAdvertisementId { get; set; }
    
    [StringLength(200)]
    public string? NewTitleAr { get; set; }
    
    [StringLength(200)]
    public string? NewTitleEn { get; set; }
    
    public DateTime? NewStartDate { get; set; }
    
    public DateTime? NewEndDate { get; set; }
    
    public int? NewCarId { get; set; }
    
    public int? NewCategoryId { get; set; }
    
    public bool CopyMetrics { get; set; } = false;
}
