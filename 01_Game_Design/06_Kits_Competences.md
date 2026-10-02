# Kits de compétences — New York (v1)

> Chaque recrue : **3 actives** (A1 de base, A2 utilitaire, A3 signature) + **1 passive** (P).
> Dégâts exprimés en % de l'Attaque. Recharge (R) en tours du personnage. A1 n'a pas de recharge.
> Valeurs de départ, à équilibrer avec le simulateur de combat (voir `04_Tech_Unity/01_Architecture.md`).

## Règles communes

### Paliers (niv. 10 / 20 / 30 / 40 / 50)
| Palier | Effet |
|---|---|
| 10 | A1 : +15 % de dégâts ou d'effet |
| 20 | P : effet renforcé |
| 30 | A2 : recharge −1 tour |
| 40 | A3 : +20 % de dégâts ou d'effet |
| 50 | A3 : effet additionnel (précisé par personnage pour Élites et Légendes ; Agents : recharge −1) |

### Rareté
| Rang | Complexité du kit |
|---|---|
| Agent | 1 effet par compétence, lisible |
| Élite | 1-2 effets, une interaction avec une altération ou la jauge ATB |
| Légende | Mécanique propre (charge, posture, marque…) qui structure l'équipe |

### Altérations (glossaire)
| Altération | Effet | Durée type |
|---|---|---|
| Brûlure | Dégâts à chaque tour (5 % PV max) | 2 tours |
| Gel | Jauge ATB remplie 50 % moins vite | 2 tours |
| Choc | Étourdi : saute son prochain tour | 1 tour |
| Corrosion | Défense −30 % | 2 tours |
| Aveuglement | 50 % de chances de rater | 2 tours |
| Vulnérabilité | Dégâts subis +25 % | 2 tours |
| Entrave | Ne peut pas utiliser A2/A3 | 1 tour |
| Marque | La prochaine attaque reçue est un coup critique | Jusqu'au coup |
| Provocation | Doit attaquer le lanceur | 1 tour |
| Bouclier | Absorbe des dégâts | 2 tours |
| Régénération | Soigne 8 % PV max par tour | 2 tours |
| Hâte | Jauge ATB remplie 30 % plus vite | 2 tours |
| Esquive | Évite la prochaine attaque | Jusqu'à l'attaque |

**Jauge ATB** : certaines compétences ajoutent ou retirent directement des points de jauge (« +20 % ATB »).

---

## Légendes

### Ancre — Nia Okafor · Physique/Bio · Tank
- **A1 Poing lesté** : 110 % à 1 ennemi, 30 % de chances de Provocation.
- **A2 Ancrage** (R3) : Provocation sur tous les ennemis + Bouclier (20 % de ses PV max) sur elle.
- **A3 Quai inébranlable** (R5) : pendant 2 tours, subit 50 % des dégâts destinés aux alliés et ne peut pas être Choquée ni repoussée.
- **P Poids mort** : chaque coup reçu donne 1 charge de *Lest* (max 5) ; A1 inflige +10 % par charge et les consomme.
- **Palier 50** : A3 renvoie aussi 20 % des dégâts absorbés.

### Écho — Kenji Arai · Perception · Détection
- **A1 Clic** : 100 % à 1 ennemi, applique Marque.
- **A2 Sonar** (R3) : révèle et retire Esquive/camouflage de tous les ennemis ; alliés +20 % de précision (2 tours).
- **A3 Résonance** (R5) : 70 % à tous les ennemis ; +100 % supplémentaires sur les ennemis Marqués.
- **P Monde sonore** : immunisé à l'Aveuglement ; ses attaques ne ratent jamais.
- **Palier 50** : A3 rend l'A2 immédiatement disponible.

### Verre — Rosa Delgado · Matière · Contrôle
- **A1 Éclat** : 100 % à 1 ennemi ; 25 % de chances de Vulnérabilité.
- **A2 Vitrification** (R4) : 1 ennemi devient *Cristal* : Entrave + ne remplit plus sa jauge (1 tour) ; le prochain coup reçu le brise (+50 % de dégâts).
- **A3 Cathédrale de verre** (R5) : Bouclier (15 % PV max de Verre) sur tous les alliés ; à l'expiration, le verre éclate et inflige 60 % à tous les ennemis.
- **P Formation militaire** : au début du combat, l'allié le plus faible reçoit un Bouclier.
- **Palier 50** : A2 s'applique à 2 ennemis.

### Givre — Tomas Reyes · Élémentaire · Dégâts
- **A1 Morsure du froid** : 100 % à 1 ennemi, applique Gel (40 %).
- **A2 Souffle polaire** (R3) : 70 % à tous les ennemis, −15 % ATB.
- **A3 Zéro absolu** (R5) : 220 % à 1 ennemi ; si Gelé, il est aussi Choqué.
- **P Contre-feu** : dégâts +30 % contre les cibles en Brûlure ; retire la Brûlure des alliés au début de son tour.
- **Palier 50** : A3 inflige 50 % des dégâts aux autres ennemis.

### Instant — Amara Diallo · Perception · Soutien offensif
- **A1 Frappe précise** : 100 % à 1 ennemi ; +30 % si la cible a moins de jauge ATB qu'Instant.
- **A2 Seconde suspendue** (R4) : Hâte sur tous les alliés + 20 % ATB.
- **A3 Temps figé** (R6) : Instant et 1 allié rejouent immédiatement.
- **P Réflexes** : 25 % de chances d'Esquive au début de chaque tour.
- **Palier 50** : A3 cible 2 alliés.

---

## Élites

### Maillon — Diego Salazar · Matière
- **A1 Chaîne** : 100 %, 20 % d'Entrave.
- **A2 Ligotage** (R3) : Entrave sur 2 ennemis.
- **A3 Carcan d'acier** (R5) : 180 % à 1 ennemi, −40 % ATB.
- **P Ferraille** : +10 % de défense par ennemi Entravé.
- **Palier 50** : A3 applique Entrave.

### Argile — Hannah Kowalski · Matière
- **A1 Projection de béton** : 100 %.
- **A2 Muraille** (R4) : Bouclier (20 % PV max) sur l'allié le plus faible + Provocation sur Argile.
- **A3 Effondrement** (R5) : 120 % à tous les ennemis, 30 % de Choc.
- **P Fondations** : immunisée au Choc.
- **Palier 50** : A2 protège 2 alliés.

### Mue — Jalen Brooks · Matière
- **A1 Fouet élastique** : 90 % à 1 ennemi, frappe 2 fois si la cible est Entravée.
- **A2 Contorsion** (R3) : Esquive sur Mue + retire une altération.
- **A3 Étreinte** (R5) : 160 %, Entrave (2 tours).
- **P Souplesse** : 15 % d'Esquive permanente.
- **Palier 50** : A3 retire les bonus de la cible.

### Arc — Priya Nair · Élémentaire
- **A1 Décharge** : 100 %, 20 % de Choc.
- **A2 Surtension** (R3) : Hâte sur 1 allié + 15 % ATB.
- **A3 Arc électrique** (R5) : 130 % à 1 ennemi puis rebondit sur 2 autres (70 %).
- **P Conductrice** : +25 % de dégâts contre les cibles en Gel (eau glacée conductrice).
- **Palier 50** : A3 rebondit sur 3 ennemis.

### Acide — Victor Hale · Élémentaire
- **A1 Crachat corrosif** : 100 %, Corrosion (40 %).
- **A2 Flaque** (R4) : Corrosion sur tous les ennemis.
- **A3 Réaction exothermique** (R5) : 170 % à 1 ennemi ; Brûlure si Corrodé.
- **P Catalyse** : ses Brûlures durent 1 tour de plus.
- **Palier 50** : A3 touche aussi un 2e ennemi.

### Rempart — Samuel Okoye · Physique/Bio
- **A1 Coup de hache** : 110 %.
- **A2 Sauvetage** (R3) : Bouclier (25 % PV max) sur 1 allié, retire Brûlure.
- **A3 Traversée des flammes** (R5) : Provocation + réduction de dégâts 40 % (2 tours).
- **P Ignifugé** : immunisé à la Brûlure.
- **Palier 50** : A3 soigne l'équipe de 10 %.

### Sève — Lucia Ferreira · Physique/Bio
- **A1 Toucher** : 80 % ; soigne l'allié le plus blessé de 50 % des dégâts infligés.
- **A2 Transfusion** (R3) : soigne 1 allié de 30 % PV max.
- **A3 Régénération partagée** (R5) : Régénération sur toute l'équipe.
- **P Vocation** : ses soins sont +25 % sur les alliés sous 30 % PV.
- **Palier 50** : A3 retire une altération de chaque allié.

### Gecko — Tyler Nguyen · Physique/Bio
- **A1 Coup plongeant** : 110 %.
- **A2 Paroi** (R3) : Esquive + 20 % ATB.
- **A3 Chute libre** (R5) : 200 % à 1 ennemi ; ignore Provocation.
- **P Prise** : ne peut pas être ralenti (immunisé au Gel).
- **Palier 50** : A3 réinitialise l'A2.

### Spectre — Imani Carter · Perception
- **A1 Frappe invisible** : 100 %, +50 % si Spectre a Esquive.
- **A2 Fondu** (R3) : Esquive + ne peut pas être ciblée (1 tour).
- **A3 Exposition** (R5) : 220 % à 1 ennemi, coup critique garanti.
- **P Angle mort** : +20 % de chances de critique contre les cibles Aveuglées.
- **Palier 50** : A3 rend Spectre invisible 1 tour.

### Boussole — Rafael Ortiz · Perception
- **A1 Impulsion** : 90 %, −10 % ATB.
- **A2 Brouillage** (R4) : désactive les drones et gadgets ennemis (AMIF, culte) + Entrave sur 1 ennemi mécanique.
- **A3 Itinéraire** (R5) : +25 % ATB à toute l'équipe.
- **P Nord magnétique** : immunisé à l'Aveuglement ; +20 % de dégâts contre les ennemis mécaniques.
- **Palier 50** : A3 donne aussi Hâte.

---

## Agents
| Agent | Famille | A1 | A2 (R3) | A3 (R5) | Passive |
|---|---|---|---|---|---|
| Rivet | Matière | Plaque projetée 100 % | Bouclier 15 % sur 1 allié | Blindage : Bouclier 10 % sur l'équipe | +10 % défense |
| Mortier | Matière | Coup de béton 100 % | Provocation + défense +20 % | Prise du béton : 120 %, −30 % ATB | Immunisé au Choc |
| Filin | Matière | Câble 90 %, 20 % Entrave | Entrave sur 1 ennemi | Toile d'acier : 80 % à tous, 20 % Entrave | +15 % dégâts contre Entravés |
| Éclat | Matière | Tesson 100 % | Vulnérabilité sur 1 ennemi | Pluie de verre : 90 % à tous | +10 % critique |
| Masque | Matière | Coup bas 100 % | Leurre : Provocation d'un clone (1 tour) | Imitation : copie l'A1 de la cible à 150 % | 10 % d'Esquive |
| Étincelle | Élémentaire | Arc 100 %, 15 % Choc | +15 % ATB à 1 allié | Court-circuit : 150 %, Choc si Gelé | +15 % dégâts contre les mécaniques |
| Fumée | Élémentaire | Souffle brûlant 100 %, Brûlure 30 % | Aveuglement sur 1 ennemi | Four : 80 % à tous, Brûlure 50 % | Brûlures +1 tour |
| Frimas | Élémentaire | Engelure 100 %, Gel 30 % | Gel sur 1 ennemi | Blizzard : 80 % à tous, −10 % ATB | +15 % dégâts contre Gelés |
| Néon | Élémentaire | Flash 90 %, Aveuglement 30 % | Aveuglement sur 2 ennemis | Stroboscope : 100 % à tous | Immunisé à l'Aveuglement |
| Vapeur | Élémentaire | Jet 100 % | Brûlure sur 1 ennemi | Explosion de conduite : 110 % à tous | +10 % dégâts contre Brûlés |
| Rouille | Élémentaire | Oxydation 90 %, Corrosion 30 % | Corrosion sur 1 ennemi | Décrépitude : Corrosion sur tous | +10 % dégâts contre Corrodés |
| Bélier | Physique/Bio | Charge 110 % | −20 % ATB sur 1 ennemi | Défonce : 190 % à 1 ennemi | +10 % attaque |
| Cuirasse | Physique/Bio | Direct 100 % | Provocation + défense +30 % | Uppercut : 150 %, 30 % Choc | −15 % dégâts subis |
| Marée | Physique/Bio | Coup de reflux 100 % | Régénération sur soi | Lame de fond : 100 % à tous | +5 % PV max par tour sous 50 % PV |
| Antidote | Physique/Bio | Seringue 80 % | Retire les altérations d'1 allié | Purge : retire 1 altération de chaque allié + soin 10 % | Immunisée à Brûlure et Corrosion |
| Pouls | Physique/Bio | Frappe 100 % | Hâte sur soi | Second souffle : +15 % ATB à l'équipe | Immunisé au Gel |
| Veille | Perception | Contre 100 % | Contre-attaque au prochain coup reçu | Garde de nuit : Esquive sur 2 alliés | Immunisée au Choc |
| Radar | Perception | Ping 90 %, Marque | Révèle les ennemis camouflés | Balayage : 80 % à tous | +10 % précision de l'équipe |
| Réflexe | Perception | Frappe rapide 90 %, +10 % ATB | Esquive | Enchaînement : 3 × 60 % | 15 % d'Esquive |
| Thermo | Perception | Tir ciblé 100 % | Révèle + Vulnérabilité sur 1 ennemi | Point chaud : 200 %, +50 % si Brûlé | +15 % critique |
