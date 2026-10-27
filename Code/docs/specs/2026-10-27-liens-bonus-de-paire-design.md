# Liens entre agents : bonus propres à chaque paire (lot D de la 0.14.0)

> Spec du 2026-10-27. Source : document N29 HQ, point « Lien entre les agents » (chaque agent jouant ensemble obtient de l'expérience de lien, augmentant un niveau de lien et débloquant des bonus à certains paliers, uniquement quand ils sont dans la même équipe). Complète `2026-10-23-synergie-liens-effets-design.md` (expérience de lien par victoires communes, niveaux 1 à 5, amplification des synergies de famille), qui reste en vigueur.

## Décisions
- **Bonus propres à chaque paire**, débloqués aux niveaux de lien 2 à 5, **en plus** de l'amplification des synergies de famille (inchangée).
- Le bonus dépend du **couple de rôles** des deux agents (15 couples) ; chaque couple a un thème et une suite de 4 bonus, un par niveau.
- **Mécaniques existantes seules** : statistiques en %, critique, bouclier d'ouverture, jauge de départ, énergie de départ, énergie par pas. Aucun nouvel effet de combat.
- **Actifs seulement quand les deux agents sont dans l'escouade du combat**, et appliqués **aux deux agents de la paire** (pas à toute l'équipe). Un agent engagé dans plusieurs paires cumule leurs bonus, plafonnés.
- L'expérience de lien reste le nombre de victoires communes ; paliers inchangés : 3, 8, 15 et 25 victoires pour les niveaux 2 à 5.

## Données (domaine, `Definitions/PairBonuses.cs`)
- `enum PairBonusKind { HpPercent, AtkPercent, DefPercent, SpdPercent, CritPercent, OpeningShieldPercent, StartGaugePercent, StartEnergy, EnergyPerStep }`.
- `readonly struct PairBonus { PairBonusKind Kind; int Value; }`.
- `readonly struct PairBonusTotal` : somme par type, **plafonnée** (`Hp/Atk/Def/Spd ≤ 12`, `Crit ≤ 15`, `OpeningShield ≤ 20`, `StartGauge ≤ 20`, `StartEnergy ≤ 20`, `EnergyPerStep ≤ 1`) ; `IsNone`.
- `static class PairBonusRules` :
  - `IReadOnlyList<PairBonus> Of(Role a, Role b, int level)` : bonus **actifs** à ce niveau (cumul des niveaux 2 jusqu'à `level`) ; vide au niveau 1 ou moins ; l'ordre des rôles ne compte pas ; niveau borné à 5.
  - `PairBonus At(Role a, Role b, int level)` : le bonus ajouté par ce niveau précis (2 à 5), pour l'interface.
  - `string ThemeKey(Role a, Role b)` : clé de texte `link.theme.<roleA>_<roleB>` (rôles dans l'ordre de l'enum).
  - `PairBonusTotal Total(IEnumerable<PairBonus> bonuses)` : somme puis plafonds.
- Aucun changement de sauvegarde : `MetaSave.links` (victoires par paire) est déjà là.

## Table des 15 couples
| Couple | Thème | Niv. 2 | Niv. 3 | Niv. 4 | Niv. 5 |
|---|---|---|---|---|---|
| Tank + Tank | Double rempart | DEF +4 | PV +4 | Bouclier 8 | Bouclier +7 |
| Tank + Dégâts | Rempart et lame | PV +3 | ATQ +3 | Énergie départ +10 | Critique +5 |
| Tank + Contrôle | Étau | DEF +3 | VIT +3 | Jauge départ +10 | Énergie départ +10 |
| Tank + Soutien | Veilleurs | PV +4 | DEF +3 | Bouclier 8 | Énergie/pas +1 |
| Tank + Précision | Garde et regard | DEF +3 | Critique +4 | Jauge départ +10 | Critique +4 |
| Dégâts + Dégâts | Double tranchant | ATQ +4 | ATQ +4 | Critique +5 | Énergie départ +10 |
| Dégâts + Contrôle | Marteau et étau | ATQ +3 | VIT +3 | Énergie départ +10 | Jauge départ +10 |
| Dégâts + Soutien | Lame soutenue | ATQ +3 | PV +3 | Énergie départ +10 | Énergie/pas +1 |
| Dégâts + Précision | Tir croisé | ATQ +3 | Critique +4 | Critique +4 | ATQ +4 |
| Contrôle + Contrôle | Tisseurs | VIT +4 | VIT +4 | Jauge départ +10 | Énergie départ +10 |
| Contrôle + Soutien | Sérénité | PV +3 | VIT +3 | Énergie départ +10 | Énergie/pas +1 |
| Contrôle + Précision | Piège | VIT +3 | Critique +4 | Jauge départ +10 | Critique +4 |
| Soutien + Soutien | Cercle de soin | PV +4 | PV +4 | Bouclier 8 | Énergie/pas +1 |
| Soutien + Précision | Appui précis | PV +3 | Critique +4 | Énergie/pas +1 | Critique +4 |
| Précision + Précision | Double regard | Critique +4 | VIT +3 | Critique +4 | Jauge départ +10 |

Unités : statistiques, critique, bouclier (% des PV max, 2 tours) et jauge de départ (% de la frise) en pourcentage ; énergie de départ en points d'énergie ; énergie/pas en points par pas de la frise. Valeurs de départ, à ajuster au playtest.

## Combat
- `UnitFactory.Ally(..., PairBonusTotal pair = default)` : les bonus de statistiques s'ajoutent aux pourcentages de PV, ATQ, DEF et VIT (comme ceux des objets) ; `CritPercent` → `CritBonusPercent` ; `OpeningShieldPercent` → bouclier de départ (comme l'effet d'objet) ; `StartGaugePercent` → `StartPointBonus` (même conversion que l'effet d'objet) ; `StartEnergy` → énergie de départ, plafonnée à l'énergie max ; `EnergyPerStep` → `EnergyRegenBonus`.
- `CombatSimulation` : avant de créer chaque allié, on parcourt ses paires dans l'escouade ; le niveau vient de `CombatSetup.LinkLevel(a, b)` ; rôles via le catalogue ; total = `PairBonusRules.Total(...)`. Un allié sans paire de niveau 2+ reçoit `default` (comportement inchangé).
- Niveau 1 partout (aucun lien) : aucun changement → simulateur d'équilibrage et rejeux identiques.
- Journal de combat : une ligne par paire active (niveau ≥ 2) au début du combat, après les lignes de synergie et d'effets d'équipe.

## Interface
- **Fenêtre d'affinités** (`AffinityFlow.Build`) : chaque ligne montre le thème du couple de rôles et ses 4 bonus : débloqués précédés de « ✓ » en couleur, à venir grisés avec leur niveau ; victoires restantes avant le niveau suivant (« encore N victoires »).
- **Écran Équipe** : sous les synergies, un bloc « Liens » listant les paires de l'escouade de niveau 2+ : thème, deux noms, niveau, bonus actifs.
- Textes : 15 thèmes (`link.theme.*`), 9 libellés de bonus, lignes d'interface et de journal.

## Documentation
- `01_Game_Design/03_Metahumains.md` (liens : bonus de paire par couple de rôles, plafonds) ; `05_Production/Decisions.md` (3 lignes : bonus de paire par rôles ; mécaniques existantes seules ; plafonds).

## Hors périmètre
Effets réactifs propres à une paire (« quand l'un tombe… »), duos nommés écrits pour des paires du lore, récompenses de niveau de lien autres que ces bonus, équilibrage fin.

## Tests
- **Domaine** (`PairBonusRulesTests`) : table (15 couples × niveaux 1 à 5, rien au niveau 1, cumul des niveaux), ordre des rôles indifférent, chaque plafond, niveau hors bornes borné.
- **Combat** : pour chaque brique (statistique, critique, bouclier, jauge, énergie de départ, énergie par pas), appliquée aux **deux** agents de la paire et **pas** à un troisième ; rien si l'un des deux n'est pas dans l'escouade ; un agent dans trois paires cumule puis plafonne ; une simulation avec liens niveau 5 se rejoue à l'identique (`CombatReplay`) ; niveau 1 = résultats identiques à l'existant.
- **Présentation** : fenêtre d'affinités (✓ et à venir, victoires restantes), écran Équipe, journal de combat.
- **Sauvegarde** : un profil existant (avec ou sans liens) se charge sans changement ; les niveaux de lien sont conservés.
- **Capture** : fenêtre d'affinités d'un agent avec des liens de niveaux 2 à 5, vérifiée à l'œil.
