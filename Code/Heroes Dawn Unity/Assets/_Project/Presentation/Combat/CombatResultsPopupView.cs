#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
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
            _title.style.color = d.Victory ? Theme.Accent : Theme.Danger;
            _reward.text = d.Reward;
            _fatigueTitle.text = d.FatigueTitle;
            FillRows(_fatigue, d.Fatigue, false);
            bool hasXp = d.Xp != null && d.Xp.Length > 0;
            _xpTitle.style.display = _xp.style.display = hasXp ? DisplayStyle.Flex : DisplayStyle.None;
            _xpTitle.text = d.XpTitle;
            FillRows(_xp, d.Xp, true);
            _verify.text = d.Verify; _verify.style.color = Theme.TextMuted;
            _toHq.text = d.ToHq;
            _toHq.style.display = string.IsNullOrEmpty(d.ToHq) ? DisplayStyle.None : DisplayStyle.Flex;
            _continue.text = d.Continue;
        }

        /// <summary>Lignes avant → après : portrait cadré de l'agent (à défaut son initiale), nom, jauge et valeur. L'expérience prend la teinte d'expérience du thème.</summary>
        static void FillRows(VisualElement host, FatigueRowData[]? rows, bool xp)
        {
            host.Clear();
            if (rows == null) return;
            foreach (var r in rows)
            {
                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("hd-poprow");
                var head = new VisualElement { pickingMode = PickingMode.Ignore };
                head.AddToClassList("hd-res__head");
                var initial = new Label { pickingMode = PickingMode.Ignore };
                initial.AddToClassList("hd-res__initial");
                head.Add(initial);
                if (!HudPortraits.ApplyAgent(head, null, r.Name)) initial.text = string.IsNullOrEmpty(r.Name) ? "?" : r.Name.Substring(0, 1).ToUpperInvariant();
                var name = new Label(r.Name) { pickingMode = PickingMode.Ignore };
                name.AddToClassList("hd-poprow__name");
                var bar = new VisualElement { pickingMode = PickingMode.Ignore };
                bar.AddToClassList("hd-bar"); bar.AddToClassList("hd-poprow__bar");
                if (xp) bar.AddToClassList("hd-bar--xp");
                var fill = new VisualElement { pickingMode = PickingMode.Ignore };
                fill.AddToClassList("hd-bar__fill");
                fill.style.width = Length.Percent(Mathf.Clamp01(r.After01) * 100f);
                if (!xp) fill.style.unityBackgroundImageTintColor = Theme.FatigueColor(r.After);
                bar.Add(fill);
                var label = new Label(string.IsNullOrEmpty(r.Delta) ? r.Label : r.Label + "  " + r.Delta) { pickingMode = PickingMode.Ignore };
                label.AddToClassList("hd-poprow__label");
                row.Add(head); row.Add(name); row.Add(bar); row.Add(label);
                host.Add(row);
            }
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
