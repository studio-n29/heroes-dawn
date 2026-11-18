# Plan de publicité de lancement — Heroes Dawn

> Version du 2026-11-18. Date de lancement non fixée : tous les repères sont relatifs (**T** = jour de sortie). Budget de départ **2 000 €**, équipe non rémunérée. Les chiffres du modèle viennent de `Analyse_Financiere_2026-11-18.html` ; les montants de plateformes publicitaires viennent de blogs de régies et sont **à revérifier dans les aides officielles** avant de dépenser.

## 1. Objectif et règle de départ

Faire de la publicité **au lancement** sans brûler le budget, parce que :

- un joueur acquis par pub rapporte environ **0,36 € net sur 12 mois** dans le scénario réaliste, pour un coût d'installation de 3 € ou plus : la publicité ne se rembourse pas seule ;
- elle sert donc à **trois choses mesurables** : donner un pic de visibilité aux fiches des stores le jour de sortie, mesurer la rétention réelle (D1, D7) et tester quels visuels donnent envie ;
- le gros du travail de découverte vient des **créateurs, de Steam, de la communauté et des fiches de stores**, qui coûtent moins par joueur.

**Règle d'or** : on ne dépense davantage que si la valeur nette d'un joueur à 12 mois dépasse **1,5 fois** son coût d'installation. Avec les hypothèses actuelles, ce n'est pas le cas : la pub reste un test plafonné.

## 2. Budget (2 000 €)

| Poste | Montant | Détail |
|---|---|---|
| Publicité payante | 600 € | Un seul canal, deux semaines autour du lancement, plafond par jour |
| Créateurs de contenu | 700 € | 3 partenariats payés (200 à 250 € chacun) + clés gratuites pour une cinquantaine d'autres |
| Page Steam | 300 € | Capsule, bande-annonce, captures soignées (hors publicité Steam) |
| Fiches des stores | 200 € | Vidéo, captures localisées en 5 langues, retouches |
| Communauté | 200 € | Outils Discord, petit concours cosmétique (jamais de puissance), mise en forme des devlogs |
| **Total** | **2 000 €** | |

Les fourchettes de tarifs de créateurs trouvées en ligne vont de 200 à 1 000 $ par vidéo pour 10 000 à 100 000 abonnés, avec de fortes divergences selon les sources. Négocie, et privilégie les créateurs dont l'audience est déjà dans le RPG tactique, les super-héros ou l'univers de Hell Rising.

## 3. Canal publicitaire

**Un seul canal, pas trois** : avec 600 €, répartir la somme empêche toute optimisation.

| Canal | Budget quotidien minimal (selon les régies) | Lecture |
|---|---|---|
| Google (campagnes d'application) | environ 50 fois le coût cible par installation, soit 150 $ par jour à 3 $ | **Écarté** : 600 € tiendraient 4 jours |
| Meta (campagnes d'installation) | environ 7 à 10 fois le coût cible par jeu de pubs, soit 20 à 30 € par jour | **Choix par défaut** : 40 €/jour pendant 14 jours tiennent dans 600 € |
| TikTok (Smart+) | aucune fourchette confirmée, 7 jours d'apprentissage conseillés | **Alternative** si les vidéos y performent mieux |

À vérifier avant de lancer : la campagne doit pouvoir mesurer les installations et, idéalement, la fin du prologue. Cela peut demander d'intégrer le kit de la régie dans Unity ou un outil d'attribution.

**Pays** : concentre les 14 jours sur **un seul marché** (la France, où se trouvent tes créateurs, ta communauté et ta traduction à jour). Quelques centaines d'installations concentrées font monter la fiche dans un petit classement national ; dispersées, elles ne changent rien. Les autres langues viennent ensuite, quand tu as lu les résultats.

**Test avant le lancement (T − 30 à T − 15)** : 100 à 150 € pris sur le budget pub pour vérifier la rétention J1 et la qualité de l'onboarding. Si J1 est sous 25 %, ne lance pas la rafale : corrige d'abord.

## 4. Vidéos publicitaires

- 5 à 6 vidéos verticales de **15 secondes**, tournées dans le jeu (la publicité doit refléter ce que le joueur verra).
- Accroche dans les 2 premières secondes : combat sur la frise, un pouvoir signature, le recrutement d'un agent, le plateau d'une ville.
- Les 3 premières variantes testent **trois accroches différentes** sur la même fin ; on garde la meilleure après quelques centaines d'impressions.
- Fin de vidéo : nom du jeu, « Gratuit », logo du store.
- Pas de promesses qui n'existent pas (puissance à acheter, récompenses non livrées). Mentionne l'utilisation d'IA si la régie ou le pays l'exige.

## 5. Calendrier

| Quand | Action | Responsable |
|---|---|---|
| T − 60 j | Page Steam publique, serveur Discord ouvert, premiers devlogs, liste de 60 créateurs ciblés | Toi |
| T − 45 j | Fiche Google Play complète en 5 langues, vidéos publicitaires tournées | Toi |
| T − 30 à − 15 j | Test de rétention (100 à 150 €), correction de l'onboarding si D1 < 25 % | Toi |
| T − 21 j | Envoi des clés aux créateurs, accord de 3 partenariats payés | Toi |
| T − 7 j | Rappel aux créateurs, annonce aux lecteurs de Hell Rising, campagne Meta prête (désactivée) | Toi |
| **T** | Sortie Android et Steam. Campagne activée à 40 €/jour, vidéos des créateurs publiées | Toi |
| T + 1 à + 14 j | Suivi quotidien : coût par installation, D1, fin du prologue. Réponse aux avis | Toi |
| T + 14 j | Arrêt de la rafale. Bilan chiffré (voir §6) | Toi |
| T + 30 j | Premier jalon du plan live-ops : point D1 (J7 < 15 % = priorité rétention) | Toi |

**Steam** : un Next Fest n'est utilisable qu'**une fois par jeu**, avec une page publique « Bientôt disponible » et une démo jouable. La prochaine édition prévue serait en février 2027, dates non confirmées dans mes recherches. Vérifie sur Steamworks avant de planifier. Si ton lancement ne tombe pas dedans, remplace-le par un Playtest et un devlog hebdomadaire.

## 6. Indicateurs et règles de décision

| Indicateur | Objectif | Règle |
|---|---|---|
| Coût par installation | ≤ 3 € | > 6 € après 100 installations : on arrête la campagne |
| Rétention J1 | ≥ 30 % | < 25 % : on coupe la pub et on corrige l'onboarding |
| Rétention J7 | ≥ 10 % | < 15 % (cible du projet) : priorité rétention, jalon D1 |
| Fin du prologue | > 85 % | En dessous, l'entonnoir fuit : correction avant tout nouvel achat de trafic |
| Conversion payeurs | ≥ 2 % | < 1 % à M + 12 : revoir pass et cosmétiques (jalon D3) |
| Valeur nette à 12 mois / coût d'installation | ≥ 1,5 | Atteint : on monte progressivement. Sinon : la pub reste un test |
| Wishlists Steam par semaine | à suivre | Aucune cible fiable pour un jeu gratuit |

## 7. À préparer avant T − 30

- **Suivi** : événements analytics du prologue et de l'entonnoir (déjà prévus dans le plan d'analytics), plus la mesure de la source des installations pour la régie choisie.
- **Consentement** : le jeu demande déjà l'accord pour la mesure, avec refus par défaut. Vérifie que la mesure publicitaire respecte ce choix.
- **Musique** : confirme que chaque morceau a été généré avec un abonnement Suno payant, car les droits commerciaux en dépendent. Garde les preuves.
- **Nom du personnage « Adam West »** : renomme-le avant d'en faire des visuels publicitaires.
- **Réserve** : garde 10 % du budget (200 €) pour un créateur qui décolle ou une correction de dernière minute.

## 8. Décisions à prendre

1. **Date de lancement** : fixe un T pour passer du calendrier relatif à un calendrier réel.
2. **Budget** : confirme 2 000 € ou donne le chiffre définitif.
3. **Canal** : Meta par défaut, TikTok si tes vidéos y marchent mieux.
4. **Marché de lancement** : France, ou un autre selon où se trouve ton public.
5. **Créateurs** : liste de 60 noms, dont 3 pour des partenariats payés.

## Sources

- Budgets minimaux : [Playio, campagnes d'application Google](https://blog.playio.co/google-app-campaigns-mobile-games), [Segwise, campagnes d'installation Meta](https://segwise.ai/blog/meta-advantage-plus-app-campaigns-guide.md), [TikTok, bonnes pratiques Smart+](https://ads.tiktok.com/help/article/best-practices-for-smart-plus-app-campaigns/)
- Tarifs de créateurs : [Influencer Marketing Hub](https://influencermarketinghub.com/youtube-influencer-rates), [Fiverr](https://block.fiverr.com/resources/guides/digital-marketing/influencer-marketing-cost)
- Steam Next Fest : [Steamworks](https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest/2026june), [GamesRadar](https://www.gamesradar.com/games/steam-next-fest-guide)
