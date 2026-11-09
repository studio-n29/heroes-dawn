# Heroes Dawn — Refonte fonctionnelle et responsive

5 novembre 2026. Suite de la direction Aube tactique choisie le 4 novembre. Le périmètre comprend les compositions, les parcours qui nécessitent une adaptation, les gabarits communs et leur vérification.

## Changements

- Boutique organisée autour d’une navigation latérale, d’offres illustrées et d’une fiche d’achat. Les lignes de catégorie ne prennent plus la place d’offres achetables.
- Recrutement avec portraits plus grands et actions séparées des informations.
- Ouverture de booster en trois temps : paquet, révélation, résumé. Le bouton Suivant ouvre le résumé et Continuer ferme. Le résumé ne relance pas le tirage. La souscription d’annulation est libérée à la fermeture.
- Atelier centré sur les recettes à l’entrée. Un mode Recherches conserve les fonctionnalités temporisées sans les mélanger à la liste de fabrication.
- Grimoire, registre, inventaire et équipement : grille à nombre de colonnes variable, filtres qui occupent leur largeur disponible, description défilante et largeur des actions adaptée au nombre de boutons visibles.
- Pages de liste : le texte ne réserve plus systématiquement la largeur de trois boutons. Dans une colonne étroite, les actions passent sous le texte ; les listes de droite défilent, de même que les textes longs des archives.
- Les consignes de mini-jeux disposent d’un fond opaque, pour ne plus concurrencer le manuel ou les objets du jeu.
- Le bandeau de quête du QG ouvre à nouveau le journal ; les deux tests autrefois ignorés sont réactivés.
- Le registre utilise désormais le cadrage des visages, au lieu de montrer des personnages entiers trop petits.
- Un paquet illustré Aube tactique remplace l’ancienne icône violette du booster. Asset : `Code/Heroes Dawn Unity/Assets/_Project/Resources/UI/AubeTactique/shop-booster.png`. Il est généré avec l’outil imagegen intégré ; [prompt exact](Aube_Tactique_2026-11-04/prompt-shop-booster.json). Il complète les 16 illustrations précédentes.
- Le cadeau quotidien reste dans le Pass, conformément au système de la version 0.15 ; la boutique propose un accès explicite. Les prix, produits et récompenses conservent leurs valeurs réelles du projet.

Les réglages sont également reconstruits : catégories à gauche, illustration verticale de Giulia, contrôles à droite et listes défilantes. Les 88 prefabs disposent désormais de recettes reproductibles, y compris la page Réglages et ses lignes qui étaient auparavant manuelles. Les paramètres de sauvegarde et le contenu de progression ne sont pas effacés.

## Stratégie responsive

Le jeu reste **mobile paysage**, conformément à ses réglages Android. La zone disponible est calculée à partir de l’écran et de `Screen.safeArea`. Le canvas conserve au moins une composition logique 1920 × 1080 dans cette zone ; une tablette ajoute de la hauteur et un téléphone très large ajoute de la largeur. L’UI se recalcule quand la taille ou les encoches changent.

Cette base protège les compositions spécifiques du combat, du plateau, des fenêtres et des réglages. Les gabarits de collection recalculent en plus leurs colonnes ; les en-têtes tiennent compte des actions réellement visibles. Les listes longues et les descriptions restent accessibles par défilement. Les filtres ne reposent plus sur une somme de largeurs fixes.

Les contrôles automatisés incluent les encoches simulées dans le calcul de référence, le 16:9, le 20:9, le 4:3, l’ultralarge et une résolution de 1280 × 720. La relecture de captures est complémentaire. Cela ne certifie pas à elle seule le confort tactile, le clavier logiciel ou tous les appareils physiques.

## Validation et rendus

Les rapports courants sont dans `Code/docs/test-reports/ui-refonte-*.xml` et les logs correspondants. Les captures de cette livraison ont leur propre répertoire afin de conserver les preuves de la première intégration.

Les résultats finaux et les liens de galerie sont indiqués ci-dessous après exécution.

## Limites de livraison

Les maquettes guident la hiérarchie et la composition ; les fonctionnalités présentes priment sur leurs éléments générés accidentellement. Aucun nouveau service de paiement, matchmaking ou social n’est créé par cette refonte. Certains objets de gameplay des mini-jeux conservent les primitives du prototype. La vérification sur téléphone et tablette physiques ainsi que le profilage restent nécessaires.

Travail local, sans commit, push ni publication. La modification préexistante des réglages globaux URP n’a pas été reprise pour la refonte.

La [revue complète du jeu](Revue_Jeu_2026-11-05.md) détaille les boucles micro et macro, les points forts et faibles, et les recommandations de périmètre.
