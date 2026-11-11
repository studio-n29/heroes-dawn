#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Étoile de bande dessinée au centre de l'écran : « COMBAT ! », « BANG ! »… Elle surgit, tremble, puis s'efface.</summary>
    public sealed class ComicBurst
    {
        const float PopTime = 0.22f, HoldTime = 0.55f, OutTime = 0.2f;
        readonly VisualElement _root, _shape;
        readonly Label _label;
        public ComicBurst(VisualElement root, VisualElement shape, Label label) { _root = root; _shape = shape; _label = label; }
        public bool Playing { get; private set; }
        public string Text => _label.text;

        public async UniTask PlayAsync(ComicCueData cue, CancellationToken ct)
        {
            if (Playing) return;
            Playing = true;
            try
            {
                _label.text = cue.Text;
                _shape.style.unityBackgroundImageTintColor = cue.Color;
                HudAnim.SetRotate(_root, cue.Tilt);
                HudAnim.SetScale(_root, 0f);
                _root.style.display = DisplayStyle.Flex;
                await Run(PopTime, k => HudAnim.SetScale(_root, k), HudAnim.EaseOutBack, ct);
                if (!MotionSettings.Reduced) HudAnim.Shake(_root, 7f, HoldTime);
                await UniTask.Delay(TimeSpan.FromSeconds(HoldTime), ignoreTimeScale: true, cancellationToken: ct);
                await Run(OutTime, k => HudAnim.SetScale(_root, Mathf.Lerp(1f, 1.35f, k)), HudAnim.Linear, ct);
            }
            finally { HudAnim.SetScale(_root, 0f); _root.style.display = DisplayStyle.None; Playing = false; }
        }

        UniTask Run(float seconds, Action<float> step, Func<float, float> ease, CancellationToken ct)
        {
            var done = new UniTaskCompletionSource();
            HudAnim.Run(_root, seconds, step, () => done.TrySetResult(), ease);
            ct.Register(() => done.TrySetCanceled());
            return done.Task;
        }
    }

    /// <summary>
    /// Plateau en UI Toolkit (panneau superposé au fond uGUI) : carte à caméra, barre du haut (zone, ressources, retour QG), objectifs, escouade,
    /// consigne, PM, réserve, dé, Lancer / Dé bonus / Terminer, carte d'information de la case visée et onomatopée de BD.
    /// </summary>
    public sealed class BoardHudView : HdView
    {
        VisualElement _viewport = null!, _preview = null!, _objList = null!, _squad = null!, _die = null!, _reserveFill = null!, _objectives = null!;
        Label _zone = null!, _legend = null!, _objTitle = null!, _pvTitle = null!, _pvBody = null!, _pvDetail = null!, _hint = null!, _pm = null!, _reserveText = null!, _dieFace = null!;
        UnityEngine.UIElements.Button _back = null!, _roll = null!, _bonus = null!, _end = null!, _go = null!, _cancel = null!;
        readonly HubShellView.WalletBar _wallet = new HubShellView.WalletBar();
        MovePreviewData? _previewData;
        BoardHudData? _hudData;
        string _zoneText = "";
        IReadOnlyList<ObjectiveLineData> _objectivesData = Array.Empty<ObjectiveLineData>();
        IReadOnlyList<SquadSlotData> _squadData = Array.Empty<SquadSlotData>();
        BoardMapData? _mapData;

        public event Action? RollClicked, BonusClicked, EndMoveClicked, BackClicked, MoveConfirmClicked, MoveCancelClicked;

        public BoardMapView? Map { get; private set; }
        public ComicBurst? Burst { get; private set; }
        public IWalletView Wallet => _wallet;
        public HdButtonRef BackButton { get; private set; } = null!;
        public HdButtonRef RollButton { get; private set; } = null!;
        public HdButtonRef EndMoveButton { get; private set; } = null!;
        public HdButtonRef MoveConfirmButton { get; private set; } = null!;
        public HdButtonRef PreviewCancelButton { get; private set; } = null!;
        public bool PreviewVisible => _previewData.HasValue;

        protected override void Bind()
        {
            _viewport = Q<VisualElement>("viewport"); _preview = Q<VisualElement>("preview"); _objList = Q<VisualElement>("objList");
            _objectives = Q<VisualElement>("objectives"); _squad = Q<VisualElement>("squad"); _die = Q<VisualElement>("die"); _reserveFill = Q<VisualElement>("reserveFill");
            _zone = Q<Label>("zone"); _legend = Q<Label>("legend"); _objTitle = Q<Label>("objTitle"); _pvTitle = Q<Label>("pvTitle"); _pvBody = Q<Label>("pvBody");
            _pvDetail = Q<Label>("pvDetail"); _hint = Q<Label>("hint"); _pm = Q<Label>("pm"); _reserveText = Q<Label>("reserveText"); _dieFace = Q<Label>("dieFace");
            _back = Q<UnityEngine.UIElements.Button>("back"); _roll = Q<UnityEngine.UIElements.Button>("roll"); _bonus = Q<UnityEngine.UIElements.Button>("bonus");
            _end = Q<UnityEngine.UIElements.Button>("end"); _go = Q<UnityEngine.UIElements.Button>("pvGo"); _cancel = Q<UnityEngine.UIElements.Button>("pvCancel");
            BackButton = new HdButtonRef(_back, this); RollButton = new HdButtonRef(_roll, this); EndMoveButton = new HdButtonRef(_end, this);
            MoveConfirmButton = new HdButtonRef(_go, this); PreviewCancelButton = new HdButtonRef(_cancel, this);
            _wallet.Attach(Q<Label>("credits"), Q<Label>("pr"), Q<Label>("gems"), Q<Label>("saving"));
            _back.text = Loc.T("board.back_hq"); _roll.text = Loc.T("board.roll"); _end.text = Loc.T("board.end_move");
            _go.text = Loc.T("board.preview.go"); _cancel.text = Loc.T("board.preview.cancel");
            _objTitle.text = Loc.T("board.objectives"); _legend.text = Loc.T("board.legend");
            _back.clicked += () => BackClicked?.Invoke(); _roll.clicked += () => RollClicked?.Invoke();
            _bonus.clicked += () => BonusClicked?.Invoke(); _end.clicked += () => EndMoveClicked?.Invoke();
            _go.clicked += () => MoveConfirmClicked?.Invoke(); _cancel.clicked += () => MoveCancelClicked?.Invoke();
            foreach (var e in new[] { Q<VisualElement>("burst"), Q<VisualElement>("burstShape"), Q<Label>("burstText"), _legend, _hint, _pm, _dieFace }) e.pickingMode = PickingMode.Ignore;
            Burst = new ComicBurst(Q<VisualElement>("burst"), Q<VisualElement>("burstShape"), Q<Label>("burstText"));
            Q<VisualElement>("burst").style.display = DisplayStyle.None;

            Map = new BoardMapView(_viewport, this);
            Q<UnityEngine.UIElements.Button>("zoomIn").clicked += () => Map.ZoomBy(1.25f);
            Q<UnityEngine.UIElements.Button>("zoomOut").clicked += () => Map.ZoomBy(0.8f);
            Q<UnityEngine.UIElements.Button>("recenter").clicked += Map.Recenter;
            Q<UnityEngine.UIElements.Button>("prevCase").clicked += () => Map.CycleFocus(-1);
            Q<UnityEngine.UIElements.Button>("nextCase").clicked += () => Map.CycleFocus(1);
            HdTutorialAnchor.Attach(this, _objectives, TutorialTarget.BoardObjectives);
            HdTutorialAnchor.Attach(this, _roll, TutorialTarget.BoardRoll);
            MapRebound?.Invoke(Map);
            Replay();
        }

        /// <summary>Levé quand la carte est recréée (le document est recréé à chaque activation) : la vue y rebranche ses événements.</summary>
        public event Action<BoardMapView>? MapRebound;

        void Replay()
        {
            _zone.text = _zoneText;
            ShowObjectives(_objectivesData); ShowSquad(_squadData);
            if (_hudData.HasValue) ShowHud(_hudData.Value);
            ShowMovePreview(_previewData);
        }

        void Update() => Map?.Tick();

        public void SetZoneTitle(string title) { _zoneText = title; if (_zone != null) _zone.text = title; }

        public void ShowObjectives(IReadOnlyList<ObjectiveLineData> objectives)
        {
            _objectivesData = objectives;
            if (_objList == null) return;
            _objList.Clear();
            foreach (var o in objectives)
            {
                var l = new Label(o.Text); l.AddToClassList("hd-board__objline");
                l.style.color = o.Done ? Theme.Hex(0x1E7A3A) : Theme.TextDark;
                _objList.Add(l);
            }
        }

        public void ShowSquad(IReadOnlyList<SquadSlotData> squad)
        {
            _squadData = squad;
            if (_squad == null) return;
            _squad.Clear();
            foreach (var s in squad)
            {
                var slot = new VisualElement(); slot.AddToClassList("hd-board__slot");
                var portrait = new VisualElement(); portrait.AddToClassList("hd-board__slotportrait");
                var initial = new Label(); initial.AddToClassList("hd-board__slotinitial"); portrait.Add(initial);
                HdPortrait.Fill(portrait, initial, s.Portrait);
                if (s.Exhausted) portrait.style.backgroundColor = Theme.Danger;
                var name = new Label(s.Name); name.AddToClassList("hd-board__slotname");
                var bar = new VisualElement(); bar.AddToClassList("hd-board__slotbar");
                var fill = new VisualElement(); fill.AddToClassList("hd-board__slotfill");
                fill.style.width = Length.Percent(Mathf.Clamp01(s.Fatigue01) * 100f);
                fill.style.backgroundColor = Theme.FatigueColor(s.Fatigue);
                bar.Add(fill);
                slot.Add(portrait); slot.Add(name); slot.Add(bar);
                _squad.Add(slot);
            }
        }

        public void ShowHud(BoardHudData d)
        {
            _hudData = d;
            if (_hint == null) return;
            _hint.text = d.Hint; _pm.text = d.PmText;
            _reserveFill.style.width = Length.Percent(Mathf.Clamp01(d.Reserve01) * 100f);
            _reserveText.text = d.ReserveLabel;
            _roll.SetEnabled(d.RollEnabled);
            _bonus.text = d.BonusLabel; _bonus.SetEnabled(d.BonusEnabled);
            _end.SetEnabled(d.EndEnabled);
        }

        public void ShowMovePreview(MovePreviewData? data)
        {
            _previewData = data;
            if (_preview == null) return;
            _preview.style.display = data.HasValue ? DisplayStyle.Flex : DisplayStyle.None;
            if (!data.HasValue) return;
            var d = data.Value;
            _pvTitle.text = d.Title; _pvTitle.style.color = ToneColors.Of(d.Tone);
            _pvBody.text = d.Body; _pvDetail.text = d.Detail;
            _go.text = d.ConfirmLabel; _cancel.text = d.CancelLabel;
        }

        public UniTask RollDieAsync(int value, int faces, CancellationToken ct)
        {
            var done = new UniTaskCompletionSource();
            HudAnim.Run(_die, 0.5f, k => { if (k < 0.95f) _dieFace.text = UnityEngine.Random.Range(1, faces + 1).ToString(); }, () =>
            {
                _dieFace.text = value.ToString();
                HudAnim.Punch(_die, 0.25f, 0.3f);
                done.TrySetResult();
            });
            return done.Task.AttachExternalCancellation(ct);
        }

        public void ResetDie() { if (_dieFace != null) _dieFace.text = "-"; }
    }
}
