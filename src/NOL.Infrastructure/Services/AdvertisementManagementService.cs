using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Application.DTOs.Common;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Domain.Extensions;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Services;

public class AdvertisementManagementService : IAdvertisementManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdvertisementManagementService> _logger;

    public AdvertisementManagementService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AdvertisementManagementService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<ApiResponse<AdminAdvertisementDto>> GetAdvertisementByIdAsync(int id)
    {
        try
        {
            var advertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advertisement == null)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            var advertisementDto = await MapToAdminAdvertisementDto(advertisement);
            return ApiResponse<AdminAdvertisementDto>.Success(advertisementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisement by ID: {Id}", id);
            return ApiResponse<AdminAdvertisementDto>.Error("An error occurred while retrieving advertisement", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsAsync(AdvertisementFilterDto filter)
    {
        try
        {
            // Validate and normalize pagination parameters
            filter.ValidateAndNormalize();

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .AsQueryable();

            // Apply filters
            if (filter.Type.HasValue)
                query = query.Where(a => a.Type == filter.Type.Value);

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(a => a.IsActive == filter.IsActive.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(a => a.IsFeatured == filter.IsFeatured.Value);

            if (filter.CarId.HasValue)
                query = query.Where(a => a.CarId == filter.CarId.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(a => a.CategoryId == filter.CategoryId.Value);

            if (!string.IsNullOrEmpty(filter.CreatedByAdminId))
                query = query.Where(a => a.CreatedByUserId == filter.CreatedByAdminId);

            if (filter.StartDateFrom.HasValue)
                query = query.Where(a => a.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                query = query.Where(a => a.StartDate <= filter.StartDateTo.Value);

            if (filter.EndDateFrom.HasValue)
                query = query.Where(a => a.EndDate >= filter.EndDateFrom.Value);

            if (filter.EndDateTo.HasValue)
                query = query.Where(a => a.EndDate <= filter.EndDateTo.Value);

            if (filter.CreatedDateFrom.HasValue)
                query = query.Where(a => a.CreatedAt >= filter.CreatedDateFrom.Value);

            if (filter.CreatedDateTo.HasValue)
                query = query.Where(a => a.CreatedAt <= filter.CreatedDateTo.Value);

            if (filter.MinDiscountPercentage.HasValue)
                query = query.Where(a => a.DiscountPercentage >= filter.MinDiscountPercentage.Value);

            if (filter.MaxDiscountPercentage.HasValue)
                query = query.Where(a => a.DiscountPercentage <= filter.MaxDiscountPercentage.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(a => a.TitleAr.Contains(filter.SearchTerm) ||
                                        a.TitleEn.Contains(filter.SearchTerm) ||
                                        a.DescriptionAr.Contains(filter.SearchTerm) ||
                                        a.DescriptionEn.Contains(filter.SearchTerm));
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "title" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(a => a.TitleEn) : query.OrderByDescending(a => a.TitleEn),
                "startdate" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(a => a.StartDate) : query.OrderByDescending(a => a.StartDate),
                "enddate" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(a => a.EndDate) : query.OrderByDescending(a => a.EndDate),
                "type" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(a => a.Type) : query.OrderByDescending(a => a.Type),
                "status" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(a => a.Status) : query.OrderByDescending(a => a.Status),
                "viewcount" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(a => a.ViewCount) : query.OrderByDescending(a => a.ViewCount),
                "clickcount" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(a => a.ClickCount) : query.OrderByDescending(a => a.ClickCount),
                "sortorder" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(a => a.SortOrder) : query.OrderByDescending(a => a.SortOrder),
                _ => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(a => a.CreatedAt) : query.OrderByDescending(a => a.CreatedAt)
            };

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                filter.Page,
                filter.PageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements with filter");
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminAdvertisementDto>> CreateAdvertisementAsync(AdminCreateAdvertisementDto createAdvertisementDto, string adminId)
    {
        try
        {
            // Validate admin exists
            var admin = await _userManager.FindByIdAsync(adminId);
            if (admin == null)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Admin not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Validate dates
            if (createAdvertisementDto.StartDate >= createAdvertisementDto.EndDate)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Start date must be before end date", (string?)null, ApiStatusCode.BadRequest);
            }

            // Validate car or category exists if specified
            if (createAdvertisementDto.CarId.HasValue)
            {
                var carExists = await _context.Cars.AnyAsync(c => c.Id == createAdvertisementDto.CarId.Value);
                if (!carExists)
                {
                    return ApiResponse<AdminAdvertisementDto>.Error("Car not found", (string?)null, ApiStatusCode.BadRequest);
                }
            }

            if (createAdvertisementDto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createAdvertisementDto.CategoryId.Value);
                if (!categoryExists)
                {
                    return ApiResponse<AdminAdvertisementDto>.Error("Category not found", (string?)null, ApiStatusCode.BadRequest);
                }
            }

            var advertisement = new Advertisement
            {
                TitleAr = createAdvertisementDto.TitleAr,
                TitleEn = createAdvertisementDto.TitleEn,
                DescriptionAr = createAdvertisementDto.DescriptionAr,
                DescriptionEn = createAdvertisementDto.DescriptionEn,
                Price = createAdvertisementDto.Price,
                DiscountPrice = createAdvertisementDto.DiscountPrice,
                DiscountPercentage = createAdvertisementDto.DiscountPercentage ?? 0,
                StartDate = createAdvertisementDto.StartDate,
                EndDate = createAdvertisementDto.EndDate,
                ImageUrl = createAdvertisementDto.ImageUrl,
                Type = createAdvertisementDto.Type,
                Status = AdvertisementStatus.Active,
                IsFeatured = createAdvertisementDto.IsFeatured,
                SortOrder = createAdvertisementDto.SortOrder,
                IsActive = createAdvertisementDto.IsActive,
                CarId = createAdvertisementDto.CarId,
                CategoryId = createAdvertisementDto.CategoryId,
                CreatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Advertisements.Add(advertisement);
            await _context.SaveChangesAsync();

            // Reload with includes
            advertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .FirstAsync(a => a.Id == advertisement.Id);

            var advertisementDto = await MapToAdminAdvertisementDto(advertisement);
            return ApiResponse<AdminAdvertisementDto>.Success(advertisementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating advertisement");
            return ApiResponse<AdminAdvertisementDto>.Error("An error occurred while creating advertisement", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminAdvertisementDto>> UpdateAdvertisementAsync(int id, AdminUpdateAdvertisementDto updateAdvertisementDto, string adminId)
    {
        try
        {
            var advertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advertisement == null)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(updateAdvertisementDto.TitleAr))
                advertisement.TitleAr = updateAdvertisementDto.TitleAr;

            if (!string.IsNullOrEmpty(updateAdvertisementDto.TitleEn))
                advertisement.TitleEn = updateAdvertisementDto.TitleEn;

            if (!string.IsNullOrEmpty(updateAdvertisementDto.DescriptionAr))
                advertisement.DescriptionAr = updateAdvertisementDto.DescriptionAr;

            if (!string.IsNullOrEmpty(updateAdvertisementDto.DescriptionEn))
                advertisement.DescriptionEn = updateAdvertisementDto.DescriptionEn;

            if (updateAdvertisementDto.Type.HasValue)
                advertisement.Type = updateAdvertisementDto.Type.Value;

            if (updateAdvertisementDto.StartDate.HasValue && updateAdvertisementDto.EndDate.HasValue)
            {
                if (updateAdvertisementDto.StartDate.Value >= updateAdvertisementDto.EndDate.Value)
                {
                    return ApiResponse<AdminAdvertisementDto>.Error("Start date must be before end date", (string?)null, ApiStatusCode.BadRequest);
                }
                advertisement.StartDate = updateAdvertisementDto.StartDate.Value;
                advertisement.EndDate = updateAdvertisementDto.EndDate.Value;
            }
            else if (updateAdvertisementDto.StartDate.HasValue)
            {
                if (updateAdvertisementDto.StartDate.Value >= advertisement.EndDate)
                {
                    return ApiResponse<AdminAdvertisementDto>.Error("Start date must be before end date", (string?)null, ApiStatusCode.BadRequest);
                }
                advertisement.StartDate = updateAdvertisementDto.StartDate.Value;
            }
            else if (updateAdvertisementDto.EndDate.HasValue)
            {
                if (advertisement.StartDate >= updateAdvertisementDto.EndDate.Value)
                {
                    return ApiResponse<AdminAdvertisementDto>.Error("Start date must be before end date", (string?)null, ApiStatusCode.BadRequest);
                }
                advertisement.EndDate = updateAdvertisementDto.EndDate.Value;
            }

            if (updateAdvertisementDto.ImageUrl != null)
                advertisement.ImageUrl = updateAdvertisementDto.ImageUrl;

            if (updateAdvertisementDto.DiscountPercentage.HasValue)
                advertisement.DiscountPercentage = updateAdvertisementDto.DiscountPercentage.Value;

            if (updateAdvertisementDto.DiscountPrice.HasValue)
                advertisement.DiscountPrice = updateAdvertisementDto.DiscountPrice.Value;

            if (updateAdvertisementDto.IsFeatured.HasValue)
                advertisement.IsFeatured = updateAdvertisementDto.IsFeatured.Value;

            if (updateAdvertisementDto.SortOrder.HasValue)
                advertisement.SortOrder = updateAdvertisementDto.SortOrder.Value;

            if (updateAdvertisementDto.IsActive.HasValue)
                advertisement.IsActive = updateAdvertisementDto.IsActive.Value;

            if (updateAdvertisementDto.Status.HasValue)
                advertisement.Status = updateAdvertisementDto.Status.Value;

            if (updateAdvertisementDto.CarId.HasValue)
            {
                var carExists = await _context.Cars.AnyAsync(c => c.Id == updateAdvertisementDto.CarId.Value);
                if (!carExists)
                {
                    return ApiResponse<AdminAdvertisementDto>.Error("Car not found", (string?)null, ApiStatusCode.BadRequest);
                }
                advertisement.CarId = updateAdvertisementDto.CarId.Value;
            }

            if (updateAdvertisementDto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateAdvertisementDto.CategoryId.Value);
                if (!categoryExists)
                {
                    return ApiResponse<AdminAdvertisementDto>.Error("Category not found", (string?)null, ApiStatusCode.BadRequest);
                }
                advertisement.CategoryId = updateAdvertisementDto.CategoryId.Value;
            }

            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var advertisementDto = await MapToAdminAdvertisementDto(advertisement);
            return ApiResponse<AdminAdvertisementDto>.Success(advertisementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating advertisement: {Id}", id);
            return ApiResponse<AdminAdvertisementDto>.Error("An error occurred while updating advertisement", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> DeleteAdvertisementAsync(int id, string adminId)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return ApiResponse.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            _context.Advertisements.Remove(advertisement);
            await _context.SaveChangesAsync();

            return ApiResponse.Success("Advertisement deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting advertisement: {Id}", id);
            return ApiResponse.Error("An error occurred while deleting advertisement", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Status Management

    public async Task<ApiResponse<AdminAdvertisementDto>> UpdateAdvertisementStatusAsync(int id, AdvertisementStatus status, string adminId)
    {
        try
        {
            var advertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advertisement == null)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            advertisement.Status = status;
            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var advertisementDto = await MapToAdminAdvertisementDto(advertisement);
            return ApiResponse<AdminAdvertisementDto>.Success(advertisementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating advertisement status: {Id}", id);
            return ApiResponse<AdminAdvertisementDto>.Error("An error occurred while updating advertisement status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkUpdateAdvertisementStatusAsync(List<int> advertisementIds, AdvertisementStatus status, string adminId)
    {
        try
        {
            var advertisements = await _context.Advertisements
                .Where(a => advertisementIds.Contains(a.Id))
                .ToListAsync();

            foreach (var advertisement in advertisements)
            {
                advertisement.Status = status;
                advertisement.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Updated status for {advertisements.Count} advertisements");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating advertisement status");
            return ApiResponse.Error("An error occurred while updating advertisement status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Scheduling

    public async Task<ApiResponse<AdminAdvertisementDto>> ScheduleAdvertisementAsync(int id, DateTime startDate, DateTime endDate, string adminId)
    {
        try
        {
            var advertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advertisement == null)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            if (startDate >= endDate)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Start date must be before end date", (string?)null, ApiStatusCode.BadRequest);
            }

            advertisement.StartDate = startDate;
            advertisement.EndDate = endDate;
            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var advertisementDto = await MapToAdminAdvertisementDto(advertisement);
            return ApiResponse<AdminAdvertisementDto>.Success(advertisementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling advertisement: {Id}", id);
            return ApiResponse<AdminAdvertisementDto>.Error("An error occurred while scheduling advertisement", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkScheduleAdvertisementsAsync(AdvertisementScheduleDto scheduleDto, string adminId)
    {
        try
        {
            if (scheduleDto.StartDate >= scheduleDto.EndDate)
            {
                return ApiResponse.Error("Start date must be before end date", (string?)null, ApiStatusCode.BadRequest);
            }

            var advertisements = await _context.Advertisements
                .Where(a => scheduleDto.AdvertisementIds.Contains(a.Id))
                .ToListAsync();

            foreach (var advertisement in advertisements)
            {
                advertisement.StartDate = scheduleDto.StartDate;
                advertisement.EndDate = scheduleDto.EndDate;
                advertisement.UpdatedAt = DateTime.UtcNow;

                if (scheduleDto.AutoActivate && DateTime.UtcNow >= scheduleDto.StartDate)
                {
                    advertisement.Status = AdvertisementStatus.Active;
                    advertisement.IsActive = true;
                }
            }

            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Scheduled {advertisements.Count} advertisements");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk scheduling advertisements");
            return ApiResponse.Error("An error occurred while scheduling advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetScheduledAdvertisementsAsync(DateTime? date = null, int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var targetDate = date ?? DateTime.UtcNow.Date;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.StartDate.Date <= targetDate && a.EndDate.Date >= targetDate)
                .OrderBy(a => a.SortOrder)
                .ThenByDescending(a => a.CreatedAt);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scheduled advertisements");
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving scheduled advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetExpiredAdvertisementsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.EndDate < DateTime.UtcNow && a.Status == AdvertisementStatus.Active)
                .OrderByDescending(a => a.EndDate);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expired advertisements");
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving expired advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Featured Content Management

    public async Task<ApiResponse<AdminAdvertisementDto>> SetAdvertisementFeaturedAsync(int id, bool isFeatured, string adminId)
    {
        try
        {
            var advertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advertisement == null)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            advertisement.IsFeatured = isFeatured;
            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var advertisementDto = await MapToAdminAdvertisementDto(advertisement);
            return ApiResponse<AdminAdvertisementDto>.Success(advertisementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting advertisement featured status: {Id}", id);
            return ApiResponse<AdminAdvertisementDto>.Error("An error occurred while updating featured status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetFeaturedAdvertisementsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.IsFeatured && a.IsActive && a.Status == AdvertisementStatus.Active)
                .OrderBy(a => a.SortOrder)
                .ThenByDescending(a => a.CreatedAt);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured advertisements");
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving featured advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> UpdateAdvertisementSortOrderAsync(int id, int sortOrder, string adminId)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return ApiResponse.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            advertisement.SortOrder = sortOrder;
            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.Success("Sort order updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating advertisement sort order: {Id}", id);
            return ApiResponse.Error("An error occurred while updating sort order", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Discount Management

    public async Task<ApiResponse<AdminAdvertisementDto>> UpdateAdvertisementDiscountAsync(int id, decimal? discountPercentage, decimal? discountPrice, string adminId)
    {
        try
        {
            var advertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (advertisement == null)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            if (discountPercentage.HasValue)
                advertisement.DiscountPercentage = discountPercentage.Value;

            if (discountPrice.HasValue)
                advertisement.DiscountPrice = discountPrice.Value;

            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var advertisementDto = await MapToAdminAdvertisementDto(advertisement);
            return ApiResponse<AdminAdvertisementDto>.Success(advertisementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating advertisement discount: {Id}", id);
            return ApiResponse<AdminAdvertisementDto>.Error("An error occurred while updating advertisement discount", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsWithDiscountsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.DiscountPercentage > 0 || a.DiscountPrice.HasValue)
                .OrderByDescending(a => a.DiscountPercentage)
                .ThenByDescending(a => a.DiscountPrice);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements with discounts");
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements with discounts", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<decimal>> CalculateDiscountedPriceAsync(int advertisementId, decimal originalPrice)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(advertisementId);
            if (advertisement == null)
            {
                return ApiResponse<decimal>.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            decimal discountedPrice = originalPrice;

            // Apply discount price if available
            if (advertisement.DiscountPrice.HasValue)
            {
                discountedPrice = advertisement.DiscountPrice.Value;
            }
            // Otherwise apply percentage discount
            else if (advertisement.DiscountPercentage > 0)
            {
                discountedPrice = originalPrice - (originalPrice * advertisement.DiscountPercentage / 100);
            }

            return ApiResponse<decimal>.Success(discountedPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating discounted price for advertisement: {Id}", advertisementId);
            return ApiResponse<decimal>.Error("An error occurred while calculating discounted price", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Performance Analytics

    public async Task<ApiResponse<AdvertisementAnalyticsDto>> GetAdvertisementAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Advertisements.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var advertisements = await query.ToListAsync();

            var analytics = new AdvertisementAnalyticsDto
            {
                TotalAdvertisements = advertisements.Count,
                ActiveAdvertisements = advertisements.Count(a => a.IsActive && a.Status == AdvertisementStatus.Active),
                ExpiredAdvertisements = advertisements.Count(a => a.EndDate < DateTime.UtcNow),
                FeaturedAdvertisements = advertisements.Count(a => a.IsFeatured),
                TotalViews = advertisements.Sum(a => a.ViewCount),
                TotalClicks = advertisements.Sum(a => a.ClickCount),
                AverageClickThroughRate = advertisements.Any() && advertisements.Sum(a => a.ViewCount) > 0
                    ? (double)advertisements.Sum(a => a.ClickCount) / advertisements.Sum(a => a.ViewCount) * 100
                    : 0,
                TotalConversions = 0, // Placeholder - would need separate tracking
                AverageConversionRate = 0, // Placeholder - would need separate tracking
                TotalRevenueGenerated = 0, // Placeholder - would need separate tracking
                TypeStats = GenerateAdvertisementTypeStats(advertisements),
                TopPerformingAds = GenerateTopPerformingAds(advertisements, 5),
                LowPerformingAds = GenerateLowPerformingAds(advertisements, 5),
                MonthlyStats = GenerateMonthlyAdvertisementStats(advertisements)
            };

            return ApiResponse<AdvertisementAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisement analytics");
            return ApiResponse<AdvertisementAnalyticsDto>.Error("An error occurred while retrieving advertisement analytics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdvertisementPerformanceDto>>> GetTopPerformingAdvertisementsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Advertisements.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var advertisements = await query.ToListAsync();
            var topPerforming = GenerateTopPerformingAds(advertisements, count);

            return ApiResponse<List<AdvertisementPerformanceDto>>.Success(topPerforming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top performing advertisements");
            return ApiResponse<List<AdvertisementPerformanceDto>>.Error("An error occurred while retrieving top performing advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdvertisementPerformanceDto>>> GetLowPerformingAdvertisementsAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Advertisements.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var advertisements = await query.ToListAsync();
            var lowPerforming = GenerateLowPerformingAds(advertisements, count);

            return ApiResponse<List<AdvertisementPerformanceDto>>.Success(lowPerforming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low performing advertisements");
            return ApiResponse<List<AdvertisementPerformanceDto>>.Error("An error occurred while retrieving low performing advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdvertisementMetricDto>>> GetAdvertisementMetricsAsync(int id, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return ApiResponse<List<AdvertisementMetricDto>>.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            // For now, return basic metrics since we don't have detailed daily tracking
            var metrics = new List<AdvertisementMetricDto>
            {
                new AdvertisementMetricDto
                {
                    Date = DateTime.UtcNow.Date,
                    ViewCount = advertisement.ViewCount,
                    ClickCount = advertisement.ClickCount,
                    ClickThroughRate = advertisement.ViewCount > 0 ? (double)advertisement.ClickCount / advertisement.ViewCount * 100 : 0,
                    ConversionCount = 0, // Placeholder
                    ConversionRate = 0, // Placeholder
                    RevenueGenerated = 0 // Placeholder
                }
            };

            return ApiResponse<List<AdvertisementMetricDto>>.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisement metrics: {Id}", id);
            return ApiResponse<List<AdvertisementMetricDto>>.Error("An error occurred while retrieving advertisement metrics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Metrics Tracking

    public async Task<ApiResponse> RecordAdvertisementViewAsync(int id, string? userId = null, string? ipAddress = null)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return ApiResponse.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            advertisement.ViewCount++;
            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.Success("View recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording advertisement view: {Id}", id);
            return ApiResponse.Error("An error occurred while recording view", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> RecordAdvertisementClickAsync(int id, string? userId = null, string? ipAddress = null)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return ApiResponse.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            advertisement.ClickCount++;
            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.Success("Click recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording advertisement click: {Id}", id);
            return ApiResponse.Error("An error occurred while recording click", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> RecordAdvertisementConversionAsync(int id, string? userId = null, decimal? conversionValue = null)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return ApiResponse.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            // For now, just update the advertisement record
            // In a real implementation, you would store this in a separate conversions table
            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.Success("Conversion recorded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording advertisement conversion: {Id}", id);
            return ApiResponse.Error("An error occurred while recording conversion", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Type-based Analytics

    public async Task<ApiResponse<List<AdvertisementTypeStatsDto>>> GetAdvertisementTypeStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Advertisements.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var advertisements = await query.ToListAsync();
            var typeStats = GenerateAdvertisementTypeStats(advertisements);

            return ApiResponse<List<AdvertisementTypeStatsDto>>.Success(typeStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisement type stats");
            return ApiResponse<List<AdvertisementTypeStatsDto>>.Error("An error occurred while retrieving advertisement type stats", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminAdvertisementDto>>> GetAdvertisementsByTypeAsync(AdvertisementType type, bool activeOnly = true)
    {
        try
        {
            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.Type == type);

            if (activeOnly)
            {
                query = query.Where(a => a.IsActive && a.Status == AdvertisementStatus.Active);
            }

            var advertisements = await query
                .OrderBy(a => a.SortOrder)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            return ApiResponse<List<AdminAdvertisementDto>>.Success(advertisementDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements by type: {Type}", type);
            return ApiResponse<List<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements by type", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Bulk Operations

    public async Task<ApiResponse> BulkOperationAsync(BulkAdvertisementOperationDto operationDto, string adminId)
    {
        try
        {
            switch (operationDto.Operation.ToLower())
            {
                case "activate":
                    return await BulkActivateAdvertisementsAsync(operationDto.AdvertisementIds, adminId);

                case "deactivate":
                    return await BulkDeactivateAdvertisementsAsync(operationDto.AdvertisementIds, adminId);

                case "delete":
                    return await BulkDeleteAdvertisementsAsync(operationDto.AdvertisementIds, adminId);

                case "updatestatus":
                    if (operationDto.NewStatus.HasValue)
                    {
                        return await BulkUpdateAdvertisementStatusAsync(operationDto.AdvertisementIds, operationDto.NewStatus.Value, adminId);
                    }
                    return ApiResponse.Error("Status is required for update status operation", (string?)null, ApiStatusCode.BadRequest);

                case "feature":
                    return await BulkSetFeaturedAsync(operationDto.AdvertisementIds, true, adminId);

                case "unfeature":
                    return await BulkSetFeaturedAsync(operationDto.AdvertisementIds, false, adminId);

                default:
                    return ApiResponse.Error("Invalid operation", (string?)null, ApiStatusCode.BadRequest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation: {Operation}", operationDto.Operation);
            return ApiResponse.Error("An error occurred while performing bulk operation", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkDeleteAdvertisementsAsync(List<int> advertisementIds, string adminId)
    {
        try
        {
            var advertisements = await _context.Advertisements
                .Where(a => advertisementIds.Contains(a.Id))
                .ToListAsync();

            _context.Advertisements.RemoveRange(advertisements);
            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Deleted {advertisements.Count} advertisements");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting advertisements");
            return ApiResponse.Error("An error occurred while deleting advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkActivateAdvertisementsAsync(List<int> advertisementIds, string adminId)
    {
        try
        {
            var advertisements = await _context.Advertisements
                .Where(a => advertisementIds.Contains(a.Id))
                .ToListAsync();

            foreach (var advertisement in advertisements)
            {
                advertisement.IsActive = true;
                advertisement.Status = AdvertisementStatus.Active;
                advertisement.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Activated {advertisements.Count} advertisements");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk activating advertisements");
            return ApiResponse.Error("An error occurred while activating advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkDeactivateAdvertisementsAsync(List<int> advertisementIds, string adminId)
    {
        try
        {
            var advertisements = await _context.Advertisements
                .Where(a => advertisementIds.Contains(a.Id))
                .ToListAsync();

            foreach (var advertisement in advertisements)
            {
                advertisement.IsActive = false;
                advertisement.Status = AdvertisementStatus.Paused;
                advertisement.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Deactivated {advertisements.Count} advertisements");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deactivating advertisements");
            return ApiResponse.Error("An error occurred while deactivating advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    private async Task<ApiResponse> BulkSetFeaturedAsync(List<int> advertisementIds, bool isFeatured, string adminId)
    {
        try
        {
            var advertisements = await _context.Advertisements
                .Where(a => advertisementIds.Contains(a.Id))
                .ToListAsync();

            foreach (var advertisement in advertisements)
            {
                advertisement.IsFeatured = isFeatured;
                advertisement.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var action = isFeatured ? "Featured" : "Unfeatured";
            return ApiResponse.Success($"{action} {advertisements.Count} advertisements");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk setting featured status");
            return ApiResponse.Error("An error occurred while updating featured status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Copy and Template Operations

    public async Task<ApiResponse<AdminAdvertisementDto>> CopyAdvertisementAsync(CopyAdvertisementDto copyDto, string adminId)
    {
        try
        {
            var sourceAdvertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == copyDto.SourceAdvertisementId);

            if (sourceAdvertisement == null)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Source advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            var newAdvertisement = new Advertisement
            {
                TitleAr = copyDto.NewTitleAr ?? sourceAdvertisement.TitleAr + " (Copy)",
                TitleEn = copyDto.NewTitleEn ?? sourceAdvertisement.TitleEn + " (Copy)",
                DescriptionAr = sourceAdvertisement.DescriptionAr,
                DescriptionEn = sourceAdvertisement.DescriptionEn,
                Price = sourceAdvertisement.Price,
                DiscountPrice = sourceAdvertisement.DiscountPrice,
                DiscountPercentage = sourceAdvertisement.DiscountPercentage,
                StartDate = copyDto.NewStartDate ?? DateTime.UtcNow,
                EndDate = copyDto.NewEndDate ?? DateTime.UtcNow.AddDays(30),
                ImageUrl = sourceAdvertisement.ImageUrl,
                Type = sourceAdvertisement.Type,
                Status = AdvertisementStatus.Draft,
                IsFeatured = false,
                SortOrder = sourceAdvertisement.SortOrder,
                IsActive = false,
                CarId = copyDto.NewCarId ?? sourceAdvertisement.CarId,
                CategoryId = copyDto.NewCategoryId ?? sourceAdvertisement.CategoryId,
                CreatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ViewCount = copyDto.CopyMetrics ? sourceAdvertisement.ViewCount : 0,
                ClickCount = copyDto.CopyMetrics ? sourceAdvertisement.ClickCount : 0
            };

            _context.Advertisements.Add(newAdvertisement);
            await _context.SaveChangesAsync();

            // Reload with includes
            newAdvertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .FirstAsync(a => a.Id == newAdvertisement.Id);

            var advertisementDto = await MapToAdminAdvertisementDto(newAdvertisement);
            return ApiResponse<AdminAdvertisementDto>.Success(advertisementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying advertisement: {SourceId}", copyDto.SourceAdvertisementId);
            return ApiResponse<AdminAdvertisementDto>.Error("An error occurred while copying advertisement", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminAdvertisementDto>>> GetAdvertisementTemplatesAsync()
    {
        try
        {
            // For now, return featured advertisements as templates
            var templates = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.IsFeatured && a.Status == AdvertisementStatus.Active)
                .OrderBy(a => a.SortOrder)
                .Take(10)
                .ToListAsync();

            var templateDtos = new List<AdminAdvertisementDto>();
            foreach (var template in templates)
            {
                templateDtos.Add(await MapToAdminAdvertisementDto(template));
            }

            return ApiResponse<List<AdminAdvertisementDto>>.Success(templateDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisement templates");
            return ApiResponse<List<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisement templates", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminAdvertisementDto>> CreateAdvertisementFromTemplateAsync(int templateId, AdminCreateAdvertisementDto createDto, string adminId)
    {
        try
        {
            var template = await _context.Advertisements.FindAsync(templateId);
            if (template == null)
            {
                return ApiResponse<AdminAdvertisementDto>.Error("Template not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Use template as base but override with provided data
            var advertisement = new Advertisement
            {
                TitleAr = createDto.TitleAr,
                TitleEn = createDto.TitleEn,
                DescriptionAr = createDto.DescriptionAr,
                DescriptionEn = createDto.DescriptionEn,
                Price = template.Price,
                DiscountPrice = createDto.DiscountPrice ?? template.DiscountPrice,
                DiscountPercentage = createDto.DiscountPercentage ?? template.DiscountPercentage,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                ImageUrl = createDto.ImageUrl ?? template.ImageUrl,
                Type = createDto.Type,
                Status = AdvertisementStatus.Draft,
                IsFeatured = createDto.IsFeatured,
                SortOrder = createDto.SortOrder,
                IsActive = createDto.IsActive,
                CarId = createDto.CarId ?? template.CarId,
                CategoryId = createDto.CategoryId ?? template.CategoryId,
                CreatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Advertisements.Add(advertisement);
            await _context.SaveChangesAsync();

            // Reload with includes
            advertisement = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .FirstAsync(a => a.Id == advertisement.Id);

            var advertisementDto = await MapToAdminAdvertisementDto(advertisement);
            return ApiResponse<AdminAdvertisementDto>.Success(advertisementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating advertisement from template: {TemplateId}", templateId);
            return ApiResponse<AdminAdvertisementDto>.Error("An error occurred while creating advertisement from template", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Car and Category Association

    public async Task<ApiResponse<List<AdminAdvertisementDto>>> GetAdvertisementsByCarAsync(int carId)
    {
        try
        {
            var advertisements = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.CarId == carId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            return ApiResponse<List<AdminAdvertisementDto>>.Success(advertisementDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements by car: {CarId}", carId);
            return ApiResponse<List<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements by car", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminAdvertisementDto>>> GetAdvertisementsByCategoryAsync(int categoryId)
    {
        try
        {
            var advertisements = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.CategoryId == categoryId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            return ApiResponse<List<AdminAdvertisementDto>>.Success(advertisementDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements by category: {CategoryId}", categoryId);
            return ApiResponse<List<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements by category", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> AssociateAdvertisementWithCarAsync(int advertisementId, int carId, string adminId)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(advertisementId);
            if (advertisement == null)
            {
                return ApiResponse.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            var carExists = await _context.Cars.AnyAsync(c => c.Id == carId);
            if (!carExists)
            {
                return ApiResponse.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            advertisement.CarId = carId;
            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.Success("Advertisement associated with car successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating advertisement with car: {AdvertisementId}, {CarId}", advertisementId, carId);
            return ApiResponse.Error("An error occurred while associating advertisement with car", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> AssociateAdvertisementWithCategoryAsync(int advertisementId, int categoryId, string adminId)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(advertisementId);
            if (advertisement == null)
            {
                return ApiResponse.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
            if (!categoryExists)
            {
                return ApiResponse.Error("Category not found", (string?)null, ApiStatusCode.NotFound);
            }

            advertisement.CategoryId = categoryId;
            advertisement.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse.Success("Advertisement associated with category successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error associating advertisement with category: {AdvertisementId}, {CategoryId}", advertisementId, categoryId);
            return ApiResponse.Error("An error occurred while associating advertisement with category", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Revenue Impact Analysis

    public async Task<ApiResponse<decimal>> GetAdvertisementRevenueImpactAsync(int id, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            if (advertisement == null)
            {
                return ApiResponse<decimal>.Error("Advertisement not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Placeholder calculation - in a real implementation, you would track actual revenue from bookings
            // that came through this advertisement
            decimal estimatedRevenue = advertisement.ClickCount * 50; // Assume $50 per click conversion

            return ApiResponse<decimal>.Success(estimatedRevenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisement revenue impact: {Id}", id);
            return ApiResponse<decimal>.Error("An error occurred while calculating revenue impact", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<MonthlyAdvertisementStatsDto>>> GetMonthlyAdvertisementStatsAsync(int year)
    {
        try
        {
            var advertisements = await _context.Advertisements
                .Where(a => a.CreatedAt.Year == year)
                .ToListAsync();

            var monthlyStats = advertisements
                .GroupBy(a => a.CreatedAt.Month)
                .Select(g => new MonthlyAdvertisementStatsDto
                {
                    Year = year,
                    Month = g.Key,
                    MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                    AdvertisementCount = g.Count(),
                    TotalViews = g.Sum(a => a.ViewCount),
                    TotalClicks = g.Sum(a => a.ClickCount),
                    ClickThroughRate = g.Sum(a => a.ViewCount) > 0 ? (double)g.Sum(a => a.ClickCount) / g.Sum(a => a.ViewCount) * 100 : 0,
                    TotalConversions = 0, // Placeholder
                    ConversionRate = 0, // Placeholder
                    RevenueGenerated = g.Sum(a => a.ClickCount) * 50 // Placeholder calculation
                })
                .OrderBy(m => m.Month)
                .ToList();

            return ApiResponse<List<MonthlyAdvertisementStatsDto>>.Success(monthlyStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly advertisement stats for year: {Year}", year);
            return ApiResponse<List<MonthlyAdvertisementStatsDto>>.Error("An error occurred while retrieving monthly stats", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<decimal>> GetTotalAdvertisementRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Advertisements.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.CreatedAt <= endDate.Value);

            var advertisements = await query.ToListAsync();

            // Placeholder calculation
            decimal totalRevenue = advertisements.Sum(a => a.ClickCount) * 50;

            return ApiResponse<decimal>.Success(totalRevenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total advertisement revenue");
            return ApiResponse<decimal>.Error("An error occurred while calculating total revenue", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Reports and Export

    public async Task<ApiResponse<AdvertisementReportDto>> GenerateAdvertisementReportAsync(AdvertisementFilterDto filter)
    {
        try
        {
            var advertisementsResponse = await GetAdvertisementsAsync(filter);
            if (!advertisementsResponse.Succeeded)
            {
                return ApiResponse<AdvertisementReportDto>.Error("Failed to generate advertisement report", (string?)null, ApiStatusCode.InternalServerError);
            }

            var analyticsResponse = await GetAdvertisementAnalyticsAsync(filter.CreatedDateFrom, filter.CreatedDateTo);
            if (!analyticsResponse.Succeeded)
            {
                return ApiResponse<AdvertisementReportDto>.Error("Failed to generate advertisement analytics", (string?)null, ApiStatusCode.InternalServerError);
            }

            var advertisements = advertisementsResponse.Data?.Data ?? new List<AdminAdvertisementDto>();
            var analytics = analyticsResponse.Data ?? new AdvertisementAnalyticsDto();

            var report = new AdvertisementReportDto
            {
                GeneratedAt = DateTime.UtcNow,
                StartDate = filter.CreatedDateFrom,
                EndDate = filter.CreatedDateTo,
                Analytics = analytics,
                Advertisements = advertisements,
                Summary = new AdvertisementReportSummaryDto
                {
                    TotalAdvertisements = advertisements.Count,
                    ActiveAdvertisements = advertisements.Count(a => a.Status == AdvertisementStatus.Active),
                    ExpiredAdvertisements = advertisements.Count(a => a.EndDate < DateTime.UtcNow),
                    FeaturedAdvertisements = advertisements.Count(a => a.IsFeatured),
                    TotalViews = advertisements.Sum(a => a.ViewCount),
                    TotalClicks = advertisements.Sum(a => a.ClickCount),
                    OverallClickThroughRate = advertisements.Sum(a => a.ViewCount) > 0
                        ? (double)advertisements.Sum(a => a.ClickCount) / advertisements.Sum(a => a.ViewCount) * 100
                        : 0,
                    TotalConversions = advertisements.Sum(a => a.ConversionCount),
                    OverallConversionRate = advertisements.Sum(a => a.ClickCount) > 0
                        ? (double)advertisements.Sum(a => a.ConversionCount) / advertisements.Sum(a => a.ClickCount) * 100
                        : 0,
                    TotalRevenueGenerated = advertisements.Sum(a => a.RevenueGenerated),
                    AverageRevenuePerAd = advertisements.Any() ? advertisements.Average(a => a.RevenueGenerated) : 0,
                    AveragePerformanceScore = advertisements.Any() ? advertisements.Average(a => a.PerformanceScore) : 0,
                    BestPerformingAdType = GetBestPerformingAdType(advertisements),
                    MostPopularDiscountType = GetMostPopularDiscountType(advertisements)
                }
            };

            return ApiResponse<AdvertisementReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating advertisement report");
            return ApiResponse<AdvertisementReportDto>.Error("An error occurred while generating advertisement report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportAdvertisementReportAsync(AdvertisementFilterDto filter, string format = "excel")
    {
        try
        {
            var reportResponse = await GenerateAdvertisementReportAsync(filter);
            if (!reportResponse.Succeeded)
            {
                return ApiResponse<byte[]>.Error("Failed to generate report for export", (string?)null, ApiStatusCode.InternalServerError);
            }

            // Placeholder for actual export implementation
            // In a real implementation, you would use libraries like EPPlus for Excel or iTextSharp for PDF
            var dummyData = System.Text.Encoding.UTF8.GetBytes("Advertisement Report Export - Implementation needed");

            return ApiResponse<byte[]>.Success(dummyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting advertisement report");
            return ApiResponse<byte[]>.Error("An error occurred while exporting advertisement report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportAdvertisementMetricsAsync(int id, DateTime? startDate = null, DateTime? endDate = null, string format = "excel")
    {
        try
        {
            var metricsResponse = await GetAdvertisementMetricsAsync(id, startDate, endDate);
            if (!metricsResponse.Succeeded)
            {
                return ApiResponse<byte[]>.Error("Failed to get metrics for export", (string?)null, ApiStatusCode.InternalServerError);
            }

            // Placeholder for actual export implementation
            var dummyData = System.Text.Encoding.UTF8.GetBytes($"Advertisement Metrics Export - {metricsResponse.Data?.Count} metrics");

            return ApiResponse<byte[]>.Success(dummyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting advertisement metrics");
            return ApiResponse<byte[]>.Error("An error occurred while exporting advertisement metrics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Validation

    public async Task<ApiResponse<bool>> ValidateAdvertisementDatesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return ApiResponse<bool>.Success(false);
            }

            if (startDate < DateTime.UtcNow.Date)
            {
                return ApiResponse<bool>.Success(false);
            }

            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating advertisement dates");
            return ApiResponse<bool>.Error("An error occurred while validating dates", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<bool>> ValidateAdvertisementDiscountAsync(decimal? discountPercentage, decimal? discountPrice)
    {
        try
        {
            if (discountPercentage.HasValue && (discountPercentage.Value < 0 || discountPercentage.Value > 100))
            {
                return ApiResponse<bool>.Success(false);
            }

            if (discountPrice.HasValue && discountPrice.Value < 0)
            {
                return ApiResponse<bool>.Success(false);
            }

            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating advertisement discount");
            return ApiResponse<bool>.Error("An error occurred while validating discount", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<string>>> ValidateAdvertisementDataAsync(AdminCreateAdvertisementDto createDto)
    {
        try
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(createDto.TitleAr))
                errors.Add("Arabic title is required");

            if (string.IsNullOrEmpty(createDto.TitleEn))
                errors.Add("English title is required");

            if (string.IsNullOrEmpty(createDto.DescriptionAr))
                errors.Add("Arabic description is required");

            if (string.IsNullOrEmpty(createDto.DescriptionEn))
                errors.Add("English description is required");

            if (createDto.StartDate >= createDto.EndDate)
                errors.Add("Start date must be before end date");

            if (createDto.StartDate < DateTime.UtcNow.Date)
                errors.Add("Start date cannot be in the past");

            if (createDto.DiscountPercentage.HasValue && (createDto.DiscountPercentage.Value < 0 || createDto.DiscountPercentage.Value > 100))
                errors.Add("Discount percentage must be between 0 and 100");

            if (createDto.DiscountPrice.HasValue && createDto.DiscountPrice.Value < 0)
                errors.Add("Discount price cannot be negative");

            if (createDto.CarId.HasValue)
            {
                var carExists = await _context.Cars.AnyAsync(c => c.Id == createDto.CarId.Value);
                if (!carExists)
                    errors.Add("Selected car does not exist");
            }

            if (createDto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createDto.CategoryId.Value);
                if (!categoryExists)
                    errors.Add("Selected category does not exist");
            }

            return ApiResponse<List<string>>.Success(errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating advertisement data");
            return ApiResponse<List<string>>.Error("An error occurred while validating advertisement data", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Search and Filter

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> SearchAdvertisementsAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.TitleAr.Contains(searchTerm) ||
                           a.TitleEn.Contains(searchTerm) ||
                           a.DescriptionAr.Contains(searchTerm) ||
                           a.DescriptionEn.Contains(searchTerm));

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching advertisements: {SearchTerm}", searchTerm);
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while searching advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminAdvertisementDto>>> GetActiveAdvertisementsAsync(DateTime? date = null)
    {
        try
        {
            var targetDate = date ?? DateTime.UtcNow;

            var advertisements = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.IsActive &&
                           a.Status == AdvertisementStatus.Active &&
                           a.StartDate <= targetDate &&
                           a.EndDate >= targetDate)
                .OrderBy(a => a.SortOrder)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            return ApiResponse<List<AdminAdvertisementDto>>.Success(advertisementDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active advertisements");
            return ApiResponse<List<AdminAdvertisementDto>>.Error("An error occurred while retrieving active advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminAdvertisementDto>>> GetAdvertisementsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var advertisements = await _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.StartDate >= startDate && a.EndDate <= endDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            return ApiResponse<List<AdminAdvertisementDto>>.Success(advertisementDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements by date range");
            return ApiResponse<List<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements by date range", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<AdminAdvertisementDto> MapToAdminAdvertisementDto(Advertisement advertisement)
    {
        var clickThroughRate = advertisement.ViewCount > 0 ? (double)advertisement.ClickCount / advertisement.ViewCount * 100 : 0;
        var performanceScore = CalculatePerformanceScore(advertisement);

        return new AdminAdvertisementDto
        {
            Id = advertisement.Id,
            Title = advertisement.TitleEn,
            Description = advertisement.DescriptionEn,
            Price = advertisement.Price,
            DiscountPrice = advertisement.DiscountPrice,
            DiscountPercentage = advertisement.DiscountPercentage,
            StartDate = advertisement.StartDate,
            EndDate = advertisement.EndDate,
            ImageUrl = advertisement.ImageUrl,
            Type = advertisement.Type,
            Status = advertisement.Status,
            ViewCount = advertisement.ViewCount,
            ClickCount = advertisement.ClickCount,
            IsFeatured = advertisement.IsFeatured,
            SortOrder = advertisement.SortOrder,
            CreatedAt = advertisement.CreatedAt,
            UpdatedAt = advertisement.UpdatedAt,
            CreatedByAdminName = advertisement.CreatedByUser?.FullName ?? "Unknown",
            ClickThroughRate = clickThroughRate,
            ConversionCount = 0, // Placeholder - would need separate tracking
            ConversionRate = 0, // Placeholder - would need separate tracking
            RevenueGenerated = 0, // Placeholder - would need separate tracking
            PerformanceScore = performanceScore,
            Car = advertisement.Car != null ? new NOL.Application.DTOs.CarDto
            {
                Id = advertisement.Car.Id,
                Brand = advertisement.Car.BrandEn,
                Model = advertisement.Car.ModelEn,
                Year = advertisement.Car.Year,
                Color = advertisement.Car.ColorEn,
                SeatingCapacity = advertisement.Car.SeatingCapacity,
                TransmissionType = advertisement.Car.TransmissionType.ToString(),
                FuelType = advertisement.Car.FuelType,
                DailyPrice = advertisement.Car.DailyRate,
                WeeklyPrice = advertisement.Car.WeeklyRate,
                MonthlyPrice = advertisement.Car.MonthlyRate,
                Status = advertisement.Car.Status.GetDescription(),
                ImageUrl = advertisement.Car.ImageUrl,
                Description = advertisement.Car.DescriptionEn,
                Mileage = advertisement.Car.Mileage,
                Category = new NOL.Application.DTOs.CategoryDto
                {
                    Id = advertisement.Car.Category?.Id ?? 0,
                    Name = advertisement.Car.Category?.NameEn ?? "",
                    Description = advertisement.Car.Category?.DescriptionEn ?? "",
                    ImageUrl = advertisement.Car.Category?.ImageUrl,
                    SortOrder = advertisement.Car.Category?.SortOrder ?? 0
                },
                Branch = new NOL.Application.DTOs.BranchDto
                {
                    Id = advertisement.Car.Branch?.Id ?? 0,
                    Name = advertisement.Car.Branch?.NameEn ?? "",
                    Description = advertisement.Car.Branch?.DescriptionEn ?? "",
                    Address = advertisement.Car.Branch?.Address ?? "",
                    City = advertisement.Car.Branch?.City ?? "",
                    Country = advertisement.Car.Branch?.Country ?? "",
                    Phone = advertisement.Car.Branch?.Phone ?? "",
                    Email = advertisement.Car.Branch?.Email ?? "",
                    Latitude = advertisement.Car.Branch?.Latitude ?? 0,
                    Longitude = advertisement.Car.Branch?.Longitude ?? 0,
                    WorkingHours = advertisement.Car.Branch?.WorkingHours
                }
            } : null,
            Category = advertisement.Category != null ? new NOL.Application.DTOs.CategoryDto
            {
                Id = advertisement.Category.Id,
                Name = advertisement.Category.NameEn,
                Description = advertisement.Category.DescriptionEn ?? "",
                ImageUrl = advertisement.Category.ImageUrl,
                SortOrder = advertisement.Category.SortOrder
            } : null,
            DailyMetrics = new List<AdvertisementMetricDto>() // Placeholder - would need separate tracking
        };
    }

    private double CalculatePerformanceScore(Advertisement advertisement)
    {
        // Simple performance score calculation based on CTR and engagement
        var ctr = advertisement.ViewCount > 0 ? (double)advertisement.ClickCount / advertisement.ViewCount : 0;
        var engagementScore = Math.Min(advertisement.ViewCount / 100.0, 10); // Max 10 points for views
        var clickScore = Math.Min(advertisement.ClickCount / 10.0, 10); // Max 10 points for clicks
        var ctrScore = ctr * 100; // CTR as percentage

        return Math.Round((engagementScore + clickScore + ctrScore) / 3, 2);
    }

    private List<AdvertisementTypeStatsDto> GenerateAdvertisementTypeStats(List<Advertisement> advertisements)
    {
        return advertisements
            .GroupBy(a => a.Type)
            .Select(g => new AdvertisementTypeStatsDto
            {
                Type = g.Key,
                TypeName = g.Key.ToString(),
                Count = g.Count(),
                Percentage = advertisements.Any() ? (double)g.Count() / advertisements.Count * 100 : 0,
                TotalViews = g.Sum(a => a.ViewCount),
                TotalClicks = g.Sum(a => a.ClickCount),
                ClickThroughRate = g.Sum(a => a.ViewCount) > 0 ? (double)g.Sum(a => a.ClickCount) / g.Sum(a => a.ViewCount) * 100 : 0,
                RevenueGenerated = g.Sum(a => a.ClickCount) * 50 // Placeholder calculation
            })
            .ToList();
    }

    private List<AdvertisementPerformanceDto> GenerateTopPerformingAds(List<Advertisement> advertisements, int count)
    {
        return advertisements
            .Select(a => new AdvertisementPerformanceDto
            {
                AdvertisementId = a.Id,
                Title = a.TitleEn,
                Type = a.Type,
                ViewCount = a.ViewCount,
                ClickCount = a.ClickCount,
                ClickThroughRate = a.ViewCount > 0 ? (double)a.ClickCount / a.ViewCount * 100 : 0,
                ConversionCount = 0, // Placeholder
                ConversionRate = 0, // Placeholder
                RevenueGenerated = a.ClickCount * 50, // Placeholder
                PerformanceScore = CalculatePerformanceScore(a),
                StartDate = a.StartDate,
                EndDate = a.EndDate
            })
            .OrderByDescending(p => p.PerformanceScore)
            .Take(count)
            .ToList();
    }

    private List<AdvertisementPerformanceDto> GenerateLowPerformingAds(List<Advertisement> advertisements, int count)
    {
        return advertisements
            .Select(a => new AdvertisementPerformanceDto
            {
                AdvertisementId = a.Id,
                Title = a.TitleEn,
                Type = a.Type,
                ViewCount = a.ViewCount,
                ClickCount = a.ClickCount,
                ClickThroughRate = a.ViewCount > 0 ? (double)a.ClickCount / a.ViewCount * 100 : 0,
                ConversionCount = 0, // Placeholder
                ConversionRate = 0, // Placeholder
                RevenueGenerated = a.ClickCount * 50, // Placeholder
                PerformanceScore = CalculatePerformanceScore(a),
                StartDate = a.StartDate,
                EndDate = a.EndDate
            })
            .OrderBy(p => p.PerformanceScore)
            .Take(count)
            .ToList();
    }

    private List<MonthlyAdvertisementStatsDto> GenerateMonthlyAdvertisementStats(List<Advertisement> advertisements)
    {
        return advertisements
            .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month })
            .Select(g => new MonthlyAdvertisementStatsDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                AdvertisementCount = g.Count(),
                TotalViews = g.Sum(a => a.ViewCount),
                TotalClicks = g.Sum(a => a.ClickCount),
                ClickThroughRate = g.Sum(a => a.ViewCount) > 0 ? (double)g.Sum(a => a.ClickCount) / g.Sum(a => a.ViewCount) * 100 : 0,
                TotalConversions = 0, // Placeholder
                ConversionRate = 0, // Placeholder
                RevenueGenerated = g.Sum(a => a.ClickCount) * 50 // Placeholder
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    private string GetBestPerformingAdType(List<AdminAdvertisementDto> advertisements)
    {
        if (!advertisements.Any()) return "None";

        return advertisements
            .GroupBy(a => a.Type)
            .OrderByDescending(g => g.Average(a => a.PerformanceScore))
            .FirstOrDefault()?.Key.ToString() ?? "None";
    }

    private string GetMostPopularDiscountType(List<AdminAdvertisementDto> advertisements)
    {
        if (!advertisements.Any()) return "None";

        var withPercentageDiscount = advertisements.Count(a => a.DiscountPercentage > 0);
        var withPriceDiscount = advertisements.Count(a => a.DiscountPrice.HasValue && a.DiscountPrice.Value > 0);

        if (withPercentageDiscount > withPriceDiscount)
            return "Percentage Discount";
        else if (withPriceDiscount > withPercentageDiscount)
            return "Fixed Price Discount";
        else
            return "Mixed";
    }

    #endregion

    #region Missing Paginated Methods

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsByTypeAsync(AdvertisementType type, bool activeOnly = true, int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.Type == type);

            if (activeOnly)
                query = query.Where(a => a.IsActive && a.Status == AdvertisementStatus.Active);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements by type: {Type}", type);
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements by type", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementTemplatesAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.TitleEn.Contains("Template") || a.TitleAr.Contains("Template")); // Assuming templates have "Template" in title

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisement templates");
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisement templates", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsByCarAsync(int carId, int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.CarId == carId);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements by car: {CarId}", carId);
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements by car", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.CategoryId == categoryId);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements by category: {CategoryId}", categoryId);
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements by category", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetActiveAdvertisementsAsync(DateTime? date = null, int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var targetDate = date ?? DateTime.UtcNow;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.IsActive &&
                           a.Status == AdvertisementStatus.Active &&
                           a.StartDate <= targetDate &&
                           a.EndDate >= targetDate);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active advertisements");
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving active advertisements", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>> GetAdvertisementsByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Advertisements
                .Include(a => a.Car)
                .Include(a => a.Category)
                .Include(a => a.CreatedByUser)
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var advertisements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var advertisementDtos = new List<AdminAdvertisementDto>();
            foreach (var advertisement in advertisements)
            {
                advertisementDtos.Add(await MapToAdminAdvertisementDto(advertisement));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminAdvertisementDto>.Create(
                advertisementDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting advertisements by date range");
            return ApiResponse<PaginatedResponseDto<AdminAdvertisementDto>>.Error("An error occurred while retrieving advertisements by date range", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion
}
