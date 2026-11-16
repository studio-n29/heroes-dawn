# Vérifie les fiches du catalogue de 250 cartes (voir docs/specs/2026-11-16-catalogue-250-cartes.md).
# Usage : powershell -File verifier-catalogue.ps1   (code de sortie 1 s'il y a des erreurs)
$ErrorActionPreference = 'Stop'
$dir = $PSScriptRoot
$header = '| Id | Nom | Name | ★ | Famille | Nature | Archétype | Rôle | Cible | Coût | Phase | Base | Niv.3 | Niv.6 | Niv.10 | Idée |'
$sections = [ordered]@{
  'cartes-existantes' = @{ Name = 'Existantes'; New = 48 }
  'cartes-brulure'    = @{ Name = 'Brûlure';    New = 33; Stars = 13,9,6,3,2 }
  'cartes-entrave'    = @{ Name = 'Entrave';    New = 28; Stars = 11,8,5,3,1 }
  'cartes-traque'     = @{ Name = 'Traque';     New = 29; Stars = 11,9,6,2,1 }
  'cartes-tempo'      = @{ Name = 'Tempo';      New = 26; Stars = 10,7,5,3,1 }
  'cartes-rempart'    = @{ Name = 'Rempart';    New = 19; Stars = 8,6,3,2,0 }
  'cartes-renouveau'  = @{ Name = 'Renouveau';  New = 27; Stars = 11,8,5,2,1 }
  'cartes-neutres'    = @{ Name = 'Neutres';    New = 40; Stars = 14,11,8,5,2 }
}
$families = 'Matière','Élémentaire','Physique/Bio','Perception'
$natures  = 'Offensive','Contrôle','Défensive','Soutien'
$archs    = 'Brûlure','Entrave','Traque','Tempo','Rempart','Renouveau'
$roles    = 'Amorce','Gain','Pont','Base'
$errors = New-Object System.Collections.Generic.List[string]
$warns  = New-Object System.Collections.Generic.List[string]
function Err($m) { $script:errors.Add($m) }
function Warn($m) { $script:warns.Add($m) }

$all = @()
foreach ($key in $sections.Keys) {
  $path = Join-Path $dir "$key.md"
  if (-not (Test-Path $path)) { Err "fichier manquant : $key.md"; continue }
  $lines = Get-Content -Path $path -Encoding UTF8
  $idx = [array]::IndexOf(($lines | ForEach-Object { $_.Trim() }), $header)
  if ($idx -lt 0) { Err "$key.md : en-tête de tableau absent ou différent"; continue }
  $rows = @()
  for ($i = $idx + 2; $i -lt $lines.Count; $i++) {
    $l = $lines[$i].Trim()
    if (-not $l.StartsWith('|')) { break }
    $cells = $l.Trim('|').Split('|') | ForEach-Object { $_.Trim() }
    if ($cells.Count -ne 16) { Err "$key.md ligne $($i+1) : $($cells.Count) colonnes au lieu de 16"; continue }
    $rows += [pscustomobject]@{ File = $key; Section = $sections[$key].Name; Line = $i + 1
      Id = $cells[0]; Nom = $cells[1]; Name = $cells[2]; Stars = $cells[3]; Family = $cells[4]; Nature = $cells[5]
      Arch = $cells[6]; Role = $cells[7]; Cible = $cells[8]; Cost = $cells[9]; Phase = $cells[10]
      Base = $cells[11]; T3 = $cells[12]; T6 = $cells[13]; T10 = $cells[14]; Idea = $cells[15] }
  }
  if ($rows.Count -ne $sections[$key].New) { Err "$key.md : $($rows.Count) cartes au lieu de $($sections[$key].New)" }
  $all += $rows
}

# Unicité
foreach ($f in 'Id','Nom','Name','Idea') {
  $all | Group-Object $f | Where-Object { $_.Count -gt 1 } | ForEach-Object { Err "$f en double : '$($_.Name)' ($($_.Group | ForEach-Object { $_.File + ':' + $_.Line }))" }
}
if ($all.Count -ne 250) { Err "total : $($all.Count) cartes au lieu de 250" }

$isEnemy = { param($c) $c -match '^(1E|ZE|2E)' }
foreach ($r in $all) {
  $w = "$($r.File):$($r.Line) $($r.Id)"
  if ($r.Id -notmatch '^card_[a-z0-9_]+$') { Err "$w : identifiant invalide" }
  if ($r.Stars -notmatch '^[1-5]$') { Err "$w : étoiles '$($r.Stars)'"; continue }
  $s = [int]$r.Stars
  if ($families -notcontains $r.Family) { Err "$w : famille '$($r.Family)'" }
  if ($natures -notcontains $r.Nature) { Err "$w : nature '$($r.Nature)'" }
  if ($roles -notcontains $r.Role) { Err "$w : rôle '$($r.Role)'" }
  if ($r.Phase -notmatch '^P[123]$') { Err "$w : phase '$($r.Phase)'" }
  if ($r.Cible -notmatch '^(1E|ZE|1A|ZA|Soi|2E)(:\S+)?$') { Err "$w : cible '$($r.Cible)'" }
  if ($r.Cost -notmatch '^\d+$') { Err "$w : coût '$($r.Cost)'" } else {
    $c = [int]$r.Cost
    $ok = switch ($s) { 1 { $c -in 40,50 } 2 { $c -in 40,50 } 3 { $c -eq 50 } 4 { $c -eq 60 } 5 { $c -eq 70 } }
    if (-not $ok) { if ($r.File -eq 'cartes-existantes') { Warn "$w : coût $c inhabituel pour ★$s" } else { Err "$w : coût $c invalide pour ★$s" } }
  }
  # Archétype
  $parts = $r.Arch -split '\s*\+\s*'
  if ($r.Arch -eq '—') { if ($r.Section -notin 'Neutres') { Err "$w : carte neutre dans la section $($r.Section)" } }
  else {
    foreach ($p in $parts) { if ($archs -notcontains $p) { Err "$w : archétype '$p'" } }
    if ($r.Section -notin 'Neutres','Existantes' -and $parts[0] -ne $r.Section) { Err "$w : archétype '$($r.Arch)' hors section $($r.Section)" }
    if ($r.Section -eq 'Neutres') { Err "$w : archétype dans la section Neutres" }
  }
  if ($r.Role -eq 'Pont' -and $parts.Count -ne 2) { Err "$w : un Pont porte deux archétypes" }
  if ($r.Role -ne 'Pont' -and $parts.Count -gt 1) { Warn "$w : deux archétypes sans rôle Pont" }
  # Paliers
  foreach ($t in 'T3','T6','T10') { if ([string]::IsNullOrWhiteSpace($r.$t) -or $r.$t -eq '—') { Err "$w : palier $t vide" } }
  if ([string]::IsNullOrWhiteSpace($r.Base)) { Err "$w : base vide" }
  # Règles de contenu
  $cells = @($r.Base, $r.T3, $r.T6, $r.T10)
  if (& $isEnemy $r.Cible) {
    foreach ($cell in $cells) {
      $clean = $cell -replace '\[si [^:\]]*:', '['   # on retire la condition (avant « : »), pas l'effet
      if ($clean -match '\b(Heal|Shield|Adv)\b|Gauge \+|St (Regen|Haste|Anchor|Evade|Taunt|Shield)\b') { Err "$w : effet allié sur une carte qui cible un ennemi ('$cell')" }
    }
  }
  if (($cells -join ' ') -match 'Bounce' -and $r.Base -notmatch 'Dmg') { Err "$w : Bounce sans Dmg de base" }
}

# Totaux par étoile et par famille
$want = 90,70,50,28,12
for ($s = 1; $s -le 5; $s++) { $n = @($all | Where-Object { $_.Stars -eq "$s" }).Count; if ($n -ne $want[$s-1]) { Err "total ★$s : $n au lieu de $($want[$s-1])" } }
foreach ($f in $families) { $n = @($all | Where-Object { $_.Family -eq $f }).Count; if ([math]::Abs($n - 62.5) -gt 3.5) { Err "famille $f : $n cartes (attendu 62 ± 3)" } }

# Quotas par section (cartes nouvelles)
foreach ($key in $sections.Keys) {
  if ($key -eq 'cartes-existantes') { continue }
  $sec = $sections[$key]; $rows = @($all | Where-Object { $_.File -eq $key }); if ($rows.Count -eq 0) { continue }
  for ($s = 1; $s -le 5; $s++) { $n = @($rows | Where-Object { $_.Stars -eq "$s" }).Count; if ($n -ne $sec.Stars[$s-1]) { Err "$($sec.Name) ★$s : $n au lieu de $($sec.Stars[$s-1])" } }
  $fam = $families | ForEach-Object { $f = $_; @($rows | Where-Object { $_.Family -eq $f }).Count }
  if (($fam | Measure-Object -Maximum).Maximum - ($fam | Measure-Object -Minimum).Minimum -gt 1) { Err "$($sec.Name) : familles déséquilibrées ($($fam -join '/'))" }
  $n = $rows.Count
  $p1 = @($rows | Where-Object { $_.Phase -eq 'P1' }).Count; $p2 = @($rows | Where-Object { $_.Phase -eq 'P2' }).Count; $p3 = @($rows | Where-Object { $_.Phase -eq 'P3' }).Count
  if ($p1 * 2 -lt $n) { Err "$($sec.Name) : P1 = $p1/$n (min 50 %)" }
  if ($p2 * 100 -gt 35 * $n) { Err "$($sec.Name) : P2 = $p2/$n (max 35 %)" }
  if ($p3 * 100 -gt 15 * $n) { Err "$($sec.Name) : P3 = $p3/$n (max 15 %)" }
  if ($sec.Name -ne 'Neutres') {
    $am = @($rows | Where-Object { $_.Role -eq 'Amorce' }).Count; $ga = @($rows | Where-Object { $_.Role -eq 'Gain' }).Count; $po = @($rows | Where-Object { $_.Role -eq 'Pont' }).Count
    if ($am * 100 -lt 30 * $n) { Err "$($sec.Name) : Amorces $am/$n (min 30 %)" }
    if ($ga * 100 -lt 35 * $n) { Err "$($sec.Name) : Gains $ga/$n (min 35 %)" }
    if ($po -lt 3) { Err "$($sec.Name) : Ponts $po (min 3)" }
  } else {
    $cond = @($rows | Where-Object { $_.Base -match '\[(si|\+)' -or $_.Cible -match ':' }).Count
    if ($cond * 100 -lt 40 * $n) { Err "Neutres : cartes à condition/échelle/ciblage $cond/$n (min 40 %)" }
  }
  $stat = @($rows | ForEach-Object { if ($_.Base -match 'St (\w+)') { $Matches[1] } } | Group-Object | Where-Object { $_.Count -gt 6 })
  foreach ($g in $stat) { Warn "$($sec.Name) : statut $($g.Name) en effet de base sur $($g.Count) cartes (max conseillé 6)" }
}

Write-Host "Cartes lues : $($all.Count)"
Write-Host ("Étoiles : " + ((1..5 | ForEach-Object { $s = $_; "★$s=" + @($all | Where-Object { $_.Stars -eq "$s" }).Count }) -join ' '))
Write-Host ("Familles : " + (($families | ForEach-Object { $f = $_; "$f=" + @($all | Where-Object { $_.Family -eq $f }).Count }) -join ' '))
Write-Host ("Archétypes (première position) : " + ((($archs + '—') | ForEach-Object { $a = $_; "$a=" + @($all | Where-Object { ($_.Arch -split '\s*\+\s*')[0] -eq $a }).Count }) -join ' '))
Write-Host ("Phases : " + (('P1','P2','P3' | ForEach-Object { $p = $_; "$p=" + @($all | Where-Object { $_.Phase -eq $p }).Count }) -join ' '))
$warns | ForEach-Object { Write-Host "AVERTISSEMENT : $_" }
$errors | ForEach-Object { Write-Host "ERREUR : $_" }
Write-Host "$($errors.Count) erreur(s), $($warns.Count) avertissement(s)"
if ($errors.Count -gt 0) { exit 1 }
