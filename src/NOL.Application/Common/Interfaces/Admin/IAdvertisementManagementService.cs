using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Application.DTOs.Common;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces.Admin;

public interface IAdvertisementManagementService
{
    // CRUD Operations
    Task<ApiResponse<AdminAdvertisementDto>> GetAdvertisementByIdAsync(int id);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsAsync(AdvertisementFilterDto filter);
    Task<ApiResponse<AdminAdvertisementDto>> CreateAdvertisementAsync(AdminCreateAdvertisementDto createAdvertisementDto, string adminId);
    Task<ApiResponse<AdminAdvertisementDto>> UpdateAdvertisementAsync(int id, AdminUpdateAdvertisementDto updateAdvertisementDto, string adminId);
    Task<ApiResponse> DeleteAdvertisementAsync(int id, string adminId);

    // Status Management
    Task<ApiResponse<AdminAdvertisementDto>> UpdateAdvertisementStatusAsync(int id, AdvertisementStatus status, string adminId);
    Task<ApiResponse> BulkUpdateAdvertisementStatusAsync(List<int> advertisementIds, AdvertisementStatus status, string adminId);

    // Scheduling
    Task<ApiResponse<AdminAdvertisementDto>> ScheduleAdvertisementAsync(int id, DateTime startDate, DateTime endDate, string adminId);
    Task<ApiResponse> BulkScheduleAdvertisementsAsync(AdvertisementScheduleDto scheduleDto, string adminId);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetScheduledAdvertisementsAsync(DateTime? date = null, int page = 1, int pageSize = 10);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetExpiredAdvertisementsAsync(int page = 1, int pageSize = 10);

    // Featured Content Management
    Task<ApiResponse<AdminAdvertisementDto>> SetAdvertisementFeaturedAsync(int id, bool isFeatured, string adminId);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetFeaturedAdvertisementsAsync(int page = 1, int pageSize = 10);
    Task<ApiResponse> UpdateAdvertisementSortOrderAsync(int id, int sortOrder, string adminId);

    // Discount Management
    Task<ApiResponse<AdminAdvertisementDto>> UpdateAdvertisementDiscountAsync(int id, decimal? discountPercentage, decimal? discountPrice, string adminId);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsWithDiscountsAsync(int page = 1, int pageSize = 10);
    Task<ApiResponse<decimal>> CalculateDiscountedPriceAsync(int advertisementId, decimal originalPrice);
    
    // Performance Analytics
    Task<ApiResponse<AdvertisementAnalyticsDto>> GetAdvertisementAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<AdvertisementPerformanceDto>>> GetTopPerformingAdvertisementsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<AdvertisementPerformanceDto>>> GetLowPerformingAdvertisementsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<AdvertisementMetricDto>>> GetAdvertisementMetricsAsync(int id, DateTime? startDate = null, DateTime? endDate = null);
    
    // Metrics Tracking
    Task<ApiResponse> RecordAdvertisementViewAsync(int id, string? userId = null, string? ipAddress = null);
    Task<ApiResponse> RecordAdvertisementClickAsync(int id, string? userId = null, string? ipAddress = null);
    Task<ApiResponse> RecordAdvertisementConversionAsync(int id, string? userId = null, decimal? conversionValue = null);
    
    // Type-based Analytics
    Task<ApiResponse<List<AdvertisementTypeStatsDto>>> GetAdvertisementTypeStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsByTypeAsync(AdvertisementType type, bool activeOnly = true, int page = 1, int pageSize = 10);

    // Bulk Operations
    Task<ApiResponse> BulkOperationAsync(BulkAdvertisementOperationDto operationDto, string adminId);
    Task<ApiResponse> BulkDeleteAdvertisementsAsync(List<int> advertisementIds, string adminId);
    Task<ApiResponse> BulkActivateAdvertisementsAsync(List<int> advertisementIds, string adminId);
    Task<ApiResponse> BulkDeactivateAdvertisementsAsync(List<int> advertisementIds, string adminId);

    // Copy and Template Operations
    Task<ApiResponse<AdminAdvertisementDto>> CopyAdvertisementAsync(CopyAdvertisementDto copyDto, string adminId);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementTemplatesAsync(int page = 1, int pageSize = 10);
    Task<ApiResponse<AdminAdvertisementDto>> CreateAdvertisementFromTemplateAsync(int templateId, AdminCreateAdvertisementDto createDto, string adminId);

    // Car and Category Association
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsByCarAsync(int carId, int page = 1, int pageSize = 10);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10);
    Task<ApiResponse> AssociateAdvertisementWithCarAsync(int advertisementId, int carId, string adminId);
    Task<ApiResponse> AssociateAdvertisementWithCategoryAsync(int advertisementId, int categoryId, string adminId);
    
    // Revenue Impact Analysis
    Task<ApiResponse<decimal>> GetAdvertisementRevenueImpactAsync(int id, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<MonthlyAdvertisementStatsDto>>> GetMonthlyAdvertisementStatsAsync(int year);
    Task<ApiResponse<decimal>> GetTotalAdvertisementRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Reports and Export
    Task<ApiResponse<AdvertisementReportDto>> GenerateAdvertisementReportAsync(AdvertisementFilterDto filter);
    Task<ApiResponse<byte[]>> ExportAdvertisementReportAsync(AdvertisementFilterDto filter, string format = "excel");
    Task<ApiResponse<byte[]>> ExportAdvertisementMetricsAsync(int id, DateTime? startDate = null, DateTime? endDate = null, string format = "excel");
    
    // Validation
    Task<ApiResponse<bool>> ValidateAdvertisementDatesAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<bool>> ValidateAdvertisementDiscountAsync(decimal? discountPercentage, decimal? discountPrice);
    Task<ApiResponse<List<string>>> ValidateAdvertisementDataAsync(AdminCreateAdvertisementDto createDto);
    
    // Search and Filter
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> SearchAdvertisementsAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetActiveAdvertisementsAsync(DateTime? date = null, int page = 1, int pageSize = 10);
    Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10);
}
