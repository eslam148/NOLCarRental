using System.ComponentModel.DataAnnotations;

namespace NOL.Application.DTOs.Admin;

#region System Configuration DTOs

public class SystemConfigDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; }
    public bool IsEncrypted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class CreateSystemConfigDto
{
    [Required]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string DataType { get; set; } = "String";
    public bool IsReadOnly { get; set; }
    public bool IsEncrypted { get; set; }
}

public class UpdateSystemConfigDto
{
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

#endregion

#region Application Settings DTOs

public class ApplicationSettingsDto
{
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationVersion { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyLogo { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = "en";
    public string DefaultCurrency { get; set; } = "AED";
    public string TimeZone { get; set; } = "UTC";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string TimeFormat { get; set; } = "HH:mm:ss";
    public bool MaintenanceMode { get; set; }
    public string MaintenanceMessage { get; set; } = string.Empty;
    public int SessionTimeout { get; set; } = 30;
    public int MaxLoginAttempts { get; set; } = 5;
    public bool EnableTwoFactorAuth { get; set; }
    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnableSmsNotifications { get; set; } = true;
    public bool EnablePushNotifications { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}

public class UpdateApplicationSettingsDto
{
    public string? ApplicationName { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyLogo { get; set; }
    public string? DefaultLanguage { get; set; }
    public string? DefaultCurrency { get; set; }
    public string? TimeZone { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public bool? MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public int? SessionTimeout { get; set; }
    public int? MaxLoginAttempts { get; set; }
    public bool? EnableTwoFactorAuth { get; set; }
    public bool? EnableEmailNotifications { get; set; }
    public bool? EnableSmsNotifications { get; set; }
    public bool? EnablePushNotifications { get; set; }
}

#endregion

#region System Health DTOs

public class SystemHealthDto
{
    public string Status { get; set; } = string.Empty; // Healthy, Warning, Critical
    public DateTime CheckedAt { get; set; }
    public List<HealthCheckDto> HealthChecks { get; set; } = new();
    public SystemResourcesDto Resources { get; set; } = new();
    public string OverallScore { get; set; } = string.Empty;
}

public class HealthCheckDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class SystemResourcesDto
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public long AvailableMemory { get; set; }
    public long TotalMemory { get; set; }
    public long AvailableDisk { get; set; }
    public long TotalDisk { get; set; }
}

public class SystemPerformanceDto
{
    public double AverageResponseTime { get; set; }
    public int RequestsPerSecond { get; set; }
    public int ActiveConnections { get; set; }
    public int TotalRequests { get; set; }
    public int ErrorCount { get; set; }
    public double ErrorRate { get; set; }
    public DateTime MeasuredAt { get; set; }
    public List<PerformanceMetricDto> Metrics { get; set; } = new();
}

public class PerformanceMetricDto
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class DatabaseHealthDto
{
    public string Status { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public TimeSpan ConnectionTime { get; set; }
    public int ActiveConnections { get; set; }
    public int MaxConnections { get; set; }
    public long DatabaseSize { get; set; }
    public List<TableInfoDto> Tables { get; set; } = new();
    public DateTime CheckedAt { get; set; }
}

public class TableInfoDto
{
    public string Name { get; set; } = string.Empty;
    public long RowCount { get; set; }
    public long SizeInBytes { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class ExternalServiceStatusDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
    public string LastError { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
    public bool IsRequired { get; set; }
}

#endregion

#region Maintenance DTOs

public class MaintenanceModeDto
{
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public bool AllowAdminAccess { get; set; } = true;
    public List<string> AllowedIpAddresses { get; set; } = new();
}

public class MaintenanceModeStatusDto
{
    public bool IsEnabled { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? EnabledAt { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string EnabledBy { get; set; } = string.Empty;
    public bool AllowAdminAccess { get; set; }
    public List<string> AllowedIpAddresses { get; set; } = new();
}

public class CreateSystemMaintenanceDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public DateTime ScheduledStart { get; set; }
    
    [Required]
    public DateTime ScheduledEnd { get; set; }
    
    public string MaintenanceType { get; set; } = string.Empty;
    public bool NotifyUsers { get; set; } = true;
    public string NotificationMessage { get; set; } = string.Empty;
}

public class SystemMaintenanceScheduleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool NotifyUsers { get; set; }
    public string NotificationMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

#endregion

#region Cache DTOs

public class CacheStatisticsDto
{
    public long TotalKeys { get; set; }
    public long MemoryUsage { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRatio { get; set; }
    public List<CacheTypeStatsDto> CacheTypes { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class CacheTypeStatsDto
{
    public string Type { get; set; } = string.Empty;
    public long KeyCount { get; set; }
    public long MemoryUsage { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRatio { get; set; }
}

#endregion

#region Backup DTOs

public class CreateBackupDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public string BackupType { get; set; } = "Full"; // Full, Incremental, Differential
    public bool IncludeDatabase { get; set; } = true;
    public bool IncludeFiles { get; set; } = true;
    public bool IncludeConfigurations { get; set; } = true;
    public bool CompressBackup { get; set; } = true;
}

public class BackupInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackupType { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string CheckSum { get; set; } = string.Empty;
    public bool IsCompressed { get; set; }
}

#endregion

#region System Logs DTOs

public class SystemLogDto
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty; // Debug, Info, Warning, Error, Critical
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Exception { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class SystemLogFilterDto
{
    public string? Level { get; set; }
    public string? Source { get; set; }
    public string? Category { get; set; }
    public string? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

#endregion

#region License DTOs

public class LicenseInfoDto
{
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseType { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int MaxUsers { get; set; }
    public int MaxBranches { get; set; }
    public int MaxCars { get; set; }
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
    public int DaysUntilExpiry { get; set; }
    public List<string> Features { get; set; } = new();
    public Dictionary<string, object> Limitations { get; set; } = new();
}

public class UpdateLicenseDto
{
    [Required]
    public string LicenseKey { get; set; } = string.Empty;
}

#endregion

#region Feature Flags DTOs

public class FeatureFlagDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class CreateFeatureFlagDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Category { get; set; } = "General";
    public Dictionary<string, object> Configuration { get; set; } = new();
}

#endregion

#region System Information DTOs

public class SystemInfoDto
{
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationVersion { get; set; } = string.Empty;
    public string BuildNumber { get; set; } = string.Empty;
    public DateTime BuildDate { get; set; }
    public string Environment { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string DotNetVersion { get; set; } = string.Empty;
    public string DatabaseProvider { get; set; } = string.Empty;
    public string DatabaseVersion { get; set; } = string.Empty;
    public DateTime StartupTime { get; set; }
    public TimeSpan Uptime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public Dictionary<string, string> Dependencies { get; set; } = new();
}

public class SystemStatisticsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalBookings { get; set; }
    public int TotalCars { get; set; }
    public int TotalBranches { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public long TotalStorageUsed { get; set; }
    public int ApiCallsToday { get; set; }
    public int ErrorsToday { get; set; }
    public DateTime LastCalculated { get; set; }
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}

public class SystemVersionDto
{
    public string CurrentVersion { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public bool IsUpdateAvailable { get; set; }
    public string ReleaseNotes { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public List<VersionHistoryDto> VersionHistory { get; set; } = new();
}

public class VersionHistoryDto
{
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string ReleaseNotes { get; set; } = string.Empty;
    public bool IsCritical { get; set; }
    public List<string> Features { get; set; } = new();
    public List<string> BugFixes { get; set; } = new();
}

#endregion
