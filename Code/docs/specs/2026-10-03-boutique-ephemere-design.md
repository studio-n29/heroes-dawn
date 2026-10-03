# Boutique éphémère de recrutement

> Statut : design validé en conversation le 2026-10-03 · à relire avant le plan d'implémentation.
> Branche : `feature/boutique-ephemere`, partie de `architecture/ecrans-prefabs` (écrans MVP en prefabs).
> Références : GDD §Recrutement et §8 Économie, `04_Tech_Unity/03_Modules_Domaine.md`, `04_Tech_Unity/07_Presentation_Flux.md`, `04_Tech_Unity/09_Tests_Outils_CI.md`.

## 1. Intention
- **Demande :** remplacer l'onglet Recrutement du QG (toutes les recrues non possédées, achat en PR) par une **boutique éphémère** : chaque jour à 9h00, 3 héros tirés au hasard, avec une chance d'apparition qui dépend de la rareté.
- **Choix validés :**
  - **9h00 heure locale** du téléphone ;
  - **renouvellement** : 1 gratuit par jour, puis payant en Crédits ;
  - **taux** : Agent 70 %, Élite 25 %, Légende 5 % ;
  - **conditions d'entrée** : une recrue peut exiger une condition (au M1 : Verre après la validation de la zone 1.1) ;
  - **approche A** : tirage **déterministe et recalculable** (graine = joueur + jour + numéro de tirage), pour qu'un serveur puisse le refaire plus tard.
- **Cohérent avec le GDD :** « pool quotidienne », « choix direct à coût fixe en PR (pas de gacha) », « pas de doublons », « renouvellement : 1 gratuit/jour, puis Crédits, puis premium », Légendes liées à l'histoire.
- **Réussite :**
  1. Le QG propose au plus 3 héros, qui changent à 9h00 heure locale, même jeu fermé ou ouvert sur l'onglet.
  2. Recruter garde les autres offres ; l'emplacement passe à « Recruté ».
  3. Renouveler : gratuit une fois par jour, puis 100 / 200 / 400 / 800 Crédits (plafond).
  4. Relancer le jeu redonne exactement la même boutique.
  5. Raretés conformes aux poids sur un grand nombre de tirages.
  6. Suites EditMode et PlayMode vertes, avec rapport HTML et synthèse en chat.

## 2. Périmètre
**Dans le périmètre :** règles de tirage (domaine), horloge locale, sauvegarde v3, cas d'usage Ouvrir / Recruter / Renouveler, onglet Boutique éphémère (prefabs), paramètres d'équilibrage, docs.

**Hors périmètre :** vérification serveur (UGS), renouvellement en monnaie premium, pool par ville (une seule ville au M1), saisie des 35 recrues de New York, modification du GDD et du tableur `05_Equilibrage.xlsx` (l'utilisateur les maintient ; textes et colonnes proposés à la fin du chantier).

## 3. Règles (domaine, C# pur)
Nouvelle assembly `N29.HeroesDawn.Domain.Shop` (références : `Core`, `Definitions`), sans dépendance à Unity.

- **Jour de boutique** — `ShopDay.Of(DateTime local, int resetHour)` : date de `local − resetHour heures`, au format `yyyy-MM-dd`. 8h59 = veille ; 9h00 = nouveau jour. Calcul sur l'heure murale locale : un changement d'heure d'été ne décale pas le renouvellement.
- **Prochain renouvellement** — `ShopDay.NextReset(DateTime local, int resetHour)` : prochain `resetHour:00` local (sert au compte à rebours).
- **Graine** — `Fnv1a64("{playerId}|{day}|{draw}")` → `Pcg32`. `draw` = 0 au premier tirage du jour, +1 à chaque renouvellement.
- **Éligibles** — recrues du contenu non possédées et dont la condition d'entrée est remplie. `MemberDef` gagne `UnlockZone` (`ZoneId?`, nul = toujours disponible) ; au M1, `verre` a `UnlockZone = zone_ny_1_1`.
- **Tirage** — `ShopDraw.Draw(IRandom rng, IReadOnlyList<MemberDef> eligible, ShopWeights w, int slots = 3)` → liste de `MemberId` (0 à 3), pour chaque emplacement :
  1. choisir la rareté selon les poids, en ignorant les raretés sans éligible restant (poids renormalisés) ;
  2. choisir uniformément une recrue de cette rareté, retirée des candidats (pas de doublon).
  Moins de 3 éligibles → moins d'emplacements ; aucun → liste vide (« collection complète »). Ordre des candidats : ordre du contenu (déterminisme).
- **Coût du renouvellement** — `ShopRules.RerollCost(int paidRerollsToday, IReadOnlyList<int> costs)` : le 1er renouvellement du jour est gratuit (0), le n-ième payant coûte `costs[min(n−1, last)]`. Par défaut `[100, 200, 400, 800]`.
- **Prix d'achat** — inchangé : `MetaRules.RecruitCost(rarity)` (150 / 450 / 1 200 PR).

## 4. Application
### Horloge
`IClock` gagne `DateTime LocalNow`. `SystemClock` renvoie `DateTime.Now` ; les tests utilisent une horloge réglable.

### État et sauvegarde
- `GameSession.Shop` : `ShopState { string Day; int Draw; int FreeRerollsUsed; int PaidRerolls; List<ShopOffer> Offers }`, `ShopOffer { MemberId Id; bool Recruited }`. Lecture seule pour l'UI via `IShopReadModel` (exposé par `ISessionReadModel.Shop`).
- `ProfileData.schemaVersion` = 3 ; nouveau bloc `shop` (`ShopSave { day, draw, freeRerollsUsed, paidRerolls, offers: List<string>, recruited: List<bool> }`). Profil v2 : chargé sans bloc, la boutique est tirée à la première ouverture. Les offres sont **mémorisées** : un achat ou une relance du jeu ne relance jamais le tirage.

### Cas d'usage (`Application/Features/Shop/`, un par intention, `UniTask<Result…>`)
| Cas d'usage | Règle |
|---|---|
| `IOpenShopUseCase` | Si `ShopDay.Of(LocalNow)` ≠ `Shop.Day` (ou pas de boutique) : nouveau jour, `Draw = 0`, compteurs de renouvellement à 0, tirage, sauvegarde, `ShopChanged`. Sinon : rien. |
| `IRecruitFromShopUseCase(int slot)` | Boutique expirée (jour changé) → `ShopErrors.Expired` + réouverture. Emplacement inconnu → `UnknownSlot`. Déjà recruté → `AlreadyRecruited`. PR insuffisants → `HubErrors.NotEnoughPr`. Sinon : débit PR, membre niveau 5 ajouté (dans l'escouade s'il y a de la place et pas de partie en cours, comme aujourd'hui), offre marquée `Recruited`, sauvegarde, `WalletChanged`, `RosterChanged`, `ShopChanged`. |
| `IRerollShopUseCase` | Boutique expirée → réouverture (renouvellement gratuit du nouveau jour non consommé) et `Expired`. Coût = `RerollCost` ; Crédits insuffisants → `NotEnoughCredits`. Sinon : débit, `Draw + 1`, compteur incrémenté, nouveau tirage (éligibles recalculés), sauvegarde, `WalletChanged`, `ShopChanged`. |

- Nouveau message `ShopChanged`. Erreurs localisées (`errors.shop.*`) affichées en toast.
- **Retirés :** `RecruitMemberUseCase`, `IRecruitMemberUseCase`, `GameSession.AvailableRecruits()`.

### Équilibrage
`BalanceConfig` gagne `ShopResetHour` (9), `ShopWeightAgent` (70), `ShopWeightElite` (25), `ShopWeightLegend` (5), `ShopRerollCosts` ([100, 200, 400, 800]) ; valeurs par défaut dans le code, surchargées par `balance.json`. L'import du tableur reconnaît ces noms de paramètres s'ils sont ajoutés à l'onglet « Paramètres » (à faire par l'utilisateur).

## 5. Écran (présentation, prefabs)
L'onglet **Recrutement** du QG devient **Boutique éphémère** ; vues générées par recettes puis retouchables.

- **En-tête** : titre « Boutique éphémère », compte à rebours « Nouveaux héros dans 5 h 12 » (minute près ; « dans 12 min » sous l'heure), bouton **Renouveler** (« Renouveler · Gratuit » / « Renouveler · 100 CR »), grisé si Crédits insuffisants ; toute dépense passe par la confirmation commune (`ConfirmPopup`).
- **Trois emplacements** côte à côte, réutilisant `RecruitCardView` (portrait, nom, rareté, rôle, stats, compétences, « Recruter · 150 PR »). États :
  - *disponible* (bouton actif si PR suffisants) ;
  - *recruté* : carte assombrie, tampon « Recruté », sans bouton ;
  - *vide* : moins de 3 éligibles ; si aucun : « Collection de New York complète ».
- **Bandeau à la couleur de la rareté** (Agent / Élite / Légende) sur chaque carte.
- **Présentateur** : `HubPresenter` ouvre la boutique à l'affichage et quand le jour change pendant l'affichage (tic : contrôle de la minute courante, au plus une ouverture par minute), rafraîchit sur `ShopChanged`, appelle Recruter / Renouveler sous `BusyGate` (double clic ignoré). Données de vue : `ShopHeaderData`, `RecruitCardData` + état de l'emplacement.
- Tous les textes dans `Fr` de `Loc.cs`.

## 6. Erreurs
| Cas | Comportement |
|---|---|
| 9h00 passe pendant que l'onglet est ouvert | Nouveau tirage au tic suivant, cartes rafraîchies. |
| Achat ou renouvellement sur une boutique expirée | Refus `Expired`, toast « La boutique s'est renouvelée », nouvelle boutique affichée. |
| Horloge du téléphone avancée | Accepté au M1 (pas de serveur) ; la graine recalculable permettra à UGS de refuser. |
| Contenu sans aucune recrue éligible | Boutique vide, message « collection complète », Renouveler désactivé. |
| Sauvegarde v2 | Chargée ; boutique tirée à la première ouverture. |

## 7. Tests (écrits avant le code, `[Description("Vérifie que … — parce que …")]`)
- **Domaine** : même graine → même tirage ; jour ou `draw` différents → graine différente ; pas de doublon ni de héros possédé ; rareté épuisée → poids renormalisés ; condition `UnlockZone` respectée ; < 3 éligibles et aucun éligible ; distribution sur 10 000 tirages à ±2 points de 70 / 25 / 5 (contenu fictif d'au moins 3 recrues par rareté : avec le vrai contenu, une rareté épuisée renormalise les poids) ; `ShopDay` à 8h59 / 9h00 et au passage à l'heure d'été ; `RerollCost` (0, 100, 200, 400, 800, 800).
- **Application** (horloge réglable) : ouverture le même jour = même boutique ; ouverture après 9h00 = nouveau tirage et compteurs remis à zéro ; recruter (succès, PR insuffisants, déjà recruté, emplacement inconnu, boutique expirée) ; renouveler (gratuit puis payant, Crédits insuffisants, boutique expirée) ; sauvegarde puis rechargement identiques ; profil v2.
- **Présentateur** (double de vue) : compte à rebours, états disponible / recruté / vide, libellé et état de Renouveler, confirmation avant dépense, double clic ignoré, rafraîchissement au changement de jour.
- **Prefabs** : structure du prefab de l'onglet et de la carte (garde-fous existants).
- **PlayMode** : `FullRunTests` recrute dans la boutique ; nouveau test : clic sur Renouveler → nouvelles offres affichées.

## 8. Documents
- `04_Tech_Unity/03_Modules_Domaine.md` (module Shop), `07_Presentation_Flux.md` (onglet), `09_Tests_Outils_CI.md` (suites), `05_Production/Decisions.md` (ligne « Boutique éphémère »).
- Proposition de texte pour le GDD §Recrutement et liste des paramètres à ajouter au tableur, remises à l'utilisateur.
