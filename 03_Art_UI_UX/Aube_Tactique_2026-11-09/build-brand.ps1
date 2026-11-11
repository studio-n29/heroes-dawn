. "$PSScriptRoot/build-extension.ps1"
$backup="$PSScriptRoot/RejectedBrandDrafts";[IO.Directory]::CreateDirectory($backup)|Out-Null
foreach($id in @('apple','google')){if(!(Test-Path "$backup/$id.png")){Copy-Item "$kitRoot/BrandIcons/$id.png" "$backup/$id.png"}}
Kit-New 'BrandIcons' 'google' 128 128 @(0,0,0,0) 96 96
$p=@();for($a=-45;$a -ge -360;$a-=3){$r=$a*[Math]::PI/180;$p+=48+35*[Math]::Cos($r);$p+=48+35*[Math]::Sin($r)}
for($a=-360;$a -le -45;$a+=3){$r=$a*[Math]::PI/180;$p+=48+23*[Math]::Cos($r);$p+=48+23*[Math]::Sin($r)}
Kit-Poly $p '#FFFFFF';Kit-Poly @(48,44,83,44,83,56,48,56) '#FFFFFF';Kit-Save 'Pictogramme G monochrome pour connexion.'
Kit-New 'BrandIcons' 'apple' 128 128 @(0,0,0,0) 96 96
$path=[Drawing.Drawing2D.GraphicsPath]::new()
$segments=@(@(48,34,35,24,19,29,17,45),@(17,45,13,61,27,84,36,84),@(36,84,42,84,43,79,49,79),@(49,79,55,79,57,84,63,84),@(63,84,73,84,82,68,85,59),@(85,59,69,54,68,40,81,34),@(81,34,73,24,63,25,54,30),@(54,30,51,32,49,34,48,34))
foreach($v in $segments){$path.AddBezier([single]$v[0],[single]$v[1],[single]$v[2],[single]$v[3],[single]$v[4],[single]$v[5],[single]$v[6],[single]$v[7])};$path.CloseFigure();$path.StartFigure();$path.AddBezier(48,29,46,18,54,9,65,8);$path.AddBezier(65,8,65,20,58,28,48,29);$path.CloseFigure()
$kitGraphics.FillPath([Drawing.Brushes]::White,$path);$path.Dispose()
[void]$kitSvg.AppendLine('<path fill="#FFFFFF" d="M48 34 C35 24 19 29 17 45 C13 61 27 84 36 84 C42 84 43 79 49 79 C55 79 57 84 63 84 C73 84 82 68 85 59 C69 54 68 40 81 34 C73 24 63 25 54 30 C51 32 49 34 48 34 Z M48 29 C46 18 54 9 65 8 C65 20 58 28 48 29 Z"/>')
Kit-Save 'Pictogramme pomme monochrome pour connexion.'
$kitRecords|ConvertTo-Json -Depth 5|Set-Content "$PSScriptRoot/manifest-brand.json"
