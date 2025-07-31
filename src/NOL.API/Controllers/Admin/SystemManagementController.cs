using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;

namespace NOL.API.Controllers.Admin;

[ApiController]
[Route("api/admin/system")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class SystemManagementController : ControllerBase
{
    private readonly ISystemManagementService _systemManagementService;
    private readonly ILogger<SystemManagementController> _logger;

    public SystemManagementController(
        ISystemManagementService systemManagementService,
        ILogger<SystemManagementController> logger)
    {
        _systemManagementService = systemManagementService;
        _logger = logger;
    }

    #region System Configuration

    /// <summary>
    /// Get all system configurations
    /// </summary>
    [HttpGet("configurations")]
    public async Task<ActionResult<ApiResponse<List<SystemConfigDto>>>> GetSystemConfigurations()
    {
        var result = await _systemManagementService.GetSystemConfigurationsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get system configuration by key
    /// </summary>
    [HttpGet("configurations/{key}")]
    public async Task<ActionResult<ApiResponse<SystemConfigDto>>> GetSystemConfiguration(string key)
    {
        var result = await _systemManagementService.GetSystemConfigurationAsync(key);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Create system configuration
    /// </summary>
    [HttpPost("configurations")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SystemConfigDto>>> CreateSystemConfiguration([FromBody] CreateSystemConfigDto createDto)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.CreateSystemConfigurationAsync(createDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update system configuration
    /// </summary>
    [HttpPut("configurations/{key}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SystemConfigDto>>> UpdateSystemConfiguration(string key, [FromBody] UpdateSystemConfigDto updateDto)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.UpdateSystemConfigurationAsync(key, updateDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete system configuration
    /// </summary>
    [HttpDelete("configurations/{key}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> DeleteSystemConfiguration(string key)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.DeleteSystemConfigurationAsync(key, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk update system configurations
    /// </summary>
    [HttpPut("configurations/bulk")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> BulkUpdateConfigurations([FromBody] List<UpdateSystemConfigDto> configurations)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.BulkUpdateConfigurationsAsync(configurations, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Application Settings

    /// <summary>
    /// Get application settings
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<ApplicationSettingsDto>>> GetApplicationSettings()
    {
        var result = await _systemManagementService.GetApplicationSettingsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update application settings
    /// </summary>
    [HttpPut("settings")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<ApplicationSettingsDto>>> UpdateApplicationSettings([FromBody] UpdateApplicationSettingsDto updateDto)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.UpdateApplicationSettingsAsync(updateDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Reset application settings to default
    /// </summary>
    [HttpPost("settings/reset")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<ApplicationSettingsDto>>> ResetApplicationSettings()
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.ResetApplicationSettingsAsync(adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region System Health

    /// <summary>
    /// Get system health status
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<ApiResponse<SystemHealthDto>>> GetSystemHealth()
    {
        var result = await _systemManagementService.GetSystemHealthAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get system performance metrics
    /// </summary>
    [HttpGet("performance")]
    public async Task<ActionResult<ApiResponse<SystemPerformanceDto>>> GetSystemPerformance()
    {
        var result = await _systemManagementService.GetSystemPerformanceAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get database health status
    /// </summary>
    [HttpGet("health/database")]
    public async Task<ActionResult<ApiResponse<DatabaseHealthDto>>> GetDatabaseHealth()
    {
        var result = await _systemManagementService.GetDatabaseHealthAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get external services status
    /// </summary>
    [HttpGet("health/external-services")]
    public async Task<ActionResult<ApiResponse<List<ExternalServiceStatusDto>>>> GetExternalServicesStatus()
    {
        var result = await _systemManagementService.GetExternalServicesStatusAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region System Maintenance

    /// <summary>
    /// Enable maintenance mode
    /// </summary>
    [HttpPost("maintenance/enable")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> EnableMaintenanceMode([FromBody] MaintenanceModeDto maintenanceDto)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.EnableMaintenanceModeAsync(maintenanceDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Disable maintenance mode
    /// </summary>
    [HttpPost("maintenance/disable")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> DisableMaintenanceMode()
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.DisableMaintenanceModeAsync(adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get maintenance mode status
    /// </summary>
    [HttpGet("maintenance/status")]
    public async Task<ActionResult<ApiResponse<MaintenanceModeStatusDto>>> GetMaintenanceModeStatus()
    {
        var result = await _systemManagementService.GetMaintenanceModeStatusAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Schedule system maintenance
    /// </summary>
    [HttpPost("maintenance/schedule")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SystemMaintenanceScheduleDto>>> ScheduleSystemMaintenance([FromBody] CreateSystemMaintenanceDto createDto)
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.ScheduleSystemMaintenanceAsync(createDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get scheduled maintenance
    /// </summary>
    [HttpGet("maintenance/scheduled")]
    public async Task<ActionResult<ApiResponse<List<SystemMaintenanceScheduleDto>>>> GetScheduledMaintenance()
    {
        var result = await _systemManagementService.GetScheduledMaintenanceAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region Cache Management

    /// <summary>
    /// Clear application cache
    /// </summary>
    [HttpPost("cache/clear")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> ClearCache([FromQuery] string cacheType = "all")
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.ClearCacheAsync(cacheType, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    [HttpGet("cache/statistics")]
    public async Task<ActionResult<ApiResponse<CacheStatisticsDto>>> GetCacheStatistics()
    {
        var result = await _systemManagementService.GetCacheStatisticsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Refresh cache
    /// </summary>
    [HttpPost("cache/refresh")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse>> RefreshCache([FromQuery] string cacheType = "all")
    {
        var adminId = User.Identity?.Name ?? "Unknown";
        var result = await _systemManagementService.RefreshCacheAsync(cacheType, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion

    #region System Information

    /// <summary>
    /// Get system information
    /// </summary>
    [HttpGet("info")]
    public async Task<ActionResult<ApiResponse<SystemInfoDto>>> GetSystemInfo()
    {
        var result = await _systemManagementService.GetSystemInfoAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get system statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<SystemStatisticsDto>>> GetSystemStatistics()
    {
        var result = await _systemManagementService.GetSystemStatisticsAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get system version information
    /// </summary>
    [HttpGet("version")]
    public async Task<ActionResult<ApiResponse<SystemVersionDto>>> GetSystemVersion()
    {
        var result = await _systemManagementService.GetSystemVersionAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    #endregion
}
