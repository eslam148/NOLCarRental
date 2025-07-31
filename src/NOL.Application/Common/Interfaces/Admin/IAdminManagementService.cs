using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces.Admin;

public interface IAdminManagementService
{
    // Admin User CRUD Operations
    Task<ApiResponse<AdminUserDto>> GetAdminByIdAsync(string adminId);
    Task<ApiResponse<List<AdminUserDto>>> GetAdminsAsync(AdminFilterDto filter);
    Task<ApiResponse<AdminUserDto>> CreateAdminAsync(CreateAdminUserDto createAdminDto, string createdByAdminId);
    Task<ApiResponse<AdminUserDto>> UpdateAdminAsync(string adminId, UpdateAdminUserDto updateAdminDto, string updatedByAdminId);
    Task<ApiResponse> DeleteAdminAsync(string adminId, string deletedByAdminId);
    
    // Admin Status Management
    Task<ApiResponse<AdminUserDto>> ActivateAdminAsync(string adminId, string activatedByAdminId);
    Task<ApiResponse<AdminUserDto>> DeactivateAdminAsync(string adminId, string deactivatedByAdminId);
    Task<ApiResponse> BulkUpdateAdminStatusAsync(List<string> adminIds, bool isActive, string updatedByAdminId);
    
    // Role and Permission Management
    Task<ApiResponse<AdminUserDto>> UpdateAdminRoleAsync(string adminId, UserRole newRole, string updatedByAdminId);
    Task<ApiResponse<List<AdminPermissionDto>>> GetAdminPermissionsAsync(string adminId);
    Task<ApiResponse> UpdateAdminPermissionsAsync(string adminId, List<string> permissions, string updatedByAdminId);
    Task<ApiResponse<List<AdminPermissionDto>>> GetAvailablePermissionsAsync();
    
    // Branch Assignment (for Branch Managers)
    Task<ApiResponse<List<int>>> GetAdminAssignedBranchesAsync(string adminId);
    Task<ApiResponse> UpdateAdminBranchAssignmentAsync(string adminId, List<int> branchIds, string updatedByAdminId);
    Task<ApiResponse<List<AdminUserDto>>> GetBranchAdminsAsync(int branchId);
    
    // Password Management
    Task<ApiResponse> ChangeAdminPasswordAsync(ChangeAdminPasswordDto changePasswordDto, string changedByAdminId);
    Task<ApiResponse> ResetAdminPasswordAsync(string adminId, string resetByAdminId);
    Task<ApiResponse> SendPasswordResetEmailAsync(string adminId, string requestedByAdminId);
    
    // Admin Analytics
    Task<ApiResponse<AdminAnalyticsDto>> GetAdminAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<AdminRoleStatsDto>>> GetAdminRoleStatsAsync();
    Task<ApiResponse<List<AdminActivityStatsDto>>> GetAdminActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Activity Logging and Audit Trail
    Task<ApiResponse<List<AdminActivityLogDto>>> GetAdminActivityLogsAsync(AuditLogFilterDto filter);
    Task<ApiResponse<List<AdminActivityLogDto>>> GetAdminActivityLogsByAdminAsync(string adminId, int page = 1, int pageSize = 10);
    Task<ApiResponse> LogAdminActivityAsync(string adminId, string action, string entityType, string entityId, string description, object? oldValues = null, object? newValues = null);
    
    // System Settings Management
    Task<ApiResponse<List<SystemSettingsDto>>> GetSystemSettingsAsync();
    Task<ApiResponse<SystemSettingsDto>> GetSystemSettingByKeyAsync(string key);
    Task<ApiResponse<SystemSettingsDto>> UpdateSystemSettingAsync(string key, UpdateSystemSettingDto updateDto, string updatedByAdminId);
    Task<ApiResponse<SystemSettingsDto>> CreateSystemSettingAsync(CreateSystemSettingDto createDto, string createdByAdminId);
    Task<ApiResponse> DeleteSystemSettingAsync(string key, string deletedByAdminId);
    
    // Bulk Operations
    Task<ApiResponse> BulkOperationAsync(BulkAdminOperationDto operationDto, string performedByAdminId);
    Task<ApiResponse> BulkUpdateAdminRoleAsync(List<string> adminIds, UserRole newRole, string updatedByAdminId);
    
    // Admin Reports
    Task<ApiResponse<AdminReportDto>> GenerateAdminReportAsync(AdminFilterDto filter);
    Task<ApiResponse<byte[]>> ExportAdminReportAsync(AdminFilterDto filter, string format = "excel");
    Task<ApiResponse<byte[]>> ExportActivityLogReportAsync(AuditLogFilterDto filter, string format = "excel");
    
    // Admin Validation
    Task<ApiResponse<bool>> ValidateAdminEmailAsync(string email, string? excludeAdminId = null);
    Task<ApiResponse<bool>> CanAdminPerformActionAsync(string adminId, string action, string? entityType = null);
    Task<ApiResponse<bool>> IsAdminSuperAdminAsync(string adminId);
    
    // Admin Search
    Task<ApiResponse<List<AdminUserDto>>> SearchAdminsAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<ApiResponse<AdminUserDto>> GetAdminByEmailAsync(string email);
    
    // Login Tracking
    Task<ApiResponse> RecordAdminLoginAsync(string adminId, string ipAddress, string userAgent);
    Task<ApiResponse<List<AdminLoginStatsDto>>> GetAdminLoginStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<DateTime?>> GetAdminLastLoginAsync(string adminId);
    
    // Security Features
    Task<ApiResponse<List<AdminUserDto>>> GetSuspiciousAdminActivitiesAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse> LockAdminAccountAsync(string adminId, string reason, string lockedByAdminId);
    Task<ApiResponse> UnlockAdminAccountAsync(string adminId, string unlockedByAdminId);
    Task<ApiResponse<bool>> IsAdminAccountLockedAsync(string adminId);
}
