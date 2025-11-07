# 🎉 Request & cURL Logging - Complete Summary

## ✅ What's Been Implemented

### 🔍 New Middleware: RequestLoggingMiddleware

**File:** `src/NOL.API/Middleware/RequestLoggingMiddleware.cs`

**Features:**
- ✅ Logs every HTTP request
- ✅ Generates equivalent cURL command
- ✅ Captures request body (POST/PUT/PATCH)
- ✅ Captures all headers
- ✅ Logs response status & time
- ✅ Logs response body (for errors)
- ✅ Redacts sensitive data (Authorization, Cookie)
- ✅ Skips static files
- ✅ Tracks with TraceId

---

## 🎯 What You'll See in Seq

### For Every Request:

**Example: GET Request**
```
HTTP Request: GET /api/enums/booking-statuses?culture=ar | User: Anonymous | IP: 192.168.1.100 | TraceId: 0HMVFE8A5 | CurlCommand: curl -X GET 'https://nolrental.runasp.net/api/enums/booking-statuses?culture=ar' -H 'Accept-Language: ar'
```

**Example: POST Request**
```
HTTP Request: POST /api/bookings | User: user@example.com | IP: 192.168.1.100 | CurlCommand: curl -X POST 'https://nolrental.runasp.net/api/bookings' -H 'Content-Type: application/json' -H 'Authorization: Bearer eyJhbG...***REDACTED***' -d '{"carId":1,"startDate":"2024-12-20",...}'

Request Body for POST /api/bookings | Body: {"carId":1,"startDate":"2024-12-20T10:00:00Z",...}

HTTP Response: POST /api/bookings | Status: 201 | Duration: 245ms | User: user@example.com
```

---

## 📊 Example Seq Log Entry

Click on any log in Seq to see:

```json
{
  "@t": "2024-11-07T10:30:00.123Z",
  "@mt": "HTTP Request: {Method} {Path} | User: {User} | IP: {IP} | CurlCommand: {CurlCommand}",
  "@l": "Information",
  "Method": "POST",
  "Path": "/api/bookings",
  "User": "user@example.com",
  "IP": "192.168.1.100",
  "TraceId": "0HMVFE8A5KQ0M:00000001",
  "CurlCommand": "curl -X POST 'https://nolrental.runasp.net/api/bookings' -H 'Content-Type: application/json' -H 'Authorization: Bearer eyJhbGciOi...***REDACTED***' -H 'Accept-Language: ar' -d '{\"carId\":1,\"startDate\":\"2024-12-20T10:00:00Z\",\"endDate\":\"2024-12-25T10:00:00Z\"}'",
  "RequestHost": "nolrental.runasp.net",
  "RequestPath": "/api/bookings",
  "SourceContext": "NOL.API.Middleware.RequestLoggingMiddleware"
}
```

**Just copy the `CurlCommand` value and run it in terminal!**

---

## 🚀 Quick Start

### Step 1: Deploy

```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental"
git add .
git commit -m "Add request logging middleware with cURL commands"
git push
```

### Step 2: Make a Test Request

```bash
curl https://nolrental.runasp.net/api/enums/booking-statuses
```

### Step 3: Check Seq

1. Open: https://seq-production-43df.up.railway.app/
2. Search: `@Message like '%HTTP Request%'`
3. Click on the log entry
4. See the **cURL command**!

---

## 🔍 Useful Seq Queries

### Get All cURL Commands

```sql
@Message like '%HTTP Request%' | select @Timestamp, Method, Path, CurlCommand
```

### Failed Requests with cURL

```sql
Status >= 400 | select Status, Method, Path, CurlCommand, ResponseBody
```

### POST Requests with Bodies

```sql
Method = 'POST' | select Path, RequestBody, CurlCommand
```

### Slow Requests (> 1 second)

```sql
Duration > 1000 | select Duration, Method, Path, CurlCommand
```

### Specific User's Requests

```sql
User = 'user@example.com' | select Method, Path, CurlCommand
```

### Today's API Calls

```sql
@Timestamp > Now() - 1d and @Message like '%HTTP Request%' | group Path by count()
```

---

## 💡 Real-World Usage

### Scenario 1: Reproduce Customer Issue

1. **Customer:** "My booking request failed!"
2. **You search Seq:** `User = 'customer@email.com' and Path like '%bookings%'`
3. **Find the failed request**
4. **Copy cURL command**
5. **Run locally:**
   ```bash
   curl -X POST 'https://nolrental.runasp.net/api/bookings' -H 'Content-Type: application/json' -d '{"carId":1,...}'
   ```
6. **Debug with exact same data**
7. **Fix the issue**

---

### Scenario 2: Performance Testing

1. **Alert:** Slow endpoint detected
2. **Search:** `Duration > 2000`
3. **Copy cURL command**
4. **Run with timing:**
   ```bash
   time curl -X POST 'https://nolrental.runasp.net/api/bookings' ...
   ```
5. **Profile and optimize**

---

### Scenario 3: API Documentation

1. **Generate real examples** from Seq
2. **Export cURL commands**
3. **Add to documentation**
4. **Share with team**

---

## 🔐 Security Features

### Sensitive Data Redaction:

| Header | How It's Logged |
|--------|-----------------|
| `Authorization` | First 20 chars + `***REDACTED***` |
| `Cookie` | `***REDACTED***` |
| `Content-Type` | ✅ Full value |
| `Accept-Language` | ✅ Full value |
| Other headers | ✅ Full value |

**Example:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiI...***REDACTED***
Cookie: ***REDACTED***
Content-Type: application/json
Accept-Language: ar
```

---

## 📋 What's Logged

### Per Request (3 Logs):

**1. Request Log**
- HTTP method
- Full URL with query params
- User info
- IP address
- TraceId
- **cURL command** ⭐

**2. Request Body Log** (if POST/PUT/PATCH)
- Full JSON body
- Method and path

**3. Response Log**
- Status code
- Duration (ms)
- User info
- TraceId

---

## 🎨 Additional Logging in BookingsController

The `CreateBooking` endpoint has **extra logging** on top of the middleware:

1. Request arrives (Middleware) ✅
2. Request body (Middleware) ✅
3. CreateBooking starts (Controller) ✅
4. Processing details (Controller) ✅
5. Success/Error (Controller) ✅
6. Response sent (Middleware) ✅

**Total: 6 log entries** for one booking request!

---

## 📊 Sample Seq Timeline

```
10:30:00.000 [INFO] HTTP Request: POST /api/bookings | CurlCommand: curl -X POST...
10:30:00.010 [INFO] Request Body for POST /api/bookings | Body: {...}
10:30:00.020 [INFO] CreateBooking Request - UserId: user-123 | RequestBody: {...}
10:30:00.030 [INFO] Creating booking for User user-123 - CarId: 1
10:30:00.245 [INFO] Booking created successfully - BookingId: 1001
10:30:00.250 [INFO] HTTP Response: POST /api/bookings | Status: 201 | Duration: 250ms
```

**TraceId links them all together!**

---

## 🧪 Test Files Created

| File | Purpose |
|------|---------|
| `RequestLoggingMiddleware.cs` | The middleware |
| `test-curl-logging.http` | Test requests |
| `REQUEST_CURL_LOGGING.md` | Full documentation |
| `CURL_LOGGING_SUMMARY.md` | This summary |

---

## ✅ Build Status

```
✅ Build Succeeded
✅ 0 Errors
✅ Middleware compiles successfully
✅ Integrated with Program.cs
✅ Ready to deploy
```

---

## 🚀 Deploy Now

```bash
git add .
git commit -m "Add request logging middleware with cURL commands"
git push
```

**After deployment (2-3 min):**
1. Make any API request
2. Check Seq
3. See the cURL command!
4. Copy and test it

---

## 🎯 Pipeline Position

Your middleware is positioned perfectly:

```
1. Serilog Request Logging
2. Exception Handler
3. Request Logging (with cURL) ← NEW!
4. HTTPS Redirect
5. Static Files
6. CORS
7. Localization
8. Authentication
9. Controllers
```

---

## 📖 Quick Reference

### View Logs:
```
https://seq-production-43df.up.railway.app/
```

### Search All Requests:
```sql
@Message like '%HTTP Request%'
```

### Get cURL Commands:
```sql
CurlCommand != null | select Method, Path, CurlCommand
```

### Copy cURL:
1. Click log entry
2. Find `CurlCommand` property
3. Copy value
4. Run in terminal

---

## 🎉 Benefits

**Before:**
- ❌ Hard to reproduce issues
- ❌ Manual cURL creation
- ❌ Missing request context

**After:**
- ✅ One-click reproduction
- ✅ Auto-generated cURL
- ✅ Complete request tracking
- ✅ Easy debugging
- ✅ Full audit trail
- ✅ Shareable commands

---

**Every request is now logged with its cURL equivalent! 🚀**

**Deploy and start seeing cURL commands in Seq!**

