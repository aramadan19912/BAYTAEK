# Quick Start - Deploy to Fly.io in 2 Minutes

## 🎉 Super Simple Setup!

The deployment is now **fully automated**! You only need to add your Fly.io token to GitHub, and everything else happens automatically.

## ✅ Step 1: Add Fly.io Token to GitHub Secrets (REQUIRED)

Your Fly.io token is ready. Now add it to GitHub:

### Option A: Via GitHub Website (Easiest)

1. **Go to your repository settings**:
   ```
   https://github.com/aramadan19912/BAYTAEK/settings/secrets/actions
   ```

2. **Click "New repository secret"**

3. **Add the secret**:
   - Name: `FLY_API_TOKEN`
   - Value: Paste your token (the one you provided)

4. **Click "Add secret"**

### Option B: Via GitHub CLI

```bash
# Install GitHub CLI if not installed
# Windows: winget install GitHub.cli
# Mac: brew install gh

# Login to GitHub
gh auth login

# Add secret
gh secret set FLY_API_TOKEN --body "YOUR_TOKEN_HERE"
```

## ✅ Step 2: Trigger Deployment (Automatic!)

The code is already pushed to GitHub! Once you add the `FLY_API_TOKEN` secret, GitHub Actions will automatically:

1. ✅ Create the `baytaek-api` app (if not exists)
2. ✅ Create the `baytaek-frontend` app (if not exists)
3. ✅ Create the persistent volume for database (if not exists)
4. ✅ Deploy the backend
5. ✅ Deploy the frontend

### Watch Deployment

1. Go to Actions tab:
   ```
   https://github.com/aramadan19912/BAYTAEK/actions
   ```

2. Click on the running workflow "Deploy to Fly.io"

3. Watch the deployment progress (3-5 minutes)

4. Once complete, you'll see:
   ```
   ✅ Backend: https://baytaek-api.fly.dev
   ✅ Frontend: https://baytaek-frontend.fly.dev
   ```

**Or trigger manually**:
```bash
# Trigger a new deployment
git commit --allow-empty -m "Deploy to Fly.io"
git push origin main
```

## ✅ Step 3: Test Your Deployment

### Test Backend

```bash
# Health check
curl https://baytaek-api.fly.dev/health

# Expected response:
# {"status":"Healthy",...}
```

### Test Frontend

Open in browser:
```
https://baytaek-frontend.fly.dev
```

### Test Swagger API Documentation

Open in browser:
```
https://baytaek-api.fly.dev/swagger
```

## 🎉 That's It!

Your application is now deployed to Fly.io for **FREE**!

## 📊 What You Got

✅ **Backend API**: https://baytaek-api.fly.dev
✅ **Frontend**: https://baytaek-frontend.fly.dev
✅ **Auto-deployment**: Push to main = auto deploy
✅ **SSL Certificate**: Automatic HTTPS
✅ **Database**: Persistent SQLite on volume
✅ **Cost**: $0/month (free tier)

## 🔧 Useful Commands

### View Logs

```bash
# Backend logs
flyctl logs -a baytaek-api

# Frontend logs
flyctl logs -a baytaek-frontend

# Follow logs in real-time
flyctl logs -a baytaek-api -f
```

### Check Status

```bash
# App status
flyctl status -a baytaek-api

# Resource usage
flyctl vm status -a baytaek-api

# List all your apps
flyctl apps list
```

### SSH into Container

```bash
# SSH into backend
flyctl ssh console -a baytaek-api

# Check database
flyctl ssh console -a baytaek-api -C "sqlite3 /app/data/homeservice.db '.tables'"
```

### Redeploy

```bash
# Just push to GitHub (automatic)
git push origin main

# Or manual deploy
cd backend
flyctl deploy
```

## ⚠️ Troubleshooting

### Issue: "App not found"

**Solution**: Create the app first:
```bash
flyctl apps create baytaek-api --org personal
```

### Issue: "Volume not found"

**Solution**: Create the volume:
```bash
flyctl volumes create baytaek_data --region fra --size 1 --app baytaek-api
```

### Issue: "Unauthorized"

**Solution**: Set your token:
```bash
flyctl auth token YOUR_TOKEN_HERE
```

### Issue: "Health check failing"

**Solution**: Check logs:
```bash
flyctl logs -a baytaek-api --lines 100
```

## 📚 Next Steps

1. ✅ **Set up custom domain** (optional)
   - See full guide: `docs/FLY_IO_DEPLOYMENT_GUIDE.md`

2. ✅ **Configure production settings**
   - Update CORS in backend
   - Update API URLs in frontend

3. ✅ **Set up monitoring**
   - Fly.io dashboard: https://fly.io/dashboard

4. ✅ **Enable database backups**
   - See backup section in full guide

## 🆘 Need Help?

- **Full Guide**: See `docs/FLY_IO_DEPLOYMENT_GUIDE.md`
- **Fly.io Docs**: https://fly.io/docs/
- **Community**: https://community.fly.io/
- **Status**: https://status.fly.io/

---

**Estimated setup time**: 5-10 minutes
**Cost**: $0/month (free tier)
**Complexity**: ⭐ Easy

🚀 Happy deploying!
