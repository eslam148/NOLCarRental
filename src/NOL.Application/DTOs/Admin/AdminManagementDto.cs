using System.ComponentModel.DataAnnotations;
using NOL.Domain.Enums;

namespace NOL.Application.DTOs.Admin;

public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole UserRole { get; set; }
    public Language PreferredLanguage { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string CreatedByAdmin { get; set; } = string.Empty;
    public List<int> AssignedBranches { get; set; } = new();
    public List<string> AssignedBranchNames { get; set; } = new();
    public List<AdminPermissionDto> Permissions { get; set; } = new();
    public List<AdminActivityLogDto> RecentActivities { get; set; } = new();
}

public class CreateAdminUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    [Required]
    public UserRole UserRole { get; set; }
    
    public Language PreferredLanguage { get; set; } = Language.Arabic;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public List<int> AssignedBranches { get; set; } = new();
    
    public List<string> Permissions { get; set; } = new();
    
    public bool SendWelcomeEmail { get; set; } = true;
}

public class UpdateAdminUserDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? FullName { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    public UserRole? UserRole { get; set; }
    
    public Language? PreferredLanguage { get; set; }
    
    public bool? IsActive { get; set; }
    
    public List<int>? AssignedBranches { get; set; }
    
    public List<string>? Permissions { get; set; }
}

public class AdminPermissionDto
{
    public string PermissionName { get; set; } = string.Empty;
    public string PermissionDescription { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}

public class AdminActivityLogDto
{
    public int Id { get; set; }
    public string AdminId { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminFilterDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public UserRole? UserRole { get; set; }
    public bool? IsActive { get; set; }
    public int? BranchId { get; set; }
    public DateTime? CreatedDateFrom { get; set; }
    public DateTime? CreatedDateTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AdminAnalyticsDto
{
    public int TotalAdmins { get; set; }
    public int ActiveAdmins { get; set; }
    public int InactiveAdmins { get; set; }
    public List<AdminRoleStatsDto> RoleStats { get; set; } = new();
    public List<AdminActivityStatsDto> ActivityStats { get; set; } = new();
    public List<BranchAdminStatsDto> BranchStats { get; set; } = new();
    public List<AdminLoginStatsDto> LoginStats { get; set; } = new();
}

public class AdminRoleStatsDto
{
    public UserRole Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class AdminActivityStatsDto
{
    public DateTime Date { get; set; }
    public int ActivityCount { get; set; }
    public int UniqueAdmins { get; set; }
    public List<string> TopActions { get; set; } = new();
}

public class BranchAdminStatsDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int AdminCount { get; set; }
    public List<string> AdminNames { get; set; } = new();
}

public class AdminLoginStatsDto
{
    public DateTime Date { get; set; }
    public int LoginCount { get; set; }
    public int UniqueLogins { get; set; }
}

public class ChangeAdminPasswordDto
{
    [Required]
    public string AdminId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public bool SendNotificationEmail { get; set; } = true;
}

public class SystemSettingsDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public SettingType Type { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateSystemSettingDto
{
    [Required]
    [StringLength(1000)]
    public string Value { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool? IsActive { get; set; }
}

public class CreateSystemSettingDto
{
    [Required]
    [StringLength(100)]
    public string Key { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Value { get; set; } = string.Empty;
    
    [Required]
    public SettingType Type { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class AuditLogFilterDto
{
    public string? AdminId { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? IpAddress { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class BulkAdminOperationDto
{
    [Required]
    public List<string> AdminIds { get; set; } = new();
    
    [Required]
    public string Operation { get; set; } = string.Empty; // "activate", "deactivate", "delete", "updateRole"
    
    public bool? IsActive { get; set; }
    public UserRole? NewRole { get; set; }
    public List<int>? AssignedBranches { get; set; }
}

public class AdminReportDto
{
    public DateTime GeneratedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public AdminAnalyticsDto Analytics { get; set; } = new();
    public List<AdminUserDto> Admins { get; set; } = new();
    public List<AdminActivityLogDto> Activities { get; set; } = new();
    public AdminReportSummaryDto Summary { get; set; } = new();
}

public class AdminReportSummaryDto
{
    public int TotalAdmins { get; set; }
    public int ActiveAdmins { get; set; }
    public int SuperAdmins { get; set; }
    public int BranchManagers { get; set; }
    public int Employees { get; set; }
    public int TotalActivities { get; set; }
    public int TotalLogins { get; set; }
    public double AverageActivitiesPerAdmin { get; set; }
    public List<string> MostActiveAdmins { get; set; } = new();
    public List<string> MostCommonActions { get; set; } = new();
}

public class CreateSystemSettingDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Value { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = false;
}

public class UpdateSystemSettingDto
{
    [Required]
    [StringLength(1000)]
    public string Value { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    public bool? IsPublic { get; set; }
}

public class AuditLogFilterDto
{
    public string? AdminId { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? IpAddress { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AdminLoginStatsDto
{
    public string AdminId { get; set; } = string.Empty;
    public string AdminName { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public int LoginCount { get; set; }
    public int TotalSessions { get; set; }
    public TimeSpan AverageSessionDuration { get; set; }
    public List<LoginActivityDto> RecentLogins { get; set; } = new();
}

public class LoginActivityDto
{
    public DateTime LoginTime { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public TimeSpan? SessionDuration { get; set; }
    public bool IsSuccessful { get; set; }
}

public class RecordLoginDto
{
    [Required]
    public string AdminId { get; set; } = string.Empty;

    [Required]
    public string IpAddress { get; set; } = string.Empty;

    [Required]
    public string UserAgent { get; set; } = string.Empty;
}
