# Plateau (graphe, dé, brouillard de guerre)

> Résumé dans le GDD : `00_GDD/GDD_Heroes_Dawn.md` §5 · Statut : **v1 complète**
> ⚑ = recommandation qui touche le feeling, à confirmer en prototype.

## Objectif du système
Porter le pilier **« Chaque lancer compte »** : à chaque lancer, le joueur a un vrai choix (quel chemin, quelle case, utiliser ou garder le dé bonus). Le plateau rythme les sessions courtes : 2 à 4 lancers et un combat tiennent en 5-10 minutes.

## Structure
- Une ville = **5 plateaux** (un par acte / arrondissement). Un plateau = **3 zones**. Une zone = un **graphe d'environ 25 cases**.
- Chaque zone a une **case d'entrée** (retour au QG) et **3 objectifs**.
- Les chemins se séparent et se rejoignent (au moins 3 embranchements par zone), sans impasse.

## Déplacement
- **Dé** : 1d6 → autant de Points de Mouvement (PM). ⚑
- Après le lancer, le jeu surligne **toutes les cases atteignables en 1 à N PM**. Le joueur choisit librement sa destination.
- **PM non utilisés** → jauge du dé bonus.
- **Arrêts forcés** : le déplacement s'arrête sur une case **Combat** ou sur une case qui **ouvre du brouillard**. Les PM restants vont dans la jauge.
- On ne peut pas revenir sur la case que l'on vient de quitter pendant le même déplacement (pas d'aller-retour pour « farmer »).

### Dé bonus ⚑
- **6 PM stockés = 1 dé bonus**. Réserve maximale : **2 dés bonus** (12 PM). Le surplus est perdu.
- Utilisable à tout moment hors combat via un bouton : le joueur lance 1d6, ajoute les PM à son déplacement en cours (ou en démarre un) et choisit sa destination.
- Relancer peut **révéler de nouveaux chemins** (décision GDD).

## Brouillard de guerre
- Visibilité : les cases à **2 liens** ou moins de la position du joueur sont révélées. Les cases déjà vues restent visibles tant que le joueur est dans la zone.
- Case qui ouvre du brouillard : arrêt, le joueur voit la nouvelle portion et décide (continuer, changer de chemin).
- **Fermeture** : le brouillard se referme quand le joueur **quitte la zone** (bouton « Retour au QG » ou défaite totale). Fermer l'application ne compte pas comme quitter : position et brouillard sont sauvegardés (zones reprenables).
- **Réouverture** : le contenu de chaque case non fixe est retiré dans sa **pool dédiée**. Les chemins restent identiques.
- **Cases fixes** (ne changent jamais) : objectifs, cases Combat de rang Élite, case d'entrée.

## Types de cases
| Type | Déclenchement | Arrêt obligatoire | Effet | Part d'une zone |
|---|---|---|---|---|
| Combat | En passant | **Oui** | Combat de rang Facile / Normal / Élite | 6 (dont 1-2 Élite) |
| Mini-jeu | Si on s'y arrête | Non | Un des 4 mini-jeux ; peut être ignoré (pas de récompense) | 3 |
| Événement narratif | Si on s'y arrête | Non | Situation + 2-3 choix (voir `02_Lore_Narration/05_Evenements_New_York.md`) | 3 |
| Ressource / Coffre | **En passant** | Non | Gain immédiat (voir Récompenses) | 6 |
| Repos / Soin | Si on s'y arrête | Non | −30 % de fatigue à toute l'équipe ; une fois par ouverture de zone | 2 |
| Carrefour / vide | — | Non | Aucun | 4 |
| Entrée | — | Non | Retour au QG | 1 |

Ramasser les ressources en passant récompense les longs déplacements et rend le dé bonus attractif.

## Objectifs de zone
- **3 objectifs par zone**, dans n'importe quel ordre. Types : vaincre un combat clé, réussir un mini-jeu, atteindre une case précise, résoudre un événement avec une issue donnée.
- Règle de composition : au moins **1 combat clé** par zone ; la **3e zone de chaque acte** contient le **mini-climax** (événement scénarisé + boss).
- **Persistance** : un objectif rempli reste rempli, même si le brouillard se referme. Les cases objectif sont fixes.
- **Validation** : quand les 3 objectifs sont remplis → écran de fin de zone, récompenses de validation, zone suivante débloquée, version **difficile** de la zone débloquée.
- La liste des 15 zones de New York et de leurs objectifs est dans `02_Lore_Narration/03_New_York.md`.

## Récompenses (valeurs du tableur, joueur à 1 zone/jour)
| Source | Crédits | PR | Autre |
|---|---|---|---|
| Combat Facile / Normal / Élite | 200 / 300 / 500 | — | XP de combat pour l'équipe |
| Ressource / Coffre (tirage) | 150-400 | 5-15 | 20 % de chances : objet (salon, consommable, accélérateur 15 min) |
| Mini-jeu réussi | 200 | 5 | — |
| Événement | Selon le choix | Selon le choix | Fatigue, objet, recrue mise en avant dans la pool |
| **Validation de zone (1re fois)** | 1 500 | 40 | 150 XP de pass |
| Validation en mode difficile (1re fois) | 2 250 | 60 | Succès cosmétique à la fin d'un acte en difficile |

Total visé par zone, en visitant environ deux tiers des cases : **~4 500 Crédits et ~75 PR** (paramètres `cr_zone` et `pr_zone` du tableur).

## Mode difficile
- Débloqué **zone par zone** après validation.
- Ennemis **+5 niveaux** au-delà de leur rang ; récompenses de validation **×1,5**.
- Mêmes objectifs, pools de cases plus agressives (plus de combats Normaux que Faciles).

## Fatigue sur le plateau
- Se déplacer ne coûte pas de fatigue. Seuls les combats (et certains événements) en coûtent.
- Si toute l'équipe atteint 100 % de fatigue : retour immédiat au QG, le brouillard se referme.

## Données / paramètres
| Paramètre | Valeur |
|---|---|
| Dé | 1d6 |
| PM par dé bonus | 6 |
| Réserve de dés bonus | 2 |
| Rayon de visibilité | 2 liens |
| Cases par zone | ~25 |
| Objectifs par zone | 3 |
| Repos | −30 % fatigue, 1 fois par ouverture |

## Outils
- Éditeur de plateau dans Unity : placer les cases, tracer les liens, assigner les pools et les cases fixes, prévisualiser la visibilité.
- Validation automatique : pas d'impasse, au moins 3 embranchements, objectifs atteignables.
