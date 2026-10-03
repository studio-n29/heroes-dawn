// Carte de New York (5 arrondissements = 5 actes) injectée dans <div id="citymap">.
(function () {
  const el = document.getElementById("citymap");
  if (!el) return;
  const lock = (d, x, y, label, rot) => `<path d="${d}" fill="url(#h)" stroke="#000" stroke-width="6"/><text x="${x}" y="${y}" text-anchor="middle" font-family="Bangers" font-size="32" fill="#7B8291"${rot ? ` transform="rotate(${rot} ${x} ${y})"` : ""}>${label}</text>`;
  const zone = (x, y, r, fill, ink, label, top) => `<circle cx="${x + 5}" cy="${y + 5}" r="${r}" fill="#000"/><circle cx="${x}" cy="${y}" r="${r}" fill="${fill}" stroke="#000" stroke-width="6"/><text x="${x}" y="${y + 11}" text-anchor="middle" font-family="Bangers" font-size="30" fill="${ink}">${label}</text>${top ? `<text x="${x}" y="${y - r - 10}" text-anchor="middle" font-family="Bangers" font-size="24" fill="#fff">${top}</text>` : ""}`;
  el.innerHTML = `<svg width="100%" height="100%" viewBox="0 0 1150 840">
    <defs><pattern id="h" width="12" height="12" patternUnits="userSpaceOnUse" patternTransform="rotate(45)"><rect width="12" height="12" fill="#14171F"/><line x1="0" y1="0" x2="0" y2="12" stroke="#262B36" stroke-width="5"/></pattern></defs>
    <rect width="1150" height="840" rx="18" fill="#0C1A26" stroke="#000" stroke-width="6"/>
    ${lock("M520 40 L700 30 L760 160 L620 220 L540 170 Z", 640, 120, "BRONX · ACTE 3 🔒")}
    ${lock("M470 180 L540 170 L600 230 L520 520 L450 500 Z", 520, 360, "MANHATTAN · 5 🔒", -72)}
    ${lock("M620 220 L1080 160 L1100 470 L760 520 L600 430 Z", 860, 350, "QUEENS · ACTE 2 🔒")}
    ${lock("M90 600 L300 560 L330 760 L120 800 Z", 210, 690, "STATEN ISLAND · 4 🔒")}
    <path d="M520 520 L600 430 L760 520 L860 600 L760 790 L470 780 L430 620 Z" fill="#2A2416" stroke="#FFD23F" stroke-width="8"/>
    <text x="650" y="752" text-anchor="middle" font-family="Bangers" font-size="40" fill="#FFD23F">BROOKLYN · ACTE 1</text>
    ${zone(540, 620, 40, "#3A4152", "#A7AFBD", "1.1", "")}
    ${zone(660, 545, 48, "#FFD23F", "#0B0D12", "1.2", "")}
    ${zone(720, 670, 40, "#E5484D", "#fff", "1.3", "")}
    ${zone(500, 585, 18, "#E5484D", "#fff", "!", "")}
  </svg>`;
})();
