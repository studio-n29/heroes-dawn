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

## Comptes joueurs et sauvegarde cloud : Supabase

Le jeu passe par deux ports : `IAccountGateway` (e-mail, Google, Apple, session, déconnexion) et `ICloudProfileGateway` (profil en ligne). Quand `Assets/_Project/Resources/Supabase/supabase.json` est valide, ce sont les adaptateurs Supabase (`Infrastructure/Supabase`) ; sinon les simulateurs en mémoire (éditeur sans réseau, tests). Les tests n'appellent **jamais** le vrai Supabase.

**Projet utilisé** : N29 HQ (`ulwbghlrglwccozbdlvr`, eu-central-1). Les tables `hq_vault` et `hq_vault_history` appartiennent à un autre outil et ne sont jamais touchées : toutes les tables du jeu ont le préfixe `hd_`.

### Fichier de configuration
`Resources/Supabase/supabase.json` : `url`, `anonKey` (clé publique par conception, rôle `anon`), `redirectUrl` (`heroesdawn://auth`). **Jamais** la clé `service_role` dans le dépôt ni dans le jeu (un test le vérifie).

### Base de données
Migration `Supabase/001_hd_profiles.sql` (appliquée sur N29 HQ le 2026-10-28) : table `hd_profiles` (une ligne par joueur : `user_id`, `profile` jsonb, `schema_version`, `updated_at` posée par le serveur), sécurité par ligne (le joueur ne lit, n'insère et ne modifie que sa ligne ; aucune suppression), profil limité à 512 Ko. L'écriture est conditionnelle sur `updated_at` : si le compte a changé depuis la dernière lecture, rien n'est écrasé et le joueur choisit quelle progression garder (l'autre est gardée dans une sauvegarde de secours sur l'appareil).

### À régler dans le tableau de bord Supabase (Authentication)
1. **Providers ▸ Google** : activer, renseigner l'identifiant et le secret du client OAuth créé dans la console Google Cloud (type « Application Web » ; URI de redirection autorisée : l'adresse de rappel indiquée par Supabase, `https://ulwbghlrglwccozbdlvr.supabase.co/auth/v1/callback`).
2. **Providers ▸ Apple** : activer, renseigner le Services ID, l'identifiant d'équipe, l'identifiant de clé et la clé privée (.p8) du compte développeur Apple ; mêmes adresses de rappel.
3. **URL Configuration ▸ Redirect URLs** : ajouter `heroesdawn://auth`.
4. **Providers ▸ Email** : décider si la confirmation d'adresse est obligatoire (le jeu gère les deux : message « E-mail de confirmation envoyé » si elle l'est).
5. Recommandé : **activer la protection contre les mots de passe compromis** (Authentication ▸ Attack Protection ; le conseiller de sécurité de Supabase la signale désactivée sur ce projet).
6. Côté appareil : enregistrer le schéma de lien `heroesdawn` (Android : filtre d'intention ; iOS : URL scheme) dans les réglages de build avant de tester Google et Apple sur un vrai téléphone.

Le guide de recette `Code/docs/recette-compte.md` liste les vérifications à faire sur appareil.
