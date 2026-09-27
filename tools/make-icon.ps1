param(
    [string]$Out = "$PSScriptRoot/../src/AbxPilot.UI/Assets/abxpilot.ico",
    [string]$Android = "$PSScriptRoot/../src/AbxPilot.Android/Resources/mipmap",
    [string]$Tokens = "$PSScriptRoot/../teknesyum-ui/theme.tokens.json"
)

Add-Type -AssemblyName System.Drawing

$sizes = 16, 24, 32, 48, 64, 128, 256
$brand = (Get-Content -Raw -Encoding UTF8 $Tokens | ConvertFrom-Json).brand
$renk = [System.Drawing.ColorTranslator]::FromHtml($brand.'renk-1'.value)
$zemin = [System.Drawing.ColorTranslator]::FromHtml($brand.surface.value)
$images = @()

foreach ($size in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear($zemin)
    $k = $size / 108.0
    $brush = New-Object System.Drawing.SolidBrush $renk

    $ring = New-Object System.Drawing.Drawing2D.GraphicsPath
    $ring.FillMode = [System.Drawing.Drawing2D.FillMode]::Alternate
    $ring.AddEllipse([single](24 * $k), [single](24 * $k), [single](60 * $k), [single](60 * $k))
    $ring.AddEllipse([single](34 * $k), [single](34 * $k), [single](40 * $k), [single](40 * $k))
    $g.FillPath($brush, $ring)
    $g.FillEllipse($brush, [single](44 * $k), [single](44 * $k), [single](20 * $k), [single](20 * $k))
    $g.FillRectangle($brush, [single](50 * $k), [single](10 * $k), [single](8 * $k), [single](14 * $k))
    $g.FillRectangle($brush, [single](50 * $k), [single](84 * $k), [single](8 * $k), [single](14 * $k))
    $g.FillRectangle($brush, [single](10 * $k), [single](50 * $k), [single](14 * $k), [single](8 * $k))
    $g.FillRectangle($brush, [single](84 * $k), [single](50 * $k), [single](14 * $k), [single](8 * $k))
    $g.Dispose()

    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    $images += , @($size, $ms.ToArray())
}

$dir = Split-Path $Out
if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force $dir | Out-Null }
$fs = [System.IO.File]::Create($Out)
$w = New-Object System.IO.BinaryWriter $fs
$w.Write([uint16]0); $w.Write([uint16]1); $w.Write([uint16]$images.Count)
$offset = 6 + 16 * $images.Count
foreach ($img in $images) {
    $s = $img[0]; $data = $img[1]
    $d = if ($s -ge 256) { 0 } else { $s }
    $w.Write([byte]$d); $w.Write([byte]$d); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([uint16]1); $w.Write([uint16]32)
    $w.Write([uint32]$data.Length); $w.Write([uint32]$offset)
    $offset += $data.Length
}
foreach ($img in $images) { $w.Write($img[1]) }
$w.Dispose()
Write-Output "icon: $Out"

$on = $brand.'renk-1'.value.ToUpperInvariant()
$arka = $brand.surface.value.ToUpperInvariant()
foreach ($xml in Get-ChildItem -Path $Android -Filter *.xml -ErrorAction SilentlyContinue) {
    $satirlar = Get-Content -Encoding UTF8 $xml.FullName | ForEach-Object {
        $renkSatir = if ($_ -match 'M0,0h108v108h-108z') { $arka } else { $on }
        $_ -replace 'android:fillColor="#[0-9A-Fa-f]{6}"', ('android:fillColor="' + $renkSatir + '"')
    }
    [System.IO.File]::WriteAllText($xml.FullName, (($satirlar -join "`n") + "`n"))
    Write-Output "android: $($xml.Name)"
}
