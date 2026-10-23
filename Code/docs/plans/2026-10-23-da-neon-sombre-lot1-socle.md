# Thème sombre néon, lot 1 « socle » : plan d'implémentation

> **Pour les agents :** SOUS-COMPÉTENCE REQUISE : utiliser superpowers:subagent-driven-development (recommandé) ou superpowers:executing-plans pour exécuter ce plan tâche par tâche. Les étapes utilisent des cases à cocher (`- [ ]`).

**Objectif :** remplacer le socle visuel pastel (jetons `Theme`, composants `Ui.Panel`/`Btn`/`Bar`/`Inked`, shaders, fond par défaut) par le thème sombre néon verre, sans casser aucun appel existant.

**Architecture :** les noms de jetons et les signatures de `Ui` sont conservés ; seules les valeurs et le rendu changent. Deux shaders UI nouveaux (`NeonEdge`, `GlassSheen`) sont ajoutés aux matériaux de `UiMaterialBuilder`. Les jetons pastel devenus sans objet restent en alias `[Obsolete]` jusqu'au lot 4 (balayage).

**Stack :** Unity 6000.6.3f1 (URP), uGUI, TextMeshPro, NUnit EditMode (`Assets/Tests/Editor`).

**Spec :** `docs/specs/2026-10-23-da-neon-sombre-design.md`. Ce plan couvre **uniquement le lot 1**. Les lots 2 (combat), 3 (hub et menus) et 4 (reste, balayage des couleurs en dur) auront chacun leur plan après validation des captures du lot précédent.

## Contraintes globales
- Aucun flou temps réel (mobile) : faux verre uniquement.
- Valeurs de la spec, à copier telles quelles : fond nuit `#0B1020` → `#1A1033` ; `Glass` `#141B38` (alpha 0,62 à 0,78) ; `Neon` `#25E6FF` ; `Accent` `#FF6B5E` (`AccentSoft` `#FF8F84`, `AccentDark` `#F0566A`) ; `Text` `#EAF1FF` ; `TextMuted` `#9AA8D4`.
- Contraste : 4,5:1 (texte courant), 3:1 (grand texte), mesuré avec `ThemeContrast.Ratio`.
- Chaque test porte `[Description("Vérifie que … — parce que …")]` (français).
- Tests : `Unity.exe -batchmode -nographics -projectPath … -runTests -testPlatform EditMode -testResults "$TEMP/editmode.xml" -logFile "$TEMP/unity.log"` (voir `SETUP.md` §6) ; filtre : `-testFilter "N29.HeroesDawn.Tests.Editor.<Classe>.<Test>"`. Résumé : `grep -o '<test-run [^>]*' "$TEMP/editmode.xml"`.
- Chemins relatifs à `Code/Heroes Dawn Unity/Assets/` ; « P/ » = `_Project/`.
- Commits en français, un par tâche, terminés par `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`. Ne jamais ajouter les fichiers modifiés par Unity sans rapport (voir `git status`).

## Points de vigilance (Review Focus)
- Texte `TextMuted` sur `Surface3` et sur `Background` : doit rester ≥ 4,5:1 (testé tâche 1).
- Bouton désactivé : libellé lisible (> 1,5:1) mais visiblement estompé sur verre (testé tâche 4).
- `Ui.Panel` appelé avec `shadow = 0` : aucune ombre ni halo (testé tâche 3).
- Panneau très petit (barre, jauge) : les couches de reflet ne débordent pas du cadre (testé tâche 3).
- Les 98 usages des jetons pastel compilent encore (alias `[Obsolete]`) ; `NoCodeBuiltScreensTests` et `UiAssetsTests` restent verts (tâche 6).

## Structure des fichiers
- Modifier `P/Presentation/Common/Theme.cs` : jetons sombres + nouveaux jetons + alias obsolètes.
- Modifier `P/Presentation/Common/Ui.cs` : `Panel`, `Btn`, `Bar`, `Inked`, nouveau `NightBackdrop`.
- Modifier `P/Presentation/Common/UiButton.cs` : états (survol, désactivé) adaptés au verre.
- Créer `P/Resources/UI/Shaders/HD_UI_NeonEdge.shader`, `HD_UI_GlassSheen.shader` ; modifier `HD_UI_Backdrop.shader`.
- Modifier `P/Editor/Assets/UiMaterialBuilder.cs` (2 matériaux de plus) et `P/Editor/Assets/FontAssetGenerator.cs` (ombre du matériau « Inked »).
- Modifier `P/Editor/Prefabs/Recipes/ScreenKit.cs` : fond nuit par défaut.
- Tests : modifier `Tests/Editor/ThemeTests.cs`, `UiStyleTests.cs`, `UiAssetsTests.cs`.

---

### Tâche 1 : Jetons `Theme` sombres

**Fichiers :**
- Modifier : `P/Presentation/Common/Theme.cs`
- Test : `Tests/Editor/ThemeTests.cs`

**Interfaces :**
- Produit (nouveaux, `public static readonly Color`) : `Night` (`#0B1020`), `NightBottom` (`#1A1033`), `Glass` (`#141B38`), `GlassHi` (blanc alpha 0,18), `Neon` (`#25E6FF`), `NeonEdge` (`Neon` alpha 0,55), `NeonGlow` (`Neon` alpha 0,25), `Shadow` (`#000000` alpha 1, à moduler par l'appelant).
- Réaffectés (mêmes noms, valeurs sombres) : `Background = Night`, `Surface = Glass` opaque, `Surface2 = #1B2447`, `Surface3 = #243059`, `SurfaceDark = Night`, `Text = #EAF1FF`, `TextDark = Night` (texte sur fond clair/corail), `TextMuted = #9AA8D4`, `TextOnDark = #EAF1FF`, `Overlay = Night` alpha 0,7, `FrostEdge = NeonEdge`, `Ink = Night`.
- Alias `[Obsolete("Lot 4 : remplacer par Night/Glass/Neon")]` mappés sur les nouvelles valeurs : `Navy` (= `Night`), `Frost` (= `Glass`), `SkyTop` (= `Night`), `SkyMid` (= `Night`), `SkyBottom` (= `NightBottom`), `Glow` (= `NeonGlow`).
- Resaturés pour fond sombre (supprimer l'éclaircissement `Soft` des types de case ; teintes de `Matter`/`Elemental`/`PhysicalBio`/`Perception`/`Hp`/`Pr`/`Fatigue`/`Shield` rendues plus vives) tout en gardant les distances de `Theme_FamiliesStayDistinct` (≥ 0,2) et `Theme_RarityStaysDistinct` (≥ 0,15).

- [ ] **Étape 1 : écrire le test de contraste qui échoue.** Dans `ThemeTests`, remplacer `Theme_TextOnSurfaces_MeetsContrast` par : `Text` sur `Surface`, `Surface2`, `Surface3`, `Background` ≥ 4,5 ; `TextMuted` sur `Surface`, `Surface2`, `Surface3`, `Background` ≥ 4,5 ; `Neon` sur `Surface` ≥ 3 (liseré lisible) ; `TextOnDark` sur `AccentDark` ≥ 3 (grand texte sur corail) ; `TextDark` sur `Accent` ≥ 4,5 ; `TextOnDark` sur `Accent` : documenté en commentaire s'il n'atteint pas 3 (alors le texte des boutons corail utilise `TextDark`).
- [ ] **Étape 2 : lancer `ThemeTests`, constater l'échec** (les valeurs sont encore pastel).
- [ ] **Étape 3 : réécrire `Theme.cs`** avec les valeurs ci-dessus. Les familles et raretés restent distinctes (les deux tests de distance existants ne changent pas).
- [ ] **Étape 4 : lancer `ThemeTests`, constater le succès.** Si une paire est sous le seuil, ajuster la teinte du jeton fautif (jamais le seuil).
- [ ] **Étape 5 : commit** `feat(da): jetons du thème sombre néon`.

### Tâche 2 : Shaders `NeonEdge`, `GlassSheen`, `Backdrop` sombre et matériaux

**Fichiers :**
- Créer : `P/Resources/UI/Shaders/HD_UI_NeonEdge.shader`, `P/Resources/UI/Shaders/HD_UI_GlassSheen.shader`
- Modifier : `P/Resources/UI/Shaders/HD_UI_Backdrop.shader`, `P/Editor/Assets/UiMaterialBuilder.cs`
- Test : `Tests/Editor/UiAssetsTests.cs`

**Interfaces :**
- Produit : matériaux `Resources/UI/Materials/UiNeonEdge.mat` (shader `HD/UI/NeonEdge`) et `UiGlassSheen.mat` (shader `HD/UI/GlassSheen`), chargeables par `RecipeKit.Mat("UiNeonEdge")` / `RecipeKit.Mat("UiGlassSheen")` ; à l'exécution `Resources.Load<Material>("UI/Materials/UiNeonEdge")`.
- `NeonEdge` : même squelette que `HD_UI_Shine.shader` (propriétés de stencil, `UnityUI.cginc`, clip rect), mélange additif (`Blend SrcAlpha One`), propriété `_Pulse` (Range 0..1, défaut 0,15) qui module l'alpha avec `_Time`. `GlassSheen` : bande diagonale statique en additif, propriétés `_Angle`, `_Width` (défaut 0,35), `_Strength` (défaut 0,16), coordonnées locales du sprite (pas écran).
- `Backdrop` : retirer la trame de points (propriétés `_Dots`/`_DotStrength` supprimées), ne garder que le vignettage, couleur par défaut `_Color` = `#0B1020`.

- [ ] **Étape 1 : écrire dans `UiAssetsTests` un test** `Materials_IncludeNeonAndGlass` : après `UiMaterialBuilder.EnsureAll()`, `Resources.Load<Material>("UI/Materials/UiNeonEdge")` et `…/UiGlassSheen` sont non nuls et leurs shaders sont `HD/UI/NeonEdge` / `HD/UI/GlassSheen`.
- [ ] **Étape 2 : lancer le test, constater l'échec.**
- [ ] **Étape 3 : écrire les deux shaders et adapter `Backdrop`** (voir Interfaces) ; ajouter `("UiNeonEdge", "HD/UI/NeonEdge")` et `("UiGlassSheen", "HD/UI/GlassSheen")` au tableau `Materials` de `UiMaterialBuilder`.
- [ ] **Étape 4 : exécuter `Heroes Dawn ▸ Setup ▸ Générer les matériaux d'interface`** (ou `-executeMethod N29.HeroesDawn.EditorTools.Assets.UiMaterialBuilder.EnsureAll -quit`), puis relancer le test : succès. Vérifier dans la console l'absence d'erreur de compilation de shader.
- [ ] **Étape 5 : commit** `feat(da): shaders NeonEdge et GlassSheen, fond sombre sans trame`.

### Tâche 3 : `Ui.Panel` en verre sombre

**Fichiers :**
- Modifier : `P/Presentation/Common/Ui.cs` (méthode `Panel`, enum `PanelVariant`)
- Test : `Tests/Editor/UiStyleTests.cs`

**Interfaces :**
- Produit : `public enum PanelVariant { Glass, Active, Accent }` ; `Ui.Panel(Transform parent, string name, Color fill, float border = 4f, float shadow = 8f, bool raycast = true, PanelVariant variant = PanelVariant.Glass) : Image` (contrat conservé : renvoie l'enfant `Fill`, `Fill.Frame()` est le cadre).
- Rendu : le cadre porte le liseré (`Theme.NeonEdge`, sprite `Sprites.Panel`) ; trois composants `Shadow` (contact `(0,-2)` alpha 0,5 ; moyenne `(0,-6)` alpha 0,35 ; diffuse `(0,-16)` alpha 0,25, tous `Theme.Shadow`) seulement si `shadow > 0` ; l'enfant `Fill` en `Theme.Glass` teinté par `fill` ; un enfant `Sheen` (sprite `Sprites.VerticalGradient`, couleur `Theme.GlassHi`, ancré sur la moitié haute, `raycastTarget = false`, matériau `UiGlassSheen`) ; variante `Active` : liseré `Theme.Neon` plein plus un enfant `Halo` du cadre (sprite `Sprites.PanelFrame`, couleur `Theme.NeonGlow`, matériau `UiNeonEdge`, débordant de 4 px) ; variante `Accent` : liseré `Theme.Accent`.

- [ ] **Étape 1 : réécrire dans `UiStyleTests`** `Panel_KeepsContract_AndHasNoBlackOutline` : mêmes assertions de contrat (nom `Fill`, marge intérieure > 0, cadre ≠ noir, sprite `Sprites.Panel`), plus : `frame.color == Theme.FrostEdge` ; exactement 3 `Shadow` avec alpha croissant de contact vers diffuse décroissant (≤ 0,5) ; un enfant `Fill/Sheen` ; avec `shadow: 0` aucun `Shadow` ; avec `PanelVariant.Active` un enfant `Halo` et `frame.color == Theme.Neon` ; le `Sheen` a `rectTransform.anchorMax.y == 1` et `anchorMin.y >= 0.5` (reste dans le cadre).
- [ ] **Étape 2 : lancer `UiStyleTests`, constater l'échec.**
- [ ] **Étape 3 : implémenter** `Panel` et `PanelVariant` comme décrit. `AccentBar` garde son contrat (`UiStyleTests.AccentBar_IsLeftEdge` inchangé).
- [ ] **Étape 4 : lancer `UiStyleTests`, constater le succès** (le test du bouton désactivé sera mis à jour à la tâche 4 ; s'il échoue ici, c'est attendu et corrigé là).
- [ ] **Étape 5 : commit** `feat(da): Ui.Panel en verre sombre avec reflet, liseré néon et ombres en couches`.

### Tâche 4 : `Ui.Btn`, `UiButton`, `Ui.Bar`, `Ui.Inked`

**Fichiers :**
- Modifier : `P/Presentation/Common/Ui.cs` (`Btn`, `Bar`, `Inked`), `P/Presentation/Common/UiButton.cs` (`ApplyState`, `OnPointerEnter`), `P/Editor/Assets/FontAssetGenerator.cs`
- Test : `Tests/Editor/UiStyleTests.cs`

**Interfaces :**
- `Ui.Btn(...)` : signature inchangée. Principal (`bg == Theme.Accent`) : `PanelVariant.Accent`, reflet `Shine` conservé, enfant `Halo` corail (`Theme.WithAlpha(Theme.Accent, 0.3f)`, matériau `UiNeonEdge`). Autre `bg` : `PanelVariant.Active` si `bg == Theme.Neon`, sinon `PanelVariant.Glass` avec `fill = bg`. Le halo est détruit/désactivé quand le bouton est désactivé.
- `UiButton.ApplyState` : désactivé → fond `Color.Lerp(_bg, Theme.Surface3, 0.8f)` conservé, libellé `Theme.WithAlpha(Theme.Text, 0.5f)` (remplace `Navy`), halo masqué ; survol → `Color.Lerp(_bg, Color.white, 0.15f)` conservé.
- `Ui.Bar` : fond `back` sur `Theme.Night` alpha 0,6, liseré `Theme.NeonEdge` à 1 px, remplissage `fill` avec enfant `Glow` (même sprite, couleur `fill` alpha 0,35, débordant de 2 px) ; `UiBar.Set(0f, …)` désactive toujours le remplissage **et** le `Glow`.
- `Ui.Inked` sur un `Graphic` non TMP : `Shadow` couleur `Theme.WithAlpha(Theme.Shadow, 0.6f)`. `FontAssetGenerator` : couleur d'underlay du matériau « Inked » = `Theme.WithAlpha(Theme.Shadow, 0.6f)` (mêmes offsets), toujours sans contour.

- [ ] **Étape 1 : mettre à jour les tests.** `Button_DisabledLooksDisabled` : même contrat (fond ≠ `Accent` désactivé, contraste libellé/fond > 1,5, alpha du libellé < 1, retour à l'état actif identique) avec `Theme.TextDark` comme `fg` si le contraste de la tâche 1 l'exige ; nouveau `Button_PrimaryHasHalo_DisabledHasNone` (enfant `Halo` actif quand interactif, inactif sinon) ; `Bar_ZeroAndFull` complété : à 0 le `Glow` est désactivé aussi, à 1 il est actif.
- [ ] **Étape 2 : lancer `UiStyleTests`, constater les échecs.**
- [ ] **Étape 3 : implémenter** selon Interfaces.
- [ ] **Étape 4 : régénérer les polices** (`Heroes Dawn ▸ Setup ▸ Générer les assets d'interface`) pour mettre à jour les matériaux « Inked » ; lancer `UiStyleTests` et `UiAssetsTests` : succès (le test existant exige `OutlineWidth == 0` et underlay activé : conservé).
- [ ] **Étape 5 : commit** `feat(da): boutons, barres et lettrage en verre néon`.

### Tâche 5 : Fond nuit par défaut

**Fichiers :**
- Modifier : `P/Presentation/Common/Ui.cs` (`NightBackdrop`), `P/Editor/Prefabs/Recipes/ScreenKit.cs` (méthodes `Backdrop`, `ArtBackdrop`)
- Test : `Tests/Editor/UiStyleTests.cs`

**Interfaces :**
- Produit : `public static Image Ui.NightBackdrop(Transform parent, string name = "Backdrop")` : image plein cadre (`Stretch`) en dégradé vertical `Theme.Night` → `Theme.NightBottom` (sprite `Sprites.VerticalGradient`, couleur du haut `Night`, fond `NightBottom` via un second `Image` sous le dégradé), `raycastTarget = false`, avec un enfant `Vignette` utilisant le matériau `UiBackdrop`.
- `ScreenKit.Backdrop(rt, top, bottom)` : ignore les couleurs pastel reçues, appelle `Ui.NightBackdrop`, supprime la skyline et la lueur pêche ; `ScreenKit.ArtBackdrop(rt, art, alpha, veil)` : n'affiche plus l'illustration (qui est pastel) mais le fond nuit ; l'illustration reste importée et inchangée.

- [ ] **Étape 1 : écrire** `NightBackdrop_IsFullStretchAndIgnoresInput` : l'image est étirée (`anchorMin == (0,0)`, `anchorMax == (1,1)`), `raycastTarget == false`, a un enfant `Vignette`.
- [ ] **Étape 2 : lancer, constater l'échec ; implémenter ; relancer, succès.**
- [ ] **Étape 3 : régénérer les prefabs d'écrans qui portent le fond** (`PrefabGenerator.GenerateAllBatch -overwrite` limité aux prefabs de `Screens/` listés par `git status` après génération ; ne pas utiliser `-overwritePrefabs`, cf. `SETUP.md`).
- [ ] **Étape 4 : commit** `feat(da): fond nuit par défaut à la place des ciels pastel`.

### Tâche 6 : Vérification de non-régression et captures du lot

**Fichiers :** aucun nouveau (rapports et captures sous `Code/docs/test-reports/`).

- [ ] **Étape 1 : lancer toute la suite EditMode** (commande en Contraintes globales). Attendu : 0 échec. Corriger les valeurs de couleur ou de taille encore codées dans les tests (jamais l'inverse), avec la mise à jour du `Description`.
- [ ] **Étape 2 : lancer la suite PlayMode** (sans `-nographics`, tutoriel inclus). Attendu : 0 échec.
- [ ] **Étape 3 : prendre les captures** : `-testPlatform PlayMode -testFilter N29.HeroesDawn.Tests.PlayMode.ScreenCaptureTests` (une capture par page du hub). Les envoyer à l'utilisateur avec `SendUserFile`, en rappelant que les illustrations pastel restantes sont hors périmètre.
- [ ] **Étape 4 : mesurer sur Android** (si un appareil est disponible) la fluidité du hub avec panneaux à 3 ombres ; si < 30 fps, supprimer la couche d'ombre « diffuse » dans `Ui.Panel` et mettre à jour le test de la tâche 3 (2 ombres).
- [ ] **Étape 5 : commit** `test(da): lot 1 du thème sombre néon vérifié` (rapports `latest-*.html` inclus s'ils changent).
- [ ] **Étape 6 : attendre la validation des captures par l'utilisateur** avant d'écrire le plan du lot 2.
