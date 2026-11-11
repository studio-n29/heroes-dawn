param([string[]]$Categories=@('Cards','Skills','Items','Dossiers','Poses','Enemies','Scenes','MiniGames','Boards','Zones','Rivals'))
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueExtension'
foreach($cat in $Categories){
 $files=@(Get-ChildItem "$root/$cat" -Filter '*.png' -ErrorAction SilentlyContinue)
 if(!$files.Count){continue}
 $cols=6;$cw=220;$ch=170;$rows=[int][Math]::Ceiling($files.Count/$cols)
 $bm=[Drawing.Bitmap]::new($cols*$cw,$rows*$ch);$g=[Drawing.Graphics]::FromImage($bm);$font=[Drawing.Font]::new('Arial',9)
 try{$g.Clear([Drawing.ColorTranslator]::FromHtml('#101c29'));$g.InterpolationMode='HighQualityBicubic'
 for($i=0;$i -lt $files.Count;$i++){$im=[Drawing.Image]::FromFile($files[$i].FullName);try{$x=($i%$cols)*$cw;$y=[Math]::Floor($i/$cols)*$ch;$scale=[Math]::Min(($cw-8)/$im.Width,140/$im.Height);$w=[int]($im.Width*$scale);$h=[int]($im.Height*$scale);$g.FillRectangle([Drawing.Brushes]::SlateGray,$x+2,$y+2,$cw-4,142);$g.DrawImage($im,[int]($x+($cw-$w)/2),[int]($y+(140-$h)/2),$w,$h);$g.DrawString($files[$i].BaseName,$font,[Drawing.Brushes]::White,[Drawing.RectangleF]::new($x+3,$y+144,$cw-6,25))}finally{$im.Dispose()}}
 $safe=$cat.Replace("/","-");$bm.Save("$PSScriptRoot/qa-$safe.png")
 }finally{$font.Dispose();$g.Dispose();$bm.Dispose()}
 Write-Output "$cat : $($files.Count)"
}

