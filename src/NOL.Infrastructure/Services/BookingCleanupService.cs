using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Services;

/// <summary>
/// Service for managing booking lifecycle and cleanup operations
/// Handles closing ended bookings and updating car availability
/// </summary>
public class BookingCleanupService : IBookingCleanupService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BookingCleanupService> _logger;

    public BookingCleanupService(
        ApplicationDbContext context,
        ILogger<BookingCleanupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Closes bookings that have ended and updates car availability
    /// </summary>
    /// <returns>Number of bookings closed</returns>
    public async Task<int> CloseEndedBookingsAsync()
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation("Starting to close ended bookings at {Time}", now);

        try
        {
            // Find bookings that have ended and are still in InProgress status
            var endedBookings = await _context.Bookings
                .Where(b => b.EndDate < now && b.Status < BookingStatus.InProgress)
                .Select(b => new { b.Id, b.CarId, b.BookingNumber })
                .ToListAsync();

            if (!endedBookings.Any())
            {
                _logger.LogInformation("No bookings to close");
                return 0;
            }

            _logger.LogInformation("Found {Count} bookings to close", endedBookings.Count);

            // Update booking statuses using efficient bulk update
            var updatedBookingsCount = await _context.Bookings
                .Where(b => b.EndDate < now && b.Status == BookingStatus.InProgress)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(b => b.Status, BookingStatus.Closed)
                    .SetProperty(b => b.UpdatedAt, DateTime.UtcNow));

            _logger.LogInformation("Successfully closed {Count} bookings", updatedBookingsCount);

            // Update car availability
            await UpdateCarAvailabilityAsync(endedBookings.Select(b => b.CarId).Distinct().ToList());

            return updatedBookingsCount;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, 
                "Database error occurred while closing ended bookings: {Message}", 
                dbEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unexpected error occurred while closing ended bookings: {Message}", 
                ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Updates car statuses to Available if they have no active bookings
    /// </summary>
    private async Task UpdateCarAvailabilityAsync(List<int> carIds)
    {
        if (!carIds.Any())
        {
            return;
        }

        _logger.LogInformation("Checking availability for {Count} cars", carIds.Count);

        var carsUpdated = 0;

        foreach (var carId in carIds)
        {
            // Check if the car has any active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.CarId == carId && 
                              (b.Status == BookingStatus.Open || 
                               b.Status == BookingStatus.Confirmed || 
                               b.Status == BookingStatus.InProgress));

            if (!hasActiveBookings)
            {
                // Update car status to Available
                var updated = await _context.Cars
                    .Where(c => c.Id == carId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(c => c.Status, CarStatus.Available));

                if (updated > 0)
                {
                    carsUpdated++;
                    _logger.LogInformation("Car {CarId} status updated to Available", carId);
                }
            }
        }

        _logger.LogInformation("Updated {Count} cars to Available status", carsUpdated);
    }
}

