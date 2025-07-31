using System.ComponentModel.DataAnnotations;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs.Admin;

public class AdminCustomerDto : UserDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastBookingDate { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageBookingValue { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public double CancellationRate { get; set; }
    public int TotalLoyaltyPoints { get; set; }
    public int AvailableLoyaltyPoints { get; set; }
    public int LifetimePointsEarned { get; set; }
    public int LifetimePointsRedeemed { get; set; }
    public DateTime? LastPointsEarnedDate { get; set; }
    public string CustomerSegment { get; set; } = string.Empty;
    public List<CustomerBookingSummaryDto> RecentBookings { get; set; } = new();
    public List<CustomerLoyaltyTransactionDto> RecentLoyaltyTransactions { get; set; } = new();
}

public class CustomerBookingSummaryDto
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string CarInfo { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerLoyaltyTransactionDto
{
    public int Id { get; set; }
    public int Points { get; set; }
    public LoyaltyPointTransactionType TransactionType { get; set; }
    public LoyaltyPointEarnReason? EarnReason { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
}

public class CustomerFilterDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public UserRole? UserRole { get; set; }
    public Language? PreferredLanguage { get; set; }
    public bool? IsActive { get; set; }
    public bool? EmailVerified { get; set; }
    public DateTime? CreatedDateFrom { get; set; }
    public DateTime? CreatedDateTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }
    public int? MinBookings { get; set; }
    public int? MaxBookings { get; set; }
    public decimal? MinSpent { get; set; }
    public decimal? MaxSpent { get; set; }
    public int? MinLoyaltyPoints { get; set; }
    public int? MaxLoyaltyPoints { get; set; }
    public string? CustomerSegment { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UpdateCustomerDto
{
    [StringLength(100)]
    public string? FullName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    public Language? PreferredLanguage { get; set; }
    
    public bool? IsActive { get; set; }
    
    public bool? EmailConfirmed { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class CustomerAnalyticsDto
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int NewCustomersLastMonth { get; set; }
    public double CustomerGrowthRate { get; set; }
    public double CustomerRetentionRate { get; set; }
    public decimal AverageCustomerValue { get; set; }
    public decimal AverageLoyaltyPoints { get; set; }
    public List<CustomerSegmentStatsDto> CustomerSegments { get; set; } = new();
    public List<MonthlyCustomerStatsDto> MonthlyStats { get; set; } = new();
    public List<CustomerLanguageStatsDto> LanguageStats { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

public class CustomerSegmentStatsDto
{
    public string SegmentName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public double Percentage { get; set; }
    public decimal AverageSpending { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageBookingFrequency { get; set; }
}

public class MonthlyCustomerStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int NewCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int ChurnedCustomers { get; set; }
    public double RetentionRate { get; set; }
}

public class CustomerLanguageStatsDto
{
    public Language Language { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public double Percentage { get; set; }
}

public class TopCustomerDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public decimal TotalSpent { get; set; }
    public int LoyaltyPoints { get; set; }
    public DateTime LastBookingDate { get; set; }
    public string CustomerSegment { get; set; } = string.Empty;
}

public class ManageLoyaltyPointsDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 10000)]
    public int Points { get; set; }
    
    [Required]
    public LoyaltyPointTransactionType TransactionType { get; set; }
    
    public LoyaltyPointEarnReason? EarnReason { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime? ExpiryDate { get; set; }
}

public class SendCustomerNotificationDto
{
    [Required]
    public List<string> CustomerIds { get; set; } = new();
    
    [Required]
    [StringLength(200)]
    public string TitleAr { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string TitleEn { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string MessageAr { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string MessageEn { get; set; } = string.Empty;
    
    [Required]
    public NotificationType Type { get; set; }
    
    public bool SendEmail { get; set; } = false;
}

public class CustomerReportDto
{
    public DateTime GeneratedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public CustomerAnalyticsDto Analytics { get; set; } = new();
    public List<AdminCustomerDto> Customers { get; set; } = new();
    public CustomerReportSummaryDto Summary { get; set; } = new();
}

public class CustomerReportSummaryDto
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public int NewCustomers { get; set; }
    public decimal TotalCustomerValue { get; set; }
    public decimal AverageCustomerValue { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalLoyaltyPointsIssued { get; set; }
    public int TotalLoyaltyPointsRedeemed { get; set; }
    public double CustomerRetentionRate { get; set; }
    public double CustomerSatisfactionRate { get; set; }
}

public class BulkCustomerOperationDto
{
    [Required]
    public List<string> CustomerIds { get; set; } = new();
    
    [Required]
    public string Operation { get; set; } = string.Empty; // "activate", "deactivate", "sendNotification", "awardPoints"
    
    public bool? IsActive { get; set; }
    public SendCustomerNotificationDto? NotificationData { get; set; }
    public ManageLoyaltyPointsDto? LoyaltyPointsData { get; set; }
}
