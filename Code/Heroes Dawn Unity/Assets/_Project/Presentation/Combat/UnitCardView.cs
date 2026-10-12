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
        [SerializeField] SkillIconView[] icons = Array.Empty<SkillIconView>();
        [SerializeField, Mvp.OptionalRef] Tutorial.TutorialTargetMarker? readyMarker;

        Color _baseFill;
        Color _nameColor = Color.white;
        bool _ally;

        public int UnitId { get; private set; }
        public Button Button => button;
        public event Action<int>? Tapped;
        /// <summary>Un des quatre icônes de compétence de l'agent a été touchée (emplacement 0 à 3).</summary>
        public event Action<int>? SkillClicked;

        public SkillIconView Icon(int i) => icons[i];

        void Awake()
        {
            _nameColor = unitName.color;
            button.onClick.AddListener(() => Tapped?.Invoke(UnitId));
            for (int i = 0; i < icons.Length; i++)
            {
                int slot = i;
                icons[i].Clicked += () => SkillClicked?.Invoke(slot);
            }
        }

        public void Setup(UnitCardData d)
        {
            UnitId = d.Id;
            _ally = d.Ally;
            _baseFill = d.Ally ? Theme.Surface : Color.Lerp(Theme.Surface, Theme.AccentSoft, 0.35f);
            fill.color = _baseFill;
            sub.text = d.Sub ?? "";
            portraitArea.color = Color.Lerp(Theme.FamilyColor(d.Family), Theme.Navy, d.Ally ? 0.25f : 0.4f);
            strip.color = Theme.FamilyColor(d.Family);
            portrait.Show(d.Portrait);
            unitName.text = d.Name;
            unitName.color = d.Boss ? Theme.Primary : _nameColor;
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
            for (int i = 0; i < icons.Length; i++)
                icons[i].Show(s.Skills != null && i < s.Skills.Length ? s.Skills[i] : default, i);
            bool usable = false;
            if (s.Skills != null) foreach (var sk in s.Skills) usable |= sk.Enabled || sk.Pending;
            if (readyMarker != null) readyMarker.enabled = usable && s.Alive;
        }

        public void Float(string text, Color color, int size) => Instantiate(floatPrefab, transform).Play(text, color, size);
        public void Hit()
        {
            Tween.Shake((RectTransform)transform, 12f, 0.25f);
            Feedback.Fx.Flash(portraitArea);
            Feedback.Fx.Sparks((RectTransform)transform, _ally ? Theme.Danger : Theme.Primary, 7, 90f, 14f);
        }
        public void Act() => Tween.Punch(transform, 0.08f, 0.25f);
    }
}
