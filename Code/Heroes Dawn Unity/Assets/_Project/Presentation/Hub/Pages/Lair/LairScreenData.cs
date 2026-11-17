#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Une ligne de la liste « Ennemis découverts » (planche 12.4) : nom, niveau, progression, état.</summary>
    public readonly struct LairBossRowData
    {
        public readonly string EnemyId, Name, Level;
        public readonly bool Defeated, Next, Locked, Selected;
        public LairBossRowData(string enemyId, string name, string level, bool defeated, bool next, bool locked, bool selected)
        { EnemyId = enemyId; Name = name; Level = level; Defeated = defeated; Next = next; Locked = locked; Selected = selected; }
    }

    /// <summary>Tout ce que l'écran Repaire affiche (planche 12.4) : liste des boss à gauche, fiche du boss choisi à droite, bouton d'entrée.</summary>
    public struct LairScreenData
    {
        public string Title, LairName, ListTitle;
        public LairBossRowData[] Bosses;
        public int SelectedIndex;
        public string BossEnemyId, BossName, BossSubtitle, ProgressTitle, ProgressText, LoreTitle, Lore, RewardLine, SquadLine;
        public float Progress;
        public string EnterLabel, EnterReason;
        public bool CanEnter;
    }

    public interface ILairPageView : IView
    {
        event Action<int> BossSelected;
        event Action EnterClicked;
        void Show(LairScreenData data);
    }
}
