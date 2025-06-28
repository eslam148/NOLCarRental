using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.Application.Common.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<List<BookingDto>>> GetUserBookingsAsync(string userId);
    Task<ApiResponse<BookingDto>> GetBookingByIdAsync(int id);
    Task<ApiResponse<BookingDto>> CreateBookingAsync(CreateBookingDto createDto, string userId);
    Task<ApiResponse<BookingDto>> CancelBookingAsync(int bookingId,  string userId);
} 