using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.API.Controllers.Admin;

/// <summary>
/// Advertisement Management Controller - Admin operations for advertisement management
/// </summary>
[ApiController]
[Route("api/admin/advertisements")]
[Authorize(Roles = "Admin,SuperAdmin,BranchManager")]
[Tags("Admin Advertisement Management")]
public class AdvertisementManagementController : ControllerBase
{
    private readonly IAdvertisementManagementService _advertisementManagementService;

    public AdvertisementManagementController(IAdvertisementManagementService advertisementManagementService)
    {
        _advertisementManagementService = advertisementManagementService;
    }

    /// <summary>
    /// Get all advertisements with advanced filtering and pagination
    /// </summary>
    /// <param name="filter">Advertisement filter parameters</param>
    /// <returns>Paginated list of advertisements with admin details</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetAdvertisements([FromQuery] AdvertisementFilterDto filter)
    {
        var result = await _advertisementManagementService.GetAdvertisementsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisement by ID with detailed admin information
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <returns>Advertisement details with performance metrics</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminAdvertisementDto>>> GetAdvertisementById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        var result = await _advertisementManagementService.GetAdvertisementByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Create a new advertisement
    /// </summary>
    /// <param name="createAdvertisementDto">Advertisement creation data</param>
    /// <returns>Created advertisement details</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminAdvertisementDto>>> CreateAdvertisement([FromBody] AdminCreateAdvertisementDto createAdvertisementDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.CreateAdvertisementAsync(createAdvertisementDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update an existing advertisement
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="updateAdvertisementDto">Advertisement update data</param>
    /// <returns>Updated advertisement details</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AdminAdvertisementDto>>> UpdateAdvertisement(int id, [FromBody] AdminUpdateAdvertisementDto updateAdvertisementDto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.UpdateAdvertisementAsync(id, updateAdvertisementDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete an advertisement
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteAdvertisement(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.DeleteAdvertisementAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update advertisement status
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="status">New advertisement status</param>
    /// <returns>Updated advertisement details</returns>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<AdminAdvertisementDto>>> UpdateAdvertisementStatus(int id, [FromBody] AdvertisementStatus status)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.UpdateAdvertisementStatusAsync(id, status, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk update advertisement status
    /// </summary>
    /// <param name="advertisementIds">List of advertisement IDs</param>
    /// <param name="status">New status for all advertisements</param>
    /// <returns>Operation result</returns>
    [HttpPatch("bulk/status")]
    public async Task<ActionResult<ApiResponse>> BulkUpdateAdvertisementStatus([FromBody] List<int> advertisementIds, [FromQuery] AdvertisementStatus status)
    {
        if (advertisementIds == null || !advertisementIds.Any())
        {
            return BadRequest(new { message = "Advertisement IDs list cannot be empty" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.BulkUpdateAdvertisementStatusAsync(advertisementIds, status, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Schedule advertisement
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Updated advertisement details</returns>
    [HttpPatch("{id}/schedule")]
    public async Task<ActionResult<ApiResponse<AdminAdvertisementDto>>> ScheduleAdvertisement(int id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        if (startDate >= endDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.ScheduleAdvertisementAsync(id, startDate, endDate, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk schedule advertisements
    /// </summary>
    /// <param name="scheduleDto">Bulk schedule data</param>
    /// <returns>Operation result</returns>
    [HttpPost("bulk/schedule")]
    public async Task<ActionResult<ApiResponse>> BulkScheduleAdvertisements([FromBody] AdvertisementScheduleDto scheduleDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.BulkScheduleAdvertisementsAsync(scheduleDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get scheduled advertisements
    /// </summary>
    /// <param name="date">Optional date filter</param>
    /// <returns>List of scheduled advertisements</returns>
    [HttpGet("scheduled")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetScheduledAdvertisements([FromQuery] DateTime? date = null)
    {
        var result = await _advertisementManagementService.GetScheduledAdvertisementsAsync(date);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get expired advertisements
    /// </summary>
    /// <returns>List of expired advertisements</returns>
    [HttpGet("expired")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetExpiredAdvertisements()
    {
        var result = await _advertisementManagementService.GetExpiredAdvertisementsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Set advertisement as featured
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="isFeatured">Featured status</param>
    /// <returns>Updated advertisement details</returns>
    [HttpPatch("{id}/featured")]
    public async Task<ActionResult<ApiResponse<AdminAdvertisementDto>>> SetAdvertisementFeatured(int id, [FromBody] bool isFeatured)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.SetAdvertisementFeaturedAsync(id, isFeatured, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get featured advertisements
    /// </summary>
    /// <returns>List of featured advertisements</returns>
    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetFeaturedAdvertisements()
    {
        var result = await _advertisementManagementService.GetFeaturedAdvertisementsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update advertisement sort order
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="sortOrder">New sort order</param>
    /// <returns>Operation result</returns>
    [HttpPatch("{id}/sort-order")]
    public async Task<ActionResult<ApiResponse>> UpdateAdvertisementSortOrder(int id, [FromBody] int sortOrder)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        if (sortOrder < 0)
        {
            return BadRequest(new { message = "Sort order cannot be negative" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.UpdateAdvertisementSortOrderAsync(id, sortOrder, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update advertisement discount
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="discountPercentage">Discount percentage</param>
    /// <param name="discountPrice">Discount price</param>
    /// <returns>Updated advertisement details</returns>
    [HttpPatch("{id}/discount")]
    public async Task<ActionResult<ApiResponse<AdminAdvertisementDto>>> UpdateAdvertisementDiscount(int id, [FromQuery] decimal? discountPercentage = null, [FromQuery] decimal? discountPrice = null)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        if (discountPercentage.HasValue && (discountPercentage < 0 || discountPercentage > 100))
        {
            return BadRequest(new { message = "Discount percentage must be between 0 and 100" });
        }

        if (discountPrice.HasValue && discountPrice < 0)
        {
            return BadRequest(new { message = "Discount price cannot be negative" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.UpdateAdvertisementDiscountAsync(id, discountPercentage, discountPrice, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisements with discounts
    /// </summary>
    /// <returns>List of advertisements with discounts</returns>
    [HttpGet("with-discounts")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetAdvertisementsWithDiscounts()
    {
        var result = await _advertisementManagementService.GetAdvertisementsWithDiscountsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Calculate discounted price
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="originalPrice">Original price</param>
    /// <returns>Discounted price</returns>
    [HttpGet("{id}/calculate-discount")]
    public async Task<ActionResult<ApiResponse<decimal>>> CalculateDiscountedPrice(int id, [FromQuery] decimal originalPrice)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        if (originalPrice <= 0)
        {
            return BadRequest(new { message = "Original price must be greater than 0" });
        }

        var result = await _advertisementManagementService.CalculateDiscountedPriceAsync(id, originalPrice);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisement analytics
    /// </summary>
    /// <param name="startDate">Start date for analytics</param>
    /// <param name="endDate">End date for analytics</param>
    /// <returns>Advertisement analytics data</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<AdvertisementAnalyticsDto>>> GetAdvertisementAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _advertisementManagementService.GetAdvertisementAnalyticsAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get top performing advertisements
    /// </summary>
    /// <param name="count">Number of top advertisements to retrieve</param>
    /// <param name="startDate">Start date for analysis</param>
    /// <param name="endDate">End date for analysis</param>
    /// <returns>List of top performing advertisements</returns>
    [HttpGet("analytics/top-performing")]
    public async Task<ActionResult<ApiResponse<List<AdvertisementPerformanceDto>>>> GetTopPerformingAdvertisements([FromQuery] int count = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (count < 1 || count > 100)
        {
            return BadRequest(new { message = "Count must be between 1 and 100" });
        }

        var result = await _advertisementManagementService.GetTopPerformingAdvertisementsAsync(count, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get low performing advertisements
    /// </summary>
    /// <param name="count">Number of low performing advertisements to retrieve</param>
    /// <param name="startDate">Start date for analysis</param>
    /// <param name="endDate">End date for analysis</param>
    /// <returns>List of low performing advertisements</returns>
    [HttpGet("analytics/low-performing")]
    public async Task<ActionResult<ApiResponse<List<AdvertisementPerformanceDto>>>> GetLowPerformingAdvertisements([FromQuery] int count = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (count < 1 || count > 100)
        {
            return BadRequest(new { message = "Count must be between 1 and 100" });
        }

        var result = await _advertisementManagementService.GetLowPerformingAdvertisementsAsync(count, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisement metrics
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Advertisement performance metrics</returns>
    [HttpGet("{id}/metrics")]
    public async Task<ActionResult<ApiResponse<List<AdvertisementMetricDto>>>> GetAdvertisementMetrics(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        var result = await _advertisementManagementService.GetAdvertisementMetricsAsync(id, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Record advertisement view
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="userId">Optional user ID</param>
    /// <param name="ipAddress">Optional IP address</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id}/view")]
    public async Task<ActionResult<ApiResponse>> RecordAdvertisementView(int id, [FromQuery] string? userId = null, [FromQuery] string? ipAddress = null)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        var result = await _advertisementManagementService.RecordAdvertisementViewAsync(id, userId, ipAddress);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Record advertisement click
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="userId">Optional user ID</param>
    /// <param name="ipAddress">Optional IP address</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id}/click")]
    public async Task<ActionResult<ApiResponse>> RecordAdvertisementClick(int id, [FromQuery] string? userId = null, [FromQuery] string? ipAddress = null)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        var result = await _advertisementManagementService.RecordAdvertisementClickAsync(id, userId, ipAddress);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Record advertisement conversion
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="userId">Optional user ID</param>
    /// <param name="conversionValue">Optional conversion value</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id}/conversion")]
    public async Task<ActionResult<ApiResponse>> RecordAdvertisementConversion(int id, [FromQuery] string? userId = null, [FromQuery] decimal? conversionValue = null)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        var result = await _advertisementManagementService.RecordAdvertisementConversionAsync(id, userId, conversionValue);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisement type statistics
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Advertisement type statistics</returns>
    [HttpGet("analytics/type-stats")]
    public async Task<ActionResult<ApiResponse<List<AdvertisementTypeStatsDto>>>> GetAdvertisementTypeStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _advertisementManagementService.GetAdvertisementTypeStatsAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisements by type
    /// </summary>
    /// <param name="type">Advertisement type</param>
    /// <param name="activeOnly">Filter for active advertisements only</param>
    /// <returns>List of advertisements by type</returns>
    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetAdvertisementsByType(AdvertisementType type, [FromQuery] bool activeOnly = true)
    {
        var result = await _advertisementManagementService.GetAdvertisementsByTypeAsync(type, activeOnly);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Perform bulk operations on advertisements
    /// </summary>
    /// <param name="operationDto">Bulk operation details</param>
    /// <returns>Operation result</returns>
    [HttpPost("bulk/operation")]
    public async Task<ActionResult<ApiResponse>> BulkOperation([FromBody] BulkAdvertisementOperationDto operationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.BulkOperationAsync(operationDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Copy advertisement
    /// </summary>
    /// <param name="copyDto">Copy advertisement data</param>
    /// <returns>Created advertisement details</returns>
    [HttpPost("copy")]
    public async Task<ActionResult<ApiResponse<AdminAdvertisementDto>>> CopyAdvertisement([FromBody] CopyAdvertisementDto copyDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _advertisementManagementService.CopyAdvertisementAsync(copyDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisement templates
    /// </summary>
    /// <returns>List of advertisement templates</returns>
    [HttpGet("templates")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetAdvertisementTemplates()
    {
        var result = await _advertisementManagementService.GetAdvertisementTemplatesAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisements by car
    /// </summary>
    /// <param name="carId">Car ID</param>
    /// <returns>List of advertisements for the car</returns>
    [HttpGet("by-car/{carId}")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetAdvertisementsByCar(int carId)
    {
        if (carId <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        var result = await _advertisementManagementService.GetAdvertisementsByCarAsync(carId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisements by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of advertisements for the category</returns>
    [HttpGet("by-category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetAdvertisementsByCategory(int categoryId)
    {
        if (categoryId <= 0)
        {
            return BadRequest(new { message = "Invalid category ID" });
        }

        var result = await _advertisementManagementService.GetAdvertisementsByCategoryAsync(categoryId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get advertisement revenue impact
    /// </summary>
    /// <param name="id">Advertisement ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Revenue impact amount</returns>
    [HttpGet("{id}/revenue-impact")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetAdvertisementRevenueImpact(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid advertisement ID" });
        }

        var result = await _advertisementManagementService.GetAdvertisementRevenueImpactAsync(id, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get monthly advertisement statistics
    /// </summary>
    /// <param name="year">Year for monthly stats</param>
    /// <returns>Monthly advertisement statistics</returns>
    [HttpGet("analytics/monthly/{year}")]
    public async Task<ActionResult<ApiResponse<List<MonthlyAdvertisementStatsDto>>>> GetMonthlyAdvertisementStats(int year)
    {
        if (year < 2020 || year > DateTime.UtcNow.Year + 1)
        {
            return BadRequest(new { message = "Invalid year" });
        }

        var result = await _advertisementManagementService.GetMonthlyAdvertisementStatsAsync(year);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get total advertisement revenue
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Total advertisement revenue</returns>
    [HttpGet("revenue/total")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalAdvertisementRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _advertisementManagementService.GetTotalAdvertisementRevenueAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Generate advertisement report
    /// </summary>
    /// <param name="filter">Filter parameters for the report</param>
    /// <returns>Comprehensive advertisement report</returns>
    [HttpPost("report")]
    public async Task<ActionResult<ApiResponse<AdvertisementReportDto>>> GenerateAdvertisementReport([FromBody] AdvertisementFilterDto filter)
    {
        var result = await _advertisementManagementService.GenerateAdvertisementReportAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Export advertisement report
    /// </summary>
    /// <param name="filter">Filter parameters for the export</param>
    /// <param name="format">Export format (excel, csv)</param>
    /// <returns>Export file</returns>
    [HttpPost("export")]
    public async Task<ActionResult> ExportAdvertisementReport([FromBody] AdvertisementFilterDto filter, [FromQuery] string format = "excel")
    {
        if (!new[] { "excel", "csv" }.Contains(format.ToLower()))
        {
            return BadRequest(new { message = "Format must be 'excel' or 'csv'" });
        }

        var result = await _advertisementManagementService.ExportAdvertisementReportAsync(filter, format.ToLower());

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCodeValue, result);
        }

        var contentType = format.ToLower() == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv";
        var fileName = $"advertisement-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{format.ToLower()}";

        return File(result.Data!, contentType, fileName);
    }

    /// <summary>
    /// Search advertisements
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of matching advertisements</returns>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> SearchAdvertisements([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(new { message = "Search term is required" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _advertisementManagementService.SearchAdvertisementsAsync(searchTerm, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get active advertisements
    /// </summary>
    /// <param name="date">Optional date filter</param>
    /// <returns>List of active advertisements</returns>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<AdminAdvertisementDto>>>> GetActiveAdvertisements([FromQuery] DateTime? date = null)
    {
        var result = await _advertisementManagementService.GetActiveAdvertisementsAsync(date);
        return StatusCode(result.StatusCodeValue, result);
    }
}
