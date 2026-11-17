#nullable enable
using System;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Écran Tournoi (UI Toolkit, planches 13.1 et 13.2) : bandeau titre + état, classement à gauche, paliers / état / boutons à droite.
    /// Sous le format 3:2 le classement passe au-dessus du panneau de droite.
    /// </summary>
    public sealed class TournamentPageView : HdView, ITournamentPageView
    {
        const float NarrowAspect = 1.5f;

        VisualElement _page = null!, _tiers = null!, _fill = null!;
        ScrollView _rows = null!;
        Label _title = null!, _state = null!, _rankingTitle = null!, _rankingEmpty = null!, _tiersTitle = null!, _statusTitle = null!, _statusMain = null!, _statusSub = null!;
        UnityEngine.UIElements.Button _claim = null!, _duel = null!, _back = null!;
        TournamentScreenData? _last;

        public event Action? ClaimClicked;
        public event Action? BackToQgClicked;

        public HdButtonRef ClaimButton { get; private set; } = null!;
        public HdButtonRef BackButton { get; private set; } = null!;

        protected override void Bind()
        {
            _page = Q<VisualElement>("page"); _tiers = Q<VisualElement>("tiers"); _fill = Q<VisualElement>("progressFill");
            _rows = Q<ScrollView>("rankRows");
            _title = Q<Label>("title"); _state = Q<Label>("state"); _rankingTitle = Q<Label>("rankingTitle"); _rankingEmpty = Q<Label>("rankingEmpty");
            _tiersTitle = Q<Label>("tiersTitle"); _statusTitle = Q<Label>("statusTitle"); _statusMain = Q<Label>("statusMain"); _statusSub = Q<Label>("statusSub");
            _claim = Q<UnityEngine.UIElements.Button>("claim"); _duel = Q<UnityEngine.UIElements.Button>("duel"); _back = Q<UnityEngine.UIElements.Button>("backToQg");
            ClaimButton = new HdButtonRef(_claim, this); BackButton = new HdButtonRef(_back, this);
            _claim.clicked += () => ClaimClicked?.Invoke();
            _back.clicked += () => BackToQgClicked?.Invoke();
            Root.RegisterCallback<GeometryChangedEvent>(_ => ApplyAspect());
            ApplyAspect();
            if (_last.HasValue) Show(_last.Value);
        }

        void ApplyAspect()
        {
            var r = Root.contentRect;
            if (r.height <= 0f) return;
            _page.EnableInClassList("hd-tourney--narrow", r.width / r.height < NarrowAspect);
        }

        public void Show(TournamentScreenData d)
        {
            _last = d;
            if (_title == null) return;
            _title.text = d.Title;
            _state.text = d.StateLabel;
            _rankingTitle.text = d.RankingTitle;
            _tiersTitle.text = d.TiersTitle;
            _statusTitle.text = d.StatusTitle;
            _statusMain.text = d.StatusMain;
            _statusSub.text = d.StatusSub;
            _fill.style.width = Length.Percent(Mathf.Clamp01(d.Progress) * 100f);

            _rows.Clear();
            foreach (var r in d.Ranking)
            {
                var row = new VisualElement { pickingMode = PickingMode.Ignore };
                row.AddToClassList("hd-rank");
                row.EnableInClassList("hd-rank--me", r.IsPlayer);
                row.Add(Text(r.Rank.ToString(), "hd-rank__n"));
                row.Add(Text(r.Name, "hd-rank__name"));
                row.Add(Text(r.Points, "hd-rank__pts"));
                _rows.Add(row);
            }
            bool empty = d.Ranking.Length == 0;
            _rankingEmpty.text = d.RankingEmpty;
            _rankingEmpty.style.display = empty ? DisplayStyle.Flex : DisplayStyle.None;
            _rows.style.display = empty ? DisplayStyle.None : DisplayStyle.Flex;

            _tiers.Clear();
            foreach (var t in d.Tiers)
            {
                var chip = new VisualElement { pickingMode = PickingMode.Ignore };
                chip.AddToClassList("hd-chip");
                chip.EnableInClassList("hd-chip--done", t.State == TournamentTierState.Done);
                chip.EnableInClassList("hd-chip--current", t.State == TournamentTierState.Current);
                chip.Add(Text(t.Label, "hd-chip__label"));
                _tiers.Add(chip);
            }

            // Tournoi en cours : Réclamer + Duel ; hors période : Retour au QG seul.
            _claim.style.display = d.Finished ? DisplayStyle.None : DisplayStyle.Flex;
            _duel.style.display = d.Finished ? DisplayStyle.None : DisplayStyle.Flex;
            _back.style.display = d.Finished ? DisplayStyle.Flex : DisplayStyle.None;
            _claim.text = d.ClaimLabel; _claim.SetEnabled(d.CanClaim); _claim.tooltip = d.ClaimReason;
            _duel.text = d.CanDuel ? d.DuelLabel : d.DuelReason; _duel.SetEnabled(d.CanDuel); _duel.tooltip = d.DuelReason;
            _back.text = d.BackLabel;
        }

        static Label Text(string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            return l;
        }
    }
}
