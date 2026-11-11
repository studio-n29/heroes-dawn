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
    /// Outil « Couleurs des ennemis » : règle les variantes de couleurs de chaque sorte d'ennemi 3D (soldat, gangster, créature) et les montre sur le modèle,
    /// côte à côte, avant de les enregistrer dans la palette utilisée par l'arène (Resources/Characters3D/EnemyPalettes).
    /// Chaque variante a deux teintes (vêtements, équipement sombre) ; la peau n'est jamais teintée. Les zones sont tirées de la texture : on règle les seuils
    /// et on voit le masque en direct.
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
        Texture2D? _baseTex, _maskTex;
        string _maskKind = "";
        bool _maskDirty = true;

        [MenuItem("Heroes Dawn/Personnages 3D/Couleurs des ennemis")]
        public static void Open()
        {
            var w = GetWindow<EnemyColorWindow>(false, "Couleurs des ennemis", true);
            w.minSize = new Vector2(900f, 640f);
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
            ReleaseMask();
            _preview?.Cleanup();
            _preview = null;
        }

        void ClearInstances()
        {
            foreach (var go in _instances) if (go != null) DestroyImmediate(go);
            _instances.Clear();
            _builtKind = ""; _builtCount = -1;
        }

        void ReleaseMask()
        {
            if (_baseTex != null) DestroyImmediate(_baseTex);
            if (_maskTex != null) DestroyImmediate(_maskTex);
            _baseTex = _maskTex = null; _maskKind = "";
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
                EditorGUILayout.LabelField("Vêtements et équipement sont teintés ; la peau ne l'est jamais", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Valeurs d'origine", EditorStyles.toolbarButton) &&
                    EditorUtility.DisplayDialog("Valeurs d'origine", "Remplacer toutes les palettes par celles d'origine ?", "Remplacer", "Annuler"))
                {
                    Undo.RecordObject(_palettes, "Palettes d'origine");
                    _palettes.kinds = EnemyPalettes.Defaults();
                    _maskDirty = true;
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
                    if (GUILayout.Toggle(i == _kind, k.displayName + "  (" + k.variants.Length + ")", style, GUILayout.Height(26f)) && _kind != i) { _kind = i; _maskDirty = true; }
                }
            }
        }

        // ------------------------------------------------------------------ Masque

        void EnsureMask(EnemyPalettes.Kind kind)
        {
            if (_maskKind != kind.id)
            {
                ReleaseMask();
                _baseTex = CharacterMaskBuilder.LoadBase(kind.id);
                _maskKind = kind.id;
                _maskDirty = true;
            }
            if (_maskDirty && _baseTex != null)
            {
                if (_maskTex != null) DestroyImmediate(_maskTex);
                _maskTex = CharacterMaskBuilder.Compute(kind.id, _baseTex, kind.mask, 4);
                _maskDirty = false;
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
            EnsureMask(kind);
            var e = Event.current;
            if (e.type == EventType.MouseDrag && rect.Contains(e.mousePosition)) { _yaw -= e.delta.x * 0.8f; e.Use(); Repaint(); }

            int n = _instances.Count;
            float spacing = 1.35f * Mathf.Max(1f, kind.scale);
            float total = (n - 1) * spacing;
            for (int i = 0; i < n; i++)
            {
                var go = _instances[i];
                go.transform.position = new Vector3(i * spacing - total * 0.5f, 0f, 0f);
                go.transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                var view = go.GetComponent<Character3DView>();
                if (view == null) continue;
                view.MaskOverride = _maskTex;
                view.SetTint(kind.variants[i].tint, kind.variants[i].tintB);
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
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Nom", EditorStyles.miniLabel, GUILayout.Width(150f));
                EditorGUILayout.LabelField("Vêtements", EditorStyles.miniLabel, GUILayout.Width(100f));
                EditorGUILayout.LabelField("Équipement", EditorStyles.miniLabel, GUILayout.Width(100f));
            }
            int remove = -1, up = -1, down = -1, dup = -1;
            for (int i = 0; i < kind.variants.Length; i++)
            {
                var v = kind.variants[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    v.name = EditorGUILayout.TextField(v.name, GUILayout.Width(150f));
                    v.tint = EditorGUILayout.ColorField(GUIContent.none, v.tint, true, false, false, GUILayout.Width(100f));
                    v.tintB = EditorGUILayout.ColorField(GUIContent.none, v.tintB, true, false, false, GUILayout.Width(100f));
                    if (GUILayout.Button("↑", GUILayout.Width(24f))) up = i;
                    if (GUILayout.Button("↓", GUILayout.Width(24f))) down = i;
                    if (GUILayout.Button("Dupliquer", GUILayout.Width(70f))) dup = i;
                    if (GUILayout.Button("Supprimer", GUILayout.Width(70f))) remove = i;
                }
            }
            var list = kind.variants.ToList();
            if (up > 0) { (list[up - 1], list[up]) = (list[up], list[up - 1]); }
            else if (down >= 0 && down < list.Count - 1) { (list[down + 1], list[down]) = (list[down], list[down + 1]); }
            else if (dup >= 0) list.Insert(dup + 1, new EnemyPalettes.Variant { name = list[dup].name + " bis", tint = list[dup].tint, tintB = list[dup].tintB });
            else if (remove >= 0 && list.Count > 1) list.RemoveAt(remove);
            kind.variants = list.ToArray();

            if (GUILayout.Button("Ajouter une variante", GUILayout.Width(180f)))
            {
                var l2 = kind.variants.ToList();
                var c = Color.HSVToRGB(Random.value, 0.55f, 0.95f);
                l2.Add(new EnemyPalettes.Variant { name = "Variante " + (l2.Count + 1), tint = c, tintB = Color.Lerp(c, Color.black, 0.45f) });
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
                    kind.variants = Enumerable.Range(0, _generateCount).Select(i =>
                    {
                        var main = Color.HSVToRGB(Mathf.Repeat(h + i / (float)_generateCount, 1f), s, vv);
                        return new EnemyPalettes.Variant { name = "Teinte " + (i + 1), tint = main, tintB = Color.Lerp(main, Color.black, 0.45f) };
                    }).ToArray();
                }
            }

            EditorGUILayout.Space(10f);
            DrawMaskRules(kind);
            if (EditorGUI.EndChangeCheck()) Dirty();
        }

        void DrawMaskRules(EnemyPalettes.Kind kind)
        {
            EditorGUILayout.LabelField("Zones teintées (tirées de la texture)", EditorStyles.miniBoldLabel);
            var r = kind.mask;
            EditorGUI.BeginChangeCheck();
            r.separateSkin = EditorGUILayout.Toggle(new GUIContent("Garder la peau intacte", "Décoché : tout le modèle est teinté (créature de pierre)."), r.separateSkin);
            using (new EditorGUI.DisabledScope(!r.separateSkin))
            {
                r.keepHands = EditorGUILayout.Toggle(new GUIContent("Mains intactes", "Peau ou gants : jamais teintes."), r.keepHands);
                r.keepHead = EditorGUILayout.Toggle(new GUIContent("Visage intact", "Decoche : casque ou masque teinte."), r.keepHead);
                r.feetAsGear = EditorGUILayout.Toggle(new GUIContent("Chaussures en equipement", "Prennent la deuxieme teinte."), r.feetAsGear);
                r.darkBelow = EditorGUILayout.Slider(new GUIContent("Équipement sombre sous", "Luminosité sous laquelle une zone compte comme équipement."), r.darkBelow, 0f, 1f);
            }
            if (EditorGUI.EndChangeCheck()) _maskDirty = true;

            EnsureMask(kind);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (_maskTex != null)
                {
                    var thumb = GUILayoutUtility.GetRect(200f, 200f, GUILayout.Width(200f), GUILayout.Height(200f));
                    GUI.DrawTexture(thumb, _maskTex, ScaleMode.ScaleToFit, false);
                }
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.HelpBox("Rouge : vêtements (première teinte)\nVert : équipement sombre (deuxième teinte)\nNoir : peau et fond, jamais teintés", MessageType.None);
                    if (GUILayout.Button("Appliquer le masque au projet", GUILayout.Height(28f)))
                    {
                        var written = CharacterMaskBuilder.Write(kind.id, kind.mask);
                        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Resources/Characters3D/Materials/" + kind.id + ".mat");
                        if (mat != null && written != null) { mat.SetTexture("_MaskMap", written); EditorUtility.SetDirty(mat); AssetDatabase.SaveAssets(); }
                        EditorUtility.SetDirty(_palettes); AssetDatabase.SaveAssets();
                        ShowNotification(new GUIContent("Masque enregistré pour " + kind.displayName));
                    }
                    EditorGUILayout.LabelField("L'aperçu ci-dessus utilise le masque en direct ; le jeu utilise le masque enregistré.", EditorStyles.wordWrappedMiniLabel);
                }
            }
        }
    }
}
