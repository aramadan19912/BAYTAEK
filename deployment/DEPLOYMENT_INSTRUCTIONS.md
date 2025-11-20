# Baytaek Deployment Instructions

## Overview
This document provides step-by-step instructions for deploying the Baytaek Home Service platform to a VPS server.

## Prerequisites

### On Your VPS Server
- Ubuntu 20.04+ or similar Linux distribution
- Root/sudo access
- .NET 8 Runtime
- Nginx
- At least 2GB RAM
- 20GB disk space

### GitHub Secrets Required
Configure these in your GitHub repository settings (Settings → Secrets and variables → Actions):

- `VPS_HOST`: Your VPS IP address or domain
- `VPS_USER`: SSH username (e.g., `root` or `ubuntu`)
- `VPS_SSH_PRIVATE_KEY`: Your SSH private key for authentication

## Initial Server Setup

### Step 1: Upload Configuration Files

```bash
# From your local machine, upload the deployment files to /tmp on your server
scp deployment/baytaek-api.service your-user@your-server:/tmp/
scp deployment/baytaek-nginx.conf your-user@your-server:/tmp/
scp deployment/setup-server.sh your-user@your-server:/tmp/
```

### Step 2: Run Server Setup Script

```bash
# SSH into your server
ssh your-user@your-server

# Make the script executable
chmod +x /tmp/setup-server.sh

# Run the setup script as root
sudo /tmp/setup-server.sh
```

This script will:
- Create application directories (`/var/www/baytaek/`)
- Install .NET 8 Runtime (if not present)
- Install Nginx (if not present)
- Set up the systemd service
- Configure Nginx
- Set proper permissions

### Step 3: Generate Self-Signed SSL Certificate (Temporary)

For testing purposes, generate a self-signed certificate:

```bash
sudo mkdir -p /etc/nginx/ssl
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout /etc/nginx/ssl/self-signed.key \
  -out /etc/nginx/ssl/self-signed.crt \
  -subj "/C=US/ST=State/L=City/O=Organization/CN=your-domain.com"
```

For production, use Let's Encrypt (see below).

### Step 4: Configure Environment Variables

Create an appsettings.Production.json file on your server:

```bash
sudo nano /var/www/baytaek/backend/appsettings.Production.json
```

Add your production configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-db-server;Database=baytaek_prod;User Id=your-user;Password=your-password;"
  },
  "JwtSettings": {
    "Secret": "your-very-long-secret-key-here-at-least-32-characters",
    "Issuer": "BaytaekAPI",
    "Audience": "BaytaekClient",
    "ExpiryMinutes": 60
  }
}
```

Set proper permissions:

```bash
sudo chown www-data:www-data /var/www/baytaek/backend/appsettings.Production.json
sudo chmod 600 /var/www/baytaek/backend/appsettings.Production.json
```

## Automated Deployment via GitHub Actions

### How It Works

The deployment workflow (`.github/workflows/deploy.yml`) automatically:

1. **Builds Backend**: Compiles .NET backend with Release configuration
2. **Builds Frontend**: Builds Angular app for production
3. **Deploys to VPS**:
   - Creates backup of current deployment
   - Ensures systemd service exists
   - Stops the API service
   - Uploads new backend files via rsync
   - Uploads new frontend files
   - Runs database migrations
   - Starts the API service
   - Verifies deployment health

### Triggering Deployment

Deployment runs automatically on:
- Push to `main` branch
- Push to `production` branch
- Manual workflow dispatch

```bash
# To deploy:
git add .
git commit -m "Deploy changes"
git push origin main
```

### Monitoring Deployment

1. Go to GitHub repository → Actions tab
2. Click on the latest workflow run
3. Monitor each job:
   - Build Backend
   - Build Frontend
   - Deploy to Hostinger VPS

## Manual Deployment

If you need to deploy manually:

### Backend

```bash
# Build locally
cd backend
dotnet publish src/HomeService.API/HomeService.API.csproj \
  -c Release \
  -o ../publish/backend

# Upload to server
rsync -avz --delete ../publish/backend/ your-user@your-server:/var/www/baytaek/backend/

# SSH to server and restart
ssh your-user@your-server
sudo systemctl restart baytaek-api
sudo systemctl status baytaek-api
```

### Frontend

```bash
# Build locally
cd frontend
npm install
npm run build

# Upload to server
rsync -avz --delete dist/frontend/browser/ your-user@your-server:/var/www/baytaek/frontend/

# Reload Nginx
ssh your-user@your-server "sudo systemctl reload nginx"
```

## SSL Certificate Setup (Production)

### Using Let's Encrypt

```bash
# Install Certbot
sudo apt-get install certbot python3-certbot-nginx

# Get certificate
sudo certbot --nginx -d your-domain.com -d www.your-domain.com

# Certbot will automatically configure Nginx
# Certificates auto-renew via cron
```

### Update Nginx Configuration

Edit `/etc/nginx/sites-available/baytaek` and update SSL paths:

```nginx
ssl_certificate /etc/letsencrypt/live/your-domain.com/fullchain.pem;
ssl_certificate_key /etc/letsencrypt/live/your-domain.com/privkey.pem;
ssl_trusted_certificate /etc/letsencrypt/live/your-domain.com/chain.pem;
```

Remove or comment out the self-signed certificate lines.

## Troubleshooting

### Check Service Status

```bash
# Backend API
sudo systemctl status baytaek-api
sudo journalctl -u baytaek-api -f

# Nginx
sudo systemctl status nginx
sudo tail -f /var/log/nginx/baytaek_error.log
```

### Common Issues

#### 1. Service Won't Start

```bash
# Check logs
sudo journalctl -u baytaek-api -n 50

# Common fixes:
# - Verify .NET is installed: dotnet --version
# - Check file permissions: ls -la /var/www/baytaek/backend
# - Verify DLL exists: ls -la /var/www/baytaek/backend/HomeService.API.dll
```

#### 2. Database Connection Issues

```bash
# Test database connectivity from server
telnet your-db-server 1433

# Check connection string in appsettings.Production.json
sudo cat /var/www/baytaek/backend/appsettings.Production.json
```

#### 3. Nginx 502 Bad Gateway

```bash
# Check if backend is running
curl http://localhost:5000/api/health

# Check Nginx error logs
sudo tail -f /var/log/nginx/baytaek_error.log
```

#### 4. Deployment Fails on GitHub Actions

- Verify all secrets are set correctly in GitHub
- Check SSH key permissions
- Review workflow logs for specific errors
- Ensure server has enough disk space: `df -h`

### Manual Rollback

If deployment fails, rollback to previous version:

```bash
# List backups
ls -la /var/www/baytaek/backend.backup.*

# Stop current service
sudo systemctl stop baytaek-api

# Restore backup (use the timestamp you want)
sudo rm -rf /var/www/baytaek/backend
sudo cp -r /var/www/baytaek/backend.backup.YYYYMMDD_HHMMSS /var/www/baytaek/backend

# Start service
sudo systemctl start baytaek-api
```

## Monitoring and Maintenance

### Set Up Log Rotation

```bash
sudo nano /etc/logrotate.d/baytaek
```

Add:

```
/var/log/nginx/baytaek_*.log {
    daily
    rotate 14
    compress
    delaycompress
    notifempty
    create 0640 www-data adm
    sharedscripts
    postrotate
        [ -f /var/run/nginx.pid ] && kill -USR1 `cat /var/run/nginx.pid`
    endscript
}
```

### Automated Backups

Add to crontab:

```bash
sudo crontab -e
```

Add:

```bash
# Daily backup at 2 AM
0 2 * * * /usr/bin/rsync -a /var/www/baytaek /backup/baytaek-$(date +\%Y\%m\%d)
```

### Health Checks

Set up a monitoring service or cron job:

```bash
*/5 * * * * curl -f http://localhost:5000/api/health || systemctl restart baytaek-api
```

## Security Checklist

- [ ] SSL certificate installed and configured
- [ ] Firewall configured (UFW or iptables)
- [ ] SSH key-only authentication enabled
- [ ] Database uses strong passwords
- [ ] JWT secret is long and random
- [ ] appsettings.Production.json has restricted permissions (600)
- [ ] Regular security updates: `sudo apt update && sudo apt upgrade`
- [ ] Fail2ban installed for SSH protection
- [ ] Regular backups configured

## Support

For issues or questions:
1. Check this documentation
2. Review GitHub Actions logs
3. Check server logs (`journalctl -u baytaek-api`)
4. Create an issue in the GitHub repository

## File Locations Reference

| Item | Location |
|------|----------|
| Backend Application | `/var/www/baytaek/backend/` |
| Frontend Static Files | `/var/www/baytaek/frontend/` |
| Systemd Service | `/etc/systemd/system/baytaek-api.service` |
| Nginx Config | `/etc/nginx/sites-available/baytaek` |
| Nginx Logs | `/var/log/nginx/baytaek_*.log` |
| Application Logs | `journalctl -u baytaek-api` |
| Backups | `/var/www/baytaek/backend.backup.*` |
