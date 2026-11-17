# Aperçu des effets d'une carte en combat (dry-run) — design

Validé avec l'utilisateur le 2026-11-16.

## But
Quand le joueur prend une carte (arme) ou la pose sur une cible, montrer ce que cela changerait sur tout le terrain : PV et boucliers,
statuts ajoutés ou retirés, unités vaincues, déplacements sur la frise d'initiative (reculs, avances, action ennemie annulée), issue du combat.

## Non-buts
- Ne jamais modifier le comportement du moteur (garde-fou combat-golden inchangé) ni le combat réel.
- Pas d'aperçu au-delà du pas qui suit la carte (pas de projection des tours suivants).
- Pas d'aperçu en mode Auto, ni en duel de tournoi (commandes appliquées hors frise).

## Approches considérées
1. **Rejeu + échantillons (retenue)** : copie du combat reconstruite depuis la graine et le journal (comme CombatReplay), commande hypothétique, un pas ; répété sur 16 flux d'aléa. Aucune logique dupliquée, robuste aux nouveaux états du moteur (tout est rejoué).
2. Clone profond de la simulation : plus rapide mais casse en silence dès qu'un état est ajouté au moteur (la session des capacités en ajoute).
3. Estimation statique à partir des définitions de cartes : duplique ciblage, conditions, échelles, réactions ; fausse dès qu'une règle change.

## Moteur
- `CombatPreview.Compute(catalog, live, commande, samples=16)` (Domain.Combat) : null si le combat n'attend pas une compétence alliée, si le rejeu ne retombe pas sur l'état réel (garde-fou), ou si la commande est impossible.
- Le premier échantillon suit le flux d'aléa réel (c'est donc le vrai résultat) ; les autres tirent d'autres flux.
- Seule modification du moteur : une surcharge `internal` du constructeur de `CombatSimulation` qui enveloppe le générateur (flux par défaut identique).
- Résultat par unité : PV avant/après (moyenne, min, max), bouclier, énergie, point de frise (moyenne, min, max), chance de défaite, annonce annulée, statuts ajoutés/retirés avec leur chance ; issue : victoire, vague suivante, défaite.

## Affichage (DA « Aube tactique »)
- Valeur moyenne + badge « variable » (choix de l'utilisateur), pas de plages chiffrées.
- Fiche d'unité : barre de PV fantôme (rouge = perte, vert = gain), delta (« −28 », « ≈ −27 »), badge « variable », statuts prévus (+Gel, −Bouclier, « ? » si possible), K.O.! / K.O. ?, action annulée.
- Frise : fantôme de position (losange en contour) relié à la place actuelle, texte « ◀ 3 » / « ▶ 2 » (« ≈ » si variable), ✕ pour une unité vaincue.
- Bandeau d'issue : VICTOIRE !, Victoire possible, VAGUE SUIVANTE.
- Déclenchement : au glisser d'une carte (armement) pour les cartes sans cible ; au survol d'une unité pendant le glisser pour les cartes ciblées ; effacé au lâcher.
- Textes FR/EN : clés `preview.*`.

## Hors périmètre / suites possibles
Aperçu au survol souris d'une carte ou d'une unité sans glisser ; aperçu pour le tir au clavier (touches 1 à 4).
