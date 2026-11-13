# Plan de stabilisation : de la 1.0.0 en cours à une version stable (1.0.x)

> **Pour les agents :** exécuter phase par phase avec `superpowers:executing-plans` ou `superpowers:subagent-driven-development`. Les cases `- [ ]` suivent l'avancement. Chaque phase se termine par un critère de sortie vérifiable.

**Objectif :** livrer une version **stable** de Heroes Dawn : sans bug bloquant, sans fonction affichée « WIP », à la baseline de tests verte, installable sur Windows et Android, et jouable de bout en bout sur Brooklyn.

**Approche :** geler le périmètre, assainir le dépôt, trancher la dette d'interface (UI Toolkit contre uGUI), puis boucler fonctionnel, équilibrage, contenu et QA. Aucune nouvelle fonctionnalité de jeu avant la fin de la phase 4.

**Stack :** Unity 6.6 (6000.6.3f1), URP, VContainer, MessagePipe, UI Toolkit (en migration), Supabase, Unity IAP.

**Sources :** `docs/plans/2026-10-14-plan-early-access.md`, `docs/release-notes-0.15.0.md`, `05_Production/Roadmap.md`, `05_Production/Risques.md`, `docs/specs/2026-11-10-refonte-ui-uitoolkit.md`, point d'état du 2026-11-12.

## Hypothèses (à confirmer)

| # | Hypothèse | Si faux |
|---|---|---|
| H1 | **Validée le 2026-11-12, modifiée.** « Stable » = **1.0.0-rc puis 1.0.x** : Brooklyn complet, **Agent D et Agent M jouables (2 à 4 zones de Moscou à créer)**, Windows + Android. Paris et Salem sont **hors périmètre**. (Proposition initiale : Agent M hors périmètre.) | n/a ; ajoute le contenu de Moscou à la phase 5 (2 à 3 semaines). |
| H2 | **Validée le 2026-11-12, décalée.** L'Early Access du **14/11** est abandonné. **rc1 vers le 22/01/2027, stable le 29/01/2027.** (Proposition initiale : rc ~10/12, stable 14/12, devenue intenable avec Moscou.) | n/a ; source unique des dates : `05_Production/Roadmap.md`. |
| H3 | **Validée le 2026-11-12.** Il n'y a pas d'iOS, de Switch ni d'achats réels côté stores à cette version : l'IAP reste branché mais testé en bac à sable. | Phase légale et stores à ajouter. |

## Contraintes globales

- Tout se passe dans Unity (décision 2026-10-02), pas de projet .NET extérieur.
- « Aube tactique » n'apparaît **jamais** dans le jeu (test existant à conserver).
- R7 : ne pas représenter les thèmes sensibles d'Eda. R8 : dire « cryptides », jamais « SCP ».
- Toute décision validée → une ligne dans `05_Production/Decisions.md`.
- Pas de migration Supabase appliquée sans accord explicite (`002_hd_arena.sql` en attente).

## Points de vigilance

1. **Régression UI pendant la migration UI Toolkit** : un écran migré perd sa navigation manette ou sa disposition 20:9. Test de capture sur 4 formats exigé (16:9, 20:9, 4:3, 1280×720).
2. **Sauvegarde cloud** : un ancien profil (0.15.x) doit charger en 1.0.0 sans perte (champs `MetaSave` compatibles).
3. **Achat interrompu** : reprise au lancement sans double octroi (déjà testée, à retester sur appareil).
4. **Boss trop faciles** : taux de victoire à 100 % contre 40-60 % visé.
5. **Hors ligne** : le jeu doit démarrer et se jouer sans réseau (compte optionnel).

---

## Phase 0 — Assainir le dépôt (1-2 jours)

**Sortie :** `develop` propre, baseline de tests connue, build 1.0.0 reproductible.

- [x] Committer ou écarter l'arbre de travail : `ProjectSettings/*.asset`, `Assets/Settings/PC_RPAsset.asset`, `UniversalRenderPipelineGlobalSettings.asset`, `Editor/Build/WebGLBuilder.cs`, `Data/` (revoir ce qui est généré et à ignorer).
- [x] Décider du worktree `integration/` (branche `feature/combat-timeline`, 8 modifications non committées) : fusionner, committer ou abandonner, puis `git worktree remove`.
- [x] Lancer la baseline : EditMode puis PlayMode (`build/run-tests.sh`), noter les chiffres dans `docs/test-reports/`.
  - Tests non exécutés classés : `docs/test-reports/2026-11-12_tests-non-executes.md` (24 légitimes, 5 conditionnels, 4 à traiter).
- [ ] Corriger le timeout intermittent de `FullRunTests` (cause racine, pas de relance automatique).
- [ ] Nettoyer le reste des obsolètes validés par vous (anciens APK, `_deploy/`, `Builds/0.15.3`, anciens rapports non référencés).
- [ ] Étiqueter `v1.0.0-wip` sur le commit propre.

## Phase 1 — Figer le périmètre (1-2 jours)

**Sortie :** trois décisions inscrites dans `Decisions.md`, roadmap alignée.

- [x] Confirmer H1/H2/H3 (validées le 2026-11-12 : H1 et H2 modifiées, voir le tableau des hypothèses).
- [~] Trancher **UI Toolkit** : faire l'étape 0 de mesures (60 fps sur Android milieu de gamme, mémoire, polices Barlow SDF, cohabitation avec les Canvas uGUI). **Go** → phase 2A ; **no-go** → repli uGUI, phase 2B.
  - Mesures faites (polices, mémoire, cohabitation : conformes) : `docs/test-reports/2026-11-12_uitk-etape0.md`. **Reste le 60 fps Android** (APK de mesure prêt, procédure dans le rapport). **Go 2A validé le 2026-11-12**, conditionnel à la mesure Android.
- [x] Réconcilier `Roadmap.md` et le plan EA (une seule source de dates).
- [x] Auditer `WipCatalog.cs` : pour chaque fonction (pass, boutique, opérations, labo, archives, missions du jour, objectifs, gemmes, illustrations de membres, 5 pages de réglages), classer **retirer l'entrée** / **à finir** / **masquer**. Le labo a été supprimé au Bloc B : retirer son entrée.
  - Validé le 2026-11-12 : 11 entrées retirées (pass, shop, operations, lab, archives, daily_missions, gems, member_art, settings_audio, settings_graphics, settings_accessibility). Restent à trancher en phase 3 : `secondary_objectives`, `settings_notifications` (aucun consommateur ; ligne « labo » obsolète), `settings_controls` (lecture seule). Mécanisme WIP (page, relais, toast) à supprimer quand le catalogue sera vide.

## Phase 2 — Interface (A : UI Toolkit, ou B : repli uGUI) (1-2 semaines)

**Sortie :** aucun écran critique cassé ; un seul système d'UI par écran ; captures des 4 formats archivées.

- **2A (go) :**
  - [ ] Base commune : jetons, composants, navigation manette, `PlatformProfile`.
  - [ ] Migrer dans l'ordre du parcours joueur (**titre : fait et validé le 2026-11-12**, déjà en UI Toolkit depuis le lot 1a ; tests `TitleScreenUiToolkitTests` : 4 formats, manette, logotype ; connexion (fenêtre Compte) : fait le 2026-11-12, `AccountPopupUiToolkitTests` ; hub et QG : fait le 2026-11-12, `HubScreenUiToolkitTests` (4 formats, boutons atteignables à la manette) ; équipe et fiche d'agent : fait le 2026-11-12, `TeamScreenUiToolkitTests` ; carte et chapitres : fait le 2026-11-12, `CityMapScreenUiToolkitTests` ; plateau (HUD) : fait le 2026-11-12, `BoardScreenUiToolkitTests` ; combat (HUD) : fait le 2026-11-12, `CombatScreenUiToolkitTests` (chevauchements en 4:3 corrigés par `HdFit`, réduction de la mise en page 1920×1080 ; reste : cartes de compétence et fiches d'unités non contrôlées individuellement) ; résultats de combat, boutique (Héros, Boosters) et pass : fait le 2026-11-12, `ShopPassResultsUiToolkitTests` (défaut corrigé : en-tête du pass écrasé en 4:3) ; réglages (8 catégories) : fait le 2026-11-12, `SettingsScreenUiToolkitTests` ; `LocIdentityTests` : pas d'« Aube tactique ») : titre → connexion → hub/QG → équipe/fiche → carte/chapitres → plateau → combat → résultats → boutique/pass → réglages.
  - [ ] Garder en uGUI le plateau, les mini-jeux et les effets (décidé).
  - [ ] Supprimer le code et les prefabs uGUI d'un écran **seulement** après sa migration validée.
- **2B (repli) :**
  - [ ] Écrire la navigation manette et la mise en page adaptative une fois dans la couche commune ; geler les 41 fichiers UXML déjà faits.
- [ ] Dans les deux cas : test de capture sur 16:9, 20:9, 4:3, 1280×720 ; test d'absence de « Aube tactique » dans les textes.

## Phase 3 — Fonctionnel et services (1 semaine, en parallèle de la phase 2)

**Sortie :** plus de WIP visible ; services configurés ; analytics en place.

- [x] Finir ou retirer chaque entrée WIP (2026-11-12) : notifications masquées, objectifs secondaires et contrôles hors périmètre 1.0, mécanisme WIP supprimé. Lignes « BIENTÔT » de la langue retirées ; la fenêtre « simuler la réussite » n'existe plus que dans les builds de développement (`ActivityCoverageTests` garantit qu'aucun type d'activité n'y tombe).
- [~] **Analytics** (priorité n°1 du plan EA) : événements clés (début de session, zone, combat, achat, abandon) ; lier le projet UGS (`cloudProjectId` vide au dernier relevé, à revérifier).
  - Fait le 2026-11-12 : voie UGS Analytics, port `IAnalytics`, portail de consentement opt-in, passerelle `AnalyticsBridge` (état, combat, zone), `UgsAnalytics` muet sans projet lié, schéma `04_Tech_Unity/10_Analytics.md`, 6 tests. **Reste** : lier le projet UGS (vous), âge déclaré et texte juridique du consentement (la question du premier lancement existe), événements de l'entonnoir (prologue, premier achat, abandon, performances), vérification de bout en bout dans le Dashboard.
- [ ] Supabase : activer « leaked password protection » (tableau de bord, action de votre part) ; **sur accord explicite**, appliquer `002_hd_arena.sql` ; vérifier les quotas Realtime.
- [~] Sauvegarde : migration 0.15.0 → 1.0.0 couverte (`MetaMigrationTests`, idempotente, remboursement unique) et conflit appareil/compte (`AccountLinkSafetyTests`). Reste : un profil 0.15.x réel (archive utilisateur) et le test sur appareil.
- [x] Tournois : repli hors ligne après 15 s confirmé par test (`TournamentArenaTests.Sans_adversaire_apres_15_secondes_le_duel_passe_hors_ligne`, vert le 2026-11-12).
- [x] Réactiver les 4 tests EditMode « à traiter » de la baseline (fait le 2026-11-12 : `TestApp.ConnectedAccountLink()` fournit un compte connecté de test ; le test de fiche boutique ouvre la vraie boutique) (détail : `docs/test-reports/2026-11-12_tests-non-executes.md`), sans les affaiblir :
  - `FriendAddTests.AddFriend_WithAValidTag_AddsAndToasts` et `FriendAddTests.AddFriend_InvalidOrDuplicate_ShowsTheErrorToast` : réécrire avec un compte de test (faux port `IFriendsPort`/session connectée), au lieu d'un compte réel.
  - `ProfilePagePresenterTests.Header_CopyTag_Toasts` : même approche, tag issu du compte de test.
  - `MemberSheetPresenterTests.Loadout_HiddenOnAShopHeroSheet` : lui donner une offre de boutique réelle en entrée, puis vérifier que l'équipement est masqué.
  - Sortie : les 4 passent, retrait de leur `[Ignore]`, baseline relancée sans nouvel échec.

## Phase 4 — Équilibrage et playtest (1 semaine)

**Sortie :** valeurs ⚑ validées ou ajustées, consignées dans `05_Equilibrage.xlsx` et `Decisions.md`.

- [ ] Rejouer le simulateur d'équilibrage (`BalanceReports/`) après le Bloc A à E.
- [ ] Boss : ramener le taux de victoire de l'équipe de test dans **40 à 60 %** (Brick, Crocodile, Titan de l'AMIF), ajuster les bonus de phase.
- [ ] Valider les ⚑ : énergie (30 / +3), pause de résolution 1,3 s, paliers de lien (3-8-15-25), table des rangs AMIF, coûts de recrutement, paliers de tournoi, boutique de confort, déséquilibre des familles (Perception 9, Élémentaire 6).
- [ ] 2 à 3 sessions de playtest réel avec le formulaire `docs/playtest-formulaire.md`.
- [ ] Vérifier la garde anti pay-to-win (test existant) après chaque changement d'économie.

## Phase 5 — Contenu minimal (1-2 semaines, en parallèle)

**Sortie :** Brooklyn jouable de bout en bout sans texte provisoire visible.

- [ ] **Vous :** relire les textes des 4 fragments et du repaire (français), fournir ou valider l'anglais.
- [ ] **Vous :** décision sur les voix : fiche de casting ElevenLabs, ou sortie sans voix avec sous-titres (état actuel, acceptable pour une rc).
- [ ] Portraits d'agents et icônes : au minimum ceux du parcours Brooklyn et de l'équipe de départ (Giulia Rossi, Ilona Varga, Lucas Mendes).
- [ ] Agent M : afficher « bientôt » proprement (pas de bouton mort), ou le livrer si H1 change.
- [ ] Corriger l'écart connu : consignes d'Ingénierie de secours qui chevauchent le manuel.

## Phase 6 — QA, légal, release (1 semaine)

**Sortie :** release candidate puis tag stable.

- [ ] Régression complète : EditMode et PlayMode verts, aucun test ignoré sans motif écrit.
- [ ] Recette sur appareils : 2 Android (dont milieu de gamme), 1 PC ; parcours complet nouvelle partie → Brooklyn fini ; reprise après coupure réseau ; achat IAP en bac à sable.
- [ ] Performances : 60 fps cible sur l'Android de référence, mémoire stable sur 30 min.
- [ ] Obligations légales (`05_Production/Obligations_Legales.md`) : politique de confidentialité, classification d'âge (IARC/PEGI), mentions des assets générés par IA (R3).
- [ ] Notes de version 1.0.0 (sur le modèle de `release-notes-0.15.0.md`).
- [ ] `v1.0.0-rc1`, correctifs bloquants uniquement, puis `v1.0.0`.

---

## Critères de « stable » (définition de terminé)

1. EditMode et PlayMode : 100 % verts sur 3 exécutions consécutives.
2. Aucune entrée `WipCatalog` visible par le joueur.
3. Parcours Brooklyn complet réussi sur 2 appareils sans plantage ni blocage.
4. Aucun écran critique sans navigation (tactile, souris, clavier, manette) sur les 4 formats.
5. Profil 0.15.x chargé en 1.0.0 sans perte ; aucun double octroi d'achat.
6. Analytics reçoivent les événements clés.
7. Boss dans la cible de 40 à 60 % ; valeurs ⚑ consignées.
8. Dépôt propre : aucun worktree orphelin, aucun fichier temporaire suivi, tags posés.

## Calendrier proposé (à valider avec H2)

| Période | Phase |
|---|---|
| 12-20 nov. | 0 et 1 |
| 23 nov. – 18 déc. | 2 (A ou B) et 3 en parallèle |
| 23 nov. 2026 – 8 janv. 2027 | 5 (contenu : textes Brooklyn, 2 à 4 zones de Moscou) en parallèle |
| 14 déc. – 8 janv. | 4 (équilibrage et playtest, après stabilisation de l'interface) |
| 11-22 janv. | 6, **rc1 vers le 22/01/2027** |
| 22-29 janv. | correctifs bloquants, **stable le 29/01/2027** |

*Mise à jour du 2026-11-12 (H1 et H2 validées) : ancien calendrier, abandonné : 12-14 nov. phases 0 et 1 ; 15 nov. – 1er déc. phases 2, 3 et 5 ; 25 nov. – 5 déc. phase 4 ; rc1 vers le 10/12 ; stable le 14/12. Seules rc1 et stable sont des décisions ; les dates intermédiaires sont des propositions.*

## Risques propres à ce plan

- Étape 0 de l'UI Toolkit négative → bascule en 2B, perte de 1 à 2 semaines d'avance.
- Équipe de 1-2 personnes (R11) : si la phase 2 dépasse le 1er décembre, couper Android ou le JcJ en ligne plutôt que décaler la date.
- Contenu non disponible (voix, portraits) : sortir avec sous-titres et portraits du parcours seulement.
