# Backend UGS — piloté depuis Unity

Tout le backend est configuré ici, sous forme de fichiers déployés par **Window ▸ Deployment**. Aucun projet .NET hors de Unity.

| Dossier | Contenu | Format |
|---|---|---|
| `CloudCode/` | Un script par point d'entrée serveur (`<PointDEntrée>.js`) | Cloud Code JS (`module.exports` + `module.exports.params`) |
| `Economy/` | Monnaies `CREDITS`, `PR`, `GEMS` (l'ID = le nom du fichier) | `.ecc` (JSON) |
| `RemoteConfig/` | `balance.rc` : clé JSON `balance` = `BalanceConfig` | **Généré** par *Heroes Dawn ▸ Équilibrage ▸ Importer* — ne pas éditer à la main |

## Règle : le client calcule, le serveur contrôle
Les règles du jeu (plateau, combat) tournent dans le client, avec le domaine C# partagé. Le serveur ne rejoue pas les combats : il fait des **contrôles légers**.

Contrôles prévus au M2 :
- **Plafond de gain** par case et par zone, lu dans la clé Remote Config `balance` (même valeur que le client).
- **Idempotence** : chaque appel porte un `requestId` ; un rejeu renvoie la même réponse sans re-créditer.
- **Horodatage serveur** pour les timers (entraînement, labo, expéditions, objectifs secondaires, pool du jour).
- **Monnaies** débitées et créditées uniquement par Economy (recrutement, renouvellement, améliorations).

## Déployer
1. *Edit ▸ Project Settings ▸ Services* : lier le projet Unity Gaming Services du studio.
2. Choisir l'environnement `dev` (jamais `production` à la main).
3. *Window ▸ Deployment* ▸ **Deploy All**. Attendu : 3 monnaies, les scripts Cloud Code et `balance.rc` affichés « Deployed ».

Après chaque modification du tableur : *Heroes Dawn ▸ Équilibrage ▸ Importer*, puis redéployer `balance.rc`.

## Comptes joueurs : Supabase (à brancher)
Le jeu passe par le port `IAccountGateway` (`SignUpAsync`, `SignInAsync`) ; en 0.9.0 il est joué par `LocalAccountGateway` (comptes en mémoire). Pour brancher Supabase Auth :
1. Créer le projet Supabase du studio et fournir l'URL et la clé `anon` (jamais la clé `service_role` dans le client).
2. Écrire `SupabaseAccountGateway : IAccountGateway` (API REST `/auth/v1/signup` et `/auth/v1/token?grant_type=password`) et l'enregistrer à la place de `LocalAccountGateway` dans `RootLifetimeScope`.
3. Ajouter l'écran e-mail / mot de passe sur l'écran titre (la première connexion en invité reste possible : pseudo et campagne sont déjà dans le profil).
