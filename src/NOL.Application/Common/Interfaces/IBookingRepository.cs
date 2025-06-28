using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface IBookingRepository
{
    Task<List<Booking>> GetUserBookingsAsync(string userId);
    Task<Booking?> GetUserBookingByIdAsync(int bookingId, string userId);
    Task<Booking> CreateBookingAsync(Booking booking);
    Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate);
    Task<Car?> GetCarByIdAsync(int carId);
    Task<List<ExtraTypePrice>> GetExtraTypePricesByIdsAsync(List<int> ids);
    Task AddBookingExtrasAsync(List<BookingExtra> bookingExtras);
    Task UpdateBookingAsync(Booking booking);
} 