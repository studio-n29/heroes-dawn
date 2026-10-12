#nullable enable
using System;

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

        public static HubShellData Empty => new HubShellData { Title = "", Tabs = Array.Empty<HubTabData>() };
    }
}
