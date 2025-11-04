# 🔧 Troubleshooting Guide - SiteASP.NET Deployment

## ❌ Application Not Working? Follow These Steps

### Step 1: Check What Error You're Getting

Visit your site: **https://site29943.siteasp.net**

#### Common Errors:

| Error | What it means |
|-------|---------------|
| **404 Not Found** | Files not deployed correctly OR wrong directory |
| **500 Internal Server Error** | Application error OR configuration issue |
| **502 Bad Gateway** | Application pool stopped OR .NET not installed |
| **503 Service Unavailable** | Application pool stopped |
| **Blank page** | Check browser console for errors |

---

## 🔍 Step 2: Check Deployment

### A. Verify Files on Server

Via FTP, check if these files exist in `wwwroot/`:

```
✅ wwwroot/NOL.API.dll
✅ wwwroot/web.config
✅ wwwroot/appsettings.json
✅ wwwroot/appsettings.Production.json
✅ wwwroot/Microsoft.*.dll
✅ wwwroot/Serilog.*.dll
```

**If files are missing:**
- Check GitHub Actions deployment logs
- Verify FTP credentials in GitHub Secrets
- Re-run the deployment

### B. Check GitHub Actions

1. Go to your GitHub repository
2. Click **Actions** tab
3. Check latest workflow run
4. Look for errors in build or deploy steps

**Common GitHub Actions Errors:**
- ❌ FTP connection failed → Check credentials
- ❌ Build failed → Check code errors
- ❌ Missing secrets → Add GitHub Secrets

---

## 🔍 Step 3: Check Server Requirements

### Required on SiteASP.NET:

- ✅ .NET 8 Runtime installed
- ✅ ASP.NET Core Hosting Bundle
- ✅ Application pool running
- ✅ Correct folder permissions

### How to Verify:

Contact SiteASP.NET support and ask them to verify:
1. .NET 8.0 is installed
2. ASP.NET Core Module V2 is installed  
3. Application pool is running
4. wwwroot has proper permissions

---

## 🔍 Step 4: Check Logs

### A. Server Logs

Check these log locations on your server (via FTP):

```
wwwroot/logs/stdout*.log     ← Most important!
wwwroot/logs/log-*.txt        ← Serilog logs
```

**How to access:**
1. Connect via FTP to site29943.siteasp.net
2. Navigate to `wwwroot/logs/`
3. Download the latest `stdout*.log` file
4. Open and check for errors

**Common Log Errors:**
- `Unable to start process` → Missing .NET runtime
- `Connection string` → Database connection issue
- `FileNotFoundException` → Missing DLL file
- `Permission denied` → Folder permissions issue

### B. Seq Logs

Check: https://seq-production-43df.up.railway.app/

Filter by:
```sql
Environment = 'Production' and @Level = 'Error'
```

---

## 🔍 Step 5: Test Database Connection

### Check if database is accessible:

```bash
# From your local machine, test database connection
dotnet run --project src/NOL.API --environment Production
```

**Common database issues:**
- ❌ Firewall blocking connection
- ❌ Wrong connection string
- ❌ Database server down
- ❌ Credentials incorrect

### Update Connection String (if needed):

Edit `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CORRECT_CONNECTION_STRING"
  }
}
```

---

## 🔍 Step 6: Simplified web.config

If still not working, try this minimal `web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\NOL.API.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

Save this and redeploy.

---

## 🔍 Step 7: Contact SiteASP.NET Support

If none of the above works, contact support with these details:

**Email:** support@siteasp.net

**Include:**
1. Your account: site29943
2. Error you're seeing (screenshot)
3. Contents of `wwwroot/logs/stdout*.log`
4. Confirm .NET 8.0 is installed
5. Confirm ASP.NET Core Module V2 is installed

**Ask them to:**
- Verify .NET 8 Runtime is installed
- Restart the application pool
- Check file permissions on wwwroot
- Check Event Viewer logs

---

## ✅ Quick Checklist

Go through this checklist:

- [ ] Files deployed to `wwwroot/` folder (not root)
- [ ] `web.config` exists in `wwwroot/`
- [ ] `NOL.API.dll` exists in `wwwroot/`
- [ ] All `.dll` dependencies exist
- [ ] GitHub Actions deployment succeeded
- [ ] FTP_SERVER_DIR secret is `./wwwroot/`
- [ ] .NET 8 Runtime installed on server
- [ ] Application pool is running
- [ ] Database connection string is correct
- [ ] Firewall allows database connection
- [ ] Check `wwwroot/logs/stdout*.log` for errors
- [ ] Check Seq logs for errors

---

## 🔨 Common Fixes

### Fix 1: Redeploy with Correct Path

```bash
# Make sure GitHub Secret FTP_SERVER_DIR = ./wwwroot/
git add .
git commit -m "Fix deployment path"
git push
```

### Fix 2: Manual FTP Upload

If GitHub Actions isn't working:

1. Build locally:
```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental/src/NOL.API"
dotnet publish -c Release -o ./publish
```

2. Upload `./publish/*` to server's `wwwroot/` via FTP client

### Fix 3: Check web.config Encoding

Ensure `web.config` is UTF-8 encoded (not UTF-8 with BOM)

### Fix 4: Restart Application

Via SiteASP control panel:
1. Go to control panel
2. Find your website
3. Click "Recycle" or "Restart" application pool

---

## 🧪 Test Endpoints

After fixing, test these:

```bash
# Test 1: Basic connectivity
curl -I https://site29943.siteasp.net

# Test 2: Swagger
curl https://site29943.siteasp.net

# Test 3: API endpoint
curl https://site29943.siteasp.net/api/enums/booking-statuses

# Test 4: With header
curl -H "Accept-Language: ar" https://site29943.siteasp.net/api/enums/booking-statuses
```

---

## 📊 Diagnostic Commands

### Check DNS:
```bash
nslookup site29943.siteasp.net
```

### Check if site is up:
```bash
ping site29943.siteasp.net
```

### Test SSL:
```bash
curl -v https://site29943.siteasp.net
```

---

## 🆘 Still Not Working?

### Create a Test HTML File

1. Create `test.html`:
```html
<!DOCTYPE html>
<html>
<body>
<h1>Test Page - If you see this, hosting works!</h1>
</body>
</html>
```

2. Upload to `wwwroot/test.html`

3. Visit: https://site29943.siteasp.net/test.html

**If HTML works but API doesn't:**
→ Issue is with .NET/ASP.NET Core, not hosting

**If HTML doesn't work:**
→ Issue is with hosting/FTP deployment

---

## 📞 Support Resources

| Resource | Link/Contact |
|----------|-------------|
| SiteASP Support | support@siteasp.net |
| Control Panel | https://panel.siteasp.net |
| GitHub Actions Help | https://docs.github.com/en/actions |
| Seq Logs | https://seq-production-43df.up.railway.app/ |
| ASP.NET Core Docs | https://docs.microsoft.com/aspnet/core |

---

## 💡 Prevention Tips

**Before deploying:**
- [ ] Test locally with `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Verify all connection strings
- [ ] Check all secrets are configured
- [ ] Review GitHub Actions logs
- [ ] Test database connectivity

**After deploying:**
- [ ] Check logs immediately
- [ ] Test all major endpoints
- [ ] Monitor Seq for errors
- [ ] Verify static files work

---

## ✅ Success Indicators

You'll know it's working when:

✅ No errors in `stdout*.log`  
✅ Swagger UI loads at https://site29943.siteasp.net  
✅ API endpoints return data  
✅ Logs appear in Seq  
✅ Database queries work  

**Good luck! 🚀**

