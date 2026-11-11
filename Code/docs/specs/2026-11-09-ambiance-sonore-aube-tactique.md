# Ambiance sonore Aube tactique — Spécification

Date : 2026-11-09. Statut : à relire. Complète `2026-11-04-aube-tactique-ui.md` (la DA) ; l'inventaire `inventaire-assets-aube-tactique.md` §2.4 listait l'audio « à confirmer ».

## Intention

Donner au jeu une identité sonore cohérente avec Aube tactique : sobre, tactique, institutionnelle. Les musiques sont générées avec Suno, les effets avec l'outil de génération de SFX. L'ensemble remplace les sons génériques actuels (packs de type Kenney) qui jurent avec une interface mate, marine et laiton.

Décisions de l'utilisateur (9 novembre 2026) :
- Identité : thriller d'agence moderne (électronique feutrée), avec des touches orchestrales **réservées aux moments forts** (titre, boss, victoire, révélation de booster, cinématique).
- SFX : génération IA sobre et cohérente, validée à l'écoute avant intégration.
- Un thème **par ville**, sur le **plateau seulement**. QG, boutique et combat restent communs.

Hors périmètre : refonte de `AudioService`, voix de cinématique, « Four-note awakening » (conservé), musique de combat ou de QG propre à chaque ville, stems.

## Charte sonore

**Socle** : électronique sobre, basses sourdes, pulsations discrètes, nappes. Plateau 70–100 BPM, combat 110–130 BPM. Pas de chant avec paroles (instrumental uniquement). **Signature** : un motif de quatre notes, repris par chaque thème (clin d'œil à « Four-note awakening »), pour que l'ensemble sonne comme une seule œuvre. Les touches orchestrales (cordes, cuivres lointains, percussions amples) ne servent qu'aux moments forts.

| Ville | Couleur |
|---|---|
| New York | Synthés froids et métalliques, basses de métro, caisse claire sèche, tension nocturne |
| Paris | Piano feutré, contrebasse, textures de vinyle, jazz sombre, intrigue élégante |
| Moscou | Cordes graves, résonances glaciales, drones, chœurs lointains, percussions lourdes |
| Salem | Nappes brumeuses, bois et harpe désaccordée, dissonances discrètes, ambiance occulte |

## Musiques : 15 pistes Suno

| MusicId | Dossier | Pistes | Orchestre |
|---|---|---|---|
| MainTheme | `MainTheme` | 1 | oui |
| Headquarters | `Headquarters` | 2 | cordes discrètes |
| BoardNewYork | `BoardNewYork` | 2 | non |
| BoardParis | `BoardParis` | 2 | non |
| BoardMoscow | `BoardMoscow` | 2 | non |
| BoardSalem | `BoardSalem` | 2 | non |
| Combat | `Combat` | 2 | non |
| Boss | `Boss` | 1 | complet |
| Shop (nouveau) | `Shop` | 1 | non |
| Cinematic | conservé | 0 | déjà orchestral |

Les deux pistes `Board` actuelles sont remplacées. `Board` reste comme repli (ville inconnue).

### Prompts Suno (modèles à ajuster à l'écoute)

Réglages communs : *Instrumental* activé, pas de paroles. Chaque prompt est à coller dans « Style of Music ». Les durées visées sont de 2 à 3 minutes (le moteur enchaîne les pistes en fondu de 4 s).

- **MainTheme** : `cinematic spy thriller theme, dark ambient electronic with swelling strings and distant brass, slow build, solemn, four-note motif, 78 bpm, instrumental`
- **Headquarters** : `warm minimal electronic, operations room at dawn, soft pads, gentle pulse, subtle strings, hopeful and calm, brass-gold warmth, 85 bpm, instrumental`
- **BoardNewYork** : `cold metallic synths, subway sub-bass, dry snare, nocturnal urban tension, sparse, 90 bpm, instrumental, four-note motif`
- **BoardParis** : `dark jazz noir, muted piano, upright bass, vinyl crackle, soft brushed drums, elegant intrigue, 80 bpm, instrumental, four-note motif`
- **BoardMoscow** : `icy dark ambient, low cellos, glacial resonances, deep drones, distant choir pads, heavy slow percussion, 72 bpm, instrumental, four-note motif`
- **BoardSalem** : `misty occult ambient, detuned harp, hollow woodwinds, fog pads, subtle dissonance, ghostly, 70 bpm, instrumental, four-note motif`
- **Combat** : `tactical electronic combat, driving pulse, tight percussion, tense bass, restrained, no orchestra, 120 bpm, instrumental`
- **Boss** : `epic cinematic orchestral boss theme, full orchestra, powerful brass, war drums, rising choir, dramatic climax, 110 bpm, instrumental`
- **Shop** : `dark lounge electronic, soft keys, muted bass, slow brushed beat, calm and refined, 75 bpm, instrumental`

Pour chaque thème : générer 4 variantes, garder les 1 ou 2 meilleures, exporter en WAV ou MP3 320, normaliser aux alentours de −16 LUFS, couper les silences de fin. Version 2 pistes : deuxième prompt = même style avec « variation, different melody ».

## Effets sonores (génération IA)

Style : feutré, métal mat, relais, papier, électronique discrète. Jamais de sons « arcade ».

- **Interface** : UiClick, UiBack, UiConfirm, UiError, PopupOpen, PopupClose, ToastInfo, ToastWarning, LoginStart.
- **Récompenses (touche orchestrale légère)** : ToastReward, ZoneComplete, Victory, Defeat.
- **Plateau** : DiceShake, DiceThrow, PawnMove, CaseEvent, CombatAlert, MiniGameHit, MiniGameMiss.
- **QG** : Recruit, Train, Heal.
- **Combat** : conservés pour l'instant (impacts actuels) ; seuls Combo, Status, Shield, HealFx sont remplacés s'ils détonnent à l'écoute.
- **Nouveaux `SfxId`** : XpGain, BoosterOpen, RewardTile, TentRest (valeurs libres à la suite de la plage existante, ex. 80–83).

Chaque son est validé à l'écoute avant d'écraser le clip précédent. Les anciens clips sont conservés dans l'historique git.

## Intégration technique

- `MusicId` : ajout de `BoardNewYork`, `BoardParis`, `BoardMoscow`, `BoardSalem`, `Shop` (valeurs à la suite, sans renuméroter l'existant pour ne pas casser le catalogue sérialisé).
- `MusicDirector.For(state, city, boss, shop)` (signature à fixer au plan) :
  - Board → thème de la ville de la zone en cours (`session.RunZone.Borough` → `StoryRoutes.CityOfBorough`), `Board` si inconnue.
  - Combat → `Boss` si le combat est de rang `CombatRank.Boss`, sinon `Combat`.
  - Hub → `Shop` sur la page Boutique, sinon `Headquarters`.
- La reprise de position après un combat continue de s'appliquer au thème du plateau de la ville.
- Un signal de page du hub vers l'audio (ou lecture de la page courante par le directeur) pour `Shop` : mécanisme à trancher au plan, sans coupler la présentation du hub à `AudioService`.
- Catalogue : dépôt des fichiers dans `Audio/Music/<MusicId>/` puis « Heroes Dawn ▸ Setup ▸ Audio ▸ Reconstruire le catalogue ». Volume, fondus et ducking se règlent ensuite dans l'inspecteur (`fadeIn`, `trackCrossfade`, `duckMusicTo`).
- `AudioService` : aucun changement.

## Vérification

- Tests unitaires de `MusicDirector.For` : chaque ville × état, repli, boss, boutique.
- Contrôle d'écoute par thème, enchaînement et fondu entre deux pistes, retour combat → plateau.
- Mixage : voix et SFX restent audibles sous la musique, jingles de victoire sans écraser le thème suivant.
- Contrôle mobile : poids des fichiers (MP3/Vorbis en streaming, WAV évités), pas de pic mémoire.
- Tests Unity lancés en batch comme d'habitude. La qualité musicale ne se vérifie qu'à l'écoute par l'utilisateur.

## Limites et risques

- Suno produit des morceaux, pas des boucles parfaites : l'enchaînement par fondu du moteur compense.
- Le motif de quatre notes n'est pas garanti par Suno. À obtenir par itération, ou à imposer en uploadant un extrait de référence.
- Suno : vérifier les droits d'usage commercial selon l'offre souscrite avant publication du jeu.
- Les prompts sont des points de départ : les résultats seront ajustés à l'écoute.
