# 🌐 CORS Configuration Guide

## ✅ CORS is Now Fully Configured

Your API now has comprehensive CORS support for cross-origin requests.

---

## 🔧 What's Configured

### 1. ASP.NET Core CORS (Program.cs)

```csharp
policy.AllowAnyOrigin()      // Allows requests from any domain
      .AllowAnyMethod()      // Allows GET, POST, PUT, DELETE, etc.
      .AllowAnyHeader()      // Allows any headers
      .WithExposedHeaders()  // Exposes specific headers to browser
```

### 2. IIS CORS Headers (web.config)

```xml
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization, Accept-Language, etc.
Access-Control-Max-Age: 3600 (1 hour cache for preflight)
```

---

## 🧪 Test CORS is Working

### Test 1: From Browser Console

Open your browser console (F12) and paste:

```javascript
// Test GET request
fetch('https://site29943.siteasp.net/api/enums/booking-statuses', {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json',
    'Accept-Language': 'ar'
  }
})
.then(response => response.json())
.then(data => console.log('✅ CORS works!', data))
.catch(error => console.error('❌ CORS error:', error));
```

### Test 2: With Authorization Header

```javascript
fetch('https://site29943.siteasp.net/api/admin/cars', {
  method: 'GET',
  headers: {
    'Authorization': 'Bearer YOUR_TOKEN',
    'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log('✅ Auth CORS works!', data))
.catch(error => console.error('❌ Error:', error));
```

### Test 3: POST Request

```javascript
fetch('https://site29943.siteasp.net/api/test', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ test: 'data' })
})
.then(response => response.json())
.then(data => console.log('✅ POST CORS works!', data))
.catch(error => console.error('❌ Error:', error));
```

---

## 📊 CORS Request Flow

### What Happens:

```
1. Browser sends OPTIONS request (preflight)
   ↓
2. Server responds with CORS headers (204 No Content)
   ↓
3. Browser checks if request is allowed
   ↓
4. Browser sends actual request (GET, POST, etc.)
   ↓
5. Server responds with data + CORS headers
```

### Example:

```
OPTIONS /api/admin/cars
Response: 204 No Content
Headers:
  Access-Control-Allow-Origin: *
  Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
  Access-Control-Allow-Headers: Content-Type, Authorization
  
Then:

GET /api/admin/cars
Response: 200 OK
Headers:
  Access-Control-Allow-Origin: *
Body: [car data...]
```

---

## 🔍 Check CORS Headers

### Using curl:

```bash
# Check preflight (OPTIONS)
curl -I -X OPTIONS https://site29943.siteasp.net/api/enums/booking-statuses \
  -H "Origin: https://example.com" \
  -H "Access-Control-Request-Method: GET"

# Check actual request
curl -I https://site29943.siteasp.net/api/enums/booking-statuses \
  -H "Origin: https://example.com"
```

### Expected Response Headers:

```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization, Accept-Language, ...
Access-Control-Max-Age: 3600
```

---

## 🎯 Frontend Integration

### React/JavaScript

```javascript
// Configure axios
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://site29943.siteasp.net',
  headers: {
    'Content-Type': 'application/json',
    'Accept-Language': 'ar'  // or 'en'
  }
});

// Make requests
api.get('/api/enums/booking-statuses')
  .then(response => console.log(response.data))
  .catch(error => console.error(error));
```

### Angular

```typescript
import { HttpClient } from '@angular/common/http';

constructor(private http: HttpClient) {}

getBookingStatuses() {
  return this.http.get('https://site29943.siteasp.net/api/enums/booking-statuses', {
    headers: {
      'Accept-Language': 'ar'
    }
  });
}
```

### Vue.js

```javascript
// Using fetch API
async fetchBookingStatuses() {
  const response = await fetch('https://site29943.siteasp.net/api/enums/booking-statuses', {
    headers: {
      'Accept-Language': 'ar'
    }
  });
  const data = await response.json();
  return data;
}
```

### Flutter/Dart

```dart
import 'package:http/http.dart' as http;

Future<void> getBookingStatuses() async {
  final response = await http.get(
    Uri.parse('https://site29943.siteasp.net/api/enums/booking-statuses'),
    headers: {
      'Accept-Language': 'ar',
      'Content-Type': 'application/json',
    },
  );
  print(response.body);
}
```

---

## 🔐 For Authenticated Requests

If you need to send credentials (cookies, tokens), update the CORS policy:

### Option A: Allow Specific Origins with Credentials

Edit `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "https://your-frontend.com",
                "https://www.your-frontend.com",
                "http://localhost:3000"  // For development
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();  // Enable credentials
    });
});
```

### Option B: Keep Current (Any Origin, No Credentials)

Current configuration allows any origin but **not** credentials:

```csharp
policy.AllowAnyOrigin()  // Works with any domain
      .AllowAnyMethod()
      .AllowAnyHeader();
// Note: Cannot use .AllowCredentials() with AllowAnyOrigin()
```

---

## ❌ Common CORS Errors & Fixes

### Error 1: "No 'Access-Control-Allow-Origin' header"

**Cause:** CORS not configured or middleware not added

**Fix:** ✅ Already fixed! CORS is configured in both Program.cs and web.config

### Error 2: "Credentials flag is true, but Access-Control-Allow-Credentials is not"

**Cause:** Using `credentials: true` in fetch but CORS doesn't allow credentials

**Fix:** Either:
- Remove `credentials: true` from your fetch call, OR
- Update CORS policy to specific origins with `.AllowCredentials()`

### Error 3: "Method [METHOD] not allowed"

**Cause:** HTTP method not in allowed methods list

**Fix:** ✅ Already fixed! All methods (GET, POST, PUT, DELETE, PATCH, OPTIONS) are allowed

### Error 4: "Header [HEADER] not allowed"

**Cause:** Custom header not in allowed headers list

**Fix:** ✅ Already fixed! Common headers are allowed. Add more if needed:

```csharp
.WithHeaders("X-Custom-Header", "Another-Header")
```

---

## 🧪 CORS Test Endpoints

Test these endpoints from your frontend:

```bash
# 1. Simple GET
https://site29943.siteasp.net/api/enums/booking-statuses

# 2. GET with headers
https://site29943.siteasp.net/api/enums/fuel-types
Headers: Accept-Language: ar

# 3. POST (if you have a test endpoint)
https://site29943.siteasp.net/api/test
Method: POST
Body: {"test": "data"}

# 4. With Authorization
https://site29943.siteasp.net/api/admin/cars
Headers: Authorization: Bearer TOKEN
```

---

## 📝 CORS Configuration Files

### 1. Program.cs (Lines 260-279)
```csharp
builder.Services.AddCors(options => { ... });
app.UseCors("AllowAll");  // Must be BEFORE UseAuthorization()
```

### 2. web.config (Lines 19-35)
```xml
<httpProtocol>
  <customHeaders>
    <add name="Access-Control-Allow-Origin" value="*" />
    ...
  </customHeaders>
</httpProtocol>
```

---

## ✅ Verification Checklist

- [x] CORS policy added in Program.cs
- [x] UseCors() added to middleware pipeline
- [x] CORS headers in web.config
- [x] OPTIONS method supported
- [x] All HTTP methods allowed
- [x] Common headers allowed
- [x] Max-Age set for preflight caching

---

## 🚀 Deploy Updated Configuration

```bash
git add .
git commit -m "Update CORS configuration"
git push
```

Wait 2-3 minutes for deployment, then test!

---

## 🔍 Debug CORS Issues

### Check Browser Console

Look for messages like:
```
Access to fetch at 'https://site29943.siteasp.net/api/...' 
from origin 'https://your-frontend.com' has been blocked by CORS policy
```

### Check Network Tab

1. Open Developer Tools (F12)
2. Go to Network tab
3. Look for failed requests
4. Check Response Headers

Should see:
```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: ...
Access-Control-Allow-Headers: ...
```

### Check Seq Logs

Visit: https://seq-production-43df.up.railway.app/

Filter:
```sql
@Message like '%OPTIONS%' or @Message like '%CORS%'
```

---

## 📞 Still Having CORS Issues?

1. **Clear browser cache** and try again
2. **Test in incognito mode** to rule out extensions
3. **Check your frontend code** - are you setting `mode: 'cors'`?
4. **Verify API URL** - make sure you're using HTTPS
5. **Check Seq logs** for any errors

---

## ✅ CORS is Ready!

Your API now accepts cross-origin requests from any domain.

**Test it:**
```bash
curl -H "Origin: https://example.com" \
     -H "Content-Type: application/json" \
     https://site29943.siteasp.net/api/enums/booking-statuses
```

**Happy cross-origin requesting! 🌐**

