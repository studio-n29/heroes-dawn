# Nouvelle interface — lot 1 : coque et hub

> Statut : design validé en conversation le 2026-10-04 · à relire avant le plan d'implémentation.
> Branche : `feature/ui-hub`, partie de `integration/tout-dans-unity` (écrans MVP en prefabs, boutique éphémère, audio).
> Cible visuelle : `03_Art_UI_UX/03_Maquettes_Screen_Flow.md` et `03_Art_UI_UX/Maquettes/ecrans/` (M01-M85). Spec des maquettes : `Code/docs/specs/2026-10-04-maquettes-ui-tutoriel-design.md`.
> Références : `04_Tech_Unity/07_Presentation_Flux.md`, `09_Tests_Outils_CI.md`, spec de la boutique éphémère.

## 1. Intention
- **Demande :** intégrer la nouvelle interface (maquettes haute fidélité), mettre en **WIP** les fonctions pas encore développées, et ajouter de nouveaux **feedbacks**.
- **Le tout est découpé en lots.** Ce lot (1) fait la **coque et le hub**. Les lots suivants : mode mission (M60-M72), Paramètres complets (M80-M85), tutoriel (T01-T12), vraies fonctions des pièces / Pass / Boutique.
- **Choix validés :**
  - lot 1 = coque + hub (QG, Équipe, Recruter), avec Pass, Boutique et pièces en WIP ;
  - **architecture A** : une scène Hub, une coque en prefab, des pages en prefabs avec un navigateur C# pur ;
  - la **boutique éphémère** (3 héros par jour à 9h00, tirage pondéré, renouvellement gratuit puis payant) **remplace** la pool de 5 cartes de M30 ; elle est habillée comme M30, avec la confirmation M31 ;
  - **Salon et Infirmerie** fonctionnent en **version simple** (entraînement et soin instantanés d'aujourd'hui, dans le style des maquettes) ; Opérations, Labo et Archives sont en WIP ;
  - le bouton **MISSION** lance la zone avec l'escouade actuelle, comme Déployer aujourd'hui ; la carte, la fiche de zone et la préparation (M60-M62) viennent avec le lot mission ;
  - les écrans de mission gardent leur aspect actuel.
- **Réussite :**
  1. Le hub a la barre de ressources (Crédits, PR, Gemmes, ⚙) et la barre de navigation (QG, Équipe, Recruter, Pass, Boutique), fidèles aux maquettes.
  2. Toutes les fonctions actuelles restent atteignables : entraîner, soigner, composer l'escouade, recruter, renouveler, lancer la mission, se déconnecter, effacer la sauvegarde.
  3. Chaque élément non développé est visible, marqué BIENTÔT, et ne mène à aucun cul-de-sac.
  4. Retour (bouton, Échap, geste Android) suit les règles des maquettes.
  5. Les feedbacks décrits en §6 sont en place et ne bloquent jamais un clic.
  6. Suites EditMode et PlayMode vertes, captures de chaque page comparées aux maquettes, APK validé sur téléphone.

## 2. Périmètre
**Dans le périmètre :** coque (barre de ressources, barre de navigation, zone de page), navigateur, pages QG / Équipe / fiche membre / Recruter + confirmation / Salon simple / Infirmerie simple / Paramètres (catégorie Compte fonctionnelle) / pages WIP, système WIP, feedbacks du hub, badge « N » de la boutique, textes, tests, documents.

**Hors périmètre :**
- mode mission (carte, fiche de zone, préparation, plateau, combat, résultats, pause) ;
- tutoriel et astuces contextuelles ;
- catégories de Paramètres autres que Compte ;
- vrais Pass, Boutique (achats), minuteurs des pièces, expéditions, Archives ;
- monnaie premium (Gemmes) : affichage en lecture seule ;
- vibration du téléphone, feedbacks de mission ;
- build publiable (clé de production).

## 3. Architecture
### 3.1 Coque
Prefab `HubShell` dans la scène Hub, comme pour les autres écrans (recette + `ScreenCatalog`) :
- **TopBar** : titre de la page courante, pastilles Crédits / PR / Gemmes (la pastille de ressource existante `ResourcePill` est réutilisée), bouton ⚙. Le bouton Retour apparaît en haut à gauche sur les pages empilées.
- **BottomNav** : 5 onglets (icône, libellé Bangers, badge), onglet actif en bloc jaune, hauteur et cibles ≥ 96 px, zones sûres de 88 px.
- **PageHost** : zone où la page courante est instanciée.

### 3.2 Navigateur
`HubNavigator` (C# pur, `Presentation/Hub/Navigation`) :
- `HubPage` : `Qg`, `Team`, `Recruit`, `Pass`, `Shop` (les 5 onglets), `Salon`, `Infirmary`, `Operations`, `Lab`, `Archives` (pièces), `MemberSheet`, `Settings`.
- `Current` (page + paramètre éventuel, ex. `MemberId`), `Stack`, `Open(page, arg)`, `SelectTab(tab)`, `Back()`.
- Règles : un onglet **remplace** la pile ; une pièce, une fiche ou les réglages **s'empilent** ; Retour dépile ; sur un onglet autre que QG, Retour va au QG ; sur le QG, Retour demande « Quitter le jeu ? » (confirmation) ; sur une modale, Retour annule la modale (`BackButtonRouter` existant).
- Le navigateur publie `PageChanged` pour la coque ; un `HubShellPresenter` relie coque, navigateur, `BackButtonRouter` et les badges.

### 3.3 Pages
Chaque page = un prefab (`Pages/…`) avec une vue `…PageView` et un présentateur `…PagePresenter` (C# pur, `PresenterBase`), créés à l'ouverture et détruits à la sortie. L'état utile (filtre, tri, onglet) vit dans le navigateur ou la session, pas dans la vue. `ScreenCatalog` liste les pages. Les présentateurs sont enregistrés dans le scope Hub par une fabrique par page.

### 3.4 WIP
- `WipCatalog` : liste unique des fonctions en WIP (`WipFeature` : identifiant, clé de texte « ce qui est prévu », page de la maquette).
- Composant `WipMark` : bandeau « BIENTÔT » en biais, élément grisé ; toucher = toast « … arrive bientôt ».
- `WipPage` (prefab commun) : titre, aperçu (image de la maquette ou illustration), liste de ce qui est prévu ; utilisé par Pass, Boutique, Opérations, Labo, Archives.
- Gemmes : pastille à 0, `WipMark` ; toucher = « Bientôt ».

## 4. Pages
| Page | Contenu |
|---|---|
| **QG (M10)** | 5 pièces en cases de BD. Salon et Infirmerie : infos réelles (« N membres à entraîner / fatigués »). Opérations, Labo, Archives : BIENTÔT. Missions du jour : WIP. Alertes : compte à rebours de la boutique réel + objectifs secondaires en WIP. Fil du QG : réplique de Rayner donnant l'objectif de la zone en cours. **MISSION** (rouge, bas droite) lance la zone. |
| **Équipe (M20)** | Grille de cartes : filtres par famille (icône + couleur), tri (niveau, rareté, fatigue). Escouade de 4 places à droite, composée ici ; combos formés par l'escouade ; toucher une carte ouvre la fiche. Bouton « Préparer une mission » = même action que MISSION. |
| **Fiche membre (M21)** | Stats, compétences (descriptions), combos par famille, flèches membre précédent / suivant, raccourcis Salon et Infirmerie, bouton **Entraîner · N CR** à la place du palier. Illustration en pied : WIP (portrait actuel). La même fiche s'ouvre pour un héros de la boutique (bouton **Recruter**). |
| **Recruter (M30 adapté)** | Règle en une phrase, compte à rebours, 3 cartes à cadre de rareté (cuivre / argent / or), Légende surélevée avec halo, prix en PR sur le bouton, « Dans l'équipe » si recruté, progression « N / 8 », **Renouveler** (gratuit, puis prix en Crédits). Recruter → **confirmation M31** : solde avant, coût, solde après (ou manque en rouge + piste pour en gagner), bouton désactivé si insuffisant. Badge « N » sur l'onglet tant que la boutique du jour n'a pas été vue. |
| **Salon (simple)** | Membres triés par niveau, bouton Entraîner · N CR par ligne (cas d'usage existant), mention « version simple ». |
| **Infirmerie (simple)** | Membres triés par fatigue (jauge verte < 60 %, orange au-delà avec libellé, rouge à 100 %), bouton Soigner · 150 CR (cas d'usage existant), mention « version simple ». |
| **Paramètres (⚙)** | Catégories à gauche. **Compte** : Se déconnecter, Effacer la sauvegarde (confirmation) — ces deux boutons quittent l'en-tête du QG. Audio, Graphismes, Accessibilité, Notifications, Contrôles : WIP avec le contenu prévu. |
| **Pass, Boutique, Opérations, Labo, Archives** | `WipPage`. |

## 5. Données et sauvegarde
- `ShopSave` gagne `seenDay` (jour de boutique ouvert pour la dernière fois sur l'onglet Recruter) ; ancien profil : vide = non vu. Compatible avec les profils actuels (champ ajouté, valeur par défaut). Le badge « N » = `Shop.Day != seenDay`.
- Nouveau cas d'usage `IMarkShopSeenUseCase` (met `seenDay` à jour, sauvegarde).
- Aucun changement d'équilibrage ; textes dans `Fr` de `Loc.cs`. Les combos d'une escouade se calculent depuis `ContentCatalog.Combos` (familles A / B), sans nouvelle donnée.

## 6. Feedbacks
Dossier `Presentation/Feedback`, sur le moteur d'animations existant (`Tween`), en temps réel (indépendant du ralenti), annulable à la destruction, **jamais bloquant** (aucun verrou d'entrée hors modales).
- **Boutons** (`UiFeedback`) : appui = réduction + rebond ; survol (souris) = léger grossissement + halo jaune ; bouton grisé = secousse + toast qui dit pourquoi.
- **Navigation** : page = fondu + léger glissement ; cascade des cartes (décalage de 40 ms) ; bloc d'onglet actif qui glisse ; badges qui pulsent doucement.
- **Ressources** : compteur qui « roule » jusqu'à la nouvelle valeur (`CounterRoll`, pur) ; pastille flottante « −450 » rouge / « +150 » vert ; punch de la pastille.
- **Recrutement** : éclat aux couleurs de la rareté + son d'équipe, tampon « Dans l'équipe » qui s'écrase, cartes qui se retournent au renouvellement, halo de Légende qui pulse.
- **Équipe** : carte survolée qui se soulève avec halo à la couleur de famille ; rebond d'une carte qui entre ou sort de l'escouade.
- **Accessibilité** : interrupteur interne `ReducedMotion` (secousses, flashs, éclats) actif par défaut ; sera relié aux Paramètres au lot suivant.
- **Sons** : via `IAudioService` et `UiSoundRelay` existants.

## 7. Erreurs et cas limites
| Cas | Comportement |
|---|---|
| Retour sur le QG | « Quitter le jeu ? » (annulable) ; ne quitte jamais sans confirmation. |
| Retour pendant une modale | La modale se ferme ; la page dessous ne bouge pas. |
| Onglet touché pendant une animation | La navigation reste prioritaire ; l'animation en cours est annulée proprement. |
| Fiche d'un membre introuvable (offre expirée) | Retour à la page précédente + toast « La boutique s'est renouvelée. » |
| Achat refusé (PR, boutique expirée) | Toast localisé existant ; la confirmation se ferme et se rafraîchit. |
| Page détruite pendant un appel asynchrone | Aucune exception ni appel à la vue après `Dispose` (comme les présentateurs actuels). |

## 8. Tests (écrits avant le code, `[Description("Vérifie que … — parce que …")]`)
- **Navigateur** : onglet remplace la pile ; pièce / fiche / réglages s'empilent ; Retour à chaque niveau ; « Quitter le jeu ? » depuis le QG ; Retour sur modale.
- **WIP** : chaque `WipFeature` a un texte localisé ; aucun bouton des pages du hub sans action ni `WipMark` ; toucher un WIP = toast, sans navigation.
- **Présentateurs** : coque (badges, titre, boutons), QG (infos réelles, MISSION lance la zone), Équipe (filtre, tri, escouade, combos), fiche (membre possédé / de la boutique, flèches), Recruter (3 cartes, états, calcul de M31, solde insuffisant, renouvellement), Salon, Infirmerie, Paramètres ▸ Compte.
- **Logique pure** : `CounterRoll`, délais de cascade, filtre `ReducedMotion`, style de rareté, calcul des combos d'escouade, tri et filtre de l'effectif.
- **Garde-fous de prefabs** : cibles tactiles ≥ 96 px sur les pages du hub, textes ≥ 20 px (existant), clés de texte connues (existant), toutes références branchées (existant).
- **Sauvegarde** : `seenDay` conservé, profil sans `seenDay` chargé.
- **PlayMode** : parcours au clic par la barre de navigation, Retour depuis une pièce et depuis un onglet, toucher un WIP, achat via la confirmation, MISSION lance la zone ; `FullRunTests` adapté à la nouvelle navigation.
- **Captures** : une par page ; comparaison une à une avec les maquettes avant relecture ; validation du toucher sur téléphone avec le nouvel APK.

## 9. Documents
`04_Tech_Unity/07_Presentation_Flux.md` (coque, navigateur, pages, WIP, feedbacks), `09_Tests_Outils_CI.md`, `05_Production/Decisions.md`. À remettre à l'utilisateur sans les écrire : adaptation de M30 à 3 cartes (la maquette montre 5 cartes), et liste des écarts entre maquettes et jeu (Gemmes, pièces en version simple).

## 10. Ordre de livraison (indicatif)
1. Socle : navigateur, coque, pages vides, système WIP.
2. QG, Équipe, fiche membre.
3. Recruter, confirmation, badge.
4. Salon, Infirmerie, Paramètres ▸ Compte, pages WIP.
5. Feedbacks.
6. Parcours de bout en bout, captures, documents, APK.
