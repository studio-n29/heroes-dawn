"""Mini-DSL de wireframes : éléments positionnés en absolu sur un cadre 1920×1080, rendus en PNG par Chromium."""
import html

W, H = 1920, 1080


def E(t, x, y, w, h, text="", n=None, **kw):
    d = dict(t=t, x=x, y=y, w=w, h=h, text=text, n=n)
    d.update(kw)
    return d


# Raccourcis
def P(x, y, w, h, text="", n=None, dark=False, **kw): return E("panel", x, y, w, h, text, n, dark=dark, **kw)
def B(x, y, w, h, text="", n=None, **kw): return E("btn", x, y, w, h, text, n, **kw)
def BP(x, y, w, h, text="", n=None, **kw): return E("btnp", x, y, w, h, text, n, **kw)
def I(x, y, w, h, text="", n=None, **kw): return E("img", x, y, w, h, text, n, **kw)
def T(x, y, w, h, text="", n=None, size=22, **kw): return E("txt", x, y, w, h, text, n, size=size, **kw)
def HD(x, y, w, h, text="", n=None, size=40, **kw): return E("title", x, y, w, h, text, n, size=size, **kw)
def C(x, y, d, text="", n=None, **kw): return E("circle", x, y, d, d, text, n, **kw)
def BAR(x, y, w, h, fill=0.6, text="", n=None, **kw): return E("bar", x, y, w, h, text, n, fill=fill, **kw)
def BADGE(x, y, text="!", n=None): return E("badge", x, y, 34, 34, text, n)
def TAG(x, y, w, h, text="", n=None, on=False): return E("tag", x, y, w, h, text, n, on=on)
def DIM(): return E("dim", 0, 0, W, H)
def NOTE(x, y, w, h, text, n=None): return E("note", x, y, w, h, text, n)
def LINE(x1, y1, x2, y2, thick=8, dashed=False, color="#9AA0AB", **kw):
    import math
    length = math.hypot(x2 - x1, y2 - y1)
    ang = math.degrees(math.atan2(y2 - y1, x2 - x1))
    return E("line", (x1 + x2) / 2 - length / 2, (y1 + y2) / 2 - thick / 2, length, thick, "", None, ang=ang, dashed=dashed, color=color, **kw)
def FOG(x, y, w, h, text="Brouillard", n=None): return E("fog", x, y, w, h, text, n)


def topbar(title, back=True, n_back=None, n_res=None, n_title=None, gems=True):
    els = [P(0, 0, W, 96, dark=True, flat=True)]
    if back:
        els.append(B(24, 18, 150, 60, "◄ Retour", n_back))
        els.append(HD(196, 18, 760, 60, title, n_title, size=38))
    else:
        els.append(HD(32, 18, 900, 60, title, n_title, size=38))
    x = 1010
    res = [("CR", "12 450"), ("PR", "185")] + ([("GM", "320")] if gems else [])
    for tag, val in res:
        els.append(E("pill", x, 22, 230, 52, val, None, tag=tag))
        x += 250
    els.append(C(x + 10, 20, 56, "⚙"))
    if n_res:
        els.append(E("anchor", 1010, 22, 10, 10, "", n_res))
    return els


CSS = """
@font-face{font-family:Bangers;src:url('file://__FONTS__/Bangers-Regular.ttf');}
*{box-sizing:border-box;margin:0;padding:0}
body{width:1920px;height:1080px;overflow:hidden;font-family:Carlito,'DejaVu Sans',sans-serif;background:#F3F4F6;color:#2B2F38}
.frame{position:relative;width:1920px;height:1080px;background:#F3F4F6;overflow:hidden}
.e{position:absolute;display:flex;align-items:center;justify-content:center;text-align:center;line-height:1.15}
.panel{background:#fff;border:3px solid #8A909C;border-radius:12px;align-items:flex-start;justify-content:flex-start;padding:12px 16px;font-size:20px;color:#6A707C}
.panel.dark{background:#DADDE3}
.panel.flat{border-radius:0;border-width:0 0 3px 0}
.btn{background:#C9CED7;border:3px solid #5E6574;border-radius:12px;font-weight:700;font-size:22px;text-transform:uppercase;letter-spacing:.5px}
.btnp{background:#3E4452;border:3px solid #22262E;border-radius:12px;font-weight:700;font-size:26px;color:#fff;text-transform:uppercase;letter-spacing:1px}
.img{background:#E3E6EB;border:3px solid #A3A9B4;border-radius:10px;font-style:italic;color:#7B818C;font-size:22px;
     background-image:linear-gradient(to top right,transparent calc(50% - 1.5px),#B9BEC7 calc(50% - 1.5px),#B9BEC7 calc(50% + 1.5px),transparent calc(50% + 1.5px)),
                      linear-gradient(to bottom right,transparent calc(50% - 1.5px),#B9BEC7 calc(50% - 1.5px),#B9BEC7 calc(50% + 1.5px),transparent calc(50% + 1.5px))}
.img.plain{background-image:none;background:#E9EBEF;align-items:flex-start;justify-content:flex-start;padding:10px 14px;font-size:18px}
.img span{background:#E3E6EB;padding:4px 10px;border-radius:6px}
.txt{justify-content:flex-start;text-align:left;color:#4A505C}
.txt.center{justify-content:center;text-align:center}
.title{justify-content:flex-start;text-align:left;font-weight:700;color:#22262E}
.circle{border-radius:50%;background:#C9CED7;border:3px solid #5E6574;font-weight:700;font-size:22px}
.bar{background:#E3E6EB;border:2px solid #7B818C;border-radius:8px;overflow:hidden;font-size:15px;font-weight:700}
.bar i{position:absolute;left:0;top:0;bottom:0;background:#9AA0AB}
.bar b{position:relative}
.badge{border-radius:50%;background:#22262E;color:#fff;font-weight:700;font-size:20px}
.tag{border:2px solid #7B818C;border-radius:30px;font-size:18px;font-weight:700;background:#fff}
.tag.on{background:#3E4452;color:#fff;border-color:#22262E}
.pill{background:#fff;border:3px solid #5E6574;border-radius:30px;justify-content:flex-end;padding-right:18px;font-weight:700;font-size:24px}
.pill em{position:absolute;left:6px;top:4px;width:38px;height:38px;border-radius:50%;background:#5E6574;color:#fff;font-style:normal;font-size:15px;display:flex;align-items:center;justify-content:center}
.dim{background:rgba(40,44,52,.55)}
.note{border:2px dashed #E5484D;border-radius:10px;color:#C2353A;font-size:18px;font-weight:700;background:rgba(255,255,255,.85);padding:6px 10px}
.anchor{opacity:0}
.line{border-radius:4px}
.fog{background:radial-gradient(ellipse at 30% 50%,rgba(70,76,88,.55),rgba(70,76,88,.82));color:#E8EAF0;font-size:30px;font-weight:700;letter-spacing:3px;text-transform:uppercase;border-radius:30px}
.num{position:absolute;width:46px;height:46px;border-radius:50%;background:#E5484D;border:3px solid #fff;color:#fff;font-weight:700;font-size:24px;
     display:flex;align-items:center;justify-content:center;box-shadow:0 2px 6px rgba(0,0,0,.35);z-index:50;font-family:Carlito,sans-serif}
.safe{position:absolute;top:0;bottom:0;border-left:2px dashed rgba(229,72,77,.35);z-index:40}
.foot{position:absolute;right:0;bottom:0;padding:4px 14px;background:rgba(243,244,246,.92);border-top-left-radius:10px;font-size:16px;color:#8A909C;z-index:60;font-family:Carlito,sans-serif}
.foot b{font-family:Bangers,sans-serif;font-size:20px;letter-spacing:1px;color:#5E6574;font-weight:400}
"""


def render_html(screen_id, name, elements, fonts_dir):
    out = []
    for e in elements:
        t = e["t"]
        style = f"left:{e['x']}px;top:{e['y']}px;width:{e['w']}px;height:{e['h']}px;"
        cls = ["e", t]
        if t == "panel" and e.get("dark"): cls.append("dark")
        if t == "panel" and e.get("flat"): cls.append("flat")
        if t == "txt" and e.get("center"): cls.append("center")
        if t == "tag" and e.get("on"): cls.append("on")
        if t == "img" and e.get("plain"): cls.append("plain")
        if "size" in e: style += f"font-size:{e['size']}px;"
        if e.get("bold"): style += "font-weight:700;"
        if e.get("radius"): style += f"border-radius:{e['radius']}px;"
        if e.get("bg"): style += f"background:{e['bg']};"
        if e.get("z"): style += f"z-index:{e['z']};"
        if t == "line":
            style += f"transform:rotate({e['ang']}deg);"
            style += (f"background:repeating-linear-gradient(90deg,{e['color']} 0 14px,transparent 14px 24px);" if e['dashed'] else f"background:{e['color']};")
        txt = html.escape(e["text"]).replace("\n", "<br>")
        if t == "img":
            inner = f"<span>{txt}</span>" if txt else ""
        elif t == "bar":
            inner = f"<i style='width:{int(e['fill']*100)}%'></i><b>{txt}</b>"
        elif t == "pill":
            inner = f"<em>{e['tag']}</em>{txt}"
        else:
            inner = txt
        out.append(f"<div class='{' '.join(cls)}' style='{style}'>{inner}</div>")
        if e.get("n") is not None:
            nx, ny = e["x"] - 16, e["y"] - 16
            if e.get("npos") == "r": nx = e["x"] + e["w"] - 30
            out.append(f"<div class='num' style='left:{nx}px;top:{ny}px'>{e['n']}</div>")
    body = "".join(out)
    safe = "<div class='safe' style='left:88px'></div><div class='safe' style='left:1832px'></div>"
    foot = f"<div class='foot'><b>HEROES DAWN</b> · {html.escape(screen_id)} · {html.escape(name)} · wireframe</div>"
    css = CSS.replace("__FONTS__", fonts_dir)
    return f"<!doctype html><html><head><meta charset='utf-8'><style>{css}</style></head><body><div class='frame'>{body}{safe}{foot}</div></body></html>"
