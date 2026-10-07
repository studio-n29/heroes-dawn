#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Combat : unités, compétences, combo, barre du haut, journal, bandeau, retours visuels. Raccourcis 1-3, C, A, Tab relayés.</summary>
    public interface ICombatView : IView
    {
        event Action? RetreatClicked;
        event Action? AutoClicked;
        event Action? SpeedClicked;
        event Action? DebugWinClicked;
        event Action? PauseClicked;
        event Action<int>? AvatarClicked;
        event Action? ComboClicked;
        event Action? CycleClicked;
        event Action<int>? SkillClicked;
        event Action<int>? UnitTapped;

        void BuildUnits(IReadOnlyList<UnitCardData> units);
        void ShowUnits(IReadOnlyList<UnitStateData> states);
        void ShowSkills(SkillPanelData data);
        void ShowAvatar(AvatarBarData data);
        void ShowCombo(bool visible, string label);
        void ShowTimeline(TimelineData data);
        void ShowTopBar(CombatTopBarData data);
        void SetHint(string hint);
        void ShowBanner(string text, Tone tone);
        void ShowLog(IReadOnlyList<string> lines);
        void Float(int unitId, string text, Tone tone, int size);
        void Hit(int unitId);
        void Act(int unitId);
        UniTask PauseAsync(float seconds, CancellationToken ct);
    }
}
