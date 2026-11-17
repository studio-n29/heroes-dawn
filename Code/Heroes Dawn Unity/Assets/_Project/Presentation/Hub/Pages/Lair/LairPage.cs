#nullable enable
using System;
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
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Écran Repaire (planche 12.4) : à gauche les boss du repaire (vaincu, prochain, plus loin), à droite la fiche du boss choisi
    /// (niveau, progression du repaire, lore, récompense) et LANCER L'OPÉRATION, qui demande confirmation puis lance le plateau du repaire comme une zone.
    /// La fiche suit le prochain boss tant que le joueur n'en a pas touché un autre.
    /// </summary>
    public sealed class LairPagePresenter : PresenterBase<ILairPageView>, IPageTitle
    {
        readonly ISessionReadModel _session;
        readonly LairBook _book;
        readonly MissionLauncher _mission;
        readonly IModalService _modals;
        readonly ILocalization _loc;
        readonly ISubscriber<MetaChanged> _meta;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        IDisposable? _subs;
        bool _confirming;
        int _chosen = -1;

        public LairPagePresenter(ILairPageView view, ISessionReadModel session, LairBook book, MissionLauncher mission, IModalService modals, ILocalization loc,
                                 IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                 ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, null)
        {
            _session = session; _book = book; _mission = mission; _modals = modals; _loc = loc;
            _meta = meta; _wallet = wallet; _roster = roster;
        }

        public string Title => _loc.T("page.lair");

        LairDef Lair => Lairs.All[0];

        protected override void OnStart()
        {
            View.BossSelected += OnSelect;
            View.EnterClicked += OnEnter;
        }

        protected override void OnShown()
        {
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _meta.Subscribe(_ => Refresh()).AddTo(bag);
            _wallet.Subscribe(_ => Refresh()).AddTo(bag);
            _roster.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
            Refresh();
        }

        protected override void OnHidden() { _subs?.Dispose(); _subs = null; }

        protected override void OnDispose()
        {
            _subs?.Dispose();
            View.BossSelected -= OnSelect;
            View.EnterClicked -= OnEnter;
        }

        void Refresh()
        {
            if (Disposed) return;
            View.Show(Build());
        }

        string ZoneShort(string zoneId)
        {
            var zone = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == zoneId);
            return zone == null ? zoneId : _loc.T(zone.NameKey.Replace(".name", ".short"));
        }

        LairScreenData Build()
        {
            var lair = Lair;
            bool open = _book.Unlocked(lair);
            var bosses = _book.Bosses(lair);
            int down = bosses.Count(b => b.Defeated);
            int level = _session.Content.Zones.FirstOrDefault(z => z.Id.Value == lair.Zone)?.RecommendedLevel ?? 1;
            int next = -1;
            for (int i = 0; i < bosses.Count && next < 0; i++) if (!bosses[i].Defeated) next = i;
            int selected = _chosen >= 0 && _chosen < bosses.Count ? _chosen : next >= 0 ? next : 0;

            var rows = new LairBossRowData[bosses.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                var b = bosses[i];
                rows[i] = new LairBossRowData(b.Boss.Enemy, _loc.T("enemies." + b.Boss.Enemy + ".name"), _loc.F("ui.level", level),
                                              b.Defeated, open && i == next, !open || (!b.Defeated && i != next), i == selected);
            }

            var current = bosses[selected].Boss;
            string enemy = current.Enemy;
            int pct = bosses.Count == 0 ? 0 : Mathf.RoundToInt(100f * down / bosses.Count);
            string enterReason = !open ? _loc.F("lair.locked", ZoneShort(lair.RequiresZone)) : _session.SquadExhausted ? _loc.T("hub.deploy.exhausted") : "";
            var squad = _session.Squad;
            return new LairScreenData
            {
                Title = _loc.T("lair.title"),
                LairName = _loc.T(lair.NameKey),
                ListTitle = _loc.T("lair.discovered"),
                Bosses = rows,
                SelectedIndex = selected,
                BossEnemyId = enemy,
                BossName = _loc.T("enemies." + enemy + ".name"),
                BossSubtitle = _loc.F("lair.sheet_sub", level, _loc.T("lair.role." + enemy)),
                ProgressTitle = _loc.T("lair.progress"),
                ProgressText = pct + " %",
                Progress = bosses.Count == 0 ? 0f : down / (float)bosses.Count,
                LoreTitle = _loc.T("lair.lore"),
                Lore = _loc.T("lair.lore." + enemy),
                RewardLine = RewardLine(current),
                SquadLine = squad.Count == 0 ? "" : _loc.F("lair.squad", squad.Count) + " · " + _loc.F("lair.fatigue", squad.Max(m => m.Fatigue)),
                EnterLabel = _loc.T("lair.enter"),
                CanEnter = open && !_session.SquadExhausted && !_confirming,
                EnterReason = enterReason,
            };
        }

        string RewardLine(LairBossDef boss)
        {
            var r = boss.Reward;
            if (r == null) return "";
            return _loc.F("lair.reward", r.Count + r.BonusCount, r.CatalystPercent);
        }

        void OnSelect(int index) { _chosen = index; Refresh(); }

        void OnEnter()
        {
            if (_confirming) return;
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
                bool ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T(lair.NameKey), _loc.F("lair.confirm.body", lair.Bosses.Count, level),
                                                                     _loc.T("lair.enter"), _loc.T("lair.confirm.cancel"), Tone.Danger), Lifetime);
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
