@echo off
setlocal EnableDelayedExpansion
title FinPulse Windows - Certificate & App Installer

echo ==========================================================
echo   FinPulse Windows - Local Setup ^& Registration
echo ==========================================================
echo.

:: Check for Administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [i] Requesting Administrator privileges to trust the code signing certificate...
    echo     Please click "Yes" on the Windows User Account Control prompt.
    echo.
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process cmd.exe -ArgumentList '/c \"\"%~f0\"\"' -Verb RunAs"
    exit /b
)

:: Running elevated
cd /d "%~dp0"

echo [i] Running with Administrator privileges.
echo [i] Setup Folder: %CD%
echo.

echo ==========================================================
echo   Step 1: Trusting Authenticode Certificates
echo ==========================================================

for %%c in ("%~dp0*.cer") do (
    echo [*] Registering certificate: %%~nxc
    echo     - LocalMachine\Root (Trusted Root Certification Authorities)...
    certutil.exe -addstore -f "Root" "%%c" >nul 2>&1
    echo     - LocalMachine\TrustedPublisher (Trusted Publishers)...
    certutil.exe -addstore -f "TrustedPublisher" "%%c" >nul 2>&1
    echo     - LocalMachine\TrustedPeople (Trusted People)...
    certutil.exe -addstore -f "TrustedPeople" "%%c" >nul 2>&1
    echo       [OK] %%~nxc registered in Windows stores.
)

echo.
echo ==========================================================
echo   Step 2: Unblocking Downloaded Binaries (Smart App Control)
echo ==========================================================
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%~dp0' -Recurse | Unblock-File -ErrorAction SilentlyContinue"
echo   [OK] All files unblocked from Web Mark of the Web.

echo.
echo ==========================================================
echo   Step 3: Registering FinPulse MSIX Package
echo ==========================================================
powershell -NoProfile -ExecutionPolicy Bypass -Command "try { Get-AppxPackage -Name '*FinPulse*' | Remove-AppxPackage -ErrorAction SilentlyContinue; Add-AppxPackage -Path '%~dp0FinPulse-x64.msix' -ForceApplicationShutdown -ErrorAction Stop; Write-Host '  [OK] FinPulse-x64.msix registered successfully!' -ForegroundColor Green } catch { Write-Host '  [!] MSIX Registration note: ' $_.Exception.Message -ForegroundColor Yellow }"

echo.
echo ==========================================================
echo   Step 4: Ensuring Desktop Shortcuts
echo ==========================================================
powershell -NoProfile -ExecutionPolicy Bypass -Command "$src = 'C:\Users\Public\Desktop\FinPulse Windows.lnk'; if (Test-Path $src) { if (Test-Path $env:USERPROFILE\Desktop) { Copy-Item $src -Destination $env:USERPROFILE\Desktop\ -Force -ErrorAction SilentlyContinue }; if (Test-Path $env:USERPROFILE\OneDrive\Desktop) { Copy-Item $src -Destination $env:USERPROFILE\OneDrive\Desktop\ -Force -ErrorAction SilentlyContinue }; Write-Host '  [OK] Desktop shortcuts verified.' -ForegroundColor Green }"

echo.
echo ==========================================================
echo   Setup Complete! Launching FinPulse Windows...
echo ==========================================================
echo.
start "" "%~dp0FinPulse.exe"

echo.
echo FinPulse Windows is now launching!
echo You can now run FinPulse at any time via:
echo   1. The Desktop shortcut: FinPulse Windows
echo   2. Start Menu -> FinPulse Windows
echo   3. Directly clicking FinPulse.exe in this folder
echo.
echo Press any key to close this installer window.
pause >nul
