#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Carte d'une recrue disponible (widget prefab) : coût en PR et bouton Recruter.</summary>
    public sealed class RecruitCardView : MemberCardView
    {
        [SerializeField] UiButton recruit = null!;

        public event Action? RecruitClicked;

        /// <summary>Bouton Recruter (tests PlayMode : clic réel).</summary>
        public UnityEngine.UI.Button RecruitButton => recruit.Button;

        void Awake() => recruit.Button.onClick.AddListener(() => RecruitClicked?.Invoke());

        public void Show(RecruitCardData d)
        {
            ShowCard(d.Card);
            recruit.SetLabel(d.RecruitLabel);
            recruit.Interactable = d.RecruitEnabled;
        }
    }
}
