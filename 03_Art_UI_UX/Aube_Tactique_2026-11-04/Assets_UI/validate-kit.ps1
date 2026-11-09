param([string]$Workspace='C:/Users/dnath/Documents/Heroes Dawn')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Join-Path $Workspace 'Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueKit'
$manifest=Get-Content (Join-Path $root 'manifest.json') -Raw | ConvertFrom-Json
$errors=[Collections.Generic.List[string]]::new();$imageCount=0;$portraitCount=0;$copiedCount=0;$alphaCount=0
$ids=@{};foreach($entry in $manifest.items){if($ids.ContainsKey($entry.id)){$errors.Add('Identifiant duplique : '+$entry.id)};$ids[$entry.id]=$true}
foreach($entry in $manifest.items) {
    $file=Join-Path $Workspace $entry.file
    if(!(Test-Path -LiteralPath $file)){$errors.Add('Fichier absent : '+$entry.file);continue}
    if(!(Test-Path -LiteralPath ($file+'.meta'))){$errors.Add('Meta Unity absente : '+$entry.file)}
    if($entry.source -and !(Test-Path -LiteralPath (Join-Path $Workspace $entry.source))){$errors.Add('Source absente : '+$entry.id)}
    if($entry.kind -in @('vector-ui','illustration')) {
        $image=[Drawing.Bitmap]::new($file);$imageCount++
        if($image.Width -ne $entry.width -or $image.Height -ne $entry.height){$errors.Add('Dimensions incorrectes : '+$entry.id)}
        if($entry.kind -eq 'vector-ui') {
            if(![Drawing.Image]::IsAlphaPixelFormat($image.PixelFormat)){$errors.Add('Canal alpha absent : '+$entry.id)}else{$alphaCount++}
            if($entry.id -like 'Frames/card-*' -and $image.GetPixel([int]($image.Width/2),[int]($image.Height/2)).A -ne 0){$errors.Add('Centre de cadre opaque : '+$entry.id)}
        }
        if($entry.file -match '/resource-(matter|elemental|bio|perception)-' -and $image.GetPixel(0,0).A -ne 0){$errors.Add('Ressource sans alpha de fond : '+$entry.id)}
        $image.Dispose()
    }
    if($entry.kind -eq 'portrait-sprite') {
        $source=Join-Path $Workspace $entry.original
        $meta=Get-Content -LiteralPath ($source+'.meta') -Raw
        $guid=[regex]::Match($meta,'(?m)^guid: ([a-f0-9]+)').Groups[1].Value
        if(!$guid -or !(Get-Content -LiteralPath $file -Raw).Contains($guid)){$errors.Add('Texture du portrait non resolue : '+$entry.id)}
        $portraitCount++
    }
    $original=$null
    if($entry.category -match '^Illustrations/(Agents|Characters|Cards|Items)$'){$original=Join-Path $Workspace ('Code/Heroes Dawn Unity/Assets/_Project/Resources/Illustrations/'+$Matches[1]+'/'+[IO.Path]::GetFileName($file))}
    elseif($entry.category -eq 'Illustrations/Production'){$original=Join-Path $Workspace ('Code/Heroes Dawn Unity/Assets/_Project/Resources/UI/AubeTactique/'+[IO.Path]::GetFileName($file))}
    elseif($entry.kind -eq 'font-or-license'){$original=Join-Path $Workspace $entry.source}
    if($original) {
        if((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $original -Algorithm SHA256).Hash){$errors.Add('Copie differente de son original : '+$entry.id)}
        $copiedCount++
    }
}
$svgCount=0
foreach($file in Get-ChildItem (Join-Path $root 'Sources') -Recurse -Filter '*.svg'){try{[xml]$xml=Get-Content -LiteralPath $file.FullName -Raw;$svgCount++}catch{$errors.Add('SVG invalide : '+$file.FullName)}}
$viewCount=0
foreach($board in $manifest.screenCoverage){$viewCount+=$board.views.Count;if(!(Test-Path -LiteralPath (Join-Path $Workspace $board.reference))){$errors.Add('Reference de planche absente : '+$board.board)};foreach($id in $board.illustrations){if(!$ids.ContainsKey($id)){$errors.Add('Couverture non resolue : '+$id)}}}
$import=Get-Content (Join-Path $root 'unity-import-report.json') -Raw | ConvertFrom-Json
if($import.importedSprites -ne 532 -or $import.missing -ne 0){$errors.Add('Rapport import inattendu')}
$report=[ordered]@{date='2026-11-05';entries=$manifest.items.Count;unitySprites=$import.importedSprites;images=$imageCount;vectorUiWithAlpha=$alphaCount;portraitSprites=$portraitCount;validSvgSources=$svgCount;originalCopiesSha256Matched=$copiedCount;referenceBoards=$manifest.screenCoverage.Count;referenceViews=$viewCount;errors=$errors.ToArray();passed=($errors.Count -eq 0)}
$json=$report | ConvertTo-Json -Depth 7
[IO.File]::WriteAllText((Join-Path $root 'validation.json'),$json);[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'validation.json'),$json)
Write-Output $json
if($errors.Count -gt 0){exit 1}
