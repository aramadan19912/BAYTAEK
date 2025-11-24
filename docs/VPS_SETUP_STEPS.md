# VPS Setup Steps for Hostinger

## Prerequisites

- SSH access to your VPS at `72.60.234.126`
- GitHub repository secrets configured (VPS_HOST, VPS_USER, VPS_SSH_PRIVATE_KEY)

## Step 1: Connect to Your VPS

```bash
ssh your-username@72.60.234.126
```

Replace `your-username` with your actual Hostinger VPS username (usually `root` or `u123456789`).

## Step 2: Download and Run Setup Script

Once connected to your VPS, run these commands:

```bash
# Download the setup script
curl -O https://raw.githubusercontent.com/aramadan19912/BAYTAEK/main/scripts/vps-setup.sh

# Make it executable
chmod +x vps-setup.sh

# Run the setup script
./vps-setup.sh
```

**What this script does:**
- ✅ Updates system packages
- ✅ Installs Nginx web server
- ✅ Installs .NET 8.0 Runtime for backend
- ✅ Installs Node.js 20.x for frontend
- ✅ Creates application directories at `/var/www/baytaek/`
- ✅ Configures firewall (UFW) to allow HTTP/HTTPS
- ✅ Creates systemd services for backend and frontend
- ✅ Configures Nginx reverse proxy

**Expected output:** You should see green checkmarks (✓) for each step.

## Step 3: Verify Setup

After the setup script completes, check the services:

```bash
# Download health check script
curl -O https://raw.githubusercontent.com/aramadan19912/BAYTAEK/main/scripts/check-deployment.sh

# Make it executable
chmod +x check-deployment.sh

# Run health check
bash check-deployment.sh
```

**Expected output:**
- ✓ Nginx is running
- ✗ Backend API service is not running (normal - not deployed yet)
- ✗ Frontend service is not running (normal - not deployed yet)
- ✓ Port 80 (HTTP) is listening
- ✓ Firewall is active

## Step 4: Verify GitHub Secrets

Make sure these secrets are configured in your GitHub repository:

1. Go to: https://github.com/aramadan19912/BAYTAEK/settings/secrets/actions

2. Verify these secrets exist:
   - `VPS_HOST` = `72.60.234.126`
   - `VPS_USER` = Your SSH username
   - `VPS_SSH_PRIVATE_KEY` = Your private SSH key

### How to Get Your SSH Private Key

If you don't have an SSH key pair yet:

**On your local machine (not VPS):**

```bash
# Generate SSH key pair
ssh-keygen -t ed25519 -C "github-actions@baytaek" -f ~/.ssh/baytaek_deploy

# Display private key (copy this to GitHub secret VPS_SSH_PRIVATE_KEY)
cat ~/.ssh/baytaek_deploy

# Display public key (add this to VPS authorized_keys)
cat ~/.ssh/baytaek_deploy.pub
```

**On your VPS:**

```bash
# Add public key to authorized_keys
mkdir -p ~/.ssh
chmod 700 ~/.ssh
nano ~/.ssh/authorized_keys
# Paste the public key content, save and exit (Ctrl+X, Y, Enter)
chmod 600 ~/.ssh/authorized_keys
```

**Test SSH connection from your computer:**

```bash
ssh -i ~/.ssh/baytaek_deploy your-username@72.60.234.126
```

## Step 5: Trigger Deployment

Now that the VPS is set up, trigger the GitHub Actions deployment:

**Option A: Push to main branch**

```bash
# On your local machine, in the project directory
git push origin main
```

**Option B: Manual trigger via GitHub**

1. Go to: https://github.com/aramadan19912/BAYTAEK/actions
2. Click on "Deploy to Hostinger VPS" workflow
3. Click "Run workflow" → Select "main" branch → Click "Run workflow"

## Step 6: Monitor Deployment

Watch the deployment progress:

1. **On GitHub:** https://github.com/aramadan19912/BAYTAEK/actions
2. **On VPS (in real-time):**

```bash
# Watch backend logs
sudo journalctl -u baytaek-api -f

# In another terminal, watch frontend logs
sudo journalctl -u baytaek-frontend -f

# Check Nginx access log
sudo tail -f /var/log/nginx/access.log
```

## Step 7: Verify Deployment

After deployment completes (usually 3-5 minutes):

**On VPS:**

```bash
# Run health check
bash check-deployment.sh

# Should show:
# ✓ Nginx is running
# ✓ Backend API service is running
# ✓ Backend health check passed
# ✓ Frontend service is running
# ✓ Frontend is accessible
```

**From your computer:**

```bash
# Test backend API
curl http://72.60.234.126/api/health

# Expected response:
# {"status":"Healthy","checks":{...}}

# Test frontend (in browser)
# Open: http://72.60.234.126
```

## Step 8: Configure Database (If Needed)

If you have a database, you need to configure the connection string:

**On VPS:**

```bash
# Edit systemd service
sudo nano /etc/systemd/system/baytaek-api.service

# Add database connection string environment variable:
Environment=ConnectionStrings__DefaultConnection="Server=your-db-server;Database=HomeServiceDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"

# Save and reload
sudo systemctl daemon-reload
sudo systemctl restart baytaek-api
```

## Troubleshooting

If anything goes wrong, refer to:
- **Comprehensive guide:** `docs/TROUBLESHOOTING.md`
- **Quick diagnostics:** Run `bash check-deployment.sh` on VPS

### Common Issues

**1. Connection refused after setup:**
```bash
# Check if Nginx is running
sudo systemctl status nginx
sudo systemctl start nginx
```

**2. 502 Bad Gateway:**
```bash
# Check backend logs
sudo journalctl -u baytaek-api -n 50
# Most likely: backend not deployed yet or failed to start
```

**3. Services not starting after deployment:**
```bash
# Check permissions
sudo chown -R www-data:www-data /var/www/baytaek
sudo chmod +x /var/www/baytaek/backend/HomeService.API

# Restart services
sudo systemctl restart baytaek-api
sudo systemctl restart baytaek-frontend
```

## Optional: Configure SSL/HTTPS

After everything is working on HTTP, you can add SSL:

```bash
# Install certbot (already done by setup script)
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Follow prompts, certbot will automatically configure Nginx
```

## Summary

After completing all steps:

✅ VPS is configured with all required software
✅ Services are created and enabled
✅ Firewall is configured
✅ GitHub Actions can deploy automatically
✅ Application is accessible at: http://72.60.234.126

**Next Steps:**
- Point your domain to `72.60.234.126`
- Configure SSL with Let's Encrypt
- Set up database backups
- Configure monitoring

## Quick Reference

```bash
# Restart all services
sudo systemctl restart baytaek-api baytaek-frontend nginx

# View logs
sudo journalctl -u baytaek-api -f
sudo journalctl -u baytaek-frontend -f
sudo tail -f /var/log/nginx/error.log

# Health check
bash check-deployment.sh

# Test endpoints
curl http://localhost:9000/health
curl http://localhost:5001
curl http://localhost/api/health
```

## Support

If you encounter issues:
1. Run `bash check-deployment.sh` and review output
2. Check `docs/TROUBLESHOOTING.md`
3. Review service logs: `sudo journalctl -u baytaek-api -n 100`
