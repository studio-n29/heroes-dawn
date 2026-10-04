#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Infirmerie (prefab Pages/Infirmary) : bandeau « version simple », fatigue de chaque membre, soin de l'équipe.</summary>
    public sealed class InfirmaryPageView : ViewBase, IInfirmaryPageView
    {
        [SerializeField] TMP_Text banner = null!;
        [SerializeField] RectTransform list = null!;
        [SerializeField] MemberRowView rowPrefab = null!;
        [SerializeField] UiButton heal = null!;
        [SerializeField] TMP_Text healReason = null!;

        public event Action? HealClicked;

        public Button HealButton => heal.Button;

        void Awake() => heal.Button.onClick.AddListener(() => HealClicked?.Invoke());

        public void Show(InfirmaryPageData d)
        {
            banner.text = d.Banner;
            list.Clear();
            foreach (var r in d.Rows) Instantiate(rowPrefab, list).Show(r);
            heal.SetLabel(d.HealLabel);
            heal.Interactable = d.HealEnabled;
            healReason.text = d.HealReason;
        }
    }
}
