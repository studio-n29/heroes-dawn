#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Page Paramètres (UI Toolkit, planches 14 et 15) : catégories à icône à gauche (onglets verticaux), réglages de la catégorie à droite
    /// (interrupteurs, curseurs à pas, sélecteurs à chevrons, boutons d'action).
    /// </summary>
    public sealed class SettingsPageView : HdView, ISettingsPageView
    {
        /// <summary>Ce que les tests lisent d'une ligne de réglage affichée.</summary>
        public sealed class RowInfo { public string Label = ""; public bool Visible; }

        sealed class Row
        {
            public VisualElement Root = null!;
            public Label Label = null!, Hint = null!, Value = null!, Percent = null!;
            public VisualElement Bar = null!, Fill = null!, Thumb = null!, MinusIcon = null!, PlusIcon = null!;
            public UnityEngine.UIElements.Button Minus = null!, Plus = null!, Action = null!;
        }

        /// <summary>Icône de chaque catégorie, dans l'ordre du présentateur (Audio, Graphismes, Accessibilité, Langue, Contrôles, Compte, Support).</summary>
        static readonly string[] CategoryIcons =
        {
            "hd-ico--audio", "hd-ico--graphics", "hd-ico--accessibility", "hd-ico--language", "hd-ico--controls", "hd-ico--account", "hd-ico--support",
        };
        const string DefaultIcon = "hd-ico--settings";

        readonly List<Row> _pool = new List<Row>();
        readonly List<RowInfo> _infos = new List<RowInfo>();
        readonly List<UnityEngine.UIElements.Button> _cats = new List<UnityEngine.UIElements.Button>();
        ScrollView _catBox = null!, _rows = null!;
        Label _title = null!;
        VisualElement _titleIcon = null!;
        string _titleIconClass = "";
        UnityEngine.UIElements.Button _reset = null!;
        SettingsPageData? _last;

        public event Action<int>? CategorySelected;
        public event Action<int, int>? RowAction;
        public event Action? ResetCategoryClicked;

        public IReadOnlyList<RowInfo> Rows => _infos;
        public HdButtonRef CategoryButton(int i) => new HdButtonRef(_cats[i], this);

        /// <summary>Premier bouton d'action portant ce libellé (tests PlayMode : clics réels).</summary>
        public HdButtonRef? ActionButton(string label)
        {
            foreach (var r in _pool)
                if (r.Root.style.display != DisplayStyle.None && r.Action.style.display != DisplayStyle.None && r.Action.text == label)
                    return new HdButtonRef(r.Action, this);
            return null;
        }

        static string CategoryIcon(int index) => index >= 0 && index < CategoryIcons.Length ? CategoryIcons[index] : DefaultIcon;

        protected override void Bind()
        {
            _catBox = Q<ScrollView>("categories"); _rows = Q<ScrollView>("rows"); _title = Q<Label>("title");
            _titleIcon = Q<VisualElement>("titleIcon"); _titleIconClass = "";
            _reset = Q<UnityEngine.UIElements.Button>("reset");
            _reset.clicked += () => ResetCategoryClicked?.Invoke();
            _pool.Clear(); _infos.Clear(); _cats.Clear();
            if (_last.HasValue) Show(_last.Value);
        }

        public void Show(SettingsPageData d)
        {
            _last = d;
            if (_catBox == null) return;
            while (_cats.Count < d.Categories.Length)
            {
                int index = _cats.Count;
                var b = new UnityEngine.UIElements.Button();
                b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-settings__cat");
                b.clicked += () => CategorySelected?.Invoke(index);
                _cats.Add(b); _catBox.Add(b);
            }
            for (int i = 0; i < _cats.Count; i++)
            {
                bool used = i < d.Categories.Length;
                _cats[i].style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used) continue;
                HdButtonContent.Fill(_cats[i], d.Categories[i], CategoryIcon(i));
                bool active = i == d.Active;
                _cats[i].EnableInClassList("hd-btn--primary", active);
                _cats[i].EnableInClassList("hd-btn--secondary", !active);
            }
            _title.text = d.CategoryTitle;
            string icon = CategoryIcon(d.Active);
            if (icon != _titleIconClass)
            {
                if (_titleIconClass.Length > 0) _titleIcon.RemoveFromClassList(_titleIconClass);
                _titleIcon.AddToClassList(icon);
                _titleIconClass = icon;
            }
            _reset.style.display = d.CanReset ? DisplayStyle.Flex : DisplayStyle.None;
            HdButtonContent.Fill(_reset, d.ResetLabel, "hd-ico--refresh");

            while (_pool.Count < d.Rows.Length) { _pool.Add(NewRow(_pool.Count)); _rows.Add(_pool[_pool.Count - 1].Root); }
            _infos.Clear();
            for (int i = 0; i < _pool.Count; i++)
            {
                bool used = i < d.Rows.Length;
                _pool[i].Root.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used) continue;
                Apply(_pool[i], d.Rows[i]);
                _infos.Add(new RowInfo { Label = d.Rows[i].Label, Visible = true });
            }
        }

        static T Part<T>(string cls) where T : VisualElement, new()
        {
            var e = new T { pickingMode = PickingMode.Ignore };
            e.AddToClassList(cls);
            return e;
        }

        Row NewRow(int index)
        {
            var r = new Row { Root = new VisualElement() };
            r.Root.AddToClassList("hd-setting");
            var text = Part<VisualElement>("hd-setting__text");
            r.Label = Part<Label>("hd-setting__label");
            r.Hint = Part<Label>("hd-setting__hint");
            text.Add(r.Label); text.Add(r.Hint);
            r.Minus = Step(out r.MinusIcon, () => RowAction?.Invoke(index, 0));
            // Curseur à pas (volumes) : piste, remplissage et poignée entre « moins » et « plus », valeur à droite.
            r.Bar = Part<VisualElement>("hd-bar"); r.Bar.AddToClassList("hd-setting__bar");
            r.Fill = Part<VisualElement>("hd-bar__fill");
            r.Thumb = Part<VisualElement>("hd-setting__thumb");
            r.Bar.Add(r.Fill); r.Bar.Add(r.Thumb);
            r.Value = Part<Label>("hd-setting__value");
            r.Plus = Step(out r.PlusIcon, () => RowAction?.Invoke(index, 1));
            r.Percent = Part<Label>("hd-setting__percent");
            r.Action = new UnityEngine.UIElements.Button();
            r.Action.AddToClassList("hd-btn"); r.Action.AddToClassList("hd-btn--small"); r.Action.AddToClassList("hd-setting__action");
            r.Action.clicked += () => RowAction?.Invoke(index, 0);
            r.Root.Add(text); r.Root.Add(r.Minus); r.Root.Add(r.Bar); r.Root.Add(r.Value); r.Root.Add(r.Plus); r.Root.Add(r.Percent); r.Root.Add(r.Action);
            return r;
        }

        /// <summary>Bouton de pas : une icône du kit (moins et plus pour un curseur, chevrons pour un sélecteur), sans texte.</summary>
        static UnityEngine.UIElements.Button Step(out VisualElement icon, Action click)
        {
            var b = new UnityEngine.UIElements.Button();
            b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--secondary"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-setting__step");
            icon = Part<VisualElement>("hd-setting__stepicon");
            b.Add(icon);
            b.clicked += click;
            return b;
        }

        /// <summary>« 80 % » donne 80 : la valeur d'un curseur de volume.</summary>
        static bool TryPercent(string? value, out int percent)
        {
            percent = 0;
            if (string.IsNullOrEmpty(value)) return false;
            string v = value!.Trim();
            if (!v.EndsWith("%", StringComparison.Ordinal)) return false;
            return int.TryParse(v.Substring(0, v.Length - 1).Trim(), out percent);
        }

        static void Show(VisualElement e, bool shown) => e.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;

        static void Apply(Row r, SettingRowData d)
        {
            r.Label.text = d.Label;
            r.Hint.text = d.Hint ?? "";
            Show(r.Hint, !string.IsNullOrEmpty(d.Hint));
            bool stepper = d.Kind == SettingKind.Stepper;
            bool toggle = d.Kind == SettingKind.Toggle;
            int percent = 0;
            bool slider = stepper && TryPercent(d.Value, out percent);
            bool choice = stepper && !slider;
            Show(r.Minus, stepper); Show(r.Plus, stepper);
            r.MinusIcon.EnableInClassList("hd-ico--minus", slider); r.MinusIcon.EnableInClassList("hd-ico--back", choice);
            r.PlusIcon.EnableInClassList("hd-ico--plus", slider); r.PlusIcon.EnableInClassList("hd-ico--forward", choice);
            Show(r.Bar, slider); Show(r.Percent, slider);
            if (slider)
            {
                float filled = Mathf.Clamp(percent, 0, 100);
                r.Fill.style.width = Length.Percent(filled);
                r.Thumb.style.left = Length.Percent(filled);
                r.Percent.text = d.Value;
            }
            // Le libellé de valeur sert au sélecteur (entre les chevrons), à l'information et à l'état de l'interrupteur.
            Show(r.Value, choice || toggle || d.Kind == SettingKind.Info);
            r.Value.text = d.Value;
            r.Value.EnableInClassList("hd-setting__value--choice", choice);
            r.Value.EnableInClassList("hd-setting__value--state", toggle);
            r.Value.EnableInClassList("hd-setting__value--on", toggle && d.On);
            bool button = toggle || d.Kind == SettingKind.Button;
            Show(r.Action, button);
            if (!button) return;
            // Le texte du bouton reste lisible par les parcours de test ; l'interrupteur le remplace par son image (kit : toggle-on, toggle-off).
            HdButtonContent.Fill(r.Action, d.Value);
            r.Action.EnableInClassList("hd-setting__switch", toggle);
            r.Action.EnableInClassList("hd-setting__switch--on", toggle && d.On);
            r.Action.EnableInClassList("hd-btn--primary", !toggle && !d.Danger);
            r.Action.EnableInClassList("hd-btn--danger", !toggle && d.Danger);
            r.Action.EnableInClassList("hd-btn--secondary", false);
        }
    }
}
