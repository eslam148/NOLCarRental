using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;

namespace NOL.API.Controllers.Admin;

/// <summary>
/// Admin Dashboard Controller - Provides overall system metrics and analytics
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin,SuperAdmin,BranchManager")]
[Tags("Admin Dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get comprehensive dashboard statistics
    /// </summary>
    /// <param name="filter">Dashboard filter parameters</param>
    /// <returns>Complete dashboard statistics</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats([FromQuery] DashboardFilterDto filter)
    {
        var result = await _dashboardService.GetDashboardStatsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get overall system statistics
    /// </summary>
    /// <param name="filter">Dashboard filter parameters</param>
    /// <returns>Overall system metrics</returns>
    [HttpGet("stats/overall")]
    public async Task<ActionResult<ApiResponse<OverallStatsDto>>> GetOverallStats([FromQuery] DashboardFilterDto filter)
    {
        var result = await _dashboardService.GetOverallStatsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get revenue statistics and trends
    /// </summary>
    /// <param name="filter">Dashboard filter parameters</param>
    /// <returns>Revenue statistics</returns>
    [HttpGet("stats/revenue")]
    public async Task<ActionResult<ApiResponse<RevenueStatsDto>>> GetRevenueStats([FromQuery] DashboardFilterDto filter)
    {
        var result = await _dashboardService.GetRevenueStatsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get booking statistics and trends
    /// </summary>
    /// <param name="filter">Dashboard filter parameters</param>
    /// <returns>Booking statistics</returns>
    [HttpGet("stats/bookings")]
    public async Task<ActionResult<ApiResponse<BookingStatsDto>>> GetBookingStats([FromQuery] DashboardFilterDto filter)
    {
        var result = await _dashboardService.GetBookingStatsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get car fleet statistics
    /// </summary>
    /// <param name="filter">Dashboard filter parameters</param>
    /// <returns>Car fleet statistics</returns>
    [HttpGet("stats/cars")]
    public async Task<ActionResult<ApiResponse<CarStatsDto>>> GetCarStats([FromQuery] DashboardFilterDto filter)
    {
        var result = await _dashboardService.GetCarStatsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer statistics and analytics
    /// </summary>
    /// <param name="filter">Dashboard filter parameters</param>
    /// <returns>Customer statistics</returns>
    [HttpGet("stats/customers")]
    public async Task<ActionResult<ApiResponse<CustomerStatsDto>>> GetCustomerStats([FromQuery] DashboardFilterDto filter)
    {
        var result = await _dashboardService.GetCustomerStatsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get recent bookings for real-time monitoring
    /// </summary>
    /// <param name="count">Number of recent bookings to retrieve</param>
    /// <returns>List of recent bookings</returns>
    [HttpGet("recent-bookings")]
    public async Task<ActionResult<ApiResponse<List<RecentBookingDto>>>> GetRecentBookings([FromQuery] int count = 10)
    {
        if (count < 1 || count > 100)
        {
            return BadRequest(new { message = "Count must be between 1 and 100" });
        }

        var result = await _dashboardService.GetRecentBookingsAsync(count);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get popular cars based on bookings and revenue
    /// </summary>
    /// <param name="count">Number of popular cars to retrieve</param>
    /// <param name="filter">Optional filter parameters</param>
    /// <returns>List of popular cars</returns>
    [HttpGet("popular-cars")]
    public async Task<ActionResult<ApiResponse<List<PopularCarDto>>>> GetPopularCars(
        [FromQuery] int count = 10,
        [FromQuery] DashboardFilterDto? filter = null)
    {
        if (count < 1 || count > 50)
        {
            return BadRequest(new { message = "Count must be between 1 and 50" });
        }

        var result = await _dashboardService.GetPopularCarsAsync(count, filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Export dashboard report in specified format
    /// </summary>
    /// <param name="filter">Dashboard filter parameters</param>
    /// <param name="format">Export format (pdf, excel)</param>
    /// <returns>Dashboard report file</returns>
    [HttpPost("export")]
    public async Task<ActionResult> ExportDashboardReport([FromBody] DashboardFilterDto filter, [FromQuery] string format = "pdf")
    {
        if (!new[] { "pdf", "excel" }.Contains(format.ToLower()))
        {
            return BadRequest(new { message = "Format must be 'pdf' or 'excel'" });
        }

        var result = await _dashboardService.ExportDashboardReportAsync(filter, format.ToLower());
        
        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCodeValue, result);
        }

        var contentType = format.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var fileName = $"dashboard-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{format.ToLower()}";

        return File(result.Data!, contentType, fileName);
    }

    /// <summary>
    /// Get dashboard report download URL
    /// </summary>
    /// <param name="filter">Dashboard filter parameters</param>
    /// <param name="format">Export format (pdf, excel)</param>
    /// <returns>Download URL for the report</returns>
    [HttpPost("report-url")]
    public async Task<ActionResult<ApiResponse<string>>> GetDashboardReportUrl([FromBody] DashboardFilterDto filter, [FromQuery] string format = "pdf")
    {
        if (!new[] { "pdf", "excel" }.Contains(format.ToLower()))
        {
            return BadRequest(new { message = "Format must be 'pdf' or 'excel'" });
        }

        var result = await _dashboardService.GetDashboardReportUrlAsync(filter, format.ToLower());
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get real-time dashboard data (for auto-refresh)
    /// </summary>
    /// <returns>Real-time dashboard metrics</returns>
    [HttpGet("realtime")]
    public async Task<ActionResult<ApiResponse<object>>> GetRealtimeData()
    {
        var filter = new DashboardFilterDto
        {
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1),
            Period = "day"
        };

        var overallStatsTask = _dashboardService.GetOverallStatsAsync(filter);
        var recentBookingsTask = _dashboardService.GetRecentBookingsAsync(5);
        var popularCarsTask = _dashboardService.GetPopularCarsAsync(5, filter);

        await Task.WhenAll(overallStatsTask, recentBookingsTask, popularCarsTask);

        var realtimeData = new
        {
            OverallStats = overallStatsTask.Result.Data,
            RecentBookings = recentBookingsTask.Result.Data,
            PopularCars = popularCarsTask.Result.Data,
            LastUpdated = DateTime.UtcNow
        };

        return Ok(new ApiResponse<object>
        {
            Data = realtimeData,
            Succeeded = true,
            Message = "Realtime data retrieved successfully"
        });
    }

    /// <summary>
    /// Get dashboard data for specific date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="branchId">Optional branch filter</param>
    /// <returns>Dashboard data for date range</returns>
    [HttpGet("date-range")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardDataForDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? branchId = null)
    {
        if (startDate >= endDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        if ((endDate - startDate).TotalDays > 365)
        {
            return BadRequest(new { message = "Date range cannot exceed 365 days" });
        }

        var filter = new DashboardFilterDto
        {
            StartDate = startDate,
            EndDate = endDate,
            BranchId = branchId
        };

        var result = await _dashboardService.GetDashboardStatsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }
}
