#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] UiButton combo = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text hint = null!;
        [SerializeField] TMP_Text logText = null!;
        [SerializeField] TMP_Text banner = null!;
        [SerializeField] CanvasGroup bannerGroup = null!;
        [SerializeField] RectTransform enemyRow = null!;
        [SerializeField] RectTransform allyRow = null!;
        [SerializeField] UnitCardView unitCardPrefab = null!;
        [SerializeField] UnitCardView allyCardPrefab = null!;
        [SerializeField] RectTransform fanRoot = null!;
        [SerializeField] SkillIconView[] fan = Array.Empty<SkillIconView>();
        [SerializeField, Mvp.OptionalRef] Tutorial.TutorialTargetMarker? fanMarker;
        [SerializeField] RectTransform descPanel = null!;
        [SerializeField] TMP_Text descTitle = null!;
        [SerializeField] TMP_Text descText = null!;
        [SerializeField, Mvp.OptionalRef] RectTransform? dropZone;

        readonly Dictionary<int, UnitCardView> _cards = new Dictionary<int, UnitCardView>();
        Coroutine? _bannerFade;
        int _skillUnit = -1;
        IReadOnlyList<UnitStateData> _lastStates = Array.Empty<UnitStateData>();
        Vector2[] _fanHome = Array.Empty<Vector2>();
        int _fanUnit = -1;

        public event Action? RetreatClicked;
        public event Action? AutoClicked;
        public event Action? SpeedClicked;
        public event Action? DebugWinClicked;
        public event Action? PauseClicked;
        public event Action<int>? AvatarClicked;
        public event Action? ComboClicked;
        public event Action? CycleClicked;
        public event Action<int>? SkillClicked;
        public event Action<int>? SkillDragBegan;
        public event Action<int, int>? SkillDropped;
        public event Action<int>? UnitTapped;

        /// <summary>Boutons Auto et vitesse (tests PlayMode : clics réels).</summary>
        public UnityEngine.UI.Button AutoButton => auto.Button;
        public UnityEngine.UI.Button SpeedButton => speed.Button;
        /// <summary>Bouton de la compétence d avatar <paramref name="i"/> (tests PlayMode : clics réels).</summary>
        public UnityEngine.UI.Button AvatarButton(int i) => avatarButtons[i].Button;
        public TimelineView Timeline => timeline;

        /// <summary>Icône de la compétence <paramref name="i"/> de l'agent prêt (tests PlayMode : clics réels).</summary>
        public UnityEngine.UI.Button SkillButton(int i) => fan[i].Button;

        /// <summary>Carte de l'éventail (visible seulement pour l'agent dont c'est le tour ; tests PlayMode).</summary>
        public SkillIconView SkillCard(int i) => fan[i];

        /// <summary>L'éventail de compétences est-il affiché ? (tests PlayMode)</summary>
        public bool FanVisible => fanRoot.gameObject.activeSelf;
        /// <summary>Les animations de combat (éventail, cartes jouées) sont-elles permises ? Faux avec « réduire les mouvements ».</summary>
        public bool AnimationsAllowed => !Feedback.MotionSettings.Reduced && UnityEngine.Application.isPlaying;
        /// <summary>Racine de l'éventail (tests PlayMode).</summary>
        public Transform FanRootForTests => fanRoot;

        /// <summary>Description affichée pendant l'appui maintenu sur une carte (tests PlayMode).</summary>
        public bool DescriptionVisible => descPanel.gameObject.activeSelf;
        public string DescriptionText => descText.text;

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
            for (int i = 0; i < fan.Length; i++)
            {
                int slot = i;
                fan[i].Clicked += () => SkillClicked?.Invoke(slot);
                fan[i].Holding += ShowDescription;
                fan[i].DragBegan += card => OnDragBegan(slot, card);
                fan[i].DragEnded += (card, at) => OnDragEnded(slot, at);
            }
            descPanel.gameObject.SetActive(false);
            _fanHome = new Vector2[fan.Length];
            for (int i = 0; i < fan.Length; i++) _fanHome[i] = ((RectTransform)fan[i].transform).anchoredPosition;
            fanRoot.gameObject.SetActive(false);
        }

        void OnDragBegan(int slot, SkillIconView card)
        {
            descPanel.gameObject.SetActive(false);
            if (dropZone != null) dropZone.gameObject.SetActive(!card.Data.NeedsTarget);
            SkillDragBegan?.Invoke(slot);
        }

        void OnDragEnded(int slot, Vector2 screenPos)
        {
            if (dropZone != null) dropZone.gameObject.SetActive(false);
            SkillDropped?.Invoke(slot, ResolveDrop(screenPos));
        }

        /// <summary>Où la carte est lâchée : sur l'éventail (annule), sur une carte d'unité (sa cible) ou dans le vide.</summary>
        int ResolveDrop(Vector2 screenPos)
        {
            var canvas = GetComponentInParent<Canvas>();
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.RectangleContainsScreenPoint(fanRoot, screenPos, cam)) return ICombatView.DropCancel;
            return UnitAt(screenPos) ?? ICombatView.DropTerrain;
        }

        /// <summary>Identifiant de l'unité dont la carte est sous ce point de l'écran (tests et dépose), null si aucune.</summary>
        public int? UnitAt(Vector2 screenPos)
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null) return null;
            var hits = new List<UnityEngine.EventSystems.RaycastResult>();
            es.RaycastAll(new UnityEngine.EventSystems.PointerEventData(es) { position = screenPos }, hits);
            foreach (var h in hits)
            {
                var c = h.gameObject.GetComponentInParent<UnitCardView>();
                if (c != null) return c.UnitId;
            }
            return null;
        }

        /// <summary>Position de l'écran du centre de la carte d'une unité (tests PlayMode : glisser sur une cible).</summary>
        public Vector2 UnitScreenPoint(int unitId)
        {
            var rt = (RectTransform)_cards[unitId].transform;
            var canvas = GetComponentInParent<Canvas>();
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            return RectTransformUtility.WorldToScreenPoint(cam, rt.TransformPoint(rt.rect.center));
        }

        /// <summary>Appui maintenu sur une carte : description de l'effet au-dessus de l'éventail, jusqu'au relâchement.</summary>
        void ShowDescription(SkillIconView card, bool held)
        {
            descPanel.gameObject.SetActive(held && card.Data.Visible && !card.Data.Locked);
            if (!held) return;
            var label = card.Data.Label ?? "";
            int close = label.IndexOf(']');
            descTitle.text = (close >= 0 ? label.Substring(close + 1) : label).Trim();
            descText.text = card.Data.Description ?? "";
        }

        void Update()
        {
            if (Keys.Pressed(Key.Digit1) || Keys.Pressed(Key.Numpad1)) SkillClicked?.Invoke(0);
            if (Keys.Pressed(Key.Digit2) || Keys.Pressed(Key.Numpad2)) SkillClicked?.Invoke(1);
            if (Keys.Pressed(Key.Digit3) || Keys.Pressed(Key.Numpad3)) SkillClicked?.Invoke(2);
            if (Keys.Pressed(Key.Digit4) || Keys.Pressed(Key.Numpad4)) SkillClicked?.Invoke(3);
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
                var card = Instantiate(u.Ally ? allyCardPrefab : unitCardPrefab, u.Ally ? allyRow : enemyRow);
                card.Setup(u);
                card.Tapped += id => UnitTapped?.Invoke(id);
                _cards[u.Id] = card;
            }
        }

        public void ShowUnits(IReadOnlyList<UnitStateData> states)
        {
            _lastStates = states;
            foreach (var s in states)
                if (_cards.TryGetValue(s.Id, out var c)) c.Refresh(s);
        }

        public void ShowSkills(SkillPanelData d)
        {
            // L'éventail du bas d'écran n'existe que pour l'agent dont c'est le tour ; il reprend les icônes calculées par UnitStateData.Skills.
            _skillUnit = d.Visible ? d.UnitId : -1;
            SkillSlotData[]? skills = null;
            foreach (var s in _lastStates) if (s.Id == _skillUnit) skills = s.Skills;
            bool show = d.Visible && skills != null;
            bool opening = show && (!fanRoot.gameObject.activeSelf || _fanUnit != _skillUnit);
            _fanUnit = show ? _skillUnit : -1;
            if (fanRoot.gameObject.activeSelf != show) fanRoot.gameObject.SetActive(show);
            if (!show) { descPanel.gameObject.SetActive(false); if (fanMarker != null) fanMarker.enabled = false; return; }
            bool usable = false;
            for (int i = 0; i < fan.Length; i++)
            {
                var slotData = i < skills!.Length ? skills[i] : default;
                fan[i].Show(slotData, i);
                usable |= slotData.Enabled || slotData.Pending;
            }
            if (fanMarker != null) fanMarker.enabled = usable;
            if (opening) AnimateFanIn();
        }

        /// <summary>Début du tour d'un agent : les cartes montent du bas de l'écran et s'ouvrent en éventail, l'une après l'autre.</summary>
        void AnimateFanIn()
        {
            if (!AnimationsAllowed) return;
            for (int i = 0; i < fan.Length && i < _fanHome.Length; i++)
            {
                var rt = (RectTransform)fan[i].transform;
                var home = _fanHome[i];
                rt.anchoredPosition = home + new Vector2(0f, -300f);
                rt.localScale = Vector3.one * 0.8f;
                Tween.Delay(i * 0.05f, () =>
                {
                    if (rt == null) return;
                    Tween.Run(0.28f, k =>
                    {
                        if (rt == null) return;
                        rt.anchoredPosition = Vector2.LerpUnclamped(home + new Vector2(0f, -300f), home, k);
                        rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.8f, 1f, k);
                    }, () => { if (rt != null) { rt.anchoredPosition = home; rt.localScale = Vector3.one; } }, rt, Tween.EaseOutBack);
                }, rt);
            }
        }

        /// <summary>Une carte est jouée : un double s'envole de l'éventail vers sa cible (ou le terrain), grossit puis s'éteint dans un éclat.</summary>
        public void PlayCard(int slot, int targetUnitId)
        {
            if (!AnimationsAllowed || slot < 0 || slot >= fan.Length) return;
            var src = fan[slot];
            if (!src.gameObject.activeInHierarchy) return;
            var canvas = GetComponentInParent<Canvas>();
            var layer = canvas != null ? canvas.rootCanvas.transform : transform;
            var ghost = Instantiate(src.gameObject, layer, false);
            foreach (var mb in ghost.GetComponentsInChildren<MonoBehaviour>(true))
                if (!(mb is UnityEngine.UI.Graphic) && !(mb is TMP_Text)) Destroy(mb);
            var grt = (RectTransform)ghost.transform;
            grt.position = src.transform.position;
            grt.rotation = src.transform.rotation;
            grt.localScale = src.transform.localScale;
            var cg = ghost.GetComponent<CanvasGroup>() ?? ghost.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            RectTransform? targetRect = null;
            Vector3 to = enemyRow.position;
            if (targetUnitId >= 0 && _cards.TryGetValue(targetUnitId, out var tc)) { targetRect = (RectTransform)tc.transform; to = targetRect.TransformPoint(targetRect.rect.center); }
            var from = grt.position;
            var fromRot = grt.rotation;
            Tween.Run(0.32f, k =>
            {
                if (grt == null) return;
                grt.position = Vector3.LerpUnclamped(from, to, k);
                grt.rotation = Quaternion.Slerp(fromRot, Quaternion.identity, k);
                grt.localScale = Vector3.one * Mathf.Lerp(1f, 1.35f, k);
            }, () =>
            {
                if (grt == null) return;
                Feedback.Fx.Sparks(targetRect != null ? targetRect : enemyRow, Theme.Glow, 12, 130f, 18f);
                Tween.Fade(cg, 0f, 0.14f, () => { if (ghost != null) Destroy(ghost); });
            }, grt, Tween.EaseOutCubic);
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
