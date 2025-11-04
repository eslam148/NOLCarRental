# 🧪 CORS Testing Guide

## Your API Endpoint

```
https://nolrental.runasp.net/api/admin/cars?sortBy=createdAt&sortOrder=desc&page=1&pageSize=10
```

---

## 🚀 Quick Test Methods

### Method 1: HTML Test Page (Recommended! 🌟)

**Easiest way to test CORS:**

1. Open `test-cors.html` in your browser:
   ```bash
   # Double-click the file or open with:
   open test-cors.html  # Mac
   # or
   xdg-open test-cors.html  # Linux
   # or
   start test-cors.html  # Windows
   ```

2. Click **"Run All Tests"** button

3. See real-time results with ✅ or ❌

**What it tests:**
- ✅ Simple GET request with CORS
- ✅ GET with custom headers (Content-Type, Accept-Language)
- ✅ OPTIONS preflight request
- ✅ Authorization header (optional)

---

### Method 2: Command Line Test

Run the bash script:

```bash
chmod +x test-cors.sh
bash test-cors.sh
```

**Output example:**
```
✅ SUCCESS
✅ CORS Headers Present
✅ Preflight Successful
```

---

### Method 3: Browser Console (Quick!)

Open any webpage, press F12, paste this:

```javascript
fetch('https://nolrental.runasp.net/api/admin/cars?sortBy=createdAt&sortOrder=desc&page=1&pageSize=10', {
  headers: {
    'Accept-Language': 'ar',
    'Content-Type': 'application/json'
  }
})
.then(r => r.json())
.then(data => {
  console.log('✅ CORS WORKS!');
  console.log('Data:', data);
})
.catch(e => {
  console.error('❌ CORS ERROR:', e);
});
```

---

### Method 4: curl (Terminal)

```bash
# Test 1: Simple GET
curl -H "Origin: https://example.com" \
  https://nolrental.runasp.net/api/admin/cars?sortBy=createdAt&sortOrder=desc&page=1&pageSize=10

# Test 2: Check CORS headers
curl -I -H "Origin: https://example.com" \
  https://nolrental.runasp.net/api/admin/cars?sortBy=createdAt&sortOrder=desc&page=1&pageSize=10

# Test 3: OPTIONS preflight
curl -X OPTIONS -I \
  -H "Origin: https://example.com" \
  -H "Access-Control-Request-Method: GET" \
  https://nolrental.runasp.net/api/admin/cars?sortBy=createdAt&sortOrder=desc&page=1&pageSize=10

# Test 4: With Arabic language
curl -H "Origin: https://example.com" \
  -H "Accept-Language: ar" \
  https://nolrental.runasp.net/api/admin/cars?sortBy=createdAt&sortOrder=desc&page=1&pageSize=10
```

---

## ✅ Expected Results

### If CORS is Working:

**Headers you should see:**
```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization, Accept-Language, ...
Access-Control-Max-Age: 3600
```

**Status codes:**
- `200 OK` for GET requests
- `204 No Content` or `200 OK` for OPTIONS requests

**Response:**
- JSON data with car list
- No CORS errors in console

---

## ❌ If CORS is NOT Working:

**Browser console errors:**
```
Access to fetch at '...' from origin '...' has been blocked by CORS policy
```

**Missing headers:**
```
Access-Control-Allow-Origin: (not present)
```

**What to check:**
1. ✅ CORS configured in `Program.cs`
2. ✅ `app.UseCors("AllowAll")` is called
3. ✅ CORS headers in `web.config`
4. ✅ API is deployed and accessible

---

## 📊 Test Results Meaning

| Test | What it checks |
|------|----------------|
| **Simple GET** | Basic CORS functionality |
| **Custom Headers** | If your app can send headers like Accept-Language |
| **OPTIONS Preflight** | If browser pre-flight requests work |
| **Authorization** | If you can send auth tokens |

---

## 🎯 Frontend Integration Examples

### React

```javascript
import axios from 'axios';

const API_URL = 'https://nolrental.runasp.net';

axios.get(`${API_URL}/api/admin/cars`, {
  params: {
    sortBy: 'createdAt',
    sortOrder: 'desc',
    page: 1,
    pageSize: 10
  },
  headers: {
    'Accept-Language': 'ar'
  }
})
.then(response => console.log(response.data))
.catch(error => console.error(error));
```

### Vue.js

```javascript
async fetchCars() {
  try {
    const response = await fetch('https://nolrental.runasp.net/api/admin/cars?sortBy=createdAt&sortOrder=desc&page=1&pageSize=10', {
      headers: {
        'Accept-Language': 'ar',
        'Content-Type': 'application/json'
      }
    });
    const data = await response.json();
    console.log(data);
  } catch (error) {
    console.error('CORS Error:', error);
  }
}
```

### Angular

```typescript
import { HttpClient } from '@angular/common/http';

constructor(private http: HttpClient) {}

getCars() {
  return this.http.get('https://nolrental.runasp.net/api/admin/cars', {
    params: {
      sortBy: 'createdAt',
      sortOrder: 'desc',
      page: '1',
      pageSize: '10'
    },
    headers: {
      'Accept-Language': 'ar'
    }
  });
}
```

### Flutter

```dart
import 'package:http/http.dart' as http;

Future<void> getCars() async {
  final response = await http.get(
    Uri.parse('https://nolrental.runasp.net/api/admin/cars?sortBy=createdAt&sortOrder=desc&page=1&pageSize=10'),
    headers: {
      'Accept-Language': 'ar',
      'Content-Type': 'application/json',
    },
  );
  print(response.body);
}
```

---

## 🔍 Debugging CORS Issues

### Step 1: Check Browser Console

Look for red CORS errors

### Step 2: Check Network Tab

1. Open DevTools (F12)
2. Go to Network tab
3. Make request
4. Click on request
5. Check Response Headers

Should see:
```
access-control-allow-origin: *
access-control-allow-methods: GET, POST, PUT, DELETE, PATCH, OPTIONS
```

### Step 3: Check Seq Logs

Visit: https://seq-production-43df.up.railway.app/

Filter by:
```sql
@Message like '%OPTIONS%' or @Message like '%admin/cars%'
```

---

## ✅ CORS Configuration Files

Your CORS is configured in:

1. **`Program.cs`** (lines 260-279)
   ```csharp
   builder.Services.AddCors(...)
   app.UseCors("AllowAll");
   ```

2. **`web.config`** (lines 19-35)
   ```xml
   <add name="Access-Control-Allow-Origin" value="*" />
   ```

---

## 📞 Still Having Issues?

### Check:
1. ✅ API is accessible: https://nolrental.runasp.net
2. ✅ Endpoint exists and returns data
3. ✅ CORS middleware is before Authentication
4. ✅ Recent deployment succeeded

### Get Help:
- Check `TROUBLESHOOTING.md`
- Check `CORS_CONFIGURATION.md`
- View logs in Seq

---

## 🎉 Success Indicators

**You'll know CORS is working when:**

✅ HTML test page shows all green checkmarks  
✅ Browser console shows no CORS errors  
✅ Network tab shows CORS headers  
✅ Frontend can make requests successfully  
✅ OPTIONS requests return 200/204  

---

## 📁 Test Files

| File | Purpose |
|------|---------|
| `test-cors.html` | Interactive browser test (recommended!) |
| `test-cors.sh` | Command-line test script |
| `TEST_CORS_README.md` | This file |
| `CORS_CONFIGURATION.md` | Detailed CORS documentation |

---

**Start testing: Open `test-cors.html` in your browser! 🚀**

