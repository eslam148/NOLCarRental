# 📊 Request Body Logging in Seq

## ✅ What's Logged Now

The `CreateBooking` endpoint now logs comprehensive information to Seq.

### Logged Information:

1. **Request Body** (complete DTO)
2. **User ID** 
3. **Car ID**
4. **Booking dates**
5. **Success/failure status**
6. **Error details** (if any)

---

## 🔍 What Gets Logged

### When Request Arrives:
```csharp
_logger.LogInformation("CreateBooking Request - UserId: {UserId}, RequestBody: {@CreateBookingDto}", 
    userId ?? "Anonymous", createBookingDto);
```

**Logs:**
- Full request body as structured JSON
- User ID from JWT token
- All DTO properties

### During Processing:
```csharp
_logger.LogInformation("Creating booking for User {UserId} - CarId: {CarId}, StartDate: {StartDate}, EndDate: {EndDate}", 
    userId, createBookingDto.CarId, createBookingDto.StartDate, createBookingDto.EndDate);
```

### On Success:
```csharp
_logger.LogInformation("Booking created successfully - BookingId: {BookingId}, UserId: {UserId}, CarId: {CarId}", 
    result.Id, userId, createBookingDto.CarId);
```

### On Error:
```csharp
_logger.LogError(ex, "Failed to create booking for User {UserId} with request {@CreateBookingDto}", 
    userId, createBookingDto);
```

---

## 📊 View in Seq

### Step 1: Open Seq
```
https://seq-production-43df.up.railway.app/
```

### Step 2: Filter for CreateBooking Requests

**Query:**
```sql
@Message like '%CreateBooking%'
```

**Or more specific:**
```sql
@Message like '%CreateBooking Request%'
```

### Step 3: View Request Body

Click on any log entry to see the **structured data**:

**Example log in Seq:**
```json
{
  "@t": "2024-01-15T10:30:00.123Z",
  "@mt": "CreateBooking Request - UserId: {UserId}, RequestBody: {@CreateBookingDto}",
  "@l": "Information",
  "UserId": "user-abc-123",
  "CreateBookingDto": {
    "carId": 501,
    "startDate": "2024-01-20T10:00:00Z",
    "endDate": "2024-01-25T10:00:00Z",
    "pickupBranchId": 1,
    "returnBranchId": 1,
    "extras": [1, 3, 5],
    "paymentMethod": "CreditCard",
    "notes": "Need GPS and child seat"
  },
  "SourceContext": "NOL.API.Controllers.BookingsController",
  "RequestPath": "/api/bookings",
  "RequestMethod": "POST"
}
```

---

## 🔎 Useful Seq Queries

### All CreateBooking Requests
```sql
@Message like '%CreateBooking Request%'
```

### Specific User's Booking Requests
```sql
UserId = 'user-abc-123' and @Message like '%CreateBooking%'
```

### Failed Bookings
```sql
@Message like '%Failed to create booking%' or (@Message like '%CreateBooking%' and @Level = 'Error')
```

### Bookings for Specific Car
```sql
CarId = 501 and @Message like '%CreateBooking%'
```

### Today's Booking Requests
```sql
@Timestamp > Now() - 1d and @Message like '%CreateBooking%'
```

### Unauthorized Attempts
```sql
@Message like '%CreateBooking - Unauthorized%'
```

### Successful Bookings
```sql
@Message like '%Booking created successfully%'
```

### By Date Range
```sql
StartDate >= '2024-01-20' and @Message like '%CreateBooking%'
```

---

## 📋 Example Seq Outputs

### Successful Request:

**Log 1: Request Received**
```
[10:30:00 INF] CreateBooking Request - UserId: user-123, RequestBody: {
  carId: 501,
  startDate: "2024-01-20",
  endDate: "2024-01-25",
  ...
}
```

**Log 2: Processing**
```
[10:30:00 INF] Creating booking for User user-123 - CarId: 501, StartDate: 2024-01-20, EndDate: 2024-01-25
```

**Log 3: Success**
```
[10:30:01 INF] Booking created successfully - BookingId: 1001, UserId: user-123, CarId: 501
```

### Failed Request:

**Log 1: Request Received**
```
[10:30:00 INF] CreateBooking Request - UserId: user-123, RequestBody: {...}
```

**Log 2: Error**
```
[10:30:00 ERR] Failed to create booking for User user-123 with request {
  carId: 999,
  startDate: "2024-01-20",
  ...
}
Exception: Car not found
```

---

## 🎯 What You Can Track

With this logging, you can now:

✅ **Debug issues** - See exact request data  
✅ **Monitor usage** - Track booking patterns  
✅ **Audit trail** - Who booked what and when  
✅ **Error analysis** - See failed requests with context  
✅ **Performance** - Track request duration  
✅ **Security** - Detect unauthorized attempts  

---

## 💡 Tips for Seq

### 1. Create Saved Queries

Save frequently used queries in Seq:
- "Failed Bookings Today"
- "Bookings by User"
- "Unauthorized Attempts"

### 2. Set Up Alerts

Create alerts for:
- Failed bookings (> 5 per hour)
- Unauthorized attempts
- Specific error patterns

### 3. Use Properties Panel

Click on any log → Properties panel shows all structured data

### 4. Export Data

Export logs for analysis:
- CSV format
- JSON format
- Excel-compatible

---

## 🔧 Extend Logging to Other Endpoints

You can add similar logging to other endpoints:

### Example for UpdateBooking:

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingDto dto)
{
    _logger.LogInformation("UpdateBooking Request - BookingId: {BookingId}, UserId: {UserId}, RequestBody: {@UpdateBookingDto}", 
        id, User.FindFirst(ClaimTypes.NameIdentifier)?.Value, dto);
    
    // ... rest of code
}
```

### Example for CancelBooking:

```csharp
[HttpPut("{id}/cancel")]
public async Task<IActionResult> CancelBooking(int id)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    _logger.LogInformation("CancelBooking Request - BookingId: {BookingId}, UserId: {UserId}", 
        id, userId);
    
    // ... rest of code
}
```

---

## 🚀 After Deployment

### Test the Logging:

1. **Make a booking request:**
```bash
curl -X POST https://nolrental.runasp.net/api/bookings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "carId": 501,
    "startDate": "2024-01-20T10:00:00Z",
    "endDate": "2024-01-25T10:00:00Z",
    "pickupBranchId": 1,
    "returnBranchId": 1
  }'
```

2. **Check Seq:**
```
https://seq-production-43df.up.railway.app/
```

3. **Search for your request:**
```sql
@Message like '%CreateBooking Request%' and @Timestamp > Now() - 5m
```

4. **Verify you see:**
- ✅ Full request body
- ✅ User ID
- ✅ All booking details
- ✅ Success/error status

---

## 📊 Dashboard Example

You can create a dashboard in Seq showing:

1. **Total Bookings Today**
   ```sql
   count(@Message like '%Booking created successfully%' and @Timestamp > Now() - 1d)
   ```

2. **Failed Bookings**
   ```sql
   count(@Message like '%Failed to create booking%' and @Timestamp > Now() - 1d)
   ```

3. **Top Users**
   ```sql
   group UserId by count where @Message like '%CreateBooking%'
   ```

4. **Top Cars**
   ```sql
   group CarId by count where @Message like '%Booking created%'
   ```

---

## ⚠️ Important Notes

### Sensitive Data

The `@` prefix in `{@CreateBookingDto}` tells Serilog to log it as **structured JSON**.

**Be careful not to log:**
- ❌ Credit card numbers
- ❌ Passwords
- ❌ Personal sensitive data

**Currently logging:**
- ✅ Booking details (safe)
- ✅ User IDs (safe)
- ✅ Car IDs (safe)
- ✅ Dates (safe)

---

## ✅ Summary

**What was added:**
- ✅ Request body logging
- ✅ User ID tracking
- ✅ Success/failure logging
- ✅ Error details with context
- ✅ Structured JSON in Seq

**Where to view:**
- 🔗 https://seq-production-43df.up.railway.app/

**How to query:**
- 📝 Use Seq query language
- 🔍 Filter by properties
- 📊 Create dashboards

**Next steps:**
1. Deploy the changes
2. Make test booking request
3. Check Seq for logs
4. Create useful queries/alerts

---

**Happy logging! 📊**

