param([string]$Workspace='C:/Users/dnath/Documents/Heroes Dawn')
$ErrorActionPreference='Stop'
$root=Join-Path $Workspace 'Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueKit'
$m=Get-Content (Join-Path $root 'manifest.json') -Raw | ConvertFrom-Json
$b=[Text.StringBuilder]::new()
[void]$b.Append(@'
<!doctype html><html lang="fr"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Aube tactique — Composants UI</title><style>
@font-face{font-family:Barlow;src:url('Fonts/Barlow-Regular.ttf')}@font-face{font-family:Dawn;src:url('Fonts/BarlowCondensed-ExtraBoldItalic.ttf')}
*{box-sizing:border-box}body{background:#101c29;color:#ece7dc;font:17px/1.5 Barlow,system-ui;margin:0}main{max-width:1500px;margin:auto;padding:32px}h1,h2,h3{font-family:Dawn;font-weight:normal}h1{font-size:48px}h2{font-size:34px;border-bottom:1px solid #d6af5b;padding-top:28px}a{color:#d6af5b}p{max-width:1000px}.matrix{overflow-x:auto}table{width:100%;min-width:800px;border-collapse:collapse}th,td{padding:14px;text-align:center;border-bottom:1px solid #344654}th{text-transform:capitalize}td img{width:100%;max-width:220px;height:65px;object-fit:contain}.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(150px,1fr));gap:14px}.tile{background:#192a38;padding:16px;min-width:0}.tile img{width:100%;height:90px;object-fit:contain}.tile span{display:block;font-size:13px;overflow-wrap:anywhere;margin-top:10px}.bars{display:grid;grid-template-columns:repeat(auto-fit,minmax(260px,1fr));gap:20px}.track{height:26px;border:1px solid #78858b;background:#142331;padding:3px;margin:12px 0}.track img{height:100%;display:block}.note{color:#aebdc4}nav{display:flex;gap:20px;flex-wrap:wrap}button{font:inherit;background:#ece7dc;color:#101c29;border:1px solid #d6af5b;padding:10px 16px;cursor:pointer}.checker .tile,.checker td{background:repeating-conic-gradient(#34414c 0% 25%,#1c2d3b 0% 50%) 50%/20px 20px}
</style><main><h1>Aube tactique · Composants UI</h1><p>Boutons, icônes, jauges et supports issus du kit. Les aperçus utilisent les PNG de production. Les libellés restent séparés pour la localisation.</p><nav><a href="catalogue.html">Catalogue complet et sources SVG</a><a href="README.md">Guide Unity</a><button onclick="document.body.classList.toggle('checker')">Fond damier</button></nav><h2>Boutons · matrice des états</h2><div class="matrix"><table><thead><tr><th>Famille</th><th>Normal</th><th>Survol</th><th>Pressé</th><th>Désactivé</th><th>Focus</th></tr></thead><tbody>
'@)
foreach($family in @('primary','secondary','ghost','danger','nav')){
 [void]$b.Append("<tr><th>$family</th>")
 foreach($state in @('normal','hover','pressed','disabled','focus')){[void]$b.Append("<td><a href='Buttons/button-$family-$state.png'><img src='Buttons/button-$family-$state.png' alt='$family $state'></a></td>")}
 [void]$b.Append('</tr>')
}
[void]$b.Append('</tbody></table></div><p class="note">Dans Unity : Image Sliced, bordures du manifeste ; Sprite Swap pour les états. Sur mobile, utiliser les états normal, pressé et désactivé. Garder une zone tactile suffisante indépendamment de la taille du pictogramme.</p><h2>Jauges · remplissages</h2><div class="bars">')
foreach($a in @($m.items | Where-Object {$_.id -like 'Controls/bar-*-fill'})){
 $name=[IO.Path]::GetFileNameWithoutExtension($a.file)
 [void]$b.Append("<div class='tile'><span>$name</span>")
 foreach($pct in @(25,60,100)){[void]$b.Append("<div class='track'><img style='width:$pct%' src='Controls/$name.png' alt='$pct %'></div>")}
 [void]$b.Append('</div>')
}
[void]$b.Append('</div><p class="note">Composition HTML de comparaison. Dans Unity, superposer le remplissage au sprite bar-track et piloter la largeur ou le fillAmount ; conserver les valeurs dans un texte indépendant.</p>')
foreach($category in @('Controls','Tabs','Inputs','Icons','IconsGold','IconsColor','Indicators','Economy','Frames','Surfaces')){
 [void]$b.Append("<h2>$category</h2><div class='grid'>")
 foreach($a in @($m.items | Where-Object {$_.category -eq $category -and $_.id -notlike 'Controls/bar-*-fill'})){
  $path=[IO.Path]::GetRelativePath($root,(Join-Path $Workspace $a.file)).Replace('\','/')
  $label=[Net.WebUtility]::HtmlEncode($a.id)
  [void]$b.Append("<a class='tile' href='$path'><img loading='lazy' src='$path' alt='$label'><span>$label</span></a>")
 }
 [void]$b.Append('</div>')
}
[void]$b.Append('</main></html>')
[IO.File]::WriteAllText((Join-Path $root 'composants.html'),$b.ToString(),[Text.UTF8Encoding]::new($false))
Write-Output 'Planche composants.html créée.'
