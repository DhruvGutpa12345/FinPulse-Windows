# Elevation script to register certificate, install MSIX, and unblock standalone binaries
$PSScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " FinPulse Windows: Setup & Smart App Control Trust Config" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""

$cerFiles = Get-ChildItem -Path $PSScriptRoot -Filter "*.cer"
foreach ($cer in $cerFiles) {
    Write-Host "[1/4] Trusting certificate: $($cer.Name)..." -ForegroundColor Yellow
    & certutil.exe -addstore -f "Root" $cer.FullName | Out-Null
    & certutil.exe -addstore -f "TrustedPublisher" $cer.FullName | Out-Null
    & certutil.exe -addstore -f "TrustedPeople" $cer.FullName | Out-Null
    Write-Host "      ✓ $($cer.Name) imported into Root, TrustedPublisher, and TrustedPeople." -ForegroundColor Green
}

Write-Host "[2/4] Unblocking all downloaded binaries from Windows Web Marks..." -ForegroundColor Yellow
Get-ChildItem -Path $PSScriptRoot -Recurse | Unblock-File -ErrorAction SilentlyContinue
Write-Host "      ✓ Files unblocked." -ForegroundColor Green

$msixPath = Join-Path $PSScriptRoot "FinPulse-x64.msix"
if (Test-Path $msixPath) {
    Write-Host "[3/4] Registering FinPulse-x64.msix package..." -ForegroundColor Yellow
    try {
        Get-AppxPackage -Name "*FinPulse*" | Remove-AppxPackage -ErrorAction SilentlyContinue
        Add-AppxPackage -Path $msixPath -ForceApplicationShutdown -ErrorAction Stop
        Write-Host "      ✓ FinPulse Windows MSIX successfully installed!" -ForegroundColor Green
    } catch {
        Write-Host "      [i] MSIX Registration message: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "[4/4] Verifying Desktop shortcuts..." -ForegroundColor Yellow
$srcLnk = "C:\Users\Public\Desktop\FinPulse Windows.lnk"
if (Test-Path $srcLnk) {
    if (Test-Path "$env:USERPROFILE\Desktop") { Copy-Item $srcLnk -Destination "$env:USERPROFILE\Desktop\" -Force -ErrorAction SilentlyContinue }
    if (Test-Path "$env:USERPROFILE\OneDrive\Desktop") { Copy-Item $srcLnk -Destination "$env:USERPROFILE\OneDrive\Desktop\" -Force -ErrorAction SilentlyContinue }
    Write-Host "      ✓ Desktop shortcut placed on user desktop." -ForegroundColor Green
}

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host " Setup complete! Starting FinPulse Windows..." -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

$exePath = Join-Path $PSScriptRoot "FinPulse.exe"
if (Test-Path $exePath) {
    Start-Process -FilePath $exePath
}

Write-Host ""
Write-Host "Press enter to exit..." -ForegroundColor Gray
Read-Host
