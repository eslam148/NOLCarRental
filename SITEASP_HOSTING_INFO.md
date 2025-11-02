# 📋 SiteASP.NET Hosting Configuration

## Important: Web Root Directory

On **SiteASP.NET** hosting, the directory structure is specific:

```
Your Account Root/
├── logs/
├── wwwroot/          ← WEB ROOT (All files go here!)
│   ├── NOL.API.dll
│   ├── web.config
│   ├── appsettings.json
│   ├── wwwroot/      ← Static files (nested)
│   │   └── uploads/
│   └── All other application files
└── Other directories
```

## ⚠️ Critical Information

### Web Root = `wwwroot` folder

**All application files must be deployed to the `wwwroot` folder**, not the account root.

- ✅ **Correct**: `./wwwroot/` (files accessible via web)
- ❌ **Wrong**: `./` (files NOT accessible via web)

## 🎯 What This Means

### Your Application Structure:

```
site29943.siteasp.net/wwwroot/
├── NOL.API.dll               ← Main application
├── web.config                ← IIS configuration
├── appsettings.json          ← Settings
├── appsettings.Production.json
├── Microsoft.*.dll           ← Dependencies
├── Serilog.*.dll
├── Hangfire.*.dll
├── wwwroot/                  ← Nested static files folder
│   └── uploads/              ← User uploaded files
└── logs/                     ← Application logs
```

### URLs Map To:

```
https://site29943.siteasp.net/
                ↓
    Points to: wwwroot/web.config
                ↓
         Loads: wwwroot/NOL.API.dll

https://site29943.siteasp.net/uploads/image.png
                ↓
    Points to: wwwroot/wwwroot/uploads/image.png
```

## 📝 Configuration in GitHub Actions

The deployment is configured to upload to `./wwwroot/`:

```yaml
- name: Deploy to SiteASP.NET via FTP
  with:
    server-dir: ./wwwroot/    # ← All files go here
```

## 🔍 How to Verify

After deployment, check via FTP:

```
site29943.siteasp.net (FTP)
└── wwwroot/
    ├── NOL.API.dll          ✅ Should be here
    ├── web.config           ✅ Should be here
    └── All other DLLs       ✅ Should be here
```

## ⚙️ GitHub Secrets Configuration

Make sure your `FTP_SERVER_DIR` secret is set to:

```
FTP_SERVER_DIR = ./wwwroot/
```

## 🚀 Testing After Deployment

```bash
# Test if web.config is accessible (should redirect to app)
curl -I https://site29943.siteasp.net/

# Test API endpoint
curl https://site29943.siteasp.net/api/enums/booking-statuses
```

## 📊 Static Files

Your ASP.NET Core app has its own `wwwroot` folder for static files.

**Server structure:**
```
./wwwroot/                    ← Web root (deployment target)
├── NOL.API.dll
├── web.config
└── wwwroot/                  ← Static files (from your app)
    ├── uploads/
    └── css/
```

**Access via:**
```
https://site29943.siteasp.net/uploads/file.png
                ↓
Actual path: ./wwwroot/wwwroot/uploads/file.png
```

## ✅ Checklist

Before deployment:
- [ ] GitHub Secret `FTP_SERVER_DIR` = `./wwwroot/`
- [ ] `web.config` is in your project
- [ ] Application publishes all DLLs
- [ ] Static files are in `src/NOL.API/wwwroot/`

After deployment:
- [ ] Files exist in `./wwwroot/` on server
- [ ] `web.config` is in place
- [ ] API responds at https://site29943.siteasp.net
- [ ] Swagger loads successfully

## 🆘 Common Issues

### Issue: 404 on all pages

**Cause:** Files deployed to wrong directory (not in `wwwroot`)

**Solution:** Check GitHub Secret `FTP_SERVER_DIR` is `./wwwroot/`

### Issue: API works but static files don't

**Cause:** Static files not in nested `wwwroot/wwwroot/`

**Solution:** Ensure `UseStaticFiles()` is configured in `Program.cs`

### Issue: 500 Error

**Cause:** `web.config` missing or incorrect

**Solution:** 
1. Verify `web.config` deployed to `./wwwroot/`
2. Check path to DLL in `web.config`: `.\NOL.API.dll`

## 📞 Support

For SiteASP.NET specific issues:
- **Control Panel**: https://panel.siteasp.net
- **Support**: support@siteasp.net
- **Documentation**: https://www.siteasp.net/help

---

**Remember:** Everything goes in `./wwwroot/` on SiteASP.NET! 🎯

