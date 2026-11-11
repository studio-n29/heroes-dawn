$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueExtension'
$out=Join-Path $root 'Exports'
$records=[Collections.Generic.List[object]]::new()
$faceY=@{giulia_rossi=.23;agent_d=.23;agent_m=.23;ilona_varga=.33;louis_dubois=.28;ada_okonkwo=.23;victor_sorel=.25;fatou_diallo=.21;clement_lefevre=.30;youssef_haddad=.26;marco_bellini=.24;severine_morel=.24;ji_hoon_park=.25;darius_brooks=.29;mei_lin_chen=.21;kwame_abara=.31;noah_kaplan=.23;freya_lindqvist=.34;tunde_oyelaran=.23;lucas_mendes=.24;rin_takeda=.22;amara_hughes=.24;sam_okafor=.22;astrid_hallstrom=.33;celeste_moreau=.25;anna_petrov=.25;haruto_nakamura=.40;eero_nieminen=.24;mateo_vasquez=.26;zofia_kowalczyk=.24;odile_ferrand=.28;yelena_ivanova=.23;brie_rayner=.25}
function Export-Image($source,$group,$id,[int]$w,[int]$h,$mode='cover',[double]$cx=.5,[double]$cy=.5,[double]$fraction=1){
 if(!(Test-Path -LiteralPath $source)){return}
 $dir=Join-Path $out $group;[IO.Directory]::CreateDirectory($dir)|Out-Null
 $dest=Join-Path $dir "$id.png"
 $im=[Drawing.Image]::FromFile($source)
 try{
  $sw=[double]$im.Width;$sh=[double]$im.Height
  $crop=[Drawing.RectangleF]::new(0,0,$sw,$sh)
  $target=[Drawing.RectangleF]::new(0,0,$w,$h)
  if($mode -eq 'contain'){$scale=[Math]::Min($w/$sw,$h/$sh);$target=[Drawing.RectangleF]::new(($w-$sw*$scale)/2,($h-$sh*$scale)/2,$sw*$scale,$sh*$scale)}
  else{$cw=[Math]::Min($sw,$sh*$w/$h)*$fraction;$ch=$cw*$h/$w;$x=[Math]::Max(0,[Math]::Min($sw-$cw,$sw*$cx-$cw/2));$y=[Math]::Max(0,[Math]::Min($sh-$ch,$sh*$cy-$ch/2));$crop=[Drawing.RectangleF]::new($x,$y,$cw,$ch)}
  if(!(Test-Path $dest) -or (Get-Item $source).LastWriteTimeUtc -gt (Get-Item $dest).LastWriteTimeUtc){
   $bm=[Drawing.Bitmap]::new($w,$h,[Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[Drawing.Graphics]::FromImage($bm)
   try{$g.Clear([Drawing.Color]::Transparent);$g.CompositingMode='SourceCopy';$g.InterpolationMode='HighQualityBicubic';$g.PixelOffsetMode='HighQuality';$attr=[Drawing.Imaging.ImageAttributes]::new();try{$attr.SetWrapMode([Drawing.Drawing2D.WrapMode]::TileFlipXY);$rect=[Drawing.Rectangle]::Round($target);$g.DrawImage($im,$rect,$crop.X,$crop.Y,$crop.Width,$crop.Height,[Drawing.GraphicsUnit]::Pixel,$attr);$bm.Save($dest,[Drawing.Imaging.ImageFormat]::Png)}finally{$attr.Dispose()}}finally{$g.Dispose();$bm.Dispose()}
  }
  $records.Add(@{source=$source;file=$dest;width=$w;height=$h;sourceWidth=$sw;sourceHeight=$sh;crop=@($crop.X,$crop.Y,$crop.Width,$crop.Height);mode=$mode;upscaled=($target.Width -gt $crop.Width -or $target.Height -gt $crop.Height)})
 }finally{$im.Dispose()}
}
foreach($file in Get-ChildItem "$root/Agents" -Filter '*.png'){
 if($file.BaseName -like '*-v2'){continue}
 $id=$file.BaseName -replace '-cutout$','';$src=$file.FullName
 if($id -eq 'ada_okonkwo' -and (Test-Path "$root/Agents/ada_okonkwo-cutout-v2.png")){$src="$root/Agents/ada_okonkwo-cutout-v2.png"}
 $y=.25;if($faceY.ContainsKey($id)){$y=$faceY[$id]}
 foreach($size in @(128,512)){Export-Image $src "Portraits$size" $id $size $size 'cover' .52 $y .56}
}
foreach($cat in @('Cards','Skills','Rivals','BrandIcons')){foreach($file in Get-ChildItem "$root/$cat" -Filter '*.png' -ErrorAction SilentlyContinue){foreach($size in @(128,256)){Export-Image $file.FullName "$cat$size" $file.BaseName $size $size};if($cat -eq 'Cards'){Export-Image $file.FullName 'Cards768x1024' $file.BaseName 768 1024}}}
foreach($list in @('prompts-sprites.json','prompts-supplement.json')){foreach($j in (Get-Content "$PSScriptRoot/$list" -Raw|ConvertFrom-Json)){if($j.category -in @('Items','Resources','MiniGames')){Export-Image "$root/$($j.category)/$($j.id).png" "$($j.category)256" $j.id 256 256 'contain'}}}
foreach($cat in @('Dossiers','MiniGameIntros','Zones','Chapters','ChaptersLocked','Boards')){foreach($file in Get-ChildItem "$root/$cat" -Filter '*.png' -ErrorAction SilentlyContinue){$w=1600;$h=900;if($cat -in @('Chapters','ChaptersLocked')){$w=1200;$h=675};if($cat -eq 'Boards'){$w=3840;$h=2160};Export-Image $file.FullName "${cat}${w}x$h" $file.BaseName $w $h}}
foreach($file in Get-ChildItem "$root/Poses" -Filter '*.png' -ErrorAction SilentlyContinue){Export-Image $file.FullName 'Poses512x768' $file.BaseName 512 768 'contain'}
Export-Image "$root/MiniGames/gear-body.png" 'MiniGames256' 'gear-body' 256 256 'contain'
foreach($file in Get-ChildItem "$root/Scenes" -Filter '*.png' -ErrorAction SilentlyContinue){
 $im=[Drawing.Image]::FromFile($file.FullName);try{$w=1920;$h=1080;if($im.Height -gt $im.Width){$w=1200;$h=1400};if($im.Width -eq $im.Height){$w=512;$h=512};if($file.BaseName -eq 'map-new-york'){$w=3840;$h=2160}}finally{$im.Dispose()}
 Export-Image $file.FullName "Scenes${w}x$h" $file.BaseName $w $h
}
$enemySources=@(Get-ChildItem "$(Split-Path $root)/AubeTactiqueKit/Illustrations/Generated" -Filter 'enemy-*.png')+@(Get-ChildItem "$root/Enemies" -Filter '*.png')
foreach($file in $enemySources){foreach($size in @(128,512)){Export-Image $file.FullName "Enemies$size" $file.BaseName $size $size 'cover' .5 .27 .65}}
$jpgRecords=[Collections.Generic.List[object]]::new()
$codec=[Drawing.Imaging.ImageCodecInfo]::GetImageEncoders()|Where-Object MimeType -eq 'image/jpeg'
foreach($r in $records){if($r.file -match '(Dossiers|MiniGameIntros|Zones|Chapters|ChaptersLocked|Boards|Scenes)[0-9]+x[0-9]+'){
 $dest=[IO.Path]::ChangeExtension($r.file,'.jpg')
 if(!(Test-Path $dest) -or (Get-Item $r.file).LastWriteTimeUtc -gt (Get-Item $dest).LastWriteTimeUtc){$im=[Drawing.Image]::FromFile($r.file);$ep=[Drawing.Imaging.EncoderParameters]::new(1);try{$ep.Param[0]=[Drawing.Imaging.EncoderParameter]::new([Drawing.Imaging.Encoder]::Quality,[long]94);$im.Save($dest,$codec,$ep)}finally{$ep.Dispose();$im.Dispose()}}
 $copy=$r.Clone();$copy.file=$dest;$copy.format='jpeg';$jpgRecords.Add($copy)
}}
foreach($r in $jpgRecords){$records.Add($r)}
$records|ConvertTo-Json -Depth 6|Set-Content "$PSScriptRoot/manifest-exports.json"
$portraits=@(Get-ChildItem "$out/Portraits128" -Filter '*.png');$cols=8;$rows=[int][Math]::Ceiling($portraits.Count/$cols)
$sheet=[Drawing.Bitmap]::new($cols*160,$rows*160);$g=[Drawing.Graphics]::FromImage($sheet);$font=[Drawing.Font]::new('Arial',9)
try{$g.Clear([Drawing.ColorTranslator]::FromHtml('#101C29'));for($i=0;$i -lt $portraits.Count;$i++){$im=[Drawing.Image]::FromFile($portraits[$i].FullName);try{$x=($i%$cols)*160;$y=[Math]::Floor($i/$cols)*160;$g.DrawImage($im,$x+16,$y,128,128);$g.DrawString($portraits[$i].BaseName,$font,[Drawing.Brushes]::White,$x+3,$y+132)}finally{$im.Dispose()}};$sheet.Save("$PSScriptRoot/qa-portraits.png")}finally{$font.Dispose();$g.Dispose();$sheet.Dispose()}
Write-Output "$($records.Count) technical exports recorded."

