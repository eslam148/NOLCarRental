# 🔑 Authorization Token Logging

## ✅ Token Logging Enabled

Your middleware now logs **complete Authorization tokens** for debugging!

---

## 🔍 What You'll See in Seq

### Example Request with Token:

**Request:**
```bash
curl -X POST https://nolrental.runasp.net/api/bookings \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyMTIzIiwibmFtZSI6IkpvaG4gRG9lIn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
```

**Seq Log:**
```json
{
  "Message": "HTTP Request: POST /api/bookings",
  "CurlCommand": "curl -X POST 'https://nolrental.runasp.net/api/bookings' -H 'Content-Type: application/json' -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyMTIzIiwibmFtZSI6IkpvaG4gRG9lIn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c' -d '{...}'",
  "User": "user@example.com",
  "IP": "192.168.1.100"
}
```

**✅ Full token is visible!**

---

## 🎯 Benefits

### For Debugging:

1. **Copy Exact Token**
   - Get token from Seq log
   - Use in Postman/Insomnia
   - Test locally with same token

2. **Reproduce Authentication Issues**
   - Copy cURL with token
   - Run locally
   - Debug token validation

3. **Verify Token Claims**
   - See which token was used
   - Check token expiry
   - Verify user permissions

---

## 🔍 Seq Queries for Token Analysis

### Find Requests with Specific Token

```sql
CurlCommand like '%eyJhbGciOiJIUzI1NiIs%'
```

### Unauthorized Requests (Invalid Tokens)

```sql
Status = 401 | select IP, Path, CurlCommand
```

### Requests by User (from Token)

```sql
User = 'user@example.com' | select Method, Path, CurlCommand
```

### Extract Tokens from Logs

```sql
CurlCommand like '%Authorization: Bearer%' | select CurlCommand
```

---

## 💡 Use Cases

### Use Case 1: Debug "Unauthorized" Error

**Customer:** "I'm getting 401 Unauthorized!"

**You:**
1. Search Seq: `User = 'customer@email.com' and Status = 401`
2. Find the request
3. See the **exact token** they used
4. Copy cURL command with token
5. Test locally:
   ```bash
   curl -X POST 'https://nolrental.runasp.net/api/bookings' -H 'Authorization: Bearer eyJhbG...'
   ```
6. Check if token is expired/invalid
7. Debug token generation

---

### Use Case 2: Test with Real Token

**Need to test authenticated endpoint:**

1. User makes a request (logged to Seq)
2. Get token from Seq log
3. Use in your tests:
   ```bash
   curl -X GET 'https://nolrental.runasp.net/api/admin/cars' \
     -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR...'
   ```

---

### Use Case 3: Verify Token Claims

**Check what's in the token:**

1. Copy token from Seq log
2. Decode at https://jwt.io
3. Verify claims:
   - UserId
   - Email
   - Roles
   - Expiry

---

## 🔐 Security Considerations

### ⚠️ Important Notes:

**Tokens are now fully logged!**

**This is useful for:**
- ✅ Development/staging environments
- ✅ Debugging authentication issues
- ✅ Reproducing customer problems
- ✅ Testing with real tokens

**Be aware:**
- ⚠️ Tokens are sensitive credentials
- ⚠️ Anyone with Seq access can see tokens
- ⚠️ Tokens can be used to impersonate users
- ⚠️ Consider token expiry times

### Best Practices:

1. **Secure Seq Access**
   - Only authorized team members
   - Use Seq authentication
   - Set up proper permissions

2. **Short Token Expiry**
   - Current: 43200 minutes (30 days)
   - Consider shorter for production

3. **Monitor Seq Access**
   - Track who views logs
   - Audit Seq usage

4. **Rotate Secrets**
   - Change JWT secret regularly
   - Invalidate old tokens

---

## 🎨 What Changed

### Before (❌ Redacted):
```
CurlCommand: curl ... -H 'Authorization: Bearer eyJhbG...***REDACTED***'
```

### After (✅ Full Token):
```
CurlCommand: curl ... -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyMTIzIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c'
```

### What's Still Redacted:

- ✅ `Cookie` header → `***REDACTED***`

---

## 📊 Example Seq Log with Token

### Full Log Entry:

```json
{
  "@t": "2024-11-07T10:30:00.123Z",
  "@l": "Information",
  "Message": "HTTP Request: POST /api/bookings",
  "Method": "POST",
  "Path": "/api/bookings",
  "User": "user@example.com",
  "IP": "192.168.1.100",
  "TraceId": "ABC123",
  "CurlCommand": "curl -X POST 'https://nolrental.runasp.net/api/bookings' -H 'Content-Type: application/json' -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyMTIzIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c' -H 'Accept-Language: ar' -d '{\"carId\":1}'"
}
```

**Click to expand → Copy token or entire cURL command!**

---

## 🧪 Test Token Logging

### Test 1: Make Authenticated Request

```bash
curl -X GET https://nolrental.runasp.net/api/admin/cars \
  -H "Authorization: Bearer YOUR_REAL_TOKEN" \
  -H "Accept-Language: ar"
```

### Test 2: Check Seq

Visit: https://seq-production-43df.up.railway.app/

Search:
```sql
@Message like '%HTTP Request%' and CurlCommand like '%Authorization%'
```

### Test 3: Copy and Test cURL

1. Click on the log entry
2. Copy the `CurlCommand` value
3. Paste in terminal
4. Run - should work exactly the same!

---

## 🔎 Token Debugging Queries

### All Authenticated Requests

```sql
CurlCommand like '%Authorization: Bearer%'
```

### Failed Auth Requests

```sql
Status = 401 | select IP, Path, CurlCommand
```

### Requests by Specific User (from token)

```sql
User = 'user@example.com' | select Method, Path, CurlCommand
```

### Extract All Tokens

```sql
CurlCommand like '%Bearer%' | select User, CurlCommand
```

---

## 💡 Practical Examples

### Extract Token for Testing:

**From Seq log:**
```
CurlCommand: curl -X POST '...' -H 'Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'
```

**Extract token:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyMTIzIiwibmFtZSI6IkpvaG4gRG9lIn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**Use in Postman:**
- Authorization tab → Bearer Token → Paste

**Use in code:**
```javascript
fetch('https://nolrental.runasp.net/api/bookings', {
  headers: {
    'Authorization': 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'
  }
})
```

---

## ⚠️ Production Considerations

### Option 1: Keep Token Logging (Current)

**Pros:**
- ✅ Easy debugging
- ✅ Complete reproduction
- ✅ Quick issue resolution

**Cons:**
- ⚠️ Tokens visible in logs
- ⚠️ Security consideration

**Mitigations:**
- 🔒 Secure Seq access
- ⏱️ Short token expiry
- 🔐 Seq authentication required

---

### Option 2: Redact Tokens in Production (Optional)

If you want to redact tokens in production but keep them in development:

```csharp
// In CaptureRequestAsync method:
if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
{
    // Check environment
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (env == "Production")
    {
        // Redact in production
        var fullValue = header.Value.ToString();
        headers[header.Key] = fullValue.Length > 20 
            ? fullValue.Substring(0, 20) + "...***REDACTED***" 
            : "***REDACTED***";
    }
    else
    {
        // Full token in dev/staging
        headers[header.Key] = header.Value.ToString();
    }
}
else
{
    headers[header.Key] = header.Value.ToString();
}
```

---

## 📋 What's Logged Now

| Data | Status |
|------|--------|
| **Authorization Token** | ✅ Full token logged |
| **Request Body** | ✅ Logged |
| **Response Body** | ✅ Logged (errors/small) |
| **Cookies** | ❌ Redacted |
| **All Other Headers** | ✅ Logged |
| **User Info** | ✅ Logged |
| **IP Address** | ✅ Logged |
| **Performance** | ✅ Logged |

---

## 🚀 Deploy and Test

### Deploy:

```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental"
git add .
git commit -m "Update middleware to log full Authorization tokens"
git push
```

### Test:

```bash
# Make authenticated request
curl -X GET https://nolrental.runasp.net/api/admin/cars \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Check Seq:

Search:
```sql
CurlCommand like '%Authorization%'
```

**You'll see the complete token in the cURL command!**

---

## ✅ Summary

**Changes Made:**
1. ✅ Authorization tokens now fully logged (not redacted)
2. ✅ cURL commands include complete Bearer tokens
3. ✅ Cookies still redacted for security
4. ✅ Easy to copy and test with real tokens

**What This Means:**
- ✅ Perfect for debugging auth issues
- ✅ Easy request reproduction
- ✅ Complete cURL commands
- ⚠️ Ensure Seq access is secured

**Next:**
- Deploy the changes
- Make authenticated request
- Check Seq for full token in cURL
- Copy and test!

---

**Full token logging is now active! 🔑**

