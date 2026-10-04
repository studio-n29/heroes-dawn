#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ligne de réglage (widget prefab) : libellé + aide, puis « − valeur + », un interrupteur, un bouton ou une valeur.</summary>
    public sealed class SettingRowView : MonoBehaviour
    {
        [SerializeField] TMP_Text label = null!;
        [SerializeField] TMP_Text hint = null!;
        [SerializeField] TMP_Text value = null!;
        [SerializeField] UiButton minus = null!;
        [SerializeField] UiButton plus = null!;
        [SerializeField] UiButton action = null!;

        public event Action<int>? Acted;

        public UiButton Minus => minus;
        public UiButton Plus => plus;
        public UiButton Action => action;

        void Awake()
        {
            minus.Button.onClick.AddListener(() => Acted?.Invoke(0));
            plus.Button.onClick.AddListener(() => Acted?.Invoke(1));
            action.Button.onClick.AddListener(() => Acted?.Invoke(0));
        }

        public void Show(SettingRowData d)
        {
            label.text = d.Label;
            hint.text = d.Hint;
            hint.gameObject.SetActive(!string.IsNullOrEmpty(d.Hint));
            var lr = (RectTransform)label.transform;
            lr.anchorMin = new Vector2(0f, string.IsNullOrEmpty(d.Hint) ? 0f : 0.5f); // centré quand il n y a pas d aide
            bool stepper = d.Kind == SettingKind.Stepper;
            minus.gameObject.SetActive(stepper);
            plus.gameObject.SetActive(stepper);
            value.gameObject.SetActive(stepper || d.Kind == SettingKind.Info);
            value.text = d.Value;
            bool button = d.Kind == SettingKind.Toggle || d.Kind == SettingKind.Button;
            action.gameObject.SetActive(button);
            if (!button) return;
            action.SetLabel(d.Value);
            if (d.Kind == SettingKind.Toggle) action.SetColors(d.On ? Theme.Success : Theme.Surface3, d.On ? Theme.TextDark : Theme.Text);
            else action.SetColors(d.Danger ? Theme.Danger : Theme.Primary, d.Danger ? Color.white : Theme.TextDark);
        }
    }
}
