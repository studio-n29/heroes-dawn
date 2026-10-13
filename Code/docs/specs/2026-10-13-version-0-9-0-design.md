# Version 0.9.0 : retours du playtest 0.8.1

## Source
`Playtest 0.8.1.pdf` : bugs constatés et objectifs de la 0.9.0.

## Livré en 0.9.0

### Bugs
- **Tutoriel bloqué à « Avancer »** : le voile bloquant empêchait de toucher « S'y rendre » sur la fiche de la case. L'étape ne pose plus de voile (anneau et bulle seulement).
- **Cartes jouées d'un simple appui** : un appui ne lance plus la carte ; elle se joue au glisser-déposer (ou avec les touches 1 à 4). Le maintien affiche toujours sa description.
- **Déplacement** : on peut viser une case au-delà d'un combat non résolu (brouillard compris). Le pion s'arrête sur le combat, garde ses PM, puis rejoint la destination choisie après la victoire ; après une défaite il recule et la destination est abandonnée.
- **Cases accessibles** : contour jaune animé (pulsation, fixe en mouvement réduit).
- Objectifs et barre de PM du plateau sans chevauchement.

### Synergie à la place des combos
- Une paire de familles dans l'escouade donne à toute l'équipe un bonus permanent de statistique (PV, attaque, défense ou vitesse), plafonné à 30 % par statistique. Dix synergies (six croisées, quatre résonances de famille).
- Suppression des combos (commande, événements, bouton, rencontre sur la frise) et des compétences du joueur (frappe, soin, barrière, élan, recul), donc de la fenêtre d'interruption et de ses pauses : le combat est plus fluide.
- Équipe, fiche d'agent et recrutement affichent les synergies ; le journal de combat les annonce au début.

### Première connexion
- Popup plein écran jouée à la première connexion d'un profil qui n'a rien validé : cinématique de 9 écrans (fondus au noir, zoom lent, braises, bandes de cinéma, texte qui s'écrit, « Passer »), pseudo demandé sous forme de dialogue avec l'accueil de la Guilde (3 à 16 caractères), choix de la campagne (New York ouverte, Paris, Tokyo et Le Caire « en création »).
- Le pseudo remplace le nom d'invité du compte et reste dans le profil.
- Illustrations provisoires (motifs dessinés par code) ; les images dessinées se déposent dans `Resources/Intro/intro_01` à `intro_09`.

### Comptes (architecture)
- Port `IAccountGateway` (inscription et connexion e-mail / mot de passe) et `LocalAccountGateway` en mémoire. Branchement Supabase décrit dans `Backend/README.md` ; il demande l'URL et la clé `anon` du projet.

### Suivi des quêtes du QG
- La carte « Quête principale » du QG ouvre le suivi : quatre quêtes principales (les zones de Brooklyn : verrouillée, en cours, terminée) et des quêtes secondaires (six, débloquées au fil des zones validées, avec progression et récompense à réclamer). Pastille sur la carte quand une récompense attend.

## Pas encore fait (prévu après la 0.9.0)
- Notifications push (rappels, fins de missions, boutique, offres) : demande la configuration Android et le service de notification.
- Connexion et création de compte avec écrans e-mail : demande le projet Supabase.
- Nouvelles interfaces des objets, des agents et du camp d'entraînement ; mise à jour des mini-jeux et de l'interface du QG.
- Illustrations des agents de New York, des compétences et de la cinématique ; nouvelles compétences.
- Narration complète de New York au-delà des quatre zones de Brooklyn ; quêtes secondaires narratives (aujourd'hui des compteurs).
