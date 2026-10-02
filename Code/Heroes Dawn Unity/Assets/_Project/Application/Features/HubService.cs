#nullable enable
using System.Collections.Generic;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features
{
    public static class HubErrors
    {
        public static readonly DomainError NotEnoughCredits = new DomainError("hub.credits", "errors.not_enough_credits");
        public static readonly DomainError NotEnoughPr = new DomainError("hub.pr", "errors.not_enough_pr");
        public static readonly DomainError AlreadyOwned = new DomainError("hub.owned", "errors.already_owned");
        public static readonly DomainError UnknownMember = new DomainError("hub.unknown", "errors.unknown_member");
        public static readonly DomainError MaxLevel = new DomainError("hub.max_level", "errors.max_level");
        public static readonly DomainError SquadFull = new DomainError("hub.squad_full", "errors.squad_full");
        public static readonly DomainError SquadEmpty = new DomainError("hub.squad_empty", "errors.squad_empty");
        public static readonly DomainError NothingToHeal = new DomainError("hub.no_fatigue", "errors.nothing_to_heal");
        public static readonly DomainError InMission = new DomainError("hub.in_mission", "errors.in_mission");
    }

    /// <summary>Actions du QG : escouade, entraînement, recrutement, infirmerie.</summary>
    public sealed class HubService
    {
        readonly GameSession _s;
        readonly SaveScheduler _save;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;

        public HubService(GameSession session, SaveScheduler save, IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster)
        {
            _s = session; _save = save; _wallet = wallet; _roster = roster;
        }

        public IReadOnlyList<MemberDef> AvailableRecruits()
        {
            var list = new List<MemberDef>();
            foreach (var m in _s.Content.Members) if (!_s.Owns(m.Id)) list.Add(m);
            return list;
        }

        public Result ToggleSquad(MemberId id)
        {
            if (_s.Run != null) return Result.Fail(HubErrors.InMission);
            var m = _s.Find(id);
            if (m == null) return Result.Fail(HubErrors.UnknownMember);
            int count = _s.Squad.Count;
            if (m.InSquad)
            {
                if (count <= 1) return Result.Fail(HubErrors.SquadEmpty);
                m.InSquad = false;
            }
            else
            {
                if (count >= MetaRules.SquadSize) return Result.Fail(HubErrors.SquadFull);
                m.InSquad = true;
            }
            _roster.PublishRoster();
            _save.RequestSave();
            return Result.Ok();
        }

        public Result Train(MemberId id)
        {
            var m = _s.Find(id);
            if (m == null) return Result.Fail(HubErrors.UnknownMember);
            if (m.Level >= MetaRules.MaxLevel) return Result.Fail(HubErrors.MaxLevel);
            if (!_s.Wallet.TrySpend(MetaRules.TrainCost(m.Level), 0)) return Result.Fail(HubErrors.NotEnoughCredits);
            m.Level++;
            _wallet.PublishWallet(_s);
            _roster.PublishRoster();
            _save.RequestSave();
            return Result.Ok();
        }

        public Result Recruit(MemberId id)
        {
            if (_s.Owns(id)) return Result.Fail(HubErrors.AlreadyOwned);
            MemberDef? def = null;
            foreach (var d in _s.Content.Members) if (d.Id == id) def = d;
            if (def == null) return Result.Fail(HubErrors.UnknownMember);
            if (!_s.Wallet.TrySpend(0, MetaRules.RecruitCost(def.Rarity))) return Result.Fail(HubErrors.NotEnoughPr);
            var member = new TeamMember(def, MetaRules.RecruitLevel);
            member.InSquad = _s.Squad.Count < MetaRules.SquadSize && _s.Run == null;
            _s.Roster.Add(member);
            _wallet.PublishWallet(_s);
            _roster.PublishRoster();
            _save.RequestSave();
            return Result.Ok();
        }

        /// <summary>Infirmerie : −30 % de fatigue pour tout l'effectif.</summary>
        public Result Heal()
        {
            bool any = false;
            foreach (var m in _s.Roster) if (m.Fatigue > 0) any = true;
            if (!any) return Result.Fail(HubErrors.NothingToHeal);
            if (!_s.Wallet.TrySpend(MetaRules.InfirmaryCost, 0)) return Result.Fail(HubErrors.NotEnoughCredits);
            foreach (var m in _s.Roster) m.AddFatigue(-MetaRules.InfirmaryReduction);
            _wallet.PublishWallet(_s);
            _roster.PublishRoster();
            _save.RequestSave();
            return Result.Ok();
        }
    }
}
