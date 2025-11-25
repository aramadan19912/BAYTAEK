# Script to manually create Fly.io apps
# Run this if GitHub Actions encounters authorization issues

Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host "   Create Fly.io Apps Manually"  -ForegroundColor Cyan
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host ""

# Check if flyctl is installed
if (!(Get-Command flyctl -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Fly CLI not installed. Installing..." -ForegroundColor Red
    Write-Host ""
    Write-Host "Run this command first:" -ForegroundColor Yellow
    Write-Host "  powershell -Command `"iwr https://fly.io/install.ps1 -useb | iex`"" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

Write-Host "✅ Fly CLI is installed" -ForegroundColor Green
Write-Host ""

# Authenticate
Write-Host "Step 1: Authenticate with Fly.io" -ForegroundColor Yellow
Write-Host "--------------------------------------" -ForegroundColor Yellow
Write-Host ""

$token = Read-Host "Enter your Fly.io API token"

flyctl auth token $token

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Authentication failed" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Authenticated successfully" -ForegroundColor Green
Write-Host ""

# Create backend app
Write-Host "Step 2: Create Backend App" -ForegroundColor Yellow
Write-Host "--------------------------------------" -ForegroundColor Yellow
Write-Host ""

$backendExists = flyctl apps list | Select-String "baytaek-api"

if ($backendExists) {
    Write-Host "⚠️  Backend app 'baytaek-api' already exists" -ForegroundColor Yellow
} else {
    Write-Host "Creating backend app 'baytaek-api'..." -ForegroundColor Cyan

    # Navigate to backend directory
    Push-Location "$PSScriptRoot\..\backend"

    # Create app using the fly.toml configuration
    flyctl apps create baytaek-api

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Backend app created successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to create backend app" -ForegroundColor Red
        Pop-Location
        exit 1
    }

    Pop-Location
}

Write-Host ""

# Create frontend app
Write-Host "Step 3: Create Frontend App" -ForegroundColor Yellow
Write-Host "--------------------------------------" -ForegroundColor Yellow
Write-Host ""

$frontendExists = flyctl apps list | Select-String "baytaek-frontend"

if ($frontendExists) {
    Write-Host "⚠️  Frontend app 'baytaek-frontend' already exists" -ForegroundColor Yellow
} else {
    Write-Host "Creating frontend app 'baytaek-frontend'..." -ForegroundColor Cyan

    # Navigate to frontend directory
    Push-Location "$PSScriptRoot\..\frontend"

    # Create app using the fly.toml configuration
    flyctl apps create baytaek-frontend

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Frontend app created successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to create frontend app" -ForegroundColor Red
        Pop-Location
        exit 1
    }

    Pop-Location
}

Write-Host ""

# Create volume for backend
Write-Host "Step 4: Create Database Volume" -ForegroundColor Yellow
Write-Host "--------------------------------------" -ForegroundColor Yellow
Write-Host ""

$volumeExists = flyctl volumes list -a baytaek-api | Select-String "baytaek_data"

if ($volumeExists) {
    Write-Host "⚠️  Volume 'baytaek_data' already exists" -ForegroundColor Yellow
} else {
    Write-Host "Creating volume 'baytaek_data' for database..." -ForegroundColor Cyan

    flyctl volumes create baytaek_data --region fra --size 1 --app baytaek-api --yes

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Volume created successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to create volume" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host "   ✅ Setup Complete!"  -ForegroundColor Green
Write-Host "=================================================="  -ForegroundColor Cyan
Write-Host ""
Write-Host "Apps created:" -ForegroundColor Yellow
Write-Host "  - baytaek-api (backend)" -ForegroundColor Cyan
Write-Host "  - baytaek-frontend (frontend)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Make sure FLY_API_TOKEN is added to GitHub secrets" -ForegroundColor White
Write-Host "2. Push to GitHub or trigger workflow manually" -ForegroundColor White
Write-Host "3. GitHub Actions will now be able to deploy to these apps" -ForegroundColor White
Write-Host ""
Write-Host "URLs after deployment:" -ForegroundColor Yellow
Write-Host "  Backend:  https://baytaek-api.fly.dev" -ForegroundColor Green
Write-Host "  Frontend: https://baytaek-frontend.fly.dev" -ForegroundColor Green
Write-Host ""
