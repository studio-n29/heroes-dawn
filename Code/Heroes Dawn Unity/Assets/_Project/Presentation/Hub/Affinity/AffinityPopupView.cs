#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Affinités d'un agent (popup) : son lien et le bonus de leur duo avec chaque agent de l'effectif ; se termine à la fermeture.</summary>
    public interface IAffinityPopupView : IPopupView
    {
        UniTask ShowAsync(AffinityPopupData data, CancellationToken ct);
    }

    /// <summary>Fenêtre d'affinités (UI Toolkit) : liste défilante des agents possédés, avec le bonus que donne chaque duo. Retour = Fermer.</summary>
    public sealed class AffinityPopupView : HdPopup, IAffinityPopupView
    {
        readonly List<AffinityRowData> _rows = new List<AffinityRowData>();
        Label _title = null!, _subtitle = null!, _empty = null!;
        ScrollView _list = null!;
        UnityEngine.UIElements.Button _close = null!;
        UniTaskCompletionSource? _done;
        AffinityPopupData? _last;

        public override bool Cancelable => true;
        public IReadOnlyList<AffinityRowData> Rows => _rows;
        public HdButtonRef CloseButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _close;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _subtitle = Q<Label>("subtitle"); _empty = Q<Label>("empty");
            _list = Q<ScrollView>("list"); _close = Q<UnityEngine.UIElements.Button>("close");
            CloseButton = new HdButtonRef(_close, this);
            _close.clicked += () => _done?.TrySetResult();
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill(AffinityPopupData d)
        {
            _title.text = d.Title; _subtitle.text = d.Subtitle; _close.text = d.Close;
            _list.Clear(); _rows.Clear();
            foreach (var r in d.Rows)
            {
                _rows.Add(r);
                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("hd-poprow");
                var art = new VisualElement { pickingMode = PickingMode.Ignore };
                art.AddToClassList("hd-poprow__art");
                // Portrait cadré sur le visage, bordé à la couleur de la famille, icône de famille en coin.
                art.AddToClassList("hd-aff__face");
                HdTeamUi.SetFamily(art, r.Portrait.Family, "hd-aff__face--");
                HdTeamUi.Head(art, r.Portrait.Art);
                var family = HdTeamUi.FamilyIcon(r.Portrait.Family);
                family.AddToClassList("hd-aff__family");
                art.Add(family);
                row.AddToClassList("hd-aff__row");
                var text = new VisualElement { pickingMode = PickingMode.Ignore };
                text.AddToClassList("hd-poprow__text");
                var name = new Label(r.Name) { pickingMode = PickingMode.Ignore }; name.AddToClassList("hd-poprow__name");
                var link = new Label(r.Link + "  " + r.Pair) { pickingMode = PickingMode.Ignore }; link.AddToClassList("hd-pop__hint");
                text.Add(name); text.Add(link);
                var bonus = new Label(r.Bonus) { pickingMode = PickingMode.Ignore }; bonus.AddToClassList("hd-poprow__label");
                bonus.style.color = r.HasBonus ? Theme.Success : Theme.TextMuted;
                row.Add(art); row.Add(text); row.Add(bonus);
                _list.Add(row);
            }
            _empty.text = d.Empty;
            _empty.style.display = d.Rows.Length == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public UniTask ShowAsync(AffinityPopupData d, CancellationToken ct)
        {
            _last = d;
            if (_title != null) Fill(d);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }

        public override void RequestCancel() => _done?.TrySetResult();
    }
}
