#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.LevelDesign
{
    /// <summary>
    /// Éditeur de plateaux (Heroes Dawn ▸ Level Design) : placer les cases sur la carte et choisir leur type (activité, table d'ennemis et manches
    /// pour un combat), créer et relier les chemins, peindre des zones de biome, générer le fond de carte (biome + météo), régler objectifs et
    /// validation, et éditer la carte de campagne (1.1, 1.2…). Les données vivent dans des assets <see cref="ZoneAsset"/> / <see cref="CampaignAsset"/>.
    /// </summary>
    public sealed partial class LevelDesignWindow : EditorWindow
    {
        enum Tab { Plateau, Campagne }
        enum Tool { Selection, Placer, Relier, Supprimer, Zone }

        const float CellW = 90f, CellH = 120f, InspectorWidth = 400f, NodeRadius = 22f;

        ZoneAsset? _zone;
        Tab _tab;
        Tool _tool = Tool.Selection;
        CaseType _placeType = CaseType.Empty;
        Vector2 _pan = new Vector2(80, 80);
        float _zoom = 0.8f;
        string? _selected;
        string? _linkFrom;
        bool _dragging;
        bool _panning;
        Vector2 _regionStart;
        RegionData? _regionDraft;
        Vector2 _scroll;
        ContentCatalog? _catalog;
        IReadOnlyList<ZoneIssue> _issues = Array.Empty<ZoneIssue>();
        bool _issuesDirty = true;
        Texture2D? _bgTexture;
        string _bgLoadedFor = "";
        bool _showBackground = true;
        bool _showGrid = true;

        [MenuItem("Heroes Dawn/Level Design/Éditeur de plateaux")]
        public static void Open() => GetWindow<LevelDesignWindow>("Level Design").minSize = new Vector2(1000, 600);

        public static void Open(ZoneAsset zone)
        {
            var w = GetWindow<LevelDesignWindow>("Level Design");
            w._zone = zone;
            w._tab = Tab.Plateau;
            w._issuesDirty = true;
        }

        [MenuItem("Heroes Dawn/Level Design/Importer les zones du prototype")]
        public static void ImportPrototypeMenu()
        {
            var c = ZoneBaker.ImportPrototype();
            Selection.activeObject = c;
            EditorUtility.DisplayDialog("Level Design", "Zones du prototype importées dans " + ZoneBaker.Folder + " (" + c.entries.Count + " plateaux + campagne).", "OK");
        }

        ContentCatalog Catalog => _catalog ??= PrototypeContent.Build();

        // ------------------------------------------------------------------ Fenêtre

        void OnGUI()
        {
            DrawToolbar();
            if (_tab == Tab.Campagne) { DrawCampaign(); return; }
            if (_zone == null)
            {
                EditorGUILayout.HelpBox("Choisis un plateau ou crée-en un. « Heroes Dawn ▸ Level Design ▸ Importer les zones du prototype » transforme les zones du jeu en plateaux modifiables.", MessageType.Info);
                return;
            }
            var area = new Rect(0, 24, position.width, position.height - 24);
            var canvas = new Rect(area.x, area.y, area.width - InspectorWidth, area.height);
            var inspector = new Rect(canvas.xMax, area.y, InspectorWidth, area.height);
            if (_issuesDirty) Revalidate();
            DrawCanvas(canvas);
            GUILayout.BeginArea(inspector, EditorStyles.helpBox);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawInspector();
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            _tab = (Tab)GUILayout.Toolbar((int)_tab, new[] { "Plateau", "Campagne" }, EditorStyles.toolbarButton, GUILayout.Width(200));
            GUILayout.Space(8);
            if (_tab == Tab.Plateau)
            {
                var z = (ZoneAsset?)EditorGUILayout.ObjectField(_zone, typeof(ZoneAsset), false, GUILayout.Width(220));
                if (z != _zone) { _zone = z; _selected = null; _linkFrom = null; _issuesDirty = true; }
                if (GUILayout.Button("Nouveau plateau", EditorStyles.toolbarButton, GUILayout.Width(110))) NewZoneDialog();
                GUILayout.Space(8);
                _tool = (Tool)GUILayout.Toolbar((int)_tool, new[] { "Sélection", "Placer", "Relier", "Supprimer", "Zone" }, EditorStyles.toolbarButton, GUILayout.Width(330));
                if (_tool == Tool.Placer) _placeType = (CaseType)EditorGUILayout.EnumPopup(_placeType, EditorStyles.toolbarPopup, GUILayout.Width(90));
                _showGrid = GUILayout.Toggle(_showGrid, "Grille", EditorStyles.toolbarButton, GUILayout.Width(50));
                _showBackground = GUILayout.Toggle(_showBackground, "Fond", EditorStyles.toolbarButton, GUILayout.Width(45));
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Enregistrer", EditorStyles.toolbarButton, GUILayout.Width(80))) AssetDatabase.SaveAssets();
            GUILayout.EndHorizontal();
        }

        void NewZoneDialog()
        {
            int n = 1;
            while (AssetDatabase.LoadAssetAtPath<ZoneAsset>(ZoneBaker.Folder + "/zone_new_" + n + ".asset") != null) n++;
            var z = ZoneBaker.NewZone("zone_new_" + n);
            _zone = z;
            _selected = null;
            _issuesDirty = true;
            Selection.activeObject = z;
        }

        void Revalidate()
        {
            _issuesDirty = false;
            _issues = _zone == null ? Array.Empty<ZoneIssue>() : LevelEditorModel.Validate(_zone, Catalog);
        }

        void Touch(string what)
        {
            if (_zone == null) return;
            Undo.RecordObject(_zone, what);
        }

        void Changed()
        {
            if (_zone != null) EditorUtility.SetDirty(_zone);
            _issuesDirty = true;
            Repaint();
        }

        // ------------------------------------------------------------------ Canevas

        Vector2 ToScreen(float gx, float gy) => new Vector2(_pan.x + gx * CellW * _zoom, _pan.y - gy * CellH * _zoom);
        Vector2 ToGrid(Vector2 p) => new Vector2((p.x - _pan.x) / (CellW * _zoom), (_pan.y - p.y) / (CellH * _zoom));

        static Color ColorOf(CaseType t)
        {
            switch (t)
            {
                case CaseType.Entry: return new Color(0.23f, 0.51f, 0.96f);
                case CaseType.Combat: return new Color(0.86f, 0.25f, 0.25f);
                case CaseType.MiniGame: return new Color(0.13f, 0.74f, 0.82f);
                case CaseType.Narrative: return new Color(0.62f, 0.42f, 0.95f);
                case CaseType.Resource: return new Color(0.95f, 0.77f, 0.2f);
                case CaseType.Rest: return new Color(0.3f, 0.78f, 0.42f);
                default: return new Color(0.55f, 0.57f, 0.62f);
            }
        }

        static string GlyphOf(CaseType t)
        {
            switch (t)
            {
                case CaseType.Entry: return "QG";
                case CaseType.Combat: return "VS";
                case CaseType.MiniGame: return "MJ";
                case CaseType.Narrative: return "!";
                case CaseType.Resource: return "$";
                case CaseType.Rest: return "Zz";
                default: return "";
            }
        }

        void LoadBackground()
        {
            string key = _zone == null ? "" : _zone.backgroundKey;
            if (key == _bgLoadedFor) return;
            _bgLoadedFor = key;
            _bgTexture = string.IsNullOrEmpty(key) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(ZoneBaker.Folder + "/" + key + ".png");
        }

        void DrawCanvas(Rect canvas)
        {
            var z = _zone!;
            LoadBackground();
            GUI.Box(canvas, GUIContent.none);
            GUI.BeginClip(canvas);
            var local = new Rect(0, 0, canvas.width, canvas.height);
            EditorGUI.DrawRect(local, new Color(0.16f, 0.17f, 0.19f));
            var e = Event.current;

            if (e.type == EventType.Repaint)
            {
                if (_showBackground && _bgTexture != null)
                {
                    var tl = ToScreen(z.backgroundMinX, z.backgroundMaxY);
                    var br = ToScreen(z.backgroundMaxX, z.backgroundMinY);
                    GUI.DrawTexture(new Rect(tl.x, tl.y, br.x - tl.x, br.y - tl.y), _bgTexture, ScaleMode.StretchToFill);
                }
                if (_showGrid) DrawGrid(local);
                DrawRegions(z);
                DrawLinks(z);
                DrawNodes(z);
                if (_tool == Tool.Relier && _linkFrom != null && z.Node(_linkFrom) != null)
                {
                    var a = ToScreen(z.Node(_linkFrom)!.x, z.Node(_linkFrom)!.y);
                    Handles.color = Color.white;
                    Handles.DrawAAPolyLine(3f, a, e.mousePosition);
                }
            }
            HandleInput(canvas, e);
            GUI.EndClip();
            GUI.Label(new Rect(canvas.x + 8, canvas.yMax - 22, canvas.width - 16, 18), Hint(), EditorStyles.miniLabel);
        }

        string Hint()
        {
            switch (_tool)
            {
                case Tool.Placer: return "Clique une cellule vide pour placer une case (" + _placeType + "). Molette : zoom · clic droit / Alt+glisser : déplacer la vue.";
                case Tool.Relier: return "Clique une case puis une autre pour créer ou retirer un chemin.";
                case Tool.Supprimer: return "Clique une case (ou un chemin) pour la supprimer.";
                case Tool.Zone: return "Glisse depuis le centre pour peindre une zone de biome (forêt, ville…) sur le fond.";
                default: return "Clique une case pour la sélectionner, glisse-la pour la déplacer (grille).";
            }
        }

        void DrawGrid(Rect local)
        {
            var tl = ToGrid(Vector2.zero); var br = ToGrid(new Vector2(local.width, local.height));
            Handles.color = new Color(1, 1, 1, 0.07f);
            for (int x = Mathf.FloorToInt(tl.x); x <= Mathf.CeilToInt(br.x); x++)
                Handles.DrawLine(ToScreen(x, tl.y), ToScreen(x, br.y));
            for (int y = Mathf.FloorToInt(br.y); y <= Mathf.CeilToInt(tl.y); y++)
                Handles.DrawLine(ToScreen(tl.x, y), ToScreen(br.x, y));
        }

        void DrawRegions(ZoneAsset z)
        {
            foreach (var r in z.regions.Concat(_regionDraft != null ? new[] { _regionDraft } : Array.Empty<RegionData>()))
            {
                var c = ToScreen(r.x, r.y);
                Handles.color = new Color(0.3f, 0.8f, 0.4f, 0.18f);
                Handles.DrawSolidDisc(c, Vector3.forward, r.radius * CellW * _zoom);
                Handles.color = new Color(0.3f, 0.8f, 0.4f, 0.8f);
                Handles.DrawWireDisc(c, Vector3.forward, r.radius * CellW * _zoom);
                GUI.Label(new Rect(c.x - 40, c.y - 8, 80, 16), r.biome.ToString(), EditorStyles.centeredGreyMiniLabel);
            }
        }

        void DrawLinks(ZoneAsset z)
        {
            foreach (var l in z.links)
            {
                var a = z.Node(l.a); var b = z.Node(l.b);
                if (a == null || b == null) { continue; }
                bool sel = _selected == l.a || _selected == l.b;
                Handles.color = sel ? Color.white : new Color(0.8f, 0.8f, 0.85f, 0.85f);
                Handles.DrawAAPolyLine(sel ? 5f : 3f, ToScreen(a.x, a.y), ToScreen(b.x, b.y));
            }
        }

        void DrawNodes(ZoneAsset z)
        {
            var objectiveTargets = new HashSet<string>(z.objectives.Select(o => o.target));
            var errors = new HashSet<string>(_issues.Where(i => i.Severity == IssueSeverity.Error && i.Case.HasValue).Select(i => i.Case!.Value.Value));
            var warns = new HashSet<string>(_issues.Where(i => i.Severity == IssueSeverity.Warning && i.Case.HasValue).Select(i => i.Case!.Value.Value));
            foreach (var n in z.nodes)
            {
                var p = ToScreen(n.x, n.y);
                float r = NodeRadius * _zoom;
                if (n.id == _selected) { Handles.color = Color.white; Handles.DrawSolidDisc(p, Vector3.forward, r + 6); }
                if (objectiveTargets.Contains(n.id)) { Handles.color = new Color(1f, 0.85f, 0.2f); Handles.DrawWireDisc(p, Vector3.forward, r + 4, 3f); }
                if (errors.Contains(n.id)) { Handles.color = Color.red; Handles.DrawWireDisc(p, Vector3.forward, r + 9, 3f); }
                else if (warns.Contains(n.id)) { Handles.color = new Color(1f, 0.7f, 0.1f); Handles.DrawWireDisc(p, Vector3.forward, r + 9, 2f); }
                Handles.color = ColorOf(n.type) * (n.isFixed ? 1f : 0.85f);
                Handles.DrawSolidDisc(p, Vector3.forward, r);
                var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
                GUI.Label(new Rect(p.x - r, p.y - 9, r * 2, 18), GlyphOf(n.type), style);
                var small = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter, normal = { textColor = new Color(1, 1, 1, 0.8f) } };
                GUI.Label(new Rect(p.x - 40, p.y + r + 2, 80, 14), n.id + (n.type == CaseType.Combat ? " " + n.rank : n.type == CaseType.MiniGame && n.activity != "" ? " " + n.activity : ""), small);
            }
        }

        NodeData? HitNode(Vector2 mouse)
        {
            var z = _zone!;
            NodeData? best = null; float bestD = (NodeRadius * _zoom + 6) * (NodeRadius * _zoom + 6);
            foreach (var n in z.nodes)
            {
                var d = (ToScreen(n.x, n.y) - mouse).sqrMagnitude;
                if (d <= bestD) { bestD = d; best = n; }
            }
            return best;
        }

        LinkData? HitLink(Vector2 mouse)
        {
            var z = _zone!;
            foreach (var l in z.links)
            {
                var a = z.Node(l.a); var b = z.Node(l.b);
                if (a == null || b == null) continue;
                if (HandleUtility.DistancePointToLineSegment(mouse, ToScreen(a.x, a.y), ToScreen(b.x, b.y)) < 8f) return l;
            }
            return null;
        }

        void HandleInput(Rect canvas, Event e)
        {
            var z = _zone!;
            if (e.type == EventType.ScrollWheel && new Rect(0, 0, canvas.width, canvas.height).Contains(e.mousePosition))
            {
                var before = ToGrid(e.mousePosition);
                _zoom = Mathf.Clamp(_zoom * (e.delta.y < 0 ? 1.1f : 1f / 1.1f), 0.25f, 2.5f);
                var after = ToGrid(e.mousePosition);
                _pan += new Vector2((after.x - before.x) * CellW * _zoom, -(after.y - before.y) * CellH * _zoom);
                e.Use();
                Repaint();
                return;
            }
            bool panButton = e.button == 1 || e.button == 2 || (e.button == 0 && e.alt);
            if (e.type == EventType.MouseDown && panButton) { _panning = true; e.Use(); return; }
            if (e.type == EventType.MouseDrag && _panning) { _pan += e.delta; e.Use(); Repaint(); return; }
            if (e.type == EventType.MouseUp && _panning) { _panning = false; e.Use(); return; }
            if (e.button != 0) return;

            var grid = ToGrid(e.mousePosition);
            int gx = Mathf.RoundToInt(grid.x), gy = Mathf.RoundToInt(grid.y);
            if (e.type == EventType.MouseDown)
            {
                var hit = HitNode(e.mousePosition);
                switch (_tool)
                {
                    case Tool.Selection:
                        _selected = hit?.id;
                        _dragging = hit != null;
                        if (hit != null) Touch("Déplacer une case");
                        break;
                    case Tool.Placer:
                        if (hit == null)
                        {
                            Touch("Placer une case");
                            var n = LevelEditorModel.AddNode(z, _placeType, gx, gy);
                            if (n != null) _selected = n.id;
                            Changed();
                        }
                        else _selected = hit.id;
                        break;
                    case Tool.Relier:
                        if (hit != null)
                        {
                            if (_linkFrom == null) _linkFrom = hit.id;
                            else if (_linkFrom != hit.id)
                            {
                                Touch("Relier des cases");
                                if (!LevelEditorModel.Connect(z, _linkFrom, hit.id)) LevelEditorModel.Disconnect(z, _linkFrom, hit.id);
                                _linkFrom = hit.id;
                                Changed();
                            }
                            _selected = hit.id;
                        }
                        else _linkFrom = null;
                        break;
                    case Tool.Supprimer:
                        if (hit != null)
                        {
                            Touch("Supprimer une case");
                            LevelEditorModel.DeleteNode(z, hit.id);
                            if (_selected == hit.id) _selected = null;
                            Changed();
                        }
                        else
                        {
                            var link = HitLink(e.mousePosition);
                            if (link != null) { Touch("Supprimer un chemin"); LevelEditorModel.Disconnect(z, link.a, link.b); Changed(); }
                        }
                        break;
                    case Tool.Zone:
                        _regionStart = grid;
                        _regionDraft = new RegionData { x = grid.x, y = grid.y, radius = 0.2f, biome = Biome.Forest };
                        break;
                }
                e.Use();
                Repaint();
            }
            else if (e.type == EventType.MouseDrag)
            {
                if (_tool == Tool.Selection && _dragging && _selected != null)
                {
                    if (LevelEditorModel.MoveNode(z, _selected, gx, gy)) Changed();
                    e.Use();
                }
                else if (_tool == Tool.Zone && _regionDraft != null)
                {
                    _regionDraft.radius = Mathf.Max(0.2f, Vector2.Distance(_regionStart, grid));
                    e.Use();
                    Repaint();
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                _dragging = false;
                if (_tool == Tool.Zone && _regionDraft != null)
                {
                    Touch("Peindre une zone");
                    _regionDraft.x = Mathf.Round(_regionDraft.x * 2) / 2f;
                    _regionDraft.y = Mathf.Round(_regionDraft.y * 2) / 2f;
                    _regionDraft.radius = Mathf.Max(0.5f, Mathf.Round(_regionDraft.radius * 2) / 2f);
                    z.regions.Add(_regionDraft);
                    _regionDraft = null;
                    Changed();
                }
            }
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && _selected != null)
            {
                Touch("Supprimer une case");
                LevelEditorModel.DeleteNode(z, _selected);
                _selected = null;
                Changed();
                e.Use();
            }
        }
    }
}
