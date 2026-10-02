# Mise en route

Projet Unity : **`Heroes Dawn Unity/`** (Unity 6000.6.3f1, URP). **Tout se passe dans Unity** : jeu, tests, outils d'équilibrage et configuration du backend. Aucun projet .NET ni script hors du projet.

## Prérequis
- Unity 6000.6.3f1 (modules Android, Windows ; iOS sur Mac).
- **Git** et **Git LFS** dans le `PATH` Windows.

## 1. Ouvrir le projet
Unity Hub ▸ ouvrir `Heroes Dawn Unity`. Unity résout les packages (1-3 min).

## 2. Installer les packages du prototype
Menu **Heroes Dawn ▸ Setup ▸ 1. Installer les packages du prototype** (Addressables, Localization, 2D Animation, Newtonsoft Json, PrimeTween ; Input System et Test Framework sont déjà là). Attendre « Packages installés ».

## 3. Brancher les références
Menu **Heroes Dawn ▸ Setup ▸ 2. Brancher les références d'assemblies**.

## 4. Backend UGS
Les packages UGS sont installés (menu **Heroes Dawn ▸ Setup ▸ 3. Installer les packages backend (UGS)**). Le backend vit dans `Assets/_Project/Backend/`, voir son `README.md`. Pour déployer :
1. *Edit ▸ Project Settings ▸ Services* : lier le projet UGS du studio.
2. Choisir l'environnement `dev`.
3. *Window ▸ Deployment* ▸ **Deploy All**.

## 5. Équilibrage
- **Heroes Dawn ▸ Équilibrage ▸ Importer 05_Equilibrage.xlsx** : lit l'onglet « Paramètres » de `01_Game_Design/05_Equilibrage.xlsx`, puis écrit `Resources/Balance/balance.json` (chargé par le jeu) et `Backend/RemoteConfig/balance.rc`. Le tableur peut rester ouvert dans Excel.
- **Heroes Dawn ▸ Équilibrage ▸ Simulateur** : combats automatiques en série sur la zone 1.1, taux de victoire par rencontre, export CSV dans `Heroes Dawn Unity/BalanceReports/` (ignoré par Git).

## 6. Tests
- Dans l'éditeur : *Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All*.
- En ligne de commande (bash), **Unity fermé** sur ce projet :
```bash
"/c/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe" -batchmode -nographics -projectPath "C:/Users/dnath/Documents/Heroes Dawn/Code/Heroes Dawn Unity" -runTests -testPlatform EditMode -testResults "$TEMP/editmode.xml" -logFile "$TEMP/unity.log"
```
  Résumé : `grep -o '<test-run [^>]*' "$TEMP/editmode.xml"`. Un seul test : ajouter `-testFilter "<Namespace>.<Classe>.<Test>"`. Lancer un menu : `-executeMethod <Classe>.<Méthode> -quit`.
- Optionnel : supprimer `Assets/TutorialInfo` et `Assets/Readme.asset` (contenu d'exemple du modèle).

## 7. Git
Le dépôt est à la racine **`Documents/Heroes Dawn/`** (documentation + code). Les fichiers binaires (images, polices, xlsx, docx, pdf) passent par Git LFS.
