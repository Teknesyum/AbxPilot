param([string]$Target = "$env:LOCALAPPDATA\Programs\AbxPilot")

$root = Resolve-Path "$PSScriptRoot/.."
$running = Get-Process AbxPilot -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "$Target*" }
if ($running) { $running | Stop-Process -Force; Start-Sleep -Milliseconds 500 }

dotnet publish "$root/src/AbxPilot.Desktop/AbxPilot.Desktop.csproj" -c Release -r win-x64 --self-contained true -o $Target -p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$exe = Join-Path $Target "AbxPilot.exe"
$desktop = [Environment]::GetFolderPath("Desktop")
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut((Join-Path $desktop "AbxPilot.lnk"))
$link.TargetPath = $exe
$link.WorkingDirectory = $Target
$link.IconLocation = "$exe,0"
$link.Description = "AbxPilot - guideline navigator"
$link.Save()
Write-Output "published: $exe"
Write-Output "shortcut: $(Join-Path $desktop 'AbxPilot.lnk')"
