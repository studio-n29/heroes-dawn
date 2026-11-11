# État de production — 10 novembre 2026

Demande : tous les assets graphiques manquants selon l’inventaire Aube tactique. Continuer jusqu’à couverture complète puis vérifications et catalogue final. Aucun commit/push ni raccordement des écrans réalisé dans ce chat. Le projet comporte des changements extérieurs à préserver.

## Livré et vérifié

- 114 PNG/SVG natifs, 33 agents + correction Ada v2, 64 compétences, 32 dossiers, 10 poses, 8 rivaux, 16 scènes. Les cartes et zones sont presque complètes : vérifier les fichiers.
- Exports techniques autorisés explicitement par l’utilisateur : « Utilise un script pour les découpes et redimensionnements. »
- export-assets.ps1 produit portraits128/512, cartes, icônes, décors PNG/JPG, plateaux4K (agrandissements déclarés).
- export-map.ps1 corrigé pour System.Drawing.Common : dix calques illustrés3840×2160, masques exacts CityMapLayout. Manhattan sélectionné inspecté.
- Portraits, compétences, dossiers, cartes intermédiaires, objets intermédiaires, poses, ennemis, rivaux et scènes inspectés en planches qa-*.png.
- Deux mauvaises générations Apple/Google sont remplacées par build-brand.ps1 (2SVG/PNG blancs). Brouillons conservés dans RejectedBrandDrafts hors dossier livré.

## Générations encore actives

- Cellule23 : cartes48 (presque terminée).
- Cellule43 : environnements62 (intros puis chapitres en fin de liste), deux workers.
- Cellule54 : zones48 (27Boards +21Zones), deux workers.
- Cellule29 a été arrêtée pour accélérer le lot restant. Ses29jobs manquants sont repris sans écraser les fichiers existants dans105,106,107 (3groupes disjoints, clés spriteRemaining et spriteJobs dans functions store). Ne pas relancer29.
- Cellules36(compétences),52(suppléments) peuvent être terminées, vérifier sur disque. Leur résultat cumulé peut dépasser la limiteIPC : éviter de récupérer toutes leurs images en un seul wait. Les fichiers copiés restent présents.
- Les listes prompts-*.json sont les plans. check-coverage.ps1 prévoit469sources (468initiales+gear-body), sans compter Ada-v2 ni les exports.

## Reste

1. Finir les générations manquantes en vérifiant coverage-generated.json, sans doublons.
2. Prévoir les huit états verrouillés désaturés des chapitres. Pas encore livrés sous forme d’images ; option runtime mentionnée auparavant. Pour une modification raster autre que découpe/redimensionnement, employer imagegen par défaut.
3. Contrôler les nouvelles planches, notamment mini-jeux, derniers décors, chapitres et logos corrigés.
4. Relancer export-assets.ps1 puis catalogue.ps1 à la fin ; export-map.ps1 déjà exécuté avec succès.
5. Actualiser README, couverture, chiffres finaux et présent état. Ne pas prétendre à une validation Unity.

Attention : les fichiers concept-* sont des propositions pour les zones futures. Les plateaux zone_ny_1_2/3/4 correspondent à Brownsville/ConeyIsland/DUMBO. Les images natives ne sont pas toujours aux dimensions conseillées ; les exports le sont. Aucune mémoire personnelle consultée.

## Reprise après réinitialisation des traitements
Les anciennes cellules ne sont plus disponibles. La nouvelle cellule5 traite les31sources absentes listées dans prompts-resume.json (quatre workers), avec vérification avant génération. La cellule7 traite les états verrouillés ny-02/03/04. ny-01 verrouillé est déjà copié. Les quatre états verrouillés mos-01/02/03/04 restent à générer après leurs originaux et inspection. Le plan de couverture compte désormais477sources +Ada-v2 (hors exports). Les logos vectoriels sont vérifiés ;116SVG sources au total.

