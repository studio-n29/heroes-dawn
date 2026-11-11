# Économie & monétisation

> Résumé dans le GDD : `00_GDD/GDD_Heroes_Dawn.md` §8 · Statut : v2 (1.0.0, bloc E) — aligné sur le code. Spec : `Code/docs/specs/2026-11-10-monetisation-design.md`

## Objectif du système
Modèle **F2P + pass saisonnier**. Principe directeur : **l'argent réel fait uniquement gagner du temps** — tout contenu acheté est aussi obtenable en jouant. Aucune recrue exclusive au premium.

## Monnaies
| Monnaie | Sources | Usages (puits) |
|---|---|---|
| **Crédits** (soft) | Campagne, coffres, expéditions, pass | Amélioration du QG, validation des paliers de compétences, consommables de mission, objets du salon, renouvellements de pool |
| **PR** — Points de Recherche | Campagne, expéditions, pass, boutique | Recrutement |
| **Gemmes** (premium) | Achat en argent réel, petite dose dans la piste premium du pass, échanges | Accélération des expéditions, repos complet, renouvellements au-delà de l'échelle en Crédits, emplacements d'expédition, cosmétiques, échanges contre Crédits / PR. **Jamais** de Composants, de catalyseurs, de recettes, de duels ni de points de tournoi |

## Recrutement
- **Pool quotidienne** de recrues, propre à la ville.
- **Choix direct** : chaque recrue a un **coût fixe en PR**. Pas de gacha.
- Renouvellement de la pool (aussi la boutique du jour et les offres de Composants) : **1 gratuit**, puis 100 / 200 / 400 / 800 Crédits, puis **10 Gemmes** chacun.
- **Aucun doublon** : une recrue possédée ne réapparaît jamais.
- Les agents d arc, d événement et de prouesse ne passent pas par la boutique : ils se gagnent en jouant.
- **Rang AMIF** (1 à 5) : remplace la rareté d agent ; coût en PR croissant. Les rangs 4 et 5 se gagnent par arc ou par prouesse (pas en boutique). Les cartes du grimoire ont 1 à 5 **étoiles** (boosters ordinaires : 50 / 28 / 15 / 6 / 1 %).

## Pass saisonnier
- Saison de **42 jours** (calendrier fixe depuis le 05/01/2026), **50 paliers de 1 300 XP**, **piste gratuite + piste premium**. Chaque nouvelle saison remet XP, paliers réclamés et premium à zéro.
- **Pass premium : 9,99 € en argent réel** (produit `pass_premium`, consommable valable pour la saison en cours) ; il ne s'achète plus en Gemmes. Un second achat dans la même saison est refusé avant tout paiement ; la saison suivante, il se rachète.
- XP de pass : missions quotidiennes (500 au total), objectifs secondaires, quêtes, combats gagnés, zones validées, expéditions, et depuis la 1.0.0 : **boss de repaire 150**, **duel de tournoi 30 gagné / 10 perdu**, **fabrication à l'Atelier 20** ⚑. Les valeurs d'XP des sources existantes ont été multipliées par 10 pour l'échelle de 1 300 XP par palier (victoire de combat 30, première zone 300, rejouée 100).
- Récompenses (`Resources/Content/pass_rewards.json`, une ligne par palier) : Crédits, PR, Gemmes, Composants, catalyseurs, accélérateurs, cosmétiques. Gratuit : ~580 PR, Composants Commun et Raffiné, 3 catalyseurs, 2 dés / pions. Premium : 3 tenues, 1 thème de QG, 300 Gemmes, 900 PR, 600 minutes d'accélérateurs, 30 Composants Raffiné, 10 catalyseurs. Aucune recrue ni recette exclusive, aucun Composant Exceptionnel.

## Cosmétiques
Purement visuels : **aucun effet en jeu**.

| Type | Contenu | Sources |
|---|---|---|
| Tenues de recrues | Portrait + sprite de combat alternatifs | Pass (premium surtout), boutique, succès, événements |
| Décor du QG | Thèmes de pièces, mobilier, posters (visibles sur le hub) | Pass, boutique, succès |
| Pion & dé | Skins de dé, cadres de pion, effets de déplacement | Pass (gratuit et premium), boutique, événements |

### Sources
- **Pass saisonnier** : l'essentiel des cosmétiques de la saison, surtout dans la piste premium.
- **Boutique (gemmes)** : achat direct, **rotation hebdomadaire** de 6 articles de source Boutique (semaine ISO, tirage déterministe, sans les cosmétiques déjà possédés). Cosmétiques de pass, de tournoi, de succès et d'événement : jamais en vente. Catalogue : `Resources/Content/cosmetics.json` ; liste unique de cosmétiques possédés dans le profil.
- **Succès en jeu** : fin d'acte, mode difficile, collection complète d'une ville (exclusifs, non achetables).
- **Événements** : récompenses exclusives de l'événement de la saison.

### Volume proposé (équipe de 1-2)
| Moment | Tenues | Thèmes QG | Dés / pions |
|---|---|---|---|
| Lancement | 4 (1 par Légende) | 2 | 3 |
| Chaque saison | 3 (pass) + 1 (événement) | 1 | 2 |

### Prix (boutique)
| Article | Gemmes |
|---|---|
| Tenue Agent / Élite / Légende | 300 / 600 / 1 000 |
| Thème de pièce du QG | 400 |
| Skin de dé ou de pion | 150 |

Soit 3 € / 6 € / 10 € pour une tenue ; aucun cosmétique au-delà de 10 €.

## Données / paramètres (valeurs de départ du tableur)
### Revenus d'un joueur gratuit actif (1 zone terminée par jour)
| Source | Crédits / jour | PR / jour | XP de pass / jour |
|---|---|---|---|
| Campagne | 4 500 | 75 | 450 |
| Labo de recherche (supprimé en 1.0.0) | — | — | — |
| Expéditions (2 emplacements, 4 h, utilisées à 50 %) | 4 800 | 90 | — |
| Objectifs secondaires (60 % réalisés) | 3 456 | 43 | 864 |
| Missions quotidiennes | — | — | 500 |
| **Total** | **~12 750** | **~256** | **~1 814** |

### Recrutement
| Élément | Valeur |
|---|---|
| Coût par rang AMIF (1 à 5) | 150 / 220 / 450 / 800 / 1 200 PR ⚑ |
| Pool quotidienne | 3 agents à 9 h locale, tirage pondéré par rang 45 / 25 / 20 / 8 / 2 ⚑ |
| Renouvellements | 1er gratuit, puis 100 Crédits, jusqu à 800 |
| Les 14 agents de la boutique (10 Agents + 4 Élites) | 3 300 PR |

### Premium
| Élément | Valeur |
|---|---|
| Gemmes par euro | 100 |
| PR par gemme | 1 (une Légende ≈ 12 €) |
| Accélération | 20 gemmes par heure |
| Pass premium | 9,99 € |

### Packs de gemmes (identifiants de produit = identifiants des stores)
| Pack | Produit | Prix | Gemmes | Bonus |
|---|---|---|---|---|
| Poignée | `gems_100` | 0,99 € | 100 | — |
| Sac | `gems_520` | 4,99 € | 520 | +4 % |
| Caisse | `gems_1080` | 9,99 € | 1 080 | +8 % |
| Coffre | `gems_2250` | 19,99 € | 2 250 | +12 % |
| Chambre forte | `gems_6000` | 49,99 € | 6 000 | +20 % |
| Offre de bienvenue (1 fois par profil) | `welcome` | 2,99 € | 500 + 1 accélérateur 8 h | — |

Les anciens identifiants `gems_550` et `gems_1200` ont disparu ; leurs compteurs d'achats dans une vieille sauvegarde sont ignorés. Tous les produits sont des **consommables** (pas de restauration d'achats). Le prix affiché vient du store (taxes par pays) ; reçu contrôlé côté client pour la 1.0.0 (validation serveur : plan LiveOps).

### Pass saisonnier
| Élément | Valeur |
|---|---|
| Durée | 42 jours |
| Paliers | 50 × 1 300 XP |
| Temps pour le finir (joueur actif) | ~36 jours (6 jours de marge) |
| Piste gratuite (50 paliers) | Crédits, PR (total ~600), objets de salon, 2 dés/pions |
| Piste premium (50 paliers) | + 3 tenues, 1 thème de QG, 300 gemmes, ~900 PR, accélérateurs |

### Missions quotidiennes (500 XP de pass au total)
| Mission | XP pass |
|---|---|
| Lancer le dé 5 fois | 100 |
| Gagner 2 combats | 100 |
| Collecter le labo | 100 |
| Lancer une expédition | 100 |
| Utiliser un combo | 100 |

### Consommables de mission (Crédits)
| Objet | Effet | Prix |
|---|---|---|
| Trousse de soin | −25 % de fatigue à un membre | 800 |
| Dé chargé | Le prochain lancer fait au moins 4 | 1 200 |
| Stimulant | +20 % ATB au début du prochain combat | 600 |
| Fusée éclairante | Révèle 2 liens de plus pendant un déplacement | 900 |

### Objets du salon (Crédits)
| Objet | Bonus XP | Prix |
|---|---|---|
| Sac de frappe | +10 % | 2 000 |
| Simulateur de combat | +20 % | 6 000 |
| Salle de réalité augmentée | +35 % | 15 000 |

## Boutique de confort (Gemmes)
- **Emplacements d'expédition** : 3 de base, **4e à 300 Gemmes, 5e à 500 Gemmes**, achat unique et permanent ; les expéditions en cours ne changent pas de place.
- **Renouvellements** : 1 gratuit par jour, puis 100 / 200 / 400 / 800 Crédits, puis **10 Gemmes** (`ShopRerollGemCost`).
- **Accélérations** : **20 Gemmes par heure**, minutes arrondies au-dessus (1 Gemme par 3 minutes entamées, au moins 1), pour les expéditions. Un **accélérateur** (minutes offertes : 480 pour l'offre de bienvenue, minutes du pass) est dépensé avant les Gemmes. Le repos complet (10 Gemmes) reste. L'Atelier est instantané et un duel de tournoi ne s'accélère pas.
- Échanges conservés : Gemmes → Crédits (20 → 1 000) et → PR (30 → 60).

## Garde anti pay-to-win
Tout ce que l'argent réel ou les Gemmes achètent est du temps, du confort ou du cosmétique. Interdits (vérifiés par `NoPayToWinTests`) : Composants, catalyseurs ou recettes contre des Gemmes ou de l'argent ; duel ou point de tournoi ; recrue exclusive au premium ; loot box ou gacha ; cosmétique qui modifie une statistique. Seul le pass donne des Composants (Commun / Raffiné) et des catalyseurs, et il se mérite en jouant (la piste gratuite en donne aussi).

## Atelier de fabrication (Composants, catalyseurs, étoiles)

L'Atelier fabrique des objets avec des **Composants** et des **recettes** obtenues en récompense (aventure, repaires, tournois). Le laboratoire de recherche, ses recherches et la compétence de création sont supprimés (bloc B de la 1.0.0). Spec : `Code/docs/specs/2026-11-10-atelier-fabrication-design.md`. Valeurs ⚑ à régler au playtest.

- **Composants** : 4 types (arme, armure, outil, implant) × 3 qualités (Commun, Raffiné, Exceptionnel). Une fabrication n'utilise qu'une seule qualité. Après un combat gagné, un type tiré au hasard : Facile 1-2 Commun ; Normal 2-3 Commun ; Élite 3-4 Commun + 1-2 Raffiné ; Boss 3-4 Raffiné + 1 Exceptionnel une fois sur deux.
- **Catalyseurs** (un par fabrication, optionnel) : Métal → DEF, Énergie → ATQ, Biologique → PV, Cristal → VIT. Drop : Élite 10 %, Boss 30 %. Avec un catalyseur, la statistique orientée est ajoutée (jusqu'à 3 statistiques) ; si la recette la contient déjà, elle est tirée dans la moitié haute de sa plage.
- **Recettes** : 12 (3 par type). Les 4 Communes sont de départ ; 4 viennent de l aventure, 3 du Repaire de l AMIF (Manteau blindé, Grappin, Plaque Égide : récompense de première victoire d un boss de repaire, jamais en butin ordinaire) et 1 du tournoi (Bobine synaptique, lot du top 3 : récompense de classement, jamais en butin). Drop : Boss 60 %, Élite 25 % (recette inconnue tirée au hasard ; si toutes sont connues : 5 Composants Commun de plus).
- **Repaires** : chaque boss de repaire verse des Composants Raffinés (et un Exceptionnel pour le Titan), un catalyseur (15 à 35 %) et sa recette ; récompense complète une fois par jour et par boss, réduite de moitié ensuite (sans recette). Spec : `Code/docs/specs/2026-11-10-repaires-boss-ia-design.md`.
- **Coût** : Commune 6 Composants (Commun minimum) ; Rare 8 (Raffiné minimum) ; Épique 10 (Raffiné minimum).
- **Étoiles (1 à 6)** : étoile de départ Commun 2, Raffiné 4, Exceptionnel 5, puis tirage −1 (25 %), 0 (50 %), +1 (25 %), bornée de 1 à 6 ; le 6 n'apparaît qu'avec des Composants Exceptionnels. Multiplicateur des statistiques : ★1 80 % · ★2 100 % · ★3 115 % · ★4 130 % · ★5 150 % · ★6 175 %.
- **Statistiques** : tirées dans la plage de la recette (Commune 8-12 %, Rare 8-14 %, Épique 10-16 % + effet spécial), multipliées par l'étoile, arrondies au plus proche, au moins 1. Fabrication instantanée ; l'objet est un exemplaire unique.
- **Boutique de Composants** : six lots par jour, renouvelés avec les héros (4 lots de 6 Commun à 120 Crédits, 2 lots de 3 Raffiné à 240 Crédits ⚑).
- **Migration 0.15.0** : scraps → Composants Commun du même type ; Noyaux → catalyseurs en rotation ; compétences de création supprimées sans remboursement ; recherches du labo supprimées et Crédits dépensés remboursés (500 × (n+1) par niveau n atteint) ; anciens objets fabriqués : ancienne qualité + 1 étoile, statistiques inchangées.

Tous ces paramètres sont dans `05_Equilibrage.xlsx` ou à y ajouter lors de l'intégration Remote Config.

## Tournois JcJ (arène)

Les tournois remplacent le tournoi simulé de la 0.15.0 (bloc D de la 1.0.0). Spec : `Code/docs/specs/2026-11-10-tournois-jcj-design.md`. Valeurs ⚑ à régler au playtest. Un compte connecté est nécessaire.

- **Calendrier** (`Resources/Content/tournaments.json`, un seul tournoi actif à la fois) : Nouvel An 29/12 → 10/01 · Saint-Valentin 07/02 → 16/02 · Printemps 20/03 → 03/04 · Été 14/06 → 28/06 · Vacances 14/07 → 31/07 · Rentrée 01/09 → 14/09 · Halloween 20/10 → 03/11 · Noël 01/12 → 26/12. Entre deux tournois la page affiche le suivant.
- **Duel** : l équipe du joueur est capturée (agents, niveaux, cartes, objets, liens) et envoyée au serveur à chaque connexion et changement d équipe. *Hors ligne* : l adversaire est la capture d un joueur de score proche (jamais soi-même), menée par l IA selon la position de ses agents ; sans capture, une équipe de repli est générée. *En ligne* : file d attente Realtime de 15 s, puis repli hors ligne ; la même simulation tourne sur les deux appareils (l hôte, identifiant le plus petit, en camp Ally), seules les commandes sont échangées, une empreinte d état est comparée tous les 20 ticks (écart : duel annulé sans points). Passer son tour après 30 s ; adversaire muet 45 s : il est déclaré parti.
- **Points** : victoire hors ligne +10, en ligne +20, défaite 0 ; quitter un duel en ligne −10 (jamais sous 0) ; quitter hors ligne ou duel annulé : sans effet et non compté. Plafond de **10 duels par jour et par tournoi** (le serveur borne aussi les points quotidiens).
- **Classement** : un par tournoi, départage par la date d atteinte du score ; top 100 et rang du joueur.
- **Récompenses** (réclamables jusqu à 7 jours après la fin, `Resources/Content/arena_rewards.json`) : paliers à 50 / 150 / 300 / 500 / 800 points (Crédits, Composants, cosmétique de palier) ; top 10 % (lot intermédiaire) ; top 3 (Composants Exceptionnels, catalyseurs, recette Bobine synaptique, cadre, titre et cosmétique exclusifs). Les cosmétiques sont rangés comme identifiants `cosmetic:<tournoi>_<id>` ; le catalogue et l affichage sont au bloc E.
- **Serveur** : script `Assets/_Project/Backend/Supabase/002_hd_arena.sql` (tables `hd_arena_*`, fonctions `hd_submit_duel`, `hd_claim_tier`…), **non appliqué** sans accord explicite. Quotas Realtime du palier gratuit à vérifier avant le lancement ; pas de vérification serveur des combats avant l arène classée (ligues M+6, arène par équipes M+18 : plan LiveOps).
