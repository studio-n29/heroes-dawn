#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Balance
{
    /// <summary>
    /// Rapport d'équilibrage complet (phase 4 du plan de stabilisation) : toutes les zones, plusieurs niveaux d'équipe, l'équipe de départ réelle
    /// et l'équipe d'essai. Le contenu est construit comme dans le jeu (zones et agents des outils de design). Les cartes équipées et les objets ne
    /// sont pas simulés : le taux est celui de l'équipe nue, un plancher.
    /// Batchmode : -executeMethod N29.HeroesDawn.EditorTools.Balance.BalanceFullReport.Run -quit (ou BossTuning).
    /// </summary>
    public static class BalanceFullReport
    {
        static readonly int[] Levels = { 3, 5, 8, 10, 12, 15 };

        static readonly (string Name, string[] Ids)[] Teams =
        {
            ("depart", new[] { "giulia_rossi", "ilona_varga", "lucas_mendes" }),
            ("essai", new[] { "darius_brooks", "mei_lin_chen", "freya_lindqvist", "kwame_abara" }),
        };

        public static ContentCatalog BuildGameContent()
        {
            var content = BalanceSimulatorWindow.LoadContent(BalanceImportMenu.ClientJsonPath);
            content = content.WithZones(N29.HeroesDawn.Authoring.ZoneLibrary.Load().Zones);
            var data = N29.HeroesDawn.Authoring.ContentDataLibrary.Load();
            if (data.Items != null) content = content.WithItems(data.Items);
            if (data.Members != null) content = content.WithMembers(data.Members);
            return content;
        }

        static string Write(string prefix, string text)
        {
            var dir = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../BalanceReports"));
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            File.WriteAllText(file, text);
            return file;
        }

        public static void Run()
        {
            var content = BuildGameContent();
            var inv = CultureInfo.InvariantCulture;
            var csv = new StringBuilder("zone;encounter;rank;enemies;team;level;runs;win_rate_pct;timeouts;avg_seconds_on_win\n");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var known = new HashSet<string>(content.Members.Select(m => m.Id.Value));
            foreach (var zone in content.Zones)
            foreach (var (teamName, ids) in Teams)
            {
                var team = ids.Where(known.Contains).Select(id => new MemberId(id)).ToArray();
                if (team.Length == 0) { Debug.LogWarning("[Heroes Dawn] Équipe « " + teamName + " » : aucun agent connu"); continue; }
                foreach (int level in Levels)
                {
                    var req = new SimulationRequest { Team = team, TeamLevel = level, RunsPerEncounter = 200, BaseSeed = 42 };
                    foreach (var s in BalanceSimulation.Run(content, zone.Id, req))
                    {
                        var enc = zone.Encounters[s.EncounterIndex];
                        string enemies = string.Join("+", enc.Waves.SelectMany(w => w).Select(e => e.Value).Distinct());
                        int pct = s.Runs == 0 ? 0 : s.Wins * 100 / s.Runs;
                        csv.Append(zone.Id.Value).Append(';').Append(s.EncounterIndex).Append(';').Append(s.Rank).Append(';').Append(enemies).Append(';')
                           .Append(teamName).Append(';').Append(level).Append(';').Append(s.Runs).Append(';').Append(pct).Append(';').Append(s.Timeouts).Append(';')
                           .Append((s.AvgTicksOnWin * content.Balance.TickMs / 1000.0).ToString("0.0", inv)).Append('\n');
                    }
                }
            }
            Debug.Log($"[Heroes Dawn] Rapport d'équilibrage complet : {Write("rapport_complet", csv.ToString())} ({sw.ElapsedMilliseconds} ms)");
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(0);
        }

        /// <summary>Boss avec points de vie et attaque multipliés (les bonus de phase restent des pourcentages du boss d'origine).</summary>
        static EnemyDef Scaled(EnemyDef d, double hp, double atk)
            => new EnemyDef(d.Id, d.NameKey, d.Position, d.Family,
                new StatBlock((int)Math.Round(d.BaseStats.Hp * hp), (int)Math.Round(d.BaseStats.Atk * atk), d.BaseStats.Def, d.BaseStats.Spd),
                d.Skills, d.IsBoss, d.Phases, d.Style);

        /// <summary>
        /// Expérience de réglage des boss : grille de multiplicateurs (PV, attaque) sur l'équipe de départ et l'équipe d'essai, niveaux 5 et 10.
        /// N'écrit que le CSV : aucune valeur du jeu n'est modifiée.
        /// </summary>
        public static void BossTuning()
        {
            var baseContent = BuildGameContent();
            var inv = CultureInfo.InvariantCulture;
            var known = new HashSet<string>(baseContent.Members.Select(m => m.Id.Value));
            var csv = new StringBuilder("boss;zone;encounter;hp_x;atk_x;team;level;runs;win_rate_pct;avg_seconds_on_win\n");
            foreach (double hp in new[] { 1.0, 2.0, 3.0, 4.0, 6.0 })
            foreach (double atk in new[] { 2.0, 3.0, 4.0, 6.0, 8.0, 12.0 })
            {
                var content = baseContent.WithEnemies(baseContent.Enemies.Select(e => e.IsBoss ? Scaled(e, hp, atk) : e).ToList());
                foreach (var zone in content.Zones)
                {
                    var bossIdx = Enumerable.Range(0, zone.Encounters.Count)
                        .Where(i => zone.Encounters[i].Waves.SelectMany(w => w).Any(id => content.Enemy(id).IsBoss)).ToList();
                    if (bossIdx.Count == 0) continue;
                    foreach (var (teamName, ids) in Teams)
                    foreach (int level in new[] { 5, 10 })
                    {
                        var team = ids.Where(known.Contains).Select(id => new MemberId(id)).ToArray();
                        var stats = BalanceSimulation.Run(content, zone.Id, new SimulationRequest { Team = team, TeamLevel = level, RunsPerEncounter = 200, BaseSeed = 7 });
                        foreach (int ei in bossIdx)
                        {
                            var s = stats[ei];
                            var boss = zone.Encounters[ei].Waves.SelectMany(w => w).Select(id => content.Enemy(id)).First(e => e.IsBoss);
                            csv.Append(boss.Id.Value).Append(';').Append(zone.Id.Value).Append(';').Append(ei).Append(';').Append(hp.ToString("0.0", inv)).Append(';')
                               .Append(atk.ToString("0.0", inv)).Append(';').Append(teamName).Append(';').Append(level).Append(';').Append(s.Runs).Append(';')
                               .Append(s.Wins * 100 / Math.Max(1, s.Runs)).Append(';').Append((s.AvgTicksOnWin * content.Balance.TickMs / 1000.0).ToString("0.0", inv)).Append('\n');
                        }
                    }
                }
            }
            Debug.Log("[Heroes Dawn] Expérience de réglage des boss : " + Write("reglage_boss", csv.ToString()));
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(0);
        }
    }
}
