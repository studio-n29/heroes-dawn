#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>
    /// Confirmation d'achat (M31, fenêtre UI Toolkit) : héros, synergie phare, solde avant / coût / après ; si le solde ne suffit pas,
    /// manque en rouge, piste pour gagner des PR et bouton d'achat désactivé. Retour = Annuler.
    /// </summary>
    public sealed class RecruitConfirmPopupView : HdPopup, IRecruitConfirmPopupView
    {
        Label _title = null!, _name = null!, _rarity = null!, _role = null!, _synergy = null!, _before = null!, _cost = null!, _after = null!, _missing = null!, _howToEarn = null!;
        VisualElement _portrait = null!;
        UnityEngine.UIElements.Button _ok = null!, _cancel = null!;
        UniTaskCompletionSource<bool>? _answer;
        RecruitConfirmData? _last;

        public override bool Cancelable => true;
        public HdButtonRef OkButton { get; private set; } = null!;
        public HdButtonRef CancelButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _ok.enabledSelf ? _ok : _cancel;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _name = Q<Label>("name"); _rarity = Q<Label>("rarity"); _role = Q<Label>("role");
            _synergy = Q<Label>("synergy"); _before = Q<Label>("before"); _cost = Q<Label>("cost"); _after = Q<Label>("after");
            _missing = Q<Label>("missing"); _howToEarn = Q<Label>("howToEarn"); _portrait = Q<VisualElement>("portrait");
            _ok = Q<UnityEngine.UIElements.Button>("ok"); _cancel = Q<UnityEngine.UIElements.Button>("cancel");
            OkButton = new HdButtonRef(_ok, this); CancelButton = new HdButtonRef(_cancel, this);
            _ok.clicked += () => _answer?.TrySetResult(true);
            _cancel.clicked += () => _answer?.TrySetResult(false);
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill(RecruitConfirmData d)
        {
            _title.text = d.Title;
            HdPopupRows.SetArt(_portrait, d.Card.Portrait.Art);
            _name.text = d.Card.Name;
            _rarity.text = d.Card.RarityText; _rarity.style.color = Theme.RarityColor(d.Card.Rarity);
            _role.text = d.Card.RoleLine; _synergy.text = d.Synergy;
            _before.text = d.Before; _cost.text = d.Cost; _after.text = d.CanAfford ? d.After : "";
            _missing.text = d.Missing; _howToEarn.text = d.HowToEarn;
            _ok.text = d.Ok; _ok.SetEnabled(d.CanAfford);
            _cancel.text = d.Cancel;
        }

        public UniTask<bool> AskAsync(RecruitConfirmData d, CancellationToken ct)
        {
            _last = d;
            if (_title != null) Fill(d);
            _answer = new UniTaskCompletionSource<bool>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public override void RequestCancel() => _answer?.TrySetResult(false);
    }
}
