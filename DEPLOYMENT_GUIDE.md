# 🚀 Deployment Guide - GitHub Actions to SiteASP.NET

## Overview

Your NOL Car Rental API is configured to automatically deploy to **site29943.siteasp.net** using GitHub Actions.

**Hosting Details:**
- **Server**: site29943.siteasp.net
- **URL**: https://site29943.siteasp.net
- **Platform**: Windows Server with IIS / ASP.NET Core
- **Deployment Method**: FTP (via GitHub Actions)

---

## 🔧 Setup Instructions

### Step 1: Set Up GitHub Repository Secrets

GitHub Secrets store sensitive information securely. You need to add your hosting credentials.

#### Go to GitHub Secrets:

1. Go to your GitHub repository
2. Click **Settings** (top menu)
3. Click **Secrets and variables** → **Actions** (left sidebar)
4. Click **New repository secret**

#### Add These Secrets:

| Secret Name | Value | Description |
|------------|-------|-------------|
| `FTP_SERVER` | `site29943.siteasp.net` | FTP server address |
| `FTP_USERNAME` | `site29943` | FTP username |
| `FTP_PASSWORD` | `sE_3J#5ptZ%8` | FTP password |
| `FTP_SERVER_DIR` | `./` | Root directory (or specific path) |
| `APP_URL` | `https://site29943.siteasp.net` | Your app URL |

**Important:** Never commit passwords to GitHub! Always use Secrets.

---

### Step 2: Configure Your Repository

#### A. Initialize Git (if not already done)

```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental"
git init
git add .
git commit -m "Initial commit with GitHub Actions deployment"
```

#### B. Create GitHub Repository

1. Go to https://github.com/new
2. Create a new repository (e.g., `NOLCarRental`)
3. **Don't** initialize with README (you already have files)

#### C. Push to GitHub

```bash
git remote add origin https://github.com/YOUR_USERNAME/NOLCarRental.git
git branch -M main
git push -u origin main
```

---

## 🎯 How Deployment Works

### Automatic Deployment Trigger:

✅ **When you push to `main` or `master` branch**

```bash
git add .
git commit -m "Update feature X"
git push
```

### What Happens:

1. **Build** 🔨
   - Checkout code
   - Setup .NET 8
   - Restore packages
   - Build project
   - Run tests
   - Publish application

2. **Deploy** 🚀
   - Upload to SiteASP.NET via FTP
   - Deploy to production server
   - Application automatically restarts

3. **Notify** 📢
   - Shows deployment status
   - Displays app URL

---

## 📁 Files Deployed

The following files are deployed to your server:

```
site29943.siteasp.net/
├── NOL.API.dll
├── appsettings.json
├── appsettings.Production.json
├── web.config (IIS configuration)
├── wwwroot/ (static files)
├── All dependencies (.dll files)
└── logs/ (created automatically)
```

**Note:** `appsettings.Development.json` and `.pdb` files are **excluded** from deployment.

---

## 🔍 Monitor Deployment

### View GitHub Actions Status:

1. Go to your GitHub repository
2. Click **Actions** tab
3. You'll see all deployment runs
4. Click on any run to see details

### Deployment Stages:

- ✅ **Build** - Compiles your application
- ✅ **Deploy** - Uploads to server  
- ✅ **Notify** - Shows status

### Check Logs:

- **GitHub**: Actions tab → Click workflow run → View logs
- **Server**: Check `logs/` folder on your server
- **Seq**: https://seq-production-43df.up.railway.app/

---

## 🌐 Access Your Deployed API

### Base URL:
```
https://site29943.siteasp.net
```

### Swagger UI:
```
https://site29943.siteasp.net
```

### Test Endpoint:
```
https://site29943.siteasp.net/api/enums/booking-statuses
```

### Health Check (if you add one):
```
https://site29943.siteasp.net/health
```

---

## 🧪 Testing After Deployment

### Test 1: Basic API Call

```bash
curl https://site29943.siteasp.net/api/enums/booking-statuses
```

### Test 2: With Headers

```bash
curl -H "Accept-Language: ar" https://site29943.siteasp.net/api/enums/booking-statuses
```

### Test 3: Swagger UI

Open in browser:
```
https://site29943.siteasp.net
```

### Test 4: Check Logs in Seq

```
https://seq-production-43df.up.railway.app/
```

Filter by: `Environment = 'Production'`

---

## 🔐 Security Checklist

Before going live, ensure:

- [x] GitHub Secrets are configured (not in code)
- [x] Strong JWT SecretKey in `appsettings.Production.json`
- [x] Database connection string is secure
- [x] HTTPS is enabled
- [x] CORS is properly configured
- [x] Email confirmation is enabled
- [x] Rate limiting is configured (if needed)
- [x] Hangfire dashboard is protected
- [x] Exception details are hidden in production

---

## 📝 Common Deployment Scenarios

### Scenario 1: Deploy a New Feature

```bash
# 1. Make your changes
git add .
git commit -m "Add new booking feature"

# 2. Push to trigger deployment
git push

# 3. Monitor in GitHub Actions
# Go to: https://github.com/YOUR_USERNAME/NOLCarRental/actions

# 4. Verify deployment
curl https://site29943.siteasp.net/api/test
```

### Scenario 2: Rollback to Previous Version

```bash
# 1. Find the commit hash of the working version
git log

# 2. Revert to that commit
git revert COMMIT_HASH

# 3. Push (triggers redeployment)
git push
```

### Scenario 3: Manual Deployment

Go to GitHub → Actions → "Build and Deploy to Hosting" → Click "Run workflow"

---

## 🛠️ Troubleshooting

### Issue 1: Deployment Fails

**Check:**
1. GitHub Actions logs for errors
2. FTP credentials in Secrets
3. Server disk space
4. File permissions on server

**Solution:**
```bash
# Re-run the workflow
# GitHub → Actions → Failed workflow → "Re-run jobs"
```

### Issue 2: Application Won't Start

**Check:**
1. `web.config` is present
2. .NET Runtime is installed on server
3. Application pool is running
4. Check server event logs

**Solution:**
- Contact SiteASP.NET support
- Check `logs/stdout` on server

### Issue 3: Database Connection Failed

**Check:**
1. Connection string in `appsettings.Production.json`
2. Database server is accessible
3. Firewall allows connection

**Solution:**
```json
// Update appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CORRECT_CONNECTION_STRING"
  }
}
```

### Issue 4: 500 Internal Server Error

**Check:**
1. Logs in Seq: https://seq-production-43df.up.railway.app/
2. `logs/` folder on server
3. Exception details in Seq

**Solution:**
- Check Seq for exception details
- Fix the error
- Commit and push

---

## 📊 Monitoring Production

### Check Application Health:

```bash
# CPU & Memory
# Check in SiteASP.NET control panel

# API Performance
# Check Seq logs for slow requests
# Query: Elapsed > 1000

# Errors
# Check Seq logs
# Query: @Level = 'Error' and Environment = 'Production'
```

### Set Up Alerts (Optional):

In Seq:
1. Go to Settings → Alerts
2. Create alert for errors
3. Configure email/webhook notification

---

## 🔄 Continuous Integration/Deployment (CI/CD) Flow

```
Developer makes changes
        ↓
   git commit & push
        ↓
GitHub Actions triggered
        ↓
Build & Test
        ↓
Create artifact
        ↓
Deploy via FTP
        ↓
Application restarts
        ↓
Production updated ✅
        ↓
Logs sent to Seq
```

---

## 📋 Pre-Deployment Checklist

Before deploying to production:

- [ ] All tests pass locally
- [ ] Database migrations are ready
- [ ] Environment variables are set
- [ ] Secrets are configured in GitHub
- [ ] `appsettings.Production.json` is correct
- [ ] CORS settings allow your frontend
- [ ] Email settings are configured
- [ ] Seq logging is working
- [ ] Exception handling is tested
- [ ] API documentation is up to date

---

## 🎯 Post-Deployment Tasks

After successful deployment:

1. **Test the API**
   ```bash
   curl https://site29943.siteasp.net/api/enums/booking-statuses
   ```

2. **Check Logs in Seq**
   - Look for startup logs
   - Check for errors
   - Verify logging is working

3. **Test Key Features**
   - User registration
   - Login
   - Create booking
   - Payment flow

4. **Monitor Performance**
   - Check response times
   - Monitor database queries
   - Check memory usage

5. **Update Documentation**
   - Update API URL in docs
   - Update Postman collections
   - Inform frontend team

---

## 📞 Support

### SiteASP.NET Support:
- **Website**: https://www.siteasp.net
- **Email**: support@siteasp.net
- **Control Panel**: https://panel.siteasp.net

### GitHub Actions:
- **Documentation**: https://docs.github.com/en/actions
- **Status**: https://www.githubstatus.com/

### Seq Logging:
- **Dashboard**: https://seq-production-43df.up.railway.app/
- **Documentation**: https://docs.datalust.co/

---

## 🚀 Quick Reference Commands

```bash
# Deploy to production
git push

# Check deployment status
# GitHub → Actions tab

# View logs
# Seq: https://seq-production-43df.up.railway.app/

# Rollback
git revert HEAD
git push

# Manual trigger
# GitHub → Actions → Run workflow
```

---

## ✅ You're Ready!

Your deployment pipeline is configured and ready to use.

**Next Steps:**
1. ✅ Add GitHub Secrets (Step 1)
2. ✅ Push code to GitHub
3. ✅ Watch automatic deployment
4. ✅ Test your API at https://site29943.siteasp.net

**Happy Deploying! 🎉**

