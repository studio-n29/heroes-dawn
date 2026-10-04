# Combat : frise chronologique, zone de risque et Time Bank — design

> Statut : demandé par l'utilisateur (2026-10-05), implémenté sur `feature/combat-timeline`. Le GDD (`00_GDD/*`) et le tableur d'équilibrage ne sont pas modifiés : tous les nouveaux paramètres vivent dans `BalanceConfig` (valeurs par défaut ci-dessous), à reporter dans le tableur par le game design.

## 1. Frise chronologique (A)
- La jauge ATB devient la **position sur la frise**, de 0 % (départ) à 100 % (point d'action). Une unité à 100 % attend son action (joueur) ou agit (IA).
- **Coût en temps** : après une compétence, l'unité est replacée selon son poids.

| Action | Position après | Paramètre |
|---|---|---|
| A1 (légère) | 30 % | `TimelineLightResetPercent` |
| A2 (intermédiaire) | 15 % | `TimelineMediumResetPercent` |
| A3 (lourde / ultime) | 0 % | `TimelineHeavyResetPercent` |
| Combo (chaque partenaire) | −15 % (canalisation à rattraper) | `TimelineComboResetPercent` |

- **Recul (push-back)** : un contrôle qui touche une unité pas encore prête la repousse : Entrave −25 %, Choc −15 %, Gel −20 % (`PushBack*Percent`). Si l'unité avait une action annoncée et sort de la zone de préparation, l'action est annulée. Une unité prête n'est pas repoussée.

## 2. Zone de préparation et risque (C)
- **Zone 80-100 %** (`TimelinePrepZonePercent`) : un ennemi qui y entre **annonce son action** (compétence choisie à ce moment, visible sur la frise ; A3 = « grosse attaque »). Il exécute l'action annoncée si elle est encore possible.
- **Critique en zone** : un coup critique sur une unité (alliée ou ennemie) en zone de préparation annule son action et la renvoie à 40 % (`TimelineInterruptResetPercent`).
- **Familles** :
  - **Perception** : précognition. Tant qu'un allié Perception est vivant, les actions ennemies sont annoncées dès 50 % (`TimelinePrecognitionPercent`). Écho ralentit tous les ennemis (A2 : −10 % de frise).
  - **Matière** : ancres temporelles. Nouveau statut **Ancrage** (2 tours) : aucun recul, aucune interruption. Posé sur toute l'équipe par les ultimes d'Ancre et de Verre.
  - **Élémentaire** : la **Brûlure** inflige 8 % des PV max par frise complète parcourue (`BurnPercentPerTimeline`), à chaque pas (reste reporté, entiers). Elle ne coûte rien à une unité qui n'avance pas.

## 3. Réserve temporelle d'équipe — Time Bank (B)
- Pool partagé 0-100 (`TimeBankMax`), 40 au départ, +1 tous les 10 ticks (0,5 s).
- **Dépassement** (40) : un allié pas prêt (le plus avancé) passe à 100 % tout de suite ; l'ennemi ciblé recule de 40 % et perd son action annoncée.
- **Combo synchronisé** (25) : un allié prêt et un partenaire de combo valide à ≥ 60 % de la frise : le Temps rattrape le partenaire et le combo part. Plus besoin d'arriver au même instant.
- Les deux usages sont des commandes journalisées (`CommandType.TimeSkip`, `Combo` avec drapeau de synchronisation) : le rejeu et la vérification serveur restent identiques (`StateHash` inclut Temps, actions annoncées, reste de brûlure).

## 4. Interface
- **Frise** sous les ennemis : alliés sous la ligne, ennemis au-dessus, zone rouge 80-100 %, action annoncée au-dessus de l'ennemi (« A3 !! »), anneau pour les unités prêtes, pastilles prêtes étalées, canalisation estompée ; réserve de Temps à gauche.
- **DÉPASSER · 40** (puis toucher l'ennemi à couper) et **COMBO SYNC · 25** à gauche du combat ; textes flottants « PRÉPARE… », « GROSSE ATTAQUE ! », « INTERROMPU ! », « RECUL −N % », bandeau « DÉPASSEMENT ! ».
- Tutoriel : la bulle de combat présente la frise ; deux astuces (frise, Temps d'équipe).

## 5. Hors périmètre
Duo Légendaire (aucune recrue concernée dans le contenu actuel) ; équilibrage fin (les taux de victoire en auto restent proches de 100 % sur les zones 1.1-1.3 au niveau 5-8, boss 98 %) ; voix et sons dédiés.
