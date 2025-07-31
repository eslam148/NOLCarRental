using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.API.Controllers.Admin;

[ApiController]
[Route("api/admin/extra-type-prices")]
[Authorize(Roles = "SuperAdmin,Admin,BranchManager")]
public class ExtraTypePriceManagementController : ControllerBase
{
    private readonly IExtraTypePriceManagementService _extraTypePriceManagementService;
    private readonly ILogger<ExtraTypePriceManagementController> _logger;

    public ExtraTypePriceManagementController(
        IExtraTypePriceManagementService extraTypePriceManagementService,
        ILogger<ExtraTypePriceManagementController> logger)
    {
        _extraTypePriceManagementService = extraTypePriceManagementService;
        _logger = logger;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all extra type prices with filtering and pagination
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<List<AdminExtraTypePriceDto>>>> GetExtraTypePrices([FromBody] ExtraTypePriceFilterDto filter)
    {
        var result = await _extraTypePriceManagementService.GetExtraTypePricesAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get all extra type prices (simple GET endpoint)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminExtraTypePriceDto>>>> GetExtraTypePrices(
        [FromQuery] ExtraType? extraType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] decimal? minDailyPrice = null,
        [FromQuery] decimal? maxDailyPrice = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "NameEn",
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new ExtraTypePriceFilterDto
        {
            ExtraType = extraType,
            IsActive = isActive,
            MinDailyPrice = minDailyPrice,
            MaxDailyPrice = maxDailyPrice,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortOrder = sortOrder,
            Page = page,
            PageSize = pageSize
        };

        var result = await _extraTypePriceManagementService.GetExtraTypePricesAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get extra type price by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminExtraTypePriceDto>>> GetExtraTypePriceById(int id)
    {
        var result = await _extraTypePriceManagementService.GetExtraTypePriceByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Create new extra type price
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<AdminExtraTypePriceDto>>> CreateExtraTypePrice([FromBody] CreateExtraTypePriceDto createDto)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _extraTypePriceManagementService.CreateExtraTypePriceAsync(createDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update extra type price
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<AdminExtraTypePriceDto>>> UpdateExtraTypePrice(int id, [FromBody] UpdateExtraTypePriceDto updateDto)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _extraTypePriceManagementService.UpdateExtraTypePriceAsync(id, updateDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete extra type price
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse>> DeleteExtraTypePrice(int id)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _extraTypePriceManagementService.DeleteExtraTypePriceAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk delete extra type prices
    /// </summary>
    [HttpPost("bulk/delete")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkDeleteExtraTypePrices([FromBody] List<int> ids)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _extraTypePriceManagementService.BulkDeleteExtraTypePricesAsync(ids, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Activate extra type price
    /// </summary>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<AdminExtraTypePriceDto>>> ActivateExtraTypePrice(int id)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _extraTypePriceManagementService.ActivateExtraTypePriceAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Deactivate extra type price
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<AdminExtraTypePriceDto>>> DeactivateExtraTypePrice(int id)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _extraTypePriceManagementService.DeactivateExtraTypePriceAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk update status of extra type prices
    /// </summary>
    [HttpPost("bulk/status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkUpdateStatus([FromBody] BulkStatusUpdateDto bulkUpdate)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _extraTypePriceManagementService.BulkUpdateStatusAsync(bulkUpdate.Ids, bulkUpdate.IsActive, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Filtering and Search

    /// <summary>
    /// Get extra type prices by type
    /// </summary>
    [HttpGet("by-type/{extraType}")]
    public async Task<ActionResult<ApiResponse<List<AdminExtraTypePriceDto>>>> GetExtraTypePricesByType(ExtraType extraType)
    {
        var result = await _extraTypePriceManagementService.GetExtraTypePricesByTypeAsync(extraType);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get active extra type prices
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<AdminExtraTypePriceDto>>>> GetActiveExtraTypePrices()
    {
        var result = await _extraTypePriceManagementService.GetActiveExtraTypePricesAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get inactive extra type prices
    /// </summary>
    [HttpGet("inactive")]
    public async Task<ActionResult<ApiResponse<List<AdminExtraTypePriceDto>>>> GetInactiveExtraTypePrices()
    {
        var result = await _extraTypePriceManagementService.GetInactiveExtraTypePricesAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Search extra type prices by name or description
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<AdminExtraTypePriceDto>>>> SearchExtraTypePrices(
        [FromQuery] string searchTerm,
        [FromQuery] string language = "en")
    {
        var result = await _extraTypePriceManagementService.SearchExtraTypePricesAsync(searchTerm, language);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get extra type prices by price range
    /// </summary>
    [HttpGet("price-range")]
    public async Task<ActionResult<ApiResponse<List<AdminExtraTypePriceDto>>>> GetExtraTypePricesByPriceRange(
        [FromQuery] decimal minPrice,
        [FromQuery] decimal maxPrice,
        [FromQuery] string priceType = "daily")
    {
        var result = await _extraTypePriceManagementService.GetExtraTypePricesByPriceRangeAsync(minPrice, maxPrice, priceType);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Analytics and Statistics

    /// <summary>
    /// Get extra type price analytics
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<ExtraTypePriceAnalyticsDto>>> GetExtraTypePriceAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _extraTypePriceManagementService.GetExtraTypePriceAnalyticsAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get popular extra type prices
    /// </summary>
    [HttpGet("popular")]
    public async Task<ActionResult<ApiResponse<List<PopularExtraTypePriceDto>>>> GetPopularExtraTypePrices(
        [FromQuery] int count = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _extraTypePriceManagementService.GetPopularExtraTypePricesAsync(count, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get extra type price usage statistics
    /// </summary>
    [HttpGet("usage-stats")]
    public async Task<ActionResult<ApiResponse<List<ExtraTypePriceUsageStatsDto>>>> GetExtraTypePriceUsageStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _extraTypePriceManagementService.GetExtraTypePriceUsageStatsAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get revenue by extra type
    /// </summary>
    [HttpGet("revenue-by-type")]
    public async Task<ActionResult<ApiResponse<List<ExtraTypeRevenueDto>>>> GetRevenueByExtraType(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _extraTypePriceManagementService.GetRevenueByExtraTypeAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Pricing Management

    /// <summary>
    /// Update pricing for extra type price
    /// </summary>
    [HttpPut("{id}/pricing")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<AdminExtraTypePriceDto>>> UpdatePricing(int id, [FromBody] UpdateExtraTypePricingDto pricingDto)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _extraTypePriceManagementService.UpdatePricingAsync(id, pricingDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get pricing history for extra type price
    /// </summary>
    [HttpGet("{id}/pricing-history")]
    public async Task<ActionResult<ApiResponse<List<ExtraTypePricingHistoryDto>>>> GetPricingHistory(int id)
    {
        var result = await _extraTypePriceManagementService.GetPricingHistoryAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validate extra type price data
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<ValidationResultDto>>> ValidateExtraTypePrice([FromBody] CreateExtraTypePriceDto createDto)
    {
        var result = await _extraTypePriceManagementService.ValidateExtraTypePriceAsync(createDto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Check if extra type price name is unique
    /// </summary>
    [HttpGet("check-name-unique")]
    public async Task<ActionResult<ApiResponse<bool>>> IsNameUnique(
        [FromQuery] string nameEn,
        [FromQuery] string nameAr,
        [FromQuery] int? excludeId = null)
    {
        var result = await _extraTypePriceManagementService.IsNameUniqueAsync(nameEn, nameAr, excludeId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Booking Integration

    /// <summary>
    /// Get booking count for extra type price
    /// </summary>
    [HttpGet("{id}/booking-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetBookingCountForExtraTypePrice(
        int id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _extraTypePriceManagementService.GetBookingCountForExtraTypePriceAsync(id, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Check if extra type price can be deleted
    /// </summary>
    [HttpGet("{id}/can-delete")]
    public async Task<ActionResult<ApiResponse<bool>>> CanDeleteExtraTypePrice(int id)
    {
        var result = await _extraTypePriceManagementService.CanDeleteExtraTypePriceAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion
}

// Helper DTOs for bulk operations
public class BulkStatusUpdateDto
{
    public List<int> Ids { get; set; } = new();
    public bool IsActive { get; set; }
}
