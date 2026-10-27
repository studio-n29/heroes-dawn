#nullable enable
using System;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    public enum MetaArea { Pass, Daily, Objectives, Operations, Lab, Archives, Boutique, Settings, Tutorial, Collection, Onboarding, Quests, Profile }

    /// <summary>Un système de méta-progression a changé (pages et badges se rafraîchissent).</summary>
    public readonly struct MetaChanged
    {
        public readonly MetaArea Area;
        public MetaChanged(MetaArea area) { Area = area; }
    }

    /// <summary>Récompense : Crédits, PR, Gemmes et points d'expérience du pass.</summary>
    public readonly struct Reward
    {
        public readonly int Credits, Pr, Gems, PassXp;
        public Reward(int credits = 0, int pr = 0, int gems = 0, int passXp = 0) { Credits = credits; Pr = pr; Gems = gems; PassXp = passXp; }
        public bool IsEmpty => Credits == 0 && Pr == 0 && Gems == 0 && PassXp == 0;
        public Reward Scaled(int percent) => new Reward(Credits * percent / 100, Pr * percent / 100, Gems, PassXp);
        public static Reward operator +(Reward a, Reward b) => new Reward(a.Credits + b.Credits, a.Pr + b.Pr, a.Gems + b.Gems, a.PassXp + b.PassXp);
    }

    public static class MetaErrors
    {
        public static readonly DomainError NotEnoughGems = new DomainError("meta.gems", "errors.not_enough_gems");
        public static readonly DomainError NotEnoughCredits = new DomainError("meta.credits", "errors.not_enough_credits");
        public static readonly DomainError Unknown = new DomainError("meta.unknown", "errors.meta_unknown");
        public static readonly DomainError NotReady = new DomainError("meta.not_ready", "errors.meta_not_ready");
        public static readonly DomainError AlreadyClaimed = new DomainError("meta.claimed", "errors.meta_claimed");
        public static readonly DomainError PremiumRequired = new DomainError("meta.premium", "errors.meta_premium");
        public static readonly DomainError LevelLocked = new DomainError("meta.level_locked", "errors.meta_level_locked");
        public static readonly DomainError Busy = new DomainError("meta.busy", "errors.meta_busy");
        public static readonly DomainError NoMembers = new DomainError("meta.no_members", "errors.meta_no_members");
        public static readonly DomainError MaxLevel = new DomainError("meta.max_level", "errors.max_level");
        public static readonly DomainError PaymentFailed = new DomainError("meta.payment", "errors.meta_payment");
        public static readonly DomainError AlreadyDone = new DomainError("meta.done", "errors.meta_done");
        public static readonly DomainError NotEnoughScraps = new DomainError("meta.scraps", "errors.not_enough_scraps");
    }

    /// <summary>Services communs aux systèmes de méta-progression : session, horloge, sauvegarde, diffusion.</summary>
    public sealed class MetaContext
    {
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<MetaChanged> _changed;
        readonly SaveScheduler _save;
        readonly IClock _clock;

        public GameSession Session { get; }
        public MetaSave Meta => Session.Meta;

        /// <summary>Vrai pendant qu'une récompense est versée : le suivi des « Crédits gagnés » ne la compte pas.</summary>
        public bool Granting { get; private set; }

        public MetaContext(GameSession session, SaveScheduler save, IClock clock, IPublisher<WalletChanged> wallet, IPublisher<MetaChanged> changed)
        {
            Session = session; _save = save; _clock = clock; _wallet = wallet; _changed = changed;
        }

        public DateTime UtcNow => _clock.UtcNow;
        public DateTime LocalNow => _clock.LocalNow;
        public string Today => _clock.LocalNow.ToString("yyyy-MM-dd");

        /// <summary>Verse Crédits, PR et Gemmes (les points du pass sont ajoutés par <see cref="PassProgress"/>).</summary>
        public void Grant(Reward r)
        {
            Granting = true;
            try
            {
                Session.Wallet.Add(r.Credits, r.Pr);
                Session.Wallet.AddGems(r.Gems);
                _wallet.PublishWallet(Session);
            }
            finally { Granting = false; }
        }

        /// <summary>Annonce un changement et demande une sauvegarde.</summary>
        public void Touch(MetaArea area)
        {
            _changed.Publish(new MetaChanged(area));
            _save.RequestSave();
        }

        public void PublishWallet()
        {
            _wallet.PublishWallet(Session);
        }
    }
}
