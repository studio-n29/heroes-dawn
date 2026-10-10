# Évolutions plateau, combat, équipement et agents — Plan d'implémentation (feuille de route)

> **Pour l'exécutant :** un sous-projet = une branche `feature/<nom>` issue de `main` (Git Flow), une spec courte dans `Code/docs/specs/`, puis un plan détaillé par tâches (superpowers:writing-plans) au moment de l'attaquer. Ce document fixe le découpage, l'ordre, les dépendances et les décisions à trancher.

**Objectif :** livrer les évolutions demandées (déplacement continu, chemins Bézier à coût, animations comics, narration, brouillard visuel ; combat lisible avec manipulation de l'ordre d'action ; objets, grimoire et cartes ; 30 agents par ville ; dialogues comics).

**Base :** `main` après fusion A→E (combat Active Timeline, plateau/déplacement, framework d'activités, outil de level design, activités jouables). Contraintes inchangées : domaine déterministe (entiers, `Pcg32`), Clean + MVP, prefabs par recettes (une `MonoBehaviour` par fichier), textes dans `Loc`, `[Description]` sur chaque test, aucune UI construite à l'exécution.

## Ordre et dépendances

```
F  Chemins Bézier + coût PM ─┬─> G  Déplacement continu
                             └─> (outil de level design : édition du coût et des courbes)
H  Animations comics ─┬─> I  Dialogue box + narration avant/après objectif
J  Combat : lisibilité ─> K  Combat : manipulation de l'ordre d'action
L  Objets (2 par agent) ─> M  Grimoire et cartes ─> (boosters : combats, quêtes, mini-jeux, boutique)
N  Obtention des agents (30 par ville, arcs, événements, prouesses)
O  Brouillard visuel (indépendant)
```

Ordre conseillé : **F → G → H → I → O → J → K → L → M → N**. Le plateau d'abord (le plus visible, peu de risque d'équilibrage), puis le combat, puis la méta-progression, qui dépend du combat stable.

---

## F — Chemins en courbes de Bézier avec coût en PM
**Contenu**
- `Definitions` : `PathDef` (ou champ sur l'arête) gagne `CostPm` (défaut 1) et deux points de contrôle (`Vec2` en coordonnées carte) ; si absents, courbe auto-générée (tangentes depuis les voisins) pour garder les zones existantes valides.
- Domaine : `BoardGraph` pondéré ; le plus court chemin devient un Dijkstra sur le coût PM (égalités départagées par l'identifiant, pour rester déterministe). `MovementRules` consomme le coût de chaque arête, pas 1 par case. Un trajet risqué mais rapide = arête à coût faible traversant un danger (combat, piège) ; le coût porte donc sur le **temps/PM**, pas sur la distance.
- Présentation : `BoardMapView` trace les courbes (échantillonnage Bézier cubique → `UILineRenderer`/mesh), étiquette de coût au milieu de chaque tracé ; à la sélection d'une case, coût total en PM du chemin affiché dans le HUD et la fiche (popup « S'y rendre »).
- Atteignabilité, portée du brouillard (`FogReachPlan`) et surbrillance : recalculées en coût pondéré.
- Outil de level design : poignées de Bézier dans la fenêtre, champ coût dans l'inspecteur d'arête, validation (`ZoneValidator` : coût ≥ 1, courbe dans les limites, pas de croisement aberrant en warning).
- Migration : `ZoneAssetConverter` accepte les anciens assets (coût 1, courbe auto) ; régénérer fonds/zones.
**Tests :** Dijkstra pondéré (égalité, coût nul interdit), consommation des PM par arête, atteignabilité avec coûts, échantillonnage Bézier (bornes, longueur), conversion ancien asset, PlayMode : coût affiché à la sélection.
**À trancher :** les chemins sont-ils orientés ? Un coût peut-il dépendre de la météo/biome (hors périmètre pour l'instant) ?

## G — Déplacement continu avec arrêt sur combat
**Contenu**
- `MovementRules` : à partir d'une case cible, calcule le chemin de base (le plus court au sens du coût) ; à chaque bifurcation le pion suit ce chemin sans demander. Il s'arrête **uniquement** à l'entrée d'une case de combat, ou en fin de PM / à l'arrivée. Les autres cases (événement, activité) ne stoppent pas ; leur déclenchement a lieu à l'arrivée sur la case cible (à confirmer : voir ci-dessous).
- Machine d'état de déplacement : `Idle → Moving(path, index) → Interrupted(combat) | Arrived`. Reprise après combat : le pion repart vers la cible initiale s'il reste des PM (sinon on reste sur la case de combat résolue).
- Présentation : animation continue du pion le long de la courbe (vitesse constante), pas d'arrêt visuel aux cases traversées ; la caméra suit ; l'animation d'entrée en combat (H) se joue à l'arrêt.
- Sauvegarde : l'état de déplacement en cours doit être restauré (case, index, cible).
**Tests :** parcours sans interruption, arrêt à la première case de combat, bifurcation → chemin de base, reprise après victoire/défaite, déterminisme (même graine → même parcours), PlayMode : pion traverse trois cases puis s'arrête sur un combat.
**À trancher :** une case événement/activité traversée (non finale) se déclenche-t-elle ? Hypothèse : non, seul le combat interrompt, comme demandé.

## H — Animations d'entrée comics (« COMBAT ! », « Bang », « Outch »)
**Contenu**
- `ComicBurstView` (prefab, recette) : texte Bangers, étoile/bulle dentelée, rotation, « pop » avec rebond, secousse d'écran légère ; coupé en partie par `MotionSettings.Reduced`.
- `IEntryCue` + catalogue : type de case → mot (`COMBAT !`, `ÉVÉNEMENT !`, `BANG`, `OUTCH` sur dégât/piège) et couleur ; durée courte (≈0,9 s), non bloquante pour la sauvegarde mais avant le chargement de scène.
- Brancher dans `CasePopupPresenter`/flux de combat : cue → puis transition ; cues réutilisables pour résultats d'activité.
**Tests :** catalogue complet (chaque type de case a un mot), présentateur appelle la cue avant la transition (ordre), mode réduit, PlayMode capture.

## I — Dialogue box améliorée + narration avant/après objectif
**Contenu**
- `DialogueBoxView` : avatar du locuteur (portrait ou primitive colorée en V1), bulle de style comics (queue vers l'avatar, contour épais), nom, effet machine à écrire, avance au toucher.
- Données : `NarrationDef` (liste de répliques `speaker`, `textKey`, `portraitKey`) attachée à une case : `Before` et/ou `After`. `NodeDef` gagne `Narration`. Les `After` ne se déclenchent qu'après résolution réussie.
- Outil de level design : éditeur de répliques par case (avant/après) + validation des clés `Loc`.
- Réutiliser la même vue dans l'activité Dialogue existante.
**Tests :** ordre avant → action → après, pas d'« après » en cas d'échec, sérialisation, validation des clés, PlayMode.

## O — Brouillard de guerre visuel
**Contenu :** effet visuel sur les zones cachées (calque de nuages/dégradé animé, bords doux autour de la portée), distinct de la logique `FogReachPlan` existante ; retrait animé à la révélation ; coût mobile faible (une texture + masque généré depuis la carte). Option `Reduced`.
**Tests :** masque généré = cases cachées, révélation met à jour le masque, capture visuelle.

---

## J — Combat : lisibilité
**Contenu**
- Boutons d'action en **icônes sous chaque agent** (4 emplacements) ; icône, état (prêt/recharge en temps actif), coût.
- Timeline agrandie et mise en évidence (largeur, marqueurs d'agents/ennemis, zone d'action 0 en surbrillance, pause globale visible).
- Rythme : coefficient de vitesse de la frise abaissé dans `BalanceConfig` (`TimelineSpeed`), à régler après essais ; ajustable en jeu pour tests.
- **Affichage des 4 compétences (point à définir)** — trois pistes à maquetter avant de coder :
  1. *Éventail sous l'agent* : 4 icônes en arc sous le portrait ; appui long = détail (recommandé : lisible, tactile, cible ≥ 96 px).
  2. *Rangée fixe en bas* : agent sélectionné → ses 4 icônes en barre (moins de place par agent, mais un tap de plus).
  3. *2 + 2* : compétences 1-2 (uniques) en gros, 3-4 (cartes) plus petites sous elles, pour refléter le grimoire.
  Décision attendue avant la tâche UI du combat ; les maquettes se feront dans `03_Art_UI_UX/Maquettes` (lecture seule côté code).
**Tests :** présentateur (état prêt/recharge des icônes), vitesse de frise configurable, captures PlayMode avant/après.

## K — Combat : manipulation de l'ordre d'action
**Contenu**
- Domaine `Combat` : effets `Knockback(units)` (repousse l'icône d'un ennemi en arrière sur la frise, avec plafond et résistance éventuelle) et `Advance(units)` (propulse un allié vers 0). Si l'allié atteint 0 avant l'ennemi, il agit en premier (court-circuite l'action ennemie).
- Règles à fixer : valeur max de recul, immunité après un recul (anti-lock), égalité de position (ordre stable par identifiant), interaction avec les combos de proximité et le cooldown en temps actif.
- Rattacher aux catégories de compétences existantes (effets secondaires) : attaques « Impact », soutiens « Élan ».
- Présentation : l'icône glisse avec animation (+ cue comics « BAM ! » sur recul), feedback sur la frise.
**Tests :** recul d'un ennemi retarde son action, avancée d'allié court-circuite, plafond/immunité, déterminisme, non-régression des 100+ tests de combat.

---

## L — Objets (jusqu'à 2 par agent)
**Contenu**
- `ItemDef` : id, rareté (Commun/Rare/Épique/Légendaire), emplacement, effets (modificateurs de stats, effets déclenchés, bonus d'activité). `AgentState` : 2 emplacements.
- Sources : boutique à rotation (étendre la boutique éphémère existante, graine par rotation), événements (récompense de case).
- UI : fiche agent (2 emplacements), inventaire, étiquette de rareté (couleur + libellé, jamais la couleur seule).
- Persistance : inventaire + équipement dans le profil ; migration de sauvegarde.
**Tests :** équiper/déséquiper (max 2), effets appliqués au combat (calcul des stats), rotation boutique déterministe, achat atomique, migration.
**À trancher :** objets consommables ou permanents ? échangeables entre agents ?

## M — Grimoire et cartes de compétences
**Contenu**
- Compétences 1 et 2 : propres à chaque agent (inchangées, définies dans `MemberDef`).
- Compétences 3 et 4 : emplacements de **cartes** ; `SkillCardDef` (catégorie d'agent autorisée, effet de base + sous-effets, rareté, doublons).
- Grimoire : collection (possédées, quantités, niveau de carte par fusion de doublons), équipement par agent (vérifie la catégorie).
- Boosters : `BoosterDef` (tirage pondéré par rareté, pitié) ; sources : exploits de jeu (récompense directe), progression narrative (fin de chapitre/niveau), mini-jeux (score → booster), boutique. Un `BoosterSource` unique appelé par chaque système, pour rester extensible.
- UI : page Grimoire (collection filtrée par catégorie), ouverture de booster (animation comics), écran d'équipement des cartes depuis la fiche agent.
- Combat : compétences 3/4 issues des cartes équipées ; mêmes effets que les compétences actuelles (réutiliser le moteur).
**Tests :** tirage déterministe (graine) et distribution, pitié, équipement limité par catégorie, doublons → niveau, sources de boosters, migration de sauvegarde, PlayMode d'ouverture.
**Dépend de :** J (emplacements de compétences à l'écran), K (effets Knockback/Advance disponibles comme cartes).

## N — Obtention des agents
**Contenu**
- Passer à **30 agents par ville** : données dans `Content` (statistiques, catégories, noms de code anglais, textes `Loc`) ; équilibrage via `BalanceConfig` ; tableau d'équilibrage à valider avec le GDD.
- Sources de déblocage : (a) fin d'un arc narratif précis (flag de progression → recrue), (b) événements temporaires dans certains niveaux (fenêtre de disponibilité, durée), (c) compagnons par prouesse (ex. terminer un niveau difficile avec un seul agent → `AchievementDef` évalué en fin de zone).
- `UnlockRule` (type + condition) évaluée par un service unique ; notifications/toasts, page Recruter/Équipe étendue (verrouillés visibles avec condition).
- Outil de level design : poser un déblocage sur une case/zone.
**Tests :** chaque règle (arc, événement temporaire, prouesse « un seul agent »), pas de doublon de recrutement, persistance, intégrité des 30 agents (statistiques valides, catégories couvertes).

---

## Découpage en livraisons
| Lot | Sous-projets | Résultat visible |
|---|---|---|
| 1 | F, G | Chemins courbes à coût, déplacement continu |
| 2 | H, I, O | Cues comics, dialogues avec avatar, brouillard |
| 3 | J, K | Combat lisible + ordre d'action manipulable |
| 4 | L, M | Objets, grimoire et cartes |
| 5 | N | 30 agents par ville et déblocages |

Chaque lot : branche(s) dédiée(s), spec, tests (EditMode + PlayMode), merge dans `main`, rapports régénérés, nouvelle APK dans `Drive/01_PROJETS/Heroes_Dawn/_builds`.

## Risques
- **Équilibrage** (K, L, M, N) : prévoir des paramètres dans `BalanceConfig` et des tests de non-régression ; aucune valeur définitive sans essais.
- **Migration de sauvegarde** (L, M, N) : versionner le profil dès L.
- **Performance mobile** (O, H) : textures partagées, pas d'allocation par frame.
- **Portée** : M et N sont lourds ; les découper en sous-lots (collection puis boosters ; 10 agents puis 30).

## Décisions attendues de ta part avant de commencer
1. Affichage des 4 compétences en combat (éventail / rangée / 2+2).
2. G : seules les cases de combat interrompent le trajet (hypothèse retenue).
3. L : objets échangeables/consommables ?
4. N : liste ou critères des 30 agents de la première ville (je peux proposer une répartition par catégorie).
