#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Une section du dossier : titre et texte.</summary>
    public readonly struct DossierSection
    {
        public readonly string Title;
        public readonly string Text;
        public DossierSection(string title, string text) { Title = title; Text = text; }
    }

    /// <summary>Ce que montre le panneau Dossier d'une fiche d'agent.</summary>
    public readonly struct DossierData
    {
        public readonly string Title;
        public readonly DossierSection[] Sections;
        public readonly string Close;
        public DossierData(string title, DossierSection[] sections, string close) { Title = title; Sections = sections; Close = close; }
    }

    /// <summary>Dossier de lore d'un agent : identité, lien à la Supernova, histoire d'origine, pouvoir, apparence (les sections sans texte sont omises).</summary>
    public static class DossierText
    {
        static readonly string[] IdentityFields = { "age", "sex", "origin" };
        static readonly string[] Sections_ = { "supernova", "story", "power", "look" };

        public static IReadOnlyList<DossierSection> Sections(ILocalization loc, string agentId)
        {
            var list = new List<DossierSection>();
            string Text(string field)
            {
                string key = "lore." + agentId + "." + field;
                return loc.Has(key) ? loc.T(key).Trim() : "";
            }
            string identity = string.Join(" · ", IdentityFields.Select(Text).Where(t => t.Length > 0));
            if (identity.Length > 0) list.Add(new DossierSection(loc.T("dossier.identity"), identity));
            foreach (var field in Sections_)
            {
                string text = Text(field);
                if (text.Length > 0) list.Add(new DossierSection(loc.T("dossier." + field), text));
            }
            return list;
        }
    }
}
