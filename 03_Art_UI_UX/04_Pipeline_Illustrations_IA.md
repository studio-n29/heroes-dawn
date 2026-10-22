# Pipeline d'illustrations par IA, dans la DA de Heroes Dawn

> Objectif : produire **toutes** les illustrations du jeu (portraits, icônes, fonds, cinématique, key arts, assets de boutique) avec des IA, avec une apparence **cohérente** d'un asset à l'autre, et les intégrer sans toucher au code.
> Auteur du contenu : vous. Ce document est le mode d'emploi. Il complète `02_Direction_Artistique_Audio.md`. Le calendrier de production est dans `Code/docs/plans/2026-10-14-plan-early-access.md`.
> Les noms d'outils, tarifs et licences changent vite : **vérifiez les conditions d'usage commercial de chaque outil le jour où vous l'utilisez** et consignez-les dans le registre de provenance (section 9).

## 1. Le principe : verrouiller le style, puis produire en série
La cohérence ne vient pas d'un bon prompt, mais de quatre verrous empilés :
1. **Un style figé** (fiche de style + planche de références) : section 2.
2. **Un modèle ou une référence de style fixe** réutilisés par tous les prompts : section 4.
3. **Des gabarits de cadrage** (même pose, même cadre, même fond pour tous les portraits) : section 5.
4. **Un contrôle qualité et une retouche systématiques** avant import : sections 7 et 8.

Ordre de travail : fiche de style → modèle de style → **6 portraits d'essai** → validation par vous → production en lot → retouche → import → audit.

## 2. Fiche de style (à valider : décision D5)
Constat : vos key arts (`Identite/`) sont **peints, sombres et saturés** (crépuscule violet, rouge de la brèche), tandis que le HUD est **pastel, clair et givré**. Les illustrations vont s'afficher *dans* le HUD pastel. Recommandation : un style unique qui garde la peinture comics des key arts mais **éclaircit la lumière** vers la palette du HUD.

| Élément | Règle |
|---|---|
| Genre | Comics moderne, rendu peint semi-réaliste, ombrage « cel » doux, aucun photoréalisme |
| Trait | Contour fin bleu marine `#1B2550` (pas de noir pur, pas de gros contour) |
| Lumière | Contre-jour crépusculaire chaud (pêche/corail `#FF6B5E`) + lumière de remplissage froide (bleu glacé `#A9CDFA`) |
| Palette d'ambiance | `#A9CDFA` bleu glacé, `#B7C4F5`/`#D3C0F0` lavande, `#F6C4D2` rose poudré, `#1B2550` marine, `#FF6B5E` corail |
| Couleur par famille (toujours doublée d'une icône) | Matière = ambre, Élémentaire = cyan, Physique/Bio = rouge, Perception = violet ; elle domine le costume et la lueur de pouvoir |
| Personnages | Adultes, proportions héroïques réalistes, expressions lisibles à 100 px de large, mains simples (cachées si possible) |
| Interdits | Logos, textes lisibles, marques, personnages ou costumes existants (Marvel, DC…), noms d'artistes dans les prompts, scènes sur les sujets sensibles listés au risque R7 |

Planche de références (à constituer dans `03_Art_UI_UX/Production/Style/`) : **uniquement des images dont vous détenez les droits** : vos key arts KA01 à KA03, la maquette HUD `2026-10-12-da-hud-pastel.png`, les premiers portraits validés. **N'entraînez jamais un modèle sur des images Marvel** ou d'autres œuvres protégées, même comme « référence d'ambiance » : cela ruine la défense juridique du projet.

## 3. Ce qu'il faut produire (volumes réels du jeu à la 0.9.0)
| Catégorie | Quantité | Remarque |
|---|---|---|
| Portraits d agents | 32 (30 numérotés + Agent D et Agent M) | Données dans `Resources/Content/agents.json` ; liste ligne à ligne dans `Prompts/prompts_illustrations.csv` |
| Icônes de compétences | 90 (porte EA : 20 génériques) | 3 par agent ; génériques = 5 catégories × 4 familles |
| Icônes de statuts | 14 | Brûlure, gel, choc, corrosion, aveuglement, vulnérable, entrave, marque, provocation, bouclier, régénération, hâte, esquive, ancrage |
| Icônes d'objets | 10 | Liste dans le CSV |
| Fonds de zones | 4 (puis 15) | Ratio actuel ≈ 3,06:1 |
| Cinématique d'introduction | 9 | 16:9 |
| Key arts | 6 | KA01-KA03 générés (Canva), KA04-KA06 à faire |
| Ennemis | ~13 + 5 boss | Pas de rig en EA : portraits comme les agents |
| Page boutique (Steam) | ~12 | Capsules, héros, logo, 5 captures ou plus : tailles à vérifier dans Steamworks |

## 4. Pile d'outils recommandée
Deux voies. Choisissez **A** si vous voulez la cohérence maximale sur 30 personnages et 90 icônes ; **B** si vous voulez démarrer aujourd'hui.

### Voie A : modèle de style entraîné (recommandée pour la série)
| Étape | Outil possible | Rôle |
|---|---|---|
| Entraînement du style | Un service d'entraînement de modèles pour jeux vidéo (par exemple **Scenario**) ou un LoRA sur **SDXL** / un modèle à licence permissive via **ComfyUI** (local) ou un hébergeur d'inférence | Apprendre *votre* style à partir de 15 à 30 images dont vous détenez les droits |
| Cadrage identique | **ControlNet** (pose et profondeur) ou l'équivalent du service, avec un gabarit de buste unique | Même cadre, même échelle, même fond pour les 30 portraits |
| Cohérence d'un personnage | Fiche de personnage + image de référence (IP-Adapter ou « référence de personnage ») | Même visage dans le portrait, la carte et la fiche |
| Détourage | **BiRefNet** / **rembg** (gratuits) ou le détourage du service | PNG transparent |
| Agrandissement | **Real-ESRGAN** (gratuit), ou un agrandisseur payant pour le marketing | Finaliser les key arts et la boutique |
| Retouche | **Krita** (gratuit) ou Photoshop/Affinity | Mains, yeux, raccords, harmonisation des couleurs |

Attention aux licences : certains modèles ouverts (dont des variantes de FLUX) ont des licences qui limitent l'usage commercial du modèle ou de ses dérivés ; lisez-les avant d'entraîner. Préférez un modèle ou un service dont les conditions autorisent explicitement l'usage commercial des images produites.

### Voie B : démarrage rapide (sans entraînement)
- **Midjourney** (plan payant, usage commercial selon l'offre) avec une **référence de style** (`--sref`) et une **référence de personnage / omni-référence** fournies par vos propres images, ou **Leonardo** / **Ideogram** avec images de référence.
- Vous gardez un contrôle plus faible du cadrage : compensez par un **gabarit d'image d'entrée** identique pour tous les portraits et une retouche plus lourde.
- Canva (utilisé pour KA01-KA03) convient aux key arts, pas à une série.

### Animation (facultatif, hors EA)
Pour l'EA, pas de rig : le jeu affiche des portraits. Pour donner de la vie à peu de frais, je peux ajouter dans Unity un léger mouvement de « respiration » et un flash de dégâts sur le portrait (shader et PrimeTween, sans asset supplémentaire).

## 5. Gabarits de cadrage (le secret de la cohérence)
Un gabarit unique par type d'asset, que vous réutilisez à l'identique :

| Asset | Gabarit | Règle de composition |
|---|---|---|
| Portrait d'agent | Buste 3/4, regard vers la gauche du cadre (les alliés sont à gauche à l'écran), tête centrée à 38 % de la hauteur, épaules coupées en bas | **Le visage doit tenir dans le cercle inscrit à 80 %** : le jeu l'affiche aussi en rond (portrait de combat) |
| Icône de compétence | Un seul symbole centré, 80 % du cadre, fond transparent, lueur à la couleur de la famille | Lisible à 64 px ; pas de texte |
| Fond de zone | Vue de dessus, ratio 3,06:1, zone centrale dégagée pour le plateau, ciel et lointain en haut | Densité de détails faible au centre (lisibilité des cases) |
| Cinématique | 16:9, sujet dans le tiers gauche ou droit, bande libre en bas pour le texte | Cohérence de la lumière sur les 9 plans |

Les gabarits (image d'entrée de ControlNet ou de référence) sont à enregistrer dans `03_Art_UI_UX/Production/Gabarits/`.

## 6. Contrat d'assets : où déposer, comment nommer
Tout est chargé **par identifiant**. Aucun code à modifier : déposez le fichier au bon endroit avec le bon nom ; sans fichier, le jeu garde la silhouette ou l'icône générique (repli).

| Asset | Dossier Unity | Nom | Format | Taille maître |
|---|---|---|---|---|
| Portrait d'agent | `Assets/_Project/Resources/Portraits/` | `<idAgent>.png` (ex. `ancre.png`, `lone_wolf.png`) | PNG transparent | 1536 × 2048 |
| Icône de compétence | `Resources/SkillIcons/` | `<idCompétence>.png` (ex. `ancre_a1.png`) ou `generic_<catégorie>_<famille>.png` | PNG transparent | 512 × 512 |
| Icône de statut | `Resources/StatusIcons/` | `<statut>.png` (ex. `burn.png`) | PNG transparent | 256 × 256 |
| Icône d'objet | `Resources/ItemIcons/` | `<idObjet>.png` (ex. `kevlar_vest.png`) | PNG transparent | 512 × 512 |
| Fond de zone | `Resources/Zones/` | `<idZone>_bg.png` (ex. `zone_ny_1_1_bg.png`) | PNG ou JPG | 3060 × 1000 |
| Cinématique | `Resources/Intro/` | `intro_01.png` … `intro_09.png` | PNG ou JPG | 1920 × 1080 |
| Key art | `Resources/KeyArts/` | `KA01_Login.jpg` … | JPG | 3840 × 2160 |

Les identifiants viennent de `agents.json`, `skills.json` et `items.json` ; la colonne `fichier_cible` du CSV les donne déjà. Les chargeurs, l'audit (`Heroes Dawn ▸ Contenu ▸ Audit des assets`) et les réglages d'import automatiques sont les tâches E2 et E3 du plan.

Conservez les **fichiers sources** (PNG non compressé, calques de retouche) dans `03_Art_UI_UX/Production/` (pas dans Unity) : seuls les exports vont dans `Resources`.

## 7. Gabarits de prompts
Les gabarits sont donnés en anglais : les modèles d'image y répondent mieux. Remplacez les `{variables}` ; le CSV fait ce travail pour vous.

**Bloc de style (à coller en tête de *tous* les prompts de série ; à ajuster une fois, puis ne plus toucher) :**
```
modern comic book illustration, painterly semi-realistic, soft cel shading, thin navy ink outline (#1B2550), warm peach-coral rim light, cool icy-blue fill light, pastel dusk palette (icy blue, lavender, powder pink), clean readable shapes, adult heroic proportions, no text, no logo, no watermark
```
**Prompt négatif :** `photorealistic, thick black outline, hard black shadows, text, letters, logo, watermark, extra fingers, deformed hands, blurry, low contrast, cropped head, nsfw, gore`

**Portrait d'agent :**
```
{STYLE BLOCK}. Waist-up portrait of {description}, three-quarter view facing left, head centered at 38% height, plain flat {couleur famille} backdrop (to be removed), costume dominated by {couleur famille} with {signe distinctif}, powers: {pouvoir en une image}, confident {expression} expression, face fully inside the central circle of the frame
```
**Icône de compétence (planche de 9 pour la cohérence, puis découpe) :**
```
{STYLE BLOCK}. A 3x3 grid sheet of nine game ability icons on a transparent-like flat background, each icon a single centered symbol, glowing {couleur famille} energy, no text. Icons: {liste de 9 symboles}
```
**Fond de zone :**
```
{STYLE BLOCK}. Top-down illustrated map of {lieu}, 3:1 panoramic, clear open center for a board game path, {ambiance}, sky and distance at the top, low detail in the center
```
**Cinématique :** `{STYLE BLOCK}. Wide cinematic scene: {événement}, subject on the left third, empty band at the bottom for subtitles, 16:9`

Règles :
- **Une variable à la fois.** Changez le personnage, jamais le bloc de style.
- **Seed et réglages consignés** pour chaque image acceptée (colonne `seed` du registre).
- Générez **4 à 8 candidats** par asset, retenez le meilleur (taux de rejet attendu : 60 à 80 %).
- Pour un personnage, faites d'abord une **feuille de référence** (face, 3/4, profil) ; elle sert de référence pour tous ses rendus.

## 8. Contrôle qualité avant import (liste de contrôle)
- [ ] Style conforme à la planche de références (comparez côte à côte avec 3 portraits déjà validés)
- [ ] Visage entièrement dans le cercle central ; mains correctes ou cachées
- [ ] Aucune lettre, aucun logo, aucun personnage reconnaissable d'une œuvre existante
- [ ] Couleur de famille dominante respectée
- [ ] Fond transparent propre (pas de frange claire sur l'alpha)
- [ ] Lisible en miniature (réduire à 128 px : on reconnaît toujours le personnage)
- [ ] Nom de fichier conforme au contrat (section 6) ; ligne ajoutée au registre de provenance

Le jeu vérifie en plus les dimensions et la transparence dans l'audit des assets.

## 9. Légal et traçabilité
- **Registre de provenance** : pour chaque asset, enregistrer l'outil, sa version, la date, le plan et la licence applicables, le prompt, la seed, le chemin de fichier et la retouche humaine effectuée. Le CSV `prompts_illustrations.csv` a les colonnes prévues ; l'import Unity les complète (tâche E4).
- **Droit d'auteur** : un contenu produit sans intervention créative humaine n'est en général pas protégeable (position de l'US Copyright Office, à confirmer auprès d'un juriste pour vos marchés). La retouche manuelle, la composition et la sélection documentées renforcent votre position : gardez les fichiers sources avec calques.
- **Steam** impose de déclarer l'usage d'IA dans le questionnaire de publication ; Google Play et l'App Store ont leurs propres règles (voir `Obligations_Legales.md`).
- Ne mettez jamais dans un prompt le nom d'un personnage ou d'un artiste protégé ; décrivez l'allure (« caped hero in a red and navy suit »).

## 10. Planning de production (pour l'EA du 14 novembre)
Hypothèse : voie A ; débit estimé de 6 à 10 portraits acceptés par jour avec retouche. À ajuster après les 6 portraits d'essai.

| Dates | Production | Livrable |
|---|---|---|
| 14 → 16 oct. | Fiche de style validée (D5), planche de références, gabarits ; choix de la voie A ou B ; vérification des licences | Dossier `Production/Style` et `Gabarits` |
| 19 → 21 oct. | Entraînement du modèle de style ; **6 portraits d'essai** : Ancre, Verre, Givre, Écho, Arc, Rempart | Validation des 6 par vous |
| 22 → 28 oct. | **30 portraits**, 5 à 6 par jour, dont les 11 plus rares en premier | `Resources/Portraits/` complet |
| 26 oct. → 1er nov. | **Icônes** : 20 génériques + 14 statuts + 10 objets (7 planches de 9) | `SkillIcons`, `StatusIcons`, `ItemIcons` |
| 2 → 6 nov. | **9 illustrations d'intro**, fonds des 4 zones, KA01 à KA03 finalisés | `Intro`, `Zones`, `KeyArts` |
| 9 → 11 nov. | Retouches de dernière minute, après lecture du rapport d'audit | Gel des assets le 11/11 |
| 16 nov. → 12 déc. | 90 icônes uniques, KA04-KA06, assets de la page boutique, éventuelles nouvelles zones | Sortie 1.0.0 |

## 11. Ce que je fais de mon côté
- Chargeurs avec repli, audit, import automatique, registre de provenance (tâches E2 à E4 du plan).
- Génération du CSV de prompts à partir des données du jeu (déjà fait : `Prompts/prompts_illustrations.csv`) ; je le régénère après chaque changement d'agent ou de compétence.
- Comparaison avant/après de chaque lot d'assets dans le jeu (captures) pour vérifier la lisibilité dans le HUD pastel.
