using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.API.Controllers.Admin;

/// <summary>
/// Admin Management Controller - Admin operations for admin user management
/// </summary>
[ApiController]
[Route("api/admin/admins")]
[Authorize(Roles = "Admin,SuperAdmin")]
[Tags("Admin Management")]
public class AdminManagementController : ControllerBase
{
    private readonly IAdminManagementService _adminManagementService;

    public AdminManagementController(IAdminManagementService adminManagementService)
    {
        _adminManagementService = adminManagementService;
    }

    /// <summary>
    /// Get all admin users with advanced filtering and pagination
    /// </summary>
    /// <param name="filter">Admin filter parameters</param>
    /// <returns>Paginated list of admin users</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminUserDto>>>> GetAdmins([FromQuery] AdminFilterDto filter)
    {
        var result = await _adminManagementService.GetAdminsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get admin user by ID with detailed information
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>Admin user details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetAdminById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        var result = await _adminManagementService.GetAdminByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Create a new admin user
    /// </summary>
    /// <param name="createAdminDto">Admin creation data</param>
    /// <returns>Created admin user details</returns>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> CreateAdmin([FromBody] CreateAdminUserDto createAdminDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(createdByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.CreateAdminAsync(createAdminDto, createdByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update an existing admin user
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="updateAdminDto">Admin update data</param>
    /// <returns>Updated admin user details</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateAdmin(string id, [FromBody] UpdateAdminUserDto updateAdminDto)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(updatedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.UpdateAdminAsync(id, updateAdminDto, updatedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete an admin user
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> DeleteAdmin(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        var deletedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(deletedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.DeleteAdminAsync(id, deletedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Activate admin user account
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>Updated admin user details</returns>
    [HttpPost("{id}/activate")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> ActivateAdmin(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        var activatedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(activatedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.ActivateAdminAsync(id, activatedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Deactivate admin user account
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>Updated admin user details</returns>
    [HttpPost("{id}/deactivate")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> DeactivateAdmin(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        var deactivatedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(deactivatedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.DeactivateAdminAsync(id, deactivatedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk update admin status
    /// </summary>
    /// <param name="adminIds">List of admin IDs</param>
    /// <param name="isActive">New active status</param>
    /// <returns>Operation result</returns>
    [HttpPatch("bulk/status")]
    public async Task<ActionResult<ApiResponse>> BulkUpdateAdminStatus([FromBody] List<string> adminIds, [FromQuery] bool isActive)
    {
        if (adminIds == null || !adminIds.Any())
        {
            return BadRequest(new { message = "Admin IDs list cannot be empty" });
        }

        var updatedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(updatedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.BulkUpdateAdminStatusAsync(adminIds, isActive, updatedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update admin role
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="newRole">New role</param>
    /// <returns>Updated admin user details</returns>
    [HttpPatch("{id}/role")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> UpdateAdminRole(string id, [FromBody] UserRole newRole)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        var updatedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(updatedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.UpdateAdminRoleAsync(id, newRole, updatedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get admin permissions
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>List of admin permissions</returns>
    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<ApiResponse<List<AdminPermissionDto>>>> GetAdminPermissions(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        var result = await _adminManagementService.GetAdminPermissionsAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update admin permissions
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="permissions">List of permission names</param>
    /// <returns>Operation result</returns>
    [HttpPut("{id}/permissions")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> UpdateAdminPermissions(string id, [FromBody] List<string> permissions)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        if (permissions == null)
        {
            return BadRequest(new { message = "Permissions list cannot be null" });
        }

        var updatedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(updatedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.UpdateAdminPermissionsAsync(id, permissions, updatedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get available permissions
    /// </summary>
    /// <returns>List of available permissions</returns>
    [HttpGet("permissions/available")]
    public async Task<ActionResult<ApiResponse<List<AdminPermissionDto>>>> GetAvailablePermissions()
    {
        var result = await _adminManagementService.GetAvailablePermissionsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get admin assigned branches
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>List of assigned branch IDs</returns>
    [HttpGet("{id}/branches")]
    public async Task<ActionResult<ApiResponse<List<int>>>> GetAdminAssignedBranches(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        var result = await _adminManagementService.GetAdminAssignedBranchesAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update admin branch assignment
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="branchIds">List of branch IDs to assign</param>
    /// <returns>Operation result</returns>
    [HttpPut("{id}/branches")]
    public async Task<ActionResult<ApiResponse>> UpdateAdminBranchAssignment(string id, [FromBody] List<int> branchIds)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        if (branchIds == null)
        {
            return BadRequest(new { message = "Branch IDs list cannot be null" });
        }

        var updatedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(updatedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.UpdateAdminBranchAssignmentAsync(id, branchIds, updatedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get branch admins
    /// </summary>
    /// <param name="branchId">Branch ID</param>
    /// <returns>List of admins assigned to the branch</returns>
    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<ApiResponse<List<AdminUserDto>>>> GetBranchAdmins(int branchId)
    {
        if (branchId <= 0)
        {
            return BadRequest(new { message = "Invalid branch ID" });
        }

        var result = await _adminManagementService.GetBranchAdminsAsync(branchId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Change admin password
    /// </summary>
    /// <param name="changePasswordDto">Password change data</param>
    /// <returns>Operation result</returns>
    [HttpPost("change-password")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> ChangeAdminPassword([FromBody] ChangeAdminPasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var changedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(changedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.ChangeAdminPasswordAsync(changePasswordDto, changedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Reset admin password
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id}/reset-password")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> ResetAdminPassword(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        var resetByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(resetByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.ResetAdminPasswordAsync(id, resetByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <returns>Operation result</returns>
    [HttpPost("{id}/send-password-reset")]
    public async Task<ActionResult<ApiResponse>> SendPasswordResetEmail(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        var requestedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(requestedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.SendPasswordResetEmailAsync(id, requestedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get admin analytics
    /// </summary>
    /// <param name="startDate">Start date for analytics</param>
    /// <param name="endDate">End date for analytics</param>
    /// <returns>Admin analytics data</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<AdminAnalyticsDto>>> GetAdminAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _adminManagementService.GetAdminAnalyticsAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get admin role statistics
    /// </summary>
    /// <returns>Admin role statistics</returns>
    [HttpGet("analytics/roles")]
    public async Task<ActionResult<ApiResponse<List<AdminRoleStatsDto>>>> GetAdminRoleStats()
    {
        var result = await _adminManagementService.GetAdminRoleStatsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get admin activity statistics
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Admin activity statistics</returns>
    [HttpGet("analytics/activity")]
    public async Task<ActionResult<ApiResponse<List<AdminActivityStatsDto>>>> GetAdminActivityStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await _adminManagementService.GetAdminActivityStatsAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get admin activity logs
    /// </summary>
    /// <param name="filter">Audit log filter parameters</param>
    /// <returns>List of admin activity logs</returns>
    [HttpGet("activity-logs")]
    public async Task<ActionResult<ApiResponse<List<AdminActivityLogDto>>>> GetAdminActivityLogs([FromQuery] AuditLogFilterDto filter)
    {
        var result = await _adminManagementService.GetAdminActivityLogsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get admin activity logs by admin ID
    /// </summary>
    /// <param name="id">Admin user ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of activity logs for the admin</returns>
    [HttpGet("{id}/activity-logs")]
    public async Task<ActionResult<ApiResponse<List<AdminActivityLogDto>>>> GetAdminActivityLogsByAdmin(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new { message = "Invalid admin ID" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _adminManagementService.GetAdminActivityLogsByAdminAsync(id, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get system settings
    /// </summary>
    /// <returns>List of system settings</returns>
    [HttpGet("system-settings")]
    public async Task<ActionResult<ApiResponse<List<SystemSettingsDto>>>> GetSystemSettings()
    {
        var result = await _adminManagementService.GetSystemSettingsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get system setting by key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>System setting</returns>
    [HttpGet("system-settings/{key}")]
    public async Task<ActionResult<ApiResponse<SystemSettingsDto>>> GetSystemSettingByKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Invalid setting key" });
        }

        var result = await _adminManagementService.GetSystemSettingByKeyAsync(key);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update system setting
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="updateDto">Setting update data</param>
    /// <returns>Updated system setting</returns>
    [HttpPut("system-settings/{key}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SystemSettingsDto>>> UpdateSystemSetting(string key, [FromBody] UpdateSystemSettingDto updateDto)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Invalid setting key" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(updatedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.UpdateSystemSettingAsync(key, updateDto, updatedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Create system setting
    /// </summary>
    /// <param name="createDto">Setting creation data</param>
    /// <returns>Created system setting</returns>
    [HttpPost("system-settings")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SystemSettingsDto>>> CreateSystemSetting([FromBody] CreateSystemSettingDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(createdByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.CreateSystemSettingAsync(createDto, createdByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete system setting
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>Operation result</returns>
    [HttpDelete("system-settings/{key}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> DeleteSystemSetting(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new { message = "Invalid setting key" });
        }

        var deletedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(deletedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.DeleteSystemSettingAsync(key, deletedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Perform bulk operations on admins
    /// </summary>
    /// <param name="operationDto">Bulk operation details</param>
    /// <returns>Operation result</returns>
    [HttpPost("bulk/operation")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> BulkOperation([FromBody] BulkAdminOperationDto operationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var performedByAdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(performedByAdminId))
        {
            return Unauthorized();
        }

        var result = await _adminManagementService.BulkOperationAsync(operationDto, performedByAdminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Generate admin report
    /// </summary>
    /// <param name="filter">Filter parameters for the report</param>
    /// <returns>Comprehensive admin report</returns>
    [HttpPost("report")]
    public async Task<ActionResult<ApiResponse<AdminReportDto>>> GenerateAdminReport([FromBody] AdminFilterDto filter)
    {
        var result = await _adminManagementService.GenerateAdminReportAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Export admin report
    /// </summary>
    /// <param name="filter">Filter parameters for the export</param>
    /// <param name="format">Export format (excel, csv)</param>
    /// <returns>Export file</returns>
    [HttpPost("export")]
    public async Task<ActionResult> ExportAdminReport([FromBody] AdminFilterDto filter, [FromQuery] string format = "excel")
    {
        if (!new[] { "excel", "csv" }.Contains(format.ToLower()))
        {
            return BadRequest(new { message = "Format must be 'excel' or 'csv'" });
        }

        var result = await _adminManagementService.ExportAdminReportAsync(filter, format.ToLower());

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCodeValue, result);
        }

        var contentType = format.ToLower() == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv";
        var fileName = $"admin-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{format.ToLower()}";

        return File(result.Data!, contentType, fileName);
    }

    /// <summary>
    /// Search admins
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of matching admins</returns>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<AdminUserDto>>>> SearchAdmins([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(new { message = "Search term is required" });
        }

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _adminManagementService.SearchAdminsAsync(searchTerm, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get admin by email
    /// </summary>
    /// <param name="email">Admin email</param>
    /// <returns>Admin details</returns>
    [HttpGet("by-email")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetAdminByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email is required" });
        }

        var result = await _adminManagementService.GetAdminByEmailAsync(email);
        return StatusCode(result.StatusCodeValue, result);
    }
}
