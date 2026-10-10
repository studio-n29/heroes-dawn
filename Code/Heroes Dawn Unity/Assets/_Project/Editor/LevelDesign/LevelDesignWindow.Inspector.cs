#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.LevelDesign
{
    public sealed partial class LevelDesignWindow
    {
        bool _foldZone = true, _foldNode = true, _foldObjectives = true, _foldEncounters, _foldRegions = true, _foldBackground = true, _foldIssues = true;

        /// <summary>Champ texte avec annulation et revalidation.</summary>
        string Text(string label, string value, int width = 0)
        {
            EditorGUI.BeginChangeCheck();
            var v = width > 0 ? EditorGUILayout.TextField(label, value, GUILayout.Width(width)) : EditorGUILayout.TextField(label, value);
            if (EditorGUI.EndChangeCheck()) { Touch("Modifier " + label); Changed(); return v; }
            return value;
        }

        int Int(string label, int value, int min = int.MinValue, int max = int.MaxValue)
        {
            EditorGUI.BeginChangeCheck();
            var v = Mathf.Clamp(EditorGUILayout.IntField(label, value), min, max);
            if (EditorGUI.EndChangeCheck()) { Touch("Modifier " + label); Changed(); return v; }
            return value;
        }

        T EnumField<T>(string label, T value) where T : System.Enum
        {
            EditorGUI.BeginChangeCheck();
            var v = (T)EditorGUILayout.EnumPopup(label, value);
            if (EditorGUI.EndChangeCheck()) { Touch("Modifier " + label); Changed(); return v; }
            return value;
        }

        bool Toggle(string label, bool value)
        {
            EditorGUI.BeginChangeCheck();
            var v = EditorGUILayout.Toggle(label, value);
            if (EditorGUI.EndChangeCheck()) { Touch("Modifier " + label); Changed(); return v; }
            return value;
        }

        /// <summary>Choix dans une liste de textes (identifiants) avec une option vide.</summary>
        string Popup(string label, string value, IReadOnlyList<string> options)
        {
            int index = Mathf.Max(0, options.ToList().IndexOf(value));
            EditorGUI.BeginChangeCheck();
            int next = EditorGUILayout.Popup(label, index, options.ToArray());
            if (EditorGUI.EndChangeCheck()) { Touch("Modifier " + label); Changed(); return options[next]; }
            return value;
        }

        void DrawInspector()
        {
            var z = _zone!;
            _foldZone = EditorGUILayout.Foldout(_foldZone, "Plateau", true);
            if (_foldZone) DrawZoneFields(z);
            _foldNode = EditorGUILayout.Foldout(_foldNode, "Case sélectionnée", true);
            if (_foldNode) DrawNodeFields(z);
            if (_selectedLink != null && !z.links.Contains(_selectedLink)) _selectedLink = null;
            if (_selectedLink != null)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Chemin sélectionné", EditorStyles.boldLabel);
                DrawLinkFields(z, _selectedLink);
            }
            _foldObjectives = EditorGUILayout.Foldout(_foldObjectives, "Objectifs (" + z.objectives.Count + ")", true);
            if (_foldObjectives) DrawObjectives(z);
            _foldEncounters = EditorGUILayout.Foldout(_foldEncounters, "Rencontres de la zone (repli des combats sans table)", true);
            if (_foldEncounters) DrawEncounters(z);
            _foldRegions = EditorGUILayout.Foldout(_foldRegions, "Zones de biome peintes (" + z.regions.Count + ")", true);
            if (_foldRegions) DrawRegionsList(z);
            _foldBackground = EditorGUILayout.Foldout(_foldBackground, "Fond de carte", true);
            if (_foldBackground) DrawBackgroundFields(z);
            int errors = _issues.Count(i => i.Severity == IssueSeverity.Error), warns = _issues.Count - errors;
            _foldIssues = EditorGUILayout.Foldout(_foldIssues, "Validation : " + errors + " erreur(s), " + warns + " avertissement(s)", true);
            if (_foldIssues) DrawIssues();
        }

        void DrawZoneFields(ZoneAsset z)
        {
            EditorGUI.indentLevel++;
            z.zoneId = Text("Identifiant", z.zoneId);
            z.displayName = Text("Nom affiché", z.displayName);
            z.shortName = Text("Nom court", z.shortName);
            EditorGUILayout.LabelField("Description");
            EditorGUI.BeginChangeCheck();
            var d = EditorGUILayout.TextArea(z.description, GUILayout.Height(48));
            if (EditorGUI.EndChangeCheck()) { Touch("Description"); z.description = d; Changed(); }
            z.code = Text("Code (campagne : calculé)", z.code);
            z.act = Int("Acte", z.act, 1, 9);
            z.borough = Text("Quartier", z.borough);
            z.recommendedLevel = Int("Niveau conseillé", z.recommendedLevel, 1, 99);
            var req = Text("Prérequis (ids, virgule)", string.Join(",", z.requires));
            if (req != string.Join(",", z.requires)) z.requires = req.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            z.biome = EnumField("Biome de base", z.biome);
            z.weather = EnumField("Météo", z.weather);
            z.hasMapPoint = Toggle("Position sur la carte de la ville", z.hasMapPoint);
            if (z.hasMapPoint)
            {
                EditorGUI.BeginChangeCheck();
                var p = new Vector2Int(EditorGUILayout.IntSlider("  X (‰)", z.mapPointPermille.x, 0, 1000), EditorGUILayout.IntSlider("  Y (‰)", z.mapPointPermille.y, 0, 1000));
                if (EditorGUI.EndChangeCheck()) { Touch("Position carte"); z.mapPointPermille = p; Changed(); }
            }
            EditorGUI.indentLevel--;
        }

        void DrawLinkFields(ZoneAsset z, LinkData l)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Entre", l.a + " ↔ " + l.b);
            int cost = Int("Coût en PM", Mathf.Max(1, l.cost), 1, 9);
            if (cost != l.cost) { Touch("Coût du chemin"); l.cost = cost; Changed(); }
            if (!l.hasCurve)
            {
                if (GUILayout.Button("Personnaliser la courbe")) { Touch("Courbe du chemin"); LevelEditorModel.CustomizeCurve(z, l); Changed(); }
            }
            else
            {
                EditorGUILayout.HelpBox("Glisse les deux points jaunes sur la carte.", MessageType.None);
                if (GUILayout.Button("Revenir à la courbe générée")) { Touch("Courbe du chemin"); l.hasCurve = false; Changed(); }
            }
            EditorGUI.indentLevel--;
        }

        void DrawNodeFields(ZoneAsset z)
        {
            var n = _selected == null ? null : z.Node(_selected);
            if (n == null) { EditorGUILayout.HelpBox("Aucune case sélectionnée.", MessageType.None); return; }
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Identifiant", n.id);
            EditorGUI.BeginChangeCheck();
            var type = (CaseType)EditorGUILayout.EnumPopup("Type", n.type);
            if (EditorGUI.EndChangeCheck()) { Touch("Type de case"); LevelEditorModel.SetType(z, n, type); Changed(); }
            n.isFixed = Toggle("Case fixe", n.isFixed);
            int x = Int("X", n.x), y = Int("Y", n.y);
            if ((x != n.x || y != n.y) && !LevelEditorModel.MoveNode(z, n.id, x, y)) { /* cellule occupée : ignoré */ }
            if (n.type == CaseType.Combat)
            {
                n.rank = EnumField("Rang", n.rank);
                n.hasCombat = Toggle("Table d'ennemis propre à la case", n.hasCombat);
                if (n.hasCombat)
                {
                    EditorGUILayout.LabelField("Table d'ennemis");
                    var enemies = Catalog.Enemies.Select(e => e.Id.Value).ToList();
                    for (int i = 0; i < n.enemyTable.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var picked = Popup("", n.enemyTable[i], enemies.Contains(n.enemyTable[i]) ? enemies : enemies.Concat(new[] { n.enemyTable[i] }).ToList());
                        n.enemyTable[i] = picked;
                        if (GUILayout.Button("✕", GUILayout.Width(24))) { Touch("Retirer un ennemi"); n.enemyTable.RemoveAt(i); Changed(); EditorGUILayout.EndHorizontal(); break; }
                        EditorGUILayout.EndHorizontal();
                    }
                    if (GUILayout.Button("+ Ajouter un ennemi à la table")) { Touch("Ajouter un ennemi"); n.enemyTable.Add(enemies.Count > 0 ? enemies[0] : ""); Changed(); }
                    n.rounds = Int("Manches (vagues)", n.rounds, 1, 9);
                    n.enemiesMin = Int("Ennemis par manche (min)", n.enemiesMin, 1, 12);
                    n.enemiesMax = Int("Ennemis par manche (max)", n.enemiesMax, n.enemiesMin, 12);
                }
            }
            else if (n.type == CaseType.MiniGame)
            {
                var ids = new List<string> { "" }.Concat(Catalog.Activities.Select(a => a.Id.Value)).ToList();
                var shown = ids.Select(i => i == "" ? "(par défaut : réflexe)" : i).ToList();
                int index = Mathf.Max(0, ids.IndexOf(n.activity));
                EditorGUI.BeginChangeCheck();
                int next = EditorGUILayout.Popup("Activité", index, shown.ToArray());
                if (EditorGUI.EndChangeCheck()) { Touch("Activité de la case"); n.activity = ids[next]; Changed(); }
                var def = Catalog.Activities.FirstOrDefault(a => a.Id.Value == n.activity);
                if (def != null) EditorGUILayout.LabelField("Type : " + def.Kind + " · gains " + def.Credits + " Cr / " + def.Pr + " PR", EditorStyles.miniLabel);
            }
            if (GUILayout.Button("Supprimer cette case"))
            {
                Touch("Supprimer une case");
                LevelEditorModel.DeleteNode(z, n.id);
                _selected = null;
                Changed();
            }
            EditorGUI.indentLevel--;
        }

        void DrawObjectives(ZoneAsset z)
        {
            EditorGUI.indentLevel++;
            var targets = z.nodes.Select(n => n.id).ToList();
            for (int i = 0; i < z.objectives.Count; i++)
            {
                var o = z.objectives[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                o.id = Text("Id", o.id);
                if (GUILayout.Button("✕", GUILayout.Width(24))) { Touch("Retirer un objectif"); z.objectives.RemoveAt(i); Changed(); EditorGUILayout.EndHorizontal(); EditorGUILayout.EndVertical(); break; }
                EditorGUILayout.EndHorizontal();
                o.type = EnumField("Type", o.type);
                o.target = Popup("Case visée", o.target, targets.Count > 0 ? targets : new List<string> { "" });
                o.text = Text("Texte", o.text);
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("+ Ajouter un objectif")) { Touch("Ajouter un objectif"); LevelEditorModel.AddObjective(z); Changed(); }
            EditorGUI.indentLevel--;
        }

        void DrawEncounters(ZoneAsset z)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < z.encounters.Count; i++)
            {
                var enc = z.encounters[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                enc.rank = EnumField("Rang", enc.rank);
                if (GUILayout.Button("✕", GUILayout.Width(24))) { Touch("Retirer une rencontre"); z.encounters.RemoveAt(i); Changed(); EditorGUILayout.EndHorizontal(); EditorGUILayout.EndVertical(); break; }
                EditorGUILayout.EndHorizontal();
                for (int w = 0; w < enc.waves.Count; w++)
                {
                    var text = Text("Manche " + (w + 1) + " (ennemis, virgule)", string.Join(",", enc.waves[w].enemies));
                    if (text != string.Join(",", enc.waves[w].enemies)) enc.waves[w].enemies = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+ manche")) { Touch("Ajouter une manche"); enc.waves.Add(new WaveData()); Changed(); }
                if (enc.waves.Count > 1 && GUILayout.Button("− manche")) { Touch("Retirer une manche"); enc.waves.RemoveAt(enc.waves.Count - 1); Changed(); }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("+ Ajouter une rencontre")) { Touch("Ajouter une rencontre"); z.encounters.Add(new EncounterData()); Changed(); }
            EditorGUI.indentLevel--;
        }

        void DrawRegionsList(ZoneAsset z)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < z.regions.Count; i++)
            {
                var r = z.regions[i];
                EditorGUILayout.BeginHorizontal();
                r.biome = EnumField("", r.biome);
                EditorGUI.BeginChangeCheck();
                float x = EditorGUILayout.FloatField(r.x, GUILayout.Width(44)), y = EditorGUILayout.FloatField(r.y, GUILayout.Width(44)), rad = EditorGUILayout.FloatField(r.radius, GUILayout.Width(44));
                if (EditorGUI.EndChangeCheck()) { Touch("Zone de biome"); r.x = x; r.y = y; r.radius = Mathf.Max(0.25f, rad); Changed(); }
                if (GUILayout.Button("✕", GUILayout.Width(24))) { Touch("Retirer une zone"); z.regions.RemoveAt(i); Changed(); EditorGUILayout.EndHorizontal(); break; }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.LabelField("Outil « Zone » : glisse sur le canevas pour peindre un cercle de biome.", EditorStyles.miniLabel);
            EditorGUI.indentLevel--;
        }

        void DrawBackgroundFields(ZoneAsset z)
        {
            EditorGUI.indentLevel++;
            z.backgroundSeed = Int("Graine", z.backgroundSeed, 1, 999999);
            EditorGUILayout.LabelField(string.IsNullOrEmpty(z.backgroundKey) ? "Pas de fond (fond uni en jeu)" : "Fond : " + z.backgroundKey + " (" + (z.backgroundMaxX - z.backgroundMinX) * LevelEditorModel.PxPerCellX + " × " + (z.backgroundMaxY - z.backgroundMinY) * LevelEditorModel.PxPerCellY + " px)");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(string.IsNullOrEmpty(z.backgroundKey) ? "Générer le fond" : "Régénérer le fond"))
            {
                Touch("Générer le fond");
                ZoneBaker.BakeBackground(z);
                _bgLoadedFor = "?";
                Changed();
            }
            if (GUILayout.Button("Graine au hasard"))
            {
                Touch("Graine");
                z.backgroundSeed = UnityEngine.Random.Range(1, 999999);
                ZoneBaker.BakeBackground(z);
                _bgLoadedFor = "?";
                Changed();
            }
            if (!string.IsNullOrEmpty(z.backgroundKey) && GUILayout.Button("Supprimer"))
            {
                Touch("Supprimer le fond");
                ZoneBaker.RemoveBackground(z);
                _bgLoadedFor = "?";
                Changed();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        void DrawIssues()
        {
            if (_issues.Count == 0) { EditorGUILayout.HelpBox("Plateau valide.", MessageType.Info); return; }
            foreach (var i in _issues)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(i.Message, i.Severity == IssueSeverity.Error ? MessageType.Error : MessageType.Warning);
                if (i.Case.HasValue && GUILayout.Button("→", GUILayout.Width(24), GUILayout.Height(32))) _selected = i.Case.Value.Value;
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
