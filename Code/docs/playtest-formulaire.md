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
