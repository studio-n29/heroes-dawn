# Exporte les pages de maquettes en PNG 1920x1080 avec Microsoft Edge (headless). Aucun téléchargement.
# Usage : pwsh export.ps1            (toutes les pages)
#         pwsh export.ps1 -Only M10,M11
param([string[]]$Only)
$ErrorActionPreference = 'Stop'
$edge = @("${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe", "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $edge) { throw "Microsoft Edge introuvable." }
$src = $PSScriptRoot
$root = Split-Path $src -Parent
$profile = Join-Path $env:TEMP "hd-maquettes-edge"
node (Join-Path $src 'check.js'); if ($LASTEXITCODE -ne 0) { throw "check.js en échec : corriger les pages avant l'export." }
$pages = Get-ChildItem (Join-Path $src 'pages') -Filter *.html | Where-Object { $_.Name -notlike '_*' -or $Only }
foreach ($p in $pages) {
    $html = Get-Content $p.FullName -Raw
    $ref = [regex]::Match($html, 'data-ref="([^"]+)"').Groups[1].Value
    $out = [regex]::Match($html, 'data-out="([^"]+)"').Groups[1].Value
    if (-not $out) { $out = 'ecrans' }
    if ($Only -and ($Only -notcontains $ref)) { continue }
    $dir = Join-Path $root $out
    New-Item -ItemType Directory -Force $dir | Out-Null
    $png = Join-Path $dir ($p.BaseName + '.png')
    $url = 'file:///' + $p.FullName.Replace([char]92, [char]47) + '?export=1'
    & $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1 --user-data-dir="$profile" `
        --window-size=1920,1080 --virtual-time-budget=4000 --screenshot="$png" $url 2>$null | Out-Null
    if (-not (Test-Path $png)) { throw "Export raté : $($p.Name)" }
    Write-Host "OK $ref -> $out/$($p.BaseName).png"
}
