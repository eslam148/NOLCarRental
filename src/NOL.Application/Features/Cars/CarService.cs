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