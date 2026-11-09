param([string]$Workspace='C:/Users/dnath/Documents/Heroes Dawn')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
[System.Threading.Thread]::CurrentThread.CurrentCulture=[System.Globalization.CultureInfo]::InvariantCulture
$project=Join-Path $Workspace 'Code/Heroes Dawn Unity'
$destination=Join-Path $project 'Assets/_Project/Art/UI/AubeTactiqueKit'
$native=Get-Content (Join-Path $PSScriptRoot 'manifest-native.json') -Raw | ConvertFrom-Json
$items=[System.Collections.Generic.List[object]]::new()
foreach($item in $native.items){$items.Add($item)}
function Relative($path){[IO.Path]::GetRelativePath($Workspace,$path).Replace('\','/')}
function Add-Art($source,$subfolder,$id,$origin,$prompt='') {
    $folder=Join-Path $destination ('Illustrations/'+$subfolder)
    New-Item -ItemType Directory -Force -Path $folder | Out-Null
    $target=Join-Path $folder ($id+[IO.Path]::GetExtension($source).ToLowerInvariant())
    if([IO.Path]::GetFullPath($source) -ne [IO.Path]::GetFullPath($target)){Copy-Item -LiteralPath $source -Destination $target -Force}
    $image=[System.Drawing.Image]::FromFile($target)
    $items.Add([ordered]@{id=('Illustrations/'+$subfolder+'/'+$id);category=('Illustrations/'+$subfolder);file=(Relative $target);source=(Relative $target);width=$image.Width;height=$image.Height;border=@(0,0,0,0);kind='illustration';origin=$origin;prompt=$prompt;usage='Illustration indépendante ; masquer ou recadrer dans un conteneur UI, sans texte préimprimé.'})
    $image.Dispose()
}
$generated=Get-Content (Join-Path $PSScriptRoot 'art-generation.json') -Raw | ConvertFrom-Json
foreach($art in $generated){$source=$art.path;if(!(Test-Path -LiteralPath $source)){$source=Join-Path $destination ('Illustrations/Generated/'+$art.id+'.png')};if(!(Test-Path -LiteralPath $source)){throw "Image générée absente : $($art.id)"}; Add-Art $source 'Generated' $art.id 'imagegen intégré — 5 novembre 2026' $art.prompt}
foreach($file in Get-ChildItem (Join-Path $project 'Assets/_Project/Resources/UI/AubeTactique') -Filter '*.png' | Sort-Object Name){Add-Art $file.FullName 'Production' $file.BaseName 'Production Aube tactique précédente'}
foreach($category in @('Agents','Characters','Cards','Items')) {
    foreach($file in Get-ChildItem (Join-Path $project ('Assets/_Project/Resources/Illustrations/'+$category)) -File | Where-Object {$_.Extension -in @('.jpg','.png')} | Sort-Object Name){Add-Art $file.FullName $category $file.BaseName 'Illustration originale du projet — pixels conservés'}
}
$fontFolder=Join-Path $destination 'Fonts';New-Item -ItemType Directory -Force -Path $fontFolder | Out-Null
foreach($file in Get-ChildItem (Join-Path $project 'Assets/_Project/Resources/Fonts') -File | Where-Object {$_.Extension -eq '.ttf' -or $_.Name -like '*OFL*.txt'}) {
    $target=Join-Path $fontFolder $file.Name;Copy-Item -LiteralPath $file.FullName -Destination $target -Force
    $items.Add([ordered]@{id=('Fonts/'+$file.BaseName);category='Fonts';file=(Relative $target);source=(Relative $file.FullName);width=0;height=0;border=@(0,0,0,0);kind='font-or-license';usage='Police et licence originales du projet.'})
}

# Points de cadrage relevés dans les illustrations originales, pas de visages régénérés.
$focus=@{
 ada_okonkwo=@(.46,.83);agent_d=@(.46,.82);agent_m=@(.47,.80);amara_hughes=@(.50,.75)
 anna_petrov=@(.84,.80);astrid_hallstrom=@(.43,.83);celeste_moreau=@(.43,.92);clement_lefevre=@(.46,.80)
 darius_brooks=@(.42,.64);eero_nieminen=@(.50,.90);fatou_diallo=@(.40,.78);freya_lindqvist=@(.50,.77)
 giulia_rossi=@(.43,.80);haruto_nakamura=@(.46,.57);ilona_varga=@(.35,.82);ji_hoon_park=@(.53,.82)
 kwame_abara=@(.45,.80);louis_dubois=@(.46,.80);lucas_mendes=@(.60,.79);marco_bellini=@(.49,.83)
 mateo_vasquez=@(.50,.87);mei_lin_chen=@(.54,.86);noah_kaplan=@(.30,.65);odile_ferrand=@(.52,.84)
 rin_takeda=@(.46,.76);sam_okafor=@(.43,.76);severine_morel=@(.41,.91);tunde_oyelaran=@(.32,.73)
 victor_sorel=@(.55,.91);yelena_ivanova=@(.37,.85);youssef_haddad=@(.50,.86);zofia_kowalczyk=@(.42,.89)
}
$portraitFolder=Join-Path $destination 'Portraits';$portraitSources=Join-Path $PSScriptRoot 'Sources/Portraits'
New-Item -ItemType Directory -Force -Path $portraitFolder,$portraitSources | Out-Null
foreach($name in ($focus.Keys | Sort-Object)) {
    $source=Join-Path $destination ('Illustrations/Agents/'+$name+'.jpg')
    if(!(Test-Path -LiteralPath $source)){throw "Portrait original manquant : $name"}
    $image=[System.Drawing.Image]::FromFile($source);$w=$image.Width;$h=$image.Height;$image.Dispose()
    $base64=[Convert]::ToBase64String([IO.File]::ReadAllBytes($source))
    foreach($variant in @('head','bust')) {
        $cw=$w*$(if($variant -eq 'head'){.25}else{.30});$ch=if($variant -eq 'head'){$cw}else{$cw*1.5}
        $cx=[math]::Max(0,[math]::Min($w-$cw,$w*$focus[$name][0]-.5*$cw))
        $cy=[math]::Max(0,[math]::Min($h-$ch,$h*$focus[$name][1]-$ch*$(if($variant -eq 'head'){.5}else{.74})))
        $top=$h-$cy-$ch
        $svg=Join-Path $portraitSources ($name+'-'+$variant+'.svg')
        [IO.File]::WriteAllText($svg,"<svg xmlns=`"http://www.w3.org/2000/svg`" width=`"$cw`" height=`"$ch`" viewBox=`"$cx $top $cw $ch`"><image width=`"$w`" height=`"$h`" href=`"data:image/jpeg;base64,$base64`"/></svg>")
        $target=Join-Path $portraitFolder ($name+'-'+$variant+'.asset')
        $items.Add([ordered]@{id=('Portraits/'+$name+'-'+$variant);category='Portraits';file=(Relative $target);source=(Relative $svg);preview=(Relative $svg);original=(Relative $source);focusX=$focus[$name][0];focusY=$focus[$name][1];variant=$variant;width=[int]$cw;height=[int]$ch;border=@(0,0,0,0);kind='portrait-sprite';usage='Sprite Unity de cadrage sur texture originale ; aucun visage repeint ni fichier original modifié.'})
    }
}

$boardNames=@('01-entree','02-compte','03-qg-gestion','04-equipe-agents','05-collections','06-mission','07-plateau','08-combat','09-narration-resultats','10-boutique','11-recompenses','12-progression-social','13-tournoi-tutoriel','14-reglages-a','15-reglages-b','16-fenetres','17-minijeux-a','18-minijeux-b','19-activites','20-heros-etats','21-complements')
$artFor=@{
 '01-entree'=@('hq-new-york','background-paris-supernova')
 '02-compte'=@('hq-new-york')
 '03-qg-gestion'=@('hq-new-york','hq-moscow','room-workshop','room-operations','room-grimoire','room-inventory','room-register')
 '04-equipe-agents'=@('hq-new-york')
 '05-collections'=@('room-grimoire','room-inventory')
 '06-mission'=@('background-map-new-york','combat-red-hook','hq-moscow')
 '07-plateau'=@('background-board-red-hook')
 '08-combat'=@('combat-red-hook','enemy-brick','enemy-bridge-titan','enemy-thug','enemy-brute')
 '09-narration-resultats'=@('combat-red-hook','hq-new-york')
 '10-boutique'=@('hq-new-york','shop-pack-operation','shop-pack-mission','shop-gift-daily','shop-gems-small','shop-gems-medium','shop-gems-large','shop-credits-small','shop-credits-medium','shop-credits-large')
 '11-recompenses'=@('shop-booster','hq-new-york')
 '12-progression-social'=@('hq-new-york','shop-gift-daily')
 '13-tournoi-tutoriel'=@('hq-new-york','combat-red-hook')
 '14-reglages-a'=@('hq-new-york')
 '15-reglages-b'=@('hq-new-york')
 '16-fenetres'=@('hq-new-york')
 '17-minijeux-a'=@('mini-dna_analysis','mini-infiltration','mini-gears','mini-snow_survival')
 '18-minijeux-b'=@('mini-rhythm','mini-free_throw','mini-rescue_engineering','mini-crate_pick')
 '19-activites'=@('background-activity-potion','background-activity-arena','background-activity-fishing')
 '20-heros-etats'=@('background-hero-bulwark','background-hero-rooftops','hq-new-york')
 '21-complements'=@('hq-new-york','background-board-red-hook')
}
$coverage=@();$common=@('Surfaces','Buttons','Icons','IconsGold','Frames','Overlays','Brand')
$referenceHtml=Get-Content (Join-Path $PSScriptRoot '../index.html') -Raw
foreach($board in $boardNames) {
    $art=@();foreach($key in $artFor[$board]){$match=@($items | Where-Object {[IO.Path]::GetFileNameWithoutExtension($_.file) -eq $key -and $_.kind -eq 'illustration'});if($match.Count -ne 1){throw "Illustration de planche non résolue : $board / $key"};$art+=$match[0].id}
    $section=[regex]::Match($referenceHtml,('<section id="'+[regex]::Escape($board)+'">(.*?)</section>'),[System.Text.RegularExpressions.RegexOptions]::Singleline)
    $descriptions=[regex]::Matches($section.Value,'<li>(.*?)</li>',[System.Text.RegularExpressions.RegexOptions]::Singleline)
    if($descriptions.Count -ne 4){throw "La planche $board ne contient pas ses quatre vues dans le HTML"}
    $views=@();for($v=0;$v -lt 4;$v++){$description=[Net.WebUtility]::HtmlDecode([regex]::Replace($descriptions[$v].Groups[1].Value,'<[^>]+>',''));$views+=[ordered]@{index=($v+1);description=$description;sharedGroups=$common;illustrations=$art}}
    $coverage+=[ordered]@{board=$board;reference=('03_Art_UI_UX/Aube_Tactique_2026-11-04/'+$board+'.png');viewCount=4;views=$views;sharedGroups=$common;illustrations=$art;additionalGroups=@('Controls','Indicators','Inputs','Economy','Tabs','Portraits','Illustrations/Agents','Illustrations/Cards','Illustrations/Items','Decorations','Board','Masks','FX')}
}
$editable=Join-Path $destination 'Sources'
New-Item -ItemType Directory -Force -Path $editable | Out-Null
Copy-Item -Path (Join-Path $PSScriptRoot 'Sources/*') -Destination $editable -Recurse -Force
foreach($item in $items) {
    if($item.kind -eq 'vector-ui' -or $item.kind -eq 'portrait-sprite') {
        $sourceAbsolute=Join-Path $Workspace $item.source
        $within=[IO.Path]::GetRelativePath((Join-Path $PSScriptRoot 'Sources'),$sourceAbsolute)
        $item.source=Relative (Join-Path $editable $within)
        if($item.kind -eq 'portrait-sprite'){$item.preview=$item.source}
    }
}
$manifest=[ordered]@{name='Heroes Dawn — Kit complet Aube tactique';version='1.0';date='2026-11-05';reference=$native.reference;palette=$native.palette;count=$items.Count;items=$items.ToArray();screenCoverage=$coverage;integration='Assets seulement : aucun prefab ni comportement de menu modifié par ce kit.'}
$json=$manifest | ConvertTo-Json -Depth 12
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'manifest.json'),$json)
[IO.File]::WriteAllText((Join-Path $destination 'manifest.json'),$json)
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'coverage.json'),($coverage | ConvertTo-Json -Depth 8))
Write-Output "Kit assemblé : $($items.Count) entrées ; $($coverage.Count) planches, 84 vues couvertes."
