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
        public static IReadOnlyList<DialogueBoxLine> Build(NarrationEvent e, ContentCatalog content, ILocalization loc)
            => e.Lines.Select(l => Build(l, content, loc)).ToList();

        public static DialogueBoxLine Build(NarrationLineDef line, ContentCatalog content, ILocalization loc)
        {
            string text = loc.Has(line.TextKey) ? loc.T(line.TextKey) : line.TextKey.Substring(line.TextKey.LastIndexOf('.') + 1);
            var member = content.Members.FirstOrDefault(m => m.Id.Value == line.Speaker);
            if (member != null)
            {
                string name = loc.MemberName(member);
                return new DialogueBoxLine(name, new PortraitData(name, member.Family), text, true);
            }
            string key = "speaker." + line.Speaker;
            string npc = loc.Has(key) ? loc.T(key) : Capitalize(line.Speaker);
            return new DialogueBoxLine(npc, new PortraitData(npc, FamilyOf(line.Speaker), boss: false), text, false);
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
