using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Features.Cars;

public class CarService : ICarService
{
    private readonly ICarRepository _carRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public CarService(
        ICarRepository carRepository,
        IFavoriteRepository favoriteRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _carRepository = carRepository;
        _favoriteRepository = favoriteRepository;
        _responseService = responseService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<List<CarDto>>> GetCarsAsync(string? sortByCost = null, int page = 1, int pageSize = 10, string? brand = null, string? userId = null)
    {
        try 
        {
            var cars = await _carRepository.GetCarsAsync(sortByCost, page, pageSize, brand);
            var carDtos = new List<CarDto>();

            foreach (var car in cars)
            {
                var carDto = await MapToCarDtoAsync(car, userId);
                carDtos.Add(carDto);
            }

            return _responseService.Success(carDtos, "CarsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<CarDto>> GetCarByIdAsync(int id, string? userId = null)
    {
        try
        {
            var car = await _carRepository.GetCarWithIncludesAsync(id);

            if (car == null)
            {
                return _responseService.NotFound<CarDto>("CarNotFound");
            }

            var carDto = await MapToCarDtoAsync(car, userId);
            return _responseService.Success(carDto, "CarRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<CarDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<CarDto>>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate, string? userId = null)
    {
        try
        {
            var cars = await _carRepository.GetAvailableCarsAsync(startDate, endDate);
            var carDtos = new List<CarDto>();

            foreach (var car in cars)
            {
                var carDto = await MapToCarDtoAsync(car, userId);
                carDtos.Add(carDto);
            }

            return _responseService.Success(carDtos, "CarsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<CarDto>>> GetCarsByCategoryAsync(int categoryId, string? userId = null)
    {
        try
        {
            var cars = await _carRepository.GetCarsByCategoryAsync(categoryId);
            var carDtos = new List<CarDto>();

            foreach (var car in cars)
            {
                var carDto = await MapToCarDtoAsync(car, userId);
                carDtos.Add(carDto);
            }

            return _responseService.Success(carDtos, "CarsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<CarDto>>> GetCarsByBranchAsync(int branchId, string? userId = null)
    {
        try
        {
            var cars = await _carRepository.GetCarsByBranchAsync(branchId);
            var carDtos = new List<CarDto>();

            foreach (var car in cars)
            {
                var carDto = await MapToCarDtoAsync(car, userId);
                carDtos.Add(carDto);
            }

            return _responseService.Success(carDtos, "CarsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarDto>>("InternalServerError");
        }
    }

    private async Task<CarDto> MapToCarDtoAsync(Domain.Entities.Car car, string? userId = null)
    {
        var isArabic = _localizationService.GetCurrentCulture() == "ar";

        var carDto = new CarDto
        {
            Id = car.Id,
            Model = isArabic ? car.ModelAr : car.ModelEn,
            Brand = isArabic ? car.BrandAr : car.BrandEn,
            Year = car.Year,
            Color = isArabic ? car.ColorAr : car.ColorEn,
            SeatingCapacity = car.SeatingCapacity,
            TransmissionType = GetLocalizedTransmissionType(car.TransmissionType, isArabic),
            FuelType = car.FuelType,
            DailyPrice = car.DailyRate,
            WeeklyPrice = car.WeeklyRate,
            MonthlyPrice = car.MonthlyRate,
            
            Status = car.Status,
            ImageUrl = car.ImageUrl,
            Description = isArabic ? car.DescriptionAr : car.DescriptionEn,
            Mileage = car.Mileage,
            Category = new CategoryDto
            {
                Id = car.Category.Id,
                Name = isArabic ? car.Category.NameAr : car.Category.NameEn,
                Description = isArabic ? car.Category.DescriptionAr : car.Category.DescriptionEn,
                ImageUrl = car.Category.ImageUrl
            },
            Branch = new BranchDto
            {
                Id = car.Branch.Id,
                Name = isArabic ? car.Branch.NameAr : car.Branch.NameEn,
                Description = isArabic ? car.Branch.DescriptionAr : car.Branch.DescriptionEn,
                Address = car.Branch.Address,
                City = car.Branch.City,
                Country = car.Branch.Country,
                Phone = car.Branch.Phone,
                Email = car.Branch.Email,
                Latitude = car.Branch.Latitude,
                Longitude = car.Branch.Longitude,
                WorkingHours = car.Branch.WorkingHours
            },
            IsFavorite = false, // Default to false, will be updated below if user is authenticated,
            AvrageRate  = car.AverageRating
        };

        // Check if car is favorite for authenticated user
        if (!string.IsNullOrEmpty(userId))
        {
            try
            {
                carDto.IsFavorite = await _favoriteRepository.IsFavoriteAsync(userId, car.Id);
            }
            catch
            {
                // If there's an error checking favorites, default to false
                carDto.IsFavorite = false;
            }
        }

        return carDto;
    }

    // Car management operations (Admin only)
    public async Task<ApiResponse<CarDto>> CreateCarAsync(CreateCarDto createCarDto)
    {
        try
        {
            // Validate category and branch exist
            if (!await _carRepository.IsCategoryValidAsync(createCarDto.CategoryId))
            {
                return _responseService.ValidationError<CarDto>("InvalidCategory");
            }

            if (!await _carRepository.IsBranchValidAsync(createCarDto.BranchId))
            {
                return _responseService.ValidationError<CarDto>("InvalidBranch");
            }

            // Check plate number uniqueness
            if (!await _carRepository.IsPlateNumberUniqueAsync(createCarDto.PlateNumber))
            {
                return _responseService.ValidationError<CarDto>("PlateNumberAlreadyExists");
            }

            // Create car entity
            var car = new Domain.Entities.Car
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
            var carDto = await MapToCarDtoAsync(createdCar!);

            return _responseService.Success(carDto, "CarCreated");
        }
        catch (Exception)
        {
            return _responseService.Error<CarDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<CarDto>> UpdateCarAsync(int id, UpdateCarDto updateCarDto)
    {
        try
        {
            var car = await _carRepository.GetCarWithIncludesAsync(id);
            if (car == null)
            {
                return _responseService.NotFound<CarDto>("CarNotFound");
            }

            // Validate category if provided
            if (updateCarDto.CategoryId.HasValue && !await _carRepository.IsCategoryValidAsync(updateCarDto.CategoryId.Value))
            {
                return _responseService.ValidationError<CarDto>("InvalidCategory");
            }

            // Validate branch if provided
            if (updateCarDto.BranchId.HasValue && !await _carRepository.IsBranchValidAsync(updateCarDto.BranchId.Value))
            {
                return _responseService.ValidationError<CarDto>("InvalidBranch");
            }

            // Check plate number uniqueness if provided
            if (!string.IsNullOrEmpty(updateCarDto.PlateNumber) &&
                !await _carRepository.IsPlateNumberUniqueAsync(updateCarDto.PlateNumber, id))
            {
                return _responseService.ValidationError<CarDto>("PlateNumberAlreadyExists");
            }

            // Update car properties
            if (!string.IsNullOrEmpty(updateCarDto.BrandAr)) car.BrandAr = updateCarDto.BrandAr;
            if (!string.IsNullOrEmpty(updateCarDto.BrandEn)) car.BrandEn = updateCarDto.BrandEn;
            if (!string.IsNullOrEmpty(updateCarDto.ModelAr)) car.ModelAr = updateCarDto.ModelAr;
            if (!string.IsNullOrEmpty(updateCarDto.ModelEn)) car.ModelEn = updateCarDto.ModelEn;
            if (updateCarDto.Year.HasValue) car.Year = updateCarDto.Year.Value;
            if (!string.IsNullOrEmpty(updateCarDto.ColorAr)) car.ColorAr = updateCarDto.ColorAr;
            if (!string.IsNullOrEmpty(updateCarDto.ColorEn)) car.ColorEn = updateCarDto.ColorEn;
            if (!string.IsNullOrEmpty(updateCarDto.PlateNumber)) car.PlateNumber = updateCarDto.PlateNumber;
            if (updateCarDto.SeatingCapacity.HasValue) car.SeatingCapacity = updateCarDto.SeatingCapacity.Value;
            if (updateCarDto.TransmissionType.HasValue) car.TransmissionType = updateCarDto.TransmissionType.Value;
            if (updateCarDto.FuelType.HasValue) car.FuelType = updateCarDto.FuelType.Value;
            if (updateCarDto.Status.HasValue) car.Status = updateCarDto.Status.Value;
            if (updateCarDto.ImageUrl != null) car.ImageUrl = updateCarDto.ImageUrl;
            if (updateCarDto.DescriptionAr != null) car.DescriptionAr = updateCarDto.DescriptionAr;
            if (updateCarDto.DescriptionEn != null) car.DescriptionEn = updateCarDto.DescriptionEn;
            if (updateCarDto.Mileage.HasValue) car.Mileage = updateCarDto.Mileage.Value;
            if (updateCarDto.Features != null) car.Features = updateCarDto.Features;
            if (updateCarDto.CategoryId.HasValue) car.CategoryId = updateCarDto.CategoryId.Value;
            if (updateCarDto.BranchId.HasValue) car.BranchId = updateCarDto.BranchId.Value;

            car.UpdatedAt = DateTime.UtcNow;

            await _carRepository.UpdateAsync(car);
            await _carRepository.SaveChangesAsync();

            // Get updated car with includes
            var updatedCar = await _carRepository.GetCarWithIncludesAsync(id);
            var carDto = await MapToCarDtoAsync(updatedCar!);

            return _responseService.Success(carDto, "CarUpdated");
        }
        catch (Exception)
        {
            return _responseService.Error<CarDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCarAsync(int id)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car == null)
            {
                return _responseService.NotFound<bool>("CarNotFound");
            }

            // Soft delete - set IsActive to false
            car.IsActive = false;
            car.UpdatedAt = DateTime.UtcNow;

            await _carRepository.UpdateAsync(car);
            await _carRepository.SaveChangesAsync();

            return _responseService.Success(true, "CarDeleted");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> ToggleCarStatusAsync(int id, CarStatus status)
    {
        try
        {
            var success = await _carRepository.UpdateCarStatusAsync(id, status);
            if (!success)
            {
                return _responseService.NotFound<bool>("CarNotFound");
            }

            return _responseService.Success(true, "CarStatusUpdated");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    // Rate management operations
    public async Task<ApiResponse<CarRatesDto>> UpdateCarRatesAsync(int id, UpdateCarRatesDto updateRatesDto)
    {
        try
        {
            var car = await _carRepository.GetCarWithIncludesAsync(id);
            if (car == null)
            {
                return _responseService.NotFound<CarRatesDto>("CarNotFound");
            }

            var success = await _carRepository.UpdateCarRatesAsync(id, updateRatesDto.DailyRate, updateRatesDto.WeeklyRate, updateRatesDto.MonthlyRate);
            if (!success)
            {
                return _responseService.Error<CarRatesDto>("FailedToUpdateRates");
            }

            // Get updated car for response
            var updatedCar = await _carRepository.GetCarWithIncludesAsync(id);
            var isArabic = _localizationService.GetCurrentCulture() == "ar";

            var carRatesDto = new CarRatesDto
            {
                CarId = updatedCar!.Id,
                CarName = $"{(isArabic ? updatedCar.BrandAr : updatedCar.BrandEn)} {(isArabic ? updatedCar.ModelAr : updatedCar.ModelEn)}",
                DailyRate = updatedCar.DailyRate,
                WeeklyRate = updatedCar.WeeklyRate,
                MonthlyRate = updatedCar.MonthlyRate,
                LastUpdated = updatedCar.UpdatedAt,
                UpdatedBy = "Admin" // TODO: Get actual user info
            };

            return _responseService.Success(carRatesDto, "CarRatesUpdated");
        }
        catch (Exception)
        {
            return _responseService.Error<CarRatesDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<CarRatesDto>>> GetAllCarRatesAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var cars = await _carRepository.GetCarsWithRatesAsync(page, pageSize);
            var isArabic = _localizationService.GetCurrentCulture() == "ar";

            var carRatesDtos = cars.Select(car => new CarRatesDto
            {
                CarId = car.Id,
                CarName = $"{(isArabic ? car.BrandAr : car.BrandEn)} {(isArabic ? car.ModelAr : car.ModelEn)}",
                DailyRate = car.DailyRate,
                WeeklyRate = car.WeeklyRate,
                MonthlyRate = car.MonthlyRate,
                LastUpdated = car.UpdatedAt,
                UpdatedBy = "Admin" // TODO: Get actual user info
            }).ToList();

            return _responseService.Success(carRatesDtos, "CarRatesRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarRatesDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<CarRatesDto>> GetCarRatesAsync(int id)
    {
        try
        {
            var car = await _carRepository.GetCarWithIncludesAsync(id);
            if (car == null)
            {
                return _responseService.NotFound<CarRatesDto>("CarNotFound");
            }

            var isArabic = _localizationService.GetCurrentCulture() == "ar";
            var carRatesDto = new CarRatesDto
            {
                CarId = car.Id,
                CarName = $"{(isArabic ? car.BrandAr : car.BrandEn)} {(isArabic ? car.ModelAr : car.ModelEn)}",
                DailyRate = car.DailyRate,
                WeeklyRate = car.WeeklyRate,
                MonthlyRate = car.MonthlyRate,
                LastUpdated = car.UpdatedAt,
                UpdatedBy = "Admin" // TODO: Get actual user info
            };

            return _responseService.Success(carRatesDto, "CarRatesRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<CarRatesDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<CarRatesDto>>> BulkUpdateRatesAsync(BulkUpdateRatesDto bulkUpdateDto)
    {
        try
        {
            var rateUpdates = bulkUpdateDto.CarRates.ToDictionary(
                cr => cr.CarId,
                cr => (cr.DailyRate, cr.WeeklyRate, cr.MonthlyRate)
            );

            var success = await _carRepository.BulkUpdateRatesAsync(rateUpdates);
            if (!success)
            {
                return _responseService.Error<List<CarRatesDto>>("FailedToBulkUpdateRates");
            }

            // Get updated cars for response
            var carIds = bulkUpdateDto.CarRates.Select(cr => cr.CarId).ToList();
            var updatedCars = await _carRepository.GetCarsWithIncludesAsync();
            var filteredCars = updatedCars.Where(c => carIds.Contains(c.Id));

            var isArabic = _localizationService.GetCurrentCulture() == "ar";
            var carRatesDtos = filteredCars.Select(car => new CarRatesDto
            {
                CarId = car.Id,
                CarName = $"{(isArabic ? car.BrandAr : car.BrandEn)} {(isArabic ? car.ModelAr : car.ModelEn)}",
                DailyRate = car.DailyRate,
                WeeklyRate = car.WeeklyRate,
                MonthlyRate = car.MonthlyRate,
                LastUpdated = car.UpdatedAt,
                UpdatedBy = "Admin" // TODO: Get actual user info
            }).ToList();

            return _responseService.Success(carRatesDtos, "CarRatesBulkUpdated");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarRatesDto>>("InternalServerError");
        }
    }

    // Utility operations
    public async Task<ApiResponse<List<CarDto>>> SearchCarsAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        try
        {
            var cars = await _carRepository.SearchCarsAsync(searchTerm, page, pageSize);
            var carDtos = new List<CarDto>();

            foreach (var car in cars)
            {
                var carDto = await MapToCarDtoAsync(car);
                carDtos.Add(carDto);
            }

            return _responseService.Success(carDtos, "CarsSearched");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> ValidateCarExistsAsync(int id)
    {
        try
        {
            var exists = await _carRepository.ExistsAsync(id);
            return _responseService.Success(exists, exists ? "CarExists" : "CarNotFound");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    private string GetLocalizedTransmissionType(TransmissionType transmissionType, bool isArabic)
    {
        return transmissionType switch
        {
            TransmissionType.Manual => isArabic ? "يدوي" : "Manual",
            TransmissionType.Automatic => isArabic ? "أوتوماتيكي" : "Automatic",
            TransmissionType.CVT => isArabic ? "متغير مستمر" : "CVT",
            _ => isArabic ? "غير محدد" : "Unknown"
        };
    }
}