#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Joystick virtuel (UI Toolkit, tactile et souris) : la valeur va de -1 à 1 sur chaque axe, au repos à zéro.</summary>
    public sealed class StickPad
    {
        const float Radius = 90f;
        readonly VisualElement _pad, _knob;
        public Vector2 Value { get; private set; }

        public StickPad(VisualElement pad, VisualElement knob)
        {
            _pad = pad; _knob = knob;
            pad.RegisterCallback<PointerDownEvent>(e => { pad.CapturePointer(e.pointerId); Move(e.position); });
            pad.RegisterCallback<PointerMoveEvent>(e => { if (pad.HasPointerCapture(e.pointerId)) Move(e.position); });
            pad.RegisterCallback<PointerUpEvent>(e => { pad.ReleasePointer(e.pointerId); Reset(); });
            pad.RegisterCallback<PointerCancelEvent>(_ => Reset());
        }

        void Move(Vector2 panelPos)
        {
            var r = _pad.worldBound;
            var local = new Vector2(panelPos.x - r.center.x, -(panelPos.y - r.center.y));
            var v = Vector2.ClampMagnitude(local / Radius, 1f);
            Value = v.magnitude < 0.12f ? Vector2.zero : v;
            HudAnimShim.Translate(_knob, v.x * Radius, -v.y * Radius);
        }

        void Reset() { Value = Vector2.zero; HudAnimShim.Translate(_knob, 0f, 0f); }
    }

    static class HudAnimShim
    {
        public static void Translate(VisualElement e, float x, float y) => N29.HeroesDawn.Presentation.Combat.Hud.HudAnim.SetTranslate(e, x, y);
    }

    /// <summary>Bouton maintenu (UI Toolkit) : « maintenu » tant que le doigt est dessus, « appuyé » une fois par appui (lu puis consommé).</summary>
    public sealed class HoldPad
    {
        bool _pressed;
        public bool Held { get; private set; }
        public bool ConsumePress() { bool p = _pressed; _pressed = false; return p; }

        public HoldPad(VisualElement e)
        {
            e.RegisterCallback<PointerDownEvent>(_ => { Held = true; _pressed = true; });
            e.RegisterCallback<PointerUpEvent>(_ => Held = false);
            e.RegisterCallback<PointerLeaveEvent>(_ => Held = false);
        }
    }

    /// <summary>
    /// Fenêtre des activités 3D (UI Toolkit) : une image de la scène 3D, le HUD (lignes, jauge, bandeau), le joystick et deux boutons d'action,
    /// le choix de l'agent (arène) et le bouton Commencer / Abandonner. Entrées : joystick tactile, clavier (ZQSD / flèches, Espace, Maj / K).
    /// </summary>
    public sealed class StageGameView : HdPopup, IStageGameView
    {
        readonly List<UnityEngine.UIElements.Button> _agents = new List<UnityEngine.UIElements.Button>();
        Label _title = null!, _line1 = null!, _line2 = null!, _banner = null!, _barLabel = null!, _introTitle = null!, _introBody = null!;
        VisualElement _viewport = null!, _barRoot = null!, _barFill = null!, _controls = null!, _intro = null!, _agentRow = null!;
        UnityEngine.UIElements.Button _a = null!, _b = null!, _start = null!, _quit = null!;
        StickPad _stick = null!;
        HoldPad _holdA = null!, _holdB = null!;
        StageRequest? _pending;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        protected override VisualElement DefaultFocus => _agents.Count > 0 ? _agents[0] : _start;

        public HdButtonRef StartButton { get; private set; } = null!;
        public HdButtonRef QuitButton { get; private set; } = null!;
        /// <summary>Texture de la scène affichée (tests).</summary>
        public Texture? ViewportTexture { get; private set; }

        /// <summary>Tests : remplace les entrées réelles par un pilote (robot).</summary>
        public Func<StageInput>? InputOverride { get; set; }
        /// <summary>Jeu en cours (tests) ; null hors partie.</summary>
        public IStageDriver? Driver { get; private set; }
        public ActivityStage? Stage { get; private set; }
        public bool Started { get; private set; }
        public int AgentButtons => _agents.Count;
        public HdButtonRef AgentButtonAt(int i) => new HdButtonRef(_agents[i], this);

        protected override void Bind()
        {
            _title = Q<Label>("title"); _line1 = Q<Label>("line1"); _line2 = Q<Label>("line2"); _banner = Q<Label>("banner");
            _barLabel = Q<Label>("barLabel"); _introTitle = Q<Label>("introTitle"); _introBody = Q<Label>("introBody");
            _viewport = Q<VisualElement>("viewport"); _barRoot = Q<VisualElement>("barRoot"); _barFill = Q<VisualElement>("barFill");
            _controls = Q<VisualElement>("controls"); _intro = Q<VisualElement>("intro"); _agentRow = Q<VisualElement>("agents");
            _a = Q<UnityEngine.UIElements.Button>("buttonA"); _b = Q<UnityEngine.UIElements.Button>("buttonB");
            _start = Q<UnityEngine.UIElements.Button>("start"); _quit = Q<UnityEngine.UIElements.Button>("quit");
            StartButton = new HdButtonRef(_start, this); QuitButton = new HdButtonRef(_quit, this);
            _stick = new StickPad(Q<VisualElement>("stick"), Q<VisualElement>("knob"));
            _holdA = new HoldPad(_a); _holdB = new HoldPad(_b);
            _start.text = Loc.T("stage.start"); _quit.text = Loc.T("stage.quit");
            foreach (var e in new VisualElement[] { _line1, _line2, _banner, _barRoot, _barFill, _barLabel }) e.pickingMode = PickingMode.Ignore;
            _banner.enableRichText = true; _line1.enableRichText = true; _line2.enableRichText = true;
            _controls.style.display = DisplayStyle.None; _barRoot.style.display = DisplayStyle.None;
        }

        public async UniTask<ActivityResult> PlayAsync(StageRequest r, CancellationToken ct)
        {
            if (_viewport == null) await UniTask.WaitUntil(() => _viewport != null, cancellationToken: ct); // le document se monte au premier affichage
            _title.text = r.Title;
            Started = false;
            Driver = null;
            _banner.text = ""; _line1.text = ""; _line2.text = "";
            _controls.style.display = DisplayStyle.None; _barRoot.style.display = DisplayStyle.None;
            _viewport.style.backgroundImage = new StyleBackground(StyleKeyword.None);
            ActivityStage? stage = null;
            try
            {
                int agent = 0;
                _intro.style.display = DisplayStyle.Flex;
                _introTitle.text = r.Title;
                _introBody.text = r.Instructions;
                _agentRow.Clear(); _agents.Clear();
                var choice = new UniTaskCompletionSource<int>();
                bool picking = r.Kind == StageKind.Arena && r.Agents.Count > 0;
                _start.style.display = picking ? DisplayStyle.None : DisplayStyle.Flex;
                var go = new UniTaskCompletionSource();
                Action onStart = () => go.TrySetResult();
                if (picking)
                {
                    for (int i = 0; i < r.Agents.Count; i++)
                    {
                        int index = i;
                        var b = new UnityEngine.UIElements.Button { text = r.Agents[i].Name + "\n" + r.Agents[i].Detail };
                        b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--secondary"); b.AddToClassList("hd-stage__agent");
                        b.clicked += () => choice.TrySetResult(index);
                        _agentRow.Add(b); _agents.Add(b);
                    }
                    agent = await choice.Task.AttachExternalCancellation(ct);
                }
                else
                {
                    _start.clicked += onStart;
                    try { await go.Task.AttachExternalCancellation(ct); }
                    finally { _start.clicked -= onStart; }
                }

                _intro.style.display = DisplayStyle.None;
                stage = new ActivityStage(r.Level);
                Stage = stage;
                ViewportTexture = stage.Texture;
                _viewport.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(stage.Texture));
                var driver = Build(r, stage, agent);
                Driver = driver;
                Action onQuit = () => driver.Abandon();
                _quit.clicked += onQuit;
                _controls.style.display = DisplayStyle.Flex;
                Started = true;

                try
                {
                    while (!driver.Finished)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, ct);
                        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                        driver.Tick(dt, ReadInput());
                        Render(driver.Hud);
                    }
                    Render(driver.Hud);
                    await UniTask.Delay(TimeSpan.FromSeconds(1.6), ignoreTimeScale: true, cancellationToken: ct);
                    return driver.Result;
                }
                finally { _quit.clicked -= onQuit; }
            }
            finally
            {
                Driver?.Dispose();
                stage?.Dispose();
                Stage = null;
                Driver = null;
                ViewportTexture = null;
                if (_viewport != null) _viewport.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                Started = false;
            }
        }

        static IStageDriver Build(StageRequest r, ActivityStage stage, int agent)
        {
            switch (r.Kind)
            {
                default: return new ArenaDriver(stage, r.ArenaFactory!(agent), r.Loc);
            }
        }

        StageInput ReadInput()
        {
            if (InputOverride != null) return InputOverride();
            var kb = Keyboard.current;
            var move = _stick.Value;
            bool a = _holdA.Held, b = _holdB.Held, ap = _holdA.ConsumePress(), bp = _holdB.ConsumePress();
            if (kb != null)
            {
                float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed || kb.qKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
                float y = (kb.wKey.isPressed || kb.zKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);
                if (Mathf.Abs(x) + Mathf.Abs(y) > 0.1f) move = new Vector2(x, y).normalized;
                a |= kb.spaceKey.isPressed || kb.jKey.isPressed;
                ap |= kb.spaceKey.wasPressedThisFrame || kb.jKey.wasPressedThisFrame;
                b |= kb.leftShiftKey.isPressed || kb.kKey.isPressed;
                bp |= kb.leftShiftKey.wasPressedThisFrame || kb.kKey.wasPressedThisFrame;
            }
            return new StageInput { Move = move, A = a, B = b, APressed = ap, BPressed = bp };
        }

        void Render(StageHud h)
        {
            _line1.text = h.Line1; _line2.text = h.Line2;
            _barRoot.style.display = h.ShowBar ? DisplayStyle.Flex : DisplayStyle.None;
            if (h.ShowBar)
            {
                _barFill.style.width = Length.Percent(Mathf.Clamp01(h.Bar01) * 100f);
                _barFill.style.backgroundColor = h.BarColor;
                _barLabel.text = h.BarLabel;
            }
            _a.style.display = h.ShowButtons ? DisplayStyle.Flex : DisplayStyle.None;
            _b.style.display = h.ShowButtons ? DisplayStyle.Flex : DisplayStyle.None;
            if (h.ShowButtons) { _a.text = h.ButtonA; _b.text = h.ButtonB; }
            _banner.text = h.Banner;
            _banner.style.color = h.BannerColor;
        }
    }
}
