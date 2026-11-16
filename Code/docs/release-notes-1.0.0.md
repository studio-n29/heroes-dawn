# Heroes Dawn 1.0.0 — notes de version

Version Unity 1.0.0 (code Android 10000). Plateformes : Windows et Android. Ni iOS ni Switch (décision H3 du 2026-11-12).

## Ce que le joueur voit

- **Brooklyn complet** (4 zones) et **Agent D et Agent M jouables**. Agent M joue le chapitre Sergueiv de Moscou : 4 zones (gare du maglev, monastère-archives, voie du maglev, dépôt), boss le Passeur, 4 fragments de souvenirs, fin « à suivre ». Paris et Salem ne sont pas dans cette version.
- **Interface refondue** sur la direction artistique de référence : barre du haut à pastilles de monnaie colorées, navigation à cinq onglets avec pastilles de notification, boutons principaux à étoile et chevron, bulles d'information compactes qui ne masquent plus le contenu.
  - QG : bandeau de quête, activités à cocher, carte de prochaine mission, pièces illustrées.
  - Équipe et fiche d'agent : portraits cadrés sur le visage, icônes de famille, statistiques à icônes.
  - Carte, plateau, repos : objectifs à cocher, boutons à icônes, dé illustré, cartouches de quartier.
  - Combat : fiches d'unités à portrait, agent actif détouré, cartes de compétence à plat, frise affinée.
  - Boutique, recrutement, réglages : onglets verticaux à icônes, prix à pièces, interrupteurs et curseurs.
- **Équilibrage** : Crocodile, Titan du pont, le Passeur et l'élite de Moscou réglés pour 40 à 60 % de victoires avec l'équipe de départ et des objets. **Brick n'est pas réglé** (voir limites).
- **Tournois JcJ** en ligne et hors ligne, avec classement et paliers.
- **Achats intégrés** (7 produits consommables), testés en bac à sable seulement.
- **Mesure d'usage** anonyme, désactivée par défaut, activée seulement après accord à la première connexion.
- **Poids** : l'APK passe de 1,59 Go à environ 251 Mo.

## Technique

- Services : migrations Supabase `002_hd_arena` et `003_hd_arena_durcissement` appliquées sur N29 HQ le 2026-11-16 (arène des tournois, droits réduits au nécessaire, aucun accès anonyme).
- Valeurs de conception ⚑ validées en bloc le 2026-11-16 (voir `05_Production/Decisions.md`).
- Correctif : exception audio à la fermeture d'une fenêtre (source sonore détruite).
- Nouveau : compteur d'images par seconde pour les builds de test (option de build `-fpsOverlay`), absent des builds de diffusion.
- Tests : EditMode 1569 réussis sur 1573 (4 ignorés), PlayMode 114 réussis sur 142 (28 ignorés), aucun échec.

## Limites connues

- **Brick** (boss) reste à 100 % de victoires : l'attaque seule ne suffit pas, sa conception est à revoir.
- **Pas de voix** : sous-titres et textes font foi.
- **Textes de Moscou** : relecture française et validation de l'anglais à faire par l'équipe.
- **Ennemis de Moscou** (garde, contrebandier, homme de main, le Passeur) : pas d'illustration propre, silhouette dans un cadre rouge.
- **Pages Profil, Pass, Tournoi et Repaire** : nouvel habillage de liste seulement ; les compositions des planches demandent des données que les vues ne fournissent pas encore.
- **Mini-jeux** : non repris dans la refonte d'interface.
- **Avatars** de création du personnage : numéros, aucune image disponible.
- **Suppression de compte** : le jeu efface la sauvegarde mais pas le compte en ligne ; nécessaire avant une mise en ligne sur Google Play (voir `05_Production/Legal/Dossier_Conformite_1.0.0.md`).
- **AAB** de test à 251 Mo, au-dessus de la limite de 200 Mo de la base Google Play (réduire encore ou passer par Play Asset Delivery).
- **Réglage Supabase** « leaked password protection » à activer dans le tableau de bord.
- **Performances** : 60 images/s non mesurées sur Android (le compteur FPS des builds de test sert à cela).
- Le test PlayMode `FullRunTests` a un timeout intermittent non résolu.
- Rien n'est étiqueté ni poussé : `v1.0.0-rc1` reste à faire.
