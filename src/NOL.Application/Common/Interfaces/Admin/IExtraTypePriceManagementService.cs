using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces.Admin;

public interface IExtraTypePriceManagementService
{
    #region CRUD Operations

    /// <summary>
    /// Get all extra type prices with filtering and pagination
    /// </summary>
    Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetExtraTypePricesAsync(ExtraTypePriceFilterDto filter);

    /// <summary>
    /// Get extra type price by ID
    /// </summary>
    Task<ApiResponse<AdminExtraTypePriceDto>> GetExtraTypePriceByIdAsync(int id);

    /// <summary>
    /// Create new extra type price
    /// </summary>
    Task<ApiResponse<AdminExtraTypePriceDto>> CreateExtraTypePriceAsync(CreateExtraTypePriceDto createDto, string adminId);

    /// <summary>
    /// Update extra type price
    /// </summary>
    Task<ApiResponse<AdminExtraTypePriceDto>> UpdateExtraTypePriceAsync(int id, UpdateExtraTypePriceDto updateDto, string adminId);

    /// <summary>
    /// Delete extra type price
    /// </summary>
    Task<ApiResponse> DeleteExtraTypePriceAsync(int id, string adminId);

    /// <summary>
    /// Bulk delete extra type prices
    /// </summary>
    Task<ApiResponse<BulkOperationResultDto>> BulkDeleteExtraTypePricesAsync(List<int> ids, string adminId);

    #endregion

    #region Status Management

    /// <summary>
    /// Activate extra type price
    /// </summary>
    Task<ApiResponse<AdminExtraTypePriceDto>> ActivateExtraTypePriceAsync(int id, string adminId);

    /// <summary>
    /// Deactivate extra type price
    /// </summary>
    Task<ApiResponse<AdminExtraTypePriceDto>> DeactivateExtraTypePriceAsync(int id, string adminId);

    /// <summary>
    /// Bulk update status of extra type prices
    /// </summary>
    Task<ApiResponse<BulkOperationResultDto>> BulkUpdateStatusAsync(List<int> ids, bool isActive, string adminId);

    #endregion

    #region Filtering and Search

    /// <summary>
    /// Get extra type prices by type
    /// </summary>
    Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetExtraTypePricesByTypeAsync(ExtraType extraType);

    /// <summary>
    /// Get active extra type prices
    /// </summary>
    Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetActiveExtraTypePricesAsync();

    /// <summary>
    /// Get inactive extra type prices
    /// </summary>
    Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetInactiveExtraTypePricesAsync();

    /// <summary>
    /// Search extra type prices by name or description
    /// </summary>
    Task<ApiResponse<List<AdminExtraTypePriceDto>>> SearchExtraTypePricesAsync(string searchTerm, string language = "en");

    /// <summary>
    /// Get extra type prices by price range
    /// </summary>
    Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetExtraTypePricesByPriceRangeAsync(decimal minPrice, decimal maxPrice, string priceType = "daily");

    #endregion

    #region Analytics and Statistics

    /// <summary>
    /// Get extra type price analytics
    /// </summary>
    Task<ApiResponse<ExtraTypePriceAnalyticsDto>> GetExtraTypePriceAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get popular extra type prices
    /// </summary>
    Task<ApiResponse<List<PopularExtraTypePriceDto>>> GetPopularExtraTypePricesAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get extra type price usage statistics
    /// </summary>
    Task<ApiResponse<List<ExtraTypePriceUsageStatsDto>>> GetExtraTypePriceUsageStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get revenue by extra type
    /// </summary>
    Task<ApiResponse<List<ExtraTypeRevenueDto>>> GetRevenueByExtraTypeAsync(DateTime? startDate = null, DateTime? endDate = null);

    #endregion

    #region Pricing Management

    /// <summary>
    /// Update pricing for extra type price
    /// </summary>
    Task<ApiResponse<AdminExtraTypePriceDto>> UpdatePricingAsync(int id, UpdateExtraTypePricingDto pricingDto, string adminId);

    /// <summary>
    /// Bulk update pricing for multiple extra type prices
    /// </summary>
    Task<ApiResponse<BulkOperationResultDto>> BulkUpdatePricingAsync(List<BulkPricingUpdateDto> pricingUpdates, string adminId);

    /// <summary>
    /// Apply percentage increase/decrease to pricing
    /// </summary>
    Task<ApiResponse<BulkOperationResultDto>> ApplyPricingAdjustmentAsync(List<int> ids, decimal percentage, bool isIncrease, string adminId);

    /// <summary>
    /// Get pricing history for extra type price
    /// </summary>
    Task<ApiResponse<List<ExtraTypePricingHistoryDto>>> GetPricingHistoryAsync(int id);

    #endregion

    #region Validation and Business Rules

    /// <summary>
    /// Validate extra type price data
    /// </summary>
    Task<ApiResponse<ValidationResultDto>> ValidateExtraTypePriceAsync(CreateExtraTypePriceDto createDto);

    /// <summary>
    /// Check if extra type price name is unique
    /// </summary>
    Task<ApiResponse<bool>> IsNameUniqueAsync(string nameEn, string nameAr, int? excludeId = null);

    /// <summary>
    /// Get validation rules for extra type prices
    /// </summary>
    Task<ApiResponse<ExtraTypePriceValidationRulesDto>> GetValidationRulesAsync();

    #endregion

    #region Import/Export

    /// <summary>
    /// Export extra type prices to Excel
    /// </summary>
    Task<ApiResponse<byte[]>> ExportExtraTypePricesToExcelAsync(ExtraTypePriceFilterDto filter);

    /// <summary>
    /// Export extra type prices to CSV
    /// </summary>
    Task<ApiResponse<byte[]>> ExportExtraTypePricesToCsvAsync(ExtraTypePriceFilterDto filter);

    /// <summary>
    /// Import extra type prices from Excel
    /// </summary>
    Task<ApiResponse<ImportResultDto>> ImportExtraTypePricesFromExcelAsync(byte[] fileData, string adminId);

    /// <summary>
    /// Get import template
    /// </summary>
    Task<ApiResponse<byte[]>> GetImportTemplateAsync();

    #endregion

    #region Localization

    /// <summary>
    /// Get extra type price with localized content
    /// </summary>
    Task<ApiResponse<AdminExtraTypePriceDto>> GetLocalizedExtraTypePriceAsync(int id, string language = "en");

    /// <summary>
    /// Update localization for extra type price
    /// </summary>
    Task<ApiResponse<AdminExtraTypePriceDto>> UpdateLocalizationAsync(int id, UpdateExtraTypePriceLocalizationDto localizationDto, string adminId);

    #endregion

    #region Booking Integration

    /// <summary>
    /// Get extra type prices used in bookings
    /// </summary>
    Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetExtraTypePricesUsedInBookingsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get booking count for extra type price
    /// </summary>
    Task<ApiResponse<int>> GetBookingCountForExtraTypePriceAsync(int id, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Check if extra type price can be deleted (not used in active bookings)
    /// </summary>
    Task<ApiResponse<bool>> CanDeleteExtraTypePriceAsync(int id);

    #endregion

    #region Reporting

    /// <summary>
    /// Generate extra type price report
    /// </summary>
    Task<ApiResponse<ExtraTypePriceReportDto>> GenerateExtraTypePriceReportAsync(ExtraTypePriceReportFilterDto filter);

    /// <summary>
    /// Get extra type price performance report
    /// </summary>
    Task<ApiResponse<ExtraTypePricePerformanceReportDto>> GetPerformanceReportAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Export extra type price report
    /// </summary>
    Task<ApiResponse<byte[]>> ExportReportAsync(ExtraTypePriceReportFilterDto filter, string format = "pdf");

    #endregion
}
