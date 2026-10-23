#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Un onglet de la barre de navigation : page, libellé, actif, badge (vide = aucun).</summary>
    public struct HubTabData
    {
        public HubPage Page;
        public string Label;
        public bool Active;
        public string Badge;
        /// <summary>Onglet qui n'apparaît pas dans la barre (Recruter vit dans la Boutique).</summary>
        public bool Hidden;
    }

    /// <summary>Ce qu'affiche la coque du hub autour de la page : titre, bouton Retour, onglets.</summary>
    public struct HubShellData
    {
        public string Title;
        public bool ShowBack;
        public HubTabData[] Tabs;
        /// <summary>Avatar du chef d unité (Agent D ou M) affiché dans la barre haute ; nom vide : masqué.</summary>
        public PortraitData Avatar;
        public string AvatarName;
        public string AvatarRole;

        public static HubShellData Empty => new HubShellData { Title = "", Tabs = Array.Empty<HubTabData>() };
    }
}
