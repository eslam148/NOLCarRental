using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;

namespace NOL.Application.Common.Interfaces.Admin;

public interface ISystemManagementService
{
    #region System Configuration

    /// <summary>
    /// Get all system configurations
    /// </summary>
    Task<ApiResponse<List<SystemConfigDto>>> GetSystemConfigurationsAsync();

    /// <summary>
    /// Get system configuration by key
    /// </summary>
    Task<ApiResponse<SystemConfigDto>> GetSystemConfigurationAsync(string key);

    /// <summary>
    /// Update system configuration
    /// </summary>
    Task<ApiResponse<SystemConfigDto>> UpdateSystemConfigurationAsync(string key, UpdateSystemConfigDto updateDto, string adminId);

    /// <summary>
    /// Create new system configuration
    /// </summary>
    Task<ApiResponse<SystemConfigDto>> CreateSystemConfigurationAsync(CreateSystemConfigDto createDto, string adminId);

    /// <summary>
    /// Delete system configuration
    /// </summary>
    Task<ApiResponse> DeleteSystemConfigurationAsync(string key, string adminId);

    /// <summary>
    /// Bulk update system configurations
    /// </summary>
    Task<ApiResponse> BulkUpdateConfigurationsAsync(List<UpdateSystemConfigDto> configurations, string adminId);

    #endregion

    #region Application Settings

    /// <summary>
    /// Get application settings
    /// </summary>
    Task<ApiResponse<ApplicationSettingsDto>> GetApplicationSettingsAsync();

    /// <summary>
    /// Update application settings
    /// </summary>
    Task<ApiResponse<ApplicationSettingsDto>> UpdateApplicationSettingsAsync(UpdateApplicationSettingsDto updateDto, string adminId);

    /// <summary>
    /// Reset application settings to default
    /// </summary>
    Task<ApiResponse<ApplicationSettingsDto>> ResetApplicationSettingsAsync(string adminId);

    #endregion

    #region System Health

    /// <summary>
    /// Get system health status
    /// </summary>
    Task<ApiResponse<SystemHealthDto>> GetSystemHealthAsync();

    /// <summary>
    /// Get system performance metrics
    /// </summary>
    Task<ApiResponse<SystemPerformanceDto>> GetSystemPerformanceAsync();

    /// <summary>
    /// Get database health status
    /// </summary>
    Task<ApiResponse<DatabaseHealthDto>> GetDatabaseHealthAsync();

    /// <summary>
    /// Get external services status
    /// </summary>
    Task<ApiResponse<List<ExternalServiceStatusDto>>> GetExternalServicesStatusAsync();

    #endregion

    #region System Maintenance

    /// <summary>
    /// Enable maintenance mode
    /// </summary>
    Task<ApiResponse> EnableMaintenanceModeAsync(MaintenanceModeDto maintenanceDto, string adminId);

    /// <summary>
    /// Disable maintenance mode
    /// </summary>
    Task<ApiResponse> DisableMaintenanceModeAsync(string adminId);

    /// <summary>
    /// Get maintenance mode status
    /// </summary>
    Task<ApiResponse<MaintenanceModeStatusDto>> GetMaintenanceModeStatusAsync();

    /// <summary>
    /// Schedule system maintenance
    /// </summary>
    Task<ApiResponse<SystemMaintenanceScheduleDto>> ScheduleSystemMaintenanceAsync(CreateSystemMaintenanceDto createDto, string adminId);

    /// <summary>
    /// Get scheduled maintenance
    /// </summary>
    Task<ApiResponse<List<SystemMaintenanceScheduleDto>>> GetScheduledMaintenanceAsync();

    #endregion

    #region Cache Management

    /// <summary>
    /// Clear application cache
    /// </summary>
    Task<ApiResponse> ClearCacheAsync(string cacheType, string adminId);

    /// <summary>
    /// Get cache statistics
    /// </summary>
    Task<ApiResponse<CacheStatisticsDto>> GetCacheStatisticsAsync();

    /// <summary>
    /// Refresh cache
    /// </summary>
    Task<ApiResponse> RefreshCacheAsync(string cacheType, string adminId);

    #endregion

    #region Backup Management

    /// <summary>
    /// Create system backup
    /// </summary>
    Task<ApiResponse<BackupInfoDto>> CreateSystemBackupAsync(CreateBackupDto createDto, string adminId);

    /// <summary>
    /// Get backup history
    /// </summary>
    Task<ApiResponse<List<BackupInfoDto>>> GetBackupHistoryAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Restore from backup
    /// </summary>
    Task<ApiResponse> RestoreFromBackupAsync(string backupId, string adminId);

    /// <summary>
    /// Delete backup
    /// </summary>
    Task<ApiResponse> DeleteBackupAsync(string backupId, string adminId);

    /// <summary>
    /// Download backup file
    /// </summary>
    Task<ApiResponse<byte[]>> DownloadBackupAsync(string backupId);

    #endregion

    #region System Logs

    /// <summary>
    /// Get system logs
    /// </summary>
    Task<ApiResponse<List<SystemLogDto>>> GetSystemLogsAsync(SystemLogFilterDto filter);

    /// <summary>
    /// Clear system logs
    /// </summary>
    Task<ApiResponse> ClearSystemLogsAsync(DateTime? beforeDate, string adminId);

    /// <summary>
    /// Export system logs
    /// </summary>
    Task<ApiResponse<byte[]>> ExportSystemLogsAsync(SystemLogFilterDto filter, string format = "csv");

    #endregion

    #region License Management

    /// <summary>
    /// Get license information
    /// </summary>
    Task<ApiResponse<LicenseInfoDto>> GetLicenseInfoAsync();

    /// <summary>
    /// Update license
    /// </summary>
    Task<ApiResponse<LicenseInfoDto>> UpdateLicenseAsync(UpdateLicenseDto updateDto, string adminId);

    /// <summary>
    /// Validate license
    /// </summary>
    Task<ApiResponse<bool>> ValidateLicenseAsync();

    #endregion

    #region Feature Flags

    /// <summary>
    /// Get feature flags
    /// </summary>
    Task<ApiResponse<List<FeatureFlagDto>>> GetFeatureFlagsAsync();

    /// <summary>
    /// Update feature flag
    /// </summary>
    Task<ApiResponse<FeatureFlagDto>> UpdateFeatureFlagAsync(string flagName, bool isEnabled, string adminId);

    /// <summary>
    /// Create feature flag
    /// </summary>
    Task<ApiResponse<FeatureFlagDto>> CreateFeatureFlagAsync(CreateFeatureFlagDto createDto, string adminId);

    #endregion

    #region System Information

    /// <summary>
    /// Get system information
    /// </summary>
    Task<ApiResponse<SystemInfoDto>> GetSystemInfoAsync();

    /// <summary>
    /// Get system statistics
    /// </summary>
    Task<ApiResponse<SystemStatisticsDto>> GetSystemStatisticsAsync();

    /// <summary>
    /// Get system version information
    /// </summary>
    Task<ApiResponse<SystemVersionDto>> GetSystemVersionAsync();

    #endregion
}
