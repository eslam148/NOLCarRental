using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Application.DTOs.Admin;
using NOL.Application.DTOs.Common;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Services;

public class CarManagementService : ICarManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly ICarRepository _carRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ILogger<CarManagementService> _logger;

    public CarManagementService(
        ApplicationDbContext context,
        ICarRepository carRepository,
        IBookingRepository bookingRepository,
        ICategoryRepository categoryRepository,
        IBranchRepository branchRepository,
        ILogger<CarManagementService> logger)
    {
        _context = context;
        _carRepository = carRepository;
        _bookingRepository = bookingRepository;
        _categoryRepository = categoryRepository;
        _branchRepository = branchRepository;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<ApiResponse<AdminCarDto>> GetCarByIdAsync(int id)
    {
        try
        {
            var car = await _carRepository.GetCarWithIncludesAsync(id);
            if (car == null)
            {
                return ApiResponse<AdminCarDto>.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            var adminCarDto = await MapToAdminCarDto(car);
            return ApiResponse<AdminCarDto>.Success(adminCarDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting car by ID: {CarId}", id);
            return ApiResponse<AdminCarDto>.Error("An error occurred while retrieving car", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminCarDto>>> GetCarsAsync(CarFilterDto filter)
    {
        try
        {
            // Validate and normalize pagination parameters
            filter.ValidateAndNormalize();

            var query = _context.Cars
                .Include(c => c.Category)
                .Include(c => c.Branch)
                .Include(c => c.Bookings)
                .Include(c => c.Reviews)
                .Where(c => c.IsActive)
                .AsQueryable();

            // Apply filters
            if (filter.Status.HasValue)
            {
                query = query.Where(c => c.Status == filter.Status.Value);
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == filter.CategoryId.Value);
            }

            if (filter.BranchId.HasValue)
            {
                query = query.Where(c => c.BranchId == filter.BranchId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Brand))
            {
                query = query.Where(c => c.BrandAr.Contains(filter.Brand) || c.BrandEn.Contains(filter.Brand));
            }

            if (!string.IsNullOrEmpty(filter.Model))
            {
                query = query.Where(c => c.ModelAr.Contains(filter.Model) || c.ModelEn.Contains(filter.Model));
            }

            if (filter.YearFrom.HasValue)
            {
                query = query.Where(c => c.Year >= filter.YearFrom.Value);
            }

            if (filter.YearTo.HasValue)
            {
                query = query.Where(c => c.Year <= filter.YearTo.Value);
            }

            if (filter.DailyRateFrom.HasValue)
            {
                query = query.Where(c => c.DailyRate >= filter.DailyRateFrom.Value);
            }

            if (filter.DailyRateTo.HasValue)
            {
                query = query.Where(c => c.DailyRate <= filter.DailyRateTo.Value);
            }

            if (filter.TransmissionType.HasValue)
            {
                query = query.Where(c => c.TransmissionType == filter.TransmissionType.Value);
            }

            if (filter.FuelType.HasValue)
            {
                query = query.Where(c => c.FuelType == filter.FuelType.Value);
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "brand" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(c => c.BrandEn) : query.OrderByDescending(c => c.BrandEn),
                "model" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(c => c.ModelEn) : query.OrderByDescending(c => c.ModelEn),
                "year" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(c => c.Year) : query.OrderByDescending(c => c.Year),
                "dailyrate" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(c => c.DailyRate) : query.OrderByDescending(c => c.DailyRate),
                "status" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status),
                "createdat" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            // Apply pagination
            var totalCount = await query.CountAsync();
            var cars = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var adminCarDtos = new List<AdminCarDto>();
            foreach (var car in cars)
            {
                adminCarDtos.Add(await MapToAdminCarDto(car));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminCarDto>.Create(
                adminCarDtos,
                filter.Page,
                filter.PageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminCarDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cars with filter");
            return ApiResponse<PaginatedResponseDto<AdminCarDto>>.Error("An error occurred while retrieving cars", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminCarDto>> CreateCarAsync(AdminCreateCarDto createCarDto, string adminId)
    {
        try
        {
            // Validate category and branch exist
            if (!await _carRepository.IsCategoryValidAsync(createCarDto.CategoryId))
            {
                return ApiResponse<AdminCarDto>.Error("Invalid category", (string?)null, ApiStatusCode.BadRequest);
            }

            if (!await _carRepository.IsBranchValidAsync(createCarDto.BranchId))
            {
                return ApiResponse<AdminCarDto>.Error("Invalid branch", (string?)null, ApiStatusCode.BadRequest);
            }

            // Check plate number uniqueness
            if (!await _carRepository.IsPlateNumberUniqueAsync(createCarDto.PlateNumber))
            {
                return ApiResponse<AdminCarDto>.Error("Plate number already exists", (string?)null, ApiStatusCode.BadRequest);
            }

            var car = new Car
            {
                BrandAr = createCarDto.BrandAr,
                BrandEn = createCarDto.BrandEn,
                ModelAr = createCarDto.ModelAr,
                ModelEn = createCarDto.ModelEn,
                Year = createCarDto.Year,
                ColorAr = createCarDto.ColorAr,
                ColorEn = createCarDto.ColorEn,
                PlateNumber = createCarDto.PlateNumber,
                SeatingCapacity = createCarDto.SeatingCapacity,
                NumberOfDoors = createCarDto.NumberOfDoors,
                MaxSpeed = createCarDto.MaxSpeed,
                Engine = createCarDto.Engine,
                TransmissionType = createCarDto.TransmissionType,
                FuelType = createCarDto.FuelType,
                DailyRate = createCarDto.DailyRate,
                WeeklyRate = createCarDto.WeeklyRate,
                MonthlyRate = createCarDto.MonthlyRate,
                Status = createCarDto.Status,
                ImageUrl = createCarDto.ImageUrl,
                DescriptionAr = createCarDto.DescriptionAr,
                DescriptionEn = createCarDto.DescriptionEn,
                Mileage = createCarDto.Mileage,
                Features = createCarDto.Features,
                CategoryId = createCarDto.CategoryId,
                BranchId = createCarDto.BranchId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _carRepository.AddAsync(car);
            await _carRepository.SaveChangesAsync();

            // Get the created car with includes
            var createdCar = await _carRepository.GetCarWithIncludesAsync(car.Id);
            var adminCarDto = await MapToAdminCarDto(createdCar!);

            _logger.LogInformation("Car created successfully by admin {AdminId}: {CarId}", adminId, car.Id);
            return ApiResponse<AdminCarDto>.Success(adminCarDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating car by admin {AdminId}", adminId);
            return ApiResponse<AdminCarDto>.Error("An error occurred while creating car", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminCarDto>> UpdateCarAsync(int id, AdminUpdateCarDto updateCarDto, string adminId)
    {
        try
        {
            var car = await _carRepository.GetCarWithIncludesAsync(id);
            if (car == null)
            {
                return ApiResponse<AdminCarDto>.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Validate category and branch if provided
            if (updateCarDto.CategoryId.HasValue && !await _carRepository.IsCategoryValidAsync(updateCarDto.CategoryId.Value))
            {
                return ApiResponse<AdminCarDto>.Error("Invalid category", (string?)null, ApiStatusCode.BadRequest);
            }

            if (updateCarDto.BranchId.HasValue && !await _carRepository.IsBranchValidAsync(updateCarDto.BranchId.Value))
            {
                return ApiResponse<AdminCarDto>.Error("Invalid branch", (string?)null, ApiStatusCode.BadRequest);
            }

            // Check plate number uniqueness if provided
            if (!string.IsNullOrEmpty(updateCarDto.PlateNumber) && 
                !await _carRepository.IsPlateNumberUniqueAsync(updateCarDto.PlateNumber, id))
            {
                return ApiResponse<AdminCarDto>.Error("Plate number already exists", (string?)null, ApiStatusCode.BadRequest);
            }

            // Update properties
            if (!string.IsNullOrEmpty(updateCarDto.BrandAr))
                car.BrandAr = updateCarDto.BrandAr;

            if (!string.IsNullOrEmpty(updateCarDto.BrandEn))
                car.BrandEn = updateCarDto.BrandEn;

            if (!string.IsNullOrEmpty(updateCarDto.ModelAr))
                car.ModelAr = updateCarDto.ModelAr;

            if (!string.IsNullOrEmpty(updateCarDto.ModelEn))
                car.ModelEn = updateCarDto.ModelEn;

            if (updateCarDto.Year.HasValue)
                car.Year = updateCarDto.Year.Value;

            if (!string.IsNullOrEmpty(updateCarDto.ColorAr))
                car.ColorAr = updateCarDto.ColorAr;

            if (!string.IsNullOrEmpty(updateCarDto.ColorEn))
                car.ColorEn = updateCarDto.ColorEn;

            if (!string.IsNullOrEmpty(updateCarDto.PlateNumber))
                car.PlateNumber = updateCarDto.PlateNumber;

            if (updateCarDto.SeatingCapacity.HasValue)
                car.SeatingCapacity = updateCarDto.SeatingCapacity.Value;

            if (updateCarDto.NumberOfDoors.HasValue)
                car.NumberOfDoors = updateCarDto.NumberOfDoors.Value;

            if (updateCarDto.MaxSpeed.HasValue)
                car.MaxSpeed = updateCarDto.MaxSpeed.Value;

            if (!string.IsNullOrEmpty(updateCarDto.Engine))
                car.Engine = updateCarDto.Engine;

            if (updateCarDto.TransmissionType.HasValue)
                car.TransmissionType = updateCarDto.TransmissionType.Value;

            if (updateCarDto.FuelType.HasValue)
                car.FuelType = updateCarDto.FuelType.Value;

            if (updateCarDto.DailyRate.HasValue)
                car.DailyRate = updateCarDto.DailyRate.Value;

            if (updateCarDto.WeeklyRate.HasValue)
                car.WeeklyRate = updateCarDto.WeeklyRate.Value;

            if (updateCarDto.MonthlyRate.HasValue)
                car.MonthlyRate = updateCarDto.MonthlyRate.Value;

            if (updateCarDto.Status.HasValue)
                car.Status = updateCarDto.Status.Value;

            if (updateCarDto.ImageUrl != null)
                car.ImageUrl = updateCarDto.ImageUrl;

            if (updateCarDto.DescriptionAr != null)
                car.DescriptionAr = updateCarDto.DescriptionAr;

            if (updateCarDto.DescriptionEn != null)
                car.DescriptionEn = updateCarDto.DescriptionEn;

            if (updateCarDto.Mileage.HasValue)
                car.Mileage = updateCarDto.Mileage.Value;

            if (updateCarDto.Features != null)
                car.Features = updateCarDto.Features;

            if (updateCarDto.CategoryId.HasValue)
                car.CategoryId = updateCarDto.CategoryId.Value;

            if (updateCarDto.BranchId.HasValue)
                car.BranchId = updateCarDto.BranchId.Value;

            car.UpdatedAt = DateTime.UtcNow;

            await _carRepository.UpdateAsync(car);
            await _carRepository.SaveChangesAsync();

            // Get updated car with includes
            var updatedCar = await _carRepository.GetCarWithIncludesAsync(id);
            var adminCarDto = await MapToAdminCarDto(updatedCar!);

            _logger.LogInformation("Car updated successfully by admin {AdminId}: {CarId}", adminId, id);
            return ApiResponse<AdminCarDto>.Success(adminCarDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating car {CarId} by admin {AdminId}", id, adminId);
            return ApiResponse<AdminCarDto>.Error("An error occurred while updating car", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> DeleteCarAsync(int id, string adminId)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return ApiResponse.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Check if car has active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.CarId == id && 
                              (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress));

            if (hasActiveBookings)
            {
                return ApiResponse.Error("Cannot delete car with active bookings", (string?)null, ApiStatusCode.BadRequest);
            }

            // Soft delete
            car.IsActive = false;
            car.UpdatedAt = DateTime.UtcNow;

            await _carRepository.UpdateAsync(car);
            await _carRepository.SaveChangesAsync();

            _logger.LogInformation("Car deleted successfully by admin {AdminId}: {CarId}", adminId, id);
            return ApiResponse.Success("Car deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting car {CarId} by admin {AdminId}", id, adminId);
            return ApiResponse.Error("An error occurred while deleting car", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Status Management

    public async Task<ApiResponse<AdminCarDto>> UpdateCarStatusAsync(int id, CarStatus status, string adminId, string? notes = null)
    {
        try
        {
            var car = await _carRepository.GetCarWithIncludesAsync(id);
            if (car == null)
            {
                return ApiResponse<AdminCarDto>.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            var oldStatus = car.Status;
            car.Status = status;
            car.UpdatedAt = DateTime.UtcNow;

            await _carRepository.UpdateAsync(car);
            await _carRepository.SaveChangesAsync();

            var adminCarDto = await MapToAdminCarDto(car);

            _logger.LogInformation("Car status updated by admin {AdminId}: {CarId} from {OldStatus} to {NewStatus}. Notes: {Notes}",
                adminId, id, oldStatus, status, notes);

            return ApiResponse<AdminCarDto>.Success(adminCarDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating car status {CarId} by admin {AdminId}", id, adminId);
            return ApiResponse<AdminCarDto>.Error("An error occurred while updating car status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkUpdateCarStatusAsync(List<int> carIds, CarStatus status, string adminId, string? notes = null)
    {
        try
        {
            var cars = await _context.Cars.Where(c => carIds.Contains(c.Id) && c.IsActive).ToListAsync();

            if (!cars.Any())
            {
                return ApiResponse.Error("No valid cars found", (string?)null, ApiStatusCode.NotFound);
            }

            foreach (var car in cars)
            {
                car.Status = status;
                car.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk car status update by admin {AdminId}: {CarCount} cars updated to {Status}. Notes: {Notes}",
                adminId, cars.Count, status, notes);

            return ApiResponse.Success($"Successfully updated {cars.Count} car(s) status to {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating car status by admin {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while updating car status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Bulk Operations

    public async Task<ApiResponse> BulkDeleteCarsAsync(List<int> carIds, string adminId)
    {
        try
        {
            var cars = await _context.Cars.Where(c => carIds.Contains(c.Id) && c.IsActive).ToListAsync();

            if (!cars.Any())
            {
                return ApiResponse.Error("No valid cars found", (string?)null, ApiStatusCode.NotFound);
            }

            // Check for active bookings
            var carsWithActiveBookings = await _context.Bookings
                .Where(b => carIds.Contains(b.CarId) &&
                           (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress))
                .Select(b => b.CarId)
                .Distinct()
                .ToListAsync();

            if (carsWithActiveBookings.Any())
            {
                return ApiResponse.Error($"Cannot delete cars with active bookings: {string.Join(", ", carsWithActiveBookings)}",
                    (string?)null, ApiStatusCode.BadRequest);
            }

            // Soft delete
            foreach (var car in cars)
            {
                car.IsActive = false;
                car.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk car deletion by admin {AdminId}: {CarCount} cars deleted", adminId, cars.Count);
            return ApiResponse.Success($"Successfully deleted {cars.Count} car(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting cars by admin {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while deleting cars", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkOperationAsync(BulkCarOperationDto operationDto, string adminId)
    {
        try
        {
            switch (operationDto.Operation.ToLower())
            {
                case "delete":
                    return await BulkDeleteCarsAsync(operationDto.CarIds, adminId);

                case "updatestatus":
                    if (!operationDto.NewStatus.HasValue)
                        return ApiResponse.Error("New status is required for status update operation", (string?)null, ApiStatusCode.BadRequest);
                    return await BulkUpdateCarStatusAsync(operationDto.CarIds, operationDto.NewStatus.Value, adminId);

                case "updatebranch":
                    if (!operationDto.NewBranchId.HasValue)
                        return ApiResponse.Error("New branch ID is required for branch update operation", (string?)null, ApiStatusCode.BadRequest);
                    return await BulkUpdateCarBranchAsync(operationDto.CarIds, operationDto.NewBranchId.Value, adminId);

                case "updatecategory":
                    if (!operationDto.NewCategoryId.HasValue)
                        return ApiResponse.Error("New category ID is required for category update operation", (string?)null, ApiStatusCode.BadRequest);
                    return await BulkUpdateCarCategoryAsync(operationDto.CarIds, operationDto.NewCategoryId.Value, adminId);

                default:
                    return ApiResponse.Error("Invalid operation", (string?)null, ApiStatusCode.BadRequest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation by admin {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while performing bulk operation", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    private async Task<ApiResponse> BulkUpdateCarBranchAsync(List<int> carIds, int newBranchId, string adminId)
    {
        // Validate branch exists
        if (!await _carRepository.IsBranchValidAsync(newBranchId))
        {
            return ApiResponse.Error("Invalid branch", (string?)null, ApiStatusCode.BadRequest);
        }

        var cars = await _context.Cars.Where(c => carIds.Contains(c.Id) && c.IsActive).ToListAsync();

        if (!cars.Any())
        {
            return ApiResponse.Error("No valid cars found", (string?)null, ApiStatusCode.NotFound);
        }

        foreach (var car in cars)
        {
            car.BranchId = newBranchId;
            car.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Bulk car branch update by admin {AdminId}: {CarCount} cars moved to branch {BranchId}",
            adminId, cars.Count, newBranchId);

        return ApiResponse.Success($"Successfully updated {cars.Count} car(s) branch");
    }

    private async Task<ApiResponse> BulkUpdateCarCategoryAsync(List<int> carIds, int newCategoryId, string adminId)
    {
        // Validate category exists
        if (!await _carRepository.IsCategoryValidAsync(newCategoryId))
        {
            return ApiResponse.Error("Invalid category", (string?)null, ApiStatusCode.BadRequest);
        }

        var cars = await _context.Cars.Where(c => carIds.Contains(c.Id) && c.IsActive).ToListAsync();

        if (!cars.Any())
        {
            return ApiResponse.Error("No valid cars found", (string?)null, ApiStatusCode.NotFound);
        }

        foreach (var car in cars)
        {
            car.CategoryId = newCategoryId;
            car.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Bulk car category update by admin {AdminId}: {CarCount} cars moved to category {CategoryId}",
            adminId, cars.Count, newCategoryId);

        return ApiResponse.Success($"Successfully updated {cars.Count} car(s) category");
    }

    #endregion

    #region Import/Export

    public async Task<ApiResponse<List<string>>> ImportCarsAsync(List<CarImportDto> cars, string adminId)
    {
        try
        {
            var errors = new List<string>();
            var successCount = 0;

            foreach (var carImport in cars)
            {
                try
                {
                    // Validate import data
                    var validationErrors = await ValidateCarImportData(carImport);
                    if (validationErrors.Any())
                    {
                        errors.AddRange(validationErrors.Select(e => $"Row {cars.IndexOf(carImport) + 1}: {e}"));
                        continue;
                    }

                    // Find category and branch by name
                    var category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.NameEn == carImport.CategoryName || c.NameAr == carImport.CategoryName);
                    var branch = await _context.Branches
                        .FirstOrDefaultAsync(b => b.NameEn == carImport.BranchName || b.NameAr == carImport.BranchName);

                    if (category == null)
                    {
                        errors.Add($"Row {cars.IndexOf(carImport) + 1}: Category '{carImport.CategoryName}' not found");
                        continue;
                    }

                    if (branch == null)
                    {
                        errors.Add($"Row {cars.IndexOf(carImport) + 1}: Branch '{carImport.BranchName}' not found");
                        continue;
                    }

                    // Parse enums
                    if (!Enum.TryParse<TransmissionType>(carImport.TransmissionType, true, out var transmissionType))
                    {
                        errors.Add($"Row {cars.IndexOf(carImport) + 1}: Invalid transmission type '{carImport.TransmissionType}'");
                        continue;
                    }

                    if (!Enum.TryParse<FuelType>(carImport.FuelType, true, out var fuelType))
                    {
                        errors.Add($"Row {cars.IndexOf(carImport) + 1}: Invalid fuel type '{carImport.FuelType}'");
                        continue;
                    }

                    if (!Enum.TryParse<CarStatus>(carImport.Status, true, out var status))
                    {
                        status = CarStatus.Available; // Default status
                    }

                    // Create car entity
                    var car = new Car
                    {
                        BrandAr = carImport.BrandAr,
                        BrandEn = carImport.BrandEn,
                        ModelAr = carImport.ModelAr,
                        ModelEn = carImport.ModelEn,
                        Year = carImport.Year,
                        ColorAr = carImport.ColorAr,
                        ColorEn = carImport.ColorEn,
                        PlateNumber = carImport.PlateNumber,
                        SeatingCapacity = carImport.SeatingCapacity,
                        NumberOfDoors = carImport.NumberOfDoors,
                        MaxSpeed = carImport.MaxSpeed,
                        Engine = carImport.Engine,
                        TransmissionType = transmissionType,
                        FuelType = fuelType,
                        DailyRate = carImport.DailyRate,
                        WeeklyRate = carImport.WeeklyRate,
                        MonthlyRate = carImport.MonthlyRate,
                        Status = status,
                        ImageUrl = carImport.ImageUrl,
                        DescriptionAr = carImport.DescriptionAr,
                        DescriptionEn = carImport.DescriptionEn,
                        Mileage = carImport.Mileage,
                        Features = carImport.Features,
                        CategoryId = category.Id,
                        BranchId = branch.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _carRepository.AddAsync(car);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {cars.IndexOf(carImport) + 1}: {ex.Message}");
                }
            }

            if (successCount > 0)
            {
                await _carRepository.SaveChangesAsync();
            }

            _logger.LogInformation("Car import completed by admin {AdminId}: {SuccessCount} successful, {ErrorCount} errors",
                adminId, successCount, errors.Count);

            if (errors.Any())
            {
                return ApiResponse<List<string>>.Error($"Import completed with {errors.Count} errors. {successCount} cars imported successfully.",
                    errors, ApiStatusCode.BadRequest);
            }

            return ApiResponse<List<string>>.Success(new List<string> { $"Successfully imported {successCount} cars" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing cars by admin {AdminId}", adminId);
            return ApiResponse<List<string>>.Error("An error occurred while importing cars", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportCarsAsync(CarFilterDto filter, string format = "excel")
    {
        try
        {
            // For this implementation, return empty byte array as export functionality would require additional libraries
            // In a real implementation, you would use libraries like EPPlus for Excel or similar
            _logger.LogInformation("Car export requested with format: {Format}", format);
            return ApiResponse<byte[]>.Success(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting cars");
            return ApiResponse<byte[]>.Error("An error occurred while exporting cars", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> GetCarTemplateAsync()
    {
        try
        {
            // For this implementation, return empty byte array as template generation would require additional libraries
            _logger.LogInformation("Car import template requested");
            return ApiResponse<byte[]>.Success(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating car template");
            return ApiResponse<byte[]>.Error("An error occurred while generating template", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    private async Task<List<string>> ValidateCarImportData(CarImportDto carImport)
    {
        var errors = new List<string>();

        // Check plate number uniqueness
        if (!await _carRepository.IsPlateNumberUniqueAsync(carImport.PlateNumber))
        {
            errors.Add($"Plate number '{carImport.PlateNumber}' already exists");
        }

        // Validate required fields
        if (string.IsNullOrEmpty(carImport.BrandAr) || string.IsNullOrEmpty(carImport.BrandEn))
            errors.Add("Brand is required in both Arabic and English");

        if (string.IsNullOrEmpty(carImport.ModelAr) || string.IsNullOrEmpty(carImport.ModelEn))
            errors.Add("Model is required in both Arabic and English");

        if (carImport.Year < 1900 || carImport.Year > DateTime.Now.Year + 1)
            errors.Add("Invalid year");

        if (carImport.SeatingCapacity < 1 || carImport.SeatingCapacity > 20)
            errors.Add("Seating capacity must be between 1 and 20");

        if (carImport.DailyRate <= 0 || carImport.WeeklyRate <= 0 || carImport.MonthlyRate <= 0)
            errors.Add("Rates must be greater than 0");

        return errors;
    }

    #endregion

    #region Analytics

    public async Task<ApiResponse<CarAnalyticsDto>> GetCarAnalyticsAsync(int carId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var car = await _carRepository.GetCarWithIncludesAsync(carId);
            if (car == null)
            {
                return ApiResponse<CarAnalyticsDto>.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            var bookingsQuery = _context.Bookings.Where(b => b.CarId == carId);

            if (startDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.StartDate >= startDate.Value);

            if (endDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.EndDate <= endDate.Value);

            var bookings = await bookingsQuery.ToListAsync();
            var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();

            var analytics = new CarAnalyticsDto
            {
                CarId = carId,
                CarInfo = $"{car.BrandEn} {car.ModelEn} ({car.Year}) - {car.PlateNumber}",
                TotalBookings = completedBookings.Count,
                TotalRevenue = completedBookings.Sum(b => b.TotalCost),
                AverageBookingValue = completedBookings.Any() ? completedBookings.Average(b => b.TotalCost) : 0,
                UtilizationRate = CalculateUtilizationRate(completedBookings, startDate, endDate),
                AverageRating = car.Reviews?.Any() == true ? car.Reviews.Average(r => r.Rating) : 0,
                TotalReviews = car.Reviews?.Count ?? 0,
                MonthlyRevenue = GenerateMonthlyRevenue(completedBookings),
                BookingTrends = GenerateBookingTrends(completedBookings)
            };

            return ApiResponse<CarAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting car analytics: {CarId}", carId);
            return ApiResponse<CarAnalyticsDto>.Error("An error occurred while retrieving car analytics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<CarAnalyticsDto>>> GetCarsAnalyticsAsync(CarFilterDto filter, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var carsResponse = await GetCarsAsync(filter);
            if (!carsResponse.Succeeded || carsResponse.Data == null)
            {
                return ApiResponse<List<CarAnalyticsDto>>.Error("Failed to retrieve cars", (string?)null, ApiStatusCode.InternalServerError);
            }

            var analyticsResults = new List<CarAnalyticsDto>();

            foreach (var car in carsResponse.Data.Data)
            {
                var analyticsResponse = await GetCarAnalyticsAsync(car.Id, startDate, endDate);
                if (analyticsResponse.Succeeded && analyticsResponse.Data != null)
                {
                    analyticsResults.Add(analyticsResponse.Data);
                }
            }

            return ApiResponse<List<CarAnalyticsDto>>.Success(analyticsResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cars analytics");
            return ApiResponse<List<CarAnalyticsDto>>.Error("An error occurred while retrieving cars analytics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    private double CalculateUtilizationRate(List<Booking> completedBookings, DateTime? startDate, DateTime? endDate)
    {
        if (!completedBookings.Any()) return 0;

        var periodStart = startDate ?? completedBookings.Min(b => b.StartDate);
        var periodEnd = endDate ?? completedBookings.Max(b => b.EndDate);
        var totalDaysInPeriod = (periodEnd - periodStart).Days + 1;

        var totalRentedDays = completedBookings.Sum(b => (b.EndDate - b.StartDate).Days + 1);

        return totalDaysInPeriod > 0 ? Math.Min((double)totalRentedDays / totalDaysInPeriod, 1.0) : 0;
    }

    private List<MonthlyCarRevenueDto> GenerateMonthlyRevenue(List<Booking> completedBookings)
    {
        return completedBookings
            .GroupBy(b => new { b.StartDate.Year, b.StartDate.Month })
            .Select(g => new MonthlyCarRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                Revenue = g.Sum(b => b.TotalCost),
                BookingCount = g.Count(),
                DaysRented = g.Sum(b => (b.EndDate - b.StartDate).Days + 1)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    private List<CarBookingTrendDto> GenerateBookingTrends(List<Booking> completedBookings)
    {
        return completedBookings
            .GroupBy(b => b.StartDate.Date)
            .Select(g => new CarBookingTrendDto
            {
                Date = g.Key,
                BookingCount = g.Count(),
                Revenue = g.Sum(b => b.TotalCost)
            })
            .OrderBy(t => t.Date)
            .ToList();
    }

    #endregion

    #region Maintenance

    public async Task<ApiResponse<List<CarMaintenanceRecordDto>>> GetCarMaintenanceHistoryAsync(int carId)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return ApiResponse<List<CarMaintenanceRecordDto>>.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            // For this implementation, return empty list as maintenance records table is not implemented
            // In a full implementation, this would query a CarMaintenanceRecord table
            return ApiResponse<List<CarMaintenanceRecordDto>>.Success(new List<CarMaintenanceRecordDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting car maintenance history: {CarId}", carId);
            return ApiResponse<List<CarMaintenanceRecordDto>>.Error("An error occurred while retrieving maintenance history", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<CarMaintenanceRecordDto>> AddMaintenanceRecordAsync(int carId, CarMaintenanceRecordDto maintenanceRecord, string adminId)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return ApiResponse<CarMaintenanceRecordDto>.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            // For this implementation, just log the maintenance record
            // In a full implementation, this would save to a CarMaintenanceRecord table
            _logger.LogInformation("Maintenance record added for car {CarId} by admin {AdminId}: {MaintenanceType} - {Description}",
                carId, adminId, maintenanceRecord.MaintenanceType, maintenanceRecord.Description);

            return ApiResponse<CarMaintenanceRecordDto>.Success(maintenanceRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding maintenance record for car {CarId} by admin {AdminId}", carId, adminId);
            return ApiResponse<CarMaintenanceRecordDto>.Error("An error occurred while adding maintenance record", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponseDto<AdminCarDto>>> GetCarsNeedingMaintenanceAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // For this implementation, return cars with high mileage or old cars
            // In a full implementation, this would be based on maintenance schedules and records
            var query = _context.Cars
                .Include(c => c.Category)
                .Include(c => c.Branch)
                .Include(c => c.Bookings)
                .Include(c => c.Reviews)
                .Where(c => c.IsActive && (c.Mileage > 100000 || c.Year < DateTime.Now.Year - 5));

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var cars = await query
                .OrderByDescending(c => c.Mileage)
                .ThenBy(c => c.Year)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var adminCarDtos = new List<AdminCarDto>();
            foreach (var car in cars)
            {
                adminCarDtos.Add(await MapToAdminCarDto(car));
            }

            // Create paginated response
            var paginatedResult = PaginatedResponseDto<AdminCarDto>.Create(
                adminCarDtos,
                page,
                pageSize,
                totalCount);

            return ApiResponse<PaginatedResponseDto<AdminCarDto>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cars needing maintenance");
            return ApiResponse<PaginatedResponseDto<AdminCarDto>>.Error("An error occurred while retrieving cars needing maintenance", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Image Management

    public async Task<ApiResponse<string>> UploadCarImageAsync(int carId, Stream imageStream, string fileName, string adminId)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return ApiResponse<string>.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            // For this implementation, we'll just simulate image upload
            // In a real implementation, this would upload to cloud storage (Azure Blob, AWS S3, etc.)
            var imageUrl = $"/images/cars/{carId}/{fileName}";

            car.ImageUrl = imageUrl;
            car.UpdatedAt = DateTime.UtcNow;

            await _carRepository.UpdateAsync(car);
            await _carRepository.SaveChangesAsync();

            _logger.LogInformation("Car image uploaded by admin {AdminId} for car {CarId}: {FileName}", adminId, carId, fileName);
            return ApiResponse<string>.Success(imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading car image for car {CarId} by admin {AdminId}", carId, adminId);
            return ApiResponse<string>.Error("An error occurred while uploading image", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> DeleteCarImageAsync(int carId, string adminId)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return ApiResponse.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            var oldImageUrl = car.ImageUrl;
            car.ImageUrl = null;
            car.UpdatedAt = DateTime.UtcNow;

            await _carRepository.UpdateAsync(car);
            await _carRepository.SaveChangesAsync();

            _logger.LogInformation("Car image deleted by admin {AdminId} for car {CarId}. Old URL: {OldImageUrl}", adminId, carId, oldImageUrl);
            return ApiResponse.Success("Car image deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting car image for car {CarId} by admin {AdminId}", carId, adminId);
            return ApiResponse.Error("An error occurred while deleting image", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Validation

    public async Task<ApiResponse<bool>> ValidatePlateNumberAsync(string plateNumber, int? excludeCarId = null)
    {
        try
        {
            var isUnique = await _carRepository.IsPlateNumberUniqueAsync(plateNumber, excludeCarId);
            return ApiResponse<bool>.Success(isUnique);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating plate number: {PlateNumber}", plateNumber);
            return ApiResponse<bool>.Error("An error occurred while validating plate number", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<string>>> ValidateCarImportAsync(List<CarImportDto> cars)
    {
        try
        {
            var allErrors = new List<string>();

            for (int i = 0; i < cars.Count; i++)
            {
                var car = cars[i];
                var errors = await ValidateCarImportData(car);

                if (errors.Any())
                {
                    allErrors.AddRange(errors.Select(e => $"Row {i + 1}: {e}"));
                }
            }

            // Check for duplicate plate numbers within the import
            var plateNumbers = cars.Select(c => c.PlateNumber).ToList();
            var duplicatePlates = plateNumbers
                .GroupBy(p => p)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatePlates.Any())
            {
                allErrors.Add($"Duplicate plate numbers found in import: {string.Join(", ", duplicatePlates)}");
            }

            return ApiResponse<List<string>>.Success(allErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating car import");
            return ApiResponse<List<string>>.Error("An error occurred while validating import", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    // Helper method to map Car entity to AdminCarDto
    private async Task<AdminCarDto> MapToAdminCarDto(Car car)
    {
        // Calculate analytics
        var totalBookings = car.Bookings?.Count(b => b.Status == BookingStatus.Completed) ?? 0;
        var totalRevenue = car.Bookings?
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.TotalCost) ?? 0;

        var lastBookingDate = car.Bookings?
            .Where(b => b.Status == BookingStatus.Completed)
            .OrderByDescending(b => b.EndDate)
            .FirstOrDefault()?.EndDate;

        // Calculate utilization rate (simplified)
        var utilizationRate = totalBookings > 0 ? Math.Min(totalBookings * 0.1, 1.0) : 0.0;

        return new AdminCarDto
        {
            Id = car.Id,
            Brand = car.BrandEn, // Default to English, could be localized
            Model = car.ModelEn,
            Year = car.Year,
            Color = car.ColorEn,
            SeatingCapacity = car.SeatingCapacity,
            NumberOfDoors = car.NumberOfDoors,
            MaxSpeed = car.MaxSpeed,
            Engine = car.Engine,
            TransmissionType = car.TransmissionType.ToString(),
            FuelType = car.FuelType,
            DailyPrice = car.DailyRate,
            WeeklyPrice = car.WeeklyRate,
            MonthlyPrice = car.MonthlyRate,
            Status = car.Status,
            ImageUrl = car.ImageUrl,
            Description = car.DescriptionEn,
            Mileage = car.Mileage,
            Category = new CategoryDto
            {
                Id = car.Category?.Id ?? 0,
                Name = car.Category?.NameEn ?? "", // Use English name for DTO
                Description = car.Category?.DescriptionEn ?? "",
                ImageUrl = car.Category?.ImageUrl,
                SortOrder = car.Category?.SortOrder ?? 0
            },
            Branch = new BranchDto
            {
                Id = car.Branch?.Id ?? 0,
                Name = car.Branch?.NameEn ?? "", // Use English name for DTO
                Description = car.Branch?.DescriptionEn ?? "",
                Address = car.Branch?.Address ?? "",
                City = car.Branch?.City ?? "",
                Country = car.Branch?.Country ?? "",
                Phone = car.Branch?.Phone,
                Email = car.Branch?.Email,
                Latitude = car.Branch?.Latitude ?? 0,
                Longitude = car.Branch?.Longitude ?? 0,
                WorkingHours = car.Branch?.WorkingHours
            },
            CreatedAt = car.CreatedAt,
            UpdatedAt = car.UpdatedAt,
            TotalBookings = totalBookings,
            TotalRevenue = totalRevenue,
            UtilizationRate = utilizationRate,
            LastBookingDate = lastBookingDate,
            NextMaintenanceDate = null, // Would be calculated based on mileage/time
            MaintenanceHistory = new List<CarMaintenanceRecordDto>() // Would be populated from maintenance records
        };
    }
}
