// Sommaire du prototype cliquable (_sources/index.html) : vignettes de toutes les maquettes, groupées, chacune ouvre sa page.
// Usage : node gen-index.js
const fs = require("fs"), path = require("path");
const dir = path.join(__dirname, "pages");
const pages = fs.readdirSync(dir).filter((f) => f.endsWith(".html") && !f.startsWith("_")).sort().map((f) => {
  const body = /<body([^>]*)>/.exec(fs.readFileSync(path.join(dir, f), "utf8"))[1];
  const a = (n) => new RegExp(`data-${n}="([^"]*)"`).exec(body)?.[1] ?? "";
  return { f, ref: a("ref"), title: a("title"), group: a("group"), out: a("out") || "ecrans" };
});
const groups = [...new Set(pages.map((p) => p.group))];
const esc = (s) => s.replace(/&/g, "&amp;").replace(/</g, "&lt;");
const html = `<!doctype html><html lang="fr"><head><meta charset="utf-8"><title>Heroes Dawn · Maquettes</title>
<style>
@font-face { font-family: Bangers; src: url("fonts/Bangers-Regular.ttf"); }
@font-face { font-family: Neue; font-weight: 700; src: url("fonts/ComicNeue-Bold.ttf"); }
body { margin: 0; padding: 32px 48px; background: #101218; color: #F4F1EA; font-family: Neue, sans-serif; }
h1 { font-family: Bangers; font-size: 56px; letter-spacing: 1.5px; color: #FFD23F; margin: 0 0 8px; }
h2 { font-family: Bangers; font-size: 32px; letter-spacing: 1px; color: #FFD23F; margin: 32px 0 12px; }
p { color: #A7AFBD; margin: 0 0 8px; }
.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 18px; }
a { color: inherit; text-decoration: none; background: #1E222B; border: 4px solid #000; border-radius: 12px; box-shadow: 6px 6px 0 #000; overflow: hidden; display: block; }
a:hover { border-color: #FFD23F; }
img { width: 100%; display: block; aspect-ratio: 16 / 9; object-fit: cover; }
span { display: block; padding: 8px 12px; font-size: 16px; } b { font-family: Bangers; font-weight: 400; letter-spacing: 1px; color: #FFD23F; margin-right: 6px; }
</style></head><body>
<h1>HEROES DAWN · MAQUETTES</h1>
<p>Prototype cliquable : ouvrir une page puis naviguer comme dans le jeu (onglets, MISSION, Retour, ⚙…). Commencer par <a style="display:inline;padding:0 6px;box-shadow:none;border-width:2px" href="pages/M01-titre.html">M01 Titre</a>.</p>
<p>Images exportées dans Maquettes/ecrans, tutoriel et reference (pwsh export.ps1). Doc : 03_Art_UI_UX/03_Maquettes_Screen_Flow.md.</p>
${groups.map((g) => `<h2>${esc(g)}</h2><div class="grid">${pages.filter((p) => p.group === g).map((p) =>
  `<a href="pages/${p.f}"><img src="../${p.out}/${p.f.replace(".html", ".png")}" alt=""><span><b>${p.ref}</b>${esc(p.title)}</span></a>`).join("")}</div>`).join("\n")}
</body></html>
`;
fs.writeFileSync(path.join(__dirname, "index.html"), html);
console.log(`index.html : ${pages.length} pages, ${groups.length} groupes`);
