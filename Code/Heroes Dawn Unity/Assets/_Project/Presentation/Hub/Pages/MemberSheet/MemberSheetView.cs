#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Fiche d'un membre (UI Toolkit), selon la maquette : illustration à gauche, niveau, histoire, stats et paliers au centre,
    /// quatre compétences, trois objets et affinités à droite ; le panneau d'inventaire et le dossier recouvrent la page.
    /// </summary>
    public sealed class MemberSheetView : HdView, IMemberSheetView
    {
        sealed class TileEl
        {
            public UnityEngine.UIElements.Button Root = null!;
            public VisualElement Art = null!;
            public Label Title = null!, Sub = null!;
        }

        readonly List<TileEl> _skills = new List<TileEl>(), _items = new List<TileEl>(), _picker = new List<TileEl>();

        VisualElement _wideArt = null!, _cutout = null!, _strip = null!, _levelFill = null!, _fatigueBar = null!, _fatigueFill = null!, _levelBar = null!;
        VisualElement _skillBox = null!, _itemBox = null!, _affinity = null!, _affinityBox = null!, _dossierRoot = null!, _pickerRoot = null!;
        Label _name = null!, _rarity = null!, _role = null!, _levelLabel = null!, _fatigueLabel = null!, _bio = null!, _stats = null!, _tiersTitle = null!, _tiers = null!;
        Label _affinityTitle = null!, _reason = null!, _dossierTitle = null!, _dossierText = null!, _pickerTitle = null!, _pickerHint = null!;
        ScrollView _pickerBox = null!, _dossierScroll = null!;
        UnityEngine.UIElements.Button _previous = null!, _next = null!, _primary = null!, _heal = null!, _dossier = null!, _loadout = null!, _dossierClose = null!, _pickerRemove = null!, _pickerClose = null!;
        MemberSheetData? _last;
        DossierData? _lastDossier;
        PickerData? _lastPicker;
        HdTutorialAnchor? _pickAnchor;

        public event Action? PreviousClicked;
        public event Action? NextClicked;
        public event Action? PrimaryClicked;
        public event Action? HealClicked;
        public event Action? DossierClicked;
        public event Action? DossierClosed;
        public event Action? LoadoutClicked;
        public event Action? AffinityClicked;
        public event Action<int>? SkillTileClicked;
        public event Action<int>? ItemTileClicked;
        public event Action<int>? PickerChosen;
        public event Action? PickerRemoved;
        public event Action? PickerClosed;

        public HdButtonRef PrimaryButton => new HdButtonRef(_primary, this);
        public HdButtonRef DossierButton => new HdButtonRef(_dossier, this);
        public HdButtonRef NextButton => new HdButtonRef(_next, this);
        public HdButtonRef LoadoutButton => new HdButtonRef(_loadout, this);
        public HdButtonRef SkillTileButton(int i) => new HdButtonRef(_skills[i].Root, this);
        public HdButtonRef ItemTileButton(int i) => new HdButtonRef(_items[i].Root, this);
        public HdButtonRef PickerTileButton(int i) => new HdButtonRef(_picker[i].Root, this);

        protected override void Bind()
        {
            _wideArt = Q<VisualElement>("wideArt"); _cutout = Q<VisualElement>("cutout"); _strip = Q<VisualElement>("strip");
            _name = Q<Label>("memberName"); _rarity = Q<Label>("rarity"); _role = Q<Label>("role");
            _levelBar = Q<VisualElement>("levelBar"); _levelFill = Q<VisualElement>("levelFill"); _levelLabel = Q<Label>("levelLabel");
            _bio = Q<Label>("bio"); _stats = Q<Label>("stats"); _tiersTitle = Q<Label>("tiersTitle"); _tiers = Q<Label>("tiers");
            _fatigueBar = Q<VisualElement>("fatigueBar"); _fatigueFill = Q<VisualElement>("fatigueFill"); _fatigueLabel = Q<Label>("fatigueLabel");
            _skillBox = Q<VisualElement>("skills"); _itemBox = Q<VisualElement>("items");
            _affinityBox = Q<VisualElement>("affinityBox"); _affinity = Q<VisualElement>("affinity"); _affinityTitle = Q<Label>("affinityTitle");
            _reason = Q<Label>("primaryReason");
            _previous = Q<UnityEngine.UIElements.Button>("previous"); _next = Q<UnityEngine.UIElements.Button>("next");
            _primary = Q<UnityEngine.UIElements.Button>("primary"); _heal = Q<UnityEngine.UIElements.Button>("heal");
            _dossier = Q<UnityEngine.UIElements.Button>("dossier"); _loadout = Q<UnityEngine.UIElements.Button>("loadout");
            _dossierRoot = Q<VisualElement>("dossierRoot"); _dossierTitle = Q<Label>("dossierTitle"); _dossierText = Q<Label>("dossierText");
            _dossierScroll = Q<ScrollView>("dossierScroll"); _dossierClose = Q<UnityEngine.UIElements.Button>("dossierClose");
            _pickerRoot = Q<VisualElement>("pickerRoot"); _pickerTitle = Q<Label>("pickerTitle"); _pickerHint = Q<Label>("pickerHint");
            _pickerBox = Q<ScrollView>("pickerTiles"); _pickerRemove = Q<UnityEngine.UIElements.Button>("pickerRemove"); _pickerClose = Q<UnityEngine.UIElements.Button>("pickerClose");
            _pickerBox.contentContainer.style.flexDirection = FlexDirection.Row;
            _pickerBox.contentContainer.style.flexWrap = Wrap.Wrap;
            _previous.clicked += () => PreviousClicked?.Invoke();
            _next.clicked += () => NextClicked?.Invoke();
            _primary.clicked += () => PrimaryClicked?.Invoke();
            _heal.clicked += () => HealClicked?.Invoke();
            _dossier.clicked += () => DossierClicked?.Invoke();
            _dossierClose.clicked += () => DossierClosed?.Invoke();
            _loadout.clicked += () => LoadoutClicked?.Invoke();
            _affinityBox.RegisterCallback<ClickEvent>(_ => AffinityClicked?.Invoke());
            _pickerRemove.text = Loc.T("sheet.picker_remove");
            _pickerClose.text = Loc.T("sheet.picker_close");
            _pickerRemove.clicked += () => PickerRemoved?.Invoke();
            _pickerClose.clicked += () => PickerClosed?.Invoke();
            _skills.Clear(); _items.Clear(); _picker.Clear(); _pickAnchor = null;
            HdTutorialAnchor.Attach(this, _primary, TutorialTarget.SheetTrain);
            for (int i = 0; i < 4; i++) { int k = i; var t = NewTile(() => SkillTileClicked?.Invoke(k)); _skills.Add(t); _skillBox.Add(t.Root); }
            for (int i = 0; i < 3; i++) { int k = i; var t = NewTile(() => ItemTileClicked?.Invoke(k)); _items.Add(t); _itemBox.Add(t.Root); }
            _dossierRoot.style.display = DisplayStyle.None;
            _pickerRoot.style.display = DisplayStyle.None;
            HdTutorialAnchor.Attach(this, _items[0].Root, TutorialTarget.SheetSlot);
            if (_last.HasValue) Show(_last.Value);
            if (_lastDossier.HasValue) ShowDossier(_lastDossier.Value);
            if (_lastPicker.HasValue) ShowPicker(_lastPicker.Value);
        }

        static TileEl NewTile(Action click)
        {
            var t = new TileEl { Root = new UnityEngine.UIElements.Button() };
            t.Root.RemoveFromClassList("unity-button");
            t.Root.AddToClassList("hd-tile"); t.Root.AddToClassList("hd-sheettile");
            t.Root.clicked += click;
            t.Art = new VisualElement { pickingMode = PickingMode.Ignore }; t.Art.AddToClassList("hd-tile__art");
            t.Title = new Label { pickingMode = PickingMode.Ignore }; t.Title.AddToClassList("hd-tile__title");
            t.Sub = new Label { pickingMode = PickingMode.Ignore }; t.Sub.AddToClassList("hd-tile__sub");
            t.Root.Add(t.Art); t.Root.Add(t.Title); t.Root.Add(t.Sub);
            return t;
        }

        static void FillTile(TileEl t, SheetTile d)
        {
            bool empty = d.Kind == SheetTileKind.Empty || d.Kind == SheetTileKind.Locked;
            t.Title.text = d.Title; t.Sub.text = d.Sub;
            t.Sub.style.display = string.IsNullOrEmpty(d.Sub) ? DisplayStyle.None : DisplayStyle.Flex;
            HdPopupRows.SetArt(t.Art, d.Art);
            t.Art.style.display = d.Art != null ? DisplayStyle.Flex : DisplayStyle.None;
            Color frame = empty ? Theme.Outline : d.Frame;
            t.Root.style.borderTopColor = t.Root.style.borderBottomColor = t.Root.style.borderLeftColor = t.Root.style.borderRightColor = frame;
            t.Root.EnableInClassList("hd-tile--locked", d.Kind == SheetTileKind.Locked || d.Dim);
            t.Title.style.color = empty || d.Dim ? Theme.TextMuted : Theme.Text;
        }

        static void FillTiles(List<TileEl> els, SheetTile[]? data)
        {
            for (int i = 0; i < els.Count; i++)
            {
                bool on = data != null && i < data.Length;
                els[i].Root.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
                if (on) FillTile(els[i], data![i]);
            }
        }

        static void Visible(VisualElement e, bool on) => e.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;

        public void Show(MemberSheetData d)
        {
            _last = d;
            if (_name == null) return;
            var family = Theme.FamilyColor(d.Card.Family);
            _strip.style.backgroundColor = family;
            var sprite = d.Card.Portrait.Art;
            Visible(_cutout, sprite != null); Visible(_wideArt, sprite != null);
            if (sprite != null)
            {
                // Agent détouré (fond transparent) au premier plan quand il existe ; l'illustration d'origine reste en fond.
                var cutout = AgentArt.Cutout(sprite.name);
                HdPopupRows.SetArt(_cutout, cutout != null ? cutout : sprite);
                HdPopupRows.SetArt(_wideArt, sprite);
            }
            _name.text = d.Card.Name;
            _rarity.text = d.Card.RarityText; _rarity.style.color = Theme.RarityColor(d.Card.Rarity);
            _role.text = d.Card.RoleLine;
            _levelFill.style.width = Length.Percent(Mathf.Clamp01(d.Level01) * 100f);
            _levelFill.style.unityBackgroundImageTintColor = family;
            _levelLabel.text = d.LevelBarText;
            _bio.text = d.Bio;
            _stats.text = d.Card.Stats;
            _fatigueFill.style.width = Length.Percent(Mathf.Clamp01(d.Fatigue01) * 100f);
            _fatigueFill.style.unityBackgroundImageTintColor = Theme.FatigueColor(d.Fatigue);
            _fatigueLabel.text = d.FatigueText;
            Visible(_fatigueBar, d.Owned);
            _tiersTitle.text = d.TiersTitle;
            var sb = new StringBuilder();
            foreach (var t in d.Tiers)
            {
                if (sb.Length > 0) sb.Append('\n');
                string color = t.Current ? "#E2B456" : t.Reached ? "#F1E9D8" : "#8996A5";
                sb.Append("<color=").Append(color).Append("><b>").Append(t.Level).Append("</b>  ").Append(t.Stats).Append("</color>");
            }
            _tiers.text = sb.ToString();

            FillTiles(_skills, d.SkillTiles);
            FillTiles(_items, d.ItemTiles);
            _affinityTitle.text = d.AffinityTitle;
            _affinity.Clear();
            if (d.Affinity != null)
                foreach (var p in d.Affinity)
                {
                    var face = new VisualElement { pickingMode = PickingMode.Ignore };
                    face.AddToClassList("hd-squadslot__art");
                    HdPopupRows.SetArt(face, p.Art);
                    _affinity.Add(face);
                }

            Visible(_previous, d.ShowArrows); Visible(_next, d.ShowArrows);
            Visible(_dossier, d.ShowDossierButton);
            if (d.ShowDossierButton) _dossier.text = d.DossierButtonLabel;
            Visible(_heal, d.ShowHeal);
            if (d.ShowHeal) { _heal.text = d.HealLabel; _heal.SetEnabled(d.HealEnabled); _heal.tooltip = d.HealReason; }
            Visible(_loadout, d.ShowLoadout);
            if (d.ShowLoadout) _loadout.text = d.LoadoutLabel;
            _primary.text = d.PrimaryLabel;
            _primary.SetEnabled(d.PrimaryEnabled);
            _primary.tooltip = d.PrimaryReason;
            _reason.text = d.PrimaryReason;
        }

        public void ShowDossier(DossierData d)
        {
            _lastDossier = d;
            if (_dossierRoot == null) return;
            _dossierTitle.text = d.Title;
            var sb = new StringBuilder();
            foreach (var section in d.Sections)
            {
                if (sb.Length > 0) sb.Append("\n\n");
                sb.Append("<color=#E2B456><b>").Append(section.Title.ToUpperInvariant()).Append("</b></color>\n").Append(section.Text);
            }
            _dossierText.text = sb.ToString();
            _dossierClose.text = d.Close;
            _dossierRoot.style.display = DisplayStyle.Flex;
            _dossierScroll.scrollOffset = Vector2.zero; // un dossier rouvert repart du haut du texte
            _dossierClose.schedule.Execute(() => _dossierClose.Focus()).StartingIn(20);
        }

        public void HideDossier()
        {
            _lastDossier = null;
            if (_dossierRoot != null) _dossierRoot.style.display = DisplayStyle.None;
        }

        public void ShowPicker(PickerData d)
        {
            _lastPicker = d;
            if (_pickerRoot == null) return;
            _pickerRoot.style.display = DisplayStyle.Flex;
            _pickerTitle.text = d.Title; _pickerHint.text = d.Hint;
            _pickerBox.Clear(); _picker.Clear();
            for (int i = 0; i < d.Options.Length; i++)
            {
                int k = i;
                var t = NewTile(() => PickerChosen?.Invoke(k));
                FillTile(t, d.Options[i]);
                _picker.Add(t); _pickerBox.Add(t.Root);
                if (i == 0) { if (_pickAnchor != null) Destroy(_pickAnchor.gameObject); _pickAnchor = HdTutorialAnchor.Attach(this, t.Root, TutorialTarget.SheetPick); }
            }
            Visible(_pickerRemove, d.CanRemove);
            (_picker.Count > 0 ? _picker[0].Root : _pickerClose).schedule.Execute(() => (_picker.Count > 0 ? (VisualElement)_picker[0].Root : _pickerClose).Focus()).StartingIn(20);
        }

        public void HidePicker()
        {
            _lastPicker = null;
            if (_pickerRoot != null) _pickerRoot.style.display = DisplayStyle.None;
        }
    }
}
