param(
    [string]$OutputPath = (Join-Path $PSScriptRoot "..\VanguardVolume.App\Assets\vanguard-volume.ico")
)

Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;

public static class IconNative
{
    [DllImport("user32.dll")]
    public static extern bool DestroyIcon(IntPtr hIcon);
}
"@

$directory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Path $directory -Force | Out-Null

$bitmap = [System.Drawing.Bitmap]::new(256, 256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.Clear([System.Drawing.Color]::FromArgb(18, 25, 37))

$frame = [System.Drawing.Rectangle]::new(18, 43, 220, 170)
$graphics.FillRectangle([System.Drawing.Brushes]::Black, $frame)
$graphics.DrawRectangle([System.Drawing.Pens]::DeepSkyBlue, $frame)

for ($row = 0; $row -lt 5; $row++) {
    for ($column = 0; $column -lt 12; $column++) {
        $x = 30 + ($column * 15) + (($row % 2) * 4)
        $y = 58 + ($row * 25)
        $key = [System.Drawing.Rectangle]::new($x, $y, 11, 17)
        $graphics.FillRectangle([System.Drawing.Brushes]::SlateGray, $key)
    }
}

$graphics.FillRectangle([System.Drawing.Brushes]::DeepSkyBlue, 211, 58, 14, 117)
$font = [System.Drawing.Font]::new("Segoe UI", 24, [System.Drawing.FontStyle]::Bold)
$graphics.DrawString("V", $font, [System.Drawing.Brushes]::White, 102, 15)
$graphics.DrawLine([System.Drawing.Pens]::DeepSkyBlue, 37, 194, 219, 194)

$pngPath = Join-Path $directory "vanguard-volume.png"
$bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
$iconHandle = $bitmap.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($iconHandle)
$stream = [System.IO.File]::Open($OutputPath, [System.IO.FileMode]::Create)
$icon.Save($stream)
$stream.Dispose()
$icon.Dispose()
[IconNative]::DestroyIcon($iconHandle) | Out-Null
$graphics.Dispose()
$bitmap.Dispose()
