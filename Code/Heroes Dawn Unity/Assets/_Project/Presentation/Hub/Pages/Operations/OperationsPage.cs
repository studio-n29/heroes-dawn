#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IOperationsPageView : IListPageView { }

    /// <summary>
    /// Salle des opérations : à gauche la liste des missions (durée, difficulté, gains, état), à droite la fiche de la mission choisie
    /// avec les agents hors escouade (les agents en mission ne se choisissent pas) et le bouton Lancer, qui demande confirmation.
    /// </summary>
    public sealed class OperationsPagePresenter : ListPagePresenter<IOperationsPageView>
    {
        readonly Expeditions _ops;
        readonly ISessionReadModel _session;
        readonly IModalService _modals;
        /// <summary>Mission affichée dans la fiche (index dans <see cref="Expeditions.Templates"/>).</summary>
        int _selected;
        /// <summary>Agents choisis pour la mission affichée, dans l'ordre du choix.</summary>
        readonly List<string> _picked = new List<string>();
        bool _confirming;
        const int AgentsPerPage = 4;
        int _page;

        public OperationsPagePresenter(IOperationsPageView view, Expeditions ops, ISessionReadModel session, IModalService modals, ILocalization loc, IToastService toasts,
                                       ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _ops = ops; _session = session; _modals = modals;
        }

        public override string Title => Loc.T("page.operations");
        protected override bool HasTimers => true;

        ExpeditionTemplate Selected => Expeditions.Templates[Mathf.Clamp(_selected, 0, Expeditions.Templates.Count - 1)];

        protected override void Render()
        {
            var agents = _ops.Agents();
            _picked.RemoveAll(id => !agents.Any(a => a.Member.Id.Value == id && !a.Busy)); // un agent parti entre-temps n'est plus choisi
            int free = agents.Count(a => !a.Busy);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("ops.title"),
                Line = free > 0 ? Loc.F("ops.free", free) : Loc.T("ops.none_free"),
                Progress = -1f,
            });
            var slots = _ops.SlotViews();
            var rows = new InfoRowData[Expeditions.Templates.Count];
            for (int i = 0; i < rows.Length; i++) rows[i] = RowOf(i, slots);
            View.ShowRows(rows);
            ShowSheet(agents, slots);
        }

        static string DifficultyKey(ExpeditionTemplate t) => "ops.difficulty." + t.Difficulty;

        static Color DifficultyColor(ExpeditionTemplate t) => t.Difficulty >= 3 ? Theme.Danger : t.Difficulty == 2 ? Theme.Accent : Theme.Success;

        InfoRowData RowOf(int index, IReadOnlyList<ExpeditionSlotView> slots)
        {
            var tpl = Expeditions.Templates[index];
            var r = InfoRowData.Basic(Loc.T("ops." + tpl.Id), Loc.F("ops.line", Loc.T(DifficultyKey(tpl)), MetaFormat.Duration(TimeSpan.FromMinutes(tpl.Minutes)), tpl.Members, MetaFormat.RewardText(tpl.Reward, Loc)));
            r.Tag = tpl.Difficulty.ToString();
            r.TagColor = DifficultyColor(tpl);
            r.Highlight = index == _selected;
            var view = slots.FirstOrDefault(s => !s.Empty && s.Template!.Id == tpl.Id);
            if (view.Template == null) return r;
            if (view.Ready)
            {
                r.TagColor = Theme.Success;
                r.Highlight = true;
                r.Progress = 1f;
                r.ProgressLabel = Loc.T("ops.back");
                r.A = RowButtonData.Of(Loc.T("ops.claim"), true, "", 0);
            }
            else
            {
                r.TagColor = Theme.Info;
                r.Progress = Mathf.Clamp01(1f - (float)(view.Left.TotalMinutes / tpl.Minutes));
                r.ProgressLabel = MetaFormat.Duration(view.Left);
                int cost = Expeditions.SpeedUpCost(view.Left);
                r.A = RowButtonData.Of(Loc.F("ops.speed", cost), _session.Wallet.Gems >= cost, Loc.T("errors.not_enough_gems"), 1);
            }
            return r;
        }

        /// <summary>Fiche de la mission choisie : ses agents si elle est lancée, sinon les agents à choisir.</summary>
        void ShowSheet(IReadOnlyList<MissionAgentView> agents, IReadOnlyList<ExpeditionSlotView> slots)
        {
            var tpl = Selected;
            string text = Loc.F("ops.gains", MetaFormat.RewardText(tpl.Reward, Loc));
            var info = InfoRowData.Basic(Loc.F("ops.sheet", Loc.T(DifficultyKey(tpl)), MetaFormat.Duration(TimeSpan.FromMinutes(tpl.Minutes))), Loc.T("ops." + tpl.Id + ".desc"));
            info.Tag = tpl.Difficulty.ToString();
            info.TagColor = DifficultyColor(tpl);
            info.Dim = true;
            var sideRows = new List<InfoRowData> { info };
            var running = slots.FirstOrDefault(s => !s.Empty && s.Template!.Id == tpl.Id);
            if (running.Template != null)
            {
                foreach (var id in running.Members)
                {
                    var m = _session.Roster.FirstOrDefault(x => x.Id.Value == id);
                    var r = InfoRowData.Basic(Loc.T("members." + id + ".name"), running.Ready ? Loc.T("ops.back") : Loc.F("ops.agent_left", MetaFormat.Duration(running.Left)));
                    r.Tag = m != null ? m.Level.ToString() : "";
                    r.TagColor = Theme.Info;
                    sideRows.Add(r);
                }
            }
            else
            {
                int pages = Math.Max(1, (agents.Count + AgentsPerPage - 1) / AgentsPerPage);
                _page = Mathf.Clamp(_page, 0, pages - 1);
                foreach (var a in agents.Skip(_page * AgentsPerPage).Take(AgentsPerPage)) sideRows.Add(AgentRow(a, tpl));
                var go = InfoRowData.Basic(Loc.F("ops.team", _picked.Count, tpl.Members), pages > 1 ? Loc.F("ops.page", _page + 1, pages) : "");
                go.Tag = "GO";
                go.TagColor = _picked.Count == tpl.Members ? Theme.Success : Theme.Surface3;
                go.A = RowButtonData.Of(Loc.T("ops.launch"), _picked.Count == tpl.Members && !_confirming, Loc.F("ops.need_pick", tpl.Members), 0);
                if (pages > 1)
                {
                    go.B = RowButtonData.Of("<", _page > 0, "", 3);
                    go.C = RowButtonData.Of(">", _page < pages - 1, "", 3);
                }
                sideRows.Add(go);
            }
            View.ShowSide(new ListSideData { Title = Loc.T("ops." + tpl.Id), Rows = sideRows.ToArray(), Text = text });
        }

        InfoRowData AgentRow(MissionAgentView a, ExpeditionTemplate tpl)
        {
            var id = a.Member.Id.Value;
            bool picked = _picked.Contains(id);
            var r = InfoRowData.Basic(Loc.T("members." + id + ".name"), a.Busy ? Loc.F("ops.agent_left", MetaFormat.Duration(a.Left)) : Loc.T("ops.agent_free"));
            r.Tag = a.Member.Level.ToString();
            r.TagColor = a.Busy ? Theme.Surface3 : Theme.Info;
            r.Dim = a.Busy;
            r.Highlight = picked;
            bool full = _picked.Count >= tpl.Members;
            if (a.Busy) r.A = RowButtonData.Of(Loc.T("ops.pick"), false, Loc.T("ops.agent_busy"), 3);
            else if (picked) r.A = RowButtonData.Of(Loc.T("ops.unpick"), true, "", 2);
            else r.A = RowButtonData.Of(Loc.T("ops.pick"), !full, Loc.F("ops.team_full", tpl.Members), 0);
            return r;
        }

        protected override void OnRowSelected(int row)
        {
            if (row < 0 || row >= Expeditions.Templates.Count || row == _selected) return;
            _selected = row;
            _page = 0;
            _picked.Clear();
            Refresh();
        }

        protected override void OnRowButton(int row, int button)
        {
            if (row < 0 || row >= Expeditions.Templates.Count) return;
            int slot = _ops.SlotOf(Expeditions.Templates[row].Id);
            if (slot < 0) return;
            var view = _ops.SlotViews()[slot];
            if (view.Ready) Report(_ops.Claim(slot), "ops.claimed");
            else Report(_ops.SpeedUp(slot));
        }

        /// <summary>Rangées de la fiche : 0 = résumé, puis les agents de la page, puis la ligne d'équipe (Lancer, page précédente, page suivante).</summary>
        protected override void OnSideButton(int row, int button)
        {
            var tpl = Selected;
            if (_ops.SlotOf(tpl.Id) >= 0) return;
            var agents = _ops.Agents();
            int shown = Math.Min(AgentsPerPage, Math.Max(0, agents.Count - _page * AgentsPerPage));
            if (row >= 1 && row <= shown)
            {
                var a = agents[_page * AgentsPerPage + row - 1];
                var id = a.Member.Id.Value;
                if (_picked.Contains(id)) _picked.Remove(id);
                else if (!a.Busy && _picked.Count < tpl.Members) _picked.Add(id);
                Refresh();
            }
            else if (row == shown + 1)
            {
                if (button == 0) { if (_picked.Count == tpl.Members) ConfirmAsync(tpl).Forget(); }
                else { _page += button == 1 ? -1 : 1; Refresh(); }
            }
        }

        async UniTaskVoid ConfirmAsync(ExpeditionTemplate tpl)
        {
            if (_confirming) return;
            _confirming = true;
            try
            {
                var names = string.Join(", ", _picked.Select(id => Loc.T("members." + id + ".name")));
                bool ok = await _modals.ConfirmAsync(new ConfirmData(Loc.T("ops.confirm.title"),
                    Loc.F("ops.confirm.body", Loc.T("ops." + tpl.Id), names, MetaFormat.Duration(TimeSpan.FromMinutes(tpl.Minutes))),
                    Loc.T("ops.launch"), Loc.T("ui.cancel"), Tone.Primary), Lifetime);
                if (!ok || Disposed) return;
                if (Report(_ops.Launch(tpl.Id, _picked.ToList()), Loc.T("ops.started"))) _picked.Clear();
            }
            catch (OperationCanceledException) { }
            finally { _confirming = false; if (!Disposed) Refresh(); }
        }
    }
}
