using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.API.Controllers.Admin;

/// <summary>
/// Booking Management Controller - Admin operations for booking management
/// </summary>
[ApiController]
[Route("api/admin/bookings")]
[Authorize(Roles = "Admin,SuperAdmin,BranchManager")]
[Tags("Admin Booking Management")]
public class BookingManagementController : ControllerBase
{
    private readonly IBookingManagementService _bookingManagementService;

    public BookingManagementController(IBookingManagementService bookingManagementService)
    {
        _bookingManagementService = bookingManagementService;
    }

    /// <summary>
    /// Get all bookings with advanced filtering and pagination
    /// </summary>
    /// <param name="filter">Booking filter parameters</param>
    /// <returns>Paginated list of bookings with admin details</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminBookingDto>>>> GetBookings([FromQuery] BookingFilterDto filter)
    {
        var result = await _bookingManagementService.GetBookingsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get booking by ID with detailed admin information
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <returns>Booking details with payments and extras</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminBookingDto>>> GetBookingById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
        }

        var result = await _bookingManagementService.GetBookingByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Create a new booking
    /// </summary>
    /// <param name="createBookingDto">Booking creation data</param>
    /// <returns>Created booking details</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminBookingDto>>> CreateBooking([FromBody] AdminCreateBookingDto createBookingDto)
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

        var result = await _bookingManagementService.CreateBookingAsync(createBookingDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update an existing booking
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <param name="modifyBookingDto">Booking modification data</param>
    /// <returns>Updated booking details</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AdminBookingDto>>> UpdateBooking(int id, [FromBody] ModifyBookingDto modifyBookingDto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
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

        var result = await _bookingManagementService.UpdateBookingAsync(id, modifyBookingDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete a booking
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteBooking(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _bookingManagementService.DeleteBookingAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update booking status
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <param name="statusDto">Status update data</param>
    /// <returns>Updated booking details</returns>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<AdminBookingDto>>> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto statusDto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
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

        var result = await _bookingManagementService.UpdateBookingStatusAsync(id, statusDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Confirm a booking
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <returns>Confirmed booking details</returns>
    [HttpPost("{id}/confirm")]
    public async Task<ActionResult<ApiResponse<AdminBookingDto>>> ConfirmBooking(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _bookingManagementService.ConfirmBookingAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Start a booking (mark as in progress)
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <returns>Updated booking details</returns>
    [HttpPost("{id}/start")]
    public async Task<ActionResult<ApiResponse<AdminBookingDto>>> StartBooking(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _bookingManagementService.StartBookingAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Complete a booking
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <returns>Completed booking details</returns>
    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ApiResponse<AdminBookingDto>>> CompleteBooking(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _bookingManagementService.CompleteBookingAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <param name="cancellationReason">Reason for cancellation</param>
    /// <returns>Cancelled booking details</returns>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<AdminBookingDto>>> CancelBooking(int id, [FromBody] string cancellationReason)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
        }

        if (string.IsNullOrWhiteSpace(cancellationReason))
        {
            return BadRequest(new { message = "Cancellation reason is required" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _bookingManagementService.CancelBookingAsync(id, cancellationReason, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk update booking status
    /// </summary>
    /// <param name="bookingIds">List of booking IDs</param>
    /// <param name="status">New status for all bookings</param>
    /// <param name="notes">Optional notes</param>
    /// <returns>Operation result</returns>
    [HttpPatch("bulk/status")]
    public async Task<ActionResult<ApiResponse>> BulkUpdateBookingStatus([FromBody] List<int> bookingIds, [FromQuery] BookingStatus status, [FromQuery] string? notes = null)
    {
        if (bookingIds == null || !bookingIds.Any())
        {
            return BadRequest(new { message = "Booking IDs list cannot be empty" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _bookingManagementService.BulkUpdateBookingStatusAsync(bookingIds, status, adminId, notes);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Perform bulk operations on bookings
    /// </summary>
    /// <param name="operationDto">Bulk operation details</param>
    /// <returns>Operation result</returns>
    [HttpPost("bulk/operation")]
    public async Task<ActionResult<ApiResponse>> BulkOperation([FromBody] BulkBookingOperationDto operationDto)
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

        var result = await _bookingManagementService.BulkOperationAsync(operationDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get booking analytics
    /// </summary>
    /// <param name="startDate">Start date for analytics</param>
    /// <param name="endDate">End date for analytics</param>
    /// <param name="branchId">Optional branch filter</param>
    /// <returns>Booking analytics data</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<BookingAnalyticsDto>>> GetBookingAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int? branchId = null)
    {
        var result = await _bookingManagementService.GetBookingAnalyticsAsync(startDate, endDate, branchId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Generate booking report
    /// </summary>
    /// <param name="filter">Filter parameters for the report</param>
    /// <returns>Comprehensive booking report</returns>
    [HttpPost("report")]
    public async Task<ActionResult<ApiResponse<BookingReportDto>>> GenerateBookingReport([FromBody] BookingFilterDto filter)
    {
        var result = await _bookingManagementService.GenerateBookingReportAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Export booking report
    /// </summary>
    /// <param name="filter">Filter parameters for the export</param>
    /// <param name="format">Export format (excel, csv)</param>
    /// <returns>Export file</returns>
    [HttpPost("export")]
    public async Task<ActionResult> ExportBookingReport([FromBody] BookingFilterDto filter, [FromQuery] string format = "excel")
    {
        if (!new[] { "excel", "csv" }.Contains(format.ToLower()))
        {
            return BadRequest(new { message = "Format must be 'excel' or 'csv'" });
        }

        var result = await _bookingManagementService.ExportBookingReportAsync(filter, format.ToLower());

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCodeValue, result);
        }

        var contentType = format.ToLower() == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv";
        var fileName = $"booking-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{format.ToLower()}";

        return File(result.Data!, contentType, fileName);
    }

    /// <summary>
    /// Get booking payments
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <returns>List of payments for the booking</returns>
    [HttpGet("{id}/payments")]
    public async Task<ActionResult<ApiResponse<List<PaymentDetailDto>>>> GetBookingPayments(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
        }

        var result = await _bookingManagementService.GetBookingPaymentsAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Add payment to booking
    /// </summary>
    /// <param name="id">Booking ID</param>
    /// <param name="paymentDto">Payment details</param>
    /// <returns>Created payment details</returns>
    [HttpPost("{id}/payments")]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> AddBookingPayment(int id, [FromBody] PaymentDetailDto paymentDto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid booking ID" });
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

        var result = await _bookingManagementService.AddBookingPaymentAsync(id, paymentDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update payment status
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="status">New payment status</param>
    /// <returns>Updated payment details</returns>
    [HttpPatch("payments/{paymentId}/status")]
    public async Task<ActionResult<ApiResponse<PaymentDetailDto>>> UpdatePaymentStatus(int paymentId, [FromBody] PaymentStatus status)
    {
        if (paymentId <= 0)
        {
            return BadRequest(new { message = "Invalid payment ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _bookingManagementService.UpdatePaymentStatusAsync(paymentId, status, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get total revenue
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="branchId">Optional branch filter</param>
    /// <returns>Total revenue amount</returns>
    [HttpGet("revenue/total")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotalRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] int? branchId = null)
    {
        var result = await _bookingManagementService.GetTotalRevenueAsync(startDate, endDate, branchId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get monthly revenue statistics
    /// </summary>
    /// <param name="year">Year for monthly stats</param>
    /// <param name="branchId">Optional branch filter</param>
    /// <returns>Monthly revenue breakdown</returns>
    [HttpGet("revenue/monthly")]
    public async Task<ActionResult<ApiResponse<List<MonthlyBookingStatsDto>>>> GetMonthlyRevenue([FromQuery] int year, [FromQuery] int? branchId = null)
    {
        if (year < 2020 || year > DateTime.UtcNow.Year + 1)
        {
            return BadRequest(new { message = "Invalid year" });
        }

        var result = await _bookingManagementService.GetMonthlyRevenueAsync(year, branchId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get daily revenue for date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="branchId">Optional branch filter</param>
    /// <returns>Daily revenue breakdown</returns>
    [HttpGet("revenue/daily")]
    public async Task<ActionResult<ApiResponse<List<DailyRevenueDto>>>> GetDailyRevenue([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? branchId = null)
    {
        if (startDate >= endDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        if ((endDate - startDate).TotalDays > 365)
        {
            return BadRequest(new { message = "Date range cannot exceed 365 days" });
        }

        var result = await _bookingManagementService.GetDailyRevenueAsync(startDate, endDate, branchId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get peak times analysis
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Peak times statistics</returns>
    [HttpGet("analytics/peak-times")]
    public async Task<ActionResult<ApiResponse<List<PeakTimeStatsDto>>>> GetPeakTimesAnalysis([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _bookingManagementService.GetPeakTimesAnalysisAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get popular cars analysis
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Popular cars statistics</returns>
    [HttpGet("analytics/popular-cars")]
    public async Task<ActionResult<ApiResponse<List<PopularCarStatsDto>>>> GetPopularCarsAnalysis([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _bookingManagementService.GetPopularCarsAnalysisAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branch performance analysis
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Branch performance statistics</returns>
    [HttpGet("analytics/branch-performance")]
    public async Task<ActionResult<ApiResponse<List<BranchBookingStatsDto>>>> GetBranchPerformance([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _bookingManagementService.GetBranchPerformanceAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Validate booking dates
    /// </summary>
    /// <param name="carId">Car ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="excludeBookingId">Booking ID to exclude from validation</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate/dates")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateBookingDates([FromQuery] int carId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? excludeBookingId = null)
    {
        if (carId <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        if (startDate >= endDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        var result = await _bookingManagementService.ValidateBookingDatesAsync(carId, startDate, endDate, excludeBookingId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Calculate booking cost
    /// </summary>
    /// <param name="carId">Car ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="extras">Optional extras</param>
    /// <returns>Calculated booking cost</returns>
    [HttpPost("calculate-cost")]
    public async Task<ActionResult<ApiResponse<decimal>>> CalculateBookingCost([FromQuery] int carId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromBody] List<AdminBookingExtraDto>? extras = null)
    {
        if (carId <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        if (startDate >= endDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        var result = await _bookingManagementService.CalculateBookingCostAsync(carId, startDate, endDate, extras);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get available cars for date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="branchId">Optional branch filter</param>
    /// <returns>List of available car IDs</returns>
    [HttpGet("available-cars")]
    public async Task<ActionResult<ApiResponse<List<int>>>> GetAvailableCars([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? branchId = null)
    {
        if (startDate >= endDate)
        {
            return BadRequest(new { message = "Start date must be before end date" });
        }

        var result = await _bookingManagementService.GetAvailableCarsAsync(startDate, endDate, branchId);
        return StatusCode(result.StatusCodeValue, result);
    }
}
