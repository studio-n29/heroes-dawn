# Équilibrage de Moscou : le Passeur et les élites (2026-11-13)

Tâche 8 du plan `Code/docs/plans/2026-11-13-moscou-agent-m.md`, cible de la spec §8 : **boss 40 à 60 %**, **élites 60 à 85 %**, combats ordinaires sans mort sur le chemin court. Outil : `BalanceFullReport.MoscowTuning*` (`Assets/_Project/Editor/Balance/`), même simulateur que Brooklyn (voir `2026-11-13_equilibrage-boss.md`).

## Protocole
- Équipe de départ : Giulia Rossi, Ilona Varga, Lucas Mendes ; profils d'équipement `Bare` (nu, plancher), `ItemsOnly` (objets moyens) et `ItemsStrong` (objets forts) ; **niveaux 5 et 10** ; 200 combats par rencontre ; graine fixe 7 ; précision environ ±7 points.
- **Cartes équipées exclues** (limite connue de la phase 4 : l'IA auto joue mal les cartes) ; le playtest les couvre.
- Les zones simulées sont les assets importés (`Resources/Zones`), comme dans le jeu. Le levier est l'attaque de départ (les PV changent peu le résultat, voir le rapport des boss).
- Les valeurs de départ de la spec (§5) sont une hypothèse reprise de Crocodile : Passeur 5000 PV / **ATQ 405**, homme de main 2800 PV / **ATQ 125**.

## 1. Mesure initiale (valeurs de la spec)
| Rencontre | Résultat |
|---|---|
| Passeur, ATQ ×1 (405) | 99 à 100 % (trop facile) |
| Homme de main (ATQ 125), les quatre zones | 100 % partout (trop facile) |
| Gardes, contrebandiers, rencontres faciles et normales | 100 % partout |

## 2. Réglage du Passeur (attaque ×, victoires en %, `Bare / ObjetsMoy / ObjetsForts`, niveau 5 puis 10)
| ATQ × | Nu 5 / 10 | Objets moyens 5 / 10 | Objets forts 5 / 10 | Moyenne objets (4 cas) |
|---|---|---|---|---|
| 1,25 | 91 / 98 | 99 / 99 | 96 / 99 | 98 |
| 1,5 | 74 / 91 | 93 / 98 | 83 / 92 | 92 |
| 1,75 | 50 / 74 | 74 / 91 | 61 / 74 | 75 |
| **2,0** | 33 / 53 | 59 / 74 | 35 / 52 | 55 |
| **2,05 (retenu, ATQ 830)** | 30 / 49 | 54 / 70 | 32 / 48 | **51** |
| 2,1 | 27 / 47 | 51 / 66 | 30 / 44 | 48 |
| 2,25 | 22 / 40 | 41 / 53 | 21 / 38 | 38 |
| 2,5 | 13 / 33 | 26 / 42 | 12 / 27 | 27 |
| 3,0 | 7 / 14 | 12 / 18 | 6 / 14 | 12 |

**Valeur retenue : ATQ 830 (×2,05)** ; mesure finale (rencontre Passeur + contrebandier + garde, M1.4) : objets moyens **54 % (niv. 5) / 70 % (niv. 10)**, objets forts **32 % / 48 %**, nu 30 % / 49 %. La **moyenne des quatre cas équipés est de 51 %** : dans la cible, avec l'écart habituel entre niveau 5 (plus dur) et niveau 10 (plus facile) ; le même profil que Crocodile (31 à 59 %) et le Titan (37 à 63 %) de Brooklyn.

## 3. Réglage de l'homme de main (élites, attaque ×)
Victoires `objets moyens / objets forts` au niveau 5 · au niveau 10, par zone (les élites changent de composition d'une zone à l'autre) :

| ATQ × | M1.1 (enf.+garde+contreb.) | M1.2 (enf.+contreb.+thug) | M1.3 (enf.+cogneur+contreb.) | M1.4 (enf.+garde ×2) |
|---|---|---|---|---|
| 1 à 3 | 100 | 100 | 100 | 99 à 100 |
| 4 | 96 / 85 · 99 / 93 | 99 / 93 · 100 / 99 | 96 / 74 · 100 / 90 | 75 / 41 · 92 / 73 |
| 4,25 | 91 / 73 · 99 / 89 | 98 / 89 · 99 / 97 | 92 / 66 · 98 / 84 | 60 / 31 · 90 / 62 |
| **4,5 (retenu, ATQ 562)** | 88 / 58 · 96 / 82 | 96 / 83 · 99 / 95 | 86 / 57 · 96 / 77 | 45 / 18 · 81 / 46 |
| 4,75 | 82 / 47 · 95 / 74 | 93 / 78 · 99 / 92 | 78 / 47 · 93 / 70 | 35 / 11 · 70 / 35 |
| 5 | 71 / 36 · 94 / 59 | 90 / 69 · 97 / 87 | 67 / 38 · 86 / 63 | 23 / 5 · 57 / 25 |

**Valeur retenue : ATQ 562 (×4,5).** Au **niveau recommandé de chaque zone** (5, 6, 8, 10, interpolation entre les mesures aux niveaux 5 et 10 et moyenne des deux profils d'objets) : M1.1 **73 %**, M1.3 **80 %**, M1.4 **63 %**, dans la cible 60 à 85 %.

**Essai sur M1.2** : avec la composition de la spec (homme de main + contrebandier + voyou), l'élite tombait à **94 %** au niveau recommandé 6, hors cible (trop facile). Composition remplacée par **homme de main + contrebandier ×2** : mesure finale objets moyens 89 / 97, objets forts 70 / 91, soit **82 %** au niveau 6. Écart de la spec consigné dans le plan (ledger) ; les autres élites gardent la composition de la spec.

## 4. Mesure finale (valeurs retenues, équipe de départ)
| Rencontre | Nu 5 / 10 | Objets moyens 5 / 10 | Objets forts 5 / 10 |
|---|---|---|---|
| M1.1 élite | 43 / 78 | 88 / 96 | 58 / 82 |
| M1.2 élite (homme de main + contrebandier ×2) | 58 / 85 | 89 / 97 | 70 / 91 |
| M1.3 élite | 44 / 69 | 86 / 96 | 57 / 77 |
| M1.4 élite | 8 / 33 | 45 / 81 | 18 / 46 |
| **M1.4 boss : le Passeur** | 30 / 49 | **54 / 70** | **32 / 48** |
| Combats faciles et normaux (toutes zones) | 100 / 100 | 100 / 100 | 100 / 100 |

Aucun combat sans fin (0 délai de 20 000 ticks). Les rencontres faciles et normales sont à 100 % : parcours sans mort sur le chemin court avec l'équipe de départ.

## 5. Tests et garde
`MoscowBalanceTests` (`Category("Balance")`, `Assets/Tests/Editor/`, graine fixe) : `Passeur_WinRate_InTarget` (moyenne des quatre cas équipés dans 35 à 65 %, soit 40 à 60 % ±5 ; aucun cas hors 20 à 80 %), `Elites_WinRate_InTarget` (60 à 85 % au niveau recommandé, tolérance ±8) et `OrdinaryFights_AreSafe_ForTheStartingTeam` (95 % ou plus). La garde anti pay-to-win (`NoPayToWinTests`) reste verte.

## 6. Limites et suite
- Écart fort entre niveau 5 et 10 (jusqu'à 30 points) et entre objets moyens et forts : la précision à 200 combats est de ±7 points ; ne pas lire un écart de moins de 10 points comme significatif.
- Les **cartes équipées** ne sont pas simulées : le playtest (tâche 9) doit confirmer que le Passeur reste battable avec cartes et AUTO, et que l'élite de M1.4 (18 à 45 % au niveau 5) n'est pas ressentie comme un mur à ce niveau (elle est prévue pour le niveau 10).
- M1.4 : l'élite (homme de main + 2 gardes) est la plus dure ; si le playtest la juge trop dure au niveau recommandé, relâcher l'attaque de l'homme de main (×4,25) ou remplacer un garde.
