using Microsoft.Extensions.Logging;

namespace NOL.API.Services;

/// <summary>
/// Helper service for structured logging throughout the application
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Log user activity
    /// </summary>
    public static void LogUserActivity<T>(this ILogger<T> logger, string userId, string action, string details = "")
    {
        logger.LogInformation("User Activity: {UserId} performed {Action}. {Details}",
            userId, action, details);
    }

    /// <summary>
    /// Log booking activity
    /// </summary>
    public static void LogBookingActivity<T>(this ILogger<T> logger, int bookingId, string action, string userId, string details = "")
    {
        logger.LogInformation("Booking Activity: Booking {BookingId} - {Action} by User {UserId}. {Details}",
            bookingId, action, userId, details);
    }

    /// <summary>
    /// Log payment activity
    /// </summary>
    public static void LogPaymentActivity<T>(this ILogger<T> logger, int bookingId, decimal amount, string paymentMethod, string status)
    {
        logger.LogInformation("Payment Activity: Booking {BookingId} - Amount {Amount} via {PaymentMethod} - Status: {Status}",
            bookingId, amount, paymentMethod, status);
    }

    /// <summary>
    /// Log car activity
    /// </summary>
    public static void LogCarActivity<T>(this ILogger<T> logger, int carId, string action, string details = "")
    {
        logger.LogInformation("Car Activity: Car {CarId} - {Action}. {Details}",
            carId, action, details);
    }

    /// <summary>
    /// Log authentication activity
    /// </summary>
    public static void LogAuthentication<T>(this ILogger<T> logger, string email, string action, bool success, string ipAddress = "")
    {
        if (success)
        {
            logger.LogInformation("Authentication: {Action} successful for {Email} from {IPAddress}",
                action, email, ipAddress);
        }
        else
        {
            logger.LogWarning("Authentication: {Action} failed for {Email} from {IPAddress}",
                action, email, ipAddress);
        }
    }

    /// <summary>
    /// Log database operation
    /// </summary>
    public static void LogDatabaseOperation<T>(this ILogger<T> logger, string operation, string entity, object entityId)
    {
        logger.LogInformation("Database: {Operation} {Entity} with ID {EntityId}",
            operation, entity, entityId);
    }

    /// <summary>
    /// Log API call to external service
    /// </summary>
    public static void LogExternalApiCall<T>(this ILogger<T> logger, string serviceName, string endpoint, bool success, int? statusCode = null)
    {
        logger.LogInformation("External API: Called {ServiceName} - {Endpoint} - Success: {Success} - StatusCode: {StatusCode}",
            serviceName, endpoint, success, statusCode);
    }

    /// <summary>
    /// Log business rule violation
    /// </summary>
    public static void LogBusinessRuleViolation<T>(this ILogger<T> logger, string ruleName, string details)
    {
        logger.LogWarning("Business Rule Violation: {RuleName} - {Details}",
            ruleName, details);
    }

    /// <summary>
    /// Log performance metric
    /// </summary>
    public static void LogPerformanceMetric<T>(this ILogger<T> logger, string operation, long milliseconds, string details = "")
    {
        if (milliseconds > 1000)
        {
            logger.LogWarning("Performance: {Operation} took {Milliseconds}ms - {Details}",
                operation, milliseconds, details);
        }
        else
        {
            logger.LogDebug("Performance: {Operation} took {Milliseconds}ms",
                operation, milliseconds);
        }
    }
}

/// <summary>
/// Examples of how to use logging in different scenarios
/// </summary>
public class LoggingExamples
{
    private readonly ILogger<LoggingExamples> _logger;

    public LoggingExamples(ILogger<LoggingExamples> logger)
    {
        _logger = logger;
    }

    public void ExampleLogging()
    {
        // 1. Simple information log
        _logger.LogInformation("User registered successfully");

        // 2. Log with structured data
        _logger.LogInformation("User {UserId} created booking {BookingId}", 
            "user123", 456);

        // 3. Log with object
        var booking = new { Id = 123, Status = "Confirmed" };
        _logger.LogInformation("Booking created: {@Booking}", booking);

        // 4. Warning log
        _logger.LogWarning("Car {CarId} has low availability", 789);

        // 5. Error log with exception
        try
        {
            throw new InvalidOperationException("Test error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process booking {BookingId}", 123);
        }

        // 6. Using custom extensions
        _logger.LogUserActivity("user123", "Login", "From mobile app");
        _logger.LogBookingActivity(456, "Created", "user123", "Weekend rental");
        _logger.LogPaymentActivity(456, 500.00m, "CreditCard", "Success");
    }
}

