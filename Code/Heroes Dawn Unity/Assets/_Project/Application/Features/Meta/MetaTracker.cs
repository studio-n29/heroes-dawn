#nullable enable
using System;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Une récompense de méta-progression a été versée (l'interface l'annonce en toast).</summary>
    public readonly struct MetaRewardEarned
    {
        public readonly string SourceKey;
        public readonly Reward Reward;
        public MetaRewardEarned(string sourceKey, Reward reward) { SourceKey = sourceKey; Reward = reward; }
    }

    /// <summary>
    /// Écoute le jeu (combats gagnés, zones validées, entraînements, recrutements, Crédits gagnés) pour faire avancer les missions du jour,
    /// le pass et les objectifs secondaires. Les entraînements et recrutements sont déduits des changements de l'effectif.
    /// </summary>
    public sealed class MetaTracker : IDisposable
    {
        public const int XpPerWin = 10, XpPerFirstClear = 50, XpPerReplay = 20, GemsPerFirstClear = 10;

        readonly MetaContext _c;
        readonly DailyMissions _daily;
        readonly PassProgress _pass;
        readonly SecondaryObjectives _secondary;
        readonly QuestLog _quests;
        readonly ISubscriber<CombatFinished> _combat;
        readonly ISubscriber<ZoneValidated> _zone;
        readonly ISubscriber<RosterChanged> _roster;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<GameStateChanged> _state;
        readonly IPublisher<MetaRewardEarned> _earned;
        IDisposable? _subs;
        int _levelSum, _count, _credits;
        // Sauvegarde vue au dernier instantané : une autre instance (profil effacé ou rechargé) repart de zéro sans compter de gain.
        N29.HeroesDawn.Application.Ports.MetaSave? _metaSeen;
        bool _armed;

        public MetaTracker(MetaContext context, DailyMissions daily, PassProgress pass, SecondaryObjectives secondary, QuestLog quests,
                           ISubscriber<CombatFinished> combat, ISubscriber<ZoneValidated> zone, ISubscriber<RosterChanged> roster,
                           ISubscriber<WalletChanged> wallet, ISubscriber<GameStateChanged> state, IPublisher<MetaRewardEarned> earned)
        {
            _c = context; _daily = daily; _pass = pass; _secondary = secondary; _quests = quests;
            _combat = combat; _zone = zone; _roster = roster; _wallet = wallet; _state = state; _earned = earned;
        }

        public void Start()
        {
            if (_subs != null) return;
            Snapshot();
            var bag = DisposableBag.CreateBuilder();
            _combat.Subscribe(OnCombat).AddTo(bag);
            _zone.Subscribe(OnZone).AddTo(bag);
            _roster.Subscribe(_ => OnRoster()).AddTo(bag);
            _wallet.Subscribe(OnWallet).AddTo(bag);
            _state.Subscribe(OnState).AddTo(bag);
            _subs = bag.Build();
        }

        /// <summary>Repart de l'état actuel (nouvelle connexion, sauvegarde effacée) sans compter de progression.</summary>
        public void Snapshot()
        {
            _count = _c.Session.Roster.Count;
            _levelSum = 0;
            foreach (var m in _c.Session.Roster) _levelSum += m.Level;
            _credits = _c.Session.Wallet.Credits;
            _metaSeen = _c.Meta;
        }

        void OnState(GameStateChanged s)
        {
            _armed = s.To != GameState.Login && s.To != GameState.Boot;
            if (s.From == GameState.Login || s.From == GameState.Boot) Snapshot(); // profil chargé : on ne compte pas son contenu
        }

        void OnCombat(CombatFinished f)
        {
            Stats.Add(_c.Meta, f.Report.Victory ? Stats.CombatsWon : Stats.CombatsLost, 1);
            _c.Touch(MetaArea.Profile);
            if (!f.Report.Victory) return;
            _daily.Record(DailyKind.WinCombats);
            if (_c.Session.RunZone != null) _quests.RecordCombatWon(_c.Session.RunZone.Id.Value);
            _quests.Record(QuestMetric.CombatsWon);
            _pass.AddXp(XpPerWin);
        }

        void OnZone(ZoneValidated z)
        {
            var id = _c.Session.RunZone?.Id.Value ?? "";
            _daily.Record(DailyKind.ValidateZone);
            if (id.Length > 0) _quests.RecordZoneValidated(id);
            if (z.Rewards.FirstClear) _quests.Record(QuestMetric.ZonesCleared);
            _pass.AddXp(z.Rewards.FirstClear ? XpPerFirstClear : XpPerReplay);
            if (z.Rewards.FirstClear)
            {
                var bonus = new Reward(gems: GemsPerFirstClear);
                _c.Grant(bonus);
                _earned.Publish(new MetaRewardEarned("meta.src.first_clear", bonus));
            }
            if (id.Length == 0) return;
            var prize = _secondary.OnZoneValidated(id);
            if (prize.HasValue) _earned.Publish(new MetaRewardEarned("meta.src.objective", prize.Value));
        }

        void OnRoster()
        {
            if (!_armed) return;
            int sum = 0;
            foreach (var m in _c.Session.Roster) sum += m.Level;
            int count = _c.Session.Roster.Count;
            if (count > _count && _levelSum > 0) { _daily.Record(DailyKind.RecruitHero, count - _count); _quests.Record(QuestMetric.Recruits, count - _count); }
            else if (sum > _levelSum && count == _count) { _daily.Record(DailyKind.TrainMember, sum - _levelSum); _quests.Record(QuestMetric.Trainings, sum - _levelSum); }
            _count = count;
            _levelSum = sum;
        }

        void OnWallet(WalletChanged w)
        {
            if (!_armed || !ReferenceEquals(_metaSeen, _c.Meta)) { _credits = w.Credits; _metaSeen = _c.Meta; return; }
            int delta = w.Credits - _credits;
            _credits = w.Credits;
            if (delta > 0) Stats.Add(_c.Meta, Stats.CreditsEarned, delta);
            if (delta > 0 && !_c.Granting) _daily.Record(DailyKind.EarnCredits, delta);
        }

        public void Dispose() { _subs?.Dispose(); _subs = null; }
    }
}
