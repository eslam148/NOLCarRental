using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<List<BookingDto>>> GetUserBookingsAsync(string userId);
    Task<ApiResponse<List<BookingDto>>> GetUserBookingsByStatusAsync(string userId, BookingStatus status);
    Task<ApiResponse<BookingDto>> GetBookingByIdAsync(int id);
    Task<ApiResponse<BookingDto>> CreateBookingAsync(CreateBookingDto createDto, string userId);
    Task<ApiResponse<BookingDto>> CancelBookingAsync(int bookingId, string userId);
} 