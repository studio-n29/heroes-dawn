#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Plateau (prefab Screens/Board) : carte, objectifs, HUD (escouade, PM, réserve, dé, Lancer, Dé bonus, Terminer).</summary>
    public sealed class BoardView : ViewBase, IBoardView
    {
        [SerializeField] TMP_Text zoneTitle = null!;
        [SerializeField] RectTransform objectiveList = null!;
        [SerializeField] RectTransform squadStrip = null!;
        [SerializeField] TMP_Text hint = null!;
        [SerializeField] TMP_Text pm = null!;
        [SerializeField] UiBar reserve = null!;
        [SerializeField] RectTransform die = null!;
        [SerializeField] TMP_Text dieFace = null!;
        [SerializeField] UiButton roll = null!;
        [SerializeField] UiButton bonus = null!;
        [SerializeField] UiButton end = null!;
        [SerializeField] UiButton back = null!;
        [SerializeField] WalletView wallet = null!;
        [SerializeField] BoardMapView map = null!;
        [SerializeField] TextLineView objectiveLinePrefab = null!;
        [SerializeField] SquadSlotView squadSlotPrefab = null!;

        public event Action? RollClicked;
        public event Action? BonusClicked;
        public event Action? EndMoveClicked;
        public event Action? BackClicked;

        public IWalletView Wallet => wallet;
        public IBoardMapView Map => map;

        /// <summary>Carte et bouton « Retour QG » (tests PlayMode : clics réels).</summary>
        public BoardMapView MapView => map;
        public Button BackButton => back.Button;
        public Button RollButton => roll.Button;
        public Button EndMoveButton => end.Button;

        void Awake()
        {
            roll.Button.onClick.AddListener(() => RollClicked?.Invoke());
            bonus.Button.onClick.AddListener(() => BonusClicked?.Invoke());
            end.Button.onClick.AddListener(() => EndMoveClicked?.Invoke());
            back.Button.onClick.AddListener(() => BackClicked?.Invoke());
        }

        void Update()
        {
            if (Keys.Pressed(Key.Space)) RollClicked?.Invoke();
            else if (Keys.Pressed(Key.Enter) || Keys.Pressed(Key.NumpadEnter)) EndMoveClicked?.Invoke();
            else if (Keys.Pressed(Key.B)) BonusClicked?.Invoke();
        }

        public void SetZoneTitle(string title) => zoneTitle.text = title;

        public void ShowObjectives(IReadOnlyList<ObjectiveLineData> objectives)
        {
            objectiveList.Clear();
            foreach (var o in objectives) Instantiate(objectiveLinePrefab, objectiveList).Show(o.Text, o.Done ? Theme.Hex(0x1E7A3A) : Theme.TextDark);
        }

        public void ShowSquad(IReadOnlyList<SquadSlotData> squad)
        {
            squadStrip.Clear();
            foreach (var s in squad) Instantiate(squadSlotPrefab, squadStrip).Show(s);
        }

        public void ShowHud(BoardHudData d)
        {
            hint.text = d.Hint;
            pm.text = d.PmText;
            reserve.Set(d.Reserve01);
            reserve.SetLabel(d.ReserveLabel);
            roll.Interactable = d.RollEnabled;
            bonus.SetLabel(d.BonusLabel);
            bonus.Interactable = d.BonusEnabled;
            end.Interactable = d.EndEnabled;
        }

        public UniTask RollDieAsync(int value, int faces, CancellationToken ct)
        {
            var done = new UniTaskCompletionSource();
            Tween.Run(0.5f, k => { if (k < 0.95f) dieFace.text = UnityEngine.Random.Range(1, faces + 1).ToString(); }, () =>
            {
                dieFace.text = value.ToString();
                Tween.Punch(die, 0.25f, 0.3f);
                done.TrySetResult();
            }, this, Tween.Linear);
            return done.Task.AttachExternalCancellation(ct);
        }

        public void ResetDie() => dieFace.text = "-";

        public UniTask PauseAsync(float seconds, CancellationToken ct)
            => UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: true, cancellationToken: ct);
    }
}
