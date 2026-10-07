#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Combat (prefab Screens/Combat) : arène (ennemis en haut, alliés en bas), compétences, combo, journal, bandeau.</summary>
    public sealed class CombatView : ViewBase, ICombatView
    {
        [SerializeField] UiButton retreat = null!;
        [SerializeField] UiButton auto = null!;
        [SerializeField] UiButton speed = null!;
        [SerializeField] UiButton debugWin = null!;
        [SerializeField] UiButton pause = null!;
        [SerializeField] RectTransform avatarPanel = null!;
        [SerializeField] UiButton[] avatarButtons = Array.Empty<UiButton>();
        [SerializeField] TimelineView timeline = null!;
        [SerializeField] TMP_Text skillTitle = null!;
        [SerializeField] UiButton combo = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text hint = null!;
        [SerializeField] TMP_Text logText = null!;
        [SerializeField] TMP_Text banner = null!;
        [SerializeField] CanvasGroup bannerGroup = null!;
        [SerializeField] RectTransform enemyRow = null!;
        [SerializeField] RectTransform allyRow = null!;
        [SerializeField] RectTransform skillPanel = null!;
        [SerializeField] SkillButtonView[] skills = Array.Empty<SkillButtonView>();
        [SerializeField] UnitCardView unitCardPrefab = null!;

        readonly Dictionary<int, UnitCardView> _cards = new Dictionary<int, UnitCardView>();
        Coroutine? _bannerFade;

        public event Action? RetreatClicked;
        public event Action? AutoClicked;
        public event Action? SpeedClicked;
        public event Action? DebugWinClicked;
        public event Action? PauseClicked;
        public event Action<int>? AvatarClicked;
        public event Action? ComboClicked;
        public event Action? CycleClicked;
        public event Action<int>? SkillClicked;
        public event Action<int>? UnitTapped;

        /// <summary>Boutons Auto et vitesse (tests PlayMode : clics réels).</summary>
        public UnityEngine.UI.Button AutoButton => auto.Button;
        public UnityEngine.UI.Button SpeedButton => speed.Button;
        /// <summary>Bouton de la compétence d avatar <paramref name="i"/> (tests PlayMode : clics réels).</summary>
        public UnityEngine.UI.Button AvatarButton(int i) => avatarButtons[i].Button;
        public TimelineView Timeline => timeline;

        /// <summary>Bouton de la compétence <paramref name="i"/> (tests PlayMode : clics réels).</summary>
        public UnityEngine.UI.Button SkillButton(int i) => skills[i].ClickButton;

        /// <summary>Cartes ennemies (tests PlayMode : cibler au toucher).</summary>
        public IReadOnlyList<UnitCardView> EnemyCards()
        {
            var list = new List<UnitCardView>();
            foreach (Transform t in enemyRow) { var c = t.GetComponent<UnitCardView>(); if (c != null) list.Add(c); }
            return list;
        }

        void Awake()
        {
            retreat.Button.onClick.AddListener(() => RetreatClicked?.Invoke());
            auto.Button.onClick.AddListener(() => AutoClicked?.Invoke());
            speed.Button.onClick.AddListener(() => SpeedClicked?.Invoke());
            debugWin.Button.onClick.AddListener(() => DebugWinClicked?.Invoke());
            pause.Button.onClick.AddListener(() => PauseClicked?.Invoke());
            for (int i = 0; i < avatarButtons.Length; i++)
            {
                int index = i;
                avatarButtons[i].Button.onClick.AddListener(() => AvatarClicked?.Invoke(index));
            }
            combo.Button.onClick.AddListener(() => ComboClicked?.Invoke());
            for (int i = 0; i < skills.Length; i++)
            {
                int slot = i;
                skills[i].Clicked += () => SkillClicked?.Invoke(slot);
            }
        }

        void Update()
        {
            if (Keys.Pressed(Key.Digit1) || Keys.Pressed(Key.Numpad1)) SkillClicked?.Invoke(0);
            if (Keys.Pressed(Key.Digit2) || Keys.Pressed(Key.Numpad2)) SkillClicked?.Invoke(1);
            if (Keys.Pressed(Key.Digit3) || Keys.Pressed(Key.Numpad3)) SkillClicked?.Invoke(2);
            if (Keys.Pressed(Key.Q)) AvatarClicked?.Invoke(0);
            if (Keys.Pressed(Key.W)) AvatarClicked?.Invoke(1);
            if (Keys.Pressed(Key.E)) AvatarClicked?.Invoke(2);
            if (Keys.Pressed(Key.R)) AvatarClicked?.Invoke(3);
            if (Keys.Pressed(Key.T)) AvatarClicked?.Invoke(4);
            if (Keys.Pressed(Key.C) && combo.gameObject.activeSelf) ComboClicked?.Invoke();
            if (Keys.Pressed(Key.A)) AutoClicked?.Invoke();
            if (Keys.Pressed(Key.S)) SpeedClicked?.Invoke();
            if (Keys.Pressed(Key.P)) PauseClicked?.Invoke();
            if (Keys.Pressed(Key.Tab)) CycleClicked?.Invoke();
        }

        public void BuildUnits(IReadOnlyList<UnitCardData> units)
        {
            enemyRow.Clear();
            allyRow.Clear();
            timeline.Clear();
            _cards.Clear();
            foreach (var u in units)
            {
                var card = Instantiate(unitCardPrefab, u.Ally ? allyRow : enemyRow);
                card.Setup(u);
                card.Tapped += id => UnitTapped?.Invoke(id);
                _cards[u.Id] = card;
            }
        }

        public void ShowUnits(IReadOnlyList<UnitStateData> states)
        {
            foreach (var s in states)
                if (_cards.TryGetValue(s.Id, out var c)) c.Refresh(s);
        }

        public void ShowSkills(SkillPanelData d)
        {
            skillPanel.gameObject.SetActive(d.Visible);
            if (!d.Visible) return;
            skillTitle.text = d.Title;
            for (int i = 0; i < skills.Length; i++) skills[i].Show(i < d.Slots.Length ? d.Slots[i] : default, i);
        }

        public void ShowTimeline(TimelineData d)
        {
            timeline.Show(d);
        }

        public void ShowAvatar(AvatarBarData d)
        {
            avatarPanel.gameObject.SetActive(d.Visible);
            if (!d.Visible) return;
            for (int i = 0; i < avatarButtons.Length; i++)
            {
                var b = avatarButtons[i];
                bool has = d.Slots != null && i < d.Slots.Length && d.Slots[i].Visible;
                if (b.gameObject.activeSelf != has) b.gameObject.SetActive(has);
                if (!has) continue;
                var s = d.Slots![i];
                b.SetLabel(s.Label);
                var bg = s.Pending ? Color.white : AvatarColor(s.Category);
                b.SetColors(bg, Theme.TextDark);
                b.Interactable = s.Enabled;
            }
        }

        static Color AvatarColor(N29.HeroesDawn.Definitions.SkillCategory c)
        {
            switch (c)
            {
                case N29.HeroesDawn.Definitions.SkillCategory.Heal: return Theme.Success;
                case N29.HeroesDawn.Definitions.SkillCategory.Protection: return Theme.Info;
                case N29.HeroesDawn.Definitions.SkillCategory.Boost: return Theme.Primary;
                case N29.HeroesDawn.Definitions.SkillCategory.TimeManagement: return Theme.Perception;
                default: return Theme.Danger;
            }
        }

        public void ShowCombo(bool visible, string label)
        {
            if (combo.gameObject.activeSelf != visible) combo.gameObject.SetActive(visible);
            if (visible) combo.SetLabel(label);
        }

        public void ShowTopBar(CombatTopBarData d)
        {
            title.text = d.Title;
            auto.SetColors(d.AutoOn ? Theme.Primary : Theme.Surface3, d.AutoOn ? Theme.TextDark : Theme.Text);
            speed.SetLabel(d.SpeedText);
            retreat.Interactable = d.RetreatEnabled;
            if (debugWin.gameObject.activeSelf != d.DebugVisible) debugWin.gameObject.SetActive(d.DebugVisible);
        }

        public void SetHint(string text)
        {
            hint.text = text;
            hint.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        public void ShowBanner(string text, Tone tone)
        {
            banner.text = text;
            banner.color = ToneColors.Of(tone);
            if (tone == Tone.Danger || tone == Tone.Primary) Tween.Punch(transform, 0.018f, 0.3f); // coup de caméra sur COMBO / VICTOIRE
            Tween.PopIn(banner.rectTransform, 0.3f);
            bannerGroup.alpha = 1f;
            if (_bannerFade != null) Tween.Stop(_bannerFade);
            _bannerFade = Tween.Delay(1.2f, () => Tween.Fade(bannerGroup, 0f, 0.4f), banner);
        }

        public void ShowLog(IReadOnlyList<string> lines) => logText.text = string.Join("\n", lines);

        public void Float(int unitId, string text, Tone tone, int size)
        {
            if (_cards.TryGetValue(unitId, out var c)) c.Float(text, ToneColors.Of(tone), size);
        }

        public void Hit(int unitId) { if (_cards.TryGetValue(unitId, out var c)) c.Hit(); }
        public void Act(int unitId) { if (_cards.TryGetValue(unitId, out var c)) c.Act(); }

        public UniTask PauseAsync(float seconds, CancellationToken ct)
            => UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: true, cancellationToken: ct);
    }
}
