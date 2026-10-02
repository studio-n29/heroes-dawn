using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.EditorTools.Balance
{
    /// <summary>Onglet « Paramètres » du tableur (colonne B = libellé, C = valeur) → <see cref="BalanceConfig"/>.</summary>
    public static class BalanceSheetMapper
    {
        sealed class Entry
        {
            public readonly string Label;
            public readonly bool Percent;
            public readonly Action<BalanceConfig, int> Set;
            public readonly Func<BalanceConfig, int> Get;
            public Entry(string label, bool percent, Action<BalanceConfig, int> set, Func<BalanceConfig, int> get)
            { Label = label; Percent = percent; Set = set; Get = get; }
        }

        static readonly Entry[] Table =
        {
            new Entry("Taille de la jauge ATB", false, (b, v) => b.AtbGaugePoints = v, b => b.AtbGaugePoints),
            new Entry("Remplissage par point de Vitesse", false, (b, v) => b.AtbFactorPerSecond = v, b => b.AtbFactorPerSecond),
            new Entry("Écart de niveau — rang Facile", false, (b, v) => b.RankEasyLevelOffset = v, b => b.RankEasyLevelOffset),
            new Entry("Écart de niveau — rang Élite", false, (b, v) => b.RankEliteLevelOffset = v, b => b.RankEliteLevelOffset),
            new Entry("Constante de défense (K)", false, (b, v) => b.DefK = v, b => b.DefK),
            new Entry("Chance de critique de base", true, (b, v) => b.CritBasePercent = v, b => b.CritBasePercent),
            new Entry("Multiplicateur de critique", true, (b, v) => b.CritMultiplierPercent = v, b => b.CritMultiplierPercent),
            new Entry("Stats ennemis — rang Facile", true, (b, v) => b.RankEasyStatPercent = v, b => b.RankEasyStatPercent),
            new Entry("Stats ennemis — rang Élite", true, (b, v) => b.RankEliteStatPercent = v, b => b.RankEliteStatPercent),
            new Entry("Fatigue par combat (victoire)", true, (b, v) => b.FatiguePerWin = v, b => b.FatiguePerWin),
            new Entry("Fatigue supplémentaire en défaite", true, (b, v) => b.FatiguePerLossExtra = v, b => b.FatiguePerLossExtra),
            new Entry("Seuil d'apparition du malus", true, (b, v) => b.FatigueMalusThreshold = v, b => b.FatigueMalusThreshold),
            new Entry("Malus de stats à 100 % de fatigue", true, (b, v) => b.FatigueMalusMaxPercent = v, b => b.FatigueMalusMaxPercent),
        };

        /// <summary>Nombre de paramètres mappés (13 au M1).</summary>
        public static int MappedCount => Table.Length;

        public static BalanceConfig Map(IReadOnlyList<string[]> rows, List<string> warnings)
        {
            var config = new BalanceConfig();
            foreach (var e in Table)
            {
                var row = rows.FirstOrDefault(r => r.Length > 2 && r[1].Trim() == e.Label);
                if (row == null)
                {
                    warnings.Add($"Paramètre absent du tableur : {e.Label} (défaut {e.Get(config)} conservé)");
                    continue;
                }
                if (!double.TryParse(row[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var raw))
                {
                    warnings.Add($"Valeur illisible pour {e.Label} : '{row[2]}'");
                    continue;
                }
                e.Set(config, (int)Math.Round(e.Percent ? raw * 100 : raw, MidpointRounding.AwayFromZero));
            }
            return config;
        }
    }
}
