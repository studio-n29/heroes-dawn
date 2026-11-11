#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Paramètres (UI Toolkit) : catégories à gauche, réglages de la catégorie à droite.</summary>
    public sealed class SettingsPageView : HdView, ISettingsPageView
    {
        /// <summary>Ce que les tests lisent d'une ligne de réglage affichée.</summary>
        public sealed class RowInfo { public string Label = ""; public bool Visible; }

        sealed class Row
        {
            public VisualElement Root = null!;
            public Label Label = null!, Hint = null!, Value = null!;
            public UnityEngine.UIElements.Button Minus = null!, Plus = null!, Action = null!;
        }

        readonly List<Row> _pool = new List<Row>();
        readonly List<RowInfo> _infos = new List<RowInfo>();
        readonly List<UnityEngine.UIElements.Button> _cats = new List<UnityEngine.UIElements.Button>();
        ScrollView _catBox = null!, _rows = null!;
        Label _title = null!;
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

        protected override void Bind()
        {
            _catBox = Q<ScrollView>("categories"); _rows = Q<ScrollView>("rows"); _title = Q<Label>("title");
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
                b.AddToClassList("hd-btn"); b.AddToClassList("hd-settings__cat");
                b.clicked += () => CategorySelected?.Invoke(index);
                _cats.Add(b); _catBox.Add(b);
            }
            for (int i = 0; i < _cats.Count; i++)
            {
                bool used = i < d.Categories.Length;
                _cats[i].style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used) continue;
                _cats[i].text = d.Categories[i];
                bool active = i == d.Active;
                _cats[i].EnableInClassList("hd-btn--primary", active);
                _cats[i].EnableInClassList("hd-btn--secondary", !active);
            }
            _title.text = d.CategoryTitle;
            _reset.style.display = d.CanReset ? DisplayStyle.Flex : DisplayStyle.None;
            _reset.text = d.ResetLabel;

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

        Row NewRow(int index)
        {
            var r = new Row { Root = new VisualElement() };
            r.Root.AddToClassList("hd-setting");
            var text = new VisualElement { pickingMode = PickingMode.Ignore };
            text.AddToClassList("hd-setting__text");
            r.Label = new Label { pickingMode = PickingMode.Ignore }; r.Label.AddToClassList("hd-setting__label");
            r.Hint = new Label { pickingMode = PickingMode.Ignore }; r.Hint.AddToClassList("hd-setting__hint");
            text.Add(r.Label); text.Add(r.Hint);
            r.Minus = Step("-", () => RowAction?.Invoke(index, 0));
            r.Value = new Label { pickingMode = PickingMode.Ignore }; r.Value.AddToClassList("hd-setting__value");
            r.Plus = Step("+", () => RowAction?.Invoke(index, 1));
            r.Action = new UnityEngine.UIElements.Button();
            r.Action.AddToClassList("hd-btn"); r.Action.AddToClassList("hd-btn--small"); r.Action.AddToClassList("hd-setting__action");
            r.Action.clicked += () => RowAction?.Invoke(index, 0);
            r.Root.Add(text); r.Root.Add(r.Minus); r.Root.Add(r.Value); r.Root.Add(r.Plus); r.Root.Add(r.Action);
            return r;
        }

        static UnityEngine.UIElements.Button Step(string label, Action click)
        {
            var b = new UnityEngine.UIElements.Button { text = label };
            b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--secondary"); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-setting__step");
            b.clicked += click;
            return b;
        }

        static void Apply(Row r, SettingRowData d)
        {
            r.Label.text = d.Label;
            r.Hint.text = d.Hint ?? "";
            r.Hint.style.display = string.IsNullOrEmpty(d.Hint) ? DisplayStyle.None : DisplayStyle.Flex;
            bool stepper = d.Kind == SettingKind.Stepper;
            r.Minus.style.display = r.Plus.style.display = stepper ? DisplayStyle.Flex : DisplayStyle.None;
            r.Value.style.display = stepper || d.Kind == SettingKind.Info ? DisplayStyle.Flex : DisplayStyle.None;
            r.Value.text = d.Value;
            bool button = d.Kind == SettingKind.Toggle || d.Kind == SettingKind.Button;
            r.Action.style.display = button ? DisplayStyle.Flex : DisplayStyle.None;
            if (!button) return;
            r.Action.text = d.Value;
            bool toggle = d.Kind == SettingKind.Toggle;
            r.Action.EnableInClassList("hd-btn--primary", (!toggle && !d.Danger) || (toggle && d.On));
            r.Action.EnableInClassList("hd-btn--danger", !toggle && d.Danger);
            r.Action.EnableInClassList("hd-btn--secondary", toggle && !d.On);
        }
    }
}
