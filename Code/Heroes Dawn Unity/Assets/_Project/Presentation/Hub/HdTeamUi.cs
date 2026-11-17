#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UIElements;
using DefPosition = N29.HeroesDawn.Definitions.Position;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Aides d'habillage des écrans « équipe et collections » (UI Toolkit) : portraits cadrés sur le visage, icônes du kit et icônes de famille.
    /// Les icônes sont posées par des classes USS de <c>hd-team.uss</c>. Les portraits reprennent le cadrage « tête » du kit
    /// (<c>AubeTactiqueKit/Portraits/AGENT-head.asset</c>) appliqué à l'illustration de l'agent déjà chargée : les fichiers du kit sont hors
    /// de <c>Resources</c>, donc non chargeables à l'exécution, mais ils cadrent la même image (1280 × 720).
    /// </summary>
    public static class HdTeamUi
    {
        const string HeadClass = "hd-tm-head";

        /// <summary>Taille de l'illustration de référence et côté du cadrage « tête », en pixels de cette image.</summary>
        const float ArtWidth = 1280f, ArtHeight = 720f, HeadSide = 320f;

        /// <summary>Coin bas gauche du cadrage « tête » de chaque agent (valeurs <c>m_Rect</c> des sprites <c>-head</c> du kit).</summary>
        static readonly Dictionary<string, Vector2> HeadOrigins = new Dictionary<string, Vector2>
        {
            { "ada_okonkwo", new Vector2(428.8f, 400.0f) },
            { "agent_d", new Vector2(428.8f, 400.0f) },
            { "agent_m", new Vector2(441.6f, 400.0f) },
            { "amara_hughes", new Vector2(480.0f, 380.0f) },
            { "anna_petrov", new Vector2(915.2f, 400.0f) },
            { "astrid_hallstrom", new Vector2(390.4f, 400.0f) },
            { "celeste_moreau", new Vector2(390.4f, 400.0f) },
            { "clement_lefevre", new Vector2(428.8f, 400.0f) },
            { "darius_brooks", new Vector2(377.6f, 300.8f) },
            { "eero_nieminen", new Vector2(480.0f, 400.0f) },
            { "fatou_diallo", new Vector2(352.0f, 400.0f) },
            { "freya_lindqvist", new Vector2(480.0f, 394.4f) },
            { "giulia_rossi", new Vector2(390.4f, 400.0f) },
            { "haruto_nakamura", new Vector2(428.8f, 250.4f) },
            { "ilona_varga", new Vector2(288.0f, 400.0f) },
            { "ji_hoon_park", new Vector2(518.4f, 400.0f) },
            { "kwame_abara", new Vector2(416.0f, 400.0f) },
            { "louis_dubois", new Vector2(428.8f, 400.0f) },
            { "lucas_mendes", new Vector2(608.0f, 400.0f) },
            { "marco_bellini", new Vector2(467.2f, 400.0f) },
            { "mateo_vasquez", new Vector2(480.0f, 400.0f) },
            { "mei_lin_chen", new Vector2(531.2f, 400.0f) },
            { "noah_kaplan", new Vector2(224.0f, 308.0f) },
            { "odile_ferrand", new Vector2(505.6f, 400.0f) },
            { "rin_takeda", new Vector2(428.8f, 387.2f) },
            { "sam_okafor", new Vector2(390.4f, 387.2f) },
            { "severine_morel", new Vector2(364.8f, 400.0f) },
            { "tunde_oyelaran", new Vector2(249.6f, 365.6f) },
            { "victor_sorel", new Vector2(544.0f, 400.0f) },
            { "yelena_ivanova", new Vector2(313.6f, 400.0f) },
            { "youssef_haddad", new Vector2(480.0f, 400.0f) },
            { "zofia_kowalczyk", new Vector2(377.6f, 400.0f) },
        };

        /// <summary>Portraits déjà découpés, par identifiant d'agent (un sprite léger par agent, sur la texture de son illustration).</summary>
        static readonly Dictionary<string, Sprite> HeadCache = new Dictionary<string, Sprite>();

        /// <summary>Portrait d'après l'illustration de l'agent : son nom de sprite est l'identifiant de l'agent.</summary>
        public static void Head(VisualElement element, Sprite? art) => Head(element, art != null ? art.name : null, art);

        /// <summary>
        /// Pose le portrait cadré sur le visage de l'agent. Sans cadrage connu pour cet identifiant, l'illustration entière est affichée
        /// (la feuille de style la recadre vers le haut dans l'élément).
        /// </summary>
        public static void Head(VisualElement element, string? agentId, Sprite? art)
        {
            element.AddToClassList(HeadClass);
            var head = HeadSprite(agentId, art);
            var shown = head != null ? head : art;
            element.EnableInClassList("hd-tm-head--full", head == null && art != null);
            element.style.backgroundImage = shown != null ? new StyleBackground(shown) : new StyleBackground(StyleKeyword.Null);
        }

        /// <summary>Sprite « tête » d'un agent, découpé dans son illustration ; null sans illustration ou sans cadrage connu.</summary>
        static Sprite? HeadSprite(string? agentId, Sprite? art)
        {
            if (agentId == null || art == null || !HeadOrigins.TryGetValue(agentId, out var origin)) return null;
            var texture = art.texture;
            if (texture == null) return null;
            if (HeadCache.TryGetValue(agentId, out var cached) && cached != null && cached.texture == texture) return cached;
            // Le cadrage est exprimé sur l'image de référence : on le rapporte à la taille réelle du sprite (texture importée plus petite).
            var full = art.rect;
            float sx = full.width / ArtWidth, sy = full.height / ArtHeight;
            float w = Mathf.Min(HeadSide * sx, full.width), h = Mathf.Min(HeadSide * sy, full.height);
            float x = Mathf.Clamp(full.x + origin.x * sx, full.x, full.xMax - w), y = Mathf.Clamp(full.y + origin.y * sy, full.y, full.yMax - h);
            if (w < 1f || h < 1f) return null;
            var sprite = Sprite.Create(texture, new Rect(x, y, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = agentId + "-head";
            HeadCache[agentId] = sprite;
            return sprite;
        }

        /// <summary>Suffixe des fichiers de famille du kit (icônes et cadres).</summary>
        public static string FamilySlug(Family family)
        {
            switch (family)
            {
                case Family.Matter: return "matter";
                case Family.Elemental: return "elemental";
                case Family.PhysicalBio: return "bio";
                default: return "perception";
            }
        }

        /// <summary>Fichier d'icône de position (<c>Missing20261116/Positions</c>) : première ligne, attaquant, contrôle, soutien, distance.</summary>
        public static string PositionSlug(DefPosition position)
        {
            switch (position)
            {
                case DefPosition.FrontLine: return "frontline";
                case DefPosition.Attacker: return "attacker";
                case DefPosition.Controller: return "control";
                case DefPosition.Support: return "support";
                default: return "ranged";
            }
        }

        /// <summary>Icône blanche de position, teintée à la couleur de la famille de l'agent, inerte au toucher.</summary>
        public static VisualElement PositionIcon(DefPosition position, Family family)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList("hd-tm-pos");
            SetPosition(e, position, family);
            return e;
        }

        /// <summary>Change la position affichée d'une icône de position et sa teinte de famille.</summary>
        public static void SetPosition(VisualElement element, DefPosition position, Family family)
        {
            foreach (DefPosition p in Enum.GetValues(typeof(DefPosition)))
                element.EnableInClassList("hd-tm-pos--" + PositionSlug(p), p == position);
            element.style.unityBackgroundImageTintColor = Theme.FamilyColor(family);
        }

        /// <summary>Insigne de rang AMIF (1 à 5, <c>Missing20261116/Ranks</c>), inerte au toucher.</summary>
        public static VisualElement RankBadge(int rank)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList("hd-tm-rank");
            SetRank(e, rank);
            return e;
        }

        /// <summary>Change le rang affiché d'un insigne (ramené dans 1..5).</summary>
        public static void SetRank(VisualElement element, int rank)
        {
            int r = Mathf.Clamp(rank, AgentRanks.Min, AgentRanks.Max);
            for (int i = AgentRanks.Min; i <= AgentRanks.Max; i++) element.EnableInClassList("hd-tm-rank--" + i, i == r);
        }

        /// <summary>Icône blanche du kit (classe <c>hd-tm-ico--nom</c>), inerte au toucher.</summary>
        public static VisualElement Icon(string name)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList("hd-tm-ico");
            e.AddToClassList("hd-tm-ico--" + name);
            return e;
        }

        /// <summary>Icône colorée d'une famille (classe <c>hd-tm-fam--suffixe</c>), inerte au toucher.</summary>
        public static VisualElement FamilyIcon(Family family)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList("hd-tm-ico");
            SetFamily(e, family);
            return e;
        }

        /// <summary>Remplace la classe de famille d'un élément (icône ou cadre) : <paramref name="prefix"/> + suffixe de la famille.</summary>
        public static void SetFamily(VisualElement element, Family family, string prefix = "hd-tm-fam--")
        {
            foreach (Family f in Enum.GetValues(typeof(Family)))
                element.EnableInClassList(prefix + FamilySlug(f), f == family);
        }
    }
}
