# Dossier de conformité — 1.0.0 (Windows + Android)

> Préparé le 2026-11-16 pour la phase 6 du plan de stabilisation. **Document de travail, pas un avis juridique** : à faire relire par un juriste. Tout ce qui est écrit ici est tiré du code et de la documentation du dépôt ; ce que seul le studio peut faire est listé en fin de document.

## 1. Classification d'âge (IARC, via Google Play Console)

Le questionnaire IARC se remplit dans Play Console (Contenu de l'application ▸ Classification du contenu). Réponses préparées d'après le contenu réel de la 1.0.0 :

| Question | Réponse proposée | Justification |
|---|---|---|
| Catégorie | Jeu | |
| Violence | Oui : violence fantastique, non réaliste, sans sang ni démembrement | Combats au tour par tour entre méta-humains, effets de pouvoirs |
| Violence envers des personnages vulnérables, torture | Non | |
| Peur, horreur | Non (créatures « cryptides » stylisées, pas d'horreur) | À confirmer en revoyant les illustrations des repaires |
| Sexualité, nudité | Non | |
| Langage grossier | Non | À confirmer sur les dialogues de Brooklyn et de Moscou (relecture des textes en cours) |
| Drogues, alcool, tabac | Non | Le « contrebandier » de Moscou ne montre ni ne nomme de drogue : à confirmer à la relecture |
| Jeux d'argent simulés | Non | Aucun casino ni pari |
| **Achats intégrés** | **Oui** | 7 produits consommables (gemmes, offre de bienvenue, pass premium) |
| **Objets aléatoires payants (loot boxes)** | **Non, sous réserve (voir ci-dessous)** | Les boosters de cartes sont aléatoires mais se paient en PR, monnaie gagnée en jeu |
| Interactions entre joueurs | Oui, limitées : duels et classement ; pseudo visible ; **aucun chat ni échange de contenu libre** | Tournois JcJ |
| Partage de la position | Non | |
| Accès Internet non filtré | Non | |

Résultat attendu : **PEGI 7 ou PEGI 12** (violence fantastique), ESRB E10+ ou Teen, avec les mentions « Achats intégrés » et « Interactions en ligne ».

**Réserve sur les boosters.** `Obligations_Legales.md` affirme « pas de loot box (recrutement au choix) ». La 1.0.0 contient des **boosters de cartes à tirage aléatoire** (50 / 28 / 15 / 6 / 1 %, pitié à ★3). Vérifié dans le code le 2026-11-16 : ils se paient en **PR** (`CollectionBook`, `booster.PricePr`), jamais en gemmes ni en argent réel, et s'obtiennent aussi en récompense de zone. Tant que les PR ne s'achètent ni en gemmes ni en argent réel, ce ne sont pas des objets aléatoires payants et la réponse reste « Non ». Si une offre future vend des PR ou des boosters contre des gemmes, la réponse devient « Oui » : il faudra afficher les probabilités dans le jeu et retirer la mention « pas de loot box ». Afficher les probabilités dès maintenant reste une bonne pratique.

**R7 (thèmes sensibles du canon)** : le suicide et l'automutilation d'Eda ne sont ni montrés ni décrits dans le jeu (décision du registre des risques). À revérifier à la relecture des 4 fragments de Moscou et des dossiers d'agents.

## 2. Sécurité des données (formulaire « Data safety » de Google Play)

| Type de données | Collectée | Partagée | Facultative | Finalité |
|---|---|---|---|---|
| Adresse e-mail | Oui | Non | Oui (compte facultatif) | Gestion du compte |
| Identifiants d'utilisateur (identifiant de compte, tag de joueur) | Oui | Non | Oui | Gestion du compte, fonctionnalités de l'appli |
| Historique des achats | Oui | Non | Oui | Fonctionnalités de l'appli (livraison, anti double octroi) |
| Interactions avec l'appli (écrans, combats, zones) | Oui, **avec accord** | Non | Oui (opt-in, retirable) | Statistiques |
| Diagnostics (version, plateforme) | Oui, avec accord | Non | Oui | Statistiques |
| Autre contenu généré par l'utilisateur (sauvegarde, équipe de tournoi) | Oui | Non | Oui | Fonctionnalités de l'appli |
| Position, contacts, photos, messages, données financières, santé | **Non** | | | |

- Données chiffrées en transit : **oui** (HTTPS, WSS).
- Possibilité de demander la suppression des données : **oui**, par e-mail (voir la politique) ; « Effacer la sauvegarde » efface la progression locale et en ligne.
- **Manque à combler pour Google Play** : une application qui permet de créer un compte doit proposer la **suppression du compte depuis l'appli et depuis une page web**. La 1.0.0 efface le profil (`ResetProgressUseCase`) mais pas le compte Supabase lui-même. À faire avant la mise en ligne sur le Play Store (pas bloquant pour un APK de test distribué à la main) : bouton « Supprimer mon compte » + fonction serveur qui supprime l'utilisateur, et une page web de demande.

## 3. Contenus générés par IA (risque R3)

Mention proposée pour les fiches de store et les crédits du jeu :

> « Certaines illustrations, musiques et éléments d'interface de Heroes Dawn ont été créés avec l'aide d'outils d'intelligence artificielle générative, puis sélectionnés, retouchés et intégrés par Studio N29. L'histoire, les règles et le code sont l'œuvre du studio. »

- **Steam** (si la sortie PC passe par Steam) : questionnaire de contenu ▸ « contenu pré-généré par IA » : oui (illustrations, musiques) ; « contenu généré en direct » : non.
- **Google Play** : pas de déclaration dédiée pour du contenu pré-généré ; la politique sur les contenus générés par IA vise la génération en direct par l'utilisateur, absente du jeu.
- **Traçabilité** : les prompts et manifestes sont versionnés (`03_Art_UI_UX/Aube_Tactique_2026-11-09/manifest-*.json`, `prompts-*.json`, `03_Art_UI_UX/Prompts_*.md`). Les musiques viennent de Google Flow Music (2026-10-03), les effets sonores de Kenney (CC0).
- **À vérifier par le studio** : les conditions d'usage commercial de chaque outil à la date de génération (outil d'images utilisé pour « Aube tactique », Google Flow Music), et les conserver en PDF dans `06_References/`.

## 4. Autres points de la liste `Obligations_Legales.md`

| Point | État au 2026-11-16 |
|---|---|
| Consentement à la mesure (RGPD) | **Fait dans le jeu** : question unique à la première connexion, refus par défaut, réglage pour changer d'avis (`ConsentPrompt`, tests `AnalyticsTests`) |
| Politique de confidentialité | **Projet rédigé** (FR et EN, ce dossier) ; reste : compléter l'identité du studio, la faire relire, la publier, la lier dans les réglages |
| Écran d'âge (COPPA / mineurs) | **Non fait.** Si la cible déclarée est 13+ (ou 16+ dans l'UE), Play Console accepte une déclaration de public cible sans écran d'âge, à condition que le jeu ne vise pas les enfants. À trancher avec le juriste |
| Achats : 7 produits déclarés, prix TTC par pays, test en bac à sable | À faire dans Play Console (compte développeur requis) ; le code est prêt (`UnityIapGateway`, reprise d'achat testée) |
| Garde anti pay-to-win | Vérifiée par test (`NoPayToWinTests`) |
| Noms : « SCP » exclu (R8) | Vérifié le 2026-11-16 : aucune occurrence du mot dans le code, les données ni les UXML |
| « Adam West » (R9), marque « Heroes Dawn » | **À faire par le studio** : avis du juriste, recherche d'antériorité (INPI, EUIPO) |
| Langues | 1.0.0 en français et en anglais ; relecture de l'anglais par un natif à prévoir |

## 5. Ce que seul le studio peut faire

1. Compléter et faire relire la politique de confidentialité, puis la publier à une adresse publique.
2. Remplir le questionnaire IARC et le formulaire de sécurité des données dans Play Console avec les réponses ci-dessus, après avoir tranché le point des boosters.
3. Activer la protection contre les mots de passe compromis dans Supabase (Authentication ▸ Attack Protection).
4. Lier le projet Unity à un projet UGS pour que les statistiques consenties soient reçues.
5. Conserver les licences des outils d'IA ; consulter un juriste pour « Adam West » et la marque.
6. Signer l'AAB avec une clé d'envoi (l'APK de test est signé avec la clé de débogage).
