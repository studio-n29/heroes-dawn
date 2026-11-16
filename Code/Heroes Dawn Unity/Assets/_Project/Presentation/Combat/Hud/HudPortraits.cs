#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat.Hud
{
    /// <summary>
    /// Portraits cadrés sur le visage (agents) et illustrations d'ennemis, posés par une classe USS (« hd-head--id », « hd-foe--id » :
    /// les images sont référencées dans hd-combat.uss). L'agent est reconnu par le nom de son illustration, à défaut par son nom affiché ;
    /// l'ennemi par son nom localisé. Sans correspondance, l'appelant garde son substitut.
    /// </summary>
    public static class HudPortraits
    {
        const string HeadPrefix = "hd-head--", FoePrefix = "hd-foe--";

        static readonly HashSet<string> Heads = new HashSet<string>
        {
            "ada_okonkwo", "agent_d", "agent_m", "amara_hughes", "anna_petrov", "astrid_hallstrom", "brie_rayner", "celeste_moreau",
            "clement_lefevre", "darius_brooks", "eero_nieminen", "fatou_diallo", "freya_lindqvist", "giulia_rossi", "haruto_nakamura",
            "ilona_varga", "ji_hoon_park", "kwame_abara", "louis_dubois", "lucas_mendes", "marco_bellini", "mateo_vasquez", "mei_lin_chen",
            "noah_kaplan", "odile_ferrand", "rin_takeda", "sam_okafor", "severine_morel", "tunde_oyelaran", "victor_sorel", "yelena_ivanova",
            "youssef_haddad", "zofia_kowalczyk",
        };

        /// <summary>Ennemis illustrés (identifiants de définition : la clé de nom est « enemies.id.name »).</summary>
        static readonly string[] Foes =
        {
            "thug", "brute", "sniper", "street_pyro", "brick", "canal_chief", "crocodile", "bridge_titan", "amif_trooper", "amif_marksman", "amif_captain",
        };

        /// <summary>Identifiant d'agent qui a un portrait cadré, ou null.</summary>
        public static string? AgentKey(Sprite? art, string? name)
        {
            if (art != null && Heads.Contains(art.name)) return art.name;
            string slug = Slug(name);
            return slug.Length > 0 && Heads.Contains(slug) ? slug : null;
        }

        /// <summary>Pose le portrait cadré de l'agent sur <paramref name="frame"/> ; faux si l'agent n'en a pas (le cadre garde alors son image).</summary>
        public static bool ApplyAgent(VisualElement frame, Sprite? art, string? name)
        {
            Clear(frame);
            string? key = AgentKey(art, name);
            if (key == null) return false;
            frame.AddToClassList(HeadPrefix + key);
            frame.style.backgroundImage = new StyleBackground(StyleKeyword.Null);
            return true;
        }

        /// <summary>Pose l'illustration de l'ennemi qui porte ce nom affiché ; faux s'il n'en a pas.</summary>
        public static bool ApplyEnemy(VisualElement frame, string? name)
        {
            Clear(frame);
            if (string.IsNullOrEmpty(name)) return false;
            foreach (var id in Foes)
            {
                if (!string.Equals(Loc.T("enemies." + id + ".name"), name, StringComparison.OrdinalIgnoreCase)) continue;
                frame.AddToClassList(FoePrefix + id);
                frame.style.backgroundImage = new StyleBackground(StyleKeyword.Null);
                return true;
            }
            return false;
        }

        static void Clear(VisualElement frame)
        {
            string? previous = null;
            foreach (var c in frame.GetClasses())
                if (c.StartsWith(HeadPrefix, StringComparison.Ordinal) || c.StartsWith(FoePrefix, StringComparison.Ordinal)) { previous = c; break; }
            if (previous != null) frame.RemoveFromClassList(previous);
        }

        /// <summary>« Séverine Morel » → « severine_morel » : minuscules sans accents, tout autre signe devient un tiret bas.</summary>
        static string Slug(string? name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            var sb = new StringBuilder(name!.Length);
            foreach (char ch in name.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
                else if (sb.Length > 0 && sb[sb.Length - 1] != '_') sb.Append('_');
            }
            while (sb.Length > 0 && sb[sb.Length - 1] == '_') sb.Length--;
            return sb.ToString();
        }
    }
}
