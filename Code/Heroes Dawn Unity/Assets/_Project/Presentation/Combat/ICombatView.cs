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
        /// <summary>Valeurs de <see cref="SkillDropped"/> : lâcher dans le vide, ou annuler.</summary>
        public const int DropTerrain = -1;
        public const int DropCancel = -2;
        event Action? RetreatClicked;
        event Action? AutoClicked;
        event Action? SpeedClicked;
        event Action? DebugWinClicked;
        event Action? PauseClicked;
        event Action<int>? AvatarClicked;
        event Action? ComboClicked;
        event Action? CycleClicked;
        event Action<int>? SkillClicked;
        /// <summary>Une carte de compétence est prise (emplacement 0 à 3).</summary>
        event Action<int>? SkillDragBegan;
        /// <summary>Carte lâchée : identifiant de l'unité visée, <see cref="DropTerrain"/> (dans le vide) ou <see cref="DropCancel"/> (retour dans l'éventail).</summary>
        event Action<int, int>? SkillDropped;
        event Action<int>? UnitTapped;

        void BuildUnits(IReadOnlyList<UnitCardData> units);
        void ShowUnits(IReadOnlyList<UnitStateData> states);
        /// <summary>Agent dont les icônes sont lançables (mêmes données que dans <see cref="UnitStateData.Skills"/>) ; sert aux raccourcis et aux tests.</summary>
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
