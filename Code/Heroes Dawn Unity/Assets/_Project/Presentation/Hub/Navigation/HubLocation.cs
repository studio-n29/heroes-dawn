#nullable enable
using System;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Une page du hub et ses arguments (membre de la fiche, fiche ouverte depuis la boutique).</summary>
    public readonly struct HubLocation : IEquatable<HubLocation>
    {
        public readonly HubPage Page;
        public readonly MemberId? Member;
        public readonly bool FromShop;

        public HubLocation(HubPage page, MemberId? member = null, bool fromShop = false)
        {
            Page = page; Member = member; FromShop = fromShop;
        }

        public bool Equals(HubLocation o) => Page == o.Page && Nullable.Equals(Member, o.Member) && FromShop == o.FromShop;
        public override bool Equals(object? obj) => obj is HubLocation o && Equals(o);
        public override int GetHashCode() => HashCode.Combine((int)Page, Member, FromShop);
        public override string ToString() => Member is MemberId m ? $"{Page}({m.Value})" : Page.ToString();
    }
}
