Add-Type -AssemblyName System.Drawing

$inputPath  = ".\in.png"
$outputPath = ".\out.png"

# Load source image
$src = [System.Drawing.Image]::FromFile($inputPath)

# ---- Crop rectangle from source ----
$cropPercent = 70

if ($cropPercent -le 0 -or $cropPercent -gt 100) {
	throw "cropPercent must be in range (0, 100]."
}

$scale = $cropPercent / 100.0
$cropW = [Math]::Max(1, [int][Math]::Round($src.Width * $scale))
$cropH = [Math]::Max(1, [int][Math]::Round($src.Height * $scale))
$cropX = [int][Math]::Floor(($src.Width - $cropW) / 2.0)
$cropY = [int][Math]::Floor(($src.Height - $cropH) / 2.0)

$cropRect = New-Object System.Drawing.Rectangle($cropX, $cropY, $cropW, $cropH)
$cropped  = New-Object System.Drawing.Bitmap($cropW, $cropH)
$g1 = [System.Drawing.Graphics]::FromImage($cropped)
$g1.DrawImage($src, (New-Object System.Drawing.Rectangle(0,0,$cropW,$cropH)), $cropRect, [System.Drawing.GraphicsUnit]::Pixel)
$g1.Dispose()

# ---- Resize cropped image ----
$newW = 128*2
$newH = 72*2
$resized = New-Object System.Drawing.Bitmap($newW, $newH)
$g2 = [System.Drawing.Graphics]::FromImage($resized)
$g2.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g2.DrawImage($cropped, 0, 0, $newW, $newH)
$g2.Dispose()

# Save and cleanup
$resized.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
$resized.Dispose()
$cropped.Dispose()
$src.Dispose()