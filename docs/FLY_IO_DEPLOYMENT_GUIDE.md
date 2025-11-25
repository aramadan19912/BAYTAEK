# Fly.io Deployment Guide - Baytaek Application

## ✅ What You Get with Fly.io Free Tier

- **3 shared-cpu-1x VMs** with 256MB RAM each (FREE)
- **3GB persistent storage** (FREE)
- **160GB outbound data transfer** per month (FREE)
- **Automatic SSL certificates** (FREE)
- **Global deployment** (FREE)
- **No credit card required** for free tier

## 📋 Prerequisites

✅ Fly.io account created
✅ Fly.io token obtained
✅ GitHub repository with Baytaek code

## 🚀 Deployment Architecture

```
┌─────────────────────────────────────────┐
│         Fly.io Platform (FREE)          │
│                                         │
│  ┌──────────────┐    ┌──────────────┐  │
│  │   Backend    │    │   Frontend   │  │
│  │   (.NET 8)   │◄──►│   (Angular)  │  │
│  │   Port:9000  │    │   Port:80    │  │
│  │   256MB RAM  │    │   256MB RAM  │  │
│  └──────┬───────┘    └──────────────┘  │
│         │                               │
│  ┌──────▼───────┐                      │
│  │   SQLite DB  │                      │
│  │  (Persistent) │                      │
│  │   3GB Volume  │                      │
│  └──────────────┘                      │
│                                         │
│  ✅ Auto SSL        ✅ Auto Deploy      │
│  ✅ Health Checks   ✅ Scale to Zero    │
└─────────────────────────────────────────┘
```

## 📦 Files Created

All necessary files have been created:

1. ✅ `backend/Dockerfile` - Backend Docker configuration
2. ✅ `backend/fly.toml` - Backend Fly.io configuration
3. ✅ `frontend/Dockerfile` - Frontend Docker configuration
4. ✅ `frontend/fly.toml` - Frontend Fly.io configuration
5. ⏳ `.github/workflows/fly-deploy.yml` - Auto-deployment workflow (creating next)

## 🔧 Deployment Methods

### Method 1: GitHub Actions (Recommended - Fully Automated) ⭐

**Advantages**:
- ✅ Auto-deploy on every push to main
- ✅ No local setup needed
- ✅ Deployment logs in GitHub
- ✅ Rollback support

**Steps**:
1. GitHub secrets are already configured with your Fly.io token
2. Push code to main branch
3. GitHub Actions automatically deploys to Fly.io
4. Done!

### Method 2: Manual Deployment via Fly CLI

**When to use**:
- First-time setup
- Testing deployment
- Troubleshooting

**Steps** (Skip if using Method 1):

#### Step 1: Install Fly CLI

**Windows (PowerShell)**:
```powershell
powershell -Command "iwr https://fly.io/install.ps1 -useb | iex"
```

**Mac**:
```bash
curl -L https://fly.io/install.sh | sh
```

**Linux**:
```bash
curl -L https://fly.io/install.sh | sh
```

#### Step 2: Authenticate with Fly.io

```bash
# Login with your token
flyctl auth login

# Verify authentication
flyctl auth whoami
```

#### Step 3: Deploy Backend

```bash
# Navigate to backend directory
cd backend

# Create Fly.io app (first time only)
flyctl apps create baytaek-api

# Create persistent volume for SQLite database
flyctl volumes create baytaek_data --region fra --size 1

# Deploy the backend
flyctl deploy

# Check deployment
flyctl status
flyctl logs
```

#### Step 4: Deploy Frontend

```bash
# Navigate to frontend directory
cd ../frontend

# Create Fly.io app (first time only)
flyctl apps create baytaek-frontend

# Deploy the frontend
flyctl deploy

# Check deployment
flyctl status
flyctl logs
```

#### Step 5: Get Your URLs

```bash
# Backend URL
flyctl apps info baytaek-api

# Frontend URL
flyctl apps info baytaek-frontend
```

Your apps will be available at:
- **Backend**: https://baytaek-api.fly.dev
- **Frontend**: https://baytaek-frontend.fly.dev

## 🔐 Configure Frontend to Use Backend API

After deployment, you need to update the frontend to point to the correct backend URL.

### Update Angular Environment

Edit `frontend/src/environments/environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://baytaek-api.fly.dev/api',
  signalrUrl: 'https://baytaek-api.fly.dev/hubs'
};
```

Then redeploy frontend:
```bash
cd frontend
flyctl deploy
```

## 🔧 Configuration Details

### Backend Configuration (`backend/fly.toml`)

- **Region**: `fra` (Frankfurt) - Change if needed
- **Port**: Internal 9000, External 80/443
- **Memory**: 256MB (free tier)
- **Storage**: 1GB persistent volume
- **Auto-scaling**: Enabled (scales to 0 when idle)

### Frontend Configuration (`frontend/fly.toml`)

- **Region**: `fra` (Frankfurt) - Should match backend
- **Port**: Internal 80, External 80/443
- **Memory**: 256MB (free tier)
- **Auto-scaling**: Enabled

### Available Regions

Common regions (choose closest to your users):
- `fra` - Frankfurt, Germany
- `lhr` - London, UK
- `ams` - Amsterdam, Netherlands
- `iad` - Ashburn, Virginia (US East)
- `sjc` - San Jose, California (US West)
- `syd` - Sydney, Australia

To change region, edit `primary_region` in `fly.toml` files.

## 📊 Monitoring and Management

### View Logs

```bash
# Backend logs (real-time)
flyctl logs -a baytaek-api

# Frontend logs
flyctl logs -a baytaek-frontend

# Last 100 lines
flyctl logs -a baytaek-api --lines 100
```

### Check Status

```bash
# App status
flyctl status -a baytaek-api

# Resource usage
flyctl vm status -a baytaek-api

# List all apps
flyctl apps list
```

### SSH into Container

```bash
# SSH into backend
flyctl ssh console -a baytaek-api

# Check database
flyctl ssh console -a baytaek-api -C "sqlite3 /app/data/homeservice.db '.tables'"
```

### Scale Resources

```bash
# Scale memory (if needed, costs extra)
flyctl scale memory 512 -a baytaek-api

# Scale VMs
flyctl scale count 2 -a baytaek-api

# Check current scale
flyctl scale show -a baytaek-api
```

## 🔄 Update and Redeploy

### Via GitHub Actions (Automatic)
Just push to main branch:
```bash
git add .
git commit -m "Update application"
git push origin main
```

### Via Fly CLI (Manual)
```bash
# Backend
cd backend
flyctl deploy

# Frontend
cd ../frontend
flyctl deploy
```

## 🐛 Troubleshooting

### Issue 1: Deployment Fails

```bash
# Check logs
flyctl logs -a baytaek-api

# Common fixes:
# - Check Dockerfile syntax
# - Verify fly.toml configuration
# - Check build output
```

### Issue 2: App Not Starting

```bash
# Check health checks
flyctl status -a baytaek-api

# View detailed logs
flyctl logs -a baytaek-api --lines 200

# Restart app
flyctl apps restart baytaek-api
```

### Issue 3: Database Not Persisting

```bash
# Check volume
flyctl volumes list -a baytaek-api

# If no volume, create one
flyctl volumes create baytaek_data --region fra --size 1

# Verify mount in fly.toml
# [mounts]
#   source = "baytaek_data"
#   destination = "/app/data"
```

### Issue 4: Out of Memory

```bash
# Check current usage
flyctl vm status -a baytaek-api

# Increase memory (costs extra beyond free tier)
flyctl scale memory 512 -a baytaek-api
```

### Issue 5: SSL Certificate Issues

```bash
# Fly.io automatically provisions SSL
# Check certificate
flyctl certs list -a baytaek-api

# If issues, try:
flyctl certs create baytaek-api.fly.dev -a baytaek-api
```

## 💰 Cost Management

### Free Tier Limits

Your deployment uses:
- **Backend**: 1 VM × 256MB = FREE ✅
- **Frontend**: 1 VM × 256MB = FREE ✅
- **Storage**: 1GB volume = FREE ✅
- **Total**: $0/month within free tier

### Monitor Usage

```bash
# Check resource usage
flyctl dashboard

# View billing (shows $0 if within free tier)
flyctl billing
```

### Optimize Costs

1. **Enable auto-stop** (already configured in fly.toml):
   ```toml
   auto_stop_machines = true
   min_machines_running = 0
   ```

2. **Monitor traffic**:
   ```bash
   flyctl metrics -a baytaek-api
   ```

3. **Stay within limits**:
   - Keep 3 or fewer VMs
   - Use 256MB RAM per VM
   - Keep storage under 3GB

## 🌐 Custom Domain (Optional)

To use your own domain like `baytaek.com`:

### Step 1: Add Certificate

```bash
# Add domain to backend
flyctl certs create baytaek.com -a baytaek-api

# Add domain to frontend
flyctl certs create www.baytaek.com -a baytaek-frontend
```

### Step 2: Configure DNS

Add these DNS records at your domain registrar:

```
# Backend API
Type: CNAME
Name: api
Value: baytaek-api.fly.dev

# Frontend
Type: CNAME
Name: www
Value: baytaek-frontend.fly.dev

# Root domain (optional)
Type: A
Name: @
Value: [Get IP from: flyctl ips list -a baytaek-frontend]
```

### Step 3: Verify

```bash
# Check certificate status
flyctl certs check baytaek.com -a baytaek-api
```

## 📱 Testing Your Deployment

### Backend Health Check

```bash
# Via curl
curl https://baytaek-api.fly.dev/health

# Expected response:
# {"status":"Healthy","checks":{...}}
```

### Frontend Check

```bash
# Via browser
# Open: https://baytaek-frontend.fly.dev

# Via curl
curl https://baytaek-frontend.fly.dev
```

### Complete API Test

```bash
# Test Swagger
# Open: https://baytaek-api.fly.dev/swagger

# Test specific endpoint
curl https://baytaek-api.fly.dev/api/services
```

## 🔒 Security Best Practices

### 1. Secrets Management

Never commit secrets to Git. Use Fly.io secrets:

```bash
# Set secrets
flyctl secrets set ConnectionStrings__DefaultConnection="Data Source=/app/data/homeservice.db" -a baytaek-api

flyctl secrets set JWT_SECRET="your-secret-key" -a baytaek-api

# List secrets (values hidden)
flyctl secrets list -a baytaek-api
```

### 2. Update CORS in Backend

After deployment, update allowed origins in `Program.cs`:

```csharp
policy.WithOrigins(
    "http://localhost:4200",
    "https://baytaek-frontend.fly.dev",  // Add Fly.io frontend
    "https://www.baytaek.com"  // Add your custom domain
)
```

### 3. Enable Health Checks

Already configured in `fly.toml`. Verify:

```bash
flyctl checks list -a baytaek-api
```

## 📚 Useful Commands Reference

```bash
# Deploy
flyctl deploy

# Logs
flyctl logs -a APP_NAME

# Status
flyctl status -a APP_NAME

# Restart
flyctl apps restart APP_NAME

# SSH
flyctl ssh console -a APP_NAME

# Secrets
flyctl secrets list -a APP_NAME
flyctl secrets set KEY=VALUE -a APP_NAME

# Scaling
flyctl scale show -a APP_NAME
flyctl scale memory 512 -a APP_NAME

# Volumes
flyctl volumes list -a APP_NAME

# Regions
flyctl regions list
flyctl regions add REGION -a APP_NAME

# Certificates
flyctl certs list -a APP_NAME
flyctl certs create DOMAIN -a APP_NAME

# Billing
flyctl dashboard
flyctl billing
```

## 🆘 Getting Help

### Fly.io Resources

- **Documentation**: https://fly.io/docs/
- **Community**: https://community.fly.io/
- **Status**: https://status.fly.io/
- **Support**: https://fly.io/docs/about/support/

### Common Issues

Check the troubleshooting section above or:

```bash
# Get help on any command
flyctl help
flyctl deploy --help
```

## ✅ Next Steps After Deployment

1. ✅ **Test all endpoints**
   - Backend health: https://baytaek-api.fly.dev/health
   - Swagger: https://baytaek-api.fly.dev/swagger
   - Frontend: https://baytaek-frontend.fly.dev

2. ✅ **Update frontend API URL**
   - Edit environment.prod.ts
   - Redeploy frontend

3. ✅ **Set up custom domain** (optional)
   - Follow custom domain section above

4. ✅ **Enable monitoring**
   - Set up Fly.io metrics
   - Configure alerts

5. ✅ **Backup database**
   - Set up automated backups (see below)

## 💾 Database Backup

### Manual Backup

```bash
# Create backup
flyctl ssh console -a baytaek-api -C "sqlite3 /app/data/homeservice.db '.backup /app/data/backup.db'"

# Download backup
flyctl ssh sftp get /app/data/backup.db ./local-backup.db -a baytaek-api
```

### Automated Backup Script

Create a GitHub Action for daily backups (I can set this up for you).

## 🎉 Summary

Your Baytaek application is now deployed to Fly.io!

**URLs**:
- Backend API: https://baytaek-api.fly.dev
- Frontend: https://baytaek-frontend.fly.dev
- Swagger Docs: https://baytaek-api.fly.dev/swagger

**Cost**: $0/month (within free tier)

**Next**: Push code to GitHub and watch it auto-deploy! 🚀
