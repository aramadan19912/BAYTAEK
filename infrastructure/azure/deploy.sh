#!/bin/bash

# ============================================
# Azure Infrastructure Deployment Script
# ============================================

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# ============================================
# Configuration
# ============================================
ENVIRONMENT=${1:-staging}
LOCATION=${2:-eastus}
BASE_NAME="homeservice"
RESOURCE_GROUP="${BASE_NAME}-${ENVIRONMENT}-rg"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Azure Infrastructure Deployment${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "Environment: ${YELLOW}${ENVIRONMENT}${NC}"
echo -e "Location: ${YELLOW}${LOCATION}${NC}"
echo -e "Resource Group: ${YELLOW}${RESOURCE_GROUP}${NC}"
echo ""

# ============================================
# Check Azure CLI
# ============================================
if ! command -v az &> /dev/null; then
    echo -e "${RED}❌ Azure CLI is not installed${NC}"
    echo "Please install: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# ============================================
# Login to Azure
# ============================================
echo -e "${GREEN}🔐 Checking Azure login...${NC}"
az account show &> /dev/null || {
    echo -e "${YELLOW}Please login to Azure${NC}"
    az login
}

# Get subscription info
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
echo -e "Subscription: ${YELLOW}${SUBSCRIPTION_NAME}${NC} (${SUBSCRIPTION_ID})"

# ============================================
# Confirm Deployment
# ============================================
echo ""
read -p "Do you want to proceed with deployment? (yes/no): " -r
if [[ ! $REPLY =~ ^[Yy]es$ ]]; then
    echo -e "${RED}Deployment cancelled${NC}"
    exit 0
fi

# ============================================
# Create Resource Group
# ============================================
echo ""
echo -e "${GREEN}📦 Creating resource group...${NC}"
az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output table

# ============================================
# Get SQL Credentials
# ============================================
echo ""
echo -e "${GREEN}🔑 SQL Server credentials${NC}"
read -p "Enter SQL Admin Username: " SQL_ADMIN_USERNAME
read -sp "Enter SQL Admin Password: " SQL_ADMIN_PASSWORD
echo ""

# Validate password strength
if [ ${#SQL_ADMIN_PASSWORD} -lt 12 ]; then
    echo -e "${RED}❌ Password must be at least 12 characters${NC}"
    exit 1
fi

# ============================================
# Deploy Infrastructure
# ============================================
echo ""
echo -e "${GREEN}🚀 Deploying infrastructure...${NC}"
DEPLOYMENT_NAME="homeservice-deployment-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --template-file main.bicep \
    --parameters \
        environment="$ENVIRONMENT" \
        location="$LOCATION" \
        baseName="$BASE_NAME" \
        sqlAdminUsername="$SQL_ADMIN_USERNAME" \
        sqlAdminPassword="$SQL_ADMIN_PASSWORD" \
    --output table

# ============================================
# Get Deployment Outputs
# ============================================
echo ""
echo -e "${GREEN}📋 Deployment outputs:${NC}"

WEB_APP_URL=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.webAppUrl.value \
    -o tsv)

SQL_SERVER_FQDN=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.sqlServerFqdn.value \
    -o tsv)

APP_INSIGHTS_KEY=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.appInsightsInstrumentationKey.value \
    -o tsv)

ACR_LOGIN_SERVER=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.containerRegistryLoginServer.value \
    -o tsv)

KEY_VAULT_URI=$(az deployment group show \
    --name "$DEPLOYMENT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query properties.outputs.keyVaultUri.value \
    -o tsv)

echo -e "Web App URL: ${YELLOW}${WEB_APP_URL}${NC}"
echo -e "SQL Server: ${YELLOW}${SQL_SERVER_FQDN}${NC}"
echo -e "App Insights Key: ${YELLOW}${APP_INSIGHTS_KEY}${NC}"
echo -e "Container Registry: ${YELLOW}${ACR_LOGIN_SERVER}${NC}"
echo -e "Key Vault: ${YELLOW}${KEY_VAULT_URI}${NC}"

# ============================================
# Save Configuration
# ============================================
CONFIG_FILE="deployment-config-${ENVIRONMENT}.json"
cat > "$CONFIG_FILE" <<EOF
{
  "environment": "$ENVIRONMENT",
  "resourceGroup": "$RESOURCE_GROUP",
  "webAppUrl": "$WEB_APP_URL",
  "sqlServerFqdn": "$SQL_SERVER_FQDN",
  "appInsightsKey": "$APP_INSIGHTS_KEY",
  "containerRegistryLoginServer": "$ACR_LOGIN_SERVER",
  "keyVaultUri": "$KEY_VAULT_URI",
  "deploymentDate": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")"
}
EOF

echo ""
echo -e "${GREEN}✅ Deployment completed successfully!${NC}"
echo -e "Configuration saved to: ${YELLOW}${CONFIG_FILE}${NC}"

# ============================================
# Next Steps
# ============================================
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Next Steps${NC}"
echo -e "${GREEN}========================================${NC}"
echo "1. Configure GitHub Secrets for CI/CD:"
echo "   - AZURE_CREDENTIALS"
echo "   - ACR_USERNAME"
echo "   - ACR_PASSWORD"
echo "   - SQL_CONNECTION_STRING"
echo ""
echo "2. Build and push Docker image:"
echo "   az acr login --name ${BASE_NAME}${ENVIRONMENT}acr"
echo "   docker build -t ${ACR_LOGIN_SERVER}/homeservice-api:latest ./backend"
echo "   docker push ${ACR_LOGIN_SERVER}/homeservice-api:latest"
echo ""
echo "3. Run database migrations"
echo ""
echo "4. Test the deployment:"
echo "   curl ${WEB_APP_URL}/health"
echo ""
