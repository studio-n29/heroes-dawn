from wf_lib import *

SCREENS = []


def screen(sid, slug, name, els, legend, intro=""):
    SCREENS.append(dict(id=sid, slug=slug, name=name, els=els, legend=legend, intro=intro))


# ------------------------------------------------------------------ W01 Titre
screen("W01", "titre", "Titre / connexion", [
    I(0, 0, 1920, 1080, "Fond : illustration clé (paysage, voilée en bas)", plain=True),
    I(560, 70, 800, 430, "Logo-titre « HEROES DAWN »", 1),
    T(460, 520, 1000, 50, "Accroche : « New York tient encore. Pour combien de temps ? »", center=True, size=26),
    BP(710, 620, 500, 104, "JOUER", 2),
    B(560, 770, 250, 70, "Google", 3), B(835, 770, 250, 70, "Apple"), B(1110, 770, 250, 70, "Steam"),
    T(660, 856, 600, 40, "Lier un compte plus tard (jouer en invité)", center=True, size=20),
    B(1620, 30, 240, 60, "FR ▾", 4),
    T(40, 1000, 500, 40, "STUDIO N29 · v0.1", size=22, bold=True),
    T(1180, 1000, 600, 40, "CGU · Confidentialité · Crédits", 5, size=20),
], [
    "Logo-titre animé (apparition + éclat de l'étoile) sur l'illustration clé ; voile sombre en bas pour la lisibilité.",
    "JOUER : connexion invité automatique (UGS). Si un compte est lié, la session reprend directement. Spinner + message pendant la connexion.",
    "Lier un compte (Google / Apple / Steam selon la plateforme), conseillé après le prologue pour sécuriser la progression.",
    "Choix de langue (FR, EN, ES, PT, DE), mémorisé.",
    "Liens légaux ; le consentement RGPD s'affiche en modale au tout premier lancement.",
])

# ------------------------------------------------------------------ W02 QG
screen("W02", "qg", "QG — hub (vue en coupe)", topbar("QG — SECTION NEW YORK", back=False, n_res=1) + [
    I(40, 120, 1400, 820, "", plain=True),
    I(70, 150, 430, 360, "Salon d'entraînement · Niv. 3", 2), BADGE(470, 136, "2", 3),
    I(520, 150, 430, 360, "Infirmerie · Niv. 2"),
    I(970, 150, 440, 360, "Salle des opérations · Niv. 2"), BADGE(1380, 136, "!"),
    I(70, 530, 640, 380, "Labo de recherche · 18 PR prêts"), BADGE(680, 516, "18"),
    I(730, 530, 680, 380, "Archives · 12/35"),
    BP(1470, 120, 410, 150, "MISSION", 4),
    B(1470, 300, 410, 96, "Recrutement", 5), BADGE(1846, 286, "N"),
    B(1470, 416, 410, 96, "Équipe", 6),
    B(1470, 532, 410, 96, "Pass · palier 12"), BAR(1500, 604, 350, 14, 0.45),
    B(1470, 648, 410, 96, "Boutique"),
    P(40, 960, 1400, 96, "Objectifs secondaires : 3 disponibles · prochaine vague dans 02:41:10   ▸", 7),
    B(1470, 960, 410, 96, "Missions du jour 3/5", 8),
], [
    "Barre de ressources (Crédits, PR, Gemmes) et ⚙ paramètres. Toucher une ressource explique d'où elle vient et propose la boutique.",
    "Vue en coupe du QG : chaque pièce est une zone tactile avec son niveau ; elle s'anime quand quelque chose s'y passe.",
    "Badges de notification : entraînement terminé, expédition revenue, labo plein, pool renouvelée.",
    "MISSION : action principale, ouvre la carte de New York (toujours au même endroit, sous le pouce droit).",
    "Recrutement, avec badge « N » quand une nouvelle pool est disponible.",
    "Accès permanents : Équipe, Pass (palier + progression), Boutique.",
    "Bandeau des objectifs secondaires en temps réel (compte à rebours jusqu'à la prochaine vague) ; toucher = carte filtrée.",
    "Missions quotidiennes (XP de pass), réclamables d'un geste.",
])

# ------------------------------------------------------------------ W03 Salon
_slots = []
for i, (nm, lv, xp, tm) in enumerate([("Givre", "Niv. 14", 0.7, "02:14:00"), ("Arc", "Niv. 9", 0.3, "05:40:00"), ("Argile", "Niv. 6", 1.0, "TERMINÉ")]):
    x = 40 + i * 380
    _slots += [P(x, 130, 360, 380, "", 2 if i == 0 else None), C(x + 120, 150, 120, nm[0]),
               T(x + 20, 284, 320, 40, f"{nm} · {lv}", center=True, size=26, bold=True),
               BAR(x + 30, 336, 300, 26, xp, "XP"), T(x + 20, 374, 320, 36, tm, center=True, size=24),
               (BP if xp >= 1 else B)(x + 40, 422, 280, 66, "Récupérer" if xp >= 1 else "Accélérer · 40 GM")]
screen("W03", "salon", "Salon d'entraînement", topbar("SALON D'ENTRAÎNEMENT · NIV. 3", n_back=1) + _slots + [
    P(1180, 130, 320, 380, "", 3), T(1190, 260, 300, 60, "+ Placer un membre", center=True, size=26, bold=True),
    P(1520, 130, 360, 380, "Objets placés", 4),
    I(1540, 180, 150, 130, "Sac"), I(1710, 180, 150, 130, "Simu."),
    T(1540, 320, 320, 60, "Bonus XP total : +30 %", size=22, bold=True), B(1540, 422, 320, 66, "Boutique d'objets"),
    P(40, 540, 1840, 300, "Paliers de compétences prêts à valider", 5),
    C(70, 600, 80, "G"), T(170, 600, 900, 80, "Givre atteint le niv. 15 → palier 2 de « Zéro absolu » (+1 tour de Gel)", size=24),
    BP(1440, 606, 400, 70, "Valider · 4 000 CR"),
    C(70, 720, 80, "A"), T(170, 720, 900, 80, "Ancre atteint le niv. 10 → palier 1 de « Ancrage »", size=24),
    BP(1440, 726, 400, 70, "Valider · 2 000 CR"),
    B(40, 880, 900, 90, "Améliorer le salon → Niv. 4 · 9 113 CR", 6),
    T(980, 880, 900, 90, "Niv. 4 : +3 % d'XP · 5e emplacement au niv. 5", size=22),
], [
    "Retour au QG (bouton, geste retour Android ou Échap sur PC) — identique sur tous les écrans de pièce.",
    "Emplacement occupé : membre, XP en cours, minuteur, accélération en Gemmes (20 GM/heure) ou récupération.",
    "Emplacement libre : ouvre la liste de l'effectif triée par niveau ; emplacements verrouillés affichés avec leur niveau de déblocage.",
    "Objets du salon placés et bonus d'XP cumulé ; accès à la boutique d'objets.",
    "Paliers de compétences débloqués par le niveau, validés en Crédits (puits principal).",
    "Amélioration de la pièce (coût, effet du niveau suivant).",
])

# ------------------------------------------------------------------ W04 Infirmerie
_rows = []
for i, (nm, f) in enumerate([("Écho", 0.72), ("Rempart", 0.55), ("Sève", 0.40), ("Argile", 0.25), ("Arc", 0.10)]):
    y = 230 + i * 120
    _rows += [C(800, y, 84, nm[0]), T(900, y, 260, 84, nm, size=26, bold=True), BAR(1170, y + 26, 420, 32, f, f"{int(f*100)} %"),
              B(1620, y + 10, 230, 64, "Placer")]
screen("W04", "infirmerie", "Infirmerie", topbar("INFIRMERIE · NIV. 2") + [
    P(40, 130, 700, 380, "Emplacement 1", 1), C(80, 190, 130, "G"), T(230, 190, 480, 50, "Givre", size=30, bold=True),
    BAR(230, 250, 480, 36, 0.85, "Fatigue 85 % → 0 %"), T(230, 300, 480, 40, "Récupération ×2 · prêt dans 01:20:00", size=22),
    B(230, 400, 220, 70, "Retirer"), B(470, 400, 240, 70, "Accélérer · 30 GM"),
    P(40, 540, 700, 300, "Emplacement 2 — débloqué au niv. 3", dark=True),
    P(760, 130, 1120, 840, "Effectif (trié par fatigue)", 2), TAG(1640, 150, 220, 50, "Tri : fatigue ▾", 3),
] + _rows + [
    B(40, 880, 700, 90, "Trousse de soin · −25 % · 800 CR", 4),
], [
    "Emplacement occupé : fatigue qui descend en temps réel, multiplicateur de récupération, minuteur, accélération.",
    "Effectif trié par fatigue décroissante ; « Placer » envoie le membre dans un emplacement libre.",
    "Tri (fatigue, niveau, famille). Code couleur de la jauge : vert < 60 %, orange au-delà (malus), rouge à 100 % (indisponible).",
    "Consommable « Trousse de soin » utilisable sans emplacement.",
])

# ------------------------------------------------------------------ W05 Opérations
screen("W05", "operations", "Salle des opérations (expéditions)", topbar("SALLE DES OPÉRATIONS · NIV. 2") + [
    P(40, 130, 820, 360, "Expédition 1 — Gowanus (1.2) · 4 h", 1),
    C(70, 200, 80, "R"), C(160, 200, 80, "S"), BAR(270, 220, 560, 34, 1.0, "TERMINÉE"),
    T(70, 310, 760, 40, "800 CR · 15 PR · +20 % de fatigue", size=24), BP(70, 380, 760, 80, "RÉCUPÉRER"),
    P(40, 520, 820, 360, "Expédition 2 — Red Hook (1.1) · 8 h"),
    C(70, 590, 80, "A"), BAR(170, 610, 660, 34, 0.35, "05:12:40"),
    B(70, 770, 760, 80, "Accélérer · 110 GM"),
    P(900, 130, 980, 840, "Nouvelle expédition", 2),
    T(930, 180, 300, 40, "Zone (validées)", size=22, bold=True),
    TAG(930, 226, 200, 50, "1.1 Red Hook", on=True), TAG(1145, 226, 200, 50, "1.2 Gowanus"), TAG(1360, 226, 200, 50, "1.3 Dumbo"),
    T(930, 310, 300, 40, "Durée", 3, size=22, bold=True),
    TAG(930, 356, 150, 56, "1 h"), TAG(1095, 356, 150, 56, "4 h", on=True), TAG(1260, 356, 150, 56, "8 h"), TAG(1425, 356, 150, 56, "12 h"),
    T(930, 450, 400, 40, "Membres (non utilisés)", 4, size=22, bold=True),
    C(930, 500, 110, "+"), C(1060, 500, 110, "+"), C(1190, 500, 110, "+"), C(1320, 500, 110, "+"),
    P(930, 650, 920, 150, "Récompense estimée : 800 Crédits · 15 PR · +20 % de fatigue par membre", 5, dark=True),
    BP(1300, 840, 550, 100, "ENVOYER", 6),
], [
    "Expéditions en cours : zone, durée, membres, minuteur ; « Récupérer » quand c'est terminé (badge sur le QG).",
    "Nouvelle expédition (2 emplacements au niv. 1, 4 au niv. 7).",
    "Durées 1 h / 4 h / 8 h / 12 h (8 h et 12 h débloquées par le niveau de la pièce).",
    "Membres disponibles (ni en escouade de mission, ni au salon, ni à l'infirmerie).",
    "Aperçu des gains et de la fatigue avant l'envoi.",
    "Envoi : lance le minuteur et notifie au retour (notification push optionnelle).",
])

# ------------------------------------------------------------------ W06 Labo
screen("W06", "labo", "Labo de recherche", topbar("LABO DE RECHERCHE · NIV. 2") + [
    I(460, 130, 1000, 420, "Illustration du labo (fioles qui se remplissent)"),
    BAR(460, 580, 1000, 56, 0.75, "18 / 24 PR stockés", 1),
    T(460, 650, 1000, 44, "Production : 3 PR/h · stock plein dans 2 h", 2, center=True, size=26),
    BP(660, 720, 600, 110, "COLLECTER 18 PR", 3),
    P(460, 870, 1000, 130, "Améliorer → Niv. 3 : 4 PR/h · 6 750 CR", 4, dark=True),
], [
    "Jauge de stock (12 h de production, 24 h au niv. 5) : au-delà, la production s'arrête.",
    "Production horaire et temps restant avant saturation.",
    "Collecte d'un geste (animation des PR vers la barre de ressources).",
    "Amélioration de la pièce.",
])

# ------------------------------------------------------------------ W07 Archives
_cards = []
for i in range(12):
    x = 40 + (i % 6) * 190; y = 230 + (i // 6) * 330
    locked = i in (4, 7, 9, 10, 11)
    _cards.append(I(x, y, 170, 300, "???" if locked else "Portrait", 2 if i == 0 else None))
screen("W07", "archives", "Archives", topbar("ARCHIVES") + [
    TAG(40, 130, 180, 60, "Recrues", 1, on=True), TAG(235, 130, 150, 60, "Lore"), TAG(400, 130, 170, 60, "Scènes"),
    TAG(585, 130, 190, 60, "Musiques"), TAG(790, 130, 160, 60, "Succès"),
] + _cards + [
    P(1220, 130, 660, 760, "", 3), I(1250, 160, 600, 330, "Illustration"),
    HD(1250, 510, 600, 50, "ARGILE — Hannah Kowalski", size=30),
    T(1250, 570, 600, 160, "Céramiste de Greenpoint. Son atelier s'est effondré le soir de la Supernova ; elle l'a reconstruit à mains nues.", size=22),
    B(1250, 790, 600, 76, "Voir la fiche"),
    BAR(40, 910, 1140, 40, 12 / 35, "Collection New York · 12 / 35", 4),
], [
    "Onglets : recrues, lore, scènes débloquées, musiques, succès.",
    "Cartes de collection ; les recrues non rencontrées restent en silhouette « ??? ».",
    "Détail : illustration, identité, bio courte ; lien vers la fiche complète (W16).",
    "Progression de la collection de la ville (pool épuisée = collection terminée).",
])

# ------------------------------------------------------------------ W08 Carte
_pins = []
districts = [("BROOKLYN · Acte 1", 120, 560, "ok"), ("QUEENS · Acte 2", 520, 300, "cur"), ("BRONX · Acte 3", 720, 130, "lock"),
             ("STATEN ISLAND · Acte 4", 120, 820, "lock"), ("MANHATTAN · Acte 5", 760, 560, "lock")]
for i, (nm, x, y, st) in enumerate(districts):
    _pins.append(P(x, y, 330, 70, nm, 1 if i == 0 else None, dark=(st == "lock")))
    for k in range(3):
        lab = "✓" if st == "ok" else ("●" if st == "cur" and k == 0 else "🔒" if st == "lock" else "○")
        _pins.append(C(x + 20 + k * 100, y + 84, 70, f"{['1', '2', '3'][k]}", 2 if (i == 0 and k == 0) else None))

screen("W08", "carte", "Carte de la ville", topbar("NEW YORK") + [
    I(40, 120, 1320, 870, "Carte illustrée de New York (vue de dessus)", plain=True),
] + _pins + [
    BADGE(236, 630, "!", 3),
    P(1390, 120, 490, 870, "Zone sélectionnée", 4),
    HD(1410, 170, 450, 50, "2.1 FLUSHING — LE MARCHÉ", size=28),
    T(1410, 240, 450, 150, "□ Combat clé : les racketteurs du Syndicat\n□ Événement : un commerçant menacé\n□ Poursuite : rattraper un indic", size=21),
    T(1410, 420, 450, 70, "Validation : 1 500 CR · 40 PR · 150 XP de pass", size=21),
    TAG(1410, 520, 210, 56, "Normal", 5, on=True), TAG(1640, 520, 210, 56, "Difficile 🔒"),
    T(1410, 600, 450, 80, "Partie en cours : 1/3 objectif · brouillard refermé", size=20),
    BP(1410, 860, 450, 100, "PRÉPARER", 6),
    BAR(60, 1000, 600, 30, 0.33, "Acte 2 · Le Syndicat · 1/3 zones"),
], [
    "Arrondissement = acte ; les actes non débloqués restent grisés et voilés.",
    "Zones (3 par acte) : validée, en cours, disponible, verrouillée. Ordre libre à l'intérieur d'un acte.",
    "Objectif secondaire temps réel présent dans une zone validée.",
    "Fiche de la zone : objectifs (cochés s'ils sont déjà remplis), récompenses de validation, état de la partie en cours.",
    "Mode difficile, débloqué par la validation de la zone.",
    "Vers la préparation de mission (W09).",
])

# ------------------------------------------------------------------ W09 Préparation
_sq = []
for i, (nm, fam, f) in enumerate([("Ancre", "Physique/Bio", 0.2), ("Verre", "Matière", 0.35), ("Givre", "Élémentaire", 0.1), ("Écho", "Perception", 0.72)]):
    x = 40 + i * 300
    _sq += [P(x, 130, 280, 470, "", 1 if i == 0 else None), I(x + 20, 150, 240, 240, "Portrait"),
            T(x + 20, 400, 240, 40, nm, center=True, size=28, bold=True), TAG(x + 40, 446, 200, 40, fam),
            BAR(x + 20, 500, 240, 26, f, f"Fatigue {int(f*100)} %"), T(x + 20, 540, 240, 40, "Niv. 14", center=True, size=22)]
_roster = [C(60 + i * 120, 690, 100, nm) for i, nm in enumerate(["Ar", "Re", "Sè", "Ét", "Mu", "Ac", "Ma", "Ai", "…"])]
screen("W09", "preparation", "Préparation de mission", topbar("PRÉPARATION — 2.1 FLUSHING") + _sq + [
    NOTE(930, 586, 300, 40, "Écho > 60 % : malus −9 %", None),
    P(40, 630, 1180, 200, "Effectif disponible (glisser vers un emplacement)", 2),
] + _roster + [
    TAG(40, 860, 140, 50, "Toutes", 3, on=True), TAG(195, 860, 140, 50, "Matière"), TAG(350, 860, 170, 50, "Élémentaire"),
    TAG(535, 860, 170, 50, "Physique"), TAG(720, 860, 170, 50, "Perception"),
    P(1260, 130, 620, 330, "Combos possibles (6 paires)", 4),
    T(1280, 180, 580, 270, "Verre + Givre · Tempête de cristal (duo)\nAncre + Verre · Armure forgée\nAncre + Givre · Corps élémentaire\nGivre + Écho · Point faible\nAncre + Écho · Triage\nVerre + Écho · Piège", size=21),
    P(1260, 480, 620, 200, "Consommables", 5),
    I(1280, 530, 130, 130, "Trousse ×2"), I(1425, 530, 130, 130, "Dé chargé ×1"), I(1570, 530, 130, 130, "Stimulant"), I(1715, 530, 145, 130, "Fusée"),
    P(1260, 700, 620, 140, "Objectifs : 0/3 · niveau conseillé 13", 6, dark=True),
    BP(1260, 860, 620, 110, "DÉPLOYER", 7),
], [
    "Escouade de 4 : portrait, famille, fatigue, niveau. Toucher = retirer ; un membre à 100 % de fatigue ne peut pas être placé.",
    "Effectif disponible (anneau de fatigue autour du portrait) ; glisser-déposer ou toucher pour remplir l'emplacement libre.",
    "Filtres par famille.",
    "Combos de l'escouade (les 6 paires), duos légendaires mis en avant : aide à composer l'équipe.",
    "Consommables de mission emportés (achetés en Crédits).",
    "Rappel des objectifs de la zone et niveau conseillé.",
    "Déployer : ouvre le plateau (W10) ; avertissement si un membre dépasse 60 % de fatigue.",
])
