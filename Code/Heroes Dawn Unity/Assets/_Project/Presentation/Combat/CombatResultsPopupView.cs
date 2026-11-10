#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Résultats du combat (fenêtre UI Toolkit) : récompense, fatigue, expérience, vérification, Continuer. Retour = Continuer.</summary>
    public sealed class CombatResultsPopupView : HdPopup, ICombatResultsPopupView
    {
        Label _title = null!, _reward = null!, _fatigueTitle = null!, _xpTitle = null!, _verify = null!, _toHq = null!;
        VisualElement _fatigue = null!, _xp = null!;
        UnityEngine.UIElements.Button _continue = null!;
        UniTaskCompletionSource? _done;
        CombatResultsData? _last;
        (string Text, Tone Tone)? _verification;

        public override bool Cancelable => true;
        public override void RequestCancel() => _done?.TrySetResult();
        /// <summary>Bouton Continuer (tests PlayMode : clic réel).</summary>
        public HdButtonRef ContinueButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _continue;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _reward = Q<Label>("reward"); _fatigueTitle = Q<Label>("fatigueTitle"); _xpTitle = Q<Label>("xpTitle");
            _verify = Q<Label>("verify"); _toHq = Q<Label>("toHq"); _fatigue = Q<VisualElement>("fatigue"); _xp = Q<VisualElement>("xp");
            _continue = Q<UnityEngine.UIElements.Button>("continue");
            ContinueButton = new HdButtonRef(_continue, this);
            HdTutorialAnchor.Attach(this, _continue, TutorialTarget.ResultsContinue);
            _continue.clicked += () => _done?.TrySetResult();
            if (_last.HasValue) Fill(_last.Value);
            if (_verification.HasValue) SetVerification(_verification.Value.Text, _verification.Value.Tone);
        }

        void Fill(CombatResultsData d)
        {
            _title.text = d.Title;
            _title.style.color = d.Victory ? Theme.Primary : Theme.Danger;
            _reward.text = d.Reward;
            _fatigueTitle.text = d.FatigueTitle;
            HdPopupRows.FillFatigue(_fatigue, d.Fatigue);
            bool hasXp = d.Xp != null && d.Xp.Length > 0;
            _xpTitle.style.display = _xp.style.display = hasXp ? DisplayStyle.Flex : DisplayStyle.None;
            _xpTitle.text = d.XpTitle;
            HdPopupRows.FillFatigue(_xp, d.Xp);
            _verify.text = d.Verify; _verify.style.color = Theme.TextMuted;
            _toHq.text = d.ToHq;
            _toHq.style.display = string.IsNullOrEmpty(d.ToHq) ? DisplayStyle.None : DisplayStyle.Flex;
            _continue.text = d.Continue;
        }

        public UniTask ShowAsync(CombatResultsData d, CancellationToken ct)
        {
            _last = d;
            _verification = null;
            if (_title != null) Fill(d);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }

        public void SetVerification(string text, Tone tone)
        {
            _verification = (text, tone);
            if (_verify == null) return;
            _verify.text = text;
            _verify.style.color = ToneColors.Of(tone);
        }
    }
}
