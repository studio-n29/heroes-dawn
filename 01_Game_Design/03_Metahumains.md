# Méta-humains : familles, rôles, synergies, progression

> Résumé dans le GDD : `00_GDD/GDD_Heroes_Dawn.md` §7 · Statut : **v1 complète**
> ⚑ = recommandation qui touche le feeling, à confirmer en prototype.
> Kits : `06_Kits_Competences.md` · Roster NY : `02_Lore_Narration/04_Recrues_New_York.md` · Stats : tableur, onglet Stats

## Objectif du système
Donner au joueur une collection qu'il **choisit** (pas de hasard) et des équipes dont la force vient des **synergies** entre familles, pour porter les piliers « Mon équipe, mes choix » et « Vivre l'univers Apex ».

## Anatomie d'une recrue
| Élément | Détail |
|---|---|
| Identité | Nom, nom de code, bio (Agent : 2-3 lignes ; Élite/Légende : fiche complète) |
| Famille | Matière, Élémentaire, Physique/Bio ou Perception |
| Rôle | Tank, Dégâts, Contrôle, Soin/Soutien ou Précision (fixe les stats de base) |
| Rareté | Agent, Élite, Légende (multiplicateur de stats ×1 / ×1,2 / ×1,45) |
| Kit | 3 actives + 1 passive |
| Niveau | 1 à 50 |
| Fatigue | 0-100 % |

## Familles
| Famille | Rôle dominant | Couleur UI | Profils du lore |
|---|---|---|---|
| Matière | Contrôle, création | Ambre | Ferromodelage, lithomodelage, cristallisation, polymorphie partielle |
| Élémentaire | Dégâts, altérations | Cyan | Électrogenèse, pyrochimie/corrosion, cryogénie |
| Physique / Bio | Tank, soin | Rouge | Force, insensibilité, régénération, adaptation, adhérence |
| Perception | Précision, esquive, détection | Violet | Bullet-time, écholocation, camouflage, magnétoréception, vision thermique, vigilance |

## Rôles des recrues de New York
| Rôle | Recrues |
|---|---|
| Tank | Ancre, Argile, Rempart, Rivet, Mortier, Cuirasse, Marée |
| Dégâts | Givre, Arc, Acide, Gecko, Éclat, Fumée, Vapeur, Bélier |
| Contrôle | Verre, Maillon, Mue, Filin, Masque, Frimas, Néon, Rouille, Étincelle |
| Soin / Soutien | Instant, Sève, Boussole, Antidote, Pouls |
| Précision | Écho, Spectre, Veille, Radar, Réflexe, Thermo |

## Synergies
### Combos de famille (validés)
| Paire | Combo | Effet |
|---|---|---|
| Matière + Élémentaire | Projectiles chargés | 150 % à tous les ennemis + altération de l'élémentaire (Brûlure, Gel, Choc ou Corrosion) |
| Matière + Physique/Bio | Armure forgée | Bouclier de 30 % PV max sur le tank + renvoi de 20 % des dégâts (2 tours) |
| Matière + Perception | Piège | 1 ennemi : Entrave + ne remplit plus sa jauge (2 tours) |
| Élémentaire + Physique/Bio | Corps élémentaire | Le tank : −30 % dégâts subis + aura de 30 % à tous les ennemis à chacun de ses tours (2 tours) |
| Élémentaire + Perception | Point faible | 350 % à 1 ennemi, ignore Bouclier et DEF |
| Physique/Bio + Perception | Triage | Soigne 25 % PV max (40 % sur les alliés sous 30 %) + retire toutes les altérations |
| Même famille | Résonance | Matière : Bouclier équipe 15 % · Élémentaire : 120 % à tous · Physique/Bio : Régénération équipe · Perception : Hâte équipe + Marque sur tous |

### Duos légendaires de New York
| Duo | Combo | Effet | Remplace |
|---|---|---|---|
| Verre + Givre | Tempête de cristal | 200 % à tous + Gel (2 tours) + Vulnérabilité | Projectiles chargés |
| Écho + Instant | Œil absolu | 3 tours : toutes les attaques de l'équipe touchent et sont critiques ; Esquive sur l'équipe | Résonance Perception |

Chaque nouvelle ville apporte 2 duos légendaires entre ses Légendes.

## Recrutement (rappel, détail dans `04_Economie.md`)
- Pool quotidienne de **5 recrues** de la ville, jamais de doublon, coût fixe en PR (150 / 450 / 1 200).
- Les Légendes entrent dans la pool après le mini-climax de leur acte ; elles y restent jusqu'à leur recrutement (**emplacement réservé**, en plus des 5).
- Premier recrutement du prologue : **gratuit**, au choix parmi 3 Agents (Rivet, Étincelle, Pouls : un tank, un attaquant, un soutien).
- Les 2 membres prêtés du prologue (Arc et Rempart) rejoignent la pool dès la fin du prologue.

## Progression
### Niveaux (1 → 50)
- XP par l'**entraînement au salon** (temps réel) et par les **combats**.
- Courbe : l'XP demandée augmente de 10 % par niveau. Environ 29 jours d'entraînement pour passer un membre de 1 à 50 (tableur, onglet Progression).
- **Plafond par acte** ⚑ : niveau max = 10 × acte le plus avancé atteint, toutes villes confondues (Acte 1 : 10, … Acte 5 : 50). Évite qu’un joueur fasse monter un seul membre trop vite.

### Paliers de compétences
- Aux niveaux **10, 20, 30, 40, 50**. Un palier atteint doit être **validé en Crédits** (2 000 → 32 000, ×2 par palier).
- Effets génériques : voir `06_Kits_Competences.md`.

### Entraînement au salon
- Emplacements : 2 au niveau 1 du salon, jusqu'à 6 (voir QG).
- Les **objets placés** donnent un bonus d'XP (+25 % en moyenne) ; ils s'achètent en Crédits et se gagnent dans les coffres.
- Un membre à l'entraînement **récupère aussi** sa fatigue à vitesse normale.

### Fatigue hors combat
- Récupération naturelle : **5 % par heure** (20 h pour 100 %).
- Infirmerie : ×2 (jusqu'à ×3 au niveau 10), emplacements limités.
- Objets : trousse de soin (−25 %), accélérateur premium.

## Données / paramètres
Multiplicateurs de rareté, courbe d'XP, coûts des paliers, fatigue : tableur (Paramètres, Progression, Stats).
