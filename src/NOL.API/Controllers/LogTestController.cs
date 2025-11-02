using Microsoft.AspNetCore.Mvc;
using NOL.API.Services;
using NOL.Domain.Enums;

namespace NOL.API.Controllers;

/// <summary>
/// Test controller to verify Seq logging and exception handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LogTestController : ControllerBase
{
    private readonly ILogger<LogTestController> _logger;

    public LogTestController(ILogger<LogTestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Test 1: Simple information log
    /// </summary>
    [HttpGet("test-info")]
    public IActionResult TestInfoLog()
    {
        _logger.LogInformation("This is a test information log");
        _logger.LogInformation("User {UserId} accessed test endpoint at {Time}", 
            "test-user-123", DateTime.UtcNow);
        
        return Ok(new
        {
            message = "Information log sent to Seq",
            checkSeq = "https://seq-production-43df.up.railway.app/"
        });
    }

    /// <summary>
    /// Test 2: User activity logging
    /// </summary>
    [HttpGet("test-user-activity")]
    public IActionResult TestUserActivity()
    {
        _logger.LogUserActivity("user-123", "Login", "From web browser");
        _logger.LogUserActivity("user-456", "Logout", "Session expired");
        
        return Ok(new
        {
            message = "User activity logs sent to Seq",
            logged = new[] { "Login", "Logout" }
        });
    }

    /// <summary>
    /// Test 3: Booking activity logging
    /// </summary>
    [HttpGet("test-booking-activity")]
    public IActionResult TestBookingActivity()
    {
        _logger.LogBookingActivity(1001, "Created", "user-123", "Weekend rental");
        _logger.LogBookingActivity(1001, "Confirmed", "user-123", "Payment successful");
        _logger.LogBookingActivity(1001, "Completed", "user-123", "Car returned");
        
        return Ok(new
        {
            message = "Booking activity logs sent to Seq",
            bookingId = 1001
        });
    }

    /// <summary>
    /// Test 4: Payment activity logging
    /// </summary>
    [HttpGet("test-payment-activity")]
    public IActionResult TestPaymentActivity()
    {
        _logger.LogPaymentActivity(1001, 500.00m, "CreditCard", "Success");
        _logger.LogPaymentActivity(1002, 750.50m, "ApplePay", "Success");
        _logger.LogPaymentActivity(1003, 300.00m, "Cash", "Pending");
        
        return Ok(new
        {
            message = "Payment activity logs sent to Seq",
            payments = 3
        });
    }

    /// <summary>
    /// Test 5: Car activity logging
    /// </summary>
    [HttpGet("test-car-activity")]
    public IActionResult TestCarActivity()
    {
        _logger.LogCarActivity(501, "Rented", "Booking #1001");
        _logger.LogCarActivity(502, "Maintenance", "Scheduled service");
        _logger.LogCarActivity(503, "Available", "Ready for rental");
        
        return Ok(new
        {
            message = "Car activity logs sent to Seq",
            cars = 3
        });
    }

    /// <summary>
    /// Test 6: Authentication logging
    /// </summary>
    [HttpGet("test-authentication")]
    public IActionResult TestAuthentication()
    {
        _logger.LogAuthentication("user@example.com", "Login", true, "192.168.1.100");
        _logger.LogAuthentication("hacker@example.com", "Login", false, "10.0.0.1");
        _logger.LogAuthentication("admin@nol.com", "Login", true, "172.16.0.1");
        
        return Ok(new
        {
            message = "Authentication logs sent to Seq",
            successful = 2,
            failed = 1
        });
    }

    /// <summary>
    /// Test 7: Warning log
    /// </summary>
    [HttpGet("test-warning")]
    public IActionResult TestWarning()
    {
        _logger.LogWarning("Low car availability for category {CategoryId}", 5);
        _logger.LogBusinessRuleViolation("MinimumRentalDays", "Rental must be at least 1 day");
        
        return Ok(new
        {
            message = "Warning logs sent to Seq",
            level = "Warning"
        });
    }

    /// <summary>
    /// Test 8: Performance logging
    /// </summary>
    [HttpGet("test-performance")]
    public IActionResult TestPerformance()
    {
        // Simulate fast operation
        _logger.LogPerformanceMetric("Fast Query", 150);
        
        // Simulate slow operation
        _logger.LogPerformanceMetric("Slow Query", 2500, "Complex join query");
        
        return Ok(new
        {
            message = "Performance logs sent to Seq",
            metrics = new[] { "Fast: 150ms", "Slow: 2500ms" }
        });
    }

    /// <summary>
    /// Test 9: Structured logging with object
    /// </summary>
    [HttpGet("test-structured")]
    public IActionResult TestStructuredLogging()
    {
        var booking = new
        {
            Id = 1001,
            UserId = "user-123",
            CarId = 501,
            Status = BookingStatus.Confirmed,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            TotalAmount = 500.00m
        };

        _logger.LogInformation("Booking created: {@Booking}", booking);
        
        return Ok(new
        {
            message = "Structured log sent to Seq",
            booking = booking
        });
    }

    /// <summary>
    /// Test 10: Exception handling - KeyNotFoundException (404)
    /// </summary>
    [HttpGet("test-exception-notfound")]
    public IActionResult TestNotFoundException()
    {
        _logger.LogInformation("About to throw KeyNotFoundException");
        throw new KeyNotFoundException("Booking with ID 999 not found");
    }

    /// <summary>
    /// Test 11: Exception handling - ArgumentException (400)
    /// </summary>
    [HttpGet("test-exception-badrequest")]
    public IActionResult TestBadRequestException()
    {
        _logger.LogInformation("About to throw ArgumentException");
        throw new ArgumentException("Invalid booking date: End date must be after start date");
    }

    /// <summary>
    /// Test 12: Exception handling - UnauthorizedAccessException (401)
    /// </summary>
    [HttpGet("test-exception-unauthorized")]
    public IActionResult TestUnauthorizedException()
    {
        _logger.LogInformation("About to throw UnauthorizedAccessException");
        throw new UnauthorizedAccessException("User is not authorized to access this resource");
    }

    /// <summary>
    /// Test 13: Exception handling - InvalidOperationException (409)
    /// </summary>
    [HttpGet("test-exception-conflict")]
    public IActionResult TestConflictException()
    {
        _logger.LogInformation("About to throw InvalidOperationException");
        throw new InvalidOperationException("Car is already rented for the selected dates");
    }

    /// <summary>
    /// Test 14: Exception handling - General exception (500)
    /// </summary>
    [HttpGet("test-exception-servererror")]
    public IActionResult TestServerErrorException()
    {
        _logger.LogInformation("About to throw general Exception");
        throw new Exception("Unexpected error occurred while processing booking");
    }

    /// <summary>
    /// Test 15: All log levels
    /// </summary>
    [HttpGet("test-all-levels")]
    public IActionResult TestAllLogLevels()
    {
        _logger.LogTrace("This is a TRACE log (very detailed)");
        _logger.LogDebug("This is a DEBUG log (development info)");
        _logger.LogInformation("This is an INFORMATION log (general flow)");
        _logger.LogWarning("This is a WARNING log (unexpected but handled)");
        _logger.LogError("This is an ERROR log (error condition)");
        _logger.LogCritical("This is a CRITICAL log (critical failure)");
        
        return Ok(new
        {
            message = "All log levels sent to Seq",
            levels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" }
        });
    }

    /// <summary>
    /// Test 16: Multiple logs with context
    /// </summary>
    [HttpGet("test-complete-flow")]
    public IActionResult TestCompleteFlow()
    {
        var userId = "user-789";
        var bookingId = 2001;
        var carId = 601;

        _logger.LogInformation("Starting complete booking flow for user {UserId}", userId);

        // Step 1: User activity
        _logger.LogUserActivity(userId, "Initiated booking", "Selected car #" + carId);

        // Step 2: Car activity
        _logger.LogCarActivity(carId, "Reserved", $"Booking #{bookingId}");

        // Step 3: Database operation
        _logger.LogDatabaseOperation("Insert", "Booking", bookingId);

        // Step 4: Booking activity
        _logger.LogBookingActivity(bookingId, "Created", userId, "5-day rental");

        // Step 5: Payment
        _logger.LogPaymentActivity(bookingId, 800.00m, "CreditCard", "Success");

        // Step 6: External API call
        _logger.LogExternalApiCall("PaymentGateway", "/api/charge", true, 200);

        // Step 7: Final status
        _logger.LogBookingActivity(bookingId, "Confirmed", userId, "All steps completed");

        _logger.LogInformation("Completed booking flow successfully for user {UserId}", userId);

        return Ok(new
        {
            message = "Complete flow logged to Seq",
            userId = userId,
            bookingId = bookingId,
            steps = 7
        });
    }

    /// <summary>
    /// Test 17: Get current configuration
    /// </summary>
    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        return Ok(new
        {
            seqUrl = "https://seq-production-43df.up.railway.app/",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            machineName = Environment.MachineName,
            tests = new
            {
                info = "/api/logtest/test-info",
                userActivity = "/api/logtest/test-user-activity",
                bookingActivity = "/api/logtest/test-booking-activity",
                paymentActivity = "/api/logtest/test-payment-activity",
                carActivity = "/api/logtest/test-car-activity",
                authentication = "/api/logtest/test-authentication",
                warning = "/api/logtest/test-warning",
                performance = "/api/logtest/test-performance",
                structured = "/api/logtest/test-structured",
                exceptionNotFound = "/api/logtest/test-exception-notfound",
                exceptionBadRequest = "/api/logtest/test-exception-badrequest",
                exceptionUnauthorized = "/api/logtest/test-exception-unauthorized",
                exceptionConflict = "/api/logtest/test-exception-conflict",
                exceptionServerError = "/api/logtest/test-exception-servererror",
                allLevels = "/api/logtest/test-all-levels",
                completeFlow = "/api/logtest/test-complete-flow"
            }
        });
    }
}

