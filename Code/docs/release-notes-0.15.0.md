# Heroes Dawn 0.15.0 — notes de version

Branche `release/0.15.0`, version Unity 0.15.0 (code Android 1500). Quatorze lots (L1 à L14) intégrés par Git Flow dans `develop` ; L6 a été fusionné dans L14.

## Ce que le joueur voit

- **Personnage** : le joueur crée son agent (nom, avatar) à l'intro, joue hors ligne par défaut ; Agent D et Agent M mentorent hors de l'escouade ; l'équipe de départ compte Giulia Rossi, Ilona Varga et Lucas Mendes ; le tag n'existe qu'avec un compte.
- **Langues** : textes français et anglais, bascule dans les réglages.
- **Boutique, QG, atelier, profil** : boutique du jour, cartes et gemmes, QG en cartes et quêtes, atelier de fabrication, carte de profil avec statistiques réelles, favoris et succès.
- **Dialogues et boss** : dialogues sur dégradé tramé avec Suivant et Auto, entrée en combat par bandeaux, combats de boss à phases (Brick, Crocodile, Titan de l'AMIF).
- **Opérations** : missions avec fiche, difficulté, choix des agents et confirmation.
- **Mini-jeux** : un thème éducatif et un objectif pédagogique pour chaque jeu, rappelés à la réussite.
- **Souvenirs** : quatre fragments (un par zone de Brooklyn) lus depuis la page Chapitres ; « Audio à venir » tant que les voix manquent.
- **Repaire** : « Le coffre de l'AMIF », trois boss à la suite, page Repaire (préparation, étapes, bilan) ouverte par le bouton REPAIRE de la page Chapitres.
- **Tournoi** : JcJ simulé sur l'appareil (une page Tournoi depuis le Profil), trois manches par jour, classement factice.
- **Cinématique** : montage de lancement de 45,5 s sans texte ; la version longue (250 s) se rejoue par le bouton CINÉMATIQUE de la page Chapitres.

## Technique

- Migration Supabase `002_hd_profiles_durcissement.sql` appliquée sur N29 HQ (production) le 2026-11-01.
- Nouveaux champs de `MetaSave` (tous compatibles avec les anciens profils) : `chaptersSeen` étendu, `fragmentsRead`, `lairBosses`, `tournament`.
- Voix : `IVoicePlayer`, fichiers dans `Resources/Voices/fr` et `en` (voir `docs/cinematic-launch.md`).

## Limites connues à connaître avant de publier

- Aucun fichier de voix : les sous-titres et les textes font foi. La fiche de casting ElevenLabs et les voix FR/EN restent à fournir.
- Le montage de lancement joue la musique de cinématique actuelle (250 s) faute de piste courte et rythmée.
- Les textes des quatre fragments et du repaire sont rédigés en français uniquement et à relire par l'équipe.
- Les bonus de phase des boss sont à rééquilibrer au playtest.
- Recherche d'amis (liste limitée à 50) reportée.
- Les consignes d'Ingénierie de secours chevauchent le manuel à l'introduction (existant avant la 0.15.0).
- Le tournoi n'est pas un duel d'agents : l'issue dépend de la puissance de l'escouade ; matchmaking, classement réel et saisons arrivent en 0.16.0, de même que les autres repaires et leur calendrier.
- Réglage Supabase Auth « leaked password protection » encore désactivé (à activer côté tableau de bord).
- Le test PlayMode `FullRunTests` a un timeout intermittent non résolu.
- Rien n'est poussé : la branche, la fusion dans `main` et l'étiquette `v0.15.0` restent à faire sur demande.
