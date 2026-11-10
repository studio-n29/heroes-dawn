# Bloc A — Rangs AMIF, positions, types et étoiles des cartes

> Spec du 2026-11-10. Source : point de développement « finaliser la 1.0.0 » (tournois, repaires, IA, équilibrage, rareté des agents, atelier). Bloc A sur 5 : A rangs/positions/cartes → B atelier → C repaires/boss/IA → D tournois → E monétisation. Les blocs B, C et D s'appuient sur les étoiles et les positions définies ici.

## Décisions validées

- **Position** (5) remplace le **rôle** : Première ligne, Attaquant, Contrôle, Soutien, Précision à distance.
- **Rang AMIF** (1 à 5) remplace la **rareté d'agent** (Agent, Élite, Légende).
- **Famille** (inchangée, 4) ne limite plus l'équipement des cartes : elle donne un bonus secondaire.
- **Type de compétence** : la nature des cartes passe à 4 valeurs — Offensif, Défensif, Contrôle, Soutien.
- **Cartes** : 5 étoiles (remplacent les 4 raretés). Un agent a **2 compétences fixes + 3 cartes équipables** (5 places en combat).
- Équipement libre : n'importe quelle carte sur n'importe quel agent, jamais deux fois la même carte sur un agent.

## 1. Agents

### Rangs AMIF

| Rang | Libellé du lore | Multiplicateur de stats |
|---|---|---|
| 5 | Capacités extraordinaires pouvant représenter une menace planétaire | ×1,65 |
| 4 | Capacités majeures pouvant représenter une menace à grande échelle | ×1,45 |
| 3 | Capacités significatives pouvant représenter une menace sérieuse | ×1,30 |
| 2 | Capacité notable pouvant représenter une menace ponctuelle | ×1,15 |
| 1 | Capacités secondaires ne représentant pas une menace directe | ×1 |

Les rangs 4 et 5 se gagnent par arc ou par prouesse (jamais en boutique), comme les Légendes actuelles. Eda et Brie Rayner sont rang 5 mais **non recrutables** (PNJ, allié d'arc ou boss).

### Positions

| Ancien rôle | Position | Compétence signature (A2) | Types boostés |
|---|---|---|---|
| Tank | Première ligne | provocation + bouclier | Défensif |
| Dégâts | Attaquant | zone | Offensif |
| Contrôle | Contrôle | entrave + recul | Contrôle |
| Soutien | Soutien | soin + élan | Soutien |
| Précision | Précision à distance | vulnérabilité + jauge ennemie | Offensif, Contrôle |

Les kits de base (A1 gratuite, A2 payante) restent ceux de l'ancien rôle correspondant : la migration est une simple renommée.

### Table des rangs (⚑ à valider)

**Les 2 personnages joueurs** : Agent D 5, Agent M 5.

**Les 30 agents numérotés**

| Rang | Agents |
|---|---|
| 5 (1) | Victor Sorel |
| 4 (5) | Odile Ferrand, Ada Okonkwo, Eero Nieminen, Astrid Hallström, Louis Dubois |
| 3 (6) | Yelena Ivanova, Marco Bellini, Zofia Kowalczyk, Mateo Vasquez, Clément Lefèvre, Youssef Haddad |
| 2 (8) | Ilona Varga, Haruto Nakamura, Noah Kaplan, Céleste Moreau, Rin Takeda, Giulia Rossi, Kwame Abara, Mei-Lin Chen |
| 1 (10) | Darius Brooks, Freya Lindqvist, Lucas Mendes, Amara Hughes, Fatou Diallo, Séverine Morel, Sam Okafor, Tunde Oyelaran, Ji-Hoon Park, Anna Petrov |

**Les 10 cryptides de Salem** (pas encore dans le jeu, acte IV ; rangs provisoires d'après leurs pouvoirs, à revoir quand ils seront écrits) : S5 esprit de la faim 4 ; S1 céphalopode, S3 homme-bouc, S6 primate géant, S9 créature de pierre 3 ; S2 insecte-humanoïde, S4 homme-phalène, S7 homme-chien, S8 créature sonique, S10 prédateur 2.

Répartition des 42 agents : rang 5 : 3, rang 4 : 6, rang 3 : 10, rang 2 : 13, rang 1 : 10.

## 2. Cartes

### Types

`CardNature { Offensive, Defensive, Control, Support }`. La déduction automatique passe à :

| Effet de base | Cible | Type |
|---|---|---|
| `Damage` sans statut de contrôle | ennemi(s) | Offensif |
| Statut de contrôle (Entrave, Choc, Gel, Marque, Vulnérabilité, Aveuglement), `PushBack`, `GaugeDelta` | ennemi(s) | Contrôle |
| `Shield` | tout | Défensif |
| `Heal` | tout | Soutien |
| `ApplyStatus`, `Advance`, `GaugeDelta` | allié(s) ou soi | Soutien |

La catégorie fine (`SkillCategory`) reste réservée aux animations. Cartes déjà saisies « Offensive » dont l'effet principal est un contrôle (par exemple card_prism_burst, entrave) : à passer en Contrôle explicitement à l'implémentation.

### Étoiles et redistribution des 48 cartes

Répartition visée : 12 ★1, 12 ★2, 12 ★3, 8 ★4, 4 ★5.

- **★5 (4)** : Monolithe, Foudre en chaîne, Poing tellurique, card_time_rift (les 4 Légendaires actuelles).
- **★4 (8)** : les 8 Épiques actuelles (Forge vivante, card_prism_burst, Carapace de glace, card_inferno, Bastion vivant, card_regen_field, Exécution silencieuse, Voile temporel).
- **★3 (12)** : 12 des 16 Rares actuelles.
- **★2 (12)** : 4 Rares plus faibles (Masque d'emprunt, Halo de néon, Pouls partagé, Prémonition ; les cartes dont l’identifiant est donné n’ont pas de nom français vérifié) et 8 Communes plus fortes (Éclats de verre, card_ember, Coup de bélier, card_second_wind, Courant tonifiant, Tir précis, Écho guide, card_mark).
- **★1 (12)** : les 12 autres Communes (Rempart de béton, Armure de rivets, Mue réparatrice, card_glass_wall, Mur de givre, Bouclier de vapeur, Brume apaisante, Peau d'acier, Ancrage profond, Sang neuf, Pas de côté, Camouflage).

Les effets de base ne changent pas dans cette spec : un passage d'une Commune en ★2 est ajusté par le tableur au playtest (+10 % sur l'effet principal en première approche). Gabarit cible par étoile (base au niveau 1) : ★1 dégâts 110–120 · ★2 120–135 · ★3 100–120 + effet fort ou zone 70 · ★4 150 / zone 110 · ★5 160 / zone 120.

Taux des boosters : 50 / 28 / 15 / 6 / 1 % (★1 à ★5).

### Emplacements et bonus

- **Emplacements** : 3 cartes équipables par agent. Les 2 compétences de base sont les « 2 cartes fixes ».
- **Bonus de position** : +15 % d'efficacité aux cartes dont le type est boosté par la position de l'agent.
- **Bonus de famille** : +5 % aux cartes de la famille de l'agent. Les deux se cumulent (jusqu'à +20 %).
- Le bonus s'applique à la valeur numérique principale de l'effet (dégâts, soin, bouclier, durée ou pourcentage de statut), après les paliers de la carte.

## 3. Domaine et données

- `Role` devient `Position` (5 valeurs renommées) ; `Rarity` (Agent, Elite, Legend) devient `AgentRank` (1 à 5) ; `MemberDef` porte `Position` et `Rank`.
- `ItemRarity` reste utilisé par les **objets** jusqu'au bloc B (qualité 1 à 6 étoiles). Les cartes passent à un `CardStars` (1 à 5) distinct, pour ne pas lier les deux blocs.
- `CardNature` gagne `Control`. `CollectionRules.CanEquip(SkillCardDef, MemberDef)` ne teste plus la famille : elle interdit seulement un doublon de carte. La limite d'emplacements de cartes passe de 2 à 3.
- `UnitFactory.SkillsWithCards` : trois cartes après les deux compétences ; l'éventail de combat passe à 5 places (côté présentation, voir plus bas).
- Contenu : `agents.json` (champs `position`, `rank`), `cards.json` (champs `stars`, `nature` étendu), `ShopWeights` (taux à 5 étoiles).
- Équilibrage : tableur (onglets Stats et Progression) — multiplicateurs de rang à la place de ceux de rareté, bonus de position et de famille, gabarits par étoile.

## 4. Migration des sauvegardes

- Rôle → position (renommée), à l'identique.
- Rareté d'agent → rang via la table ci-dessus (par identifiant d'agent).
- Cartes équipées : conservées ; le 3e emplacement est vide. Une carte équipée sur un agent d'une autre famille n'existait pas, donc aucun cas à traiter.
- Cartes possédées : l'étoile est recalculée par la table (par identifiant). Idempotent.

## 5. Interface — hors périmètre

La session « UI mise à jour Aube tactique » refait les écrans : cette spec ne touche à aucun écran ni prefab. Un lot UI ultérieur ajoutera, après sa fusion : la 5e place de l'éventail, les badges de rang (étoiles AMIF), de position et d'étoiles de carte, le 3e emplacement du grimoire, le filtre par type à 4 valeurs et l'indicateur de bonus (position/famille). Textes à prévoir : `rank.1` à `rank.5` (libellés ci-dessus), `position.*`, `card.nature.control`.

## 6. Documentation

- `01_Game_Design/02_Combat.md` (rôles → positions), `03_Metahumains.md` (rangs, positions, famille en bonus), `04_Economie.md` (raretés → rangs/étoiles, taux), `06_Kits_Competences.md` (types, étoiles, table des 48 cartes, kits par position).
- `02_Lore_Narration/04_Agents_The_Dawn.md` : rang AMIF de chaque fiche.
- `05_Production/Decisions.md` : une ligne par décision ci-dessus.

## 7. Tests

- Domaine : mapping rôle → position, rang → multiplicateur, déduction du type à 4 valeurs, équipement libre et refus du doublon, bonus de position et de famille (cumul), 3 cartes max.
- Contenu : 42 agents avec un rang, répartition 3 / 6 / 10 / 13 / 10, 48 cartes avec une étoile, répartition 12 / 12 / 12 / 8 / 4.
- Migration : sauvegarde 0.15.0 chargée, positions et rangs corrects, cartes conservées, idempotence.

## Points ouverts ⚑

- Rangs des agents et des cryptides à valider.
- Valeurs +15 % / +5 % et multiplicateurs de rang à régler au playtest.
- Les 4 Rares rétrogradées en ★2 sont choisies sur leur effet seul ; à confirmer dans le tableur.
