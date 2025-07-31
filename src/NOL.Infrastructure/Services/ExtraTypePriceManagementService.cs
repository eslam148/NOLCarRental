using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Services;

public class ExtraTypePriceManagementService : IExtraTypePriceManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExtraTypePriceManagementService> _logger;

    public ExtraTypePriceManagementService(
        ApplicationDbContext context,
        ILogger<ExtraTypePriceManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetExtraTypePricesAsync(ExtraTypePriceFilterDto filter)
    {
        try
        {
            var query = _context.ExtraTypePrices
                .Include(e => e.BookingExtras)
                .AsQueryable();

            // Apply filters
            if (filter.ExtraType.HasValue)
                query = query.Where(e => e.ExtraType == filter.ExtraType.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(e => e.IsActive == filter.IsActive.Value);

            if (filter.MinDailyPrice.HasValue)
                query = query.Where(e => e.DailyPrice >= filter.MinDailyPrice.Value);

            if (filter.MaxDailyPrice.HasValue)
                query = query.Where(e => e.DailyPrice <= filter.MaxDailyPrice.Value);

            if (filter.MinWeeklyPrice.HasValue)
                query = query.Where(e => e.WeeklyPrice >= filter.MinWeeklyPrice.Value);

            if (filter.MaxWeeklyPrice.HasValue)
                query = query.Where(e => e.WeeklyPrice <= filter.MaxWeeklyPrice.Value);

            if (filter.MinMonthlyPrice.HasValue)
                query = query.Where(e => e.MonthlyPrice >= filter.MinMonthlyPrice.Value);

            if (filter.MaxMonthlyPrice.HasValue)
                query = query.Where(e => e.MonthlyPrice <= filter.MaxMonthlyPrice.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(e => 
                    e.NameEn.ToLower().Contains(searchTerm) ||
                    e.NameAr.Contains(searchTerm) ||
                    e.DescriptionEn.ToLower().Contains(searchTerm) ||
                    e.DescriptionAr.Contains(searchTerm));
            }

            if (filter.CreatedAfter.HasValue)
                query = query.Where(e => e.CreatedAt >= filter.CreatedAfter.Value);

            if (filter.CreatedBefore.HasValue)
                query = query.Where(e => e.CreatedAt <= filter.CreatedBefore.Value);

            if (filter.UpdatedAfter.HasValue)
                query = query.Where(e => e.UpdatedAt >= filter.UpdatedAfter.Value);

            if (filter.UpdatedBefore.HasValue)
                query = query.Where(e => e.UpdatedAt <= filter.UpdatedBefore.Value);

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "namear" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(e => e.NameAr) : query.OrderBy(e => e.NameAr),
                "nameen" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(e => e.NameEn) : query.OrderBy(e => e.NameEn),
                "extratype" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(e => e.ExtraType) : query.OrderBy(e => e.ExtraType),
                "dailyprice" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(e => e.DailyPrice) : query.OrderBy(e => e.DailyPrice),
                "weeklyprice" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(e => e.WeeklyPrice) : query.OrderBy(e => e.WeeklyPrice),
                "monthlyprice" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(e => e.MonthlyPrice) : query.OrderBy(e => e.MonthlyPrice),
                "createdat" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
                "updatedat" => filter.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(e => e.UpdatedAt) : query.OrderBy(e => e.UpdatedAt),
                _ => query.OrderBy(e => e.NameEn)
            };

            // Apply pagination
            var totalCount = await query.CountAsync();
            var extraTypePrices = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = extraTypePrices.Select(MapToAdminDto).ToList();

            return ApiResponse<List<AdminExtraTypePriceDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting extra type prices");
            return ApiResponse<List<AdminExtraTypePriceDto>>.Error("An error occurred while retrieving extra type prices", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminExtraTypePriceDto>> GetExtraTypePriceByIdAsync(int id)
    {
        try
        {
            var extraTypePrice = await _context.ExtraTypePrices
                .Include(e => e.BookingExtras)
                .ThenInclude(be => be.Booking)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extraTypePrice == null)
            {
                return ApiResponse<AdminExtraTypePriceDto>.Error("Extra type price not found", (string?)null, ApiStatusCode.NotFound);
            }

            var result = MapToAdminDto(extraTypePrice);
            return ApiResponse<AdminExtraTypePriceDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting extra type price by ID: {Id}", id);
            return ApiResponse<AdminExtraTypePriceDto>.Error("An error occurred while retrieving extra type price", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminExtraTypePriceDto>> CreateExtraTypePriceAsync(CreateExtraTypePriceDto createDto, string adminId)
    {
        try
        {
            // Validate business rules
            var validation = await ValidateExtraTypePriceAsync(createDto);
            if (!validation.Data!.IsValid)
            {
                return ApiResponse<AdminExtraTypePriceDto>.Error("Validation failed", validation.Data.Errors, ApiStatusCode.BadRequest);
            }

            var extraTypePrice = new ExtraTypePrice
            {
                ExtraType = createDto.ExtraType,
                NameAr = createDto.NameAr,
                NameEn = createDto.NameEn,
                DescriptionAr = createDto.DescriptionAr,
                DescriptionEn = createDto.DescriptionEn,
                DailyPrice = createDto.DailyPrice,
                WeeklyPrice = createDto.WeeklyPrice,
                MonthlyPrice = createDto.MonthlyPrice,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ExtraTypePrices.Add(extraTypePrice);
            await _context.SaveChangesAsync();

            var result = MapToAdminDto(extraTypePrice);
            _logger.LogInformation("Extra type price created: {Id} by {AdminId}", extraTypePrice.Id, adminId);

            return ApiResponse<AdminExtraTypePriceDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating extra type price");
            return ApiResponse<AdminExtraTypePriceDto>.Error("An error occurred while creating extra type price", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminExtraTypePriceDto>> UpdateExtraTypePriceAsync(int id, UpdateExtraTypePriceDto updateDto, string adminId)
    {
        try
        {
            var extraTypePrice = await _context.ExtraTypePrices.FindAsync(id);
            if (extraTypePrice == null)
            {
                return ApiResponse<AdminExtraTypePriceDto>.Error("Extra type price not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Update only provided fields
            if (updateDto.NameAr != null) extraTypePrice.NameAr = updateDto.NameAr;
            if (updateDto.NameEn != null) extraTypePrice.NameEn = updateDto.NameEn;
            if (updateDto.DescriptionAr != null) extraTypePrice.DescriptionAr = updateDto.DescriptionAr;
            if (updateDto.DescriptionEn != null) extraTypePrice.DescriptionEn = updateDto.DescriptionEn;
            if (updateDto.DailyPrice.HasValue) extraTypePrice.DailyPrice = updateDto.DailyPrice.Value;
            if (updateDto.WeeklyPrice.HasValue) extraTypePrice.WeeklyPrice = updateDto.WeeklyPrice.Value;
            if (updateDto.MonthlyPrice.HasValue) extraTypePrice.MonthlyPrice = updateDto.MonthlyPrice.Value;
            if (updateDto.IsActive.HasValue) extraTypePrice.IsActive = updateDto.IsActive.Value;

            extraTypePrice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = MapToAdminDto(extraTypePrice);
            _logger.LogInformation("Extra type price updated: {Id} by {AdminId}", id, adminId);

            return ApiResponse<AdminExtraTypePriceDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating extra type price: {Id}", id);
            return ApiResponse<AdminExtraTypePriceDto>.Error("An error occurred while updating extra type price", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> DeleteExtraTypePriceAsync(int id, string adminId)
    {
        try
        {
            var extraTypePrice = await _context.ExtraTypePrices
                .Include(e => e.BookingExtras)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extraTypePrice == null)
            {
                return ApiResponse.Error("Extra type price not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Check if it's used in any bookings
            if (extraTypePrice.BookingExtras.Any())
            {
                return ApiResponse.Error("Cannot delete extra type price that is used in bookings", (string?)null, ApiStatusCode.BadRequest);
            }

            _context.ExtraTypePrices.Remove(extraTypePrice);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Extra type price deleted: {Id} by {AdminId}", id, adminId);
            return ApiResponse.Success("Extra type price deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting extra type price: {Id}", id);
            return ApiResponse.Error("An error occurred while deleting extra type price", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<BulkOperationResultDto>> BulkDeleteExtraTypePricesAsync(List<int> ids, string adminId)
    {
        try
        {
            var result = new BulkOperationResultDto
            {
                TotalItems = ids.Count,
                SuccessfulItems = 0,
                FailedItems = 0,
                Errors = new List<string>()
            };

            foreach (var id in ids)
            {
                var deleteResult = await DeleteExtraTypePriceAsync(id, adminId);
                if (deleteResult.Succeeded)
                {
                    result.SuccessfulItems++;
                }
                else
                {
                    result.FailedItems++;
                    result.Errors.Add($"ID {id}: {deleteResult.Message}");
                }
            }

            return ApiResponse<BulkOperationResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting extra type prices");
            return ApiResponse<BulkOperationResultDto>.Error("An error occurred while bulk deleting extra type prices", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Status Management

    public async Task<ApiResponse<AdminExtraTypePriceDto>> ActivateExtraTypePriceAsync(int id, string adminId)
    {
        try
        {
            var extraTypePrice = await _context.ExtraTypePrices.FindAsync(id);
            if (extraTypePrice == null)
            {
                return ApiResponse<AdminExtraTypePriceDto>.Error("Extra type price not found", (string?)null, ApiStatusCode.NotFound);
            }

            extraTypePrice.IsActive = true;
            extraTypePrice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = MapToAdminDto(extraTypePrice);
            _logger.LogInformation("Extra type price activated: {Id} by {AdminId}", id, adminId);

            return ApiResponse<AdminExtraTypePriceDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating extra type price: {Id}", id);
            return ApiResponse<AdminExtraTypePriceDto>.Error("An error occurred while activating extra type price", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminExtraTypePriceDto>> DeactivateExtraTypePriceAsync(int id, string adminId)
    {
        try
        {
            var extraTypePrice = await _context.ExtraTypePrices.FindAsync(id);
            if (extraTypePrice == null)
            {
                return ApiResponse<AdminExtraTypePriceDto>.Error("Extra type price not found", (string?)null, ApiStatusCode.NotFound);
            }

            extraTypePrice.IsActive = false;
            extraTypePrice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = MapToAdminDto(extraTypePrice);
            _logger.LogInformation("Extra type price deactivated: {Id} by {AdminId}", id, adminId);

            return ApiResponse<AdminExtraTypePriceDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating extra type price: {Id}", id);
            return ApiResponse<AdminExtraTypePriceDto>.Error("An error occurred while deactivating extra type price", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<BulkOperationResultDto>> BulkUpdateStatusAsync(List<int> ids, bool isActive, string adminId)
    {
        try
        {
            var extraTypePrices = await _context.ExtraTypePrices
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();

            var result = new BulkOperationResultDto
            {
                TotalItems = ids.Count,
                SuccessfulItems = extraTypePrices.Count,
                FailedItems = ids.Count - extraTypePrices.Count,
                Errors = new List<string>()
            };

            foreach (var extraTypePrice in extraTypePrices)
            {
                extraTypePrice.IsActive = isActive;
                extraTypePrice.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk status update completed: {Count} items {Status} by {AdminId}", 
                extraTypePrices.Count, isActive ? "activated" : "deactivated", adminId);

            return ApiResponse<BulkOperationResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating status");
            return ApiResponse<BulkOperationResultDto>.Error("An error occurred while bulk updating status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Helper Methods and Placeholder Implementations

    private AdminExtraTypePriceDto MapToAdminDto(ExtraTypePrice extraTypePrice)
    {
        return new AdminExtraTypePriceDto
        {
            Id = extraTypePrice.Id,
            ExtraType = extraTypePrice.ExtraType,
            ExtraTypeName = extraTypePrice.ExtraType.ToString(),
            NameAr = extraTypePrice.NameAr,
            NameEn = extraTypePrice.NameEn,
            DescriptionAr = extraTypePrice.DescriptionAr,
            DescriptionEn = extraTypePrice.DescriptionEn,
            DailyPrice = extraTypePrice.DailyPrice,
            WeeklyPrice = extraTypePrice.WeeklyPrice,
            MonthlyPrice = extraTypePrice.MonthlyPrice,
            IsActive = extraTypePrice.IsActive,
            CreatedAt = extraTypePrice.CreatedAt,
            UpdatedAt = extraTypePrice.UpdatedAt,
            TotalBookings = extraTypePrice.BookingExtras?.Count ?? 0,
            ActiveBookings = extraTypePrice.BookingExtras?.Count(be => be.Booking.Status == BookingStatus.Confirmed || be.Booking.Status == BookingStatus.InProgress) ?? 0,
            TotalRevenue = extraTypePrice.BookingExtras?.Sum(be => be.TotalPrice) ?? 0,
            AverageRating = 4.5m, // Placeholder
            UsageCount = extraTypePrice.BookingExtras?.Count ?? 0,
            LastUsed = extraTypePrice.BookingExtras?.OrderByDescending(be => be.CreatedAt).FirstOrDefault()?.CreatedAt
        };
    }

    private decimal CalculatePopularityScore(ExtraTypePrice extraTypePrice)
    {
        var bookingCount = extraTypePrice.BookingExtras?.Count ?? 0;
        var revenue = extraTypePrice.BookingExtras?.Sum(be => be.TotalPrice) ?? 0;
        var recentUsage = extraTypePrice.BookingExtras?.Count(be => be.CreatedAt >= DateTime.UtcNow.AddDays(-30)) ?? 0;

        // Simple popularity score calculation
        return (bookingCount * 0.4m) + (revenue * 0.0001m) + (recentUsage * 0.6m);
    }

    // Placeholder implementations for remaining interface methods
    public async Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetExtraTypePricesByTypeAsync(ExtraType extraType) =>
        await GetExtraTypePricesAsync(new ExtraTypePriceFilterDto { ExtraType = extraType });

    public async Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetActiveExtraTypePricesAsync() =>
        await GetExtraTypePricesAsync(new ExtraTypePriceFilterDto { IsActive = true });

    public async Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetInactiveExtraTypePricesAsync() =>
        await GetExtraTypePricesAsync(new ExtraTypePriceFilterDto { IsActive = false });

    public async Task<ApiResponse<List<AdminExtraTypePriceDto>>> SearchExtraTypePricesAsync(string searchTerm, string language = "en") =>
        await GetExtraTypePricesAsync(new ExtraTypePriceFilterDto { SearchTerm = searchTerm, Language = language });

    public async Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetExtraTypePricesByPriceRangeAsync(decimal minPrice, decimal maxPrice, string priceType = "daily") =>
        await GetExtraTypePricesAsync(new ExtraTypePriceFilterDto { MinDailyPrice = minPrice, MaxDailyPrice = maxPrice });

    public async Task<ApiResponse<ExtraTypePriceAnalyticsDto>> GetExtraTypePriceAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null) =>
        ApiResponse<ExtraTypePriceAnalyticsDto>.Success(new ExtraTypePriceAnalyticsDto());

    public async Task<ApiResponse<List<PopularExtraTypePriceDto>>> GetPopularExtraTypePricesAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null) =>
        ApiResponse<List<PopularExtraTypePriceDto>>.Success(new List<PopularExtraTypePriceDto>());

    public async Task<ApiResponse<List<ExtraTypePriceUsageStatsDto>>> GetExtraTypePriceUsageStatsAsync(DateTime? startDate = null, DateTime? endDate = null) =>
        ApiResponse<List<ExtraTypePriceUsageStatsDto>>.Success(new List<ExtraTypePriceUsageStatsDto>());

    public async Task<ApiResponse<List<ExtraTypeRevenueDto>>> GetRevenueByExtraTypeAsync(DateTime? startDate = null, DateTime? endDate = null) =>
        ApiResponse<List<ExtraTypeRevenueDto>>.Success(new List<ExtraTypeRevenueDto>());

    public async Task<ApiResponse<AdminExtraTypePriceDto>> UpdatePricingAsync(int id, UpdateExtraTypePricingDto pricingDto, string adminId) =>
        await UpdateExtraTypePriceAsync(id, new UpdateExtraTypePriceDto { DailyPrice = pricingDto.DailyPrice, WeeklyPrice = pricingDto.WeeklyPrice, MonthlyPrice = pricingDto.MonthlyPrice }, adminId);

    public async Task<ApiResponse<BulkOperationResultDto>> BulkUpdatePricingAsync(List<BulkPricingUpdateDto> pricingUpdates, string adminId) =>
        ApiResponse<BulkOperationResultDto>.Success(new BulkOperationResultDto());

    public async Task<ApiResponse<BulkOperationResultDto>> ApplyPricingAdjustmentAsync(List<int> ids, decimal percentage, bool isIncrease, string adminId) =>
        ApiResponse<BulkOperationResultDto>.Success(new BulkOperationResultDto());

    public async Task<ApiResponse<List<ExtraTypePricingHistoryDto>>> GetPricingHistoryAsync(int id) =>
        ApiResponse<List<ExtraTypePricingHistoryDto>>.Success(new List<ExtraTypePricingHistoryDto>());

    public async Task<ApiResponse<ValidationResultDto>> ValidateExtraTypePriceAsync(CreateExtraTypePriceDto createDto)
    {
        var result = new ValidationResultDto { IsValid = true };

        // Basic validation
        if (createDto.DailyPrice <= 0) result.Errors.Add("Daily price must be greater than 0");
        if (createDto.WeeklyPrice <= 0) result.Errors.Add("Weekly price must be greater than 0");
        if (createDto.MonthlyPrice <= 0) result.Errors.Add("Monthly price must be greater than 0");
        if (string.IsNullOrEmpty(createDto.NameEn)) result.Errors.Add("English name is required");
        if (string.IsNullOrEmpty(createDto.NameAr)) result.Errors.Add("Arabic name is required");

        result.IsValid = !result.Errors.Any();
        return ApiResponse<ValidationResultDto>.Success(result);
    }

    public async Task<ApiResponse<bool>> IsNameUniqueAsync(string nameEn, string nameAr, int? excludeId = null)
    {
        var exists = await _context.ExtraTypePrices
            .Where(e => (e.NameEn == nameEn || e.NameAr == nameAr) && (!excludeId.HasValue || e.Id != excludeId.Value))
            .AnyAsync();
        return ApiResponse<bool>.Success(!exists);
    }

    public async Task<ApiResponse<ExtraTypePriceValidationRulesDto>> GetValidationRulesAsync() =>
        ApiResponse<ExtraTypePriceValidationRulesDto>.Success(new ExtraTypePriceValidationRulesDto());

    public async Task<ApiResponse<byte[]>> ExportExtraTypePricesToExcelAsync(ExtraTypePriceFilterDto filter) =>
        ApiResponse<byte[]>.Success(new byte[0]);

    public async Task<ApiResponse<byte[]>> ExportExtraTypePricesToCsvAsync(ExtraTypePriceFilterDto filter) =>
        ApiResponse<byte[]>.Success(new byte[0]);

    public async Task<ApiResponse<ImportResultDto>> ImportExtraTypePricesFromExcelAsync(byte[] fileData, string adminId) =>
        ApiResponse<ImportResultDto>.Success(new ImportResultDto());

    public async Task<ApiResponse<byte[]>> GetImportTemplateAsync() =>
        ApiResponse<byte[]>.Success(new byte[0]);

    public async Task<ApiResponse<AdminExtraTypePriceDto>> GetLocalizedExtraTypePriceAsync(int id, string language = "en") =>
        await GetExtraTypePriceByIdAsync(id);

    public async Task<ApiResponse<AdminExtraTypePriceDto>> UpdateLocalizationAsync(int id, UpdateExtraTypePriceLocalizationDto localizationDto, string adminId) =>
        await UpdateExtraTypePriceAsync(id, new UpdateExtraTypePriceDto { NameAr = localizationDto.NameAr, NameEn = localizationDto.NameEn, DescriptionAr = localizationDto.DescriptionAr, DescriptionEn = localizationDto.DescriptionEn }, adminId);

    public async Task<ApiResponse<List<AdminExtraTypePriceDto>>> GetExtraTypePricesUsedInBookingsAsync(DateTime? startDate = null, DateTime? endDate = null) =>
        ApiResponse<List<AdminExtraTypePriceDto>>.Success(new List<AdminExtraTypePriceDto>());

    public async Task<ApiResponse<int>> GetBookingCountForExtraTypePriceAsync(int id, DateTime? startDate = null, DateTime? endDate = null)
    {
        var count = await _context.BookingExtras
            .Where(be => be.ExtraTypePriceId == id &&
                        (!startDate.HasValue || be.CreatedAt >= startDate.Value) &&
                        (!endDate.HasValue || be.CreatedAt <= endDate.Value))
            .CountAsync();
        return ApiResponse<int>.Success(count);
    }

    public async Task<ApiResponse<bool>> CanDeleteExtraTypePriceAsync(int id)
    {
        var hasActiveBookings = await _context.BookingExtras
            .Include(be => be.Booking)
            .AnyAsync(be => be.ExtraTypePriceId == id &&
                           (be.Booking.Status == BookingStatus.Confirmed || be.Booking.Status == BookingStatus.InProgress));
        return ApiResponse<bool>.Success(!hasActiveBookings);
    }

    public async Task<ApiResponse<ExtraTypePriceReportDto>> GenerateExtraTypePriceReportAsync(ExtraTypePriceReportFilterDto filter) =>
        ApiResponse<ExtraTypePriceReportDto>.Success(new ExtraTypePriceReportDto());

    public async Task<ApiResponse<ExtraTypePricePerformanceReportDto>> GetPerformanceReportAsync(DateTime? startDate = null, DateTime? endDate = null) =>
        ApiResponse<ExtraTypePricePerformanceReportDto>.Success(new ExtraTypePricePerformanceReportDto());

    public async Task<ApiResponse<byte[]>> ExportReportAsync(ExtraTypePriceReportFilterDto filter, string format = "pdf") =>
        ApiResponse<byte[]>.Success(new byte[0]);

    #endregion
}
