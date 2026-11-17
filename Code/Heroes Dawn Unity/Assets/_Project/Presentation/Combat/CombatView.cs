#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;
using UnityEngine.InputSystem;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Combat (prefab Screens/Combat) : fond uGUI et HUD UI Toolkit (<see cref="CombatHudView"/>) portant l'arène, les cartes, la frise, l'éventail, le journal et le bandeau.</summary>
    public sealed partial class CombatView : ViewBase, ICombatView
    {
        [SerializeField] CombatHudView hud = null!;

        int _skillUnit = -1;
        IReadOnlyList<UnitStateData> _lastStates = Array.Empty<UnitStateData>();
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
        public event Action<int, int>? SkillDragOver;
        int _lastDragOver = int.MinValue;

        /// <summary>Boutons Auto et vitesse (tests PlayMode : clics réels).</summary>
        public UiToolkit.HdButtonRef AutoButton => hud.AutoButton;
        public UiToolkit.HdButtonRef SpeedButton => hud.SpeedButton;
        /// <summary>HUD UI Toolkit (barre du haut, terrain, frise, éventail ; tests PlayMode).</summary>
        public CombatHudView Hud => hud;
        public TimelineHud Timeline => hud.Timeline;

        /// <summary>Carte de compétence de l'agent prêt (visible seulement pour l'agent dont c'est le tour ; tests PlayMode).</summary>
        public SkillCardHud SkillCard(int i) => hud.SkillCard(i);

        /// <summary>L'éventail de compétences est-il affiché ? (tests PlayMode)</summary>
        public bool FanVisible => hud.FanVisible;
        /// <summary>Les animations de combat (éventail, cartes jouées) sont-elles permises ? Faux avec « réduire les mouvements ».</summary>
        public bool AnimationsAllowed => HudAnim.Allowed;

        /// <summary>Description affichée pendant l'appui maintenu sur une carte (tests PlayMode).</summary>
        public bool DescriptionVisible => hud.DescriptionVisible;
        public string DescriptionText => hud.DescriptionText;

        /// <summary>Cartes ennemies (tests PlayMode : cibler au toucher).</summary>
        public IReadOnlyList<UnitCardHud> EnemyCards()
        {
            var list = new List<UnitCardHud>();
            foreach (var c in hud.Cards) if (c.Root.name.StartsWith("Enemy_", StringComparison.Ordinal)) list.Add(c);
            return list;
        }

        void Awake()
        {
            hud.RetreatClicked += () => RetreatClicked?.Invoke();
            hud.AutoClicked += () => AutoClicked?.Invoke();
            hud.SpeedClicked += () => SpeedClicked?.Invoke();
            hud.DebugWinClicked += () => DebugWinClicked?.Invoke();
            hud.PauseClicked += () => PauseClicked?.Invoke();
            hud.UnitTapped += id => UnitTapped?.Invoke(id);
            hud.SkillHolding += ShowDescription;
            hud.SkillDragBegan += OnDragBegan;
            hud.SkillDragEnded += (card, at) => OnDragEnded(SlotOf(card), at);
            hud.SkillDragMoved += (card, at) => OnDragMoved(SlotOf(card), at);
        }

        /// <summary>L'unité sous la carte glissée change : le présentateur recalcule l'aperçu (une seule levée par changement de cible).</summary>
        void OnDragMoved(int slot, Vector2 screenPos)
        {
            int over = ResolveDrop(screenPos);
            if (over == _lastDragOver) return;
            _lastDragOver = over;
            SkillDragOver?.Invoke(slot, over);
        }

        public void ShowPreview(CombatPreviewData? data) => hud.ShowPreview(data);

        int SlotOf(SkillCardHud card)
        {
            for (int i = 0; i < 4; i++) if (hud.SkillCard(i) == card) return i;
            return -1;
        }

        void OnDragBegan(SkillCardHud card)
        {
            hud.SetDescription(false, "", "");
            _lastDragOver = int.MinValue;
            hud.SetDropZone(!card.Data.NeedsTarget);
            SkillDragBegan?.Invoke(SlotOf(card));
        }

        void OnDragEnded(int slot, Vector2 screenPos)
        {
            hud.SetDropZone(false);
            _lastDragOver = int.MinValue;
            SkillDropped?.Invoke(slot, ResolveDrop(screenPos));
        }

        /// <summary>Où la carte est lâchée : sur l'éventail (annule), sur une carte d'unité (sa cible) ou dans le vide.</summary>
        int ResolveDrop(Vector2 screenPos)
        {
            if (hud.IsOverFan(screenPos)) return ICombatView.DropCancel;
            return UnitAt(screenPos) ?? ICombatView.DropTerrain;
        }

        /// <summary>Identifiant de l'unité dont la carte est sous ce point de l'écran (tests et dépose), null si aucune.</summary>
        public int? UnitAt(Vector2 screenPos) => hud.UnitAt(screenPos);

        /// <summary>Position de l'écran du centre de la carte d'une unité (tests PlayMode : glisser sur une cible).</summary>
        public Vector2 UnitScreenPoint(int unitId) => hud.TryCard(unitId, out var c) ? c.ScreenCenter() : Vector2.zero;

        /// <summary>Appui maintenu sur une carte : description de l'effet au-dessus de l'éventail, jusqu'au relâchement.</summary>
        void ShowDescription(SkillCardHud card, bool held)
        {
            bool show = held && card.Data.Visible && !card.Data.Locked;
            var label = card.Data.Label ?? "";
            int close = label.IndexOf(']');
            hud.SetDescription(show, (close >= 0 ? label.Substring(close + 1) : label).Trim(), card.Data.Description ?? "");
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
            hud.ClearTimeline();
            hud.BuildUnits(units);
        }

        public void ShowUnits(IReadOnlyList<UnitStateData> states)
        {
            _lastStates = states;
            hud.ShowStates(states);
        }

        public void ShowSkills(SkillPanelData d)
        {
            // L'éventail du bas d'écran n'existe que pour l'agent dont c'est le tour ; il reprend les icônes calculées par UnitStateData.Skills.
            _skillUnit = d.Visible ? d.UnitId : -1;
            SkillSlotData[]? skills = null;
            foreach (var s in _lastStates) if (s.Id == _skillUnit) skills = s.Skills;
            bool show = d.Visible && skills != null;
            bool opening = show && (!hud.FanVisible || _fanUnit != _skillUnit);
            _fanUnit = show ? _skillUnit : -1;
            bool usable = false;
            if (show) foreach (var slot in skills!) usable |= slot.Enabled || slot.Pending;
            hud.ShowFan(show, skills, opening, usable);
        }

        /// <summary>Une carte est jouée : un double s'envole de l'éventail vers sa cible (ou le terrain), grossit puis s'éteint dans un éclat.</summary>
        public void PlayCard(int slot, int targetUnitId) => hud.PlayCard(slot, targetUnitId, hud.EnemyCenterPanel());

        public void ShowTimeline(TimelineData d) => hud.ShowTimeline(d);

        public void ShowTopBar(CombatTopBarData d) => hud.SetTopBar(d);

        public void SetHint(string text) => hud.SetHint(text);

        public void ShowBanner(string text, Tone tone)
        {
            hud.ShowBanner(text, ToneColors.Of(tone));
            if (tone == Tone.Danger || tone == Tone.Primary) Tween.Punch(transform, 0.018f, 0.3f); // coup de caméra sur COMBO / VICTOIRE
        }

        public void ShowLog(IReadOnlyList<string> lines) => hud.SetLog(lines);

        public void ShowBoss(BossBarData? data, bool phaseChanged = false)
        {
            hud.ShowBoss(data, phaseChanged);
            if (phaseChanged) Tween.Punch(transform, 0.01f, 0.5f);
        }

        public void ShowBonuses(IReadOnlyList<BonusChipData> bonuses) => hud.ShowBonuses(bonuses);

        public void Float(int unitId, string text, Tone tone, int size)
        {
            if (hud.TryCard(unitId, out var c)) c.Float(text, ToneColors.Of(tone), size);
        }

        public void Hit(int unitId) { if (hud.TryCard(unitId, out var c)) c.Hit(hud.FxLayer); }

        public void Act(int unitId)
        {
            if (!hud.TryCard(unitId, out var c)) return;
            c.Act();
            hud.ActingUnit(unitId);
        }

        public UniTask PauseAsync(float seconds, CancellationToken ct)
            => UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: true, cancellationToken: ct);
    }
}
