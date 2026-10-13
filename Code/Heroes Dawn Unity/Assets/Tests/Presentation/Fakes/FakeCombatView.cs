#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeCombatView : FakeView, ICombatView
    {
        public event Action? RetreatClicked;
        public event Action? AutoClicked;
        public event Action? SpeedClicked;
        public event Action? DebugWinClicked;
        public event Action? PauseClicked;
        public event Action? CycleClicked;
        public event Action<int>? SkillClicked;
        public event Action<int>? SkillDragBegan;
        public event Action<int, int>? SkillDropped;
        public event Action<int>? UnitTapped;

        public IReadOnlyList<UnitCardData> Built = Array.Empty<UnitCardData>();
        public IReadOnlyList<UnitStateData> States = Array.Empty<UnitStateData>();
        public SkillPanelData Skills;
        public TimelineData Timeline;
        public CombatTopBarData TopBar;
        public string Hint = "";
        public IReadOnlyList<string> Log = Array.Empty<string>();

        public void ClickRetreat() => RetreatClicked?.Invoke();
        public void ClickAuto() => AutoClicked?.Invoke();
        public void ClickSpeed() => SpeedClicked?.Invoke();
        public void ClickSkill(int slot) => SkillClicked?.Invoke(slot);
        public void DragSkill(int slot, int where) { SkillDragBegan?.Invoke(slot); SkillDropped?.Invoke(slot, where); }
        public void BeginDragSkill(int slot) => SkillDragBegan?.Invoke(slot);
        public void DropSkill(int slot, int where) => SkillDropped?.Invoke(slot, where);
        public void TapUnit(int id) => UnitTapped?.Invoke(id);

        public void BuildUnits(IReadOnlyList<UnitCardData> units) => Built = units;
        public void ShowUnits(IReadOnlyList<UnitStateData> states) => States = states;
        public void ShowSkills(SkillPanelData data) => Skills = data;
        public void ShowTimeline(TimelineData data) => Timeline = data;
        public void ShowTopBar(CombatTopBarData data) => TopBar = data;
        public void SetHint(string hint) => Hint = hint;
        public void ShowBanner(string text, Tone tone) { }
        public void ShowLog(IReadOnlyList<string> lines) => Log = lines;
        public void Float(int unitId, string text, Tone tone, int size) { }
        public void Hit(int unitId) { }
        public void Act(int unitId) { }
        public (int Slot, int Target)? LastPlayedCard;
        public void PlayCard(int slot, int targetUnitId) => LastPlayedCard = (slot, targetUnitId);
        public UniTask PauseAsync(float seconds, CancellationToken ct) => UniTask.CompletedTask;
    }

    /// <summary>Résultats de test : « Continuer » sur demande (ou Retour), ou immédiatement si <see cref="AutoContinue"/>.</summary>
    public sealed class FakeCombatResultsPopup : FakePopup, ICombatResultsPopupView
    {
        public bool AutoContinue = true;
        public CombatResultsData? Data;
        /// <summary>Dernier texte de vérification affiché (ShowAsync puis SetVerification, comme la vraie vue).</summary>
        public string? VerifyText;
        UniTaskCompletionSource? _done;

        public FakeCombatResultsPopup() { Cancelable = true; }

        public UniTask ShowAsync(CombatResultsData d, CancellationToken ct)
        {
            Data = d;
            VerifyText = d.Verify;
            if (AutoContinue) return UniTask.CompletedTask;
            _done = new UniTaskCompletionSource();
            return _done.Task;
        }

        public void SetVerification(string text, Tone tone) => VerifyText = text;
        public override void RequestCancel() { base.RequestCancel(); _done?.TrySetResult(); }
    }
}
