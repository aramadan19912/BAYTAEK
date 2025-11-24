#!/bin/bash

# Baytaek Deployment Health Check Script
# Run this on your VPS to diagnose deployment issues

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_header() {
    echo -e "\n${BLUE}=== $1 ===${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "  $1"
}

# Header
echo "=================================================="
echo "   Baytaek Deployment Health Check"
echo "=================================================="

# 1. Check Nginx
print_header "Nginx Status"
if systemctl is-active --quiet nginx; then
    print_success "Nginx is running"
    nginx -v 2>&1 | head -1
else
    print_error "Nginx is not running"
    print_info "Run: sudo systemctl start nginx"
fi

# 2. Check Backend API
print_header "Backend API Status"
if systemctl is-active --quiet baytaek-api; then
    print_success "Backend API service is running"

    # Test health endpoint
    BACKEND_HEALTH=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:9000/health 2>/dev/null)
    if [ "$BACKEND_HEALTH" = "200" ]; then
        print_success "Backend health check passed (HTTP $BACKEND_HEALTH)"
    else
        print_error "Backend health check failed (HTTP $BACKEND_HEALTH)"
        print_info "Check logs: sudo journalctl -u baytaek-api -n 50"
    fi
else
    print_error "Backend API service is not running"
    if [ -f /etc/systemd/system/baytaek-api.service ]; then
        print_info "Service exists but not running"
        print_info "Check logs: sudo journalctl -u baytaek-api -n 50"
        print_info "Start with: sudo systemctl start baytaek-api"
    else
        print_warning "Service not configured"
        print_info "Run: bash scripts/vps-setup.sh"
    fi
fi

# 3. Check Frontend
print_header "Frontend Status"
if systemctl is-active --quiet baytaek-frontend; then
    print_success "Frontend service is running"

    # Test frontend endpoint
    FRONTEND_HEALTH=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5001 2>/dev/null)
    if [ "$FRONTEND_HEALTH" = "200" ] || [ "$FRONTEND_HEALTH" = "304" ]; then
        print_success "Frontend is accessible (HTTP $FRONTEND_HEALTH)"
    else
        print_error "Frontend not accessible (HTTP $FRONTEND_HEALTH)"
        print_info "Check logs: sudo journalctl -u baytaek-frontend -n 50"
    fi
else
    print_error "Frontend service is not running"
    if [ -f /etc/systemd/system/baytaek-frontend.service ]; then
        print_info "Service exists but not running"
        print_info "Check logs: sudo journalctl -u baytaek-frontend -n 50"
        print_info "Start with: sudo systemctl start baytaek-frontend"
    else
        print_warning "Service not configured"
        print_info "Run: bash scripts/vps-setup.sh"
    fi
fi

# 4. Check Listening Ports
print_header "Port Status"
if netstat -tuln 2>/dev/null | grep -q ":80 "; then
    print_success "Port 80 (HTTP) is listening"
else
    print_error "Port 80 (HTTP) is not listening"
fi

if netstat -tuln 2>/dev/null | grep -q ":443 "; then
    print_success "Port 443 (HTTPS) is listening"
else
    print_warning "Port 443 (HTTPS) is not listening (SSL not configured)"
fi

if netstat -tuln 2>/dev/null | grep -q ":9000 "; then
    print_success "Port 9000 (Backend) is listening"
else
    print_error "Port 9000 (Backend) is not listening"
fi

if netstat -tuln 2>/dev/null | grep -q ":5001 "; then
    print_success "Port 5001 (Frontend) is listening"
else
    print_error "Port 5001 (Frontend) is not listening"
fi

# 5. Check Firewall
print_header "Firewall Status"
if command -v ufw >/dev/null 2>&1; then
    if ufw status | grep -q "Status: active"; then
        print_success "Firewall is active"
        if ufw status | grep -q "80/tcp.*ALLOW"; then
            print_success "Port 80 is allowed"
        else
            print_error "Port 80 is not allowed"
            print_info "Run: sudo ufw allow 80/tcp"
        fi
        if ufw status | grep -q "443/tcp.*ALLOW"; then
            print_success "Port 443 is allowed"
        else
            print_warning "Port 443 is not allowed (needed for HTTPS)"
            print_info "Run: sudo ufw allow 443/tcp"
        fi
    else
        print_warning "Firewall is not active"
        print_info "Run: sudo ufw enable"
    fi
fi

# 6. Check Nginx Configuration
print_header "Nginx Configuration"
if nginx -t 2>&1 | grep -q "syntax is ok"; then
    print_success "Nginx configuration is valid"
else
    print_error "Nginx configuration has errors"
    print_info "Run: sudo nginx -t"
fi

if [ -f /etc/nginx/sites-enabled/baytaek ]; then
    print_success "Baytaek site is enabled"
else
    print_error "Baytaek site is not enabled"
    print_info "Run setup script or manually link config"
fi

# 7. Check Application Files
print_header "Application Files"
if [ -d /var/www/baytaek/backend ] && [ "$(ls -A /var/www/baytaek/backend 2>/dev/null)" ]; then
    print_success "Backend files exist"
    if [ -x /var/www/baytaek/backend/HomeService.API ]; then
        print_success "Backend executable has correct permissions"
    else
        print_error "Backend executable is not executable"
        print_info "Run: sudo chmod +x /var/www/baytaek/backend/HomeService.API"
    fi
else
    print_error "Backend files not found or directory empty"
    print_info "Deploy backend using GitHub Actions or manually"
fi

if [ -d /var/www/baytaek/frontend/dist/frontend/browser ] && [ "$(ls -A /var/www/baytaek/frontend/dist/frontend/browser 2>/dev/null)" ]; then
    print_success "Frontend files exist"
else
    print_error "Frontend files not found or directory empty"
    print_info "Deploy frontend using GitHub Actions or manually"
fi

# 8. Check External Access
print_header "External Access Test"
PUBLIC_IP=$(curl -s ifconfig.me 2>/dev/null || echo "Unable to determine")
print_info "Public IP: $PUBLIC_IP"

if [ "$PUBLIC_IP" != "Unable to determine" ]; then
    print_info "Testing external HTTP access..."
    HTTP_TEST=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 5 "http://$PUBLIC_IP" 2>/dev/null || echo "000")
    if [ "$HTTP_TEST" = "200" ] || [ "$HTTP_TEST" = "304" ] || [ "$HTTP_TEST" = "301" ] || [ "$HTTP_TEST" = "302" ]; then
        print_success "External HTTP access working (HTTP $HTTP_TEST)"
    else
        print_error "External HTTP access failed (HTTP $HTTP_TEST)"
        print_info "Check firewall and Nginx configuration"
    fi
fi

# 9. Check Recent Logs
print_header "Recent Errors (Last 10)"
if systemctl is-active --quiet baytaek-api; then
    ERROR_COUNT=$(journalctl -u baytaek-api -n 50 --no-pager 2>/dev/null | grep -i "error\|exception\|fatal" | wc -l)
    if [ "$ERROR_COUNT" -gt 0 ]; then
        print_warning "Found $ERROR_COUNT recent errors in backend logs"
        print_info "View with: sudo journalctl -u baytaek-api -n 50"
    else
        print_success "No recent errors in backend logs"
    fi
fi

if [ -f /var/log/nginx/error.log ]; then
    NGINX_ERRORS=$(tail -50 /var/log/nginx/error.log 2>/dev/null | grep -i "error" | wc -l)
    if [ "$NGINX_ERRORS" -gt 0 ]; then
        print_warning "Found $NGINX_ERRORS recent errors in Nginx logs"
        print_info "View with: sudo tail -50 /var/log/nginx/error.log"
    else
        print_success "No recent errors in Nginx logs"
    fi
fi

# 10. Summary
print_header "Summary"
ERRORS=0

systemctl is-active --quiet nginx || ((ERRORS++))
systemctl is-active --quiet baytaek-api || ((ERRORS++))
systemctl is-active --quiet baytaek-frontend || ((ERRORS++))

if [ $ERRORS -eq 0 ]; then
    print_success "All core services are running"
    echo ""
    print_info "Access your application at:"
    print_info "  http://$PUBLIC_IP"
    print_info "  http://$PUBLIC_IP/api/health"
else
    print_error "$ERRORS core service(s) not running"
    echo ""
    print_info "Next steps:"
    print_info "1. Run: bash scripts/vps-setup.sh"
    print_info "2. Deploy application via GitHub Actions"
    print_info "3. Check service logs: sudo journalctl -u baytaek-api -n 50"
fi

echo ""
print_header "Quick Commands"
print_info "Restart all:  sudo systemctl restart baytaek-api baytaek-frontend nginx"
print_info "View logs:    sudo journalctl -u baytaek-api -f"
print_info "Test backend: curl http://localhost:9000/health"
print_info "Test via web: http://$PUBLIC_IP/api/health"

echo ""
echo "=================================================="
echo "For detailed troubleshooting, see:"
echo "docs/TROUBLESHOOTING.md"
echo "=================================================="
