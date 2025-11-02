# 🧪 How to Test Seq Logging & Exception Handling

## 🚀 Quick Start (3 Steps)

### Step 1: Start the Application

Open terminal and run:

```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental/src/NOL.API"
dotnet restore
dotnet run
```

**Wait for this message:**
```
Now listening on: https://localhost:7001
```

### Step 2: Open Seq Dashboard

Open in your browser:
```
https://seq-production-43df.up.railway.app/
```

**Keep this tab open** - you'll see logs appear in real-time!

### Step 3: Run Tests

Open the file: `TEST_SEQ_LOGGING.http`

Click **"Send Request"** above any test to execute it.

---

## 📋 Available Tests

### ✅ Basic Logging Tests

| Test | Endpoint | What it does |
|------|----------|--------------|
| 1 | `/api/logtest/test-info` | Simple information log |
| 2 | `/api/logtest/test-user-activity` | User activity logging |
| 3 | `/api/logtest/test-booking-activity` | Booking activity logging |
| 4 | `/api/logtest/test-payment-activity` | Payment activity logging |
| 5 | `/api/logtest/test-car-activity` | Car activity logging |
| 6 | `/api/logtest/test-authentication` | Authentication logging |
| 7 | `/api/logtest/test-warning` | Warning logs |
| 8 | `/api/logtest/test-performance` | Performance metrics |
| 9 | `/api/logtest/test-structured` | Structured object logging |
| 10 | `/api/logtest/test-all-levels` | All log levels |

### 🔥 Exception Handling Tests

| Test | Endpoint | Expected Result |
|------|----------|-----------------|
| 11 | `/api/logtest/test-exception-notfound` | 404 Not Found |
| 12 | `/api/logtest/test-exception-badrequest` | 400 Bad Request |
| 13 | `/api/logtest/test-exception-unauthorized` | 401 Unauthorized |
| 14 | `/api/logtest/test-exception-conflict` | 409 Conflict |
| 15 | `/api/logtest/test-exception-servererror` | 500 Server Error |

### 🎯 Complete Flow Test

| Test | Endpoint | What it does |
|------|----------|--------------|
| 16 | `/api/logtest/test-complete-flow` | Complete booking flow (7+ logs) |

---

## 🎬 Recommended Testing Order

### 1. **Start Simple** ✅
```http
GET https://localhost:7001/api/logtest/test-info
```
- Run this first
- Check Seq - you should see the log!
- This confirms everything is working

### 2. **Test Custom Extensions** 📝
```http
GET https://localhost:7001/api/logtest/test-user-activity
GET https://localhost:7001/api/logtest/test-booking-activity
GET https://localhost:7001/api/logtest/test-payment-activity
```
- These test the custom logging methods
- Check Seq for structured logs with UserId, BookingId, etc.

### 3. **Test Exception Handling** 🔥
```http
GET https://localhost:7001/api/logtest/test-exception-notfound
```
- This will return a 404 error - **that's expected!**
- Check Seq - you should see:
  - Error log with full details
  - Exception message
  - Stack trace
  - Request context (IP, path, user, etc.)

### 4. **Test Complete Flow** 🎯
```http
GET https://localhost:7001/api/logtest/test-complete-flow
```
- Generates 7+ related logs
- Shows a complete booking process
- All logs have the same UserId for easy filtering

---

## 🔍 What to Look for in Seq

### After Running a Test:

1. **Log appears in Seq** ✅
   - Should appear within seconds
   - Real-time updates

2. **Structured Data** 📊
   - Click on a log entry
   - Expand to see all properties
   - Example: `UserId`, `BookingId`, `Amount`, etc.

3. **Context Information** 📝
   - MachineName
   - Application name
   - Timestamp
   - Source context

4. **Exception Details** (for error tests)
   - Exception type
   - Exception message
   - Stack trace
   - Request path
   - IP address
   - TraceId

---

## 🔎 Search Examples in Seq

After running tests, try these searches in Seq:

```sql
-- All errors
@Level = 'Error'

-- Specific user
UserId = 'user-123'

-- Specific booking
BookingId = 1001

-- Payment logs
@Message like '%Payment%'

-- Failed authentication
@Level = 'Warning' and @Message like '%Authentication%'

-- Slow operations
Milliseconds > 1000

-- Today's logs
@Timestamp > Now() - 1d

-- Complete flow test
UserId = 'user-789'

-- All test controller logs
SourceContext like '%LogTestController%'
```

---

## 🎨 Visual Guide

### What You'll See in the Console:
```
[10:30:00 INF] User Activity: user-123 performed Login. From web browser
[10:30:01 INF] Booking Activity: Booking 1001 - Created by User user-123. Weekend rental
[10:30:02 INF] Payment Activity: Booking 1001 - Amount 500.00 via CreditCard - Status: Success
```

### What You'll See in Seq:
```json
{
  "@t": "2024-01-15T10:30:00.123Z",
  "@mt": "User Activity: {UserId} performed {Action}. {Details}",
  "@l": "Information",
  "UserId": "user-123",
  "Action": "Login",
  "Details": "From web browser",
  "MachineName": "YOUR-PC",
  "SourceContext": "NOL.API.Controllers.LogTestController"
}
```

---

## ✅ Verification Checklist

After running all tests, verify:

- [ ] Logs appear in Seq within 1-2 seconds
- [ ] Information logs are visible
- [ ] Warning logs are visible
- [ ] Error logs (from exception tests) are visible
- [ ] Structured data is preserved (UserId, BookingId, etc.)
- [ ] Exception details include stack traces
- [ ] Request context is logged (IP, path, TraceId)
- [ ] Can search and filter logs in Seq
- [ ] TraceId links related logs together

---

## 🚨 Troubleshooting

### Logs not appearing in Seq?

**Check 1:** Is the app running?
```bash
# Should see: Now listening on: https://localhost:7001
```

**Check 2:** Is Seq URL correct?
- Open `appsettings.json`
- Verify: `"serverUrl": "https://seq-production-43df.up.railway.app/"`

**Check 3:** Check console output
- Logs should appear in console even if Seq is down
- Look for any Serilog errors

**Check 4:** Test basic connectivity
```bash
curl https://seq-production-43df.up.railway.app/
```

### Exception tests returning HTML instead of JSON?

- This is normal in browsers
- Use the .http file or Postman for proper JSON responses

### Too many logs?

- Increase minimum log level in `appsettings.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"  // Only Warning and above
    }
  }
}
```

---

## 🎯 Next Steps After Testing

1. ✅ Confirm all tests work
2. ✅ Explore Seq dashboard features
3. ✅ Create custom searches/filters in Seq
4. ✅ Add logging to your own controllers
5. ✅ Set up Seq alerts (optional)
6. ✅ Remove test controller in production (optional)

---

## 📚 Files

- **Test Controller**: `src/NOL.API/Controllers/LogTestController.cs`
- **Test Requests**: `TEST_SEQ_LOGGING.http`
- **Configuration**: `src/NOL.API/appsettings.json`
- **Middleware**: `src/NOL.API/Middleware/GlobalExceptionHandlerMiddleware.cs`
- **Extensions**: `src/NOL.API/Services/LoggingService.cs`

---

## 💡 Pro Tips

1. **Keep Seq open** while testing to see real-time logs
2. **Use TraceId** to track related logs across requests
3. **Test exceptions** - they're the most important to log!
4. **Try different searches** in Seq to learn the query language
5. **Check performance** - slow operations are logged automatically

---

## 🎉 Success Criteria

You'll know it's working when:

✅ You run a test  
✅ Response is received  
✅ Log appears in Seq within 2 seconds  
✅ Log contains structured data  
✅ You can search/filter logs  
✅ Exception details are complete  

**Happy Testing! 🚀**

