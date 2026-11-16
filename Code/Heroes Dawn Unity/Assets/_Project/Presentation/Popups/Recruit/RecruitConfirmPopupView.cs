#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>
    /// Confirmation d'achat (M31, fenêtre UI Toolkit, planche 11.1) : portrait du héros dans le cadre de sa famille, synergie phare,
    /// solde avant / coût / après en rangées à icône de monnaie ; si le solde ne suffit pas, manque en rouge, piste pour gagner des PR
    /// et bouton d'achat désactivé. Retour = Annuler.
    /// </summary>
    public sealed class RecruitConfirmPopupView : HdPopup, IRecruitConfirmPopupView
    {
        static readonly string[] FamilyKeys = { "matter", "elemental", "bio", "perception" };

        Label _title = null!, _name = null!, _rarity = null!, _role = null!, _synergy = null!, _before = null!, _cost = null!, _after = null!, _missing = null!, _howToEarn = null!;
        VisualElement _portrait = null!, _bust = null!, _frame = null!, _family = null!, _afterRow = null!;
        UnityEngine.UIElements.Button _ok = null!, _cancel = null!;
        UniTaskCompletionSource<bool>? _answer;
        RecruitConfirmData? _last;
        string _bustClass = "";

        public override bool Cancelable => true;
        public HdButtonRef OkButton { get; private set; } = null!;
        public HdButtonRef CancelButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _ok.enabledSelf ? _ok : _cancel;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _name = Q<Label>("name"); _rarity = Q<Label>("rarity"); _role = Q<Label>("role");
            _synergy = Q<Label>("synergy"); _before = Q<Label>("before"); _cost = Q<Label>("cost"); _after = Q<Label>("after");
            _missing = Q<Label>("missing"); _howToEarn = Q<Label>("howToEarn"); _portrait = Q<VisualElement>("portrait");
            _bust = Q<VisualElement>("portraitBust"); _frame = Q<VisualElement>("portraitFrame"); _family = Q<VisualElement>("family");
            _afterRow = Q<VisualElement>("afterRow");
            _bustClass = "";
            Q<Label>("beforeLabel").text = Loc.T("recruit.confirm.before");
            Q<Label>("costLabel").text = Loc.T("recruit.confirm.cost");
            Q<Label>("afterLabel").text = Loc.T("recruit.confirm.after");
            _ok = Q<UnityEngine.UIElements.Button>("ok"); _cancel = Q<UnityEngine.UIElements.Button>("cancel");
            OkButton = new HdButtonRef(_ok, this); CancelButton = new HdButtonRef(_cancel, this);
            _ok.clicked += () => _answer?.TrySetResult(true);
            _cancel.clicked += () => _answer?.TrySetResult(false);
            if (_last.HasValue) Fill(_last.Value);
        }

        static void SetFamily(VisualElement e, string prefix, Family family)
        {
            string key = family == Family.Matter ? FamilyKeys[0] : family == Family.Elemental ? FamilyKeys[1] : family == Family.PhysicalBio ? FamilyKeys[2] : FamilyKeys[3];
            foreach (var k in FamilyKeys) e.EnableInClassList(prefix + k, k == key);
        }

        static void Show(VisualElement e, bool shown) => e.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;

        void Fill(RecruitConfirmData d)
        {
            _title.text = d.Title;
            HdPopupRows.SetArt(_portrait, d.Card.Portrait.Art);
            // Portrait cadré sur le visage (kit) posé sur l'illustration : sans portrait au kit, l'illustration reste visible.
            string bust = "hd-bust--" + d.Card.Id.Value;
            if (bust != _bustClass)
            {
                if (_bustClass.Length > 0) _bust.RemoveFromClassList(_bustClass);
                _bust.AddToClassList(bust);
                _bustClass = bust;
            }
            SetFamily(_frame, "hd-hero__frame--", d.Card.Family);
            SetFamily(_family, "hd-famicon--", d.Card.Family);
            _name.text = d.Card.Name;
            _rarity.text = d.Card.RarityText; _rarity.style.color = Theme.RarityColor(d.Card.Rarity);
            _role.text = d.Card.RoleLine; _synergy.text = d.Synergy;
            Show(_synergy, !string.IsNullOrEmpty(d.Synergy));
            HdButtonContent.Price(_before, d.Before);
            HdButtonContent.Price(_cost, d.Cost);
            HdButtonContent.Price(_after, d.CanAfford ? d.After : "");
            Show(_afterRow, d.CanAfford);
            _missing.text = d.Missing; _howToEarn.text = d.HowToEarn;
            Show(_missing, !string.IsNullOrEmpty(d.Missing));
            Show(_howToEarn, !string.IsNullOrEmpty(d.HowToEarn));
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
