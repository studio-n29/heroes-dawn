// Vérifie les pages de maquettes : identité, pastilles ↔ légendes, liens du prototype.
// Usage : node check.js [--strict]  (code 0 = tout est bon ; --strict : les liens vers des pages pas encore écrites sont des erreurs)
const fs = require("fs"), path = require("path");
const dir = path.join(__dirname, "pages");
const pages = fs.readdirSync(dir).filter(f => f.endsWith(".html"));
const refs = new Set();
const parsed = pages.map(f => {
  const html = fs.readFileSync(path.join(dir, f), "utf8");
  const body = /<body([^>]*)>/.exec(html)?.[1] ?? "";
  const attr = n => new RegExp(`data-${n}="([^"]*)"`).exec(body)?.[1];
  const ref = attr("ref");
  if (ref) refs.add(ref);
  return { f, html, ref, title: attr("title") };
});
const errors = [], pending = [];
const strict = process.argv.includes("--strict");
for (const p of parsed) {
  if (!p.ref) errors.push(`${p.f} : data-ref manquant`);
  if (!p.title) errors.push(`${p.f} : data-title manquant`);
  const pins = [...p.html.matchAll(/class="pin[^"]*"[^>]*data-n="(\d+)"/g)].map(m => +m[1]);
  const legend = /<ol class="legend">([\s\S]*?)<\/ol>/.exec(p.html)?.[1] ?? "";
  const items = (legend.match(/<li>/g) || []).length;
  const distinct = new Set(pins).size;
  if (distinct !== items) errors.push(`${p.f} : ${distinct} pastilles, ${items} légendes`);
  for (let n = 1; n <= distinct; n++) if (!pins.includes(n)) errors.push(`${p.f} : pastille ${n} absente`);
  for (const m of p.html.matchAll(/data-go="([^"]+)"/g))
    if (!refs.has(m[1]) && !parsed.some(q => q.ref === m[1])) (strict ? errors : pending).push(`${p.f} : lien vers ${m[1]} introuvable`);
}
// Routes du prototype cliquable (réf. → fichier), lues par kit.js.
const routes = Object.fromEntries(parsed.filter(p => p.ref).map(p => [p.ref, p.f]));
fs.writeFileSync(path.join(dir, "routes.js"), "window.HD_ROUTES = " + JSON.stringify(routes, null, 1) + ";\n");
if (errors.length) { console.log(errors.join("\n")); process.exit(1); }
if (pending.length) console.log(`${new Set(pending.map(x => x.split("vers ")[1])).size} lien(s) vers des pages à venir`);
console.log(`${parsed.length} page(s) OK`);
