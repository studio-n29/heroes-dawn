# Alignement architecture — Sous-projet 3 : Écrans MVP en prefabs générés

> Statut : design validé en conversation le 2026-10-03 · à relire avant le plan d'implémentation.
> Précédent : sous-projet 2 (scènes `Boot`/`Hub`/`Board`/`Combat`, `GameFlowMachine`, rapports de test), fusionné dans `integration/tout-dans-unity`.
> Références : `04_Tech_Unity/07_Presentation_Flux.md` (MVP, modales, UI), `03_Art_UI_UX/03_Maquettes_Screen_Flow.md` (§3 règles de conception), `04_Tech_Unity/09_Tests_Outils_CI.md`.

## 1. Intention
- **Demande :** aligner les écrans sur l'architecture cible (doc 07). Les vues deviennent des prefabs **générés une fois par un outil éditeur, puis éditables dans Unity** ; la logique d'écran passe dans des présentateurs C# purs ; ajout de la navigation « Retour ». Tout reste dans Unity.
- **Choix validés :**
  - **conversion fidèle** : mêmes écrans, même contenu, même jouabilité qu'aujourd'hui ; la mise en page des maquettes viendra ensuite, en retouchant les prefabs ;
  - **TextMeshPro** pour tout le texte ;
  - **approche A** : générer une fois, puis le prefab fait foi.
- **Réussite :**
  1. Les 4 écrans (Connexion, QG, Plateau, Combat), les popups et les modales sont des prefabs ; aucun écran n'est plus construit par code à l'exécution.
  2. Jouabilité identique, plus la navigation Retour (§7).
  3. Tout le texte est en TMP (Bangers / Comic Neue).
  4. Suites EditMode et PlayMode vertes, avec rapport HTML et synthèse en chat.
  5. Docs 07, 09, `Decisions.md` et `Code/README.md` à jour.
  6. Une retouche manuelle d'un prefab (couleur, position) survit à une nouvelle exécution du générateur sans confirmation d'écrasement.

## 2. Périmètre
**Dans le périmètre**
1. Vues `MonoBehaviour` + interfaces `I…View`, présentateurs C# purs, données de vue `readonly struct …Data`.
2. Générateur de prefabs (menu + batchmode) et recettes par prefab.
3. Migration TextMeshPro : Font Assets, matériau « encré », helpers `Ui.*`.
4. `ScreenCatalog` et instanciation des écrans par les scopes de scène.
5. `IModalService` (une modale à la fois, file d'attente), popups de case en prefabs.
6. `BackButtonRouter` (Échap / retour Android) et `IBackHandler`.
7. Tests (présentateurs, prefabs, générateur, PlayMode) et docs.

**Hors périmètre**
- Mise en page des maquettes, QG en vue en coupe, pile d'écrans du QG (`ScreenStack`).
- Écrans M2 (carte, préparation, prologue, paramètres…), Addressables, PrimeTween.
- Accessibilité (taille de texte, mode daltonien), piste playtest.
- Tout changement de règle de jeu, de scène ou de `GameFlowMachine`.

*Rejetées :*
- **prefab de base régénérable + variante éditable** : régénérer la base change les identifiants internes des objets et fait perdre les retouches de la variante, sauf à écrire un générateur de mise à jour en place, beaucoup plus complexe ;
- **prefabs écrits à la main** : impossible à produire et vérifier en batchmode, perte de la parité avec l'existant.

## 3. Découpage
| Écran | Vue (prefab) | Sous-vues (prefabs imbriqués) | Présentateur | Scope |
|---|---|---|---|---|
| Connexion | `LoginView` | — | `LoginPresenter` | Root (Boot) |
| QG | `HubView` (onglets Équipe/Recrutement, escouade, opérations, infirmerie) | `TeamCardView`, `RecruitCardView`, `SquadSlotView` | `HubPresenter` | Hub |
| Plateau | `BoardView` (HUD, objectifs, dé) + `BoardMapView` (cases, liens, pion) | `BoardNodeView` | `BoardPresenter` | Board |
| Popups de case | `RestPopupView`, `NarrativePopupView`, `MiniGamePopupView`, `ZoneCompletePopupView` | `ChoiceButtonView` | `CasePopupPresenter` | Board |
| Combat + résultats | `CombatView`, `CombatResultsView` | `UnitCardView`, `SkillButtonView` | `CombatPresenter` | Combat |
| Commun | `ModalLayer`, `Toasts`, `WalletView`, `ConfirmPopupView` | — | — | Root |

**Règles**
- Une vue n'expose que des méthodes `Show…(…Data)` / `Set…(…)` et des événements d'intention (`RollClicked`, `NodeTapped(CaseId)`, `SkillClicked(int)`…). Elle ne lit pas la session, n'appelle aucun use case, ne connaît pas `UiContext`. Elle garde animations, tweens, chiffres flottants.
- Un présentateur ne touche jamais un `GameObject`, `Transform` ou composant.
- Une vue = un présentateur ; les sous-vues sont pilotées par le présentateur parent via la vue parente.
- Navigation : le présentateur publie `NavigateRequested` / `CombatResultsClosed` comme les écrans aujourd'hui. `GameFlowMachine`, scènes, use cases et bus sont inchangés.
- À la fin du sous-projet, `UiScreen`, `UiContext`, `UiServices`, `CasePopups` et les `Create(...)` statiques des écrans sont supprimés. Les helpers `Ui.*` restent mais ne sont plus appelés qu'en éditeur (recettes).
- **Ordre de conversion** : Connexion (pilote) → QG → Plateau + popups → Combat. Chaque étape est jouable et verte avant la suivante.

## 4. Générateur de prefabs
- Menu *Heroes Dawn ▸ Setup ▸ Générer les prefabs d'écran* ; batchmode : `PrefabGenerator.GenerateAll()`.
- Sortie : `Assets/_Project/Prefabs/Screens/`, `Prefabs/Widgets/`, `Prefabs/Popups/`.
- Une **recette** par prefab (`Editor/Prefabs/Recipes/<Nom>Recipe.cs`) : reprend la construction actuelle de l'écran (helpers `Ui.*` passés en TMP) et branche les champs `[SerializeField]` de la vue. Aucun texte localisé en dur : un composant `LocalizedText` porte la clé, résolue à l'exécution.
- Ordre : sous-vues d'abord (`Widgets/`), puis écrans et popups qui les instancient en **prefabs imbriqués** — une retouche de `TeamCardView` se propage partout.
- **Jamais d'écrasement silencieux** : depuis le menu, une fenêtre liste les prefabs existants (décochés par défaut) ; en batchmode, il faut `-overwritePrefabs`. Un prefab absent est toujours créé.
- Déterministe : deux générations sur un dossier vide produisent la même structure (noms, hiérarchie, références).
- **`ScreenCatalog`** (ScriptableObject, `Settings/ScreenCatalog.asset`) : référence le prefab de chaque écran et popup. Le générateur le remplit ; le générateur de scènes le branche sur `RootLifetimeScope` et les `SceneScreenScope`. Les scènes ne contiennent pas les écrans : le scope instancie le prefab sous son canvas `SafeArea` et l'enregistre (`RegisterInstance<I…View>`). Retoucher un prefab ne touche donc jamais une scène.

## 5. TextMeshPro
- TMP inclus dans uGUI 2.6 (aucun paquet ajouté). Les ressources essentielles TMP sont importées par l'outil éditeur, pas à la main.
- Menu *Heroes Dawn ▸ Setup ▸ Générer les polices* : Font Assets dynamiques `Bangers` (titres, boutons, chiffres) et `ComicNeue` (texte), depuis `Resources/Fonts/*.ttf`, plus un matériau « encré » (contour noir) qui remplace `Inked()` et `Outline`. Sortie : `Assets/_Project/Art/Fonts/`.
- `Ui.Txt` renvoie un `TextMeshProUGUI` ; `FontKind` choisit le Font Asset et son matériau.
- Règle des maquettes (§3) : aucun texte sous **20 px** en 1080p — vérifiée par test sur les prefabs.

## 6. Présentateurs et cycle de vie
- Enregistrés dans le scope de leur scène : `Register<HubPresenter>(Lifetime.Scoped).AsImplementedInterfaces()` (`IStartable`, `IDisposable`, `IBackHandler` le cas échéant).
- Dépendances : use cases, `ISessionReadModel`, `IActiveCombatReadModel`, publishers/subscribers MessagePipe, `ILocalization` (port fin autour de `Loc`, pour tester des clés), `IModalService`, `IToastService`.
- Abonnements pris dans `Start`, libérés dans `Dispose` (le scope les dispose avec sa scène). Le plateau masqué pendant le combat garde ses abonnements, comme aujourd'hui.
- Async : chaque présentateur possède un `CancellationTokenSource` annulé dans `Dispose`.
- **`BusyGate`** : garde anti-doublon commune (remplace `Act()`/`_deploying` du QG, `RunBoard` du plateau, `Submit` du combat). Le déploiement réussi garde la garde levée jusqu'au prochain `Shown`, comme aujourd'hui.
- Visibilité : la machine d'états continue d'activer/désactiver le canvas de scène ; la vue relaie `OnEnable`/`OnDisable` en événements `Shown`/`Hidden` ; le présentateur rafraîchit sur `Shown`. Pas d'`IGameState` : la doc 07 est corrigée en ce sens.

## 7. Modales et navigation Retour
- **`IModalService`** (Root, implémenté par `ModalLayer`) : `UniTask<T> Open<T>(popup prefab, données)`. Une modale à la fois ; les suivantes attendent (file FIFO). Une modale déclare si elle est annulable. Le plateau est déjà en pause derrière une modale (comportement actuel inchangé).
- **`BackButtonRouter`** (Root) : lit l'action « Retour » de l'Input System (Échap, retour Android ; remplace la lecture d'Échap dans `Keys`). Ordre : modale du dessus, puis l'`IBackHandler` de l'écran visible (enregistré sur `Shown`, retiré sur `Hidden`).

| Contexte | Retour |
|---|---|
| Modale ouverte | Annulable : la ferme (= « Annuler ») ; sinon rien |
| Connexion | Rien |
| QG | Rien (« Quitter le jeu ? » viendra avec les paramètres) |
| Plateau | Confirmation « Retour au QG ? » (comme le bouton actuel) |
| Combat, ciblage en cours | Annule le ciblage |
| Combat en cours | Rien (la retraite reste volontaire, W11a) |
| Combat, résultats | « Continuer » |

Le bouton ◄ visible n'existe que là où il y a un retour : sur le plateau, c'est le bouton « Retour QG » actuel, inchangé.

## 8. Gestion des erreurs
- Use case en échec : le présentateur affiche le toast d'erreur localisé (comportement actuel), la garde `BusyGate` retombe.
- Prefab ou référence manquants au démarrage d'une scène : erreur explicite (`"Prefab d'écran manquant dans ScreenCatalog : Hub"`), attrapée par le test du catalogue avant d'atteindre le jeu.
- Générateur : une recette en échec n'interrompt pas les autres ; le résumé liste les échecs et le menu se termine en erreur (code ≠ 0 en batchmode).

## 9. Tests
### 9.1 EditMode — présentateurs (sans scène)
Fausse vue par écran (`FakeLoginView`, `FakeHubView`…) qui enregistre les `Show…` reçus et déclenche les intentions ; vrais use cases sur `TestApp`.
- Connexion : un seul appel malgré deux clics sur Jouer ; état occupé ; erreur affichée et garde relâchée.
- QG : recrutement visible dans l'effectif ; entraînement ; soin ; déploiement publie `NavigateRequested(Board)` une seule fois malgré deux clics.
- Plateau : lancer, dé bonus, choix de case, fin de déplacement, ouverture du bon popup par type de case, retour au QG confirmé, rafraîchissement sur `ReturnedToBoard`.
- Combat : sélection automatique de l'allié prêt, ciblage puis annulation, combo, auto, vitesse, retraite, résultats puis `CombatResultsClosed`.
- `BusyGate`, `BackButtonRouter` (ordre modale puis écran), `ModalService` (file, annulation).

### 9.2 EditMode — prefabs et générateur (`Tests.Editor`)
- Structure de tous les prefabs de `Prefabs/` : chaque `[SerializeField]` de vue branché ; aucun `UnityEngine.UI.Text` ; aucun TMP sous 20 px ; chaque `LocalizedText` a une clé connue de `Loc`.
- Générateur : produit tous les prefabs listés ; n'écrase rien sans demande ; deux générations donnent la même structure.
- `ScreenCatalog` : chaque scène d'écran référence un prefab existant.

### 9.3 PlayMode
- Les 14 tests actuels sont conservés et pilotent le jeu par les intentions des vues (les méthodes de test des écrans — `Deploy()`, `TapNode()`, `ContinueForTests()`… — disparaissent).
- Nouveaux : Échap sur le plateau ouvre la confirmation, un second Échap la ferme ; partie complète via les prefabs (connexion → recrutement → déploiement → case combat → résultats → plateau → QG).

### 9.4 Rapports
Chaque test porte `[Description("Vérifie que … — parce que …")]` ; `latest-*.html` régénérés et commités ; synthèse en chat après chaque série.

## 10. Ordre des étapes
1. TMP : génération des polices, `Ui.*` en TMP, `LocalizedText` ; écrans actuels encore construits par code mais en TMP (tests verts).
2. Socle : `ScreenCatalog`, générateur de prefabs (squelette + tests), `BusyGate`, `ILocalization`, `IModalService`, `BackButtonRouter`.
3. Connexion (pilote) : `LoginView` + recette + `LoginPresenter` + tests.
4. QG : sous-vues, `HubView`, `HubPresenter`, tests.
5. Plateau : `BoardNodeView`, `BoardMapView`, `BoardView`, popups, `BoardPresenter`, `CasePopupPresenter`, tests.
6. Combat : `UnitCardView`, `SkillButtonView`, `CombatView`, `CombatResultsView`, `CombatPresenter`, tests.
7. Nettoyage (`UiScreen`, `UiContext`, `UiServices`, `CasePopups`), tests PlayMode nouveaux, docs.

## 11. Risques
- **Taille** (~3 800 lignes de présentation à redécouper) : ordre écran par écran, chaque étape jouable ; le plan peut découper le combat en deux tâches.
- **Rendu TMP différent** (métriques de police, retours à la ligne) : vérifié par captures en PlayMode à l'étape 1, avant toute conversion.
- **Références de prefab cassées par une retouche** : test de structure (§9.2).
- **Prefabs binaires en LFS** : les `.prefab` sont du YAML texte ; ils ne passent pas par LFS (vérifier `.gitattributes`).
