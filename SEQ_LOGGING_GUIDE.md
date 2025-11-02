# 📊 Seq Logging & Exception Handling Guide

## Overview

Your NOL Car Rental API now has **comprehensive logging** with [Seq](https://datalust.co/seq) and **global exception handling**.

**Seq URL**: https://seq-production-43df.up.railway.app/

---

## 🎯 What's Configured

### ✅ 1. Serilog with Multiple Sinks

Logs are sent to **3 destinations**:

1. **Console** - For local development
2. **File** - Stored in `logs/` folder (30 days retention)
3. **Seq** - Centralized log server with search & analysis

### ✅ 2. Global Exception Handler

All unhandled exceptions are:
- ✅ Caught automatically
- ✅ Logged to Seq with full context
- ✅ Returned as consistent error responses
- ✅ Include request details (IP, user, path, etc.)

### ✅ 3. Request Logging

Every HTTP request is logged with:
- Method, Path, Status Code
- Response time
- User information
- IP address
- User agent

---

## 🚀 Quick Start

### View Logs in Seq

1. Open: https://seq-production-43df.up.railway.app/
2. You'll see all logs from your API in real-time
3. Use the search bar to filter logs

### Common Searches

```sql
-- All errors
@Level = 'Error'

-- Specific user activity
UserId = 'user123'

-- Slow requests (> 1 second)
Elapsed > 1000

-- Failed authentication
@Message like '%Authentication%' and @Level = 'Warning'

-- Booking operations
@Message like '%Booking%'

-- Today's errors
@Timestamp > Now() - 1d and @Level = 'Error'
```

---

## 💡 How to Use Logging in Your Code

### 1. **In Controllers**

```csharp
[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly ILogger<BookingsController> _logger;
    private readonly IBookingService _bookingService;

    public BookingsController(
        ILogger<BookingsController> logger,
        IBookingService bookingService)
    {
        _logger = logger;
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking(BookingDto dto)
    {
        try
        {
            _logger.LogInformation("Creating booking for user {UserId} - Car {CarId}", 
                dto.UserId, dto.CarId);

            var booking = await _bookingService.CreateAsync(dto);

            _logger.LogBookingActivity(booking.Id, "Created", dto.UserId, 
                $"From {dto.StartDate} to {dto.EndDate}");

            return Ok(booking);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create booking for user {UserId}", dto.UserId);
            throw; // Global handler will catch this
        }
    }
}
```

### 2. **In Services**

```csharp
public class BookingService : IBookingService
{
    private readonly ILogger<BookingService> _logger;
    private readonly IBookingRepository _repository;

    public async Task<Booking> CreateAsync(BookingDto dto)
    {
        _logger.LogInformation("Processing booking creation for user {UserId}", dto.UserId);

        // Business logic here

        _logger.LogDatabaseOperation("Create", "Booking", booking.Id);

        return booking;
    }
}
```

### 3. **Custom Logging Extensions**

We've created helper methods for common scenarios:

```csharp
// User activity
_logger.LogUserActivity("user123", "Login", "From mobile app");

// Booking activity
_logger.LogBookingActivity(456, "Confirmed", "user123", "Payment successful");

// Payment activity
_logger.LogPaymentActivity(456, 500.00m, "CreditCard", "Success");

// Car activity
_logger.LogCarActivity(789, "Rented", "Booking #456");

// Authentication
_logger.LogAuthentication("user@example.com", "Login", true, "192.168.1.1");

// External API calls
_logger.LogExternalApiCall("PaymentGateway", "/api/charge", true, 200);

// Business rule violations
_logger.LogBusinessRuleViolation("MinimumRentalDays", "Rental must be at least 1 day");

// Performance metrics
_logger.LogPerformanceMetric("Database Query", 1500, "Slow query detected");
```

### 4. **Structured Logging**

Always use structured logging with placeholders:

```csharp
// ✅ GOOD - Structured (searchable in Seq)
_logger.LogInformation("User {UserId} booked car {CarId} for {Days} days", 
    userId, carId, days);

// ❌ BAD - String interpolation (not searchable)
_logger.LogInformation($"User {userId} booked car {carId} for {days} days");
```

### 5. **Logging Objects**

Use `@` prefix to log entire objects:

```csharp
var booking = new Booking { Id = 123, Status = BookingStatus.Confirmed };

// Log object structure
_logger.LogInformation("Booking created: {@Booking}", booking);

// This will show in Seq as:
// {
//   "Id": 123,
//   "Status": "Confirmed",
//   "CarId": 456
// }
```

---

## 🔍 Log Levels

Use appropriate log levels:

| Level | When to Use | Example |
|-------|-------------|---------|
| `LogDebug` | Development details | "Entering method X with params Y" |
| `LogInformation` | General flow | "User logged in", "Booking created" |
| `LogWarning` | Unexpected but handled | "Car availability low", "Retry attempt 3/5" |
| `LogError` | Errors, exceptions | "Failed to process payment" |
| `LogCritical` | Critical failures | "Database connection lost" |

```csharp
_logger.LogDebug("Cache hit for key {Key}", cacheKey);
_logger.LogInformation("User {UserId} registered successfully", userId);
_logger.LogWarning("Car {CarId} has only {Count} days until maintenance", carId, days);
_logger.LogError(ex, "Payment processing failed for booking {BookingId}", bookingId);
_logger.LogCritical("Database connection pool exhausted");
```

---

## 🛡️ Exception Handling

### Automatic Exception Handling

All exceptions are automatically caught by the `GlobalExceptionHandlerMiddleware`:

```csharp
// You just throw exceptions - middleware handles logging
public async Task<Booking> GetBooking(int id)
{
    var booking = await _repository.GetByIdAsync(id);
    
    if (booking == null)
        throw new KeyNotFoundException($"Booking {id} not found");
    
    return booking;
}
```

**What happens:**
1. Exception is caught by middleware
2. Full details logged to Seq
3. Appropriate HTTP status code returned
4. Consistent error response sent to client

### Exception Types & HTTP Status Codes

| Exception Type | HTTP Status | Response |
|----------------|-------------|----------|
| `ArgumentNullException` | 400 Bad Request | Invalid Request |
| `ArgumentException` | 400 Bad Request | Invalid Request |
| `UnauthorizedAccessException` | 401 Unauthorized | Unauthorized |
| `KeyNotFoundException` | 404 Not Found | Resource Not Found |
| `InvalidOperationException` | 409 Conflict | Operation Not Valid |
| `NotImplementedException` | 501 Not Implemented | Feature Not Implemented |
| `TimeoutException` | 408 Request Timeout | Request Timeout |
| Any other exception | 500 Internal Server Error | Internal Server Error |

### Error Response Format

```json
{
  "status": 404,
  "title": "Resource Not Found",
  "detail": "Booking with ID 123 not found",
  "instance": "/api/bookings/123",
  "type": "https://httpstatuses.com/404",
  "traceId": "0HMVFE8A5KQ0M:00000001",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**In Development**, additional details are included:
```json
{
  "exception": "KeyNotFoundException",
  "stackTrace": "at NOL.Application...",
  "innerException": "..."
}
```

---

## 📊 What Gets Logged to Seq

### Automatic Logs

1. **All HTTP Requests**
   - Method, Path, Status Code
   - Response time
   - User, IP address
   - User agent

2. **All Exceptions**
   - Exception type & message
   - Stack trace
   - Request details
   - User context

3. **Application Lifecycle**
   - Application start/stop
   - Configuration changes
   - Database operations

### Custom Logs

Anything you explicitly log using `ILogger`:

```csharp
_logger.LogInformation("Custom event happened");
```

---

## 🔧 Configuration

### appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "https://seq-production-43df.up.railway.app/",
          "apiKey": "",
          "restrictedToMinimumLevel": "Information"
        }
      }
    ]
  }
}
```

### Add API Key (Optional)

If your Seq instance requires authentication:

1. Get API key from Seq dashboard
2. Add to appsettings.json:
   ```json
   "apiKey": "your-api-key-here"
   ```

---

## 🎯 Best Practices

### ✅ DO

```csharp
// Use structured logging
_logger.LogInformation("User {UserId} created booking {BookingId}", userId, bookingId);

// Log important business events
_logger.LogBookingActivity(id, "Confirmed", userId);

// Include context in exceptions
throw new InvalidOperationException($"Car {carId} is not available");

// Log at appropriate levels
_logger.LogWarning("Retry attempt {Attempt} of {MaxAttempts}", attempt, max);
```

### ❌ DON'T

```csharp
// Don't use string interpolation
_logger.LogInformation($"User {userId} did something"); // BAD

// Don't log sensitive data
_logger.LogInformation("Password: {Password}", password); // NEVER!

// Don't over-log in loops
for (int i = 0; i < 10000; i++)
{
    _logger.LogInformation("Processing item {I}", i); // BAD
}

// Don't swallow exceptions
catch (Exception ex)
{
    // Silent fail - BAD
}
```

---

## 📈 Monitoring & Alerts

### Key Metrics to Monitor in Seq

1. **Error Rate**
   ```sql
   @Level = 'Error' | group by hour
   ```

2. **Slow Requests**
   ```sql
   Elapsed > 1000
   ```

3. **Failed Authentication**
   ```sql
   @Message like '%Authentication%' and @Level = 'Warning'
   ```

4. **Payment Failures**
   ```sql
   @Message like '%Payment%' and @Level = 'Error'
   ```

---

## 🔗 Related Files

- **Middleware**: `NOL.API/Middleware/GlobalExceptionHandlerMiddleware.cs`
- **Logging Extensions**: `NOL.API/Services/LoggingService.cs`
- **Configuration**: `appsettings.json`
- **Program.cs**: Serilog setup

---

## 🚦 Testing Logging

### Test Exception Handling

Create a test endpoint to verify exceptions are logged:

```csharp
[HttpGet("test-error")]
public IActionResult TestError()
{
    throw new InvalidOperationException("This is a test error");
}
```

Visit: `GET /api/test/test-error`

Check Seq to see the error logged with full context!

---

## 📚 Resources

- **Seq Documentation**: https://docs.datalust.co/docs
- **Serilog Documentation**: https://serilog.net/
- **Your Seq Instance**: https://seq-production-43df.up.railway.app/

---

## 💡 Pro Tips

1. **Search by TraceId** - Every request has a unique TraceId for tracking
2. **Use Tags** - Add tags to group related logs
3. **Create Dashboards** - Build custom dashboards in Seq
4. **Set up Alerts** - Configure email/Slack alerts for errors
5. **Filter Noise** - Exclude noisy logs in Seq filters

