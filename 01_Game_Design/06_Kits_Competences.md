# Kits de compétences (réalignés sur la version 0.10.1, 2026-10-23)

> Chaque agent a **2 compétences de base** propres à lui : **A1** légère et gratuite, **A2** spéciale qui coûte de l'énergie. La **3e place** du combat est une **carte du grimoire** équipée ; une 2e carte prend la 4e place. Les agents n'ont plus de passive ni de A3.
> Dégâts exprimés en % de l'Attaque. Le coût est une dépense d'**énergie** (voir `02_Combat.md`) ; il n'y a plus de recharge par tour.
> Valeurs de départ, à équilibrer avec le simulateur d'équilibrage (menu *Heroes Dawn ▸ Équilibrage ▸ Simulateur*).
> Source des données : `Code/Heroes Dawn Unity/Assets/_Project/Content/Prototype/PrototypeAgents.cs` (gabarits), `Resources/Content/skills.json` (valeurs éditables dans l'outil Compétences), `Presentation/Common/LocDawnAgents.cs` (noms).

## Gabarits par position
Chaque compétence est construite sur la **position** de l agent (ancien rôle), plus un **statut propre** à l'agent (voir le tableau plus bas). Seuls les noms et les statuts distinguent deux agents de la même position : des effets entièrement sur mesure restent à écrire. ⚑

| Position | A1 (gratuite) | A2 (coût 30, 40 pour un contrôle) |
|---|---|---|
| Première ligne | 110 % à 1 ennemi, recul de 10 %, + statut propre (30 %) | Sur lui : Provocation (2 tours) + Bouclier (25 % de ses PV), + statut propre |
| Attaquant | 100 % à 1 ennemi, + statut propre (40 %) | 90 % à tous les ennemis, + statut propre (30 %) |
| Contrôle | 90 % à 1 ennemi, + statut propre (30 %) | Entrave (2 tours) + recul de 25 % sur 1 ennemi, + statut propre |
| Soutien | 80 % à 1 ennemi, + statut propre (25 %) | Soin de 35 % + élan de 20 % sur 1 allié, + statut propre |
| Précision à distance | 100 % à 1 ennemi + Marque, + statut propre (30 %) | Vulnérabilité (2 tours) + 10 % de la frise retirés à tous les ennemis, + statut propre (30 %) |

## Cartes du grimoire (3 places équipables)
Équipement **libre** : n importe quelle carte sur n importe quel agent, jamais deux fois la même carte sur un agent (3 cartes + 2 compétences de base = 5 places en combat). La famille donne +5 % aux cartes de l agent, et sa position +15 % aux cartes dont le type est boosté (Défensif, Offensif, Contrôle, Soutien) ; les ennemis n en reçoivent pas. Le coût d une carte est dérivé de sa recharge d origine (40 ou 50). Le catalogue des cartes est dans `Resources` (collection) et se complète avec le grimoire.

### Catalogue
**48 cartes**, 4 types (Offensive, Défensive, Soutien, Contrôle) et **5 étoiles** : 12 ★1, 12 ★2, 12 ★3, 8 ★4, 4 ★5 (taux du booster ordinaire 50 / 28 / 15 / 6 / 1 %). Types Contrôle : card_shackle, card_prism_burst, card_mark, card_guard_break, card_ram_strike. Étoiles : ★5 card_monolith, card_chain_lightning, card_telluric_fist, card_time_rift ; ★4 card_living_forge, card_prism_burst, card_ice_carapace, card_inferno, card_living_bastion, card_regen_field, card_silent_takedown, card_time_veil ; ★3 card_reflecting_prism, card_support_cables, card_shackle, card_storm, card_spark_rush, card_power_flux, card_bloodletting, card_challenge, card_bulwark_charge, card_guard_break, card_shared_watch, card_precog ; ★2 card_borrowed_mask, card_neon_halo, card_shared_pulse, card_premonition, card_glass_shards, card_ember, card_ram_strike, card_second_wind, card_tonic_current, card_precise_shot, card_guiding_echo, card_mark ; ★1 les 12 autres. Chaque carte a 3 paliers (niveaux 3, 6, 10). Table complète des 36 cartes ajoutées, gabarits de puissance par rareté et règles de contenu : `Code/docs/specs/2026-10-27-catalogue-36-cartes-design.md`. Illustrations temporaires : recadrages teintés de l'art du Drive, une source par carte (`Resources/Illustrations/Cards/SOURCES.md`).

### Catégories des cartes
Une carte se classe sur deux axes : sa **famille** (le type de pouvoir, celle des agents qui peuvent l'équiper) × sa **nature** : **Offensive**, **Défensive** ou **Soutien**. La nature est **saisie par carte** ; à défaut, elle est déduite de l'effet de base et de la cible :

| Effet de base | Cible | Nature |
|---|---|---|
| Dégâts | ennemi(s) | Offensive |
| Statut, recul, jauge | ennemi(s) | Offensive |
| Bouclier | tout | Défensive |
| Soin | tout | Soutien |
| Statut, élan, jauge | allié(s) ou soi | Soutien |
| aucun effet | — | Offensive |

### Niveaux et paliers des cartes
Une carte monte du **niveau 1 au niveau 10** avec des doublons et des Crédits : passer du niveau N au suivant coûte **(N + 1) / 2 doublons** (1, 1, 2, 2, 3, 3, 4, 4, 5 : 25 au total) et **200 × N Crédits** (9 000 au total). Chaque niveau ajoute **+10 %** aux dégâts, soins et boucliers de base (140 % au niveau 5, 190 % au niveau 10).

Aux niveaux **3, 6 et 10**, un **palier** ajoute des effets secondaires à la carte, qui s'appliquent aux mêmes cibles (non augmentés par le niveau). Une carte qui vise un ennemi n'a pas d'effet allié dans ses paliers. Le grimoire montre les paliers atteints (●) et à venir, et le bouton Améliorer annonce le palier qu'il débloque.

| Mécanique | Effet |
|---|---|
| Vol de vie X % | Le lanceur récupère X % des dégâts infligés à la cible (après bouclier), sans dépasser ses PV max |
| Purge (N) / Purge totale | Retire N statuts (ou tous), les plus récents d'abord : altérations sur un allié, bonus (Bouclier, Régénération, Hâte, Esquive, Ancrage, Provocation) sur un ennemi |
| Rebond X % | Un autre ennemi vivant, tiré au sort, subit le premier effet de dégâts de la carte à X % |
| Exécution +X % | Dégâts +X % sur une cible sous 30 % de ses PV max |

Spec : `Code/docs/specs/2026-10-26-cartes-paliers-mecaniques-design.md` (table des paliers des 12 cartes).

La catégorie fine (Dégâts, Soins, Protection, Boost, Gestion du temps) reste l'étiquette des animations. Le grimoire affiche la nature de chaque carte et se filtre par famille × nature. Spec : `Code/docs/specs/2026-10-26-categories-agents-cartes-objets-design.md`.

## Paliers de compétences (niv. 10 / 20 / 30 / 40 / 50) ⚑ — pas encore implémentés
| Palier | Effet proposé |
|---|---|
| 10 | A1 : +15 % de dégâts ou d'effet |
| 20 | A2 : coût −5 |
| 30 | A2 : +20 % de dégâts ou d'effet |
| 40 | Le statut propre de l'agent dure 1 tour de plus |
| 50 | A2 : effet additionnel (propre à chaque agent) |

## Altérations (glossaire, valeurs du jeu)
| Altération | Effet | Durée type |
|---|---|---|
| Brûlure | 8 % des PV max par frise complète, versés point par point | 2 tours |
| Gel | Avance un pas sur deux ; recul de 20 % sur la frise | 2 tours |
| Choc | Saute son prochain tour ; recul de 15 % | 1 tour |
| Corrosion | Défense −30 % | 2 tours |
| Aveuglement | 50 % de chances de rater (prévu, non utilisé par les agents) | 2 tours |
| Vulnérabilité | Dégâts subis +25 % | 2 tours |
| Entrave | Seule A1 est utilisable ; recul de 25 % | 2 tours |
| Marque | La prochaine attaque reçue est un coup critique | Jusqu'au coup |
| Provocation | Doit attaquer le lanceur | 1 à 2 tours |
| Bouclier | Absorbe des dégâts | 2 tours |
| Régénération | Soigne 8 % des PV max à chaque tour | 2 tours |
| Hâte | Avance de 2 points un pas sur trois | 2 tours |
| Esquive | Évite la prochaine attaque | Jusqu'à l'attaque |
| Ancrage | Insensible aux reculs sur la frise | 2 tours |

Certaines compétences déplacent directement les unités sur la frise : **recul** (« −25 % de la frise »), **élan** (« +20 %, à 100 % l'agent agit aussitôt ») et **retrait ou ajout de frise** (« −10 % »).

## Les 32 kits
Rang AMIF : ×1 / ×1,15 / ×1,30 / ×1,45 / ×1,65 sur les statistiques (rangs 1 à 5). Entre parenthèses : le statut propre de la compétence. Fiches de lore : `02_Lore_Narration/04_Agents_The_Dawn.md`.

| Agent | Famille | Position | Rang | A1 (gratuite) | A2 (coût 30 ou 40) |
|---|---|---|---|---|---|
| Agent D | Matière | Contrôle | 5 | Barrière de craie (Entrave) | Créature dessinée (Vulnérabilité) |
| Agent M | Perception | Précision à distance | 5 | Vision du présent (Choc) | Futur fragmenté (Vulnérabilité) |
| Darius Brooks | Matière | Première ligne | 1 | Poutre propulsée (Vulnérabilité) | Rempart de ferraille (Ancrage) |
| Mei-Lin Chen | Élémentaire | Attaquant | 2 | Jet de flammes (Brûlure) | Brasier (Brûlure) |
| Freya Lindqvist | Perception | Soutien | 1 | Murmure mental (Marque) | Esprit partagé (Hâte) |
| Kwame Abara | Physique/Bio | Attaquant | 2 | Griffes du fauve (Entrave) | Meute déchaînée (Vulnérabilité) |
| Giulia Rossi | Matière | Attaquant | 2 | Projection (Choc) | Tempête d'objets (Vulnérabilité) |
| Lucas Mendes | Physique/Bio | Première ligne | 1 | Frappe fantôme (Marque) | Corps de brume (Esquive) |
| Rin Takeda | Élémentaire | Contrôle | 2 | Décharge radio (Choc) | Brouillage (Choc) |
| Amara Hughes | Perception | Soutien | 1 | Note apaisante (Entrave) | Chœur de guérison (Régénération) |
| Ilona Varga | Perception | Contrôle | 2 | Ombre de la peur (Choc) | Cauchemar (Marque) |
| Fatou Diallo | Physique/Bio | Première ligne | 1 | Foulée explosive (Choc) | Bouclier cinétique (Hâte) |
| Clément Lefèvre | Perception | Précision à distance | 3 | Mirage (Choc) | Grande illusion (Entrave) |
| Youssef Haddad | Physique/Bio | Précision à distance | 3 | Écoute absolue (Vulnérabilité) | Perception totale (Marque) |
| Séverine Morel | Physique/Bio | Soutien | 1 | Drain vital (Corrosion) | Transfusion (Régénération) |
| Sam Okafor | Physique/Bio | Première ligne | 1 | Encaisser (Choc) | Onde de choc (Ancrage) |
| Astrid Hallström | Élémentaire | Soutien | 4 | Vague (Gel) | Marée vive (Régénération) |
| Céleste Moreau | Élémentaire | Contrôle | 2 | Brume toxique (Corrosion) | Nuage soporifique (Corrosion) |
| Haruto Nakamura | Perception | Contrôle | 2 | Âme errante (Entrave) | Souffle astral (Vulnérabilité) |
| Yelena Ivanova | Élémentaire | Contrôle | 3 | Givre mordant (Gel) | Glace éternelle (Gel) |
| Noah Kaplan | Matière | Contrôle | 2 | Permutation (Choc) | Échange forcé (Vulnérabilité) |
| Tunde Oyelaran | Matière | Soutien | 1 | Poche d'espace (Entrave) | Refuge plié (Bouclier) |
| Victor Sorel | Perception | Contrôle | 5 | Emprise (Marque) | Volonté dominée (Vulnérabilité) |
| Odile Ferrand | Perception | Soutien | 4 | Seconde suspendue (Gel) | Temps accéléré (Hâte) |
| Louis Dubois | Matière | Contrôle | 4 | Aimant (Choc) | Champ magnétique (Choc) |
| Marco Bellini | Matière | Attaquant | 3 | Transmutation (Corrosion) | Or alchimique (Corrosion) |
| Zofia Kowalczyk | Élémentaire | Attaquant | 3 | Archet vibrant (Choc) | Crescendo (Choc) |
| Mateo Vasquez | Physique/Bio | Soutien | 3 | Pression sanguine (Corrosion) | Coagulation (Régénération) |
| Ada Okonkwo | Physique/Bio | Soutien | 4 | Mutation (Corrosion) | Régénération cellulaire (Régénération) |
| Eero Nieminen | Matière | Attaquant | 4 | Poids mort (Entrave) | Gravité écrasante (Vulnérabilité) |
| Ji-Hoon Park | Perception | Précision à distance | 1 | Coup invisible (Choc) | Disparition (Vulnérabilité) |
| Anna Petrov | Perception | Précision à distance | 1 | Saut instantané (Choc) | Pirouette téléportée (Entrave) |

## Ennemis
Les compétences des ennemis (Voyou, Gros bras, Pyro de rue, Brick, Crocodile…) sont dans `Content/Prototype/PrototypeContent.cs` et `PrototypeZones.cs` ; elles suivent les mêmes règles d énergie.

Chaque ennemi a un **style d IA** (Agressif, Défensif, Contrôle, Soutien, Équilibré), par défaut selon sa position, et un boss peut changer de jeu de compétences et de style à chaque phase : voir `02_Combat.md`, sections *Styles d IA* et *Boss à phases*. Un boss peut avoir plus de trois compétences (le Crocodile en a quatre en phase 1).
