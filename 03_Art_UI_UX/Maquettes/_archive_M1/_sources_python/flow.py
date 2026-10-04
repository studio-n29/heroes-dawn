"""Screen flow global + bandes de parcours (session type, onboarding, boucle de retour)."""
import asyncio, html, os

FONTS = "/mnt/user-data/uploads/Documents/Heroes Dawn/Code/Heroes Dawn Unity/Assets/_Project/Resources/Fonts"
WF = "/home/claude/mock/out/wireframes"
OUT = "/home/claude/mock/out/flow"
os.makedirs(OUT, exist_ok=True)

BASE_CSS = f"""
@font-face{{font-family:Bangers;src:url('file://{FONTS}/Bangers-Regular.ttf');}}
*{{box-sizing:border-box;margin:0;padding:0}}
body{{width:1920px;height:1080px;overflow:hidden;background:#F3F4F6;font-family:Carlito,sans-serif;color:#22262E}}
h1{{position:absolute;left:40px;top:22px;font-family:Bangers;font-weight:400;font-size:54px;letter-spacing:2px}}
h1 small{{font-family:Carlito;font-size:22px;letter-spacing:0;color:#6A707C;margin-left:14px}}
"""

# ------------------------------------------------------------------ Screen flow
N = {}  # id -> (x, y, w, h, label, ref, kind)


def node(i, x, y, label, ref="", kind="screen", w=180, h=70):
    N[i] = (x, y, w, h, label, ref, kind)


node("start", 40, 470, "Lancement", "", "start", 170, 56)
node("W01", 40, 560, "Titre / connexion", "W01")
node("pro", 280, 330, "Prologue (~5 min)", "1er lancement", "flow", 190, 70)
node("W02", 280, 540, "QG — hub", "W02", "hub", 210, 110)
for k, (i, lab) in enumerate([("W03", "Salon d'entraînement"), ("W04", "Infirmerie"), ("W05", "Salle des opérations"),
                              ("W06", "Labo de recherche"), ("W07", "Archives")]):
    node(i, 600, 110 + k * 86, lab, i)
for k, (i, lab) in enumerate([("W15", "Recrutement"), ("W16", "Équipe / fiche"), ("W17", "Pass saisonnier"),
                              ("W18", "Boutique"), ("W19", "Paramètres")]):
    node(i, 310, 700 + k * 74, lab, i, w=190, h=62)
node("W08", 860, 550, "Carte de la ville", "W08")
node("W09", 1065, 550, "Préparation", "W09")
node("W10", 1270, 535, "Plateau", "W10a / W10b", "hub", 190, 100)
node("W11", 1500, 330, "Combat", "W11a / W11b")
node("W12", 1500, 440, "Mini-jeu", "W12", "modal")
node("W13", 1500, 550, "Événement", "W13", "modal")
node("rest", 1500, 660, "Repos (planque)", "modale", "modal")
node("W14", 1720, 330, "Résultats", "W14", "screen", 170, 70)


def R(i):  # bord droit, milieu
    x, y, w, h, *_ = N[i]; return x + w, y + h / 2
def Lft(i):
    x, y, w, h, *_ = N[i]; return x, y + h / 2
def Top(i, dx=0.5):
    x, y, w, h, *_ = N[i]; return x + w * dx, y
def Bot(i, dx=0.5):
    x, y, w, h, *_ = N[i]; return x + w * dx, y + h


def curve(p1, p2, dashed=False, bold=False, label="", lx=None, ly=None, color="#3E4452"):
    (x1, y1), (x2, y2) = p1, p2
    mx = (x1 + x2) / 2
    d = f"M{x1},{y1} C{mx},{y1} {mx},{y2} {x2},{y2}"
    return path(d, dashed, bold, label, lx if lx is not None else mx, ly if ly is not None else (y1 + y2) / 2 - 8, color)


def path(d, dashed=False, bold=False, label="", lx=0, ly=0, color="#3E4452"):
    dash = "stroke-dasharray='10 8'" if dashed else ""
    sw = 5 if bold else 3
    s = f"<path d='{d}' fill='none' stroke='{color}' stroke-width='{sw}' {dash} marker-end='url(#arr)'/>"
    if label:
        s += (f"<text x='{lx}' y='{ly}' text-anchor='middle' font-size='18' font-weight='700' fill='{color}' "
              f"paint-order='stroke' stroke='#F3F4F6' stroke-width='6'>{html.escape(label)}</text>")
    return s


def flow_html():
    edges = [
        curve(Bot("start"), Top("W01")) if False else path(f"M{Bot('start')[0]},{Bot('start')[1]} L{Top('W01')[0]},{Top('W01')[1]}"),
        curve(R("W01"), Lft("W02"), bold=True),
        curve(R("W01"), Lft("pro"), dashed=True, label="1er lancement", lx=235, ly=420),
        path(f"M{Bot('pro')[0]},{Bot('pro')[1]} L{Top('W02')[0]},{Top('W02')[1]}", dashed=True),
    ]
    for i in ["W03", "W04", "W05", "W06", "W07"]:
        edges.append(curve(R("W02"), Lft(i)))
    for i in ["W15", "W16", "W17", "W18", "W19"]:
        x, y, w, h, *_ = N[i]
        edges.append(path(f"M300,630 H272 V{y + h / 2} H{x}"))
    edges += [
        curve(R("W02"), Lft("W08"), bold=True, label="MISSION", lx=690, ly=578),
        curve(R("W08"), Lft("W09"), bold=True), curve(R("W09"), Lft("W10"), bold=True),
    ]
    for i in ["W11", "W12", "W13", "rest"]:
        edges.append(curve(R("W10"), Lft(i)))
    edges += [
        curve(R("W11"), Lft("W14"), bold=True),
        # Résultats → Plateau (continuer)
        path(f"M{Top('W14')[0]},{Top('W14')[1]} C{Top('W14')[0]},230 {Top('W10')[0]},230 {Top('W10')[0]},{Top('W10')[1]}",
             label="Continuer → plateau", lx=1560, ly=246),
        # Résultats → QG (zone validée / escouade épuisée)
        path(f"M{Bot('W14')[0]},{Bot('W14')[1]} V880 H540 V640 H490", dashed=True,
             label="Zone sécurisée, ou escouade épuisée → QG", lx=1180, ly=870, color="#C2353A"),
        # Plateau → QG (abandon)
        path(f"M{Bot('W10', 0.3)[0]},{Bot('W10')[1]} V820 H560 V622 H490", dashed=True,
             label="Retour QG (brouillard refermé)", lx=960, ly=810, color="#C2353A"),
        # Recrutement → fiche
        path(f"M{N['W15'][0] + 190},{N['W15'][1] + 31} C540,{N['W15'][1] + 31} 540,{N['W16'][1] + 31} {N['W16'][0] + 190},{N['W16'][1] + 31}",
             label="fiche", lx=600, ly=770),
    ]
    boxes = []
    for i, (x, y, w, h, lab, ref, kind) in N.items():
        cls = kind
        boxes.append(f"<div class='n {cls}' style='left:{x}px;top:{y}px;width:{w}px;height:{h}px'><b>{html.escape(lab)}</b>"
                     + (f"<i>{html.escape(ref)}</i>" if ref else "") + "</div>")
    notes = [
        ("Retour = écran parent (bouton ◄, geste retour, Échap).", 620, 960),
        ("⚙ Paramètres accessibles depuis toutes les barres du haut.", 620, 995),
        ("Mini-jeu, événement, repos → retour direct au plateau.", 1370, 786),
    ]
    note_html = "".join(f"<div class='note' style='left:{x}px;top:{y}px'>{html.escape(t)}</div>" for t, x, y in notes)
    legend = """<div class='legend'>
      <div><span class='sw screen'></span>Écran</div><div><span class='sw hub'></span>Écran central (hub)</div>
      <div><span class='sw modal'></span>Modale (par-dessus le plateau)</div><div><span class='sw flow'></span>Séquence scénarisée</div>
      <div><svg width='60' height='14'><line x1='0' y1='7' x2='60' y2='7' stroke='#3E4452' stroke-width='5'/></svg>Parcours principal</div>
      <div><svg width='60' height='14'><line x1='0' y1='7' x2='60' y2='7' stroke='#C2353A' stroke-width='3' stroke-dasharray='10 8'/></svg>Conditionnel / sortie</div>
    </div>"""
    css = BASE_CSS + """
.n{position:absolute;background:#fff;border:3px solid #5E6574;border-radius:12px;display:flex;flex-direction:column;align-items:center;justify-content:center;text-align:center;line-height:1.1;z-index:2}
.n b{font-size:20px}.n i{font-size:15px;color:#6A707C;font-style:normal;margin-top:3px}
.n.hub{background:#3E4452;border-color:#22262E;color:#fff}.n.hub i{color:#C9CED7}.n.hub b{font-size:24px}
.n.modal{border-style:dashed;background:#F8F9FB}
.n.flow{background:#FFF6D6;border-color:#B08A1E}
.n.start{border-radius:30px;background:#22262E;color:#fff}
svg.edges{position:absolute;left:0;top:0;z-index:1}
.note{position:absolute;font-size:18px;color:#4A505C;font-weight:700}
.legend{position:absolute;right:30px;top:24px;background:#fff;border:2px solid #9AA0AB;border-radius:12px;padding:12px 18px;display:grid;grid-template-columns:auto auto;gap:8px 26px;font-size:17px;z-index:3}
.legend div{display:flex;align-items:center;gap:10px}
.sw{display:inline-block;width:34px;height:22px;border-radius:6px;border:3px solid #5E6574;background:#fff}
.sw.hub{background:#3E4452}.sw.modal{border-style:dashed}.sw.flow{background:#FFF6D6;border-color:#B08A1E}
.group{position:absolute;border:2px dashed #B9BEC7;border-radius:16px;z-index:0}
.group span{position:absolute;top:-14px;left:16px;background:#F3F4F6;padding:0 8px;font-size:17px;font-weight:700;color:#6A707C}
"""
    groups = ("<div class='group' style='left:580px;top:92px;width:222px;height:450px'><span>Pièces du QG</span></div>"
              "<div class='group' style='left:290px;top:682px;width:228px;height:392px'><span>Menus du hub</span></div>"
              "<div class='group' style='left:840px;top:300px;width:860px;height:470px'><span>Boucle de mission</span></div>")
    svg = ("<svg class='edges' width='1920' height='1080'><defs><marker id='arr' markerUnits='userSpaceOnUse' markerWidth='16' markerHeight='16' refX='14' refY='8' orient='auto'>"
           "<path d='M0,0 L16,8 L0,16 z' fill='context-stroke'/></marker></defs>" + "".join(edges) + "</svg>")
    return (f"<!doctype html><html><head><meta charset='utf-8'><style>{css}</style></head><body>"
            f"<h1>SCREEN FLOW <small>Heroes Dawn · navigation complète · références = wireframes Wxx</small></h1>"
            f"{groups}{svg}{''.join(boxes)}{note_html}{legend}</body></html>")


# ------------------------------------------------------------------ Bandes de parcours
ROWS = [
    ("SESSION TYPE (5-10 MIN)", [("W02-qg", "QG : collecter"), ("W08-carte", "Carte : reprendre la zone"), ("W10a-plateau-lancer", "Plateau : 2-4 lancers"),
                                 ("W11a-combat-pret", "1 combat"), ("W14-resultats", "Résultats"), ("W02-qg", "Retour au QG")]),
    ("PREMIER LANCEMENT — PROLOGUE (~5 MIN)", [("W01-titre", "Titre : jouer"), ("W13-evenement", "Scène : Rayner confie la section"),
                                               ("W10b-plateau-destination", "Mini-plateau scripté (8 cases)"), ("W11b-combat-ciblage", "1er combat + combo guidé"),
                                               ("W02-qg", "Arrivée au QG"), ("W15-recrutement", "1er recrutement gratuit")]),
    ("BOUCLE DE RETOUR (2 MIN)", [("W06-labo", "Collecter le labo"), ("W05-operations", "Récupérer les expéditions"), ("W05-operations", "Relancer les expéditions"),
                                  ("W04-infirmerie", "Placer les fatigués"), ("W15-recrutement", "Regarder la pool")]),
]


def strips_html():
    css = BASE_CSS + """
.row{position:absolute;left:40px;right:40px}
.row h2{font-family:Bangers;font-weight:400;font-size:32px;letter-spacing:1px;margin-bottom:8px}
.items{display:flex;align-items:flex-start;gap:0}
.it{width:262px;text-align:center}
.it img{width:262px;height:147px;border:3px solid #3E4452;border-radius:10px;display:block;background:#fff}
.it p{font-size:19px;font-weight:700;margin-top:6px;line-height:1.1}
.it .k{display:inline-block;width:30px;height:30px;border-radius:50%;background:#E5484D;color:#fff;font-size:18px;line-height:30px;margin-right:6px}
.arr{width:44px;text-align:center;font-size:40px;color:#5E6574;margin-top:48px}
"""
    parts = []
    for r, (title, items) in enumerate(ROWS):
        top = 110 + r * 320
        cells = []
        for k, (img, cap) in enumerate(items):
            if k: cells.append("<div class='arr'>➜</div>")
            cells.append(f"<div class='it'><img src='file://{WF}/{img}.png'><p><span class='k'>{k + 1}</span>{html.escape(cap)}</p></div>")
        parts.append(f"<div class='row' style='top:{top}px'><h2>{html.escape(title)}</h2><div class='items'>{''.join(cells)}</div></div>")
    return (f"<!doctype html><html><head><meta charset='utf-8'><style>{css}</style></head><body>"
            f"<h1>PARCOURS JOUEUR <small>enchaînements d'écrans des trois parcours clés</small></h1>{''.join(parts)}</body></html>")


async def main():
    from playwright.async_api import async_playwright
    async with async_playwright() as p:
        b = await p.chromium.launch()
        pg = await b.new_page(viewport={"width": 1920, "height": 1080})
        for name, content in [("F01-screen-flow", flow_html()), ("F02-parcours", strips_html())]:
            f = f"/home/claude/mock/tmp_{name}.html"
            open(f, "w", encoding="utf-8").write(content)
            await pg.goto("file://" + f)
            await pg.wait_for_timeout(200)
            await pg.screenshot(path=f"{OUT}/{name}.png")
        await b.close()

asyncio.run(main())
print("ok")
