// Génère les maquettes du tutoriel (T01–T12) : l'écran réel en dessous (iframe, sans pastilles), puis le calque
// didactique : voile + projecteur sur l'élément à toucher, main, bulle de Rayner (étape, 2 lignes, progression, Passer).
// Usage : node gen-tutorial.js (réécrit pages/T01…T12).
const fs = require("fs"), path = require("path");
const out = (f) => path.join(__dirname, "pages", f);

const under = (file) => `<iframe class="under" src="${file}?export=1&embed=1"></iframe>`;
const spot = ([x, y, w, h], round = false) => `<div class="spotlight${round ? " round" : ""}" style="left:${x}px;top:${y}px;width:${w}px;height:${h}px"></div>`;
const hand = (x, y, rot = 0) => `<div class="hand center" style="left:${x}px;top:${y}px;font-size:96px;transform:rotate(${rot}deg)">👆</div>`;
const guide = (x, y) => `<div class="guide center t" style="left:${x}px;top:${y}px;--img:none;background:radial-gradient(circle at 35% 30%,#FFE58A,#C9962B);color:#1B1408;font-size:64px">BR</div>`;
const bubble = ([x, y, w], step, title, text, action = "") => `<div class="bubble tail-l" style="left:${x}px;top:${y}px;width:${w}px">
    <span class="caption">${title}</span>${text}
    ${action}<div class="foot"><span>BRIE RAYNER · <span class="progress">${step}</span></span><span>PASSER LE TUTORIEL ▸</span></div></div>`;
const btn = (label) => `<div class="btn primary small" style="margin-top:18px;height:72px;width:280px">${label}</div>`;
const veil = `<div class="veil"></div>`;
const pins = (list) => list.map(([n, x, y]) => `  <i class="pin" data-n="${n}" style="left:${x}px;top:${y}px"></i>`).join("\n");

const page = (file, ref, title, layers, pinList, legend) => fs.writeFileSync(out(file), `<!doctype html><html lang="fr"><head><meta charset="utf-8"><link rel="stylesheet" href="../kit.css"><script src="../kit.js" defer></script></head>
<body data-ref="${ref}" data-title="${title}" data-group="Tutoriel" data-out="tutoriel" data-mode="plain">
<main>
  ${layers.join("\n  ")}
${pins(pinList)}
</main>
<ol class="legend">
${legend.map((l) => `<li>${l}</li>`).join("\n")}
</ol>
</body></html>
`);

// Guide à gauche de la bulle : la queue de la bulle pointe vers Rayner.
const say = (gx, gy, w, step, title, text, action) => [guide(gx, gy), bubble([gx + 180, gy, Math.max(w, 1000)], step, title, text, action)];

page("T01-bienvenue.html", "T01", "Tutoriel 1/9 · Bienvenue", [
  under("M10-qg.html"), veil,
  ...say(470, 300, 820, "1 / 9", "BIENVENUE À RED HOOK",
    "Cet entrepôt est maintenant ta section de la Guilde, Directeur. Je vais te montrer l'essentiel, en jouant.", btn("CONTINUER")),
], [[1, 450, 300], [2, 650, 290], [3, 665, 560], [4, 1625, 560]], [
  "Pas de projecteur à la première étape : tout l'écran est voilé, on présente le guide. Rayner (directrice de la Guilde) parle en médaillon, dans une bulle de BD.",
  "Récitatif jaune = titre de l'étape ; 2 lignes de texte au maximum, en Comic Neue.",
  "Progression discrète (« 1 / 9 ») : le joueur sait que c'est court.",
  "« Passer le tutoriel » toujours présent mais discret ; il demande une confirmation (T12).",
]);

page("T02-escouade.html", "T02", "Tutoriel 2/9 · Escouade", [
  under("M20-effectif.html"), spot([318, 578, 222, 342]), hand(440, 840, -15),
  ...say(620, 600, 720, "2 / 9", "COMPOSE TON ESCOUADE",
    "Une mission se joue à 4 héros. Il te manque quelqu'un : touche <b>Instant</b> pour l'ajouter."),
], [[1, 300, 560], [2, 470, 905], [3, 780, 700]], [
  "Projecteur : seul l'élément à toucher est découpé dans le voile et cerclé de jaune pulsé ; tout le reste est bloqué tant que l'étape n'est pas faite.",
  "Main animée qui tapote l'élément.",
  "Une seule notion (l'escouade de 4), au moment où elle sert ; le geste demandé est écrit en gras.",
]);

page("T03-mission.html", "T03", "Tutoriel 3/9 · Mission", [
  under("M10-qg.html"), spot([1482, 782, 358, 166]), hand(1560, 900, -10),
  ...say(560, 470, 720, "3 / 9", "EN MISSION !",
    "Les pillards tiennent les quais de Red Hook. Touche <b>MISSION</b>, puis Déployer."),
], [[1, 1460, 780], [2, 540, 470]], [
  "Le bouton le plus important de l'écran est celui qu'on apprend en premier.",
  "La bulle se place loin de l'élément désigné pour ne jamais le cacher.",
]);

page("T04-objectifs.html", "T04", "Tutoriel 4/9 · Objectifs", [
  under("M63-plateau-lancer.html"), spot([80, 128, 346, 352]),
  ...say(560, 200, 760, "4 / 9", "TES OBJECTIFS",
    "Chaque zone a 3 objectifs. Remplis-les pour sécuriser le quartier. Le premier est déjà en vue.", btn("COMPRIS")),
], [[1, 60, 300], [2, 760, 400]], [
  "Étape d'explication : le projecteur montre le panneau, et c'est « Compris » qui fait avancer (pas de geste à deviner).",
  "Le texte annonce aussi la suite (« le premier est déjà en vue ») pour enchaîner sur le lancer.",
]);

page("T05-lancer.html", "T05", "Tutoriel 5/9 · Lancer le dé", [
  under("M63-plateau-lancer.html"), spot([1472, 782, 368, 186]), hand(1580, 920, -10),
  ...say(560, 470, 720, "5 / 9", "LANCE LE DÉ",
    "Le dé te donne des points de mouvement (PM). Touche <b>LANCER !</b>"),
], [[1, 1450, 800], [2, 540, 470]], [
  "Geste réel sur la vraie interface : le joueur lance vraiment son premier dé de la zone 1.1.",
  "Le vocabulaire (PM) est introduit au moment où il apparaît à l'écran.",
]);

page("T06-destination.html", "T06", "Tutoriel 6/9 · Destination", [
  under("M64-plateau-destination.html"), spot([948, 362, 130, 130], true), hand(1000, 470, -10),
  ...say(220, 600, 720, "6 / 9", "CHOISIS OÙ ALLER",
    "Les cases cerclées sont à ta portée ; le chiffre = le coût. Touche la case <b>MJ</b>. Le brouillard recule à chaque pas."),
], [[1, 930, 350], [2, 200, 600]], [
  "Projecteur rond sur une destination désignée : pour la première fois, le choix est guidé ; ensuite le joueur choisit librement.",
  "Deux notions liées en une bulle (portée et brouillard) parce qu'elles se voient au même moment.",
]);

page("T07-combat-atb.html", "T07", "Tutoriel 7/9 · Temps actif", [
  under("M65-combat-pret.html"), spot([630, 628, 300, 324]),
  ...say(600, 300, 720, "7 / 9", "LE COMBAT EN TEMPS ACTIF",
    "Les jauges jaunes se remplissent toutes seules. Quand celle d'un héros est pleine, il agit. Le combat est figé pendant que je parle.", btn("COMPRIS")),
], [[1, 610, 640], [2, 800, 470]], [
  "Le combat (temps actif) est mis en pause tant que la bulle est ouverte : le joueur lit sans stress.",
  "« Compris » relance le combat ; Givre est déjà prête, l'étape suivante enchaîne aussitôt.",
]);

page("T08-combat-competence.html", "T08", "Tutoriel 8/9 · Compétence et cible", [
  under("M66-combat-ciblage.html"), spot([922, 112, 316, 366]), hand(1060, 380, -10),
  ...say(240, 560, 640, "8 / 9", "VISE !",
    "Tu as choisi Éclat de givre. Maintenant, touche le <b>chef du gang</b> pour l'attaquer."),
], [[1, 900, 130], [2, 220, 560]], [
  "Compétence puis cible : deux gestes, deux étapes visibles (la compétence choisie reste encadrée dans le panneau de gauche).",
  "Après ce coup, le tutoriel se tait jusqu'à la fin du combat : le joueur finit seul (AUTO et combos seront expliqués par des astuces).",
]);

page("T09-resultats.html", "T09", "Tutoriel 9/9 · Fatigue", [
  under("M70-resultats-combat.html"), spot([298, 488, 1334, 156]),
  ...say(420, 700, 820, "9 / 9", "VICTOIRE… ET FATIGUE",
    "Chaque combat fatigue l'escouade. Au-delà de 60 %, un héros est moins efficace : pense à l'infirmerie du QG.", btn("CONTINUER")),
], [[1, 280, 500], [2, 400, 760]], [
  "La fatigue, règle clé de la gestion d'équipe, est expliquée sur les jauges réelles du combat qui vient d'avoir lieu.",
  "Le conseil donne déjà le réflexe suivant (l'infirmerie), sans l'imposer.",
]);

page("T10-astuce-combo.html", "T10", "Astuce contextuelle · Combo", [
  under("M66-combat-ciblage.html"),
  `<div class="tip-card" style="left:1180px;top:650px;width:640px">
    <span class="caption" style="left:20px;top:-24px">ASTUCE · COMBO</span>
    <div class="n" style="font-size:24px;line-height:1.4;margin-top:6px">Deux héros prêts dont les familles s'accordent peuvent unir leurs forces. Touche <b class="red">COMBO !</b> : les deux jouent ensemble.</div>
    <div class="rowf" style="justify-content:space-between;margin-top:16px"><span class="tiny muted">Retrouve cette fiche dans Pause ▸ Aide</span><div class="btn secondary small" style="width:180px;height:66px">OK</div></div>
  </div>`,
  `<div class="abs" style="left:1572px;top:522px;width:268px;height:112px;border:6px solid var(--yellow);border-radius:18px;z-index:24"></div>`,
], [[1, 1160, 680], [2, 1560, 510], [3, 1700, 900]], [
  "Astuce contextuelle : carte non bloquante qui apparaît la première fois qu'une mécanique se présente (ici, le premier combo disponible). Le combat continue derrière.",
  "L'élément concerné est seulement souligné (cadre jaune), sans voile.",
  "Fermeture d'un geste ; la fiche reste consultable dans Pause ▸ Aide. Autres astuces : AUTO et vitesse, dé bonus, Terminer, cases Événement / Mini-jeu / Repos, défaite, fatigue > 60 %, zone sécurisée, recrutement, infirmerie.",
]);

page("T11-fin.html", "T11", "Tutoriel terminé", [
  under("M10-qg.html"), veil,
  `<div class="burst center" style="left:360px;top:160px;width:1200px;height:170px;font-size:84px;z-index:34">TUTORIEL TERMINÉ !</div>`,
  ...say(470, 420, 820, "✔", "À TOI DE JOUER",
    "Tu connais l'essentiel. Le reste, je te le dirai au bon moment. Prochaine étape : Gowanus, le canal.", btn("C'EST PARTI")),
], [[1, 340, 230], [2, 650, 420], [3, 1300, 700]], [
  "Fin célébrée en éclat comics : le joueur sait que la phase guidée est finie.",
  "Rayner annonce la suite (astuces au bon moment) et le prochain objectif de l'histoire.",
  "Aucune récompense supplémentaire : l'économie reste côté serveur ; le joueur arrive sur le QG libre de ses choix.",
]);

page("T12-passer.html", "T12", "Passer le tutoriel ?", [
  under("M63-plateau-lancer.html"), `<div class="scrim" style="z-index:30"></div>`,
  `<div class="panel modal" style="left:560px;top:260px;width:800px;height:520px;z-index:31">
    <span class="caption" style="font-size:38px">PASSER LE TUTORIEL ?</span>
    <div class="abs n" style="left:36px;top:70px;width:728px;font-size:25px;line-height:1.45">Tu pourras le revoir à tout moment dans <b class="yellow">Paramètres ▸ Support &amp; aide</b>. Les astuces continueront d'apparaître la première fois que tu découvres une mécanique.</div>
    <div class="abs btn primary" style="left:36px;top:300px;width:728px;height:90px;font-size:36px">CONTINUER LE TUTORIEL</div>
    <div class="abs btn secondary" style="left:36px;top:410px;width:728px;height:76px;font-size:30px">PASSER</div>
  </div>`,
], [[1, 540, 330], [2, 540, 440], [3, 540, 550]], [
  "Passer demande une confirmation : un toucher accidentel ne fait pas perdre le guidage.",
  "Le joueur sait où le revoir et que les astuces contextuelles restent actives.",
  "L'action par défaut (jaune) est de continuer ; Retour = continuer. L'étape atteinte est enregistrée : quitter le jeu puis revenir reprend au même endroit.",
]);

console.log("T01–T12 générées");
