#nullable enable
using System;
using System.Text;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Fiche d'un membre (prefab Pages/MemberSheet) : identité, stats, compétences, combos, flèches, raccourcis, action principale.</summary>
    public sealed class MemberSheetView : ViewBase, IMemberSheetView
    {
        [SerializeField] Image strip = null!;
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text memberName = null!;
        [SerializeField] TMP_Text rarity = null!;
        [SerializeField] TMP_Text role = null!;
        [SerializeField] TMP_Text level = null!;
        [SerializeField] TMP_Text stats = null!;
        [SerializeField] UiBar fatigue = null!;
        [SerializeField] TMP_Text skills = null!;
        [SerializeField] TMP_Text combos = null!;
        [SerializeField] UiButton previous = null!;
        [SerializeField] UiButton next = null!;
        [SerializeField] UiButton salon = null!;
        [SerializeField] UiButton infirmary = null!;
        [SerializeField] UiButton loadout = null!;
        [SerializeField] UiButton primary = null!;
        [SerializeField] TMP_Text primaryReason = null!;

        public event Action? PreviousClicked;
        public event Action? NextClicked;
        public event Action? PrimaryClicked;
        public event Action<HubPage>? RoomClicked;
        public event Action? LoadoutClicked;

        public Button PrimaryButton => primary.Button;
        public Button NextButton => next.Button;

        void Awake()
        {
            previous.Button.onClick.AddListener(() => PreviousClicked?.Invoke());
            next.Button.onClick.AddListener(() => NextClicked?.Invoke());
            primary.Button.onClick.AddListener(() => PrimaryClicked?.Invoke());
            salon.Button.onClick.AddListener(() => RoomClicked?.Invoke(HubPage.Salon));
            infirmary.Button.onClick.AddListener(() => RoomClicked?.Invoke(HubPage.Infirmary));
            loadout.Button.onClick.AddListener(() => LoadoutClicked?.Invoke());
        }

        public void Show(MemberSheetData d)
        {
            strip.color = Theme.FamilyColor(d.Card.Family);
            portrait.Show(d.Card.Portrait);
            memberName.text = d.Card.Name;
            rarity.text = d.Card.RarityText;
            rarity.color = Theme.RarityColor(d.Card.Rarity);
            role.text = d.Card.RoleLine;
            level.text = d.Card.LevelText;
            stats.text = d.Card.Stats;
            fatigue.SetColor(Theme.FatigueColor(d.Fatigue));
            fatigue.Set(d.Fatigue01, true);
            fatigue.SetLabel(d.FatigueText);
            if (fatigue.gameObject.activeSelf != d.Owned) fatigue.gameObject.SetActive(d.Owned);

            var sb = new StringBuilder();
            foreach (var s in d.Skills)
            {
                if (sb.Length > 0) sb.Append("\n\n");
                sb.Append("<b><size=28><color=#FFD23F>").Append(s.Name).Append("</color></size></b>\n").Append(s.Description);
            }
            skills.text = sb.ToString();
            combos.text = string.Join("\n", d.Combos);

            SetActive(previous, d.ShowArrows);
            SetActive(next, d.ShowArrows);
            SetActive(salon, d.ShowRooms);
            SetActive(infirmary, d.ShowRooms);
            SetActive(loadout, d.ShowLoadout);
            if (d.ShowLoadout) loadout.SetLabel(d.LoadoutLabel);
            primary.SetLabel(d.PrimaryLabel);
            primary.Interactable = d.PrimaryEnabled;
            primary.DisabledReason = d.PrimaryReason;
            primaryReason.text = d.PrimaryReason;
        }

        static void SetActive(Component c, bool on)
        {
            if (c.gameObject.activeSelf != on) c.gameObject.SetActive(on);
        }
    }
}
