# CI/CD Setup Guide for BAYTAEK

This guide will help you set up continuous integration and deployment for your BAYTAEK application using GitHub Actions and Hostinger VPS.

---

## 📋 Overview

We've created **3 GitHub Actions workflows**:

1. **`deploy.yml`** - Deploys to production (Hostinger VPS) when pushing to `main` branch
2. **`test.yml`** - Runs tests on pull requests and development branch
3. **`staging.yml`** - Deploys to staging environment from `development` branch

---

## 🔧 Setup Steps

### Step 1: Generate SSH Key for GitHub Actions

On your **local machine** or **VPS**, generate an SSH key pair:

```bash
# Generate SSH key (no passphrase for automation)
ssh-keygen -t ed25519 -C "github-actions@baytaek" -f ~/.ssh/github_actions_baytaek

# This creates two files:
# - github_actions_baytaek (private key)
# - github_actions_baytaek.pub (public key)
```

### Step 2: Add Public Key to VPS

Copy the **public key** to your Hostinger VPS:

```bash
# Copy public key to clipboard
cat ~/.ssh/github_actions_baytaek.pub

# SSH into your VPS
ssh root@your-vps-ip

# Add the public key to authorized_keys
mkdir -p ~/.ssh
echo "PUBLIC_KEY_CONTENT_HERE" >> ~/.ssh/authorized_keys
chmod 700 ~/.ssh
chmod 600 ~/.ssh/authorized_keys

# If deploying as a different user (e.g., www-data), add to that user's authorized_keys:
sudo mkdir -p /home/deploy/.ssh
sudo echo "PUBLIC_KEY_CONTENT_HERE" >> /home/deploy/.ssh/authorized_keys
sudo chown -R deploy:deploy /home/deploy/.ssh
sudo chmod 700 /home/deploy/.ssh
sudo chmod 600 /home/deploy/.ssh/authorized_keys
```

### Step 3: Add GitHub Secrets

Go to your GitHub repository → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

Add the following secrets:

#### Required Secrets:

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `VPS_HOST` | Your VPS IP address or domain | `123.45.67.89` |
| `VPS_USER` | SSH user (usually `root` or `deploy`) | `root` |
| `VPS_SSH_PRIVATE_KEY` | Private SSH key content | Content of `github_actions_baytaek` file |

#### Optional Secrets (for staging):

| Secret Name | Description |
|-------------|-------------|
| `STAGING_HOST` | Staging server IP/domain |
| `STAGING_USER` | Staging SSH user |
| `STAGING_SSH_PRIVATE_KEY` | Staging SSH private key |

#### How to add the SSH private key:

```bash
# On your local machine, copy the entire private key:
cat ~/.ssh/github_actions_baytaek

# Copy the entire output including:
# -----BEGIN OPENSSH PRIVATE KEY-----
# ...key content...
# -----END OPENSSH PRIVATE KEY-----

# Paste this as the value for VPS_SSH_PRIVATE_KEY in GitHub
```

---

## 🚀 Workflow Triggers

### Deploy to Production (`deploy.yml`)

**Triggers:**
- Push to `main` branch
- Manual trigger via GitHub Actions UI

**What it does:**
1. ✅ Builds .NET backend
2. ✅ Runs backend tests
3. ✅ Builds Angular frontend
4. ✅ Deploys backend to VPS
5. ✅ Runs database migrations
6. ✅ Deploys frontend to VPS
7. ✅ Performs health checks
8. ✅ Creates automatic backups
9. ✅ Cleans up old backups

### Run Tests (`test.yml`)

**Triggers:**
- Pull requests to `main` or `development`
- Push to `development` branch

**What it does:**
1. ✅ Runs backend unit tests
2. ✅ Runs frontend unit tests
3. ✅ Lints code
4. ✅ Generates coverage reports
5. ✅ Code quality checks

### Deploy to Staging (`staging.yml`)

**Triggers:**
- Push to `development` branch
- Manual trigger

**What it does:**
1. ✅ Builds and deploys to staging server
2. ✅ Runs smoke tests
3. ✅ Faster feedback before production

---

## 📝 Usage Examples

### Example 1: Deploy to Production

```bash
# Make changes to your code
git add .
git commit -m "Add new feature"

# Push to main branch (triggers production deployment)
git push origin main

# GitHub Actions will automatically:
# - Build your app
# - Run tests
# - Deploy to Hostinger VPS
# - Perform health checks
```

### Example 2: Create a Pull Request

```bash
# Create a feature branch
git checkout -b feature/new-payment-method

# Make changes
git add .
git commit -m "Add new payment method"

# Push to GitHub
git push origin feature/new-payment-method

# Create PR on GitHub
# Tests will run automatically before merging
```

### Example 3: Manual Deployment

1. Go to **GitHub** → **Actions** tab
2. Click **Deploy to Hostinger VPS** workflow
3. Click **Run workflow** button
4. Select branch and click **Run workflow**

### Example 4: Rollback

If something goes wrong:

1. Go to **GitHub** → **Actions** tab
2. Click **Deploy to Hostinger VPS** workflow
3. Click **Run workflow**
4. The rollback job will restore the previous deployment

Or manually on VPS:

```bash
# SSH into VPS
ssh root@your-vps-ip

# Find latest backup
ls -lt /var/www/baytaek/ | grep backup

# Stop service
sudo systemctl stop baytaek-api

# Restore backup
sudo rm -rf /var/www/baytaek/backend
sudo cp -r /var/www/baytaek/backend.backup.YYYYMMDD_HHMMSS /var/www/baytaek/backend

# Start service
sudo systemctl start baytaek-api
```

---

## 🔍 Monitoring Deployments

### View Deployment Logs

1. Go to **GitHub** → **Actions** tab
2. Click on a workflow run
3. Click on a job to see detailed logs
4. Expand steps to see specific commands

### Check Deployment Status

```bash
# SSH into VPS
ssh root@your-vps-ip

# Check backend service status
sudo systemctl status baytaek-api

# View recent logs
sudo journalctl -u baytaek-api -n 50

# Check Nginx status
sudo systemctl status nginx

# View Nginx logs
sudo tail -50 /var/log/nginx/error.log
```

---

## 🛠️ Customization

### Add Environment-Specific Configurations

**Backend** - Create `appsettings.Staging.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=staging-db;Database=baytaek_staging;..."
  },
  "JwtSettings": {
    "Secret": "staging-secret-key",
    "Issuer": "https://staging.yourdomain.com"
  }
}
```

**Frontend** - Create `environment.staging.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://staging.yourdomain.com/api',
  stripePublishableKey: 'pk_test_...'
};
```

### Add Slack/Discord Notifications

Add to `deploy.yml` after the health check step:

```yaml
- name: Send Slack notification
  if: always()
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    text: 'Deployment to production ${{ job.status }}'
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

### Add Database Backup Before Deployment

Add this step before "Deploy Backend to VPS":

```yaml
- name: Backup Database
  run: |
    ssh ${{ secrets.VPS_USER }}@${{ secrets.VPS_HOST }} "
      pg_dump -U baytaek_user baytaek_db | gzip > /var/backups/baytaek/pre_deploy_\$(date +%Y%m%d_%H%M%S).sql.gz
    "
```

---

## 🧪 Testing CI/CD Pipeline

### Test Without Deploying

Modify workflow to skip deployment:

```yaml
deploy:
  name: Deploy to Hostinger VPS
  needs: [build-backend, build-frontend]
  runs-on: ubuntu-latest
  if: false  # Temporarily disable deployment
```

### Dry Run Deployment

Add `--dry-run` flag to rsync:

```yaml
- name: Deploy Backend to VPS (Dry Run)
  run: |
    rsync -avz --dry-run --delete ./backend-build/ ${{ secrets.VPS_USER }}@${{ secrets.VPS_HOST }}:/var/www/baytaek/backend/
```

---

## 🔒 Security Best Practices

1. **Never commit secrets** to Git
   - Use GitHub Secrets for sensitive data
   - Add `.env` files to `.gitignore`

2. **Use separate SSH keys** for CI/CD
   - Don't use your personal SSH keys
   - Generate dedicated keys for GitHub Actions

3. **Limit SSH key permissions**
   ```bash
   # On VPS, restrict what the deploy key can do
   # Edit /home/deploy/.ssh/authorized_keys:
   command="rsync --server --daemon .",no-port-forwarding,no-X11-forwarding,no-agent-forwarding ssh-ed25519 AAAA...
   ```

4. **Use environment-specific secrets**
   - Different secrets for staging vs production
   - Rotate secrets regularly

5. **Enable branch protection**
   - GitHub → Settings → Branches → Add rule
   - Require pull request reviews
   - Require status checks to pass

---

## 📊 Monitoring & Alerts

### Set Up Health Check Monitoring

**Option 1: UptimeRobot (Free)**
1. Sign up at https://uptimerobot.com
2. Add monitor for: `https://yourdomain.com/api/health`
3. Set alert interval to 5 minutes
4. Add email/SMS notifications

**Option 2: GitHub Actions Scheduled Check**

Create `.github/workflows/health-check.yml`:

```yaml
name: Health Check

on:
  schedule:
    - cron: '*/15 * * * *'  # Every 15 minutes

jobs:
  health-check:
    runs-on: ubuntu-latest
    steps:
      - name: Check API Health
        run: |
          RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" https://yourdomain.com/api/health)
          if [ $RESPONSE -ne 200 ]; then
            echo "API is down! Status: $RESPONSE"
            exit 1
          fi
```

---

## 🐛 Troubleshooting

### Issue: SSH Connection Fails

```bash
# Test SSH connection manually
ssh -i ~/.ssh/github_actions_baytaek root@your-vps-ip

# Check SSH key permissions
chmod 600 ~/.ssh/github_actions_baytaek

# Verify public key is in authorized_keys
cat ~/.ssh/authorized_keys | grep github-actions
```

### Issue: Deployment Fails

Check GitHub Actions logs:
1. Look for specific error messages
2. Check "Deploy Backend" or "Deploy Frontend" steps
3. Common issues:
   - SSH key problems
   - Permissions issues
   - Disk space full
   - Service restart failed

### Issue: Health Check Fails

```bash
# SSH into VPS
ssh root@your-vps-ip

# Check if backend is running
curl http://localhost:5000/api/health

# Check service logs
sudo journalctl -u baytaek-api -n 100

# Restart service if needed
sudo systemctl restart baytaek-api
```

### Issue: Frontend Not Updating

```bash
# Clear Nginx cache
sudo systemctl reload nginx

# Check if files were updated
ls -la /var/www/baytaek/frontend/dist/frontend/browser/

# Hard refresh browser (Ctrl + F5)
```

---

## 📈 Performance Optimization

### Enable Caching

Add to workflow:

```yaml
- name: Cache .NET packages
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}

- name: Cache npm packages
  uses: actions/cache@v3
  with:
    path: ~/.npm
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
```

### Parallel Deployments

Deploy backend and frontend in parallel:

```yaml
- name: Deploy Backend & Frontend
  run: |
    # Deploy both simultaneously
    rsync ... backend &
    rsync ... frontend &
    wait
```

---

## 📋 Checklist

Before going live with CI/CD:

- [ ] SSH keys generated and configured
- [ ] GitHub secrets added
- [ ] VPS prepared (following DEPLOYMENT_GUIDE_HOSTINGER.md)
- [ ] Systemd service created
- [ ] Nginx configured
- [ ] SSL certificate installed
- [ ] Database migrations work
- [ ] Health check endpoint works
- [ ] Test deployment to staging first
- [ ] Backup strategy in place
- [ ] Monitoring set up
- [ ] Team trained on rollback procedures

---

## 🎯 Next Steps

1. **Set up staging environment** (recommended)
   - Clone your VPS setup for staging
   - Use `development` branch for staging deploys
   - Test features before production

2. **Add automated tests**
   - Write unit tests for backend
   - Add integration tests
   - Frontend component tests

3. **Implement blue-green deployment**
   - Zero-downtime deployments
   - Instant rollback capability

4. **Set up monitoring dashboard**
   - Grafana + Prometheus
   - Application insights
   - Error tracking (Sentry)

---

## 📞 Support

**GitHub Actions Documentation:** https://docs.github.com/en/actions
**GitHub Actions Marketplace:** https://github.com/marketplace?type=actions

**Common Commands:**

```bash
# Test deployment locally
act -j deploy  # Requires 'act' tool

# View workflow runs
gh run list    # Requires GitHub CLI

# Cancel a running workflow
gh run cancel <run-id>

# Re-run a failed workflow
gh run rerun <run-id>
```

---

## ✨ Summary

Your CI/CD pipeline is now set up to:

✅ **Automatically test** code on every pull request
✅ **Automatically deploy** to production on merge to main
✅ **Create backups** before each deployment
✅ **Run health checks** after deployment
✅ **Support rollback** if issues occur
✅ **Keep deployment history** in GitHub Actions

**Happy deploying! 🚀**
