#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ligne de membre des pièces (widget prefab) : Salon (bouton Entraîner) ou Infirmerie (fatigue et palier écrit).</summary>
    public sealed class MemberRowView : MonoBehaviour
    {
        [SerializeField] Image strip = null!;
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text memberName = null!;
        [SerializeField] TMP_Text detail = null!;
        [SerializeField] UiBar fatigue = null!;
        [SerializeField] TMP_Text band = null!;
        [SerializeField] UiButton action = null!;
        [SerializeField] TMP_Text reason = null!;

        public event Action? ActionClicked;

        public Button ActionButton => action.Button;

        void Awake() => action.Button.onClick.AddListener(() => ActionClicked?.Invoke());

        void ShowCard(MemberCardData c)
        {
            strip.color = Theme.FamilyColor(c.Family);
            portrait.Show(c.Portrait);
            memberName.text = c.Name;
            detail.text = c.LevelText + " · " + c.RoleLine;
        }

        public void Show(SalonRowData d)
        {
            ShowCard(d.Card);
            SetActive(fatigue, false);
            SetActive(band, false);
            SetActive(action, true);
            action.SetLabel(d.Label);
            action.Interactable = d.Enabled;
            reason.text = d.Reason;
        }

        public void Show(InfirmaryRowData d)
        {
            ShowCard(d.Card);
            SetActive(fatigue, true);
            SetActive(band, true);
            SetActive(action, false);
            fatigue.SetColor(Theme.FatigueColor(d.Fatigue));
            fatigue.Set(d.Fatigue01, true);
            fatigue.SetLabel(d.FatigueText);
            band.text = d.BandText;
            band.color = d.Band == FatigueBand.Exhausted ? Theme.Danger : d.Band == FatigueBand.Tired ? Theme.Fatigue : Theme.Success;
            reason.text = "";
        }

        static void SetActive(Component c, bool on)
        {
            if (c.gameObject.activeSelf != on) c.gameObject.SetActive(on);
        }
    }
}
