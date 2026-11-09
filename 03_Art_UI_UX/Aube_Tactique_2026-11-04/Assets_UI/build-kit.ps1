param([string]$Workspace = 'C:/Users/dnath/Documents/Heroes Dawn')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
[System.Threading.Thread]::CurrentThread.CurrentCulture = [System.Globalization.CultureInfo]::InvariantCulture
$kitRoot = Join-Path $Workspace 'Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueKit'
$kitSources = Join-Path $PSScriptRoot 'Sources'
$kitRecords = [System.Collections.Generic.List[object]]::new()
$ink = '#101C29'; $ivory = '#ECE7DC'; $brass = '#D6AF5B'; $edge = '#78858B'; $surface = '#1C2D3B'
function Kit-Color([string]$hex, [int]$alpha=255) {
    $c = [System.Drawing.ColorTranslator]::FromHtml($hex)
    return [System.Drawing.Color]::FromArgb($alpha,$c.R,$c.G,$c.B)
}
function Kit-New($category,$name,[int]$width,[int]$height,$border=@(0,0,0,0),[int]$viewWidth=0,[int]$viewHeight=0) {
    $script:kitCategory=$category; $script:kitName=$name; $script:kitBorder=$border
    $script:kitWidth=$width; $script:kitHeight=$height
    if($viewWidth -eq 0){$viewWidth=$width}; if($viewHeight -eq 0){$viewHeight=$height}
    $script:kitBitmap=[System.Drawing.Bitmap]::new($width,$height,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $script:kitGraphics=[System.Drawing.Graphics]::FromImage($script:kitBitmap)
    $script:kitGraphics.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $script:kitGraphics.CompositingQuality=[System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $script:kitGraphics.Clear([System.Drawing.Color]::Transparent)
    $script:kitGraphics.ScaleTransform($width/[single]$viewWidth,$height/[single]$viewHeight)
    $script:kitSvg=[System.Text.StringBuilder]::new()
    [void]$script:kitSvg.AppendLine("<svg xmlns=`"http://www.w3.org/2000/svg`" width=`"$width`" height=`"$height`" viewBox=`"0 0 $viewWidth $viewHeight`">")
}
function Kit-Polyline([float[]]$xy,$color=$ivory,[single]$weight=2.2,[bool]$close=$false) {
    $points=[System.Collections.Generic.List[System.Drawing.PointF]]::new()
    $pairs=[System.Collections.Generic.List[string]]::new()
    for($i=0;$i -lt $xy.Length;$i+=2){$points.Add([System.Drawing.PointF]::new($xy[$i],$xy[$i+1]));$pairs.Add("$($xy[$i]),$($xy[$i+1])")}
    $pen=[System.Drawing.Pen]::new((Kit-Color $color),$weight)
    $pen.StartCap=$pen.EndCap=[System.Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin=[System.Drawing.Drawing2D.LineJoin]::Round
    if($close){$script:kitGraphics.DrawPolygon($pen,$points.ToArray())}else{$script:kitGraphics.DrawLines($pen,$points.ToArray())}
    $tag=if($close){'polygon'}else{'polyline'}
    [void]$script:kitSvg.AppendLine("<$tag points=`"$($pairs -join ' ')`" fill=`"none`" stroke=`"$color`" stroke-width=`"$weight`" stroke-linejoin=`"round`" stroke-linecap=`"round`"/>")
    $pen.Dispose()
}
function Kit-Poly([float[]]$xy,$color,[int]$alpha=255) {
    $points=[System.Collections.Generic.List[System.Drawing.PointF]]::new();$pairs=[System.Collections.Generic.List[string]]::new()
    for($i=0;$i -lt $xy.Length;$i+=2){$points.Add([System.Drawing.PointF]::new($xy[$i],$xy[$i+1]));$pairs.Add("$($xy[$i]),$($xy[$i+1])")}
    $brush=[System.Drawing.SolidBrush]::new((Kit-Color $color $alpha));$script:kitGraphics.FillPolygon($brush,$points.ToArray());$brush.Dispose()
    [void]$script:kitSvg.AppendLine("<polygon points=`"$($pairs -join ' ')`" fill=`"$color`" fill-opacity=`"$($alpha/255.0)`"/>")
}
function Kit-Circle([single]$x,[single]$y,[single]$r,$stroke=$ivory,[single]$weight=2.2,$fill='') {
    if($fill){$b=[System.Drawing.SolidBrush]::new((Kit-Color $fill));$script:kitGraphics.FillEllipse($b,$x-$r,$y-$r,2*$r,2*$r);$b.Dispose()}
    if($stroke){$p=[System.Drawing.Pen]::new((Kit-Color $stroke),$weight);$script:kitGraphics.DrawEllipse($p,$x-$r,$y-$r,2*$r,2*$r);$p.Dispose()}
    $f=if($fill){$fill}else{'none'};$s=if($stroke){$stroke}else{'none'}
    [void]$script:kitSvg.AppendLine("<circle cx=`"$x`" cy=`"$y`" r=`"$r`" fill=`"$f`" stroke=`"$s`" stroke-width=`"$weight`"/>")
}
function Kit-Rect($x,$y,$w,$h,$color=$ivory,$weight=2.2){Kit-Polyline @($x,$y,($x+$w),$y,($x+$w),($y+$h),$x,($y+$h)) $color $weight $true}
function Kit-Star($x,$y,$radius,$color=$brass){$q=$radius*.19; Kit-Poly @($x,($y-$radius),($x+$q),($y-$q),($x+$radius),$y,($x+$q),($y+$q),$x,($y+$radius),($x-$q),($y+$q),($x-$radius),$y,($x-$q),($y-$q)) $color}
function Kit-Chamfer($x,$y,$w,$h,$cut,$color,$weight=0,[int]$alpha=255) {
    $xy=@($x,$y,($x+$w-$cut),$y,($x+$w),($y+$cut),($x+$w),($y+$h),$x,($y+$h))
    if($weight -gt 0){Kit-Polyline $xy $color $weight $true}else{Kit-Poly $xy $color $alpha}
}
function Kit-Save($usage='') {
    $dir=Join-Path $kitRoot $script:kitCategory; $src=Join-Path $kitSources $script:kitCategory
    [void](New-Item -ItemType Directory -Force -Path $dir,$src)
    $png=Join-Path $dir ($script:kitName+'.png');$svg=Join-Path $src ($script:kitName+'.svg')
    $script:kitBitmap.Save($png,[System.Drawing.Imaging.ImageFormat]::Png)
    [void]$script:kitSvg.AppendLine('</svg>');[System.IO.File]::WriteAllText($svg,$script:kitSvg.ToString())
    $kitRecords.Add([ordered]@{id=($script:kitCategory+'/'+$script:kitName);category=$script:kitCategory;file=($png.Substring($Workspace.Length+1).Replace('\','/'));source=($svg.Substring($Workspace.Length+1).Replace('\','/'));width=$script:kitWidth;height=$script:kitHeight;border=$script:kitBorder;usage=$usage;kind='vector-ui';alpha=$true})
    $script:kitGraphics.Dispose();$script:kitBitmap.Dispose()
}

# Surfaces extensibles, sans texte ni contrôles intégrés.
$panels=@{navy=$ink;surface=$surface;raised='#263C4A';ivory=$ivory;muted='#344652';danger='#401F2B'}
foreach($name in ($panels.Keys | Sort-Object)) {
    Kit-New 'Surfaces' ('panel-'+$name) 1024 512 @(40,32,40,40)
    Kit-Chamfer 3 3 1018 506 28 $panels[$name]
    Kit-Chamfer 3 3 1018 506 28 $edge 1.6
    Kit-Polyline @(26,10,180,10) $brass 1.5
    Kit-Save 'Panneau 9-slice ; conserver les marges et le coin supérieur droit coupé.'
}
foreach($name in @('window','drawer','tooltip','toast','tutorial','header','footer')) {
    $h=if($name -in @('header','footer','toast')){192}else{512}
    Kit-New 'Surfaces' ('surface-'+$name) 1024 $h @(40,32,40,40)
    Kit-Chamfer 3 3 1018 ($h-6) 28 $ink
    Kit-Chamfer 3 3 1018 ($h-6) 28 $brass 1.8
    Kit-Chamfer 10 10 1004 ($h-20) 22 $edge .7
    Kit-Save 'Conteneur 9-slice ; titre, texte et actions restent des éléments séparés.'
}

$styles=@{primary=$ivory;secondary=$surface;danger='#A33443';nav=$ivory;ghost=$ink}
foreach($style in ($styles.Keys | Sort-Object)) {foreach($state in @('normal','hover','pressed','disabled','focus')) {
    Kit-New 'Buttons' ("button-$style-$state") 512 144 @(40,24,40,40)
    $fill=$styles[$style]; if($state -eq 'pressed'){$fill=if($style -in @('primary','nav')){'#D3CDBE'}else{'#101C29'}}
    if($state -eq 'hover'){$fill=if($style -in @('primary','nav')){'#FFF7E4'}else{'#304856'}}
    if($state -eq 'disabled'){$fill='#34414B'}
    $rim=if($state -eq 'focus'){$brass}elseif($state -eq 'disabled'){'#59656D'}elseif($style -in @('primary','nav')){$brass}else{$edge}
    if($state -eq 'focus'){Kit-Chamfer 2 2 508 140 27 $brass 3}
    Kit-Chamfer 6 6 500 132 24 $fill
    Kit-Chamfer 6 6 500 132 24 $rim 1.6
    Kit-Polyline @(22,12,125,12) $rim 1
    Kit-Save 'Bouton 9-slice sans texte ; iconographie et label posés au-dessus. États normal, survol, appui, indisponible et focus.'
}}
foreach($state in @('active','inactive','disabled')) {foreach($dir in @('horizontal','vertical')) {
    Kit-New 'Tabs' ("tab-$dir-$state") 512 144 @(40,24,40,40)
    $fill=if($state -eq 'active'){$ivory}else{$ink};$rim=if($state -eq 'active'){$brass}else{$edge}
    Kit-Chamfer 3 3 506 138 24 $fill;Kit-Chamfer 3 3 506 138 24 $rim 1.6
    if($state -eq 'active'){Kit-Polyline @(18,135,494,135) $brass 2}
    Kit-Save 'Onglet sélectionné, neutre ou indisponible ; découpe extensible.'
}}

$rarities=@{common='#ADB7B9';rare='#67C3DA';epic='#AE8BD3';legendary=$brass;mythic='#E5C884'}
$families=@{matter='#E4AE46';elemental='#65D5E5';bio='#E46B91';perception='#AB89CF'}
$frames=@{};foreach($k in $rarities.Keys){$frames['rarity-'+$k]=$rarities[$k]};foreach($k in $families.Keys){$frames['family-'+$k]=$families[$k]}
foreach($name in ($frames.Keys | Sort-Object)) {
    foreach($state in @('normal','selected','locked')) {
        Kit-New 'Frames' ("card-$name-$state") 512 768 @(40,56,40,40)
        $c=if($state -eq 'locked'){$edge}else{$frames[$name]}
        Kit-Chamfer 5 5 502 758 30 $c 3
        Kit-Chamfer 13 13 486 742 23 $c 1
        if($state -eq 'selected'){Kit-Chamfer 2 2 508 764 32 $brass 3;Kit-Polyline @(28,18,130,18) $ivory 2}
        Kit-Save 'Cadre de carte transparent, rareté ou famille ; illustration et bandeau de nom séparés.'
    }
}
foreach($name in @('ally','enemy','boss','selected','empty')+$families.Keys) {
    Kit-New 'Frames' ('portrait-'+$name) 256 256
    $c=if($families.ContainsKey($name)){$families[$name]}elseif($name -in @('enemy','boss')){'#BF5263'}elseif($name -eq 'empty'){$edge}else{$brass}
    Kit-Circle 128 128 119 $c 5; Kit-Circle 128 128 111 $ivory 1
    Kit-Save 'Anneau transparent de portrait ; masque circulaire indépendant.'
}
foreach($shape in @('card-caption','card-caption-small','card-back','nameplate','dossier-cover')) {
    Kit-New 'Cards' $shape 512 256 @(32,24,32,32)
    Kit-Chamfer 2 2 508 252 24 $ink;Kit-Chamfer 2 2 508 252 24 $brass 1.5
    if($shape -in @('card-back','dossier-cover')){Kit-Star 256 128 68 $brass}
    Kit-Save 'Habillage de carte séparé ; le nom, la quantité et la description restent localisables.'
}

foreach($name in @('health','energy','fatigue','xp','loading','danger','neutral')) {
    $c=switch($name){health{'#58B47A'} energy{'#86C0D0'} fatigue{'#C89950'} danger{'#B64254'} neutral{$ivory} default{$brass}}
    Kit-New 'Controls' ('bar-'+$name+'-fill') 512 32 @(8,8,8,8)
    Kit-Chamfer 1 1 510 30 8 $c;Kit-Save 'Remplissage de jauge ; ancrer à gauche, ne pas étirer le cadre avec la valeur.'
}
Kit-New 'Controls' 'bar-track' 512 40 @(12,12,12,12);Kit-Chamfer 1 1 510 38 8 $ink;Kit-Chamfer 1 1 510 38 8 $edge 1.5;Kit-Save 'Piste de jauge séparée.'
foreach($on in @('on','off','disabled')) {
    Kit-New 'Controls' ('toggle-'+$on) 192 96
    $c=if($on -eq 'on'){$brass}else{$surface};Kit-Chamfer 3 3 186 90 16 $c;Kit-Chamfer 3 3 186 90 16 $edge 1.5
    $x=if($on -eq 'on'){144}else{48};Kit-Circle $x 48 34 $ivory 1 $ivory;Kit-Save 'Interrupteur à deux positions ; état visuel complet, sans libellé.'
}
foreach($kind in @('checkbox-empty','checkbox-checked','radio-empty','radio-selected','slider-thumb')) {
    Kit-New 'Controls' $kind 96 96
    if($kind.StartsWith('checkbox')){Kit-Chamfer 12 12 72 72 12 $ink;Kit-Chamfer 12 12 72 72 12 $brass 2;if($kind.EndsWith('checked')){Kit-Polyline @(26,48,42,64,72,30) $ivory 5}}
    else{Kit-Circle 48 48 34 $brass 2 $ink;if($kind -ne 'radio-empty'){Kit-Circle 48 48 21 '' 0 $ivory}}
    Kit-Save 'Contrôle transparent ; conserver une cible tactile supérieure au seul visuel.'
}
foreach($name in @('danger','gold','navy','success')) {
    Kit-New 'Indicators' ('badge-'+$name) 96 96
    $c=switch($name){danger{'#A33443'} gold{$brass} success{'#3D8F71'} default{$ink}}
    Kit-Circle 48 48 43 '' 0 $c;Kit-Save 'Fond de badge ; chiffre ou libellé indépendant.'
}
foreach($name in @('timeline-unit','timeline-ready','timeline-danger','target-ring','selection-corners','route-node','route-node-active','route-node-locked')) {
    Kit-New 'Indicators' $name 192 192
    if($name -like 'timeline-*'){Kit-Poly @(96,13,179,96,96,179,13,96) $ink;Kit-Polyline @(96,13,179,96,96,179,13,96) $(if($name -eq 'timeline-danger'){'#A33443'}else{$brass}) 4 $true;if($name -eq 'timeline-ready'){Kit-Circle 96 96 89 $ivory 4}}
    elseif($name -eq 'selection-corners'){foreach($p in @(@(12,52,12,12,52,12),@(140,12,180,12,180,52),@(180,140,180,180,140,180),@(52,180,12,180,12,140))){Kit-Polyline $p $brass 4}}
    else{Kit-Circle 96 96 78 $brass 3;if($name -eq 'route-node-active'){Kit-Circle 96 96 88 $ivory 4};if($name -eq 'route-node-locked'){Kit-Circle 96 96 66 '' 0 $surface}}
    Kit-Save 'Indicateur transparent de chronologie, de ciblage ou de déplacement.'
}
for($i=0;$i -lt 12;$i++) {
    Kit-New 'Indicators' ('spinner-'+$i.ToString('00')) 128 128
    for($j=0;$j -lt 12;$j++){$a=$j*[math]::PI/6;$v=if($j -eq $i){$brass}else{$edge};Kit-Polyline @((64+35*[math]::Cos($a)),(64+35*[math]::Sin($a)),(64+50*[math]::Cos($a)),(64+50*[math]::Sin($a))) $v 5}
    Kit-Save 'Séquence de chargement 12 images ; aucune rotation de texte.'
}

# Pictogrammes : tracés monochromes teintables, jamais du texte Unicode dépendant d'une police.
function Kit-Glyph($name,$color='#FFFFFF',$category='Icons') {
    Kit-New $category $name 192 192 @(0,0,0,0) 96 96
    function S([float[]]$p){Kit-Polyline $p $color 2.3}
    function B($x,$y,$w,$h){Kit-Rect $x $y $w $h $color 2.3}
    function C($x,$y,$r){Kit-Circle $x $y $r $color 2.3}
    switch($name){
        dawn {Kit-Star 48 48 38 $color}
        back {S @(60,18,30,48,60,78)} forward {S @(36,18,66,48,36,78)}
        up {S @(18,60,48,30,78,60)} down {S @(18,36,48,66,78,36)}
        close {S @(24,24,72,72);S @(72,24,24,72)} check {S @(18,48,38,68,80,26)}
        plus {S @(18,48,78,48);S @(48,18,48,78)} minus {S @(18,48,78,48)}
        menu {S @(18,26,78,26);S @(18,48,78,48);S @(18,70,78,70)}
        home {S @(14,43,48,15,82,43);S @(23,39,23,80,42,80,42,58,56,58,56,80,73,80,73,39)}
        team {C 32 30 10;C 65 30 10;S @(14,78,14,58,23,48,41,48,49,59,57,48,75,48,83,58,83,78,14,78)}
        profile {C 48 30 14;S @(20,80,20,64,33,48,63,48,76,64,76,80,20,80)}
        pass {B 16 24 64 48;S @(34,24,34,72);S @(62,24,62,72);S @(42,48,54,48)}
        shop {B 20 33 56 48;S @(30,33,35,17,61,17,66,33);S @(38,53,58,53)}
        booster {S @(24,15,70,15,76,81,18,81,24,15);S @(22,24,72,24);S @(19,72,75,72);Kit-Star 48 48 15 $color}
        gift {B 18 39 60 42;B 13 29 70 12;S @(48,29,48,81);C 35 22 10;C 61 22 10}
        inventory {S @(16,30,48,13,80,30,80,67,48,84,16,67,16,30,48,48,80,30);S @(48,48,48,84)}
        grimoire {S @(48,24,25,15,14,20,14,75,25,71,48,81,71,71,82,75,82,20,71,15,48,24,48,81)}
        workshop {C 23 74 7;S @(28,69,62,35,59,22,70,13,72,26,84,29,74,39,62,35);S @(17,68,55,30)}
        operations {C 48 48 26;C 48 48 7;S @(48,10,48,32);S @(48,64,48,86);S @(10,48,32,48);S @(64,48,86,48)}
        register {B 25 12 56 72;S @(15,27,34,27);S @(15,48,34,48);S @(15,69,34,69);C 54 34 9;S @(41,64,44,53,65,53,68,64)}
        quest {B 22 19 52 65;B 35 12 26 13;S @(32,47,42,57,63,35);S @(34,70,62,70)}
        map {S @(12,23,35,14,61,24,84,15,84,73,61,82,35,72,12,81,12,23);S @(35,14,35,72);S @(61,24,61,82)}
        location {C 48 35 10;S @(48,84,23,47,20,35,23,23,34,14,48,10,62,14,73,23,76,35,73,47,48,84)}
        flag {S @(23,84,23,13,74,13,64,34,74,53,23,53)}
        lock {B 23 41 50 39;S @(32,41,32,27,38,17,58,17,64,27,64,41);C 48 59 3;S @(48,62,48,70)}
        unlock {B 23 41 50 39;S @(33,41,33,26,39,17,59,17,67,28);C 48 59 3;S @(48,62,48,70)}
        info {C 48 48 33;C 48 30 1.4;S @(48,43,48,69)}
        warning {S @(48,12,86,80,10,80,48,12);S @(48,35,48,56);C 48 68 1.4}
        error {C 48 48 33;S @(34,34,62,62);S @(62,34,34,62)}
        help {C 48 48 33;S @(35,34,38,26,52,23,62,31,62,41,48,51,48,58);C 48 70 1.4}
        refresh {S @(78,39,72,25,55,16,35,18,21,32,17,49,23,67,39,79,59,78,74,66);S @(78,18,78,39,58,39)}
        copy {B 30 27 48 58;S @(62,27,62,12,15,12,15,70,30,70)}
        share {C 24 48 9;C 73 22 9;C 73 74 9;S @(32,44,65,26);S @(32,52,65,70)}
        search {C 41 40 25;S @(59,59,82,82)}
        sort {S @(23,16,23,80,13,69);S @(23,80,33,69);S @(44,23,81,23);S @(44,46,70,46);S @(44,69,58,69)}
        filter {S @(13,20,83,20,58,49,58,75,39,85,39,49,13,20)}
        calendar {B 15 23 66 60;S @(15,39,81,39);S @(32,13,32,31);S @(64,13,64,31);B 30 51 12 12;B 53 51 12 12}
        clock {C 48 48 34;S @(48,24,48,48,66,59)}
        play {Kit-Poly @(31,15,79,48,31,81) $color}
        pause {Kit-Poly @(25,19,38,19,38,77,25,77) $color;Kit-Poly @(58,19,71,19,71,77,58,77) $color}
        skip {S @(16,18,59,48,16,78,16,18);S @(73,18,73,78)}
        auto {S @(21,65,48,17,75,65);S @(33,49,63,49);S @(18,75,48,84,78,75)}
        speed {S @(15,22,41,48,15,74);S @(47,22,73,48,47,74)}
        retreat {B 17 15 29 66;S @(40,48,82,48);S @(65,31,82,48,65,65)}
        dice {B 17 17 62 62;foreach($p in @(@(32,32),@(64,32),@(48,48),@(32,64),@(64,64))){C $p[0] $p[1] 3}}
        energy {S @(54,10,24,53,46,53,39,86,75,41,52,41,54,10)}
        health {S @(48,80,17,51,12,35,19,21,32,17,48,31,64,17,77,21,84,35,79,51,48,80)}
        attack {S @(20,77,67,15,82,13,82,29,33,82,20,77);S @(18,55,44,78);S @(13,82,23,71)}
        defense {S @(48,12,77,23,74,56,65,72,48,85,31,72,22,56,19,23,48,12);S @(48,26,48,68)}
        fatigue {C 48 48 32;S @(30,38,39,38);S @(57,38,66,38);S @(31,65,39,58,57,58,65,65)}
        heal {B 17 17 62 62;S @(48,29,48,67);S @(29,48,67,48)}
        level {S @(15,67,48,34,81,67);S @(15,43,48,10,81,43);S @(48,44,48,85)}
        trophy {B 31 13 34 42;S @(31,22,13,22,16,43,32,49);S @(65,22,83,22,80,43,64,49);S @(48,55,48,77);S @(30,82,66,82)}
        friend {C 39 29 13;S @(15,80,15,65,27,50,51,50,60,62);S @(74,48,74,79);S @(59,64,89,64)}
        mail {B 13 24 70 50;S @(13,26,48,52,83,26)}
        password {C 30 39 17;S @(44,49,78,80,84,74,76,67,69,74);C 25 34 3}
        eye {S @(10,48,28,30,48,24,68,30,86,48,68,66,48,72,28,66,10,48);C 48 48 12}
        eye-off {S @(10,48,28,30,48,24,68,30,86,48,68,66,48,72,28,66,10,48);S @(14,14,82,82)}
        settings {C 48 48 28;C 48 48 11;for($a=0;$a -lt 8;$a++){$t=$a*[math]::PI/4;S @((48+29*[math]::Cos($t)),(48+29*[math]::Sin($t)),(48+39*[math]::Cos($t)),(48+39*[math]::Sin($t)))}}
        audio {S @(15,36,32,36,52,18,52,78,32,60,15,60,15,36);S @(64,31,73,41,73,55,64,65);S @(75,19,87,36,87,60,75,77)}
        music {S @(31,70,31,24,74,15,74,61);S @(31,35,74,26);C 22 73 9;C 65 64 9}
        voice {S @(35,20,39,13,57,13,61,20,61,49,55,58,41,58,35,49,35,20);S @(24,42,24,53,34,67,48,72,62,67,72,53,72,42);S @(48,72,48,84);S @(34,84,62,84)}
        graphics {B 12 18 72 48;S @(48,66,48,82);S @(29,82,67,82);S @(22,56,40,38,53,49,66,29,76,42)}
        accessibility {C 48 17 8;S @(17,34,48,42,79,34);S @(48,42,48,60,29,84);S @(48,60,67,84)}
        language {C 48 48 34;S @(14,48,82,48);S @(20,30,76,30);S @(20,66,76,66);S @(48,14,34,31,31,48,34,66,48,82,62,66,65,48,62,31,48,14)}
        bell {S @(18,69,25,60,25,37,32,23,48,17,64,23,71,37,71,60,78,69,18,69);S @(37,78,42,84,54,84,59,78)}
        controls {S @(18,28,78,28,88,72,74,79,58,65,38,65,22,79,8,72,18,28);S @(30,38,30,57);S @(20,48,40,48);C 66 42 3;C 76 53 3}
        account {C 48 29 13;S @(22,79,22,63,34,49,62,49,74,63,74,79);C 48 48 41}
        support {C 48 42 29;S @(19,42,19,67,31,67,31,44,19,44);S @(77,42,77,67,65,67,65,44,77,44);S @(77,63,69,80,51,84);C 46 84 4}
        weapon {S @(20,77,66,18,80,14,80,30,34,82,20,77);S @(15,57,43,79)}
        armor {S @(29,15,40,24,56,24,67,15,83,37,70,49,65,42,65,82,31,82,31,42,26,49,13,37,29,15)}
        tool {S @(18,77,69,26);S @(52,20,61,12,82,26,73,37,52,20);S @(13,72,23,82)}
        implant {B 26 26 44 44;B 38 38 20 20;foreach($v in @(35,48,61)){S @($v,14,$v,26);S @($v,70,$v,82);S @(14,$v,26,$v);S @(70,$v,82,$v)}}
        core {S @(48,10,79,29,79,67,48,86,17,67,17,29,48,10);S @(48,24,65,35,65,61,48,72,31,61,31,35,48,24)}
        credits {C 48 48 34;S @(62,31,50,25,35,31,29,46,34,63,49,70,63,64)}
        pr {C 48 48 34;S @(31,70,31,28,51,28,61,35,61,46,51,52,31,52);S @(49,52,65,70)}
        gems {S @(24,22,72,22,86,41,48,82,10,41,24,22);S @(10,41,86,41);S @(35,22,28,41,48,82,68,41,61,22)}
        family-matter {S @(48,10,81,29,81,67,48,86,15,67,15,29,48,10);S @(30,60,30,43,38,39,38,30,46,29,50,36,58,32,65,38,65,59,54,69,40,69,30,60)}
        family-elemental {S @(48,10,81,29,81,67,48,86,15,67,15,29,48,10);S @(53,23,32,51,48,51,43,74,66,43,51,43,53,23)}
        family-bio {S @(48,10,81,29,81,67,48,86,15,67,15,29,48,10);C 48 32 8;C 33 59 8;C 63 59 8;S @(44,39,37,52);S @(52,39,59,52);S @(41,59,55,59)}
        family-perception {S @(48,10,81,29,81,67,48,86,15,67,15,29,48,10);$points=@();for($i=0;$i -le 42;$i++){$t=$i*.22;$r=2+$i*.46;$points+=48+$r*[math]::Cos($t);$points+=48+$r*[math]::Sin($t)};S $points}
        fragment {S @(39,12,66,19,58,38,76,50,57,83,24,69,34,49,19,38,39,12);S @(34,49,58,38);S @(39,12,34,49,57,83)}
        link {S @(40,31,51,20,66,18,77,27,77,42,65,55);S @(55,65,43,77,28,78,18,67,20,52,31,41);S @(34,62,62,34)}
        cloud {S @(24,70,16,67,11,54,16,43,29,39,33,25,46,18,60,21,69,34,81,38,87,50,84,63,73,70,24,70)}
        save {S @(18,14,67,14,80,27,80,82,18,82,18,14);B 31 14 30 25;B 30 55 38 27}
        logout {B 15 15 33 66;S @(37,48,83,48);S @(66,30,83,48,66,66)}
        document {S @(22,12,59,12,77,30,77,84,22,84,22,12);S @(59,12,59,30,77,30);S @(33,46,65,46);S @(33,59,65,59);S @(33,72,56,72)}
        chapter {B 22 13 53 70;S @(34,13,34,83);S @(46,32,64,32);S @(46,45,64,45);S @(46,58,59,58)}
        dialogue {S @(14,17,82,17,82,65,44,65,24,83,24,65,14,65,14,17);C 31 42 2;C 48 42 2;C 65 42 2}
        memory {B 17 18 62 63;S @(22,72,40,50,51,61,64,43,75,57);C 37 34 6}
        dna {S @(27,12,27,24,39,38,58,49,69,64,69,84);S @(69,12,69,24,57,38,38,49,27,64,27,84);S @(28,20,68,20);S @(36,35,60,35);S @(35,60,61,60);S @(28,76,68,76)}
        rhythm {S @(14,48,25,48,34,20,47,78,61,32,69,48,82,48)}
        basketball {C 48 48 34;S @(14,48,82,48);S @(48,14,48,82);S @(25,22,36,34,38,48,36,62,25,74);S @(71,22,60,34,58,48,60,62,71,74)}
        snow {S @(48,12,48,84);S @(17,30,79,66);S @(17,66,79,30);S @(36,18,48,30,60,18);S @(36,78,48,66,60,78)}
        fishing {S @(20,80,73,15,81,19,38,72);S @(73,15,72,57,67,69,58,75,48,71,43,61)}
        rescue {B 15 27 66 54;S @(33,27,33,15,63,15,63,27);S @(48,41,48,67);S @(35,54,61,54)}
        crates {B 15 20 66 62;S @(15,20,81,82);S @(81,20,15,82);B 25 30 46 42}
        run {C 60 17 7;S @(49,31,64,32,74,45,87,45);S @(49,31,36,47,22,47);S @(54,38,46,59,68,73,75,85);S @(46,59,30,77,15,77)}
        arena {S @(15,27,48,12,81,27,81,69,48,84,15,69,15,27);S @(29,38,67,65);S @(67,38,29,65)}
        bulwark {S @(15,80,15,25,29,25,29,37,41,37,41,25,55,25,55,37,67,37,67,25,81,25,81,80,15,80);S @(39,80,39,59,48,50,57,59,57,80)}
        boss {S @(17,29,31,47,48,17,65,47,79,29,73,77,23,77,17,29);S @(24,66,72,66)}
        default {throw "Pictogramme non dessiné : $name"}
    }
    Kit-Save 'Pictogramme blanc teintable, tracé vectoriel indépendant des polices ; repère sémantique dans son nom.'
}
$glyphs=@('dawn','back','forward','up','down','close','check','plus','minus','menu','home','team','profile','pass','shop','booster','gift','inventory','grimoire','workshop','operations','register','quest','map','location','flag','lock','unlock','info','warning','error','help','refresh','copy','share','search','sort','filter','calendar','clock','play','pause','skip','auto','speed','retreat','dice','energy','health','attack','defense','fatigue','heal','level','trophy','friend','mail','password','eye','eye-off','settings','audio','music','voice','graphics','accessibility','language','bell','controls','account','support','weapon','armor','tool','implant','core','credits','pr','gems','family-matter','family-elemental','family-bio','family-perception','fragment','link','cloud','save','logout','document','chapter','dialogue','memory','dna','rhythm','basketball','snow','fishing','rescue','crates','run','arena','bulwark','boss')
foreach($glyph in $glyphs){Kit-Glyph $glyph}
foreach($glyph in @('dawn','home','team','profile','pass','shop','booster','gift','inventory','grimoire','workshop','operations','register','quest','settings','trophy')){Kit-Glyph $glyph $brass 'IconsGold'}
foreach($family in $families.Keys){Kit-Glyph ('family-'+$family) $families[$family] 'IconsColor'}
foreach($pair in @(@('credits','#D6AF5B'),@('pr','#65D3AF'),@('gems','#AB89CF'),@('health','#58B47A'),@('energy','#86C0D0'),@('warning','#D6AF5B'),@('error','#BF5263'))){Kit-Glyph $pair[0] $pair[1] 'IconsColor'}

foreach($kind in @('divider-star','divider-simple','accent-line','corner-ornament','pointer-up','pointer-down','pointer-left','pointer-right')) {
    if($kind.StartsWith('pointer')){Kit-New 'Decorations' $kind 96 96}else{Kit-New 'Decorations' $kind 512 96}
    switch($kind){
        divider-star {Kit-Polyline @(8,48,216,48) $brass 1.5;Kit-Star 256 48 28 $brass;Kit-Polyline @(296,48,504,48) $brass 1.5}
        divider-simple {Kit-Polyline @(8,48,504,48) $edge 1}
        accent-line {Kit-Polyline @(8,48,504,48) $brass 3}
        corner-ornament {Kit-Polyline @(8,84,8,12,440,12,500,72) $brass 2}
        pointer-up {Kit-Poly @(22,78,48,20,74,78) $surface}
        pointer-down {Kit-Poly @(22,20,48,78,74,20) $surface}
        pointer-left {Kit-Poly @(74,18,16,48,74,78) $surface}
        pointer-right {Kit-Poly @(22,18,80,48,22,78) $surface}
    }
    Kit-Save 'Ornement transparent ; ne pas inclure le texte dans le sprite.'
}

# Mot-symbole : la marque uniquement, aucun texte de navigation rasterisé.
$fontPath=Join-Path $Workspace 'Code/Heroes Dawn Unity/Assets/_Project/Resources/Fonts/BarlowCondensed-ExtraBoldItalic.ttf'
$fonts=[System.Drawing.Text.PrivateFontCollection]::new();$fonts.AddFontFile($fontPath)
foreach($variant in @('ivory','gold','navy')) {
    $c=switch($variant){ivory{$ivory} gold{$brass} default{$ink}}
    Kit-New 'Brand' ('heroes-dawn-'+$variant) 1200 180
    $font=[System.Drawing.Font]::new($fonts.Families[0],126,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Pixel)
    $brush=[System.Drawing.SolidBrush]::new((Kit-Color $c));$script:kitGraphics.TextRenderingHint=[System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $heroWidth=$script:kitGraphics.MeasureString('HEROES',$font).Width
    $dawnWidth=$script:kitGraphics.MeasureString('DAWN',$font).Width
    $start=(1200-$heroWidth-$dawnWidth-150)/2
    $starX=$start+$heroWidth+65; $dawnX=$start+$heroWidth+150
    $script:kitGraphics.DrawString('HEROES',$font,$brush,[single]$start,6);$script:kitGraphics.DrawString('DAWN',$font,$brush,[single]$dawnX,6)
    Kit-Star $starX 90 58 $c
    [void]$script:kitSvg.AppendLine("<text x=`"$start`" y=`"135`" font-family=`"Barlow Condensed`" font-size=`"126`" font-weight=`"800`" font-style=`"italic`" fill=`"$c`">HEROES</text><text x=`"$dawnX`" y=`"135`" font-family=`"Barlow Condensed`" font-size=`"126`" font-weight=`"800`" font-style=`"italic`" fill=`"$c`">DAWN</text>")
    $font.Dispose();$brush.Dispose();Kit-Save 'Marque Heroes Dawn avec étoile à quatre branches ; police Barlow Condensed sous licence OFL dans le projet.'
}
$fonts.Dispose()
foreach($state in @('normal','focus','error','disabled')) {
    Kit-New 'Inputs' ('field-'+$state) 768 144 @(32,24,32,32)
    $rim=switch($state){focus{$brass} error{'#BF5263'} disabled{'#4C5C65'} default{$edge}}
    Kit-Chamfer 3 3 762 138 20 $ink;Kit-Chamfer 3 3 762 138 20 $rim 2
    Kit-Save 'Champ de saisie 9-slice ; texte, placeholder et icône restent séparés.'
}
foreach($name in @('resource-pill','price-ribbon','quantity-chip','new-ribbon','level-chip')) {
    Kit-New 'Economy' $name 384 112 @(32,24,32,32)
    $fill=if($name -in @('price-ribbon','level-chip')){$ivory}elseif($name -eq 'new-ribbon'){$brass}else{$ink}
    Kit-Chamfer 3 3 378 106 20 $fill;Kit-Chamfer 3 3 378 106 20 $brass 1.5
    Kit-Save 'Support extensible de monnaie, prix, quantité, nouveauté ou niveau. Aucun montant préimprimé.'
}
foreach($opacity in @(25,50,75,90)) {
    Kit-New 'Overlays' ('veil-'+$opacity) 64 64
    Kit-Poly @(0,0,64,0,64,64,0,64) $ink ([int](255*$opacity/100))
    Kit-Save 'Voile marine plein cadre avec alpha réel.'
}
Kit-New 'Controls' 'scroll-thumb' 32 128 @(12,12,12,12)
Kit-Chamfer 3 3 26 122 8 $brass;Kit-Save 'Poignée de défilement extensible.'
Kit-New 'Controls' 'scroll-track' 32 128 @(12,12,12,12)
Kit-Chamfer 3 3 26 122 8 $ink;Kit-Save 'Piste de défilement.'
foreach($side in @('top','bottom','left','right')) {
    Kit-New 'Overlays' ('gradient-dark-'+$side) 256 256
    $start=switch($side){top{[System.Drawing.Point]::new(0,0)} bottom{[System.Drawing.Point]::new(0,256)} left{[System.Drawing.Point]::new(0,0)} right{[System.Drawing.Point]::new(256,0)}}
    $end=switch($side){top{[System.Drawing.Point]::new(0,256)} bottom{[System.Drawing.Point]::new(0,0)} left{[System.Drawing.Point]::new(256,0)} right{[System.Drawing.Point]::new(0,0)}}
    $brush=[System.Drawing.Drawing2D.LinearGradientBrush]::new($start,$end,(Kit-Color $ink 245),(Kit-Color $ink 0))
    $script:kitGraphics.FillRectangle($brush,0,0,256,256);$brush.Dispose()
    $x1=$start.X/256;$y1=$start.Y/256;$x2=$end.X/256;$y2=$end.Y/256
    [void]$script:kitSvg.AppendLine("<defs><linearGradient id=`"shade`" x1=`"$x1`" y1=`"$y1`" x2=`"$x2`" y2=`"$y2`"><stop stop-color=`"$ink`" stop-opacity=`"0.96`"/><stop offset=`"1`" stop-color=`"$ink`" stop-opacity=`"0`"/></linearGradient></defs><rect width=`"256`" height=`"256`" fill=`"url(#shade)`"/>")
    Kit-Save 'Dégradé avec alpha pour protéger les textes sur les illustrations.'
}
foreach($mask in @('circle','chamfer','square')) {
    Kit-New 'Masks' ('mask-'+$mask) 256 256
    if($mask -eq 'circle'){Kit-Circle 128 128 126 '' 0 '#FFFFFF'}elseif($mask -eq 'chamfer'){Kit-Chamfer 1 1 254 254 24 '#FFFFFF'}else{Kit-Poly @(0,0,256,0,256,256,0,256) '#FFFFFF'}
    Kit-Save 'Masque blanc à alpha réel pour portrait, tuile ou carte.'
}
for($face=1;$face -le 6;$face++) {
    Kit-New 'Board' ('dice-face-'+$face) 192 192 @(0,0,0,0) 96 96
    Kit-Chamfer 12 12 72 72 10 $ink;Kit-Chamfer 12 12 72 72 10 $brass 2
    if($face%2 -eq 1){Kit-Circle 48 48 4 '' 0 $ivory}
    if($face -ge 2){Kit-Circle 29 29 4 '' 0 $ivory;Kit-Circle 67 67 4 '' 0 $ivory}
    if($face -ge 4){Kit-Circle 67 29 4 '' 0 $ivory;Kit-Circle 29 67 4 '' 0 $ivory}
    if($face -eq 6){Kit-Circle 29 48 4 '' 0 $ivory;Kit-Circle 67 48 4 '' 0 $ivory}
    Kit-Save 'Face de dé indépendante de la police.'
}
foreach($state in @('normal','active')) {
    Kit-New 'Board' ('route-line-'+$state) 256 16
    Kit-Polyline @(0,8,256,8) $(if($state -eq 'active'){$brass}else{$edge}) 4
    Kit-Save 'Segment de chemin teintable, extensible dans sa longueur.'
}
foreach($family in $families.Keys) {
    Kit-New 'Decorations' ('family-band-'+$family) 512 12
    Kit-Poly @(0,0,512,0,512,12,0,12) $families[$family]
    Kit-Save 'Bande de famille, complément du pictogramme et du libellé.'
}
Kit-New 'Controls' 'joystick-ring' 256 256
Kit-Circle 128 128 119 $ivory 3;Kit-Circle 128 128 106 $edge 1;Kit-Save 'Anneau transparent du joystick flottant.'
Kit-New 'Controls' 'joystick-thumb' 128 128
Kit-Circle 64 64 56 $brass 2 $ivory;Kit-Save 'Poignée du joystick flottant.'
foreach($name in @('white','gold')+$families.Keys) {
    $c=if($families.ContainsKey($name)){$families[$name]}elseif($name -eq 'gold'){$brass}else{$ivory}
    Kit-New 'FX' ('glow-'+$name) 256 256
    $path=[System.Drawing.Drawing2D.GraphicsPath]::new();$path.AddEllipse(1,1,254,254)
    $brush=[System.Drawing.Drawing2D.PathGradientBrush]::new($path);$brush.CenterColor=Kit-Color $c 230;$brush.SurroundColors=@((Kit-Color $c 0))
    $script:kitGraphics.FillEllipse($brush,1,1,254,254);$brush.Dispose();$path.Dispose()
    [void]$script:kitSvg.AppendLine("<defs><radialGradient id=`"glow`"><stop stop-color=`"$c`" stop-opacity=`"0.9`"/><stop offset=`"1`" stop-color=`"$c`" stop-opacity=`"0`"/></radialGradient></defs><circle cx=`"128`" cy=`"128`" r=`"127`" fill=`"url(#glow)`"/>")
    Kit-Save 'Lueur UI à alpha progressif ; additive ou alpha selon le matériau choisi.'
}
Kit-New 'FX' 'spark-four-point' 128 128;Kit-Star 64 64 56 '#FFFFFF';Kit-Save 'Éclat teintable à quatre branches.'
Kit-New 'FX' 'slash-streak' 256 128;Kit-Poly @(5,113,180,22,251,8,90,95) '#FFFFFF';Kit-Save "Trainee pour impact ou transition, teinte au rendu."
$manifest=[ordered]@{name='Heroes Dawn — Aube tactique';version='1.0';reference='03_Art_UI_UX/Aube_Tactique_2026-11-04/index.html';palette=@{ink=$ink;ivory=$ivory;brass=$brass};items=$kitRecords.ToArray()}
$json=$manifest | ConvertTo-Json -Depth 9
[System.IO.File]::WriteAllText((Join-Path $PSScriptRoot 'manifest-native.json'),$json)
[System.IO.File]::WriteAllText((Join-Path $kitRoot 'manifest-native.json'),$json)
Write-Output "Kit natif : $($kitRecords.Count) PNG transparents et autant de sources SVG."

