# Mise en route

Projet Unity : **`Heroes Dawn Unity/`** (Unity 6000.6.3f1, URP). Le squelette (assemblies, dossiers, package partagé, dépendances Git) est déjà posé.

## Prérequis
- Unity 6000.6.3f1 (modules Android, Windows ; iOS sur Mac).
- **Git** et **Git LFS** dans le `PATH` Windows (VContainer, UniTask et MessagePipe sont récupérés par Git).
- .NET SDK 8 (tests hors Unity et simulateur).

## 1. Ouvrir le projet
Unity Hub ▸ ouvrir `Heroes Dawn Unity`. Unity résout les packages Git (1-3 min).

## 2. Installer les packages du prototype
Menu **Heroes Dawn ▸ Setup ▸ 1. Installer les packages du prototype** (Addressables, Localization, 2D Animation, Newtonsoft Json, PrimeTween ; Input System et Test Framework sont déjà là). Attendre « Packages installés ».

## 3. Brancher les références
Menu **Heroes Dawn ▸ Setup ▸ 2. Brancher les références d'assemblies**.

## 4. Vérifier
- *Project Settings ▸ Player ▸ Active Input Handling* : **Input System Package (New)** (déjà le cas avec ce modèle).
- *Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All* : `Skeleton_Compiles` vert.
- `dotnet test Tools/SharedTests` : vert.
- Optionnel : supprimer `Assets/TutorialInfo` et `Assets/Readme.asset` (contenu d'exemple du modèle).

## 5. Git
```bash
git init
git lfs install
git add .
git commit -m "chore: squelette du projet Heroes Dawn (M1)"
```

## Plus tard (M2)
Menu **Heroes Dawn ▸ Setup ▸ 3. Installer les packages backend** (Unity Gaming Services, IAP).
