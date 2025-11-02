using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Domain.Extensions;

namespace NOL.Application.Features.Advertisements;

public class AdvertisementService : IAdvertisementService
{
    private readonly IAdvertisementRepository _advertisementRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public AdvertisementService(
        IAdvertisementRepository advertisementRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _advertisementRepository = advertisementRepository;
        _responseService = responseService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<List<AdvertisementDto>>> GetActiveAdvertisementsAsync()
    {
        try
        {
            var advertisements = await _advertisementRepository.GetActiveAdvertisementsAsync();
            var advertisementDtos = advertisements.Select(MapToAdvertisementDto).ToList();
            return _responseService.Success(advertisementDtos, "AdvertisementsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<AdvertisementDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<AdvertisementDto>>> GetAdvertisementsByCarIdAsync(int carId)
    {
        try
        {
            var advertisements = await _advertisementRepository.GetAdvertisementsByCarIdAsync(carId);
            var advertisementDtos = advertisements.Select(MapToAdvertisementDto).ToList();
            return _responseService.Success(advertisementDtos, "AdvertisementsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<AdvertisementDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<AdvertisementDto>>> GetAdvertisementsByCategoryIdAsync(int categoryId)
    {
        try
        {
            var advertisements = await _advertisementRepository.GetAdvertisementsByCategoryIdAsync(categoryId);
            var advertisementDtos = advertisements.Select(MapToAdvertisementDto).ToList();
            return _responseService.Success(advertisementDtos, "AdvertisementsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<AdvertisementDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<AdvertisementDto>>> GetFeaturedAdvertisementsAsync()
    {
        try
        {
            var advertisements = await _advertisementRepository.GetFeaturedAdvertisementsAsync();
            var advertisementDtos = advertisements.Select(MapToAdvertisementDto).ToList();
            return _responseService.Success(advertisementDtos, "FeaturedAdvertisementsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<AdvertisementDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<AdvertisementDto>> GetAdvertisementByIdAsync(int id)
    {
        try
        {
            var advertisement = await _advertisementRepository.GetAdvertisementByIdAsync(id);
            if (advertisement == null)
            {
                return _responseService.NotFound<AdvertisementDto>("AdvertisementNotFound");
            }

            // Increment view count
            await _advertisementRepository.IncrementViewCountAsync(id);

            var advertisementDto = MapToAdvertisementDto(advertisement);
            return _responseService.Success(advertisementDto, "AdvertisementRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<AdvertisementDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<AdvertisementDto>> CreateAdvertisementAsync(CreateAdvertisementDto createDto, string userId)
    {
        try
        {
            // Validate business rules
            if (createDto.StartDate >= createDto.EndDate)
            {
                return _responseService.ValidationError<AdvertisementDto>("InvalidDateRange");
            }

            if (createDto.CarId.HasValue && createDto.CategoryId.HasValue)
            {
                return _responseService.Error<AdvertisementDto>("CannotSetBothCarAndCategory");
            }

            if (!createDto.CarId.HasValue && !createDto.CategoryId.HasValue)
            {
                return _responseService.Error<AdvertisementDto>("MustSetEitherCarOrCategory");
            }

            // Calculate discount percentage if discount price is provided
            var discountPercentage = 0m;
            if (createDto.DiscountPrice.HasValue && createDto.DiscountPrice.Value > 0)
            {
                discountPercentage = ((createDto.Price - createDto.DiscountPrice.Value) / createDto.Price) * 100;
            }

            var advertisement = new Advertisement
            {
                TitleAr = createDto.TitleAr,
                TitleEn = createDto.TitleEn,
                DescriptionAr = createDto.DescriptionAr,
                DescriptionEn = createDto.DescriptionEn,
                Price = createDto.Price,
                DiscountPrice = createDto.DiscountPrice,
                DiscountPercentage = discountPercentage,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                ImageUrl = createDto.ImageUrl,
                Type = createDto.Type,
                Status = AdvertisementStatus.Active,
                IsFeatured = createDto.IsFeatured,
                SortOrder = createDto.SortOrder,
                CarId = createDto.CarId,
                CategoryId = createDto.CategoryId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdAdvertisement = await _advertisementRepository.CreateAdvertisementAsync(advertisement);
            var advertisementDto = MapToAdvertisementDto(createdAdvertisement);

            return _responseService.Success(advertisementDto, "AdvertisementCreated");
        }
        catch (Exception)
        {
            return _responseService.Error<AdvertisementDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<AdvertisementDto>> UpdateAdvertisementAsync(int id, UpdateAdvertisementDto updateDto)
    {
        try
        {
            var advertisement = await _advertisementRepository.GetAdvertisementByIdAsync(id);
            if (advertisement == null)
            {
                return _responseService.NotFound<AdvertisementDto>("AdvertisementNotFound");
            }

            // Validate business rules
            if (updateDto.StartDate >= updateDto.EndDate)
            {
                return _responseService.ValidationError<AdvertisementDto>("InvalidDateRange");
            }

            // Calculate discount percentage if discount price is provided
            var discountPercentage = 0m;
            if (updateDto.DiscountPrice.HasValue && updateDto.DiscountPrice.Value > 0)
            {
                discountPercentage = ((updateDto.Price - updateDto.DiscountPrice.Value) / updateDto.Price) * 100;
            }

            // Update the advertisement
            advertisement.TitleAr = updateDto.TitleAr;
            advertisement.TitleEn = updateDto.TitleEn;
            advertisement.DescriptionAr = updateDto.DescriptionAr;
            advertisement.DescriptionEn = updateDto.DescriptionEn;
            advertisement.Price = updateDto.Price;
            advertisement.DiscountPrice = updateDto.DiscountPrice;
            advertisement.DiscountPercentage = discountPercentage;
            advertisement.StartDate = updateDto.StartDate;
            advertisement.EndDate = updateDto.EndDate;
            advertisement.ImageUrl = updateDto.ImageUrl;
            advertisement.Type = updateDto.Type;
            advertisement.Status = updateDto.Status;
            advertisement.IsFeatured = updateDto.IsFeatured;
            advertisement.SortOrder = updateDto.SortOrder;
            advertisement.CarId = updateDto.CarId;
            advertisement.CategoryId = updateDto.CategoryId;

            var updatedAdvertisement = await _advertisementRepository.UpdateAdvertisementAsync(advertisement);
            var advertisementDto = MapToAdvertisementDto(updatedAdvertisement);

            return _responseService.Success(advertisementDto, "AdvertisementUpdated");
        }
        catch (Exception)
        {
            return _responseService.Error<AdvertisementDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAdvertisementAsync(int id)
    {
        try
        {
            var result = await _advertisementRepository.DeleteAdvertisementAsync(id);
            if (!result)
            {
                return _responseService.NotFound<bool>("AdvertisementNotFound");
            }

            return _responseService.Success(true, "AdvertisementDeleted");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> IncrementViewCountAsync(int id)
    {
        try
        {
            var result = await _advertisementRepository.IncrementViewCountAsync(id);
            return _responseService.Success(result, "ViewCountIncremented");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> IncrementClickCountAsync(int id)
    {
        try
        {
            var result = await _advertisementRepository.IncrementClickCountAsync(id);
            return _responseService.Success(result, "ClickCountIncremented");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> UpdateAdvertisementStatusAsync(int id, AdvertisementStatus status)
    {
        try
        {
            var result = await _advertisementRepository.UpdateAdvertisementStatusAsync(id, status);
            if (!result)
            {
                return _responseService.NotFound<bool>("AdvertisementNotFound");
            }

            return _responseService.Success(true, "AdvertisementStatusUpdated");
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    private AdvertisementDto MapToAdvertisementDto(Advertisement advertisement)
    {
        var isArabic = _localizationService.GetCurrentCulture() == "ar";

        return new AdvertisementDto
        {
            Id = advertisement.Id,
            Title = isArabic ? advertisement.TitleAr : advertisement.TitleEn,
            Description = isArabic ? advertisement.DescriptionAr : advertisement.DescriptionEn,
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
            Car = advertisement.Car != null ? new CarDto
            {
                Id = advertisement.Car.Id,
                Brand = isArabic ? advertisement.Car.BrandAr : advertisement.Car.BrandEn,
                Model = isArabic ? advertisement.Car.ModelAr : advertisement.Car.ModelEn,
                Year = advertisement.Car.Year,
                Color = isArabic ? advertisement.Car.ColorAr : advertisement.Car.ColorEn,
                SeatingCapacity = advertisement.Car.SeatingCapacity,
                TransmissionType = GetLocalizedTransmissionType(advertisement.Car.TransmissionType, isArabic),
                FuelType = advertisement.Car.FuelType,
                DailyPrice = advertisement.Car.DailyRate,
                //WeeklyRate = advertisement.Car.WeeklyRate,
                //MonthlyRate = advertisement.Car.MonthlyRate,
                Status = advertisement.Car.Status.GetDescription(),
                ImageUrl = advertisement.Car.ImageUrl
            } : null,
            Category = advertisement.Category != null ? new CategoryDto
            {
                Id = advertisement.Category.Id,
                Name = isArabic ? advertisement.Category.NameAr : advertisement.Category.NameEn,
                Description = isArabic ? advertisement.Category.DescriptionAr : advertisement.Category.DescriptionEn,
                ImageUrl = advertisement.Category.ImageUrl,
                SortOrder = advertisement.Category.SortOrder
            } : null
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