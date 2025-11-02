using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Domain.Extensions;

namespace NOL.Application.Features.Bookings;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ICarRepository _carRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;
    private readonly IRateCalculationService _rateCalculationService;

    public BookingService(
        IBookingRepository bookingRepository,
        ICarRepository carRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService,
        IRateCalculationService rateCalculationService)
    {
        _bookingRepository = bookingRepository;
        _carRepository = carRepository;
        _responseService = responseService;
        _localizationService = localizationService;
        _rateCalculationService = rateCalculationService;
    }

    public async Task<ApiResponse<List<BookingDto>>> GetUserBookingsAsync(string userId)
    {
        try
        {
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            var bookingDtos = bookings.Select(MapToBookingDto).ToList();
            return _responseService.Success(bookingDtos, "BookingsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<BookingDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<BookingDto>>> GetUserBookingsByStatusAsync(string userId, BookingStatus status)
    {
        try
        {
            var bookings = await _bookingRepository.GetUserBookingsByStatusAsync(userId, status);
            var bookingDtos = bookings.Select(MapToBookingDto).ToList();
            return _responseService.Success(bookingDtos, "BookingsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<BookingDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<BookingDto>> GetBookingByIdAsync(int id)
    {
        try
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return _responseService.NotFound<BookingDto>("BookingNotFound");
            }

            var bookingDto = MapToBookingDto(booking);
            return _responseService.Success(bookingDto, "BookingRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<BookingDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<BookingDto>> CreateBookingAsync(CreateBookingDto createDto, string userId)
    {
        try
        {
            // Validate car exists and is available
            var car = await _carRepository.GetByIdAsync(createDto.CarId);
            if (car == null)
            {
                return _responseService.NotFound<BookingDto>("CarNotFound");
            }

            // Check car availability for the requested dates
            var isAvailable = await _bookingRepository.IsCarAvailableAsync(
                createDto.CarId, 
                createDto.StartDate, 
                createDto.EndDate);

            if (!isAvailable)
            {
                return _responseService.Error<BookingDto>("CarNotAvailable");
            }

            // Validate branches exist and are active
            var isReceivingBranchAvailable = await _bookingRepository.IsBranchAvailableAsync(createDto.ReceivingBranchId);
            if (!isReceivingBranchAvailable)
            {
                return _responseService.Error<BookingDto>("ReceivingBranchNotAvailable");
            }

            var isDeliveryBranchAvailable = await _bookingRepository.IsBranchAvailableAsync(createDto.DeliveryBranchId);
            if (!isDeliveryBranchAvailable)
            {
                return _responseService.Error<BookingDto>("DeliveryBranchNotAvailable");
            }

            // Calculate rental period and costs
            var totalDays = (createDto.EndDate - createDto.StartDate).Days;
            if (totalDays <= 0)
            {
                return _responseService.ValidationError<BookingDto>("InvalidDateRange");
            }

            // Calculate rental cost using the optimized rate calculation service
            var carRentalCost = CalculateRentalCost(car, totalDays);

            // Process extras
            var extrasCost = 0m;
            var extraTypePriceIds = createDto.Extras.Select(e => e.ExtraTypePriceId).ToList();
            var extraTypePrices = await _bookingRepository.GetExtraTypePricesAsync(extraTypePriceIds);

            foreach (var extra in createDto.Extras)
            {
                var extraTypePrice = extraTypePrices.FirstOrDefault(etp => etp.Id == extra.ExtraTypePriceId);
                if (extraTypePrice != null)
                {
                    var extraCost = CalculateExtraCost(extraTypePrice, 1, totalDays);
                    extrasCost += extraCost;
                }
            }

            var totalCost = carRentalCost + extrasCost;

            // Generate booking number
            var bookingNumber = GenerateBookingNumber();

            var booking = new Booking
            {
                BookingNumber = bookingNumber,
                UserId = userId,
                CarId = createDto.CarId,
                ReceivingBranchId = createDto.ReceivingBranchId,
                DeliveryBranchId = createDto.DeliveryBranchId,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                TotalDays = totalDays,
                CarRentalCost = carRentalCost,
                ExtrasCost = extrasCost,
                TotalCost = totalCost,
                DiscountAmount = 0, // No discount logic for now
                FinalAmount = totalCost,
                Status = BookingStatus.Open,
                Notes = createDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBooking = await _bookingRepository.CreateBookingAsync(booking);

            // Create booking extras
            foreach (var extra in createDto.Extras)
            {
                var extraTypePrice = extraTypePrices.FirstOrDefault(etp => etp.Id == extra.ExtraTypePriceId);
                if (extraTypePrice != null)
                {
                    var unitPrice = GetUnitPrice(extraTypePrice, totalDays);
                    var bookingExtra = new BookingExtra
                    {
                        BookingId = createdBooking.Id,
                        ExtraTypePriceId = extra.ExtraTypePriceId,
                        Quantity = 1,
                        UnitPrice = unitPrice,
                        TotalPrice = unitPrice * 1
                    };
                    createdBooking.BookingExtras.Add(bookingExtra);
                }
            }

            await _bookingRepository.UpdateBookingAsync(createdBooking);

            // Retrieve the complete booking with all relations
            var completeBooking = await _bookingRepository.GetBookingByIdAsync(createdBooking.Id);
            var bookingDto = MapToBookingDto(completeBooking!);

            return _responseService.Success(bookingDto, "BookingCreated");
        }
        catch (Exception)
        {
            return _responseService.Error<BookingDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<BookingDto>> CancelBookingAsync(int bookingId, string userId)
    {
        try
        {
            // Get the booking with all related data
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return _responseService.NotFound<BookingDto>("BookingNotFound");
            }

            // Check if the user owns this booking
            if (booking.UserId != userId)
            {
                return _responseService.Forbidden<BookingDto>("UnauthorizedToModifyBooking");
            }

            // Check if booking can be canceled
            var cancellableStatuses = new[] { BookingStatus.Open, BookingStatus.Confirmed };
            if (!cancellableStatuses.Contains(booking.Status))
            {
                return _responseService.Error<BookingDto>("BookingCannotBeCanceled");
            }

            // Check cancellation policy (can't cancel if already started)
            if (booking.StartDate <= DateTime.UtcNow)
            {
                return _responseService.Error<BookingDto>("CannotCancelStartedBooking");
            }

            // Update booking status and add cancellation reason
            booking.Status = BookingStatus.Canceled;
            booking.CancellationReason = "Canceled by user";
            booking.UpdatedAt = DateTime.UtcNow;

            // Update the booking in the database
            var updatedBooking = await _bookingRepository.UpdateBookingAsync(booking);

            // Map to DTO and return
            var bookingDto = MapToBookingDto(updatedBooking);
            return _responseService.Success(bookingDto, "BookingCanceledSuccessfully");
        }
        catch (Exception)
        {
            return _responseService.Error<BookingDto>("InternalServerError");
        }
    }

    private decimal CalculateRentalCost(Car car, int totalDays)
    {
        var request = new RateCalculationRequestDto
        {
            TotalDays = totalDays,
            DailyRate = car.DailyRate,
            WeeklyRate = car.WeeklyRate,
            MonthlyRate = car.MonthlyRate
        };

        var result = _rateCalculationService.CalculateOptimalRate(request);
        return result.TotalCost;
    }

    private decimal CalculateExtraCost(ExtraTypePrice extraTypePrice, int quantity, int totalDays)
    {
        var request = new ExtraRateCalculationRequestDto
        {
            TotalDays = totalDays,
            Quantity = quantity,
            DailyPrice = extraTypePrice.DailyPrice,
            WeeklyPrice = extraTypePrice.WeeklyPrice,
            MonthlyPrice = extraTypePrice.MonthlyPrice
        };

        var result = _rateCalculationService.CalculateExtraRate(request);
        return result.TotalCost;
    }

    private decimal GetUnitPrice(ExtraTypePrice extraTypePrice, int totalDays)
    {
        var request = new ExtraRateCalculationRequestDto
        {
            TotalDays = totalDays,
            Quantity = 1, // Unit price calculation
            DailyPrice = extraTypePrice.DailyPrice,
            WeeklyPrice = extraTypePrice.WeeklyPrice,
            MonthlyPrice = extraTypePrice.MonthlyPrice
        };

        var result = _rateCalculationService.CalculateExtraRate(request);
        return result.TotalCost;
    }

    private string GenerateBookingNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"BK{timestamp}{random}";
    }

    private BookingDto MapToBookingDto(Booking booking)
    {
        var isArabic = _localizationService.GetCurrentCulture() == "ar";

        return new BookingDto
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            TotalDays = booking.TotalDays,
            CarRentalCost = booking.CarRentalCost,
            ExtrasCost = booking.ExtrasCost,
            TotalCost = booking.TotalCost,
            DiscountAmount = booking.DiscountAmount,
            FinalAmount = booking.FinalAmount,
            Status = booking.Status,
            Notes = booking.Notes,
            CancellationReason = booking.CancellationReason,
            CreatedAt = booking.CreatedAt,
            Car = new CarDto
            {
                Id = booking.Car.Id,
                Brand = isArabic ? booking.Car.BrandAr : booking.Car.BrandEn,
                Model = isArabic ? booking.Car.ModelAr : booking.Car.ModelEn,
                Year = booking.Car.Year,
                Color = isArabic ? booking.Car.ColorAr : booking.Car.ColorEn,
                SeatingCapacity = booking.Car.SeatingCapacity,
                TransmissionType = GetLocalizedTransmissionType(booking.Car.TransmissionType, isArabic),
                FuelType = booking.Car.FuelType,
                DailyPrice = booking.Car.DailyRate,
               
                Status = booking.Car.Status.GetDescription(),
                ImageUrl = booking.Car.ImageUrl
            },
            ReceivingBranch = new BranchDto
            {
                Id = booking.ReceivingBranch.Id,
                Name = isArabic ? booking.ReceivingBranch.NameAr : booking.ReceivingBranch.NameEn,
                Description = isArabic ? booking.ReceivingBranch.DescriptionAr : booking.ReceivingBranch.DescriptionEn,
                Address = booking.ReceivingBranch.Address,
                City = booking.ReceivingBranch.City,
                Country = booking.ReceivingBranch.Country,
                Phone = booking.ReceivingBranch.Phone,
                Email = booking.ReceivingBranch.Email,
                Latitude = booking.ReceivingBranch.Latitude,
                Longitude = booking.ReceivingBranch.Longitude,
                WorkingHours = booking.ReceivingBranch.WorkingHours
            },
            DeliveryBranch = new BranchDto
            {
                Id = booking.DeliveryBranch.Id,
                Name = isArabic ? booking.DeliveryBranch.NameAr : booking.DeliveryBranch.NameEn,
                Description = isArabic ? booking.DeliveryBranch.DescriptionAr : booking.DeliveryBranch.DescriptionEn,
                Address = booking.DeliveryBranch.Address,
                City = booking.DeliveryBranch.City,
                Country = booking.DeliveryBranch.Country,
                Phone = booking.DeliveryBranch.Phone,
                Email = booking.DeliveryBranch.Email,
                Latitude = booking.DeliveryBranch.Latitude,
                Longitude = booking.DeliveryBranch.Longitude,
                WorkingHours = booking.DeliveryBranch.WorkingHours
            },
            Extras = booking.BookingExtras.Select(be => new BookingExtraDto
            {
                Id = be.Id,
                ExtraName = isArabic ? be.ExtraTypePrice.NameAr : be.ExtraTypePrice.NameEn,
                Quantity = be.Quantity,
                UnitPrice = be.UnitPrice,
                TotalPrice = be.TotalPrice
            }).ToList()
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