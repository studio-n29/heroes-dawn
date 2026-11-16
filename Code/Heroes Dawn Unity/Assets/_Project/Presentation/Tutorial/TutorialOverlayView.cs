#nullable enable
using System;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Tutorial
{
    /// <summary>Texte d'une étape à afficher dans la bulle de Rayner.</summary>
    public struct TutorialStepView
    {
        public string Title;
        public string Body;
        public string Progress;
        public string OkLabel;
        public string SkipLabel;
        public bool ShowOk;
    }

    /// <summary>
    /// Couche du tutoriel (UI Toolkit, au-dessus de tout) : voile bleu nuit avec une découpe rectangulaire sur l'élément à toucher,
    /// cadre pulsé, équerres animées et flèche qui le désignent, bulle de Rayner (titre, texte, progression),
    /// et carte « Astuce » non bloquante pour les mécaniques contextuelles. La découpe se donne dans les coordonnées locales
    /// de l'objet (celles des marqueurs uGUI) ; elle est convertie en coordonnées du panneau.
    /// </summary>
    public sealed class TutorialOverlayView : HdView
    {
        const float Pad = 8f;

        VisualElement _veil = null!, _top = null!, _bottom = null!, _left = null!, _right = null!, _ring = null!, _ripple = null!, _bubble = null!, _hint = null!, _fx = null!, _medal = null!;
        readonly VisualElement[] _brackets = new VisualElement[4];
        Label _pointer = null!, _title = null!, _body = null!, _progress = null!, _hintTitle = null!, _hintBody = null!;
        UnityEngine.UIElements.Button _ok = null!, _skip = null!, _hintOk = null!;

        float _clock, _hintLeft;
        bool _hasHole, _stepVisible, _hintVisible;
        Rect _holePanel;
        Vector2 _pointerBase;

        public event Action? AcknowledgeClicked;
        public event Action? SkipClicked;
        public event Action? HintClosed;

        public bool StepVisible => _stepVisible;
        public bool HintVisible => _hintVisible;
        public HdButtonRef OkButton { get; private set; } = null!;
        public HdButtonRef SkipButton { get; private set; } = null!;
        public HdButtonRef HintOkButton { get; private set; } = null!;
        /// <summary>Nombre d'équerres visibles (tests).</summary>
        public int VisibleBrackets { get { int n = 0; foreach (var b in _brackets) if (b != null && _hasHole && _stepVisible) n++; return n; } }

        protected override void Bind()
        {
            _veil = Q<VisualElement>("veil"); _top = Q<VisualElement>("veilTop"); _bottom = Q<VisualElement>("veilBottom");
            _left = Q<VisualElement>("veilLeft"); _right = Q<VisualElement>("veilRight");
            _ring = Q<VisualElement>("ring"); _ripple = Q<VisualElement>("ripple"); _bubble = Q<VisualElement>("bubble");
            _hint = Q<VisualElement>("hint"); _fx = Q<VisualElement>("fx"); _medal = Q<VisualElement>("medal");
            for (int i = 0; i < 4; i++) _brackets[i] = Q<VisualElement>("bracket" + i);
            _pointer = Q<Label>("pointer"); _title = Q<Label>("title"); _body = Q<Label>("body"); _progress = Q<Label>("progress");
            _hintTitle = Q<Label>("hintTitle"); _hintBody = Q<Label>("hintBody");
            _ok = Q<UnityEngine.UIElements.Button>("ok"); _skip = Q<UnityEngine.UIElements.Button>("skip"); _hintOk = Q<UnityEngine.UIElements.Button>("hintOk");
            OkButton = new HdButtonRef(_ok, this); SkipButton = new HdButtonRef(_skip, this); HintOkButton = new HdButtonRef(_hintOk, this);
            _fx.pickingMode = PickingMode.Ignore; _ripple.pickingMode = PickingMode.Ignore; _pointer.pickingMode = PickingMode.Ignore;
            foreach (var b in _brackets) b.pickingMode = PickingMode.Ignore;
            _veil.pickingMode = PickingMode.Ignore;
            _medal.style.backgroundImage = AgentArt.For("rayner") is Sprite face ? new StyleBackground(Background.FromSprite(face)) : new StyleBackground(StyleKeyword.None);
            _ok.clicked += () => AcknowledgeClicked?.Invoke();
            _skip.clicked += () => SkipClicked?.Invoke();
            _hintOk.clicked += () => { HideHint(); HintClosed?.Invoke(); };
            Hide(); HideHint();
        }

        void Update()
        {
            if (_veil == null) return;
            _clock += Time.unscaledDeltaTime;
            if (_hintVisible && (_hintLeft -= Time.unscaledDeltaTime) <= 0f) { HideHint(); HintClosed?.Invoke(); }
            if (!_stepVisible || !_hasHole) return;
            float k = (_clock * 0.9f) % 1f;
            LayoutBrackets(Mathf.Sin(_clock * 5f) * 0.5f + 0.5f);
            HudAnim.SetScale(_ripple, 1f + 0.07f * k);
            _ripple.style.borderTopColor = _ripple.style.borderBottomColor = _ripple.style.borderLeftColor = _ripple.style.borderRightColor
                = new Color(Theme.Primary.r, Theme.Primary.g, Theme.Primary.b, 0.8f * (1f - k));
            _pointer.style.top = _pointerBase.y + Mathf.Sin(_clock * 6f) * 10f;
        }

        /// <summary>Équerres aux quatre coins du cadre ; elles « respirent » vers l'extérieur (0 à 1).</summary>
        void LayoutBrackets(float breathe)
        {
            float o = 2f + 8f * breathe, s = 26f;
            var h = _holePanel;
            var c = new[] { new Vector2(h.xMin - o, h.yMin - o), new Vector2(h.xMax + o - s, h.yMin - o), new Vector2(h.xMax + o - s, h.yMax + o - s), new Vector2(h.xMin - o, h.yMax + o - s) };
            for (int i = 0; i < 4; i++)
            {
                _brackets[i].style.left = c[i].x; _brackets[i].style.top = c[i].y;
                HudAnim.SetRotate(_brackets[i], 90f * i);
            }
        }

        static void Place(VisualElement e, float x0, float y0, float x1, float y1)
        {
            e.style.left = x0; e.style.top = y0;
            e.style.width = Mathf.Max(0f, x1 - x0); e.style.height = Mathf.Max(0f, y1 - y0);
            e.style.display = x1 > x0 && y1 > y0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>Affiche une étape. <paramref name="hole"/> : zone à laisser libre, en coordonnées locales de l'objet (null : voile plein). <paramref name="blockHole"/> : la découpe elle-même ne répond pas (étapes « Compris »).</summary>
        public void ShowStep(Rect? hole, TutorialStepView text, bool bubbleAtTop, bool blockHole, bool dim = true)
        {
            if (_veil == null) return;
            _stepVisible = true;
            _veil.style.display = DisplayStyle.Flex;
            var tint = Theme.WithAlpha(Theme.Navy, dim ? 0.5f : 0f);
            foreach (var v in new[] { _top, _bottom, _left, _right })
            {
                v.style.backgroundColor = tint;
                v.pickingMode = dim ? PickingMode.Position : PickingMode.Ignore;
            }
            var panel = Root.layout.size;
            if (panel.x <= 0f) panel = new Vector2(Screen.width, Screen.height);
            _hasHole = hole.HasValue;
            if (hole.HasValue)
            {
                var area = ((RectTransform)transform).rect;
                var r = Rect.MinMaxRect(hole.Value.xMin - Pad, hole.Value.yMin - Pad, hole.Value.xMax + Pad, hole.Value.yMax + Pad);
                float fx = panel.x / Mathf.Max(1f, area.width), fy = panel.y / Mathf.Max(1f, area.height);
                var h = Rect.MinMaxRect((r.xMin - area.xMin) * fx, (area.yMax - r.yMax) * fy, (r.xMax - area.xMin) * fx, (area.yMax - r.yMin) * fy);
                _holePanel = h;
                Place(_top, 0f, 0f, panel.x, h.yMin);
                Place(_bottom, 0f, h.yMax, panel.x, panel.y);
                Place(_left, 0f, h.yMin, h.xMin, h.yMax);
                Place(_right, h.xMax, h.yMin, panel.x, h.yMax);
                foreach (var e in new[] { _ring, _ripple }) { Place(e, h.xMin, h.yMin, h.xMax, h.yMax); }
                _ring.pickingMode = blockHole ? PickingMode.Position : PickingMode.Ignore;
                _ring.style.display = DisplayStyle.Flex;
                _pointer.style.display = DisplayStyle.Flex;
                foreach (var b in _brackets) b.style.display = DisplayStyle.Flex;
                LayoutBrackets(0f);
                // La flèche est posée du côté opposé à la bulle, pointant vers la cible.
                _pointerBase = new Vector2(h.center.x - 20f, bubbleAtTop ? h.yMax + 8f : h.yMin - 48f);
                _pointer.style.left = _pointerBase.x;
                // Chevron du kit (teinté par USS) : vers le haut quand la bulle est en haut, la cible étant alors au-dessus de la flèche.
                _pointer.text = "";
                _pointer.EnableInClassList("hd-tut__pointer--up", bubbleAtTop);
                _pointer.EnableInClassList("hd-tut__pointer--down", !bubbleAtTop);
            }
            else
            {
                Place(_top, 0f, 0f, panel.x, panel.y);
                Place(_bottom, 0, 0, 0, 0); Place(_left, 0, 0, 0, 0); Place(_right, 0, 0, 0, 0);
                _ring.style.display = DisplayStyle.None; _ripple.style.display = DisplayStyle.None; _pointer.style.display = DisplayStyle.None;
                foreach (var b in _brackets) b.style.display = DisplayStyle.None;
            }

            _title.text = text.Title; _body.text = text.Body; _progress.text = text.Progress;
            _ok.text = text.OkLabel; _skip.text = text.SkipLabel;
            _ok.style.display = text.ShowOk ? DisplayStyle.Flex : DisplayStyle.None;
            _bubble.style.display = DisplayStyle.Flex;
            _bubble.style.top = bubbleAtTop ? 130f : StyleKeyword.Auto;
            _bubble.style.bottom = bubbleAtTop ? StyleKeyword.Auto : 140f;
        }

        public void Hide()
        {
            _stepVisible = false;
            if (_veil == null) return;
            _veil.style.display = DisplayStyle.None;
            _bubble.style.display = DisplayStyle.None;
        }

        public void ShowHint(string heading, string text, string okLabel)
        {
            if (_hint == null) return;
            _hintTitle.text = heading; _hintBody.text = text; _hintOk.text = okLabel;
            _hint.style.display = DisplayStyle.Flex;
            _hintVisible = true;
            _hintLeft = 6f;
            HudAnim.PopIn(_hint, 0.25f);
        }

        public void HideHint()
        {
            _hintVisible = false;
            if (_hint != null) _hint.style.display = DisplayStyle.None;
        }

        /// <summary>Étape réussie : l'anneau et la bulle explosent en étincelles dorées (avant la bulle de l'étape suivante).</summary>
        public void StepDone()
        {
            if (_fx == null || !HudAnim.Allowed) return;
            HudAnim.Sparks(_fx, _hasHole ? _ring : _bubble, Theme.Primary, 14, 170f, 18f);
            HudAnim.Punch(_bubble, 0.08f, 0.3f);
        }

        /// <summary>Fin du tutoriel : une grande gerbe d'étincelles au centre de l'écran.</summary>
        public void Celebrate()
        {
            if (_fx == null || !HudAnim.Allowed) return;
            HudAnim.Sparks(_fx, Root, Theme.Primary, 26, 420f, 22f);
            HudAnim.Sparks(_fx, Root, Theme.Success, 18, 300f, 18f);
        }
    }
}
