# Catalogue de 250 cartes : vocabulaire, format et quotas

> Spec du 2026-11-16. Suite de `2026-11-16-capacites-composables-synergies-design.md` (objectif de catalogue : au moins 250 cartes différentes, toutes à la sortie). Ce document fixe le **vocabulaire de briques**, le **format des fiches**, les **quotas** et la répartition des 48 cartes existantes. Les fiches elles-mêmes sont dans `Code/docs/catalogue/`, les descriptifs d'illustrations dans `03_Art_UI_UX/Prompts_illustrations_cartes_<section>.md`.

## Composition : 48 existantes + 202 nouvelles = 250
| Section | Existantes | Nouvelles | Total |
|---|---|---|---|
| Brûlure | 2 | 33 | 35 |
| Entrave | 7 | 28 | 35 |
| Traque | 6 | 29 | 35 |
| Tempo | 9 | 26 | 35 |
| Rempart | 16 | 19 | 35 |
| Renouveau | 8 | 27 | 35 |
| Neutres (sans archétype) | 0 | 40 | 40 |

**Étoiles des 202 nouvelles** (total 250 : ★1 90 · ★2 70 · ★3 50 · ★4 28 · ★5 12) :
| Section | ★1 | ★2 | ★3 | ★4 | ★5 | Total |
|---|---|---|---|---|---|---|
| Brûlure | 13 | 9 | 6 | 3 | 2 | 33 |
| Entrave | 11 | 8 | 5 | 3 | 1 | 28 |
| Traque | 11 | 9 | 6 | 2 | 1 | 29 |
| Tempo | 10 | 7 | 5 | 3 | 1 | 26 |
| Rempart | 8 | 6 | 3 | 2 | 0 | 19 |
| Renouveau | 11 | 8 | 5 | 2 | 1 | 27 |
| Neutres | 14 | 11 | 8 | 5 | 2 | 40 |
| **Total** | 78 | 58 | 38 | 20 | 8 | 202 |

**Familles** : les 4 familles (Matière, Élémentaire, Physique/Bio, Perception) sont réparties à parts quasi égales dans chaque section (écart d'au plus 1 carte entre deux familles d'une section ; total par famille 62 ± 3).

## Les 48 cartes existantes et leur archétype
Elles gardent leurs identifiants, valeurs, paliers et noms ; on leur ajoute seulement l'archétype, le rôle et la phase P1.
- **Brûlure** : card_ember, card_inferno.
- **Entrave** : card_shackle, card_prism_burst, card_storm, card_monolith, card_chain_lightning, card_ram_strike, card_telluric_fist.
- **Traque** : card_mark, card_glass_shards, card_bloodletting, card_precise_shot, card_guard_break, card_silent_takedown.
- **Tempo** : card_time_rift, card_precog, card_support_cables, card_spark_rush, card_power_flux, card_side_step, card_guiding_echo, card_shared_watch, card_time_veil.
- **Rempart** : card_glass_wall, card_bulwark_charge, card_concrete_rampart, card_rivet_armor, card_reflecting_prism, card_borrowed_mask, card_frost_wall, card_steam_shield, card_neon_halo, card_ice_carapace, card_steel_skin, card_deep_anchor, card_challenge, card_living_bastion, card_camouflage, card_premonition.
- **Renouveau** : card_second_wind, card_regen_field, card_healing_molt, card_living_forge, card_tonic_current, card_soothing_mist, card_fresh_blood, card_shared_pulse.

## Les 6 archétypes
| Archétype | Idée | Statuts / thème de la frise | Couleur d'illustration |
|---|---|---|---|
| Brûlure | dégâts dans la durée ; bonus sur cible qui brûle | Burn (dégâts à chaque pas de frise) | orange braise, charbon |
| Entrave | contrôler la frise des ennemis | Shock, Freeze, Bind, Push (recul) | cyan froid, violet acier |
| Traque | marquer puis achever | Mark, Vulnerable, Corrosion, Exécution | rouge cramoisi, noir |
| Tempo | accélérer l'équipe | Advance (élan), Haste, jauge | jaune électrique, blanc, traînées de mouvement |
| Rempart | encaisser et protéger | Shield, Anchor, Taunt, Evade | ambre, gris béton |
| Renouveau | tenir dans la durée | Regen, Heal, Purge | vert, or doux |

Accent par famille dans l'illustration : Matière = ambre · Élémentaire = cyan · Physique/Bio = rouge · Perception = violet.

## Vocabulaire de briques
### Effets (inchangés, 11 types)
`Dmg N` dégâts N % ATQ · `Heal N` soin N % PV max de la cible · `Shield N` bouclier N % PV max (2 tours) · `St Statut Nt (C %)` statut N tours, chance C (100 % si absente) · `Push N` recul N % de la frise · `Adv N` élan N % · `Gauge +N` jauge (sur 1 000) · `Steal N` vol de vie N % · `Purge N` (0 = totale) · `Bounce N` · `Exec N`. Statuts : Burn, Freeze, Shock, Corrosion, Blind, Vulnerable, Bind, Mark, Taunt, Shield, Regen, Haste, Evade, Anchor.

### Briques de la phase P1 (jouées à l'utilisation, `Phase = P1`)
- **Condition** : `[si <condition> : <effet ou bonus>]`, évaluée par cible au moment de l'effet. Conditions : `cible a X` · `cible sans X` · `cible < N% PV` · `cible > N% PV` · `lanceur a X` · `lanceur < N% PV` · `un allié a X` · `cible a ≥ N statuts` · `cible a annoncé son action` · `≥ N ennemis vivants` · `≤ N ennemis vivants`.
- **Échelle** : `[+N par <compte> (max M)]` appliquée à la valeur principale. Comptes : `statut de la cible` · `ennemi vivant` · `allié vivant` · `10 % de PV manquants de la cible` · `10 % de PV manquants du lanceur`. Le plafond `max` est obligatoire.
- **Ciblage fin** (remplace la cible par défaut) : `1E:faible` (PV les plus bas en %) · `1E:fort` · `1E:<statut>` (porteur du statut, sinon le premier vivant) · `1E:prochain` (le plus proche d'agir sur la frise) · `1E:aléatoire` · `2E:aléatoires` · `1A:faible` · `1A:prochain` · `1A:<statut>`.

### Briques de la phase P2 (`Phase = P2`)
- **Réaction** : `[Réaction N t : quand <événement> → <effet>]`, armée en jouant la carte, N tours du porteur. Événements : `un ennemi reçoit X` · `un allié reçoit X` · `un ennemi est vaincu` · `un allié est vaincu` · `le porteur est touché` · `le porteur inflige un critique` · `début de tour du porteur`. Une réaction ne répond pas à un événement produit par une réaction.
- **Nouveaux effets** : `Consomme X` (retire le statut X de la cible et déclenche un effet proportionnel) · `Propage X` (copie le statut X de la cible aux autres ennemis).

### Briques de la phase P3 (`Phase = P3`)
- **Échelle d'archétype** : `[+N par carte <archétype> équipée dans l'escouade (max M)]`.

## Format des fiches
Un fichier par section dans `Code/docs/catalogue/cartes-<section>.md`, avec **un seul tableau** de cette forme exacte (l'en-tête est vérifié par un script) :

`| Id | Nom | Name | ★ | Famille | Nature | Archétype | Rôle | Cible | Coût | Phase | Base | Niv.3 | Niv.6 | Niv.10 | Idée |`

- **Id** : `card_<mots_anglais_en_snake_case>`, unique dans tout le catalogue.
- **Nom** : nom français (1 à 3 mots, évocateur, univers urbain et super-héroïque de HeroesDawn), unique. **Name** : nom anglais, unique.
- **★** : 1 à 5. **Famille** : `Matière` · `Élémentaire` · `Physique/Bio` · `Perception`.
- **Nature** (déduite de l'effet de base) : `Offensive` (dégâts sur ennemis) · `Contrôle` (statut de contrôle, recul ou jauge sur ennemis) · `Défensive` (bouclier) · `Soutien` (soin, élan, jauge ou statut sur alliés).
- **Archétype** : un ou deux de Brûlure, Entrave, Traque, Tempo, Rempart, Renouveau, séparés par ` + ` ; `—` pour une carte neutre.
- **Rôle** : `Amorce` (pose ou prépare le statut du thème) · `Gain` (récompense si le thème est présent) · `Pont` (lie deux archétypes : 2 archétypes obligatoires) · `Base` (carte simple et solide).
- **Cible** : `1E` `ZE` `1A` `ZA` `Soi`, éventuellement avec un ciblage fin (`1E:faible`…).
- **Coût** : énergie, par étoile : ★1 et ★2 : 40 ou 50 ; ★3 : 50 ; ★4 : 60 ; ★5 : 70.
- **Phase** : `P1`, `P2` ou `P3` (la plus tardive brique utilisée).
- **Base** : effet de base (niveau 1) en notation ci-dessus, conditions et échelles comprises. **Niv.3 / Niv.6 / Niv.10** : une brique ajoutée à chaque palier (non augmentée par le niveau), parfois avec condition.
- **Idée** : une phrase de 12 mots au plus, qui dit ce qui distingue la carte.

## Règles de conception
- **Distinction.** Deux cartes ne partagent jamais le même nom, la même idée, ni la même signature (cible + archétype + types d'effet de base + forme de condition). Une carte = une idée qu'on dit en une phrase ; les ★4 et ★5 peuvent combiner deux briques, jamais plus de trois.
- **Rôles par section** (cartes nouvelles) : au moins 30 % d'Amorces, au moins 35 % de Gains, au moins 3 Ponts (sections d'archétype). Les Gains citent le statut ou la brique qu'ils lisent, et ce statut est posé par des Amorces de la même section ou d'une section partenaire.
- **Phases** (cartes nouvelles, par section) : au moins 50 % en P1, au plus 35 % en P2, au plus 15 % en P3 ; les P2 et P3 sont surtout des ★3 à ★5.
- **Gabarits de puissance** (niveau 1, tirés de `2026-11-10-rangs-positions-cartes-design.md`) : ★1 dégâts 110 à 120 · ★2 120 à 135 · ★3 100 à 120 plus effet fort, ou zone 70 · ★4 150, zone 110 · ★5 160, zone 120 ; soins ★1 30 (zone 15) ; boucliers ★1 18 à 25 (zone 12 à 15). **Budget d'une brique** : la valeur de base d'une carte à condition ou échelle est d'environ 10 % sous le gabarit, et la condition remplie ajoute 30 à 50 % ; une échelle plafonne à +50 %.
- **Règles de contenu existantes** : une carte qui vise un ennemi ne porte aucun effet allié (base et paliers) ; `Bounce` seulement sur une carte avec `Dmg` ; chaque carte a exactement 3 paliers (niveaux 3, 6, 10).
- **Variété** : dans une section, aucune brique ne sert à plus de 20 % des cartes ; au moins 3 formes de condition ou de ciblage différentes ; pas plus de 6 cartes avec le même statut en effet de base.
- **Statuts à ne pas dupliquer sans raison** : `Taunt` et `Evade` sont rares et fortes ; `Freeze` coupe les pas de frise un sur deux.

## Illustrations (le porteur du projet crée les images)
Un fichier par section : `03_Art_UI_UX/Prompts_illustrations_cartes_<section>.md`. En tête : le **style commun** et le **descriptif négatif** (constantes ci-dessous). Ensuite, **un bloc par carte nouvelle** :

````
### <Nom> — `<id>` · ★<n> · <Nature>
```
<scène en anglais : silhouette d'un personnage, son action, le lieu, la lumière, la palette de l'archétype et l'accent de la famille>
```
````

Style commun (à coller après chaque scène) : `stylized semi-3D animated cinematic still, low-poly faceted shading with visible polygonal facets on figures, clothing and rocks, cel-shaded lighting with soft painterly texture, desaturated dark shadows, dramatic rim light, dynamic camera angle (low angle or tight close-up), shallow depth of field, cinematic 16:9 framing, graphic novel animation quality`

Descriptif négatif : `photorealistic, 3D render, plastic skin, extra fingers, deformed hands, blurry face, asymmetrical eyes, text, watermark, logo, cartoon chibi, pastel colors, bright daylight, cape covering face`

Règles : chaque scène est visuellement différente des autres (personnage, geste, lieu, cadrage) ; un même lieu n'apparaît pas plus de 3 fois dans un fichier ; la scène montre l'effet de la carte (une brûlure qui court, un bouclier qui se forme, une cible marquée…) ; fichier à enregistrer sous le nom `<id>.jpg` dans `03_Art_UI_UX/Illustrations/Competences/`. Les 48 cartes existantes ont déjà leur illustration.

## Vérification (script)
`Code/docs/catalogue/verifier-catalogue.ps1` lit tous les tableaux et contrôle : en-tête, 250 cartes au total, identifiants, noms et idées uniques, quotas d'étoiles et de familles par section, rôles, phases, trois paliers renseignés, coûts par étoile, absence d'effet allié sur une carte qui cible un ennemi, `Bounce` seulement avec `Dmg`.
