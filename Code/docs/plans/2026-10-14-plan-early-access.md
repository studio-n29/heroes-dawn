# Plan d'implantation : de la 0.9.0 à l'Early Access (14 novembre) et à la sortie (14 décembre 2026)

> Source des dates : document Drive « Heroes Dawn • Early Access » (objectif : version early access pour un playtest réel jusqu'à la sortie ; **deadline 14 novembre**, **sortie prévue 14 décembre** ; priorités : **Analytics** et **contenu MVP**).
> Rédigé le 2026-10-14 (mercredi), **mis à jour le 2026-10-23 (section 0)**. Le 14/11 est un samedi, le 14/12 un lundi. Il reste **31 jours** avant l'Early Access et **61 jours** avant la sortie.
> Répartition : **Moi (Claude)** = code, outils, infrastructure, tests, builds. **Vous** = écriture, assets finaux, comptes et textes légaux (actions que je ne peux ou ne dois pas faire à votre place). Le plan d'illustration est dans `03_Art_UI_UX/04_Pipeline_Illustrations_IA.md`.


## 0. Mise à jour du 2026-10-23 : ce qui a changé depuis le 14 octobre

**Nouveau canon (2026-10-22).** L'organisation du joueur est **The Dawn** ; le joueur choisit **Agent D** (New York) ou **Agent M** (Moscou) ; 30 agents numérotés + 10 cryptides de Salem ; 4 actes (New York, Paris, Moscou, Salem) ; cinématique de lancement de 46 plans. Documents : `02_Lore_Narration/` ; décisions : `05_Production/Decisions.md`.

### Livré depuis le plan initial
| Chantier | État | Version |
|---|---|---|
| Lore et documents (The Dawn, histoire, agents, cinématique) | Fait | n/a |
| 30 agents numérotés dans le jeu (familles, rôles, raretés, obtention) | Fait | 0.10.0 |
| Choix Agent D / Agent M, équipe de départ par parcours, acte en cours | Fait ; **Agent M indisponible** (pas de zones à Moscou) | 0.10.0 |
| Combat : nouvelle disposition, énergie, synergie à trois couches, 2 compétences de base uniques, pause de résolution et animations par famille | Fait | 0.10.0 |
| Logo, icône (corrigée sur Android), key arts, cinématique d'intro provisoire | Fait | 0.10.1 |
| Réalignement des documents de game design | Fait | n/a |

Build disponible : `HeroesDawn-0.10.1.apk` (Drive, dossier `_builds`).

### Décisions à prendre (vous)
| # | Décision | Recommandation |
|---|---|---|
| D7 | **Agent M à l'Early Access** : créer des zones à Moscou avec l'outil de level design, ou livrer Agent D seul (carte « Agent M, bientôt ») | Livrer **Agent D seul** à l'EA et annoncer Agent M pour la sortie ; créer 2 à 4 zones de Moscou pendant la boucle d'analytics, puis passer `available` à vrai dans `StoryRoutes.M` |
| D8 | **Contenu de Paris, Moscou et Salem** : hors périmètre de l'EA (seuls les 4 zones de Brooklyn sont jouables) | Hors EA ; la sortie se concentre sur Brooklyn + Paris si le temps le permet |
| D9 | **Équilibrage de départ** (énergie 30 / +3 par pas, pause de résolution 1,3 s, paliers de lien 3-8-15-25, déséquilibre des familles) | Valider en playtest ; réglable sans code (`BalanceConfig`, outil Compétences) |

### Ce que cette mise à jour change dans le périmètre (section 3)
- **Agents** : 32 au lieu de 30 (Agent D et Agent M) ; **compétences** : 64 (2 par agent) au lieu de 90, donc moins d'icônes à produire ; icônes par **famille d'animation** (11).
- **Portraits, icônes, fonds de zones** : toujours à produire ; le fichier `03_Art_UI_UX/Prompts/prompts_illustrations.csv` est regénéré pour les nouveaux agents.
- **Cinématique d'intro** : 9 illustrations provisoires déjà intégrées (banque `03_Art_UI_UX/Illustrations/`).
- Les tâches des chantiers WS-A (analytics), WS-C (builds, Steamworks), WS-D (légal) et le calendrier de la section 4 **sont inchangés** ; la version numérotée **0.10.0 prévue le 23/10 par ce plan est dépassée** : la 0.10.1 contient déjà le nouveau contenu, l'analytics reste à faire.
## 1. Ce que ce calendrier change par rapport à la roadmap
La roadmap (`05_Production/Roadmap.md`) visait l'Early Access en juillet-août 2027 et le mobile en septembre-octobre 2027. Le nouveau calendrier comprime **9 mois en 1** : M2 (vertical slice), l'infrastructure de M3 et l'essentiel de M4 (analytics, légal) doivent tenir en un mois. Ce n'est tenable qu'en **réduisant le périmètre** :

| Hors périmètre Early Access (reporté après le 14/12) | Pourquoi |
|---|---|
| Achats réels (gemmes, pass premium) | Complexité légale et technique ; un playtest n'en a pas besoin |
| Économie serveur complète (Economy, anti-triche, rejeu de combats) | Les contrôles légers suffisent pour un playtest |
| 5 langues, doublage | Français pour l'EA, anglais pour la sortie si le temps le permet |
| Comptes e-mail (Supabase) | Compte invité UGS suffisant |
| Notifications push, iOS, villes 2 à 5, PvP | Hors lancement |

## 2. Décisions à prendre (vous) avant le vendredi 16 octobre
Rien en aval ne peut être planifié proprement sans elles. Ma recommandation est indiquée.

| # | Décision | Recommandation |
|---|---|---|
| D1 | **Canal de distribution de l'Early Access** | **Windows** via **Steam Playtest** si votre compte Steamworks est utilisable à temps, sinon **itch.io** (accès restreint par clé) ; **APK Android** en parallèle pour les testeurs mobiles. Un vrai « Early Access Steam » exige une page boutique publique plusieurs semaines avant et une revue de build : trop risqué pour le 14/11, réaliste pour le 14/12 si la page est publiée avant le **30/11** |
| D2 | **Périmètre de contenu MVP** (section 3) | Brooklyn (4 zones) finalisé + 32 agents avec portraits + icônes par famille d animation. Agent D jouable ; Agent M selon la décision D7 |
| D3 | **Backend** | **Unity Gaming Services seul** (Authentication anonyme, Cloud Save, Remote Config, Analytics). Supabase reste prévu après la sortie |
| D4 | **Langues** | EA en français ; framework de localisation posé dès l'EA ; anglais à la sortie |
| D5 | **Direction des illustrations** | Style unique « comics peint, lumière crépusculaire éclaircie par la palette pastel du HUD » (détail dans le pipeline) |
| D6 | **Monétisation à la sortie** | EA : aucun achat réel. Sortie : à décider d'ici le 14/11 d'après les analytics |

### Actions à lancer cette semaine (délais externes que je ne peux pas accélérer)
- **Vous** : lier le projet Unity à un projet UGS (Dashboard) avec environnements `dev` et `production` (le `cloudProjectId` des ProjectSettings est vide aujourd'hui).
- **Vous** : ouvrir le compte **Steamworks** (identité, fiscalité, banque, droit d'inscription) et créer l'application ; sans l'AppID je livre l'intégration avec un identifiant provisoire.
- **Vous** : ouvrir le compte développeur **Google Play** si l'Android est dans le périmètre de la sortie. Pour un compte personnel récent, Google impose un test fermé avec un minimum de testeurs pendant 14 jours avant l'accès à la production (conditions à vérifier dans la Play Console) : sans démarrage du test fermé avant le **30/11**, l'Android du 14/12 est compromis.
- **Vous** : rédiger la politique de confidentialité et les conditions d'utilisation (héberger sur une page publique) ; je les branche dans le jeu.

## 3. Périmètre de contenu MVP proposé (D2), à valider
« Contenu MVP » dans votre document : voici ce que le code supporte déjà et ce qu'il faut pour l'EA. Vous écrivez et produisez ; je fournis le canal d'intégration (section 5, WS-E).

| Élément | Existe | Porte EA (14/11) | Porte sortie (14/12) |
|---|---|---|---|
| Zones jouables | 4 (Brooklyn) | 4 avec narration et fonds définitifs | 4 + éventuellement 2 à 4 zones (Queens) si le temps le permet |
| Agents | 32 (30 numérotés + Agent D et Agent M), 2 compétences de base chacun | 32 avec portraits illustrés (repli : silhouette) | 32 + bios courtes |
| Compétences | 64 (2 par agent) + cartes du grimoire | Icônes par **famille d animation** (11) + 14 statuts | 64 icônes uniques |
| Objets | 10 | 10 icônes | 10 |
| Cinématique d intro | 9 écrans, **illustrations provisoires intégrées (2026-10-23)** | 9 illustrations définitives (ou retouche des provisoires) | idem + musique ; cinématique de 46 plans (`02_Lore_Narration/05_Cinematique_Lancement.md`) hors EA |
| Key arts | KA01 et KA02 intégrés (2026-10-23) | KA01 et KA02 + logo et icône définitifs (faits) | KA03 à KA06 |
| Quêtes secondaires | 6 (compteurs) | Inchangées | Scénarisées |
| Audio | 4 thèmes, ~67 effets | Jingles victoire/défaite | Thèmes de zone, boss |
| Assets de la page boutique | aucun | Capsules et captures si page Steam | idem |

## 4. Calendrier
| Semaine | Dates | Objectif | Jalon de sortie (vendredi) |
|---|---|---|---|
| S1 | 14 → 18 oct. | Décisions, schéma d'analytics, comptes, contrat d'assets | **V1 (ven. 18)** : décisions D1 à D6 actées, schéma validé, comptes créés |
| S2 | 19 → 25 oct. | Analytics + consentement, build Windows, chargement des illustrations | **V2 (ven. 23)** : version **0.10.0**, événements visibles dans le Dashboard |
| S3 | 26 oct. → 1er nov. | Auth + sauvegarde cloud, Steamworks, audit des assets, interfaces restantes | **V3 (ven. 30)** : version **0.11.0**, joueur invité persistant dans le cloud |
| S4 | 2 → 8 nov. | **Test fermé n°0** (5 à 10 personnes), Remote Config, perfs, légal intégré | **V4 (lun. 2)** : test fermé lancé ; **ven. 6** : correctifs |
| S5 | 9 → 14 nov. | Gel du code, candidates de build, tests de fumée, publication | **Mer. 11** : gel du code · **jeu. 12** : build candidate · **ven. 13** : tests finaux · **sam. 14** : **Early Access** (version **0.12.0**) |
| S6 à S9 | 15 nov. → 12 déc. | Boucle hebdomadaire d'analytics (section 6) | **Ven. 4 déc.** : candidate de sortie · **lun. 7** : go / no-go · **lun. 14** : **sortie 1.0.0** |

## 5. Chantiers et tâches

Légende : **C** = Claude, **V** = Vous. Effort en jours de travail effectif pour moi (estimation).

### WS-A Analytics et consentement (priorité n°1)
| # | Tâche | Qui | Échéance | Critère d'acceptation |
|---|---|---|---|---|
| A1 | Schéma d'événements `04_Tech_Unity/10_Analytics.md` : entonnoir `Live_Ops.md` (installation → connexion → prologue étapes 1-5 → 1er recrutement → zone 1 → Acte 1 → 1er achat), identifiant de build, plateforme, âge du profil | C | 16/10 | Validé par vous ; chaque KPI de `Live_Ops.md` a son événement source |
| A2 | Port `IAnalytics` (Application/Ports) + `NullAnalytics` + `UgsAnalytics` (Infrastructure) + passerelle `AnalyticsBridge` abonnée aux messages MessagePipe existants (Combat, Flow, Session, Zone) | C (2 j) | 21/10 | Tests : chaque message clé produit l'événement attendu ; aucun appel direct dans les présentateurs |
| A3 | Écran de consentement RGPD au premier lancement + bascule dans Paramètres ; aucun événement avant consentement ; âge déclaré | C (1 j) | 23/10 | Test : sans consentement, zéro événement émis ; refus conservé après redémarrage |
| A4 | Événements d'erreur et de performance (exceptions non gérées, images/s moyennes, temps de chargement) ; rapport de plantage via un SDK gratuit (Sentry ou équivalent, à valider) | C (1 j) | 4/11 | Un plantage provoqué volontairement apparaît dans le tableau de bord |
| A5 | **Vous** : tableaux de bord UGS (entonnoir, rétention J1/J7/J30, durée de session) ; je fournis la liste des requêtes à créer | V + C | 30/10 | Les cibles de `Live_Ops.md` sont lisibles sans export |
| A6 | Bouton « Donner mon avis / signaler un bug » (Paramètres ▸ Support) : formulaire externe pré-rempli (build, joueur, écran) + événement | C (1 j) | 30/10 | Un testeur envoie un retour en 3 touchers |

### WS-B Backend et comptes (UGS)
| # | Tâche | Qui | Échéance | Critère |
|---|---|---|---|---|
| B1 | Lier le projet UGS, créer `dev` et `production` | V | 16/10 | `cloudProjectId` renseigné ; je peux déployer |
| B2 | `UgsAuthGateway : IAuthGateway` (connexion anonyme persistante) ; le pseudo narratif reste local | C (1 j) | 28/10 | Test : même joueur après réinstallation avec le même identifiant |
| B3 | `CloudProfileGateway : IProfileGateway` : sauvegarde Cloud Save + cache local, version de schéma, migration, résolution de conflit (le plus récent gagne), mode hors ligne | C (3 j) | 30/10 | Tests : migration 0.9→0.10, conflit, hors ligne ; jamais de perte de profil |
| B4 | Déploiement Remote Config (`balance.rc` + drapeaux : multiplicateurs de récompenses, coupe-circuit boutique/boosters, version minimale) | C (1 j) | 4/11 | Une valeur changée dans le Dashboard modifie le jeu sans nouveau build |
| B5 | Heure serveur (Cloud Code `GetServerTime` déployé) pour pool quotidienne de 9h, missions du jour et chronos | C (1 j) | 4/11 | Changer l'heure du PC ne donne pas de récompense anticipée |
| B6 | Écran « mise à jour requise » si la version est inférieure à la version minimale de Remote Config | C (0,5 j) | 6/11 | Test : version trop basse bloque l'entrée en jeu |

### WS-C Builds et plateformes
| # | Tâche | Qui | Échéance | Critère |
|---|---|---|---|---|
| C1 | Script de build reproductible (méthode éditeur appelable en ligne de commande) : Windows x64 et Android, numéro de build auto, sortie dans `_deploy/` | C (1 j) | 21/10 | Une commande produit les deux builds versionnés |
| C2 | Cible PC : fenêtre redimensionnable 16:9, plein écran, souris et clavier, réglages graphiques, écran de sortie | C (2 j) | 23/10 | Parcours complet jouable à la souris, sans erreur de console |
| C3 | Steamworks.NET (initialisation, overlay, nom Steam comme pseudo par défaut), script de dépôt SteamCMD | C (1,5 j) | 30/10 | Build lancé via Steam ; repli propre hors Steam (itch.io) |
| C4 | Android : signature (clé que **vous** générez et sauvegardez), API cible exigée par Google Play, arm64, poids réduit (musiques `.wav` à passer en Vorbis) | C (1 j) + V (clé) | 30/10 | APK/AAB signé, sensiblement plus léger que 89 Mo |
| C5 | Passe de performance : budget 60 images/s PC, 30 mini Android milieu de gamme, mémoire, temps de chargement | C (2 j) | 6/11 | Rapport chiffré dans `docs/test-reports` |
| C6 | Version affichée dans les Paramètres + notes de version | C (0,5 j) | 23/10 | |

### WS-D Localisation (socle seulement pour l'EA)
| # | Tâche | Qui | Échéance | Critère |
|---|---|---|---|---|
| D1 | Port `ILocalizer` + tables de textes (`Content/texts.json` existe déjà) ; sélecteur de langue | C (1,5 j) | 6/11 | Un texte migré change selon la langue |
| D2 | Outil d'audit : recense les chaînes françaises en dur dans `Presentation` (251 fichiers) et les migre écran par écran, du plus vu au moins vu | C (3 j, étalé) | 4/12 | Rapport de couverture par écran ; 100 % des écrans du parcours principal |
| D3 | **Vous** : traduction anglaise (ou relecture si IA) | V | 4/12 | Fichier importé ; polices vérifiées |

### WS-E Intégration de vos assets (le canal pour votre travail de contenu)
Ce chantier garantit que **déposer un fichier suffit** : aucune retouche de code par asset.

| # | Tâche | Qui | Échéance | Critère |
|---|---|---|---|---|
| E1 | Contrat d'assets (noms, tailles, dossiers) publié dans `03_Art_UI_UX/04_Pipeline_Illustrations_IA.md`, section 6 | C | 16/10 | Validé par vous |
| E2 | Chargeurs avec repli : portraits d'agents (`PortraitView.Art`), icônes de compétences (cartes de combat aujourd'hui des lettres B, O, R), icônes d'objets, fonds de zones, 9 écrans d'intro (`Resources/Intro/intro_01..09`), key arts | C (3 j) | 23/10 | Déposer `Portraits/<idAgent>.png` remplace la silhouette partout (équipe, boutique, combat, fiche, registre) |
| E3 | Fenêtre `Heroes Dawn ▸ Contenu ▸ Audit des assets` : manquants, mal nommés, mauvaise taille, couverture en % par catégorie ; réglages d'import automatiques (compression, atlas) | C (2 j) | 28/10 | Le tableau de bord indique à tout moment ce qu'il reste à produire |
| E4 | Registre de provenance (outil, date, licence, prompt) généré à l'import, base de la déclaration IA Steam | C (1 j) | 30/10 | Un CSV par asset, exporté dans `docs/` |
| E5 | Audio : import en Vorbis, normalisation de volume, jingles de victoire et de défaite branchés ; thèmes de zone via le catalogue | C (1 j) | 4/11 | |
| E6 | Fonds de zones, outil de level design : prise en charge de N zones et des actes dans `Campaign.asset` (2 à 4 zones supplémentaires si vous en produisez) | C (1 j) | 30/10 | Ajouter une zone ne demande aucun code |

### WS-F Jeu : ce qui reste côté gameplay
| # | Tâche | Qui | Échéance | Critère |
|---|---|---|---|---|
| F1 | Nouvelles interfaces objets, agents, camp d'entraînement (prévues en 0.9.0) | C (4 j) | 4/11 | Parcours équipement et entraînement sans écran provisoire |
| F2 | Mise à jour des 5 activités et de l'interface du QG | C (2 j) | 6/11 | Captures avant/après validées |
| F3 | Quêtes secondaires pilotées par données (vous écrivez, l'outil de narration les charge) | C (1 j) | 30/10 | |
| F4 | Passe d'équilibrage : simulateur sur les 4 zones, courbe de difficulté, économie d'un jour ; paramètres exposés dans Remote Config | C (2 j) | 6/11 | Rapport `BalanceReports` ; un joueur moyen termine la zone 1.1 en moins de 10 minutes |
| F5 | Tutoriel et 10 premières minutes : mesurer puis réduire les points d'abandon (à partir de l'étape 4 : analytics) | C | continu dès 2/11 | Prologue > 85 % (cible `Live_Ops.md`) |
| F6 | Correctifs issus des playtests | C | continu | Aucun bug bloquant à l'ouverture |

### WS-G Légal et conformité
| # | Tâche | Qui | Échéance | Critère |
|---|---|---|---|---|
| G1 | Politique de confidentialité et conditions d'utilisation (texte et page publique) | V | 30/10 | URL fournie |
| G2 | Liens dans le consentement et les Paramètres, écran de crédits (Kenney CC0, Barlow OFL, bibliothèques MIT) | C (1 j) | 4/11 | |
| G3 | « Supprimer mes données » (suppression du profil cloud) + adresse de support | C (1 j) | 6/11 | Test de bout en bout |
| G4 | Déclaration d'usage d'IA (Steam), questionnaire d'âge (IARC/PEGI), fiche Play Data safety | V (je prépare les réponses à partir du registre de provenance) | 30/11 | Cases de `Obligations_Legales.md` cochées |
| G5 | Vérifications restantes : licence commerciale des musiques Google Flow, noms « Heroes Dawn » (marque) et « Adam West » | V | 6/11 | |

### WS-H Qualité et publication
| # | Tâche | Qui | Échéance | Critère |
|---|---|---|---|---|
| H1 | Test de fumée PlayMode « boucle complète » : démarrage → intro → pseudo → tutoriel → zone 1.1 → combat → résultats → QG, avec vérification des événements d'analytics | C (1,5 j) | 30/10 | Exécuté à chaque build, rapport HTML |
| H2 | Les 742 tests restent verts ; chaque ajout porte un `[Description]` | C | continu | 0 échec |
| H3 | **Test fermé n°0** : 5 à 10 personnes, build 0.11.0, pour vérifier que les données arrivent et que la sauvegarde cloud tient | V (recrute) + C | lancé le 2/11 | 100 % des testeurs visibles dans le Dashboard ; retours consignés |
| H4 | Gel du code mercredi 11/11 ; build candidate jeudi 12/11 ; tests finaux vendredi 13/11 ; publication samedi 14/11 | C + V | 11-14/11 | Liste de contrôle de sortie (section 7) cochée |
| H5 | Canal de retour (serveur Discord ou formulaire) et ligne de conduite de réponse | V | 30/10 | Lien intégré au bouton A6 |

## 6. Phase « Early Access » (15 novembre → 12 décembre)
Cadence hebdomadaire fixe :
| Jour | Activité |
|---|---|
| Lundi | Revue des analytics contre `Live_Ops.md` : rétention, entonnoir, durée de session, abandons par zone ; liste des 3 problèmes principaux |
| Mardi - jeudi | Correctifs et réglages. Les valeurs d'équilibrage passent par **Remote Config** (sans nouveau build) |
| Vendredi | Build de correction (0.12.x), notes de version, annonce aux testeurs |
| Chaque semaine | Livraison de **votre** contenu (zone, illustrations) par le canal WS-E, sans retouche de code |

Jalons de sortie : **vendredi 4/12** candidate 1.0.0 ; **lundi 7/12** go / no-go ; **7 → 11/12** soumission des builds et revues des plateformes ; **lundi 14/12** sortie.
Critères de go indicatifs (à valider d'après les données de l'EA) : taux de sessions sans plantage ≥ 99 %, aucun bug bloquant ouvert, prologue terminé par plus de 85 % des testeurs actifs, sauvegarde cloud sans perte constatée, conformité (G4) cochée, page boutique publique depuis au moins deux semaines (donc avant le 30/11).

## 7. Liste de contrôle de sortie de l'Early Access (13 novembre)
- [ ] Build Windows et APK signés, version 0.12.0, installables sur une machine vierge
- [ ] Consentement et événements vérifiés dans le Dashboard ; entonnoir complet lisible
- [ ] Connexion invitée et sauvegarde cloud : réinstallation sans perte
- [ ] Remote Config actif, coupe-circuit testé, version minimale testée
- [ ] 742+ tests verts, test de fumée vert, aucune erreur de console dans le parcours
- [ ] Contenu MVP (section 3) intégré, rapport d'audit des assets sans élément manquant bloquant
- [ ] Politique de confidentialité et liens en jeu ; suppression de données fonctionnelle
- [ ] Canal de retour en ligne ; notes de version et guide testeur rédigés
- [ ] Sauvegarde du dépôt, clé de signature Android sauvegardée hors du poste

## 8. Risques
| Risque | Effet | Parade |
|---|---|---|
| Délais Steamworks et revues de plateforme | Pas de distribution le 14/11 ou le 14/12 | Lancer les comptes cette semaine ; repli itch.io ; page Steam publique avant le 30/11 |
| Compte Google Play récent (test fermé obligatoire) | Android impossible le 14/12 | Test fermé lancé avant le 30/11 ou Android hors sortie |
| Volume d'assets (30 portraits, 20 à 90 icônes, 9 illustrations) en 4 semaines | Contenu MVP incomplet | Pipeline IA du document dédié ; repli par silhouettes et icônes génériques (déjà géré par les chargeurs avec repli) |
| 251 fichiers de présentation en français en dur | Anglais incomplet à la sortie | Migration étalée, priorité aux écrans du parcours principal ; FR seul à l'EA |
| Perte de données joueur lors du passage au cloud | Perte de confiance des testeurs | Migration testée, cache local, test fermé n°0 |
| Mesures insuffisantes pour décider | Go / no-go à l'aveugle | Schéma validé dès le 16/10, test fermé n°0 le 2/11 |
| Un seul contributeur | Aucune marge si indisponibilité | Gel du périmètre après V2 ; tout ajout retire une autre tâche |

## 9. Règle de pilotage
Tout ajout de périmètre après le **vendredi 23 octobre** doit retirer une tâche de poids équivalent. Le plan est mis à jour dans ce fichier chaque vendredi, avec les écarts constatés.
