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


        /// <summary>Diagnostic : la rencontre de chaque boss, attaque ×1, équipe de départ, selon le profil (dont objets seuls et cartes seules), niveaux 5 et 10, 400 combats.</summary>
        public static void EquipmentDiagnostic()
        {
            var content = BuildGameContent();
            var team = Teams[0].Ids.Select(id => new MemberId(id)).ToArray();
            var sb = new StringBuilder();
            foreach (var zone in content.Zones)
            {
                var bossIdx = Enumerable.Range(0, zone.Encounters.Count).Where(i => zone.Encounters[i].Waves.SelectMany(w => w).Any(id => content.Enemy(id).IsBoss)).ToList();
                if (bossIdx.Count == 0) continue;
                foreach (var profile in new[] { EquipmentProfile.Bare, EquipmentProfile.ItemsOnly, EquipmentProfile.ItemsStrong })
                {
                    var stats = BalanceSimulation.Run(content, zone.Id, new SimulationRequest { Team = team, TeamLevel = 5, RunsPerEncounter = 400, BaseSeed = 11, Equipment = profile });
                    foreach (int ei in bossIdx) sb.Append($"DIAG {zone.Id.Value} enc{ei} {profile} niv5 : {stats[ei].Wins * 100 / stats[ei].Runs} % ({stats[ei].Timeouts} délais)\n");
                }
            }
            var used = new HashSet<string>();
            foreach (var m in team)
            {
                var e = Loadouts.Entry(content, m, 5, 0, EquipmentProfile.Average, used);
                sb.Append($"DIAG équipement moyen {m.Value} : objets [{string.Join(",", e.Items.Select(i => i.Value))}] cartes [{string.Join(",", e.Cards.Select(c => c?.Value))}]\n");
            }
            Debug.Log(sb.ToString());
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(0);
        }
        /// <summary>
        /// Réglage de Moscou (chapitre Sergueiv) : multiplicateurs d'attaque du Passeur et de l'homme de main, équipe de départ (profils nu, objets, objets forts), niveaux 5 et 10,
        /// toutes les rencontres des quatre zones de Moscou (les élites et les rencontres normales sont à attaque ×1). N'écrit que le CSV.
        /// Batchmode : -executeMethod N29.HeroesDawn.EditorTools.Balance.BalanceFullReport.MoscowTuning
        /// </summary>
        public static void MoscowTuning() => MoscowTuning(new[] { 0.5, 0.75, 1.0, 1.25, 1.5, 2.0, 3.0 }, new[] { 1.0 });

        /// <summary>Grille fine du Passeur (attaque ×1,75 à ×2,75).</summary>
        public static void MoscowBossTuning() => MoscowTuning(new[] { 1.75, 2.0, 2.25, 2.5, 2.75 }, new[] { 1.0 });

        /// <summary>Grille de l'homme de main (attaque ×1 à ×5), Passeur inchangé.</summary>
        public static void MoscowEliteTuning() => MoscowTuning(new[] { 1.0 }, new[] { 1.0, 1.5, 2.0, 3.0, 4.0, 5.0 });

        /// <summary>Grille libre : variables d'environnement MOS_PASSEUR et MOS_ENFORCER (listes séparées par des virgules, par exemple « 2,2.1 »). Absentes : ×1.</summary>
        public static void MoscowTuningFromEnvironment()
        {
            double[] Parse(string name) => (System.Environment.GetEnvironmentVariable(name) ?? "1").Split(',').Select(x => double.Parse(x.Trim(), CultureInfo.InvariantCulture)).ToArray();
            MoscowTuning(Parse("MOS_PASSEUR"), Parse("MOS_ENFORCER"));
        }

        public static void MoscowTuning(double[] passeurAtk, double[] enforcerAtk)
        {
            var baseContent = BuildGameContent();
            var inv = CultureInfo.InvariantCulture;
            var team = Teams[0].Ids.Where(id => baseContent.Members.Any(m => m.Id.Value == id)).Select(id => new MemberId(id)).ToArray();
            var csv = new StringBuilder("zone;encounter;rank;enemies;profile;passeur_atk_x;enforcer_atk_x;level;runs;win_rate_pct;timeouts;avg_seconds_on_win\n");
            foreach (double atk in passeurAtk)
            foreach (double eAtk in enforcerAtk)
            {
                var content = baseContent.WithEnemies(baseContent.Enemies.Select(e => e.Id.Value == "mos_passeur" ? Scaled(e, 1.0, atk) : e.Id.Value == "mos_enforcer" ? Scaled(e, 1.0, eAtk) : e).ToList());
                foreach (var zone in content.Zones.Where(z => z.Borough == "sergueiv"))
                foreach (var profile in new[] { EquipmentProfile.Bare, EquipmentProfile.ItemsOnly, EquipmentProfile.ItemsStrong })
                foreach (int level in new[] { 5, 10 })
                {
                    var stats = BalanceSimulation.Run(content, zone.Id, new SimulationRequest { Team = team, TeamLevel = level, RunsPerEncounter = 200, BaseSeed = 7, Equipment = profile });
                    foreach (var s in stats)
                    {
                        var enc = zone.Encounters[s.EncounterIndex];
                        bool hasPasseur = enc.Waves.SelectMany(w => w).Any(id => id.Value == "mos_passeur");
                        bool hasEnforcer = enc.Waves.SelectMany(w => w).Any(id => id.Value == "mos_enforcer");
                        // Une rencontre sans le Passeur (ou sans l'homme de main) ne dépend pas de son multiplicateur : une seule mesure.
                        if ((!hasPasseur && atk != passeurAtk[0]) || (!hasEnforcer && eAtk != enforcerAtk[0])) continue;
                        csv.Append(zone.Id.Value).Append(';').Append(s.EncounterIndex).Append(';').Append(s.Rank).Append(';')
                           .Append(string.Join("+", enc.Waves.SelectMany(w => w).Select(e => e.Value).Distinct())).Append(';').Append(profile).Append(';')
                           .Append(hasPasseur ? atk.ToString("0.00", inv) : "-").Append(';').Append(hasEnforcer ? eAtk.ToString("0.00", inv) : "-").Append(';').Append(level).Append(';').Append(s.Runs).Append(';')
                           .Append(s.Wins * 100 / Math.Max(1, s.Runs)).Append(';').Append(s.Timeouts).Append(';')
                           .Append((s.AvgTicksOnWin * content.Balance.TickMs / 1000.0).ToString("0.0", inv)).Append('\n');
                    }
                }
            }
            Debug.Log("[Heroes Dawn] Réglage de Moscou : " + Write("reglage_moscou", csv.ToString()));
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(0);
        }

        /// <summary>
        /// Grille de réglage des boss sur l'<b>équipe de départ équipée</b> (profils nu, léger, moyen, fort), multiplicateurs d'attaque de 1 à 12.
        /// N'écrit que le CSV : aucune valeur du jeu n'est modifiée.
        /// </summary>
        public static void BossTuningEquipped()
        {
            var baseContent = BuildGameContent();
            var inv = CultureInfo.InvariantCulture;
            var team = Teams[0].Ids.Where(id => baseContent.Members.Any(m => m.Id.Value == id)).Select(id => new MemberId(id)).ToArray();
            Debug.Log($"[Heroes Dawn] Collection : {baseContent.Collection.Items.Count} objets, {baseContent.Collection.Cards.Count} cartes ; équipe {team.Length} agents");
            var csv = new StringBuilder("boss;zone;encounter;profile;hp_x;atk_x;level;runs;win_rate_pct;avg_seconds_on_win\n");
            foreach (double hp in new[] { 1.0, 2.0 })
            foreach (double atk in new[] { 1.0, 2.0, 3.0, 4.0, 6.0, 8.0, 12.0, 16.0 })
            {
                var content = baseContent.WithEnemies(baseContent.Enemies.Select(e => e.IsBoss ? Scaled(e, hp, atk) : e).ToList());
                foreach (var zone in content.Zones)
                {
                    var bossIdx = Enumerable.Range(0, zone.Encounters.Count)
                        .Where(i => zone.Encounters[i].Waves.SelectMany(w => w).Any(id => content.Enemy(id).IsBoss)).ToList();
                    if (bossIdx.Count == 0) continue;
                    foreach (var profile in new[] { EquipmentProfile.Bare, EquipmentProfile.ItemsOnly, EquipmentProfile.ItemsStrong })
                    foreach (int level in new[] { 5, 10 })
                    {
                        var req = new SimulationRequest { Team = team, TeamLevel = level, RunsPerEncounter = 200, BaseSeed = 7, Equipment = profile };
                        var stats = BalanceSimulation.Run(content, zone.Id, req);
                        foreach (int ei in bossIdx)
                        {
                            var s = stats[ei];
                            var boss = zone.Encounters[ei].Waves.SelectMany(w => w).Select(id => content.Enemy(id)).First(e => e.IsBoss);
                            csv.Append(boss.Id.Value).Append(';').Append(zone.Id.Value).Append(';').Append(ei).Append(';').Append(profile).Append(';')
                               .Append(hp.ToString("0.0", inv)).Append(';').Append(atk.ToString("0.0", inv)).Append(';').Append(level).Append(';').Append(s.Runs).Append(';')
                               .Append(s.Wins * 100 / Math.Max(1, s.Runs)).Append(';').Append((s.AvgTicksOnWin * content.Balance.TickMs / 1000.0).ToString("0.0", inv)).Append('\n');
                        }
                    }
                }
            }
            Debug.Log("[Heroes Dawn] Réglage des boss, équipe équipée : " + Write("reglage_boss_equipe", csv.ToString()));
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(0);
        }
    }
}
