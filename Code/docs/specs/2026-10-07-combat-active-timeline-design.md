# Combat : Active Timeline (v2) — design

> Statut : **Time Bank retirée** (voir §2) ; demandé par l'utilisateur (2026-10-07), implémenté sur `feature/combat-active-timeline` (départ de `main`).
> Sous-projet **A** du découpage A → B → C → E → D (A combat, B plateau/déplacement, C framework d'activités, E outil de level design, D activités).
> Il **fait évoluer** la frise de `2026-10-05-combat-timeline-design.md` (zone de préparation, recul, Ancrage, brûlure au pas restent en place ; la Time Bank est retirée) ; ce document ne décrit que les changements.

## 1. Règles demandées
1. Chaque participant avance sur la frise de **100 à 0** (affichage ; en interne la position reste 0 → 100 %, `RemainingPercent = 100 − position`).
2. Quand un participant arrive à **0**, c'est son tour : **toute la frise est figée** (aucune unité n'avance, aucune recharge ne baisse, la brûlure ne bouge pas) jusqu'à ce que l'action soit jouée. Les ennemis et le mode auto agissent immédiatement ; la pause ne concerne que l'attente du joueur.
3. **Combo** : quand un allié arrive à 0 et qu un allié de combo valide est proche de 0 (`ComboSyncWindowPercent` = 60 % de progression), le combo est possible, sans coût.
4. **Compétences d'agent** : utilisables uniquement à leur tour. **Compétences d'avatar** (le joueur) : utilisables à tout moment, y compris pendant la pause, avec une recharge qui ne baisse que pendant le temps actif.
5. **Catégories** : Dégâts, Soins, Protection, Boost, Gestion du temps. Chaque compétence a un **effet de base** (le premier de `Effects`) et des **sous-effets** (les suivants : faiblesses, altérations…).
6. **Noms de code** des agents : toujours en anglais, quelle que soit la langue.
7. **Vagues** : un combat = une ou plusieurs vagues (ex. vague 1 : 3 ennemis, vague 2 : 2 ennemis). Victoire quand la dernière vague est vaincue.

## 2. Décisions de design (à confirmer)
- **Time Bank retirée** (décision de l utilisateur, 2026-10-07) : plus de réserve de Temps, ni de « Dépassement ». Les compétences d avatar de gestion du temps (Recul) en tiennent lieu. Le combo à proximité est gratuit.
- **Catégorie dérivée** de l'effet de base (pas de donnée en plus) : Dégâts = `Damage` ou altération sur ennemi ; Soins = `Heal` ; Protection = `Shield` ou altération défensive (Provocation, Esquive, Ancrage) ; Boost = autre altération sur alliés (Hâte, Régénération) ; Gestion du temps = `GaugeDelta`.
- **Recharge d'avatar en ticks de temps actif** (`CooldownTicks`) ; la pause ne la fait pas baisser.
- **Vagues** : les ennemis de toutes les vagues reçoivent leurs identifiants à l'apparition de la vague (déterministe, rejouable). Les alliés gardent PV, statuts et recharges d'une vague à l'autre.
- Un `CombatSetup` sans vagues explicites se comporte comme avant (une vague).

## 3. Interface
- Frise horizontale : **100 à gauche → 0 à droite** (zone rouge = les 20 derniers points) ; alliés sous la ligne, ennemis au-dessus. À 0, tout se fige et le panneau de compétences s ouvre.
- **Barre d avatar** (5 boutons Q W E R T) sous le bouton de combo : nom court, recharge en secondes de temps actif, couleur par catégorie ; cible unique ennemie = ciblage au toucher (Échap annule).
- **Vagues** : les ennemis d une nouvelle vague remplacent ceux de la précédente (carte reconstruite), bandeau « VAGUE n ! », compteur dans la barre du haut. `EncounterDef.Waves` décrit les vagues d une rencontre.
- **Noms de code** (`Loc.CodeNames`) : Anchor, Glass, Frost, Echo, Arc, Bulwark, Sap, Spark — identiques dans toutes les langues. ⚑ Noms anglais proposés, à valider par le game design/lore.

## 4. Hors périmètre de ce sous-projet
Éditeur de vagues dans l'outil de level design (sous-projet E), contenu final des compétences d'avatar.
