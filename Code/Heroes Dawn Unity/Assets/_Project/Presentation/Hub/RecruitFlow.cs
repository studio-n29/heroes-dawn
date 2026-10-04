#nullable enable
using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;

namespace N29.HeroesDawn.Presentation.Hub
{
    public enum RecruitOutcome { Cancelled, Recruited, Failed, Expired }

    /// <summary>
    /// Achat d'un héros de la boutique (page Recruter et fiche) : confirmation M31, puis achat. Un seul achat à la fois.
    /// Si la boutique a changé pendant la confirmation (9h00, renouvellement), l'achat est refusé sans débit.
    /// Rien n'est affiché après l'annulation du jeton (page quittée).
    /// </summary>
    public sealed class RecruitFlow
    {
        readonly IModalService _modals;
        readonly IRecruitFromShopUseCase _recruit;
        readonly ISessionReadModel _session;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly BusyGate _gate = new BusyGate();

        public RecruitFlow(IModalService modals, IRecruitFromShopUseCase recruit, ISessionReadModel session, IToastService toasts, ILocalization loc)
        {
            _modals = modals; _recruit = recruit; _session = session; _toasts = toasts; _loc = loc;
        }

        public bool IsBusy => _gate.IsBusy;

        public async UniTask<RecruitOutcome> RunAsync(int slot, CancellationToken ct)
        {
            if (ct.IsCancellationRequested || !_gate.TryEnter()) return RecruitOutcome.Cancelled;
            try { return await RunCoreAsync(slot, ct); }
            catch (OperationCanceledException) { return RecruitOutcome.Cancelled; }
            finally { _gate.Exit(); }
        }

        async UniTask<RecruitOutcome> RunCoreAsync(int slot, CancellationToken ct)
        {
            var shop = _session.Shop;
            if (slot < 0 || slot >= shop.Offers.Count) return Expired();
            var offer = shop.Offers[slot];
            var def = _session.Content.Member(offer.Id);
            var balance = RecruitBalance.Of(_session.Wallet.Pr, MetaRules.RecruitCost(def.Rarity));
            string day = shop.Day;
            int draw = shop.Draw;

            var view = await _modals.OpenAsync<IRecruitConfirmPopupView>(ct);
            bool ok;
            try { ok = await view.AskAsync(Data(def, balance), ct); }
            finally { _modals.Close(view); }
            if (!ok || !balance.CanAfford || ct.IsCancellationRequested) return RecruitOutcome.Cancelled;
            if (shop.Day != day || shop.Draw != draw) return Expired();

            var r = await _recruit.ExecuteAsync(slot, ct);
            if (ct.IsCancellationRequested) return RecruitOutcome.Cancelled;
            if (r.IsFailure)
            {
                if (r.Error == ShopErrors.Expired) return Expired();
                _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
                return RecruitOutcome.Failed;
            }
            _toasts.Show(_loc.F("hub.recruited", _loc.MemberName(def)), ToastKind.Reward);
            return RecruitOutcome.Recruited;
        }

        RecruitOutcome Expired()
        {
            _toasts.Show(_loc.Error(ShopErrors.Expired), ToastKind.Error);
            return RecruitOutcome.Expired;
        }

        RecruitConfirmData Data(Definitions.MemberDef def, RecruitBalance b)
        {
            var name = _loc.MemberName(def);
            return new RecruitConfirmData
            {
                Title = _loc.F("recruit.confirm.title", name),
                Card = MemberCards.Card(def, MetaRules.RecruitLevel, _session.Content.Balance, _loc),
                Combo = SignatureCombo(def),
                Before = b.Before + " PR",
                Cost = "-" + b.Cost + " PR",
                After = b.After + " PR",
                CanAfford = b.CanAfford,
                Missing = b.CanAfford ? "" : _loc.F("recruit.missing", b.Missing),
                HowToEarn = b.CanAfford ? "" : _loc.T("recruit.how_to_earn"),
                Ok = _loc.T("recruit.confirm.ok"),
                Cancel = _loc.T("ui.cancel"),
            };
        }

        /// <summary>Combo phare : le premier combo que le héros formerait avec un membre de l'escouade, sinon celui de sa famille.</summary>
        string SignatureCombo(Definitions.MemberDef def)
        {
            var combos = SquadCombos.For(def.Family, _session.Content.Combos);
            foreach (var c in combos)
                foreach (var m in _session.Squad)
                    if (c.Matches(def.Family, m.Def.Family))
                        return _loc.F("recruit.combo", _loc.ComboName(c), _loc.MemberName(m.Def));
            var first = combos.FirstOrDefault();
            return first == null ? "" : _loc.F("recruit.combo.family", _loc.ComboName(first));
        }
    }
}
