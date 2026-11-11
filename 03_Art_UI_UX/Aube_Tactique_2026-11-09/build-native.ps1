. "$PSScriptRoot/build-extension.ps1"
$white='#FFFFFF'
function Line($p){Kit-Polyline $p $white 2.3}
function Circle($x,$y,$r){Kit-Circle $x $y $r $white 2.3}
function Rect($x,$y,$w,$h){Kit-Rect $x $y $w $h $white 2.3}
function Person($x,$y){Circle $x $y 7; Line @(($x-11),($y+27),($x-11),($y+17),$x,($y+11),($x+11),($y+17),($x+11),($y+27))}
function Check($x,$y){Line @($x,$y,($x+7),($y+7),($x+21),($y-10))}
function Star5($filled){$p=@();for($i=0;$i -lt 10;$i++){$a=($i*36-90)*[Math]::PI/180;$r=if($i%2){14}else{33};$p+=48+$r*[Math]::Cos($a);$p+=48+$r*[Math]::Sin($a)};if($filled){Kit-Poly $p $white}else{Kit-Polyline $p $white 2.3 $true}}
$icons=[ordered]@{
'handshake'={Line @(12,35,25,24,40,31,53,25,72,31,84,45,72,68,61,74,43,67,28,53,17,56,12,35);Line @(40,31,30,43,37,49,49,40,69,57);Line @(49,55,64,68);Line @(42,61,55,72)}
'shield-check'={Line @(48,13,76,24,72,56,62,71,48,82,34,71,24,56,20,24,48,13);Check 33 47}
'crowd'={Person 48 29;Person 22 40;Person 74 40}
'tent'={Line @(13,78,48,19,83,78,13,78);Line @(35,78,48,50,61,78);Line @(42,12,54,25)}
'mountain'={Line @(9,78,38,24,57,53,69,37,88,78,9,78);Line @(27,45,37,41,44,48,48,41)}
'star-threat-full'={Star5 $true};'star-threat-empty'={Star5 $false}
'crossed-swords'={Line @(20,15,33,19,75,69,67,76,21,29,20,15);Line @(76,15,63,19,21,69,29,76,75,29,76,15);Line @(16,60,36,80);Line @(60,80,80,60);Line @(17,79,24,72);Line @(72,72,79,79)}
'node-crate'={Rect 20 25 56 50;Line @(20,25,76,75);Line @(76,25,20,75);Line @(16,19,80,19)}
'node-alert'={Line @(48,13,83,79,13,79,48,13);Line @(48,35,48,56);Circle 48 67 2}
'node-tower'={Line @(25,81,34,28,62,28,71,81);Rect 26 17 44 12;Line @(31,49,65,49);Line @(28,67,68,67);Line @(48,17,48,7)}
'node-chest'={Rect 18 42 60 35;Line @(18,42,23,25,73,25,78,42);Line @(18,52,78,52);Rect 43 46 10 15}
'crown'={Line @(15,29,31,44,48,18,65,44,81,29,72,75,24,75,15,29);Line @(25,65,71,65)}
'smartphone'={Rect 29 12 38 72;Line @(40,19,56,19);Circle 48 76 2}
'person-check'={Person 37 26;Check 57 65}
'friend-online'={Person 40 28;Kit-Circle 72 68 8 '' 0 '#FFFFFF'}
'lightning-source'={Line @(54,12,27,51,44,51,38,83,70,40,52,40,54,12)}
'gear'={$p=@();for($i=0;$i -lt 48;$i++){$a=$i*2*[Math]::PI/48;$r=if(($i%6) -in @(0,1,2)){37}else{29};$p+=48+$r*[Math]::Cos($a);$p+=48+$r*[Math]::Sin($a)};Kit-Polyline $p $white 2.3 $true;Circle 48 48 10}
'arrow-cycle'={Line @(23,34,36,20,59,19,75,34,77,49);Line @(66,41,77,51,85,38);Line @(73,62,60,76,37,77,21,62,19,47);Line @(30,55,19,45,11,58)}
'ticket'={Line @(16,26,80,26,80,40,72,48,80,56,80,70,16,70,16,56,24,48,16,40,16,26);Line @(60,31,60,39);Line @(60,45,60,53);Line @(60,59,60,65)}
'skull'={Line @(27,65,19,48,22,29,35,18,61,18,74,29,77,48,69,65,61,65,61,80,35,80,35,65,27,65);Circle 34 45 8;Circle 62 45 8;Line @(45,62,48,57,51,62);Line @(43,71,43,80);Line @(53,71,53,80)}
'dumbbell'={Rect 13 29 12 38;Rect 71 29 12 38;Rect 25 39 46 18}
'truck'={Rect 12 28 45 36;Line @(57,39,71,39,84,52,84,64,57,64);Circle 28 70 8;Circle 72 70 8}
'microscope'={Line @(35,15,55,24,43,49,23,40,35,15);Line @(52,38,69,44,75,60,68,73,29,73);Line @(20,83,79,83);Line @(32,58,54,58);Line @(48,73,48,83)}
'moon'={Line @(61,15,42,18,26,32,20,50,26,69,43,81,64,80,78,66,58,65,43,52,39,35,47,21,61,15)}
'touch'={Line @(28,70,19,53,26,49,38,60,38,23,45,19,51,24,51,47,59,43,75,50,78,68,67,81,42,81,28,70);Circle 45 20 12}
'swipe'={Line @(15,26,81,26);Line @(24,17,15,26,24,35);Line @(72,17,81,26,72,35);Line @(32,75,25,59,32,55,42,64,42,43,49,39,56,43,56,58,70,60,72,75,62,84,43,84,32,75)}
'zoom-in'={Circle 40 40 25;Line @(59,59,81,81);Line @(27,40,53,40);Line @(40,27,40,53)}
'monitor'={Rect 12 19 72 49;Line @(48,68,48,81);Line @(30,81,66,81)}
'trash'={Line @(22,28,27,81,69,81,74,28);Line @(16,24,80,24);Line @(34,24,34,14,62,14,62,24);Line @(39,38,39,69);Line @(57,38,57,69)}
'graduation-cap'={Line @(10,35,48,17,86,35,48,53,10,35);Line @(25,43,25,66,48,77,71,66,71,43);Line @(86,35,86,64)}
'book-open'={Line @(48,27,34,18,13,18,13,74,34,74,48,82,62,74,83,74,83,18,62,18,48,27,48,82)}
'id-tag'={Rect 18 23 60 58;Rect 36 13 24 18;Circle 37 45 7;Line @(25,66,28,56,46,56,49,66);Line @(57,44,69,44);Line @(57,57,69,57)}
'check-box-ok'={Rect 18 18 60 60;Check 32 48}
'layers'={Line @(12,32,48,15,84,32,48,49,12,32);Line @(12,48,48,65,84,48);Line @(12,64,48,81,84,64)}
'paper-plane'={Line @(12,42,84,14,62,83,45,59,12,42);Line @(45,59,84,14);Line @(45,59,40,80,54,71)}
'chat-bubble'={Line @(16,19,80,19,80,65,44,65,24,81,24,65,16,65,16,19);Line @(28,35,68,35);Line @(28,48,57,48)}
'target'={Circle 48 48 32;Circle 48 48 20;Circle 48 48 7}
'hand-hold'={Line @(15,64,29,51,40,51,49,58,68,49,79,51,81,57,60,77,34,77,25,83);Circle 55 28 14}
'flame'={Line @(48,12,59,35,69,28,79,52,75,68,62,81,39,83,23,70,18,54,29,30,31,51,43,39,48,12)}
'herb'={Line @(27,83,52,30,70,15);Line @(45,45,27,44,19,27,36,30,45,45);Line @(37,62,58,63,75,44,57,46,37,62)}
'mineral-salt'={Line @(17,71,25,44,41,34,51,55,58,25,73,19,84,64,70,80,34,82,17,71);Line @(58,25,68,62,84,64)}
'mortar'={Line @(18,46,78,46,70,68,59,78,37,78,26,68,18,46);Line @(50,45,69,15,78,21,64,45);Line @(28,83,68,83)}
'flask'={Line @(36,14,60,14);Line @(40,15,40,39,20,72,24,82,72,82,76,72,56,39,56,15);Line @(31,61,65,61)}
'heat'={Line @(16,78,80,78);Line @(28,65,22,54,28,43,22,32,28,21);Line @(48,65,42,54,48,43,42,32,48,21);Line @(68,65,62,54,68,43,62,32,68,21)}
'sprint'={Circle 61 20 8;Line @(21,44,39,33,56,38,67,52,81,52);Line @(56,38,44,59,61,68,57,84);Line @(44,59,30,73,13,73);Line @(12,29,32,29);Line @(8,55,24,55)}
'blast'={Line @(48,10,56,33,78,20,67,42,88,49,65,56,78,79,55,66,47,88,39,65,17,78,29,56,8,48,31,40,19,18,40,31,48,10)}
'wave'={Line @(14,68,22,44,35,27,52,22,69,30,75,47,63,41,53,44,48,54,53,66,70,73,83,70);Line @(14,80,83,80)}
'exit'={Line @(45,16,20,16,20,80,45,80);Line @(40,48,83,48);Line @(69,34,83,48,69,62)}
'power-supply'={Rect 17 29 62 46;Rect 36 19 24 10;Line @(50,36,38,53,51,53,44,69,62,47,50,47)}
'regulation'={Line @(18,25,78,25);Line @(18,48,78,48);Line @(18,71,78,71);Circle 35 25 7;Circle 62 48 7;Circle 42 71 7}
'control-chip'={Rect 29 29 38 38;for($v=35;$v -le 61;$v+=13){Line @($v,17,$v,29);Line @($v,67,$v,79);Line @(17,$v,29,$v);Line @(67,$v,79,$v)}}
}
foreach($entry in $icons.GetEnumerator()){Kit-New 'Icons' $entry.Key 128 128 @(0,0,0,0) 96 96; & $entry.Value; Kit-Save 'Pictogramme monochrome blanc, teinte runtime.'}
foreach($side in @('left','right')){Kit-New 'Decorations' ('laurel-'+$side) 128 256 @(0,0,0,0) 96 192;$sign=if($side -eq 'left'){1}else{-1};$cx=if($side -eq 'left'){64}else{32};for($i=0;$i -lt 8;$i++){$y=28+$i*18;$x=$cx-$sign*(27*[Math]::Sin(($i+1)*[Math]::PI/10));Kit-Poly @($x,$y,($x-18*$sign),($y-17),($x-20*$sign),($y-3),$x,($y+7)) $brass;Kit-Poly @($x,$y,($x+17*$sign),($y-10),($x+12*$sign),($y+5),$x,($y+10)) $brass};Kit-Save 'Demi-couronne de victoire dorée.'}
foreach($rank in 1..3){Kit-New 'IconsGold' "medal-$rank" 128 128 @(0,0,0,0) 96 96;$col=@($brass,'#B8C7D4','#AF7950')[$rank-1];Kit-Poly @(26,12,43,12,58,43,43,49) '#A33443';Kit-Poly @(53,12,70,12,53,49,38,43) '#385A74';Kit-Circle 48 59 23 $col 3 $ink;for($i=0;$i -lt $rank;$i++){Kit-Polyline @((43+5*$i),50,(43+5*$i),68) $col 3};Kit-Save 'Médaille de rang, I / II / III.'}
foreach($kind in @('objectives-paper','node-detail','reward-tile','agent-xp','leaderboard-row','leaderboard-player','spiral-notebook','enemy-slanted-frame','fatigue-portrait','new-corner','rarity-corner','bolt-pill','hex-round-chip','gold-toggle-on','gold-toggle-off','reflex-success','reflex-cursor','action-circle','touch-left','touch-right','touch-up','vs-banner')){
 $w=512;$h=256;if($kind -in @('reward-tile','fatigue-portrait','new-corner','rarity-corner','hex-round-chip','action-circle','touch-left','touch-right','touch-up')){$w=256;$h=256};if($kind -in @('leaderboard-row','leaderboard-player')){$w=1024;$h=128};if($kind -eq 'spiral-notebook'){$w=512;$h=640};if($kind -eq 'vs-banner'){$w=1920;$h=400}
 Kit-New 'Components' $kind $w $h @(16,16,16,16)
 switch($kind){
 'new-corner' {Kit-Poly @(0,0,160,0,256,96,256,256) $brass}
 'rarity-corner' {Kit-Poly @(0,0,120,0,256,136,256,220) '#AE8BD3'}
 'vs-banner' {Kit-Poly @(0,60,920,60,830,340,0,340) $ivory 225;Kit-Poly @(1090,60,1920,60,1920,340,1000,340) '#A33443' 225;Kit-Star 960 200 95 $brass}
 'enemy-slanted-frame' {Kit-Polyline @(40,4,508,4,472,252,4,252,40,4) '#A33443' 4}
 'fatigue-portrait' {Kit-Chamfer 3 3 250 250 16 $brass 2;Kit-Chamfer 14 224 228 18 7 $ink;Kit-Chamfer 17 227 222 12 5 '#344652'}
 'bolt-pill' {Kit-Chamfer 3 3 506 250 60 $ink;Kit-Chamfer 3 3 506 250 60 $brass 3;Kit-Poly @(145,40,88,135,137,135,113,215,196,105,151,105) $brass}
 'hex-round-chip' {$p=@(64,8,192,8,248,128,192,248,64,248,8,128);Kit-Poly $p $ink;Kit-Polyline $p $brass 3 $true}
 'gold-toggle-on' {Kit-Poly @(74,65,438,65,438,191,74,191) $brass;Kit-Circle 74 128 63 '' 0 $brass;Kit-Circle 438 128 63 '' 0 $brass;Kit-Circle 438 128 53 '' 0 $ivory}
 'gold-toggle-off' {Kit-Poly @(74,65,438,65,438,191,74,191) '#344652';Kit-Circle 74 128 63 '' 0 '#344652';Kit-Circle 438 128 63 '' 0 '#344652';Kit-Circle 74 128 53 '' 0 $ivory}
 'reflex-success' {for($x=8;$x -lt 500;$x+=24){Kit-Polyline @($x,45,($x+12),45) '#79B681' 4;Kit-Polyline @($x,210,($x+12),210) '#79B681' 4};Kit-Poly @(0,55,512,55,512,200,0,200) '#79B681' 45}
 'reflex-cursor' {Kit-Poly @(235,12,277,12,256,42) $brass;Kit-Polyline @(256,42,256,244) $ivory 5}
 {$_ -in @('action-circle','touch-left','touch-right','touch-up')} {Kit-Circle 128 128 118 $brass 3 $ivory;if($kind -eq 'touch-left'){Kit-Polyline @(150,76,98,128,150,180) $ink 9};if($kind -eq 'touch-right'){Kit-Polyline @(106,76,158,128,106,180) $ink 9};if($kind -eq 'touch-up'){Kit-Polyline @(76,150,128,98,180,150) $ink 9}}
 default {$fill=if($kind -in @('objectives-paper','spiral-notebook')){$ivory}else{$ink};Kit-Chamfer 3 3 ($w-6) ($h-6) 18 $fill;Kit-Chamfer 3 3 ($w-6) ($h-6) 18 $brass 2;if($kind -eq 'objectives-paper'){Kit-Star 36 34 16 $brass;Kit-Polyline @(20,65,492,65) $brass 1;Kit-Polyline @(20,184,492,184) $brass 1};if($kind -eq 'spiral-notebook'){for($y=28;$y -lt 620;$y+=38){Kit-Circle 18 $y 9 '#78858B' 3};for($y=72;$y -lt 620;$y+=32){Kit-Polyline @(46,$y,482,$y) '#C8C2B6' 1}};if($kind -eq 'leaderboard-player'){Kit-Poly @(3,3,12,3,12,125,3,125) $brass};if($kind -eq 'agent-xp'){Kit-Rect 15 15 170 170 $edge 2;Kit-Chamfer 205 180 285 20 5 '#344652'};if($kind -eq 'node-detail'){Kit-Polyline @(170,16,170,240) $brass 2}}
 }
 Kit-Save 'Fond composable sans texte : valeurs, portraits et progression restent dynamiques. Bordures à adapter à chaque gabarit.'
}
foreach($color in @('amber','cyan','pink')){
 $col=@{amber='#EFC25F';cyan='#69D6E5';pink='#E879B5'}[$color]
 Kit-New 'MiniGames' "rhythm-note-$color" 256 256
 Kit-Poly @(128,32,224,128,128,224,32,128) $col 65;Kit-Poly @(128,53,203,128,128,203,53,128) $col;Kit-Polyline @(128,68,188,128,128,188,68,128,128,68) $ivory 3;Kit-Save 'Note losange lumineuse.'
 Kit-New 'MiniGames' "rhythm-lane-$color" 256 1024
 Kit-Poly @(64,0,192,0,256,1024,0,1024) $ink 180;Kit-Polyline @(64,0,0,1024) $col 3;Kit-Polyline @(192,0,256,1024) $col 3;Kit-Save 'Piste à étirer verticalement.'
}
Kit-New 'MiniGames' 'vision-cone' 512 512
for($i=30;$i -ge 1;$i--){$r=$i*16;Kit-Poly @(256,20,(256-$r/2),(20+$r),(256+$r/2),(20+$r)) '#DF5260' 4};Kit-Save 'Cône rouge translucide, pivot à la pointe haute.'
Kit-New 'MiniGames' 'hazard-stripe' 512 96
Kit-Poly @(0,0,512,0,512,96,0,96) $ink
for($x=0;$x -lt 512;$x+=96){Kit-Poly @($x,0,($x+48),0,$x,96,($x-48),96) $brass};Kit-Save 'Bande de danger répétable.'
Kit-New 'MiniGames' 'footprints' 256 256
Kit-Chamfer 63 25 40 75 14 '#72899D' 0 130;Kit-Chamfer 145 135 40 75 14 '#72899D' 0 130;Kit-Save 'Paire de traces dans la neige.'
Kit-New 'MiniGames' 'molecule-link' 256 32
Kit-Poly @(0,8,256,8,256,24,0,24) '#8DE2DA' 50;Kit-Polyline @(0,16,256,16) '#D2FFF6' 4;Kit-Save 'Liaison moléculaire lumineuse.'
Kit-New 'Icons' 'hearts-full' 128 128 @(0,0,0,0) 96 96
Kit-Poly @(48,80,16,49,13,32,23,20,37,19,48,30,59,19,73,20,83,32,80,49) $white;Kit-Save 'Vie pleine.'
Kit-New 'IconsGold' 'gear' 256 256 @(0,0,0,0) 96 96
$points=@();for($i=0;$i -lt 48;$i++){$a=$i*2*[Math]::PI/48;$r=if(($i%6) -in @(0,1,2)){37}else{29};$points+=48+$r*[Math]::Cos($a);$points+=48+$r*[Math]::Sin($a)};Kit-Polyline $points $brass 4 $true;Kit-Circle 48 48 11 $brass 4;Kit-Save 'Roue dentée dorée.'
Kit-New 'Icons' 'boss-diamond' 128 128 @(0,0,0,0) 96 96
Kit-Polyline @(48,8,88,48,48,88,8,48,48,8) $white 2.3;Kit-Polyline @(28,40,38,48,48,32,58,48,68,40,63,65,33,65,28,40) $white 2.3;Kit-Save 'Boss en losange.'
$map=Get-Content (Join-Path $Workspace 'Code/Heroes Dawn Unity/Assets/_Project/Presentation/Hub/Pages/CityMap/CityMapLayout.cs') -Raw
foreach($m in [regex]::Matches($map,'new BoroughShape\("([^"]+)"[^\r\n]+')){
 $id=$m.Groups[1].Value;$vs=@([regex]::Matches($m.Value,'V\(([0-9.]+)f, ([0-9.]+)f\)'));$xy=@();foreach($v in $vs[1..($vs.Count-1)]){$xy+=[float]$v.Groups[1].Value*3840;$xy+=[float]$v.Groups[2].Value*2160}
 Kit-New 'MapMasks' $id 3840 2160;Kit-Poly $xy '#FFFFFF';Kit-Save 'Masque alpha exact pour afficher la texture urbaine commune dans chaque arrondissement.'
 foreach($state in @('normal','selected')){Kit-New 'MapOverlays' "$id-$state" 3840 2160;Kit-Poly $xy $brass $(if($state -eq 'selected'){38}else{9});Kit-Polyline $xy $(if($state -eq 'selected'){$brass}else{$edge}) $(if($state -eq 'selected'){8}else{3}) $true;Kit-Save 'Calque polygonal exact CityMapLayout. Illustration de fond séparée ; axe Y vers le bas.'}
}
foreach($family in @('matter','elemental','bio','perception')){Kit-New 'MiniGames' "rhythm-platform-$family" 256 128;$col=@{matter='#D6AF5B';elemental='#69D6E5';bio='#E879B5';perception='#AE8BD3'}[$family];Kit-Chamfer 8 8 240 112 24 $ink;Kit-Chamfer 8 8 240 112 24 $col 4;Kit-Save 'Plateforme de rythme ; superposer une icone de famille existante.'}
Kit-New 'MiniGames' 'basket-fence' 512 512
for($x=-512;$x -lt 1024;$x+=48){Kit-Polyline @($x,0,($x+512),512) '#8499A6' 2;Kit-Polyline @($x,0,($x-512),512) '#8499A6' 2};Kit-Save 'Grillage transparent répétable.'
Kit-New 'Components' 'podium' 1024 512
foreach($p in @(@(40,230,300,270),@(350,100,324,400),@(684,300,300,200))){Kit-Chamfer $p[0] $p[1] $p[2] $p[3] 18 $ink;Kit-Chamfer $p[0] $p[1] $p[2] $p[3] 18 $brass 2};Kit-Save 'Podium à trois places, chiffres et avatars séparés.'
Kit-New 'Components' 'recipe-book' 1024 640
Kit-Poly @(10,30,470,15,512,45,554,15,1014,30,1014,620,554,605,512,635,470,605,10,620) $ivory;Kit-Polyline @(512,45,512,635) '#B5AA95' 3;for($y=100;$y -lt 600;$y+=42){Kit-Polyline @(40,$y,470,$y) '#C8C2B6' 1;Kit-Polyline @(554,$y,980,$y) '#C8C2B6' 1};Kit-Save 'Livre de recettes ouvert, sans texte figé.'
$kitRecords | ConvertTo-Json -Depth 6 | Set-Content "$PSScriptRoot/manifest-native.json"
Write-Output ("Generated {0} native PNG + SVG assets" -f $kitRecords.Count)
