# Objets, grimoire et cartes de compétences (sous-projets L et M)

## L — Objets (2 par agent)
- `ItemDef` : rareté (Commun, Rare, Épique, Légendaire), bonus de PV/ATQ/DEF/VIT en %, effet spécial (départ avancé sur la frise, bouclier d'ouverture, critique), prix en Crédits. 10 objets au prototype.
- Inventaire en sauvegarde (`MetaSave.collection`) ; un objet équipé reste compté dans la réserve (disponible = possédé − équipé). `MemberSave.items` : 2 objets maximum par agent.
- En combat, `AllyEntry.Items` : bonus appliqués aux statistiques de l'unité, effets spéciaux à la création (rejeu identique : l'équipement fait partie de `CombatSetup`).
- Sources : boutique du jour (4 offres, rotation quotidienne déterministe, achat en Crédits), événements narratifs (`ItemFoundEvent`, chance `EventItemChancePercent`, tirage propre à la case), boosters/offres futures.

## M — Grimoire et cartes
- Compétences 1 et 2 : propres à l'agent. Compétence 3 : la sienne, remplacée par la **carte 1** équipée. Compétence 4 : la **carte 2** (exige la carte 1). `SkillSlot.A4`, 4 recharges, 4e icône sous la carte d'agent, touche 4.
- `SkillCardDef` : catégorie d'agent (famille), rareté, compétence. 12 cartes. Une carte d'une autre catégorie est refusée à l'équipement et ignorée en combat.
- Boosters : `BoosterDef` (3 cartes, poids 70/22/7/1, pitié tous les 5 sans Rare, 120 PR). Tirage déterministe (joueur + compteur d'ouvertures). Sources : exploit (victoire Élite), progression narrative (1re validation d'une zone), mini-jeu réussi (chance 50 %), boutique (PR).
- Pages : Équipement (depuis la fiche d'agent), Grimoire (depuis la Boutique ou l'Équipement) ; la Boutique affiche les objets du jour et le booster.
- Persistance : anciennes sauvegardes valides ; contenu inconnu, en trop ou d'une autre catégorie ignoré au chargement.

## Hors périmètre (à venir)
Niveau des cartes par doublons, échange d'objets, objets consommables, icônes illustrées.
