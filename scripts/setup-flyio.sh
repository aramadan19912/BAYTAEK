#!/bin/bash

# Fly.io Setup Script for Linux/Mac
# Run this script to set up Fly.io deployment

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}   Baytaek - Fly.io Setup Script${NC}"
echo -e "${CYAN}==================================================${NC}"
echo ""

# Check if Fly CLI is installed
echo -e "${YELLOW}Checking Fly CLI installation...${NC}"
if ! command -v flyctl &> /dev/null; then
    echo -e "${GREEN}Installing Fly CLI...${NC}"
    curl -L https://fly.io/install.sh | sh

    echo ""
    echo -e "${GREEN}✅ Fly CLI installed successfully!${NC}"
    echo -e "${YELLOW}⚠️  Please add Fly CLI to your PATH and run this script again:${NC}"
    echo -e "${CYAN}   export PATH=\"\$HOME/.fly/bin:\$PATH\"${NC}"
    echo -e "${CYAN}   source ~/.bashrc  # or source ~/.zshrc${NC}"
    exit 0
else
    FLY_VERSION=$(flyctl version)
    echo -e "${GREEN}✅ Fly CLI is installed: $FLY_VERSION${NC}"
fi

echo ""
echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}   Step 1: Authenticate with Fly.io${NC}"
echo -e "${CYAN}==================================================${NC}"

# Get token from user
read -p "Enter your Fly.io token (or press Enter if already authenticated): " token

if [ ! -z "$token" ]; then
    echo -e "${YELLOW}Authenticating with provided token...${NC}"
    flyctl auth token "$token"
else
    echo -e "${YELLOW}Using existing authentication...${NC}"
fi

# Verify authentication
echo ""
echo -e "${YELLOW}Verifying authentication...${NC}"
if ! flyctl auth whoami; then
    echo -e "${RED}❌ Authentication failed. Please check your token.${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Successfully authenticated!${NC}"

echo ""
echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}   Step 2: Create Backend App${NC}"
echo -e "${CYAN}==================================================${NC}"

# Create backend app
echo -e "${YELLOW}Creating backend app: baytaek-api...${NC}"
if flyctl apps create baytaek-api --org personal 2>/dev/null; then
    echo -e "${GREEN}✅ Backend app created successfully!${NC}"
else
    echo -e "${YELLOW}⚠️  App may already exist (this is OK)${NC}"
fi

echo ""
echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}   Step 3: Create Frontend App${NC}"
echo -e "${CYAN}==================================================${NC}"

# Create frontend app
echo -e "${YELLOW}Creating frontend app: baytaek-frontend...${NC}"
if flyctl apps create baytaek-frontend --org personal 2>/dev/null; then
    echo -e "${GREEN}✅ Frontend app created successfully!${NC}"
else
    echo -e "${YELLOW}⚠️  App may already exist (this is OK)${NC}"
fi

echo ""
echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}   Step 4: Create Persistent Volume for Database${NC}"
echo -e "${CYAN}==================================================${NC}"

# Create volume
echo -e "${YELLOW}Creating volume: baytaek_data (1GB)...${NC}"
if flyctl volumes create baytaek_data --region fra --size 1 --app baytaek-api 2>/dev/null; then
    echo -e "${GREEN}✅ Volume created successfully!${NC}"
else
    echo -e "${YELLOW}⚠️  Volume may already exist (this is OK)${NC}"
fi

echo ""
echo -e "${CYAN}==================================================${NC}"
echo -e "${CYAN}   Step 5: Add GitHub Secret${NC}"
echo -e "${CYAN}==================================================${NC}"

echo ""
echo -e "${YELLOW}Next, add your Fly.io token to GitHub Secrets:${NC}"
echo ""
echo "1. Go to: https://github.com/aramadan19912/BAYTAEK/settings/secrets/actions"
echo "2. Click 'New repository secret'"
echo "3. Name: FLY_API_TOKEN"
echo "4. Value: Your Fly.io token"
echo "5. Click 'Add secret'"
echo ""

read -p "Open GitHub secrets page in browser? (y/n): " open_github
if [[ "$open_github" == "y" || "$open_github" == "Y" ]]; then
    if command -v xdg-open &> /dev/null; then
        xdg-open "https://github.com/aramadan19912/BAYTAEK/settings/secrets/actions"
    elif command -v open &> /dev/null; then
        open "https://github.com/aramadan19912/BAYTAEK/settings/secrets/actions"
    else
        echo "Please open the URL manually in your browser"
    fi
fi

echo ""
echo -e "${CYAN}==================================================${NC}"
echo -e "${GREEN}   ✅ Setup Complete!${NC}"
echo -e "${CYAN}==================================================${NC}"
echo ""
echo -e "${YELLOW}What happens next:${NC}"
echo ""
echo "1. Add FLY_API_TOKEN to GitHub secrets (if not done)"
echo "2. Push your code to GitHub:"
echo -e "${CYAN}   git push origin main${NC}"
echo "3. GitHub Actions will automatically deploy to Fly.io!"
echo ""
echo -e "${YELLOW}Your apps will be available at:${NC}"
echo -e "${GREEN}  Backend:  https://baytaek-api.fly.dev${NC}"
echo -e "${GREEN}  Frontend: https://baytaek-frontend.fly.dev${NC}"
echo -e "${GREEN}  Swagger:  https://baytaek-api.fly.dev/swagger${NC}"
echo ""
echo -e "${YELLOW}Useful commands:${NC}"
echo -e "${CYAN}  flyctl logs -a baytaek-api        # View backend logs${NC}"
echo -e "${CYAN}  flyctl status -a baytaek-api      # Check backend status${NC}"
echo -e "${CYAN}  flyctl apps list                  # List all your apps${NC}"
echo ""
echo -e "${GREEN}Cost: \$0/month (Free Tier) 🎉${NC}"
echo -e "${CYAN}==================================================${NC}"
