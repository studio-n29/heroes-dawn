# Lot F — UI : Profil, boutique en cartes, salons sur la fiche d'agent

Date : 2026-10-27 · Version 0.14.0 · Source : PDF « N29 HQ » (playtest 0.13.0), points « L'onglet Profil », « La boutique » et « Les salons d'agents (entraînement et infirmerie) ».

## Objet

Trois changements d'interface :

1. Un **Profil** de joueur (tag personnel, amis, statistiques, accès au registre) à la place de l'onglet qui ouvre aujourd'hui les Archives.
2. Une **boutique** plus légère et plus visuelle : des cartes avec icône et description, comme un store.
3. L'**entraînement et l'infirmerie sur la carte de l'agent** : plus de pages de salon.

Décisions validées :

- Amis : un port simulé local (ajout par tag, enregistré dans le profil) ; le lot I branchera Supabase sans changer l'écran.
- Les pages Salon d'entraînement, Infirmerie et Salon unique sont supprimées ; les cartes du QG renvoient à la liste des agents filtrée.
- Soin par agent (−30 points de fatigue, 50 Crédits) sur la carte ; le soin collectif (150 Crédits) reste sur la page Équipe.

Hors lot : comptes Supabase, amis en ligne, notifications push (lot I) ; narration (lot G) ; mini-jeux (lot H).

## 1. Profil

### Navigation

- `HubNavigator.Tabs` remplace `HubPage.Archives` par un nouveau `HubPage.Profile` (même position, dernière tab). Libellé « Profil » (clé `nav.profile`, déjà utilisée pour cette tab).
- Le « Registre » du PDF est le registre des agents (`HubPage.Agents`) ; les Archives restent une page ouverte par un second bouton du Profil. Le badge des Archives (`MetaOverview.ArchivesBadge`) s’affiche sur l’onglet Profil.

### Page `ProfilePage` (`Presentation/Hub/Pages/Profile/`)

Page de type liste (`ListPagePresenter`), rafraîchie par `MetaChanged`, `WalletChanged` et `RosterChanged`.

- **En-tête** : pseudo (`OnboardingSave.pseudo`, « Invité » si vide), héros choisi (Agent D ou M, `OnboardingSave.hero`), niveau de pass, tag personnel.
- **Tag** : `Pseudo#NNNN`. `NNNN` = 4 chiffres (0000 à 9999) déduits de l'identifiant du joueur par un hachage stable (le même identifiant donne toujours le même tag). Bouton « Copier mon tag » : copie dans le presse-papiers.
- **Statistiques** (rangées) : combats gagnés ; combats perdus ; zones validées ; agents possédés ; plus haut niveau d'agent ; meilleur lien (victoires ensemble de la paire la plus avancée) ; objets fabriqués ; Crédits gagnés au total.
- **Amis** : titre de section, une rangée par ami (nom, tag, état « en attente de connexion en ligne »), bouton Retirer ; bouton « Ajouter un ami » qui ouvre une saisie de tag.
- **Navigation** : deux rangées « Registre des agents » (→ `HubPage.Agents`) et « Archives » (→ `HubPage.Archives`), dans l’ordre navigation, statistiques, section Amis, amis ; les Paramètres restent accessibles par le bouton existant du bandeau.

### Statistiques : stockage

`MetaSave` gagne `List<CounterSave> stats` (compteurs `combatsWon`, `combatsLost`, `creditsEarned`). `MetaTracker` les incrémente (victoire / défaite de combat ; Crédits gagnés hors récompenses versées par `MetaContext.Grant` comme aujourd'hui pour les missions). Les autres valeurs sont déduites du profil à l'affichage : zones validées (`CompletedZones`), agents possédés (`Roster`), plus haut niveau (`Roster`), meilleur lien (`TeamLinks`), objets fabriqués (`Meta.crafted`).

### Amis : port

- `IFriendsPort` (`Application/Ports/`) : `IReadOnlyList<FriendEntry> Friends()`, `Result Add(string tag)`, `Result Remove(string tag)`. `FriendEntry { string Tag; string Name; }`.
- Validation de `Add` (domaine) : format `Nom#NNNN` (nom de 1 à 20 caractères sans `#`, 4 chiffres) ; refus de son propre tag, des doublons (insensible à la casse) et au-delà de 50 amis ; erreurs `friends.invalid`, `friends.self`, `friends.duplicate`, `friends.full`.
- Adaptateur simulé `LocalFriendsPort` : liste de tags dans `MetaSave.friends` (`List<string>`), sans vérification d'existence (impossible hors ligne). L'état « en attente » s'affiche pour tous.
- Saisie du tag : fenêtre de saisie réutilisant le champ de texte de l'écran de pseudo de la première connexion.

## 2. Boutique en cartes

- Les onglets restent : Boosters, Objets du jour, Gemmes et crédits, Cadeau ; « Héros » continue d'ouvrir la page Recruter.
- `BoutiquePageView` affiche les rangées sous forme de **cartes en grille** (même vue que l'inventaire : `CardGridPageView`) : grande illustration, titre, description courte, étiquette de prix, bouton d'achat. `BoutiquePagePresenter` garde ses données, ses règles d'achat et ses actions ; il renseigne `InfoRowData.Art` pour chaque offre.
- En-tête allégé : solde de gemmes et bouton du cadeau du jour. Le texte « achats simulés » n'est plus affiché.
- Illustrations : les objets du jour utilisent `AgentArt.Item`. Les autres offres reçoivent une **icône SVG unique** (booster, paquets de gemmes, échange en Crédits, échange en PR, repos complet, cadeau du jour) rasterisée en PNG dans `Assets/_Project/Resources/Illustrations/Shop/<id>.png`, chargée par `AgentArt.Shop(id)`. Le SVG source est conservé dans `03_Art_UI_UX/Illustrations/Shop/`. Sans fichier, le jeu garde le repli actuel (pastille de lettre).

## 3. Salons sur la carte de l'agent

- **Fiche d'agent** (`MemberSheetPresenter` / `MemberSheetView`) : bouton **Soigner** (−30 points de fatigue de cet agent, 50 Crédits ; grisé avec la raison si l'agent n'a pas de fatigue ou si les Crédits manquent). L'entraînement (bouton existant) reste. Les deux boutons de salle (Salon, Infirmerie) sont retirés.
- **Cas d'usage** `IHealMemberUseCase` / `HealMemberUseCase(MemberId)` : dépense `MetaRules.HealMemberCost` (50), retire `MetaRules.InfirmaryReduction` (30) de fatigue à l'agent, publie le changement de Crédits et d'effectif, demande une sauvegarde. Erreurs : agent inconnu, rien à soigner (`HubErrors.NothingToHeal`), Crédits insuffisants.
- **Page Équipe** : bouton « Soigner toute l'équipe » (cas d'usage existant `IHealRosterUseCase`, 150 Crédits).
- **Suppressions** : `HubPage.Salon`, `HubPage.Infirmary`, les présentateurs et vues `LoungePage`, `SalonPage`, `InfirmaryPage`, `MemberRowView`, leurs prefabs et recettes de génération, leurs textes et leurs tests. `HubPageFactory` et `HubShellPresenter` (astuce du tutoriel « infirmerie ») sont mis à jour.
- **QG** : une seule carte « Équipe et soins » (écart acté à l’exécution) ouvre l’onglet Équipe (`HubPage.Team`, déjà triable par fatigue) ; son badge compte les agents fatigués. Aucun nouveau filtre.
- **Tutoriel** : les astuces qui nomment l'infirmerie ou le salon renvoient à la fiche d'agent.

## 4. Erreurs et cas limites

- Tag d'ami invalide, doublon, propre tag, liste pleine : message d'erreur, rien n'est enregistré.
- Un profil ancien (sans `stats` ni `friends`) se charge : compteurs à 0, aucun ami.
- Une icône manquante ne bloque pas la boutique.
- Soin d'un agent sans fatigue ou sans Crédits : refusé, rien n'est dépensé.
- Une sauvegarde ouverte sur une page supprimée (Salon, Infirmerie) ne doit pas bloquer : la navigation retombe sur le QG.

## 5. Tests

- Domaine / application : calcul du tag (stable, 4 chiffres, formats), validation des amis (formats, doublon, soi-même, 50 maximum), `LocalFriendsPort` (aller-retour de sauvegarde, ancien profil), `HealMemberUseCase` (coût, réduction, bornes à 0, refus), compteurs `combatsWon` / `combatsLost` / `creditsEarned`.
- Présentation : page Profil (rangées de statistiques, amis, boutons, copie du tag), boutique (offres en cartes avec leur art et leur action, règles d'achat inchangées), fiche d'agent (bouton Soigner), navigation (l'onglet Profil, plus de page Salon ni Infirmerie, cartes du QG).
- PlayMode : captures de la page Profil, de la boutique et de la fiche d'agent.

## Revue finale — points d'attention

- Aucune référence restante à `HubPage.Salon` / `HubPage.Infirmary` (code, prefabs, tests).
- Les achats de la boutique se comportent comme avant (boosters, objets, gemmes, échanges, cadeau).
- Le badge des Archives apparaît bien sur l'onglet Profil.
- Un profil sans pseudo ni héros choisi affiche quand même un tag valide.
