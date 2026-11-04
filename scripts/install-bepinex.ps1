param(
    [string]$GameDir = "D:\SteamLibrary\steamapps\steamapps\common\CCFR",
    [string]$BepInExVersion = "5.4.23.6"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $GameDir)) {
    throw "Game folder not found: $GameDir"
}

if (Test-Path (Join-Path $GameDir "BepInEx\core\BepInEx.dll")) {
    Write-Host "BepInEx already installed in $GameDir"
    return
}

$zipName = "BepInEx_win_x64_$BepInExVersion.zip"
$url = "https://github.com/BepInEx/BepInEx/releases/download/v$BepInExVersion/$zipName"
$tmp = Join-Path $env:TEMP "opencode\ccfr"
New-Item -ItemType Directory -Force -Path $tmp | Out-Null
$zip = Join-Path $tmp $zipName

Write-Host "Downloading $url"
Invoke-WebRequest -Uri $url -OutFile $zip -UseBasicParsing

Write-Host "Extracting into $GameDir"
Expand-Archive -Path $zip -DestinationPath $GameDir -Force

Write-Host "BepInEx $BepInExVersion installed."
