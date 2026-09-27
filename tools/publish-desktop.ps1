param([string]$Target = "$env:LOCALAPPDATA\Programs\AbxPilot")

$root = Resolve-Path "$PSScriptRoot/.."
$running = Get-Process AbxPilot -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "$Target*" }
if ($running) { $running | Stop-Process -Force; Start-Sleep -Milliseconds 500 }

dotnet publish "$root/src/AbxPilot.Desktop/AbxPilot.Desktop.csproj" -c Release -r win-x64 --self-contained true -o $Target -p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$exe = Join-Path $Target "AbxPilot.exe"
$ico = Join-Path $Target "abxpilot.ico"
$iconLocation = if (Test-Path $ico) { "$ico,0" } else { "$exe,0" }
$shell = New-Object -ComObject WScript.Shell
foreach ($folder in @([Environment]::GetFolderPath("Desktop"), [Environment]::GetFolderPath("Programs"))) {
    $path = Join-Path $folder "AbxPilot.lnk"
    $link = $shell.CreateShortcut($path)
    $link.TargetPath = $exe
    $link.WorkingDirectory = $Target
    $link.IconLocation = $iconLocation
    $link.Description = "AbxPilot - guideline navigator"
    $link.Save()
    Write-Output "shortcut: $path ($iconLocation)"
}
& "$env:SystemRoot\System32\ie4uinit.exe" -show
Write-Output "published: $exe"
