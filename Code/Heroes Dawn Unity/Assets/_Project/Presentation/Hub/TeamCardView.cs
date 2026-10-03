#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Carte d'un membre de l'effectif (widget prefab) : fatigue, escouade, entraînement.</summary>
    public sealed class TeamCardView : MemberCardView
    {
        [SerializeField] UiBar fatigue = null!;
        [SerializeField] UiButton squad = null!;
        [SerializeField] UiButton train = null!;

        public event Action? SquadClicked;
        public event Action? TrainClicked;

        void Awake()
        {
            squad.Button.onClick.AddListener(() => SquadClicked?.Invoke());
            train.Button.onClick.AddListener(() => TrainClicked?.Invoke());
        }

        public void Show(TeamCardData d)
        {
            ShowCard(d.Card);
            fatigue.SetColor(Theme.FatigueColor(d.Fatigue));
            fatigue.Set(d.Fatigue01, true);
            fatigue.SetLabel(d.FatigueText);
            squad.SetLabel(d.SquadLabel);
            squad.SetColors(d.InSquad ? Theme.Primary : Theme.Surface3, d.InSquad ? Theme.TextDark : Theme.Text);
            train.SetLabel(d.TrainLabel);
            train.Interactable = d.TrainEnabled;
            if (d.Exhausted) fill.color = Color.Lerp(Theme.Surface, Theme.Danger, 0.25f);
        }
    }
}
