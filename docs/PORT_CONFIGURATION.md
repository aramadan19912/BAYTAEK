# Port Configuration Guide

## Port Assignments

### Backend API
- **Port:** 9000
- **Protocol:** HTTP
- **Binding:** 0.0.0.0:9000 (all interfaces)

### Frontend
- **Port:** 5001
- **Protocol:** HTTP
- **Binding:** localhost:5001

### Public Access (via Nginx)
- **Port 80:** HTTP (redirects to HTTPS in production)
- **Port 443:** HTTPS (when SSL is configured)

## Configuration Files

### 1. Backend Configuration

#### appsettings.json
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:9000"
      }
    }
  }
}
```

#### Systemd Service (baytaek-api.service)
```ini
[Service]
Environment=ASPNETCORE_URLS=http://0.0.0.0:9000
```

### 2. Frontend Configuration

The Angular dev server runs on port 5001. In production, you can:

**Option A: Use Angular CLI (Development)**
```bash
ng serve --port 5001
```

**Option B: Use Node.js Server (Production)**
Create a simple server to serve the built Angular app on port 5001.

### 3. Nginx Reverse Proxy

The Nginx configuration is located at: `nginx/baytaek.conf`

**Key routes:**
- `/api/*` → Backend API (port 9000)
- `/hubs/*` → SignalR WebSocket connections (port 9000)
- `/health` → Health check endpoint (port 9000)
- `/*` → Frontend (port 5001)

## Deployment Steps

### Step 1: Deploy Backend Service

The GitHub Actions workflow automatically creates/updates the systemd service with port 9000.

To manually update on the VPS:

```bash
# SSH into VPS
ssh user@your-vps-host

# Edit systemd service
sudo nano /etc/systemd/system/baytaek-api.service

# Add environment variable
Environment=ASPNETCORE_URLS=http://0.0.0.0:9000

# Reload systemd and restart service
sudo systemctl daemon-reload
sudo systemctl restart baytaek-api

# Verify service is running on port 9000
sudo netstat -tulpn | grep 9000
curl http://localhost:9000/health
```

### Step 2: Configure Nginx

```bash
# Copy nginx configuration
sudo cp nginx/baytaek.conf /etc/nginx/sites-available/baytaek

# Create symbolic link
sudo ln -s /etc/nginx/sites-available/baytaek /etc/nginx/sites-enabled/

# Remove default site if needed
sudo rm /etc/nginx/sites-enabled/default

# Test nginx configuration
sudo nginx -t

# Reload nginx
sudo systemctl reload nginx
```

### Step 3: Configure Frontend

If using a Node.js server for the frontend:

```bash
# Install serve package (if not already)
npm install -g serve

# Create systemd service for frontend
sudo nano /etc/systemd/system/baytaek-frontend.service
```

Add the following content:

```ini
[Unit]
Description=Baytaek Frontend
After=network.target

[Service]
Type=simple
User=www-data
WorkingDirectory=/var/www/baytaek/frontend/dist/frontend/browser
ExecStart=/usr/local/bin/serve -s . -l 5001
Restart=always
RestartSec=5
Environment=NODE_ENV=production

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable baytaek-frontend
sudo systemctl start baytaek-frontend
sudo systemctl status baytaek-frontend
```

### Step 4: Configure Firewall

```bash
# Allow HTTP and HTTPS through firewall
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Backend port (should only be accessible locally, not externally)
# Do NOT open port 9000 externally - use Nginx proxy instead

# Reload firewall
sudo ufw reload
sudo ufw status
```

### Step 5: Verify Deployment

```bash
# Check backend
curl http://localhost:9000/health

# Check frontend
curl http://localhost:5001

# Check via Nginx
curl http://localhost/api/health
curl http://localhost/
```

## CORS Configuration

Update the CORS policy in `Program.cs` to include your production domain:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",      // Development
            "http://localhost:5001",      // Production Frontend
            "https://your-domain.com",    // Production Domain
            "https://www.your-domain.com" // Production Domain with www
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

## Troubleshooting

### Backend not responding on port 9000

```bash
# Check if service is running
sudo systemctl status baytaek-api

# Check logs
sudo journalctl -u baytaek-api -n 100 --no-pager

# Check if port is in use
sudo netstat -tulpn | grep 9000

# Restart service
sudo systemctl restart baytaek-api
```

### Frontend not responding on port 5001

```bash
# Check if frontend service is running
sudo systemctl status baytaek-frontend

# Check logs
sudo journalctl -u baytaek-frontend -n 100 --no-pager

# Restart service
sudo systemctl restart baytaek-frontend
```

### Nginx proxy issues

```bash
# Check nginx configuration
sudo nginx -t

# Check nginx logs
sudo tail -f /var/log/nginx/error.log
sudo tail -f /var/log/nginx/access.log

# Reload nginx
sudo systemctl reload nginx
```

## SSL/HTTPS Setup (Optional)

To enable HTTPS with Let's Encrypt:

```bash
# Install certbot
sudo apt update
sudo apt install certbot python3-certbot-nginx

# Obtain certificate
sudo certbot --nginx -d your-domain.com -d www.your-domain.com

# Certbot will automatically update nginx configuration
# Test automatic renewal
sudo certbot renew --dry-run
```

After SSL is configured, uncomment the HTTPS server block in `nginx/baytaek.conf`.

## Architecture Diagram

```
Internet (Port 80/443)
         ↓
    Nginx Reverse Proxy
         ↓
    ┌────┴────┐
    ↓         ↓
Backend    Frontend
Port 9000  Port 5001
```

## Environment Variables

### Backend (baytaek-api.service)
```ini
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:9000
```

### Frontend (baytaek-frontend.service)
```ini
Environment=NODE_ENV=production
Environment=PORT=5001
```

## Next Steps

1. Update your DNS records to point to your VPS IP
2. Configure SSL certificate with Let's Encrypt
3. Update CORS origins with your production domain
4. Configure connection strings and API keys in production
5. Enable Nginx rate limiting for security
6. Set up monitoring and log aggregation
