# BAYTAEK Deployment Guide - Hostinger VPS

**Target Environment:** Hostinger VPS
**Application:** BAYTAEK (Home Services Platform)
**Stack:** .NET 8 Backend + Angular 18 Frontend + PostgreSQL/SQL Server

---

## 📋 Pre-Deployment Checklist

### 1. VPS Requirements
- **OS:** Ubuntu 20.04/22.04 LTS or CentOS 8+ (recommended: Ubuntu 22.04)
- **RAM:** Minimum 2GB (4GB+ recommended)
- **Storage:** Minimum 20GB SSD
- **CPU:** 2+ cores recommended
- **Ports:** 80 (HTTP), 443 (HTTPS), 5000 (Backend API)

### 2. What You'll Need
- ✅ Hostinger VPS access (SSH)
- ✅ Domain name (optional but recommended)
- ✅ SSL certificate (Let's Encrypt - free)
- ✅ Database connection string
- ✅ SMTP credentials (for emails)
- ✅ Stripe API keys (for payments)

---

## 🚀 Step-by-Step Deployment

### Step 1: Connect to Your Hostinger VPS

```bash
# SSH into your VPS
ssh root@your-vps-ip

# Or if you have a specific user
ssh username@your-vps-ip
```

---

### Step 2: Update System & Install Dependencies

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Install required packages
sudo apt install -y curl wget git nginx software-properties-common
```

---

### Step 3: Install .NET 8 Runtime

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET 8 SDK and Runtime
sudo apt update
sudo apt install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0

# Verify installation
dotnet --version
```

---

### Step 4: Install Node.js & Angular CLI

```bash
# Install Node.js 20 LTS
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs

# Verify installation
node --version
npm --version

# Install Angular CLI globally
sudo npm install -g @angular/cli

# Verify Angular CLI
ng version
```

---

### Step 5: Install PostgreSQL Database

```bash
# Install PostgreSQL
sudo apt install -y postgresql postgresql-contrib

# Start PostgreSQL service
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Create database and user
sudo -u postgres psql

# Inside PostgreSQL prompt:
CREATE DATABASE baytaek_db;
CREATE USER baytaek_user WITH ENCRYPTED PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE baytaek_db TO baytaek_user;
\q
```

**Alternative: Use SQL Server**
If you prefer SQL Server (as your backend might be configured for):

```bash
# Follow Microsoft's SQL Server installation guide for Linux
# https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-ubuntu
```

---

### Step 6: Clone Your Application

```bash
# Create application directory
sudo mkdir -p /var/www/baytaek
sudo chown -R $USER:$USER /var/www/baytaek
cd /var/www/baytaek

# Clone your repository (if using Git)
git clone https://github.com/yourusername/BAYTAEK.git .

# Or upload files via SCP/SFTP
# From your local machine:
# scp -r C:\combined\BAYTAEK root@your-vps-ip:/var/www/baytaek/
```

---

### Step 7: Configure Backend (.NET API)

```bash
cd /var/www/baytaek/backend

# Update appsettings.Production.json
sudo nano appsettings.Production.json
```

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=baytaek_db;Username=baytaek_user;Password=your_secure_password"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-characters-long",
    "Issuer": "https://yourdomain.com",
    "Audience": "https://yourdomain.com",
    "ExpirationInMinutes": 60
  },
  "Stripe": {
    "SecretKey": "sk_live_your_stripe_secret_key",
    "PublishableKey": "pk_live_your_stripe_publishable_key",
    "WebhookSecret": "whsec_your_webhook_secret"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.hostinger.com",
    "SmtpPort": 587,
    "SmtpUsername": "noreply@yourdomain.com",
    "SmtpPassword": "your_email_password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "BAYTAEK"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com,www.yourdomain.com",
  "Urls": "http://0.0.0.0:5000"
}
```

```bash
# Build the backend
dotnet restore
dotnet build --configuration Release

# Run database migrations
dotnet ef database update --project YourProject.csproj

# Test the backend
dotnet run --configuration Release
# Press Ctrl+C to stop after testing
```

---

### Step 8: Build Angular Frontend

```bash
cd /var/www/baytaek/frontend

# Install dependencies
npm install

# Update environment.prod.ts
sudo nano src/environments/environment.prod.ts
```

**environment.prod.ts:**
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://yourdomain.com/api',
  stripePublishableKey: 'pk_live_your_stripe_publishable_key',
  googleMapsApiKey: 'your_google_maps_api_key'
};
```

```bash
# Build for production
ng build --configuration production

# Built files will be in dist/frontend/browser/
```

---

### Step 9: Setup Backend as Systemd Service

```bash
# Create systemd service file
sudo nano /etc/systemd/system/baytaek-api.service
```

**baytaek-api.service:**
```ini
[Unit]
Description=BAYTAEK .NET Web API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/baytaek/backend
ExecStart=/usr/bin/dotnet /var/www/baytaek/backend/bin/Release/net8.0/YourProject.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=baytaek-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
# Enable and start the service
sudo systemctl daemon-reload
sudo systemctl enable baytaek-api
sudo systemctl start baytaek-api

# Check status
sudo systemctl status baytaek-api

# View logs
sudo journalctl -u baytaek-api -f
```

---

### Step 10: Configure Nginx as Reverse Proxy

```bash
# Create Nginx configuration
sudo nano /etc/nginx/sites-available/baytaek
```

**baytaek nginx config:**
```nginx
# Backend API Server
upstream backend_api {
    server 127.0.0.1:5000;
    keepalive 32;
}

# Main server block - HTTP (will redirect to HTTPS)
server {
    listen 80;
    listen [::]:80;
    server_name yourdomain.com www.yourdomain.com;

    # Redirect all HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

# HTTPS server block
server {
    listen 443 ssl http2;
    listen [::]:443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    # SSL Configuration (will be added by Certbot)
    # ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    # ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;

    # Frontend - Angular application
    root /var/www/baytaek/frontend/dist/frontend/browser;
    index index.html;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript
               application/x-javascript application/xml+rss application/json
               application/javascript image/svg+xml;

    # API proxy
    location /api/ {
        proxy_pass http://backend_api/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        proxy_buffering off;
        proxy_request_buffering off;
        client_max_body_size 50M;
    }

    # SignalR WebSocket support
    location /hubs/ {
        proxy_pass http://backend_api/hubs/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # Serve static files with caching
    location ~* \.(jpg|jpeg|png|gif|ico|css|js|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Angular routes - try files first, then index.html
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Deny access to sensitive files
    location ~ /\. {
        deny all;
    }
}
```

```bash
# Enable the site
sudo ln -s /etc/nginx/sites-available/baytaek /etc/nginx/sites-enabled/

# Test Nginx configuration
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx
```

---

### Step 11: Install SSL Certificate (Let's Encrypt)

```bash
# Install Certbot
sudo apt install -y certbot python3-certbot-nginx

# Obtain SSL certificate
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Follow the prompts:
# - Enter your email
# - Agree to terms
# - Choose to redirect HTTP to HTTPS (recommended)

# Test auto-renewal
sudo certbot renew --dry-run

# Certificate will auto-renew via cron
```

---

### Step 12: Configure Firewall

```bash
# Install UFW (if not installed)
sudo apt install -y ufw

# Allow SSH (IMPORTANT - do this first!)
sudo ufw allow 22/tcp

# Allow HTTP and HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Enable firewall
sudo ufw enable

# Check status
sudo ufw status
```

---

### Step 13: Setup Database Backups

```bash
# Create backup script
sudo nano /usr/local/bin/backup-baytaek-db.sh
```

**backup-baytaek-db.sh:**
```bash
#!/bin/bash
BACKUP_DIR="/var/backups/baytaek"
DATE=$(date +%Y%m%d_%H%M%S)
mkdir -p $BACKUP_DIR

# Backup PostgreSQL database
pg_dump -U baytaek_user baytaek_db | gzip > $BACKUP_DIR/baytaek_db_$DATE.sql.gz

# Keep only last 7 days of backups
find $BACKUP_DIR -name "*.sql.gz" -mtime +7 -delete

echo "Backup completed: baytaek_db_$DATE.sql.gz"
```

```bash
# Make script executable
sudo chmod +x /usr/local/bin/backup-baytaek-db.sh

# Add to crontab (daily at 2 AM)
sudo crontab -e

# Add this line:
0 2 * * * /usr/local/bin/backup-baytaek-db.sh >> /var/log/baytaek-backup.log 2>&1
```

---

### Step 14: Setup Application Monitoring

```bash
# Install PM2 for process monitoring (optional alternative to systemd)
sudo npm install -g pm2

# Or use system monitoring tools
sudo apt install -y htop iotop

# Check backend logs
sudo journalctl -u baytaek-api -f

# Check Nginx logs
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

---

## 🔧 Post-Deployment Configuration

### 1. Test Your Deployment

```bash
# Test backend API
curl https://yourdomain.com/api/health

# Check if frontend loads
curl https://yourdomain.com

# Test database connection
sudo -u postgres psql -d baytaek_db -c "SELECT version();"
```

### 2. Configure Email Sending (Hostinger)

Your Hostinger email settings:
- **SMTP Server:** smtp.hostinger.com
- **Port:** 587 (TLS) or 465 (SSL)
- **Username:** your-email@yourdomain.com
- **Password:** your email password

### 3. Setup Stripe Webhooks

1. Go to Stripe Dashboard → Webhooks
2. Add endpoint: `https://yourdomain.com/api/payments/webhook`
3. Select events: `payment_intent.succeeded`, `payment_intent.failed`, etc.
4. Copy webhook signing secret to `appsettings.Production.json`

### 4. Configure CORS (if needed)

In your backend `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", builder =>
    {
        builder.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

app.UseCors("ProductionPolicy");
```

---

## 📊 Monitoring & Maintenance

### Check Service Status
```bash
# Backend API status
sudo systemctl status baytaek-api

# Nginx status
sudo systemctl status nginx

# PostgreSQL status
sudo systemctl status postgresql
```

### View Logs
```bash
# Backend API logs
sudo journalctl -u baytaek-api -n 100

# Nginx access logs
sudo tail -100 /var/log/nginx/access.log

# Nginx error logs
sudo tail -100 /var/log/nginx/error.log
```

### Performance Monitoring
```bash
# CPU and Memory usage
htop

# Disk usage
df -h

# Network connections
ss -tulpn
```

---

## 🔄 Updating Your Application

### Update Backend
```bash
cd /var/www/baytaek/backend

# Pull latest code
git pull origin main

# Build
dotnet build --configuration Release

# Run migrations
dotnet ef database update

# Restart service
sudo systemctl restart baytaek-api
```

### Update Frontend
```bash
cd /var/www/baytaek/frontend

# Pull latest code
git pull origin main

# Install dependencies
npm install

# Build
ng build --configuration production

# Nginx will automatically serve new files
```

---

## 🐛 Troubleshooting

### Backend not starting?
```bash
# Check logs
sudo journalctl -u baytaek-api -n 50

# Check if port 5000 is in use
sudo lsof -i :5000

# Test manual run
cd /var/www/baytaek/backend
dotnet run --configuration Release
```

### Nginx 502 Bad Gateway?
```bash
# Check if backend is running
curl http://localhost:5000/api/health

# Check Nginx error logs
sudo tail -50 /var/log/nginx/error.log

# Restart both services
sudo systemctl restart baytaek-api
sudo systemctl restart nginx
```

### Database connection issues?
```bash
# Test database connection
sudo -u postgres psql -d baytaek_db

# Check if PostgreSQL is running
sudo systemctl status postgresql

# Check connection string in appsettings.Production.json
```

### SSL certificate issues?
```bash
# Renew certificate manually
sudo certbot renew

# Check certificate status
sudo certbot certificates

# Test Nginx config
sudo nginx -t
```

---

## 📝 Important Security Notes

1. **Change default passwords** in `appsettings.Production.json`
2. **Use environment variables** for sensitive data (alternative to config files)
3. **Enable firewall** with UFW
4. **Regular updates:** `sudo apt update && sudo apt upgrade`
5. **Monitor logs** for suspicious activity
6. **Backup regularly** (database + code + uploads)
7. **Use strong JWT secret** (minimum 32 characters)
8. **Limit SSH access** (consider key-based authentication only)

---

## 🎯 Quick Deployment Checklist

- [ ] VPS access configured (SSH)
- [ ] .NET 8 Runtime installed
- [ ] Node.js & Angular CLI installed
- [ ] PostgreSQL/SQL Server installed
- [ ] Database created and migrated
- [ ] Backend configured (appsettings.Production.json)
- [ ] Frontend built (ng build --prod)
- [ ] Systemd service created and running
- [ ] Nginx configured and running
- [ ] SSL certificate installed
- [ ] Firewall configured (UFW)
- [ ] Domain DNS pointed to VPS IP
- [ ] Email SMTP configured
- [ ] Stripe webhooks configured
- [ ] Backups scheduled
- [ ] Monitoring setup

---

## 📞 Support Resources

- **Hostinger VPS Docs:** https://support.hostinger.com/
- **.NET Deployment:** https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx
- **Angular Deployment:** https://angular.io/guide/deployment
- **Let's Encrypt:** https://letsencrypt.org/getting-started/
- **Nginx Docs:** https://nginx.org/en/docs/

---

**Deployment Status:** Ready to Deploy!
**Estimated Time:** 2-3 hours for first-time setup
**Difficulty:** Intermediate

Good luck with your deployment! 🚀
