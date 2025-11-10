using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces;

namespace NOL.Application.Hangfire;

/// <summary>
/// Hangfire job that automatically closes bookings that have ended
/// Runs periodically to update booking and car statuses
/// </summary>
public class EndBookingSchedulJob
{
    private readonly IBookingCleanupService _bookingCleanupService;
    private readonly ILogger<EndBookingSchedulJob> _logger;

    public EndBookingSchedulJob(
        IBookingCleanupService bookingCleanupService,
        ILogger<EndBookingSchedulJob> logger)
    {
        _bookingCleanupService = bookingCleanupService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the job to close ended bookings and update car availability
    /// </summary>
    public async Task Execute()
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("EndBookingSchedulJob started at {StartTime}", startTime);

        try
        {
            var closedBookingsCount = await _bookingCleanupService.CloseEndedBookingsAsync();

            var duration = (DateTime.UtcNow - startTime).TotalSeconds;
            _logger.LogInformation(
                "EndBookingSchedulJob completed successfully in {Duration:F2}s. Closed {Count} bookings",
                duration, closedBookingsCount);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalSeconds;
            _logger.LogError(ex, 
                "EndBookingSchedulJob failed after {Duration:F2}s: {Message}", 
                duration, ex.Message);
            throw;
        }
    }
}