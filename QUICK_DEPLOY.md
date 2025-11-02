# 🚀 Quick Deployment Guide

## ⚡ 3-Step Setup

### Step 1: Add GitHub Secrets

Go to your GitHub repository → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

Add these 5 secrets:

| Name | Value |
|------|-------|
| `FTP_SERVER` | `site29943.siteasp.net` |
| `FTP_USERNAME` | `site29943` |
| `FTP_PASSWORD` | `sE_3J#5ptZ%8` |
| `FTP_SERVER_DIR` | `./` |
| `APP_URL` | `https://site29943.siteasp.net` |

### Step 2: Push to GitHub

```bash
cd "/media/eslam/New Volume/NOL Work/NOLCarRental"

# Initialize git (if not already done)
git init
git add .
git commit -m "Initial commit - ready for deployment"

# Create repository on GitHub first, then:
git remote add origin https://github.com/YOUR_USERNAME/NOLCarRental.git
git branch -M main
git push -u origin main
```

### Step 3: Automatic Deployment

✅ **Done!** Every push to `main` branch will automatically deploy.

Watch the deployment:
- **GitHub**: Go to Actions tab
- **Live Site**: https://site29943.siteasp.net
- **Logs**: https://seq-production-43df.up.railway.app/

---

## 🎯 Test Your Deployed API

### Test Endpoint:
```bash
curl https://site29943.siteasp.net/api/enums/booking-statuses
```

### Open Swagger:
```
https://site29943.siteasp.net
```

### Check Logs:
```
https://seq-production-43df.up.railway.app/
```

---

## 🔄 Deploy Updates

```bash
# Make changes
git add .
git commit -m "Your update message"
git push
```

**Deployment happens automatically!** ✨

---

## 📊 Monitoring

- **Deployment Status**: GitHub → Actions tab
- **Application Logs**: https://seq-production-43df.up.railway.app/
- **Server Status**: SiteASP control panel

---

## ⚠️ Important Notes

1. **Never commit passwords** - Use GitHub Secrets
2. **Test locally first** - Before pushing to production
3. **Monitor Seq logs** - After deployment
4. **Check Actions tab** - If deployment fails

---

## 🆘 Quick Troubleshooting

### Deployment Failed?
- Check GitHub Actions logs
- Verify FTP credentials in Secrets
- Re-run the workflow

### API Not Working?
- Check Seq logs for errors
- Verify database connection
- Check `web.config` on server

---

## 📚 Full Documentation

For complete guide: **`DEPLOYMENT_GUIDE.md`**

---

## ✅ Checklist

- [ ] GitHub Secrets configured
- [ ] Code pushed to GitHub
- [ ] Deployment successful
- [ ] API tested and working
- [ ] Logs visible in Seq

**You're all set! 🎉**

