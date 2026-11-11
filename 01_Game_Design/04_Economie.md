# Économie & monétisation

> Résumé dans le GDD : `00_GDD/GDD_Heroes_Dawn.md` §8 · Statut : v1 validée

## Objectif du système
Modèle **F2P + pass saisonnier**. Principe directeur : **l'argent réel fait uniquement gagner du temps** — tout contenu acheté est aussi obtenable en jouant. Aucune recrue exclusive au premium.

## Monnaies
| Monnaie | Sources | Usages (puits) |
|---|---|---|
| **Crédits** (soft) | Campagne, coffres, expéditions, pass | Amélioration du QG, validation des paliers de compétences, consommables de mission, objets du salon, renouvellements de pool |
| **PR** — Points de Recherche | Campagne, expéditions, pass, boutique | Recrutement |
| **Premium** ($) | Achat, petite dose dans le pass | PR, accélération entraînement / fatigue, renouvellements de pool, piste premium du pass |

## Recrutement
- **Pool quotidienne** de recrues, propre à la ville.
- **Choix direct** : chaque recrue a un **coût fixe en PR**. Pas de gacha.
- Renouvellement de la pool : **1 gratuit**, puis payant en Crédits (100 → 800), puis en premium.
- **Aucun doublon** : une recrue possédée ne réapparaît jamais.
- Les agents d arc, d événement et de prouesse ne passent pas par la boutique : ils se gagnent en jouant.
- **Rang AMIF** (1 à 5) : remplace la rareté d agent ; coût en PR croissant. Les rangs 4 et 5 se gagnent par arc ou par prouesse (pas en boutique). Les cartes du grimoire ont 1 à 5 **étoiles** (boosters ordinaires : 50 / 28 / 15 / 6 / 1 %).

## Pass saisonnier
- Saison de **6 semaines**, **piste gratuite + piste premium**.
- XP de pass : objectifs secondaires, missions.
- Récompenses : PR, Crédits, accélérateurs, cosmétiques. Pas de recrue exclusive.

## Cosmétiques
Purement visuels : **aucun effet en jeu**.

| Type | Contenu | Sources |
|---|---|---|
| Tenues de recrues | Portrait + sprite de combat alternatifs | Pass (premium surtout), boutique, succès, événements |
| Décor du QG | Thèmes de pièces, mobilier, posters (visibles sur le hub) | Pass, boutique, succès |
| Pion & dé | Skins de dé, cadres de pion, effets de déplacement | Pass (gratuit et premium), boutique, événements |

### Sources
- **Pass saisonnier** : l'essentiel des cosmétiques de la saison, surtout dans la piste premium.
- **Boutique (gemmes)** : achat direct, rotation hebdomadaire.
- **Succès en jeu** : fin d'acte, mode difficile, collection complète d'une ville (exclusifs, non achetables).
- **Événements** : récompenses exclusives de l'événement de la saison.

### Volume proposé (équipe de 1-2)
| Moment | Tenues | Thèmes QG | Dés / pions |
|---|---|---|---|
| Lancement | 4 (1 par Légende) | 2 | 3 |
| Chaque saison | 3 (pass) + 1 (événement) | 1 | 2 |

### Prix indicatifs (boutique)
| Article | Gemmes |
|---|---|
| Tenue Agent / Élite / Légende | 300 / 600 / 1 000 |
| Thème de pièce du QG | 400 |
| Skin de dé | 150 |

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

### Packs de gemmes proposés
| Pack | Prix | Gemmes | Bonus |
|---|---|---|---|
| Poignée | 0,99 € | 100 | — |
| Sac | 4,99 € | 520 | +4 % |
| Caisse | 9,99 € | 1 080 | +8 % |
| Coffre | 19,99 € | 2 250 | +12 % |
| Chambre forte | 49,99 € | 6 000 | +20 % |
| Offre de bienvenue (1 fois) | 2,99 € | 500 + 1 accélérateur 8 h | — |

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

## Atelier de fabrication (Composants, catalyseurs, étoiles)

L'Atelier fabrique des objets avec des **Composants** et des **recettes** obtenues en récompense (aventure, repaires, tournois). Le laboratoire de recherche, ses recherches et la compétence de création sont supprimés (bloc B de la 1.0.0). Spec : `Code/docs/specs/2026-11-10-atelier-fabrication-design.md`. Valeurs ⚑ à régler au playtest.

- **Composants** : 4 types (arme, armure, outil, implant) × 3 qualités (Commun, Raffiné, Exceptionnel). Une fabrication n'utilise qu'une seule qualité. Après un combat gagné, un type tiré au hasard : Facile 1-2 Commun ; Normal 2-3 Commun ; Élite 3-4 Commun + 1-2 Raffiné ; Boss 3-4 Raffiné + 1 Exceptionnel une fois sur deux.
- **Catalyseurs** (un par fabrication, optionnel) : Métal → DEF, Énergie → ATQ, Biologique → PV, Cristal → VIT. Drop : Élite 10 %, Boss 30 %. Avec un catalyseur, la statistique orientée est ajoutée (jusqu'à 3 statistiques) ; si la recette la contient déjà, elle est tirée dans la moitié haute de sa plage.
- **Recettes** : 12 (3 par type). Les 4 Communes sont de départ ; 5 viennent de l aventure, 3 du Repaire de l AMIF (Manteau blindé, Grappin, Plaque Égide : récompense de première victoire d un boss de repaire, jamais en butin ordinaire) et les tournois (bloc D) en donneront d autres. Drop : Boss 60 %, Élite 25 % (recette inconnue tirée au hasard ; si toutes sont connues : 5 Composants Commun de plus).
- **Repaires** : chaque boss de repaire verse des Composants Raffinés (et un Exceptionnel pour le Titan), un catalyseur (15 à 35 %) et sa recette ; récompense complète une fois par jour et par boss, réduite de moitié ensuite (sans recette). Spec : `Code/docs/specs/2026-11-10-repaires-boss-ia-design.md`.
- **Coût** : Commune 6 Composants (Commun minimum) ; Rare 8 (Raffiné minimum) ; Épique 10 (Raffiné minimum).
- **Étoiles (1 à 6)** : étoile de départ Commun 2, Raffiné 4, Exceptionnel 5, puis tirage −1 (25 %), 0 (50 %), +1 (25 %), bornée de 1 à 6 ; le 6 n'apparaît qu'avec des Composants Exceptionnels. Multiplicateur des statistiques : ★1 80 % · ★2 100 % · ★3 115 % · ★4 130 % · ★5 150 % · ★6 175 %.
- **Statistiques** : tirées dans la plage de la recette (Commune 8-12 %, Rare 8-14 %, Épique 10-16 % + effet spécial), multipliées par l'étoile, arrondies au plus proche, au moins 1. Fabrication instantanée ; l'objet est un exemplaire unique.
- **Boutique de Composants** : six lots par jour, renouvelés avec les héros (4 lots de 6 Commun à 120 Crédits, 2 lots de 3 Raffiné à 240 Crédits ⚑).
- **Migration 0.15.0** : scraps → Composants Commun du même type ; Noyaux → catalyseurs en rotation ; compétences de création supprimées sans remboursement ; recherches du labo supprimées et Crédits dépensés remboursés (500 × (n+1) par niveau n atteint) ; anciens objets fabriqués : ancienne qualité + 1 étoile, statistiques inchangées.

Tous ces paramètres sont dans `05_Equilibrage.xlsx` ou à y ajouter lors de l'intégration Remote Config.
