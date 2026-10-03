from wf_lib import *
from screens_a import screen

# ------------------------------------------------------------------ Plateau : géométrie de la zone 1.1
NODES = {"n00": ("E", 0, 2), "n01": ("$", 1, 2), "n02": ("", 2, 2), "n03": ("VS", 3, 3), "n04": ("$", 4, 3), "n05": ("!", 5, 3),
         "n06": ("$", 3, 1), "n07": ("VS", 4, 1), "n08": ("MJ", 5, 1), "n09": ("", 6, 2), "n10": ("Zz", 7, 2),
         "n11": ("VS", 8, 3), "n12": ("$", 9, 3), "n13": ("!", 10, 3), "n14": ("$", 8, 1), "n15": ("MJ", 9, 1), "n16": ("VS", 10, 1),
         "n17": ("", 11, 2), "n18": ("$", 12, 2), "n19": ("VS", 13, 3), "n20": ("!", 14, 3), "n21": ("MJ", 13, 1), "n22": ("Zz", 14, 1),
         "n23": ("VS", 15, 2), "n24": ("", 16, 2)}
LINKS = [("n00", "n01"), ("n01", "n02"), ("n02", "n03"), ("n02", "n06"), ("n03", "n04"), ("n04", "n05"), ("n06", "n07"), ("n07", "n08"),
         ("n05", "n09"), ("n08", "n09"), ("n09", "n10"), ("n10", "n11"), ("n10", "n14"), ("n11", "n12"), ("n12", "n13"), ("n14", "n15"),
         ("n15", "n16"), ("n13", "n17"), ("n16", "n17"), ("n17", "n18"), ("n18", "n19"), ("n18", "n21"), ("n19", "n20"), ("n21", "n22"),
         ("n20", "n23"), ("n22", "n23"), ("n23", "n24")]


def pos(nid):
    _, x, y = NODES[nid]
    return 150 + x * 102, 560 - (y - 2) * 190


def board(revealed_max_x=7, dest=None, current="n02", path=None):
    dest = dest or {}
    els = [I(0, 96, 1920, 984, "", plain=True)]
    for a, b in LINKS:
        (x1, y1), (x2, y2) = pos(a), pos(b)
        vis = NODES[a][1] <= revealed_max_x and NODES[b][1] <= revealed_max_x
        hl = path and (a in path and b in path)
        els.append(LINE(x1, y1, x2, y2, 12 if hl else 8, dashed=not vis, color="#3E4452" if hl else ("#9AA0AB" if vis else "#C3C7CF")))
    for nid, (g, x, y) in NODES.items():
        cx, cy = pos(nid)
        if x > revealed_max_x:
            els.append(C(cx - 34, cy - 34, 68, "?", bg="#B9BEC7"))
        else:
            if nid in dest:
                els.append(C(cx - 50, cy - 50, 100, "", bg="rgba(0,0,0,0)", z=1))
            els.append(C(cx - 38, cy - 38, 76, g, bg="#fff" if nid not in dest else "#3E4452", size=24))
            if nid in dest:
                els.append(E("badge", cx + 22, cy - 62, 38, 38, str(dest[nid])))
    els.append(FOG(150 + 7.6 * 102, 130, 1920 - (150 + 7.6 * 102) - 20, 660, "Brouillard de guerre"))
    cx, cy = pos(current)
    els.append(C(cx - 30, cy - 104, 60, "P", bg="#22262E", size=26))
    return els


hud = [
    P(0, 0, 1920, 96, dark=True, flat=True), B(24, 18, 150, 60, "◄ QG"),
    HD(196, 18, 900, 60, "ZONE 1.1 · RED HOOK — LES QUAIS", size=34),
    E("pill", 1360, 22, 230, 52, "12 450", tag="CR"), E("pill", 1610, 22, 230, 52, "185", tag="PR"),
    P(24, 116, 520, 190, "OBJECTIFS"), T(44, 160, 480, 140, "☑ Couper le générateur du quai\n☐ Vaincre Brick à l'entrepôt\n☐ Atteindre la grue du terminal", size=21),
]
squad = [P(24, 870, 640, 190, "")] + sum([[C(44 + i * 154, 890, 96, n), BAR(44 + i * 154, 1000, 120, 16, f)] for i, (n, f) in
                                          enumerate([("An", 0.3), ("Ve", 0.4), ("Gi", 0.2), ("Éc", 0.7)])], [])

screen("W10a", "plateau-lancer", "Plateau — attente du lancer",
       board(current="n02") + hud + squad + [
           E("anchor", 24, 116, 10, 10, "", 1),
           E("anchor", 368, 452, 10, 10, "", 2),
           P(1420, 840, 476, 220, ""), P(1444, 864, 150, 150, "5", size=90, bold=True),
           BP(1614, 864, 260, 92, "LANCER !", 3),
           B(1614, 966, 260, 70, "Dé bonus ×1", 4), BAR(1444, 1024, 150, 22, 4 / 6, "4/6"),
           B(680, 990, 150, 70, "Sac", 5),
           E("anchor", 24, 870, 10, 10, "", 6),
           E("anchor", 1050, 140, 10, 10, "", 7),
       ], [
           "Objectifs de la zone (3), cochés même si le brouillard se referme ; toucher = centre la caméra sur l'objectif révélé.",
           "Pion de l'escouade sur sa case. Glisser = déplacer la caméra, pincer = zoom.",
           "LANCER ! : 1d6 (grande face du dé + animation). Raccourci : Espace.",
           "Dé bonus (6 PM non utilisés = 1 dé, réserve max 2) et jauge de PM en réserve.",
           "Sac : consommables emportés (trousse, dé chargé, fusée éclairante…).",
           "Escouade : portraits + fatigue ; toucher = fiche rapide.",
           "Brouillard : les cases au-delà du rayon de vision restent cachées (« ? ») ; il se referme quand on quitte la zone.",
       ])

screen("W10b", "plateau-destination", "Plateau — choix de destination",
       board(current="n02", dest={"n03": 1, "n06": 1, "n04": 2, "n07": 2, "n05": 3, "n08": 3}, path=["n02", "n03", "n04"]) + hud + squad + [
           E("anchor", 444, 344, 10, 10, "", 1),
           P(1420, 840, 476, 220, ""), HD(1444, 860, 430, 70, "3 PM", 2, size=56),
           B(1444, 950, 430, 86, "TERMINER", 3),
           NOTE(560, 640, 330, 110, "Case Combat · Facile\n(appui long = infos)", 4),
           NOTE(900, 140, 420, 60, "Nouveau secteur en vue ! (toast)", 5),
           E("anchor", 554, 450, 10, 10, "", 6),
       ], [
           "Destinations atteignables avec les PM du lancer : anneau pulsé + coût en pas (pastille).",
           "PM en main.",
           "Terminer : les PM restants vont dans la réserve de dé bonus.",
           "Appui long (survol sur PC) : type de case, rang de combat, ressource déjà ramassée, etc.",
           "Arrêt de brouillard : si un pas révèle du terrain, le pion s'arrête et le joueur peut repartir avec ses PM restants.",
           "Chemin prévisualisé (le plus court) ; on ne traverse pas un combat non résolu.",
       ])

# ------------------------------------------------------------------ Combat
def arena(target=False):
    els = [I(0, 96, 1920, 984, "", plain=True)]
    for i, nm in enumerate(["Ancre", "Verre", "Givre", "Écho"]):
        x = 140 + i * 190; y = 260 + (i % 2) * 120
        els.append(I(x, y, 160, 260, nm))
    for i, nm in enumerate(["Cogneur", "Voyou", "Pyro"]):
        x = 1160 + i * 230; y = 210 + (i % 2) * 110
        els += [I(x, y + 60, 190, 280, nm), BAR(x, y, 190, 20, [0.8, 0.45, 1.0][i]), BAR(x, y + 26, 190, 12, [0.3, 0.9, 0.6][i])]
        if target:
            els.append(C(x - 20, y + 40, 230, "", bg="rgba(0,0,0,0)", z=1))
    return els


def portraits(ready=0, sel=0):
    els = []
    for i, nm in enumerate(["Ancre", "Verre", "Givre", "Écho"]):
        x = 140 + i * 420
        els += [P(x, 880, 400, 180, "", dark=(i == sel)), C(x + 16, 900, 120, nm[0]),
                T(x + 150, 896, 240, 36, nm + ("  · PRÊT" if i in ready else ""), size=24, bold=True),
                BAR(x + 150, 940, 230, 24, [0.9, 0.6, 0.75, 0.5][i], "PV"),
                BAR(x + 150, 974, 230, 18, 1.0 if i in ready else [0.3, 0.5, 0.2, 0.7][i], "ATB"),
                T(x + 150, 1000, 230, 40, "Fatigue " + ["30", "40", "20", "70"][i] + " %", size=18)]
    return els


top = [P(0, 0, 1920, 96, dark=True, flat=True), B(24, 18, 200, 60, "Retraite"), HD(560, 18, 800, 60, "COMBAT · NORMAL", size=36),
       B(1550, 18, 160, 60, "AUTO"), B(1730, 18, 160, 60, "x1")]

screen("W11a", "combat-pret", "Combat — un allié est prêt",
       arena() + top + portraits(ready=[0], sel=0) + [
           E("anchor", 24, 18, 10, 10, "", 1),
           E("anchor", 1160, 210, 10, 10, "", 2),
           E("anchor", 140, 880, 10, 10, "", 3),
           P(140, 690, 1250, 170, ""),
           B(160, 710, 390, 130, "[Q] Coup d'amarre\nDégâts 110 %", 4), B(570, 710, 390, 130, "[W] Ancrage\nProvoc. + bouclier"),
           B(980, 710, 390, 130, "[E] Mur vivant\nRecharge 2 t", bg="#E3E6EB"),
           NOTE(1450, 420, 140, 60, "−312", 5),
           T(1160, 556, 200, 56, "Brûlure 2 · Vulnérable 1", 6, size=18, bold=True),
       ], [
           "Barre du haut : Retraite (confirmation : défaite + recul d'une case), AUTO, vitesse ×1/×2.",
           "Ennemis : PV (épais) et jauge ATB (fine) au-dessus de chaque figure ; le boss a un cadre doré.",
           "4 portraits en bas : PV, ATB, fatigue. Un allié prêt s'illumine et est sélectionné automatiquement (le plus ancien prêt).",
           "Ses 3 compétences s'ouvrent au-dessus : nom, effet court, recharge restante (grisée). Raccourcis Q/W/E, portraits 1-4.",
           "Retours visuels : chiffres flottants, critiques en jaune, « RATÉ », secousse de la cible.",
           "Statuts actifs de l'unité (icône + durée), détaillés à l'appui long.",
       ])

screen("W11b", "combat-ciblage", "Combat — ciblage et combo",
       arena(target=True) + top + portraits(ready=[0, 2], sel=0) + [
           E("anchor", 1150, 250, 10, 10, "", 1),
           P(140, 690, 1250, 170, ""),
           BP(160, 710, 390, 130, "Coup d'amarre\n▸ choisis une cible", 2), B(570, 710, 390, 130, "Ancrage"), B(980, 710, 390, 130, "Mur vivant"),
           HD(560, 140, 800, 60, "CHOISIS UNE CIBLE", size=40),
           BP(1500, 690, 380, 170, "COMBO !\nCorps élémentaire", 3),
           T(1500, 600, 380, 80, "Ancre + Givre prêts\n(couleurs des 2 familles)", size=20),
           E("anchor", 1550, 18, 10, 10, "", 4),
           E("anchor", 980, 880, 10, 10, "", 5),
       ], [
           "Compétence mono-cible choisie : les cibles valides pulsent ; toucher = lancer. Toucher ailleurs = annuler (Échap).",
           "Compétence en cours de ciblage mise en évidence.",
           "COMBO : apparaît quand deux alliés prêts forment une paire ; les deux consomment leur tour. Raccourci C.",
           "AUTO activé : panneau de compétences masqué, l'IA joue (combos inclus). La jauge ne s'arrête jamais (temps actif).",
           "Plusieurs alliés prêts : toucher un autre portrait prêt change la sélection (Tab sur PC).",
       ])

# ------------------------------------------------------------------ W12 Mini-jeux
screen("W12", "mini-jeux", "Mini-jeux (4 formats)", [I(0, 0, 1920, 1080, "", plain=True), DIM(),
    P(60, 40, 1800, 1000, ""), HD(100, 60, 900, 60, "MINI-JEU — 4 FORMATS", size=40), B(1640, 60, 190, 60, "Ignorer", 1),
    P(100, 140, 840, 430, "A · Démonstration (test de pouvoir)", 2),
    BAR(140, 300, 760, 70, 0.0, ""), P(520, 306, 140, 58, "", bg="#9AA0AB", radius=6), P(440, 290, 16, 90, "", bg="#22262E", radius=4),
    BP(390, 420, 260, 90, "STOP !"), T(140, 200, 760, 60, "Essais 2/3 · réussites 1/2 · membre : Ancre (+50 % de zone)", size=20),
    P(980, 140, 840, 430, "B · Piratage (puzzle)", 3),
    ] + [P(1100 + (i % 4) * 110, 200 + (i // 4) * 85, 96, 76, "╋" if i % 3 else "┃", size=30) for i in range(16)] + [
    BAR(1560, 210, 220, 30, 0.6, "0:18"),
    P(100, 600, 840, 420, "C · Poursuite (QTE)", 4),
    C(420, 690, 200, "↑", size=90), T(140, 910, 760, 50, "Geste 4/8 · erreurs 0/2 · fenêtre allongée par la VIT", center=True, size=20),
    BAR(300, 960, 440, 24, 0.4),
    P(980, 600, 840, 420, "D · Intervention (choix de membre)", 5),
    ] + sum([[C(1020 + i * 200, 700, 120, n), T(1020 + i * 200, 830, 120, 40, p, center=True, size=26, bold=True)]
             for i, (n, p) in enumerate([("An", "75 %"), ("Ve", "50 %"), ("Gi", "52 %"), ("Éc", "54 %")])], []) + [
    BP(1240, 910, 320, 80, "Envoyer Ancre"),
], [
    "Commun : choix du membre (sauf Puzzle), « Ignorer » sans récompense, option « temps étendu » en accessibilité.",
    "Démonstration : un curseur balaie la jauge ; s'arrêter dans la zone (élargie si la famille du membre correspond). 3 essais, 2 réussites.",
    "Piratage : faire pivoter les segments pour relier source et sortie, 30 s (+10 s avec Boussole ou Étincelle).",
    "Poursuite : suite de 6 à 10 gestes chronométrés, 2 erreurs tolérées.",
    "Intervention : la chance de réussite de chaque membre est affichée avant de confirmer.",
])

# ------------------------------------------------------------------ W13 Événement
screen("W13", "evenement", "Événement narratif", [I(0, 0, 1920, 1080, "Plateau", plain=True), DIM(),
    P(260, 70, 1400, 940, ""),
    I(290, 100, 1340, 400, "Illustration de l'événement (case de BD)", 1),
    HD(300, 520, 1300, 60, "CONTENEUR PIÉGÉ", 2, size=44),
    C(1500, 450, 120, "Gi", 5),
    T(300, 590, 1300, 140, "Des coups sourds résonnent dans un conteneur scellé. Des civils y ont été enfermés par la bande de Brick. « On ne peut pas les laisser là », souffle Givre.", 3, size=26),
    B(300, 760, 1320, 70, "Forcer la serrure (Ancre) · +250 Crédits", 4),
    B(300, 845, 1320, 70, "Récupérer le manifeste · +10 PR"),
    B(300, 930, 1320, 70, "Rassurer les civils · −15 % de fatigue pour l'escouade"),
], [
    "Illustration plein cadre façon case de comics (IA + retouche, charte DA).",
    "Titre en lettrage (Bangers).",
    "Texte court (3-4 lignes max), avance au toucher si plus long.",
    "2-3 choix, conséquence annoncée (ou masquée « ??? » pour les choix de scénario) ; un choix peut être lié à un membre présent.",
    "Membre qui parle : portrait en médaillon, sa réplique peut être doublée.",
])

# ------------------------------------------------------------------ W14 Résultats
_cards = []
for i, (nm, b, a) in enumerate([("Ancre", 30, 42), ("Verre", 40, 52), ("Givre", 20, 32), ("Écho", 70, 82)]):
    x = 160 + i * 410
    _cards += [P(x, 470, 380, 300, "", 3 if i == 0 else None), C(x + 130, 490, 120, nm[0]), T(x + 20, 620, 340, 40, nm + " · Niv. 14", center=True, size=26, bold=True),
               BAR(x + 30, 670, 320, 26, a / 100, f"Fatigue {b} % → {a} %"), BAR(x + 30, 710, 320, 20, 0.6, "XP +120")]
screen("W14", "resultats", "Résultats (combat / zone)", [I(0, 0, 1920, 1080, "Arène voilée", plain=True), DIM(),
    HD(560, 90, 800, 130, "VICTOIRE !", 1, size=110),
    E("pill", 560, 260, 250, 64, "+300", tag="CR"), E("pill", 835, 260, 250, 64, "+0", tag="PR"), E("pill", 1110, 260, 250, 64, "+40", tag="XP"),
    E("anchor", 560, 260, 10, 10, "", 2),
    T(560, 340, 800, 60, "Vérification serveur ✓", center=True, size=22),
] + _cards + [
    P(160, 800, 1220, 110, "Objectifs : ☑ générateur · ☑ Brick · ☐ grue — encore 1 !", 4),
    BP(1420, 800, 420, 110, "CONTINUER", 5),
    NOTE(1420, 940, 420, 110, "Variante zone validée : « ZONE SÉCURISÉE », 1 500 CR + 40 PR, mode difficile débloqué", 6),
], [
    "Bandeau Victoire / Défaite (défaite : « l'équipe recule d'une case »).",
    "Récompenses qui défilent vers la barre de ressources ; validation serveur du combat (rejeu) en tâche de fond.",
    "Escouade : fatigue avant → après (+12 %, +15 % de plus en défaite), XP gagnée, montée de niveau.",
    "Progression des objectifs de la zone.",
    "Continuer : retour au plateau (ou au QG si toute l'escouade est épuisée).",
    "Variante fin de zone : récompenses de validation et déblocage du mode difficile, retour au QG.",
])

# ------------------------------------------------------------------ W15 Recrutement
_pool = []
for i, (nm, rar, fam, cost) in enumerate([("Argile", "Agent", "Matière", "150"), ("Acide", "Élite", "Élémentaire", "450"),
                                          ("Rempart", "Élite", "Physique/Bio", "450"), ("Instant", "LÉGENDE", "Perception", "1 200"), ("Mue", "Agent", "Matière", "150")]):
    x = 40 + i * 370
    _pool += [P(x, 130, 350, 720, "", 1 if i == 0 else None, dark=(rar == "LÉGENDE")), I(x + 20, 150, 310, 330, "Portrait"),
              TAG(x + 20, 496, 150, 44, rar, 2 if i == 3 else None, on=(rar == "LÉGENDE")), TAG(x + 180, 496, 150, 44, fam),
              HD(x + 20, 556, 310, 50, nm.upper(), size=34), T(x + 20, 610, 310, 110, "Rôle · 3 compétences + passive\nCombo de famille", size=20),
              BP(x + 20, 760, 310, 76, f"{cost} PR")]
screen("W15", "recrutement", "Recrutement", topbar("RECRUTEMENT — POOL DE NEW YORK") + _pool + [
    B(40, 880, 620, 90, "Renouveler la pool · gratuit (1/1)", 3), T(680, 880, 520, 90, "Nouvelle pool dans 14:22:05", size=24),
    BAR(1220, 900, 660, 46, 12 / 35, "Collection New York · 12 / 35", 4),
    E("anchor", 1150, 300, 10, 10, "", 5),
], [
    "Pool du jour : 5 cartes, choix direct à coût fixe en PR (pas de gacha, pas de doublons).",
    "Rareté (Agent / Élite / Légende) : cadre et coût croissants ; une Légende liée à l'histoire entre dans la pool après chaque mini-climax.",
    "Renouvellement : 1 gratuit par jour, puis 3 000 Crédits, puis 50 Gemmes.",
    "Collection de la ville : pool épuisée = collection terminée.",
    "Toucher une carte : fiche complète (W16) avant d'acheter ; confirmation d'achat en modale.",
])

# ------------------------------------------------------------------ W16 Équipe / fiche
screen("W16", "fiche-membre", "Équipe / fiche membre", topbar("ÉQUIPE") + [
    P(40, 130, 340, 900, "Effectif (35 max)", 1),
] + [C(70 + (i % 3) * 100, 190 + (i // 3) * 110, 86, n) for i, n in enumerate(["An", "Ve", "Gi", "Éc", "Ar", "Re", "Sè", "Ét", "Ar", "Mu", "Ac", "Ma", "Ai", "…"])] + [
    I(400, 130, 560, 640, "Illustration du membre", 2),
    HD(400, 790, 560, 60, "GIVRE", size=48), T(400, 850, 560, 40, "Légende · Élémentaire · Dégâts · Niv. 14", size=22),
    BAR(400, 900, 560, 30, 0.6, "XP 2 340 / 3 900"), BAR(400, 944, 560, 30, 0.2, "Fatigue 20 %", 7),
    P(990, 130, 890, 150, "PV 2 196 · ATQ 285 · DÉF 154 · VIT 115", 3),
    P(990, 300, 890, 470, "Compétences", 4),
    T(1010, 350, 850, 400, "A1 Morsure — dégâts 100 % · Gel 2 t (40 %)   ◆◇◇\nA2 Souffle polaire — tous les ennemis · 70 % · jauge −15 %   ◆◇◇\nA3 Zéro absolu — 220 % · recharge 5 t   ◆◆◇\nPassive : Froid mordant — +15 % de dégâts sur les cibles gelées", size=22),
    BP(1460, 690, 400, 66, "Palier 2 · 4 000 CR", 5),
    P(990, 790, 890, 240, "Combos", 6),
    T(1010, 840, 850, 180, "Tempête de cristal (duo avec Verre)\nCorps élémentaire (avec Physique/Bio) · Point faible (avec Perception)\nRésonance élémentaire (avec Élémentaire)", size=21),
], [
    "Effectif : grille de portraits, tri et filtres (famille, rareté, niveau, fatigue).",
    "Illustration en pied ; balayer à gauche/droite = membre suivant.",
    "Statistiques au niveau actuel (malus de fatigue affiché en orange s'il s'applique).",
    "Compétences actives et passive avec description générée à partir des effets ; losanges = paliers validés.",
    "Validation du prochain palier (niveau requis atteint).",
    "Combos du membre, duo légendaire en tête.",
    "XP et fatigue ; raccourcis vers le salon et l'infirmerie.",
])

# ------------------------------------------------------------------ W17 Pass
_tiles = []
for i in range(8):
    x = 40 + i * 175; lvl = 10 + i
    _tiles += [P(x, 330, 160, 220, f"{lvl}", dark=(lvl <= 12)), I(x + 20, 370, 120, 120, "CR" if i % 2 else "PR"),
               P(x, 580, 160, 220, f"{lvl}", dark=False), I(x + 20, 620, 120, 120, "🔒 Tenue" if i == 3 else "🔒")]
screen("W17", "pass", "Pass saisonnier", topbar("PASS — SAISON 1 : L'AUBE DE NEW YORK") + [
    C(40, 120, 150, "12", 1, size=56), BAR(210, 150, 1000, 40, 0.45, "Palier 12 · 585 / 1 300 XP"), T(210, 200, 1000, 40, "Fin de saison dans 41 j", size=22),
    T(40, 290, 300, 40, "GRATUIT", size=24, bold=True), T(40, 556, 300, 30, "PREMIUM", size=24, bold=True),
] + _tiles + [
    E("anchor", 40, 330, 10, 10, "", 2),
    P(560, 315, 6, 500, "", bg="#E5484D", radius=3, n=3),
    BP(1460, 330, 420, 130, "PASS PREMIUM\n9,99 €", 4),
    P(1460, 490, 420, 540, "Missions du jour", 5),
    T(1480, 540, 380, 470, "☑ Lancer le dé 5 fois · 100 XP\n☑ Gagner 2 combats · 100 XP\n☐ Collecter le labo · 100 XP\n☐ Lancer une expédition · 100 XP\n☐ Utiliser un combo · 100 XP", size=22),
    B(40, 860, 1360, 90, "Tout réclamer (3 récompenses)"),
], [
    "Palier actuel, progression d'XP, temps restant de la saison (42 jours, 50 paliers).",
    "Deux pistes alignées par palier : gratuite (Crédits, PR, objets) et premium (tenues, thème de QG, Gemmes, PR).",
    "Marqueur du palier courant ; défilement horizontal, palier courant centré à l'ouverture.",
    "Achat du pass premium (rétroactif sur les paliers déjà atteints).",
    "Missions quotidiennes (500 XP par jour) avec réclamation.",
])

# ------------------------------------------------------------------ W18 Boutique
screen("W18", "boutique", "Boutique", topbar("BOUTIQUE") + [
    TAG(40, 130, 170, 60, "Offres", 1, on=True), TAG(225, 130, 170, 60, "Gemmes"), TAG(410, 130, 150, 60, "PR"),
    TAG(575, 130, 240, 60, "Accélérateurs"), TAG(830, 130, 220, 60, "Cosmétiques"),
    I(40, 220, 1840, 300, "Offre de bienvenue · 500 Gemmes + accélérateur 8 h · 2,99 € (une seule fois)", 2),
] + sum([[P(40 + i * 372, 550, 352, 380, ""), I(70 + i * 372, 570, 292, 190, nm), T(60 + i * 372, 770, 312, 60, g, center=True, size=24, bold=True),
          BP(80 + i * 372, 840, 272, 70, p)]
         for i, (nm, g, p) in enumerate([("Poignée", "100 GM", "0,99 €"), ("Sac", "520 GM (+4 %)", "4,99 €"), ("Caisse", "1 080 GM (+8 %)", "9,99 €"),
                                         ("Coffre", "2 250 GM (+12 %)", "19,99 €"), ("Chambre forte", "6 000 GM (+20 %)", "49,99 €")])], []) + [
    E("anchor", 40, 550, 10, 10, "", 3),
    T(40, 960, 1840, 50, "Prix TTC · aucun achat aléatoire · l'argent fait uniquement gagner du temps (hors cosmétiques)", 4, center=True, size=20),
], [
    "Onglets : offres, Gemmes, packs de PR, accélérateurs, cosmétiques (tenues, décor du QG, dés et pions).",
    "Mise en avant : offre de bienvenue (une fois), offres d'événement.",
    "Packs de Gemmes avec bonus croissant ; prix localisés par le store.",
    "Mentions obligatoires et principe « pas de pay-to-win, pas de loot box ».",
])

# ------------------------------------------------------------------ W19 Paramètres
screen("W19", "parametres", "Paramètres", topbar("PARAMÈTRES") + [
    P(40, 130, 400, 900, "", 1),
] + [B(60, 160 + i * 110, 360, 90, t) for i, t in enumerate(["Audio", "Affichage", "Accessibilité", "Langue", "Compte", "Confidentialité"])] + [
    P(470, 130, 1410, 600, "Accessibilité", 2),
    T(500, 190, 500, 50, "Taille du texte", size=26), BAR(1000, 200, 800, 30, 0.5, "100 %"),
    T(500, 270, 500, 50, "Mode daltonien (icônes renforcées)", size=26), TAG(1600, 270, 200, 54, "Activé", on=True),
    T(500, 350, 500, 50, "Réduction des flashs", size=26), TAG(1600, 350, 200, 54, "Désactivé"),
    T(500, 430, 600, 50, "Temps étendu pour les mini-jeux (+50 %)", size=26), TAG(1600, 430, 200, 54, "Désactivé"),
    T(500, 510, 500, 50, "Sous-titres", size=26), TAG(1600, 510, 200, 54, "Activé", on=True),
    T(500, 590, 500, 50, "Vitesse de combat par défaut", size=26), TAG(1600, 590, 200, 54, "x1"),
    P(470, 760, 1410, 270, "Compte", 3),
    B(500, 820, 420, 80, "Lier Google / Apple / Steam"), B(940, 820, 420, 80, "Exporter mes données"), B(1380, 820, 470, 80, "Supprimer mon compte"),
    T(500, 930, 1300, 60, "ID joueur : guest-3f9a…  ·  Version 0.1 (règles 0.1.0)", 4, size=20),
], [
    "Catégories : audio (musique, effets, voix), affichage, accessibilité, langue, compte, confidentialité.",
    "Accessibilité au lancement : taille de texte, mode daltonien (familles = icône + couleur), flashs, temps étendu, sous-titres.",
    "Compte : liaison, export et suppression des données (RGPD, stores).",
    "Identifiant et versions (utile au support).",
])
