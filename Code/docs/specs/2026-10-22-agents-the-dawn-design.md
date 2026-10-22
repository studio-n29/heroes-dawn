# Les 30 agents de The Dawn dans le jeu (morceau 2)

> Spec du 2026-10-22. Source : `02_Lore_Narration/04_Agents_The_Dawn.md`. Décision de périmètre : remplacer les 30 agents provisoires par les 30 agents numérotés. Les personnages joueurs (Agent D, Agent M) et les cryptides de Salem sont reportés au morceau 3 (choix D/M et actes).

## 1. Objectif et critères de succès
- Le jeu n'expose que les 30 agents numérotés de The Dawn : aucun agent provisoire (Bastion, Ancre, Phoenix…) ne reste dans le catalogue, les textes ou les tests.
- Mêmes règles qu'aujourd'hui : 30 agents, 3 compétences chacun, quatre voies d'obtention (départ, boutique, arc, événement de week-end, prouesse).
- Les outils de design (fenêtre Agents) ouvrent les 30 nouveaux agents sans erreur de validation.
- Tests Edit Mode et Play Mode verts.

## 2. Ce qui change dans le code
| Zone | Fichier | Changement |
|---|---|---|
| Agents de base (8) et agents ajoutés (22) | `Content/Prototype/PrototypeContent.cs`, `PrototypeAgents.cs` | `PrototypeAgents.Roster` porte les 30 agents ; l équipe de départ est portée par `ContentCatalog.StarterTeam`. Les kits restent générés par rôle |
| Données éditables | `Resources/Content/agents.json`, `skills.json` | Régénérés depuis le code (menu *Réinitialiser les données depuis le code*) |
| Textes | `Presentation/Common/LocAgents.cs` | Noms affichés (« Ilona Varga ») et descriptions de pouvoir ; noms de compétence génériques par rôle conservés |
| Tests | `Tests/Domain/AgentRosterTests.cs`, `Tests/Presentation/AgentsPagePresenterTests.cs`, tests utilisant `BuildCore()`, campagne et tutoriel | Anciens ids remplacés ; le plan d'implémentation liste chaque référence trouvée |

Identifiants : nom civil en minuscules sans accent, séparé par `_` (`ilona_varga`). Nom affiché : « Prénom Nom ». Le pouvoir est affiché comme sous-titre.

## 3. Tableau des 30 agents (à valider)
Les stats viennent du barème actuel par rôle et rareté. Les familles et les rôles sont des propositions d'après le pouvoir ; ajustables ensuite dans la fenêtre Agents.

| # | Agent | Pouvoir | Zone | Famille | Rôle | Rareté | Obtention |
|---|---|---|---|---|---|---|---|
| 3 | Darius Brooks | Animation de matière inerte | New York | Matière | Tank | Agent | Départ |
| 4 | Mei-Lin Chen | Feu | New York | Élémentaire | Dégâts | Agent | Départ |
| 8 | Kwame Abara | Métamorphose animale | New York | Physique/Bio | Dégâts | Agent | Départ |
| 12 | Freya Lindqvist | Télépathie | New York | Perception | Soutien | Agent | Départ |
| 15 | Giulia Rossi | Télékinésie | New York | Matière | Dégâts | Agent | Boutique |
| 24 | Lucas Mendes | Intangibilité | New York | Physique/Bio | Tank | Agent | Boutique |
| 27 | Rin Takeda | Ondes électromagnétiques | New York | Élémentaire | Contrôle | Agent | Boutique |
| 30 | Amara Hughes | Résonance émotionnelle | New York | Perception | Soutien | Agent | Boutique, après la zone 1.1 |
| 2 | Ilona Varga | Matérialisation des peurs | Paris | Perception | Contrôle | Agent | Boutique |
| 17 | Fatou Diallo | Énergie cinétique | Paris | Physique/Bio | Tank | Agent | Boutique |
| 18 | Clément Lefèvre | Illusions | Paris | Perception | Précision | Élite | Boutique |
| 21 | Youssef Haddad | Sens décuplés | Paris | Physique/Bio | Précision | Élite | Boutique |
| 28 | Séverine Morel | Vampirisme d'énergie vitale | Paris | Physique/Bio | Soutien | Agent | Boutique |
| 1 | Sam Okafor | Absorption cinétique | Moscou | Physique/Bio | Tank | Agent | Boutique |
| 5 | Astrid Hallström | Eau | Moscou | Élémentaire | Soutien | Élite | Boutique |
| 7 | Céleste Moreau | Production de gaz | Moscou | Élémentaire | Contrôle | Agent | Boutique |
| 14 | Haruto Nakamura | Projection astrale | Moscou | Perception | Contrôle | Agent | Boutique |
| 25 | Yelena Ivanova | Cryokinésie | Moscou | Élémentaire | Contrôle | Élite | Boutique |
| 10 | Noah Kaplan | Transposition | New York | Matière | Contrôle | Agent | Arc, zone 1.1 |
| 23 | Tunde Oyelaran | Poche dimensionnelle | New York | Matière | Soutien | Agent | Arc, zone 1.2 |
| 13 | Victor Sorel | Contrôle mental | Paris | Perception | Contrôle | Légende | Arc, zone 1.2 |
| 22 | Odile Ferrand | Dilatation temporelle | Moscou | Perception | Soutien | Légende | Arc, zone 1.3 |
| 6 | Louis Dubois | Champs magnétiques | Paris | Matière | Contrôle | Élite | Week-end, zone 1.1 |
| 26 | Marco Bellini | Alchimie de contact | Paris | Matière | Dégâts | Élite | Week-end, zone 1.2 |
| 20 | Zofia Kowalczyk | Son | Moscou | Élémentaire | Dégâts | Élite | Week-end, zone 1.2 |
| 19 | Mateo Vasquez | Sang | Moscou | Physique/Bio | Soutien | Élite | Week-end, zone 1.3 |
| 11 | Ada Okonkwo | Biokinésie | Paris | Physique/Bio | Soutien | Légende | Prouesse : zone 1.1 sans équipier |
| 16 | Eero Nieminen | Gravité | Moscou | Matière | Dégâts | Légende | Prouesse : zone 1.2 sans défaite |
| 29 | Ji-Hoon Park | Invisibilité | Paris | Perception | Précision | Agent | Prouesse : zone 1.3 sans équipier |
| 9 | Anna Petrov | Téléportation | Moscou | Perception | Précision | Agent | Prouesse : zone 1.3 sans défaite |

Totaux : 18 Agents, 8 Élites, 4 Légendes. Familles : Matière 7, Élémentaire 6, Physique/Bio 8, Perception 9. Rôles : Dégâts 6, Contrôle 8, Soutien 8, Précision 4, Tank 4.
Les 4 agents de départ (Brooks, Chen, Lindqvist, Abara) couvrent les 4 familles ; 14 agents entrent dans la boutique (dont Amara Hughes après la zone 1.1), 4 par arc, 4 par événement de week-end, 4 par prouesse.

**Provisoire** : l'obtention suit la structure actuelle (zones 1.1 à 1.3 de Brooklyn) car seules ces zones existent. Elle sera refaite au morceau 3 quand les zones de Paris, Moscou et Salem seront définies.

## 4. Hors périmètre
- Agent D, Agent M, cryptides de Salem, portraits, compétences propres à chaque pouvoir, passage à 2 compétences de base (refonte du combat).
- Campagne de Brooklyn : l'ordre des combats, les ennemis et les dialogues ne changent pas ; seuls les noms d'agents cités dans les textes sont vérifiés.

## 5. Risques
- Les 8 agents de départ actuels sont cités par les tests de la boutique (`BuildCore`), le tutoriel et la sauvegarde de départ ; le plan d'implémentation recense chaque référence avant modification.
- Les sauvegardes existantes ne retrouvent pas les anciens agents (ils sont ignorés au chargement, comme l'indique le guide des outils) ; acceptable avant l'Early Access.
- L'équilibrage des familles est déplacé (Perception 9, Élémentaire 6) : les synergies par paires de familles sont à revoir avec le game design.

## 6. Réalisation (2026-10-22)
- Les anciens agents (Ancre, Givre… et les 22 ajoutés) sont conservés comme jeu de données de test dans `Assets/Tests/Domain/Fixtures/LegacyRoster.cs` (`TestContent.Build()` / `BuildCore()`) ; le jeu livré n'expose que les 30 agents de The Dawn.
- `ContentCatalog.StarterTeam` remplace `MetaRules.StarterTeam` ; les répliques de la campagne de Brooklyn dites par « Ancre » le sont par `darius_brooks`.
- Les textes des anciens agents restent dans `Loc` (inertes, utiles aux tests) ; les 30 nouveaux sont dans `LocDawnAgents.cs` (nom, pouvoir, fiche du registre).
- Tests : Edit Mode 674 réussis, 1 ignoré ; Play Mode 53 réussis, 14 ignorés, 0 échec.
