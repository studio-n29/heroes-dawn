# Bloc E — Monétisation : pass, boutique de confort, cosmétiques, achats réels

> Spec du 2026-11-10. Source : point de développement « finaliser la 1.0.0 », objectif « système de monétisation ». Bloc E sur 5. Étend le modèle déjà validé dans `01_Game_Design/04_Economie.md` (v1) et l'aligne sur le code, qui s'en écarte. Dépend du bloc B (Composants, que le pass peut donner) et du bloc D (cosmétiques de tournoi).

## Principe
F2P + pass saisonnier. L'argent réel achète du **temps, du confort et des cosmétiques**. Tout ce qui est acheté (hors cosmétiques exclusifs) est aussi gagnable en jouant.

**Interdits** (vérifiés par un test) : puissance ; duel ou point de tournoi supplémentaire ; Composants, catalyseurs ou recettes contre des gemmes ; loot box ou gacha ; recrue exclusive au premium ; capacité d'inventaire limitée pour être revendue.

## Existant et écarts à corriger

| Sujet | Doc validée (`04_Economie.md`) | Code (0.15.0) |
|---|---|---|
| Packs de gemmes | 6 packs + offre de bienvenue | 3 packs (`gems_100`, `gems_550`, `gems_1200`) |
| Pass | 42 jours, 50 paliers de 1 300 XP, premium 9,99 € | 30 niveaux de 100 points, 30 jours, premium 300 gemmes (`PassTrack`) |
| Accélération | 20 gemmes par heure | 1 gemme par 4 minutes (`Expeditions.SpeedUpCost`) soit 15 par heure |
| Renouvellements | 1 gratuit, puis Crédits 100 → 800, puis gemmes | gratuit puis Crédits (`ShopRules.RerollCost`), pas de palier en gemmes |
| Emplacements d'expédition | — | 3 fixes (`Expeditions.Slots`) |

Le code s'aligne sur la doc. Le jeu n'est pas publié : le pass repart de zéro, sans migration des anciens niveaux ⚑.

## 1. Gemmes et packs
100 gemmes par euro. Packs (identifiants de produit = identifiants de pack) :

| Pack | Prix | Gemmes |
|---|---|---|
| `gems_100` Poignée | 0,99 € | 100 |
| `gems_520` Sac | 4,99 € | 520 |
| `gems_1080` Caisse | 9,99 € | 1 080 |
| `gems_2250` Coffre | 19,99 € | 2 250 |
| `gems_6000` Chambre forte | 49,99 € | 6 000 |
| `welcome` Offre de bienvenue (1 fois) | 2,99 € | 500 + 1 accélérateur de 8 h |

Les identifiants `gems_550` et `gems_1200` disparaissent.

## 2. Pass saisonnier
- **42 jours**, **50 paliers de 1 300 XP**, piste gratuite et piste premium. Le pass premium est un achat en argent réel (**9,99 €**, produit `pass_premium`), valable pour la saison en cours ; il ne s'achète plus en gemmes.
- Piste gratuite : Crédits, PR (~600 au total), objets de salon, 2 dés ou pions, Composants Commun et Raffiné, quelques catalyseurs.
- Piste premium : 3 tenues, 1 thème de QG, 300 gemmes, ~900 PR, accélérateurs, Composants Raffiné, catalyseurs. Aucune recrue exclusive, aucune recette exclusive.
- XP de pass : missions quotidiennes (500), objectifs secondaires, et **nouvelles sources** : boss de repaire vaincu (150), tournoi (30 par duel gagné, 10 par duel perdu), fabrication à l'Atelier (20) ⚑.
- Les tableaux de récompenses sont des données (`Resources/Content/pass_rewards.json`), pas du code.

## 3. Boutique de confort (gemmes)
- **Emplacements d'expédition** : 3 de base, 4e à 300 gemmes, 5e à 500 gemmes, achat unique permanent ⚑.
- **Renouvellements** (pool de recrues, boutique du jour, offres de Composants) : 1 gratuit par jour, puis Crédits (barème actuel jusqu'à 800), puis **10 gemmes** chacun ⚑.
- **Accélérations** : 20 gemmes par heure, arrondi à la minute supérieure, pour les expéditions ; le repos complet (10 gemmes) reste. L'Atelier est instantané et un duel de tournoi ne s'accélère pas : aucune accélération pour eux.
- Les échanges Crédits et PR contre gemmes (`ex_credits`, `ex_pr`) sont conservés.

## 4. Cosmétiques
- Catalogue `Resources/Content/cosmetics.json` : `id`, `nameKey`, `type` (Outfit, HqTheme, Dice, Pawn, Frame, Title), `rarity` (Agent, Élite, Légende, pour le prix), `source` (Shop, Pass, Tournament, Achievement, Event).
- Identifiants stockés dans `MetaSave.cosmetics` (liste unique du profil). Les cosmétiques de tournoi (bloc D) y sont ajoutés par la même opération.
- Boutique à **rotation hebdomadaire** (6 articles de source `Shop`, tirés par la semaine et la graine du catalogue). Prix : tenue Agent / Élite / Légende 300 / 600 / 1 000 gemmes ; thème de QG 400 ; dé ou pion 150 ; aucun article au-delà de 1 000 gemmes.
- Lancement : 4 tenues, 2 thèmes de QG, 3 dés et pions (volume du doc). Le contenu artistique est un lot à part ; la spec définit les données et la boutique.
- Cadres et titres de tournoi, succès : jamais en vente.

## 5. Achats réels
- `IPurchaseGateway` (existant) garde son contrat ; implémentation `UnityIapGateway` (package `com.unity.purchasing` 5.4.3, déjà dans le manifeste) pour Google Play et App Store ; simulateur conservé en éditeur et en test.
- Tous les produits sont des **consommables** (packs, offre de bienvenue, pass premium de la saison) : pas de « restauration des achats » à prévoir (obligatoire seulement pour les non-consommables). Les achats permanents (emplacements) se paient en gemmes.
- Validation du reçu **côté client** pour la 1.0.0. Une validation serveur (fonction Supabase) est une amélioration notée dans le plan LiveOps.
- Plateforme Windows / Steam : point ouvert du plan LiveOps, non traité ici.
- La passerelle ne donne les gemmes qu'après confirmation du store ; un achat interrompu (fermeture de l'appli) est repris au prochain lancement (transactions en attente).

## 6. Conformité
`05_Production/Obligations_Legales.md` : prix TTC affichés par le store, remboursements gérés par les stores, mention « pas de loot box », politique de confidentialité mise à jour (identifiants d'achat). L'écran d'âge existant reste.

## 7. Tests
Barème du pass (50 paliers, XP par palier, récompenses du JSON) ; packs ; offre de bienvenue une seule fois ; pass premium non cumulable dans une saison, rejoué à la saison suivante ; emplacements (solde, achat unique, effet sur `Expeditions`) ; coût des renouvellements ; accélération à 20 gemmes par heure ; boutique de cosmétiques (rotation hebdomadaire déterministe, achat unique d'un cosmétique) ; **garde anti pay-to-win** : aucun produit ni échange de gemmes ne donne de Composant, de catalyseur, de recette, de duel ou de point ; achat interrompu repris.

## Hors périmètre
Écrans (boutique, pass, cosmétiques équipés dans le QG et les fiches) : lot UI. Illustrations des cosmétiques : lot artistique. Validation serveur des reçus. Windows / Steam.

## Points ouverts ⚑
Prix des emplacements et des renouvellements en gemmes ; valeurs d'XP des nouvelles sources ; contenu exact des pistes du pass ; remise à zéro du pass (jeu non publié).
