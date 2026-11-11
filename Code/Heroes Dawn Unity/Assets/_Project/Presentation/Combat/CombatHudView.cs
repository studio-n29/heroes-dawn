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
    }
}
