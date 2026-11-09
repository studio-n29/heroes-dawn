# Heroes Dawn — Aube tactique sur l'ensemble de l'interface

La suite fonctionnelle du 5 novembre est documentée dans le [rapport de refonte](../Refonte_UI_2026-11-05.md) et la [revue du jeu](../Revue_Jeu_2026-11-05.md). Les planches ci-dessous restent les intentions de conception.

4 novembre 2026. Direction choisie explicitement par l'utilisateur : Aube tactique. Cette livraison décline la DA en maquettes, dans la continuité de la demande précédente ; elle ne modifie pas le runtime Unity.

## Livrables

21 planches de quatre vues, soit 84 vues de conception, certaines représentant des états d'un même écran. Galerie : index.html. Chaque planche est conservée séparément en PNG. Prompts exacts : prompts.json et prompts-complements.json. L'outil intégré imagegen est utilisé avec la planche approuvée comme référence de style.

## Charte fixée

- Marine encre #101C29, ivoire chaud #ECE7DC, laiton solaire #D6AF5B.
- Panneaux mats opaques ; filets fins ; un coin coupé ; étoile The Dawn à quatre branches.
- Une action principale ivoire, danger rouge, états secondaires moins contrastés. L'or sélectionne et signe l'identité, sans remplacer toute la signalétique.
- Portraits cadrés sur le visage ; illustrations urbaines peintes ; pouvoirs colorés localisés.
- Corps de texte sans empattement et sans contour ; titres affirmés, valeurs alignées.
- Navigation du hub : QG / Équipe / Pass / Boutique / Profil. Absente des missions et mini-jeux.
- Familles : icône + couleur, Matière ambre, Élémentaire cyan, Physique/Bio rose, Perception violet. La couleur d'un pouvoir ne définit pas sa famille.

## Périmètre vérifié

Catalogue des prefabs locaux : 4 Screens, 20 Pages dont la page technique WipPage, 24 Popups. Vérification supplémentaire des enums HubPage, SettingsCategory, ShopTab, HeroGameKind et des recettes d'activités. Les anciennes vues Salon/Infirmerie ne sont pas recréées comme pages autonomes : les soins se font depuis Équipe/fiche d'agent dans la version documentée actuelle.

### Correspondance aux pages et écrans

| Source | Planche |
|---|---|
| Login | 01, 02 |
| Hub / Qg | 03 |
| Board | 07 |
| Combat | 08 |
| Team / MemberSheet / Agents | 04 |
| Loadout / Inventory / Grimoire | 05 |
| Operations / Lab / Archives | 03 (Lab est présenté comme Atelier de fabrication) |
| CityMap / Chapters | 06 |
| Recruit / Boutique | 10 |
| Pass / Profile / Lair | 12 |
| Tournament | 13 |
| Settings : huit catégories | 14–15 |
| WipPage | 21 |

### Correspondance aux fenêtres et états

| Source | Planche |
|---|---|
| AccountPopup / ProgressChoicePopup | 02 ; récupération 21 |
| AffinityPopup | 05 |
| IntroPopup / ChapterPopup | 01 et 06 : langage d'introduction et de chapitre |
| CombatIntroPopup | 08 |
| CombatResultsPopup / ZoneCompletePopup | 09 |
| DialogueBox / DialoguePopup / NarrativePopup | 09 et 19 |
| RestPopup | 07 |
| RecruitConfirmPopup / BoosterOpeningPopup | 11 |
| FriendAddPopup | 12 |
| ConfirmPopup / QuestLogPopup / PlaytestFormPopup | 16 |
| TutorialOverlay / toasts | 13 ; notifications contextuelles dans les autres vues |
| MiniGame2DPopup : huit jeux | 17–18 |
| PotionPopup / StagePopup : pêche, arène, survie | 19 ; arène/survie partagent le même HUD |
| HeroGamePopup : rempart / course des toits | 20 |
| MiniGamePopup : timing / ActivityPopup technique | 21 |

L'interface technique des activités partage le gabarit commun. Les variantes de texte et chaque permutation possible d'état ne constituent pas des écrans distincts à dessiner. Le questionnaire playtest est représenté graphiquement uniquement, aucun envoi n'a été effectué.

## Lecture des planches

Ordre des quatre vues : haut gauche, haut droite, bas gauche, bas droite. La galerie affiche les intentions fonctionnelles exactes sous les images. En cas d'écart de génération, le comportement existant et ces intentions priment ; aucun changement des règles du jeu n'est demandé.

### 01 — Entrée et lancement

Image : 01-entree.png

1. Titre : peinture panoramique New York reconstruite à l'aube, logo Heroes Dawn et étoile, Jouer principal, Se connecter secondaire, langue discrète.
2. Premier lancement : choix Français / English, consentements et Continuer, panneau uni sur illustration assombrie.
3. Chargement : illustration urbaine narrative, barre de progression fine et astuce courte.
4. Introduction cinématique : Paris Supernova en illustration dramatique, sous-titres lisibles, Passer discret ; ne pas inventer de scène 3D jouable.

### 02 — Compte et sauvegarde

Image : 02-compte.png

1. Connexion : e-mail, mot de passe, Se connecter, Google, Apple, mot de passe oublié, onglet créer.
2. Création de compte : pseudo, e-mail, mot de passe, Créer mon compte ; champs espacés.
3. Choix de progression : deux cartes Cet appareil / Le compte, niveau, crédits, date, choisir explicitement, Plus tard.
4. Erreur de connexion : même formulaire avec message court Hors connexion, Réessayer, Continuer en invité ; pas de nouvelle mécanique.

### 03 — QG et gestion

Image : 03-qg-gestion.png

1. QG New York : cinq tuiles illustrées Atelier, Opérations, Grimoire, Inventaire, Registre ; prochaine mission Red Hook dominante ; activités du jour explicites sans lettres B ni ! seules.
2. Salle des opérations : missions en liste, patrouille sélectionnée, agents disponibles, durée, récompense, Envoyer.
3. Atelier de fabrication : recettes filtrées, composants requis, détail illustré à droite, file de fabrication et Fabriquer.
4. Archives : chapitres et scènes débloquées en liste, illustration et texte de lore à droite ; Relire.

### 04 — Équipe et agents

Image : 04-equipe-agents.png

1. Équipe : grille de portraits cadrés sur visages, filtres familles, escouade de quatre à droite, fatigue et Soigner toute l'équipe.
2. Fiche Giulia Rossi : grand portrait gauche cheveux bruns télékinésie magenta, Matière icône ambre, niveau 5, PV 1168 ATQ 150 DÉF 79 VIT 105, fatigue30%, compétences4 objets3, Améliorer, Soigner, Dossier.
3. Dossier d'agent : portrait et identité Giulia, récit casino Manhattan et pouvoir télékinétique dans court texte lisible, progression du dossier.
4. Registre des agents : portraits possédés et silhouettes non recrutées clairement distingués, filtre par famille, détail sélectionné et collection.

### 05 — Cartes et équipement

Image : 05-collections.png

1. Grimoire : grille de cartes illustrées, filtre Matière Élémentaire Physique/Bio Perception, carte sélectionnée et description à droite.
2. Équipement de Giulia : objets disponibles et trois emplacements, stats comparées avant/après, Équiper, Retirer.
3. Inventaire : objets et consommables illustrés en grille, quantité, fiche sélectionnée à droite, Utiliser.
4. Affinités : portrait de Giulia et agents liés par relations claires, bonus de combo lisible, détails sélectionnés ; pas d'arbre de compétences inventé.

### 06 — Mission et chapitres

Image : 06-mission.png

1. Carte New York : arrondissements géographiques illustrés, Brooklyn sélectionné, zones et statuts, objectifs à droite, Déployer.
2. Fiche de zone Red Hook : illustration des quais, niveau conseillé, trois objectifs, récompenses, mode normal/difficile, Préparer.
3. Préparation : quatre agents sélectionnés, réserve filtrable, fatigue, combos possibles, consommables et Déployer.
4. Chapitres : huit entrées New York/Moscou avec En cours Terminé Verrouillé, résumé illustré du chapitre choisi, Relire.

### 07 — Plateau et exploration

Image : 07-plateau.png

1. Plateau attente de lancer : vraie carte vue de dessus Red Hook canal et chemins de cases rondes, brouillard, objectifs compacts à gauche, portraits quatre en bas gauche, dé et Lancer en bas droite.
2. Plateau choix de destination : même carte, cases atteignables cerclées d'or et coûts PM, chemin prévisualisé, compteur 4 PM, Terminer.
3. Fiche d'une case : panneau contextuel sur carte voilée, Combat élite, menace et récompense, Continuer ; conserver la carte visible.
4. Repos : fenêtre sur plateau, fatigue avant/après des quatre agents, Se reposer / Continuer, conséquences courtes.

### 08 — Combat

Image : 08-combat.png

1. Introduction du combat : quatre portraits alliés contre deux ennemis identifiés, bande VS discrète sur quai Red Hook, difficulté, Continuer.
2. Combat normal : cartes alliées gauche quatre visages, ennemis droite deux cartes, frise temporelle fine en haut, compétences quatre en bas centre, Auto x2 Pause, centre pour effets, pas d'arène 3D.
3. Ciblage : même duel de cartes, carte Projection sélectionnée, cible ennemie cerclée d'or, consigne Touchez une cible, annulation ; aucun bouton Confirmer cible.
4. Boss phase2 : même duel de cartes, grand portrait boss AMIF Titan côté droit avec barre PV et Phase 2, alliés gauche, compétences bas ; pas de nouvel ennemi géant 3D au centre.

### 09 — Narration et résultats

Image : 09-narration-resultats.png

1. Dialogue : agent au premier plan devant quais, court sous-titre sur panneau opaque et bouton Continuer, environnement reste visible.
2. Événement narratif : illustration de civils bloqués aux quais, texte court et trois choix aux conséquences lisibles ; aucune bulle cartoon.
3. Résultats de combat : Victoire, crédits et objets illustrés, XP des quatre agents, fatigue avant/après, Continuer.
4. Zone sécurisée : trois objectifs cochés, récompenses, mode difficile débloqué, prochaine zone, Retour au QG.

### 10 — Boutique et recrutement

Image : 10-boutique.png

1. Recrutement onglet Héros : trois cartes d'agents du jour avec portrait cohérent, famille et coût PR, fiche sélectionnée, Recruter.
2. Boutique Boosters : deux packs illustrés, contenu affiché dans fiche droite et prix explicite, acheter ; nav QG Équipe Pass Boutique Profil.
3. Boutique Gemmes et crédits : grille de ressources illustrées, prix précis, pas de bonus inventé ni compte à rebours promotionnel.
4. Boutique Cadeau : paquet gratuit du jour sélectionné, contenu indiqué, Réclamer, état déjà réclamé secondaire.

### 11 — Révélations et récompenses

Image : 11-recompenses.png

1. Confirmation recrutement : portrait agent, coût PR, solde après achat, Annuler / Recruter ; panneau sur boutique voilée.
2. Booster fermé : paquet portant étoile The Dawn au centre, Touchez pour ouvrir, fond sobre et halo limité.
3. Révélation booster : trois cartes illustrées distinctes face visible, raretés sur bordure seulement, Nouveau sur une carte, Suivant.
4. Résumé booster : cartes obtenues en rangée, noms et quantités, Continuer ; pas de rouleau casino.

### 12 — Pass et profil

Image : 12-progression-social.png

1. Pass saisonnier : piste gratuite et premium alignées par paliers, palier actuel centré, Réclamer, missions quotidiennes à droite.
2. Profil : Agent D, tag AgentD#1234, stats en grille, compte Invité, Lier mon compte, liste amis, Ajouter.
3. Ajouter un ami : saisie du tag, exemple AgentM#5678, Ajouter / Annuler, état de recherche lisible sans inventer messagerie.
4. Repaire : ennemis découverts Brick / AMIF Titan / Crocodile en liste et fiche boss avec progression et lore.

### 13 — Tournoi et tutoriel

Image : 13-tournoi-tutoriel.png

1. Tournoi en cours : classement lisible à gauche, manches et état à droite, Continuer ; pas de modes ou classement mondial inventé.
2. Tournoi terminé : classement final, récompenses et progression, Retour au QG.
3. Tutoriel au QG : portrait de Brie Rayner en guide, bulle rectangulaire opaque deux lignes, projecteur autour Mission sans couvrir bouton, 3/9, Passer discret.
4. Tutoriel combat : temps figé, une compétence illuminée, instruction courte Touchez une compétence, portrait guide, Passer ; vrai combat de cartes derrière.

### 14 — Réglages — 1

Image : 14-reglages-a.png

1. Paramètres Audio : catégories latérales, Musique Effets Voix Interface, curseurs et valeurs, Réinitialiser.
2. Paramètres Graphismes : qualité Auto Élevée, 30/60 IPS, batterie, texte court expliquant choix.
3. Paramètres Accessibilité : taille du texte avec aperçu, daltonisme icône+couleur, réduction flashs/secousses, temps étendu, sous-titres.
4. Paramètres Langue : Français English avec sélection évidente, exemple texte, retour.

### 15 — Réglages — 2

Image : 15-reglages-b.png

1. Paramètres Notifications : activation générale puis entraînement expéditions recherche et pass, heures calmes 22h–8h.
2. Paramètres Contrôles : commandes tactiles, sensibilité carte et zoom, panneau secondaire raccourcis PC ; mobile reste priorité.
3. Paramètres Compte : statut invité ou compte lié, synchronisation, confidentialité, déconnexion, supprimer séparé rouge.
4. Support et aide : tutoriel, fiches d'aide, support, identifiant copiable, version, crédits.

### 16 — Fenêtres communes

Image : 16-fenetres.png

1. Pause sur plateau : panneau centré Reprendre, Paramètres, Aide, Retour au QG, contexte zone et progression.
2. Confirmation quitter : Retour au QG ?, conséquence courte progression enregistrée, Continuer la mission principal / Retour au QG secondaire.
3. Journal de quêtes : objectifs principaux et quotidiens, progrès chiffrés, récompenses et Réclamer.
4. Questionnaire playtest : trois questions notées 1à5, commentaire, Envoyer / Plus tard ; décor neutre.

### 17 — Mini-jeux — 1

Image : 17-minijeux-a.png

1. Analyse ADN : grand champ tactile molécules vertes et bleues, score temps vies en tête, consigne courte, Abandonner discret.
2. Le rythme : trois pistes ennemis descendant vers ligne d'appui, score combo temps, grandes zones tactiles.
3. Caisse au choix : trois caisses, jauge de charge maintenir/relâcher, score temps et Abandonner.
4. Engrenage : réseau de roues avec sens clairs et source à connecter, toucher pour inverser, score temps en haut.

### 18 — Mini-jeux — 2

Image : 18-minijeux-b.png

1. Tir franc : terrain 2D latéral panier et ballon, jauge puissance grande, geste maintenir relâcher, score temps.
2. Infiltration : terrain top-down 2D avec gardes cônes de vision, agent et joystick tactile gauche, sortie repérée.
3. Ingénierie de secours : manuel d'indices gauche, composants tactiles droite, assemblage en trois emplacements, Valider.
4. Survie en neige : terrain top-down 2D neige et totems, agent et joystick, jauge chaleur temps, consigne courte.

### 19 — Activités

Image : 19-activites.png

1. Potion : livre d'indices gauche, ingrédients et quantités droite, étapes de préparation, Préparer ; activité existante pas boutique.
2. Dialogue à choix : réplique d'un personnage, trois réponses, réaction et progression ; présentation commune des activités.
3. Pêche : scène jouable existante présentée comme viewport central, jauge de pêche et commande tactile, score et temps, interface Aube tactique.
4. Arène / survie : viewport de scène existante avec joystick gauche et deux boutons tactiles droite, choix agent, PV/temps ; distinguer de combat principal à cartes.

### 20 — Défis de héros et états

Image : 20-heros-etats.png

1. Rempart : mini-jeu 2D héros interceptant projectiles sur couloirs, flèches tactiles gauche/droite, Interceptés 3/14, vies et temps.
2. Course des toits : mini-jeu 2D latéral héros sauvant civils entre toits, contrôles tactiles et Civils sauvés 2/10, temps/vies.
3. Introduction de mini-jeu : illustration du défi Analyse ADN, une phrase de geste, objectif et Démarrer / Abandonner.
4. Fin de mini-jeu : Défi réussi, score, récompense existante illustrée et Continuer ; panneau commun adaptable à échec sans nouvelle mécanique.

### 21 — Compléments

Image : 21-complements.png

1. Mini-jeu de réflexe
2. Activité générique de test
3. Fonction bientôt disponible
4. Récupération du mot de passe

## Passage en production

Ces images servent à décider de l'assemblage, de la composition et des matières. Ne pas rasteriser une interface entière : reconstruire avec les composants Unity, conserver les illustrations séparées et utiliser des panneaux découpables. Réutiliser le catalogue actuel et ses recettes, puis les composants communs de Theme, ScreenKit et ShellRecipes.

Cibles à vérifier sur appareil : texte courant 16–18 unités logiques, zones tactiles au moins 48, marges sûres 16:9 et 20:9, contraste 4,5:1 pour le corps, états sélectionné/verrouillé/vide/erreur distingués autrement que par couleur. Ces cibles ne sont pas certifiées par les illustrations générées. Les mini-jeux gardent leurs commandes, les combats gardent leur duel de cartes, et le ciblage se fait sans étape de confirmation ajoutée.

## Limites de fidélité

La génération peut varier les visages secondaires, costumes, niveaux, prix, noms de cartes, famille d'un agent et quelques libellés. Toute valeur doit être reprise des données actuelles ; aucun prix dessiné n'est une décision économique. Les scènes illustrées sont des propositions de composition et ne créent pas de nouveau canon. The Dawn reste The Dawn, étoile à quatre branches. Les membres fondateurs ne deviennent pas des recrues jouables. Ne pas ajouter de fonctionnalité suggérée accidentellement par une image.

Sources consultées : catalogue local Prefabs/Screens, Pages et Popups ; recettes Unity ; Theme.cs ; documents de contexte Heroes Dawn et dossier DA précédent. L'analyse initiale n'était pas un test interactif de tous les états du jeu. L'intégration Unity décrite ci-dessous a ensuite modifié les composants et les prefabs.


## Notes de relecture des images

Les planches sont des maquettes de composition ; les corrections ci-dessous sont nécessaires avant intégration :

- Retirer « Aube tactique » des sous-titres internes au jeu : c'est le nom de la direction artistique, pas un nouveau sous-titre de Heroes Dawn.
- Reprendre les portraits de chaque agent depuis ses illustrations réelles : les visages secondaires répétés dans les grilles ne constituent pas de nouvelles recrues.
- Conserver la couleur de famille actuelle de chaque agent, indépendamment de la couleur dessinée de son pouvoir. Certaines familles des personnages secondaires varient dans les images.
- Reprendre intégralement les textes narratifs depuis le lore. En particulier, la Supernova modifie la génétique mais l'activation des pouvoirs survient ensuite lors d'un choc ; le dossier de Giulia ne doit pas affirmer une activation immédiate pendant la Supernova. Sa présence dessinée dans une composition de Paris n'établit aucun fait canon.
- Conserver la navigation du hub sur toutes ses pages, même si la génération l'omet sur certains détails. Les boutons Retour des sous-pages ne la remplacent pas.
- La fiche de progression doit indiquer clairement quelle sauvegarde sera retenue ; les niveaux et dates dessinés sont des exemples.
- Les coûts de soin et d'amélioration doivent rester visibles avant action. Les illustrations omettent parfois ces montants.
- Les compétences, bonus d'affinité et recettes dessinés n'introduisent aucune modification de règles. Remplacer les noms et effets par les données du jeu.

## Intégration Unity

Voir le [rapport de livraison](Rapport_Integration_Unity.md) pour les assets, les validations et les limites.

La direction a ensuite été intégrée dans le projet Unity à la demande de l'utilisateur. Voir `Code/docs/specs/2026-11-04-aube-tactique-ui.md` pour le système de composants et `Code/docs/test-reports/aube-tactique/index.html` pour les captures réelles. Le rapport d'intégration final est fourni dans ce dossier. Les planches présentes restent les concepts de conception, distincts du rendu du jeu.
