# Fly.io Setup Script for Windows (PowerShell)
# Run this script to set up Fly.io deployment

Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host "   Baytaek - Fly.io Setup Script"  -ForegroundColor Cyan
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host ""

# Check if Fly CLI is installed
Write-Host "Checking Fly CLI installation..." -ForegroundColor Yellow
if (!(Get-Command flyctl -ErrorAction SilentlyContinue)) {
    Write-Host "Installing Fly CLI..." -ForegroundColor Green
    iwr https://fly.io/install.ps1 -useb | iex

    Write-Host ""
    Write-Host "✅ Fly CLI installed successfully!" -ForegroundColor Green
    Write-Host "⚠️  Please close and reopen your terminal, then run this script again." -ForegroundColor Yellow
    exit
} else {
    $flyVersion = flyctl version
    Write-Host "✅ Fly CLI is installed: $flyVersion" -ForegroundColor Green
}

Write-Host ""
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host "   Step 1: Authenticate with Fly.io"  -ForegroundColor Cyan
Write-Host "=================================================="  -ForegroundColor Cyan

# Get token from user
$token = Read-Host "Enter your Fly.io token (or press Enter if already authenticated)"

if ($token) {
    Write-Host "Authenticating with provided token..." -ForegroundColor Yellow
    flyctl auth token $token
} else {
    Write-Host "Using existing authentication..." -ForegroundColor Yellow
}

# Verify authentication
Write-Host ""
Write-Host "Verifying authentication..." -ForegroundColor Yellow
flyctl auth whoami

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Authentication failed. Please check your token." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Successfully authenticated!" -ForegroundColor Green

Write-Host ""
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host "   Step 2: Create Backend App"  -ForegroundColor Cyan
Write-Host "=================================================="  -ForegroundColor Cyan

# Create backend app
Write-Host "Creating backend app: baytaek-api..." -ForegroundColor Yellow
flyctl apps create baytaek-api --org personal 2>$null

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Backend app created successfully!" -ForegroundColor Green
} else {
    Write-Host "⚠️  App may already exist (this is OK)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host "   Step 3: Create Frontend App"  -ForegroundColor Cyan
Write-Host "=================================================="  -ForegroundColor Cyan

# Create frontend app
Write-Host "Creating frontend app: baytaek-frontend..." -ForegroundColor Yellow
flyctl apps create baytaek-frontend --org personal 2>$null

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Frontend app created successfully!" -ForegroundColor Green
} else {
    Write-Host "⚠️  App may already exist (this is OK)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host "   Step 4: Create Persistent Volume for Database"  -ForegroundColor Cyan
Write-Host "=================================================="  -ForegroundColor Cyan

# Create volume
Write-Host "Creating volume: baytaek_data (1GB)..." -ForegroundColor Yellow
flyctl volumes create baytaek_data --region fra --size 1 --app baytaek-api 2>$null

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Volume created successfully!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Volume may already exist (this is OK)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host "   Step 5: Add GitHub Secret"  -ForegroundColor Cyan
Write-Host "=================================================="  -ForegroundColor Cyan

Write-Host ""
Write-Host "Next, add your Fly.io token to GitHub Secrets:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Go to: https://github.com/aramadan19912/BAYTAEK/settings/secrets/actions" -ForegroundColor White
Write-Host "2. Click 'New repository secret'" -ForegroundColor White
Write-Host "3. Name: FLY_API_TOKEN" -ForegroundColor White
Write-Host "4. Value: Your Fly.io token" -ForegroundColor White
Write-Host "5. Click 'Add secret'" -ForegroundColor White
Write-Host ""

$openGitHub = Read-Host "Open GitHub secrets page in browser? (y/n)"
if ($openGitHub -eq "y" -or $openGitHub -eq "Y") {
    Start-Process "https://github.com/aramadan19912/BAYTAEK/settings/secrets/actions"
}

Write-Host ""
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host "   ✅ Setup Complete!"  -ForegroundColor Green
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host ""
Write-Host "What happens next:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Add FLY_API_TOKEN to GitHub secrets (if not done)" -ForegroundColor White
Write-Host "2. Push your code to GitHub:" -ForegroundColor White
Write-Host "   git push origin main" -ForegroundColor Cyan
Write-Host "3. GitHub Actions will automatically deploy to Fly.io!" -ForegroundColor White
Write-Host ""
Write-Host "Your apps will be available at:" -ForegroundColor Yellow
Write-Host "  Backend:  https://baytaek-api.fly.dev" -ForegroundColor Green
Write-Host "  Frontend: https://baytaek-frontend.fly.dev" -ForegroundColor Green
Write-Host "  Swagger:  https://baytaek-api.fly.dev/swagger" -ForegroundColor Green
Write-Host ""
Write-Host "Useful commands:" -ForegroundColor Yellow
Write-Host "  flyctl logs -a baytaek-api        # View backend logs" -ForegroundColor Cyan
Write-Host "  flyctl status -a baytaek-api      # Check backend status" -ForegroundColor Cyan
Write-Host "  flyctl apps list                  # List all your apps" -ForegroundColor Cyan
Write-Host ""
Write-Host "Cost: \$0/month (Free Tier) 🎉" -ForegroundColor Green
Write-Host "=================================================="  -ForegroundColor Cyan
