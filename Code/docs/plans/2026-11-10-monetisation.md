# Monétisation — Plan d'implémentation

> **Pour les agents :** sous-compétence requise : superpowers:executing-plans (exécution native) ou superpowers:subagent-driven-development. Cases à cocher (`- [ ]`).

**Objectif :** aligner le code sur le modèle F2P validé (packs, pass 50 paliers, accélérations), ajouter la boutique de confort (emplacements, renouvellements en gemmes), les cosmétiques, et brancher les achats réels (Unity IAP).

**Architecture :** `Application/Features/Meta` (`Boutique`, `PassProgress`, `Expeditions`, nouveau `CosmeticShop`), données dans `Resources/Content/*.json`, passerelle d'achat `UnityIapGateway` dans `Infrastructure/`. Aucun écran modifié.

**Stack :** C# Unity 6000.6.3f1, NUnit Edit Mode, `com.unity.purchasing` 5.4.3.

**Spec :** `Code/docs/specs/2026-11-10-monetisation-design.md`

## Contraintes globales

- Français (textes, commentaires, commits conventionnels) ; chaque test a `[Description("Vérifie que … — parce que …")]`.
- Tests en batch, Unity fermé (commande de `Code/SETUP.md`, §Tests), résumé dans `$TEMP/editmode.xml`.
- `git add` par chemin, jamais `-A` ni `.` ; pas de push.
- Aucun prefab, aucune scène, aucun écran modifié ; les écrans `Boutique*`, `Pass*` ne reçoivent que des corrections de compilation.
- Aucune clé secrète dans le dépôt ; aucun identifiant de produit de store réel inventé : les identifiants de la spec sont les identifiants de produit à déclarer par l'utilisateur dans Google Play Console et App Store Connect.
- **Ordre :** après les plans A à D, sur la même branche. Les tâches 1 à 3 et 5 sont indépendantes du bloc D ; la tâche 4 le complète.
- Valeurs de la spec : voir tableaux §1 à §4 (packs, pass 42 jours / 50 paliers / 1 300 XP / 9,99 €, emplacements 300 et 500 gemmes, renouvellement 10 gemmes, accélération 20 gemmes par heure).

## Revue ciblée

1. Achat interrompu (appli fermée entre le paiement et l'octroi) : repris au lancement suivant, une seule fois, sans double octroi des gemmes.
2. Pass premium acheté deux fois dans la même saison, ou à la fin d'une saison : refusé sans débit ; la saison suivante, rachetable.
3. Offre de bienvenue rachetée : refusée sans débit.
4. Solde de gemmes insuffisant pour un emplacement ou un renouvellement : refusé, rien n'est dépensé, rien n'est octroyé.
5. Emplacement d'expédition acheté, expédition en cours dans l'emplacement 3 : la liste des emplacements s'allonge sans déplacer les expéditions existantes.
6. Sauvegarde 0.15.0 avec `gems_550` ou `gems_1200` dans `purchases` : les compteurs d'achats anciens sont ignorés sans erreur.
7. Cosmétique déjà possédé : l'achat est refusé et la boutique l'affiche comme possédé.

## Structure des fichiers

| Fichier | Rôle |
|---|---|
| `Application/Features/Meta/Boutique.cs` | 6 packs + offre de bienvenue, emplacements, renouvellements en gemmes |
| `Application/Features/Meta/PassProgress.cs` | `PassTrack` : 50 paliers, 1 300 XP, 42 jours, premium par achat réel ; récompenses lues dans le JSON |
| `Resources/Content/pass_rewards.json` (nouveau) | récompenses des deux pistes |
| `Application/Features/Meta/Expeditions.cs` | `SlotCount(meta)`, coût d'accélération |
| `Packages/com.n29.heroesdawn.domain/Domain/Shop/ShopRules.cs` | coût de renouvellement en gemmes |
| `Application/Features/Meta/CosmeticShop.cs`, `CosmeticOps.cs` (nouveaux) | catalogue, rotation, achat, octroi |
| `Packages/com.n29.heroesdawn.domain/Definitions/Cosmetics.cs`, `Resources/Content/cosmetics.json` (nouveaux) | données |
| `Application/Ports/MetaSave.cs` | `cosmetics`, `expeditionSlotsBought`, `welcomeClaimed`, `passSeason` |
| `Infrastructure/Platform/UnityIapGateway.cs` (nouveau) | achats réels |
| `Assets/Tests/Application/BoutiqueTests.cs`, `PassTrackTests.cs`, `ComfortShopTests.cs`, `CosmeticShopTests.cs`, `NoPayToWinTests.cs`, `Assets/Tests/Infrastructure/UnityIapGatewayTests.cs` | tests |

---

### Tâche 1 : Packs de gemmes et offre de bienvenue

**Fichiers :** modifier `Boutique.cs`, `MetaSave.cs` ; adapter les tests existants de la boutique ; créer `BoutiqueTests.cs`.

**Interfaces — produit :**
```csharp
// Boutique.Packs : 6 GemPack (gems_100, gems_520, gems_1080, gems_2250, gems_6000) + Welcome
public static readonly GemPack Welcome = new GemPack("welcome", 500, "2,99 €");   // + accélérateur de 8 h
public async UniTask<Result<int>> BuyPackAsync(string packId, CancellationToken ct);   // « welcome » : une seule fois
// MetaSave : public bool welcomeClaimed ; public int speedUpTokens (accélérateurs en minutes, 480 pour 8 h)
```
Prix texte : 0,99 €, 4,99 €, 9,99 €, 19,99 €, 49,99 € (le texte vrai vient du store ; ce texte ne sert qu'au simulateur).

- [ ] **Étape 1 : tests.** `Les_packs_sont_les_cinq_de_la_spec` (gemmes 100 / 520 / 1 080 / 2 250 / 6 000) ; `Acheter_un_pack_donne_les_gemmes_apres_confirmation` (passerelle simulée) ; `Un_paiement_refuse_ne_donne_rien` ; `L_offre_de_bienvenue_donne_500_gemmes_et_un_accelerateur_de_8_heures` ; `L_offre_de_bienvenue_ne_s_achete_qu_une_fois_sans_debit` (Revue 3) ; `Les_anciens_compteurs_gems_550_sont_ignores` (Revue 6).
- [ ] **Étape 2 : implémenter.** Un accélérateur est dépensé automatiquement par `Expeditions.SpeedUp` avant les gemmes (jusqu'à épuisement de ses minutes).
- [ ] **Étape 3 : tests Edit Mode.** **Commit** : `feat(boutique): cinq packs de gemmes et offre de bienvenue`.

---

### Tâche 2 : Pass saisonnier de 42 jours, 50 paliers, premium en argent réel

**Fichiers :** modifier `PassProgress.cs`, `MetaSave.cs` (`passSeason`), `RootLifetimeScope.cs` ; créer `Resources/Content/pass_rewards.json`, `Assets/Tests/Application/PassTrackTests.cs` ; adapter `MetaServiceTests` (pass) ; corrections de compilation des écrans qui lisent `PassTrack.Levels`/`PremiumCost`.

**Interfaces — produit :**
```csharp
public static class PassTrack {
    public const int Levels = 50, XpPerLevel = 1300, SeasonDays = 42;
    public const string PremiumProduct = "pass_premium";
    public static Reward Free(int level);       // lu dans pass_rewards.json
    public static Reward Premium(int level);
    public static IReadOnlyList<PassItem> Items(int level, bool premium);   // PassItem : Cosmetic id | Component(kind?, grade, count) | Catalyst | SpeedUpMinutes
}
// PassProgress
public async UniTask<Result> BuyPremiumAsync(CancellationToken ct);   // via IPurchaseGateway(PremiumProduct) ; refus si déjà premium dans la saison
public int SeasonIndex { get; }                                       // numéro de saison depuis une date de référence fixe du calendrier
```
Total de la piste gratuite : ~600 PR ; piste premium : 3 tenues, 1 thème de QG, 300 gemmes, ~900 PR, accélérateurs, Composants Raffiné, catalyseurs (la spec §2) ; Composants Commun et Raffiné et quelques catalyseurs en piste gratuite. Le fichier fixe les valeurs par palier (une ligne par palier de 1 à 50) : l'implémenteur les construit pour respecter ces totaux, et les affiche dans la conversation pour relecture avant de lancer les tests.

- [ ] **Étape 1 : tests.** `Le_pass_a_50_paliers_de_1300_XP` ; `Le_niveau_se_calcule_a_partir_de_l_XP` (0 → 0, 1 299 → 0, 1 300 → 1, 65 000 → 50, 70 000 → 50) ; `Les_totaux_des_pistes_respectent_la_spec` (PR gratuits 540 à 660 ; PR premium 810 à 990 ; gemmes premium 300 ; 3 tenues + 1 thème en premium, aucune tenue en gratuit) ; `Le_premium_s_achete_en_argent_reel_et_une_fois_par_saison` (Revue 2) ; `La_saison_suivante_le_premium_est_a_racheter` ; `Une_nouvelle_saison_remet_l_XP_et_les_paliers_reclames_a_zero` ; `Aucun_palier_ne_donne_de_recette_ni_de_duel`.
- [ ] **Étape 2 : implémenter.** Sources d'XP ajoutées : appeler `PassProgress.AddXp` depuis la fin d'un boss de repaire (150), un duel de tournoi (30 gagné, 10 perdu) et une fabrication (20), aux endroits des plans C, D et B (`LairBook.GrantBossReward`, `TournamentService.FinishDuelAsync`, `Workshop.Craft`) ; si ces plans ne sont pas encore exécutés, laisser des méthodes publiques `PassXp.Lair()`, `PassXp.Duel(bool won)`, `PassXp.Craft()` et noter dans la conversation les trois appels à brancher.
- [ ] **Étape 3 : tests Edit Mode.** **Commit** : `feat(pass): saison de 42 jours, 50 paliers, premium en achat réel`.

---

### Tâche 3 : Boutique de confort (emplacements, renouvellements, accélérations)

**Fichiers :** modifier `Expeditions.cs`, `Boutique.cs`, `MetaSave.cs` (`expeditionSlotsBought`), `Domain/Shop/ShopRules.cs`, `RerollShopUseCase.cs`, `BalanceConfig.cs` (`ShopRerollGemCost = 10`) ; créer `Assets/Tests/Application/ComfortShopTests.cs`.

**Interfaces — produit :**
```csharp
// Expeditions
public const int BaseSlots = 3, MaxSlots = 5;
public static int SlotCount(MetaSave m) => BaseSlots + Math.Min(2, m.expeditionSlotsBought);
public static int SpeedUpCost(TimeSpan left) => Math.Max(1, (int)Math.Ceiling(left.TotalMinutes / 3.0));   // 20 gemmes par heure
// Boutique
public static int SlotPrice(int slotNumber);                       // 4 → 300, 5 → 500
public Result BuyExpeditionSlot();                                 // le prochain emplacement ; refus au maximum ou sans gemmes
// ShopRules
public static (int Credits, int Gems) RerollPrice(int rerollsToday, BalanceConfig b);   // 0 → gratuit ; ladder en Crédits ; au-delà, gemmes
```
`Expeditions.Slots` (constante) est remplacé par `SlotCount(meta)` partout (`grep -rn "Expeditions.Slots"`).

- [ ] **Étape 1 : tests.** `Le_4e_emplacement_coute_300_gemmes_et_le_5e_500` ; `Acheter_sans_assez_de_gemmes_ne_depense_rien` (Revue 4) ; `Au_dela_du_5e_emplacement_l_achat_est_refuse` ; `Les_expeditions_en_cours_ne_bougent_pas_quand_un_emplacement_s_ajoute` (Revue 5) ; `Le_premier_renouvellement_du_jour_est_gratuit_puis_Credits_puis_10_gemmes` ; `Une_acceleration_coute_20_gemmes_par_heure_arrondie_a_la_minute` (60 min → 20 ; 61 min → 21 ; 1 min → 1) ; `Un_accelerateur_est_depense_avant_les_gemmes`.
- [ ] **Étape 2 : implémenter**, `RerollShopUseCase` choisit la monnaie selon `RerollPrice`.
- [ ] **Étape 3 : tests Edit Mode.** **Commit** : `feat(boutique): emplacements d'expédition, renouvellements en gemmes, accélération à 20 gemmes par heure`.

---

### Tâche 4 : Cosmétiques

**Fichiers :** créer `Definitions/Cosmetics.cs`, `Resources/Content/cosmetics.json`, `Application/Features/Meta/CosmeticShop.cs`, `CosmeticOps.cs`, `Assets/Tests/Application/CosmeticShopTests.cs` ; modifier `MetaSave.cs` (`cosmetics`), et, si le plan D est exécuté, retirer `ArenaSave.cosmetics` en migrant son contenu vers `MetaSave.cosmetics` et en faisant appeler `CosmeticOps.Unlock` par `TournamentService.ClaimTierAsync`.

**Interfaces — produit :**
```csharp
public enum CosmeticType { Outfit, HqTheme, Dice, Pawn, Frame, Title }
public enum CosmeticSource { Shop, Pass, Tournament, Achievement, Event }
public sealed class CosmeticDef { public string Id; public string NameKey; public CosmeticType Type; public int Price /* gemmes ; 0 hors boutique */; public CosmeticSource Source; }
public static class CosmeticOps { public static bool Owns(MetaSave m, string id); public static bool Unlock(MetaSave m, string id); }   // renvoie faux si déjà possédé
public sealed class CosmeticShop {
    public IReadOnlyList<CosmeticOffer> Weekly(DateTime day);        // 6 articles de source Shop, déterministe pour la semaine ISO
    public Result Buy(string cosmeticId);                            // hors rotation, déjà possédé, ou gemmes insuffisantes : refus
}
```
Le catalogue de lancement : 4 tenues (une par Légende : `agent_d`, `agent_m`, `victor_sorel`, `odile_ferrand`, prix 1 000), 2 thèmes de QG (400), 3 dés et pions (150), plus 2 cadres et 2 titres de tournoi (source `Tournament`, prix 0). Les noms et visuels sont des clés de texte `cosmetic.<id>.name` ; les illustrations sont un lot artistique séparé (aucune image dans ce plan).

- [ ] **Étape 1 : tests.** `La_rotation_hebdomadaire_est_deterministe_et_change_chaque_semaine` ; `La_rotation_ne_propose_que_des_articles_de_source_Shop_et_aucun_possede` ; `Acheter_debite_les_gemmes_et_ajoute_le_cosmetique` ; `Un_cosmetique_possede_ne_se_rachete_pas` (Revue 7) ; `Un_cosmetique_de_tournoi_n_est_jamais_en_vente` ; `Aucun_article_ne_depasse_1000_gemmes` ; `Unlock_est_idempotent`.
- [ ] **Étape 2 : implémenter.** **Étape 3 : tests Edit Mode.** **Commit** : `feat(boutique): catalogue de cosmétiques et rotation hebdomadaire`.

---

### Tâche 5 : Achats réels (Unity IAP)

**Fichiers :** créer `Infrastructure/Platform/UnityIapGateway.cs`, `Assets/Tests/Infrastructure/UnityIapGatewayTests.cs` ; modifier `RootLifetimeScope.cs` (choix de l'implémentation : éditeur et tests = simulateur, appareil = Unity IAP) ; documenter dans `Code/SETUP.md` la déclaration des produits.

**Interfaces — produit :**
```csharp
public sealed class UnityIapGateway : IPurchaseGateway, IDisposable {
    public UniTask<Result> PurchaseAsync(string productId, CancellationToken ct);
    // Produits consommables : gems_100, gems_520, gems_1080, gems_2250, gems_6000, welcome, pass_premium
    // Transactions en attente au démarrage : événement PendingPurchase(productId) traité par Boutique/PassProgress
}
```
Utiliser l'API de la version 5.4.3 du package (`StoreController`) : lire la documentation du package installé avant d'écrire (`Library/PackageCache/com.unity.purchasing@5.4.3/Documentation~`). Un achat n'est confirmé (`ConfirmPurchase`) qu'après l'octroi des gemmes ou du pass ; reçu validé côté client.

- [ ] **Étape 1 : tests** avec un faux magasin derrière une interface `IStoreAdapter` écrite dans le même fichier (le test n'utilise pas le vrai SDK) : `Un_achat_reussi_est_confirme_apres_l_octroi` ; `Un_achat_annule_ne_donne_rien` ; `Un_produit_inconnu_est_refuse` ; `Un_achat_en_attente_est_repris_au_lancement_une_seule_fois` (Revue 1) ; `Une_erreur_du_store_donne_une_erreur_localisee`.
- [ ] **Étape 2 : implémenter** l'adaptateur réel autour de `IStoreAdapter`.
- [ ] **Étape 3 : vérification manuelle (utilisateur).** Liste à fournir dans la conversation : déclarer les 7 produits consommables dans Google Play Console et App Store Connect avec les identifiants ci-dessus, créer des testeurs de licence, tester un achat en bac à sable sur appareil. Ce plan ne peut pas l'exécuter.
- [ ] **Étape 4 : tests Edit Mode.** **Commit** : `feat(achats): passerelle Unity IAP (consommables) et reprise des achats en attente`.

---

### Tâche 6 : Garde anti pay-to-win, documentation et conformité

**Fichiers :** créer `Assets/Tests/Application/NoPayToWinTests.cs` ; modifier `01_Game_Design/04_Economie.md` (réécriture : packs, pass, boutique de confort, cosmétiques, Atelier à la place du labo), `00_GDD/GDD_Heroes_Dawn.md` §8, `05_Production/Obligations_Legales.md` (section Monétisation), `05_Production/Decisions.md` (`git add -p`), `05_Production/Live_Ops_Plan_24_mois.md` (validation serveur des reçus, Windows / Steam).

- [ ] **Étape 1 : tests.**
  - `Aucun_pack_ni_echange_ni_article_ne_donne_de_composant_catalyseur_ou_recette` : parcourt `Boutique.Packs`, `Boutique.Exchanges`, la boutique de cosmétiques et les récompenses des deux pistes du pass ; aucune récompense n'est de ces types hors Composants du pass (autorisés, mais pas les recettes ni les points de tournoi).
  - `Aucun_achat_ne_donne_de_duel_ni_de_point_de_tournoi`.
  - `Aucune_recrue_n_est_exclusive_au_premium`.
  - `Aucun_cosmetique_ne_modifie_une_statistique` : un cosmétique ne figure dans aucun calcul de `UnitFactory`.
- [ ] **Étape 2 : documentation.** Mettre à jour les documents (le tableau de la spec « Existant et écarts » devient historique) ; ajouter à `Obligations_Legales.md` les cases à cocher : déclaration des produits, prix TTC par pays, politique de confidentialité pour les identifiants d'achat, test en bac à sable.
- [ ] **Étape 3 : suite complète** Edit Mode, aucun échec nouveau ; `git diff --stat` sans prefab ni scène. **Commit** : `docs(conception): monétisation, confort et garde anti pay-to-win`.

---

## Hors plan
Écrans (boutique, pass, cosmétiques équipés) : lot UI. Illustrations des cosmétiques : lot artistique. Validation serveur des reçus ; Windows / Steam ; déclaration des produits et tests en bac à sable sur les stores (utilisateur).

## Auto-relecture
- Couverture : §1 (tâche 1), §2 (2), §3 (3), §4 (4), §5 (5), §6 (6), §7 (tests de chaque tâche et tâche 6).
- Les tableaux de récompenses du pass sont à construire puis relire : c'est la partie la moins déterminée de ce plan (étape 1 de la tâche 2 impose l'affichage avant les tests).
- Dépendances : les sources d'XP (tâche 2) branchent des plans B, C et D ; le déplacement des cosmétiques de tournoi (tâche 4) suppose le plan D.
