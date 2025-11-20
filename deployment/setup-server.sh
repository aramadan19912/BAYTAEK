#!/bin/bash
set -e

echo "=========================================="
echo "Baytaek Server Setup Script"
echo "=========================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if running as root
if [[ $EUID -ne 0 ]]; then
   echo -e "${RED}This script must be run as root (use sudo)${NC}"
   exit 1
fi

echo -e "${GREEN}Step 1: Creating application directories...${NC}"
mkdir -p /var/www/baytaek/backend
mkdir -p /var/www/baytaek/frontend
mkdir -p /var/log/baytaek

echo -e "${GREEN}Step 2: Setting up permissions...${NC}"
chown -R www-data:www-data /var/www/baytaek
chmod -R 755 /var/www/baytaek
chown -R www-data:www-data /var/log/baytaek

echo -e "${GREEN}Step 3: Installing .NET 8 Runtime (if not already installed)...${NC}"
if ! command -v dotnet &> /dev/null; then
    echo "Installing .NET 8..."
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    apt-get update
    apt-get install -y aspnetcore-runtime-8.0
else
    echo ".NET is already installed: $(dotnet --version)"
fi

echo -e "${GREEN}Step 4: Installing Nginx (if not already installed)...${NC}"
if ! command -v nginx &> /dev/null; then
    apt-get install -y nginx
else
    echo "Nginx is already installed"
fi

echo -e "${GREEN}Step 5: Setting up systemd service...${NC}"
if [ -f "/tmp/baytaek-api.service" ]; then
    cp /tmp/baytaek-api.service /etc/systemd/system/baytaek-api.service
    chmod 644 /etc/systemd/system/baytaek-api.service
    systemctl daemon-reload
    echo "Systemd service installed"
else
    echo -e "${YELLOW}Warning: baytaek-api.service not found in /tmp${NC}"
    echo "Please upload the service file first"
fi

echo -e "${GREEN}Step 6: Configuring Nginx...${NC}"
if [ -f "/tmp/baytaek-nginx.conf" ]; then
    cp /tmp/baytaek-nginx.conf /etc/nginx/sites-available/baytaek
    ln -sf /etc/nginx/sites-available/baytaek /etc/nginx/sites-enabled/
    nginx -t && systemctl reload nginx
    echo "Nginx configured"
else
    echo -e "${YELLOW}Warning: baytaek-nginx.conf not found in /tmp${NC}"
    echo "Please upload the nginx config first"
fi

echo -e "${GREEN}Step 7: Setting up firewall rules...${NC}"
if command -v ufw &> /dev/null; then
    ufw allow 'Nginx Full'
    ufw allow 22/tcp
    echo "Firewall rules configured"
fi

echo ""
echo -e "${GREEN}=========================================="
echo "Server setup completed!"
echo "==========================================${NC}"
echo ""
echo "Next steps:"
echo "1. Upload your backend build to /var/www/baytaek/backend"
echo "2. Upload your frontend build to /var/www/baytaek/frontend"
echo "3. Start the service: sudo systemctl start baytaek-api"
echo "4. Enable on boot: sudo systemctl enable baytaek-api"
echo "5. Check status: sudo systemctl status baytaek-api"
echo ""
