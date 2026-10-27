# Catalogue : 36 nouvelles cartes (lot C2 de la 0.14.0)

> Spec du 2026-10-27. Source : document N29 HQ, point « Liste des cartes de compétences » (assez de cartes pour un vrai deck building). Suite de C1 (`2026-10-26-cartes-paliers-mecaniques-design.md`) : moteur à 10 niveaux, paliers 3/6/10, vol de vie, purge, rebond, exécution. S'appuie sur les catégories du lot B (famille × nature).

## Décisions
- **48 cartes** au total : **12 par famille**, **4 par nature** dans chaque famille ; 36 nouvelles s'ajoutent aux 12 existantes (inchangées).
- **Raretés par famille** : 5 Communes, 4 Rares, 2 Épiques, 1 Légendaire (48 cartes : 20 / 16 / 8 / 4). Taux des boosters inchangés (70 / 22 / 7 / 1, pitié tous les 5).
- **Illustrations** : recadrages teintés de l'art du Drive (`03_Art_UI_UX/`), une source différente par carte, comme les 12 premières.

## Gabarits de puissance (base au niveau 1)
| Rareté | Recharge (coût = recharge × 10) | Dégâts 1 cible | Dégâts zone | Soin 1 / zone | Bouclier 1 / zone / soi |
|---|---|---|---|---|---|
| Commune | 4–5 | 110–130 | — | 30 / 15 | 18–20 / — / 25 |
| Rare | 5 | 100–120 (+ effet fort) | 70 | — / 18 | — / 12 / 20 |
| Épique | 6 | 150 | 110 | — / 15 | — / 15–18 / — |
| Légendaire | 7 | 160 | 120 | — | — |

Règles reprises de C1 : une carte qui vise un ennemi ne porte aucun effet allié (base et paliers) ; `Bounce` seulement sur une carte avec `Damage` ; chaque carte a 3 paliers (niveaux 3, 6, 10). La **nature saisie est égale à la nature déduite** de l'effet de base (Dégâts sur ennemi : Offensive ; Bouclier : Défensive ; Soin, élan, jauge ou statut allié : Soutien).

## Notation
`Dmg N` dégâts N % ATQ · `Heal N` / `Shield N` % PV max (bouclier : 2 tours) · `St Statut Nt (C %)` statut N tours (chance C, 100 % si absente) · `Push N` / `Adv N` recul / élan N % · `Gauge +N` jauge +N/1000 · `Steal N`, `Purge N` (0 = totale), `Bounce N`, `Exec N`. Cibles : 1E (un ennemi), ZE (tous les ennemis), 1A (un allié), ZA (tous les alliés), Soi.

## Matière (ambre) — 9 nouvelles
| Id | Nom | Rareté | Nature | Cible | Rech. | Base | Niv. 3 | Niv. 6 | Niv. 10 |
|---|---|---|---|---|---|---|---|---|---|
| card_glass_shards | Éclats de verre | Commune | Off. | 1E | 4 | Dmg 120, St Vulnerable 1t (30 %) | Bounce 40 | St Corrosion 2t | Exec 30 |
| card_concrete_rampart | Rempart de béton | Commune | Déf. | 1A | 5 | Shield 20 | St Anchor 1t | Purge 1 | St Regen 2t |
| card_rivet_armor | Armure de rivets | Commune | Déf. | Soi | 4 | Shield 25 | St Taunt 1t | St Anchor 1t | St Regen 2t |
| card_healing_molt | Mue réparatrice | Commune | Sout. | 1A | 5 | Heal 30 | Purge 1 | St Regen 2t | St Haste 1t |
| card_reflecting_prism | Prisme réfléchissant | Rare | Déf. | ZA | 5 | Shield 12 | St Evade 1t (30 %) | Purge 1 | St Anchor 1t |
| card_support_cables | Filins d'appui | Rare | Sout. | 1A | 5 | Adv 40, St Anchor 1t | Shield 8 | Purge 0 | St Haste 1t |
| card_borrowed_mask | Masque d'emprunt | Rare | Sout. | Soi | 5 | St Evade 2t | St Haste 1t | Purge 0 | Shield 15 |
| card_living_forge | Forge vivante | Épique | Sout. | ZA | 6 | Heal 15 | St Regen 2t | Purge 1 | Shield 10 |
| card_monolith | Monolithe | Légendaire | Off. | ZE | 7 | Dmg 120, St Bind 1t | St Vulnerable 2t | Steal 15 | Exec 40 |

## Élémentaire (cyan) — 9 nouvelles
| Id | Nom | Rareté | Nature | Cible | Rech. | Base | Niv. 3 | Niv. 6 | Niv. 10 |
|---|---|---|---|---|---|---|---|---|---|
| card_frost_wall | Mur de givre | Commune | Déf. | 1A | 5 | Shield 20 | Purge 1 | St Anchor 1t | St Regen 2t |
| card_steam_shield | Bouclier de vapeur | Commune | Déf. | Soi | 4 | Shield 25 | St Evade 1t (30 %) | Purge 1 | St Regen 2t |
| card_tonic_current | Courant tonifiant | Commune | Sout. | 1A | 5 | Heal 30 | St Haste 1t | Purge 1 | St Regen 2t |
| card_soothing_mist | Brume apaisante | Commune | Sout. | ZA | 5 | Heal 15 | Purge 1 | St Regen 2t | Shield 8 |
| card_neon_halo | Halo de néon | Rare | Déf. | ZA | 5 | Shield 12 | Purge 1 | St Evade 1t (30 %) | St Regen 2t |
| card_spark_rush | Étincelle d'élan | Rare | Sout. | 1A | 5 | Adv 50 | St Haste 1t | Shield 10 | Purge 0 |
| card_power_flux | Flux électrique | Rare | Sout. | ZA | 5 | Gauge +100 | St Haste 1t | Purge 1 | Shield 8 |
| card_ice_carapace | Carapace de glace | Épique | Déf. | ZA | 6 | Shield 18 | St Anchor 1t | Purge 1 | St Regen 2t |
| card_chain_lightning | Foudre en chaîne | Légendaire | Off. | 1E | 7 | Dmg 160, St Shock 1t (40 %) | Bounce 60 | St Vulnerable 2t | Exec 50 |

## Physique / Bio (rouge) — 9 nouvelles
| Id | Nom | Rareté | Nature | Cible | Rech. | Base | Niv. 3 | Niv. 6 | Niv. 10 |
|---|---|---|---|---|---|---|---|---|---|
| card_ram_strike | Coup de bélier | Commune | Off. | 1E | 4 | Dmg 110, Push 20 | St Vulnerable 1t | Steal 15 | Bounce 40 |
| card_steel_skin | Peau d'acier | Commune | Déf. | Soi | 4 | Shield 25 | St Taunt 1t | St Regen 2t | St Anchor 1t |
| card_deep_anchor | Ancrage profond | Commune | Déf. | 1A | 5 | Shield 15, St Anchor 1t | Purge 1 | St Regen 2t | St Haste 1t |
| card_fresh_blood | Sang neuf | Commune | Sout. | 1A | 5 | Heal 30 | St Regen 2t | Purge 0 | Shield 10 |
| card_bloodletting | Saignée | Rare | Off. | 1E | 5 | Dmg 120, Steal 20 | St Corrosion 2t | Exec 30 | Bounce 50 |
| card_challenge | Défi | Rare | Déf. | Soi | 5 | Shield 20, St Taunt 2t | St Anchor 1t | St Regen 2t | Purge 0 |
| card_shared_pulse | Pouls partagé | Rare | Sout. | ZA | 5 | Heal 18 | Purge 1 | St Regen 2t | St Haste 1t |
| card_living_bastion | Bastion vivant | Épique | Déf. | ZA | 6 | Shield 18 | St Regen 2t | St Anchor 1t | Purge 1 |
| card_telluric_fist | Poing tellurique | Légendaire | Off. | ZE | 7 | Dmg 120, Push 25 | St Vulnerable 2t | Steal 20 | Exec 40 |

## Perception (violet) — 9 nouvelles
| Id | Nom | Rareté | Nature | Cible | Rech. | Base | Niv. 3 | Niv. 6 | Niv. 10 |
|---|---|---|---|---|---|---|---|---|---|
| card_precise_shot | Tir précis | Commune | Off. | 1E | 4 | Dmg 130 | Exec 30 | St Vulnerable 1t | Bounce 40 |
| card_side_step | Pas de côté | Commune | Déf. | Soi | 4 | Shield 15, St Evade 1t | St Haste 1t | Purge 1 | St Regen 2t |
| card_camouflage | Camouflage | Commune | Déf. | 1A | 5 | Shield 18 | St Evade 1t (30 %) | Purge 1 | St Haste 1t |
| card_guiding_echo | Écho guide | Commune | Sout. | 1A | 5 | Adv 40 | St Haste 1t | Shield 10 | Purge 0 |
| card_guard_break | Faille de garde | Rare | Off. | 1E | 5 | Dmg 100, Purge 1, St Vulnerable 2t | St Mark 1t | Exec 40 | Bounce 50 |
| card_premonition | Prémonition | Rare | Déf. | ZA | 5 | Shield 12 | St Evade 1t (30 %) | Purge 1 | St Haste 1t |
| card_shared_watch | Veille partagée | Rare | Sout. | ZA | 5 | Gauge +100 | Purge 1 | St Haste 1t | Shield 8 |
| card_silent_takedown | Exécution silencieuse | Épique | Off. | 1E | 6 | Dmg 150, Exec 50 | St Blind 1t | Steal 15 | Bounce 60 |
| card_time_veil | Voile temporel | Épique | Déf. | ZA | 6 | Shield 15, Adv 20 | Purge 1 | St Evade 1t (30 %) | St Regen 2t |

## Récapitulatif (48 cartes, existantes comprises)
| Famille | Offensive | Défensive | Soutien | C / R / É / L |
|---|---|---|---|---|
| Matière | 4 | 4 | 4 | 5 / 4 / 2 / 1 |
| Élémentaire | 4 | 4 | 4 | 5 / 4 / 2 / 1 |
| Physique / Bio | 4 | 4 | 4 | 5 / 4 / 2 / 1 |
| Perception | 4 | 4 | 4 | 5 / 4 / 2 / 1 |

## Contenu et textes
- `Content/Prototype/PrototypeCollection.cs` : les 36 cartes, après les 12 existantes, par famille dans l'ordre des tableaux.
- `Presentation/Common/LocCollection.cs` : les 36 noms (`skills.<id>`), comme les 12 existants.

## Illustrations
- **Sources** : le dépôt ne contient que 41 images uniques, dont environ 22 déjà prises par les 12 cartes et les 10 objets ; l'utilisateur **ajoute de nouvelles images** dans `03_Art_UI_UX/Illustrations/` (n'importe quel sous-dossier). Les 23 images présentes le 2026-10-27 dans ce dossier sont **réservées** (on ne sait pas lesquelles ont servi aux 12 premières cartes) : elles sont listées dans `Resources/Illustrations/Cards/SOURCES.md`, section « Réservées », et l'outil ne les prend pas. Il faut donc **36 nouvelles images** ; l'utilisateur peut retirer de la liste « Réservées » une image qu'il sait libre.
- Outil d'éditeur `Editor/Assets/CardArtCropper.cs` (menu « Heroes Dawn ▸ Setup ▸ Recadrer les illustrations de cartes », appelable en `-executeMethod N29.HeroesDawn.EditorTools.Assets.CardArtCropper.CropMissingBatch`) : pour chaque carte du catalogue sans image dans `Resources/Illustrations/Cards/`, prend la première source libre (ordre alphabétique du chemin, ni réservée ni déjà attribuée), la recadre au centre en 16:9, la réduit en **640 × 360** (format des 12 premières), la teinte vers la couleur de la famille (`Theme.FamilyColor`, mélange à 30 %) et l'enregistre en `<id>.jpg` (qualité 85). Les images existantes ne sont jamais regénérées.
- **Source nommée d'après une carte** : un fichier dont le nom (sans extension, casse indifférente) est l'identifiant d'une carte (`card_monolith.jpg`) lui est attribué en priorité, quel que soit son sous-dossier ; les autres cartes reçoivent les sources libres restantes par ordre alphabétique. Prompts de génération : `03_Art_UI_UX/Prompts_illustrations_cartes_C2.md`.
- **Une source différente par carte** : chaque attribution est ajoutée à `SOURCES.md`, section « Attribuées » (une ligne par carte : `id → chemin source`). S'il manque des sources, l'outil s'arrête sans rien écrire et indique combien d'images ajouter.
- **Génération partielle** : `CropNamedBatch` (ou le menu « … images nommées seulement ») ne traite que les images nommées d'après une carte et laisse les autres cartes sans image (illustration générique), sans erreur. Utilisé le 2026-10-27 pour 32 cartes sur 36 ; **restent sans illustration : Mue réparatrice, Monolithe, Sang neuf, Saignée**.
- Le rendu se vérifie à l'œil sur la capture du grimoire. Seule cette étape attend les nouvelles images ; le reste du lot avance sans elles (une carte sans image affiche l'illustration générique).

## Tests
- **Contenu** (`CollectionRulesTests`) : 48 cartes ; par famille 12 cartes, 4 par nature, raretés 5/4/2/1 ; nature saisie = nature déduite pour chaque carte ; règles C1 déjà en place (3 paliers 3/6/10, pas d'effet allié sur une carte ennemie, rebond seulement avec dégâts) vérifiées sur les 48 ; identifiants uniques.
- **Catégories** (`DesignToolsDataTests.CodeCatalog_HasTheSpecCategories`) : la table des natures passe à 48 entrées.
- **Textes** : chaque carte a un nom (`skills.<id>` présent).
- **Illustrations** : l'outil choisit des sources libres, jamais réservées ni en double, et s'arrête s'il en manque (tests sur un dossier temporaire) ; après génération, chaque carte du catalogue a son image (`AgentArt.Card` trouve `Illustrations/Cards/<id>`) et `SOURCES.md` n'attribue jamais deux fois la même source.
- **Combat** : un combat complet en mode Auto avec des agents équipés de nouvelles cartes de chaque famille se termine et se rejoue à l'identique.
- **Capture** : grimoire avec le filtre d'une famille, pour vérifier les nouvelles tuiles (illustrations, pastilles).

## Documentation
- `01_Game_Design/06_Kits_Competences.md` : 48 cartes, répartition, renvoi à cette spec.
- `05_Production/Decisions.md` : une ligne (catalogue 48 cartes, répartition des raretés, illustrations par recadrage).

## Hors périmètre
Équilibrage fin en jeu (après playtest), illustrations définitives, nouveaux boosters thématiques.
