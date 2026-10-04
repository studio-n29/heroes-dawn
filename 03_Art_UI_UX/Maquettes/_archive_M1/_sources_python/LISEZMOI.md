# Sources des maquettes

Scripts Python qui génèrent les images de `Maquettes/` (Chromium headless via Playwright, `pip install playwright` puis `playwright install chromium`).

- `screens_a.py`, `screens_b.py` : définition des 21 wireframes (éléments positionnés + légendes) ; `wf_lib.py` : mini-DSL et rendu HTML.
- `render_wf.py` : wireframes → `wireframes/*.png`.
- `flow.py` : screen flow et parcours → `flow/*.png`.
- `mockups.py` : maquettes stylisées et kit UI → `stylisees/*.png`.
- `build_md.py` : régénère `03_Maquettes_Screen_Flow.md` à partir des légendes.

Les chemins en tête de fichier (polices, illustrations, dossier de sortie) sont à adapter à ta machine.
