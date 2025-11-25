# Azure Free Tier - Complete Setup Guide

## What You Get with Azure Free Tier

### 12 Months Free Services
- **Azure App Service**: 10 web apps (F1 tier)
- **Azure SQL Database**: 250 GB storage
- **Azure Blob Storage**: 5 GB
- **Azure Functions**: 1 million executions/month
- **$200 Credit** for first 30 days to try any Azure service

### Always Free Services (After 12 months)
- **Azure Functions**: 1 million executions/month (forever)
- **Azure Cosmos DB**: 1000 RU/s + 25 GB storage
- **Azure Static Web Apps**: 100 GB bandwidth/month

## Step-by-Step Subscription Process

### Step 1: Create Microsoft Account (If You Don't Have One)

1. Go to: https://signup.live.com
2. Click "Get a new email address"
3. Create account with email like: `yourname@outlook.com`
4. Set password and complete verification

### Step 2: Sign Up for Azure Free Account

1. **Go to Azure Free Tier Page**
   ```
   https://azure.microsoft.com/en-us/free/
   ```

2. **Click "Start free"** (big green button)

3. **Sign in** with your Microsoft account

4. **Fill in Your Information**
   - Country/Region: Select your country
   - First name: Your first name
   - Last name: Your last name
   - Email: Your Microsoft account email
   - Phone: Your phone number (for verification)

5. **Identity Verification by Phone**
   - Enter your phone number
   - Choose "Text me" or "Call me"
   - Enter the verification code you receive
   - Click "Verify code"

6. **Identity Verification by Card**
   ⚠️ **IMPORTANT**: You need a credit/debit card for verification

   **What you need to know**:
   - Azure will charge $1 (or local equivalent) temporarily for verification
   - This charge will be refunded within 3-5 business days
   - Your card will NOT be charged unless you upgrade to pay-as-you-go
   - You'll get notifications before free tier expires

   **Card Information Required**:
   - Card number
   - Expiration date (MM/YY)
   - CVV/Security code
   - Cardholder name
   - Billing address

7. **Agreement**
   - Read the terms
   - Check the boxes:
     - ✅ I agree to the subscription agreement
     - ✅ I agree to the offer details
     - ✅ I agree to the privacy statement
   - Click "Sign up"

8. **Wait for Setup**
   - Azure will set up your subscription (1-2 minutes)
   - You'll see "Your subscription is ready"

9. **Access Azure Portal**
   - Click "Go to Azure Portal"
   - URL: https://portal.azure.com

### Step 3: Verify Your Free Subscription

1. **In Azure Portal**, click on "Subscriptions" (left menu or search bar)

2. **Check Your Subscription**
   - Name: "Azure subscription 1" or "Free Trial"
   - Status: Should be "Active"
   - Subscription ID: (copy this, you'll need it)

3. **Check Your Credit**
   - Go to "Cost Management + Billing" → "Credits + Commitments"
   - You should see: **$200.00** available
   - Valid for: 30 days from signup

## What to Do After Signup

### Option A: Deploy Baytaek to Azure (Recommended)

I can help you deploy your application to Azure using one of these approaches:

#### 1. Azure App Service (Easiest)
- **Cost**: FREE for 12 months (F1 tier)
- **Setup time**: 10-15 minutes
- **What we'll do**:
  - Create App Service for backend
  - Create Static Web App for frontend
  - Set up automatic deployment from GitHub
  - Configure database (SQLite or Azure SQL free tier)

#### 2. Azure Container Instances
- **Cost**: Uses your $200 credit (~$10-15/month normally)
- **Setup time**: 15-20 minutes
- **What we'll do**:
  - Build Docker containers
  - Deploy to Azure Container Instances
  - Set up networking and domain

### Option B: Keep Using Hostinger VPS

- Your current deployment should be working now
- Azure free tier can be used for:
  - Staging/testing environment
  - Database backups
  - Static file hosting
  - Future scaling

## Important Notes About Free Tier

### ✅ You Will NOT Be Charged If:
- You stay within free tier limits
- You don't upgrade to pay-as-you-go
- Your $200 credit runs out (services just stop, no charges)

### ⚠️ You WILL Be Charged If:
- You manually upgrade to pay-as-you-go
- You exceed free tier quotas AND have enabled pay-as-you-go
- You deploy resources outside free tier (like expensive VMs)

### 🔔 Billing Alerts (Recommended Setup)

After signup, set up billing alerts:

1. Go to "Cost Management + Billing"
2. Click "Cost alerts"
3. Click "+ Add"
4. Create alerts for:
   - $50 spent (25% of credit)
   - $100 spent (50% of credit)
   - $150 spent (75% of credit)
   - $190 spent (95% of credit)

## Common Issues and Solutions

### Issue 1: Card Declined
**Solution**:
- Use a different card (Visa/Mastercard work best)
- Make sure your card supports international transactions
- Contact your bank to allow the verification charge

### Issue 2: Phone Verification Failed
**Solution**:
- Try "Call me" instead of "Text me"
- Use a different phone number
- Wait 24 hours and try again

### Issue 3: "This offer is not available in your region"
**Solution**:
- Check if Azure is available in your country: https://azure.microsoft.com/en-us/explore/global-infrastructure/geographies/
- Use a VPN to a supported region (not recommended for production)
- Try Azure for Students (no credit card needed): https://azure.microsoft.com/en-us/free/students/

### Issue 4: Already Used Free Trial
**Solution**:
- Free trial is once per Microsoft account/credit card
- Create a new Microsoft account with different card
- Use pay-as-you-go with cost controls

## Next Steps After Azure Signup

Choose one of these paths:

### Path 1: Deploy to Azure Free Tier (I can help you)
Tell me you want to deploy to Azure and I'll:
1. Create Azure App Service configuration
2. Set up GitHub Actions for Azure deployment
3. Configure database and storage
4. Deploy frontend and backend
5. Set up custom domain (optional)

### Path 2: Use Azure as Backup/Staging
- Keep Hostinger for production
- Use Azure for testing new features
- Learn Azure gradually

### Path 3: Hybrid Approach
- Backend on Azure App Service
- Frontend on Azure Static Web Apps
- Database on Azure SQL (free tier)
- Use Hostinger as backup

## Azure Free Tier vs Hostinger VPS Comparison

| Feature | Azure Free Tier | Hostinger VPS |
|---------|----------------|---------------|
| **Cost (First Year)** | $0 + $200 credit | $48-120/year |
| **Setup Complexity** | Low (I'll help) | Medium |
| **Reliability** | 99.95% SLA | 99.9% SLA |
| **Scalability** | Easy | Manual |
| **SSL Certificate** | Automatic | Manual (Let's Encrypt) |
| **CI/CD Integration** | Built-in | Custom setup |
| **Database** | Azure SQL (free tier) | SQLite/Self-managed |
| **Monitoring** | Application Insights (free) | Manual setup |
| **Backup** | Automatic | Manual |

## Quick Start Commands (After Azure Signup)

### Install Azure CLI (Optional but Recommended)

**Windows (PowerShell)**:
```powershell
Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi
Start-Process msiexec.exe -Wait -ArgumentList '/I AzureCLI.msi /quiet'
```

**Mac**:
```bash
brew update && brew install azure-cli
```

**Linux**:
```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

### Login to Azure CLI

```bash
az login
```

### Check Your Subscription

```bash
# List subscriptions
az account list --output table

# Show current subscription
az account show

# Check remaining credit
az consumption budget list
```

## Need Help?

After you sign up for Azure free tier, let me know and I can:

1. ✅ Deploy your Baytaek application to Azure
2. ✅ Set up GitHub Actions for automatic deployment
3. ✅ Configure database and storage
4. ✅ Set up custom domain and SSL
5. ✅ Configure monitoring and alerts
6. ✅ Optimize costs to stay within free tier

---

## Summary: What You Need to Sign Up

✅ Microsoft account (Outlook email)
✅ Valid credit/debit card (for verification only)
✅ Phone number (for SMS/call verification)
✅ 10-15 minutes of time

**Ready to start?** Go to: https://azure.microsoft.com/en-us/free/

After signup, come back and tell me - I'll help you deploy Baytaek to Azure! 🚀
