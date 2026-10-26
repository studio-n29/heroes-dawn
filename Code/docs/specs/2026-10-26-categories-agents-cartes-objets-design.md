# Catégories des agents, des cartes et des objets (lot B de la 0.14.0)

> Spec du 2026-10-26. Source : document N29 HQ « Playtest 0.13.0 — Préparation de la version 0.14.0 », point « Les catégories et spécifications des familles d'agents, de cartes et d'objets ». Prépare les lots C (cartes et paliers), D (liens) et E (labo de recherche).

## Décisions
- **Agents** : leur catégorie est leur **famille** (`Family` : Matière, Élémentaire, Physique/Bio, Perception), déjà fondée sur la nature du pouvoir. Aucun changement de données.
- **Cartes** : deux axes, **famille** (le « type de pouvoir », déjà présent) × **nature** (Offensive, Défensive, Soutien). La nature est un **champ saisi par carte**, pré-rempli par une déduction depuis l'effet de base.
- **Objets** : un **type** (Arme, Armure, Outil, Implant). Un agent garde 2 emplacements libres mais **ne peut pas porter deux objets du même type**.
- Représentation : **enums typés dans les définitions** (comme `Family`, `Rarity`, `ItemPerk`), pas de catégories en données ni d'étiquettes libres.

## Domaine (`com.n29.heroesdawn.domain`)

### Nature des cartes
- `Definitions/CollectionDefs.cs` (à côté de `ItemRarity` et `ItemPerk`) : `public enum CardNature { Offensive, Defensive, Support }` ; `ItemType` (ci-dessous) au même endroit.
- `SkillCardDef` : propriété `CardNature Nature`. Le constructeur prend `CardNature? nature = null` ; nul → `CardNatures.Of(skill)`.
- `CardNatures.Of(SkillDef skill)` (nouvelle classe statique, à côté de `SkillCategories`) — d'après l'effet de base et la cible :

| Effet de base | Cible | Nature |
|---|---|---|
| `Damage` | ennemi(s) | Offensive |
| `ApplyStatus`, `PushBack`, `GaugeDelta` | ennemi(s) | Offensive |
| `Shield` | tout | Défensive |
| `Heal` | tout | Soutien |
| `ApplyStatus`, `Advance`, `GaugeDelta` | allié(s) ou soi | Soutien |
| aucun effet | — | Offensive |

- `SkillDef.Category` (Dégâts, Soins, Protection, Boost, Gestion du temps) **reste** : étiquette fine utilisée par les animations ; la nature ne la remplace pas.

### Type des objets
- `public enum ItemType { Weapon, Armor, Tool, Implant }`.
- `ItemDef` : propriété `ItemType Type`. Le constructeur prend `ItemType? type = null` ; nul → `ItemTypes.Of(item)`.
- `ItemTypes.Of` : d'abord l'effet spécial, sinon la stat la plus forte (égalité : ATQ, puis PV/DEF, puis VIT) :

| Effet spécial | Stat dominante (sans effet) | Type |
|---|---|---|
| `SkillPower` | — | Implant |
| `CritChance` | ATQ | Arme |
| `OpeningShield` | PV ou DEF | Armure |
| `StartGauge` | VIT | Outil |

### Règle d'équipement
- `CollectionRules.CanEquip(ItemDef item, IEnumerable<ItemDef> equipped)` : faux si un objet équipé a le même `Type`.
- La règle des cartes ne change pas (`CanEquip(SkillCardDef, MemberDef)` : même famille).

## Application
- `CollectionErrors.SameItemType` (`"collection.same_type"`, texte `errors.collection_same_type` : « Cet agent porte déjà un objet de ce type. »).
- `CollectionBook.EquipItem` : refuse avec `SameItemType` avant tout autre effet. Pas de remplacement automatique : le joueur retire d'abord l'objet du même type.
- **Migration des sauvegardes** : au chargement de la session, pour chaque agent portant deux objets du même type, le second (ordre de la liste) est retiré de `Items` ; il redevient disponible (stock = possédés − équipés). Une ligne `Warn` dans le log par objet retiré. Idempotente.

## Contenu
- `PrototypeCollection` : nature explicite sur les 12 cartes, type explicite sur les 12 objets.
- `Resources/Content/items.json` : champ `type` (entier de l'enum, comme `rarity` et `perk`) sur les 10 objets ; `ContentDataConverter` le lit (absent → déduction).
- Valeurs de départ (égales à la déduction, saisies pour être relues) :

| Cartes | Nature |
|---|---|
| card_prism_burst, card_shackle, card_ember, card_storm, card_inferno, card_bulwark_charge, card_mark | Offensive |
| card_glass_wall | Défensive |
| card_second_wind, card_regen_field, card_precog, card_time_rift | Soutien |

| Objets | Type |
|---|---|
| steel_knuckles, titan_gauntlets, apex_module | Arme |
| kevlar_vest, riot_shield, reactive_plating, phoenix_core | Armure |
| running_shoes, adrenaline_patch, time_shard | Outil |
| power_gauntlet, arcane_focus | Implant |

## Interface
- **Grimoire** : badge de nature (icône + couleur) sur chaque carte et dans la fiche, à côté de la pastille de famille. Rangée de filtres combinables : *Toutes / Matière / Élémentaire / Physique-Bio / Perception* et *Toutes / Offensive / Défensive / Soutien*. Tri inchangé. Ouvert depuis la fiche d'un agent : filtre de famille pré-réglé sur la sienne.
- **Équipement** : icône de type sur chaque objet et dans la fiche ; filtres par type. Sur la fiche agent, chaque emplacement montre le type porté ; les objets d'un type déjà porté sont grisés avec « Type déjà porté » ; une tentative affiche l'erreur en toast.
- **Textes** : `card.nature.offensive|defensive|support`, `item.type.weapon|armor|tool|implant`, `filters.all`, `errors.collection_same_type`, `equip.type_taken`.

## Outils de design
- Fenêtre Objets : le champ `type` apparaît en menu déroulant (enums déjà dessinés par `RecordDrawer`) ; il suffit de l'ajouter à l'enregistrement et au convertisseur.

## Documentation
- `01_Game_Design/06_Kits_Competences.md` : nature des cartes et table de déduction.
- `01_Game_Design/03_Metahumains.md` : la famille est la catégorie de l'agent.
- GDD : types d'objets et règle d'équipement.
- `05_Production/Decisions.md` : trois lignes (type de pouvoir = famille ; nature saisie et pré-remplie ; pas deux objets du même type).

## Hors périmètre
Nouvelles cartes et effets par palier (lot C), liens entre agents (lot D), recettes et objets fabriqués (lot E), refonte de la boutique (lot F).

## Tests
- Domaine : `CardNatures.Of` pour chaque ligne de la table (dont frise sur ennemi et sur allié) ; `ItemTypes.Of` pour chaque effet spécial et chaque stat dominante, égalités comprises ; `CanEquip` refuse un doublon de type et accepte deux types différents.
- Application : `EquipItem` renvoie `SameItemType` ; la migration d'une sauvegarde à deux armes rend la seconde disponible et ne touche pas une sauvegarde valide.
- Contenu : chaque carte et chaque objet (code et `items.json`) a une nature ou un type saisi égal à sa déduction, sauf exception documentée.
- Présentation : filtres du grimoire (famille × nature) et de l'équipement ; objet grisé quand son type est déjà porté.
