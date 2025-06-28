using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Features.Cars;

public class CarService : ICarService
{
    private readonly ICarRepository _carRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public CarService(
        ICarRepository carRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _carRepository = carRepository;
        _responseService = responseService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<List<CarDto>>> GetCarsAsync(CarStatus? status = null, int? categoryId = null, int page = 1, int pageSize = 10)
    {
        try
        {
            var cars = await _carRepository.GetCarsAsync(status, categoryId, page, pageSize);
            var carDtos = cars.Select(MapToCarDto).ToList();
            return _responseService.Success(carDtos, "CarsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<CarDto>> GetCarByIdAsync(int id)
    {
        try
        {
            var car = await _carRepository.GetCarWithIncludesAsync(id);

            if (car == null)
            {
                return _responseService.NotFound<CarDto>("CarNotFound");
            }

            var carDto = MapToCarDto(car);
            return _responseService.Success(carDto, "CarRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<CarDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<CarDto>>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var cars = await _carRepository.GetAvailableCarsAsync(startDate, endDate);
            var carDtos = cars.Select(MapToCarDto).ToList();
            return _responseService.Success(carDtos, "CarsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<CarDto>>> GetCarsByCategoryAsync(int categoryId)
    {
        try
        {
            var cars = await _carRepository.GetCarsByCategoryAsync(categoryId);
            var carDtos = cars.Select(MapToCarDto).ToList();
            return _responseService.Success(carDtos, "CarsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<CarDto>>> GetCarsByBranchAsync(int branchId)
    {
        try
        {
            var cars = await _carRepository.GetCarsByBranchAsync(branchId);
            var carDtos = cars.Select(MapToCarDto).ToList();
            return _responseService.Success(carDtos, "CarsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CarDto>>("InternalServerError");
        }
    }

    private CarDto MapToCarDto(Domain.Entities.Car car)
    {
        var isArabic = _localizationService.GetCurrentCulture() == "ar";

        return new CarDto
        {
            Id = car.Id,
            Model = isArabic ? car.ModelAr : car.ModelEn,
            Brand = isArabic ? car.BrandAr : car.BrandEn,
            Year = car.Year,
            Color = car.Color,
            SeatingCapacity = car.SeatingCapacity,
            TransmissionType = car.TransmissionType,
            FuelType = car.FuelType,
            DailyRate = car.DailyRate,
            WeeklyRate = car.WeeklyRate,
            MonthlyRate = car.MonthlyRate,
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
            }
        };
    }
} 