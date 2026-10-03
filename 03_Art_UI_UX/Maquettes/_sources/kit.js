// Heroes Dawn — chrome commun des maquettes : barre haute, navigation basse, pause, liens du prototype.
// Attributs du <body> : data-ref, data-title, data-mode (hub | mission | plain), data-tab, data-back (réf. de retour),
// data-badges ("recruter:N,pass:2"), data-res ("12 450|185|320"), data-nores (pas de ressources).
(function () {
  const ICONS = {
    qg: "M4 22V11l10-8 10 8v11h-7v-7h-6v7z",
    equipe: "M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8zm10 0a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7zM1 23c0-4.5 3.6-8 8-8s8 3.5 8 8zm17 0c0-2.6-.9-4.9-2.4-6.6.9-.3 1.6-.4 2.4-.4 3.9 0 7 3.1 7 7z",
    recruter: "M14 1l3.6 7.6 8.4 1-6.1 5.8 1.6 8.3L14 19.6l-7.5 4.1 1.6-8.3L2 9.6l8.4-1z",
    pass: "M2 7a2 2 0 0 1 2-2h20a2 2 0 0 1 2 2v4a3 3 0 0 0 0 6v4a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2v-4a3 3 0 0 0 0-6zm9 0v14h2V7z",
    boutique: "M6 8h16l-1.5 15h-13zm3 0a5 5 0 0 1 10 0h-2a3 3 0 0 0-6 0z",
    gear: "M14 9a5 5 0 1 0 0 10 5 5 0 0 0 0-10zm11 7.3l-2.6.5a8.6 8.6 0 0 1-.9 2.1l1.5 2.2-2.6 2.6-2.2-1.5a8.6 8.6 0 0 1-2.1.9L15.6 26h-3.2l-.5-2.6a8.6 8.6 0 0 1-2.1-.9l-2.2 1.5L5 21.4l1.5-2.2a8.6 8.6 0 0 1-.9-2.1L3 16.6v-3.2l2.6-.5a8.6 8.6 0 0 1 .9-2.1L5 8.6 7.6 6l2.2 1.5a8.6 8.6 0 0 1 2.1-.9L12.4 4h3.2l.5 2.6a8.6 8.6 0 0 1 2.1.9L20.4 6 23 8.6l-1.5 2.2c.4.7.7 1.4.9 2.1l2.6.5z",
    pause: "M7 4h5v20H7zm9 0h5v20h-5z",
    back: "M18 3L6 14l12 11V18h8v-8h-8z",
  };
  const svg = (name) => `<svg viewBox="0 0 28 28"><path d="${ICONS[name]}"/></svg>`;
  window.HD_ICON = svg;

  const b = document.body, d = b.dataset;
  const params = new URLSearchParams(location.search);
  if (params.has("export")) b.classList.add("export");
  const main = document.querySelector("main") || b;
  const add = (html) => main.insertAdjacentHTML("beforeend", html);

  function resources() {
    if ("nores" in d) return "";
    const [cr, pr, gm] = (d.res || "12 450|185|320").split("|");
    return `<div class="pill" style="left:1100px" data-go="M50"><i style="background:var(--yellow)">CR</i>${cr}</div>
      <div class="pill" style="left:1330px;min-width:170px" data-go="M50"><i style="background:var(--elementaire)">PR</i>${pr}</div>
      <div class="pill" style="left:1520px;min-width:170px" data-go="M50"><i style="background:var(--perception)">GM</i>${gm}<span class="plus">+</span></div>`;
  }

  if (d.mode === "hub" || d.mode === "mission") {
    const back = d.back ? `<div class="btn ghost small" data-go="${d.back}" style="left:88px;top:18px;width:170px;height:68px">${svg("back").replace("<svg", "<svg style='width:30px;height:30px;fill:#fff'")} RETOUR</div>` : "";
    const titleX = d.titlex ? +d.titlex : d.back ? 290 : 88;
    const right = d.mode === "hub"
      ? resources() +
        ("nores" in d ? "" : `<div class="icon-btn" data-go="M80" style="left:1736px;top:4px;width:92px;height:92px">${svg("gear")}</div>`)
      : `<div class="icon-btn" data-go="M72" style="left:1736px;top:4px;width:92px;height:92px">${svg("pause")}</div>`;
    add(`<div class="topbar">${back}<div class="title" style="left:${titleX}px">${d.title}</div>${right}</div>`);
  }

  if (d.mode === "hub" && d.tab !== "none") {
    const badges = Object.fromEntries((d.badges || "").split(",").filter(Boolean).map(s => s.split(":")));
    const tabs = [["qg", "QG", "M10"], ["equipe", "ÉQUIPE", "M20"], ["recruter", "RECRUTER", "M30"], ["pass", "PASS", "M40"], ["boutique", "BOUTIQUE", "M50"]];
    add(`<nav class="navbar">${tabs.map(([id, label, ref]) =>
      `<div class="tab${d.tab === id ? " on" : ""}" data-go="${ref}">${svg(id)}${label}${badges[id] ? `<span class="badge">${badges[id]}</span>` : ""}</div>`).join("")}</nav>`);
  }

  b.insertAdjacentHTML("beforeend", `<div class="reftag"><b>HEROES DAWN</b> · ${d.ref} · ${d.title}</div>`);

  // Prototype cliquable : data-go="M20" → page de la référence (routes.js généré par check.js).
  if (!b.classList.contains("export")) {
    document.addEventListener("click", (e) => {
      const el = e.target.closest("[data-go]");
      if (!el || !window.HD_ROUTES) return;
      const file = window.HD_ROUTES[el.dataset.go];
      if (file) location.href = file;
    });
    const s = document.createElement("script");
    s.src = "routes.js";
    document.head.appendChild(s);
  }
})();
