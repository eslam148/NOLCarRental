using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Application.DTOs.Common;
using NOL.Domain.Enums;

namespace NOL.API.Controllers.Admin;

/// <summary>
/// Branch Management Controller - Admin operations for branch management
/// </summary>
[ApiController]
[Route("api/admin/branches")]
[Authorize(Roles = "Admin,SuperAdmin,BranchManager")]
[Tags("Admin Branch Management")]
public class BranchManagementController : ControllerBase
{
    private readonly IBranchManagementService _branchManagementService;

    public BranchManagementController(IBranchManagementService branchManagementService)
    {
        _branchManagementService = branchManagementService;
    }

    #region CRUD Operations

    /// <summary>
    /// Get all branches with optional filtering
    /// </summary>
    /// <param name="searchTerm">Search term for branch name, city, country, or address</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="city">Filter by city</param>
    /// <param name="country">Filter by country</param>
    /// <param name="sortBy">Sort field (name, city, country, createdat)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of branches with metadata</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponseDto<AdminBranchDto>>>> GetBranches(
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new BranchFilterDto
        {
            SearchTerm = searchTerm,
            IsActive = isActive,
            City = city,
            Country = country,
            SortBy = sortBy,
            SortOrder = sortOrder,
            Page = page,
            PageSize = pageSize
        };

        var result = await _branchManagementService.GetBranchesAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branch by ID with complete admin details
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <returns>Branch details with statistics</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminBranchDto>>> GetBranchById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var result = await _branchManagementService.GetBranchByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branches with advanced filtering, sorting, and pagination
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filtered list of branches</returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<List<AdminBranchDto>>>> GetBranches([FromBody] BranchFilterDto filter)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _branchManagementService.GetBranchesAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Create a new branch
    /// </summary>
    /// <param name="createBranchDto">Branch creation data</param>
    /// <returns>Created branch details</returns>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<AdminBranchDto>>> CreateBranch([FromBody] AdminCreateBranchDto createBranchDto)
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

        var result = await _branchManagementService.CreateBranchAsync(createBranchDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update branch information
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <param name="updateBranchDto">Branch update data</param>
    /// <returns>Updated branch details</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<AdminBranchDto>>> UpdateBranch(int id, [FromBody] AdminUpdateBranchDto updateBranchDto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
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

        var result = await _branchManagementService.UpdateBranchAsync(id, updateBranchDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete branch (soft delete with validation)
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <returns>Operation result</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> DeleteBranch(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _branchManagementService.DeleteBranchAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Branch Status Management

    /// <summary>
    /// Activate branch
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <returns>Updated branch details</returns>
    [HttpPost("{id}/activate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<AdminBranchDto>>> ActivateBranch(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _branchManagementService.ActivateBranchAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Deactivate branch (with active bookings/cars validation)
    /// </summary>
    /// <param name="id">Branch ID</param>
    /// <returns>Updated branch details</returns>
    [HttpPost("{id}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<AdminBranchDto>>> DeactivateBranch(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _branchManagementService.DeactivateBranchAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk update branch status
    /// </summary>
    /// <param name="branchIds">List of branch IDs</param>
    /// <param name="isActive">New status</param>
    /// <returns>Operation result</returns>
    [HttpPost("bulk/status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse>> BulkUpdateBranchStatus([FromBody] List<int> branchIds, [FromQuery] bool isActive)
    {
        if (!branchIds.Any())
        {
            return BadRequest(new { message = "No branch IDs provided" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _branchManagementService.BulkUpdateBranchStatusAsync(branchIds, isActive, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Branch Analytics

    /// <summary>
    /// Get comprehensive branch analytics
    /// </summary>
    /// <param name="startDate">Start date for analytics</param>
    /// <param name="endDate">End date for analytics</param>
    /// <returns>Branch analytics data</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<BranchAnalyticsDto>>> GetBranchAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _branchManagementService.GetBranchAnalyticsAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get top performing branches
    /// </summary>
    /// <param name="count">Number of branches to return</param>
    /// <param name="startDate">Start date for performance calculation</param>
    /// <param name="endDate">End date for performance calculation</param>
    /// <returns>Top performing branches</returns>
    [HttpGet("top-performing")]
    public async Task<ActionResult<ApiResponse<List<BranchPerformanceDto>>>> GetTopPerformingBranches([FromQuery] int count = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _branchManagementService.GetTopPerformingBranchesAsync(count, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get low performing branches
    /// </summary>
    /// <param name="count">Number of branches to return</param>
    /// <param name="startDate">Start date for performance calculation</param>
    /// <param name="endDate">End date for performance calculation</param>
    /// <returns>Low performing branches</returns>
    [HttpGet("low-performing")]
    public async Task<ActionResult<ApiResponse<List<BranchPerformanceDto>>>> GetLowPerformingBranches([FromQuery] int count = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _branchManagementService.GetLowPerformingBranchesAsync(count, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branch performance comparison
    /// </summary>
    /// <param name="comparisonDto">Comparison criteria</param>
    /// <returns>Branch comparison results</returns>
    [HttpPost("compare")]
    public async Task<ActionResult<ApiResponse<BranchComparisonResultDto>>> CompareBranchPerformance([FromBody] BranchComparisonDto comparisonDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _branchManagementService.CompareBranchPerformanceAsync(comparisonDto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branch revenue analysis
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="startDate">Start date for analysis</param>
    /// <param name="endDate">End date for analysis</param>
    /// <returns>Branch revenue analysis</returns>
    [HttpGet("{branchId}/revenue-analysis")]
    public async Task<ActionResult<ApiResponse<BranchRevenueAnalysisDto>>> GetBranchRevenueAnalysis(int branchId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (branchId <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var result = await _branchManagementService.GetBranchRevenueAnalysisAsync(branchId, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Staff Management

    /// <summary>
    /// Get branch staff members
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of staff assigned to branch</returns>
    [HttpGet("{branchId}/staff")]
    public async Task<ActionResult<ApiResponse<List<BranchStaffDto>>>> GetBranchStaff(int branchId)
    {
        if (branchId <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var result = await _branchManagementService.GetBranchStaffAsync(branchId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Assign staff to branch
    /// </summary>
    /// <param name="assignmentDto">Staff assignment data</param>
    /// <returns>Operation result</returns>
    [HttpPost("staff/assign")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse>> AssignStaffToBranch([FromBody] BranchStaffAssignmentDto assignmentDto)
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

        var result = await _branchManagementService.AssignStaffToBranchAsync(assignmentDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Remove staff from branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="staffIds">List of staff IDs to remove</param>
    /// <returns>Operation result</returns>
    [HttpDelete("{branchId}/staff")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse>> RemoveStaffFromBranch(int branchId, [FromBody] List<string> staffIds)
    {
        if (branchId <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        if (!staffIds.Any())
        {
            return BadRequest(new { message = "No staff IDs provided" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _branchManagementService.RemoveStaffFromBranchAsync(branchId, staffIds, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Car Fleet Management

    /// <summary>
    /// Get cars assigned to branch
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="status">Optional car status filter</param>
    /// <returns>List of cars in branch</returns>
    [HttpGet("{branchId}/cars")]
    public async Task<ActionResult<ApiResponse<List<AdminCarDto>>>> GetBranchCars(int branchId, [FromQuery] CarStatus? status = null)
    {
        if (branchId <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var result = await _branchManagementService.GetBranchCarsAsync(branchId, status);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Transfer cars between branches
    /// </summary>
    /// <param name="transferDto">Car transfer data</param>
    /// <returns>Operation result</returns>
    [HttpPost("cars/transfer")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse>> TransferCarsBetweenBranches([FromBody] BranchCarTransferDto transferDto)
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

        var result = await _branchManagementService.TransferCarsBetweenBranchesAsync(transferDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branch car utilization statistics
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <returns>Car utilization statistics</returns>
    [HttpGet("{branchId}/car-utilization")]
    public async Task<ActionResult<ApiResponse<BranchCarStatsDto>>> GetBranchCarUtilization(int branchId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (branchId <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var result = await _branchManagementService.GetBranchCarUtilizationAsync(branchId, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Booking Management

    /// <summary>
    /// Get branch booking statistics
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <returns>Booking statistics</returns>
    [HttpGet("{branchId}/booking-stats")]
    public async Task<ActionResult<ApiResponse<BranchBookingStatsDto>>> GetBranchBookingStats(int branchId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (branchId <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var result = await _branchManagementService.GetBranchBookingStatsAsync(branchId, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branches by booking volume
    /// </summary>
    /// <param name="startDate">Start date for analysis</param>
    /// <param name="endDate">End date for analysis</param>
    /// <returns>Branches ordered by booking volume</returns>
    [HttpGet("by-booking-volume")]
    public async Task<ActionResult<ApiResponse<List<BranchPerformanceDto>>>> GetBranchesByBookingVolume([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _branchManagementService.GetBranchesByBookingVolumeAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Geographic Analytics

    /// <summary>
    /// Get branch statistics by city
    /// </summary>
    /// <returns>City-wise branch statistics</returns>
    [HttpGet("stats/by-city")]
    public async Task<ActionResult<ApiResponse<List<BranchCityStatsDto>>>> GetBranchStatsByCity()
    {
        var result = await _branchManagementService.GetBranchStatsByCityAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branch statistics by country
    /// </summary>
    /// <returns>Country-wise branch statistics</returns>
    [HttpGet("stats/by-country")]
    public async Task<ActionResult<ApiResponse<List<BranchCountryStatsDto>>>> GetBranchStatsByCountry()
    {
        var result = await _branchManagementService.GetBranchStatsByCountryAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branches within radius of location
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <param name="radiusKm">Radius in kilometers</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated nearby branches</returns>
    [HttpGet("nearby")]
    public async Task<ActionResult<ApiResponse<PaginatedResponseDto<AdminBranchDto>>>> GetBranchesNearLocation([FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] double radiusKm = 50, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _branchManagementService.GetBranchesNearLocationAsync(latitude, longitude, radiusKm, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Perform bulk operations on branches
    /// </summary>
    /// <param name="operationDto">Bulk operation details</param>
    /// <returns>Operation result</returns>
    [HttpPost("bulk/operation")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> BulkOperation([FromBody] BulkBranchOperationDto operationDto)
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

        var result = await _branchManagementService.BulkOperationAsync(operationDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Reports and Export

    /// <summary>
    /// Generate comprehensive branch report
    /// </summary>
    /// <param name="filter">Report filter criteria</param>
    /// <returns>Branch report</returns>
    [HttpPost("reports/generate")]
    public async Task<ActionResult<ApiResponse<BranchReportDto>>> GenerateBranchReport([FromBody] BranchFilterDto filter)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _branchManagementService.GenerateBranchReportAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Export branch report to file
    /// </summary>
    /// <param name="filter">Report filter criteria</param>
    /// <param name="format">Export format (excel/csv)</param>
    /// <returns>Exported file</returns>
    [HttpPost("reports/export")]
    public async Task<ActionResult> ExportBranchReport([FromBody] BranchFilterDto filter, [FromQuery] string format = "excel")
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _branchManagementService.ExportBranchReportAsync(filter, format);

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCodeValue, result);
        }

        var contentType = format.ToLower() == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var fileName = $"branch-report-{DateTime.UtcNow:yyyy-MM-dd}.{format}";

        return File(result.Data!, contentType, fileName);
    }

    #endregion

    #region Validation and Search

    /// <summary>
    /// Search branches by multiple criteria
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated search results</returns>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PaginatedResponseDto<AdminBranchDto>>>> SearchBranches([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(new { message = "Search term is required" });
        }

        var result = await _branchManagementService.SearchBranchesAsync(searchTerm, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get active branches only
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated active branches</returns>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<PaginatedResponseDto<AdminBranchDto>>>> GetActiveBranches([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _branchManagementService.GetActiveBranchesAsync(page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branches by geographic region
    /// </summary>
    /// <param name="city">City name</param>
    /// <param name="country">Country name</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated branches in specified region</returns>
    [HttpGet("by-region")]
    public async Task<ActionResult<ApiResponse<PaginatedResponseDto<AdminBranchDto>>>> GetBranchesByRegion([FromQuery] string city, [FromQuery] string country, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
        {
            return BadRequest(new { message = "City and country are required" });
        }

        var result = await _branchManagementService.GetBranchesByRegionAsync(city, country, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion
}
