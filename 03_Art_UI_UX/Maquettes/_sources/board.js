// Plateau de la zone 1.2 (Gowanus) dessiné en SVG dans <div id="board" data-dest="n5:3,n6:4,n8:4" data-path="n4,n5" data-pawn="n4">.
// Types : qg, combat, event, mj, res, rest, fog. data-dest = destinations atteignables (nœud:coût), data-path = chemin prévisualisé.
(function () {
  const el = document.getElementById("board");
  if (!el) return;
  const N = {
    n1: [150, 600, "qg"], n2: [330, 520, "res"], n3: [500, 600, "combat"], n4: [640, 470, "event"],
    n5: [820, 380, "mj"], n6: [820, 600, "res"], n7: [1000, 300, "combat"], n8: [1010, 520, "rest"],
    n9: [1190, 420, "fog"], n10: [1190, 650, "fog"], n11: [1360, 330, "fog"], n12: [1370, 560, "fog"],
  };
  const L = [["n1","n2"],["n2","n3"],["n2","n4"],["n3","n6"],["n4","n5"],["n4","n6"],["n5","n7"],["n5","n8"],["n6","n8"],["n7","n9"],["n8","n9"],["n8","n10"],["n9","n11"],["n10","n12"],["n11","n12"]];
  const LOOK = {
    qg: ["#FFD23F", "QG", "#0B0D12"], combat: ["#E5484D", "VS", "#fff"], event: ["#FFD23F", "!", "#0B0D12"], mj: ["#3FD0E8", "MJ", "#0B0D12"],
    res: ["#4CD964", "$", "#0B0D12"], rest: ["#A77BFF", "Zz", "#fff"], fog: ["#2B3140", "?", "#6B7280"], done: ["#3A4152", "✔", "#A7AFBD"],
  };
  const d = el.dataset;
  const dest = Object.fromEntries((d.dest || "").split(",").filter(Boolean).map(s => s.split(":")));
  const path = (d.path || "").split(",").filter(Boolean);
  const spent = new Set((d.spent || "").split(",").filter(Boolean));
  let s = `<svg width="${el.offsetWidth}" height="${el.offsetHeight}" viewBox="0 0 1500 820" style="position:absolute;inset:0">`;
  s += `<defs><pattern id="hatch" width="14" height="14" patternUnits="userSpaceOnUse" patternTransform="rotate(45)"><rect width="14" height="14" fill="#0E1016"/><line x1="0" y1="0" x2="0" y2="14" stroke="#2A2F3B" stroke-width="5"/></pattern></defs>`;
  s += `<path d="M1080 0 C1150 260 1100 520 1240 820 L1500 820 L1500 0 Z" fill="url(#hatch)" opacity=".95"/>`; // brouillard
  for (const [a, b] of L) {
    const [x1, y1] = N[a], [x2, y2] = N[b];
    const fog = N[a][2] === "fog" && N[b][2] === "fog";
    s += `<line x1="${x1}" y1="${y1}" x2="${x2}" y2="${y2}" stroke="#000" stroke-width="16" opacity="${fog ? .35 : 1}"/><line x1="${x1}" y1="${y1}" x2="${x2}" y2="${y2}" stroke="${fog ? "#3A4152" : "#C9CED7"}" stroke-width="8"/>`;
  }
  for (let i = 0; i + 1 < path.length; i++) {
    const [x1, y1] = N[path[i]], [x2, y2] = N[path[i + 1]];
    s += `<line x1="${x1}" y1="${y1}" x2="${x2}" y2="${y2}" stroke="#FFD23F" stroke-width="9" stroke-dasharray="18 14"/>`;
  }
  for (const [id, [x, y, t]] of Object.entries(N)) {
    const [fill, glyph, ink] = LOOK[spent.has(id) ? "done" : t];
    if (dest[id]) s += `<circle cx="${x}" cy="${y}" r="70" fill="none" stroke="#FFD23F" stroke-width="8" stroke-dasharray="14 10"/>`;
    s += `<circle cx="${x + 6}" cy="${y + 6}" r="48" fill="#000"/><circle cx="${x}" cy="${y}" r="48" fill="${fill}" stroke="#000" stroke-width="6"/>`;
    s += `<text x="${x}" y="${y + 13}" text-anchor="middle" font-family="Bangers" font-size="38" fill="${ink}">${glyph}</text>`;
    if (dest[id]) s += `<circle cx="${x + 50}" cy="${y - 50}" r="26" fill="#FFD23F" stroke="#000" stroke-width="5"/><text x="${x + 50}" y="${y - 39}" text-anchor="middle" font-family="Bangers" font-size="30" fill="#0B0D12">${dest[id]}</text>`;
  }
  if (d.pawn) {
    const [x, y] = N[d.pawn];
    s += `<clipPath id="pc"><circle cx="${x - 40}" cy="${y - 70}" r="40"/></clipPath><circle cx="${x - 36}" cy="${y - 66}" r="44" fill="#000"/><image href="../img/p_blanc.jpg" x="${x - 80}" y="${y - 110}" width="80" height="80" clip-path="url(#pc)" preserveAspectRatio="xMidYMid slice"/><circle cx="${x - 40}" cy="${y - 70}" r="40" fill="none" stroke="#3FD0E8" stroke-width="7"/>`;
  }
  el.innerHTML = s + "</svg>";
})();
