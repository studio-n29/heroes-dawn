#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface ILairPageView : IListPageView { }

    /// <summary>
    /// Page Repaire : à gauche les étapes (un boss par rangée, vaincu ou à affronter), à droite la préparation (niveau conseillé, escouade, fatigue)
    /// et le bilan (boss vaincus, repaires terminés). Le bouton ENTRER du bandeau demande confirmation puis lance le plateau du repaire,
    /// comme une zone : la progression des boss est retenue d'une partie à l'autre, la validation de la zone verse les récompenses.
    /// </summary>
    public sealed class LairPagePresenter : ListPagePresenter<ILairPageView>
    {
        readonly ISessionReadModel _session;
        readonly LairBook _book;
        readonly MissionLauncher _mission;
        readonly IModalService _modals;
        bool _confirming;

        public LairPagePresenter(ILairPageView view, ISessionReadModel session, LairBook book, MissionLauncher mission, IModalService modals, ILocalization loc,
                                 IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                 ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _session = session; _book = book; _mission = mission; _modals = modals;
        }

        public override string Title => Loc.T("page.lair");

        LairDef Lair => Lairs.All[0];

        string ZoneShort(string zoneId)
        {
            var zone = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == zoneId);
            return zone == null ? zoneId : Loc.T(zone.NameKey.Replace(".name", ".short"));
        }

        protected override void Render()
        {
            var lair = Lair;
            bool open = _book.Unlocked(lair);
            var bosses = _book.Bosses(lair);
            int down = bosses.Count(b => b.Defeated);
            string enterReason = !open ? Loc.F("lair.locked", ZoneShort(lair.RequiresZone)) : _session.SquadExhausted ? Loc.T("hub.deploy.exhausted") : "";
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("lair.title"),
                Line = open ? Loc.F("lair.line", Loc.T(lair.NameKey), down, bosses.Count, _book.Clears(lair)) : enterReason,
                Progress = open ? down / (float)bosses.Count : 0f,
                ProgressLabel = down + " / " + bosses.Count,
                A = RowButtonData.Of(Loc.T("lair.enter"), open && !_session.SquadExhausted && !_confirming, enterReason, 0),
            });

            var rows = new InfoRowData[bosses.Count];
            bool markedNext = false;
            for (int i = 0; i < rows.Length; i++)
            {
                var b = bosses[i];
                bool next = !b.Defeated && !markedNext;
                markedNext |= next;
                var r = InfoRowData.Basic(Loc.T("enemies." + b.Boss.Enemy + ".name"),
                                          b.Defeated ? Loc.T("lair.boss.down") : next ? Loc.F("lair.boss.next", i + 1) : Loc.F("lair.boss.later", i + 1));
                r.Tag = (i + 1).ToString();
                r.TagColor = b.Defeated ? Theme.Success : next ? Theme.Primary : Theme.TextMuted;
                r.Highlight = open && next;
                r.Dim = !open || (!b.Defeated && !next);
                rows[i] = r;
            }
            View.ShowRows(rows);
            ShowPreparation(lair, open, down, bosses.Count);
        }

        void ShowPreparation(LairDef lair, bool open, int down, int total)
        {
            var side = new List<InfoRowData>();
            int level = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == lair.Zone)?.RecommendedLevel ?? 1;
            var info = InfoRowData.Basic(Loc.F("lair.recommended", level), Loc.T(lair.DescKey));
            info.Tag = "!";
            info.TagColor = Theme.Accent;
            info.Dim = true;
            side.Add(info);
            var squad = _session.Squad;
            var team = InfoRowData.Basic(Loc.F("lair.squad", squad.Count), string.Join(" · ", squad.Select(m => Loc.MemberName(m.Def) + " " + m.Level)) + " — " + Loc.F("lair.fatigue", squad.Count == 0 ? 0 : squad.Max(m => m.Fatigue)));
            team.Tag = "A";
            team.TagColor = squad.Any(m => m.Exhausted) ? Theme.Danger : Theme.Info;
            side.Add(team);
            var bilan = InfoRowData.Basic(Loc.T("lair.bilan"), Loc.F("lair.bilan.line", down, total, _book.Clears(lair)));
            bilan.Tag = "B";
            bilan.TagColor = _book.Clears(lair) > 0 ? Theme.Success : Theme.TextMuted;
            side.Add(bilan);
            View.ShowSide(new ListSideData { Title = Loc.T(lair.NameKey), Rows = side.ToArray(), Text = "" });
        }

        protected override void OnHeader(int index)
        {
            if (index != 0 || _confirming) return;
            var lair = Lair;
            if (!_book.Unlocked(lair) || _session.SquadExhausted) return;
            ConfirmAsync(lair).Forget();
        }

        async UniTaskVoid ConfirmAsync(LairDef lair)
        {
            _confirming = true;
            Refresh();
            try
            {
                int level = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == lair.Zone)?.RecommendedLevel ?? 1;
                bool ok = await _modals.ConfirmAsync(new ConfirmData(Loc.T(lair.NameKey), Loc.F("lair.confirm.body", lair.Bosses.Count, level),
                                                                     Loc.T("lair.enter"), Loc.T("lair.confirm.cancel"), Tone.Danger), Lifetime);
                if (ok && !Disposed) await _mission.TryLaunchAsync(new ZoneId(lair.Zone), Lifetime);
            }
            catch (OperationCanceledException) { }
            finally
            {
                _confirming = false;
                if (!Disposed) Refresh();
            }
        }
    }
}
