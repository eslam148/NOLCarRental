namespace NOL.Application.Common.Interfaces;

/// <summary>
/// Service for managing booking lifecycle and cleanup operations
/// </summary>
public interface IBookingCleanupService
{
    /// <summary>
    /// Closes bookings that have ended and updates car availability
    /// </summary>
    /// <returns>Number of bookings closed</returns>
    Task<int> CloseEndedBookingsAsync();
}

