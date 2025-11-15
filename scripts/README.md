# BAYTAEK Scripts Directory

This directory contains helpful scripts for deploying and managing your BAYTAEK application.

---

## 📁 Available Scripts

### 1. SSH Keys Setup Scripts

**For Linux/Mac:** `setup-ssh-keys.sh`
**For Windows:** `setup-ssh-keys.bat`

These scripts help you generate and configure SSH keys for GitHub Actions CI/CD.

---

## 🚀 Quick Start

### Windows Users:

```cmd
# Run the batch script
cd scripts
setup-ssh-keys.bat
```

Then select **Option 7** (Complete setup) from the menu.

### Linux/Mac Users:

```bash
# Make script executable
chmod +x scripts/setup-ssh-keys.sh

# Run the script
cd scripts
./setup-ssh-keys.sh
```

Then select **Option 9** (Complete setup) from the menu.

---

## 📋 What the Scripts Do

### Automated Steps:

1. ✅ **Generate SSH Key Pair**
   - Creates ED25519 key (modern and secure)
   - Stores in `~/.ssh/github_actions_baytaek`
   - No passphrase (required for automation)

2. ✅ **Display Keys**
   - Shows public key (for VPS)
   - Shows private key (for GitHub Secrets)
   - Copies to clipboard automatically

3. ✅ **Save Backup Files**
   - Creates `ssh-keys-backup/` directory
   - Saves `public_key.txt`
   - Saves `private_key.txt`
   - Creates `INSTRUCTIONS.txt` with steps

4. ✅ **Add to VPS** (Linux/Mac only)
   - Automatically copies public key to VPS
   - Tests connection
   - Sets correct permissions

5. ✅ **GitHub Instructions**
   - Shows exactly which secrets to add
   - Provides copy-paste ready values

---

## 🔧 Manual Setup (Alternative)

If you prefer to do it manually:

### Step 1: Generate Key

**Windows (PowerShell):**
```powershell
ssh-keygen -t ed25519 -C "github-actions@baytaek" -f $HOME\.ssh\github_actions_baytaek -N '""'
```

**Linux/Mac:**
```bash
ssh-keygen -t ed25519 -C "github-actions@baytaek" -f ~/.ssh/github_actions_baytaek -N ""
```

### Step 2: Copy Public Key to VPS

```bash
ssh-copy-id -i ~/.ssh/github_actions_baytaek.pub root@your-vps-ip
```

Or manually:
```bash
cat ~/.ssh/github_actions_baytaek.pub | ssh root@your-vps-ip "mkdir -p ~/.ssh && cat >> ~/.ssh/authorized_keys"
```

### Step 3: Add to GitHub

1. Go to: **GitHub** → **Settings** → **Secrets and variables** → **Actions**
2. Add three secrets:

   **VPS_HOST:**
   ```
   your-vps-ip-or-domain
   ```

   **VPS_USER:**
   ```
   root
   ```

   **VPS_SSH_PRIVATE_KEY:**
   ```
   [Copy entire content of ~/.ssh/github_actions_baytaek]
   ```

### Step 4: Test Connection

```bash
ssh -i ~/.ssh/github_actions_baytaek root@your-vps-ip
```

---

## 📊 Script Menu Options

### setup-ssh-keys.sh (Linux/Mac)

```
1) Generate new SSH key pair
2) Display public key
3) Display private key
4) Add public key to VPS (automatic)
5) Show GitHub Secrets instructions
6) Save keys to backup files
7) Test SSH connection
8) Create deploy user script
9) Complete setup (all steps) ⭐ RECOMMENDED
0) Exit
```

### setup-ssh-keys.bat (Windows)

```
1. Generate new SSH key pair
2. Display public key
3. Display private key
4. Show GitHub Secrets instructions
5. Save keys to backup files
6. Open SSH folder in Explorer
7. Complete setup (recommended) ⭐ RECOMMENDED
0. Exit
```

---

## 🔒 Security Notes

### ⚠️ Important:

1. **Never commit private keys to Git**
   - The `ssh-keys-backup/` directory is in `.gitignore`
   - Private keys should only be in GitHub Secrets and `~/.ssh/`

2. **Keep private keys secure**
   - Don't share private keys via email or chat
   - Don't upload to public services
   - Store backup in secure password manager

3. **Use dedicated keys**
   - Don't use your personal SSH keys for CI/CD
   - Generate separate keys for different environments

4. **Rotate keys regularly**
   - Generate new keys every 6-12 months
   - Update GitHub Secrets when rotating

---

## 🐛 Troubleshooting

### "ssh-keygen not found" (Windows)

**Solution:**
1. Install OpenSSH Client:
   - Settings → Apps → Optional Features
   - Add "OpenSSH Client"

2. Or install Git for Windows:
   - Download from https://git-scm.com/download/win

### "Permission denied (publickey)" when testing

**Solutions:**
1. Verify public key is on VPS:
   ```bash
   ssh root@vps-ip "cat ~/.ssh/authorized_keys"
   ```

2. Check VPS SSH config allows key auth:
   ```bash
   ssh root@vps-ip "grep PubkeyAuthentication /etc/ssh/sshd_config"
   ```
   Should show: `PubkeyAuthentication yes`

3. Check file permissions on VPS:
   ```bash
   ssh root@vps-ip "ls -la ~/.ssh/"
   ```
   - `.ssh/` should be `700`
   - `authorized_keys` should be `600`

### Keys generated but can't find them

**Location:**
- **Windows:** `C:\Users\YourUsername\.ssh\github_actions_baytaek`
- **Linux/Mac:** `~/.ssh/github_actions_baytaek`

**View with:**
- Windows: `type %USERPROFILE%\.ssh\github_actions_baytaek.pub`
- Linux/Mac: `cat ~/.ssh/github_actions_baytaek.pub`

---

## 📁 Output Files

After running the script, you'll have:

```
ssh-keys-backup/
├── public_key.txt          # Add to VPS ~/.ssh/authorized_keys
├── private_key.txt         # Add to GitHub Secrets
└── INSTRUCTIONS.txt        # Step-by-step guide
```

**Note:** This directory is gitignored and safe to keep locally, but delete after setup if preferred.

---

## 🎯 Next Steps After Setup

1. ✅ **Test the pipeline**
   ```bash
   # Make a small change
   git add .
   git commit -m "Test CI/CD pipeline"
   git push origin main
   ```

2. ✅ **Monitor deployment**
   - Go to GitHub → Actions tab
   - Watch the workflow run live

3. ✅ **Verify deployment**
   ```bash
   # Check if site is live
   curl https://yourdomain.com/api/health
   ```

4. ✅ **Set up monitoring**
   - UptimeRobot: https://uptimerobot.com
   - Or use GitHub Actions scheduled checks

---

## 📞 Support

**Documentation:**
- Main guide: `../CI_CD_SETUP_GUIDE.md`
- Deployment guide: `../DEPLOYMENT_GUIDE_HOSTINGER.md`

**Common Issues:**
- SSH connection problems → Check firewall on VPS
- GitHub Actions failing → Check secrets are correct
- Deployment not working → Check VPS service status

**Test Connection:**
```bash
# From your machine
ssh -i ~/.ssh/github_actions_baytaek root@your-vps-ip "echo 'Connection works!'"
```

---

## ✨ Tips

1. **Use VS Code Remote SSH** to edit files on VPS directly
2. **Create a staging environment** for safer deployments
3. **Set up Slack/Discord** notifications for deployments
4. **Enable branch protection** on GitHub to require PR reviews
5. **Keep backups** of your SSH keys in a password manager

---

**Generated:** 2025-11-15
**Version:** 1.0
**Status:** Production Ready ✅
