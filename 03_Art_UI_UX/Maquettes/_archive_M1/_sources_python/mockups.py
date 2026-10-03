"""Maquettes stylisées (DA comics « Secret Wars 2015 ») + planche kit UI, rendues en PNG 1920×1080."""
import asyncio, os

FONTS = "/mnt/user-data/uploads/Documents/Heroes Dawn/Code/Heroes Dawn Unity/Assets/_Project/Resources/Fonts"
ART = "/home/claude/art"
CROPS = "/home/claude/mock/art"
OUT = "/home/claude/mock/out/stylisees"
os.makedirs(OUT, exist_ok=True)

FAM = {"M": "#F2A93B", "E": "#3BC7F2", "P": "#E5484D", "V": "#9B6BF2"}
FAM_NAME = {"M": "MATIÈRE", "E": "ÉLÉMENTAIRE", "P": "PHYSIQUE / BIO", "V": "PERCEPTION"}

CSS = f"""
@font-face{{font-family:Bangers;src:url('file://{FONTS}/Bangers-Regular.ttf');}}
@font-face{{font-family:Neue;font-weight:700;src:url('file://{FONTS}/ComicNeue-Bold.ttf');}}
@font-face{{font-family:Neue;font-weight:400;src:url('file://{FONTS}/ComicNeue-Regular.ttf');}}
*{{box-sizing:border-box;margin:0;padding:0}}
body{{width:1920px;height:1080px;overflow:hidden;background:#101218;color:#fff;font-family:Neue,sans-serif;font-weight:700}}
.abs{{position:absolute}}
.bg{{position:absolute;inset:0;background-size:cover;background-position:center}}
.halftone{{background-image:radial-gradient(rgba(255,255,255,.07) 1.6px,transparent 1.8px);background-size:11px 11px}}
.panel{{position:absolute;background:#1E222B;border:5px solid #000;border-radius:14px;box-shadow:9px 9px 0 rgba(0,0,0,.85)}}
.cap{{position:absolute;background:#FFD23F;color:#101218;border:4px solid #000;font-family:Bangers;letter-spacing:1px;box-shadow:5px 5px 0 #000;padding:4px 14px}}
.title{{font-family:Bangers;letter-spacing:1.5px;font-weight:400}}
.ink{{text-shadow:3px 3px 0 #000,-2px -2px 0 #000,2px -2px 0 #000,-2px 2px 0 #000,5px 6px 0 rgba(0,0,0,.6)}}
.btn{{position:absolute;display:flex;align-items:center;justify-content:center;font-family:Bangers;letter-spacing:1.5px;border:5px solid #000;border-radius:14px;
      box-shadow:7px 7px 0 #000;background:#FFD23F;color:#101218;text-align:center;line-height:1}}
.btn.red{{background:#E5484D;color:#fff}} .btn.cyan{{background:#3BC7F2}} .btn.dark{{background:#2B3140;color:#fff}} .btn.off{{background:#3A4152;color:#7E8697;box-shadow:4px 4px 0 #000}}
.pill{{position:absolute;height:60px;border:4px solid #000;border-radius:32px;background:#101218;display:flex;align-items:center;justify-content:flex-end;padding-right:20px;font-family:Bangers;font-size:34px;letter-spacing:1px}}
.pill i{{position:absolute;left:6px;top:5px;width:42px;height:42px;border-radius:50%;display:flex;align-items:center;justify-content:center;font-style:normal;font-size:20px;color:#101218;border:3px solid #000}}
.bar{{position:absolute;border:3px solid #000;border-radius:8px;background:#0B0D12;overflow:hidden}}
.bar b{{position:absolute;left:0;top:0;bottom:0;border-radius:4px}}
.bar span{{position:absolute;inset:0;display:flex;align-items:center;justify-content:center;font-size:15px;text-shadow:1px 1px 0 #000,-1px -1px 0 #000}}
.por{{position:absolute;border-radius:50%;border:5px solid #000;background-size:cover;background-position:center;box-shadow:4px 4px 0 rgba(0,0,0,.7)}}
.badge{{position:absolute;min-width:42px;height:42px;border-radius:21px;background:#E5484D;border:4px solid #000;font-family:Bangers;font-size:24px;display:flex;align-items:center;justify-content:center;padding:0 8px}}
.chip{{position:absolute;border:3px solid #000;border-radius:20px;padding:2px 12px;font-size:17px;color:#101218}}
.foot{{position:absolute;right:16px;bottom:8px;font-size:15px;color:rgba(255,255,255,.55);font-weight:400}}
.foot b{{font-family:Bangers;font-weight:400;letter-spacing:1px;font-size:19px;color:rgba(255,255,255,.75)}}
"""


def page(body, sid, name):
    return (f"<!doctype html><html><head><meta charset='utf-8'><style>{CSS}</style></head><body>{body}"
            f"<div class='foot'><b>HEROES DAWN</b> · {sid} · {name} · maquette stylisée (portraits provisoires extraits de l'illustration clé)</div></body></html>")


def topbar(title, back=True, logo=False):
    s = "<div class='abs halftone' style='left:0;top:0;width:1920px;height:100px;background-color:#0B0D12;border-bottom:5px solid #000'></div>"
    x = 24
    if back:
        s += "<div class='btn dark' style='left:24px;top:18px;width:150px;height:62px;font-size:28px'>◄ RETOUR</div>"; x = 200
    if logo:
        s += f"<img class='abs' src='file://{ART}/HD_Logo_1024.png' style='left:14px;top:-8px;width:118px'>"; x = 140
    s += f"<div class='abs title ink' style='left:{x}px;top:16px;font-size:54px;color:#FFD23F'>{title}</div>"
    s += ("<div class='pill' style='left:1150px;top:20px;width:230px'><i style='background:#FFD23F'>CR</i>12 450</div>"
          "<div class='pill' style='left:1400px;top:20px;width:190px'><i style='background:#7FE0C8'>PR</i>185</div>"
          "<div class='pill' style='left:1610px;top:20px;width:190px'><i style='background:#9B6BF2;color:#fff'>GM</i>320</div>"
          "<div class='btn dark' style='left:1820px;top:20px;width:66px;height:60px;font-size:30px;border-radius:50%'>⚙</div>")
    return s


def bar(x, y, w, h, ratio, color, label=""):
    return f"<div class='bar' style='left:{x}px;top:{y}px;width:{w}px;height:{h}px'><b style='width:{ratio*100:.0f}%;background:{color}'></b><span>{label}</span></div>"


def por(x, y, d, img, fam, extra=""):
    return f"<div class='por' style='left:{x}px;top:{y}px;width:{d}px;height:{d}px;background-image:url(file://{CROPS}/{img}.jpg);border-color:#000;outline:5px solid {FAM[fam]};outline-offset:-10px;{extra}'></div>"


# ------------------------------------------------------------------ S01 Titre
def s01():
    b = f"<div class='bg' style='background-image:url(file://{ART}/HD_BG_Landscape.jpg)'></div>"
    b += "<div class='abs' style='inset:0;background:linear-gradient(180deg,rgba(16,18,24,.25),rgba(16,18,24,0) 40%,rgba(16,18,24,.85) 80%,#101218)'></div>"
    b += f"<img class='abs' src='file://{ART}/HD_Title.png' style='left:460px;top:10px;width:1000px;filter:drop-shadow(0 18px 24px rgba(0,0,0,.7))'>"
    b += "<div class='abs title ink' style='left:0;width:1920px;top:650px;text-align:center;font-size:44px;color:#fff'>NEW YORK TIENT ENCORE. POUR COMBIEN DE TEMPS ?</div>"
    b += "<div class='btn' style='left:700px;top:740px;width:520px;height:120px;font-size:68px'>JOUER</div>"
    b += ("<div class='abs' style='left:0;width:1920px;top:900px;text-align:center;font-size:24px;color:#C9CED7'>"
          "Lier un compte : <u>Google</u> · <u>Apple</u> · <u>Steam</u> &nbsp;—&nbsp; ou jouer en invité</div>")
    b += "<div class='btn dark' style='left:1690px;top:28px;width:200px;height:62px;font-size:28px'>FR ▾</div>"
    b += "<div class='abs title' style='left:40px;top:1010px;font-size:30px;color:#FFD23F;letter-spacing:5px'>STUDIO N29</div>"
    return page(b, "S01", "Titre")


# ------------------------------------------------------------------ S02 QG
def room(x, y, w, h, name, lvl, pos, badge="", status=""):
    s = (f"<div class='panel' style='left:{x}px;top:{y}px;width:{w}px;height:{h}px;overflow:hidden;"
         f"background:url(file://{ART}/HD_BG_Landscape.jpg) {pos}/1920px auto'></div>")
    s += f"<div class='abs' style='left:{x+5}px;top:{y+5}px;width:{w-10}px;height:{h-10}px;border-radius:9px;background:linear-gradient(180deg,rgba(16,18,24,.15),rgba(16,18,24,.75))'></div>"
    s += f"<div class='cap' style='left:{x-10}px;top:{y-14}px;font-size:28px'>{name}</div>"
    s += f"<div class='abs title ink' style='left:{x+20}px;top:{y+h-56}px;font-size:30px'>{lvl}</div>"
    if status:
        s += f"<div class='abs' style='left:{x+20}px;top:{y+h-92}px;font-size:21px;color:#E8EAF0;text-shadow:1px 1px 0 #000'>{status}</div>"
    if badge:
        s += f"<div class='badge' style='left:{x+w-34}px;top:{y-18}px'>{badge}</div>"
    return s


def s02():
    b = "<div class='bg halftone' style='background-color:#14171F'></div>" + topbar("QG — SECTION NEW YORK", back=False, logo=True)
    b += room(50, 140, 450, 380, "SALON D'ENTRAÎNEMENT", "NIV. 3", "-200px -300px", "2", "Givre · Arc · entraînement fini")
    b += room(530, 140, 430, 380, "INFIRMERIE", "NIV. 2", "-700px -350px", "", "Givre récupère · 01:20")
    b += room(990, 140, 450, 380, "SALLE DES OPÉRATIONS", "NIV. 2", "-1100px -250px", "!", "Expédition revenue")
    b += room(50, 560, 660, 390, "LABO DE RECHERCHE", "NIV. 2", "-900px -600px", "18", "18 PR prêts à collecter")
    b += room(740, 560, 700, 390, "ARCHIVES", "12 / 35", "-50px -500px", "", "Collection New York")
    b += ("<div class='btn red' style='left:1480px;top:140px;width:400px;height:190px;font-size:76px;"
          "background:radial-gradient(circle at 50% 50%,#FF6A6E,#E5484D 60%,#B5262B)'>MISSION</div>")
    for i, (t, c, bd) in enumerate([("RECRUTEMENT", "cyan", "N"), ("ÉQUIPE", "", ""), ("PASS · PALIER 12", "dark", ""), ("BOUTIQUE", "dark", "")]):
        y = 370 + i * 118
        b += f"<div class='btn {c}' style='left:1480px;top:{y}px;width:400px;height:94px;font-size:38px'>{t}</div>"
        if bd:
            b += f"<div class='badge' style='left:1846px;top:{y-16}px'>{bd}</div>"
    b += bar(1520, 600, 320, 18, 0.45, "#FFD23F")
    b += ("<div class='cap' style='left:50px;top:985px;width:1390px;height:70px;font-size:30px;display:flex;align-items:center'>"
          "3 OBJECTIFS SECONDAIRES DISPONIBLES · PROCHAINE VAGUE DANS 02:41:10 ▸</div>")
    b += "<div class='btn' style='left:1480px;top:985px;width:400px;height:76px;font-size:32px'>MISSIONS DU JOUR 3/5</div>"
    return page(b, "S02", "QG — hub")


# ------------------------------------------------------------------ S03 Plateau
NODES = {"n00": ("QG", 0, 2, "#5B6478"), "n01": ("$", 1, 2, "#F2C23B"), "n02": ("", 2, 2, "#3A4152"), "n03": ("VS", 3, 3, "#E88A4D"),
         "n04": ("$", 4, 3, "#F2C23B"), "n05": ("!", 5, 3, "#9B6BF2"), "n06": ("$", 3, 1, "#F2C23B"), "n07": ("VS", 4, 1, "#E88A4D"),
         "n08": ("MJ", 5, 1, "#3BC7F2"), "n09": ("", 6, 2, "#3A4152"), "n10": ("Zz", 7, 2, "#4CC96B")}
LINKS = [("n00", "n01"), ("n01", "n02"), ("n02", "n03"), ("n02", "n06"), ("n03", "n04"), ("n04", "n05"), ("n06", "n07"), ("n07", "n08"),
         ("n05", "n09"), ("n08", "n09"), ("n09", "n10"), ("n10", "x1"), ("n10", "x2")]


def npos(n):
    if n == "x1": return 1330, 410
    if n == "x2": return 1330, 790
    _, x, y, _ = NODES[n]
    return 280 + x * 135, 600 - (y - 2) * 190


def s03():
    b = (f"<div class='bg' style='background-image:url(file://{ART}/HD_BG_Landscape.jpg);filter:blur(6px) brightness(.35) saturate(1.2);transform:scale(1.05)'></div>"
         "<div class='bg halftone'></div>")
    svg = "<svg class='abs' style='left:0;top:0' width='1920' height='1080'>"
    for a, c in LINKS:
        (x1, y1), (x2, y2) = npos(a), npos(c)
        hl = (a, c) in [("n02", "n03"), ("n03", "n04")]
        svg += f"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='#000' stroke-width='20' stroke-linecap='round'/>"
        dash = 'stroke-dasharray="2 16"' if hl else ''
        col = '#FFD23F' if hl else '#C9CED7'
        svg += f"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='{col}' stroke-width='9' stroke-linecap='round' {dash}/>"
    svg += "</svg>"
    b += svg
    dest = {"n03": 1, "n06": 1, "n04": 2, "n07": 2, "n05": 3, "n08": 3}
    for n, (g, x, y, col) in NODES.items():
        cx, cy = npos(n)
        if n in dest:
            b += f"<div class='abs' style='left:{cx-66}px;top:{cy-66}px;width:132px;height:132px;border-radius:50%;border:7px solid #FFD23F;box-shadow:0 0 30px #FFD23F'></div>"
            b += f"<div class='badge' style='left:{cx+30}px;top:{cy-80}px;background:#FFD23F;color:#101218'>{dest[n]}</div>"
        b += (f"<div class='abs title ink' style='left:{cx-48}px;top:{cy-48}px;width:96px;height:96px;border-radius:50%;background:{col};border:6px solid #000;"
              f"display:flex;align-items:center;justify-content:center;font-size:40px;box-shadow:5px 5px 0 rgba(0,0,0,.7)'>{g}</div>")
        if g == "VS":
            b += f"<div class='chip' style='left:{cx-46}px;top:{cy+52}px;background:#E88A4D'>FACILE</div>"
    # brouillard
    b += ("<div class='abs' style='left:1170px;top:110px;width:750px;height:740px;background:radial-gradient(ellipse at 40% 50%,rgba(40,44,60,.7),rgba(12,14,20,.96) 70%);"
          "-webkit-mask-image:linear-gradient(90deg,transparent,#000 22%)'></div>")
    b += "<div class='abs title ink' style='left:1440px;top:560px;font-size:56px;color:rgba(255,255,255,.55);letter-spacing:8px'>BROUILLARD</div>"
    for i, (x, y) in enumerate([(1360, 410), (1360, 790), (1520, 600), (1660, 410), (1660, 790), (1810, 480)]):
        b += f"<div class='abs title' style='left:{x-30}px;top:{y-30}px;width:60px;height:60px;border-radius:50%;background:rgba(255,255,255,.08);border:3px dashed rgba(255,255,255,.3);display:flex;align-items:center;justify-content:center;font-size:30px;color:rgba(255,255,255,.4)'>?</div>"
    # pion
    cx, cy = npos("n02")
    b += por(cx - 50, cy - 150, 100, "p_colosse", "P", "outline-color:#FFD23F;box-shadow:0 10px 18px rgba(0,0,0,.8)")
    b += f"<div class='abs' style='left:{cx-12}px;top:{cy-56}px;width:0;height:0;border-left:12px solid transparent;border-right:12px solid transparent;border-top:18px solid #000'></div>"
    b += topbar("ZONE 1.1 · RED HOOK — LES QUAIS")
    b += ("<div class='cap' style='left:30px;top:124px;width:560px;font-size:32px'>OBJECTIFS</div>"
          "<div class='panel' style='left:30px;top:178px;width:560px;height:170px;padding:16px 22px;font-size:25px;line-height:1.55'>"
          "<span style='color:#4CC96B'>✔ Couper le générateur du quai</span><br>☐ Vaincre Brick à l'entrepôt<br>☐ Atteindre la grue du terminal</div>")
    b += ("<div class='cap' style='left:700px;top:124px;font-size:34px;transform:rotate(-2deg);background:#fff'>NOUVEAU SECTEUR EN VUE !</div>")
    # escouade
    b += "<div class='panel halftone' style='left:24px;top:880px;width:660px;height:180px'></div>"
    for i, (img, fam, f) in enumerate([("p_colosse", "P", .3), ("p_noir", "E", .2), ("p_blanc", "V", .72), ("p_rouge", "P", .4)]):
        x = 48 + i * 158
        b += por(x, 896, 104, img, fam)
        b += bar(x - 4, 1012, 116, 22, f, "#8FD16B" if f < .6 else "#F2873B")
    # dé
    b += "<div class='panel halftone' style='left:1420px;top:840px;width:476px;height:220px'></div>"
    b += "<div class='abs title' style='left:1446px;top:866px;width:150px;height:150px;background:#fff;color:#101218;border:6px solid #000;border-radius:22px;display:flex;align-items:center;justify-content:center;font-size:120px;box-shadow:6px 6px 0 #000;transform:rotate(-6deg)'>3</div>"
    b += "<div class='abs title ink' style='left:1630px;top:858px;font-size:64px;color:#FFD23F'>3 PM</div>"
    b += "<div class='btn dark' style='left:1630px;top:950px;width:240px;height:80px;font-size:36px'>TERMINER</div>"
    b += bar(1446, 1028, 150, 20, 4 / 6, "#FFD23F", "4/6")
    return page(b, "S03", "Plateau — choix de destination")


# ------------------------------------------------------------------ S04 Combat
def s04():
    b = (f"<div class='bg' style='background-image:url(file://{ART}/HD_BG_Landscape.jpg);filter:blur(3px) brightness(.4) saturate(1.3) hue-rotate(-15deg)'></div>"
         "<div class='bg halftone'></div>")
    # alliés : cases de BD inclinées (splash)
    for i, (img, fam, rot) in enumerate([("p_colosse", "P", -4), ("p_noir", "E", 3), ("p_blanc", "V", -2), ("p_rouge", "P", 4)]):
        x = 60 + i * 215; y = 190 + (i % 2) * 70
        b += (f"<div class='abs' style='left:{x}px;top:{y}px;width:220px;height:330px;border:6px solid #000;transform:rotate({rot}deg);"
              f"background:url(file://{CROPS}/{img}.jpg) center/cover;box-shadow:8px 8px 0 rgba(0,0,0,.8);outline:4px solid {FAM[fam]};outline-offset:-12px'></div>")
    # ennemis : silhouettes
    for i, (nm, hp, atb, sc) in enumerate([("COGNEUR", .8, .3, 1.15), ("VOYOU", .45, .9, 1.0), ("PYRO DE RUE", 1.0, .6, 1.0)]):
        x = 1130 + i * 250; y = 230 + (i % 2) * 70
        tgt = i == 1
        b += (f"<div class='abs' style='left:{x}px;top:{y+70}px;width:{int(200*sc)}px;height:{int(300*sc)}px;border-radius:90px 90px 20px 20px;"
              f"background:radial-gradient(circle at 50% 22%,#3A2028 0 18%,#1A0E12 19%),linear-gradient(#24121A,#0B0608);border:5px solid #000;"
              f"box-shadow:0 0 30px rgba(229,72,77,.45)'></div>")
        b += f"<div class='abs' style='left:{x+70}px;top:{y+130}px;width:60px;height:10px;background:radial-gradient(circle,#FF3B3F 30%,transparent 32%) 0 0/30px 10px repeat-x'></div>"
        b += f"<div class='cap' style='left:{x}px;top:{y-14}px;font-size:24px;background:#E5484D;color:#fff'>{nm}</div>"
        b += bar(x, y + 28, 200, 22, hp, "#E5484D") + bar(x, y + 52, 200, 12, atb, "#FFD23F")
        if tgt:
            b += f"<div class='abs' style='left:{x-24}px;top:{y+50}px;width:250px;height:360px;border:7px dashed #FFD23F;border-radius:30px'></div>"
    b += "<div class='abs title' style='left:1390px;top:330px;font-size:84px;color:#FFD23F;transform:rotate(-8deg);text-shadow:4px 4px 0 #000,-3px -3px 0 #000,3px -3px 0 #000,-3px 3px 0 #000'>-312!</div>"
    b += ("<div class='abs title' style='left:1560px;top:250px;width:250px;height:110px;display:flex;align-items:center;justify-content:center;font-size:42px;color:#101218;"
          "background:#fff;clip-path:polygon(50% 0,61% 30%,95% 12%,74% 45%,100% 60%,68% 66%,78% 100%,50% 75%,22% 100%,32% 66%,0 60%,26% 45%,5% 12%,39% 30%)'>CRIT !</div>")
    # barre du haut
    b += ("<div class='abs' style='left:0;top:0;width:1920px;height:100px;background:#0B0D12;border-bottom:5px solid #000'></div>"
          "<div class='btn dark' style='left:24px;top:18px;width:220px;height:62px;font-size:30px'>RETRAITE</div>"
          "<div class='abs title ink' style='left:0;width:1920px;text-align:center;top:16px;font-size:58px;color:#FFD23F'>COMBAT · NORMAL</div>"
          "<div class='btn' style='left:1550px;top:18px;width:170px;height:62px;font-size:32px'>AUTO</div>"
          "<div class='btn dark' style='left:1740px;top:18px;width:150px;height:62px;font-size:32px'>X1</div>")
    b += "<div class='cap' style='left:760px;top:130px;font-size:40px;background:#fff'>CHOISIS UNE CIBLE</div>"
    # compétences
    sk = [("COUP D'AMARRE", "Dégâts 110 % · Provoc. 30 %", "", "Q"), ("ANCRAGE", "Provocation + bouclier 20 %", "cyan", "W"), ("MUR VIVANT", "Recharge : 2 tours", "off", "E")]
    for i, (n, d, c, k) in enumerate(sk):
        x = 140 + i * 420
        b += f"<div class='btn {c}' style='left:{x}px;top:690px;width:395px;height:150px;flex-direction:column;gap:8px;font-size:40px'>{n}<span style=\"font-family:Neue;font-size:21px;letter-spacing:0\">{d}</span></div>"
        b += f"<div class='badge' style='left:{x-12}px;top:676px;background:#101218;color:#fff'>{k}</div>"
    # combo
    b += ("<div class='abs title' style='left:1450px;top:640px;width:440px;height:230px;display:flex;flex-direction:column;align-items:center;justify-content:center;"
          "font-size:74px;color:#fff;background:conic-gradient(from 0deg,#E5484D 0 25%,#3BC7F2 25% 50%,#E5484D 50% 75%,#3BC7F2 75%);"
          "clip-path:polygon(50% 0,60% 18%,82% 6%,80% 28%,100% 30%,88% 50%,100% 70%,80% 72%,82% 94%,60% 82%,50% 100%,40% 82%,18% 94%,20% 72%,0 70%,12% 50%,0 30%,20% 28%,18% 6%,40% 18%);"
          "text-shadow:4px 4px 0 #000,-3px -3px 0 #000,3px -3px 0 #000,-3px 3px 0 #000'>COMBO !<span style='font-size:30px'>CORPS ÉLÉMENTAIRE</span></div>")
    # portraits
    for i, (img, fam, nm, hp, atb, rdy) in enumerate([("p_colosse", "P", "ANCRE", .9, 1, True), ("p_noir", "E", "GIVRE", .6, 1, True),
                                                       ("p_blanc", "V", "ÉCHO", .75, .4, False), ("p_rouge", "P", "REMPART", .5, .7, False)]):
        x = 140 + i * 420
        sel = i == 0
        b += f"<div class='panel halftone' style='left:{x}px;top:875px;width:400px;height:185px;{'border-color:#FFD23F;background-color:#3A3420' if sel else ''}'></div>"
        b += por(x + 16, 895, 140, img, fam)
        b += f"<div class='abs title ink' style='left:{x+170}px;top:888px;font-size:38px;color:{'#FFD23F' if rdy else '#fff'}'>{nm}{' · PRÊT' if rdy else ''}</div>"
        b += bar(x + 170, 942, 210, 26, hp, "#4CC96B", "PV") + bar(x + 170, 976, 210, 18, atb, "#FFD23F")
        b += f"<div class='abs' style='left:{x+170}px;top:1004px;font-size:19px;color:#C9CED7'>Fatigue {['30', '20', '72', '40'][i]} %</div>"
    return page(b, "S04", "Combat — ciblage et combo")


# ------------------------------------------------------------------ S05 Recrutement
def s05():
    b = (f"<div class='bg' style='background-image:url(file://{ART}/HD_BG_Portrait.jpg);background-size:1920px auto;background-position:0 -900px;filter:blur(8px) brightness(.3)'></div>"
         "<div class='bg halftone'></div>" + topbar("RECRUTEMENT — POOL DE NEW YORK"))
    cards = [("p_noir", "E", "ACIDE", "ÉLITE", "Dégâts", "450", "#C9D1E0"), ("p_rouge", "P", "REMPART", "ÉLITE", "Tank", "450", "#C9D1E0"),
             ("p_blanc", "V", "INSTANT", "LÉGENDE", "Précision", "1 200", "#FFD23F"), ("p_colosse", "P", "ROC", "AGENT", "Tank", "150", "#B07A4A"),
             (None, "M", "ARGILE", "AGENT", "Contrôle", "150", "#B07A4A")]
    for i, (img, fam, nm, rar, role, cost, rc) in enumerate(cards):
        x = 40 + i * 372; leg = rar == "LÉGENDE"
        y = 130 if not leg else 118
        b += f"<div class='panel' style='left:{x}px;top:{y}px;width:350px;height:{740 if not leg else 764}px;{'border-color:#FFD23F;box-shadow:0 0 40px rgba(255,210,63,.5),9px 9px 0 #000' if leg else ''}'></div>"
        if img:
            b += f"<div class='abs' style='left:{x+16}px;top:{y+16}px;width:318px;height:380px;border:5px solid #000;border-radius:8px;background:url(file://{CROPS}/{img}.jpg) center/cover'></div>"
        else:
            b += f"<div class='abs title' style='left:{x+16}px;top:{y+16}px;width:318px;height:380px;border:5px solid #000;border-radius:8px;background:linear-gradient(160deg,#5A3A12,#1E222B);display:flex;align-items:center;justify-content:center;font-size:120px;color:rgba(242,169,59,.5)'>?</div>"
        b += f"<div class='abs' style='left:{x+16}px;top:{y+396}px;width:318px;height:12px;background:{FAM[fam]};border:3px solid #000'></div>"
        b += f"<div class='cap' style='left:{x+30}px;top:{y+366}px;font-size:24px;background:{rc}'>{rar}</div>"
        b += f"<div class='abs title ink' style='left:{x+24}px;top:{y+424}px;font-size:56px'>{nm}</div>"
        b += f"<div class='chip' style='left:{x+24}px;top:{y+494}px;background:{FAM[fam]}'>{FAM_NAME[fam]}</div>"
        b += f"<div class='chip' style='left:{x+24}px;top:{y+532}px;background:#C9CED7'>{role}</div>"
        b += f"<div class='abs' style='left:{x+24}px;top:{y+574}px;width:300px;font-size:19px;font-weight:400;color:#C9CED7'>3 compétences + passive · combo de famille</div>"
        b += f"<div class='btn {'cyan' if not leg else ''}' style='left:{x+24}px;top:{y+640 + (24 if leg else 0)}px;width:302px;height:78px;font-size:40px'>{cost} PR</div>"
    b += "<div class='btn dark' style='left:40px;top:930px;width:620px;height:90px;font-size:34px'>RENOUVELER LA POOL · GRATUIT 1/1</div>"
    b += "<div class='abs' style='left:690px;top:958px;font-size:26px;color:#C9CED7'>Nouvelle pool dans 14:22:05</div>"
    b += "<div class='abs title ink' style='left:1220px;top:915px;font-size:30px'>COLLECTION NEW YORK</div>" + bar(1220, 960, 660, 44, 12 / 35, "#3BC7F2", "12 / 35")
    return page(b, "S05", "Recrutement")


# ------------------------------------------------------------------ S06 Kit UI
def s06():
    b = "<div class='bg halftone' style='background-color:#14171F'></div>"
    b += "<div class='abs title ink' style='left:40px;top:20px;font-size:68px;color:#FFD23F'>KIT UI — COMPOSANTS</div>"
    b += "<div class='abs' style='left:1290px;top:30px;width:600px;font-size:21px;color:#C9CED7;font-weight:400'>Référence comics : Secret Wars (2015) · contours noirs épais, ombres décalées, récitatifs jaunes</div>"
    # boutons
    b += "<div class='cap' style='left:40px;top:120px;font-size:26px'>BOUTONS</div>"
    for i, (t, c) in enumerate([("PRINCIPAL", ""), ("DANGER", "red"), ("SECONDAIRE", "dark"), ("INFO", "cyan"), ("INDISPONIBLE", "off")]):
        b += f"<div class='btn {c}' style='left:{40 + i * 250}px;top:180px;width:225px;height:84px;font-size:34px'>{t}</div>"
    # récitatif + case
    b += "<div class='cap' style='left:40px;top:310px;font-size:26px'>RÉCITATIF / TITRE DE FENÊTRE</div>"
    b += "<div class='cap' style='left:40px;top:370px;font-size:44px'>ZONE SÉCURISÉE !</div>"
    b += "<div class='cap' style='left:460px;top:370px;font-size:44px;background:#fff'>NOUVEAU SECTEUR EN VUE !</div>"
    b += "<div class='cap' style='left:40px;top:460px;font-size:26px'>CASE DE BD (PANNEAU)</div>"
    b += "<div class='panel halftone' style='left:40px;top:520px;width:600px;height:220px;padding:20px;font-size:26px;line-height:1.4'>Fond #1E222B, contour noir 5 px,<br>rayon 14 px, ombre décalée 9 px.<br><span style='font-weight:400;color:#C9CED7'>Texte courant en Comic Neue.</span></div>"
    # jauges
    b += "<div class='cap' style='left:700px;top:460px;font-size:26px'>JAUGES</div>"
    for i, (lab, r, c) in enumerate([("PV allié", .75, "#4CC96B"), ("PV ennemi", .45, "#E5484D"), ("ATB", .9, "#FFD23F"), ("Fatigue < 60 %", .4, "#8FD16B"),
                                     ("Fatigue > 60 % (malus)", .78, "#F2873B"), ("Bouclier", .3, "#9FD8FF")]):
        y = 520 + i * 40
        b += f"<div class='abs' style='left:700px;top:{y}px;font-size:21px;width:260px'>{lab}</div>" + bar(960, y, 340, 28, r, c)
    # familles
    b += "<div class='cap' style='left:1360px;top:120px;font-size:26px'>FAMILLES (COULEUR + ICÔNE)</div>"
    for i, (k, ico) in enumerate([("M", "◆"), ("E", "ϟ"), ("P", "✊"), ("V", "◉")]):
        y = 180 + i * 78
        b += f"<div class='abs title' style='left:1360px;top:{y}px;width:64px;height:64px;border-radius:50%;background:{FAM[k]};border:5px solid #000;display:flex;align-items:center;justify-content:center;font-family:DejaVu Sans;font-size:30px;color:#101218'>{ico}</div>"
        b += f"<div class='abs title ink' style='left:1440px;top:{y+8}px;font-size:40px;color:{FAM[k]}'>{FAM_NAME[k]}</div>"
        b += f"<div class='abs' style='left:1740px;top:{y+18}px;font-size:20px;color:#C9CED7'>{FAM[k]}</div>"
    # pastilles, badges, portraits
    b += "<div class='cap' style='left:1360px;top:510px;font-size:26px'>PORTRAIT · BADGES · PILULES</div>"
    b += por(1360, 570, 130, "p_blanc", "V")
    b += "<div class='badge' style='left:1470px;top:560px'>3</div>"
    b += "<div class='pill' style='left:1530px;top:580px;width:260px'><i style='background:#FFD23F'>CR</i>12 450</div>"
    b += "<div class='pill' style='left:1530px;top:660px;width:260px'><i style='background:#7FE0C8'>PR</i>185</div>"
    # typo
    b += "<div class='cap' style='left:40px;top:790px;font-size:26px'>TYPOGRAPHIE</div>"
    b += "<div class='abs title ink' style='left:40px;top:850px;font-size:96px;color:#fff'>BANGERS — TITRES 96</div>"
    b += "<div class='abs' style='left:40px;top:975px;font-size:34px'>Comic Neue Bold — boutons secondaires, libellés (34)</div>"
    b += "<div class='abs' style='left:40px;top:1022px;font-size:26px;font-weight:400;color:#C9CED7'>Comic Neue Regular — descriptions, infobulles (26). Accents : é è à ç œ « » …</div>"
    # retours de combat
    b += "<div class='cap' style='left:1100px;top:790px;font-size:26px'>RETOURS VISUELS</div>"
    b += "<div class='abs title' style='left:1100px;top:850px;font-size:80px;color:#FFD23F;transform:rotate(-6deg);text-shadow:4px 4px 0 #000,-3px -3px 0 #000,3px -3px 0 #000,-3px 3px 0 #000'>-312!</div>"
    b += "<div class='abs title' style='left:1330px;top:860px;font-size:64px;color:#4CC96B;text-shadow:4px 4px 0 #000,-3px -3px 0 #000,3px -3px 0 #000,-3px 3px 0 #000'>+180</div>"
    b += "<div class='abs title' style='left:1520px;top:860px;font-size:60px;color:#C9CED7;text-shadow:4px 4px 0 #000,-3px -3px 0 #000,3px -3px 0 #000,-3px 3px 0 #000'>RATÉ</div>"
    b += ("<div class='abs title' style='left:1690px;top:840px;width:200px;height:110px;display:flex;align-items:center;justify-content:center;font-size:36px;color:#101218;background:#fff;"
          "clip-path:polygon(50% 0,61% 30%,95% 12%,74% 45%,100% 60%,68% 66%,78% 100%,50% 75%,22% 100%,32% 66%,0 60%,26% 45%,5% 12%,39% 30%)'>CRIT !</div>")
    return page(b, "S06", "Kit UI")


async def main():
    from playwright.async_api import async_playwright
    async with async_playwright() as p:
        br = await p.chromium.launch()
        pg = await br.new_page(viewport={"width": 1920, "height": 1080})
        for sid, slug, fn in [("S01", "titre", s01), ("S02", "qg", s02), ("S03", "plateau", s03), ("S04", "combat", s04),
                              ("S05", "recrutement", s05), ("S06", "kit-ui", s06)]:
            f = f"/home/claude/mock/tmp_{sid}.html"
            open(f, "w", encoding="utf-8").write(fn())
            await pg.goto("file://" + f)
            await pg.wait_for_timeout(300)
            await pg.screenshot(path=f"{OUT}/{sid}-{slug}.png")
        await br.close()

asyncio.run(main())
print("ok")
