# Capacités composables et synergies d'archétype

> Spec du 2026-11-16. Source : échange de conception avec le porteur du projet (les cartes et les boosters doivent devenir un contenu majeur, « comme Hearthstone »). S'appuie sur `2026-10-26-cartes-paliers-mecaniques-design.md` (niveaux, paliers, 4 mécaniques), `2026-10-27-catalogue-36-cartes-design.md` (48 cartes) et `2026-11-10-rangs-positions-cartes-design.md` (étoiles, positions, bonus +15 %/+5 %).

## Intention
Les cartes et les boosters doivent être un contenu important du jeu : on veut collectionner, construire et se surprendre. Aujourd'hui une carte est une cible et une liste d'effets immédiats et sans condition (11 types d'effet, 3 paliers qui en ajoutent). Les 48 cartes sont des variantes chiffrées les unes des autres (Purge 1, Régénération 2 t, Ancrage 1 t, Bouclier 10 reviennent partout). Ce format ne peut pas porter des centaines de cartes qui se ressemblent peu.

**Ce que le porteur du projet veut le plus : des synergies.** Hearthstone sert de référence pour la profondeur de collection, pas pour la mécanique (pas de main, pas de pioche).

## Objectif de catalogue : au moins 250 cartes différentes
Le catalogue passe de 48 à **250 cartes ou plus**. « Différentes » veut dire que deux cartes ne se distinguent pas seulement par leurs chiffres : chacune a une **signature** propre (déclencheur, condition, ciblage, effets, archétype).

- **Répartition proposée** (à valider) : ★1 : 90 · ★2 : 70 · ★3 : 50 · ★4 : 28 · ★5 : 12 (total 250). Les taux de boosters (50 / 28 / 15 / 6 / 1 %) restent ; avec ces effectifs, une ★5 donnée sort environ une fois sur 1 200 cartes ouvertes.
- **Par archétype** : environ 35 cartes pour chacun des 6 archétypes (210), plus une quarantaine de cartes **neutres** sans archétype (soin, bouclier, dégâts de base) ; chaque famille (4) en compte environ 62.
- **Les cartes doivent rester lisibles** : une carte = une idée qu'on peut dire en une phrase. Les cartes ★4 et ★5 peuvent combiner deux briques, jamais plus de trois.
- **Contenu en données.** Aujourd'hui les cartes sont écrites en C# (`PrototypeCollection.cs`), une carte par bloc de code. À 250 cartes ce n'est plus tenable : le catalogue passe en **fichier de données** (un enregistrement par carte, lu et validé au chargement, exporté vers le tableur par `ContentCsvExport`). C'est une tâche de P1, avant les premières nouvelles cartes.
- **Garde-fou de variété** (test) : deux cartes n'ont jamais la même signature ; chaque archétype contient au moins 3 formes de déclencheur ou de condition différentes ; aucune brique ne sert à plus de 20 % des cartes d'un archétype.
- **Illustrations** : 250 images (aujourd'hui 48, recadrées depuis le Drive). **Le porteur du projet crée les images** (décidé le 2026-11-16) ; l'assistant fournit, **pour chaque lot de cartes**, une liste des illustrations à créer avec leur descriptif.
  - Format de la liste : celui de `03_Art_UI_UX/Prompts_illustrations_cartes_C2.md`, un bloc par carte : nom, identifiant, étoiles, nature, descriptif de l'image (personnage en silhouette, lieu, ambiance, palette liée à l'archétype et à la famille) et descriptif négatif. Un fichier par lot : `03_Art_UI_UX/Prompts_illustrations_cartes_<lot>.md`.
  - Livraison : la liste d'un lot est remise **en même temps que les données des cartes du lot**, pour que les images avancent en parallèle (elles sont sur le chemin critique de la sortie).
  - Dépôt des images : `<id de la carte>.jpg` dans `03_Art_UI_UX/Illustrations/Competences/` ; l'outil `CardArtCropper` les recadre en 640 × 360 (voir `2026-10-27-catalogue-36-cartes-design.md`). Chaque descriptif contient des éléments propres à la carte pour que deux cartes ne se ressemblent pas visuellement.
  - Une carte sans image affiche l'illustration générique (repli existant) : elle n'empêche pas la livraison d'un lot, mais est listée.
- **Textes** : 250 noms, plus les descriptions générées, en français et en anglais.
- **Économie** (point ouvert) : monter une carte au niveau 10 demande 25 doublons (`CopiesToUpgrade`). Avec 250 cartes, les doublons arrivent beaucoup plus lentement : l'économie de niveau et de doublons devra être revue (voir « Questions ouvertes »).

## Modèle de jeu (inchangé)
Une carte est une compétence jouée en combat. Un agent a **2 compétences fixes uniques** et le joueur y ajoute **3 cartes équipables** (5 compétences en tout). Le combat reste la frise active : l'énergie limite l'usage, le joueur choisit ou laisse l'Auto jouer.

## Objectifs
- Des cartes dont l'effet dépend du contexte : statuts de la cible ou des alliés, PV, état de la frise, autres cartes de l'escouade.
- Des synergies **entre les cartes d'un même agent** et **entre les agents de l'escouade**, organisées autour d'une poignée d'**archétypes**.
- Ajouter une carte = surtout des données (briques réutilisables), pas du code par carte.
- Les 48 cartes actuelles, les ennemis et les boss se comportent **exactement comme avant** après la migration.
- **Au moins 250 cartes différentes** dans le jeu (demande du porteur du projet du 2026-11-16), voir « Objectif de catalogue ».

## Hors périmètre
- Main, pioche, deck pendant le combat.
- Refonte de la frise, de l'énergie, des statuts existants, des taux de boosters et des étoiles. (L'économie des doublons est un point ouvert, voir plus bas.)
- Invocations, cartes créées en combat, effets qui modifient l'équipement.
- Nouveaux écrans : seuls les textes, une icône de réaction et un éclat de combat sont prévus.
- L'équilibrage fin (après playtest).

## Contre-objectifs (échec même si « ça marche »)
- Une combinaison unique devient obligatoire, ou une carte hors archétype est inutile.
- Un combat devient illisible : le joueur ne comprend plus pourquoi un effet s'est déclenché.
- Le rejeu déterministe ou `LocalCombatVerifier` se désynchronise.
- Une carte dont le texte ne dit pas ce qu'elle fait.
- Une sauvegarde existante ne se recharge plus.

## Contraintes trouvées dans le code
- **Déterminisme.** Un seul générateur dans la simulation (`_rng`), les événements sont rejoués (`CombatReplay`, `InputLog`). Tout nouveau tirage doit avoir un ordre stable.
- **Boucle d'effets** (`CombatSimulation.Actions.cs`, `ApplyEffects`) : cible par cible, effet par effet dans l'ordre, chacun avec sa chance. Les conditions et l'échelle s'insèrent dans cette boucle ; les réactions réagissent aux événements qu'elle produit.
- **`SkillSlot.Passive` existe mais n'est jamais utilisé** : la simulation l'exclut seulement du décompte des actives. Il n'y a aucun déclencheur aujourd'hui.
- **Niveaux et paliers** : `CardLevels.SkillAt` met à l'échelle dégâts/soins/boucliers (+10 %/niveau) et ajoute les effets des paliers 3/6/10 sans les augmenter ; `CardBonuses.Apply` ajoute +15 % (position) et +5 % (famille) ; `UnitFactory.EmpoweredSkills` applique la puissance de compétence des objets.
- **Ennemis et boss** utilisent aussi `SkillDef` (`EnemyDef.Skills`, `BossPhaseDef.Skills`) et ne reçoivent jamais les bonus des cartes.
- **Règles de contenu existantes** : une carte qui vise un ennemi ne porte aucun effet allié ; `Bounce` seulement avec `Damage` ; 3 paliers aux niveaux 3, 6, 10 ; nature saisie = nature déduite.
- **Textes** : `Loc.DescribeSkill` est écrit à la main par type d'effet ; il ne passera pas à l'échelle.
- **Sauvegardes** : elles stockent des identifiants de carte et des niveaux ; ces identifiants restent stables.
- **Outils de design** (tableur, outil de level design, `DesignToolsDataTests`) lisent aujourd'hui `SkillDef.Effects` ; ils doivent continuer à fonctionner.
- **Synergies existantes** (à ne pas confondre) : `SynergyDef` (paires de familles, bonus de statistique) et `PairBonusRules` (paires de positions, bonus par niveau de lien) ne concernent pas les cartes.

## Approches étudiées
1. **Étendre `EffectDef`** avec une condition et une échelle facultatives. Peu coûteux, mais les déclencheurs s'y greffent mal et chaque ajout touche encore plusieurs `switch`. Écartée.
2. **Capacité composable : déclencheur + condition + ciblage + effets.** Les effets actuels deviennent des briques. **Retenue** par le porteur du projet, en deux temps : migrer les 48 cartes sans rien changer, puis ajouter déclencheurs, conditions et étiquettes par lots.
3. **Une classe de code par carte.** Libre, mais chaque carte coûte du code et des tests et contourne les outils de données. Écartée.

## Conception

### 1. La capacité (`Ability`)
Une compétence porte une ou plusieurs capacités. Une capacité = **déclencheur** + **condition** (facultative) + **ciblage** + **effets**.

- **Déclencheur.** `OnCast` (à l'utilisation : le comportement actuel). Pour les réactions (§3) : réception d'un statut (camp et statut), unité vaincue (camp), début de tour du porteur, coup critique. La liste finale est arrêtée au plan de la phase 2.
- **Condition** (briques, évaluées au moment de l'effet, par cible) : la cible a un statut, la cible a moins de X % de PV, le lanceur a un statut, un allié a un statut, le nombre de statuts de la cible ≥ N, la cible est prête / repoussée sur la frise.
- **Échelle** : la valeur d'un effet grandit selon un compte lisible (statuts sur la cible, ennemis vivants), avec un plafond obligatoire. Le compte « cartes d'un archétype équipées » n'existe qu'avec la phase 3 (§4).
- **Ciblage** : les 5 règles actuelles plus quelques variantes (l'ennemi aux PV les plus bas, au hasard, le porteur d'un statut donné).
- **Effets** : les 11 types actuels, inchangés ; un effet peut porter sa propre condition.

`SkillDef.Effects` reste disponible comme **vue de la capacité `OnCast`** pour que le reste du code et les outils continuent de compiler pendant la migration.

### 2. Archétypes (étiquettes)
Un **archétype** est une étiquette sur une carte (une ou deux par carte), fondée sur un statut ou un thème de la frise. Les archétypes sont un axe distinct de la famille (4), de la nature (4) et des étoiles (5).

Poignée initiale proposée (noms provisoires) :

| Archétype | Idée | Statuts / thème |
|---|---|---|
| Brûlure | dégâts dans la durée, bonus sur cible brûlée | Burn |
| Entrave | contrôle de la frise | Shock, Freeze, Bind, recul |
| Traque | marquer puis exécuter | Mark, Vulnerable, Corrosion, Exécution |
| Tempo | accélérer l'équipe | Advance, Haste, jauge |
| Rempart | encaisser et protéger | Shield, Anchor, Taunt |
| Renouveau | tenir dans la durée | Regen, Heal, Purge |

### 3. Réactions
Une carte peut **armer une réaction** quand on la joue : une capacité à déclencheur qui s'installe sur le lanceur (ou sur la cible alliée) pour N tours du porteur, comme un statut. Exemple : « Armer 3 tours : quand un ennemi reçoit Choc d'un allié, il subit 40 % ATQ. »

- Une réaction est affichée comme une icône de statut, expire comme un statut, et peut être retirée par `Purge` (côté ennemi).
- Aucun coût d'énergie : le coût est celui de la carte qui l'arme.
- **Anti-boucle** : une réaction ne répond pas à un événement produit par une réaction ; au plus un déclenchement par réaction et par événement.
- Les capacités à déclencheur ne sont jamais permanentes (pas de passif d'équipement) : le coût d'énergie et le choix du moment restent le cœur du jeu.

### 4. Les trois niveaux de synergie
1. **Carte ↔ carte (même agent)** : des conditions qui lisent les statuts posés par les autres compétences du même agent (Brûlure posée → l'autre carte frappe plus fort).
2. **Agent ↔ agent (escouade)** : les statuts d'un ennemi sont communs à toute l'escouade, donc conditions et réactions croisent les agents (un allié pose Choc, un autre en profite).
3. **Carte ↔ agent (léger)** : on conserve le bonus +15 % de position et +5 % de famille actuel. Rien de plus dans ce lot.

**Option proposée (phase 3, à confirmer)** : seuils d'archétype par escouade, sur le modèle de `SynergyDef` (par exemple 2 / 4 / 6 cartes d'un archétype équipées dans l'escouade donnent un petit bonus plafonné). Cela récompense le deck cohérent sans l'imposer.

### 5. Cohérence ou mélange
Le mélange reste viable : la valeur de base d'une carte reste dans le gabarit de puissance existant (par étoile), et la condition ou l'échelle est un **supplément** budgété (de l'ordre de +30 à +50 % quand elle est remplie, avec la base légèrement sous le gabarit). Une carte seule n'est jamais morte ; un groupe cohérent est nettement meilleur.

### 6. Interactions avec l'existant
- **Niveaux et paliers** : un palier ajoute des capacités (pas seulement des effets). `SkillAt` garde son comportement actuel pour les 48 cartes.
- **Puissance** : `CardBonuses.Apply`, `ScalePercent` et la puissance de compétence des objets s'appliquent aussi aux dégâts, soins et boucliers contenus dans les réactions.
- **Ennemis et boss** : mêmes structures, uniquement `OnCast` au départ.
- **IA Auto** : doit rester déterministe ; elle peut préférer une compétence dont la condition est remplie (règle simple, arrêtée au plan).
- **Combat (lisibilité)** : nouveaux événements (réaction armée, réaction déclenchée, bonus de synergie appliqué) ; un éclat de couleur et un libellé court, dans le style de « EXÉCUTION ! », indiquent pourquoi un effet est plus fort.
- **Textes** : la description d'une carte est **générée** à partir de ses briques (clés de texte par condition, déclencheur et échelle, en français et en anglais), plus de code de texte par carte.
- **Sauvegardes** : aucun changement de format ; les identifiants de carte et les niveaux sont conservés.

### 7. Phases (chacune aura son propre plan)
- **P0 — Structure et garde-fous** — faite le 2026-11-16. Introduire `Ability`, migrer les 48 cartes, les ennemis et les boss à l'identique. Garde-fou : combats graine fixe, journal d'événements identique avant et après sur tout le catalogue.
- **P1 — Conditions, échelle, ciblage, archétypes** — faite le 2026-11-16. Briques `OnCast`, étiquettes sur les 48 cartes, textes générés, une première vague d'environ 12 cartes (2 par archétype) pour éprouver le modèle.
- **P2 — Réactions** — faite le 2026-11-16. Déclencheurs, événements, icône de réaction, anti-boucle.
- **P3 — Seuils d'archétype** (optionnelle, voir §4) — faite le 2026-11-16 (échelle seulement, seuils non implémentés).
- **P4 — Contenu par lots, jusqu'à 250 cartes** — faite le 2026-11-16 (les 250 cartes en jeu d'un coup). Lots d'environ 40 cartes (un archétype ou les neutres à la fois), répartition par étoile, textes, **liste d'illustrations avec descriptifs remise au porteur du projet pour chaque lot (il crée les images)**, équilibrage en simulation après chaque lot. Environ 5 lots après la vague de P1.

## Tests (critères d'acceptation)
- **Garde-fou P0** : pour chaque carte (aux niveaux 1, 3, 6, 10) et pour les ennemis, journal d'événements identique à celui d'avant la migration, sur plusieurs graines.
- **Briques** : chaque condition, chaque règle d'échelle (avec plafond) et chaque ciblage sur graine fixe ; réaction : ordre de déclenchement, anti-boucle, expiration, purge, déterminisme (`LocalCombatVerifier`).
- **Contenu** : toute règle de contenu existante vérifiée aussi sur les capacités ; chaque carte a au moins un nom, une description générée et (après P1) un archétype ou « aucun » explicite ; aucune carte ennemie n'a d'effet allié.
- **Textes** : chaque brique a ses clés en français et en anglais.
- **Équilibrage (P4)** : en simulation automatique, une escouade d'un seul archétype ne dépasse pas une marge convenue de victoires sur une escouade mixte de même niveau, et inversement.

## À la charge du constructeur
Liste finale des déclencheurs et des conditions de P1/P2 ; profondeur exacte des chaînes (au plus une) ; règle de préférence de l'IA Auto ; forme de l'icône et de l'éclat de combat ; noms définitifs des archétypes ; format de stockage des capacités dans les données.

## Précisions issues de la relecture (appels du rédacteur, à contester)
- **Garde-fou P0.** Le journal d'événements ne trace pas les tirages du générateur. Avant tout changement, on fige les journaux de référence (dans une étape séparée) **et** on y ajoute l'état final du générateur (un accesseur en lecture, ou le prochain tirage). Une migration qui change l'ordre des tirages est ainsi détectée.
- **Ordre d'exécution.** Les capacités `OnCast` d'une compétence sont **concaténées dans l'ordre** (base, puis paliers) et passent dans la boucle `ApplyEffects` actuelle (cible par cible, effet par effet). C'est ce qui garantit l'identité des journaux. Conditions et échelles sont évaluées dans cette boucle, par cible, au moment de l'effet.
- **Ordre des multiplicateurs.** Valeur de base × niveau (+10 %/niveau) → bonus de position et de famille → puissance de compétence des objets, comme aujourd'hui sur dégâts, soins et boucliers. Le supplément d'une condition ou d'une échelle s'applique **ensuite**, sur la valeur déjà calculée, et n'est pas remis à l'échelle ; les effets ajoutés par un palier restent non augmentés. Arrondis entiers, comme `CardBonuses.Apply`.
- **Ennemis et boss.** Migrés vers la même structure en P0 (capacité `OnCast` seule). La vue de compatibilité `SkillDef.Effects` est **en lecture seule** : le seul outil qui la lit (`ContentCsvExport`) exporte en CSV et n'écrit rien.
- **Réactions et état.** Une réaction est rangée avec les statuts de l'unité. Les adversaires de duel de tournoi (`UnitFactory.Opponent`) passent par le même chemin que les alliés : ils **peuvent** armer des réactions, donc la Purge côté ennemi a un sens. Les ennemis de campagne n'en arment pas au départ. Les replays se rejouent par simulation à partir des commandes enregistrées, il n'y a pas d'état de combat sérialisé à migrer.
- **Résolution des réactions.** En file : elles se résolvent après l'action qui les a déclenchées, jamais en récursion. Plusieurs réactions sur le même événement : par numéro d'unité croissant, puis par ordre d'armement.
- **P3.** Les étiquettes d'archétype existent dès P1 ; la lecture « cartes d'un archétype équipées » (échelle ou seuil) n'existe qu'avec P3. Sans P3, la cohérence vient des conditions de statut.
- **Équilibrage.** Marge proposée pour P4 : en simulation automatique de duels miroirs de même niveau, chaque escouade d'archétype gagne entre 40 et 60 % contre une escouade mixte. Le gabarit de puissance par étoile est celui de `2026-11-10-rangs-positions-cartes-design.md` ; le budget de condition (+30 à +50 %) se règle au tableur au playtest.
- **Textes.** Je rédige les noms et descriptions FR/EN des cartes de P1, vous les relisez. Le générateur peut reformuler les 48 cartes existantes ; un test vérifie que chaque nombre et chaque statut de l'ancienne description figure dans la nouvelle.
- **Versions.** P0 ne change aucune règle : `RulesVersion` (Core) reste la même. À partir de P1, tout changement de contenu de carte incrémente `RulesVersion`. Les anciens replays ne sont pas conservés au-delà de la version courante.

## Questions ouvertes
- **Calendrier des 250 cartes (décidé le 2026-11-16)** : **toutes à la sortie**. P4 est donc un jalon de sortie, pas un flux continu. Conséquences : (1) la révision de l'économie des doublons devient un prérequis de sortie, à traiter avant le premier lot de P4 ; (2) les illustrations et les textes des 250 cartes entrent dans le chemin critique (le repli sur l'illustration générique reste permis pour la sortie, mais chaque carte sans image doit être listée) ; (3) l'équilibrage se fait en un seul passage de simulation sur le catalogue complet, en plus des passages par lot ; (4) le plan de live ops ne compte plus sur ces cartes comme contenu post-sortie.
- **Économie des doublons** : 25 doublons pour passer une carte au niveau 10 est calibré pour 48 cartes. Pistes à étudier (hors de ce document) : coût en doublons réduit, doublons convertis en monnaie de fabrication, ou niveau maximal plus accessible.

## Points tranchés par défaut (à contester)
- Les réactions s'arment en jouant une carte ; pas de passif d'équipement permanent.
- Six archétypes de départ (liste du §2).
- Mélange viable plutôt que decks obligatoires.
- Seuils d'archétype par escouade en option (P3), car le porteur du projet a demandé deux sortes de synergie plus un bonus léger carte-agent, pas ce troisième mécanisme.
- `SkillDef.Effects` conservé comme vue de compatibilité.
