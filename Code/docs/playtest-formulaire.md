# Formulaire de retour de playtest (Google Forms)

Le jeu propose le formulaire à la première victoire de la zone 1.1. Les réponses partent vers un Google Forms ; il s'enregistre dans ton Google Drive et ses réponses arrivent dans un Google Sheets lié. Sans réglage, le retour reste local (fichier `playtest_feedback.txt` du téléphone + presse-papiers).

## 1. Créer le formulaire (2 minutes)

Dans Google Forms (compte du Drive du projet), crée un formulaire « Retour playtest Heroes Dawn » avec **5 questions de type réponse courte** (pas de liste) :

1. `fun` — C'était amusant ? (note 1 à 5)
2. `clarity` — Les écrans étaient clairs ? (note 1 à 5)
3. `difficulty` — La difficulté était juste ? (note 1 à 5)
4. `tutorial` — Le tutoriel t'a aidé ? (note 1 à 5)
5. `comment` — Commentaire (réponse longue)

Onglet **Réponses ▸ Lier à Sheets** : les réponses s'ajoutent dans un tableur du Drive.
Optionnel : une 6e question courte `version` pour la version du jeu.

## 2. Récupérer les numéros de champs

Menu ⋮ ▸ **Obtenir le lien prérempli**, remplis chaque champ avec un texte repérable (`a1`, `a2`…), clique **Obtenir le lien** et copie-le. Il ressemble à :

```
https://docs.google.com/forms/d/e/1FAIpQLSd…/viewform?usp=pp_url&entry.111111=a1&entry.222222=a2&entry.333333=a3&entry.444444=a4&entry.555555=a5
```

## 3. Renseigner le jeu

Édite `Assets/_Project/Resources/Playtest/PlaytestForm.txt` :

```
formResponse=https://docs.google.com/forms/d/e/1FAIpQLSd…/formResponse   (le même lien, /viewform… remplacé par /formResponse)
fun=entry.111111
clarity=entry.222222
difficulty=entry.333333
tutorial=entry.444444
comment=entry.555555
version=
```

Refais une build : à l'envoi, le jeu poste les réponses sur `formResponse` (sans compte) et affiche « Ton retour a bien été envoyé ». Si le réseau échoue, le rapport reste copié dans le presse-papiers.

## 5. Playtest de Moscou (parcours d'Agent M)

Le jeu propose aussi le formulaire à la **première validation de la zone M1.1** (une seule fois par profil), pour que les testeurs d'Agent M donnent leur retour sans passer par Brooklyn. Deux sessions de playtest réel sont prévues avant le gel du contenu (15/01/2027), avec un profil neuf choisissant **Agent M** à la première connexion.

À observer pendant la session (noter dans le commentaire du formulaire ou à côté) :
1. **Parcours** : carte de Moscou affichée (Sergueiv ouvert, trois quartiers « bientôt »), zones M1.1 à M1.4 dans l'ordre, aucune zone de Brooklyn proposée ni dans le journal de quêtes, ni dans le QG.
2. **Difficulté** : le Passeur (M1.4) doit se jouer entre 40 et 60 % de victoires ; l'élite de M1.4 (homme de main + deux gardes) est la plus dure : vérifier qu'elle n'est pas un mur au niveau 8 à 10. Tester avec **cartes équipées et AUTO** (non simulé).
3. **Narration** : les répliques s'enchaînent sans clé brute à l'écran en français et en anglais ; noms des locuteurs (Agent M, Ilona Varga, Homme de main, Le Passeur) corrects.
4. **Fin du chapitre** : après le Passeur, l'écran de l'acte suivant (Paris, « bientôt ») ne propose ni Brooklyn ni bouton mort ; la carte de Moscou reste consultable et les zones se rejouent.
5. **Souvenirs** : quatre fragments (page Chapitres, chapitre Sergueiv), débloqués zone par zone.
6. **Mini-jeux** : infiltration (M1.1), engrenages (M1.2), sauvetage sur les toits (M1.3), tri de caisses (M1.4) se lancent et se terminent.
