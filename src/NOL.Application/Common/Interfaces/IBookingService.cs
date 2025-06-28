using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.Application.Common.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<List<BookingDto>>> GetUserBookingsAsync(string userId);
    Task<ApiResponse<BookingDto>> GetUserBookingByIdAsync(int bookingId, string userId);
    Task<ApiResponse<BookingDto>> CreateBookingAsync(CreateBookingDto createBookingDto, string userId);
} 