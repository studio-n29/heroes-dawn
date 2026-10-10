# Activités jouables (sous-projet D)

## Objectif
Rendre jouables les 5 activités du framework : potion (2D), survie en neige (3D), pêche (3D), arène de combat (3D vue de dessus), dialogue (échec lié à une autre activité).

## Architecture
- **Logique pure** (`Domain/Board/Activities/Games/`) : `SnowSurvivalGame`, `FishingGame`, `ArenaGame`, `PotionGame`, `DialogueGame`. Déterministe à graine égale ; paramètres lus dans le niveau généré (`ActivityLevel`) et `BalanceConfig`.
- **Présentation** (`Presentation/Activities/`) : `StageGameView` (fenêtre unique des activités 3D : image de la scène, HUD, joystick, deux boutons, choix de l'agent), `ActivityStage` (primitives plates dans une scène dédiée rendue dans une RenderTexture), `StageDrivers` (un pilote par jeu), `PotionPopupView`, `DialoguePopupView`.
- `CasePopupPresenter.PlayActivityAsync` choisit la vue selon `ActivityDef.Kind` ; les autres types gardent la fenêtre générique.
- Entrées : joystick tactile + boutons (`StageInputWidgets`), clavier ZQSD/flèches, Espace, Maj.
- Textes : `LocStages` (potion, dialogue, HUD). Dialogues écrits pour `dialogue_dockers` seulement.

## Règles clés
- Survie : terrains neige (rapide, froid), herbe (lent), sable ; totems de chaleur ; victoire à moins de 1,3 m de l'objet.
- Pêche : lancer, attente, touche, tirer ; réussite = nombre de poissons requis dans le temps.
- Arène : l'agent choisi (niveau, compétences) affronte des vagues issues des marqueurs du niveau.
- Potion : étapes ingrédient/dose, essais limités ; échec = malus. Dialogue : réponses bonne / moyenne / mauvaise ; échec = activité liée.

## Tests
`ActivityGamesTests` (domaine), `ActivityGamePresenterTests` (présentation), `ActivityGamesPlayTests` (prefabs réels), `ActivityCaptureTests` (captures explicites).
