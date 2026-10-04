# Maquettes & screen flow

> Rendu cible du **jeu complet** en haute fidélité (DA comics, référence *Secret Wars* 2015), paysage 1920×1080, et du **tutoriel guidé** du premier lancement.
> Spec : `Code/docs/specs/2026-10-04-maquettes-ui-tutoriel-design.md`. Les pastilles jaunes numérotées renvoient aux légendes.
> Sources : `Maquettes/_sources` (kit HTML/CSS, une page par écran). Export : `pwsh export.ps1` (Edge headless) ; vérification : `node check.js --strict` ; cette page : `node build-doc.js` ; prototype cliquable : `Maquettes/_sources/index.html`.
> Portraits provisoires (extraits de l'illustration clé). Les anciennes maquettes du M1 (wireframes W, maquettes S) sont archivées dans `Maquettes/_archive_M1`.

## 1. Principes

- **Menus bien séparés** : un menu = un onglet de la barre de navigation basse (QG, Équipe, Recruter, Pass, Boutique), toujours au même endroit, avec ses badges. Les 5 pièces s'ouvrent depuis le QG.
- **Mode mission plein écran** : de la carte aux résultats, pas de barre de navigation ; retour en haut à gauche, ⏸ ouvre la pause (Reprendre, Paramètres, Aide, Retour au QG).
- **Placement constant** : retour en haut à gauche ; titre à côté ; ressources puis ⚙ en haut à droite ; action principale en bas à droite (zone du pouce).
- **Une action principale par écran** (jaune, ou rouge pour MISSION et les actions dangereuses) ; actions secondaires sombres ; prix et conséquences écrits sur les boutons.
- **Lisibilité** : Bangers pour titres, boutons et chiffres ; Comic Neue pour tout texte de plus de 4 mots ; 20 px minimum ; couleur jamais seule porteuse de sens (famille = icône + couleur, état = libellé).
- **Tactile d'abord** : cibles ≥ 96 px, 24 px d'écart, zones sûres de 88 px sur les côtés ; raccourcis PC affichés en pastilles quand l'option est active.
- **Retour** (bouton, geste Android, Échap) : pièce → QG, onglet → QG, QG → « Quitter le jeu ? », mission → étape précédente, combat → pause, modale → annuler.

## 2. Tutoriel : principes didactiques

- **Apprendre en faisant** sur la vraie zone 1.1 : chaque étape demande un geste réel ; une notion à la fois, au moment où elle sert ; 2 lignes de texte maximum.
- **Guide** : Brie Rayner en médaillon, bulle de BD, récitatif jaune pour le titre de l'étape, progression « n / 9 ».
- **Projecteur** sur l'élément à toucher (le reste est voilé et bloqué) + main animée ; étapes d'explication validées par « Compris ».
- Combat figé pendant une bulle ; étape enregistrée dans le profil (reprise après un départ) ; « Passer » avec confirmation ; « Revoir » dans Paramètres ▸ Support & aide.
- Ensuite, **astuces contextuelles** non bloquantes à la première rencontre de chaque mécanique (combo, AUTO et vitesse, dé bonus, Terminer, cases Événement / Mini-jeu / Repos, défaite, fatigue > 60 %, zone sécurisée, recrutement, infirmerie).

## 3. Entrée

### M01 — Titre / connexion

![M01](Maquettes/ecrans/M01-titre.png)

1. Logo-titre sur l'illustration clé (KA01 : l'escouade sur un toit de Brooklyn face à la brèche) ; voile sombre en bas pour la lisibilité.
2. JOUER, seule action principale, dans la zone du pouce : connexion invité automatique, puis QG (ou tutoriel au premier lancement).
3. État de connexion en clair : invité ou compte lié ; un compte lié reprend la progression sur tous les appareils.
4. Lier un compte (Google, Apple ou Steam selon la plateforme) : conseillé dès la fin du tutoriel.
5. Langue toujours accessible avant de jouer (FR, EN, ES, PT, DE).
6. Liens légaux, crédits et version (utile au support).

### M02 — Premier lancement

![M02](Maquettes/ecrans/M02-premier-lancement.png)

1. Langue choisie en premier (détectée depuis l'appareil, modifiable) : tout le reste s'affiche ensuite dans cette langue.
2. Consentement RGPD en langage simple, une seule fois, au tout premier lancement ; rappel de l'endroit où le modifier.
3. Seul le traitement facultatif est un interrupteur ; le nécessaire est expliqué mais pas désactivable.
4. Personnaliser : détail des traitements et des partenaires (écran Compte & confidentialité, M85).
5. Action principale : valide et lance le chargement puis le tutoriel (T01).

### M03 — Chargement

![M03](Maquettes/ecrans/M03-chargement.png)

1. Astuce tirée des fiches d'aide déjà vues ou à venir : le chargement apprend quelque chose au lieu de faire attendre.
2. Étape en cours en clair et progression chiffrée ; au-delà de 10 s, message « Connexion lente… » et bouton Réessayer.

## 4. Onglet QG

### M10 — QG · Section New York

![M10](Maquettes/ecrans/M10-qg.png)

1. Barre haute commune à tout le hub : titre de l'écran, Crédits, PR et Gemmes (toucher = d'où vient la ressource, lien boutique), ⚙ Paramètres.
2. Les 5 pièces du QG, chacune une case de BD avec son nom, son niveau et ce qui s'y passe ; toucher = entrer dans la pièce.
3. Badges : rouge = action en attente (entraînement fini, expédition revenue), jaune = ressource à collecter.
4. Missions du jour (XP du pass), réclamables d'un geste ; toucher = onglet Pass.
5. Alertes : objectifs secondaires temps réel et compte à rebours de la prochaine vague ; toucher = carte filtrée.
6. Fil du QG : une réplique de Rayner (ou d'un mentor) qui donne le prochain objectif de l'histoire.
7. MISSION : l'action principale, seule en rouge, sous le pouce droit ; rappelle la prochaine zone de l'histoire.
8. Barre de navigation : un menu = un onglet (QG, Équipe, Recruter, Pass, Boutique), avec badges ; elle reste à la même place sur tous les écrans du hub.

### M11 — Salon d'entraînement · Niv. 3

![M11](Maquettes/ecrans/M11-salon.png)

1. Retour (bouton, geste Android, Échap) : ramène toujours au QG ; la barre de navigation reste visible, l'onglet QG reste actif.
2. Entraînement terminé : le seul bouton jaune de la pièce récupère le niveau gagné.
3. Entraînement en cours : XP, minuteur, accélération en Gemmes (prix affiché sur le bouton).
4. Emplacement libre : ouvre l'effectif trié par niveau pour y placer un membre.
5. Emplacement verrouillé : grisé, avec le niveau de pièce qui le débloque.
6. Objets placés et bonus d'XP cumulé ; lien vers les objets de la boutique.
7. Amélioration de la pièce : effet et durée du niveau suivant, coût en Crédits.

### M12 — Infirmerie · Niv. 2

![M12](Maquettes/ecrans/M12-infirmerie.png)

1. Lit occupé : fatigue qui descend en temps réel, multiplicateur de l'infirmerie, minuteur restant.
2. Lit libre : toucher = choisir un membre (le plus fatigué est proposé en premier).
3. Trousse de soin : consommable utilisable sans lit ni attente.
4. Effectif trié par fatigue décroissante. Jauge verte sous 60 %, orange au-delà (malus affiché en clair), rouge à 100 % (indisponible) : la couleur est toujours doublée d'un libellé.
5. Tri par fatigue, niveau ou famille.

### M13 — Salle des opérations · Niv. 2

![M13](Maquettes/ecrans/M13-operations.png)

1. Expédition terminée : gains et fatigue affichés, un geste pour récupérer (badge sur le QG tant que ce n'est pas fait).
2. Expédition en cours : minuteur, heure de retour, progression.
3. Destination puis durée (1 h, 4 h, 8 h, 12 h ; les plus longues débloquées par le niveau de la pièce) : on remplit de haut en bas, étapes numérotées.
4. Membres : seuls les membres disponibles sont proposés, avec la règle rappelée en clair.
5. Aperçu des gains et de la fatigue avant l'envoi.
6. Envoyer : action principale, avec l'heure de retour sur le bouton.

### M14 — Labo de recherche · Niv. 2

![M14](Maquettes/ecrans/M14-labo.png)

1. Stock du labo en grand : quantité, capacité et jauge (couleur des PR).
2. Production horaire, temps avant saturation, capacité de stockage.
3. Avertissement en clair : au-delà de la capacité, la production s'arrête.
4. Collecter : action principale, le montant est écrit sur le bouton ; les PR volent vers la barre de ressources.
5. Explication de la ressource et raccourci vers son usage (Recruter) : le joueur comprend pourquoi collecter.
6. Amélioration de la pièce : effet du niveau suivant, durée et coût.

### M15 — Archives

![M15](Maquettes/ecrans/M15-archives.png)

1. Onglets de collection : recrues, lore, scènes débloquées, musiques, succès.
2. Progression de la collection de la ville (pool épuisée = collection terminée).
3. Cartes de collection ; les recrues pas encore rencontrées restent en silhouette « ??? ».
4. Détail de la carte sélectionnée : illustration, famille (icône + couleur), rareté, bio courte, lien vers la fiche complète (M21).

## 5. Onglet Équipe

### M20 — Équipe · 12 membres

![M20](Maquettes/ecrans/M20-effectif.png)

1. Filtres par famille (icône + couleur, jamais la couleur seule) et tri (niveau, rareté, fatigue).
2. Effectif en grille de cartes : rareté, nom, niveau, famille et fatigue d'un coup d'œil ; un membre occupé (salon, infirmerie, expédition) est grisé avec son occupation, un membre épuisé est signalé en rouge.
3. Pastille « E » : membre de l'escouade de mission. Toucher une carte = fiche complète (M21).
4. Escouade de mission (4 places) toujours visible à droite : on la compose ici, sans passer par la préparation.
5. Combos de l'escouade actuelle, pour aider à la composer.
6. Action principale : préparer une mission avec cette escouade (carte de la ville).

### M21 — Givre

![M21](Maquettes/ecrans/M21-fiche-membre.png)

1. Illustration en pied ; famille (icône + couleur) et rareté.
2. Flèches ou balayage à gauche/droite : membre suivant, sans revenir à la grille.
3. Statistiques au niveau actuel ; le malus de fatigue est affiché en orange avec une flèche (▼) et en clair sur la jauge.
4. Raccourcis vers le salon et l'infirmerie.
5. Compétences actives (A1–A3) et passive (P) : nom, losanges = paliers validés, description générée depuis les effets.
6. Prochain palier et validation en Crédits : seule action principale de la fiche.
7. Combos du membre avec chaque famille partenaire, effet résumé.

## 6. Onglet Recruter

### M30 — Recrutement · Pool du jour

![M30](Maquettes/ecrans/M30-recrutement.png)

1. La règle du recrutement en une phrase : prix fixe, pas de hasard, pas de doublon.
2. Compte à rebours jusqu'à la prochaine pool (badge « N » sur l'onglet à son arrivée).
3. Cinq cartes au format collection : rareté (cuivre, argent, or) en cadre et récitatif, portrait, famille (icône + couleur), prix en PR sur le bouton ; toucher la carte = fiche (M21).
4. La Légende, liée à l'histoire, est surélevée et entourée d'un halo doré ; son bouton est le seul jaune.
5. Carte déjà recrutée : grisée, marquée « Dans l'équipe ».
6. Renouvellement : 1 gratuit par jour, puis en Crédits, puis en Gemmes ; le prix suivant est écrit sur le bouton.
7. Progression de la collection de la ville.

### M31 — Recrutement · Pool du jour

![M31](Maquettes/ecrans/M31-confirmation-recrutement.png)

1. Modale de confirmation sur l'écran voilé : portrait, famille, rareté, rôle et combo phare pour décider en connaissance de cause.
2. Le calcul du solde est montré : solde actuel, coût, solde après achat (ou montant manquant en rouge).
3. Si le solde ne suffit pas, le jeu dit comment obtenir ce qui manque, au lieu d'un simple refus.
4. Bouton Recruter désactivé tant que le solde est insuffisant ; Annuler et Retour ferment la modale.

## 7. Onglet Pass

### M40 — Pass · Saison 1

![M40](Maquettes/ecrans/M40-pass.png)

1. Palier actuel, XP vers le suivant et temps restant de la saison (42 jours, 50 paliers).
2. Pass premium : l'avantage clé (rétroactif) et le prix sont écrits sur le bouton.
3. Piste gratuite : récompenses réclamées grisées avec ✔, palier courant encadré de jaune avec Réclamer, paliers suivants numérotés.
4. Marqueur du palier courant ; défilement horizontal, palier courant centré à l'ouverture.
5. Piste premium alignée par palier, verrouillée (cadenas) sans le pass : on voit ce qu'on gagnerait.
6. Missions du jour (500 XP par jour) réclamables ici comme sur le QG.

## 8. Onglet Boutique

### M50 — Boutique

![M50](Maquettes/ecrans/M50-boutique.png)

1. Onglets de la boutique : offres, Gemmes, PR, accélérateurs, cosmétiques (tenues, décor du QG, dés et pions).
2. Mise en avant : offre de bienvenue (une seule fois) avec son contenu détaillé et sa valeur.
3. Packs de Gemmes au bonus croissant (étiquette jaune) ; prix localisés par le store.
4. Achats en Gemmes : PR, accélérateurs, cosmétiques ; le prix est toujours écrit sur le bouton.
5. Mentions obligatoires et principe « pas de pay-to-win, pas de loot box » ; restauration des achats.

## 9. Mission

### M60 — New York · Acte 1

![M60](Maquettes/ecrans/M60-carte.png)

1. Arrondissement = acte ; les actes verrouillés sont tramés, grisés, avec un cadenas et leur numéro.
2. Zones de l'acte en cours : validée (grise), en cours (jaune), disponible (rouge, boss) ; toucher une zone = sa fiche (M61).
3. Pastille « ! » : objectif secondaire temps réel dans une zone déjà validée.
4. Liste des zones de l'acte : état écrit en clair, partie en pause signalée, boss annoncé ; règle d'ordre rappelée.
5. Objectifs secondaires avec compte à rebours.
6. Action principale : reprendre la zone en cours (ou la suivante).

### M61 — New York · Acte 1

![M61](Maquettes/ecrans/M61-fiche-zone.png)

1. Fiche de la zone par-dessus la carte : illustration, situation en 2 phrases, niveau conseillé et taille du plateau.
2. Objectifs, cochés s'ils sont déjà remplis.
3. Récompenses de validation, dont le déblocage du mode difficile.
4. Partie en cours : où on en est et avec quelle escouade.
5. Mode difficile, verrouillé jusqu'à la validation de la zone.
6. Reprendre (ou Commencer) mène à la préparation (M62).

### M62 — Préparation · Zone 1.2

![M62](Maquettes/ecrans/M62-preparation.png)

1. Escouade de 4 : portrait, famille, niveau, fatigue ; un membre au-dessus de 60 % est signalé (pastille orange) ; toucher = retirer.
2. Membres disponibles avec leur fatigue ; les indisponibles (épuisés, au salon…) restent visibles mais grisés avec la raison.
3. Filtres par famille.
4. Combos de l'escouade ; en surbrillance, ce qu'apporterait le membre sélectionné : aide à composer l'équipe.
5. Sac : consommables emportés, achat rapide en Crédits.
6. Déployer : action principale ; l'avertissement de fatigue est écrit juste au-dessus, sans bloquer.

### M63 — Zone 1.2 · Gowanus, le canal

![M63](Maquettes/ecrans/M63-plateau-lancer.png)

1. Mode mission : plus de barre de navigation, tout l'écran pour le plateau. Retour ou ⏸ ouvre la pause (M72) ; titre = zone en cours.
2. Objectifs de la zone (3), cochés même si le brouillard se referme ; l'objectif à suivre est encadré ; toucher = centrer la caméra dessus.
3. Pion de l'escouade (portrait du leader) ; cases colorées par type ET marquées d'un sigle (VS, !, MJ, $, Zz) ; cases déjà jouées grisées avec ✔. Glisser = déplacer la caméra, pincer = zoom.
4. Brouillard tramé : les cases au-delà du rayon de vision restent cachées (« ? »).
5. Escouade : portraits + fatigue ; toucher = fiche rapide.
6. Sac : consommables emportés (trousse, dé chargé, fusée éclairante…).
7. Dé bonus : 6 PM non utilisés = 1 dé (réserve de 2 max), jauge et compteur.
8. LANCER ! : l'action principale, énorme, sous le pouce droit ; raccourci Espace sur PC.

### M64 — Zone 1.2 · Gowanus, le canal

![M64](Maquettes/ecrans/M64-plateau-destination.png)

1. Destinations atteignables avec les PM du lancer : anneau jaune pointillé + coût en pas dans une pastille. Toucher une destination = s'y rendre (pas de bouton à chercher).
2. PM en main, en grand, avec la consigne en une ligne.
3. Terminer : les PM restants vont dans la réserve du dé bonus (l'effet est écrit sur le bouton).
4. Appui long (survol sur PC) : détail de la case (type, rang de combat, règle d'arrêt).
5. Chemin le plus court prévisualisé en pointillés jaunes ; s'il révèle du terrain, le pion s'arrête et peut repartir avec ses PM restants.

### M65 — Pillards du canal · Élite

![M65](Maquettes/ecrans/M65-combat-pret.png)

1. Barre du haut : Retraite (confirmation : défaite + recul d'une case), AUTO, vitesse ×1/×2/×3, pause ; raccourcis indiqués (A, S).
2. Ennemis : PV (barre épaisse, chiffrée) et jauge d'action ATB (fine, jaune) ; l'élite porte une étiquette, le boss un cadre doré. Silhouettes en attendant les illustrations.
3. Escouade en cases de BD inclinées : PV, ATB, fatigue ou bouclier ; l'allié prêt s'illumine en jaune et est sélectionné automatiquement ; touches 1-4.
4. Ses 3 compétences s'ouvrent : nom, effet court, recharge restante (grisée avec le compteur) ; touches Q/W/E.
5. Retours visuels : dégâts en chiffres encrés, « RATÉ », secousse de la cible.
6. Statuts de l'unité (icône + nom + durée), détaillés à l'appui long.

### M66 — Pillards du canal · Élite

![M66](Maquettes/ecrans/M66-combat-ciblage.png)

1. Compétence mono-cible choisie : elle reste encadrée de jaune pendant le ciblage.
2. Les cibles valides sont encadrées de jaune ; toucher = lancer ; toucher ailleurs, Retour ou Échap = annuler. La consigne est écrite sous l'escouade.
3. COMBO : apparaît quand deux alliés prêts forment une paire (ici Givre + Instant = Point faible) ; les deux consomment leur tour ; raccourci C.
4. AUTO : panneau de compétences masqué, l'IA joue (combos compris) ; la jauge ne s'arrête jamais (temps actif), seule la pause la fige.
5. Plusieurs alliés prêts : toucher un autre portrait prêt change la sélection (Tab sur PC).
6. Critique : éclat comics « CRIT ! » et chiffre jaune ; la cible tremble.

### M67 — Mini-jeux · 4 formats

![M67](Maquettes/ecrans/M67-mini-jeux.png)

1. Commun aux 4 formats : choix du membre (sauf Piratage), bonus de famille annoncé en vert, consigne en une ligne, « Ignorer » sans récompense, option « temps étendu » (accessibilité).
2. Démonstration : un curseur balaie la jauge ; l'arrêter dans la zone verte (élargie si la famille correspond) ; 3 essais, 2 réussites ; gros bouton STOP (ou Espace).
3. Piratage : faire pivoter les segments pour relier source et sortie en 30 s (+10 s avec un membre Élémentaire) ; le chemin déjà relié est coloré.
4. Poursuite : suite de 6 à 10 gestes chronométrés ; le geste attendu est en grand et en jaune ; 2 erreurs tolérées.
5. Intervention : chance de réussite de chaque membre affichée avant de confirmer.

### M68 — Zone 1.2 · Gowanus, le canal

![M68](Maquettes/ecrans/M68-evenement.png)

1. Illustration plein cadre façon case de comics (IA + retouche, charte DA).
2. Titre en lettrage Bangers dans un récitatif jaune.
3. Texte court (3-4 lignes max) dans un récitatif blanc ; s'il est plus long, il avance au toucher.
4. 2-3 choix, conséquence annoncée sous chacun (ou masquée « ??? » pour les choix de scénario) ; un choix lié à un membre présent porte son icône de famille.
5. Le membre qui parle : portrait en médaillon et bulle de BD ; sa réplique peut être doublée (🔊), avec sous-titres.

### M69 — Zone 1.2 · Gowanus, le canal

![M69](Maquettes/ecrans/M69-repos.png)

1. Popup de case par-dessus le plateau voilé (le plateau reste en pause), couleur et sigle du type de case.
2. Effet chiffré membre par membre avant de choisir : fatigue avant → après.
3. Les deux options expliquées en une phrase ; le choix conseillé est le bouton jaune. Popup non annulable (il faut choisir), comme toutes les cases.

### M70 — Résultats du combat

![M70](Maquettes/ecrans/M70-resultats-combat.png)

1. Bandeau Victoire / Défaite en éclat comics (défaite : « l'équipe recule d'une case »).
2. Récompenses qui volent vers la barre de ressources ; la vérification du combat se fait en fond (✔ quand c'est confirmé).
3. Escouade : fatigue avant → après chiffrée (+12 %, +15 % de plus en défaite), montée de niveau mise en avant.
4. Progression des objectifs de la zone, le nouvel objectif rempli en jaune.
5. Continuer : retour au plateau (ou au QG si toute l'escouade est épuisée, annoncé en clair).

### M71 — Zone sécurisée

![M71](Maquettes/ecrans/M71-zone-securisee.png)

1. Fin de zone : les 3 objectifs cochés, en rappel.
2. Récompenses de validation, en grand.
3. Déblocage du mode difficile, avec ce qu'il change.
4. Suite de l'histoire : la prochaine zone et son enjeu, pour donner envie de continuer.
5. Retour au QG : action principale ; le brouillard qui se referme est expliqué juste à côté.

### M72 — Zone 1.2 · Gowanus, le canal

![M72](Maquettes/ecrans/M72-pause.png)

1. Pause : rappel de l'état de la zone ; tout est figé (temps actif du combat compris). Reprendre est l'action principale, Retour la déclenche aussi.
2. Paramètres accessibles en pleine mission (son, accessibilité…).
3. Aide : les fiches d'astuces déjà vues, pour réviser une règle sans quitter la partie.
4. Retour au QG en rouge, séparé, avec sa conséquence écrite juste en dessous (pas de surprise).

## 10. Transverse

### M80 — Paramètres

![M80](Maquettes/ecrans/M80-parametres-audio.png)

1. Catégories toujours visibles à gauche, la catégorie ouverte en jaune ; Retour revient à l'écran d'où l'on vient. Pas de barre de navigation : l'écran est plein.
2. Un réglage par ligne : libellé en clair et, en dessous, ce qu'il concerne.
3. Curseur 0 à 100 avec la valeur écrite ; le changement s'entend tout de suite (aperçu sonore), sans bouton Enregistrer.
4. Bouton muet par canal, sans perdre le niveau réglé.
5. Interrupteurs : vert = activé, toujours avec la position du bouton (pas seulement la couleur).
6. Réinitialiser remet cette catégorie seulement à ses valeurs par défaut.

### M81 — Paramètres

![M81](Maquettes/ecrans/M81-parametres-graphismes.png)

1. Qualité en 4 niveaux, Auto par défaut (le jeu choisit selon l'appareil).
2. 30 ou 60 images par seconde, avec la conséquence écrite (fluidité contre batterie).
3. Économie de batterie : bascule automatique sous 20 % de batterie (mobile).
4. Réglages réservés au PC regroupés sous un intertitre ; changer la résolution demande une confirmation avec retour automatique après 10 s.
5. Réinitialiser la catégorie.

### M82 — Paramètres

![M82](Maquettes/ecrans/M82-parametres-accessibilite.png)

1. Taille du texte en 3 niveaux, avec un aperçu immédiat dans la ligne.
2. Mode daltonien (protanopie, deutéranopie, tritanopie) : palette des familles adaptée ; leurs icônes restent toujours affichées, la couleur n'est jamais seule.
3. Réduction des flashs et des secousses, séparées (certains joueurs ont besoin de l'une sans l'autre).
4. Temps étendu des mini-jeux : plus de temps et des zones de réussite élargies, sans pénalité.
5. Sous-titres des répliques doublées et des sons importants. Ces réglages suivent le compte d'un appareil à l'autre.

### M83 — Paramètres

![M83](Maquettes/ecrans/M83-parametres-notifications.png)

1. Interrupteur général, avec l'état de l'autorisation du système (sinon, bouton « Autoriser dans les réglages du téléphone »).
2. Un interrupteur par type de notification : le joueur garde celles qui l'aident à revenir au bon moment.
3. Heures calmes réglables (22 h à 8 h par défaut).

### M84 — Paramètres

![M84](Maquettes/ecrans/M84-parametres-controles.png)

1. Raccourcis par action, regroupés par usage ; toucher une touche pour la changer (catégorie visible seulement sur PC).
2. Mode d'écoute : « APPUIE… » en jaune jusqu'à la nouvelle touche ; un conflit avec une autre action est signalé et proposé en échange.
3. Afficher les touches entre crochets sur les boutons du jeu.
4. Caméra du plateau : sensibilité du glisser et sens du zoom à la molette.

### M85 — Paramètres

![M85](Maquettes/ecrans/M85-parametres-compte-support.png)

1. Compte : services liés, liaison d'un autre service, consentement aux statistiques, export des données (RGPD).
2. Support & aide : « Revoir le tutoriel » en premier, fiches d'aide déjà vues (glossaire), contact du support.
3. ID joueur copiable, version et numéro de build : ce que le support demande en premier ; crédits (dont musiques et sons Kenney CC0).
4. Suppression du compte : double confirmation, il faut écrire SUPPRIMER ; tant que ce n'est pas fait, le bouton rouge reste désactivé.
5. Conséquences écrites sans détour ; l'action proposée par défaut (jaune) est de garder son compte.

## 11. Tutoriel

### T01 — Tutoriel 1/9 · Bienvenue

![T01](Maquettes/tutoriel/T01-bienvenue.png)

1. Pas de projecteur à la première étape : tout l'écran est voilé, on présente le guide. Rayner (directrice de la Guilde) parle en médaillon, dans une bulle de BD.
2. Récitatif jaune = titre de l'étape ; 2 lignes de texte au maximum, en Comic Neue.
3. Progression discrète (« 1 / 9 ») : le joueur sait que c'est court.
4. « Passer le tutoriel » toujours présent mais discret ; il demande une confirmation (T12).

### T02 — Tutoriel 2/9 · Escouade

![T02](Maquettes/tutoriel/T02-escouade.png)

1. Projecteur : seul l'élément à toucher est découpé dans le voile et cerclé de jaune pulsé ; tout le reste est bloqué tant que l'étape n'est pas faite.
2. Main animée qui tapote l'élément.
3. Une seule notion (l'escouade de 4), au moment où elle sert ; le geste demandé est écrit en gras.

### T03 — Tutoriel 3/9 · Mission

![T03](Maquettes/tutoriel/T03-mission.png)

1. Le bouton le plus important de l'écran est celui qu'on apprend en premier.
2. La bulle se place loin de l'élément désigné pour ne jamais le cacher.

### T04 — Tutoriel 4/9 · Objectifs

![T04](Maquettes/tutoriel/T04-objectifs.png)

1. Étape d'explication : le projecteur montre le panneau, et c'est « Compris » qui fait avancer (pas de geste à deviner).
2. Le texte annonce aussi la suite (« le premier est déjà en vue ») pour enchaîner sur le lancer.

### T05 — Tutoriel 5/9 · Lancer le dé

![T05](Maquettes/tutoriel/T05-lancer.png)

1. Geste réel sur la vraie interface : le joueur lance vraiment son premier dé de la zone 1.1.
2. Le vocabulaire (PM) est introduit au moment où il apparaît à l'écran.

### T06 — Tutoriel 6/9 · Destination

![T06](Maquettes/tutoriel/T06-destination.png)

1. Projecteur rond sur une destination désignée : pour la première fois, le choix est guidé ; ensuite le joueur choisit librement.
2. Deux notions liées en une bulle (portée et brouillard) parce qu'elles se voient au même moment.

### T07 — Tutoriel 7/9 · Temps actif

![T07](Maquettes/tutoriel/T07-combat-atb.png)

1. Le combat (temps actif) est mis en pause tant que la bulle est ouverte : le joueur lit sans stress.
2. « Compris » relance le combat ; Givre est déjà prête, l'étape suivante enchaîne aussitôt.

### T08 — Tutoriel 8/9 · Compétence et cible

![T08](Maquettes/tutoriel/T08-combat-competence.png)

1. Compétence puis cible : deux gestes, deux étapes visibles (la compétence choisie reste encadrée dans le panneau de gauche).
2. Après ce coup, le tutoriel se tait jusqu'à la fin du combat : le joueur finit seul (AUTO et combos seront expliqués par des astuces).

### T09 — Tutoriel 9/9 · Fatigue

![T09](Maquettes/tutoriel/T09-resultats.png)

1. La fatigue, règle clé de la gestion d'équipe, est expliquée sur les jauges réelles du combat qui vient d'avoir lieu.
2. Le conseil donne déjà le réflexe suivant (l'infirmerie), sans l'imposer.

### T10 — Astuce contextuelle · Combo

![T10](Maquettes/tutoriel/T10-astuce-combo.png)

1. Astuce contextuelle : carte non bloquante qui apparaît la première fois qu'une mécanique se présente (ici, le premier combo disponible). Le combat continue derrière.
2. L'élément concerné est seulement souligné (cadre jaune), sans voile.
3. Fermeture d'un geste ; la fiche reste consultable dans Pause ▸ Aide. Autres astuces : AUTO et vitesse, dé bonus, Terminer, cases Événement / Mini-jeu / Repos, défaite, fatigue > 60 %, zone sécurisée, recrutement, infirmerie.

### T11 — Tutoriel terminé

![T11](Maquettes/tutoriel/T11-fin.png)

1. Fin célébrée en éclat comics : le joueur sait que la phase guidée est finie.
2. Rayner annonce la suite (astuces au bon moment) et le prochain objectif de l'histoire.
3. Aucune récompense supplémentaire : l'économie reste côté serveur ; le joueur arrive sur le QG libre de ses choix.

### T12 — Passer le tutoriel ?

![T12](Maquettes/tutoriel/T12-passer.png)

1. Passer demande une confirmation : un toucher accidentel ne fait pas perdre le guidage.
2. Le joueur sait où le revoir et que les astuces contextuelles restent actives.
3. L'action par défaut (jaune) est de continuer ; Retour = continuer. L'étape atteinte est enregistrée : quitter le jeu puis revenir reprend au même endroit.

## 12. Référence

### M90 — Kit UI comics

![M90](Maquettes/reference/M90-kit-ui.png)

1. Boutons : principal jaune #FFD23F (un seul par écran), danger / MISSION rouge #E5484D, secondaire #2B3140, désactivé estompé ; contour noir 5 px, ombre décalée 7 px ; raccourci PC en pastille.
2. Récitatifs (jaune = titre, blanc = info, rouge = alerte) et cases de BD (#1E222B, contour 5 px, rayon 14 px, ombre 9 px, trame de points) ; badges rouge (action) et jaune (collecte).
3. Familles toujours en icône + couleur ; raretés en cadre cuivre, argent, or.
4. Jauges : PV allié vert, PV ennemi rouge, ATB jaune (fine), fatigue verte puis orange au-delà de 60 % (rouge à 100 %), bouclier bleu ; libellés dans la jauge.
5. Contrôles des paramètres : interrupteurs, curseurs, choix segmentés, pastilles de ressources.
6. Tutoriel : médaillon de Rayner, bulle de BD, projecteur jaune ; retours de combat : éclat « CRIT ! », chiffres encrés.

### M91 — Screen flow

![M91](Maquettes/reference/M91-screen-flow.png)

1. Entrée : titre, premier lancement (langue + consentement), chargement, puis tutoriel guidé au tout premier lancement (QG directement les fois suivantes).
2. Hub : un menu = un onglet de la barre de navigation basse (QG, Équipe, Recruter, Pass, Boutique) ; les pièces s'ouvrent depuis le QG.
3. Paramètres accessibles partout : ⚙ dans la barre haute du hub, Pause en mission.
4. Mode mission, plein écran sans barre de navigation : carte → fiche de zone → préparation → plateau.
5. Depuis le plateau : cases (combat, événement, mini-jeu, repos) puis retour au plateau ; fin de zone ou retour volontaire → QG.
