Add-Type -AssemblyName System.Drawing

$assetsDir = "c:\Users\dg321\Downloads\Antigravity\Calculator\src\FinPulse\Assets"
$masterPath = Join-Path $assetsDir "MasterLogo1024.png"

if (-not (Test-Path $masterPath)) {
    Write-Error "MasterLogo1024.png not found at $masterPath"
    exit 1
}

$masterImg = [System.Drawing.Image]::FromFile($masterPath)

# Helper function to downscale an image with highest quality
function Resize-Image {
    param (
        [System.Drawing.Image]$source,
        [int]$width,
        [int]$height,
        [string]$outputPath
    )
    $destBitmap = New-Object System.Drawing.Bitmap($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($destBitmap)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)

    $graphics.DrawImage($source, 0, 0, $width, $height)
    $graphics.Dispose()

    $destBitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $destBitmap.Dispose()
    Write-Host "Created $outputPath ($width x $height)"
}

# 1. Square150x150Logo.png (150 x 150)
Resize-Image -source $masterImg -width 150 -height 150 -outputPath (Join-Path $assetsDir "Square150x150Logo.png")

# 2. Square44x44Logo.png (44 x 44)
Resize-Image -source $masterImg -width 44 -height 44 -outputPath (Join-Path $assetsDir "Square44x44Logo.png")

# 3. StoreLogo.png (50 x 50)
Resize-Image -source $masterImg -width 50 -height 50 -outputPath (Join-Path $assetsDir "StoreLogo.png")

# 4. Wide310x150Logo.png (310 x 150)
$w = 310
$h = 150
$bmp = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

# Background: Clean deep navy gradient
$bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.Point(0, 0)),
    (New-Object System.Drawing.Point($w, $h)),
    [System.Drawing.Color]::FromArgb(255, 9, 18, 38),
    [System.Drawing.Color]::FromArgb(255, 15, 30, 60)
)
$g.FillRectangle($bgBrush, 0, 0, $w, $h)
$bgBrush.Dispose()

# Subtle outer border
$borderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(50, 56, 189, 248), 1)
$g.DrawRectangle($borderPen, 0, 0, $w - 1, $h - 1)
$borderPen.Dispose()

# Accent bottom line
$accentBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.Point(0, 0)),
    (New-Object System.Drawing.Point($w, 0)),
    [System.Drawing.Color]::FromArgb(0, 34, 211, 238),
    [System.Drawing.Color]::FromArgb(220, 34, 211, 238)
)
$pen2 = New-Object System.Drawing.Pen($accentBrush, 2)
$g.DrawLine($pen2, 0, $h - 2, $w, $h - 2)
$pen2.Dispose()
$accentBrush.Dispose()

# Draw Logo on left
$logoSize = 120
$g.DrawImage($masterImg, 15, 15, $logoSize, $logoSize)

# Font families
$ff = New-Object System.Drawing.FontFamily("Segoe UI")

# Vector Path for 'FinPulse' title for ultimate sharpness
$titlePath = New-Object System.Drawing.Drawing2D.GraphicsPath
$titlePath.AddString("FinPulse", $ff, [int][System.Drawing.FontStyle]::Bold, 28, (New-Object System.Drawing.PointF(144, 20)), (New-Object System.Drawing.StringFormat))
$titleBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$g.FillPath($titleBrush, $titlePath)
$titlePath.Dispose()
$titleBrush.Dispose()

# 'XIRR CALCULATOR'
$subPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$subPath.AddString("XIRR CALCULATOR", $ff, [int][System.Drawing.FontStyle]::Bold, 12, (New-Object System.Drawing.PointF(146, 56)), (New-Object System.Drawing.StringFormat))
$subBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 34, 211, 238))
$g.FillPath($subBrush, $subPath)
$subPath.Dispose()
$subBrush.Dispose()

# Tagline
$tagPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$tagPath.AddString("Financial Intelligence Suite", $ff, [int][System.Drawing.FontStyle]::Regular, 10, (New-Object System.Drawing.PointF(146, 76)), (New-Object System.Drawing.StringFormat))
$tagBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 148, 163, 184))
$g.FillPath($tagBrush, $tagPath)
$tagPath.Dispose()
$tagBrush.Dispose()

# Pill badge with modules
$pillRect = New-Object System.Drawing.Rectangle(146, 100, 152, 24)
$pillBg = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(90, 15, 23, 42))
$pillBorder = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(90, 56, 189, 248), 1)
$g.FillRectangle($pillBg, $pillRect)
$g.DrawRectangle($pillBorder, $pillRect)
$pillBg.Dispose()
$pillBorder.Dispose()

$modPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$bullet = [char]0x2022
$modString = "XIRR $bullet SIP $bullet EMI $bullet CAGR"
$modPath.AddString($modString, $ff, [int][System.Drawing.FontStyle]::Bold, 9.5, (New-Object System.Drawing.PointF(151, 104)), (New-Object System.Drawing.StringFormat))
$modBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 226, 232, 240))
$g.FillPath($modBrush, $modPath)
$modPath.Dispose()
$modBrush.Dispose()

$ff.Dispose()
$g.Dispose()

$widePath = Join-Path $assetsDir "Wide310x150Logo.png"
$bmp.Save($widePath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "Created $widePath ($w x $h)"

# 5. Generate AppIcon.ico with multiple resolutions (16, 24, 32, 48, 64, 128, 256)
$sizes = @(16, 24, 32, 48, 64, 128, 256)
$pngStreams = @()

foreach ($sz in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap($sz, $sz, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.DrawImage($masterImg, 0, 0, $sz, $sz)
    $g.Dispose()

    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $pngStreams += $ms
}

$icoPath = Join-Path $assetsDir "AppIcon.ico"
$fs = [System.IO.File]::Create($icoPath)
$bw = New-Object System.IO.BinaryWriter($fs)

# Header
$bw.Write([uint16]0) # Reserved
$bw.Write([uint16]1) # Icon type (1 = ICO)
$bw.Write([uint16]$sizes.Count) # Image count

# Calculate initial offset (6 bytes header + 16 bytes * count)
$offset = 6 + (16 * $sizes.Count)

for ($i = 0; $i -lt $sizes.Count; $i++) {
    $sz = $sizes[$i]
    $dataLength = $pngStreams[$i].Length
    
    $wByte = if ($sz -ge 256) { [byte]0 } else { [byte]$sz }
    $hByte = if ($sz -ge 256) { [byte]0 } else { [byte]$sz }

    $bw.Write($wByte)
    $bw.Write($hByte)
    $bw.Write([byte]0) # Color palette count
    $bw.Write([byte]0) # Reserved
    $bw.Write([uint16]1) # Color planes
    $bw.Write([uint16]32) # BPP
    $bw.Write([uint32]$dataLength)
    $bw.Write([uint32]$offset)

    $offset += $dataLength
}

for ($i = 0; $i -lt $sizes.Count; $i++) {
    $bytes = $pngStreams[$i].ToArray()
    $bw.Write($bytes)
    $pngStreams[$i].Dispose()
}

$bw.Flush()
$fs.Dispose()
Write-Host "Created $icoPath successfully!"

$masterImg.Dispose()
Write-Host "All assets generated successfully!"
