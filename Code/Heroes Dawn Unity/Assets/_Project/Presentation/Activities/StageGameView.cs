#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Fenêtre des activités 3D (prefab) : une image de la scène 3D, le HUD (lignes, jauge, bandeau), le joystick et deux boutons d'action,
    /// le choix de l'agent (arène) et le bouton Commencer / Abandonner. Entrées : joystick tactile, clavier (ZQSD / flèches, Espace, Maj / K).
    /// </summary>
    public sealed class StageGameView : PopupViewBase, IStageGameView
    {
        [SerializeField] RawImage viewport = null!;
        [SerializeField] TMP_Text line1 = null!;
        [SerializeField] TMP_Text line2 = null!;
        [SerializeField] TMP_Text banner = null!;
        [SerializeField] UiBar bar = null!;
        [SerializeField] GameObject barRoot = null!;
        [SerializeField] RectTransform controls = null!;
        [SerializeField] VirtualStick stick = null!;
        [SerializeField] UiButton buttonA = null!;
        [SerializeField] UiButton buttonB = null!;
        [SerializeField] HoldButton holdA = null!;
        [SerializeField] HoldButton holdB = null!;
        [SerializeField] RectTransform introPanel = null!;
        [SerializeField] TMP_Text introTitle = null!;
        [SerializeField] TMP_Text introBody = null!;
        [SerializeField] RectTransform agentRow = null!;
        [SerializeField] UiButton agentPrefab = null!;
        [SerializeField] UiButton startButton = null!;
        [SerializeField] UiButton quitButton = null!;

        public UnityEngine.UI.Button StartButton => startButton.Button;
        public UnityEngine.UI.Button QuitButton => quitButton.Button;
        public RawImage Viewport => viewport;

        /// <summary>Tests : remplace les entrées réelles par un pilote (robot).</summary>
        public Func<StageInput>? InputOverride { get; set; }
        /// <summary>Jeu en cours (tests) ; null hors partie.</summary>
        public IStageDriver? Driver { get; private set; }
        public ActivityStage? Stage { get; private set; }
        public bool Started { get; private set; }
        public int AgentButtons => agentRow.childCount;
        public UnityEngine.UI.Button AgentButtonAt(int i) => agentRow.GetChild(i).GetComponent<UiButton>().Button;

        public async UniTask<ActivityResult> PlayAsync(StageRequest r, CancellationToken ct)
        {
            Open(r.Title);
            Started = false;
            Driver = null;
            banner.text = "";
            controls.gameObject.SetActive(false);
            barRoot.SetActive(false);
            line1.text = ""; line2.text = "";
            viewport.texture = null;
            ActivityStage? stage = null;
            try
            {
                int agent = 0;
                introPanel.gameObject.SetActive(true);
                introTitle.text = r.Title;
                introBody.text = r.Instructions;
                agentRow.Clear();
                var choice = new UniTaskCompletionSource<int>();
                bool picking = r.Kind == StageKind.Arena && r.Agents.Count > 0;
                startButton.gameObject.SetActive(!picking);
                if (picking)
                {
                    for (int i = 0; i < r.Agents.Count; i++)
                    {
                        int index = i;
                        var b = Instantiate(agentPrefab, agentRow);
                        b.SetLabel(r.Agents[i].Name + "\n<size=60%>" + r.Agents[i].Detail + "</size>");
                        b.Button.onClick.AddListener(() => choice.TrySetResult(index));
                    }
                    agent = await choice.Task.AttachExternalCancellation(ct);
                }
                else
                {
                    var go = new UniTaskCompletionSource();
                    startButton.Button.onClick.RemoveAllListeners();
                    startButton.Button.onClick.AddListener(() => go.TrySetResult());
                    await go.Task.AttachExternalCancellation(ct);
                }

                introPanel.gameObject.SetActive(false);
                stage = new ActivityStage(r.Level);
                Stage = stage;
                viewport.texture = stage.Texture;
                var driver = Build(r, stage, agent);
                Driver = driver;
                quitButton.Button.onClick.RemoveAllListeners();
                quitButton.Button.onClick.AddListener(() => driver.Abandon());
                controls.gameObject.SetActive(true);
                Started = true;

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
            finally
            {
                Driver?.Dispose();
                stage?.Dispose();
                Stage = null;
                Driver = null;
                viewport.texture = null;
                Started = false;
            }
        }

        static IStageDriver Build(StageRequest r, ActivityStage stage, int agent)
        {
            switch (r.Kind)
            {
                case StageKind.Survival: return new SurvivalDriver(stage, r.Survival!, r.Loc);
                default: return new ArenaDriver(stage, r.ArenaFactory!(agent), r.Loc);
            }
        }

        StageInput ReadInput()
        {
            if (InputOverride != null) return InputOverride();
            var kb = Keyboard.current;
            var move = stick.Value;
            bool a = holdA.Held, b = holdB.Held, ap = holdA.ConsumePress(), bp = holdB.ConsumePress();
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
            line1.text = h.Line1;
            line2.text = h.Line2;
            barRoot.SetActive(h.ShowBar);
            if (h.ShowBar) { bar.Set(h.Bar01); bar.SetLabel(h.BarLabel); bar.SetColor(h.BarColor); }
            buttonA.gameObject.SetActive(h.ShowButtons);
            buttonB.gameObject.SetActive(h.ShowButtons);
            if (h.ShowButtons)
            {
                buttonA.SetLabel(h.ButtonA);
                buttonB.SetLabel(h.ButtonB);
            }
            banner.text = h.Banner;
            banner.color = h.BannerColor;
        }
    }
}
