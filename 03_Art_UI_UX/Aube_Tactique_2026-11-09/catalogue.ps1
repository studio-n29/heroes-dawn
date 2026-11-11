$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueExtension'
$files=@(Get-ChildItem $root -Recurse -Filter '*.png' | Sort-Object FullName)
$records=@();$cards=@()
foreach($f in $files){
 $im=[Drawing.Bitmap]::FromFile($f.FullName)
 $minAlpha=255;$maxAlpha=0
 for($y=0;$y -lt $im.Height;$y+=[Math]::Max(1,[int]($im.Height/32))){for($x=0;$x -lt $im.Width;$x+=[Math]::Max(1,[int]($im.Width/32))){$a=$im.GetPixel($x,$y).A;$minAlpha=[Math]::Min($minAlpha,$a);$maxAlpha=[Math]::Max($maxAlpha,$a)}}
 $relative=[IO.Path]::GetRelativePath($PSScriptRoot,$f.FullName).Replace('\','/')
 $category=$f.Directory.Name
 $isExport=$f.FullName.StartsWith((Join-Path $root 'Exports'),[StringComparison]::OrdinalIgnoreCase)
 $records+=@{id="$category/$($f.BaseName)";file=$f.FullName;width=$im.Width;height=$im.Height;alphaSampleMin=$minAlpha;alphaSampleMax=$maxAlpha;isExport=$isExport;runtimeIntegration='not-verified-by-this-task'}
 $name=[System.Net.WebUtility]::HtmlEncode($f.BaseName)
 $cards+="<figure data-category='$category' data-export='$isExport'><a href='$relative'><img loading='lazy' src='$relative' alt='$name'></a><figcaption>$name <small>$category · $($im.Width) × $($im.Height)</small></figcaption></figure>"
 $im.Dispose()
}
$records | ConvertTo-Json -Depth 5 | Set-Content "$PSScriptRoot/manifest-generated.json"
$html=@"
<!doctype html><html lang='fr'><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=1'><title>Aube tactique — Assets</title><style>
*{box-sizing:border-box}body{margin:0;padding:32px;background:#101c29;color:#ece7dc;font:16px system-ui}header{max-width:1000px;margin-bottom:28px}h1{font-size:36px;margin:8px 0;color:#d6af5b}p{line-height:1.6}input{padding:12px;width:min(500px,100%);background:#1c2d3b;color:#ece7dc;border:1px solid #d6af5b}main{display:grid;grid-template-columns:repeat(auto-fill,minmax(220px,1fr));gap:18px}figure{margin:0;border:1px solid #52616b;background:#1c2d3b;overflow:hidden}img{width:100%;height:260px;object-fit:contain;background:repeating-conic-gradient(#283946 0% 25%,#334653 0% 50%) 50%/24px 24px}figcaption{padding:12px;overflow-wrap:anywhere}small{display:block;color:#aebcc6;margin-top:6px}a{color:#d6af5b}
</style><header><p>HEROES DAWN · 9–11 NOVEMBRE 2026</p><h1>Aube tactique — Assets générés</h1><p>$($records.Count) PNG présents sur disque. Sources SVG dans Sources/. Le raccordement aux écrans du jeu reste à vérifier dans Unity. Les transparences se lisent sur le damier. Cliquer sur un asset pour ouvrir son fichier original.</p><input id='search' placeholder='Filtrer par nom ou catégorie…' aria-label='Filtrer les assets'><select id='scope' aria-label='Type de fichiers'><option value='original'>Originaux</option><option value='export'>Exports techniques</option><option value='all'>Tous</option></select></header><main>$($cards -join "`n")</main><script>function filter(){const q=document.querySelector('#search').value.toLowerCase(),s=document.querySelector('#scope').value;document.querySelectorAll('figure').forEach(f=>{const e=f.dataset.export.toLowerCase()==='true';f.hidden=!f.textContent.toLowerCase().includes(q)||(s==='original'&&e)||(s==='export'&&!e)})};document.querySelector('#search').addEventListener('input',filter);document.querySelector('#scope').addEventListener('change',filter);filter()</script></html>
"@
$html | Set-Content "$PSScriptRoot/index.html"
$icons=@($files | Where-Object {$_.Directory.Name -eq 'Icons'})
$sheet=[Drawing.Bitmap]::new(1024,([int][Math]::Ceiling($icons.Count/8)*152))
$g=[Drawing.Graphics]::FromImage($sheet);$g.Clear([Drawing.ColorTranslator]::FromHtml('#101c29'))
$font=[Drawing.Font]::new('Arial',9);$brush=[Drawing.Brushes]::White
for($i=0;$i -lt $icons.Count;$i++){$x=($i%8)*128;$y=[int][Math]::Floor($i/8)*152;$im=[Drawing.Image]::FromFile($icons[$i].FullName);$g.DrawImage($im,$x+16,$y,96,96);$g.DrawString($icons[$i].BaseName,$font,$brush,[Drawing.RectangleF]::new($x,$y+98,128,50));$im.Dispose()}
$sheet.Save("$PSScriptRoot/qa-icons.png");$font.Dispose();$g.Dispose();$sheet.Dispose()
Write-Output "$($records.Count) PNG vérifiés et catalogue actualisé."



