#nullable enable
using N29.HeroesDawn.Presentation.Common;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Pass saisonnier : 30 niveaux et missions du jour (UI Toolkit). Les lignes de palier (contrat <see cref="IListPageView"/> inchangé) sont habillées
    /// après coup, selon l'état que portent leurs boutons : deux cases (gratuit, premium) verrouillée / à réclamer / réclamée, et le losange du
    /// palier en cours (le prochain niveau à atteindre : ligne grisée et mise en avant). Les rubans « GRATUIT » et « PREMIUM » (texte français inclus,
    /// jamais étirés) forment la légende au-dessus de la liste en français seulement.
    /// </summary>
    public sealed class PassPageView : HdListPageView, IPassPageView
    {
        /// <summary>État d'une case de récompense, avec la classe USS correspondante.</summary>
        enum CaseState { Locked, Claimable, Claimed }

        static readonly string[] StateClass = { "hd-pass__case--locked", "hd-pass__case--claimable", "hd-pass__case--claimed" };

        InfoRowData[]? _data;
        VisualElement? _legend;

        protected override bool WideSide => true;

        protected override void Bind()
        {
            base.Bind();
            _legend = null;
            var page = TryQ<VisualElement>("page");
            if (page != null) page.AddToClassList("hd-pass");
            var rows = TryQ<ScrollView>("rows");
            if (rows != null && rows.parent != null)
            {
                _legend = NewLegend();
                rows.parent.Insert(rows.parent.IndexOf(rows), _legend);
            }
            if (_data != null) Decorate(_data);
        }

        /// <summary>Les lignes passent d'abord par la page de liste commune, puis reçoivent leur habillage de piste (appelé aussi par l'interface de la page).</summary>
        public new void ShowRows(InfoRowData[] rows)
        {
            _data = rows;
            base.ShowRows(rows);
            Decorate(rows);
        }

        static VisualElement NewLegend()
        {
            var legend = new VisualElement { pickingMode = PickingMode.Ignore };
            legend.AddToClassList("hd-pass__legend");
            var free = new VisualElement { pickingMode = PickingMode.Ignore }; free.AddToClassList("hd-pass__ribbon"); free.AddToClassList("hd-pass__ribbon--free");
            var premium = new VisualElement { pickingMode = PickingMode.Ignore }; premium.AddToClassList("hd-pass__ribbon"); premium.AddToClassList("hd-pass__ribbon--premium");
            legend.Add(free); legend.Add(premium);
            return legend;
        }

        /// <summary>Case d'après le bouton de la piste : « pris » (style 3) = réclamée, bouton actif = à réclamer, sinon verrouillée.</summary>
        static CaseState StateOf(RowButtonData b) => b.Style == 3 ? CaseState.Claimed : b.Visible && b.Enabled ? CaseState.Claimable : CaseState.Locked;

        static VisualElement NewCase(string track)
        {
            var c = new VisualElement { pickingMode = PickingMode.Ignore };
            c.AddToClassList("hd-pass__case");
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("hd-pass__caseico"); icon.AddToClassList("hd-pass__caseico--" + track);
            c.Add(icon);
            return c;
        }

        static void SetState(VisualElement c, CaseState s)
        {
            for (int i = 0; i < StateClass.Length; i++) c.EnableInClassList(StateClass[i], i == (int)s);
        }

        void Decorate(InfoRowData[] data)
        {
            if (_legend != null) _legend.style.display = Loc.Language == "fr" ? DisplayStyle.Flex : DisplayStyle.None;
            var rows = TryQ<ScrollView>("rows");
            if (rows == null) return;
            int i = 0;
            foreach (var row in rows.contentContainer.Children())
            {
                if (!row.ClassListContains("hd-row")) continue;
                if (i >= data.Length) break;
                var d = data[i++];
                var tag = row.Q<VisualElement>(className: "hd-row__tag");
                var cases = row.Q<VisualElement>(className: "hd-pass__cases");
                if (cases == null)
                {
                    cases = new VisualElement { pickingMode = PickingMode.Ignore };
                    cases.AddToClassList("hd-pass__cases");
                    cases.Add(NewCase("free")); cases.Add(NewCase("premium"));
                    row.Insert(tag != null ? row.IndexOf(tag) + 1 : 0, cases);
                }
                SetState(cases[0], StateOf(d.A));
                SetState(cases[1], StateOf(d.B));
                if (tag != null)
                {
                    // La pastille perd son aplat de couleur (posé en ligne par la page de liste) ; le losange marque le palier en cours.
                    tag.style.backgroundColor = new StyleColor(StyleKeyword.Null);
                    tag.EnableInClassList("hd-pass__tag--current", d.Highlight && d.Dim);
                }
            }
        }
    }
}
