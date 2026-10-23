#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Bouton d'une ligne ou de l'en-tête : style 0 jaune, 1 turquoise (PR), 2 rouge, 3 neutre.</summary>
    public struct RowButtonData
    {
        public string Label;
        public bool Visible;
        public bool Enabled;
        public string Reason;
        public int Style;

        public static RowButtonData Of(string label, bool enabled = true, string reason = "", int style = 0)
            => new RowButtonData { Label = label, Visible = true, Enabled = enabled, Reason = reason, Style = style };
    }

    /// <summary>Ligne d'une liste : pastille, titre, sous-titre, barre, jusqu'à trois boutons.</summary>
    public struct InfoRowData
    {
        public string Tag;
        public Color TagColor;
        public string Title;
        public string Subtitle;
        /// <summary>0..1, ou négatif : pas de barre.</summary>
        public float Progress;
        public string ProgressLabel;
        public string Badge;
        public bool Highlight;
        public bool Dim;
        public RowButtonData A, B, C;
        /// <summary>Illustration de la tuile (pages en grille) ; nulle : aucune.</summary>
        public Sprite? Art;

        public static InfoRowData Basic(string title, string subtitle = "") => new InfoRowData { Title = title, Subtitle = subtitle, Progress = -1f, TagColor = Color.clear };
    }

    public struct ListHeaderData
    {
        public string Title;
        public string Line;
        public float Progress;
        public string ProgressLabel;
        public RowButtonData A, B;
    }

    /// <summary>Colonne de droite optionnelle : titre, lignes, texte de détail.</summary>
    public struct ListSideData
    {
        public string Title;
        public InfoRowData[] Rows;
        public string Text;
    }

    /// <summary>Page « liste » : en-tête, lignes, colonne de droite optionnelle (Pass, Boutique, Opérations, Labo, Archives).</summary>
    public interface IListPageView : IView
    {
        event Action<int> HeaderClicked;
        event Action<int, int> RowButtonClicked;
        event Action<int> RowSelected;
        event Action<int, int> SideButtonClicked;
        event Action<int> SideRowSelected;
        void ShowHeader(ListHeaderData data);
        void ShowRows(InfoRowData[] rows);
        void ShowSide(ListSideData data);
    }
}
