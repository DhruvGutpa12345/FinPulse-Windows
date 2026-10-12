# Packaging & Smart App Control Signing Guide

FinPulse Windows produces three distinct distribution formats:
1. **Standalone Executable (`.exe`)** (`publish/win-x64/FinPulse.exe`): Single-file self-contained binary.
2. **Standard MSI Setup (`.msi`)** (`dist/FinPulse-Setup-x64.msi`): Per-machine installer with WiX v4.
3. **MSIX Modern Package (`.msix`)** (`dist/FinPulse-x64.msix`): Modern Windows App container with identity and auto-update support.

---

## 🛡️ Windows Smart App Control (SAC) & Defender SmartScreen

Windows 11 features **Smart App Control (SAC)** and **SmartScreen**, which block or flag applications lacking cryptographic trust or algorithmic reputation.

### 1. Requirements for Zero-Warning Execution on Clean Windows 11 Systems
To pass Smart App Control without warnings out-of-the-box on consumer PCs:
- **Code Signing Certificate**: The binary must be signed with an Authenticode digital signature from a trusted Commercial Certificate Authority (e.g. DigiCert, Sectigo, GlobalSign) via an EV (Extended Validation) hardware token / Azure Key Vault or standard OV code signing certificate.
- **RFC 3161 Timestamp**: The signature **must** include an RFC 3161 timestamp server (e.g. `http://timestamp.digicert.com`). This ensures the binary remains valid even after the certificate expires.
- **Application Reputation**: Smart App Control checks Microsoft's cloud reputation telemetry. Trusted CA certificates build this reputation rapidly.

### 2. CI/CD Automated Signing with Production Certificate
To sign with your commercial certificate in the automated GitHub Actions pipeline:
1. Base64-encode your `.pfx` certificate:
   ```powershell
   [Convert]::ToBase64String([IO.File]::ReadAllBytes("path\to\your_certificate.pfx")) | Set-Clipboard
   ```
2. In your GitHub repository settings (**Settings > Secrets and variables > Actions**), add:
   - `CODE_SIGNING_PFX_BASE64`: The Base64 string from step 1.
   - `CODE_SIGNING_PASSWORD`: The private key password.
3. The `.github/workflows/build-and-release.yml` pipeline will automatically detect these secrets, sign the `.exe`, `.msi`, and `.msix` using `signtool.exe`, and timestamp with DigiCert.

### 3. Local Development / Self-Signed Testing (Trusting on Local Windows)
When running builds locally or from testing releases before purchasing a commercial EV certificate:
1. Open PowerShell as Administrator.
2. Generate and trust the development code signing certificate:
   ```powershell
   # 1. Create the certificate
   $cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=FinPulse Windows, O=FinPulse Open Source, C=US" -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(5)
   
   # 2. Export public certificate
   Export-Certificate -Cert $cert -FilePath "FinPulse-Root.cer"
   
   # 3. Import to Trusted Root Certification Authorities & Trusted Publishers (allows running without SAC block)
   Import-Certificate -FilePath "FinPulse-Root.cer" -CertStoreLocation "Cert:\LocalMachine\Root"
   Import-Certificate -FilePath "FinPulse-Root.cer" -CertStoreLocation "Cert:\LocalMachine\TrustedPublisher"
   ```
3. Sign any built binary:
   ```powershell
   & signtool.exe sign /fd sha256 /tr http://timestamp.digicert.com /td sha256 /a "path\to\FinPulse.exe"
   ```
