// Régénère 03_Art_UI_UX/03_Maquettes_Screen_Flow.md depuis les pages (titre, groupe, légendes) — la doc ne diverge jamais des images.
// Usage : node build-doc.js
const fs = require("fs"), path = require("path");
const dir = path.join(__dirname, "pages");
const strip = (s) => s.replace(/<[^>]+>/g, "").replace(/&amp;/g, "&").replace(/&lt;/g, "<").replace(/&gt;/g, ">").trim();
const pages = fs.readdirSync(dir).filter((f) => f.endsWith(".html") && !f.startsWith("_")).sort().map((f) => {
  const html = fs.readFileSync(path.join(dir, f), "utf8");
  const body = /<body([^>]*)>/.exec(html)[1];
  const a = (n) => new RegExp(`data-${n}="([^"]*)"`).exec(body)?.[1] ?? "";
  const legend = [.../<ol class="legend">([\s\S]*?)<\/ol>/.exec(html)[1].matchAll(/<li>([\s\S]*?)<\/li>/g)].map((m) => strip(m[1]));
  return { f, ref: a("ref"), title: a("title"), group: a("group"), out: a("out") || "ecrans", legend };
});
const ORDER = ["Entrée", "Onglet QG", "Onglet Équipe", "Onglet Recruter", "Onglet Pass", "Onglet Boutique", "Mission", "Transverse", "Tutoriel", "Référence"];
const missing = pages.filter((p) => !ORDER.includes(p.group));
if (missing.length) throw new Error("Groupe inconnu : " + missing.map((p) => p.f).join(", "));
for (const p of pages) if (!fs.existsSync(path.join(__dirname, "..", p.out, p.f.replace(".html", ".png")))) throw new Error("Image manquante : " + p.f);

const section = (p) => `### ${p.ref} — ${p.title}

![${p.ref}](Maquettes/${p.out}/${p.f.replace(".html", ".png")})

${p.legend.map((l, i) => `${i + 1}. ${l}`).join("\n")}
`;

const md = `# Maquettes & screen flow

> Rendu cible du **jeu complet** en haute fidélité (DA comics, référence *Secret Wars* 2015), paysage 1920×1080, et du **tutoriel guidé** du premier lancement.
> Spec : \`Code/docs/specs/2026-10-04-maquettes-ui-tutoriel-design.md\`. Les pastilles jaunes numérotées renvoient aux légendes.
> Sources : \`Maquettes/_sources\` (kit HTML/CSS, une page par écran). Export : \`pwsh export.ps1\` (Edge headless) ; vérification : \`node check.js --strict\` ; cette page : \`node build-doc.js\` ; prototype cliquable : \`Maquettes/_sources/index.html\`.
> Portraits provisoires (extraits de l'illustration clé). Les anciennes maquettes du M1 (wireframes W, maquettes S) sont archivées dans \`Maquettes/_archive_M1\`.

## 1. Principes

- **Menus bien séparés** : un menu = un onglet de la barre de navigation basse (QG, Équipe, Recruter, Pass, Boutique), toujours au même endroit, avec ses badges. Les 5 pièces s'ouvrent depuis le QG.
- **Mode mission plein écran** : de la carte aux résultats, pas de barre de navigation ; retour en haut à gauche, ⏸ ouvre la pause (Reprendre, Paramètres, Aide, Retour au QG).
- **Placement constant** : retour en haut à gauche ; titre à côté ; ressources puis ⚙ en haut à droite ; action principale en bas à droite (zone du pouce).
- **Une action principale par écran** (jaune, ou rouge pour MISSION et les actions dangereuses) ; actions secondaires sombres ; prix et conséquences écrits sur les boutons.
- **Lisibilité** : Bangers pour titres, boutons et chiffres ; Comic Neue pour tout texte de plus de 4 mots ; 20 px minimum ; couleur jamais seule porteuse de sens (famille = icône + couleur, état = libellé).
- **Tactile d'abord** : cibles ≥ 96 px, 24 px d'écart, zones sûres de 88 px sur les côtés ; raccourcis PC affichés en pastilles quand l'option est active.
- **Retour** (bouton, geste Android, Échap) : pièce → QG, onglet → QG, QG → « Quitter le jeu ? », mission → étape précédente, combat → pause, modale → annuler.

## 2. Tutoriel : principes didactiques

- **Apprendre en faisant** sur la vraie zone 1.1 : chaque étape demande un geste réel ; une notion à la fois, au moment où elle sert ; 2 lignes de texte maximum.
- **Guide** : Brie Rayner en médaillon, bulle de BD, récitatif jaune pour le titre de l'étape, progression « n / 9 ».
- **Projecteur** sur l'élément à toucher (le reste est voilé et bloqué) + main animée ; étapes d'explication validées par « Compris ».
- Combat figé pendant une bulle ; étape enregistrée dans le profil (reprise après un départ) ; « Passer » avec confirmation ; « Revoir » dans Paramètres ▸ Support & aide.
- Ensuite, **astuces contextuelles** non bloquantes à la première rencontre de chaque mécanique (combo, AUTO et vitesse, dé bonus, Terminer, cases Événement / Mini-jeu / Repos, défaite, fatigue > 60 %, zone sécurisée, recrutement, infirmerie).

${ORDER.map((g, i) => `## ${i + 3}. ${g}

${pages.filter((p) => p.group === g).map(section).join("\n")}`).join("\n")}`;

fs.writeFileSync(path.join(__dirname, "..", "..", "03_Maquettes_Screen_Flow.md"), md);
console.log(`03_Maquettes_Screen_Flow.md : ${pages.length} maquettes`);
