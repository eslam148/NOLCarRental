using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Services;

public partial class AdminManagementService : IAdminManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminManagementService> _logger;

    public AdminManagementService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService,
        ILogger<AdminManagementService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
        _logger = logger;
    }

    #region Admin User CRUD Operations

    public async Task<ApiResponse<AdminUserDto>> GetAdminByIdAsync(string adminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse<AdminUserDto>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            var adminDto = await MapToAdminUserDto(admin);
            return ApiResponse<AdminUserDto>.Success(adminDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin by ID: {AdminId}", adminId);
            return ApiResponse<AdminUserDto>.Error("An error occurred while retrieving admin", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminUserDto>>> GetAdminsAsync(AdminFilterDto filter)
    {
        try
        {
            var query = _userManager.Users.AsQueryable();

            query = query.Where(u => u.UserRole == UserRole.Admin);

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(u => u.FullName.Contains(filter.Name));
            }

            if (!string.IsNullOrEmpty(filter.Email))
            {
                query = query.Where(u => u.Email!.Contains(filter.Email));
            }

            if (filter.UserRole.HasValue)
            {
                query = query.Where(u => u.UserRole == filter.UserRole.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            if (filter.CreatedDateFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= filter.CreatedDateFrom.Value);
            }

            if (filter.CreatedDateTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= filter.CreatedDateTo.Value);
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.FullName) : query.OrderByDescending(u => u.FullName),
                "email" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "createdat" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            // Apply pagination
            var totalCount = await query.CountAsync();
            var admins = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var adminDtos = new List<AdminUserDto>();
            foreach (var admin in admins)
            {
                adminDtos.Add(await MapToAdminUserDto(admin));
            }

            return ApiResponse<List<AdminUserDto>>.Success(adminDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admins with filter");
            return ApiResponse<List<AdminUserDto>>.Error("An error occurred while retrieving admins", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminUserDto>> CreateAdminAsync(CreateAdminUserDto createAdminDto, string createdByAdminId)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(createAdminDto.Email);
            if (existingUser != null)
            {
                return ApiResponse<AdminUserDto>.Error("Email already exists", (string?)null, ApiStatusCode.BadRequest);
            }

            // Create new admin user
            var admin = new ApplicationUser
            {
                UserName = createAdminDto.Email,
                Email = createAdminDto.Email,
                FullName = createAdminDto.FullName,
                PhoneNumber = createAdminDto.PhoneNumber,
                UserRole = UserRole.Admin,
                PreferredLanguage = createAdminDto.PreferredLanguage,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(admin, createAdminDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<AdminUserDto>.Error($"Failed to create admin: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Add to appropriate role
            var roleName = createAdminDto.UserRole.ToString();
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                await _userManager.AddToRoleAsync(admin, roleName);
            }

            // Log activity
            await LogAdminActivityAsync(createdByAdminId, "CreateAdmin", "ApplicationUser", admin.Id, 
                $"Created admin user: {admin.Email}");

            // Send welcome email if requested
            if (createAdminDto.SendWelcomeEmail)
            {
                await _emailService.SendWelcomeEmailAsync(admin.Email!, admin.FullName);
            }

            var adminDto = await MapToAdminUserDto(admin);
            return ApiResponse<AdminUserDto>.Success(adminDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin");
            return ApiResponse<AdminUserDto>.Error("An error occurred while creating admin", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminUserDto>> UpdateAdminAsync(string adminId, UpdateAdminUserDto updateAdminDto, string updatedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse<AdminUserDto>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            var oldValues = new { admin.FullName, admin.PhoneNumber, admin.UserRole, admin.PreferredLanguage, admin.IsActive };

            // Update properties
            if (!string.IsNullOrEmpty(updateAdminDto.FullName))
                admin.FullName = updateAdminDto.FullName;

            if (!string.IsNullOrEmpty(updateAdminDto.PhoneNumber))
                admin.PhoneNumber = updateAdminDto.PhoneNumber;

            if (updateAdminDto.UserRole.HasValue)
                admin.UserRole = updateAdminDto.UserRole.Value;

            if (updateAdminDto.PreferredLanguage.HasValue)
                admin.PreferredLanguage = updateAdminDto.PreferredLanguage.Value;

            if (updateAdminDto.IsActive.HasValue)
                admin.IsActive = updateAdminDto.IsActive.Value;

            admin.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(admin);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<AdminUserDto>.Error($"Failed to update admin: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            var newValues = new { admin.FullName, admin.PhoneNumber, admin.UserRole, admin.PreferredLanguage, admin.IsActive };

            // Log activity
            await LogAdminActivityAsync(updatedByAdminId, "UpdateAdmin", "ApplicationUser", admin.Id, 
                $"Updated admin user: {admin.Email}", oldValues, newValues);

            var adminDto = await MapToAdminUserDto(admin);
            return ApiResponse<AdminUserDto>.Success(adminDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin: {AdminId}", adminId);
            return ApiResponse<AdminUserDto>.Error("An error occurred while updating admin", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> DeleteAdminAsync(string adminId, string deletedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Prevent deletion of super admin
            if (admin.UserRole == UserRole.SuperAdmin)
            {
                return ApiResponse.Error("Cannot delete super admin", (string?)null, ApiStatusCode.BadRequest);
            }

            var result = await _userManager.DeleteAsync(admin);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse.Error($"Failed to delete admin: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Log activity
            await LogAdminActivityAsync(deletedByAdminId, "DeleteAdmin", "ApplicationUser", admin.Id, 
                $"Deleted admin user: {admin.Email}");

            return ApiResponse.Success("Admin deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting admin: {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while deleting admin", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Admin Status Management

    public async Task<ApiResponse<AdminUserDto>> ActivateAdminAsync(string adminId, string activatedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse<AdminUserDto>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            admin.IsActive = true;
            admin.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(admin);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<AdminUserDto>.Error($"Failed to activate admin: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Log activity
            await LogAdminActivityAsync(activatedByAdminId, "ActivateAdmin", "ApplicationUser", admin.Id,
                $"Activated admin user: {admin.Email}");

            var adminDto = await MapToAdminUserDto(admin);
            return ApiResponse<AdminUserDto>.Success(adminDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating admin: {AdminId}", adminId);
            return ApiResponse<AdminUserDto>.Error("An error occurred while activating admin", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminUserDto>> DeactivateAdminAsync(string adminId, string deactivatedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse<AdminUserDto>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Prevent deactivation of super admin
            if (admin.UserRole == UserRole.SuperAdmin)
            {
                return ApiResponse<AdminUserDto>.Error("Cannot deactivate super admin", (string?)null, ApiStatusCode.BadRequest);
            }

            admin.IsActive = false;
            admin.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(admin);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<AdminUserDto>.Error($"Failed to deactivate admin: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Log activity
            await LogAdminActivityAsync(deactivatedByAdminId, "DeactivateAdmin", "ApplicationUser", admin.Id,
                $"Deactivated admin user: {admin.Email}");

            var adminDto = await MapToAdminUserDto(admin);
            return ApiResponse<AdminUserDto>.Success(adminDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating admin: {AdminId}", adminId);
            return ApiResponse<AdminUserDto>.Error("An error occurred while deactivating admin", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkUpdateAdminStatusAsync(List<string> adminIds, bool isActive, string updatedByAdminId)
    {
        try
        {
            var admins = await _userManager.Users.Where(u => adminIds.Contains(u.Id)).ToListAsync();

            foreach (var admin in admins)
            {
                // Skip super admins for deactivation
                if (!isActive && admin.UserRole == UserRole.SuperAdmin)
                    continue;

                admin.IsActive = isActive;
                admin.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(admin);

                // Log activity
                var action = isActive ? "ActivateAdmin" : "DeactivateAdmin";
                await LogAdminActivityAsync(updatedByAdminId, action, "ApplicationUser", admin.Id,
                    $"{(isActive ? "Activated" : "Deactivated")} admin user: {admin.Email}");
            }

            return ApiResponse.Success($"Successfully updated {admins.Count} admin(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating admin status");
            return ApiResponse.Error("An error occurred while updating admin status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Role and Permission Management

    public async Task<ApiResponse<AdminUserDto>> UpdateAdminRoleAsync(string adminId, UserRole newRole, string updatedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse<AdminUserDto>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            var oldRole = admin.UserRole;
            admin.UserRole = newRole;
            admin.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(admin);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<AdminUserDto>.Error($"Failed to update admin role: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Update Identity roles
            var currentRoles = await _userManager.GetRolesAsync(admin);
            await _userManager.RemoveFromRolesAsync(admin, currentRoles);
            await _userManager.AddToRoleAsync(admin, newRole.ToString());

            // Log activity
            await LogAdminActivityAsync(updatedByAdminId, "UpdateAdminRole", "ApplicationUser", admin.Id,
                $"Updated admin role from {oldRole} to {newRole} for: {admin.Email}");

            var adminDto = await MapToAdminUserDto(admin);
            return ApiResponse<AdminUserDto>.Success(adminDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin role: {AdminId}", adminId);
            return ApiResponse<AdminUserDto>.Error("An error occurred while updating admin role", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminPermissionDto>>> GetAdminPermissionsAsync(string adminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse<List<AdminPermissionDto>>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            var permissions = GetPermissionsByRole(admin.UserRole);
            return ApiResponse<List<AdminPermissionDto>>.Success(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin permissions: {AdminId}", adminId);
            return ApiResponse<List<AdminPermissionDto>>.Error("An error occurred while retrieving permissions", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> UpdateAdminPermissionsAsync(string adminId, List<string> permissions, string updatedByAdminId)
    {
        try
        {
            // For this implementation, permissions are role-based and cannot be individually updated
            // This method could be extended to support custom permissions in the future
            await LogAdminActivityAsync(updatedByAdminId, "UpdateAdminPermissions", "ApplicationUser", adminId,
                $"Attempted to update permissions for admin: {adminId}");

            return ApiResponse.Success("Permissions are role-based and cannot be individually updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin permissions: {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while updating permissions", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminPermissionDto>>> GetAvailablePermissionsAsync()
    {
        try
        {
            var allPermissions = new List<AdminPermissionDto>();

            // Add permissions for each role
            foreach (UserRole role in Enum.GetValues<UserRole>())
            {
                var rolePermissions = GetPermissionsByRole(role);
                allPermissions.AddRange(rolePermissions);
            }

            // Remove duplicates
            var uniquePermissions = allPermissions
                .GroupBy(p => p.PermissionName)
                .Select(g => g.First())
                .ToList();

            return ApiResponse<List<AdminPermissionDto>>.Success(uniquePermissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available permissions");
            return ApiResponse<List<AdminPermissionDto>>.Error("An error occurred while retrieving permissions", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    // Helper method to map ApplicationUser to AdminUserDto
    private async Task<AdminUserDto> MapToAdminUserDto(ApplicationUser admin)
    {
        return new AdminUserDto
        {
            Id = admin.Id,
            Email = admin.Email!,
            FullName = admin.FullName,
            PhoneNumber = admin.PhoneNumber ?? string.Empty,
            UserRole = admin.UserRole,
            PreferredLanguage = admin.PreferredLanguage,
            IsActive = admin.IsActive,
            EmailConfirmed = admin.EmailConfirmed,
            CreatedAt = admin.CreatedAt,
            UpdatedAt = admin.UpdatedAt,
            LastLoginDate = admin.LastLoginDate
        };
    }

    // Public method for logging activities (part of interface)
    public async Task<ApiResponse> LogAdminActivityAsync(string adminId, string action, string entityType, string entityId, string description, object? oldValues = null, object? newValues = null)
    {
        try
        {
            // For this implementation, we'll just log to the application logger
            // In a full implementation, this would save to an AdminActivityLog table
            _logger.LogInformation("Admin Activity - AdminId: {AdminId}, Action: {Action}, EntityType: {EntityType}, EntityId: {EntityId}, Description: {Description}",
                adminId, action, entityType, entityId, description);

            return ApiResponse.Success("Activity logged successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging admin activity");
            return ApiResponse.Error("An error occurred while logging activity", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    // Helper method to get permissions by role
    private List<AdminPermissionDto> GetPermissionsByRole(UserRole role)
    {
        return role switch
        {
            UserRole.SuperAdmin => new List<AdminPermissionDto>
            {
                new() { PermissionName = "ManageAdmins", PermissionDescription = "Manage admin users", Category = "User Management", IsGranted = true },
                new() { PermissionName = "ManageCars", PermissionDescription = "Manage car fleet", Category = "Fleet Management", IsGranted = true },
                new() { PermissionName = "ManageBookings", PermissionDescription = "Manage bookings", Category = "Booking Management", IsGranted = true },
                new() { PermissionName = "ManageCustomers", PermissionDescription = "Manage customers", Category = "Customer Management", IsGranted = true },
                new() { PermissionName = "ViewReports", PermissionDescription = "View all reports", Category = "Reporting", IsGranted = true },
                new() { PermissionName = "ManageSettings", PermissionDescription = "Manage system settings", Category = "System", IsGranted = true }
            },
            UserRole.Admin => new List<AdminPermissionDto>
            {
                new() { PermissionName = "ManageCars", PermissionDescription = "Manage car fleet", Category = "Fleet Management", IsGranted = true },
                new() { PermissionName = "ManageBookings", PermissionDescription = "Manage bookings", Category = "Booking Management", IsGranted = true },
                new() { PermissionName = "ManageCustomers", PermissionDescription = "Manage customers", Category = "Customer Management", IsGranted = true },
                new() { PermissionName = "ViewReports", PermissionDescription = "View reports", Category = "Reporting", IsGranted = true }
            },
            UserRole.Employee => new List<AdminPermissionDto>
            {
                new() { PermissionName = "ViewCars", PermissionDescription = "View car fleet", Category = "Fleet Management", IsGranted = true },
                new() { PermissionName = "ViewBookings", PermissionDescription = "View bookings", Category = "Booking Management", IsGranted = true },
                new() { PermissionName = "ViewCustomers", PermissionDescription = "View customers", Category = "Customer Management", IsGranted = true }
            },
            _ => new List<AdminPermissionDto>()
        };
    }

    #region Activity Logging and Audit Trail

    public async Task<ApiResponse<List<AdminActivityLogDto>>> GetAdminActivityLogsAsync(AuditLogFilterDto filter)
    {
        try
        {
            // For this implementation, we'll return empty list as activity logging table is not implemented
            // This would require a separate AdminActivityLog entity
            return ApiResponse<List<AdminActivityLogDto>>.Success(new List<AdminActivityLogDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin activity logs");
            return ApiResponse<List<AdminActivityLogDto>>.Error("An error occurred while retrieving activity logs", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminActivityLogDto>>> GetAdminActivityLogsByAdminAsync(string adminId, int page = 1, int pageSize = 10)
    {
        try
        {
            // For this implementation, we'll return empty list as activity logging table is not implemented
            return ApiResponse<List<AdminActivityLogDto>>.Success(new List<AdminActivityLogDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin activity logs by admin: {AdminId}", adminId);
            return ApiResponse<List<AdminActivityLogDto>>.Error("An error occurred while retrieving activity logs", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region System Settings Management

    public async Task<ApiResponse<List<SystemSettingsDto>>> GetSystemSettingsAsync()
    {
        try
        {
            var settings = await _context.SystemSettings.ToListAsync();
            var settingDtos = settings.Select(s => new SystemSettingsDto
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                Type = s.Type,
                Description = s.Description,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList();

            return ApiResponse<List<SystemSettingsDto>>.Success(settingDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system settings");
            return ApiResponse<List<SystemSettingsDto>>.Error("An error occurred while retrieving system settings", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<SystemSettingsDto>> GetSystemSettingByKeyAsync(string key)
    {
        try
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                return ApiResponse<SystemSettingsDto>.Error("Setting not found", (string?)null, ApiStatusCode.NotFound);
            }

            var settingDto = new SystemSettingsDto
            {
                Id = setting.Id,
                Key = setting.Key,
                Value = setting.Value,
                Type = setting.Type,
                Description = setting.Description,
                IsActive = setting.IsActive,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };

            return ApiResponse<SystemSettingsDto>.Success(settingDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system setting by key: {Key}", key);
            return ApiResponse<SystemSettingsDto>.Error("An error occurred while retrieving system setting", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<SystemSettingsDto>> UpdateSystemSettingAsync(string key, UpdateSystemSettingDto updateDto, string updatedByAdminId)
    {
        try
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                return ApiResponse<SystemSettingsDto>.Error("Setting not found", (string?)null, ApiStatusCode.NotFound);
            }

            var oldValue = setting.Value;
            setting.Value = updateDto.Value;
            setting.Description = updateDto.Description ?? setting.Description;
            setting.IsActive = updateDto.IsActive ?? setting.IsActive;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Log activity
            await LogAdminActivityAsync(updatedByAdminId, "UpdateSystemSetting", "SystemSettings", setting.Id.ToString(),
                $"Updated system setting: {key}", new { Value = oldValue }, new { Value = setting.Value });

            var settingDto = new SystemSettingsDto
            {
                Id = setting.Id,
                Key = setting.Key,
                Value = setting.Value,
                Type = setting.Type,
                Description = setting.Description,
                IsActive = setting.IsActive,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };

            return ApiResponse<SystemSettingsDto>.Success(settingDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system setting: {Key}", key);
            return ApiResponse<SystemSettingsDto>.Error("An error occurred while updating system setting", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<SystemSettingsDto>> CreateSystemSettingAsync(CreateSystemSettingDto createDto, string createdByAdminId)
    {
        try
        {
            // Check if key already exists
            var existingSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == createDto.Key);
            if (existingSetting != null)
            {
                return ApiResponse<SystemSettingsDto>.Error("Setting key already exists", (string?)null, ApiStatusCode.BadRequest);
            }

            var setting = new SystemSettings
            {
                Key = createDto.Key,
                Value = createDto.Value,
                Type = createDto.Type,
                Description = createDto.Description,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SystemSettings.Add(setting);
            await _context.SaveChangesAsync();

            // Log activity
            await LogAdminActivityAsync(createdByAdminId, "CreateSystemSetting", "SystemSettings", setting.Id.ToString(),
                $"Created system setting: {setting.Key}");

            var settingDto = new SystemSettingsDto
            {
                Id = setting.Id,
                Key = setting.Key,
                Value = setting.Value,
                Type = setting.Type,
                Description = setting.Description,
                IsActive = setting.IsActive,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };

            return ApiResponse<SystemSettingsDto>.Success(settingDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating system setting");
            return ApiResponse<SystemSettingsDto>.Error("An error occurred while creating system setting", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> DeleteSystemSettingAsync(string key, string deletedByAdminId)
    {
        try
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                return ApiResponse.Error("Setting not found", (string?)null, ApiStatusCode.NotFound);
            }

            _context.SystemSettings.Remove(setting);
            await _context.SaveChangesAsync();

            // Log activity
            await LogAdminActivityAsync(deletedByAdminId, "DeleteSystemSetting", "SystemSettings", setting.Id.ToString(),
                $"Deleted system setting: {key}");

            return ApiResponse.Success("System setting deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting system setting: {Key}", key);
            return ApiResponse.Error("An error occurred while deleting system setting", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Branch Assignment (for Branch Managers)

    public async Task<ApiResponse<List<int>>> GetAdminAssignedBranchesAsync(string adminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse<List<int>>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            // For this implementation, we'll return empty list as branch assignment is not implemented in the entity
            // This could be extended with a separate AdminBranch table
            return ApiResponse<List<int>>.Success(new List<int>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin assigned branches: {AdminId}", adminId);
            return ApiResponse<List<int>>.Error("An error occurred while retrieving assigned branches", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> UpdateAdminBranchAssignmentAsync(string adminId, List<int> branchIds, string updatedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Log activity
            await LogAdminActivityAsync(updatedByAdminId, "UpdateBranchAssignment", "ApplicationUser", adminId,
                $"Updated branch assignment for admin: {admin.Email}");

            return ApiResponse.Success("Branch assignment updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin branch assignment: {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while updating branch assignment", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminUserDto>>> GetBranchAdminsAsync(int branchId)
    {
        try
        {
            // For this implementation, we'll return all admins as branch assignment is not implemented
            var admins = await _userManager.Users
                .Where(u => u.UserRole == UserRole.Admin || u.UserRole == UserRole.Employee)
                .ToListAsync();

            var adminDtos = new List<AdminUserDto>();
            foreach (var admin in admins)
            {
                adminDtos.Add(await MapToAdminUserDto(admin));
            }

            return ApiResponse<List<AdminUserDto>>.Success(adminDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch admins: {BranchId}", branchId);
            return ApiResponse<List<AdminUserDto>>.Error("An error occurred while retrieving branch admins", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Password Management

    public async Task<ApiResponse> ChangeAdminPasswordAsync(ChangeAdminPasswordDto changePasswordDto, string changedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(changePasswordDto.AdminId);
            if (admin == null)
            {
                return ApiResponse.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(admin);
            var result = await _userManager.ResetPasswordAsync(admin, token, changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse.Error($"Failed to change password: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Log activity
            await LogAdminActivityAsync(changedByAdminId, "ChangeAdminPassword", "ApplicationUser", admin.Id,
                $"Changed password for admin: {admin.Email}");

            // Send notification email if requested
            if (changePasswordDto.SendNotificationEmail)
            {
                await _emailService.SendPasswordChangeNotificationAsync(admin.Email!, admin.FullName);
            }

            return ApiResponse.Success("Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing admin password: {AdminId}", changePasswordDto.AdminId);
            return ApiResponse.Error("An error occurred while changing password", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> ResetAdminPasswordAsync(string adminId, string resetByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Generate a temporary password
            var tempPassword = GenerateTemporaryPassword();
            var token = await _userManager.GeneratePasswordResetTokenAsync(admin);
            var result = await _userManager.ResetPasswordAsync(admin, token, tempPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse.Error($"Failed to reset password: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Log activity
            await LogAdminActivityAsync(resetByAdminId, "ResetAdminPassword", "ApplicationUser", admin.Id,
                $"Reset password for admin: {admin.Email}");

            // Send temporary password via email
            await _emailService.SendTemporaryPasswordAsync(admin.Email!, admin.FullName, tempPassword);

            return ApiResponse.Success("Password reset successfully. Temporary password sent via email.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting admin password: {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while resetting password", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> SendPasswordResetEmailAsync(string adminId, string requestedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(admin);

            // Log activity
            await LogAdminActivityAsync(requestedByAdminId, "SendPasswordResetEmail", "ApplicationUser", admin.Id,
                $"Sent password reset email to admin: {admin.Email}");

            // Send password reset email
            await _emailService.SendPasswordResetEmailAsync(admin.Email!, admin.FullName, token);

            return ApiResponse.Success("Password reset email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email: {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while sending password reset email", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Admin Analytics

    public async Task<ApiResponse<AdminAnalyticsDto>> GetAdminAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var analytics = await GenerateAdminAnalyticsAsync();
            return ApiResponse<AdminAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin analytics");
            return ApiResponse<AdminAnalyticsDto>.Error("An error occurred while retrieving admin analytics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminRoleStatsDto>>> GetAdminRoleStatsAsync()
    {
        try
        {
            var roleStats = new List<AdminRoleStatsDto>();
            var totalAdmins = await _userManager.Users.CountAsync();

            foreach (UserRole role in Enum.GetValues<UserRole>())
            {
                var count = await _userManager.Users.CountAsync(u => u.UserRole == role);
                var percentage = totalAdmins > 0 ? (double)count / totalAdmins * 100 : 0;

                roleStats.Add(new AdminRoleStatsDto
                {
                    Role = role,
                    RoleName = role.ToString(),
                    Count = count,
                    Percentage = percentage
                });
            }

            return ApiResponse<List<AdminRoleStatsDto>>.Success(roleStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin role stats");
            return ApiResponse<List<AdminRoleStatsDto>>.Error("An error occurred while retrieving role stats", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminActivityStatsDto>>> GetAdminActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // For this implementation, return empty list as activity tracking is not fully implemented
            return ApiResponse<List<AdminActivityStatsDto>>.Success(new List<AdminActivityStatsDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin activity stats");
            return ApiResponse<List<AdminActivityStatsDto>>.Error("An error occurred while retrieving activity stats", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Bulk Operations

    public async Task<ApiResponse> BulkOperationAsync(BulkAdminOperationDto operationDto, string performedByAdminId)
    {
        try
        {
            switch (operationDto.Operation.ToLower())
            {
                case "activate":
                    return await BulkUpdateAdminStatusAsync(operationDto.AdminIds, true, performedByAdminId);
                case "deactivate":
                    return await BulkUpdateAdminStatusAsync(operationDto.AdminIds, false, performedByAdminId);
                case "updaterole":
                    if (operationDto.NewRole.HasValue)
                        return await BulkUpdateAdminRoleAsync(operationDto.AdminIds, operationDto.NewRole.Value, performedByAdminId);
                    break;
                case "delete":
                    return await BulkDeleteAdminsAsync(operationDto.AdminIds, performedByAdminId);
            }

            return ApiResponse.Error("Invalid operation", (string?)null, ApiStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation");
            return ApiResponse.Error("An error occurred while performing bulk operation", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkUpdateAdminRoleAsync(List<string> adminIds, UserRole newRole, string updatedByAdminId)
    {
        try
        {
            var admins = await _userManager.Users.Where(u => adminIds.Contains(u.Id)).ToListAsync();

            foreach (var admin in admins)
            {
                var oldRole = admin.UserRole;
                admin.UserRole = newRole;
                admin.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(admin);

                // Update Identity roles
                var currentRoles = await _userManager.GetRolesAsync(admin);
                await _userManager.RemoveFromRolesAsync(admin, currentRoles);
                await _userManager.AddToRoleAsync(admin, newRole.ToString());

                // Log activity
                await LogAdminActivityAsync(updatedByAdminId, "BulkUpdateAdminRole", "ApplicationUser", admin.Id,
                    $"Updated admin role from {oldRole} to {newRole} for: {admin.Email}");
            }

            return ApiResponse.Success($"Successfully updated role for {admins.Count} admin(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating admin roles");
            return ApiResponse.Error("An error occurred while updating admin roles", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    private async Task<ApiResponse> BulkDeleteAdminsAsync(List<string> adminIds, string deletedByAdminId)
    {
        try
        {
            var admins = await _userManager.Users.Where(u => adminIds.Contains(u.Id)).ToListAsync();
            var deletedCount = 0;

            foreach (var admin in admins)
            {
                // Skip super admins
                if (admin.UserRole == UserRole.SuperAdmin)
                    continue;

                await _userManager.DeleteAsync(admin);
                deletedCount++;

                // Log activity
                await LogAdminActivityAsync(deletedByAdminId, "BulkDeleteAdmin", "ApplicationUser", admin.Id,
                    $"Deleted admin user: {admin.Email}");
            }

            return ApiResponse.Success($"Successfully deleted {deletedCount} admin(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting admins");
            return ApiResponse.Error("An error occurred while deleting admins", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Admin Reports

    public async Task<ApiResponse<AdminReportDto>> GenerateAdminReportAsync(AdminFilterDto filter)
    {
        try
        {
            var admins = await GetAdminsAsync(filter);
            if (!admins.Succeeded)
            {
                return ApiResponse<AdminReportDto>.Error("Failed to retrieve admins for report", (string?)null, ApiStatusCode.InternalServerError);
            }

            var analytics = await GenerateAdminAnalyticsAsync();

            var report = new AdminReportDto
            {
                GeneratedAt = DateTime.UtcNow,
                StartDate = filter.CreatedDateFrom,
                EndDate = filter.CreatedDateTo,
                Analytics = analytics,
                Admins = admins.Data ?? new List<AdminUserDto>(),
                Activities = new List<AdminActivityLogDto>(), // Would be populated from activity log
                Summary = new AdminReportSummaryDto
                {
                    TotalAdmins = admins.Data?.Count ?? 0,
                    ActiveAdmins = admins.Data?.Count(a => a.IsActive) ?? 0,
                    SuperAdmins = admins.Data?.Count(a => a.UserRole == UserRole.SuperAdmin) ?? 0,
                    BranchManagers = admins.Data?.Count(a => a.UserRole == UserRole.Admin) ?? 0,
                    Employees = admins.Data?.Count(a => a.UserRole == UserRole.Employee) ?? 0
                }
            };

            return ApiResponse<AdminReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating admin report");
            return ApiResponse<AdminReportDto>.Error("An error occurred while generating report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportAdminReportAsync(AdminFilterDto filter, string format = "excel")
    {
        try
        {
            // This would implement actual export functionality
            // For now, return empty byte array
            return ApiResponse<byte[]>.Success(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting admin report");
            return ApiResponse<byte[]>.Error("An error occurred while exporting report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportActivityLogReportAsync(AuditLogFilterDto filter, string format = "excel")
    {
        try
        {
            // This would implement actual export functionality
            // For now, return empty byte array
            return ApiResponse<byte[]>.Success(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting activity log report");
            return ApiResponse<byte[]>.Error("An error occurred while exporting activity log report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Admin Validation

    public async Task<ApiResponse<bool>> ValidateAdminEmailAsync(string email, string? excludeAdminId = null)
    {
        try
        {
            var query = _userManager.Users.Where(u => u.Email == email);
            if (!string.IsNullOrEmpty(excludeAdminId))
            {
                query = query.Where(u => u.Id != excludeAdminId);
            }

            var exists = await query.AnyAsync();
            return ApiResponse<bool>.Success(!exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating admin email: {Email}", email);
            return ApiResponse<bool>.Error("An error occurred while validating email", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<bool>> CanAdminPerformActionAsync(string adminId, string action, string? entityType = null)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null || !admin.IsActive)
            {
                return ApiResponse<bool>.Success(false);
            }

            var permissions = GetPermissionsByRole(admin.UserRole);
            var canPerform = permissions.Any(p => p.PermissionName.Contains(action, StringComparison.OrdinalIgnoreCase));

            return ApiResponse<bool>.Success(canPerform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking admin permissions: {AdminId}", adminId);
            return ApiResponse<bool>.Error("An error occurred while checking permissions", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<bool>> IsAdminSuperAdminAsync(string adminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            var isSuperAdmin = admin?.UserRole == UserRole.SuperAdmin;
            return ApiResponse<bool>.Success(isSuperAdmin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if admin is super admin: {AdminId}", adminId);
            return ApiResponse<bool>.Error("An error occurred while checking admin role", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Admin Search

    public async Task<ApiResponse<List<AdminUserDto>>> SearchAdminsAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        try
        {
            var query = _userManager.Users.Where(u =>
                u.FullName.Contains(searchTerm) ||
                u.Email!.Contains(searchTerm) ||
                u.PhoneNumber!.Contains(searchTerm));

            var totalCount = await query.CountAsync();
            var admins = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var adminDtos = new List<AdminUserDto>();
            foreach (var admin in admins)
            {
                adminDtos.Add(await MapToAdminUserDto(admin));
            }

            return ApiResponse<List<AdminUserDto>>.Success(adminDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching admins: {SearchTerm}", searchTerm);
            return ApiResponse<List<AdminUserDto>>.Error("An error occurred while searching admins", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminUserDto>> GetAdminByEmailAsync(string email)
    {
        try
        {
            var admin = await _userManager.FindByEmailAsync(email);
            if (admin == null)
            {
                return ApiResponse<AdminUserDto>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            var adminDto = await MapToAdminUserDto(admin);
            return ApiResponse<AdminUserDto>.Success(adminDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin by email: {Email}", email);
            return ApiResponse<AdminUserDto>.Error("An error occurred while retrieving admin", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Login Tracking

    public async Task<ApiResponse> RecordAdminLoginAsync(string adminId, string ipAddress, string userAgent)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin != null)
            {
                admin.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(admin);

                // Log activity
                await LogAdminActivityAsync(adminId, "Login", "ApplicationUser", adminId,
                    $"Admin logged in from IP: {ipAddress}");
            }

            return ApiResponse.Success("Login recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording admin login: {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while recording login", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminLoginStatsDto>>> GetAdminLoginStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // For this implementation, return empty list as login tracking table is not implemented
            return ApiResponse<List<AdminLoginStatsDto>>.Success(new List<AdminLoginStatsDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin login stats");
            return ApiResponse<List<AdminLoginStatsDto>>.Error("An error occurred while retrieving login stats", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<DateTime?>> GetAdminLastLoginAsync(string adminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            return ApiResponse<DateTime?>.Success(admin?.LastLoginDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin last login: {AdminId}", adminId);
            return ApiResponse<DateTime?>.Error("An error occurred while retrieving last login", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Security Features

    public async Task<ApiResponse<List<AdminUserDto>>> GetSuspiciousAdminActivitiesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // For this implementation, return empty list as activity monitoring is not fully implemented
            return ApiResponse<List<AdminUserDto>>.Success(new List<AdminUserDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suspicious admin activities");
            return ApiResponse<List<AdminUserDto>>.Error("An error occurred while retrieving suspicious activities", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> LockAdminAccountAsync(string adminId, string reason, string lockedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Prevent locking super admin
            if (admin.UserRole == UserRole.SuperAdmin)
            {
                return ApiResponse.Error("Cannot lock super admin account", (string?)null, ApiStatusCode.BadRequest);
            }

            admin.LockoutEnabled = true;
            admin.LockoutEnd = DateTimeOffset.MaxValue; // Lock indefinitely
            await _userManager.UpdateAsync(admin);

            // Log activity
            await LogAdminActivityAsync(lockedByAdminId, "LockAdminAccount", "ApplicationUser", adminId,
                $"Locked admin account: {admin.Email}. Reason: {reason}");

            return ApiResponse.Success("Admin account locked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking admin account: {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while locking admin account", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> UnlockAdminAccountAsync(string adminId, string unlockedByAdminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            admin.LockoutEnd = null;
            await _userManager.UpdateAsync(admin);

            // Log activity
            await LogAdminActivityAsync(unlockedByAdminId, "UnlockAdminAccount", "ApplicationUser", adminId,
                $"Unlocked admin account: {admin.Email}");

            return ApiResponse.Success("Admin account unlocked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking admin account: {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while unlocking admin account", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<bool>> IsAdminAccountLockedAsync(string adminId)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminId);
            var isLocked = admin != null && await _userManager.IsLockedOutAsync(admin);
            return ApiResponse<bool>.Success(isLocked);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if admin account is locked: {AdminId}", adminId);
            return ApiResponse<bool>.Error("An error occurred while checking account lock status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    // Helper method to generate temporary password
    private string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // Helper method to generate admin analytics
    private async Task<AdminAnalyticsDto> GenerateAdminAnalyticsAsync()
    {
        var totalAdmins = await _userManager.Users.CountAsync();
        var activeAdmins = await _userManager.Users.CountAsync(u => u.IsActive);

        return new AdminAnalyticsDto
        {
            TotalAdmins = totalAdmins,
            ActiveAdmins = activeAdmins,
            InactiveAdmins = totalAdmins - activeAdmins,
            RoleStats = new List<AdminRoleStatsDto>(),
            ActivityStats = new List<AdminActivityStatsDto>(),
            BranchStats = new List<BranchAdminStatsDto>(),
            LoginStats = new List<AdminLoginStatsDto>()
        };
    }
}
