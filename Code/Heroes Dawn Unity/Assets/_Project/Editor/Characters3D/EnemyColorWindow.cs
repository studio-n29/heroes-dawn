#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Presentation.Characters3D;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Characters3D
{
    /// <summary>
    /// Outil « Couleurs des ennemis » : règle les variantes de teinte de chaque sorte d'ennemi 3D (soldat, gangster, créature) et les montre sur le modèle,
    /// côte à côte, avant de les enregistrer dans la palette utilisée par l'arène (Resources/Characters3D/EnemyPalettes).
    /// </summary>
    public sealed class EnemyColorWindow : EditorWindow
    {
        EnemyPalettes _palettes = null!;
        int _kind;
        Vector2 _scroll;
        float _yaw = 25f;
        int _generateCount = 6;
        Color _baseColor = new Color(0.9f, 0.35f, 0.35f);
        PreviewRenderUtility? _preview;
        readonly List<GameObject> _instances = new List<GameObject>();
        string _builtKind = "";
        int _builtCount = -1;

        [MenuItem("Heroes Dawn/Personnages 3D/Couleurs des ennemis")]
        public static void Open()
        {
            var w = GetWindow<EnemyColorWindow>(false, "Couleurs des ennemis", true);
            w.minSize = new Vector2(760f, 560f);
            w.Show();
        }

        void OnEnable()
        {
            _palettes = CharacterModelSetup.EnsurePalettes();
            _preview = new PreviewRenderUtility();
            _preview.cameraFieldOfView = 22f;
            _preview.camera.clearFlags = CameraClearFlags.SolidColor;
            _preview.camera.backgroundColor = new Color(0.16f, 0.18f, 0.22f);
            _preview.camera.nearClipPlane = 0.1f;
            _preview.camera.farClipPlane = 60f;
        }

        void OnDisable()
        {
            ClearInstances();
            _preview?.Cleanup();
            _preview = null;
        }

        void ClearInstances()
        {
            foreach (var go in _instances) if (go != null) DestroyImmediate(go);
            _instances.Clear();
            _builtKind = ""; _builtCount = -1;
        }

        void OnGUI()
        {
            if (_palettes == null) _palettes = CharacterModelSetup.EnsurePalettes();
            DrawToolbar();
            if (_palettes.kinds.Length == 0) { EditorGUILayout.HelpBox("Aucune sorte d'ennemi : utilise « Valeurs d'origine ».", MessageType.Info); return; }
            _kind = Mathf.Clamp(_kind, 0, _palettes.kinds.Length - 1);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawKindList();
                using (new EditorGUILayout.VerticalScope())
                {
                    var kind = _palettes.kinds[_kind];
                    DrawPreview(kind);
                    _scroll = EditorGUILayout.BeginScrollView(_scroll);
                    DrawKind(kind);
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField("Teintes multipliées par la texture grise du modèle", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Valeurs d'origine", EditorStyles.toolbarButton) &&
                    EditorUtility.DisplayDialog("Valeurs d'origine", "Remplacer toutes les palettes par celles d'origine ?", "Remplacer", "Annuler"))
                {
                    Undo.RecordObject(_palettes, "Palettes d'origine");
                    _palettes.kinds = EnemyPalettes.Defaults();
                    Dirty();
                }
                if (GUILayout.Button("Enregistrer", EditorStyles.toolbarButton)) { EditorUtility.SetDirty(_palettes); AssetDatabase.SaveAssets(); }
            }
        }

        void Dirty()
        {
            EditorUtility.SetDirty(_palettes);
            Repaint();
        }

        void DrawKindList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(170f)))
            {
                EditorGUILayout.LabelField("Ennemis", EditorStyles.boldLabel);
                for (int i = 0; i < _palettes.kinds.Length; i++)
                {
                    var k = _palettes.kinds[i];
                    var style = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontStyle = i == _kind ? FontStyle.Bold : FontStyle.Normal };
                    if (GUILayout.Toggle(i == _kind, k.displayName + "  (" + k.variants.Length + ")", style, GUILayout.Height(26f))) _kind = i;
                }
            }
        }

        // ------------------------------------------------------------------ Aperçu

        void BuildInstances(EnemyPalettes.Kind kind)
        {
            ClearInstances();
            var prefab = Character3DLibrary.Prefab(kind.id);
            if (prefab == null || _preview == null) return;
            var controller = prefab.GetComponent<Animator>()?.runtimeAnimatorController as AnimatorController;
            var idleState = controller?.layers[0].stateMachine.states.FirstOrDefault(s => s.state.name == Character3DView.Idle).state;
            var idleClip = idleState?.motion as AnimationClip;
            for (int i = 0; i < kind.variants.Length; i++)
            {
                var go = _preview.InstantiatePrefabInScene(prefab);
                go.transform.localScale = Vector3.one * kind.scale;
                if (idleClip != null)
                {
                    try { idleClip.SampleAnimation(go, 0.6f); } catch { /* aperçu en T-pose si le clip ne s'applique pas hors jeu */ }
                }
                _instances.Add(go);
            }
            _builtKind = kind.id; _builtCount = kind.variants.Length;
        }


        void DrawPreview(EnemyPalettes.Kind kind)
        {
            var rect = GUILayoutUtility.GetRect(10f, 300f, GUILayout.ExpandWidth(true));
            if (_preview == null) return;
            if (kind.id != _builtKind || kind.variants.Length != _builtCount) BuildInstances(kind);
            if (_instances.Count == 0)
            {
                EditorGUI.HelpBox(rect, "Prefab « " + kind.id + " » introuvable : lance « 2. Générer les prefabs et les palettes ».", MessageType.Warning);
                return;
            }
            var e = Event.current;
            if (e.type == EventType.MouseDrag && rect.Contains(e.mousePosition)) { _yaw -= e.delta.x * 0.8f; e.Use(); Repaint(); }

            int n = _instances.Count;
            float spacing = 1.35f * Mathf.Max(1f, kind.scale);
            float total = (n - 1) * spacing;
            var block = new MaterialPropertyBlock();
            for (int i = 0; i < n; i++)
            {
                var go = _instances[i];
                go.transform.position = new Vector3(i * spacing - total * 0.5f, 0f, 0f);
                go.transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                var tint = kind.variants[i].tint;
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                {
                    r.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", tint);
                    r.SetPropertyBlock(block);
                }
            }
            float height = 1.9f * Mathf.Max(1f, kind.scale);
            float aspect = Mathf.Max(0.5f, rect.width / rect.height);
            float width = total + spacing;
            float fov = _preview.cameraFieldOfView * Mathf.Deg2Rad;
            float dist = Mathf.Max(height * 0.62f / Mathf.Tan(fov * 0.5f), width * 0.5f / aspect / Mathf.Tan(fov * 0.5f)) * 1.15f;
            _preview.camera.transform.position = new Vector3(0f, height * 0.5f, -dist);
            _preview.camera.transform.rotation = Quaternion.identity;
            _preview.BeginPreview(rect, GUIStyle.none);
            _preview.camera.Render();
            var tex = _preview.EndPreview();
            GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, false);
            EditorGUI.LabelField(new Rect(rect.x + 6f, rect.y + 4f, rect.width, 16f), "Glisse pour tourner", EditorStyles.miniLabel);
            for (int i = 0; i < n; i++)
            {
                float x = rect.x + rect.width * ((i * spacing - total * 0.5f) / (width * 1.15f) + 0.5f);
                var label = new Rect(x - 60f, rect.yMax - 20f, 120f, 18f);
                GUI.Label(label, kind.variants[i].name, new GUIStyle(EditorStyles.whiteMiniLabel) { alignment = TextAnchor.MiddleCenter });
            }
        }

        // ------------------------------------------------------------------ Réglages

        void DrawKind(EnemyPalettes.Kind kind)
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(_palettes, "Couleurs des ennemis");
            EditorGUILayout.LabelField(kind.displayName + "  —  modèle « " + kind.id + " »", EditorStyles.boldLabel);
            kind.scale = EditorGUILayout.Slider("Échelle dans l'arène", kind.scale, 0.5f, 2.5f);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Variantes (une est choisie par ennemi, dans l'ordre)", EditorStyles.miniBoldLabel);

            int remove = -1, up = -1, down = -1, dup = -1;
            for (int i = 0; i < kind.variants.Length; i++)
            {
                var v = kind.variants[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    v.name = EditorGUILayout.TextField(v.name, GUILayout.Width(150f));
                    v.tint = EditorGUILayout.ColorField(GUIContent.none, v.tint, true, false, false, GUILayout.Width(120f));
                    EditorGUILayout.LabelField("#" + ColorUtility.ToHtmlStringRGB(v.tint), GUILayout.Width(70f));
                    if (GUILayout.Button("↑", GUILayout.Width(24f))) up = i;
                    if (GUILayout.Button("↓", GUILayout.Width(24f))) down = i;
                    if (GUILayout.Button("Dupliquer", GUILayout.Width(70f))) dup = i;
                    if (GUILayout.Button("Supprimer", GUILayout.Width(70f))) remove = i;
                }
            }
            var list = kind.variants.ToList();
            if (up > 0) { (list[up - 1], list[up]) = (list[up], list[up - 1]); }
            else if (down >= 0 && down < list.Count - 1) { (list[down + 1], list[down]) = (list[down], list[down + 1]); }
            else if (dup >= 0) list.Insert(dup + 1, new EnemyPalettes.Variant { name = list[dup].name + " bis", tint = list[dup].tint });
            else if (remove >= 0 && list.Count > 1) list.RemoveAt(remove);
            kind.variants = list.ToArray();

            if (GUILayout.Button("Ajouter une variante", GUILayout.Width(180f)))
            {
                var l2 = kind.variants.ToList();
                l2.Add(new EnemyPalettes.Variant { name = "Variante " + (l2.Count + 1), tint = Color.HSVToRGB(Random.value, 0.55f, 0.95f) });
                kind.variants = l2.ToArray();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Générer une série de variantes par décalage de teinte", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                _baseColor = EditorGUILayout.ColorField(GUIContent.none, _baseColor, true, false, false, GUILayout.Width(120f));
                _generateCount = EditorGUILayout.IntSlider(_generateCount, 2, 12);
                if (GUILayout.Button("Générer (remplace les variantes)", GUILayout.Width(220f)))
                {
                    Color.RGBToHSV(_baseColor, out var h, out var s, out var vv);
                    kind.variants = Enumerable.Range(0, _generateCount).Select(i => new EnemyPalettes.Variant
                    {
                        name = "Teinte " + (i + 1),
                        tint = Color.HSVToRGB(Mathf.Repeat(h + i / (float)_generateCount, 1f), s, vv),
                    }).ToArray();
                }
            }
            if (EditorGUI.EndChangeCheck()) Dirty();
        }
    }
}
