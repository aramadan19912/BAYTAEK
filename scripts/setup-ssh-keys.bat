@echo off
REM ################################################################################
REM SSH Keys Setup Script for BAYTAEK CI/CD (Windows)
REM This script generates SSH keys and helps configure them for GitHub Actions
REM ################################################################################

setlocal enabledelayedexpansion

REM Configuration
set KEY_NAME=github_actions_baytaek
set KEY_PATH=%USERPROFILE%\.ssh\%KEY_NAME%
set KEY_COMMENT=github-actions@baytaek-deploy

title BAYTAEK CI/CD - SSH Keys Setup

echo ================================================================
echo    BAYTAEK CI/CD - SSH Keys Setup Script (Windows)
echo ================================================================
echo.

REM Check if ssh-keygen is available
where ssh-keygen >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo ERROR: ssh-keygen not found!
    echo.
    echo Please install OpenSSH or Git for Windows:
    echo - OpenSSH: Settings ^> Apps ^> Optional Features ^> OpenSSH Client
    echo - Git for Windows: https://git-scm.com/download/win
    echo.
    pause
    exit /b 1
)

:MENU
cls
echo ================================================================
echo    BAYTAEK SSH Keys Setup - Main Menu
echo ================================================================
echo.
echo 1. Generate new SSH key pair
echo 2. Display public key
echo 3. Display private key
echo 4. Show GitHub Secrets instructions
echo 5. Save keys to backup files
echo 6. Open SSH folder in Explorer
echo 7. Complete setup (recommended)
echo 0. Exit
echo.
set /p choice="Enter your choice (0-7): "

if "%choice%"=="1" goto GENERATE_KEY
if "%choice%"=="2" goto SHOW_PUBLIC
if "%choice%"=="3" goto SHOW_PRIVATE
if "%choice%"=="4" goto GITHUB_INSTRUCTIONS
if "%choice%"=="5" goto SAVE_KEYS
if "%choice%"=="6" goto OPEN_FOLDER
if "%choice%"=="7" goto COMPLETE_SETUP
if "%choice%"=="0" goto EXIT
goto MENU

:GENERATE_KEY
cls
echo ================================================================
echo    Step 1: Generating SSH Key Pair
echo ================================================================
echo.

REM Create .ssh directory if it doesn't exist
if not exist "%USERPROFILE%\.ssh" (
    mkdir "%USERPROFILE%\.ssh"
    echo Created .ssh directory
)

REM Check if key already exists
if exist "%KEY_PATH%" (
    echo WARNING: SSH key already exists at: %KEY_PATH%
    set /p overwrite="Do you want to overwrite it? (yes/no): "
    if /i not "!overwrite!"=="yes" (
        echo Aborted. Using existing key.
        pause
        goto MENU
    )
    echo Backing up existing key...
    copy "%KEY_PATH%" "%KEY_PATH%.backup.%date:~-4%%date:~3,2%%date:~0,2%_%time:~0,2%%time:~3,2%%time:~6,2%" >nul
    copy "%KEY_PATH%.pub" "%KEY_PATH%.pub.backup.%date:~-4%%date:~3,2%%date:~0,2%_%time:~0,2%%time:~3,2%%time:~6,2%" >nul
)

echo Generating ED25519 key...
ssh-keygen -t ed25519 -C "%KEY_COMMENT%" -f "%KEY_PATH%" -N ""

if %ERRORLEVEL% equ 0 (
    echo.
    echo [SUCCESS] SSH key pair generated successfully!
    echo Location: %KEY_PATH%
) else (
    echo.
    echo [ERROR] Failed to generate SSH key!
)

echo.
pause
goto MENU

:SHOW_PUBLIC
cls
echo ================================================================
echo    Step 2: Your Public Key
echo ================================================================
echo.

if not exist "%KEY_PATH%.pub" (
    echo ERROR: Public key not found. Please generate keys first.
    pause
    goto MENU
)

echo Copy this public key to add to your VPS:
echo ----------------------------------------------------------------
type "%KEY_PATH%.pub"
echo ----------------------------------------------------------------
echo.
echo Instructions for VPS:
echo 1. SSH into your VPS
echo 2. Run: mkdir -p ~/.ssh ^&^& chmod 700 ~/.ssh
echo 3. Run: echo '[PASTE KEY ABOVE]' ^>^> ~/.ssh/authorized_keys
echo 4. Run: chmod 600 ~/.ssh/authorized_keys
echo.

REM Copy to clipboard if clip is available
type "%KEY_PATH%.pub" | clip 2>nul
if %ERRORLEVEL% equ 0 (
    echo [INFO] Public key copied to clipboard!
)

pause
goto MENU

:SHOW_PRIVATE
cls
echo ================================================================
echo    Step 3: Your Private Key (for GitHub Secrets)
echo ================================================================
echo.

if not exist "%KEY_PATH%" (
    echo ERROR: Private key not found. Please generate keys first.
    pause
    goto MENU
)

echo Copy this ENTIRE private key to GitHub Secrets:
echo ----------------------------------------------------------------
type "%KEY_PATH%"
echo ----------------------------------------------------------------
echo.
echo IMPORTANT:
echo - Include the BEGIN and END lines
echo - Add to GitHub as VPS_SSH_PRIVATE_KEY
echo - Never share this key or commit it to Git
echo.

REM Copy to clipboard
type "%KEY_PATH%" | clip 2>nul
if %ERRORLEVEL% equ 0 (
    echo [INFO] Private key copied to clipboard!
)

pause
goto MENU

:GITHUB_INSTRUCTIONS
cls
echo ================================================================
echo    Step 4: Add Secrets to GitHub
echo ================================================================
echo.
echo 1. Go to your GitHub repository
echo 2. Navigate to: Settings ^> Secrets and variables ^> Actions
echo 3. Click "New repository secret"
echo.
echo Add these 3 secrets:
echo ----------------------------------------------------------------
echo.
echo Secret Name: VPS_HOST
echo Value: [Your VPS IP or domain, e.g., 123.45.67.89]
echo.
echo Secret Name: VPS_USER
echo Value: [SSH user, e.g., 'root' or 'deploy']
echo.
echo Secret Name: VPS_SSH_PRIVATE_KEY
echo Value: [Copy from the private key - use option 3]
echo        Include BEGIN and END lines!
echo ----------------------------------------------------------------
echo.
pause
goto MENU

:SAVE_KEYS
cls
echo ================================================================
echo    Step 5: Save Keys to Backup Files
echo ================================================================
echo.

set OUTPUT_DIR=%~dp0..\ssh-keys-backup
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

REM Save public key
copy "%KEY_PATH%.pub" "%OUTPUT_DIR%\public_key.txt" >nul
echo [SUCCESS] Public key saved to: %OUTPUT_DIR%\public_key.txt

REM Save private key
copy "%KEY_PATH%" "%OUTPUT_DIR%\private_key.txt" >nul
echo [SUCCESS] Private key saved to: %OUTPUT_DIR%\private_key.txt

REM Create instructions file
(
echo BAYTAEK CI/CD SSH Keys Setup
echo ============================
echo.
echo Generated on: %date% %time%
echo.
echo Files in this directory:
echo - public_key.txt: Add this to your VPS ~/.ssh/authorized_keys
echo - private_key.txt: Add this to GitHub Secrets as VPS_SSH_PRIVATE_KEY
echo.
echo GitHub Secrets Setup:
echo 1. Go to: GitHub Repository -^> Settings -^> Secrets and variables -^> Actions
echo 2. Click "New repository secret"
echo 3. Add these secrets:
echo.
echo    VPS_HOST: [Your VPS IP or domain]
echo    VPS_USER: [SSH user, e.g., 'root']
echo    VPS_SSH_PRIVATE_KEY: [Content of private_key.txt]
echo.
echo VPS Setup:
echo 1. SSH into your VPS
echo 2. Run: mkdir -p ~/.ssh ^&^& chmod 700 ~/.ssh
echo 3. Run: echo '[content of public_key.txt]' ^>^> ~/.ssh/authorized_keys
echo 4. Run: chmod 600 ~/.ssh/authorized_keys
echo.
echo Test Connection:
echo ssh -i %KEY_PATH% [user]@[vps-ip]
echo.
echo IMPORTANT:
echo - Keep private_key.txt secure and never commit it to Git
echo - This directory is in .gitignore
echo - After setup, you can delete this backup directory
) > "%OUTPUT_DIR%\INSTRUCTIONS.txt"

echo [SUCCESS] Instructions saved to: %OUTPUT_DIR%\INSTRUCTIONS.txt
echo.
echo WARNING: Keep these files secure! They contain sensitive keys.
echo.
pause
goto MENU

:OPEN_FOLDER
cls
echo Opening SSH folder...
explorer "%USERPROFILE%\.ssh"
goto MENU

:COMPLETE_SETUP
cls
echo ================================================================
echo    Complete Setup - All Steps
echo ================================================================
echo.
echo This will:
echo 1. Generate SSH key pair
echo 2. Display public key
echo 3. Display private key
echo 4. Save keys to backup files
echo 5. Show GitHub instructions
echo.
set /p confirm="Continue? (yes/no): "
if /i not "%confirm%"=="yes" goto MENU

REM Step 1: Generate key
call :GENERATE_KEY_SILENT

REM Step 2: Save keys
call :SAVE_KEYS_SILENT

REM Step 3: Show results
cls
echo ================================================================
echo    Setup Complete!
echo ================================================================
echo.
echo Next steps:
echo.
echo 1. [VPS] Add public key to your VPS
type "%KEY_PATH%.pub"
echo.
echo 2. [GITHUB] Add these secrets to GitHub:
echo    - VPS_HOST: your-vps-ip
echo    - VPS_USER: root
echo    - VPS_SSH_PRIVATE_KEY: [content from %OUTPUT_DIR%\private_key.txt]
echo.
echo 3. [TEST] Test SSH connection from your machine:
echo    ssh -i %KEY_PATH% root@your-vps-ip
echo.
echo 4. [DEPLOY] Push to main branch to trigger deployment!
echo.
echo Files saved to: %OUTPUT_DIR%
echo.
pause
goto MENU

:GENERATE_KEY_SILENT
if not exist "%USERPROFILE%\.ssh" mkdir "%USERPROFILE%\.ssh"
if exist "%KEY_PATH%" (
    copy "%KEY_PATH%" "%KEY_PATH%.backup" >nul 2>nul
)
ssh-keygen -t ed25519 -C "%KEY_COMMENT%" -f "%KEY_PATH%" -N "" >nul 2>nul
goto :eof

:SAVE_KEYS_SILENT
set OUTPUT_DIR=%~dp0..\ssh-keys-backup
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
copy "%KEY_PATH%.pub" "%OUTPUT_DIR%\public_key.txt" >nul
copy "%KEY_PATH%" "%OUTPUT_DIR%\private_key.txt" >nul
goto :eof

:EXIT
cls
echo.
echo Thanks for using BAYTAEK SSH Setup!
echo.
timeout /t 2 >nul
exit /b 0
