using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;

namespace NOL.Application.Common.Interfaces.Admin;

public interface ICustomerManagementService
{
    // Customer CRUD Operations
    Task<ApiResponse<AdminCustomerDto>> GetCustomerByIdAsync(string customerId);
    Task<ApiResponse<List<AdminCustomerDto>>> GetCustomersAsync(CustomerFilterDto filter);
    Task<ApiResponse<AdminCustomerDto>> UpdateCustomerAsync(string customerId, UpdateCustomerDto updateCustomerDto, string adminId);
    Task<ApiResponse> DeleteCustomerAsync(string customerId, string adminId);
    
    // Customer Status Management
    Task<ApiResponse<AdminCustomerDto>> ActivateCustomerAsync(string customerId, string adminId);
    Task<ApiResponse<AdminCustomerDto>> DeactivateCustomerAsync(string customerId, string adminId);
    Task<ApiResponse> BulkUpdateCustomerStatusAsync(List<string> customerIds, bool isActive, string adminId);
    
    // Customer Analytics
    Task<ApiResponse<CustomerAnalyticsDto>> GetCustomerAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<TopCustomerDto>>> GetTopCustomersAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<CustomerSegmentStatsDto>>> GetCustomerSegmentationAsync();
    
    // Customer Booking History
    Task<ApiResponse<List<CustomerBookingSummaryDto>>> GetCustomerBookingHistoryAsync(string customerId, int page = 1, int pageSize = 10);
    Task<ApiResponse<decimal>> GetCustomerLifetimeValueAsync(string customerId);
    Task<ApiResponse<double>> GetCustomerSatisfactionRatingAsync(string customerId);
    
    // Loyalty Points Management
    Task<ApiResponse<List<CustomerLoyaltyTransactionDto>>> GetCustomerLoyaltyTransactionsAsync(string customerId, int page = 1, int pageSize = 10);
    Task<ApiResponse<CustomerLoyaltyTransactionDto>> AwardLoyaltyPointsAsync(ManageLoyaltyPointsDto loyaltyPointsDto, string adminId);
    Task<ApiResponse<CustomerLoyaltyTransactionDto>> DeductLoyaltyPointsAsync(ManageLoyaltyPointsDto loyaltyPointsDto, string adminId);
    Task<ApiResponse<int>> GetCustomerAvailablePointsAsync(string customerId);
    Task<ApiResponse> ExpireLoyaltyPointsAsync(string customerId, string adminId);
    
    // Customer Communication
    Task<ApiResponse> SendNotificationToCustomerAsync(string customerId, SendCustomerNotificationDto notificationDto, string adminId);
    Task<ApiResponse> SendBulkNotificationAsync(SendCustomerNotificationDto notificationDto, string adminId);
    Task<ApiResponse> SendWelcomeEmailAsync(string customerId, string adminId);
    Task<ApiResponse> SendCustomerReportAsync(string customerId, string adminId);
    
    // Customer Segmentation
    Task<ApiResponse<string>> GetCustomerSegmentAsync(string customerId);
    Task<ApiResponse<List<AdminCustomerDto>>> GetCustomersBySegmentAsync(string segment, int page = 1, int pageSize = 10);
    Task<ApiResponse<List<string>>> GetAvailableCustomerSegmentsAsync();
    
    // Bulk Operations
    Task<ApiResponse> BulkOperationAsync(BulkCustomerOperationDto operationDto, string adminId);
    Task<ApiResponse> BulkAwardLoyaltyPointsAsync(List<string> customerIds, ManageLoyaltyPointsDto loyaltyPointsDto, string adminId);
    
    // Customer Reports
    Task<ApiResponse<CustomerReportDto>> GenerateCustomerReportAsync(CustomerFilterDto filter);
    Task<ApiResponse<byte[]>> ExportCustomerReportAsync(CustomerFilterDto filter, string format = "excel");
    Task<ApiResponse<byte[]>> ExportCustomerLoyaltyReportAsync(DateTime? startDate = null, DateTime? endDate = null, string format = "excel");
    
    // Customer Retention Analysis
    Task<ApiResponse<double>> GetCustomerRetentionRateAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<AdminCustomerDto>>> GetChurnRiskCustomersAsync(int count = 50);
    Task<ApiResponse<List<AdminCustomerDto>>> GetInactiveCustomersAsync(int daysSinceLastActivity = 90, int page = 1, int pageSize = 10);
    
    // Customer Validation
    Task<ApiResponse<bool>> ValidateCustomerEmailAsync(string email, string? excludeCustomerId = null);
    Task<ApiResponse<bool>> ValidateCustomerPhoneAsync(string phone, string? excludeCustomerId = null);
    
    // Customer Search
    Task<ApiResponse<List<AdminCustomerDto>>> SearchCustomersAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<ApiResponse<AdminCustomerDto>> GetCustomerByEmailAsync(string email);
    Task<ApiResponse<AdminCustomerDto>> GetCustomerByPhoneAsync(string phone);
}
