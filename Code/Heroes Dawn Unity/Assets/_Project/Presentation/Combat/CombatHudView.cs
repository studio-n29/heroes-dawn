#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>
    /// HUD du combat (UI Toolkit) posé sur l'arène uGUI : barre du haut (retraite, titre, AUTO, vitesse, pause), bandeau central,
    /// consigne et journal. Les clics des zones vides traversent vers l'arène. Rejoue le dernier état à chaque activation.
    /// </summary>
    public sealed class CombatHudView : HdView
    {
        UnityEngine.UIElements.Button _retreat = null!, _auto = null!, _speed = null!, _pause = null!, _debug = null!;
        Label _title = null!, _banner = null!, _hint = null!, _log = null!;
        VisualElement _hintBand = null!;
        IVisualElementScheduledItem? _bannerOff;

        CombatTopBarData? _topBar;
        string _hintText = "", _bannerText = "", _logText = "";
        UnityEngine.Color _bannerColor = UnityEngine.Color.white;

        public event Action? RetreatClicked, AutoClicked, SpeedClicked, DebugWinClicked, PauseClicked;

        public HdButtonRef RetreatButton { get; private set; } = null!;
        public HdButtonRef AutoButton { get; private set; } = null!;
        public HdButtonRef SpeedButton { get; private set; } = null!;
        public HdButtonRef PauseButton { get; private set; } = null!;
        public HdButtonRef DebugButton { get; private set; } = null!;
        public string Hint => _hintText;
        public string BannerText => _bannerText;
        public string LogText => _logText;
        public bool HintVisible => !string.IsNullOrEmpty(_hintText);

        protected override void Bind()
        {
            _retreat = Q<UnityEngine.UIElements.Button>("retreat"); _auto = Q<UnityEngine.UIElements.Button>("auto");
            _speed = Q<UnityEngine.UIElements.Button>("speed"); _pause = Q<UnityEngine.UIElements.Button>("pause");
            _debug = Q<UnityEngine.UIElements.Button>("debugWin");
            _title = Q<Label>("title"); _banner = Q<Label>("banner"); _hint = Q<Label>("hint"); _log = Q<Label>("log");
            _hintBand = Q<VisualElement>("hintBand");
            RetreatButton = new HdButtonRef(_retreat, this); AutoButton = new HdButtonRef(_auto, this);
            SpeedButton = new HdButtonRef(_speed, this); PauseButton = new HdButtonRef(_pause, this); DebugButton = new HdButtonRef(_debug, this);
            _retreat.clicked += () => RetreatClicked?.Invoke();
            _auto.clicked += () => AutoClicked?.Invoke();
            _speed.clicked += () => SpeedClicked?.Invoke();
            _pause.clicked += () => PauseClicked?.Invoke();
            _debug.clicked += () => DebugWinClicked?.Invoke();
            _retreat.text = Loc.T("combat.retreat");
            _pause.text = Loc.T("combat.pause");
            _auto.text = Loc.T("combat.auto_hint");
            _debug.text = Loc.T("combat.debug_win");
            _banner.pickingMode = PickingMode.Ignore;
            BindBossAndBonuses();
            if (_topBar.HasValue) SetTopBar(_topBar.Value);
            SetHint(_hintText);
            _log.text = _logText;
            _banner.text = _bannerText;
            _banner.style.color = _bannerColor;
        }

        public void SetTopBar(CombatTopBarData d)
        {
            _topBar = d;
            if (_title == null) return;
            _title.text = d.Title;
            _auto.EnableInClassList("hd-hud__btn--on", d.AutoOn);
            _speed.text = d.SpeedText;
            _retreat.SetEnabled(d.RetreatEnabled);
            _debug.style.display = d.DebugVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetHint(string text)
        {
            _hintText = text;
            if (_hint == null) return;
            _hint.text = text;
            _hintBand.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void SetLog(IReadOnlyList<string> lines)
        {
            _logText = string.Join("\n", lines);
            if (_log != null) _log.text = _logText;
        }

        /// <summary>Bandeau central (COMBO, VICTOIRE…) : apparaît d'un coup puis s'efface au bout d'1,2 s.</summary>
        public void ShowBanner(string text, UnityEngine.Color color)
        {
            _bannerText = text; _bannerColor = color;
            if (_banner == null) return;
            _banner.text = text;
            _banner.style.color = color;
            _banner.EnableInClassList("hd-hud__banner--on", true);
            _bannerOff?.Pause();
            _bannerOff = _banner.schedule.Execute(() => _banner.EnableInClassList("hd-hud__banner--on", false));
            _bannerOff.ExecuteLater(1200);
        }

        // ---- Barre du boss et pastilles de bonus
        VisualElement _boss = null!, _bossFill = null!, _bossMarks = null!, _bossFlash = null!, _bonuses = null!;
        Label _bossName = null!, _bossPhase = null!, _bossHp = null!;
        readonly List<VisualElement> _chips = new List<VisualElement>();
        BossBarData? _bossData;
        IReadOnlyList<BonusChipData> _bonusData = System.Array.Empty<BonusChipData>();

        /// <summary>Barre du boss affichée (tests PlayMode).</summary>
        public bool BossVisible => _bossData.HasValue;
        public string BossName => _bossData.HasValue ? _bossData.Value.Name : "";
        public string BossPhase => _bossData.HasValue ? _bossData.Value.Phase : "";
        public int BossMarkCount => _bossData.HasValue ? _bossData.Value.Marks.Count : 0;
        public int BonusCount => _bonusData.Count;

        void BindBossAndBonuses()
        {
            _boss = Q<VisualElement>("boss"); _bossFill = Q<VisualElement>("bossFill"); _bossMarks = Q<VisualElement>("bossMarks");
            _bossFlash = Q<VisualElement>("bossFlash"); _bonuses = Q<VisualElement>("bonuses");
            _bossName = Q<Label>("bossName"); _bossPhase = Q<Label>("bossPhase"); _bossHp = Q<Label>("bossHp");
            _bossFlash.pickingMode = PickingMode.Ignore;
            _chips.Clear(); _bonuses.Clear();
            for (int i = 0; i < 6; i++)
            {
                var chip = new VisualElement(); chip.AddToClassList("hd-hud__chip");
                var name = new Label(); name.AddToClassList("hd-hud__chip-name");
                var effect = new Label(); effect.AddToClassList("hd-hud__chip-effect");
                chip.Add(name); chip.Add(effect);
                chip.pickingMode = PickingMode.Ignore;
                _bonuses.Add(chip); _chips.Add(chip);
            }
            ShowBoss(_bossData, false);
            ShowBonuses(_bonusData);
        }

        public void ShowBoss(BossBarData? data, bool phaseChanged)
        {
            _bossData = data;
            if (_boss == null) return;
            _boss.style.display = data.HasValue ? DisplayStyle.Flex : DisplayStyle.None;
            if (!data.HasValue) return;
            var d = data.Value;
            _bossName.text = d.Name;
            _bossPhase.text = d.Phase;
            _bossPhase.style.display = d.Phase.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _bossFill.style.width = Length.Percent(UnityEngine.Mathf.Clamp01(d.Hp01) * 100f);
            _bossHp.text = d.HpText;
            _bossMarks.Clear();
            foreach (var m in d.Marks)
            {
                var mark = new VisualElement(); mark.AddToClassList("hd-hud__boss-mark");
                mark.style.left = Length.Percent(UnityEngine.Mathf.Clamp01(m) * 100f);
                mark.pickingMode = PickingMode.Ignore;
                _bossMarks.Add(mark);
            }
            if (!phaseChanged) return;
            // Télégraphie du changement de phase : un éclat blanc s'éteint.
            _bossFlash.EnableInClassList("hd-hud__boss-flash--on", true);
            _bossFlash.schedule.Execute(() => _bossFlash.EnableInClassList("hd-hud__boss-flash--on", false)).ExecuteLater(60);
        }

        public void ShowBonuses(IReadOnlyList<BonusChipData> bonuses)
        {
            _bonusData = bonuses;
            if (_bonuses == null) return;
            for (int i = 0; i < _chips.Count; i++)
            {
                bool on = i < bonuses.Count;
                _chips[i].style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
                if (!on) continue;
                _chips[i].style.borderLeftColor = bonuses[i].Color;
                ((Label)_chips[i][0]).text = bonuses[i].Name;
                ((Label)_chips[i][1]).text = bonuses[i].Effect;
            }
        }
    }
}
