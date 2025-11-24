#!/bin/bash

# Baytaek VPS Setup Script
# Run this script on your Hostinger VPS to set up the environment

set -e  # Exit on any error

echo "=================================================="
echo "Baytaek VPS Setup - Hostinger Deployment"
echo "=================================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored messages
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}→ $1${NC}"
}

# Check if running as root
if [ "$EUID" -eq 0 ]; then
    print_error "Please do not run this script as root"
    exit 1
fi

# Update system
print_info "Updating system packages..."
sudo apt update && sudo apt upgrade -y
print_success "System updated"

# Install required packages
print_info "Installing required packages..."
sudo apt install -y \
    nginx \
    ufw \
    certbot \
    python3-certbot-nginx \
    curl \
    wget \
    git \
    net-tools
print_success "Required packages installed"

# Install .NET 8.0 Runtime (if not already installed)
print_info "Checking .NET installation..."
if ! command -v dotnet &> /dev/null; then
    print_info "Installing .NET 8.0 Runtime..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel 8.0 --runtime aspnetcore
    rm dotnet-install.sh

    # Add to PATH
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
    echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
    source ~/.bashrc
    print_success ".NET 8.0 Runtime installed"
else
    print_success ".NET is already installed: $(dotnet --version)"
fi

# Install Node.js and npm (for frontend)
print_info "Checking Node.js installation..."
if ! command -v node &> /dev/null; then
    print_info "Installing Node.js..."
    curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
    sudo apt install -y nodejs
    print_success "Node.js installed: $(node --version)"
else
    print_success "Node.js is already installed: $(node --version)"
fi

# Install serve package for frontend
print_info "Installing serve package for frontend..."
sudo npm install -g serve
print_success "Serve package installed"

# Create application directories
print_info "Creating application directories..."
sudo mkdir -p /var/www/baytaek/backend
sudo mkdir -p /var/www/baytaek/frontend/dist/frontend/browser
sudo chown -R $USER:$USER /var/www/baytaek
print_success "Directories created"

# Configure firewall
print_info "Configuring firewall..."
sudo ufw --force enable
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw status
print_success "Firewall configured"

# Create systemd service for backend
print_info "Creating systemd service for backend..."
sudo tee /etc/systemd/system/baytaek-api.service > /dev/null <<EOF
[Unit]
Description=Baytaek API
After=network.target

[Service]
Type=simple
User=www-data
WorkingDirectory=/var/www/baytaek/backend
ExecStart=/var/www/baytaek/backend/HomeService.API
Restart=always
RestartSec=5
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:9000

[Install]
WantedBy=multi-user.target
EOF

sudo systemctl daemon-reload
sudo systemctl enable baytaek-api.service
print_success "Backend systemd service created"

# Create systemd service for frontend
print_info "Creating systemd service for frontend..."
sudo tee /etc/systemd/system/baytaek-frontend.service > /dev/null <<EOF
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
EOF

sudo systemctl daemon-reload
sudo systemctl enable baytaek-frontend.service
print_success "Frontend systemd service created"

# Configure Nginx
print_info "Configuring Nginx..."

# Remove default site
sudo rm -f /etc/nginx/sites-enabled/default

# Create Nginx configuration
sudo tee /etc/nginx/sites-available/baytaek > /dev/null <<'EOF'
# Baytaek Nginx Configuration
upstream baytaek_backend {
    server localhost:9000;
    keepalive 32;
}

server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name _;

    # Increase timeouts for slow connections
    proxy_connect_timeout 600;
    proxy_send_timeout 600;
    proxy_read_timeout 600;
    send_timeout 600;

    # Backend API routes
    location /api/ {
        proxy_pass http://baytaek_backend/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # SignalR WebSocket hubs
    location /hubs/ {
        proxy_pass http://baytaek_backend/hubs/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_connect_timeout 7d;
        proxy_send_timeout 7d;
        proxy_read_timeout 7d;
    }

    # Health check
    location /health {
        proxy_pass http://baytaek_backend/health;
        access_log off;
    }

    # Swagger (remove in production)
    location /swagger {
        proxy_pass http://baytaek_backend/swagger;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
    }

    # Frontend
    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }

    # Error pages
    error_page 502 503 504 /50x.html;
    location = /50x.html {
        root /usr/share/nginx/html;
    }
}
EOF

# Enable site
sudo ln -sf /etc/nginx/sites-available/baytaek /etc/nginx/sites-enabled/

# Test Nginx configuration
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx
print_success "Nginx configured and restarted"

# Check if services are enabled
print_info "Checking service status..."
sudo systemctl status baytaek-api.service --no-pager || print_info "Backend service not started yet (needs deployment)"
sudo systemctl status baytaek-frontend.service --no-pager || print_info "Frontend service not started yet (needs deployment)"
sudo systemctl status nginx --no-pager

echo ""
echo "=================================================="
print_success "VPS Setup Complete!"
echo "=================================================="
echo ""
echo "Next steps:"
echo "1. Deploy your application using GitHub Actions"
echo "2. Verify backend: curl http://localhost:9000/health"
echo "3. Verify frontend: curl http://localhost:5001"
echo "4. Access via browser: http://$(curl -s ifconfig.me)"
echo ""
echo "To check service logs:"
echo "  sudo journalctl -u baytaek-api -n 50 -f"
echo "  sudo journalctl -u baytaek-frontend -n 50 -f"
echo "  sudo tail -f /var/log/nginx/error.log"
echo ""
echo "To configure SSL (after DNS is pointing to this server):"
echo "  sudo certbot --nginx -d your-domain.com -d www.your-domain.com"
echo ""
