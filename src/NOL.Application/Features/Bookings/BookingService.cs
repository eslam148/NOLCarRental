using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Features.Bookings;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public BookingService(
        IBookingRepository bookingRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _bookingRepository = bookingRepository;
        _responseService = responseService;
        _localizationService = localizationService;
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

    public async Task<ApiResponse<BookingDto>> GetUserBookingByIdAsync(int bookingId, string userId)
    {
        try
        {
            var booking = await _bookingRepository.GetUserBookingByIdAsync(bookingId, userId);

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

    public async Task<ApiResponse<BookingDto>> CreateBookingAsync(CreateBookingDto createBookingDto, string userId)
    {
        try
        {
            // Validate car availability
            var isCarAvailable = await _bookingRepository.IsCarAvailableAsync(
                createBookingDto.CarId, 
                createBookingDto.StartDate, 
                createBookingDto.EndDate);

            if (!isCarAvailable)
            {
                return _responseService.Error<BookingDto>("CarNotAvailable");
            }

            var car = await _bookingRepository.GetCarByIdAsync(createBookingDto.CarId);
            if (car == null)
            {
                return _responseService.NotFound<BookingDto>("CarNotFound");
            }

            // Calculate booking cost
            var totalDays = (decimal)(createBookingDto.EndDate - createBookingDto.StartDate).TotalDays;
            if (totalDays <= 0)
            {
                return _responseService.Error<BookingDto>("InvalidDateRange");
            }

            var carRentalCost = CalculateCarRentalCost(car, totalDays);
            var bookingNumber = GenerateBookingNumber();

            // Create booking entity
            var booking = new Booking
            {
                BookingNumber = bookingNumber,
                StartDate = createBookingDto.StartDate,
                EndDate = createBookingDto.EndDate,
                TotalDays = totalDays,
                CarRentalCost = carRentalCost,
                ExtrasCost = 0,
                TotalCost = carRentalCost,
                DiscountAmount = 0,
                FinalAmount = carRentalCost,
                Status = BookingStatus.Open,
                Notes = createBookingDto.Notes,
                UserId = userId,
                CarId = createBookingDto.CarId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create the booking
            var createdBooking = await _bookingRepository.CreateBookingAsync(booking);

            // Handle extras if any
            if (createBookingDto.Extras.Any())
            {
                var extrasCost = await ProcessBookingExtrasAsync(createdBooking.Id, createBookingDto.Extras, totalDays);
                
                createdBooking.ExtrasCost = extrasCost;
                createdBooking.TotalCost = carRentalCost + extrasCost;
                createdBooking.FinalAmount = createdBooking.TotalCost - createdBooking.DiscountAmount;
                createdBooking.UpdatedAt = DateTime.UtcNow;

                await _bookingRepository.UpdateBookingAsync(createdBooking);
            }

            return _responseService.Success<BookingDto>(null!, "BookingCreated");
        }
        catch (Exception)
        {
            return _responseService.Error<BookingDto>("InternalServerError");
        }
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
            CreatedAt = booking.CreatedAt,
            Car = new CarDto
            {
                Id = booking.Car.Id,
                Brand = isArabic ? booking.Car.BrandAr : booking.Car.BrandEn,
                Model = isArabic ? booking.Car.ModelAr : booking.Car.ModelEn,
                Year = booking.Car.Year,
                Color = booking.Car.Color,
                SeatingCapacity = booking.Car.SeatingCapacity,
                TransmissionType = booking.Car.TransmissionType,
                FuelType = booking.Car.FuelType,
                DailyRate = booking.Car.DailyRate,
                WeeklyRate = booking.Car.WeeklyRate,
                MonthlyRate = booking.Car.MonthlyRate,
                Status = booking.Car.Status,
                ImageUrl = booking.Car.ImageUrl
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

    private decimal CalculateCarRentalCost(Car car, decimal totalDays)
    {
        // Business logic for calculating rental cost
        // Could implement weekly/monthly discounts here
        if (totalDays >= 30)
        {
            return car.MonthlyRate * (totalDays / 30);
        }
        else if (totalDays >= 7)
        {
            return car.WeeklyRate * (totalDays / 7);
        }
        else
        {
            return car.DailyRate * totalDays;
        }
    }

    private string GenerateBookingNumber()
    {
        return $"NOL-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
    }

    private async Task<decimal> ProcessBookingExtrasAsync(int bookingId, List<BookingExtraRequestDto> extrasRequest, decimal totalDays)
    {
        var extraTypePriceIds = extrasRequest.Select(e => e.ExtraTypePriceId).ToList();
        var extraTypePrices = await _bookingRepository.GetExtraTypePricesByIdsAsync(extraTypePriceIds);

        var bookingExtras = new List<BookingExtra>();
        decimal totalExtrasCost = 0;

        foreach (var extraRequest in extrasRequest)
        {
            var extraTypePrice = extraTypePrices.FirstOrDefault(etp => etp.Id == extraRequest.ExtraTypePriceId);
            if (extraTypePrice != null)
            {
                var unitPrice = extraTypePrice.DailyPrice;
                var totalPrice = unitPrice * extraRequest.Quantity * totalDays;
                totalExtrasCost += totalPrice;

                var bookingExtra = new BookingExtra
                {
                    BookingId = bookingId,
                    ExtraTypePriceId = extraRequest.ExtraTypePriceId,
                    Quantity = extraRequest.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    CreatedAt = DateTime.UtcNow
                };

                bookingExtras.Add(bookingExtra);
            }
        }

        if (bookingExtras.Any())
        {
            await _bookingRepository.AddBookingExtrasAsync(bookingExtras);
        }

        return totalExtrasCost;
    }
} 