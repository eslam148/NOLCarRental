# 🚀 Quick Start - Seq Logging & Exception Handling

## ✅ What's Already Done

Your application now has:
- ✅ **Serilog** configured with Seq, Console, and File sinks
- ✅ **Global Exception Handler** middleware
- ✅ **Request logging** for all HTTP requests
- ✅ **Seq integration** with https://seq-production-43df.up.railway.app/
- ✅ **Custom logging extensions** for common scenarios
- ✅ **Automatic exception logging** with full context

## 🏃 Getting Started (3 Steps)

### Step 1: Restore Packages

```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental/src/NOL.API"
dotnet restore
```

### Step 2: Run the Application

```bash
dotnet run
```

### Step 3: View Logs in Seq

1. Open your browser
2. Go to: **https://seq-production-43df.up.railway.app/**
3. You'll see logs appearing in real-time! 🎉

---

## 📝 Quick Examples

### Example 1: Basic Logging in a Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly ILogger<CarsController> _logger;

    public CarsController(ILogger<CarsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCar(int id)
    {
        _logger.LogInformation("Fetching car with ID {CarId}", id);
        
        // Your code here
        
        return Ok(car);
    }
}
```

### Example 2: Logging with Exception

```csharp
[HttpPost]
public async Task<IActionResult> CreateBooking(BookingDto dto)
{
    try
    {
        _logger.LogInformation("Creating booking for user {UserId}", dto.UserId);
        
        var booking = await _service.CreateAsync(dto);
        
        _logger.LogBookingActivity(booking.Id, "Created", dto.UserId);
        
        return Ok(booking);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create booking for user {UserId}", dto.UserId);
        throw; // Global handler catches this
    }
}
```

### Example 3: Using Custom Extensions

```csharp
// User activity
_logger.LogUserActivity("user123", "Login", "From mobile app");

// Booking activity
_logger.LogBookingActivity(456, "Confirmed", "user123");

// Payment activity
_logger.LogPaymentActivity(456, 500.00m, "CreditCard", "Success");
```

---

## 🔍 Testing the Setup

### Test 1: View Request Logs

1. Make any API request (e.g., `GET /api/enums/booking-statuses`)
2. Open Seq: https://seq-production-43df.up.railway.app/
3. You'll see the request logged with:
   - HTTP Method
   - Path
   - Status Code
   - Response Time
   - User Info
   - IP Address

### Test 2: Test Exception Handling

Create a test endpoint:

```csharp
[HttpGet("test-exception")]
public IActionResult TestException()
{
    _logger.LogInformation("About to throw test exception");
    throw new InvalidOperationException("This is a test exception!");
}
```

1. Call this endpoint
2. Check Seq - you'll see:
   - Exception details
   - Stack trace
   - Request context
   - User information

---

## 📊 What You'll See in Seq

### Log Entry Example:

```json
{
  "@t": "2024-01-15T10:30:00.123Z",
  "@mt": "User {UserId} created booking {BookingId}",
  "@l": "Information",
  "UserId": "user123",
  "BookingId": 456,
  "Application": "NOL Car Rental API",
  "MachineName": "SERVER01",
  "SourceContext": "NOL.API.Controllers.BookingsController"
}
```

### Exception Entry Example:

```json
{
  "@t": "2024-01-15T10:30:00.123Z",
  "@mt": "Failed to create booking for user {UserId}",
  "@l": "Error",
  "@x": "System.InvalidOperationException: Car not available...",
  "UserId": "user123",
  "RequestPath": "/api/bookings",
  "RequestMethod": "POST",
  "RemoteIP": "192.168.1.100",
  "TraceId": "0HMVFE8A5KQ0M:00000001"
}
```

---

## 🔎 Useful Seq Queries

```sql
-- All errors from last hour
@Level = 'Error' and @Timestamp > Now() - 1h

-- Specific user activity
UserId = 'user123'

-- Slow requests (> 1 second)
Elapsed > 1000

-- Failed bookings
@Message like '%Booking%' and @Level = 'Error'

-- Authentication failures
@Message like '%Authentication%' and @Level = 'Warning'
```

---

## 🎯 Log Levels Quick Reference

```csharp
_logger.LogDebug("Debug info");         // Development details
_logger.LogInformation("Info");         // General flow
_logger.LogWarning("Warning");          // Unexpected but handled
_logger.LogError(ex, "Error");          // Errors & exceptions
_logger.LogCritical("Critical");        // Critical failures
```

---

## 🛡️ Exception Handling - How It Works

1. **Exception occurs** in your code
2. **Global middleware catches** it automatically
3. **Full details logged** to Seq with context
4. **Consistent error response** returned to client
5. **Developer sees** exception in Seq dashboard

**You don't need to handle exceptions everywhere - just throw them!**

---

## 📁 Files Created

1. ✅ `NOL.API.csproj` - Added Serilog packages
2. ✅ `appsettings.json` - Serilog configuration
3. ✅ `Program.cs` - Serilog setup & middleware
4. ✅ `Middleware/GlobalExceptionHandlerMiddleware.cs` - Exception handler
5. ✅ `Services/LoggingService.cs` - Custom logging extensions
6. ✅ `SEQ_LOGGING_GUIDE.md` - Complete guide
7. ✅ `QUICK_START_LOGGING.md` - This file

---

## ⚙️ Configuration

### Seq URL
```
https://seq-production-43df.up.railway.app/
```

### Log Locations

1. **Seq** - https://seq-production-43df.up.railway.app/
2. **Console** - Terminal output
3. **Files** - `logs/log-{date}.txt` (30 days retention)

### Change Log Level

Edit `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"  // Change to: Debug, Warning, Error
    }
  }
}
```

---

## 🎨 Customize Logging

### Add Custom Properties

```csharp
using Serilog.Context;

using (LogContext.PushProperty("TenantId", "tenant123"))
{
    _logger.LogInformation("User activity"); 
    // All logs in this scope will have TenantId = "tenant123"
}
```

### Log Performance

```csharp
var stopwatch = Stopwatch.StartNew();

// Your operation here

stopwatch.Stop();
_logger.LogPerformanceMetric("Database Query", stopwatch.ElapsedMilliseconds);
```

---

## 🚨 Troubleshooting

### Issue: Logs not appearing in Seq

**Check:**
1. ✅ Seq URL is correct in `appsettings.json`
2. ✅ Application is running
3. ✅ No firewall blocking the connection
4. ✅ Check console logs for Serilog errors

### Issue: Too many logs

**Solution:** Increase minimum log level

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"  // Only Warning, Error, Critical
    }
  }
}
```

### Issue: Performance impact

**Solution:** Log levels are optimized, but you can:
1. Reduce file retention days
2. Disable console logging in production
3. Use asynchronous logging (already enabled)

---

## 📖 Next Steps

1. ✅ Run your application
2. ✅ Open Seq dashboard
3. ✅ Make some API calls
4. ✅ Watch logs appear in real-time
5. ✅ Try searching and filtering
6. ✅ Add logging to your controllers/services
7. ✅ Test exception handling

---

## 📚 Full Documentation

For complete guide, see: `SEQ_LOGGING_GUIDE.md`

---

## 🎉 You're All Set!

Your API now has **enterprise-grade logging and exception handling**!

Every request, every error, every important event is logged and searchable in Seq.

**Start your app and watch the magic happen!** ✨

