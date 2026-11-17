#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Option d'un choix d'histoire : libellé du bouton et scène jouée si elle est retenue.</summary>
    public sealed class ChoiceOptionDef
    {
        public string Id { get; }
        public string LabelKey { get; }
        public IReadOnlyList<NarrationLineDef> Branch { get; }
        public ChoiceOptionDef(string id, string labelKey, IReadOnlyList<NarrationLineDef> branch) { Id = id; LabelKey = labelKey; Branch = branch; }
    }

    /// <summary>
    /// Choix d'histoire (un seul par profil, définitif) proposé quand le dialogue d'une case atteint <see cref="AfterKey"/> :
    /// <see cref="Prompt"/> est joué, puis le joueur retient une option, dont la branche est jouée ; le dialogue reprend ensuite.
    /// </summary>
    public sealed class ChoiceDef
    {
        public string Id { get; }
        public string ZoneId { get; }
        public string CaseId { get; }
        /// <summary>Clé de la réplique après laquelle le choix s insère (dernière réplique de la scène qui le précède) ; vide : le choix précède toutes les répliques de la case.</summary>
        public string AfterKey { get; }
        public IReadOnlyList<NarrationLineDef> Prompt { get; }
        public string TitleKey { get; }
        public IReadOnlyList<ChoiceOptionDef> Options { get; }

        public ChoiceDef(string id, string zoneId, string caseId, string afterKey, IReadOnlyList<NarrationLineDef> prompt, string titleKey, IReadOnlyList<ChoiceOptionDef> options)
        {
            Id = id; ZoneId = zoneId; CaseId = caseId; AfterKey = afterKey; Prompt = prompt; TitleKey = titleKey; Options = options;
        }

        public ChoiceOptionDef? Option(string id) => Options.FirstOrDefault(o => o.Id == id);
    }

    /// <summary>Épilogue d'un chapitre : une scène par option du choix, puis une scène commune ; joué après la dernière zone du chapitre.</summary>
    public sealed class EpilogueDef
    {
        public string ChapterId { get; }
        public string ChoiceId { get; }
        public IReadOnlyDictionary<string, IReadOnlyList<NarrationLineDef>> ByOption { get; }
        public IReadOnlyList<NarrationLineDef> Common { get; }

        public EpilogueDef(string chapterId, string choiceId, IReadOnlyDictionary<string, IReadOnlyList<NarrationLineDef>> byOption, IReadOnlyList<NarrationLineDef> common)
        {
            ChapterId = chapterId; ChoiceId = choiceId; ByOption = byOption; Common = common;
        }

        /// <summary>Scène de l'option retenue (vide si le choix n'a pas été fait), suivie de la scène commune.</summary>
        public IReadOnlyList<NarrationLineDef> For(string? option)
        {
            var list = new List<NarrationLineDef>();
            if (option != null && ByOption.TryGetValue(option, out var scene)) list.AddRange(scene);
            list.AddRange(Common);
            return list;
        }
    }

    /// <summary>Choix et épilogues de l'histoire (script narratif : Harlem 3.3, Place Rouge 4.4).</summary>
    public static class StoryChoices
    {
        static NarrationLineDef L(string speaker, string key, string? route = null, string? replaces = null) => new NarrationLineDef(speaker, key, route, replaces);

        public const string HarlemPriority = "harlem_priority";
        public const string PlaceRougeTrust = "place_rouge_trust";

        public static readonly IReadOnlyList<ChoiceDef> All = new[]
        {
            new ChoiceDef(HarlemPriority, "zone_ny_3_3", "n19", "",
                new[] { L("ilona_varga", "narration.ny_3_3.choice.0"), L("narrateur", "narration.ny_3_3.choice.1") },
                "choice.harlem_priority.title",
                new[]
                {
                    new ChoiceOptionDef("clinic", "choice.harlem_priority.clinic", new[]
                    {
                        L("agent_d", "narration.ny_3_3.branch_clinic.0", "agent_d"), L("agent_m", "narration.ny_3_3.branch_clinic.0.m", "agent_m", null),
                        L("commercant", "narration.ny_3_3.branch_clinic.1"), L("agent_d", "narration.ny_3_3.branch_clinic.2"),
                    }),
                    new ChoiceOptionDef("market", "choice.harlem_priority.market", new[]
                    {
                        L("agent_d", "narration.ny_3_3.branch_market.0", "agent_d"), L("agent_m", "narration.ny_3_3.branch_market.0.m", "agent_m", null),
                        L("dr_colby", "narration.ny_3_3.branch_market.1"), L("agent_d", "narration.ny_3_3.branch_market.2"),
                    }),
                }),
            new ChoiceDef(PlaceRougeTrust, "zone_mos_4_4", "n18", "narration.mos_4_4.boss.after.5",
                new[] { L("ilona_varga", "narration.mos_4_4.choice.0"), L("narrateur", "narration.mos_4_4.choice.1") },
                "choice.place_rouge_trust.title",
                new[]
                {
                    new ChoiceOptionDef("arrest", "choice.place_rouge_trust.arrest", new[] { L("agent_m", "narration.mos_4_4.branch_arrest.0"), L("conseiller", "narration.mos_4_4.branch_arrest.1") }),
                    new ChoiceOptionDef("listen", "choice.place_rouge_trust.listen", new[] { L("agent_m", "narration.mos_4_4.branch_listen.0"), L("conseiller", "narration.mos_4_4.branch_listen.1") }),
                }),
        };

        public static readonly IReadOnlyList<EpilogueDef> Epilogues = new[]
        {
            new EpilogueDef("ny_harlem", HarlemPriority,
                new Dictionary<string, IReadOnlyList<NarrationLineDef>>
                {
                    ["clinic"] = new[] { L("narrateur", "narration.ny_3.epilogue.clinic.0"), L("commercant", "narration.ny_3.epilogue.clinic.1"), L("dr_colby", "narration.ny_3.epilogue.clinic.2") },
                    ["market"] = new[] { L("narrateur", "narration.ny_3.epilogue.market.0"), L("dr_colby", "narration.ny_3.epilogue.market.1"), L("commercant", "narration.ny_3.epilogue.market.2") },
                },
                new[] { L("ilona_varga", "narration.ny_3.epilogue.common.0"), L("giulia_rossi", "narration.ny_3.epilogue.common.1") }),
            new EpilogueDef("mos_place_rouge", PlaceRougeTrust,
                new Dictionary<string, IReadOnlyList<NarrationLineDef>>
                {
                    ["arrest"] = new[] { L("narrateur", "narration.mos_4.epilogue.arrest.0"), L("ilona_varga", "narration.mos_4.epilogue.arrest.1") },
                    ["listen"] = new[] { L("narrateur", "narration.mos_4.epilogue.listen.0"), L("ilona_varga", "narration.mos_4.epilogue.listen.1") },
                },
                new[] { L("giulia_rossi", "narration.mos_4.epilogue.common.0"), L("ilona_varga", "narration.mos_4.epilogue.common.1") }),
        };

        /// <summary>Choix proposé sur cette case de cette zone, s'il y en a un.</summary>
        public static ChoiceDef? At(string zoneId, string caseId) => All.FirstOrDefault(c => c.ZoneId == zoneId && c.CaseId == caseId);

        public static ChoiceDef? Find(string id) => All.FirstOrDefault(c => c.Id == id);

        public static EpilogueDef? EpilogueOf(string chapterId) => Epilogues.FirstOrDefault(e => e.ChapterId == chapterId);
    }
}
