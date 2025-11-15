#!/bin/bash

################################################################################
# SSH Keys Setup Script for BAYTAEK CI/CD
# This script generates SSH keys and helps configure them for GitHub Actions
################################################################################

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
KEY_NAME="github_actions_baytaek"
KEY_PATH="$HOME/.ssh/$KEY_NAME"
KEY_COMMENT="github-actions@baytaek-deploy"

echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║   BAYTAEK CI/CD - SSH Keys Setup Script                   ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

################################################################################
# Function: Generate SSH Key
################################################################################
generate_ssh_key() {
    echo -e "${YELLOW}Step 1: Generating SSH Key Pair...${NC}"
    echo ""

    # Check if key already exists
    if [ -f "$KEY_PATH" ]; then
        echo -e "${YELLOW}⚠️  SSH key already exists at: $KEY_PATH${NC}"
        read -p "Do you want to overwrite it? (yes/no): " overwrite
        if [ "$overwrite" != "yes" ]; then
            echo -e "${RED}Aborted. Using existing key.${NC}"
            return
        fi
        echo "Backing up existing key..."
        mv "$KEY_PATH" "$KEY_PATH.backup.$(date +%Y%m%d_%H%M%S)"
        mv "$KEY_PATH.pub" "$KEY_PATH.pub.backup.$(date +%Y%m%d_%H%M%S)"
    fi

    # Create .ssh directory if it doesn't exist
    mkdir -p "$HOME/.ssh"
    chmod 700 "$HOME/.ssh"

    # Generate key (ED25519 is modern and secure)
    echo "Generating ED25519 key..."
    ssh-keygen -t ed25519 -C "$KEY_COMMENT" -f "$KEY_PATH" -N ""

    # Set proper permissions
    chmod 600 "$KEY_PATH"
    chmod 644 "$KEY_PATH.pub"

    echo -e "${GREEN}✅ SSH key pair generated successfully!${NC}"
    echo ""
}

################################################################################
# Function: Display Public Key
################################################################################
display_public_key() {
    echo -e "${YELLOW}Step 2: Your Public Key${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
    cat "$KEY_PATH.pub"
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "${GREEN}📋 Copy this public key to add to your VPS${NC}"
    echo ""
}

################################################################################
# Function: Display Private Key
################################################################################
display_private_key() {
    echo -e "${YELLOW}Step 3: Your Private Key (for GitHub Secrets)${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
    cat "$KEY_PATH"
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "${GREEN}📋 Copy this ENTIRE private key (including BEGIN and END lines)${NC}"
    echo -e "${GREEN}   to add to GitHub Secrets as VPS_SSH_PRIVATE_KEY${NC}"
    echo ""
}

################################################################################
# Function: Add Public Key to VPS
################################################################################
add_to_vps() {
    echo -e "${YELLOW}Step 4: Add Public Key to VPS${NC}"
    echo ""

    read -p "Enter your VPS IP address or hostname: " vps_host
    read -p "Enter SSH user (usually 'root' or 'deploy'): " vps_user

    echo ""
    echo -e "${BLUE}Attempting to add public key to VPS...${NC}"
    echo ""

    # Copy public key to VPS
    if ssh-copy-id -i "$KEY_PATH.pub" "$vps_user@$vps_host"; then
        echo -e "${GREEN}✅ Public key successfully added to VPS!${NC}"
        echo ""

        # Test the connection
        echo -e "${BLUE}Testing SSH connection...${NC}"
        if ssh -i "$KEY_PATH" -o "StrictHostKeyChecking=no" "$vps_user@$vps_host" "echo 'Connection successful!'" 2>/dev/null; then
            echo -e "${GREEN}✅ SSH connection test passed!${NC}"
        else
            echo -e "${RED}❌ SSH connection test failed. Please check manually.${NC}"
        fi
    else
        echo -e "${RED}❌ Failed to add key automatically.${NC}"
        echo ""
        echo -e "${YELLOW}Manual steps:${NC}"
        echo "1. SSH into your VPS: ssh $vps_user@$vps_host"
        echo "2. Run: mkdir -p ~/.ssh && chmod 700 ~/.ssh"
        echo "3. Run: echo '$(cat $KEY_PATH.pub)' >> ~/.ssh/authorized_keys"
        echo "4. Run: chmod 600 ~/.ssh/authorized_keys"
    fi
    echo ""
}

################################################################################
# Function: GitHub Secrets Instructions
################################################################################
github_secrets_instructions() {
    echo -e "${YELLOW}Step 5: Add Secrets to GitHub${NC}"
    echo ""
    echo -e "${BLUE}Go to your GitHub repository:${NC}"
    echo "  Settings → Secrets and variables → Actions → New repository secret"
    echo ""
    echo -e "${GREEN}Add these 3 secrets:${NC}"
    echo ""
    echo -e "${BLUE}1. VPS_HOST${NC}"
    echo "   Value: [Your VPS IP or domain]"
    echo ""
    echo -e "${BLUE}2. VPS_USER${NC}"
    echo "   Value: [SSH user, e.g., 'root' or 'deploy']"
    echo ""
    echo -e "${BLUE}3. VPS_SSH_PRIVATE_KEY${NC}"
    echo "   Value: [Copy from the private key output above]"
    echo "   ⚠️  Include the BEGIN and END lines!"
    echo ""
}

################################################################################
# Function: Save Keys to File
################################################################################
save_keys_to_file() {
    echo -e "${YELLOW}Step 6: Save Keys to Files${NC}"
    echo ""

    OUTPUT_DIR="./ssh-keys-backup"
    mkdir -p "$OUTPUT_DIR"

    # Save public key
    cp "$KEY_PATH.pub" "$OUTPUT_DIR/public_key.txt"
    echo -e "${GREEN}✅ Public key saved to: $OUTPUT_DIR/public_key.txt${NC}"

    # Save private key
    cp "$KEY_PATH" "$OUTPUT_DIR/private_key.txt"
    echo -e "${GREEN}✅ Private key saved to: $OUTPUT_DIR/private_key.txt${NC}"

    # Create instructions file
    cat > "$OUTPUT_DIR/INSTRUCTIONS.txt" <<EOF
BAYTAEK CI/CD SSH Keys Setup
============================

Generated on: $(date)

Files in this directory:
- public_key.txt: Add this to your VPS ~/.ssh/authorized_keys
- private_key.txt: Add this to GitHub Secrets as VPS_SSH_PRIVATE_KEY

GitHub Secrets Setup:
1. Go to: GitHub Repository → Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Add these secrets:

   VPS_HOST: [Your VPS IP or domain]
   VPS_USER: [SSH user, e.g., 'root']
   VPS_SSH_PRIVATE_KEY: [Content of private_key.txt]

VPS Setup:
1. SSH into your VPS
2. Run: mkdir -p ~/.ssh && chmod 700 ~/.ssh
3. Run: echo '[content of public_key.txt]' >> ~/.ssh/authorized_keys
4. Run: chmod 600 ~/.ssh/authorized_keys

Test Connection:
ssh -i ~/.ssh/github_actions_baytaek [user]@[vps-ip]

IMPORTANT:
- Keep private_key.txt secure and never commit it to Git
- This directory is already in .gitignore
- After setup, you can delete this backup directory
EOF

    echo -e "${GREEN}✅ Instructions saved to: $OUTPUT_DIR/INSTRUCTIONS.txt${NC}"
    echo ""
    echo -e "${YELLOW}⚠️  IMPORTANT: Keep these files secure! They contain sensitive keys.${NC}"
    echo ""
}

################################################################################
# Function: Update .gitignore
################################################################################
update_gitignore() {
    GITIGNORE="../.gitignore"

    if [ -f "$GITIGNORE" ]; then
        # Check if ssh-keys-backup is already in .gitignore
        if ! grep -q "ssh-keys-backup" "$GITIGNORE"; then
            echo "" >> "$GITIGNORE"
            echo "# SSH keys backup (CI/CD setup)" >> "$GITIGNORE"
            echo "ssh-keys-backup/" >> "$GITIGNORE"
            echo -e "${GREEN}✅ Added ssh-keys-backup/ to .gitignore${NC}"
        else
            echo -e "${GREEN}✅ ssh-keys-backup/ already in .gitignore${NC}"
        fi
    fi
}

################################################################################
# Function: Test SSH Connection
################################################################################
test_connection() {
    echo -e "${YELLOW}Step 7: Test SSH Connection${NC}"
    echo ""

    read -p "Enter VPS host to test: " test_host
    read -p "Enter VPS user to test: " test_user

    echo ""
    echo -e "${BLUE}Testing connection to $test_user@$test_host...${NC}"
    echo ""

    if ssh -i "$KEY_PATH" -o "ConnectTimeout=10" -o "StrictHostKeyChecking=no" "$test_user@$test_host" "echo 'Successfully connected to VPS!'; hostname; uptime"; then
        echo ""
        echo -e "${GREEN}✅ SSH connection successful!${NC}"
        echo -e "${GREEN}   Your CI/CD pipeline should be able to connect.${NC}"
    else
        echo ""
        echo -e "${RED}❌ SSH connection failed!${NC}"
        echo -e "${YELLOW}Troubleshooting:${NC}"
        echo "1. Verify the public key is in ~/.ssh/authorized_keys on the VPS"
        echo "2. Check VPS firewall allows SSH (port 22)"
        echo "3. Ensure SSH service is running on VPS"
        echo "4. Try manual connection: ssh -i $KEY_PATH $test_user@$test_host"
    fi
    echo ""
}

################################################################################
# Function: Create Deploy User (Optional)
################################################################################
create_deploy_user() {
    echo -e "${YELLOW}Optional: Create Dedicated Deploy User${NC}"
    echo ""
    echo "It's recommended to create a dedicated 'deploy' user instead of using root."
    echo ""

    read -p "Do you want to create a deploy user script? (yes/no): " create_user

    if [ "$create_user" = "yes" ]; then
        cat > "./create-deploy-user.sh" <<'EOF'
#!/bin/bash
# Run this script on your VPS as root

echo "Creating deploy user..."

# Create deploy user
useradd -m -s /bin/bash deploy

# Add to www-data group
usermod -aG www-data deploy

# Create .ssh directory
mkdir -p /home/deploy/.ssh
chmod 700 /home/deploy/.ssh

# Add public key (replace with your actual public key)
cat > /home/deploy/.ssh/authorized_keys <<'PUBKEY'
# PASTE YOUR PUBLIC KEY HERE
PUBKEY

chmod 600 /home/deploy/.ssh/authorized_keys
chown -R deploy:deploy /home/deploy/.ssh

# Grant deploy user permissions for application directory
chown -R deploy:www-data /var/www/baytaek
chmod -R 775 /var/www/baytaek

# Allow deploy user to restart services without password
cat > /etc/sudoers.d/deploy <<'SUDOERS'
deploy ALL=(ALL) NOPASSWD: /bin/systemctl start baytaek-api
deploy ALL=(ALL) NOPASSWD: /bin/systemctl stop baytaek-api
deploy ALL=(ALL) NOPASSWD: /bin/systemctl restart baytaek-api
deploy ALL=(ALL) NOPASSWD: /bin/systemctl reload nginx
deploy ALL=(ALL) NOPASSWD: /bin/systemctl status baytaek-api
SUDOERS

chmod 440 /etc/sudoers.d/deploy

echo "Deploy user created successfully!"
echo "Test with: ssh deploy@localhost"
EOF

        chmod +x "./create-deploy-user.sh"
        echo -e "${GREEN}✅ Created create-deploy-user.sh${NC}"
        echo -e "${BLUE}   Upload this to your VPS and run as root:${NC}"
        echo "   scp create-deploy-user.sh root@your-vps:/tmp/"
        echo "   ssh root@your-vps 'bash /tmp/create-deploy-user.sh'"
        echo ""
    fi
}

################################################################################
# Main Menu
################################################################################
main_menu() {
    while true; do
        echo ""
        echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
        echo -e "${GREEN}What would you like to do?${NC}"
        echo -e "${BLUE}═══════════════════════════════════════════════════════════${NC}"
        echo "1) Generate new SSH key pair"
        echo "2) Display public key"
        echo "3) Display private key"
        echo "4) Add public key to VPS (automatic)"
        echo "5) Show GitHub Secrets instructions"
        echo "6) Save keys to backup files"
        echo "7) Test SSH connection"
        echo "8) Create deploy user script"
        echo "9) Complete setup (all steps)"
        echo "0) Exit"
        echo ""
        read -p "Enter your choice (0-9): " choice

        case $choice in
            1) generate_ssh_key ;;
            2) display_public_key ;;
            3) display_private_key ;;
            4) add_to_vps ;;
            5) github_secrets_instructions ;;
            6) save_keys_to_file; update_gitignore ;;
            7) test_connection ;;
            8) create_deploy_user ;;
            9)
                generate_ssh_key
                display_public_key
                display_private_key
                read -p "Press Enter to continue to VPS setup..."
                add_to_vps
                save_keys_to_file
                update_gitignore
                github_secrets_instructions
                test_connection
                create_deploy_user
                echo -e "${GREEN}✅ Complete setup finished!${NC}"
                ;;
            0)
                echo ""
                echo -e "${GREEN}Thanks for using BAYTAEK SSH Setup!${NC}"
                echo ""
                exit 0
                ;;
            *)
                echo -e "${RED}Invalid choice. Please try again.${NC}"
                ;;
        esac
    done
}

################################################################################
# Script Entry Point
################################################################################

# Check if running in scripts directory
if [ ! -f "../CI_CD_SETUP_GUIDE.md" ]; then
    echo -e "${RED}⚠️  Please run this script from the 'scripts' directory${NC}"
    echo "   cd scripts && ./setup-ssh-keys.sh"
    exit 1
fi

# Start main menu
main_menu
