param(
    [string]$GameDir = "D:\SteamLibrary\steamapps\steamapps\common\CCFR"
)

$ErrorActionPreference = "Stop"

$repo = Split-Path -Parent $PSScriptRoot
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$dest = Join-Path $repo "backups\$stamp"
New-Item -ItemType Directory -Force -Path $dest | Out-Null

$saveDir = Join-Path $env:USERPROFILE "AppData\LocalLow\GonzoSSM\Caravan Carnage Fruity Road"
$json = Join-Path $GameDir "Caravan Carnage Fruity Road_Data\StreamingAssets\jsonData.json"

foreach ($name in @("playerInfo.dat", "playerInfo.dat.bak", "intro.dat")) {
    $src = Join-Path $saveDir $name
    if (Test-Path $src) {
        Copy-Item $src $dest -Force
        Write-Host "Backed up $name"
    }
}

if (Test-Path $json) {
    Copy-Item $json $dest -Force
    Write-Host "Backed up jsonData.json"
}

Write-Host "Backup written to: $dest"
