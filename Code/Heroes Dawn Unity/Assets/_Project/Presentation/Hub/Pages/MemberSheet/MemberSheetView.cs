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
    /// <summary>
    /// Fiche d'un membre (prefab Pages/MemberSheet), selon la maquette : illustration à gauche, niveau, histoire, stats et paliers au centre,
    /// quatre compétences, trois objets et affinités à droite ; le panneau d'inventaire remplace la grille de compétences.
    /// </summary>
    public sealed class MemberSheetView : ViewBase, IMemberSheetView
    {
        [SerializeField] Image wideArt = null!;
        [SerializeField] Image strip = null!;
        [SerializeField] Image art = null!;
        [SerializeField] TMP_Text memberName = null!;
        [SerializeField] TMP_Text rarity = null!;
        [SerializeField] TMP_Text role = null!;
        [SerializeField] UiBar levelBar = null!;
        [SerializeField] TMP_Text bio = null!;
        [SerializeField] TMP_Text stats = null!;
        [SerializeField] TMP_Text tiersTitle = null!;
        [SerializeField] TMP_Text tiers = null!;
        [SerializeField] UiBar fatigue = null!;
        [SerializeField] SheetTileView[] skillTiles = null!;
        [SerializeField] SheetTileView[] itemTiles = null!;
        [SerializeField] PortraitView[] affinity = null!;
        [SerializeField] TMP_Text affinityTitle = null!;
        [SerializeField] Button affinityButton = null!;
        [SerializeField] UiButton previous = null!;
        [SerializeField] UiButton next = null!;
        [SerializeField] UiButton heal = null!;
        [SerializeField] UiButton loadout = null!;
        [SerializeField] UiButton primary = null!;
        [SerializeField] TMP_Text primaryReason = null!;
        [SerializeField] GameObject pickerRoot = null!;
        [SerializeField] TMP_Text pickerTitle = null!;
        [SerializeField] TMP_Text pickerHint = null!;
        [SerializeField] SheetTileView[] pickerTiles = null!;
        [SerializeField] UiButton pickerRemove = null!;
        [SerializeField] UiButton pickerClose = null!;

        public event Action? PreviousClicked;
        public event Action? NextClicked;
        public event Action? PrimaryClicked;
        public event Action? HealClicked;
        public event Action? LoadoutClicked;
        public event Action? AffinityClicked;
        public event Action<int>? SkillTileClicked;
        public event Action<int>? ItemTileClicked;
        public event Action<int>? PickerChosen;
        public event Action? PickerRemoved;
        public event Action? PickerClosed;

        public Button PrimaryButton => primary.Button;
        public Button NextButton => next.Button;
        public Button SkillTileButton(int i) => skillTiles[i].Button;
        public Button ItemTileButton(int i) => itemTiles[i].Button;
        public Button PickerTileButton(int i) => pickerTiles[i].Button;

        void Awake()
        {
            previous.Button.onClick.AddListener(() => PreviousClicked?.Invoke());
            next.Button.onClick.AddListener(() => NextClicked?.Invoke());
            primary.Button.onClick.AddListener(() => PrimaryClicked?.Invoke());
            heal.Button.onClick.AddListener(() => HealClicked?.Invoke());
            loadout.Button.onClick.AddListener(() => LoadoutClicked?.Invoke());
            affinityButton.onClick.AddListener(() => AffinityClicked?.Invoke());
            for (int i = 0; i < skillTiles.Length; i++) { int k = i; skillTiles[i].Button.onClick.AddListener(() => SkillTileClicked?.Invoke(k)); }
            for (int i = 0; i < itemTiles.Length; i++) { int k = i; itemTiles[i].Button.onClick.AddListener(() => ItemTileClicked?.Invoke(k)); }
            for (int i = 0; i < pickerTiles.Length; i++) { int k = i; pickerTiles[i].Button.onClick.AddListener(() => PickerChosen?.Invoke(k)); }
            pickerRemove.Button.onClick.AddListener(() => PickerRemoved?.Invoke());
            pickerClose.Button.onClick.AddListener(() => PickerClosed?.Invoke());
            pickerRoot.SetActive(false);
        }

        public void Show(MemberSheetData d)
        {
            var family = Theme.FamilyColor(d.Card.Family);
            strip.color = family;
            var sprite = d.Card.Portrait.Art;
            art.gameObject.SetActive(sprite != null);
            wideArt.gameObject.SetActive(sprite != null);
            if (sprite != null)
            {
                art.sprite = sprite;
                wideArt.sprite = sprite;
                float ratio = sprite.rect.width / sprite.rect.height;
                var f1 = art.GetComponent<AspectRatioFitter>();
                if (f1 != null) f1.aspectRatio = ratio;
                var f2 = wideArt.GetComponent<AspectRatioFitter>();
                if (f2 != null) f2.aspectRatio = ratio;
            }
            memberName.text = d.Card.Name;
            rarity.text = d.Card.RarityText;
            rarity.color = Theme.RarityColor(d.Card.Rarity);
            role.text = d.Card.RoleLine;
            levelBar.SetColor(family);
            levelBar.Set(d.Level01, true);
            levelBar.SetLabel(d.LevelBarText);
            bio.text = d.Bio;
            stats.text = d.Card.Stats;
            fatigue.SetColor(Theme.FatigueColor(d.Fatigue));
            fatigue.Set(d.Fatigue01, true);
            fatigue.SetLabel(d.FatigueText);
            if (fatigue.gameObject.activeSelf != d.Owned) fatigue.gameObject.SetActive(d.Owned);

            tiersTitle.text = d.TiersTitle;
            var sb = new StringBuilder();
            foreach (var t in d.Tiers)
            {
                if (sb.Length > 0) sb.Append('\n');
                string color = t.Current ? "#FFCF33" : t.Reached ? "#FFFFFF" : "#9A93D0";
                sb.Append("<color=").Append(color).Append("><b>").Append(t.Level).Append("</b>  ").Append(t.Stats).Append("</color>");
            }
            tiers.text = sb.ToString();

            Fill(skillTiles, d.SkillTiles);
            Fill(itemTiles, d.ItemTiles);
            affinityTitle.text = d.AffinityTitle;
            for (int i = 0; i < affinity.Length; i++)
            {
                bool on = d.Affinity != null && i < d.Affinity.Length;
                affinity[i].gameObject.SetActive(on);
                if (on) affinity[i].Show(d.Affinity![i]);
            }

            SetActive(previous, d.ShowArrows);
            SetActive(next, d.ShowArrows);
            SetActive(heal, d.ShowHeal);
            if (d.ShowHeal)
            {
                heal.SetLabel(d.HealLabel);
                heal.Interactable = d.HealEnabled;
                heal.DisabledReason = d.HealReason;
            }
            SetActive(loadout, d.ShowLoadout);
            if (d.ShowLoadout) loadout.SetLabel(d.LoadoutLabel);
            primary.SetLabel(d.PrimaryLabel);
            primary.Interactable = d.PrimaryEnabled;
            primary.DisabledReason = d.PrimaryReason;
            primaryReason.text = d.PrimaryReason;
        }

        public void ShowPicker(PickerData d)
        {
            pickerRoot.SetActive(true);
            pickerTitle.text = d.Title;
            pickerHint.text = d.Hint;
            for (int i = 0; i < pickerTiles.Length; i++)
            {
                bool on = i < d.Options.Length;
                pickerTiles[i].gameObject.SetActive(on);
                if (on) pickerTiles[i].Show(d.Options[i]);
            }
            SetActive(pickerRemove, d.CanRemove);
        }

        public void HidePicker() => pickerRoot.SetActive(false);

        static void Fill(SheetTileView[] views, SheetTile[]? tiles)
        {
            for (int i = 0; i < views.Length; i++)
            {
                bool on = tiles != null && i < tiles.Length;
                if (views[i].gameObject.activeSelf != on) views[i].gameObject.SetActive(on);
                if (on) views[i].Show(tiles![i]);
            }
        }

        static void SetActive(Component c, bool on)
        {
            if (c.gameObject.activeSelf != on) c.gameObject.SetActive(on);
        }
    }
}
