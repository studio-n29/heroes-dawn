# Aube tactique — Intégration Unity

Livraison locale du 4 novembre 2026 dans `Code/Heroes Dawn Unity`.

## Contenu livré

- 21 planches de conception : 84 vues dans la galerie `index.html`.
- 16 illustrations de production : QG New York et Moscou, combat Red Hook, cinq pièces du QG et huit mini-jeux.
- Kit de 27 sprites UI, dont 11 nouveaux glyphes et emblèmes ; panneaux à angle coupé, cadres fins, étoile à quatre branches.
- Polices Barlow pour les textes et Barlow Condensed pour les titres, licences conservées.
- 88 prefabs harmonisés : 86 reconstruits depuis leurs recettes et deux prefabs de réglages migrés en préservant leurs références.

La palette associe marine, ivoire et laiton. Les composants partagés portent les changements sur les écrans, pages, fenêtres et états. Les compositions spécifiques retravaillent notamment le QG, les cartes de combat, la chronologie, les portraits, le tutoriel et les mini-jeux. Les portraits existants sont réutilisés. Les illustrations sont séparées des contrôles Unity ; les valeurs et textes restent issus des données du projet.

## Consultation

- [Rendu réel Unity](../../Code/docs/test-reports/aube-tactique/index.html)
- [Comparaison 16:9](../../Code/docs/test-reports/aube-tactique/captures-16-9/index.html)
- [Spécification](../../Code/docs/specs/2026-11-04-aube-tactique-ui.md)

Assets : `Code/Heroes Dawn Unity/Assets/_Project/Resources/UI/AubeTactique/`.

Dans Unity : **Heroes Dawn > Direction artistique > Appliquer Aube tactique** reconstruit les assets procéduraux et les prefabs. Les GUID existants sont conservés. Cette commande ne se lance pas automatiquement au démarrage.

## Validation

- Suite EditMode complète : **1 311 réussis, 0 échec, 4 ignorés**.
- Parcours PlayMode ciblés : **30 réussis, 0 échec** (compte, navigation, combat, boss et tutoriel).
- Après les dernières corrections : **14 contrôles UI réussis**, incluant le non-chevauchement du titre, de l’avatar, des monnaies et des réglages en 1920 × 1080 et 2160 × 972.
- Captures PlayMode : 61 images par format ; résultats détaillés dans les XML `aube-tactique-captures*.xml`.
- Vérification des sources C# et Markdown avec `git diff --check` réussie. Les prefabs sérialisés par Unity conservent ses espaces après les champs YAML vides.

Rapports XML et logs : `Code/docs/test-reports/`. Les captures montrent les parcours de test et peuvent afficher un tutoriel, un message de diagnostic ou des données de test.

## Limites

Le projet est modifié localement ; aucun commit, push ou déploiement effectué. La validation visuelle sur téléphone, les performances GPU et les safe areas restent à vérifier sur matériel. Aucun nouvel APK construit. Les planches représentent les intentions de composition et ne reproduisent pas exactement les écrans intégrés. Certains objets de gameplay des mini-jeux restent des primitives visuelles du prototype ; leurs décors et leur habillage UI ont été remplacés, sans reconstruire leurs personnages ou obstacles.
