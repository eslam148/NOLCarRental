using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Features.Favorites;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ICarRepository _carRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public FavoriteService(
        IFavoriteRepository favoriteRepository,
        ICarRepository carRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _favoriteRepository = favoriteRepository;
        _carRepository = carRepository;
        _responseService = responseService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<List<FavoriteDto>>> GetUserFavoritesAsync(string userId)
    {
        try
        {
            var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);
            var favoriteDtos = favorites.Select(MapToFavoriteDto).ToList();
            return _responseService.Success(favoriteDtos, "FavoritesRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<FavoriteDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<FavoriteDto>> AddToFavoritesAsync(string userId, AddFavoriteDto addFavoriteDto)
    {
        try
        {
            // Check if car exists
            var car = await _carRepository.GetCarWithIncludesAsync(addFavoriteDto.CarId);
            if (car == null)
            {
                return _responseService.NotFound<FavoriteDto>("CarNotFound");
            }

            // Check if already in favorites
            var existingFavorite = await _favoriteRepository.IsFavoriteAsync(userId, addFavoriteDto.CarId);
            if (existingFavorite)
            {
                return _responseService.Error<FavoriteDto>("CarAlreadyInFavorites");
            }

            // Create new favorite
            var favorite = new Favorite
            {
                UserId = userId,
                CarId = addFavoriteDto.CarId,
                CreatedAt = DateTime.UtcNow
            };

            await _favoriteRepository.AddAsync(favorite);
            await _favoriteRepository.SaveChangesAsync();

            // Get the created favorite with includes for response
            var createdFavorite = await _favoriteRepository.GetFavoriteAsync(userId, addFavoriteDto.CarId);
            if (createdFavorite != null)
            {
                // Load navigation properties manually since GetFavoriteAsync might not include them
                createdFavorite.Car = car;
            }

            var favoriteDto = MapToFavoriteDto(createdFavorite!);
            return _responseService.Success(favoriteDto, "FavoriteAdded");
        }
        catch (Exception)
        {
            return _responseService.Error<FavoriteDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> RemoveFromFavoritesAsync(string userId, int carId)
    {
        try
        {
            var existingFavorite = await _favoriteRepository.IsFavoriteAsync(userId, carId);
            if (!existingFavorite)
            {
                return _responseService.NotFound<bool>("FavoriteNotFound");
            }

            await _favoriteRepository.RemoveFavoriteAsync(userId, carId);
            return _responseService.Success(true, "FavoriteRemoved");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> IsFavoriteAsync(string userId, int carId)
    {
        try
        {
            var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, carId);
            return _responseService.Success(isFavorite, "FavoriteStatusRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    private FavoriteDto MapToFavoriteDto(Favorite favorite)
    {
        var isArabic = _localizationService.GetCurrentCulture() == "ar";

        return new FavoriteDto
        {
            Id = favorite.Id,
            UserId = favorite.UserId,
            CarId = favorite.CarId,
            CreatedAt = favorite.CreatedAt,
            Car = new CarDto
            {
                Id = favorite.Car.Id,
                Model = isArabic ? favorite.Car.ModelAr : favorite.Car.ModelEn,
                Brand = isArabic ? favorite.Car.BrandAr : favorite.Car.BrandEn,
                Year = favorite.Car.Year,
                Color = isArabic ? favorite.Car.ColorAr : favorite.Car.ColorEn,
                SeatingCapacity = favorite.Car.SeatingCapacity,
                TransmissionType = GetLocalizedTransmissionType(favorite.Car.TransmissionType, isArabic),
                FuelType = favorite.Car.FuelType,
                DailyPrice = favorite.Car.DailyRate,
                
                Status = favorite.Car.Status,
                ImageUrl = favorite.Car.ImageUrl,
                Description = isArabic ? favorite.Car.DescriptionAr : favorite.Car.DescriptionEn,
                Mileage = favorite.Car.Mileage,
                Category = new CategoryDto
                {
                    Id = favorite.Car.Category.Id,
                    Name = isArabic ? favorite.Car.Category.NameAr : favorite.Car.Category.NameEn,
                    Description = isArabic ? favorite.Car.Category.DescriptionAr : favorite.Car.Category.DescriptionEn,
                    ImageUrl = favorite.Car.Category.ImageUrl
                },
                Branch = new BranchDto
                {
                    Id = favorite.Car.Branch.Id,
                    Name = isArabic ? favorite.Car.Branch.NameAr : favorite.Car.Branch.NameEn,
                    Description = isArabic ? favorite.Car.Branch.DescriptionAr : favorite.Car.Branch.DescriptionEn,
                    Address = favorite.Car.Branch.Address,
                    City = favorite.Car.Branch.City,
                    Country = favorite.Car.Branch.Country,
                    Phone = favorite.Car.Branch.Phone,
                    Email = favorite.Car.Branch.Email,
                    Latitude = favorite.Car.Branch.Latitude,
                    Longitude = favorite.Car.Branch.Longitude,
                    WorkingHours = favorite.Car.Branch.WorkingHours
                }
            }
        };
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