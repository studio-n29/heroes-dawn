# Onomatopées, narration et brouillard visuel (sous-projets H, I, O)

## H — Onomatopées de bande dessinée
- `ComicCues` : un type de case d'action (Combat, Événement, Défi, Repos) a une étoile de texte (« COMBAT ! »…) ; BANG et OUTCH servent aux coups et aux reculs.
- `BoardView.PlayCueAsync` joue `ComicBurstView` : l'étoile surgit (rebond), tremble (sauf mouvements réduits), s'efface (~1 s). Le présentateur la joue avant d'ouvrir la case ou de lancer le combat, et « OUTCH ! » au recul après une défaite.

## I — Narration avant / après une case
- `NodeDef.Narration` (`NarrationDef` : répliques `Before` et `After`). Chaque réplique : locuteur (identifiant d'agent ou de personnage) + clé de texte.
- Domaine : `ZoneRun` émet `NarrationEvent` une fois par plateau et par moment : « avant » à l'arrivée (avant le déclenchement), « après » à la résolution (victoire, case résolue) ou tout de suite pour une case sans action. Une défaite ne joue pas la conclusion.
- Présentation : `DialogueBoxView` (avatar du locuteur, plaque de nom, bulle, texte qui s'écrit, toucher pour avancer, « Passer »). Agents à gauche, autres personnages à droite.
- Outil de level design : `NodeData.narrationBefore/After` (locuteur + texte) ; `ZoneAssetConverter` produit les clés `narration.<zone>.<case>.<before|after>.<i>` et leurs textes.
- Exemple : combat d'élite de Brick (zone 1.1, case n23).

## O — Brouillard de guerre visuel
- `FogCloudView` : deux nuages par case sous le brouillard (entre chemins et cases), dérive lente, fondu à la révélation ; `BoardMapView.BuildFog` / `Refresh`.
- Sprites générés `Burst` et `Cloud` (Resources/UI).
