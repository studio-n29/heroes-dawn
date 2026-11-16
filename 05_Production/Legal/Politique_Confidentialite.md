# Politique de confidentialité — Heroes Dawn (projet)

> **Projet à faire relire par un juriste avant publication.** Rédigé le 2026-11-16 à partir de ce que le code de la 1.0.0 fait réellement. Les champs entre crochets sont à compléter par le studio : ils ne sont pas connus du dépôt.
> Version anglaise : `Privacy_Policy_EN.md`. À publier à une adresse web publique (exigée par Google Play) et à lier depuis Réglages ▸ Compte et confidentialité.

Dernière mise à jour : [date de publication]

## 1. Qui sommes-nous

Heroes Dawn est édité par **[raison sociale de Studio N29, forme juridique, adresse, numéro d'immatriculation]** (« nous »), responsable du traitement.
Contact pour toute question ou demande : **[adresse e-mail de contact]**.

## 2. Données traitées et raisons

| Donnée | Quand | Pourquoi | Base légale (RGPD) |
|---|---|---|---|
| Adresse e-mail et identifiant de compte ; identifiant renvoyé par Google ou Apple si vous choisissez ces connexions | Si vous créez un compte (facultatif : le jeu se joue sans compte) | Vous connecter et retrouver votre progression sur un autre appareil | Exécution du contrat |
| Sauvegarde de jeu (progression, équipe, monnaies virtuelles, réglages, pseudo et tag de joueur) | En jeu ; envoyée en ligne seulement si vous avez un compte | Sauvegarde en ligne, synchronisation entre appareils | Exécution du contrat |
| Composition de votre équipe de tournoi, pseudo, points et classement | Si vous participez aux tournois avec un compte | Duels contre d'autres joueurs, classement visible des autres joueurs connectés | Exécution du contrat |
| Identifiants d'achat et de transaction | Si vous faites un achat intégré | Livrer l'achat, éviter un double octroi, reprendre un achat interrompu | Exécution du contrat, obligation légale (comptabilité) |
| Statistiques d'usage anonymes : écrans visités, combats, zones terminées, version du jeu, plateforme, événements de session | **Seulement si vous acceptez** la question posée à la première connexion | Comprendre comment le jeu est joué et l'améliorer | Consentement, retirable à tout moment |

Nous ne collectons ni votre nom, ni votre position, ni vos contacts, ni vos photos. Le jeu n'affiche aucune publicité et ne partage aucune donnée à des fins publicitaires. Nous ne vendons aucune donnée.

Les données de paiement (carte bancaire, etc.) sont traitées par Google Play ou l'App Store ; nous n'y avons pas accès.

## 3. Prestataires (sous-traitants)

| Prestataire | Rôle | Lieu |
|---|---|---|
| Supabase | Comptes, sauvegarde en ligne, tournois | Union européenne (région eu-central-1, Francfort) |
| Unity Technologies (Unity Gaming Services Analytics) | Statistiques d'usage, uniquement avec votre accord | [à confirmer : région de traitement choisie dans le tableau de bord UGS ; transferts hors UE encadrés par les clauses contractuelles types de Unity] |
| Google Play / Apple App Store (Unity IAP) | Achats intégrés | Selon le magasin |
| Google, Apple | Connexion « Se connecter avec Google / Apple », si vous la choisissez | Selon le fournisseur |

## 4. Durées de conservation

- Compte et sauvegarde en ligne : tant que le compte existe ; supprimés sur demande (section 6) ou après **[durée d'inactivité à fixer, par exemple 36 mois]**.
- Données de tournoi : captures d'équipe non utilisées au-delà de 60 jours pour les duels ; scores conservés pour l'historique des éditions jusqu'à suppression du compte.
- Statistiques d'usage : **[durée réglée dans UGS, à confirmer]**.
- Identifiants d'achat : durée exigée par les obligations comptables.

## 5. Mineurs

Heroes Dawn ne s'adresse pas aux enfants de moins de **[13 ou 16 ans, à trancher avec la classification d'âge]**. Nous ne collectons pas sciemment de données d'enfants en dessous de cet âge ; si vous pensez qu'un enfant nous a transmis des données, écrivez-nous pour les faire supprimer.

## 6. Vos droits

Vous pouvez demander l'accès à vos données, leur rectification, leur suppression, la limitation du traitement, leur portabilité, et vous opposer à un traitement. Vous pouvez retirer votre accord aux statistiques à tout moment dans **Réglages ▸ Compte et confidentialité**.

- **Supprimer votre progression** : Réglages ▸ Compte et confidentialité ▸ « Effacer la sauvegarde » (efface aussi la sauvegarde en ligne du compte).
- **Supprimer votre compte** : écrivez à **[adresse e-mail de contact]** depuis l'adresse du compte ; la suppression est faite sous 30 jours. *(Note interne : Google Play exige aussi une page web de demande de suppression ; une suppression depuis le jeu reste à développer, voir `Dossier_Conformite_1.0.0.md`.)*

Vous pouvez aussi saisir l'autorité de protection des données de votre pays (en France : la CNIL, cnil.fr).

## 7. Sécurité

Les échanges sont chiffrés (HTTPS, WSS). Chaque joueur n'accède qu'à ses propres données de compte (sécurité par ligne côté serveur). Les mots de passe sont gérés par Supabase Auth et ne nous sont jamais lisibles.

## 8. Modifications

Nous pouvons mettre à jour cette politique ; la date en tête fait foi et tout changement important est annoncé dans le jeu.
