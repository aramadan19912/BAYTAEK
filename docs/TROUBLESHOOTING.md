# Troubleshooting Guide - Baytaek VPS Deployment

## Issue: "Site can't be reached" or Connection Timeout

### Quick Diagnosis Commands

Run these commands on your VPS to diagnose the issue:

```bash
# 1. Check if Nginx is running
sudo systemctl status nginx

# 2. Check if backend API is running
sudo systemctl status baytaek-api

# 3. Check if frontend is running
sudo systemctl status baytaek-frontend

# 4. Check which ports are listening
sudo netstat -tulpn | grep -E ':(80|443|9000|5001)'

# 5. Test backend locally
curl http://localhost:9000/health

# 6. Test frontend locally
curl http://localhost:5001

# 7. Test via Nginx
curl http://localhost/api/health

# 8. Check firewall status
sudo ufw status

# 9. Check recent logs
sudo journalctl -u baytaek-api -n 50 --no-pager
sudo journalctl -u baytaek-frontend -n 50 --no-pager
sudo tail -30 /var/log/nginx/error.log
```

### Common Issues and Solutions

#### 1. Nginx Not Running

**Symptoms:** Connection refused, can't reach site

**Check:**
```bash
sudo systemctl status nginx
```

**Solution:**
```bash
# Start Nginx
sudo systemctl start nginx

# If configuration error
sudo nginx -t
# Fix any errors shown, then:
sudo systemctl restart nginx
```

#### 2. Backend API Not Running

**Symptoms:** 502 Bad Gateway on /api/ routes

**Check:**
```bash
sudo systemctl status baytaek-api
sudo journalctl -u baytaek-api -n 100 --no-pager
```

**Solutions:**

**If service not found:**
```bash
# Run the VPS setup script
cd /path/to/baytaek
bash scripts/vps-setup.sh
```

**If service failed to start:**
```bash
# Check logs for errors
sudo journalctl -u baytaek-api -n 100 --no-pager

# Common issues:
# - Missing executable: Deploy backend first
# - Permission denied:
sudo chmod +x /var/www/baytaek/backend/HomeService.API
sudo chown -R www-data:www-data /var/www/baytaek/backend

# - Database connection error: Update connection string
# - Port already in use:
sudo netstat -tulpn | grep 9000
# Kill process using port 9000 if needed

# Restart service
sudo systemctl restart baytaek-api
```

**If backend not deployed:**
```bash
# Backend hasn't been deployed yet
# Trigger GitHub Actions deployment or deploy manually
```

#### 3. Frontend Not Running

**Symptoms:** Main page shows error or Nginx default page

**Check:**
```bash
sudo systemctl status baytaek-frontend
sudo journalctl -u baytaek-frontend -n 100 --no-pager
```

**Solutions:**

**If service not found:**
```bash
# Run VPS setup script to create service
bash scripts/vps-setup.sh
```

**If service failed:**
```bash
# Check if serve is installed
which serve
# If not:
sudo npm install -g serve

# Check if files exist
ls -la /var/www/baytaek/frontend/dist/frontend/browser/

# If empty, frontend hasn't been deployed yet
# Trigger GitHub Actions or deploy manually

# Fix permissions
sudo chown -R www-data:www-data /var/www/baytaek/frontend

# Restart
sudo systemctl restart baytaek-frontend
```

#### 4. Firewall Blocking Connections

**Symptoms:** Timeout from external connections, but localhost works

**Check:**
```bash
sudo ufw status
```

**Solution:**
```bash
# Enable firewall with correct rules
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw --force enable
sudo ufw status

# Test from external
curl http://YOUR_VPS_IP
```

#### 5. Ports Not Listening

**Symptoms:** "Connection refused" errors

**Check:**
```bash
sudo netstat -tulpn | grep -E ':(80|9000|5001)'
```

**Expected output:**
```
tcp    0.0.0.0:80         0.0.0.0:*    LISTEN    1234/nginx
tcp    0.0.0.0:9000       0.0.0.0:*    LISTEN    5678/HomeService.API
tcp    127.0.0.1:5001     0.0.0.0:*    LISTEN    9012/node
```

**Solutions:**

**If port 80 not listening:**
```bash
sudo systemctl start nginx
```

**If port 9000 not listening:**
```bash
sudo systemctl start baytaek-api
# Check logs if it fails to start
sudo journalctl -u baytaek-api -n 50
```

**If port 5001 not listening:**
```bash
sudo systemctl start baytaek-frontend
```

#### 6. Database Connection Issues

**Symptoms:** Backend starts but returns errors, 500 errors on API calls

**Check logs:**
```bash
sudo journalctl -u baytaek-api -n 100 --no-pager | grep -i "database\|connection"
```

**Solution:**
```bash
# Update connection string in environment or appsettings
# For production, use environment variables:
sudo nano /etc/systemd/system/baytaek-api.service

# Add:
Environment=ConnectionStrings__DefaultConnection="Server=YOUR_DB;Database=HomeServiceDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"

# Reload and restart
sudo systemctl daemon-reload
sudo systemctl restart baytaek-api
```

#### 7. Nginx Configuration Errors

**Symptoms:** nginx -t shows errors

**Check:**
```bash
sudo nginx -t
```

**Solution:**
```bash
# Common issues:
# - Syntax error: Fix the error shown
# - Duplicate server_name: Remove conflicting configs
# - SSL certificate not found: Comment out SSL sections until certificates are ready

# Edit config
sudo nano /etc/nginx/sites-available/baytaek

# Test again
sudo nginx -t

# Reload
sudo systemctl reload nginx
```

#### 8. SSL/HTTPS Issues

**Symptoms:** HTTPS not working, certificate errors

**Check:**
```bash
# Check if certbot installed
which certbot

# Check existing certificates
sudo certbot certificates
```

**Solution:**
```bash
# Install certbot
sudo apt install certbot python3-certbot-nginx

# Obtain certificate (DNS must be pointing to your server)
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Test auto-renewal
sudo certbot renew --dry-run
```

### Step-by-Step First Time Setup

If this is your first deployment:

#### Step 1: Run VPS Setup Script

```bash
# SSH into your VPS
ssh user@72.60.234.126

# Download setup script
curl -O https://raw.githubusercontent.com/YOUR_USERNAME/BAYTAEK/main/scripts/vps-setup.sh

# Make executable
chmod +x vps-setup.sh

# Run setup
./vps-setup.sh
```

#### Step 2: Configure GitHub Secrets

Make sure these secrets are set in your GitHub repository:
- `VPS_HOST`: 72.60.234.126
- `VPS_USER`: Your SSH username
- `VPS_SSH_PRIVATE_KEY`: Your SSH private key

#### Step 3: Trigger Deployment

```bash
# Push to main branch to trigger deployment
git push origin main

# Or manually trigger via GitHub Actions UI
```

#### Step 4: Verify Deployment

```bash
# On VPS, check services
sudo systemctl status baytaek-api
sudo systemctl status baytaek-frontend
sudo systemctl status nginx

# Test locally
curl http://localhost:9000/health
curl http://localhost:5001
curl http://localhost/api/health

# Test from your computer
curl http://72.60.234.126/api/health
```

### Manual Deployment (If GitHub Actions Fails)

#### Deploy Backend:

```bash
# On your local machine
cd backend
dotnet publish src/HomeService.API/HomeService.API.csproj -c Release -o ./publish -r linux-x64 --self-contained true

# Copy to VPS
scp -r ./publish/* user@72.60.234.126:/var/www/baytaek/backend/

# On VPS
ssh user@72.60.234.126
sudo chown -R www-data:www-data /var/www/baytaek/backend
sudo chmod +x /var/www/baytaek/backend/HomeService.API
sudo systemctl restart baytaek-api
```

#### Deploy Frontend:

```bash
# On your local machine
cd frontend
npm install
npm run build

# Copy to VPS
scp -r ./dist/frontend/browser/* user@72.60.234.126:/var/www/baytaek/frontend/dist/frontend/browser/

# On VPS
ssh user@72.60.234.126
sudo chown -R www-data:www-data /var/www/baytaek/frontend
sudo systemctl restart baytaek-frontend
```

### Monitoring Commands

```bash
# Real-time logs
sudo journalctl -u baytaek-api -f
sudo journalctl -u baytaek-frontend -f
sudo tail -f /var/log/nginx/error.log
sudo tail -f /var/log/nginx/access.log

# Service status
watch -n 2 'systemctl status baytaek-api baytaek-frontend nginx'

# Port monitoring
watch -n 2 'sudo netstat -tulpn | grep -E ":(80|443|9000|5001)"'

# Resource usage
htop
```

### Getting Help

If issues persist:

1. Collect diagnostic information:
```bash
# Run this and save output
{
  echo "=== System Info ==="
  uname -a

  echo -e "\n=== Service Status ==="
  systemctl status baytaek-api baytaek-frontend nginx --no-pager

  echo -e "\n=== Listening Ports ==="
  sudo netstat -tulpn | grep -E ":(80|443|9000|5001)"

  echo -e "\n=== Firewall Status ==="
  sudo ufw status

  echo -e "\n=== Recent Backend Logs ==="
  sudo journalctl -u baytaek-api -n 30 --no-pager

  echo -e "\n=== Recent Nginx Errors ==="
  sudo tail -30 /var/log/nginx/error.log

  echo -e "\n=== Disk Space ==="
  df -h

  echo -e "\n=== Memory Usage ==="
  free -h
} > ~/diagnostic-report.txt

cat ~/diagnostic-report.txt
```

2. Share the diagnostic report when asking for help

### Quick Reference Card

```bash
# Restart all services
sudo systemctl restart baytaek-api baytaek-frontend nginx

# View all logs
sudo journalctl -u baytaek-api -u baytaek-frontend -n 100

# Check health
curl http://localhost:9000/health && echo "Backend OK"
curl http://localhost:5001 && echo "Frontend OK"
curl http://localhost/api/health && echo "Nginx OK"

# Fix permissions
sudo chown -R www-data:www-data /var/www/baytaek
sudo chmod +x /var/www/baytaek/backend/HomeService.API

# Reload Nginx config
sudo nginx -t && sudo systemctl reload nginx
```
