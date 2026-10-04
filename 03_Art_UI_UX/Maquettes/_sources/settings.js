// Colonne des catégories des Paramètres, injectée dans <div id="settings-nav" data-cat="audio">.
(function () {
  const el = document.getElementById("settings-nav");
  if (!el) return;
  const cats = [
    ["audio", "🔊", "AUDIO", "M80"], ["graphismes", "🖥", "GRAPHISMES", "M81"], ["accessibilite", "♿", "ACCESSIBILITÉ", "M82"],
    ["langue", "🌐", "LANGUE", "M02"], ["notifications", "🔔", "NOTIFICATIONS", "M83"], ["controles", "⌨", "CONTRÔLES PC", "M84"],
    ["compte", "👤", "COMPTE & DONNÉES", "M85"], ["support", "?", "SUPPORT & AIDE", "M85"],
  ];
  el.innerHTML = cats.map(([id, icon, label, ref]) =>
    `<div class="item${el.dataset.cat === id ? " on" : ""}" data-go="${ref}" style="height:92px;gap:18px;${el.dataset.cat === id ? "background:var(--yellow);color:var(--ink);" : ""}">
       <span style="font-size:34px;width:44px;text-align:center">${icon}</span><span class="t" style="font-size:30px;flex:1">${label}</span>${id === "controles" ? '<span class="tag" style="background:var(--blue)">PC</span>' : ""}
     </div>`).join("");
  el.classList.add("list");
  el.style.gap = "10px";
})();
