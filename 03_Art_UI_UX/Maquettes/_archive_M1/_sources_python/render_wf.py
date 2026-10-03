import asyncio, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import screens_a, screens_b  # noqa: F401  (enregistre les écrans)
from screens_a import SCREENS
from wf_lib import render_html

FONTS = "/mnt/user-data/uploads/Documents/Heroes Dawn/Code/Heroes Dawn Unity/Assets/_Project/Resources/Fonts"
OUT = "/home/claude/mock/out/wireframes"
os.makedirs(OUT, exist_ok=True)
only = sys.argv[1:]


async def main():
    from playwright.async_api import async_playwright
    async with async_playwright() as p:
        b = await p.chromium.launch()
        pg = await b.new_page(viewport={"width": 1920, "height": 1080})
        for s in SCREENS:
            if only and s["id"] not in only:
                continue
            f = f"/home/claude/mock/tmp_{s['id']}.html"
            open(f, "w", encoding="utf-8").write(render_html(s["id"], s["name"], s["els"], FONTS))
            await pg.goto("file://" + f)
            await pg.wait_for_timeout(100)
            await pg.screenshot(path=f"{OUT}/{s['id']}-{s['slug']}.png")
        await b.close()

asyncio.run(main())

from PIL import Image
import glob
fs = sorted(glob.glob(OUT + "/*.png"))
th = [Image.open(f).resize((480, 270)) for f in fs]
for k in range(0, len(th), 8):
    grp = th[k:k + 8]
    c = Image.new("RGB", (960, 270 * ((len(grp) + 1) // 2)), "white")
    for i, t in enumerate(grp):
        c.paste(t, ((i % 2) * 480, (i // 2) * 270))
    c.save(f"/home/claude/mock/sheet{k // 8}.png")
print(len(SCREENS), "écrans rendus")
