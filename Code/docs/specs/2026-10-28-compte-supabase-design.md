# Lot I (0.14.0) : compte Supabase et sauvegarde cloud

Source : PDF « N29 HQ » (playtest 0.13.0), section « À ajouter » : « compte utilisateur : Supabase (Mail/Google/Apple) » et « notifications push sur mobile ». Décision de cadrage : ce lot fait le **compte et la sauvegarde cloud** ; les notifications push font un lot suivant (comptes Firebase et Apple à fournir).

## 1. But et périmètre

Permettre à un joueur de créer un compte (e-mail, Google ou Apple), de le lier à sa progression d'invité, et de retrouver sa progression sur un autre appareil.

Dans le lot : port de compte étendu, client Supabase Auth, sauvegarde cloud du profil, écrans (fenêtre Compte, bandeau du profil, choix en cas de conflit), restauration de session au lancement, migration SQL de la table `hd_profiles` sur le projet existant **N29 HQ** (`ulwbghlrglwccozbdlvr`, eu-central-1).

Hors lot : notifications push ; amis en ligne ; achats liés au compte ; suppression de compte par le joueur (à prévoir avant la publication sur les stores) ; configuration des fournisseurs Google et Apple dans le tableau de bord Supabase (faite par l'utilisateur, la liste exacte des réglages est fournie).

Décisions de cadrage prises avec l'utilisateur : « compte d'abord, push ensuite » ; projet « N29 HQ existant » ; sauvegarde du « profil complet en cloud ».

Contraintes : les tables `hq_vault` et `hq_vault_history` du projet appartiennent à un autre outil et ne sont jamais modifiées ; toutes les tables du jeu ont le préfixe `hd_`. La clé `service_role` n'entre jamais dans le dépôt ni dans le client.

## 2. Ports et configuration

- **`IAccountGateway`** (Application/Ports) garde `SignUpAsync` et `SignInAsync` et gagne :
  - `SignInWithProviderAsync(AuthProvider provider, CancellationToken ct)` (Google, Apple) ;
  - `RestoreSessionAsync(CancellationToken ct)` : rouvre la session mémorisée, résultat vide si aucune ;
  - `SignOutAsync(CancellationToken ct)` ;
  - `ResetPasswordAsync(string email, CancellationToken ct)`.

  `PlayerAccount` porte l'identifiant, le pseudo, l'e-mail et le fournisseur. Les jetons restent dans la passerelle.
- **`SupabaseAccountGateway`** (Infrastructure/Supabase) appelle l'API REST de Supabase Auth par une couche `ISupabaseHttp` (`UnityWebRequest` en jeu, faux client en test) :
  - e-mail : `POST /auth/v1/signup`, `POST /auth/v1/token?grant_type=password`, `POST /auth/v1/recover` ;
  - session : `POST /auth/v1/token?grant_type=refresh_token` (rafraîchissement automatique avant expiration et sur réponse 401) ;
  - Google et Apple : `GET /auth/v1/authorize?provider=…` dans le navigateur du téléphone avec PKCE, retour dans l'app par le lien `heroesdawn://auth` (analyse du code puis échange `grant_type=pkce`).
- **Configuration** : un fichier `Resources/Supabase/supabase.json` (URL du projet, clé `anon` publique par conception, lien de retour). Sans ce fichier, ou en éditeur sans réseau, le jeu utilise le simulateur `LocalAccountGateway` et un faux cloud en mémoire.
- **Jetons** : conservés hors du profil (stockage de l'appareil), jamais écrits dans les journaux, effacés à la déconnexion.
- **`ICloudProfileGateway`** (Application/Ports) : `GetAsync(ct)` (profil et `updated_at`, ou vide) et `UpsertAsync(profile, expectedUpdatedAt, ct)` (écriture conditionnelle ; renvoie un conflit si la ligne a changé). `SupabaseCloudProfileGateway` appelle PostgREST `rest/v1/hd_profiles` avec le jeton du joueur.
- **Erreurs** : chaque échec (réseau, identifiants, e-mail déjà pris, mot de passe faible, e-mail non confirmé, limite de débit, conflit) est un `DomainError` à clé de localisation `errors.account_*`, en français.

## 3. Base de données

Migration versionnée `Code/Heroes Dawn Unity/Assets/_Project/Backend/Supabase/001_hd_profiles.sql`, appliquée sur N29 HQ **seulement après accord de l'utilisateur sur le SQL** :

- table `public.hd_profiles` : `user_id uuid primary key references auth.users on delete cascade`, `profile jsonb not null`, `schema_version int not null`, `updated_at timestamptz not null default now()` ;
- contrainte de taille : `pg_column_size(profile) <= 524288` (512 Ko) ;
- sécurité par ligne activée ; politiques `select`, `insert`, `update` limitées à `auth.uid() = user_id` ; aucune politique `delete` ;
- déclencheur qui renseigne `updated_at` à chaque écriture ;
- vérifications : `get_advisors` (sécurité) sans alerte sur la nouvelle table, et un jeu de requêtes d'essai qui prouve l'isolation entre deux utilisateurs.

## 4. Parcours et écrans

Tous les écrans viennent de prefabs générés (règle du projet : aucun écran construit par code). Tous les textes sont en français dans `Presentation/Common/LocAccount.cs`.

- **Écran titre** : « Jouer » reste (invité) ; un bouton « Se connecter » ouvre la fenêtre **Compte** (`Popups/AccountPopup`), deux onglets Connexion et Créer un compte : e-mail, mot de passe (8 caractères minimum, vérifié avant l'envoi), pseudo à la création, « Continuer avec Google », « Continuer avec Apple », « Mot de passe oublié », états chargement / erreur / « E-mail de confirmation envoyé ». Cibles tactiles ≥ 96 px.
- **Lancement** : le jeu tente `RestoreSessionAsync` ; session valide = connexion directe puis récupération du profil cloud ; sinon l'écran titre habituel.
- **Page Profil** : bandeau « Compte ». Invité : « Lier mon compte » (ouvre la fenêtre Compte). Connecté : e-mail, fournisseur, dernière synchronisation, « Synchroniser maintenant », « Se déconnecter ».
- **Rattachement d'un invité** : si le compte n'a pas de profil cloud, le profil local y est envoyé. Si le compte en a un et que l'appareil a aussi une progression, la fenêtre **Choix de progression** (`Popups/ProgressChoicePopup`) propose « Garder cet appareil » ou « Garder le compte », chacun avec niveau, crédits et date. L'autre version est d'abord copiée dans une sauvegarde locale de secours.
- **Déconnexion** : après confirmation, le joueur reste en invité sur l'appareil avec sa progression actuelle, qui n'est plus synchronisée ; les jetons sont effacés.
- **Hors ligne** : le jeu continue avec le profil local ; la synchronisation reprend à la reconnexion.

## 5. Synchronisation

- Au lancement (après restauration de session) : lecture de la ligne du joueur.
- À chaque sauvegarde locale du profil : envoi différé (au plus un envoi toutes les 20 s) et un envoi final quand l'appli passe en arrière-plan ou se ferme. Un seul envoi à la fois ; les modifications arrivées pendant un envoi partent au suivant.
- Écriture conditionnelle sur `updated_at` : si la ligne a changé depuis la dernière lecture, l'envoi est refusé, le jeu relit la ligne et ouvre le choix de progression.
- Un profil du cloud d'une version de schéma plus récente que celle du jeu n'est pas appliqué (message « mets le jeu à jour »).
- Un échec réseau n'interrompt jamais la partie : nouvel essai espacé (5 s, 20 s, 60 s, puis à la prochaine sauvegarde).

## 6. Tests

- **EditMode** (faux client HTTP et faux cloud) : inscription, connexion, erreurs traduites, jeton expiré puis rafraîchi, reprise de session, flux OAuth (adresse construite avec PKCE, lecture du retour, rejet d'un état ou code invalide), envoi différé et envoi final, un seul envoi à la fois, conflit et choix du joueur, rattachement d'un invité (cloud vide / cloud non vide), déconnexion (jetons effacés, profil local conservé), version de schéma trop récente, textes présents pour chaque erreur.
- **PlayMode** : fenêtre Compte (onglets, validation, états), bandeau du profil (invité / connecté), choix de progression, avec le simulateur.
- **Captures** : une par écran (`CaptureAccount`, explicite).
- **Base** : requêtes d'essai et conseillers de sécurité sur N29 HQ après la migration. Avec l'accord de l'utilisateur à ce moment-là : un test réel d'inscription et de sauvegarde avec un compte jetable, supprimé ensuite.
- **Non vérifiable ici** : connexion Google et Apple réelle (tableau de bord à configurer, navigateur du téléphone), e-mail de confirmation, retour dans l'app sur un vrai appareil. Ces points sont listés dans un guide de recette pour l'utilisateur.

## 7. Documents

Cette spec, le plan (`Code/docs/plans/2026-10-28-compte-supabase.md`), `Assets/_Project/Backend/README.md` (section « Comptes joueurs » remplacée par le mode d'emploi réel, dont la liste des réglages à faire dans Supabase), `03_Art_UI_UX/01_UX_Ecrans.md` (section « Lot I »), `05_Production/Decisions.md`.

## 8. Découpage prévu des tâches

Ports et erreurs ; couche HTTP et client Auth (e-mail, session) ; flux OAuth ; port et client de sauvegarde cloud ; migration SQL (accord demandé) ; service de synchronisation et conflits ; fenêtre Compte et titre ; bandeau du profil et choix de progression ; restauration au lancement ; configuration et sélection du simulateur ou de Supabase ; documents, captures et suites complètes ; relecture finale par un agent indépendant.
