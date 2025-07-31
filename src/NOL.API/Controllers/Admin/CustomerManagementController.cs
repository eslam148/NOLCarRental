using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;

namespace NOL.API.Controllers.Admin;

/// <summary>
/// Customer Management Controller - Admin operations for customer management
/// </summary>
[ApiController]
[Route("api/admin/customers")]
[Authorize(Roles = "Admin,SuperAdmin,BranchManager")]
[Tags("Admin Customer Management")]
public class CustomerManagementController : ControllerBase
{
    private readonly ICustomerManagementService _customerManagementService;

    public CustomerManagementController(ICustomerManagementService customerManagementService)
    {
        _customerManagementService = customerManagementService;
    }

    /// <summary>
    /// Get all customers with advanced filtering and pagination
    /// </summary>
    /// <param name="filter">Customer filter parameters</param>
    /// <returns>Paginated list of customers with admin details</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminCustomerDto>>>> GetCustomers([FromQuery] CustomerFilterDto filter)
    {
        var result = await _customerManagementService.GetCustomersAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer by ID with detailed admin information
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer details with analytics</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminCustomerDto>>> GetCustomerById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var result = await _customerManagementService.GetCustomerByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update customer information
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="updateCustomerDto">Customer update data</param>
    /// <returns>Updated customer details</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AdminCustomerDto>>> UpdateCustomer(string id, [FromBody] UpdateCustomerDto updateCustomerDto)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
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

        var result = await _customerManagementService.UpdateCustomerAsync(id, updateCustomerDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCustomer(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _customerManagementService.DeleteCustomerAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Activate customer account
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Updated customer details</returns>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult<ApiResponse<AdminCustomerDto>>> ActivateCustomer(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _customerManagementService.ActivateCustomerAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Deactivate customer account
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Updated customer details</returns>
    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult<ApiResponse<AdminCustomerDto>>> DeactivateCustomer(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _customerManagementService.DeactivateCustomerAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk update customer status
    /// </summary>
    /// <param name="customerIds">List of customer IDs</param>
    /// <param name="isActive">New active status</param>
    /// <returns>Operation result</returns>
    [HttpPatch("bulk/status")]
    public async Task<ActionResult<ApiResponse>> BulkUpdateCustomerStatus([FromBody] List<string> customerIds, [FromQuery] bool isActive)
    {
        if (customerIds == null || !customerIds.Any())
        {
            return BadRequest(new { message = "Customer IDs list cannot be empty" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _customerManagementService.BulkUpdateCustomerStatusAsync(customerIds, isActive, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer analytics
    /// </summary>
    /// <param name="startDate">Start date for analytics</param>
    /// <param name="endDate">End date for analytics</param>
    /// <returns>Customer analytics data</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<CustomerAnalyticsDto>>> GetCustomerAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _customerManagementService.GetCustomerAnalyticsAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get top customers
    /// </summary>
    /// <param name="count">Number of top customers to retrieve</param>
    /// <param name="startDate">Start date for analysis</param>
    /// <param name="endDate">End date for analysis</param>
    /// <returns>List of top customers</returns>
    [HttpGet("top")]
    public async Task<ActionResult<ApiResponse<List<TopCustomerDto>>>> GetTopCustomers([FromQuery] int count = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (count < 1 || count > 100)
        {
            return BadRequest(new { message = "Count must be between 1 and 100" });
        }

        var result = await _customerManagementService.GetTopCustomersAsync(count, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer segmentation
    /// </summary>
    /// <returns>Customer segment statistics</returns>
    [HttpGet("segmentation")]
    public async Task<ActionResult<ApiResponse<List<CustomerSegmentStatsDto>>>> GetCustomerSegmentation()
    {
        var result = await _customerManagementService.GetCustomerSegmentationAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer booking history
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Customer booking history</returns>
    [HttpGet("{id}/bookings")]
    public async Task<ActionResult<ApiResponse<List<CustomerBookingSummaryDto>>>> GetCustomerBookingHistory(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _customerManagementService.GetCustomerBookingHistoryAsync(id, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer lifetime value
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer lifetime value</returns>
    [HttpGet("{id}/lifetime-value")]
    public async Task<ActionResult<ApiResponse<decimal>>> GetCustomerLifetimeValue(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var result = await _customerManagementService.GetCustomerLifetimeValueAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer satisfaction rating
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer satisfaction rating</returns>
    [HttpGet("{id}/satisfaction")]
    public async Task<ActionResult<ApiResponse<double>>> GetCustomerSatisfactionRating(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var result = await _customerManagementService.GetCustomerSatisfactionRatingAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer loyalty transactions
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Customer loyalty point transactions</returns>
    [HttpGet("{id}/loyalty-transactions")]
    public async Task<ActionResult<ApiResponse<List<CustomerLoyaltyTransactionDto>>>> GetCustomerLoyaltyTransactions(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _customerManagementService.GetCustomerLoyaltyTransactionsAsync(id, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Award loyalty points to customer
    /// </summary>
    /// <param name="loyaltyPointsDto">Loyalty points award data</param>
    /// <returns>Created loyalty transaction</returns>
    [HttpPost("loyalty-points/award")]
    public async Task<ActionResult<ApiResponse<CustomerLoyaltyTransactionDto>>> AwardLoyaltyPoints([FromBody] ManageLoyaltyPointsDto loyaltyPointsDto)
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

        var result = await _customerManagementService.AwardLoyaltyPointsAsync(loyaltyPointsDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Deduct loyalty points from customer
    /// </summary>
    /// <param name="loyaltyPointsDto">Loyalty points deduction data</param>
    /// <returns>Created loyalty transaction</returns>
    [HttpPost("loyalty-points/deduct")]
    public async Task<ActionResult<ApiResponse<CustomerLoyaltyTransactionDto>>> DeductLoyaltyPoints([FromBody] ManageLoyaltyPointsDto loyaltyPointsDto)
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

        var result = await _customerManagementService.DeductLoyaltyPointsAsync(loyaltyPointsDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer available loyalty points
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Available loyalty points</returns>
    [HttpGet("{id}/loyalty-points/available")]
    public async Task<ActionResult<ApiResponse<int>>> GetCustomerAvailablePoints(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var result = await _customerManagementService.GetCustomerAvailablePointsAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Expire customer loyalty points
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id}/loyalty-points/expire")]
    public async Task<ActionResult<ApiResponse>> ExpireLoyaltyPoints(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _customerManagementService.ExpireLoyaltyPointsAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Send notification to customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <param name="notificationDto">Notification data</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id}/notification")]
    public async Task<ActionResult<ApiResponse>> SendNotificationToCustomer(string id, [FromBody] SendCustomerNotificationDto notificationDto)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Override the customer IDs with the single customer ID from the route
        notificationDto.CustomerIds = new List<string> { id };

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _customerManagementService.SendNotificationToCustomerAsync(id, notificationDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Send bulk notification to customers
    /// </summary>
    /// <param name="notificationDto">Bulk notification data</param>
    /// <returns>Operation result</returns>
    [HttpPost("bulk/notification")]
    public async Task<ActionResult<ApiResponse>> SendBulkNotification([FromBody] SendCustomerNotificationDto notificationDto)
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

        var result = await _customerManagementService.SendBulkNotificationAsync(notificationDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Send welcome email to customer
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id}/welcome-email")]
    public async Task<ActionResult<ApiResponse>> SendWelcomeEmail(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _customerManagementService.SendWelcomeEmailAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer segment
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer segment</returns>
    [HttpGet("{id}/segment")]
    public async Task<ActionResult<ApiResponse<string>>> GetCustomerSegment(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid customer ID" });
        }

        var result = await _customerManagementService.GetCustomerSegmentAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customers by segment
    /// </summary>
    /// <param name="segment">Customer segment</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of customers in segment</returns>
    [HttpGet("segment/{segment}")]
    public async Task<ActionResult<ApiResponse<List<AdminCustomerDto>>>> GetCustomersBySegment(string segment, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            return BadRequest(new { message = "Invalid segment" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _customerManagementService.GetCustomersBySegmentAsync(segment, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get available customer segments
    /// </summary>
    /// <returns>List of available segments</returns>
    [HttpGet("segments")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableCustomerSegments()
    {
        var result = await _customerManagementService.GetAvailableCustomerSegmentsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Perform bulk operations on customers
    /// </summary>
    /// <param name="operationDto">Bulk operation details</param>
    /// <returns>Operation result</returns>
    [HttpPost("bulk/operation")]
    public async Task<ActionResult<ApiResponse>> BulkOperation([FromBody] BulkCustomerOperationDto operationDto)
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

        var result = await _customerManagementService.BulkOperationAsync(operationDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Generate customer report
    /// </summary>
    /// <param name="filter">Filter parameters for the report</param>
    /// <returns>Comprehensive customer report</returns>
    [HttpPost("report")]
    public async Task<ActionResult<ApiResponse<CustomerReportDto>>> GenerateCustomerReport([FromBody] CustomerFilterDto filter)
    {
        var result = await _customerManagementService.GenerateCustomerReportAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Export customer report
    /// </summary>
    /// <param name="filter">Filter parameters for the export</param>
    /// <param name="format">Export format (excel, csv)</param>
    /// <returns>Export file</returns>
    [HttpPost("export")]
    public async Task<ActionResult> ExportCustomerReport([FromBody] CustomerFilterDto filter, [FromQuery] string format = "excel")
    {
        if (!new[] { "excel", "csv" }.Contains(format.ToLower()))
        {
            return BadRequest(new { message = "Format must be 'excel' or 'csv'" });
        }

        var result = await _customerManagementService.ExportCustomerReportAsync(filter, format.ToLower());

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCodeValue, result);
        }

        var contentType = format.ToLower() == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv";
        var fileName = $"customer-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{format.ToLower()}";

        return File(result.Data!, contentType, fileName);
    }

    /// <summary>
    /// Get customer retention rate
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Customer retention rate</returns>
    [HttpGet("retention-rate")]
    public async Task<ActionResult<ApiResponse<double>>> GetCustomerRetentionRate([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _customerManagementService.GetCustomerRetentionRateAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customers at risk of churning
    /// </summary>
    /// <param name="count">Number of customers to retrieve</param>
    /// <returns>List of churn risk customers</returns>
    [HttpGet("churn-risk")]
    public async Task<ActionResult<ApiResponse<List<AdminCustomerDto>>>> GetChurnRiskCustomers([FromQuery] int count = 50)
    {
        if (count < 1 || count > 200)
        {
            return BadRequest(new { message = "Count must be between 1 and 200" });
        }

        var result = await _customerManagementService.GetChurnRiskCustomersAsync(count);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get inactive customers
    /// </summary>
    /// <param name="daysSinceLastActivity">Days since last activity</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of inactive customers</returns>
    [HttpGet("inactive")]
    public async Task<ActionResult<ApiResponse<List<AdminCustomerDto>>>> GetInactiveCustomers([FromQuery] int daysSinceLastActivity = 90, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (daysSinceLastActivity < 1)
        {
            return BadRequest(new { message = "Days since last activity must be greater than 0" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _customerManagementService.GetInactiveCustomersAsync(daysSinceLastActivity, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Search customers
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of matching customers</returns>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<AdminCustomerDto>>>> SearchCustomers([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(new { message = "Search term is required" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _customerManagementService.SearchCustomersAsync(searchTerm, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer by email
    /// </summary>
    /// <param name="email">Customer email</param>
    /// <returns>Customer details</returns>
    [HttpGet("by-email")]
    public async Task<ActionResult<ApiResponse<AdminCustomerDto>>> GetCustomerByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        var result = await _customerManagementService.GetCustomerByEmailAsync(email);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get customer by phone
    /// </summary>
    /// <param name="phone">Customer phone</param>
    /// <returns>Customer details</returns>
    [HttpGet("by-phone")]
    public async Task<ActionResult<ApiResponse<AdminCustomerDto>>> GetCustomerByPhone([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new { message = "Phone is required" });
        }

        var result = await _customerManagementService.GetCustomerByPhoneAsync(phone);
        return StatusCode(result.StatusCodeValue, result);
    }
}
