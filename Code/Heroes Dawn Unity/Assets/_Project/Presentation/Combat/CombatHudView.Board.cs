#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Terrain du combat en UI Toolkit : cartes d'unités, frise, éventail de compétences (glisser-déposer), description, zone de dépôt, effets.</summary>
    public sealed partial class CombatHudView
    {
        const int FanSlots = 4;
        /// <summary>Largeur de la zone des cartes, largeur d'une carte et pas entre deux cartes (pixels de référence, voir .hd-hud__fan et .hd-skill).</summary>
        const float FanWidth = 760f, FanCardWidth = 180f, FanStep = 190f;

        VisualElement _allies = null!, _enemies = null!, _fan = null!, _fanAnchor = null!, _desc = null!, _drop = null!, _fx = null!, _spotlight = null!;
        Label _descTitle = null!, _descText = null!, _arenaTitle = null!;
        readonly Dictionary<int, UnitCardHud> _cards = new Dictionary<int, UnitCardHud>();
        readonly List<UnitCardHud> _cardList = new List<UnitCardHud>();
        readonly List<SkillCardHud> _fanCards = new List<SkillCardHud>();
        IReadOnlyList<UnitCardData>? _units;
        IReadOnlyList<UnitStateData> _states = Array.Empty<UnitStateData>();
        TimelineData? _timelineData;
        bool _fanShown, _fanUsable;
        SkillSlotData[]? _fanSkills;
        Sprite? _spotlightArt;

        public TimelineHud Timeline { get; private set; } = null!;
        public event Action<int>? UnitTapped;
        public event Action<SkillCardHud, bool>? SkillHolding;
        public event Action<SkillCardHud>? SkillDragBegan;
        public event Action<SkillCardHud, Vector2>? SkillDragEnded;

        public bool FanVisible => _fanShown;
        public IReadOnlyList<UnitCardHud> Cards => _cardList;
        public SkillCardHud SkillCard(int i) => _fanCards[i];
        public bool DescriptionVisible => _desc != null && _desc.style.display == DisplayStyle.Flex;
        public string DescriptionText => _descText != null ? _descText.text : "";
        public bool TryCard(int id, out UnitCardHud card) => _cards.TryGetValue(id, out card!);
        public VisualElement FxLayer => _fx;

        void BindBoard()
        {
            _allies = Q<VisualElement>("allies"); _enemies = Q<VisualElement>("enemies");
            _fan = Q<VisualElement>("fan"); _fanAnchor = Q<VisualElement>("fanAnchor");
            _desc = Q<VisualElement>("description"); _drop = Q<VisualElement>("dropZone"); _fx = Q<VisualElement>("fx");
            _descTitle = Q<Label>("descTitle"); _descText = Q<Label>("descText");
            _spotlight = Q<VisualElement>("spotlight"); _arenaTitle = Q<Label>("arenaTitle");
            Q<VisualElement>("arena").pickingMode = PickingMode.Ignore;
            foreach (var e in new[] { _spotlight, _arenaTitle, _fx, _desc, _drop, _fanAnchor, _fan, _allies, _enemies }) e.pickingMode = PickingMode.Ignore;
            Q<Label>("axisLabel").text = Loc.T("combat.timeline_action");
            Q<Label>("dropText").text = Loc.T("combat.drop_here");
            Q<VisualElement>("timeline").pickingMode = PickingMode.Ignore;
            Timeline = new TimelineHud(Q<VisualElement>("track"), Q<VisualElement>("prep"), _fx);

            _fanCards.Clear(); _fan.Clear(); _fan.Add(_fanAnchor);
            for (int i = 0; i < FanSlots; i++)
            {
                var card = new SkillCardHud();
                card.Root.style.left = (FanWidth - (FanSlots * FanStep - (FanStep - FanCardWidth))) * 0.5f + i * FanStep;
                card.SetHome(0f);
                card.Holding += (c, held) => SkillHolding?.Invoke(c, held);
                card.DragBegan += c => SkillDragBegan?.Invoke(c);
                card.DragEnded += (c, at) => SkillDragEnded?.Invoke(c, at);
                _fan.Add(card.Root); _fanCards.Add(card);
            }
            // Le tutoriel (encore en uGUI) cerne les ennemis et l'éventail.
            HdTutorialAnchor.Attach(this, _enemies, TutorialTarget.CombatEnemies);
            HdTutorialAnchor.Attach(this, _fanAnchor, TutorialTarget.CombatSkills);

            // Rejoue le dernier état (le document est recréé à chaque activation).
            _cards.Clear(); _cardList.Clear();
            if (_units != null) BuildUnits(_units);
            if (_states.Count > 0) ShowStates(_states);
            if (_timelineData.HasValue) Timeline.Show(_timelineData.Value);
            ShowFan(_fanShown, _fanSkills, false, _fanUsable);
            if (_spotlightArt != null) Spotlight(_spotlightArt);
        }

        void Update() => Timeline?.Tick();

        public void ClearTimeline() => Timeline?.Clear();

        public void BuildUnits(IReadOnlyList<UnitCardData> units)
        {
            _units = units;
            if (_allies == null) return;
            _allies.Clear(); _enemies.Clear(); _cards.Clear(); _cardList.Clear();
            bool first = true;
            foreach (var u in units)
            {
                var card = new UnitCardHud();
                card.Setup(u);
                card.Tapped += id => UnitTapped?.Invoke(id);
                (u.Ally ? _allies : _enemies).Add(card.Root);
                _cards[u.Id] = card; _cardList.Add(card);
                if (u.Ally && first) { first = false; Spotlight(card.PortraitArt); }
            }
        }

        public void ShowStates(IReadOnlyList<UnitStateData> states)
        {
            _states = states;
            foreach (var s in states) if (_cards.TryGetValue(s.Id, out var c)) c.Refresh(s);
        }

        public void ShowTimeline(TimelineData d)
        {
            _timelineData = d;
            if (Timeline != null) Timeline.Show(d);
        }

        public void Spotlight(Sprite? art)
        {
            if (art == null) return;
            _spotlightArt = art;
            if (_spotlight == null) return;
            // L'agent détouré est posé en grand sur le décor ; sans détourage, l'illustration reste dans un cadre fin.
            var cutout = AgentArt.Cutout(art.name);
            _spotlight.EnableInClassList("hd-hud__spotlight--cutout", cutout != null);
            _spotlight.style.backgroundImage = new StyleBackground(Background.FromSprite(cutout != null ? cutout : art));
            _spotlight.style.display = DisplayStyle.Flex;
        }

        public void ShowArenaTitle(string title, Color color, float seconds)
        {
            if (_arenaTitle == null) return;
            _arenaTitle.text = title.ToUpperInvariant();
            _arenaTitle.style.color = color;
            _arenaTitle.style.display = DisplayStyle.Flex;
            HudAnim.PopIn(_arenaTitle, 0.25f);
            HudAnim.After(_arenaTitle, seconds, () => _arenaTitle.style.display = DisplayStyle.None);
        }

        // ---- Éventail
        public void ShowFan(bool show, SkillSlotData[]? skills, bool opening, bool usable)
        {
            _fanShown = show; _fanSkills = skills; _fanUsable = usable;
            if (_fan == null) return;
            _fan.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            _fanAnchor.style.display = show && usable ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show) { SetDescription(false, "", ""); return; }
            for (int i = 0; i < _fanCards.Count; i++)
            {
                var slotData = skills != null && i < skills.Length ? skills[i] : default;
                _fanCards[i].Show(slotData, i);
                // Trois cartes en éventail ; la 4e (carte du grimoire) n'apparaît que si elle existe.
                if (i >= 3) _fanCards[i].Root.style.display = slotData.Visible && !slotData.Locked ? DisplayStyle.Flex : DisplayStyle.None;
            }
            // Les cartes affichées sont alignées à plat et centrées en bas de l'écran.
            int shown = 0;
            foreach (var c in _fanCards) if (c.Root.style.display == DisplayStyle.Flex) shown++;
            float x = (FanWidth - (shown * FanStep - (FanStep - FanCardWidth))) * 0.5f;
            foreach (var c in _fanCards)
            {
                if (c.Root.style.display != DisplayStyle.Flex) continue;
                c.Root.style.left = x;
                x += FanStep;
            }
            if (opening) AnimateFanIn();
        }

        /// <summary>Début du tour d'un agent : les cartes montent du bas de l'écran et s'ouvrent en éventail, l'une après l'autre.</summary>
        void AnimateFanIn()
        {
            if (!HudAnim.Allowed) return;
            for (int i = 0; i < _fanCards.Count; i++)
            {
                var card = _fanCards[i];
                card.SetFanOffset(300f);
                HudAnim.SetScale(card.Root, 0.8f);
                HudAnim.After(card.Root, i * 0.05f, () =>
                    HudAnim.Run(card.Root, 0.28f, k =>
                    {
                        card.SetFanOffset(Mathf.LerpUnclamped(300f, 0f, k));
                        HudAnim.SetScale(card.Root, Mathf.LerpUnclamped(0.8f, 1f, k));
                    }, () => { card.SetFanOffset(0f); HudAnim.SetScale(card.Root, 1f); }, HudAnim.EaseOutBack));
            }
        }

        public void SetDescription(bool visible, string title, string text)
        {
            if (_desc == null) return;
            _desc.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible) return;
            _descTitle.text = title; _descText.text = text;
        }

        public void SetDropZone(bool on) { if (_drop != null) _drop.style.display = on ? DisplayStyle.Flex : DisplayStyle.None; }

        /// <summary>Le point de l'écran est-il sur l'éventail (déposer une carte dessus l'annule) ?</summary>
        public bool IsOverFan(Vector2 screen) => _fan != null && _fan.worldBound.Contains(HudSpace.ScreenToPanel(_fan, screen));

        /// <summary>Identifiant de l'unité dont la carte est sous ce point de l'écran, null si aucune.</summary>
        public int? UnitAt(Vector2 screen)
        {
            foreach (var c in _cardList) if (c.ContainsScreenPoint(screen)) return c.UnitId;
            return null;
        }

        /// <summary>Une carte jouée s'envole de l'éventail vers sa cible (ou le terrain), grossit puis s'éteint dans un éclat.</summary>
        public void PlayCard(int slot, int targetUnitId, Vector2 terrainFallbackPanel)
        {
            if (!HudAnim.Allowed || slot < 0 || slot >= _fanCards.Count || _fx == null) return;
            var src = _fanCards[slot];
            if (src.Root.resolvedStyle.display == DisplayStyle.None) return;
            var ghost = new SkillCardHud();
            ghost.Show(src.Data, slot);
            ghost.Root.pickingMode = PickingMode.Ignore;
            var srcBox = src.Root.worldBound;
            ghost.Root.style.left = srcBox.x; ghost.Root.style.top = srcBox.y;
            ghost.Root.style.bottom = StyleKeyword.Auto;
            _fx.Add(ghost.Root);
            var from = srcBox.center;
            UnitCardHud? target = null;
            var to = terrainFallbackPanel;
            if (targetUnitId >= 0 && _cards.TryGetValue(targetUnitId, out var tc)) { target = tc; to = tc.Root.worldBound.center; }
            var delta = to - from;
            HudAnim.Run(ghost.Root, 0.32f, k =>
            {
                HudAnim.SetTranslate(ghost.Root, delta.x * k, delta.y * k);
                HudAnim.SetScale(ghost.Root, Mathf.Lerp(1f, 1.35f, k));
            }, () =>
            {
                HudAnim.Sparks(_fx, target != null ? target.Root : ghost.Root, Theme.Glow, 12, 130f, 18f);
                ghost.Root.style.opacity = 0f;
                HudAnim.After(ghost.Root, 0.15f, () => ghost.Root.RemoveFromHierarchy());
            }, HudAnim.EaseOutCubic);
        }

        public Vector2 EnemyCenterPanel() => _enemies != null ? _enemies.worldBound.center : Vector2.zero;
    }
}
