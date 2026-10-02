# Plateformes et services

> Architecture logicielle complète : `00_Vue_Ensemble.md` et suivants. Ce fichier couvre les cibles, les services UGS et les appareils.

## Cible
| | |
|---|---|
| Moteur | Unity 6.6 |
| Plateformes | Android, iOS, PC (Steam) |
| Minimum proposé | Android 9 (API 28) / iOS 15, 3 Go RAM ; PC : Windows 10, 4 Go RAM |
| Connexion | **Requise** (économie serveur, pass, objectifs en temps réel) |
| Langues | FR, EN, ES, PT-BR, DE (Unity Localization, textes externalisés dès le début) |

## Backend : Unity Gaming Services
| Service UGS | Usage |
|---|---|
| Authentication | Invité, Google Play Games, Apple, Steam ; liaison de comptes (cross-progression mobile ↔ PC) |
| Cloud Save | Progression, équipe, état des zones (sauvegarde à chaque case) |
| Economy | Crédits, PR, Gemmes, inventaire, achats |
| Cloud Code | Logique **côté serveur** : récompenses, recrutement, rotation de pool, pass, horloge des objectifs secondaires |
| Remote Config | Paramètres d'équilibrage (issus de `05_Equilibrage.xlsx`), événements, A/B tests |
| Analytics | KPIs, entonnoirs (voir Live-ops) |
| IAP | Unity IAP (Google Play, App Store) ; Steam : microtransactions Steamworks |

**Règle** : toute récompense et tout coût sont validés par Cloud Code. Le client ne fait qu'afficher.

## Architecture client (résumé — détail dans 02 à 07)
- **Data-driven** : ScriptableObjects pour membres, compétences (gabarits par famille), ennemis, cases, pools, événements, plateaux.
- **Plateau** : graphe (nœuds/arêtes) édité via un outil éditeur Unity ; pools de contenu par case ; brouillard calculé côté client, résultats tirés côté serveur.
- **Combat** : simulation ATB déterministe séparée du rendu (même principe MVC que Polo Rivals), permettant le mode auto et des tests automatisés.
- **Addressables** pour le contenu (villes livrées en live-ops sans mise à jour de l'app quand possible).
- **Input System** : tactile + souris/clavier.

## Outils à produire
- Éditeur de plateau (placement de cases, chemins, pools).
- Import des paramètres du tableur vers Remote Config.
- Simulateur de combat en lot (équilibrage des combos et des rangs).

## Tests
- Tests unitaires sur la simulation de combat et l'économie (Cloud Code).
- Matrice d'appareils : 3 Android bas/milieu/haut de gamme, 2 iPhone, 1 PC minimum.
