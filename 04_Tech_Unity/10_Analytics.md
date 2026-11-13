# Analytics (UGS Analytics)

Décision du 2026-11-12 : mesure d'usage par **Unity Gaming Services Analytics**, opt-in, branchée sur les messages existants (aucun présentateur n'émet directement).

## Architecture

| Élément | Rôle | Fichier |
|---|---|---|
| `IAnalytics` + `AnalyticsEvents` | Port et noms d'événements | `Application/Ports/IAnalytics.cs` |
| `ConsentGatedAnalytics` | Ne transmet rien sans consentement ; démarre / arrête la collecte du service | `Application/Features/Analytics/Analytics.cs` |
| `AnalyticsBridge` | Traduit `GameStateChanged`, `CombatFinished`, `ZoneValidated` en événements ; suit les changements de réglage | idem |
| `NullAnalytics` | Éditeur et tests : aucune mesure | idem |
| `UgsAnalytics` | Service réel : initialise Unity Services et démarre la collecte **au premier consentement** ; muet si le projet Cloud n'est pas lié | `Infrastructure/Analytics/UgsAnalytics.cs` |

Branchement : `Composition/RootLifetimeScope.cs` (build joueur : `UgsAnalytics` ; éditeur : `NullAnalytics`).

## Consentement (RGPD)

- **Opt-in** : `SettingsSave.consentStats` vaut faux par défaut et `consentAnswered` marque une réponse explicite. La mesure exige les deux : un ancien profil dont le réglage valait « vrai » avant l'opt-in n'a **pas** consenti.
- Retrait : l'interrupteur « Partager des statistiques anonymes » (Réglages ▸ Compte et confidentialité) arrête la collecte (`StopDataCollection`).
- **Question de consentement** (`Presentation/Account/ConsentPrompt.cs`) : posée une seule fois par profil, à la première connexion, après la création du personnage et avant le QG. « Accepter » mémorise le oui ; « Refuser » et Retour mémorisent un refus ; la question ne revient pas. Textes `consent.*` (FR et EN).
- **Reste à faire** : l'âge déclaré (classification d'âge, voir `Obligations_Legales.md`) et le texte définitif validé par le juridique ; la politique de confidentialité doit citer UGS.

## Événements

Paramètres communs ajoutés par `UgsAnalytics` : `build` (version), `platform`. UGS enregistre en plus ses événements standard (`sessionStart`, `newPlayer`, `gameRunning`...) dès que la collecte démarre.

| Événement | Source | Paramètres |
|---|---|---|
| `state_entered` | `GameStateChanged` | `state` (Boot, Hub, Board, Combat), `from` |
| `combat_finished` | `CombatFinished` | `victory`, `boss`, `rank`, `credits` |
| `zone_validated` | `ZoneValidated` | `first_clear`, `credits`, `pr` |
| `consent_changed` | réservé (non émis pour l'instant) | `granted` |

### À ajouter (entonnoir de `05_Production/Live_Ops.md`)
Prologue étapes 1 à 5, premier recrutement, premier achat (IAP), abandon (sortie du jeu depuis un écran), erreurs et performances (images/s moyennes, temps de chargement). Chaque ajout passe par un message existant ou un nouveau message, jamais par un appel depuis un présentateur.

## Mise en service (actions du propriétaire)

1. Lier le projet Unity à un projet UGS (Édition ▸ Project Settings ▸ Services, ou Unity Dashboard) ; `ProjectSettings.cloudProjectId` est vide aujourd'hui. Créer les environnements `dev` et `production`.
2. Dans le Dashboard Analytics : activer la collecte, créer les tableaux de bord (entonnoir, rétention J1/J7/J30, durée de session).
3. Politique de confidentialité : mentionner la mesure UGS (voir `Obligations_Legales.md`).

Tant que l'étape 1 n'est pas faite, `UgsAnalytics` journalise un avertissement unique et reste muet : le jeu n'est jamais bloqué.

## Tests

`Tests/Domain/Application/AnalyticsTests.cs` : aucun événement sans consentement, collecte démarrée une seule fois, retrait du consentement, opt-in par défaut, traduction des messages, pas de doublon à un second démarrage.
Non testé ici : l'envoi réel vers UGS (exige un projet lié et un appareil).
