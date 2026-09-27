param([switch]$Onar, [switch]$Prova, [switch]$Otomatik, [string]$Surum = "", [string]$Paket = "")

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.Windows.Forms.Application]::EnableVisualStyles()
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

$kaynak = Split-Path -Parent $MyInvocation.MyCommand.Path
$betik = $MyInvocation.MyCommand.Path
if ($env:KUR_KOK) {
  $yerel = Join-Path $env:KUR_KOK "AppData\Local"
  $masaustu = Join-Path $env:KUR_KOK "Desktop"
  $belgeler = Join-Path $env:KUR_KOK "Documents"
} else {
  $yerel = $env:LOCALAPPDATA
  $masaustu = [Environment]::GetFolderPath("Desktop")
  $belgeler = [Environment]::GetFolderPath("MyDocuments")
}
if (-not $Surum) { $Surum = [string]$env:KUR_SURUM }
if (-not $Paket) { $Paket = [string]$env:KUR_PAKET }
$S = [hashtable]::Synchronized(@{
  ad = "AbxPilot"
  altbaslik = "Kılavuz gezgini"
  depo = "Teknesyum/AbxPilot"
  onar = [bool]$Onar
  kaynak = $kaynak
  hedef = Join-Path $yerel "Programs\AbxPilot"
  eski = Join-Path $belgeler "AbxPilot"
  masaustu = $masaustu
  yedek = Join-Path $yerel "AbxPilot\yedek"
  gunluk = Join-Path $yerel "AbxPilot\kurulum.log"
  surum = $Surum
  paket = $Paket
  yuzde = 0
  tavan = 2
  adim = "Hazırlanıyor"
  log = [System.Collections.ArrayList]::Synchronized((New-Object System.Collections.ArrayList))
  durum = "calisiyor"
  cevrimdisi = $false
  prova = ([bool]$Prova -or [bool]$env:KUR_PROVA)
  otomatik = ([bool]$Otomatik -or [bool]$env:KUR_OTOMATIK)
  baslat = $null
})

$is = {
  param($S)
  $ErrorActionPreference = "Continue"
  $ProgressPreference = "SilentlyContinue"
  [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
  New-Item -ItemType Directory -Force (Split-Path $S.gunluk) | Out-Null
  function Yaz([string]$m) {
    $satir = (Get-Date -Format "HH:mm:ss") + "  " + $m
    [void]$S.log.Add($satir)
    Add-Content -Path $S.gunluk -Value $satir -Encoding UTF8
    if ($S.otomatik) { Write-Host $satir }
  }
  function Adim([int]$y, [int]$t, [string]$m) { $S.yuzde = $y; $S.tavan = $t; $S.adim = $m; Yaz $m }
  function Durdur([string]$kok) {
    Get-Process | Where-Object { $_.Path -and $_.Path.StartsWith($kok + "\", [StringComparison]::OrdinalIgnoreCase) } | ForEach-Object {
      Yaz ("Kapatılıyor: " + $_.ProcessName)
      Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Milliseconds 800
  }
  function Api([string]$yol) {
    Invoke-RestMethod -Uri ("https://api.github.com/repos/" + $S.depo + $yol) -Headers @{ "User-Agent" = "AbxPilot-Kurulum"; "Accept" = "application/vnd.github+json" } -UseBasicParsing -TimeoutSec 30 -ErrorAction Stop
  }
  function Indir([string]$url, [string]$yol) {
    Invoke-WebRequest -Uri $url -OutFile $yol -Headers @{ "User-Agent" = "AbxPilot-Kurulum" } -UseBasicParsing -TimeoutSec 900 -ErrorAction Stop
  }
  function VeriYedekle([string[]]$kokler, [string]$yedekKok) {
    foreach ($k in $kokler) {
      if (-not (Test-Path -LiteralPath $k)) { continue }
      foreach ($d in @($k) + @(Get-ChildItem -LiteralPath $k -Directory -ErrorAction SilentlyContinue | ForEach-Object FullName)) {
        $v = Join-Path $d "veri"
        if (Test-Path -LiteralPath $v) {
          if ($d -eq $k) { $yv = Join-Path $yedekKok "_kok" } else { $yv = Join-Path $yedekKok (Split-Path $d -Leaf) }
          robocopy $v $yv /E /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
          if ($LASTEXITCODE -ge 8) { throw "Veri yedeklenemedi (robocopy $LASTEXITCODE): $v" }
          Yaz "Veri yedeklendi: $yv"
        }
      }
    }
  }
  function VeriGeriYukle([string]$yedekKok, [string]$hedef) {
    if (-not (Test-Path -LiteralPath $yedekKok)) { return }
    foreach ($y in @(Get-ChildItem -LiteralPath $yedekKok -Directory)) {
      if ($y.Name -eq "_kok") { $v = Join-Path $hedef "veri" } else { $v = Join-Path (Join-Path $hedef $y.Name) "veri" }
      robocopy $y.FullName $v /E /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
      if ($LASTEXITCODE -ge 8) { throw ("Veri geri yüklenemedi (robocopy $LASTEXITCODE); yedek duruyor: " + $y.FullName) }
      Yaz "Veri geri yüklendi: $v"
    }
  }

  $yeni = $null
  try {
    $kaynak = $S.kaynak
    $hedef = $S.hedef
    $gercekHedef = $hedef
    $gecici = Join-Path $env:TEMP ("kur-" + [guid]::NewGuid().ToString("N").Substring(0, 8))
    New-Item -ItemType Directory -Force $gecici | Out-Null
    Yaz "Kaynak: $kaynak"
    Yaz "Hedef : $hedef"
    if ($S.onar) { Yaz "Onarım: kurulum baştan yapılacak" }

    Adim 2 10 "GitHub bağlantısı sınanıyor"
    $zipUrl = $null; $sumUrl = $null; $zipYerel = $null; $sumYerel = $null; $etiket = $null
    $paketDizin = $S.paket
    if ($paketDizin) { Yaz "Yerel paket klasörü: $paketDizin" }
    else {
      $rel = $null
      $istenen = $null
      if ($S.surum) { $istenen = $S.surum; if (-not $istenen.StartsWith("v")) { $istenen = "v" + $istenen } }
      try {
        if ($istenen) { $rel = Api "/releases/tags/$istenen" }
        else {
          try { $rel = Api "/releases/latest" }
          catch {
            if ([int]$_.Exception.Response.StatusCode -ne 404) { throw }
            $rel = Api "/releases?per_page=20" | ForEach-Object { $_ } | Where-Object { -not $_.draft } | Select-Object -First 1
          }
        }
      } catch {
        if ([int]$_.Exception.Response.StatusCode -eq 404) {
          if ($istenen) { throw ("GitHub'da bu sürüm yok ya da depo herkese açık değil: " + $S.depo + " " + $istenen) }
          throw ("GitHub'da depo bulunamadı ya da herkese açık değil: " + $S.depo)
        }
        $kod = 0
        if ($_.Exception.Response) { $kod = [int]$_.Exception.Response.StatusCode }
        if ($kod -eq 403 -or $kod -eq 429) {
          Yaz "GitHub API sınırı doldu, sürüm sayfasından okunuyor"
          try {
            $etiket = $istenen
            if (-not $etiket) {
              $besleme = Invoke-WebRequest -Uri ("https://github.com/" + $S.depo + "/releases.atom") -Headers @{ "User-Agent" = "AbxPilot-Kurulum" } -UseBasicParsing -TimeoutSec 30 -ErrorAction Stop
              $m = [regex]::Match([string]$besleme.Content, '/releases/tag/([^"<>\s]+)')
              if (-not $m.Success) { throw "GitHub'da yayımlanmış bir sürüm yok" }
              $etiket = [Uri]::UnescapeDataString($m.Groups[1].Value)
            }
            $zipAdi = "AbxPilot-win-x64-$etiket.zip"
            $zipUrl = "https://github.com/" + $S.depo + "/releases/download/$etiket/$zipAdi"
            $sumUrl = $zipUrl + ".sha256"
            $rel = "sayfa"
            Yaz "Sürüm: $etiket"
          } catch {
            $S.cevrimdisi = $true
            Yaz ("GitHub'a ulaşılamadı: " + $_.Exception.Message)
          }
        } else {
          $S.cevrimdisi = $true
          Yaz ("GitHub'a ulaşılamadı: " + $_.Exception.Message)
        }
      }
      if (-not $S.cevrimdisi -and $rel -ne "sayfa") {
        if (-not $rel) { throw "GitHub'da yayımlanmış bir sürüm yok" }
        Yaz "GitHub erişimi tamam"
        $etiket = [string]$rel.tag_name
        $zipVarlik = @($rel.assets) | Where-Object { $_.name -like "AbxPilot-win-x64-*.zip" } | Select-Object -First 1
        if (-not $zipVarlik) { throw "Sürümde Windows paketi yok: $etiket" }
        $sumVarlik = @($rel.assets) | Where-Object { $_.name -eq ($zipVarlik.name + ".sha256") } | Select-Object -First 1
        if (-not $sumVarlik) { throw "Sürümde sağlama toplamı dosyası yok; doğrulamasız kurulum yapılmaz: $etiket" }
        $zipAdi = [string]$zipVarlik.name
        $zipUrl = [string]$zipVarlik.browser_download_url
        $sumUrl = [string]$sumVarlik.browser_download_url
        Yaz ("Sürüm: $etiket" + $(if ($rel.prerelease) { " (önizleme)" } else { "" }))
      } else { $paketDizin = $kaynak }
    }
    if (-not $zipUrl) {
      $zipYerel = Get-ChildItem -LiteralPath $paketDizin -Filter "AbxPilot-win-x64-*.zip" -File -ErrorAction SilentlyContinue | Sort-Object Name -Descending | Select-Object -First 1
      if (-not $zipYerel) {
        if ($S.cevrimdisi) { throw "GitHub'a ulaşılamadı ve kurulum klasöründe paket yok ($paketDizin)" }
        throw "Paket klasöründe AbxPilot-win-x64-*.zip yok: $paketDizin"
      }
      $sumYerel = $zipYerel.FullName + ".sha256"
      if (-not (Test-Path -LiteralPath $sumYerel)) { throw ("Paketin yanında sağlama toplamı dosyası yok; doğrulamasız kurulum yapılmaz: " + $zipYerel.Name + ".sha256") }
      $zipAdi = $zipYerel.Name
      $etiket = $zipAdi -replace '^AbxPilot-win-x64-(.+)\.zip$', '$1'
      Yaz "Yerel paket: $zipAdi"
    }

    Adim 10 16 "Eski kurulum aranıyor"
    $mevcut = Test-Path -LiteralPath $hedef
    $saglam = Test-Path -LiteralPath (Join-Path $hedef "AbxPilot.exe")
    if (-not $mevcut) { Yaz "Eski kurulum yok" }
    elseif ($S.onar) { Yaz "Onarım: eski kurulum doğrulanmış paketle değiştirilecek" }
    elseif (-not $saglam) { throw "Kurulum klasörü bozuk görünüyor ($hedef). Onar düğmesiyle baştan kurun." }
    else { Yaz "Kurulum var, yerinde güncellenecek" }
    $eskiVar = Test-Path -LiteralPath (Join-Path $S.eski ".git")
    if ($eskiVar) { Yaz ("Eski konumdaki kurulum bulundu: " + $S.eski + " — verisi taşınacak, klasör elle silinebilir") }

    if ($S.prova) { $hedef = Join-Path $gecici $S.ad; $S.hedef = $hedef; Yaz "Prova hedefi: $hedef" }

    Adim 16 20 "Kurulum yeri sınanıyor"
    $ust = Split-Path $hedef
    New-Item -ItemType Directory -Force $ust -ErrorAction SilentlyContinue | Out-Null
    $deneme = Join-Path $ust (".yazma-" + [guid]::NewGuid().ToString("N").Substring(0, 8))
    try { [IO.File]::WriteAllText($deneme, ""); Remove-Item -LiteralPath $deneme -Force }
    catch { throw "Kurulum yerine yazılamıyor: $ust" }
    Yaz "Yazma izni tamam: $ust"

    $zip = Join-Path $gecici $zipAdi
    $sum = $zip + ".sha256"
    if ($zipUrl) {
      Adim 20 60 "Güncel sürüm GitHub'dan indiriliyor"
      try { Indir $sumUrl $sum; Indir $zipUrl $zip }
      catch { throw ("İndirme başarısız: " + $_.Exception.Message) }
    } else {
      if ($S.cevrimdisi) { Adim 20 60 "USB'deki sürüm kopyalanıyor" } else { Adim 20 60 "Yerel paket kopyalanıyor" }
      Copy-Item -LiteralPath $zipYerel.FullName -Destination $zip -Force
      Copy-Item -LiteralPath $sumYerel -Destination $sum -Force
    }
    Yaz ("Paket: $zipAdi (" + [math]::Round((Get-Item -LiteralPath $zip).Length / 1MB, 1) + " MB)")

    Adim 60 66 "Sağlama toplamı doğrulanıyor"
    $parca = @(([IO.File]::ReadAllText($sum)).Trim() -split '\s+')
    $beklenen = $parca[0].ToLowerInvariant()
    if ($beklenen -notmatch '^[0-9a-f]{64}$') { throw "Sağlama toplamı dosyası okunamadı: $zipAdi.sha256" }
    if ($parca.Count -gt 1 -and $parca[1].TrimStart("*") -ne $zipAdi) { throw ("Sağlama toplamı başka bir dosyaya ait: " + $parca[1]) }
    $akis = [IO.File]::OpenRead($zip)
    try { $bulunan = -join ([Security.Cryptography.SHA256]::Create().ComputeHash($akis) | ForEach-Object { $_.ToString("x2") }) }
    finally { $akis.Dispose() }
    if ($bulunan -ne $beklenen) {
      Yaz "Beklenen SHA-256: $beklenen"
      Yaz "Bulunan  SHA-256: $bulunan"
      throw "Sağlama toplamı uyuşmadı; paket bozuk ya da değiştirilmiş. Kurulum durduruldu, bilgisayarda hiçbir şey değiştirilmedi."
    }
    Yaz "SHA-256 doğrulandı: $bulunan"

    Adim 66 78 "Paket açılıyor"
    $yeni = $hedef + ".yeni-" + [guid]::NewGuid().ToString("N").Substring(0, 8)
    [IO.Compression.ZipFile]::ExtractToDirectory($zip, $yeni)
    if (-not (Test-Path -LiteralPath (Join-Path $yeni "AbxPilot.exe"))) { throw "Pakette AbxPilot.exe yok: $zipAdi" }
    Yaz "Paket açıldı"

    Adim 78 88 "Eski kurulum kaldırılıyor"
    if ($S.prova) { $yedekKok = Join-Path $gecici "yedek" }
    else { $yedekKok = Join-Path $S.yedek (Get-Date -Format "yyyyMMdd-HHmmss") }
    $kokler = @()
    if ($eskiVar) { $kokler += $S.eski }
    if ($mevcut) { $kokler += $gercekHedef }
    VeriYedekle $kokler $yedekKok
    if ($mevcut) {
      if ($S.prova) { Yaz "Prova: silinmedi" }
      else {
        Durdur $hedef
        Remove-Item -LiteralPath $hedef -Recurse -Force -ErrorAction SilentlyContinue
        if (Test-Path -LiteralPath $hedef) { throw "Eski kurulum silinemedi (açık bir dosya olabilir): $hedef" }
        Yaz "Silindi: $hedef"
      }
    }
    Move-Item -LiteralPath $yeni -Destination $hedef
    $yeni = $null
    VeriGeriYukle $yedekKok $hedef
    Yaz "Program hazır: $hedef"

    Adim 92 97 "Masaüstü kısayolu yazılıyor"
    $calistir = Join-Path $hedef "AbxPilot.exe"
    if ($S.prova) { Yaz "Prova: kısayol yazılmadı" }
    elseif (Test-Path -LiteralPath $calistir) {
      New-Item -ItemType Directory -Force $S.masaustu | Out-Null
      $kisayol = Join-Path $S.masaustu ($S.ad + ".lnk")
      $lnk = (New-Object -ComObject WScript.Shell).CreateShortcut($kisayol)
      $lnk.TargetPath = $calistir
      $lnk.WorkingDirectory = $hedef
      $lnk.IconLocation = "$calistir,0"
      $lnk.Description = "AbxPilot - kılavuz gezgini"
      $lnk.Save()
      $S.baslat = $kisayol
      Yaz "Kısayol: $kisayol"
    } else { Yaz "Çalıştırılacak dosya bulunamadı, kısayol yazılmadı" }

    @{ tarih = (Get-Date).ToString("s"); surum = "$etiket"; paket = $zipAdi; sha256 = $bulunan; bilgisayar = $env:COMPUTERNAME; cevrimdisi = [bool]$S.cevrimdisi; prova = [bool]$S.prova } | ConvertTo-Json | Set-Content (Join-Path $hedef "kurulum.json") -Encoding UTF8
    if (-not $S.prova) { Remove-Item $gecici -Recurse -Force -ErrorAction SilentlyContinue }
    Adim 100 100 "Kurulum tamamlandı · sürüm $etiket"
    $S.durum = "bitti"
  } catch {
    if ($yeni -and (Test-Path -LiteralPath $yeni)) { Remove-Item -LiteralPath $yeni -Recurse -Force -ErrorAction SilentlyContinue }
    Yaz ("HATA: " + $_)
    $S.adim = "Kurulum yarıda kaldı: " + $_
    $S.durum = "hata"
  }
}

if ($S.otomatik) {
  & $is $S
  if ($S.durum -eq "bitti") { exit 0 } else { exit 1 }
}

function Renk([string]$h, [int]$a = 255) { [System.Drawing.Color]::FromArgb($a, [System.Drawing.ColorTranslator]::FromHtml($h)) }
function Yazi([int]$px, [string]$stil = "Regular", [string]$aile = "Segoe UI") { New-Object System.Drawing.Font($aile, $px, [System.Drawing.FontStyle]$stil, [System.Drawing.GraphicsUnit]::Pixel) }

$R = @{
  zemin = Renk "#08090a"; metin = Renk "#ffffff"; mavi = Renk "#00f3ff"; mor = Renk "#b026ff"
  basari = Renk "#34d399"; tehlike = Renk "#ff54eb"; sonuk = Renk "#71717a"
  kenar = Renk "#00f3ff" 128; iz = Renk "#00f3ff" 77
}
$YZ = @{ baslik = Yazi 24 "Bold"; adim = Yazi 16; kucuk = Yazi 14; log = Yazi 14 "Regular" "Consolas"; dugme = Yazi 14 "Bold" }
$B = @{}
foreach ($k in $R.Keys) { $B[$k] = New-Object System.Drawing.SolidBrush $R[$k] }
$G = @{ goster = 0.0; faz = 0.0; surukle = $null; sonlandi = $false; ikon = $null }

$f = New-Object System.Windows.Forms.Form
$f.Text = $S.ad + " Kurulum"
$f.FormBorderStyle = "None"
$f.StartPosition = "CenterScreen"
$f.ClientSize = New-Object System.Drawing.Size(560, 424)
$f.BackColor = $R.zemin
$f.ForeColor = $R.metin
$f.KeyPreview = $true
$f.GetType().GetProperty("DoubleBuffered", [Reflection.BindingFlags]"Instance,NonPublic").SetValue($f, $true, $null)
$simgeYol = Join-Path $kaynak "simge.ico"
if (Test-Path $simgeYol) {
  $f.Icon = New-Object System.Drawing.Icon($simgeYol)
  $G.ikon = (New-Object System.Drawing.Icon($simgeYol, 64, 64)).ToBitmap()
}

$f.Add_MouseDown({ if ($_.Button -eq "Left") { $G.surukle = $_.Location } })
$f.Add_MouseMove({ if ($G.surukle) { $f.Location = New-Object System.Drawing.Point(($f.Location.X + $_.X - $G.surukle.X), ($f.Location.Y + $_.Y - $G.surukle.Y)) } })
$f.Add_MouseUp({ $G.surukle = $null })

$f.Add_Paint({
  $cz = $_.Graphics
  $cz.SmoothingMode = "AntiAlias"
  $cz.TextRenderingHint = "ClearTypeGridFit"
  $w = $f.ClientSize.Width
  $h = $f.ClientSize.Height
  $bicim = New-Object System.Drawing.StringFormat
  $bicim.Trimming = "EllipsisCharacter"
  $bicim.FormatFlags = "NoWrap"
  $cz.DrawRectangle((New-Object System.Drawing.Pen($R.kenar, 1)), 0, 0, $w - 1, $h - 1)
  if ($G.ikon) { $cz.DrawImage($G.ikon, 24, 24, 48, 48) }
  $cz.DrawString($S.ad, $YZ.baslik, $B.metin, 84, 20)
  $gen = $cz.MeasureString($S.ad, $YZ.baslik).Width
  $cz.DrawString("Kurulum", $YZ.baslik, $B.mavi, 84 + $gen - 4, 20)
  if ($S.durum -eq "hata") { $alt = "Günlük  ·  " + $S.gunluk }
  elseif ($S.altbaslik) { $alt = $S.altbaslik + "  ·  " + $S.hedef }
  else { $alt = $S.hedef }
  $cz.DrawString($alt, $YZ.kucuk, $B.sonuk, (New-Object System.Drawing.RectangleF(86, 52, ($w - 110), 20)), $bicim)

  $renk = switch ($S.durum) { "bitti" { $R.basari } "hata" { $R.tehlike } default { $R.metin } }
  $yuzdeMetin = [string][math]::Floor($G.goster) + "%"
  $yg = $cz.MeasureString($yuzdeMetin, $YZ.adim).Width
  $alan = New-Object System.Drawing.RectangleF(24, 100, ($w - 60 - $yg), 22)
  $cz.DrawString($S.adim, $YZ.adim, (New-Object System.Drawing.SolidBrush $renk), $alan, $bicim)
  $cz.DrawString($yuzdeMetin, $YZ.adim, $B.mavi, ($w - 24 - $yg), 100)

  $bx = 24; $by = 132; $bw = $w - 48; $bh = 8
  $cz.FillRectangle((New-Object System.Drawing.SolidBrush $R.iz), $bx, $by, $bw, $bh)
  $dolu = [int]($bw * [math]::Min(100, $G.goster) / 100)
  if ($dolu -gt 1) {
    $dik = New-Object System.Drawing.Rectangle($bx, $by, $dolu, $bh)
    if ($S.durum -eq "calisiyor") { $fr = New-Object System.Drawing.Drawing2D.LinearGradientBrush($dik, $R.mavi, $R.mor, 0.0) }
    else { $fr = New-Object System.Drawing.SolidBrush $renk }
    $cz.FillRectangle($fr, $dik)
    for ($i = 1; $i -le 3; $i++) { $cz.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb([int](40 / $i), $renk))), $bx, $by - $i, $dolu, $bh + 2 * $i) }
    if ($S.durum -eq "calisiyor") {
      $px = $bx + (($G.faz % 1.0) * ($dolu + 120)) - 120
      $pr = New-Object System.Drawing.Rectangle([int]$px, $by, 120, $bh)
      $pg = New-Object System.Drawing.Drawing2D.LinearGradientBrush($pr, (Renk "#ffffff" 0), (Renk "#ffffff" 0), 0.0)
      $bl = New-Object System.Drawing.Drawing2D.ColorBlend(3)
      $bl.Colors = @((Renk "#ffffff" 0), (Renk "#ffffff" 150), (Renk "#ffffff" 0))
      $bl.Positions = @(0.0, 0.5, 1.0)
      $pg.InterpolationColors = $bl
      $cz.SetClip($dik)
      $cz.FillRectangle($pg, $pr)
      $cz.ResetClip()
    }
  }

  $satirlar = $S.log.ToArray()
  $n = $satirlar.Count
  $bas = [math]::Max(0, $n - 9)
  $y = 160
  for ($i = $bas; $i -lt $n; $i++) {
    $fircaLog = if ($i -eq $n - 1) { $B.mavi } else { $B.sonuk }
    $cz.DrawString($satirlar[$i], $YZ.log, $fircaLog, (New-Object System.Drawing.RectangleF(24, $y, ($w - 48), 20)), $bicim)
    $y += 20
  }
})

function Dugme([string]$metin, [bool]$birincil, [int]$x) {
  $dg = New-Object System.Windows.Forms.Button
  $dg.Text = $metin
  $dg.FlatStyle = "Flat"
  $dg.Font = $YZ.dugme
  $dg.Size = New-Object System.Drawing.Size(160, 36)
  $dg.Location = New-Object System.Drawing.Point($x, 364)
  $dg.Cursor = "Hand"
  if ($birincil) { $dg.BackColor = $R.mavi; $dg.ForeColor = $R.zemin; $dg.FlatAppearance.BorderSize = 0 }
  else { $dg.BackColor = $R.zemin; $dg.ForeColor = $R.mavi; $dg.FlatAppearance.BorderColor = $R.mavi }
  $dg.Visible = $false
  $f.Controls.Add($dg)
  $dg
}
$programAc = Dugme "Programı Aç" $true 24
$gunlukAc = Dugme "Günlüğü Aç" $true 24
$onarDugme = Dugme "Onar" $false 200
$kapat = Dugme "Kapat" $false 376
$programAc.Add_Click({ Start-Process $S.baslat; $f.Close() })
$gunlukAc.Add_Click({ Start-Process notepad.exe $S.gunluk })
$onarDugme.Add_Click({
  $argumanlar = @("-NoProfile", "-STA", "-File", ('"' + $betik + '"'), "-Onar")
  if ($S.prova) { $argumanlar += "-Prova" }
  if ($S.surum) { $argumanlar += @("-Surum", $S.surum) }
  if ($S.paket) { $argumanlar += @("-Paket", ('"' + $S.paket + '"')) }
  Start-Process powershell.exe -ArgumentList $argumanlar
  $f.Close()
})
$kapat.Add_Click({ $f.Close() })
$f.Add_KeyDown({ if ($_.KeyCode -eq "Escape" -and $S.durum -ne "calisiyor") { $f.Close() } })
$f.Add_FormClosing({ if ($S.durum -eq "calisiyor") { $_.Cancel = $true } })

$zaman = New-Object System.Windows.Forms.Timer
$zaman.Interval = 16
$zaman.Add_Tick({
  $hy = [double]$S.yuzde
  if ($G.goster -lt $hy) { $G.goster = [math]::Min($hy, $G.goster + [math]::Max(0.2, ($hy - $G.goster) * 0.08)) }
  elseif ($S.durum -eq "calisiyor" -and $G.goster -lt ($S.tavan - 0.5)) { $G.goster += ($S.tavan - $G.goster) * 0.006 }
  $G.faz += 0.012
  if ($S.durum -ne "calisiyor" -and -not $G.sonlandi) {
    $G.sonlandi = $true
    $kapat.Visible = $true
    $onarDugme.Visible = $true
    if ($S.durum -eq "bitti") {
      if (-not $S.prova -and $S.baslat) { $programAc.Visible = $true; [void]$programAc.Focus() } else { [void]$kapat.Focus() }
    } else { $gunlukAc.Visible = $true; [void]$gunlukAc.Focus() }
  }
  $f.Invalidate()
})

$rs = [runspacefactory]::CreateRunspace()
$rs.ApartmentState = "STA"
$rs.Open()
$ps = [powershell]::Create()
$ps.Runspace = $rs
[void]$ps.AddScript($is).AddArgument($S)
$f.Add_Shown({ [void]$ps.BeginInvoke(); $zaman.Start() })
[System.Windows.Forms.Application]::Run($f)
$zaman.Stop()
$rs.Close()
