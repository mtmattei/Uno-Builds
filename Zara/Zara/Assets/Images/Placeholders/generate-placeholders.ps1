# PowerShell script to generate placeholder images
Add-Type -AssemblyName System.Drawing

$placeholderDir = $PSScriptRoot

# Product images (360x480)
$productNames = @(
    "product-1", "product-2", "product-3", "product-4",
    "product-5", "product-6", "product-7", "product-8", "product-9"
)

foreach ($name in $productNames) {
    $bmp = New-Object System.Drawing.Bitmap(360, 480)
    $graphics = [System.Drawing.Graphics]::FromImage($bmp)
    $graphics.Clear([System.Drawing.Color]::FromArgb(245, 245, 245))

    $font = New-Object System.Drawing.Font("Arial", 16, [System.Drawing.FontStyle]::Bold)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(117, 117, 117))
    $text = $name.ToUpper()
    $textSize = $graphics.MeasureString($text, $font)
    $x = (360 - $textSize.Width) / 2
    $y = (480 - $textSize.Height) / 2

    $graphics.DrawString($text, $font, $brush, $x, $y)

    $filePath = Join-Path $placeholderDir "$name.png"
    $bmp.Save($filePath)

    $graphics.Dispose()
    $bmp.Dispose()

    Write-Host "Created $filePath"
}

# Fit images (400x533)
$fitNames = @("fit-baggy", "fit-flare", "fit-slim", "fit-straight")

foreach ($name in $fitNames) {
    $bmp = New-Object System.Drawing.Bitmap(400, 533)
    $graphics = [System.Drawing.Graphics]::FromImage($bmp)
    $graphics.Clear([System.Drawing.Color]::FromArgb(236, 236, 236))

    $font = New-Object System.Drawing.Font("Arial", 18, [System.Drawing.FontStyle]::Bold)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(26, 26, 26))
    $text = $name.Replace("fit-", "").ToUpper() + " FIT"
    $textSize = $graphics.MeasureString($text, $font)
    $x = (400 - $textSize.Width) / 2
    $y = (533 - $textSize.Height) / 2

    $graphics.DrawString($text, $font, $brush, $x, $y)

    $filePath = Join-Path $placeholderDir "$name.png"
    $bmp.Save($filePath)

    $graphics.Dispose()
    $bmp.Dispose()

    Write-Host "Created $filePath"
}

Write-Host "`nPlaceholder images generated successfully!"
Write-Host "Replace these with actual product images when available."
