#nullable enable
using System;
using System.Linq;
using N29.HeroesDawn.Authoring;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.LevelDesign
{
    public sealed partial class LevelDesignWindow
    {
        CampaignAsset? _campaign;
        Vector2 _campaignScroll;

        /// <summary>Onglet Campagne : ordre des zones, actes, codes (1.1, 1.2, 2.1…), prérequis et position sur la carte de la ville.</summary>
        void DrawCampaign()
        {
            _campaign ??= AssetDatabase.LoadAssetAtPath<CampaignAsset>(ZoneBaker.Folder + "/Campaign.asset");
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _campaign = (CampaignAsset?)EditorGUILayout.ObjectField("Campagne", _campaign, typeof(CampaignAsset), false);
            if (GUILayout.Button("Nouvelle campagne", GUILayout.Width(140))) _campaign = ZoneBaker.NewCampaign();
            if (GUILayout.Button("Importer le prototype", GUILayout.Width(150))) _campaign = ZoneBaker.ImportPrototype();
            EditorGUILayout.EndHorizontal();
            if (_campaign == null)
            {
                EditorGUILayout.HelpBox("Crée une campagne (ou importe celle du prototype) : elle ordonne les plateaux jouables et leur donne leurs codes (acte.rang).", MessageType.Info);
                return;
            }
            var c = _campaign;
            _campaignScroll = EditorGUILayout.BeginScrollView(_campaignScroll);
            for (int i = 0; i < c.entries.Count; i++)
            {
                var e = c.entries[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(c.CodeOf(i), EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUI.BeginChangeCheck();
                var zone = (ZoneAsset?)EditorGUILayout.ObjectField(e.zone, typeof(ZoneAsset), false);
                int act = EditorGUILayout.IntField("Acte", e.act, GUILayout.Width(120));
                if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(c, "Campagne"); e.zone = zone; e.act = Mathf.Clamp(act, 1, 9); EditorUtility.SetDirty(c); }
                if (GUILayout.Button("↑", GUILayout.Width(26)) && LevelEditorModel.MoveCampaignEntry(c, i, -1)) { EditorUtility.SetDirty(c); GUIUtility.ExitGUI(); }
                if (GUILayout.Button("↓", GUILayout.Width(26)) && LevelEditorModel.MoveCampaignEntry(c, i, 1)) { EditorUtility.SetDirty(c); GUIUtility.ExitGUI(); }
                if (GUILayout.Button("Ouvrir", GUILayout.Width(52)) && e.zone != null) { Open(e.zone); GUIUtility.ExitGUI(); }
                if (GUILayout.Button("✕", GUILayout.Width(26))) { Undo.RecordObject(c, "Campagne"); c.entries.RemoveAt(i); EditorUtility.SetDirty(c); GUIUtility.ExitGUI(); }
                EditorGUILayout.EndHorizontal();
                EditorGUI.BeginChangeCheck();
                string code = EditorGUILayout.TextField("Code (vide : " + c.CodeOf(i) + ")", e.codeOverride);
                string req = EditorGUILayout.TextField("Prérequis (ids de zone, virgule)", string.Join(",", e.requires));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(c, "Campagne");
                    e.codeOverride = code;
                    e.requires = req.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                    EditorUtility.SetDirty(c);
                }
                if (e.zone != null)
                {
                    EditorGUI.BeginChangeCheck();
                    bool has = EditorGUILayout.Toggle("Point sur la carte de la ville", e.zone.hasMapPoint);
                    Vector2Int p = e.zone.mapPointPermille;
                    if (has) p = new Vector2Int(EditorGUILayout.IntSlider("  X (‰)", p.x, 0, 1000), EditorGUILayout.IntSlider("  Y (‰)", p.y, 0, 1000));
                    if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(e.zone, "Point carte"); e.zone.hasMapPoint = has; e.zone.mapPointPermille = p; EditorUtility.SetDirty(e.zone); }
                    var issues = LevelEditorModel.Validate(e.zone, Catalog);
                    int errors = issues.Count(x => x.Severity == N29.HeroesDawn.Domain.Board.IssueSeverity.Error);
                    EditorGUILayout.LabelField(errors == 0 ? "Plateau jouable (" + (issues.Count) + " avertissement(s))" : errors + " erreur(s) de validation", errors == 0 ? EditorStyles.miniLabel : EditorStyles.boldLabel);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Nouvelle zone dans la campagne"))
            {
                int act = c.entries.Count > 0 ? c.entries[c.entries.Count - 1].act : 1;
                int n = c.entries.Count + 1;
                var z = ZoneBaker.NewZone("zone_c" + n);
                Undo.RecordObject(c, "Campagne");
                LevelEditorModel.AddCampaignZone(c, z, act);
                EditorUtility.SetDirty(c);
            }
            var free = AssetDatabase.FindAssets("t:ZoneAsset", new[] { ZoneBaker.Folder }).Select(g => AssetDatabase.LoadAssetAtPath<ZoneAsset>(AssetDatabase.GUIDToAssetPath(g)))
                                    .Where(z => z != null && c.entries.All(e => e.zone != z)).ToList();
            EditorGUI.BeginDisabledGroup(free.Count == 0);
            if (GUILayout.Button("+ Ajouter une zone existante (" + free.Count + ")"))
            {
                var menu = new GenericMenu();
                foreach (var z in free)
                {
                    var zone = z;
                    menu.AddItem(new GUIContent(zone.zoneId), false, () => { Undo.RecordObject(c, "Campagne"); LevelEditorModel.AddCampaignZone(c, zone, 1); EditorUtility.SetDirty(c); });
                }
                menu.ShowAsContext();
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Chaîner les prérequis")) { Undo.RecordObject(c, "Prérequis"); LevelEditorModel.ChainRequirements(c); EditorUtility.SetDirty(c); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }
    }
}
