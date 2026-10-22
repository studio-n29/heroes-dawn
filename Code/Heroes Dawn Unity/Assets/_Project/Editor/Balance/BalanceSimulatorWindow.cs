#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Balance
{
    /// <summary>Menu Heroes Dawn ▸ Équilibrage ▸ Simulateur : taux de victoire par rencontre de la zone 1.1, export CSV.</summary>
    public sealed class BalanceSimulatorWindow : EditorWindow
    {
        static readonly string[] DefaultTeam = { "darius_brooks", "mei_lin_chen", "freya_lindqvist", "kwame_abara" };

        ContentCatalog _content = null!;
        string[] _memberIds = Array.Empty<string>();
        readonly int[] _team = new int[4];
        int _level = 5, _fatigue, _runs = 1000;
        long _seed = 42;
        IReadOnlyList<EncounterStats>? _results;
        Vector2 _scroll;

        [MenuItem("Heroes Dawn/Équilibrage/Simulateur")]
        static void Open() => GetWindow<BalanceSimulatorWindow>("Simulateur d'équilibrage");

        void OnEnable()
        {
            _content = LoadContent(BalanceImportMenu.ClientJsonPath);
            _memberIds = _content.Members.Select(m => m.Id.Value).ToArray();
            for (int i = 0; i < _team.Length; i++) _team[i] = Math.Max(0, Array.IndexOf(_memberIds, DefaultTeam[i]));
        }

        /// <summary>Contenu du prototype avec l'équilibrage importé (balance.json) s'il existe.</summary>
        public static ContentCatalog LoadContent(string path)
        {
            if (!File.Exists(path)) return PrototypeContent.Build();
            var parsed = BalanceJson.Deserialize(File.ReadAllText(path));
            if (parsed.IsSuccess) return PrototypeContent.Build(parsed.Value);
            Debug.LogWarning("[Heroes Dawn] balance.json illisible : valeurs par défaut");
            return PrototypeContent.Build();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Équipe (mode auto, zone 1.1)", EditorStyles.boldLabel);
            for (int i = 0; i < _team.Length; i++) _team[i] = EditorGUILayout.Popup($"Membre {i + 1}", _team[i], _memberIds);
            _level = EditorGUILayout.IntSlider("Niveau", _level, 1, 50);
            _fatigue = EditorGUILayout.IntSlider("Fatigue (%)", _fatigue, 0, 100);
            _runs = Math.Max(1, EditorGUILayout.IntField("Combats par rencontre", _runs));
            _seed = EditorGUILayout.LongField("Graine", _seed);

            EditorGUILayout.Space();
            if (GUILayout.Button("Lancer", GUILayout.Height(30))) Simulate();

            if (_results == null) return;
            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Rencontre   Rang     Victoires   Défaites   Temps écoulé   Durée moy. (s)", EditorStyles.boldLabel);
            foreach (var s in _results)
            {
                int pct = s.Runs == 0 ? 0 : s.Wins * 100 / s.Runs;
                float secs = s.AvgTicksOnWin * _content.Balance.TickMs / 1000f;
                EditorGUILayout.LabelField($"{s.EncounterIndex,-11} {s.Rank,-8} {pct,3} %       {s.Losses,-10} {s.Timeouts,-14} {secs:0.0}");
            }
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Exporter CSV")) Export(_results, _content.Balance.TickMs);
        }

        void Simulate()
        {
            _content = LoadContent(BalanceImportMenu.ClientJsonPath); // relu à chaque lancement : prend en compte le dernier import
            var req = new SimulationRequest
            {
                Team = _team.Select(i => new MemberId(_memberIds[i])).ToArray(),
                TeamLevel = _level, FatiguePercent = _fatigue, RunsPerEncounter = _runs, BaseSeed = (ulong)_seed,
            };
            try
            {
                _results = BalanceSimulation.Run(_content, PrototypeContent.Zone11, req,
                    (e, n) => EditorUtility.DisplayProgressBar("Simulation", $"Rencontre {e + 1} / {n}", (float)e / n));
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        static string Export(IReadOnlyList<EncounterStats> results, int tickMs)
        {
            var dir = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../BalanceReports"));
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"simulation_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            File.WriteAllText(file, BalanceSimulation.ToCsv(results, tickMs));
            Debug.Log("[Heroes Dawn] Rapport de simulation : " + file);
            return file;
        }

        /// <summary>Batchmode : équipe par défaut, niveau 5, 1 000 combats par rencontre, export CSV.</summary>
        public static void RunDefaultAndExport()
        {
            var content = LoadContent(BalanceImportMenu.ClientJsonPath);
            var req = new SimulationRequest
            {
                Team = DefaultTeam.Select(id => new MemberId(id)).ToArray(), TeamLevel = 5, RunsPerEncounter = 1000, BaseSeed = 42,
            };
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var results = BalanceSimulation.Run(content, PrototypeContent.Zone11, req);
            Debug.Log($"[Heroes Dawn] Simulation terminée en {sw.ElapsedMilliseconds} ms\n" + BalanceSimulation.ToCsv(results, content.Balance.TickMs));
            Export(results, content.Balance.TickMs);
        }
    }
}
