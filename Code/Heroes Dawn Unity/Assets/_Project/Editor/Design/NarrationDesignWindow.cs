#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Presentation.Common;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Design
{
    /// <summary>
    /// Outil de narration : onglet Dialogues (répliques jouées avant et après une case de chaque zone : créer, modifier, réordonner, supprimer)
    /// et onglet Textes (tutoriel, astuces, introduction, quêtes, QG… : modifier un texte du jeu, en ajouter, revenir à l'original).
    /// </summary>
    public sealed class NarrationDesignWindow : EditorWindow
    {
        [MenuItem("Heroes Dawn/Outils/Narration")]
        static void Open() => GetWindow<NarrationDesignWindow>("Outil narration");

        static readonly string[] Tabs = { "Dialogues", "Textes" };
        int _tab;
        Vector2 _scroll, _nodesScroll;

        // ---- Dialogues
        List<ZoneAsset> _zones = new List<ZoneAsset>();
        int _zoneIndex;
        int _nodeIndex = -1;
        SerializedObject? _so;

        // ---- Textes
        string _search = "";
        int _category;
        string _newKey = "";
        static readonly (string Label, string[] Prefixes)[] Categories =
        {
            ("Tous", new string[0]),
            ("Tutoriel (étapes)", new[] { "tutorial.step.", "tutorial.progress", "tutorial.ok", "tutorial.skip" }),
            ("Astuces", new[] { "tutorial.hint." }),
            ("Première connexion", new[] { "intro.", "onboarding.", "campaign." }),
            ("Quêtes", new[] { "quest.", "quests." }),
            ("QG", new[] { "qg." }),
            ("Combat", new[] { "combat." }),
            ("Plateau", new[] { "board." }),
            ("Personnages", new[] { "speaker." }),
            ("Agents et compétences", new[] { "members.", "skills." }),
            ("Objets", new[] { "items." }),
        };

        void OnEnable()
        {
            DesignStore.EnsureLoaded();
            RefreshZones();
        }

        void RefreshZones()
        {
            _zones = AssetDatabase.FindAssets("t:ZoneAsset").Select(g => AssetDatabase.LoadAssetAtPath<ZoneAsset>(AssetDatabase.GUIDToAssetPath(g)))
                                  .Where(z => z != null).OrderBy(z => z.zoneId, System.StringComparer.Ordinal).ToList();
            _zoneIndex = Mathf.Clamp(_zoneIndex, 0, Mathf.Max(0, _zones.Count - 1));
            _so = null;
            _nodeIndex = -1;
        }

        void OnGUI()
        {
            DesignStore.EnsureLoaded();
            _tab = GUILayout.Toolbar(_tab, Tabs, GUILayout.Height(26));
            if (_tab == 0) DrawDialogues(); else DrawTexts();
        }

        // ------------------------------------------------------------------ Dialogues

        string[] SpeakerChoices()
        {
            var ids = DesignStore.Agents.agents.Select(a => a.id)
                .Concat(Loc.AllKeys().Where(k => k.StartsWith("speaker.", System.StringComparison.Ordinal)).Select(k => k.Substring(8)))
                .Concat(DesignStore.Texts.texts.Where(t => t.key.StartsWith("speaker.", System.StringComparison.Ordinal)).Select(t => t.key.Substring(8)))
                .Distinct().OrderBy(s => s, System.StringComparer.Ordinal).ToArray();
            return ids;
        }

        static string SpeakerName(string id)
        {
            var over = DesignStore.Texts.texts.FirstOrDefault(t => t.key == "speaker." + id);
            if (over != null) return over.text;
            if (Loc.Has("speaker." + id)) return Loc.T("speaker." + id);
            var agent = DesignStore.Agents.agents.FirstOrDefault(a => a.id == id);
            return agent != null ? agent.name : id;
        }

        void DrawDialogues()
        {
            if (_zones.Count == 0)
            {
                EditorGUILayout.HelpBox("Aucune zone trouvée. Importe les zones du prototype (Heroes Dawn ▸ Level Design).", MessageType.Info);
                if (GUILayout.Button("Actualiser")) RefreshZones();
                return;
            }
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            int zone = EditorGUILayout.Popup(_zoneIndex, _zones.Select(z => z.zoneId).ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(220));
            if (zone != _zoneIndex || _so == null) { _zoneIndex = zone; _so = new SerializedObject(_zones[_zoneIndex]); _nodeIndex = -1; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Enregistrer", EditorStyles.toolbarButton, GUILayout.Width(90))) { AssetDatabase.SaveAssets(); }
            EditorGUILayout.EndHorizontal();

            _so!.Update();
            var nodes = _so.FindProperty("nodes");
            EditorGUILayout.BeginHorizontal();
            DrawNodeList(nodes);
            EditorGUILayout.BeginVertical();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_nodeIndex < 0 || _nodeIndex >= nodes.arraySize) EditorGUILayout.HelpBox("Choisis une case à gauche pour voir ou créer son dialogue.", MessageType.Info);
            else DrawNodeDialogue(nodes.GetArrayElementAtIndex(_nodeIndex));
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            if (_so.ApplyModifiedProperties()) EditorUtility.SetDirty(_zones[_zoneIndex]);
        }

        void DrawNodeList(SerializedProperty nodes)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            GUILayout.Label("Cases (● = a un dialogue)", EditorStyles.miniLabel);
            _nodesScroll = EditorGUILayout.BeginScrollView(_nodesScroll, "box");
            for (int i = 0; i < nodes.arraySize; i++)
            {
                var n = nodes.GetArrayElementAtIndex(i);
                int before = n.FindPropertyRelative("narrationBefore").arraySize, after = n.FindPropertyRelative("narrationAfter").arraySize;
                string label = (before + after > 0 ? "● " : "   ") + n.FindPropertyRelative("id").stringValue + "  ·  " + n.FindPropertyRelative("type").enumDisplayNames[n.FindPropertyRelative("type").enumValueIndex]
                             + (before + after > 0 ? "  (" + before + " + " + after + ")" : "");
                var rect = EditorGUILayout.GetControlRect(false, 20);
                if (i == _nodeIndex) EditorGUI.DrawRect(rect, new Color(0.24f, 0.48f, 0.9f, 0.9f));
                if (GUI.Button(rect, label, i == _nodeIndex ? EditorStyles.whiteLabel : EditorStyles.label)) { _nodeIndex = i; GUI.FocusControl(null); }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawNodeDialogue(SerializedProperty node)
        {
            EditorGUILayout.LabelField("Case " + node.FindPropertyRelative("id").stringValue, EditorStyles.boldLabel);
            var speakers = SpeakerChoices();
            DrawLines("Avant l'action de la case (à l'arrivée du pion)", node.FindPropertyRelative("narrationBefore"), speakers);
            EditorGUILayout.Space(10);
            DrawLines("Après la résolution de la case", node.FindPropertyRelative("narrationAfter"), speakers);
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Supprimer tout le dialogue de cette case") &&
                EditorUtility.DisplayDialog("Supprimer le dialogue", "Toutes les répliques de cette case seront supprimées.", "Supprimer", "Annuler"))
            {
                node.FindPropertyRelative("narrationBefore").ClearArray();
                node.FindPropertyRelative("narrationAfter").ClearArray();
            }
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Aperçu", EditorStyles.boldLabel);
            var sb = new System.Text.StringBuilder();
            foreach (var section in new[] { "narrationBefore", "narrationAfter" })
            {
                var list = node.FindPropertyRelative(section);
                for (int i = 0; i < list.arraySize; i++)
                    sb.AppendLine(SpeakerName(list.GetArrayElementAtIndex(i).FindPropertyRelative("speaker").stringValue) + " : " + list.GetArrayElementAtIndex(i).FindPropertyRelative("text").stringValue);
                if (list.arraySize > 0 && section == "narrationBefore") sb.AppendLine("— action de la case —");
            }
            EditorGUILayout.HelpBox(sb.Length == 0 ? "aucun dialogue" : sb.ToString().TrimEnd(), MessageType.None);
        }

        void DrawLines(string title, SerializedProperty list, string[] speakers)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            int remove = -1, up = -1, down = -1;
            for (int i = 0; i < list.arraySize; i++)
            {
                var line = list.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                var speaker = line.FindPropertyRelative("speaker");
                var options = new List<string>(speakers);
                int index = options.IndexOf(speaker.stringValue);
                if (index < 0) { options.Insert(0, speaker.stringValue.Length == 0 ? "(choisir)" : speaker.stringValue + " (inconnu)"); index = 0; }
                var shown = options.Select(s => System.Array.IndexOf(speakers, s) >= 0 ? SpeakerName(s) + "  (" + s + ")" : s).ToArray();
                int pick = EditorGUILayout.Popup(index, shown);
                if (pick != index) speaker.stringValue = options[pick];
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("▲", GUILayout.Width(26))) up = i;
                if (GUILayout.Button("▼", GUILayout.Width(26))) down = i;
                if (GUILayout.Button("✕", GUILayout.Width(26))) remove = i;
                EditorGUILayout.EndHorizontal();
                var text = line.FindPropertyRelative("text");
                text.stringValue = EditorGUILayout.TextArea(text.stringValue, GUILayout.MinHeight(42));
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("+ Réplique"))
            {
                list.arraySize++;
                var added = list.GetArrayElementAtIndex(list.arraySize - 1);
                added.FindPropertyRelative("speaker").stringValue = speakers.Length > 0 ? speakers[0] : "";
                added.FindPropertyRelative("text").stringValue = "";
            }
            if (remove >= 0) list.DeleteArrayElementAtIndex(remove);
            else if (up > 0) list.MoveArrayElement(up, up - 1);
            else if (down >= 0 && down < list.arraySize - 1) list.MoveArrayElement(down, down + 1);
        }

        // ------------------------------------------------------------------ Textes

        static readonly Regex Placeholder = new Regex(@"\{(\d+)\}");

        static TextRecord? Override(string key) => DesignStore.Texts.texts.FirstOrDefault(t => t.key == key);

        static string OriginalOf(string key) => Loc.Has(key) ? Loc.T(key) : "";

        IReadOnlyList<string> TextProblems()
        {
            var e = new List<string>();
            foreach (var g in DesignStore.Texts.texts.GroupBy(t => t.key).Where(g => g.Count() > 1)) e.Add("clé « " + g.Key + " » en double");
            foreach (var t in DesignStore.Texts.texts)
            {
                if (t.key.Length == 0 || t.key.Any(char.IsWhiteSpace)) { e.Add("clé « " + t.key + " » invalide (vide ou avec espaces)"); continue; }
                var wanted = Placeholder.Matches(OriginalOf(t.key)).Cast<Match>().Select(m => m.Value).Distinct();
                foreach (var p in wanted) if (!t.text.Contains(p)) e.Add("« " + t.key + " » doit contenir " + p + " (valeur insérée par le jeu)");
            }
            return e;
        }

        void DrawTexts()
        {
            var problems = TextProblems();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _category = EditorGUILayout.Popup(_category, Categories.Select(c => c.Label).ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(190));
            _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
            GUILayout.Label(DesignStore.Texts.texts.Count + " modifié(s)", EditorStyles.miniLabel, GUILayout.Width(90));
            using (new EditorGUI.DisabledScope(!DesignStore.TextsDirty || problems.Count > 0))
                if (GUILayout.Button("Enregistrer", EditorStyles.toolbarButton, GUILayout.Width(90))) DesignStore.SaveTexts();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _newKey = EditorGUILayout.TextField(new GUIContent("Nouvelle clé", "Ex. speaker.mon_perso pour un nouveau personnage de dialogue."), _newKey);
            using (new EditorGUI.DisabledScope(_newKey.Trim().Length == 0 || Override(_newKey.Trim()) != null))
                if (GUILayout.Button("+ Ajouter", GUILayout.Width(80)))
                {
                    DesignStore.Texts.texts.Add(new TextRecord { key = _newKey.Trim(), text = OriginalOf(_newKey.Trim()) });
                    DesignStore.TextsDirty = true;
                    _newKey = "";
                }
            EditorGUILayout.EndHorizontal();

            var keys = Loc.AllKeys().Concat(DesignStore.Texts.texts.Select(t => t.key)).Distinct()
                          .Where(MatchesCategory).Where(MatchesSearch).OrderBy(k => k, System.StringComparer.Ordinal).ToList();
            GUILayout.Label(keys.Count + " texte(s)" + (keys.Count > 150 ? " (150 affichés : affine la recherche)" : ""), EditorStyles.miniLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var key in keys.Take(150)) DrawTextRow(key);
            EditorGUILayout.EndScrollView();
            if (problems.Count > 0) EditorGUILayout.HelpBox(string.Join("\n", problems.Take(6)), MessageType.Error);
        }

        bool MatchesCategory(string key)
        {
            var prefixes = Categories[_category].Prefixes;
            return prefixes.Length == 0 || prefixes.Any(p => key.StartsWith(p, System.StringComparison.Ordinal));
        }

        bool MatchesSearch(string key)
            => _search.Length == 0 || key.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0
               || (Override(key)?.text ?? OriginalOf(key)).IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0;

        void DrawTextRow(string key)
        {
            var over = Override(key);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(key + (over != null ? "   ● modifié" : ""), EditorStyles.miniBoldLabel);
            if (over != null && GUILayout.Button("Revenir à l'original", GUILayout.Width(130)))
            {
                DesignStore.Texts.texts.Remove(over);
                DesignStore.TextsDirty = true;
                GUI.FocusControl(null);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();
            string shown = over?.text ?? OriginalOf(key);
            EditorGUI.BeginChangeCheck();
            string edited = EditorGUILayout.TextArea(shown, GUILayout.MinHeight(30));
            if (EditorGUI.EndChangeCheck() && edited != shown)
            {
                if (over == null) { over = new TextRecord { key = key }; DesignStore.Texts.texts.Add(over); }
                over.text = edited;
                if (edited == OriginalOf(key) && Loc.Has(key)) DesignStore.Texts.texts.Remove(over);
                DesignStore.TextsDirty = true;
            }
            EditorGUILayout.EndVertical();
        }
    }
}
