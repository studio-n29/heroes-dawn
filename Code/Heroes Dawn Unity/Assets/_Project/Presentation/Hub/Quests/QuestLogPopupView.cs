#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Suivi des quêtes (fenêtre UI Toolkit) : activités du jour, quêtes principales, puis quêtes secondaires à réclamer. Retour = Fermer.</summary>
    public sealed class QuestLogPopupView : HdPopup, IQuestLogPopupView
    {
        /// <summary>Ce que les tests lisent d'une quête principale affichée.</summary>
        public sealed class MainRowInfo { public int Status; public string Title = ""; }

        /// <summary>Ce que les tests lisent d'une quête secondaire affichée, avec son bouton Réclamer.</summary>
        public sealed class SideRowInfo
        {
            public string Id = "";
            public SideQuestRowData Data;
            public HdButtonRef ClaimButton = null!;
        }

        readonly List<MainRowInfo> _main = new List<MainRowInfo>();
        readonly List<SideRowInfo> _side = new List<SideRowInfo>();
        readonly List<SideRowInfo> _activities = new List<SideRowInfo>();
        Label _title = null!;
        ScrollView _body = null!;
        UnityEngine.UIElements.Button _close = null!;
        UniTaskCompletionSource? _done;
        QuestLogData? _last;

        public override bool Cancelable => true;
        public event Action<string>? ClaimClicked;
        public IReadOnlyList<MainRowInfo> MainRows => _main;
        public IReadOnlyList<SideRowInfo> SideRows => _side;
        public IReadOnlyList<SideRowInfo> ActivityRows => _activities;
        public HdButtonRef CloseButton { get; private set; } = null!;

        protected override VisualElement DefaultFocus => _close;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _body = Q<ScrollView>("body"); _close = Q<UnityEngine.UIElements.Button>("close");
            CloseButton = new HdButtonRef(_close, this);
            _close.clicked += () => _done?.TrySetResult();
            if (_last.HasValue) Refresh(_last.Value);
        }

        public UniTask ShowAsync(QuestLogData d, CancellationToken ct)
        {
            Refresh(d);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }

        public void Refresh(QuestLogData d)
        {
            _last = d;
            _main.Clear(); _side.Clear(); _activities.Clear();
            if (_body == null) return;
            _title.text = d.Title;
            _close.text = d.Close;
            _body.Clear();
            Section(d.ActivityTitle, d.Activities.Length == 0 ? d.ActivityEmpty : null);
            foreach (var s in d.Activities) _activities.Add(SideRow(s));
            Section(d.MainTitle, null);
            foreach (var m in d.Main) MainRow(m);
            Section(d.SideTitle, d.Side.Length == 0 ? d.SideEmpty : null);
            foreach (var s in d.Side) _side.Add(SideRow(s));
        }

        void Section(string title, string? empty)
        {
            var label = new Label(title);
            label.AddToClassList("hd-quests__section");
            _body.Add(label);
            if (empty == null) return;
            var e = new Label(empty);
            e.AddToClassList("hd-quests__empty");
            _body.Add(e);
        }

        void MainRow(MainQuestRowData d)
        {
            _main.Add(new MainRowInfo { Status = d.Status, Title = d.Title });
            var row = new VisualElement();
            row.AddToClassList("hd-quest");
            row.AddToClassList(d.Status == 2 ? "hd-quest--done" : d.Status == 1 ? "hd-quest--active" : "hd-quest--locked");
            var accent = new VisualElement { pickingMode = PickingMode.Ignore };
            accent.AddToClassList("hd-quest__accent");
            var text = new VisualElement { pickingMode = PickingMode.Ignore };
            text.AddToClassList("hd-quest__text");
            var title = new Label(d.Title) { pickingMode = PickingMode.Ignore };
            title.AddToClassList("hd-quest__title");
            var detail = new Label(d.Detail) { pickingMode = PickingMode.Ignore };
            detail.AddToClassList("hd-quest__detail");
            text.Add(title); text.Add(detail);
            var state = new Label(d.State) { pickingMode = PickingMode.Ignore };
            state.AddToClassList("hd-quest__state");
            // Icône d'état du kit (étoile : en cours, coche : terminée, cadenas : verrouillée) ; la classe de la ligne choisit l'image.
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("hd-quest__icon");
            row.Add(accent); row.Add(icon); row.Add(text); row.Add(state);
            _body.Add(row);
        }

        SideRowInfo SideRow(SideQuestRowData d)
        {
            var row = new VisualElement();
            row.AddToClassList("hd-quest");
            row.AddToClassList("hd-quest--side");
            var text = new VisualElement { pickingMode = PickingMode.Ignore };
            text.AddToClassList("hd-quest__text");
            var title = new Label(d.Title) { pickingMode = PickingMode.Ignore };
            title.AddToClassList("hd-quest__title");
            var desc = new Label(d.Description) { pickingMode = PickingMode.Ignore };
            desc.AddToClassList("hd-quest__detail");
            var bar = new VisualElement { pickingMode = PickingMode.Ignore };
            bar.AddToClassList("hd-bar"); bar.AddToClassList("hd-quest__bar");
            var fill = new VisualElement { pickingMode = PickingMode.Ignore };
            fill.AddToClassList("hd-bar__fill");
            fill.style.width = Length.Percent(Mathf.Clamp01(d.Progress01) * 100f);
            if (d.Claimed) fill.style.unityBackgroundImageTintColor = new Color(0.376f, 0.729f, 0.608f);
            var barLabel = new Label(d.ProgressText) { pickingMode = PickingMode.Ignore };
            barLabel.AddToClassList("hd-list__barlabel");
            bar.Add(fill); bar.Add(barLabel);
            var reward = new Label(d.RewardText) { pickingMode = PickingMode.Ignore };
            reward.AddToClassList("hd-quest__reward");
            text.Add(title); text.Add(desc); text.Add(bar); text.Add(reward);
            var claim = new UnityEngine.UIElements.Button { text = d.ClaimLabel };
            claim.AddToClassList("hd-btn"); claim.AddToClassList("hd-btn--primary"); claim.AddToClassList("hd-btn--small"); claim.AddToClassList("hd-quest__claim");
            claim.style.display = d.Claimed ? DisplayStyle.None : DisplayStyle.Flex;
            claim.SetEnabled(d.Claimable);
            string id = d.Id;
            claim.clicked += () => ClaimClicked?.Invoke(id);
            row.Add(text); row.Add(claim);
            _body.Add(row);
            return new SideRowInfo { Id = d.Id, Data = d, ClaimButton = new HdButtonRef(claim, this) };
        }

        public override void RequestCancel() => _done?.TrySetResult();
    }
}
