# Règles et conventions de code

## Les 12 règles d'architecture
1. **Direction des dépendances** : Présentation → Application → Domaine → Core. Jamais l'inverse (garanti par les `.asmdef`).
2. **Domaine pur** : aucun `using UnityEngine` dans `Core`, `Definitions`, `Domain.*`, `Application`.
3. **Déterminisme** : règles de `05_Simulation_Deterministe.md` (pas de `float`, RNG injecté, ordre stable).
4. **Serveur autoritaire** : toute dépense, récompense ou fin de timer passe par un use case qui appelle Cloud Code.
5. **Un use case par intention**, un agrégat modifié de façon autoritaire par appel serveur.
6. **Vues passives** : une vue n'a ni état de jeu ni référence à la session.
7. **Événements publiés après l'état** : jamais d'événement « optimiste » sur une donnée autoritaire.
8. **Pas de singleton** ni d'accès global (`Instance`, `FindObjectOfType`, `GameObject.Find`, `static` mutable). Tout passe par l'injection.
9. **ScriptableObjects = données d'édition** : jamais modifiés à l'exécution ; convertis en `Definitions` immuables au chargement.
10. **Contenu = données** : ajouter une recrue, une case ou un événement ne demande aucun code.
11. **Textes localisés uniquement** : aucune chaîne visible en dur.
12. **Tout est annulable** : chaque méthode asynchrone reçoit un `CancellationToken` lié au scope.

## Nommage
| Élément | Convention | Exemple |
|---|---|---|
| Espace de noms | `N29.HeroesDawn.<Couche>.<Module>` | `N29.HeroesDawn.Domain.Board` |
| Classe, méthode, propriété | PascalCase | `ReachabilityRules.FindDestinations` |
| Champ privé | `_camelCase` | `_session` |
| Interface | `I` + nom | `IBoardGateway` |
| Use case | Verbe + nom + `UseCase` | `ValidatePalierUseCase` |
| Message du bus | Fait au passé | `ZoneValidated` |
| Commande de combat | Impératif | `UseSkillCommand` |
| Vue / présentateur | `<Écran>View`, `<Écran>Presenter` | `RecruitmentView`, `RecruitmentPresenter` |
| Méthode asynchrone | Suffixe `Async` | `OpenZoneAsync` |
| Définition de contenu | Suffixe `Def` ; SO d'édition : suffixe `Asset` | `MemberDef`, `MemberAsset` |
| Identifiants de contenu | `snake_case` stables, jamais renommés | `member_verre`, `skill_givre_a3`, `zone_ny_1_1` |

## Style C#
- C# 9 dans le code partagé ; `sealed` par défaut ; `readonly struct` pour les petits types de valeur ; `record` pour les messages et les définitions.
- Un type public par fichier ; fichier ≤ 300 lignes (au-delà : découper).
- Nullable activé dans le code partagé (`#nullable enable`).
- Pas de LINQ dans les chemins chauds (tick de combat, `Update`, rendu de listes).
- Pas de `async void` (sauf gestionnaires d'événements Unity, avec `Forget()` UniTask et capture d'erreur).
- Pas de `Update()` dans les vues sauf besoin d'animation continue ; préférer les tweens et les événements.
- Journalisation via `ILogger` (niveaux Trace → Error) ; `Debug.Log` interdit en dehors de l'adaptateur.
- Commentaires : expliquer le **pourquoi** ; les règles de game design citent la spec (`// GDD §5 — arrêt forcé sur case Combat`).

## Gestion des erreurs
- Échec métier → `Result`/`DomainError` (code + clé de localisation).
- Bug → exception ; jamais attrapée silencieusement.
- Réseau → géré par les adaptateurs (tentatives, délais) ; l'Application ne voit que `Result`.

## Git
- Dépôt unique (Unity + Server + Tools), **Git LFS** pour les binaires (png, psd, wav, fbx…).
- Branche `main` toujours jouable ; branches courtes `feat/…`, `fix/…`, fusionnées par pull request.
- Messages de commit : `type(scope): message` (`feat(board): dé bonus`, `fix(combat): ordre des statuts`).
- Versions : `majeur.mineur.correctif` + numéro de build ; `RulesVersion` incrémentée à chaque changement du code partagé ou du contenu.

## Checklist de revue (à chaque pull request)
- [ ] Aucune dépendance vers une couche supérieure ; pas de `UnityEngine` dans le code pur.
- [ ] Aucune règle de jeu dans une vue ou un présentateur.
- [ ] Toute dépense/récompense passe par Cloud Code.
- [ ] Nouvelle règle de simulation → test unitaire + test de rejeu.
- [ ] Nouveau contenu → validateur de contenu vert.
- [ ] Textes localisés, pas de chaîne en dur.
- [ ] Abonnements au bus libérés (scope ou `Dispose`).
- [ ] Pas d'allocation dans le tick de combat (vérifié au profileur si modifié).
- [ ] Spec ou journal des décisions mis à jour si le comportement change.
