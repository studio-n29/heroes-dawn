param([string]$Workspace = 'C:/Users/dnath/Documents/Heroes Dawn')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
[System.Threading.Thread]::CurrentThread.CurrentCulture = [System.Globalization.CultureInfo]::InvariantCulture
$kitRoot = Join-Path $Workspace 'Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueExtension'
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


