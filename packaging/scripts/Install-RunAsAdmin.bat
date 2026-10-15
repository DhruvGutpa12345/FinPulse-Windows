@echo off
setlocal EnableDelayedExpansion
title FinPulse Windows - Certificate ^& MSIX Installer

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
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:: Running elevated
cd /d "%~dp0"

echo [i] Running with Administrator privileges.
echo [i] Setup Folder: %CD%
echo.

echo ==========================================================
echo   Step 1: Trusting Authenticode Certificates (certutil)
echo ==========================================================

for %%c in ("%~dp0*.cer") do (
    echo [*] Registering certificate: %%~nxc
    echo     - LocalMachine\Root (Trusted Root Certification Authorities)...
    certutil.exe -f -addstore Root "%%c"
    echo     - LocalMachine\TrustedPublisher (Trusted Publishers)...
    certutil.exe -f -addstore TrustedPublisher "%%c"
    echo     - LocalMachine\TrustedPeople (Trusted People)...
    certutil.exe -f -addstore TrustedPeople "%%c"
    echo       [OK] %%~nxc registered via certutil.
    echo.
)

echo ==========================================================
echo   Step 2: Trusting Authenticode Certificates (PowerShell)
echo ==========================================================
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%~dp0*.cer' | ForEach-Object { Import-Certificate -FilePath $_.FullName -CertStoreLocation 'Cert:\LocalMachine\Root' | Out-Null; Import-Certificate -FilePath $_.FullName -CertStoreLocation 'Cert:\LocalMachine\TrustedPublisher' | Out-Null; Import-Certificate -FilePath $_.FullName -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' | Out-Null; Write-Host '  [OK] ' $_.Name ' verified in LocalMachine stores.' -ForegroundColor Green }"

echo.
echo ==========================================================
echo   Step 3: Unblocking Downloaded Binaries
echo ==========================================================
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-ChildItem -Path '%~dp0' -Recurse | Unblock-File -ErrorAction SilentlyContinue; Write-Host '  [OK] All files unblocked from Web Mark of the Web.' -ForegroundColor Green"

echo.
echo ==========================================================
echo   Step 4: Registering FinPulse MSIX Package
echo ==========================================================
powershell -NoProfile -ExecutionPolicy Bypass -Command "try { Get-AppxPackage -Name '*FinPulse*' | Remove-AppxPackage -ErrorAction SilentlyContinue; Add-AppxPackage -Path '%~dp0FinPulse-x64.msix' -ForceApplicationShutdown -ErrorAction Stop; Write-Host '  [OK] FinPulse-x64.msix registered successfully!' -ForegroundColor Green } catch { Write-Host '  [!] MSIX Registration note: ' $_.Exception.Message -ForegroundColor Yellow }"

echo.
echo ==========================================================
echo   Step 5: Verifying Desktop Shortcuts
echo ==========================================================
powershell -NoProfile -ExecutionPolicy Bypass -Command "$src = 'C:\Users\Public\Desktop\FinPulse Windows.lnk'; if (Test-Path $src) { if (Test-Path $env:USERPROFILE\Desktop) { Copy-Item $src -Destination $env:USERPROFILE\Desktop\ -Force -ErrorAction SilentlyContinue }; if (Test-Path $env:USERPROFILE\OneDrive\Desktop) { Copy-Item $src -Destination $env:USERPROFILE\OneDrive\Desktop\ -Force -ErrorAction SilentlyContinue }; Write-Host '  [OK] Desktop shortcuts verified.' -ForegroundColor Green }"

echo.
echo ==========================================================
echo   Setup Complete!
echo ==========================================================
echo.
echo The certificate is now trusted by Windows.
echo You can now:
echo   1. Double-click FinPulse-x64.msix at any time to install or update.
echo   2. Run FinPulse from your Desktop shortcut or Start Menu.
echo   3. Run FinPulse.exe directly in this folder.
echo.
echo Press any key to close this window.
pause >nul
