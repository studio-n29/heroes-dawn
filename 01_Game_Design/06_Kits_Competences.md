# Kits de compétences (réalignés sur la version 0.10.1, 2026-10-23)

> Chaque agent a **2 compétences de base** propres à lui : **A1** légère et gratuite, **A2** spéciale qui coûte de l'énergie. La **3e place** du combat est une **carte du grimoire** équipée ; une 2e carte prend la 4e place. Les agents n'ont plus de passive ni de A3.
> Dégâts exprimés en % de l'Attaque. Le coût est une dépense d'**énergie** (voir `02_Combat.md`) ; il n'y a plus de recharge par tour.
> Valeurs de départ, à équilibrer avec le simulateur d'équilibrage (menu *Heroes Dawn ▸ Équilibrage ▸ Simulateur*).
> Source des données : `Code/Heroes Dawn Unity/Assets/_Project/Content/Prototype/PrototypeAgents.cs` (gabarits), `Resources/Content/skills.json` (valeurs éditables dans l'outil Compétences), `Presentation/Common/LocDawnAgents.cs` (noms).

## Gabarits par rôle
Chaque compétence est construite sur le **rôle** de l'agent, plus un **statut propre** à l'agent (voir le tableau plus bas). Seuls les noms et les statuts distinguent deux agents du même rôle : des effets entièrement sur mesure restent à écrire. ⚑

| Rôle | A1 (gratuite) | A2 (coût 30, 40 pour un contrôle) |
|---|---|---|
| Tank | 110 % à 1 ennemi, recul de 10 %, + statut propre (30 %) | Sur lui : Provocation (2 tours) + Bouclier (25 % de ses PV), + statut propre |
| Dégâts | 100 % à 1 ennemi, + statut propre (40 %) | 90 % à tous les ennemis, + statut propre (30 %) |
| Contrôle | 90 % à 1 ennemi, + statut propre (30 %) | Entrave (2 tours) + recul de 25 % sur 1 ennemi, + statut propre |
| Soutien | 80 % à 1 ennemi, + statut propre (25 %) | Soin de 35 % + élan de 20 % sur 1 allié, + statut propre |
| Précision | 100 % à 1 ennemi + Marque, + statut propre (30 %) | Vulnérabilité (2 tours) + 10 % de la frise retirés à tous les ennemis, + statut propre (30 %) |

## Cartes du grimoire (3e et 4e places)
Une carte de la **même famille** que l'agent s'équipe en 3e place (la 2e carte en 4e). Une carte d'une autre famille est refusée. Le coût d une carte est dérivé de sa recharge d origine (40 ou 50). Le catalogue des cartes est dans `Resources` (collection) et se complète avec le grimoire.

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
Rareté : Agent ×1, Élite ×1,2, Légende ×1,45 sur les statistiques. Entre parenthèses : le statut propre de la compétence. Fiches de lore : `02_Lore_Narration/04_Agents_The_Dawn.md`.

| Agent | Famille | Rôle | Rareté | A1 (gratuite) | A2 (coût 30 ou 40) |
|---|---|---|---|---|---|
| Agent D | Matière | Contrôle | Légende | Barrière de craie (Entrave) | Créature dessinée (Vulnérabilité) |
| Agent M | Perception | Précision | Légende | Vision du présent (Choc) | Futur fragmenté (Vulnérabilité) |
| Darius Brooks | Matière | Tank | Agent | Poutre propulsée (Vulnérabilité) | Rempart de ferraille (Ancrage) |
| Mei-Lin Chen | Élémentaire | Dégâts | Agent | Jet de flammes (Brûlure) | Brasier (Brûlure) |
| Freya Lindqvist | Perception | Soutien | Agent | Murmure mental (Marque) | Esprit partagé (Hâte) |
| Kwame Abara | Physique/Bio | Dégâts | Agent | Griffes du fauve (Entrave) | Meute déchaînée (Vulnérabilité) |
| Giulia Rossi | Matière | Dégâts | Agent | Projection (Choc) | Tempête d'objets (Vulnérabilité) |
| Lucas Mendes | Physique/Bio | Tank | Agent | Frappe fantôme (Marque) | Corps de brume (Esquive) |
| Rin Takeda | Élémentaire | Contrôle | Agent | Décharge radio (Choc) | Brouillage (Choc) |
| Amara Hughes | Perception | Soutien | Agent | Note apaisante (Entrave) | Chœur de guérison (Régénération) |
| Ilona Varga | Perception | Contrôle | Agent | Ombre de la peur (Choc) | Cauchemar (Marque) |
| Fatou Diallo | Physique/Bio | Tank | Agent | Foulée explosive (Choc) | Bouclier cinétique (Hâte) |
| Clément Lefèvre | Perception | Précision | Élite | Mirage (Choc) | Grande illusion (Entrave) |
| Youssef Haddad | Physique/Bio | Précision | Élite | Écoute absolue (Vulnérabilité) | Perception totale (Marque) |
| Séverine Morel | Physique/Bio | Soutien | Agent | Drain vital (Corrosion) | Transfusion (Régénération) |
| Sam Okafor | Physique/Bio | Tank | Agent | Encaisser (Choc) | Onde de choc (Ancrage) |
| Astrid Hallström | Élémentaire | Soutien | Élite | Vague (Gel) | Marée vive (Régénération) |
| Céleste Moreau | Élémentaire | Contrôle | Agent | Brume toxique (Corrosion) | Nuage soporifique (Corrosion) |
| Haruto Nakamura | Perception | Contrôle | Agent | Âme errante (Entrave) | Souffle astral (Vulnérabilité) |
| Yelena Ivanova | Élémentaire | Contrôle | Élite | Givre mordant (Gel) | Glace éternelle (Gel) |
| Noah Kaplan | Matière | Contrôle | Agent | Permutation (Choc) | Échange forcé (Vulnérabilité) |
| Tunde Oyelaran | Matière | Soutien | Agent | Poche d'espace (Entrave) | Refuge plié (Bouclier) |
| Victor Sorel | Perception | Contrôle | Légende | Emprise (Marque) | Volonté dominée (Vulnérabilité) |
| Odile Ferrand | Perception | Soutien | Légende | Seconde suspendue (Gel) | Temps accéléré (Hâte) |
| Louis Dubois | Matière | Contrôle | Élite | Aimant (Choc) | Champ magnétique (Choc) |
| Marco Bellini | Matière | Dégâts | Élite | Transmutation (Corrosion) | Or alchimique (Corrosion) |
| Zofia Kowalczyk | Élémentaire | Dégâts | Élite | Archet vibrant (Choc) | Crescendo (Choc) |
| Mateo Vasquez | Physique/Bio | Soutien | Élite | Pression sanguine (Corrosion) | Coagulation (Régénération) |
| Ada Okonkwo | Physique/Bio | Soutien | Légende | Mutation (Corrosion) | Régénération cellulaire (Régénération) |
| Eero Nieminen | Matière | Dégâts | Légende | Poids mort (Entrave) | Gravité écrasante (Vulnérabilité) |
| Ji-Hoon Park | Perception | Précision | Agent | Coup invisible (Choc) | Disparition (Vulnérabilité) |
| Anna Petrov | Perception | Précision | Agent | Saut instantané (Choc) | Pirouette téléportée (Entrave) |

## Ennemis
Les compétences des ennemis (Voyou, Gros bras, Pyro de rue, Brick, Crocodile…) sont dans `Content/Prototype/PrototypeContent.cs` et `PrototypeZones.cs` ; elles suivent les mêmes règles d énergie.
