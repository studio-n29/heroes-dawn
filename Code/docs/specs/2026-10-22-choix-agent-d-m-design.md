# Choix Agent D / Agent M et parcours en quatre actes (morceau 3)

> Spec du 2026-10-22. Source : `02_Lore_Narration/03_Histoire_et_Actes.md` (sections 5 et 8) et `04_Agents_The_Dawn.md` (personnages joueurs). Implémentée le jour même.

## Objectif
À la première connexion, le joueur choisit Agent D ou Agent M. Le choix décide de son personnage dans l'équipe, de son équipe de départ et de l'ordre des quatre actes.

## Décisions
| Sujet | Décision |
|---|---|
| Écran de choix | L'étape « campagne » de la première connexion devient « choisis ton agent » : deux cartes (identifiant `agent_d`, `agent_m`). Aucune nouvelle vue : `ChooseCampaignAsync` est réutilisé |
| Parcours | `StoryRoute` (domaine) : D = New York, Paris, Moscou, Salem ; M = Moscou, Paris, New York, Salem. `StoryRoutes.For(id)` renvoie D pour un profil sans choix |
| Disponibilité | Agent D est jouable (Brooklyn). **Agent M est affiché « en création »** et refusé (`onboarding.campaign.unavailable`) : sa première ville, Moscou, n'a aucune zone. Il suffit de passer `available` à vrai dans `StoryRoutes.M` une fois les zones créées |
| Personnages joueurs | `agent_d` (Matière, Contrôle, Légende) et `agent_m` (Perception, Précision, Légende) dans `PrototypeAgents`, déblocage `UnlockKind.Hero` : jamais en boutique, absents du registre tant qu'ils ne sont pas dans l'équipe |
| Équipe de départ | Personnage joueur + 3 compagnons. D : Darius Brooks, Mei-Lin Chen, Freya Lindqvist. M : Sam Okafor, Céleste Moreau, Haruto Nakamura. Kwame Abara entre en boutique |
| Enregistrement | `OnboardingSave.hero` et `campaign` (= première ville). `GameSession.Route` ; `GameSession.StartWithTeam` remplace l'effectif au choix |
| Acte en cours | `StoryProgress.Current` : premier acte dont la ville n'est pas entièrement validée ; un acte sans zone n'est jamais terminé |
| Carte | Titre de page et bandeau d'acte suivent l'acte en cours (« New York · Acte 1 ») ; la carte dessinée reste celle de New York |

## Hors périmètre
Zones de Paris, Moscou et Salem ; carte de ces villes ; cinématique de 46 plans ; textes encore marqués « Guilde » ; quêtes par acte ; refonte du combat.

## Tests
`StoryRouteTests` (ordre des actes, défaut, personnages hors boutique, équipe, acte en cours), `OnboardingTests` et `OnboardingFlowTests` (choix, refus d'Agent M, enregistrement), `IntroPlayTests` (parcours complet). Edit Mode 681 réussis, Play Mode 53 réussis.
