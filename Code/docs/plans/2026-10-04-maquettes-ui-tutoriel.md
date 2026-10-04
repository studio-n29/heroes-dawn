# Maquettes UI du jeu complet + tutoriel — plan d'implémentation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produire les maquettes haute fidélité de tous les écrans du jeu final, des Paramètres et du tutoriel guidé, avec un prototype cliquable et la doc régénérée.

**Architecture:** Un kit HTML/CSS comics partagé (`kit.css` + `kit.js` qui injecte barre haute, navigation basse et pastilles de légende à partir d'attributs `data-*`), une page HTML par écran avec sa légende embarquée, un export PNG par Edge headless, un script Node qui vérifie les pages et régénère `03_Maquettes_Screen_Flow.md`.

**Tech Stack:** HTML/CSS/JS sans dépendance, Microsoft Edge headless (PowerShell), Node.js (vérification + génération de la doc).

**Spec:** `Code/docs/specs/2026-10-04-maquettes-ui-tutoriel-design.md`

## Global Constraints
- Canvas 1920×1080, zones sûres 88 px gauche/droite, cibles ≥ 96 px, écart 24 px.
- Polices : Bangers (titres, boutons, chiffres), Comic Neue (texte > 4 mots), 20 px minimum ; fichiers copiés depuis `Code/Heroes Dawn Unity/Assets/_Project/Resources/Fonts/`.
- Couleurs : jaune #FFD23F (principal), rouge #E5484D (MISSION/danger), secondaire #2B3140, case #1E222B, contour noir 5 px, ombre décalée 7–9 px ; familles Matière ambre, Élémentaire cyan, Physique/Bio rouge, Perception violet, toujours avec icône.
- Navigation basse `QG · ÉQUIPE · RECRUTER · PASS · BOUTIQUE` sur les écrans du hub ; absente en mode mission (⏸ à la place).
- Une seule action principale par écran ; retour en haut à gauche ; ressources puis ⚙ en haut à droite.
- Textes en français ; aucune image hors `Identite/` et portraits provisoires existants.
- Aucun téléchargement : export par `msedge.exe` installé.

## Review Focus
- Texte qui déborde de son cadre (noms longs, montants à 6 chiffres) : les pages utilisent les valeurs les plus longues plausibles (« 1 250 000 », « Élémentaire »).
- Pastille numérotée sans légende ou légende sans pastille : vérifiée par `check.js`.
- Lien du prototype cliquable vers une page inexistante : vérifié par `check.js`.
- Élément interactif dans la zone sûre de 88 px : `check.js` contrôle la position des éléments `.btn`, `.tab`, `.icon-btn` dans Edge (mesure via la page `_measure`) — à défaut, contrôle visuel sur chaque PNG.
- Lisibilité du tutoriel : bulle ≤ 2 lignes, projecteur sur un seul élément ; vérifié visuellement sur T01–T12.

---

### Task 1: Kit, export et vérification

**Files:**
- Create: `03_Art_UI_UX/Maquettes/_sources/kit.css`, `kit.js`, `fonts/` (copie des TTF + OFL), `img/` (fond et portraits provisoires réutilisés)
- Create: `03_Art_UI_UX/Maquettes/_sources/export.ps1`, `check.js`, `build-doc.js`
- Create: `03_Art_UI_UX/Maquettes/_sources/pages/_demo.html` (page témoin, supprimée à la fin de la tâche 9)
- Move: anciennes images et scripts Python → `03_Art_UI_UX/Maquettes/_archive_M1/`

**Interfaces:**
- Produces : `<body data-ref="M10" data-title="QG — Section New York" data-group="Onglet QG" data-out="ecrans" data-tab="qg|equipe|recruter|pass|boutique" data-mode="hub|mission|plain">` ; `kit.js` injecte la barre haute (hub), la barre basse (hub, onglet actif), le bouton ⏸ (mission) ; `<i class="pin" data-n="3" style="left:..;top:..">` = pastille ; `<ol class="legend">` = légendes (une `<li>` par pastille, dans l'ordre) ; `<a data-go="M20">` = lien du prototype.
- Composants CSS : `.btn.primary|.danger|.secondary`, `.panel` (case de BD), `.caption` (récitatif), `.bar.hp|.atb|.fatigue|.shield`, `.portrait.fam-matiere|...`, `.badge`, `.toggle`, `.slider`, `.seg` (choix segmenté), `.bubble` + `.guide` (Rayner), `.spotlight` (voile + découpe), `.hand`, `.tip-card`, `.modal`.
- `export.ps1 [-Only M10]` : pour chaque page, `msedge --headless --disable-gpu --hide-scrollbars --window-size=1920,1080 --screenshot=<out>/<ref>-<slug>.png file:///...?export=1` (en mode export, `kit.js` masque les liens de navigation du prototype).
- `node check.js` : code de sortie 0 si toutes les pages ont `data-ref`, `data-title`, autant de `.pin` que de `<li>` de légende, et des `data-go` qui existent ; affiche la liste des erreurs sinon.

- [ ] **Step 1:** Écrire `check.js` et la page `_demo.html` avec 2 pastilles et 1 seule légende ; `node check.js` → échec attendu « _demo : 2 pastilles, 1 légende ».
- [ ] **Step 2:** Écrire `kit.css`, `kit.js`, compléter la légende de `_demo.html` ; `node check.js` → 0 erreur.
- [ ] **Step 3:** Écrire `export.ps1` ; `pwsh export.ps1 -Only DEMO` → PNG 1920×1080 créé ; l'ouvrir et vérifier polices Bangers/Comic Neue chargées, barre haute et basse présentes.
- [ ] **Step 4:** Déplacer l'ancien contenu dans `_archive_M1/` ; commit `docs(maquettes): kit HTML comics, export Edge et vérification`.

### Task 2: Entrée (M01–M03)
**Files:** `pages/M01-titre.html`, `M02-premier-lancement.html`, `M03-chargement.html` (`data-mode="plain"`).
Contenu : spec §4 + wireframe W01 (logo, JOUER, lier un compte, langue, mentions) ; M02 = modale langue + consentement RGPD (Accepter / Personnaliser) ; M03 = illustration, barre de chargement, astuce.
- [ ] Écrire les 3 pages ; `node check.js` → 0 erreur ; `export.ps1 -Only M01,M02,M03` ; contrôle visuel des 3 PNG ; commit.

### Task 3: Onglet QG (M10–M15)
**Files:** `pages/M10-qg.html` … `M15-archives.html` (`data-tab="qg"`).
Contenu : M10 = 5 pièces en cases de BD avec niveau, état et badge, bandeau objectifs secondaires, missions du jour, bouton MISSION flottant ; M11–M15 = contenus de W03–W07 dans le nouveau cadre (retour en haut à gauche, barre basse visible, onglet QG actif).
- [ ] Écrire les 6 pages ; check ; export ; contrôle visuel ; commit.

### Task 4: Onglets Équipe, Recruter, Pass, Boutique (M20–M50)
**Files:** `M20-effectif.html`, `M21-fiche-membre.html`, `M30-recrutement.html`, `M31-confirmation-recrutement.html`, `M40-pass.html`, `M50-boutique.html`.
Contenu : W15–W18 ; M31 = modale sur M30 voilé (coût en PR, solde après achat, Recruter / Annuler).
- [ ] Écrire les 6 pages ; check ; export ; contrôle visuel ; commit.

### Task 5: Mission — carte, préparation, plateau, pause (M60–M64, M72)
**Files:** `M60-carte.html`, `M61-fiche-zone.html`, `M62-preparation.html`, `M63-plateau-lancer.html`, `M64-plateau-destination.html`, `M72-pause.html` (`data-mode="mission"`).
Contenu : W08–W10b ; M72 = modale Pause (Reprendre, Paramètres, Abandonner la zone) sur plateau voilé.
- [ ] Écrire les 6 pages ; check ; export ; contrôle visuel ; commit.

### Task 6: Mission — combat et cases (M65–M71)
**Files:** `M65-combat-pret.html`, `M66-combat-ciblage.html`, `M67-mini-jeux.html`, `M68-evenement.html`, `M69-repos.html`, `M70-resultats-combat.html`, `M71-zone-securisee.html`.
Contenu : W11a–W14 ; M69 = popup repos (soigner / garder le temps, effet sur la fatigue).
- [ ] Écrire les 7 pages ; check ; export ; contrôle visuel ; commit.

### Task 7: Paramètres (M80–M85)
**Files:** `M80-parametres-audio.html` … `M85-parametres-compte-support.html` (`data-mode="hub"`, sans onglet actif).
Contenu : spec §5, une catégorie active par page, colonne des 8 catégories toujours visible (Contrôles PC marqué « PC »), « Réinitialiser » ; M85 montre Compte & Support avec la modale de double confirmation de suppression.
- [ ] Écrire les 6 pages ; check ; export ; contrôle visuel ; commit.

### Task 8: Tutoriel (T01–T12)
**Files:** `pages/T01-bienvenue.html` … `T12-passer.html` (`data-out="tutoriel"`).
Contenu : spec §6, une page par étape du parcours (T01–T09) sur l'écran cible correspondant, avec `.spotlight` sur l'élément à toucher, `.hand`, `.bubble` de Rayner (récitatif d'étape + 2 lignes + « 3 / 9 » + « Passer ») ; T10 carte Astuce (combo) non bloquante ; T11 fin ; T12 confirmation « Passer le tutoriel ? ».
- [ ] Écrire les 12 pages ; check ; export ; contrôle visuel (projecteur sur un seul élément, bulle ≤ 2 lignes) ; commit.

### Task 9: Références et prototype cliquable
**Files:** `M90-kit-ui.html`, `M91-screen-flow.html` (`data-out="reference"`), `_sources/index.html` (sommaire + point d'entrée M01), liens `data-go` ajoutés sur les pages ; suppression de `_demo.html`.
- [ ] Écrire les pages ; ajouter les liens (onglets de la barre basse, MISSION → M60 → M61 → M62 → M63, retours, ⚙ → M80) ; `node check.js` → 0 erreur ; ouvrir `index.html` dans le navigateur et parcourir M01 → M10 → M60 → M63 → M65 → M70 ; export ; commit.

### Task 10: Documentation
**Files:** `build-doc.js` (génère `03_Art_UI_UX/03_Maquettes_Screen_Flow.md` : intro, règles §3 de la spec, navigation, puis une section par page dans l'ordre des références avec image + légende), `03_Art_UI_UX/01_UX_Ecrans.md` (navigation par onglets, paramètres, onboarding = tutoriel guidé), `03_Art_UI_UX/README.md`, `05_Production/Decisions.md` (ligne 2026-10-04).
- [ ] `node build-doc.js` ; relire le Markdown généré (toutes les images existent) ; mettre à jour les 3 docs ; commit `docs(ux): maquettes haute fidélité, paramètres et tutoriel`.
