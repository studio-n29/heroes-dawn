# Heroes Dawn - livraison 2D du 16 novembre 2026

203 exports prêts à intégrer, 63 masters de génération conservés, guide PDF de 32 pages. Les vérifications de fichiers et la revue des planches visuelles sont terminées.

Le point d’entrée est **Guide_Integration_Assets_2D.html** : aperçu de chaque export, référence de la liste source, usage et clé de chargement proposée. **Guide_Integration_Assets_2D.pdf** fournit le guide imprimable avec les repères visuels du document d’origine.

- `Exports/` : fichiers destinés à l’intégration, classés par usage.
- `Integration_Nouveaux.csv` : correspondance fichier / référence PDF / dimensions / destination / clé Resources / border.
- `Integration_Existants_Liste_B.csv` : sources existantes et destinations proposées, avec les vrais identifiants de chapitre.
- `manifest.json` et `manifest.csv` : inventaire et empreintes des exports.
- `Sources/Generated/` : masters originaux de génération, avant redimensionnement.
- `Sources/` : sources SVG des prolongements du kit UI.
- `generated-jobs.json`, `generated-fx-jobs.json`, `generated-corrections.json` et `core-generation-prompts.json` : prompts des générations et origine des fichiers.
- `QA/` et `verification.json` : planches de contrôle et vérifications techniques.

La création utilise l’outil imagegen intégré ; les composants vectoriels prolongent le kit Aube tactique existant. Aucun fallback API/CLI n’a été utilisé. Aucun asset existant du jeu n’a été remplacé et aucune intégration Unity n’a été effectuée : la livraison est isolée pour permettre une intégration manuelle.

R-03 reste à réaliser dans le jeu final : huit captures de store et, en option, quatre captures tablette. Ces captures ne sont pas simulées. La liste B réutilise les ressources déjà présentes ; elle n’est pas régénérée.

Les formats exportés suivent la liste. La carte 3840 x 2160 et certains autres grands formats sont redimensionnés depuis les masters retournés par le générateur ; les sources originales sont conservées. G-04 est livré selon l’option **image fixe + particules**, avec un projectile et un impact par famille. Les trois tenues Pass sont proposées sur Giulia, faute d’attribution dans le catalogue ; leur affectation et les propositions signalées « à confirmer » dans le PDF restent à valider visuellement.

Les scripts `build-assets.cjs`, `package-images.py`, `build-guide.py` et `verify-assets.py` reconstruisent les exports et les documents à partir des sources locales. Les chemins de runtime sont ceux de l’environnement de production de cette livraison ; les adapter sur une autre machine.
