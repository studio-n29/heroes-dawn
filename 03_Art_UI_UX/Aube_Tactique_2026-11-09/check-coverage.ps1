$ErrorActionPreference='Stop'
$root=Join-Path (Split-Path (Split-Path $PSScriptRoot)) 'Code/Heroes Dawn Unity/Assets/_Project/Art/UI/AubeTactiqueExtension'
$expected=[Collections.Generic.List[object]]::new()
function Add-Expected($category,$name,$source){$path=Join-Path $root "$category/$name.png";$expected.Add(@{id="$category/$name";source=$source;file=$path;exists=(Test-Path -LiteralPath $path)})}
Add-Expected 'Agents' 'giulia_rossi-cutout' 'prompt-giulia.json'
Add-Expected 'MiniGames' 'gear-body' 'prompt-gear.json'
foreach($id in @('ny-01','ny-02','ny-03','ny-04','mos-01','mos-02','mos-03','mos-04')){Add-Expected 'ChaptersLocked' $id 'prompts-chapters-locked.json'}
foreach($j in (Get-Content "$PSScriptRoot/prompts-agents.json" -Raw|ConvertFrom-Json)){Add-Expected 'Agents' ($j.id+'-cutout') 'prompts-agents.json'}
foreach($j in (Get-Content "$PSScriptRoot/prompts-cards.json" -Raw|ConvertFrom-Json)){Add-Expected 'Cards' $j.id 'prompts-cards.json'}
foreach($file in @('prompts-skills.json','prompts-sprites.json','prompts-environments.json','prompts-zones.json','prompts-supplement.json')){foreach($j in (Get-Content (Join-Path $PSScriptRoot $file) -Raw|ConvertFrom-Json)){Add-Expected $j.category $j.id $file}}
foreach($j in (Get-Content "$PSScriptRoot/manifest-native.json" -Raw|ConvertFrom-Json)){Add-Expected $j.category (($j.id -split '/')[1]) 'manifest-native.json'}
$duplicates=@($expected|Group-Object id|Where-Object Count -gt 1)
if($duplicates.Count){throw ('Duplicate planned asset ids: '+($duplicates.Name -join ', '))}
$missing=@($expected|Where-Object {-not $_.exists})
$report=@{planned=$expected.Count;present=$expected.Count-$missing.Count;missing=$missing;items=$expected}
$report|ConvertTo-Json -Depth 6|Set-Content "$PSScriptRoot/coverage-generated.json"
$summary=@($expected|Group-Object {$_.id.Split('/')[0]}|ForEach-Object {@{category=$_.Name;planned=$_.Count;present=@($_.Group|Where-Object exists).Count}})
$summary|ConvertTo-Json -Compress
Write-Output ("Present {0}/{1}; missing {2}" -f $report.present,$report.planned,$missing.Count)


