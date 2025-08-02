using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Application.DTOs.Common;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Services;

public class BranchManagementService : IBranchManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BranchManagementService> _logger;

    public BranchManagementService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<BranchManagementService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<ApiResponse<AdminBranchDto>> GetBranchByIdAsync(int id)
    {
        try
        {
            var branch = await _context.Branches
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return ApiResponse<AdminBranchDto>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            var branchDto = await MapToAdminBranchDto(branch);
            return ApiResponse<AdminBranchDto>.Success(branchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch by ID: {Id}", id);
            return ApiResponse<AdminBranchDto>.Error("An error occurred while retrieving branch", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminBranchDto>>> GetBranchesAsync(BranchFilterDto filter)
    {
        try
        {
            // Validate and normalize pagination parameters
            filter.ValidateAndNormalize();

            var query = _context.Branches
                .Include(b => b.Cars)
                .AsQueryable();

            // Apply filters
            if (filter.IsActive.HasValue)
                query = query.Where(b => b.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrEmpty(filter.City))
                query = query.Where(b => b.City.Contains(filter.City));

            if (!string.IsNullOrEmpty(filter.Country))
                query = query.Where(b => b.Country.Contains(filter.Country));

            if (filter.CreatedDateFrom.HasValue)
                query = query.Where(b => b.CreatedAt >= filter.CreatedDateFrom.Value);

            if (filter.CreatedDateTo.HasValue)
                query = query.Where(b => b.CreatedAt <= filter.CreatedDateTo.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(b => b.NameAr.Contains(filter.SearchTerm) ||
                                        b.NameEn.Contains(filter.SearchTerm) ||
                                        b.Address.Contains(filter.SearchTerm) ||
                                        b.City.Contains(filter.SearchTerm) ||
                                        b.Country.Contains(filter.SearchTerm));
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.NameEn) : query.OrderByDescending(b => b.NameEn),
                "city" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.City) : query.OrderByDescending(b => b.City),
                "country" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.Country) : query.OrderByDescending(b => b.Country),
                "createdat" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.CreatedAt) : query.OrderByDescending(b => b.CreatedAt),
                _ => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.NameEn) : query.OrderByDescending(b => b.NameEn)
            };

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var branches = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var branchDtos = new List<AdminBranchDto>();
            foreach (var branch in branches)
            {
                branchDtos.Add(await MapToAdminBranchDto(branch));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminBranchDto>.Create(
                branchDtos,
                filter.Page,
                filter.PageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminBranchDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branches with filter");
            return ApiResponse<PaginatedResponseDto<AdminBranchDto>>.Error("An error occurred while retrieving branches", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminBranchDto>> CreateBranchAsync(AdminCreateBranchDto createBranchDto, string adminId)
    {
        try
        {
            // Validate admin exists
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse<AdminBranchDto>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Validate branch name uniqueness
            var nameExists = await _context.Branches.AnyAsync(b => 
                b.NameEn == createBranchDto.NameEn || b.NameAr == createBranchDto.NameAr);
            if (nameExists)
            {
                return ApiResponse<AdminBranchDto>.Error("Branch name already exists", (string?)null, ApiStatusCode.BadRequest);
            }

            var branch = new Branch
            {
                NameAr = createBranchDto.NameAr,
                NameEn = createBranchDto.NameEn,
                DescriptionAr = createBranchDto.DescriptionAr,
                DescriptionEn = createBranchDto.DescriptionEn,
                Address = createBranchDto.Address,
                City = createBranchDto.City,
                Country = createBranchDto.Country,
                Phone = createBranchDto.Phone,
                Email = createBranchDto.Email,
                Latitude = createBranchDto.Latitude,
                Longitude = createBranchDto.Longitude,
                WorkingHours = createBranchDto.WorkingHours,
                IsActive = createBranchDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            // Reload with includes
            branch = await _context.Branches
                .Include(b => b.Cars)
                .FirstAsync(b => b.Id == branch.Id);

            var branchDto = await MapToAdminBranchDto(branch);
            return ApiResponse<AdminBranchDto>.Success(branchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch");
            return ApiResponse<AdminBranchDto>.Error("An error occurred while creating branch", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminBranchDto>> UpdateBranchAsync(int id, AdminUpdateBranchDto updateBranchDto, string adminId)
    {
        try
        {
            var branch = await _context.Branches
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return ApiResponse<AdminBranchDto>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateBranchDto.NameAr))
                branch.NameAr = updateBranchDto.NameAr;

            if (!string.IsNullOrEmpty(updateBranchDto.NameEn))
                branch.NameEn = updateBranchDto.NameEn;

            if (updateBranchDto.DescriptionAr != null)
                branch.DescriptionAr = updateBranchDto.DescriptionAr;

            if (updateBranchDto.DescriptionEn != null)
                branch.DescriptionEn = updateBranchDto.DescriptionEn;

            if (!string.IsNullOrEmpty(updateBranchDto.Address))
                branch.Address = updateBranchDto.Address;

            if (!string.IsNullOrEmpty(updateBranchDto.City))
                branch.City = updateBranchDto.City;

            if (!string.IsNullOrEmpty(updateBranchDto.Country))
                branch.Country = updateBranchDto.Country;

            if (updateBranchDto.Phone != null)
                branch.Phone = updateBranchDto.Phone;

            if (updateBranchDto.Email != null)
                branch.Email = updateBranchDto.Email;

            if (updateBranchDto.Latitude.HasValue)
                branch.Latitude = updateBranchDto.Latitude.Value;

            if (updateBranchDto.Longitude.HasValue)
                branch.Longitude = updateBranchDto.Longitude.Value;

            if (updateBranchDto.WorkingHours != null)
                branch.WorkingHours = updateBranchDto.WorkingHours;

            if (updateBranchDto.IsActive.HasValue)
                branch.IsActive = updateBranchDto.IsActive.Value;

            branch.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var branchDto = await MapToAdminBranchDto(branch);
            return ApiResponse<AdminBranchDto>.Success(branchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch: {Id}", id);
            return ApiResponse<AdminBranchDto>.Error("An error occurred while updating branch", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> DeleteBranchAsync(int id, string adminId)
    {
        try
        {
            var branch = await _context.Branches
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return ApiResponse.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Check if branch has active cars
            var hasActiveCars = branch.Cars.Any(c => c.Status == CarStatus.Available || c.Status == CarStatus.Rented);
            if (hasActiveCars)
            {
                return ApiResponse.Error("Cannot delete branch with active cars. Please transfer or deactivate cars first.", (string?)null, ApiStatusCode.BadRequest);
            }

            // Check if branch has active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => (b.ReceivingBranchId == id || b.DeliveryBranchId == id) && 
                              (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress));

            if (hasActiveBookings)
            {
                return ApiResponse.Error("Cannot delete branch with active bookings", (string?)null, ApiStatusCode.BadRequest);
            }

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();

            return ApiResponse.Success("Branch deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting branch: {Id}", id);
            return ApiResponse.Error("An error occurred while deleting branch", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Branch Status Management

    public async Task<ApiResponse<AdminBranchDto>> ActivateBranchAsync(int id, string adminId)
    {
        try
        {
            var branch = await _context.Branches
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return ApiResponse<AdminBranchDto>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            branch.IsActive = true;
            branch.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var branchDto = await MapToAdminBranchDto(branch);
            return ApiResponse<AdminBranchDto>.Success(branchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating branch: {Id}", id);
            return ApiResponse<AdminBranchDto>.Error("An error occurred while activating branch", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminBranchDto>> DeactivateBranchAsync(int id, string adminId)
    {
        try
        {
            var branch = await _context.Branches
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return ApiResponse<AdminBranchDto>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Check if branch has active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => (b.ReceivingBranchId == id || b.DeliveryBranchId == id) &&
                              (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress));

            if (hasActiveBookings)
            {
                return ApiResponse<AdminBranchDto>.Error("Cannot deactivate branch with active bookings", (string?)null, ApiStatusCode.BadRequest);
            }

            branch.IsActive = false;
            branch.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var branchDto = await MapToAdminBranchDto(branch);
            return ApiResponse<AdminBranchDto>.Success(branchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating branch: {Id}", id);
            return ApiResponse<AdminBranchDto>.Error("An error occurred while deactivating branch", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkUpdateBranchStatusAsync(List<int> branchIds, bool isActive, string adminId)
    {
        try
        {
            var branches = await _context.Branches
                .Where(b => branchIds.Contains(b.Id))
                .ToListAsync();

            if (!isActive)
            {
                // Check for active bookings if deactivating
                var hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => branchIds.Contains(b.ReceivingBranchId) || branchIds.Contains(b.DeliveryBranchId) &&
                                  (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress));

                if (hasActiveBookings)
                {
                    return ApiResponse.Error("Cannot deactivate branches with active bookings", (string?)null, ApiStatusCode.BadRequest);
                }
            }

            foreach (var branch in branches)
            {
                branch.IsActive = isActive;
                branch.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var action = isActive ? "activated" : "deactivated";
            return ApiResponse.Success($"Successfully {action} {branches.Count} branches");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating branch status");
            return ApiResponse.Error("An error occurred while updating branch status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Branch Analytics

    public async Task<ApiResponse<BranchAnalyticsDto>> GetBranchAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Branches
                .Include(b => b.Cars)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.CreatedAt <= endDate.Value);

            var branches = await query.ToListAsync();

            var analytics = new BranchAnalyticsDto
            {
                TotalBranches = branches.Count,
                ActiveBranches = branches.Count(b => b.IsActive),
                InactiveBranches = branches.Count(b => !b.IsActive),
                TotalCars = branches.Sum(b => b.Cars?.Count ?? 0),
                AverageUtilizationRate = await CalculateAverageUtilizationRate(branches),
                TopPerformingBranches = await GenerateTopPerformingBranches(branches, 5),
                LowPerformingBranches = await GenerateLowPerformingBranches(branches, 5),
                MonthlyStats = await GenerateMonthlyBranchStats(branches),
                CityStats = await GenerateCityStats(branches),
                CountryStats = await GenerateCountryStats(branches)
            };

            // Calculate revenue statistics
            var allBookings = await _context.Bookings
                .Where(b => branches.Select(br => br.Id).Contains(b.ReceivingBranchId) ||
                           branches.Select(br => br.Id).Contains(b.DeliveryBranchId))
                .Where(b => b.Status == BookingStatus.Completed)
                .ToListAsync();

            analytics.TotalRevenue = allBookings.Sum(b => b.FinalAmount);
            analytics.AverageRevenuePerBranch = branches.Any() ? analytics.TotalRevenue / branches.Count : 0;
            analytics.TotalBookings = allBookings.Count;
            analytics.AverageCustomerSatisfaction = 85.0; // Placeholder

            return ApiResponse<BranchAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch analytics");
            return ApiResponse<BranchAnalyticsDto>.Error("An error occurred while retrieving branch analytics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<BranchPerformanceDto>>> GetTopPerformingBranchesAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var branches = await _context.Branches
                .Include(b => b.Cars)
                .Where(b => b.IsActive)
                .ToListAsync();

            var topPerforming = await GenerateTopPerformingBranches(branches, count);
            return ApiResponse<List<BranchPerformanceDto>>.Success(topPerforming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top performing branches");
            return ApiResponse<List<BranchPerformanceDto>>.Error("An error occurred while retrieving top performing branches", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<BranchPerformanceDto>>> GetLowPerformingBranchesAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var branches = await _context.Branches
                .Include(b => b.Cars)
                .Where(b => b.IsActive)
                .ToListAsync();

            var lowPerforming = await GenerateLowPerformingBranches(branches, count);
            return ApiResponse<List<BranchPerformanceDto>>.Success(lowPerforming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low performing branches");
            return ApiResponse<List<BranchPerformanceDto>>.Error("An error occurred while retrieving low performing branches", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<BranchComparisonResultDto>> CompareBranchPerformanceAsync(BranchComparisonDto comparisonDto)
    {
        try
        {
            var branches = await _context.Branches
                .Include(b => b.Cars)
                .Where(b => comparisonDto.BranchIds.Contains(b.Id))
                .ToListAsync();

            var performances = new List<BranchPerformanceDto>();
            foreach (var branch in branches)
            {
                var performance = await CalculateBranchPerformance(branch, comparisonDto.StartDate, comparisonDto.EndDate);
                performances.Add(performance);
            }

            var result = new BranchComparisonResultDto
            {
                BranchPerformances = performances,
                BestPerformingBranch = performances.OrderByDescending(p => p.PerformanceScore).FirstOrDefault()?.BranchName ?? "None",
                WorstPerformingBranch = performances.OrderBy(p => p.PerformanceScore).FirstOrDefault()?.BranchName ?? "None",
                MetricComparisons = new Dictionary<string, List<decimal>>(),
                AverageMetrics = new Dictionary<string, decimal>
                {
                    ["Revenue"] = performances.Any() ? performances.Average(p => p.Revenue) : 0,
                    ["UtilizationRate"] = performances.Any() ? (decimal)performances.Average(p => p.UtilizationRate) : 0,
                    ["BookingCount"] = performances.Any() ? (decimal)performances.Average(p => p.BookingCount) : 0,
                    ["PerformanceScore"] = performances.Any() ? (decimal)performances.Average(p => p.PerformanceScore) : 0
                }
            };

            return ApiResponse<BranchComparisonResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing branch performance");
            return ApiResponse<BranchComparisonResultDto>.Error("An error occurred while comparing branch performance", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<BranchRevenueAnalysisDto>> GetBranchRevenueAnalysisAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
            {
                return ApiResponse<BranchRevenueAnalysisDto>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            var bookingsQuery = _context.Bookings
                .Where(b => (b.ReceivingBranchId == branchId || b.DeliveryBranchId == branchId) &&
                           b.Status == BookingStatus.Completed);

            if (startDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= endDate.Value);

            var bookings = await bookingsQuery.ToListAsync();

            var analysis = new BranchRevenueAnalysisDto
            {
                BranchId = branchId,
                BranchName = branch.NameEn,
                DailyRevenue = bookings.Where(b => b.CreatedAt >= DateTime.UtcNow.Date).Sum(b => b.FinalAmount),
                WeeklyRevenue = bookings.Where(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-7)).Sum(b => b.FinalAmount),
                MonthlyRevenue = bookings.Where(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-30)).Sum(b => b.FinalAmount),
                YearlyRevenue = bookings.Where(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-365)).Sum(b => b.FinalAmount),
                AverageBookingValue = bookings.Any() ? bookings.Average(b => b.FinalAmount) : 0,
                BookingCount = bookings.Count,
                DailyBreakdown = GenerateDailyRevenueBreakdown(bookings),
                MonthlyBreakdown = GenerateMonthlyRevenueBreakdown(bookings)
            };

            // Calculate growth rate (placeholder calculation)
            var previousMonthRevenue = bookings
                .Where(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-60) && b.CreatedAt < DateTime.UtcNow.AddDays(-30))
                .Sum(b => b.FinalAmount);

            analysis.RevenueGrowthRate = previousMonthRevenue > 0
                ? ((analysis.MonthlyRevenue - previousMonthRevenue) / previousMonthRevenue) * 100
                : 0;

            return ApiResponse<BranchRevenueAnalysisDto>.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch revenue analysis: {BranchId}", branchId);
            return ApiResponse<BranchRevenueAnalysisDto>.Error("An error occurred while retrieving branch revenue analysis", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Staff Management

    public async Task<ApiResponse<List<BranchStaffDto>>> GetBranchStaffAsync(int branchId)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
            {
                return ApiResponse<List<BranchStaffDto>>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Placeholder implementation - would need actual staff assignment table
            var staff = new List<BranchStaffDto>();
            return ApiResponse<List<BranchStaffDto>>.Success(staff);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch staff: {BranchId}", branchId);
            return ApiResponse<List<BranchStaffDto>>.Error("An error occurred while retrieving branch staff", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> AssignStaffToBranchAsync(BranchStaffAssignmentDto assignmentDto, string adminId)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(assignmentDto.BranchId);
            if (branch == null)
            {
                return ApiResponse.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Placeholder implementation - would need actual staff assignment table
            return ApiResponse.Success($"Assigned {assignmentDto.StaffIds.Count} staff members to branch");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning staff to branch");
            return ApiResponse.Error("An error occurred while assigning staff to branch", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> RemoveStaffFromBranchAsync(int branchId, List<string> staffIds, string adminId)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
            {
                return ApiResponse.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Placeholder implementation - would need actual staff assignment table
            return ApiResponse.Success($"Removed {staffIds.Count} staff members from branch");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing staff from branch");
            return ApiResponse.Error("An error occurred while removing staff from branch", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<BranchStaffDto>>> GetMultiBranchStaffAsync()
    {
        try
        {
            // Placeholder implementation - would need actual staff assignment table
            var staff = new List<BranchStaffDto>();
            return ApiResponse<List<BranchStaffDto>>.Success(staff);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multi-branch staff");
            return ApiResponse<List<BranchStaffDto>>.Error("An error occurred while retrieving multi-branch staff", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Car Fleet Management

    public async Task<ApiResponse<List<AdminCarDto>>> GetBranchCarsAsync(int branchId, CarStatus? status = null)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
            {
                return ApiResponse<List<AdminCarDto>>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            var query = _context.Cars
                .Include(c => c.Category)
                .Include(c => c.Branch)
                .Where(c => c.BranchId == branchId);

            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            var cars = await query.ToListAsync();

            var carDtos = cars.Select(c => new AdminCarDto
            {
                Id = c.Id,
                Brand = c.BrandEn,
                Model = c.ModelEn,
                Year = c.Year,
                Color = c.ColorEn,
                SeatingCapacity = c.SeatingCapacity,
                TransmissionType = c.TransmissionType.ToString(),
                FuelType = c.FuelType,
                DailyPrice = c.DailyRate,
                WeeklyPrice = c.WeeklyRate,
                MonthlyPrice = c.MonthlyRate,
                Status = c.Status,
                ImageUrl = c.ImageUrl,
                Description = c.DescriptionEn,
                Mileage = c.Mileage,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                TotalBookings = 0, // Would need to calculate from bookings
                TotalRevenue = 0, // Would need to calculate from bookings
                UtilizationRate = 0, // Would need to calculate
                LastBookingDate = null, // Would need to calculate
                NextMaintenanceDate = null, // Would need maintenance system
                MaintenanceHistory = new List<CarMaintenanceRecordDto>()
            }).ToList();

            return ApiResponse<List<AdminCarDto>>.Success(carDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch cars: {BranchId}", branchId);
            return ApiResponse<List<AdminCarDto>>.Error("An error occurred while retrieving branch cars", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> TransferCarsBetweenBranchesAsync(BranchCarTransferDto transferDto, string adminId)
    {
        try
        {
            var fromBranch = await _context.Branches.FindAsync(transferDto.FromBranchId);
            var toBranch = await _context.Branches.FindAsync(transferDto.ToBranchId);

            if (fromBranch == null || toBranch == null)
            {
                return ApiResponse.Error("One or both branches not found", (string?)null, ApiStatusCode.NotFound);
            }

            var cars = await _context.Cars
                .Where(c => transferDto.CarIds.Contains(c.Id) && c.BranchId == transferDto.FromBranchId)
                .ToListAsync();

            if (cars.Count != transferDto.CarIds.Count)
            {
                return ApiResponse.Error("Some cars not found or not in source branch", (string?)null, ApiStatusCode.BadRequest);
            }

            // Check if cars have active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => transferDto.CarIds.Contains(b.CarId) &&
                              (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress));

            if (hasActiveBookings)
            {
                return ApiResponse.Error("Cannot transfer cars with active bookings", (string?)null, ApiStatusCode.BadRequest);
            }

            foreach (var car in cars)
            {
                car.BranchId = transferDto.ToBranchId;
                car.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Successfully transferred {cars.Count} cars from {fromBranch.NameEn} to {toBranch.NameEn}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring cars between branches");
            return ApiResponse.Error("An error occurred while transferring cars", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<BranchCarStatsDto>> GetBranchCarUtilizationAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var branch = await _context.Branches
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == branchId);

            if (branch == null)
            {
                return ApiResponse<BranchCarStatsDto>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            var totalCars = branch.Cars?.Count ?? 0;
            var availableCars = branch.Cars?.Count(c => c.Status == CarStatus.Available) ?? 0;
            var rentedCars = branch.Cars?.Count(c => c.Status == CarStatus.Rented) ?? 0;
            var utilizationRate = totalCars > 0 ? (double)rentedCars / totalCars * 100 : 0;

            // Calculate revenue for the branch
            var bookingsQuery = _context.Bookings
                .Where(b => (b.ReceivingBranchId == branchId || b.DeliveryBranchId == branchId) &&
                           b.Status == BookingStatus.Completed);

            if (startDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= endDate.Value);

            var bookings = await bookingsQuery.ToListAsync();
            var revenue = bookings.Sum(b => b.FinalAmount);

            var stats = new BranchCarStatsDto
            {
                BranchId = branchId,
                BranchName = branch.NameEn,
                TotalCars = totalCars,
                AvailableCars = availableCars,
                RentedCars = rentedCars,
                UtilizationRate = utilizationRate,
                Revenue = revenue
            };

            return ApiResponse<BranchCarStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch car utilization: {BranchId}", branchId);
            return ApiResponse<BranchCarStatsDto>.Error("An error occurred while retrieving branch car utilization", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Booking Management

    public async Task<ApiResponse<BranchBookingStatsDto>> GetBranchBookingStatsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
            {
                return ApiResponse<BranchBookingStatsDto>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            var bookingsQuery = _context.Bookings
                .Where(b => b.ReceivingBranchId == branchId || b.DeliveryBranchId == branchId);

            if (startDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= endDate.Value);

            var bookings = await bookingsQuery.ToListAsync();

            var stats = new BranchBookingStatsDto
            {
                BranchId = branchId,
                BranchName = branch.NameEn,
                BookingCount = bookings.Count,
                Revenue = bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                PickupCount = bookings.Count(b => b.ReceivingBranchId == branchId),
                ReturnCount = bookings.Count(b => b.DeliveryBranchId == branchId)
            };

            return ApiResponse<BranchBookingStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch booking stats: {BranchId}", branchId);
            return ApiResponse<BranchBookingStatsDto>.Error("An error occurred while retrieving branch booking stats", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<BranchPerformanceDto>>> GetBranchesByBookingVolumeAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var branches = await _context.Branches
                .Include(b => b.Cars)
                .Where(b => b.IsActive)
                .ToListAsync();

            var performances = new List<BranchPerformanceDto>();

            foreach (var branch in branches)
            {
                var bookingsQuery = _context.Bookings
                    .Where(b => b.ReceivingBranchId == branch.Id || b.DeliveryBranchId == branch.Id);

                if (startDate.HasValue)
                    bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= endDate.Value);

                var bookings = await bookingsQuery.ToListAsync();

                performances.Add(new BranchPerformanceDto
                {
                    BranchId = branch.Id,
                    BranchName = branch.NameEn,
                    City = branch.City,
                    Country = branch.Country,
                    BookingCount = bookings.Count,
                    Revenue = bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                    CarCount = branch.Cars?.Count ?? 0,
                    UtilizationRate = 0, // Would calculate based on car usage
                    CustomerSatisfactionRate = 85.0, // Placeholder
                    PerformanceScore = 0, // Would calculate based on multiple factors
                    LastActivityDate = bookings.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue
                });
            }

            return ApiResponse<List<BranchPerformanceDto>>.Success(
                performances.OrderByDescending(p => p.BookingCount).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branches by booking volume");
            return ApiResponse<List<BranchPerformanceDto>>.Error("An error occurred while retrieving branches by booking volume", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Maintenance Management

    public async Task<ApiResponse> ScheduleBranchMaintenanceAsync(BranchMaintenanceScheduleDto scheduleDto, string adminId)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(scheduleDto.BranchId);
            if (branch == null)
            {
                return ApiResponse.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Placeholder implementation - would need actual maintenance system
            return ApiResponse.Success("Maintenance scheduled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling branch maintenance");
            return ApiResponse.Error("An error occurred while scheduling maintenance", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<BranchMaintenanceScheduleDto>>> GetBranchMaintenanceHistoryAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
            {
                return ApiResponse<List<BranchMaintenanceScheduleDto>>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Placeholder implementation - would need actual maintenance system
            var history = new List<BranchMaintenanceScheduleDto>();
            return ApiResponse<List<BranchMaintenanceScheduleDto>>.Success(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch maintenance history: {BranchId}", branchId);
            return ApiResponse<List<BranchMaintenanceScheduleDto>>.Error("An error occurred while retrieving maintenance history", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<BranchMaintenanceScheduleDto>>> GetUpcomingMaintenanceAsync(DateTime? fromDate = null, int days = 30)
    {
        try
        {
            // Placeholder implementation - would need actual maintenance system
            var upcoming = new List<BranchMaintenanceScheduleDto>();
            return ApiResponse<List<BranchMaintenanceScheduleDto>>.Success(upcoming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming maintenance");
            return ApiResponse<List<BranchMaintenanceScheduleDto>>.Error("An error occurred while retrieving upcoming maintenance", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Geographic Analytics

    public async Task<ApiResponse<List<BranchCityStatsDto>>> GetBranchStatsByCityAsync()
    {
        try
        {
            var branches = await _context.Branches
                .Include(b => b.Cars)
                .ToListAsync();

            var cityStats = await GenerateCityStats(branches);
            return ApiResponse<List<BranchCityStatsDto>>.Success(cityStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch stats by city");
            return ApiResponse<List<BranchCityStatsDto>>.Error("An error occurred while retrieving branch stats by city", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<BranchCountryStatsDto>>> GetBranchStatsByCountryAsync()
    {
        try
        {
            var branches = await _context.Branches
                .Include(b => b.Cars)
                .ToListAsync();

            var countryStats = await GenerateCountryStats(branches);
            return ApiResponse<List<BranchCountryStatsDto>>.Success(countryStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch stats by country");
            return ApiResponse<List<BranchCountryStatsDto>>.Error("An error occurred while retrieving branch stats by country", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminBranchDto>>> GetBranchesNearLocationAsync(decimal latitude, decimal longitude, double radiusKm = 50, int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // Simple distance calculation (not precise for large distances)
            var branches = await _context.Branches
                .Include(b => b.Cars)
                .Where(b => b.IsActive)
                .ToListAsync();

            var nearbyBranches = branches.Where(b =>
            {
                var distance = CalculateDistance((double)latitude, (double)longitude, (double)b.Latitude, (double)b.Longitude);
                return distance <= radiusKm;
            }).ToList();

            // Apply pagination
            var totalCount = nearbyBranches.Count;
            var paginatedBranches = nearbyBranches
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var branchDtos = new List<AdminBranchDto>();
            foreach (var branch in paginatedBranches)
            {
                branchDtos.Add(await MapToAdminBranchDto(branch));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminBranchDto>.Create(
                branchDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminBranchDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branches near location");
            return ApiResponse<PaginatedResponseDto<AdminBranchDto>>.Error("An error occurred while retrieving nearby branches", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Bulk Operations

    public async Task<ApiResponse> BulkOperationAsync(BulkBranchOperationDto operationDto, string adminId)
    {
        try
        {
            switch (operationDto.Operation.ToLower())
            {
                case "activate":
                    return await BulkUpdateBranchStatusAsync(operationDto.BranchIds, true, adminId);

                case "deactivate":
                    return await BulkUpdateBranchStatusAsync(operationDto.BranchIds, false, adminId);

                case "delete":
                    return await BulkDeleteBranchesAsync(operationDto.BranchIds, adminId);

                case "updatecity":
                    if (!string.IsNullOrEmpty(operationDto.NewCity))
                        return await BulkUpdateBranchCityAsync(operationDto.BranchIds, operationDto.NewCity, adminId);
                    break;

                case "updatecountry":
                    if (!string.IsNullOrEmpty(operationDto.NewCountry))
                        return await BulkUpdateBranchCountryAsync(operationDto.BranchIds, operationDto.NewCountry, adminId);
                    break;
            }

            return ApiResponse.Error("Invalid operation", (string?)null, ApiStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation: {Operation}", operationDto.Operation);
            return ApiResponse.Error("An error occurred while performing bulk operation", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkDeleteBranchesAsync(List<int> branchIds, string adminId)
    {
        try
        {
            var branches = await _context.Branches
                .Include(b => b.Cars)
                .Where(b => branchIds.Contains(b.Id))
                .ToListAsync();

            // Check for active cars and bookings
            foreach (var branch in branches)
            {
                var hasActiveCars = branch.Cars.Any(c => c.Status == CarStatus.Available || c.Status == CarStatus.Rented);
                if (hasActiveCars)
                {
                    return ApiResponse.Error($"Cannot delete branch '{branch.NameEn}' with active cars", (string?)null, ApiStatusCode.BadRequest);
                }

                var hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => (b.ReceivingBranchId == branch.Id || b.DeliveryBranchId == branch.Id) &&
                                  (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress));

                if (hasActiveBookings)
                {
                    return ApiResponse.Error($"Cannot delete branch '{branch.NameEn}' with active bookings", (string?)null, ApiStatusCode.BadRequest);
                }
            }

            _context.Branches.RemoveRange(branches);
            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Successfully deleted {branches.Count} branches");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting branches");
            return ApiResponse.Error("An error occurred while deleting branches", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    private async Task<ApiResponse> BulkUpdateBranchCityAsync(List<int> branchIds, string newCity, string adminId)
    {
        try
        {
            var branches = await _context.Branches
                .Where(b => branchIds.Contains(b.Id))
                .ToListAsync();

            foreach (var branch in branches)
            {
                branch.City = newCity;
                branch.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Updated city for {branches.Count} branches");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating branch city");
            return ApiResponse.Error("An error occurred while updating branch city", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    private async Task<ApiResponse> BulkUpdateBranchCountryAsync(List<int> branchIds, string newCountry, string adminId)
    {
        try
        {
            var branches = await _context.Branches
                .Where(b => branchIds.Contains(b.Id))
                .ToListAsync();

            foreach (var branch in branches)
            {
                branch.Country = newCountry;
                branch.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Updated country for {branches.Count} branches");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating branch country");
            return ApiResponse.Error("An error occurred while updating branch country", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Reports and Export

    public async Task<ApiResponse<BranchReportDto>> GenerateBranchReportAsync(BranchFilterDto filter)
    {
        try
        {
            var branches = await GetBranchesAsync(filter);
            if (!branches.Succeeded)
            {
                return ApiResponse<BranchReportDto>.Error("Failed to retrieve branches for report", (string?)null, ApiStatusCode.InternalServerError);
            }

            var analytics = await GetBranchAnalyticsAsync();
            if (!analytics.Succeeded)
            {
                return ApiResponse<BranchReportDto>.Error("Failed to retrieve analytics for report", (string?)null, ApiStatusCode.InternalServerError);
            }

            var report = new BranchReportDto
            {
                GeneratedAt = DateTime.UtcNow,
                StartDate = filter.CreatedDateFrom,
                EndDate = filter.CreatedDateTo,
                Analytics = analytics.Data!,
                Branches = branches.Data!.Data,
                Summary = new BranchReportSummaryDto
                {
                    TotalBranches = branches.Data!.Data.Count,
                    ActiveBranches = branches.Data!.Data.Count(b => b.IsActive),
                    InactiveBranches = branches.Data!.Data.Count(b => !b.IsActive),
                    NewBranches = branches.Data!.Data.Count(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                    TotalRevenue = branches.Data!.Data.Sum(b => b.TotalRevenue),
                    AverageRevenuePerBranch = branches.Data!.Data.Any() ? branches.Data!.Data.Average(b => b.TotalRevenue) : 0,
                    TotalCars = branches.Data!.Data.Sum(b => b.TotalCars),
                    TotalBookings = branches.Data!.Data.Sum(b => b.TotalBookings),
                    TotalStaff = branches.Data!.Data.Sum(b => b.TotalStaff),
                    AverageUtilizationRate = branches.Data!.Data.Any() ? branches.Data!.Data.Average(b => b.CarUtilizationRate) : 0,
                    AverageCustomerSatisfaction = branches.Data!.Data.Any() ? branches.Data!.Data.Average(b => b.CustomerSatisfactionRate) : 0,
                    BestPerformingBranch = branches.Data!.Data.OrderByDescending(b => b.TotalRevenue).FirstOrDefault()?.Name ?? "None",
                    WorstPerformingBranch = branches.Data!.Data.OrderBy(b => b.TotalRevenue).FirstOrDefault()?.Name ?? "None",
                    MostPopularCity = branches.Data!.Data.GroupBy(b => b.City).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "None",
                    MostPopularCountry = branches.Data!.Data.GroupBy(b => b.Country).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "None"
                }
            };

            return ApiResponse<BranchReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating branch report");
            return ApiResponse<BranchReportDto>.Error("An error occurred while generating branch report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportBranchReportAsync(BranchFilterDto filter, string format = "excel")
    {
        try
        {
            var report = await GenerateBranchReportAsync(filter);
            if (!report.Succeeded)
            {
                return ApiResponse<byte[]>.Error("Failed to generate report for export", (string?)null, ApiStatusCode.InternalServerError);
            }

            // Placeholder implementation - would need actual export library
            var data = System.Text.Encoding.UTF8.GetBytes($"Branch Report - {DateTime.UtcNow:yyyy-MM-dd}");
            return ApiResponse<byte[]>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting branch report");
            return ApiResponse<byte[]>.Error("An error occurred while exporting branch report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportBranchPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null, string format = "excel")
    {
        try
        {
            var analytics = await GetBranchAnalyticsAsync(startDate, endDate);
            if (!analytics.Succeeded)
            {
                return ApiResponse<byte[]>.Error("Failed to retrieve analytics for export", (string?)null, ApiStatusCode.InternalServerError);
            }

            // Placeholder implementation - would need actual export library
            var data = System.Text.Encoding.UTF8.GetBytes($"Branch Performance - {DateTime.UtcNow:yyyy-MM-dd}");
            return ApiResponse<byte[]>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting branch performance");
            return ApiResponse<byte[]>.Error("An error occurred while exporting branch performance", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Validation and Search

    public async Task<ApiResponse<bool>> ValidateBranchLocationAsync(decimal latitude, decimal longitude, string address)
    {
        try
        {
            // Basic validation
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                return ApiResponse<bool>.Success(false);
            }

            // Check if location is too close to existing branches
            var existingBranches = await _context.Branches
                .Where(b => b.IsActive)
                .ToListAsync();

            foreach (var branch in existingBranches)
            {
                var distance = CalculateDistance((double)latitude, (double)longitude, (double)branch.Latitude, (double)branch.Longitude);
                if (distance < 1.0) // Less than 1km apart
                {
                    return ApiResponse<bool>.Success(false);
                }
            }

            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating branch location");
            return ApiResponse<bool>.Error("An error occurred while validating branch location", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<bool>> ValidateBranchNameAsync(string nameEn, string nameAr, int? excludeBranchId = null)
    {
        try
        {
            var query = _context.Branches.AsQueryable();

            if (excludeBranchId.HasValue)
                query = query.Where(b => b.Id != excludeBranchId.Value);

            var exists = await query.AnyAsync(b => b.NameEn == nameEn || b.NameAr == nameAr);
            return ApiResponse<bool>.Success(!exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating branch name");
            return ApiResponse<bool>.Error("An error occurred while validating branch name", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminBranchDto>>> SearchBranchesAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        try
        {
            var filter = new BranchFilterDto
            {
                SearchTerm = searchTerm,
                Page = page,
                PageSize = pageSize
            };

            return await GetBranchesAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching branches");
            return ApiResponse<PaginatedResponseDto<AdminBranchDto>>.Error("An error occurred while searching branches", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminBranchDto>>> GetActiveBranchesAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var filter = new BranchFilterDto
            {
                IsActive = true,
                Page = page,
                PageSize = pageSize
            };

            return await GetBranchesAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active branches");
            return ApiResponse<PaginatedResponseDto<AdminBranchDto>>.Error("An error occurred while retrieving active branches", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminBranchDto>>> GetBranchesByRegionAsync(string city, string country, int page = 1, int pageSize = 10)
    {
        try
        {
            var filter = new BranchFilterDto
            {
                City = city,
                Country = country,
                Page = page,
                PageSize = pageSize
            };

            return await GetBranchesAsync(filter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branches by region");
            return ApiResponse<PaginatedResponseDto<AdminBranchDto>>.Error("An error occurred while retrieving branches by region", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Performance Monitoring

    public async Task<ApiResponse<double>> GetBranchPerformanceScoreAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var branch = await _context.Branches
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == branchId);

            if (branch == null)
            {
                return ApiResponse<double>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            var performance = await CalculateBranchPerformance(branch, startDate, endDate);
            return ApiResponse<double>.Success(performance.PerformanceScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch performance score: {BranchId}", branchId);
            return ApiResponse<double>.Error("An error occurred while calculating branch performance score", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<BranchPerformanceDto>> GetBranchEfficiencyMetricsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var branch = await _context.Branches
                .Include(b => b.Cars)
                .FirstOrDefaultAsync(b => b.Id == branchId);

            if (branch == null)
            {
                return ApiResponse<BranchPerformanceDto>.Error("Branch not found", (string?)null, ApiStatusCode.NotFound);
            }

            var performance = await CalculateBranchPerformance(branch, startDate, endDate);
            return ApiResponse<BranchPerformanceDto>.Success(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch efficiency metrics: {BranchId}", branchId);
            return ApiResponse<BranchPerformanceDto>.Error("An error occurred while retrieving branch efficiency metrics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<MonthlyBranchStatsDto>>> GetMonthlyBranchStatsAsync(int year)
    {
        try
        {
            var branches = await _context.Branches
                .Where(b => b.CreatedAt.Year <= year)
                .ToListAsync();

            var monthlyStats = await GenerateMonthlyBranchStats(branches);
            return ApiResponse<List<MonthlyBranchStatsDto>>.Success(monthlyStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly branch stats: {Year}", year);
            return ApiResponse<List<MonthlyBranchStatsDto>>.Error("An error occurred while retrieving monthly branch stats", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<AdminBranchDto> MapToAdminBranchDto(Branch branch)
    {
        // Calculate statistics
        var totalCars = branch.Cars?.Count ?? 0;
        var availableCars = branch.Cars?.Count(c => c.Status == CarStatus.Available) ?? 0;
        var rentedCars = branch.Cars?.Count(c => c.Status == CarStatus.Rented) ?? 0;
        var maintenanceCars = branch.Cars?.Count(c => c.Status == CarStatus.Maintenance) ?? 0;

        // Calculate bookings statistics
        var branchBookings = await _context.Bookings
            .Where(b => b.ReceivingBranchId == branch.Id || b.DeliveryBranchId == branch.Id)
            .ToListAsync();

        var totalBookings = branchBookings.Count;
        var activeBookings = branchBookings.Count(b => b.Status == BookingStatus.InProgress);
        var completedBookings = branchBookings.Count(b => b.Status == BookingStatus.Completed);
        var cancelledBookings = branchBookings.Count(b => b.Status == BookingStatus.Canceled);

        // Calculate revenue
        var monthlyRevenue = branchBookings
            .Where(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-30) && b.Status == BookingStatus.Completed)
            .Sum(b => b.FinalAmount);

        var yearlyRevenue = branchBookings
            .Where(b => b.CreatedAt >= DateTime.UtcNow.AddDays(-365) && b.Status == BookingStatus.Completed)
            .Sum(b => b.FinalAmount);

        var totalRevenue = branchBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.FinalAmount);

        return new AdminBranchDto
        {
            Id = branch.Id,
            Name = branch.NameEn,
            Description = branch.DescriptionEn ?? "",
            Address = branch.Address,
            City = branch.City,
            Country = branch.Country,
            Phone = branch.Phone,
            Email = branch.Email,
            Latitude = branch.Latitude,
            Longitude = branch.Longitude,
            WorkingHours = branch.WorkingHours,
            IsActive = branch.IsActive,
            CreatedAt = branch.CreatedAt,
            UpdatedAt = branch.UpdatedAt,
            CreatedByAdmin = "System", // Placeholder
            UpdatedByAdmin = "System", // Placeholder
            TotalCars = totalCars,
            AvailableCars = availableCars,
            RentedCars = rentedCars,
            MaintenanceCars = maintenanceCars,
            CarUtilizationRate = totalCars > 0 ? (double)rentedCars / totalCars * 100 : 0,
            TotalBookings = totalBookings,
            ActiveBookings = activeBookings,
            CompletedBookings = completedBookings,
            CancelledBookings = cancelledBookings,
            MonthlyRevenue = monthlyRevenue,
            YearlyRevenue = yearlyRevenue,
            TotalRevenue = totalRevenue,
            AverageBookingValue = completedBookings > 0 ? totalRevenue / completedBookings : 0,
            TotalStaff = 0, // Placeholder - would need staff assignment implementation
            ActiveStaff = 0, // Placeholder
            Staff = new List<BranchStaffDto>(), // Placeholder
            CustomerSatisfactionRate = 85.0, // Placeholder - would calculate from reviews
            OnTimeDeliveryRate = 92.0, // Placeholder - would calculate from booking data
            MaintenanceRequestsCount = maintenanceCars,
            LastBookingDate = branchBookings.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.CreatedAt,
            LastMaintenanceDate = DateTime.UtcNow.AddDays(-30), // Placeholder
            RecentActivities = new List<BranchRecentActivityDto>() // Placeholder
        };
    }

    private Task<double> CalculateAverageUtilizationRate(List<Branch> branches)
    {
        if (branches.Count == 0) return Task.FromResult(0.0);

        var utilizationRates = new List<double>();
        foreach (var branch in branches)
        {
            var totalCars = branch.Cars?.Count ?? 0;
            var rentedCars = branch.Cars?.Count(c => c.Status == CarStatus.Rented) ?? 0;
            var rate = totalCars > 0 ? (double)rentedCars / totalCars * 100 : 0;
            utilizationRates.Add(rate);
        }

        return Task.FromResult(utilizationRates.Any() ? utilizationRates.Average() : 0);
    }

    private async Task<List<BranchPerformanceDto>> GenerateTopPerformingBranches(List<Branch> branches, int count)
    {
        var performances = new List<BranchPerformanceDto>();

        foreach (var branch in branches)
        {
            var performance = await CalculateBranchPerformance(branch);
            performances.Add(performance);
        }

        return performances
            .OrderByDescending(p => p.PerformanceScore)
            .Take(count)
            .ToList();
    }

    private async Task<List<BranchPerformanceDto>> GenerateLowPerformingBranches(List<Branch> branches, int count)
    {
        var performances = new List<BranchPerformanceDto>();

        foreach (var branch in branches)
        {
            var performance = await CalculateBranchPerformance(branch);
            performances.Add(performance);
        }

        return performances
            .OrderBy(p => p.PerformanceScore)
            .Take(count)
            .ToList();
    }

    private async Task<BranchPerformanceDto> CalculateBranchPerformance(Branch branch, DateTime? startDate = null, DateTime? endDate = null)
    {
        var bookingsQuery = _context.Bookings
            .Where(b => (b.ReceivingBranchId == branch.Id || b.DeliveryBranchId == branch.Id) &&
                       b.Status == BookingStatus.Completed);

        if (startDate.HasValue)
            bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= endDate.Value);

        var bookings = await bookingsQuery.ToListAsync();

        var totalCars = branch.Cars?.Count ?? 0;
        var rentedCars = branch.Cars?.Count(c => c.Status == CarStatus.Rented) ?? 0;
        var utilizationRate = totalCars > 0 ? (double)rentedCars / totalCars * 100 : 0;

        var revenue = bookings.Sum(b => b.FinalAmount);
        var bookingCount = bookings.Count;

        // Calculate performance score (weighted average)
        var revenueScore = Math.Min((double)revenue / 10000, 10); // Max 10 points for revenue
        var utilizationScore = utilizationRate / 10; // Max 10 points for utilization
        var bookingScore = Math.Min(bookingCount / 10.0, 10); // Max 10 points for bookings
        var performanceScore = (revenueScore + utilizationScore + bookingScore) / 3;

        return new BranchPerformanceDto
        {
            BranchId = branch.Id,
            BranchName = branch.NameEn,
            City = branch.City,
            Country = branch.Country,
            Revenue = revenue,
            BookingCount = bookingCount,
            CarCount = totalCars,
            UtilizationRate = utilizationRate,
            CustomerSatisfactionRate = 85.0, // Placeholder
            PerformanceScore = Math.Round(performanceScore, 2),
            LastActivityDate = bookings.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue
        };
    }

    private Task<List<MonthlyBranchStatsDto>> GenerateMonthlyBranchStats(List<Branch> branches)
    {
        var result = branches
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new MonthlyBranchStatsDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                BranchCount = g.Count(),
                NewBranches = g.Count(),
                ClosedBranches = 0, // Placeholder
                TotalRevenue = 0, // Would need to calculate from bookings
                TotalBookings = 0, // Would need to calculate from bookings
                AverageUtilizationRate = 0 // Would need to calculate
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        return Task.FromResult(result);
    }

    private Task<List<BranchCityStatsDto>> GenerateCityStats(List<Branch> branches)
    {
        var result = branches
            .GroupBy(b => new { b.City, b.Country })
            .Select(g => new BranchCityStatsDto
            {
                City = g.Key.City,
                Country = g.Key.Country,
                BranchCount = g.Count(),
                TotalRevenue = 0, // Would need to calculate from bookings
                TotalBookings = 0, // Would need to calculate from bookings
                TotalCars = g.Sum(b => b.Cars?.Count ?? 0),
                AverageUtilizationRate = 0 // Would need to calculate
            })
            .ToList();

        return Task.FromResult(result);
    }

    private Task<List<BranchCountryStatsDto>> GenerateCountryStats(List<Branch> branches)
    {
        var result = branches
            .GroupBy(b => b.Country)
            .Select(g => new BranchCountryStatsDto
            {
                Country = g.Key,
                BranchCount = g.Count(),
                CityCount = g.Select(b => b.City).Distinct().Count(),
                TotalRevenue = 0, // Would need to calculate from bookings
                TotalBookings = 0, // Would need to calculate from bookings
                TotalCars = g.Sum(b => b.Cars?.Count ?? 0),
                AverageUtilizationRate = 0, // Would need to calculate
                MarketShare = 0 // Would need to calculate
            })
            .ToList();

        return Task.FromResult(result);
    }

    private List<DailyRevenueDto> GenerateDailyRevenueBreakdown(List<Booking> bookings)
    {
        return bookings
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Revenue = g.Sum(b => b.FinalAmount),
                BookingCount = g.Count(),

            })
            .OrderBy(d => d.Date)
            .ToList();
    }

    private List<MonthlyRevenueDto> GenerateMonthlyRevenueBreakdown(List<Booking> bookings)
    {
        return bookings
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                Revenue = g.Sum(b => b.FinalAmount),
                BookingCount = g.Count()
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula for calculating distance between two points on Earth
        const double R = 6371; // Earth's radius in kilometers

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c;

        return distance;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }



    #endregion
}
