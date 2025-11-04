# ✅ Pre-Deployment Checklist

## Run This Before Deploying!

### 1️⃣ Test Locally First

```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental/src/NOL.API"

# Test in Production mode
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

**Expected:** Application starts without errors

---

### 2️⃣ Check Required Files Exist

```bash
# Check web.config
ls -la src/NOL.API/web.config

# Check appsettings
ls -la src/NOL.API/appsettings.Production.json

# Expected: Both files exist
```

---

### 3️⃣ Test Build

```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental"

# Clean build
dotnet clean
dotnet build -c Release

# Expected: Build succeeded, 0 errors
```

---

### 4️⃣ Test Publish

```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental/src/NOL.API"

# Publish
dotnet publish -c Release -o ./test-publish

# Check output
ls -la ./test-publish/
```

**Must have:**
- ✅ NOL.API.dll
- ✅ web.config
- ✅ appsettings.json
- ✅ appsettings.Production.json
- ✅ All dependencies (.dll files)

---

### 5️⃣ Verify GitHub Secrets

Go to: GitHub → Settings → Secrets → Actions

**Required secrets:**

```
✅ FTP_SERVER = site29943.siteasp.net
✅ FTP_USERNAME = site29943
✅ FTP_PASSWORD = sE_3J#5ptZ%8
✅ FTP_SERVER_DIR = ./wwwroot/
✅ APP_URL = https://site29943.siteasp.net
```

---

### 6️⃣ Check Database Connection

Test connection string:

```bash
# In appsettings.Production.json
"DefaultConnection": "Server=db22486.public.databaseasp.net; Database=db22486; User Id=db22486; Password=6h+E!7Rq8Jc_; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;"
```

**Test it:**
```bash
dotnet ef database update --project src/NOL.Infrastructure --startup-project src/NOL.API --configuration Release
```

---

### 7️⃣ Check web.config is Valid

```bash
cat src/NOL.API/web.config
```

**Must have:**
```xml
<aspNetCore processPath="dotnet" 
            arguments=".\NOL.API.dll"
```

**No duplicate sections!**

---

### 8️⃣ Test GitHub Actions Locally

```bash
# Install act (GitHub Actions locally)
# https://github.com/nektos/act

# Run workflow
act -j build
```

---

## 🚀 Ready to Deploy?

If all checks pass:

```bash
git add .
git commit -m "Ready for deployment"
git push
```

Then:
1. ✅ Go to GitHub → Actions
2. ✅ Watch deployment
3. ✅ Check logs
4. ✅ Test site

---

## ❌ If Any Check Fails:

**Build fails:**
- Fix code errors
- Check dependencies
- Restore packages

**Publish fails:**
- Check project references
- Verify all projects build

**Database test fails:**
- Check connection string
- Verify database server is accessible
- Check firewall rules

**web.config invalid:**
- Use the fixed version from repository
- Check for duplicate sections

---

## 📊 After Deployment

### Immediate Checks (within 2 minutes):

```bash
# 1. Check if site responds
curl -I https://site29943.siteasp.net

# 2. Test API endpoint
curl https://site29943.siteasp.net/api/enums/booking-statuses

# 3. Check Seq logs
# Visit: https://seq-production-43df.up.railway.app/
# Filter: Environment = 'Production'
```

### Within 5 minutes:

- [ ] Check server logs via FTP: `wwwroot/logs/stdout*.log`
- [ ] Test Swagger UI: https://site29943.siteasp.net
- [ ] Test a POST endpoint
- [ ] Check database connectivity

### Within 30 minutes:

- [ ] Monitor Seq for errors
- [ ] Test all major features
- [ ] Check performance
- [ ] Verify static files load

---

## 🆘 Quick Fix Commands

```bash
# Force rebuild
dotnet clean
dotnet build -c Release --no-incremental

# Restore all packages
dotnet restore --force

# Clear local cache
dotnet nuget locals all --clear

# Rebuild and push
git add .
git commit -m "Force rebuild"
git push
```

---

## ✅ Success Criteria

**Before deployment:**
- [x] All files build successfully
- [x] Tests pass (if any)
- [x] Local run works
- [x] GitHub secrets configured
- [x] web.config is valid

**After deployment:**
- [x] GitHub Actions shows success
- [x] Files exist in wwwroot/
- [x] No errors in stdout.log
- [x] API responds to requests
- [x] Logs appear in Seq

---

**Everything checked? Deploy with confidence! 🚀**

