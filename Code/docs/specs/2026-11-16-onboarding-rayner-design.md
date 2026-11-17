# Première connexion : Rayner, fiche d'identification, choix de la ville

> Statut : conception validée oralement par l'utilisateur le 2026-11-16 (pouvoirs : 4 familles, beaucoup de choix, présentés en tableau groupé par famille ; travail dans un worktree). Les points marqués **[mon choix]** sont des appels de l'implémenteur, à relire.

## But
Au premier lancement, à la place du simple écran « nom + avatar », une courte séquence à l'esprit de l'ouverture de Pokémon : Rayner explique vaguement l'univers de The Dawn, demande la fiche d'identification, la reprend rapidement, puis le joueur choisit sa ville de départ.

## Déroulé
1. **Rayner en Full Art** (portrait plein écran, bulle en bas, un toucher = réplique suivante) : 4 répliques d'univers, la dernière étant la question « Tu peux me remplir ta fiche d'identification pour les fichiers de The Dawn ? ».
2. **Fiche** : Nom, Prénom, Âge, Avatar (galerie existante), Pouvoir (bouton qui ouvre un tableau : une colonne par famille, un bouton par pouvoir).
3. **Rayner reprend la parole** (2 répliques, la première cite le prénom).
4. **Choix de la ville** : New York avec l'agent D, ou Moscou avec l'agent M.
5. Enregistrement dans le profil, puis QG.

## Données (profil, `OnboardingSave`)
`lastName`, `firstName`, `age`, `power` (identifiant du catalogue) en plus de `avatar`, `pseudo`, `campaign`, `hero`. Anciennes sauvegardes : champs vides ou 0 (JsonUtility).
**[mon choix]** Le prénom devient le nom du compte (`pseudo`) : il est court et sert au tag. Le nom de famille est gardé dans la fiche.
**[mon choix]** Âge entre 12 et 99.
**[mon choix]** Rien n'est enregistré avant le choix de la ville : un joueur qui quitte à mi-parcours revoit l'accueil.

## Pouvoirs
`HeroPowers` : 4 familles du lore (Matière, Élémentaire, Physique/Bio, Perception), 7 pouvoirs chacune (28), issus de `02_Lore_Narration/02_Profils_Pouvoirs.md` (+ quelques extensions : polymorphie totale, magnétisme, vapeur, précognition **[mon choix]**). Le pouvoir est un trait de personnage (profil, récit) ; il n'ajoute pas d'unité de combat. Les identifiants sont définitifs (sauvegarde).

## Villes et parcours
`StoryRoutes.D` (New York) est ouvert. `StoryRoutes.M` (Moscou) reste **fermé** (`Available=false`) : la carte Moscou le laisse grisée « BIENTÔT ». Il suffit de passer `Available` à vrai quand les plateaux de Moscou sont livrés ; le flux, la sauvegarde et les textes sont prêts (`campaign=moscow`, `hero=agent_m`).

## Textes
Français dans `LocOnboarding.cs` / `LocOnboardingRayner.cs`, anglais dans `WithOnboardingEnglish` (mêmes clés, test d'alignement). Ton de Rayner : tutoiement, phrases courtes, lore (Supernova il y a cinq ans, AMIF, Apex).

## Hors périmètre / à faire
- **Full Art dédié** : en attendant, l'illustration de cinématique `Resources/Cinematic/Art/rayner.jpg` (1672 × 941) sert de fond.
- Affichage de la fiche (nom, âge, pouvoir) sur la page Profil : non fait.
- Voix de Rayner : non faite.
- Captures et test visuel Unity : à faire quand Unity est libre (`IntroPlayTests.CaptureTheFirstConnection` mis à jour).
