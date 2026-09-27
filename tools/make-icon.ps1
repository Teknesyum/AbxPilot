param(
    [string]$Out = "$PSScriptRoot/../src/AbxPilot.UI/Assets/abxpilot.ico",
    [string]$Android = "$PSScriptRoot/../src/AbxPilot.Android/Resources/mipmap",
    [string]$Tokens = "$PSScriptRoot/../teknesyum-ui/theme.tokens.json",
    [string]$Preview = ""
)

Add-Type -AssemblyName System.Drawing

$T = Get-Content -Raw -Encoding UTF8 $Tokens | ConvertFrom-Json
$hex = @{
    mavi  = $T.brand.'renk-1'.value.ToUpperInvariant()
    pembe = $T.brand.'renk-2'.value.ToUpperInvariant()
    mor   = $T.brand.'renk-3'.value.ToUpperInvariant()
    zemin = $T.brand.surface.value.ToUpperInvariant()
    isik  = $T.role.text.value.ToUpperInvariant()
}
$artiAlfa = 0.3
$renk = @{}
foreach ($k in $hex.Keys) { $renk[$k] = [System.Drawing.ColorTranslator]::FromHtml($hex[$k]) }
$renk.arti = [System.Drawing.Color]::FromArgb([int][Math]::Round(255 * $artiAlfa, [MidpointRounding]::AwayFromZero), $renk.mor)

$mavi = 'M41.98,41.98 L17.98,65.98 A17,17 0 0,0 42.02,90.02 L66.02,66.02 Z'
$pembe = 'M66.02,66.02 L90.02,42.02 A17,17 0 0,0 65.98,17.98 L41.98,41.98 Z'
$ara = 'M43.41,40.59 L67.41,64.59 L64.59,67.41 L40.59,43.41 Z'
$arti = 'M40,8h28v92h-28z M8,40h92v28h-92z'
$isiklar = @(@(26, 70, 36, 60), @(62, 34, 70, 26))

function Ciz([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear($renk.zemin)
    $k = [single]($size / 108.0)
    $g.ScaleTransform($k, $k)
    $b = New-Object System.Drawing.SolidBrush $renk.arti
    $g.FillRectangle($b, 40, 8, 28, 92)
    $g.FillRectangle($b, 8, 40, 92, 28)
    $kapsul = New-Object System.Drawing.Pen $renk.mavi, 34
    $kapsul.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($kapsul, 30, 78, 54, 54)
    $kapsul = New-Object System.Drawing.Pen $renk.pembe, 34
    $kapsul.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($kapsul, 54, 54, 78, 30)
    $g.DrawLine((New-Object System.Drawing.Pen $renk.zemin, 4), 42, 42, 66, 66)
    $isik = New-Object System.Drawing.Pen $renk.isik, 5
    $isik.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $isik.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    foreach ($c in $isiklar) { $g.DrawLine($isik, $c[0], $c[1], $c[2], $c[3]) }
    $g.Dispose()
    $bmp
}

$sizes = 16, 24, 32, 48, 64, 128, 256
$images = @()
foreach ($size in $sizes) {
    $bmp = Ciz $size
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

if ($Preview) {
    $bmp = Ciz 256
    $bmp.Save($Preview, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Output "preview: $Preview"
}

$artiHex = '#' + ([int][Math]::Round(255 * $artiAlfa, [MidpointRounding]::AwayFromZero)).ToString('X2') + $hex.mor.TrimStart('#')
$bas = @(
    '<?xml version="1.0" encoding="utf-8"?>',
    '<vector xmlns:android="http://schemas.android.com/apk/res/android"',
    '    android:width="108dp"',
    '    android:height="108dp"',
    '    android:viewportWidth="108"',
    '    android:viewportHeight="108">'
)
$zeminSatir = '  <path android:fillColor="' + $hex.zemin + '" android:pathData="M0,0h108v108h-108z"/>'
$artiSatir = '  <path android:fillColor="' + $artiHex + '" android:pathData="' + $arti + '"/>'
function Kapsul([string]$girinti) {
    $s = @(
        ($girinti + '<path android:fillColor="' + $hex.mavi + '" android:pathData="' + $mavi + '"/>'),
        ($girinti + '<path android:fillColor="' + $hex.pembe + '" android:pathData="' + $pembe + '"/>'),
        ($girinti + '<path android:fillColor="' + $hex.zemin + '" android:pathData="' + $ara + '"/>')
    )
    foreach ($c in $isiklar) {
        $s += $girinti + '<path android:strokeColor="' + $hex.isik + '" android:strokeWidth="5" android:strokeLineCap="round" android:pathData="M' + $c[0] + ',' + $c[1] + ' L' + $c[2] + ',' + $c[3] + '"/>'
    }
    $s
}
$dosyalar = @{
    'ic_launcher.xml'            = $bas + $zeminSatir + $artiSatir + (Kapsul '  ') + '</vector>'
    'ic_launcher_background.xml' = $bas + $zeminSatir + $artiSatir + '</vector>'
    'ic_launcher_foreground.xml' = $bas + '  <group android:pivotX="54" android:pivotY="54" android:scaleX="0.8" android:scaleY="0.8">' + (Kapsul '    ') + '  </group>' + '</vector>'
}
if (Test-Path $Android) {
    foreach ($ad in $dosyalar.Keys) {
        [System.IO.File]::WriteAllText((Join-Path $Android $ad), (($dosyalar[$ad] -join "`n") + "`n"))
        Write-Output "android: $ad"
    }
}
