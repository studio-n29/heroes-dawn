# Direction artistique HUD pastel : plan d'implémentation

> **Pour les agents :** sous-compétence requise : superpowers:executing-plans (exécution native choisie par l'utilisateur : « implémente tout »). Les étapes utilisent des cases `- [ ]`.

**Goal :** Repeindre toute l'interface en HUD pastel façon Marvel's Spider-Man (verre givré, coins coupés, accent corail, polices condensées italiques), sans changer la logique ni les écrans.

**Architecture :** Les valeurs de `Theme` changent (noms conservés) ; `Ui.Panel/Btn/Bar/Inked` sont réécrits en gardant leurs signatures ; de nouveaux sprites procéduraux sont cuits par `UiAssetBaker` ; les prefabs sont régénérés par les recettes ; un balayage remplace les couleurs en dur par des jetons.

**Tech Stack :** Unity 6000.6.3f1, uGUI + TextMeshPro, prefabs générés par recettes (`Editor/Prefabs/Recipes`), `UiAssetBaker` (Resources/UI), `FontAssetGenerator`.

**Spec :** `Code/docs/specs/2026-10-12-direction-artistique-design.md` (maquette : `Code/docs/specs/assets/2026-10-12-da-hud-pastel.png`).

## Global Constraints
- Noms publics de `Theme`, `Ui`, `Sprites` conservés : aucun appelant ne doit casser ; on ajoute, on ne renomme pas.
- Palette pastel : ciel `#A9CDFA` / `#B7C4F5` / `#D3C0F0` / `#F6C4D2` ; `Navy` `#1B2550` ; `Accent` corail `#FF6B5E` (dégradé `#FF8F84` vers `#F0566A`) ; `TextMuted` `#3B4A8A` ; `FrostEdge` blanc.
- Plus de contour noir épais ni d'ombre dure ; ombre douce uniquement.
- Contraste texte/fond : au moins 4,5:1 (texte courant), 3:1 (grand texte, taille ≥ 24).
- Chaque test porte `[Description("Vérifie que … — parce que …")]` ; commits terminés par `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>` ; jamais de push ; ne pas committer `Data/`, `_deploy/`, ProjectSettings/TMP régénérés.
- Commandes Unity lancées depuis la racine du projet, Unity fermé ; `-nographics` pour EditMode et génération, sans pour PlayMode.
- Téléchargement de police : demander l'accord de l'utilisateur (Task 8).

## Review Focus
- Texte blanc sur fond givré clair (illisible) : tout texte posé sur `Frost` utilise `Navy`. Test de contraste sur les paires du thème (Task 1).
- Couleurs de famille/rareté trop proches après adoucissement : quatre familles restent distinctes (écart de teinte minimal) (Task 1).
- Bouton désactivé indistinguable de l'actif en pastel : alpha et désaturation vérifiés (Task 3).
- Barre de PV à valeur 0 ou 1 : remplissage correct sans contour noir (Task 3).
- Écran qui garde un fond noir ou des couleurs en dur : test de balayage sur `Presentation` (Task 7).

## Structure de fichiers
- Modifier `Presentation/Common/Theme.cs` (jetons), `Ui.cs` (Panel/Btn/Bar/Inked/Backdrop), `Sprites.cs` (nouveaux sprites), `UiButton.cs` (survol), `UiBar.cs` (inchangé sauf couleur).
- Créer `Presentation/Common/ThemeContrast.cs` (calcul du contraste, partagé par test et éditeur) ; `Tests/Editor/ThemeTests.cs`, `Tests/Editor/UiStyleTests.cs`, `Tests/Editor/NoHardcodedColorsTests.cs`.
- Modifier `Editor/Assets/UiAssetBaker.cs`, `FontAssetGenerator.cs`, `Editor/Prefabs/Recipes/*.cs` (ScreenKit, CombatRecipes, CommonRecipes, TeamRecipes, BoardRecipes, LoginRecipe, TutorialRecipes, WipRecipes), vues `TimelineView`, `UnitCardView`, `SkillIconView`, `CombatView`, `BoardMapView`, etc.

---

### Task 1 : Jetons de couleur et contraste

**Files :** Modify `Presentation/Common/Theme.cs` ; Create `Presentation/Common/ThemeContrast.cs`, `Tests/Editor/ThemeTests.cs`.

**Interfaces :**
- Produces : `Theme.Accent`, `AccentSoft`, `AccentDark`, `Navy`, `Frost`, `FrostEdge`, `SkyTop`, `SkyMid`, `SkyBottom`, `Glow` (tous `Color`) ; `ThemeContrast.Ratio(Color a, Color b) : float` (WCAG, composition du fond à alpha 1).
- Les anciens noms (`Primary`, `Danger`, `Background`, `Surface*`, `Text`, `TextDark`, `TextMuted`, `Overlay`, familles, `Hp`…) gardent leur nom ; valeurs : `Background` = `SkyMid`, `Surface`/`Surface2`/`Surface3` = niveaux de `Frost` (opaques), `SurfaceDark` = `Navy`, `Text` = `Navy`, `TextDark` = `Navy`, `Primary` = `Accent`, `Danger` = `Accent`, `Ink` = `Navy` (plus de noir pur), `Overlay` = `Navy` alpha 0,55.

- [ ] **Step 1 : tests.** `Theme_TextOnSurfaces_MeetsContrast` (paires : `Text` sur `Surface`, `Surface2`, `Surface3`, `Background` ; blanc sur `Accent` et sur `Navy` ; `TextMuted` sur `Surface` ≥ 4,5) ; `Theme_FamiliesStayDistinct` (les 4 couleurs de famille ont deux à deux un écart de teinte HSV ≥ 0,08 ou de luminance ≥ 0,15) ; `Theme_RarityStaysOrdered` (les 3 raretés sont distinctes). Run : FAIL (jetons absents).
- [ ] **Step 2 : implémenter** les jetons et `ThemeContrast.Ratio` ; adoucir familles/raretés/`CaseColor` (même teinte, plus claire).
- [ ] **Step 3 : lancer** `ThemeTests` (EditMode) : PASS. Commit `feat(da): jetons pastel et test de contraste`.

### Task 2 : Sprites du HUD

**Files :** Modify `Presentation/Common/Sprites.cs`, `Editor/Assets/UiAssetBaker.cs` (via `UiSpriteNames`) ; Test `Tests/Editor/UiAssetsTests.cs` (étendre).

**Interfaces :**
- Produces : `Sprites.Panel` (rectangle à coins coupés 9-slice, coin de 14 px sur 64), `Sprites.PanelSmall` (coin 8 px sur 32), `Sprites.Diamond`, `Sprites.Shine` (bande diagonale en dégradé), `Sprites.Skyline` (silhouette de ville, 512×128, alpha) ; ajoutés à `UiSpriteNames` et à `Procedural(name)`.

- [ ] **Step 1 : test** `UiSprites_AllBaked` : chaque nom de `UiSpriteNames` charge `Resources/UI/<nom>` (étendre le test existant) ; `Panel_IsSlicedWithBorder` : `Sprites.Panel.border` > 0. FAIL.
- [ ] **Step 2 : implémenter** les générateurs procéduraux (`MakePanel(size, cut)` : alpha 0 hors du polygone octogonal à coins coupés, bords anticrénelés ; losange ; bande diagonale ; skyline à immeubles de hauteurs déterministes).
- [ ] **Step 3 : cuire** `UiAssetBaker.BakeAll` en batch (`-executeMethod ...UiAssetBaker.BakeAll -quit`), relancer le test : PASS. Commit.

### Task 3 : Composants `Ui` (Panel, Btn, Bar, Inked) et UiButton

**Files :** Modify `Presentation/Common/Ui.cs`, `UiButton.cs` ; Test `Tests/Editor/UiStyleTests.cs`.

**Interfaces :**
- Consumes : jetons et sprites des Tasks 1-2.
- Produces : `Ui.Panel(Transform parent, string name, Color fill, float border = 4f, float shadow = 8f, bool raycast = true)` garde sa signature et son contrat (renvoie l'image de fond ; `.Frame()` = cadre) mais dessine : cadre `Sprites.Panel` en `FrostEdge`, fond `Sprites.Panel` teinté, ombre douce (`Shadow` alpha 0,25, distance `shadow/2`) ; nouveau `Ui.AccentBar(Image panelFill, Color? color = null) : Image` (barre verticale de 4 px à gauche, `Theme.Accent` par défaut) ; `Ui.Btn` utilise `Panel` et un dégradé corail si `bg` est `Theme.Accent`/`Theme.Primary`, sinon givré ; texte `fg` ; `Ui.Bar` : fond `Navy` alpha 0,25, bord 1 px `FrostEdge`, remplissage net ; `Ui.Inked` : pour TMP, matériau d'ombre légère (pas de contour noir) ; pour un autre graphique, `Shadow` doux.
- `UiButton` : survol éclaircit vers `Frost` ; désactivé = alpha 0,55 et fond désaturé.

- [ ] **Step 1 : tests** `Panel_KeepsFrameContract` (Frame() existe, enfant `Fill`, `Fill.rectTransform` étiré avec marge `border`) ; `Panel_HasNoBlackOutline` (couleur du cadre ≠ noir, alpha de l'ombre ≤ 0,3) ; `Button_DisabledLooksDisabled` (couleur du fond désactivé a une saturation < celle de l'actif, alpha du libellé 0,55) ; `Bar_ZeroAndFull` (`Set(0)` désactive le remplissage, `Set(1)` l'étire) ; `AccentBar_IsLeftEdge`. FAIL.
- [ ] **Step 2 : implémenter** (réutiliser les sprites ; garder `Shadow` pour l'ombre douce).
- [ ] **Step 3 : lancer** `UiStyleTests` et les tests EditMode existants de `Ui` : PASS. Commit.

### Task 4 : Fond ciel et ScreenKit

**Files :** Modify `Editor/Prefabs/Recipes/ScreenKit.cs` (+ matériau `UiBackdrop` si besoin), appelants des 11 `Backdrop/ArtBackdrop`.

**Interfaces :** `ScreenKit.Backdrop(RectTransform rt, Color top, Color bottom)` garde sa signature : peint le dégradé ciel (`SkyTop`→`SkyBottom`), la lueur pêche, la trame de points discrète et `Sprites.Skyline` en bas ; `ArtBackdrop` : voile `Navy` alpha ≤ 0,25 au lieu de noir 0,6+. Les appelants passent `Theme.SkyTop/SkyBottom`.

- [ ] **Step 1 :** régénérer les prefabs (`PrefabGenerator.GenerateAllBatch -overwrite <chemins>`), ouvrir/capturer l'écran Login et Hub : le fond est pastel avec skyline.
- [ ] **Step 2 :** lancer EditMode ; commit.

### Task 5 : Combat

**Files :** Modify `Editor/Prefabs/Recipes/CombatRecipes.cs`, `Presentation/Combat/TimelineView.cs`, `UnitCardView.cs`, `SkillIconView.cs`, `CombatView.cs` ; Test `Tests/PlayMode/CombatTimelinePlayTests.cs` (adapter), capture.

**Interfaces :** `TimelineView` : segments rendus en traits fins `Navy`, point d'action = bloc `Accent` (« GO ») avec `Sprites.Diamond` pour les marqueurs (ennemi `AccentSoft`, allié bleu `Info`) ; `UnitCardView` : carte `Sprites.Panel` teinte de l'agent adoucie, reflet `Sprites.Shine`, barre de PV fine, nom en blanc avec ombre légère ; carte active : cadre `Navy` + anneau `Accent` ; `SkillIconView` : anneaux pastel (actif `Navy`/`Accent`) ; légende « Au tour de … » en bloc `Navy` positionné pour ne pas recouvrir les cartes.

- [ ] **Step 1 :** adapter/écrire `CombatTimelinePlayTests.Timeline_UsesDiamondMarkers` (les marqueurs utilisent `Sprites.Diamond`) et `Combat_ActiveCardIsAccentFramed`.
- [ ] **Step 2 :** implémenter ; régénérer `Combat.prefab` ; capturer `22-combat-pastel`.
- [ ] **Step 3 :** lancer PlayMode combat et `TutorialFlowTests` : PASS. Commit.

### Task 6 : Hub et menus

**Files :** Modify `Editor/Prefabs/Recipes/CommonRecipes.cs`, `TeamRecipes.cs`, `LoginRecipe.cs`, `WipRecipes.cs`, `Presentation/Hub/**` (couleurs en dur).

**Interfaces :** barre de navigation (onglet actif `Navy`, autres `Frost`), en-têtes (niveau, portefeuille) en puces à coin coupé (`Accent` pour la monnaie), cartes de menu en verre avec `AccentBar`.

- [ ] **Step 1 :** implémenter, régénérer les prefabs concernés, capturer `23-hub-pastel`.
- [ ] **Step 2 :** lancer EditMode + PlayMode hub ; commit.

### Task 7 : Plateau, popups, reste et balayage des couleurs en dur

**Files :** Modify `Editor/Prefabs/Recipes/BoardRecipes.cs`, `TutorialRecipes.cs`, `Presentation/Board/BoardMapView.cs`, `CasePopupPresenter.cs`, `Presentation/Activities/*`, `Narration/*`, `Feedback/Feedback.cs`, `Hub/Pages/CityMap`, `MemberSheet`, `Tutorial/TutorialOverlayView.cs` ; Create `Tests/Editor/NoHardcodedColorsTests.cs`.

**Interfaces :** toute couleur littérale (`new Color(`, `Color32(`, `Hex(0x…)`) hors `Theme.cs` devient un jeton ; exceptions autorisées listées dans le test (`Color.white`, `Color.clear`, alpha seul).

- [ ] **Step 1 : test** `Presentation_HasNoLiteralColorsOutsideTheme` (parcourt `Presentation/**/*.cs` hors `Theme.cs`, échoue sur chaque littéral hors liste d'exceptions). FAIL (≈ 76 occurrences).
- [ ] **Step 2 :** remplacer chaque littéral par un jeton (ajouter les jetons nécessaires à `Theme`) ; régénérer tous les prefabs.
- [ ] **Step 3 :** lancer toute la suite EditMode et PlayMode : PASS. Commit.

### Task 8 : Polices (Barlow Condensed)

**Files :** Modify `Editor/Assets/FontAssetGenerator.cs`, `Presentation/Common/Ui.cs` (`TitleFont/BodyFont/RegularFont`) ; Create `Resources/Fonts/BarlowCondensed-*.ttf` + `OFL-BarlowCondensed.txt`.

**Interfaces :** `Ui.TitleFont` = Barlow Condensed ExtraBold Italic, `BodyFont` = Barlow Condensed SemiBold, `RegularFont` = Barlow Condensed Regular ; `FontAssetGenerator.Fonts` mis à jour ; matériau « Inked » remplacé par ombre légère (sans contour noir).

- [ ] **Step 1 :** **demander l'accord de l'utilisateur** pour télécharger les fichiers (nom, source Google Fonts / dépôt officiel OFL, taille) ; sans accord : conserver ComicNeue/Bangers et le signaler.
- [ ] **Step 2 :** ajouter les .ttf et la licence, tester l'affichage des accents français (« É », « à », « ç ») dans le `FontAssetGenerator` (test EditMode `Fonts_CoverFrenchGlyphs`), régénérer les polices TMP.
- [ ] **Step 3 :** régénérer les prefabs, recapturer tous les écrans, vérifier la troncature des libellés (`autosize` déjà actif sur les boutons) ; commit.

### Task 9 : Validation et fusion

- [ ] **Step 1 :** suite complète EditMode puis PlayMode ; analyser les XML (`grep`), consigner le total.
- [ ] **Step 2 :** captures avant/après des écrans principaux (login, hub, équipe, boutique, plateau, combat, popups) dans `Code/docs/test-reports/captures`, envoyées à l'utilisateur.
- [ ] **Step 3 :** `git merge --no-ff feature/direction-artistique` dans `main` (jamais de push) ; produire une APK `HeroesDawn-0.7.0-direction-artistique.apk` dans `G:/Mon Drive/01_PROJETS/Heroes_Dawn/_builds/` si l'utilisateur le demande.
