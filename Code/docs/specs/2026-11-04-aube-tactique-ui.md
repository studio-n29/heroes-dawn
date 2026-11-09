# Aube tactique — Spécification d'intégration

Décision utilisateur du 4 novembre 2026 : appliquer la proposition Aube tactique à l'ensemble de l'interface, créer ses assets et l'intégrer dans Unity. Cette charte remplace les directions d'interface pastel et Mo.Co ; les illustrations et le lore existants restent les références du contenu.

## Système visuel

- Fond marine #101C29, panneaux mats, ivoire #ECE7DC pour l'action, laiton #D6AF5B pour l'identité et la sélection.
- Filets fins, coin supérieur droit coupé, ombre courte ; aucune lèvre épaisse sur les boutons.
- Étoile The Dawn à quatre branches et pictogrammes vectoriels rasterisés en sprites. Les formes de gameplay (anneau, losange, brouillard) gardent leur rôle.
- Titres Barlow Condensed ExtraBold Italic sans contour ; textes Barlow SemiBold/Regular à chasse normale. Polices libres SIL OFL, licence embarquée.
- Couleur de famille + icône ; aucune règle ne déduit une famille de la couleur d'une illustration.
- Texte de bouton adapté au contraste de son fond y compris après changement d'état.

## Compositions intégrées

Le QG laisse respirer son panorama, réserve une rangée basse aux cinq fonctions illustrées, regroupe les activités à droite et met Mission en ivoire. Les deux alertes ont un pictogramme et un libellé, avec leurs badges existants.

Le hub utilise le fond de New York ou de Moscou selon la route fournie par le présentateur. La barre de navigation garde les cinq destinations, ajoute des pictogrammes et conserve le marqueur d'onglet actif.

Le titre remplace le gros logo comics au centre par un mot-symbole localisé sobre, une étoile et des actions alignées à droite. Les assets historiques restent présents pour les autres usages de marque.

Le combat présente les unités dans des colonnes compactes, une frise réduite et des cartes de compétence illustrées. Les vues reçoivent l'identifiant du visuel de la compétence, sans modification des règles de ciblage ou d'énergie. Les illustrations des agents et compétences viennent du contenu actuel. Les initiales n'apparaissent qu'en l'absence d'illustration. Le coût de la compétence reste sur un badge distinct.

Toutes les pages et fenêtres sont reconstruites à partir des mêmes composants partagés. Les soins, opérations, atelier, inventaire, archives, pass, boutique, compte, réglages, tutoriel et mini-jeux conservent leurs données et interactions actuelles.

## Assets

Les illustrations nouvelles sont des images sans texte, dans `Assets/_Project/Resources/UI/AubeTactique`. Les contrôles sont des éléments uGUI, pas des images d'écran plates. Les sprites UI sont reproductibles par `UiAssetBaker` et `TacticalGlyphs` ; les prefabs restent reproductibles par les recettes.

Les fonds de mini-jeux ne dessinent pas d'obstacles ou de cibles supplémentaires : seuls les drivers déterminent les objets interactifs. Les anciens fonds restent disponibles comme repli.

## Commande d'application

Menu Unity : **Heroes Dawn > Direction artistique > Appliquer Aube tactique**.

Batch : `-executeMethod N29.HeroesDawn.EditorTools.Assets.AubeTactiqueSetup.ApplyBatch`.

Cette commande importe les illustrations, génère les sprites et polices, puis reconstruit les prefabs existants depuis leurs recettes. Elle est destinée à une migration explicitement demandée, pas à un lancement automatique de l'éditeur. Les GUID sont conservés sur les assets remplacés.

## Vérification

### Passe fonctionnelle du 5 novembre

La boutique utilise une navigation latérale ; le booster dispose d'un résumé distinct après la révélation. L'atelier propose les recettes en premier et un mode séparé pour les recherches. Les collections et les listes utilisent un reflow commun, avec descriptions et listes secondaires défilantes. Le viewport sûr conserve une composition logique minimale 1920 × 1080 dans les encoches, puis exploite la largeur ou hauteur supplémentaire. Le jeu reste en paysage. Voir `03_Art_UI_UX/Refonte_UI_2026-11-05.md` pour les preuves et les limites.

La première migration du 4 novembre couvrait 86 recettes et deux prefabs manuels de réglages. La passe du 5 novembre donne désormais une recette aux 88 prefabs. Le contrôle de la barre haute vérifie le non-chevauchement en 16:9 et 20:9. Le lot de production compte 17 illustrations séparées de l'UI avec le paquet de booster ajouté le 5 novembre.

Les maquettes de conception sont dans `03_Art_UI_UX/Aube_Tactique_2026-11-04` à la racine du dépôt. Elles servent à la direction visuelle ; textes, prix, portraits secondaires et éventuels contrôles dessinés ne font pas autorité.

Les rapports de génération, tests et captures de la véritable interface sont dans `Code/docs/test-reports/aube-tactique*`. Consulter le rapport d'intégration pour les résultats finaux et les limites de validation.
