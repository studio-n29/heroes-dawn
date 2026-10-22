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
    /// <summary>Combat (prefab Screens/Combat) : arène (ennemis en haut, alliés en bas), compétences, journal, bandeau.</summary>
    public sealed class CombatView : ViewBase, ICombatView
    {
        [SerializeField] UiButton retreat = null!;
        [SerializeField] UiButton auto = null!;
        [SerializeField] UiButton speed = null!;
        [SerializeField] UiButton debugWin = null!;
        [SerializeField] UiButton pause = null!;
        [SerializeField] TimelineView timeline = null!;
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
        public event Action? CycleClicked;
        public event Action<int>? SkillClicked;
        public event Action<int>? SkillDragBegan;
        public event Action<int, int>? SkillDropped;
        public event Action<int>? UnitTapped;

        /// <summary>Boutons Auto et vitesse (tests PlayMode : clics réels).</summary>
        public UnityEngine.UI.Button AutoButton => auto.Button;
        public UnityEngine.UI.Button SpeedButton => speed.Button;
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
            foreach (var c in enemyRow.GetComponentsInChildren<UnitCardView>(true)) list.Add(c);
            return list;
        }

        void Awake()
        {
            retreat.Button.onClick.AddListener(() => RetreatClicked?.Invoke());
            auto.Button.onClick.AddListener(() => AutoClicked?.Invoke());
            speed.Button.onClick.AddListener(() => SpeedClicked?.Invoke());
            debugWin.Button.onClick.AddListener(() => DebugWinClicked?.Invoke());
            pause.Button.onClick.AddListener(() => PauseClicked?.Invoke());
            for (int i = 0; i < fan.Length; i++)
            {
                int slot = i;
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
            if (Keys.Pressed(Key.A)) AutoClicked?.Invoke();
            if (Keys.Pressed(Key.S)) SpeedClicked?.Invoke();
            if (Keys.Pressed(Key.P)) PauseClicked?.Invoke();
            if (Keys.Pressed(Key.Tab)) CycleClicked?.Invoke();
        }

        public void BuildUnits(IReadOnlyList<UnitCardData> units)
        {
            ClearRows(enemyRow);
            ClearRows(allyRow);
            timeline.Clear();
            _cards.Clear();
            int allies = 0, enemies = 0;
            foreach (var u in units)
            {
                int index = u.Ally ? allies++ : enemies++;
                var card = Instantiate(u.Ally ? allyCardPrefab : unitCardPrefab, RowFor(u.Ally ? allyRow : enemyRow, index));
                card.Setup(u);
                card.Tapped += id => UnitTapped?.Invoke(id);
                _cards[u.Id] = card;
            }
        }

        /// <summary>Chaque camp est un bloc de deux rangées de deux cartes (croquis du combat) ; sans rangées, les cartes sont posées dans le bloc.</summary>
        static RectTransform RowFor(RectTransform block, int index)
            => block.childCount == 0 ? block : (RectTransform)block.GetChild(Mathf.Min(index / 2, block.childCount - 1));

        static void ClearRows(RectTransform block)
        {
            if (block.childCount == 0) return;
            for (int i = 0; i < block.childCount; i++) block.GetChild(i).Clear();
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
                // Trois cartes en éventail ; la 4e (carte du grimoire) n apparaît que si elle existe.
                if (i >= 3) fan[i].gameObject.SetActive(slotData.Visible && !slotData.Locked);
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
