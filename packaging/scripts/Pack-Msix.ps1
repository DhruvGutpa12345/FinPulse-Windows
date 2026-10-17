$ErrorActionPreference = "Stop"

$makeappx = "C:\Users\dg321\.nuget\packages\microsoft.windows.sdk.buildtools\10.0.26100.1742\bin\10.0.26100.0\x64\makeappx.exe"
$signtool = "C:\Users\dg321\.nuget\packages\microsoft.windows.sdk.buildtools\10.0.26100.1742\bin\10.0.26100.0\x64\signtool.exe"
$pfx = "packaging\signing\FinPulseDevCert.pfx"
$pfxPass = "FinPulsePass123!"

if (Test-Path "msix_staging") {
    Remove-Item -Path "msix_staging" -Recurse -Force
}
New-Item -ItemType Directory -Path "msix_staging" -Force | Out-Null

Write-Host "Copying published binaries to staging..."
Copy-Item -Path "publish\win-x64\*" -Destination "msix_staging" -Recurse -Force
Copy-Item -Path "packaging\msix\AppxManifest.xml" -Destination "msix_staging\AppxManifest.xml" -Force
Copy-Item -Path "src\FinPulse\Assets\*" -Destination "msix_staging\Assets" -Recurse -Force

Write-Host "Stripping debug symbol (.pdb) files from package payload..."
Get-ChildItem -Path "msix_staging" -Filter "*.pdb" -Recurse | Remove-Item -Force

if (-not (Test-Path "dist")) {
    New-Item -ItemType Directory -Path "dist" -Force | Out-Null
}

Write-Host "Packing MSIX package..."
& $makeappx pack /d "msix_staging" /p "dist\FinPulse-x64.msix" /o /v

Write-Host "Signing MSIX package..."
& $signtool sign /f $pfx /p $pfxPass /tr http://timestamp.digicert.com /td sha256 /fd sha256 /v "dist\FinPulse-x64.msix"

Write-Host "MSIX packaging and signing completed successfully!"
