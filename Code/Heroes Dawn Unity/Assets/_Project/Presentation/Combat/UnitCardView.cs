#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Carte d'une unité en combat (widget prefab) : portrait, PV, jauge ATB, statuts, états (prête, sélectionnée, ciblable, K.O.).</summary>
    public sealed class UnitCardView : MonoBehaviour
    {
        [SerializeField] Image fill = null!;
        [SerializeField] Image select = null!;
        [SerializeField] Image target = null!;
        [SerializeField] Image strip = null!;
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text unitName = null!;
        [SerializeField] UiBar hp = null!;
        [SerializeField] UiBar gauge = null!;
        [SerializeField] TMP_Text statuses = null!;
        [SerializeField] TMP_Text sub = null!;
        [SerializeField] Image portraitArea = null!;
        [SerializeField] TMP_Text ko = null!;
        [SerializeField] Pulse readyPulse = null!;
        [SerializeField] CanvasGroup group = null!;
        [SerializeField] Button button = null!;
        [SerializeField] LayoutElement layout = null!;
        [SerializeField] FloatingTextView floatPrefab = null!;

        Color _baseFill;
        bool _ally;

        public int UnitId { get; private set; }
        public Button Button => button;
        public event Action<int>? Tapped;

        void Awake() => button.onClick.AddListener(() => Tapped?.Invoke(UnitId));

        public void Setup(UnitCardData d)
        {
            UnitId = d.Id;
            _ally = d.Ally;
            _baseFill = d.Ally ? Theme.Surface : Theme.Hex(0x2A1C22);
            fill.color = _baseFill;
            layout.preferredWidth = layout.minWidth = 250;
            layout.preferredHeight = layout.minHeight = 290;
            sub.text = d.Sub ?? "";
            portraitArea.color = Color.Lerp(Theme.FamilyColor(d.Family), Color.black, d.Ally ? 0.55f : 0.72f);
            strip.color = Theme.FamilyColor(d.Family);
            portrait.Show(d.Portrait);
            unitName.text = d.Name;
            unitName.color = d.Boss ? Theme.Primary : Color.white;
            hp.SetColor(d.Ally ? Theme.Hp : Theme.HpEnemy);
            name = (d.Ally ? "Ally_" : "Enemy_") + d.Id;
        }

        public void Refresh(UnitStateData s)
        {
            hp.Set(s.Hp01, s.Instant);
            hp.SetLabel(s.HpText);
            gauge.Set(s.Gauge01, true);
            gauge.SetColor(s.GaugeFull ? Color.white : Theme.Gauge);
            statuses.text = s.Statuses;
            ko.gameObject.SetActive(!s.Alive);
            group.alpha = s.Alive ? 1f : 0.45f;
            fill.color = s.Highlight && s.Alive ? Color.Lerp(_baseFill, Theme.Primary, 0.22f) : _baseFill;
            readyPulse.enabled = s.Highlight && s.Alive;
            select.gameObject.SetActive(s.Selected && s.Alive);
            target.gameObject.SetActive(s.Targetable && s.Alive);
            target.color = _ally ? Theme.Success : Theme.Danger;
        }

        public void Float(string text, Color color, int size) => Instantiate(floatPrefab, transform).Play(text, color, size);
        public void Hit() => Tween.Shake((RectTransform)transform, 12f, 0.25f);
        public void Act() => Tween.Punch(transform, 0.08f, 0.25f);
    }
}
