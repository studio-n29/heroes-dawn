#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes des mini-jeux 2D du lot H (0.14.0) : fenêtre commune, puis un bloc par jeu.</summary>
    public static partial class Loc
    {
        static partial void ExtendMiniGames(Dictionary<string, string> d)
        {
            d["mg2d.start"] = "TOUCHE POUR COMMENCER";
            d["mg2d.abandon"] = "ABANDONNER";
            d["mg2d.time"] = "{0} s";
            d["mg2d.won"] = "<size=140%>RÉUSSI !</size>";
            d["mg2d.lost"] = "<size=140%>ÉCHEC…</size>";
        }
    }
}
