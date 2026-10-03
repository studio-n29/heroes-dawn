#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs
{
    /// <summary>
    /// Fenêtre du générateur : les prefabs absents sont toujours créés ; ceux qui existent déjà (et leurs retouches)
    /// ne sont écrasés que s'ils sont cochés — tous décochés par défaut.
    /// </summary>
    public sealed class PrefabGeneratorWindow : EditorWindow
    {
        readonly HashSet<string> _overwrite = new HashSet<string>();
        Vector2 _scroll;

        public static void Open() => GetWindow<PrefabGeneratorWindow>(true, "Générer les prefabs d'écran");

        void OnGUI()
        {
            EditorGUILayout.HelpBox("Les prefabs absents seront créés. Cocher un prefab existant l'écrase : ses retouches manuelles seront perdues.", MessageType.Info);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var r in PrefabGenerator.Recipes)
            {
                bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabGenerator.DefaultRoot + "/" + r.RelativePath) != null;
                if (!exists) { EditorGUILayout.LabelField(r.RelativePath, "à créer"); continue; }
                bool on = EditorGUILayout.ToggleLeft(r.RelativePath + "  (existe : écraser ?)", _overwrite.Contains(r.RelativePath));
                if (on) _overwrite.Add(r.RelativePath); else _overwrite.Remove(r.RelativePath);
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Générer"))
            {
                var report = PrefabGenerator.Generate(PrefabGenerator.DefaultRoot, OverwritePolicy.Selected, _overwrite);
                Debug.Log("[Heroes Dawn] Prefabs : " + report);
                EditorUtility.DisplayDialog("Prefabs d'écran", report.ToString(), "OK");
                _overwrite.Clear();
            }
        }
    }
}
