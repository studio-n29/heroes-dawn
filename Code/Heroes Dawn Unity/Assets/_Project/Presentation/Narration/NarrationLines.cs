#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Narration
{
    /// <summary>Traduit les répliques d'un événement de narration en lignes affichables (nom, portrait, côté, texte).</summary>
    public static class NarrationLines
    {
        /// <summary>Lignes d'un événement de narration pour un parcours (<paramref name="hero"/> : <c>agent_d</c> ou <c>agent_m</c>) : variantes .d / .m et répliques réservées appliquées.</summary>
        public static IReadOnlyList<DialogueBoxLine> Build(NarrationEvent e, ContentCatalog content, ILocalization loc, string? hero = null)
            => Build(e.Lines, content, loc, hero);

        public static IReadOnlyList<DialogueBoxLine> Build(IReadOnlyList<NarrationLineDef> lines, ContentCatalog content, ILocalization loc, string? hero = null)
            => NarrationLineDef.ForRoute(lines, hero).Select(l => Build(l, content, loc)).ToList();

        public static DialogueBoxLine Build(NarrationLineDef line, ContentCatalog content, ILocalization loc)
        {
            string text = loc.Has(line.TextKey) ? loc.T(line.TextKey) : line.TextKey.Substring(line.TextKey.LastIndexOf('.') + 1);
            var member = content.Members.FirstOrDefault(m => m.Id.Value == line.Speaker);
            if (member != null)
            {
                string name = loc.MemberName(member);
                return new DialogueBoxLine(name, new PortraitData(name, member.Family, art: AgentArt.For(member.Id.Value)), text, true, line.TextKey);
            }
            string key = "speaker." + line.Speaker;
            string npc = loc.Has(key) ? loc.T(key) : Capitalize(line.Speaker);
            return new DialogueBoxLine(npc, new PortraitData(npc, FamilyOf(line.Speaker), boss: false, art: AgentArt.For(line.Speaker)), text, false, line.TextKey, line.Speaker);
        }

        /// <summary>Couleur stable d'un personnage non joueur (selon son identifiant).</summary>
        static Family FamilyOf(string id)
        {
            int h = 17;
            foreach (char c in id) h = h * 31 + c;
            return (Family)(((h % 4) + 4) % 4);
        }

        static string Capitalize(string s) => string.IsNullOrEmpty(s) ? "?" : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
