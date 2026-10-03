// Génère les 6 pages de Paramètres (M80–M85) : colonne de catégories commune + contenu propre à chaque vue.
// Usage : node gen-settings.js (réécrit pages/M80…M85).
const fs = require("fs"), path = require("path");
const out = (f) => path.join(__dirname, "pages", f);

const head = (ref, cat) => `<!doctype html><html lang="fr"><head><meta charset="utf-8"><link rel="stylesheet" href="../kit.css"><script src="../kit.js" defer></script><script src="../settings.js" defer></script></head>
<body data-ref="${ref}" data-title="Paramètres" data-group="Transverse" data-mode="hub" data-tab="none" data-back="M10" data-nores>
<main>
  <div class="bg heavy" style="--img:url(../../Identite/HD_Illustration_Landscape_4K.png)"></div>
  <div id="settings-nav" data-cat="${cat}" style="left:88px;top:140px;width:450px"></div>
`;
const panel = (title, rows, h = 820) => `  <div class="panel" style="left:580px;top:140px;width:1252px;height:${h}px">
    <span class="caption">${title}</span>
    <div class="abs" style="left:16px;right:16px;top:36px">${rows}</div>
    <div class="abs btn secondary small" style="right:24px;bottom:22px;width:300px;height:72px">↺ RÉINITIALISER</div>
  </div>
`;
const row = (lbl, sub, ctrl) => `<div class="setrow"><div class="lbl">${lbl}${sub ? `<small>${sub}</small>` : ""}</div>${ctrl}</div>`;
const slider = (v, mute = true) => `<div class="rowf" style="gap:26px"><div class="slider" style="width:460px;--v:${v}%"></div><span class="t" style="font-size:32px;width:70px;text-align:right">${v}</span>${mute ? '<div class="icon-btn" style="width:72px;height:72px;font-size:30px">🔊</div>' : ""}</div>`;
const toggle = (on) => `<div class="toggle${on ? " on" : ""}"></div>`;
const seg = (opts, on, w = 560) => `<div class="seg" style="width:${w}px">${opts.map((o, i) => `<span${i === on ? ' class="on"' : ""}>${o}</span>`).join("")}</div>`;
const key = (k, w = 56, hl = false) => `<span class="key" style="font-size:24px;height:56px;min-width:${w}px${hl ? ";background:var(--yellow);color:var(--ink)" : ""}">${k}</span>`;
const pins = (list) => list.map(([n, x, y]) => `  <i class="pin" data-n="${n}" style="left:${x}px;top:${y}px"></i>`).join("\n");
const page = (file, ref, cat, body, pinList, legend) => fs.writeFileSync(out(file), `${head(ref, cat)}${body}${pins(pinList)}
</main>
<ol class="legend">
${legend.map((l) => `<li>${l}</li>`).join("\n")}
</ol>
</body></html>
`);

page("M80-parametres-audio.html", "M80", "audio",
  panel("AUDIO",
    row("Volume général", "", slider(80)) + row("Musique", "Thèmes du QG, du plateau et du combat", slider(70)) +
    row("Effets sonores", "Interface, dés, coups, pouvoirs", slider(90)) + row("Voix", "Répliques de combat (en anglais)", slider(100)) +
    row("Couper le son en arrière-plan", "Quand le jeu n'est plus au premier plan", toggle(true))),
  [[1, 60, 300], [2, 600, 250], [3, 1190, 268], [4, 1810, 268], [5, 600, 620], [6, 1530, 900]],
  ["Catégories toujours visibles à gauche, la catégorie ouverte en jaune ; Retour revient à l'écran d'où l'on vient. Pas de barre de navigation : l'écran est plein.",
   "Un réglage par ligne : libellé en clair et, en dessous, ce qu'il concerne.",
   "Curseur 0 à 100 avec la valeur écrite ; le changement s'entend tout de suite (aperçu sonore), sans bouton Enregistrer.",
   "Bouton muet par canal, sans perdre le niveau réglé.",
   "Interrupteurs : vert = activé, toujours avec la position du bouton (pas seulement la couleur).",
   "Réinitialiser remet cette catégorie seulement à ses valeurs par défaut."]);

page("M81-parametres-graphismes.html", "M81", "graphismes",
  panel("GRAPHISMES & PERFORMANCE",
    row("Qualité", "Auto : choisie selon l'appareil", seg(["AUTO", "BAS", "MOYEN", "HAUT"], 0)) +
    row("Images par seconde", "60 i/s : plus fluide, consomme plus", seg(["30", "60"], 1, 300)) +
    row("Économie de batterie", "30 i/s et effets réduits sous 20 % de batterie", toggle(false)) +
    row("Effets de combat", "Particules, éclats et secousses", seg(["COMPLETS", "RÉDUITS"], 0, 420)) +
    `<div class="t" style="font-size:26px;color:var(--blue);padding:14px 26px 0">PC UNIQUEMENT</div>` +
    row("Résolution · Plein écran · VSync", "", `<div class="rowf"><div class="chip dark" style="font-size:22px;padding:8px 18px">1920 × 1080 ▾</div>${toggle(true)}${toggle(true)}</div>`)),
  [[1, 600, 230], [2, 600, 330], [3, 600, 430], [4, 600, 690], [5, 1530, 900]],
  ["Qualité en 4 niveaux, Auto par défaut (le jeu choisit selon l'appareil).",
   "30 ou 60 images par seconde, avec la conséquence écrite (fluidité contre batterie).",
   "Économie de batterie : bascule automatique sous 20 % de batterie (mobile).",
   "Réglages réservés au PC regroupés sous un intertitre ; changer la résolution demande une confirmation avec retour automatique après 10 s.",
   "Réinitialiser la catégorie."]);

page("M82-parametres-accessibilite.html", "M82", "accessibilite",
  panel("ACCESSIBILITÉ",
    row("Taille du texte", "Aperçu : <span style='font-size:24px;color:var(--white)'>Givre est prête !</span>", seg(["NORMAL", "GRAND", "TRÈS GRAND"], 1, 520)) +
    row("Mode daltonien", "Les icônes de famille <span style='color:var(--matiere)'>⬢</span> <span style='color:var(--elementaire)'>≋</span> <span style='color:var(--physique)'>✚</span> <span style='color:var(--perception)'>◉</span> restent affichées", seg(["NON", "PROTAN.", "DEUTÉR.", "TRITAN."], 0, 560)) +
    row("Réduire les flashs", "Éclats de combat et écrans blancs atténués", toggle(true)) +
    row("Réduire les secousses", "Caméra et cibles touchées", toggle(false)) +
    row("Temps étendu des mini-jeux", "+50 % de temps, zones élargies", toggle(false)) +
    row("Sous-titres", "Répliques doublées et sons importants", toggle(true))),
  [[1, 600, 230], [2, 600, 330], [3, 600, 470], [4, 600, 615], [5, 600, 710]],
  ["Taille du texte en 3 niveaux, avec un aperçu immédiat dans la ligne.",
   "Mode daltonien (protanopie, deutéranopie, tritanopie) : palette des familles adaptée ; leurs icônes restent toujours affichées, la couleur n'est jamais seule.",
   "Réduction des flashs et des secousses, séparées (certains joueurs ont besoin de l'une sans l'autre).",
   "Temps étendu des mini-jeux : plus de temps et des zones de réussite élargies, sans pénalité.",
   "Sous-titres des répliques doublées et des sons importants. Ces réglages suivent le compte d'un appareil à l'autre."]);

page("M83-parametres-notifications.html", "M83", "notifications",
  panel("NOTIFICATIONS",
    row("Notifications", "Autorisées par le système ✔", toggle(true)) +
    `<div style="display:grid;grid-template-columns:1fr 1fr;column-gap:30px">` +
    row("Entraînement terminé", "", toggle(true)) + row("Expédition revenue", "", toggle(true)) +
    row("Labo plein", "", toggle(true)) + row("Membre rétabli", "", toggle(false)) +
    row("Nouvelle pool", "", toggle(true)) + row("Objectifs secondaires", "", toggle(false)) +
    row("Fin de saison du pass", "", toggle(true)) + `<div></div></div>` +
    row("Heures calmes", "Aucune notification pendant ce créneau", `<div class="rowf"><span class="chip dark" style="font-size:24px;padding:8px 18px">22:00</span>→<span class="chip dark" style="font-size:24px;padding:8px 18px">08:00</span>${toggle(true)}</div>`)),
  [[1, 600, 230], [2, 600, 420], [3, 600, 750]],
  ["Interrupteur général, avec l'état de l'autorisation du système (sinon, bouton « Autoriser dans les réglages du téléphone »).",
   "Un interrupteur par type de notification : le joueur garde celles qui l'aident à revenir au bon moment.",
   "Heures calmes réglables (22 h à 8 h par défaut)."]);

page("M84-parametres-controles.html", "M84", "controles",
  panel("CONTRÔLES PC",
    `<div style="display:grid;grid-template-columns:1fr 1fr;column-gap:30px">` +
    row("Lancer le dé", "", key("ESPACE", 150)) +
    row("Compétences", "", `<div class="rowf">${key("Q")}${key("W")}${key("E")}</div>`) +
    row("Portraits", "", key("1 – 4", 150)) +
    row("Combo", "", key("APPUIE…", 150, true)) +
    row("Auto · Vitesse", "", `<div class="rowf">${key("A")}${key("S")}</div>`) +
    row("Pause · Retour", "", `<div class="rowf">${key("P")}${key("ÉCHAP", 90)}</div>`) +
    `</div>` +
    row("Afficher les touches sur les boutons", "", toggle(true)) +
    row("Caméra du plateau", "Sensibilité du glisser", slider(60, false)) +
    row("Inverser le zoom à la molette", "", toggle(false))),
  [[1, 600, 230], [2, 1230, 330], [3, 600, 520], [4, 600, 620]],
  ["Raccourcis par action, regroupés par usage ; toucher une touche pour la changer (catégorie visible seulement sur PC).",
   "Mode d'écoute : « APPUIE… » en jaune jusqu'à la nouvelle touche ; un conflit avec une autre action est signalé et proposé en échange.",
   "Afficher les touches entre crochets sur les boutons du jeu.",
   "Caméra du plateau : sensibilité du glisser et sens du zoom à la molette."]);

page("M85-parametres-compte-support.html", "M85", "compte",
  `  <div class="panel" style="left:580px;top:140px;width:610px;height:820px">
    <span class="caption">COMPTE & DONNÉES</span>
    <div class="abs list" style="left:20px;right:20px;top:46px;gap:14px">
      <div class="item"><span class="t green" style="font-size:30px">✔</span><div class="grow">Google Play<br><span class="tiny muted">Lié · progression sauvegardée</span></div></div>
      <div class="btn secondary small" style="height:72px">LIER APPLE</div>
      <div class="btn secondary small" style="height:72px">LIER STEAM</div>
    </div>
    <div class="abs" style="left:8px;right:8px;top:330px">${row("Statistiques d'usage", "Anonymes", toggle(true))}</div>
    <div class="abs btn secondary small" style="left:20px;top:460px;width:570px;height:72px">EXPORTER MES DONNÉES</div>
    <div class="abs n tiny" style="left:20px;top:560px;color:var(--blue)">Conditions · Confidentialité</div>
    <div class="abs btn danger small" style="left:20px;top:620px;width:570px;height:72px">SUPPRIMER MON COMPTE</div>
  </div>
  <div class="panel" style="left:1222px;top:140px;width:610px;height:820px">
    <span class="caption white">SUPPORT & AIDE</span>
    <div class="abs btn primary" style="left:20px;top:50px;width:570px;height:96px;font-size:34px">▶ REVOIR LE TUTORIEL</div>
    <div class="abs list" style="left:20px;right:20px;top:180px;gap:14px">
      <div class="btn secondary small" style="height:72px">FICHES D'AIDE · 9 VUES</div>
      <div class="btn secondary small" style="height:72px">CONTACTER LE SUPPORT</div>
      <div class="item"><div class="grow small">ID joueur<br><span class="t" style="font-size:28px">HD-7F3K-92QX</span></div><span class="chip">COPIER</span></div>
      <div class="item small muted">Version 0.1.0 (M1) · build 214</div>
      <div class="btn secondary small" style="height:72px">CRÉDITS</div>
    </div>
  </div>
  <div class="scrim" style="background:rgba(5,6,9,.45)"></div>
  <div class="panel modal" style="left:600px;top:300px;width:580px;height:600px;border-color:var(--red)">
    <span class="caption red" style="font-size:34px">SUPPRIMER TON COMPTE ?</span>
    <div class="abs n" style="left:28px;top:56px;width:524px;font-size:22px;line-height:1.45">Toute ta progression, tes recrues et tes achats seront effacés <b class="red">définitivement</b>, sur tous tes appareils. Les achats ne sont pas remboursés.</div>
    <div class="abs n small" style="left:28px;top:250px">Écris <b class="yellow">SUPPRIMER</b> pour confirmer :</div>
    <div class="abs" style="left:28px;top:290px;width:524px;height:72px;border:4px solid #000;border-radius:12px;background:#0B0D12;font-family:Bangers;font-size:34px;padding:12px 20px;color:var(--muted)">SUPPR|</div>
    <div class="abs btn primary" style="left:28px;top:400px;width:524px;height:80px;font-size:32px">GARDER MON COMPTE</div>
    <div class="abs btn danger off" style="left:28px;top:500px;width:524px;height:70px;font-size:28px">SUPPRIMER</div>
  </div>
`,
  [[1, 560, 230], [2, 1200, 250], [3, 1200, 560], [4, 580, 920], [5, 580, 440]],
  ["Compte : services liés, liaison d'un autre service, consentement aux statistiques, export des données (RGPD).",
   "Support & aide : « Revoir le tutoriel » en premier, fiches d'aide déjà vues (glossaire), contact du support.",
   "ID joueur copiable, version et numéro de build : ce que le support demande en premier ; crédits (dont musiques et sons Kenney CC0).",
   "Suppression du compte : double confirmation, il faut écrire SUPPRIMER ; tant que ce n'est pas fait, le bouton rouge reste désactivé.",
   "Conséquences écrites sans détour ; l'action proposée par défaut (jaune) est de garder son compte."]);

console.log("M80–M85 générées");
