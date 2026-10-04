#nullable enable
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IPassPageView : IListPageView { }

    public sealed class PassPagePresenter : ListPagePresenter<IPassPageView>
    {
        readonly PassProgress _pass;
        readonly DailyMissions _daily;

        public PassPagePresenter(IPassPageView view, PassProgress pass, DailyMissions daily, ILocalization loc, IToastService toasts,
                                 ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _pass = pass; _daily = daily;
        }

        public override string Title => Loc.T("page.pass");
        protected override bool HasTimers => true;

        protected override void Render()
        {
            int claimable = _pass.ClaimableCount();
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.F("pass.level", _pass.Level, PassTrack.Levels),
                Line = Loc.F("pass.season", MetaFormat.Duration(_pass.TimeLeft())),
                Progress = _pass.IsMax ? 1f : _pass.XpInLevel / (float)PassTrack.XpPerLevel,
                ProgressLabel = _pass.IsMax ? Loc.T("pass.max") : Loc.F("pass.xp", _pass.XpInLevel, PassTrack.XpPerLevel),
                A = _pass.PremiumOwned
                    ? RowButtonData.Of(Loc.T("pass.premium_on"), false, "", 3)
                    : RowButtonData.Of(Loc.F("pass.buy_premium", PassTrack.PremiumCost), true, "", 1),
                B = RowButtonData.Of(claimable > 0 ? Loc.F("pass.claim_all", claimable) : Loc.T("pass.nothing"), claimable > 0, Loc.T("pass.nothing_reason"), 0),
            });

            var levels = _pass.Levels();
            var rows = new InfoRowData[levels.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                var l = levels[i];
                rows[i] = new InfoRowData
                {
                    Tag = l.Level.ToString(),
                    TagColor = l.Reached ? Theme.Primary : Theme.Surface3,
                    Title = Loc.F("pass.free", MetaFormat.RewardText(l.Free, Loc)),
                    Subtitle = Loc.F("pass.prem", MetaFormat.RewardText(l.Premium, Loc)),
                    Progress = -1f,
                    Highlight = l.Level == _pass.Level + 1 || (l.FreeClaimable || l.PremiumClaimable),
                    Dim = !l.Reached,
                    A = l.FreeClaimed ? RowButtonData.Of(Loc.T("pass.taken"), false, "", 3)
                      : RowButtonData.Of(Loc.T("pass.take_free"), l.Reached, Loc.F("pass.locked_level", l.Level), 0),
                    B = l.PremiumClaimed ? RowButtonData.Of(Loc.T("pass.taken"), false, "", 3)
                      : RowButtonData.Of(Loc.T("pass.take_premium"), l.PremiumClaimable, l.PremiumOwned ? Loc.F("pass.locked_level", l.Level) : Loc.T("pass.premium_needed"), 1),
                };
            }
            View.ShowRows(rows);

            var missions = _daily.Today();
            var side = new InfoRowData[missions.Count];
            for (int i = 0; i < side.Length; i++)
            {
                var m = missions[i];
                side[i] = new InfoRowData
                {
                    Tag = (i + 1).ToString(),
                    TagColor = m.Claimed ? Theme.Success : m.Complete ? Theme.Primary : Theme.Info,
                    Title = Loc.F("daily." + m.Template.Id, m.Template.Target),
                    Subtitle = MetaFormat.RewardText(m.Template.Reward, Loc),
                    Progress = Mathf.Clamp01(m.Progress / (float)m.Template.Target),
                    ProgressLabel = m.Progress + " / " + m.Template.Target,
                    Highlight = m.Claimable,
                    Dim = m.Claimed,
                    A = m.Claimed ? RowButtonData.Of(Loc.T("pass.taken"), false, "", 3) : RowButtonData.Of(Loc.T("daily.claim"), m.Claimable, Loc.T("daily.not_done"), 0),
                };
            }
            View.ShowSide(new ListSideData
            {
                Title = Loc.T("qg.daily"),
                Rows = side,
                Text = Loc.F("daily.reset", MetaFormat.Duration(_daily.UntilReset())),
            });
        }

        protected override void OnHeader(int index)
        {
            if (index == 0) { if (Report(_pass.BuyPremium(), Loc.T("pass.premium_bought"))) { } }
            else Report(_pass.ClaimAll(), "pass.claimed");
        }

        protected override void OnRowButton(int row, int button)
        {
            var levels = _pass.Levels();
            if (row < 0 || row >= levels.Count) return;
            Report(_pass.Claim(levels[row].Level, button == 1), "pass.claimed");
        }

        protected override void OnSideButton(int row, int button)
        {
            var missions = _daily.Today();
            if (row < 0 || row >= missions.Count) return;
            Report(_daily.Claim(missions[row].Template.Id), "daily.claimed");
        }
    }
}
