# Génère un recadrage JPEG avec cadre jaune par ligne de l'Asset List 2D (liste A et B).
# Lit les lignes (id + ref:R("image",[[x,y,w,h],...],...)) dans le HTML voisin ; coordonnées en pixels de l'image source.
# Usage : powershell -File build-crops.ps1
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$here  = $PSScriptRoot
$prod  = Split-Path $here
$html  = Join-Path $prod 'Asset_List_2D_Manquants_2026-11-16.html'
$plDir = Join-Path (Split-Path $prod) '03_Art_UI_UX\Aube_Tactique_2026-11-04'
$src = @{
  '01'='01-entree.png'; '02'='02-compte.png'; '04'='04-equipe-agents.png'; '05'='05-collections.png'; '06'='06-mission.png'
  '07'='07-plateau.png'; '08'='08-combat.png'; '09'='09-narration-resultats.png'; '10'='10-boutique.png'; '11'='11-recompenses.png'
  '12'='12-progression-social.png'; '13'='13-tournoi-tutoriel.png'; '16'='16-fenetres.png'; '18'='18-minijeux-b.png'
  '20'='20-heros-etats.png'; '21'='21-complements.png'
}
$text = [IO.File]::ReadAllText($html, [Text.Encoding]::UTF8)
$rx = [regex]'id:"([A-Z]-\d\d)"[\s\S]*?ref:R\("([^"]+)",(\[\[[\d,\[\] ]*\]\])'
$out = Join-Path $here 'crops'
New-Item -ItemType Directory -Force $out | Out-Null
$ASPECT = 1.6
$cache = @{}
foreach ($m in $rx.Matches($text)) {
  $id = $m.Groups[1].Value; $key = $m.Groups[2].Value
  $path = if ($key -like 'cap-*') { Join-Path $here ($(if($key -eq 'cap-combat'){'04-combat.png'}else{'91b-chapitres-souvenir.png'})) } else { Join-Path $plDir $src[$key] }
  if (-not $cache.ContainsKey($path)) { $cache[$path] = [System.Drawing.Image]::FromFile($path) }
  $img = $cache[$path]; $W = $img.Width; $H = $img.Height
  $boxes = @(); foreach ($b in ([regex]'\[(\d+),(\d+),(\d+),(\d+)\]').Matches($m.Groups[3].Value)) { $boxes += ,@([double]$b.Groups[1].Value,[double]$b.Groups[2].Value,[double]$b.Groups[3].Value,[double]$b.Groups[4].Value) }
  $x0 = ($boxes | % { $_[0] } | Measure-Object -Minimum).Minimum; $y0 = ($boxes | % { $_[1] } | Measure-Object -Minimum).Minimum
  $x1 = ($boxes | % { $_[0]+$_[2] } | Measure-Object -Maximum).Maximum; $y1 = ($boxes | % { $_[1]+$_[3] } | Measure-Object -Maximum).Maximum
  $uw = $x1-$x0; $uh = $y1-$y0; $cx = $x0+$uw/2; $cy = $y0+$uh/2
  $tw = [Math]::Max([Math]::Max($uw*1.55, $uh*1.55*$ASPECT), 330*($W/1774.0)); $th = $tw/$ASPECT
  if ($th -lt $uh*1.4) { $th = $uh*1.4; $tw = $th*$ASPECT }
  if ($tw -gt $W) { $tw = $W; $th = $tw/$ASPECT }
  if ($th -gt $H) { $th = $H; $tw = $th*$ASPECT }
  $x = [Math]::Max(0,[Math]::Min($W-$tw,$cx-$tw/2)); $y = [Math]::Max(0,[Math]::Min($H-$th,$cy-$th/2))
  $ow = [int][Math]::Min(960, [Math]::Max(640, $tw)); $oh = [int]($ow/$ASPECT)
  $bmp = New-Object System.Drawing.Bitmap $ow, $oh
  $g = [System.Drawing.Graphics]::FromImage($bmp)
  $g.InterpolationMode = 'HighQualityBicubic'; $g.SmoothingMode = 'AntiAlias'
  $g.DrawImage($img, (New-Object System.Drawing.Rectangle 0,0,$ow,$oh), (New-Object System.Drawing.Rectangle ([int]$x),([int]$y),([int]$tw),([int]$th)), 'Pixel')
  $k = $ow / $tw
  $fill = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(34,255,210,31))
  $pen  = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255,255,210,31)), 3
  $pen2 = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255,0,0,0)), 1
  foreach ($b in $boxes) {
    $r = New-Object System.Drawing.RectangleF ([single](($b[0]-$x)*$k)),([single](($b[1]-$y)*$k)),([single]($b[2]*$k)),([single]($b[3]*$k))
    $g.FillRectangle($fill, $r)
    $g.DrawRectangle($pen2, $r.X-2, $r.Y-2, $r.Width+4, $r.Height+4)
    $g.DrawRectangle($pen2, $r.X+2, $r.Y+2, $r.Width-4, $r.Height-4)
    $g.DrawRectangle($pen, $r.X, $r.Y, $r.Width, $r.Height)
  }
  $g.Dispose()
  $enc = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | ? { $_.MimeType -eq 'image/jpeg' }
  $ep = New-Object System.Drawing.Imaging.EncoderParameters 1
  $ep.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality), ([long]88)
  $bmp.Save((Join-Path $out "$id.jpg"), $enc, $ep); $bmp.Dispose()
  "{0} <- {1}  crop {2:N0},{3:N0} {4:N0}x{5:N0}" -f $id, (Split-Path $path -Leaf), $x, $y, $tw, $th
}
foreach ($i in $cache.Values) { $i.Dispose() }
