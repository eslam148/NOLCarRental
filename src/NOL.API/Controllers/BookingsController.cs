using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserBookings([FromQuery] BookingStatus? status = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = status.HasValue 
            ? await _bookingService.GetUserBookingsByStatusAsync(userId, status.Value)
            : await _bookingService.GetUserBookingsAsync(userId);
        
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        var result = await _bookingService.GetBookingByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto createBookingDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Log the incoming request body
        _logger.LogInformation("CreateBooking Request - UserId: {UserId}, RequestBody: {@CreateBookingDto}", 
            userId ?? "Anonymous", createBookingDto);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("CreateBooking - Unauthorized attempt. No UserId found in token");
            return Unauthorized();
        }

        try
        {
            _logger.LogInformation("Creating booking for User {UserId} - CarId: {CarId}, StartDate: {StartDate}, EndDate: {EndDate}", 
                userId, createBookingDto.CarId, createBookingDto.StartDate, createBookingDto.EndDate);
            
            var result = await _bookingService.CreateBookingAsync(createBookingDto, userId);
            
            _logger.LogInformation("Booking created successfully - BookingId: {BookingId}, UserId: {UserId}, CarId: {CarId}", 
                result.Id, userId, createBookingDto.CarId);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create booking for User {UserId} with request {@CreateBookingDto}", 
                userId, createBookingDto);
            throw; // Global exception handler will catch this
        }
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _bookingService.CancelBookingAsync(id, userId);
        return StatusCode(result.StatusCodeValue, result);
    }
} 