# Infrastructure & DevOps Documentation

## Overview

This directory contains infrastructure as code (IaC) and deployment configurations for the Home Service Platform.

## Directory Structure

```
infrastructure/
├── azure/                     # Azure deployment files
│   ├── main.bicep            # Main Bicep template
│   ├── parameters.staging.json
│   ├── parameters.production.json
│   └── deploy.sh             # Deployment script
└── README.md
```

## Quick Start

### Prerequisites

1. **Azure CLI**
   ```bash
   # Install Azure CLI
   curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

   # Login
   az login
   ```

2. **Docker**
   ```bash
   # Install Docker
   curl -fsSL https://get.docker.com -o get-docker.sh
   sudo sh get-docker.sh
   ```

3. **Git**
   ```bash
   sudo apt-get install git
   ```

### Deploy to Azure

1. **Clone Repository**
   ```bash
   git clone https://github.com/your-org/homeservice.git
   cd homeservice/infrastructure/azure
   ```

2. **Run Deployment Script**
   ```bash
   chmod +x deploy.sh
   ./deploy.sh staging eastus
   ```

3. **Configure Secrets**
   ```bash
   # Add secrets to GitHub
   gh secret set AZURE_CREDENTIALS
   gh secret set ACR_USERNAME
   gh secret set ACR_PASSWORD
   gh secret set SQL_CONNECTION_STRING
   ```

## Architecture

### Production Architecture

```
                                    ┌─────────────┐
                                    │  Cloudflare │
                                    │  (CDN/WAF)  │
                                    └──────┬──────┘
                                           │
                                    ┌──────▼──────┐
                                    │Azure Front  │
                                    │    Door     │
                                    └──────┬──────┘
                                           │
                        ┌──────────────────┼──────────────────┐
                        │                  │                  │
                   ┌────▼────┐        ┌───▼────┐      ┌─────▼─────┐
                   │  App    │        │  App   │      │   App     │
                   │ Service │        │Service │      │  Service  │
                   │ (West)  │        │(East)  │      │  (Central)│
                   └────┬────┘        └───┬────┘      └─────┬─────┘
                        │                 │                  │
                        └────────┬────────┴──────────────────┘
                                 │
                        ┌────────▼────────┐
                        │  Azure SQL DB   │
                        │  (Geo-Replicated)│
                        └────────┬────────┘
                                 │
                        ┌────────▼────────┐
                        │ Azure Redis     │
                        │    Cache        │
                        └─────────────────┘
```

### Services

| Service | Purpose | SKU | Monthly Cost |
|---------|---------|-----|--------------|
| App Service Plan | Host API | B1/P1V2 | $13-$146 |
| Azure SQL Database | Primary database | Basic/Standard | $5-$30 |
| Azure Redis Cache | Caching/Sessions | Basic C0 | $16 |
| Azure Storage | File storage | Standard LRS | $0.02/GB |
| Application Insights | Monitoring | Pay-as-you-go | ~$10 |
| Container Registry | Docker images | Basic | $5 |
| Key Vault | Secrets management | Standard | $0.03/10k ops |
| **Total (Staging)** | | | **~$60/month** |
| **Total (Production)** | | | **~$250/month** |

## CI/CD Pipeline

### GitHub Actions Workflows

1. **`.github/workflows/ci.yml`** - Continuous Integration
   - Runs on: Pull requests, pushes to develop
   - Steps:
     - Build & test
     - Code quality analysis
     - Security scanning
     - Docker build test

2. **`.github/workflows/cd-azure.yml`** - Continuous Deployment
   - Runs on: Push to main, manual trigger
   - Steps:
     - Build & push Docker image
     - Run database migrations
     - Deploy to Azure App Service
     - Run smoke tests

3. **`.github/workflows/security-scan.yml`** - Security Scanning
   - Runs on: Daily schedule, PRs
   - Steps:
     - Dependency scanning
     - OWASP dependency check
     - Container scanning
     - Secret scanning
     - CodeQL analysis

### GitHub Secrets

Required secrets for CI/CD:

```bash
# Azure credentials
AZURE_CREDENTIALS            # Service principal JSON
ACR_USERNAME                # Container registry username
ACR_PASSWORD                # Container registry password

# Database
SQL_CONNECTION_STRING       # SQL Server connection string

# Application
JWT_SECRET_KEY              # JWT signing key
SENDGRID_API_KEY           # Email service
TWILIO_ACCOUNT_SID         # SMS service
TWILIO_AUTH_TOKEN          # SMS service
FCM_SERVER_KEY             # Push notifications
STRIPE_SECRET_KEY          # Payment gateway
STRIPE_WEBHOOK_SECRET      # Payment webhooks

# Azure Services
AZURE_STORAGE_CONNECTION_STRING
APPLICATIONINSIGHTS_CONNECTION_STRING

# Optional
SLACK_WEBHOOK_URL          # Deployment notifications
```

### Setting Secrets

```bash
# Using GitHub CLI
gh secret set AZURE_CREDENTIALS < azure-credentials.json
gh secret set JWT_SECRET_KEY --body "your-secret-key"

# Using GitHub Web UI
# Repository → Settings → Secrets and variables → Actions
```

## Docker Setup

### Local Development

1. **Start Services**
   ```bash
   cd backend
   docker-compose up -d
   ```

2. **View Logs**
   ```bash
   docker-compose logs -f api
   ```

3. **Stop Services**
   ```bash
   docker-compose down
   ```

### Build Production Image

```bash
cd backend
docker build -t homeservice-api:latest .
docker run -p 8080:8080 homeservice-api:latest
```

### Docker Compose Services

- **api**: Backend API (port 5000)
- **sqlserver**: SQL Server 2022 (port 1433)
- **redis**: Redis cache (port 6379)
- **seq**: Logging UI (port 5341)

## Database Migrations

### Entity Framework Core

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Create migration
cd backend/src/HomeService.API
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script -o migration.sql
```

### Production Migrations

Migrations run automatically in CI/CD pipeline:

```bash
dotnet ef database update \
  --connection "$SQL_CONNECTION_STRING"
```

## Monitoring & Logging

### Application Insights

Access: https://portal.azure.com → Application Insights

**Key Metrics:**
- Request rate & duration
- Failure rate
- Dependency calls
- Exceptions
- Custom events

**Useful Queries:**

```kusto
// Failed requests in last hour
requests
| where timestamp > ago(1h)
| where success == false
| summarize count() by resultCode, operation_Name
| order by count_ desc

// Slow requests
requests
| where timestamp > ago(1h)
| where duration > 5000
| project timestamp, name, duration, operation_Id
| order by duration desc

// Top 10 exceptions
exceptions
| where timestamp > ago(24h)
| summarize count() by type, outerMessage
| top 10 by count_
```

### Health Checks

- **Endpoint**: `/health`
- **Dashboard**: `/health-ui`
- **Checks**:
  - SQL Server connectivity
  - Redis connectivity
  - Hangfire job server
  - Disk space
  - Memory usage

### Logging

**Serilog** structured logging to:
- Console (development)
- Files (`logs/homeservice-*.log`)
- Application Insights (production)
- Seq (optional, development)

Access Seq: http://localhost:5341

## Security

### SSL/TLS Certificates

**Production**: Azure App Service manages certificates automatically
**Custom Domain**: Use Cloudflare for SSL

### Secrets Management

**Azure Key Vault** stores:
- Database credentials
- API keys
- Connection strings
- Certificates

Access secrets in code:
```csharp
var secret = builder.Configuration["KeyVault:SecretName"];
```

### Security Headers

Implemented via `SecurityHeadersMiddleware`:
- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- Strict-Transport-Security
- Content-Security-Policy
- Referrer-Policy
- Permissions-Policy

### Rate Limiting

Implemented via `RateLimitingMiddleware`:
- General: 100 requests/minute
- Auth: 5 requests/minute
- Password reset: 3 requests/hour

## Scaling

### Horizontal Scaling

```bash
# Scale out App Service
az appservice plan update \
  --name homeservice-asp \
  --resource-group homeservice-rg \
  --number-of-workers 3
```

### Vertical Scaling

```bash
# Scale up to P2V2
az appservice plan update \
  --name homeservice-asp \
  --resource-group homeservice-rg \
  --sku P2V2
```

### Auto-Scaling Rules

```bash
# CPU-based auto-scale
az monitor autoscale create \
  --resource-group homeservice-rg \
  --resource homeservice-asp \
  --resource-type Microsoft.Web/serverfarms \
  --min-count 2 \
  --max-count 10 \
  --count 2

az monitor autoscale rule create \
  --resource-group homeservice-rg \
  --autoscale-name homeservice-autoscale \
  --condition "Percentage CPU > 70 avg 5m" \
  --scale out 1
```

## Disaster Recovery

### Backup Strategy

**Automated Backups:**
- SQL Database: Daily, 30-day retention
- Redis: Daily export to storage
- Application files: Container Registry tags
- Configuration: Git repository

**Manual Backup:**
```bash
# Export database
az sql db export \
  --resource-group homeservice-rg \
  --server homeservice-sql \
  --name homeservicedb \
  --admin-user sqladmin \
  --admin-password $PASSWORD \
  --storage-key $STORAGE_KEY \
  --storage-key-type StorageAccessKey \
  --storage-uri https://storage.blob.core.windows.net/backups/db.bacpac
```

### Recovery Time Objective (RTO)

- Database restore: < 1 hour
- Application redeployment: < 30 minutes
- Full system recovery: < 2 hours

### Recovery Point Objective (RPO)

- Database: < 5 minutes (transaction log)
- Files: < 24 hours (daily backup)

## Cost Optimization

### Tips

1. **Use Reserved Instances** - Save up to 72% on compute
2. **Right-size Resources** - Match SKU to actual usage
3. **Enable Auto-shutdown** - Dev/test environments
4. **Use Azure Hybrid Benefit** - If you have Windows licenses
5. **Monitor Spending** - Set up cost alerts
6. **Clean Up Unused Resources** - Delete old deployments

### Cost Monitoring

```bash
# View current costs
az consumption usage list \
  --start-date 2024-01-01 \
  --end-date 2024-01-31

# Set budget alert
az consumption budget create \
  --budget-name monthly-budget \
  --amount 500 \
  --time-grain Monthly \
  --start-date 2024-01-01
```

## Troubleshooting

### Common Issues

**Issue: Deployment fails**
```bash
# Check deployment logs
az webapp log tail \
  --name homeservice-api-staging \
  --resource-group homeservice-rg

# Check container logs
az webapp log download \
  --name homeservice-api-staging \
  --resource-group homeservice-rg \
  --log-file logs.zip
```

**Issue: Database connection fails**
```bash
# Test connection
sqlcmd -S homeservice-sql.database.windows.net \
  -d homeservicedb \
  -U sqladmin \
  -P $PASSWORD

# Check firewall rules
az sql server firewall-rule list \
  --resource-group homeservice-rg \
  --server homeservice-sql
```

**Issue: High response times**
```bash
# Check Application Insights
# Performance → Server response time
# Investigate slow queries and dependencies
```

## Support

- **DevOps Issues**: devops@homeservice.com
- **Azure Support**: https://portal.azure.com/#create/Microsoft.Support
- **Documentation**: https://docs.homeservice.com

## Resources

- [Azure Documentation](https://docs.microsoft.com/azure)
- [Bicep Documentation](https://docs.microsoft.com/azure/azure-resource-manager/bicep)
- [Docker Documentation](https://docs.docker.com)
- [GitHub Actions](https://docs.github.com/actions)
