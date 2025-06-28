using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface IBookingRepository
{
    Task<List<Booking>> GetUserBookingsAsync(string userId);
    Task<List<Booking>> GetUserBookingsByStatusAsync(string userId, BookingStatus status);
    Task<Booking?> GetUserBookingByIdAsync(int bookingId, string userId);
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<Booking> CreateBookingAsync(Booking booking);
    Task<Booking> UpdateBookingAsync(Booking booking);
    Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate, int? excludeBookingId = null);
    Task<bool> IsBranchAvailableAsync(int branchId);
    Task<Car?> GetCarByIdAsync(int carId);
    Task<List<ExtraTypePrice>> GetExtraTypePricesByIdsAsync(List<int> ids);
    Task<List<ExtraTypePrice>> GetExtraTypePricesAsync(List<int> extraTypePriceIds);
    Task AddBookingExtrasAsync(List<BookingExtra> bookingExtras);
} 