#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Repos (fenêtre UI Toolkit) : fatigue avant → après de l'escouade, Passer / Se reposer. Non annulable.</summary>
    public sealed class RestPopupView : HdPopup, IRestPopupView
    {
        Label _title = null!, _body = null!, _fatigueTitle = null!;
        VisualElement _fatigue = null!;
        UnityEngine.UIElements.Button _skip = null!, _take = null!;
        UniTaskCompletionSource<bool>? _choice;
        RestPopupData? _last;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        public HdButtonRef SkipButton { get; private set; } = null!;
        public HdButtonRef TakeButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _take;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _body = Q<Label>("body"); _fatigueTitle = Q<Label>("fatigueTitle");
            _fatigue = Q<VisualElement>("fatigue");
            _skip = Q<UnityEngine.UIElements.Button>("skip"); _take = Q<UnityEngine.UIElements.Button>("take");
            SkipButton = new HdButtonRef(_skip, this); TakeButton = new HdButtonRef(_take, this);
            _skip.clicked += () => Choose(false);
            _take.clicked += () => Choose(true);
            if (_last.HasValue) Fill(_last.Value);
        }

        void Choose(bool value)
        {
            _skip.SetEnabled(false); _take.SetEnabled(false); // une seule résolution, même en cas de double clic
            _choice?.TrySetResult(value);
        }

        void Fill(RestPopupData d)
        {
            _title.text = d.Title; _body.text = d.Body;
            bool rows = d.Fatigue != null && d.Fatigue.Length > 0;
            _fatigueTitle.style.display = rows ? DisplayStyle.Flex : DisplayStyle.None;
            _fatigue.style.display = rows ? DisplayStyle.Flex : DisplayStyle.None;
            _fatigueTitle.text = d.FatigueTitle;
            FillFatigue(d.Fatigue);
            _skip.text = d.Skip; _take.text = d.Take;
            _skip.SetEnabled(true); _take.SetEnabled(true);
        }

        /// <summary>Une ligne par agent : nom, barre de fatigue actuelle, valeur actuelle, flèche, valeur après repos.</summary>
        void FillFatigue(N29.HeroesDawn.Presentation.Combat.FatigueRowData[]? rows)
        {
            _fatigue.Clear();
            if (rows == null) return;
            foreach (var r in rows)
            {
                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("hd-m-rest__row");
                var name = new Label(r.Name) { pickingMode = PickingMode.Ignore };
                name.AddToClassList("hd-m-rest__name");
                var bar = new VisualElement { pickingMode = PickingMode.Ignore };
                bar.AddToClassList("hd-bar"); bar.AddToClassList("hd-m-rest__bar");
                var fill = new VisualElement { pickingMode = PickingMode.Ignore };
                fill.AddToClassList("hd-bar__fill");
                float before = Mathf.Clamp01(r.Before01);
                fill.style.width = Length.Percent(before * 100f);
                fill.style.unityBackgroundImageTintColor = Theme.FatigueColor(Mathf.RoundToInt(before * 100f));
                bar.Add(fill);
                row.Add(name); row.Add(bar);
                // Le libellé reçu est « avant → après » : les deux valeurs encadrent la flèche du kit.
                string label = r.Label ?? "";
                int arrow = label.IndexOf('→');
                var current = new Label(arrow >= 0 ? label.Substring(0, arrow).Trim() : label) { pickingMode = PickingMode.Ignore };
                current.AddToClassList("hd-m-rest__value");
                row.Add(current);
                if (arrow >= 0)
                {
                    var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                    icon.AddToClassList("hd-m-rest__arrow");
                    var after = new Label(label.Substring(arrow + 1).Trim()) { pickingMode = PickingMode.Ignore };
                    after.AddToClassList("hd-m-rest__value"); after.AddToClassList("hd-m-rest__value--after");
                    row.Add(icon); row.Add(after);
                }
                _fatigue.Add(row);
            }
        }

        public UniTask<bool> ChooseAsync(RestPopupData d, CancellationToken ct)
        {
            _last = d;
            if (_title != null) Fill(d);
            _choice = new UniTaskCompletionSource<bool>();
            ct.Register(() => _choice.TrySetCanceled());
            return _choice.Task;
        }
    }
}
