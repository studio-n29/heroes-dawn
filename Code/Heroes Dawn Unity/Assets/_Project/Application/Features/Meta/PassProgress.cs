#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Barème du pass saisonnier : 30 niveaux de 100 points, deux pistes (gratuite, premium).</summary>
    public static class PassTrack
    {
        public const int Levels = 30;
        public const int XpPerLevel = 100;
        public const int PremiumCost = 300;
        public const int SeasonDays = 30;

        public static Reward Free(int level)
        {
            if (level % 10 == 0) return new Reward(credits: 500, gems: 20);
            if (level % 5 == 0) return new Reward(gems: 10);
            if (level % 2 == 0) return new Reward(pr: 10 + level);
            return new Reward(credits: 150 + level * 10);
        }

        public static Reward Premium(int level)
        {
            if (level % 10 == 0) return new Reward(gems: 60);
            if (level % 3 == 0) return new Reward(gems: 5);
            return new Reward(credits: 300 + level * 20, pr: level % 2 == 0 ? 8 : 0);
        }
    }

    /// <summary>Un niveau du pass tel que l'interface l'affiche.</summary>
    public readonly struct PassLevelView
    {
        public readonly int Level;
        public readonly Reward Free, Premium;
        public readonly bool Reached, FreeClaimed, PremiumClaimed, PremiumOwned;

        public PassLevelView(int level, Reward free, Reward premium, bool reached, bool freeClaimed, bool premiumClaimed, bool premiumOwned)
        {
            Level = level; Free = free; Premium = premium; Reached = reached; FreeClaimed = freeClaimed; PremiumClaimed = premiumClaimed;
            PremiumOwned = premiumOwned;
        }

        public bool FreeClaimable => Reached && !FreeClaimed;
        public bool PremiumClaimable => Reached && PremiumOwned && !PremiumClaimed;
    }

    /// <summary>Pass saisonnier (local : la saison dure 30 jours à partir du premier lancement, la boutique « premium » est simulée).</summary>
    public sealed class PassProgress
    {
        readonly MetaContext _c;

        public PassProgress(MetaContext context) { _c = context; }

        public int Xp => _c.Meta.passXp;
        public bool PremiumOwned => _c.Meta.passPremium;
        public int Level => Math.Min(PassTrack.Levels, Xp / PassTrack.XpPerLevel);
        public bool IsMax => Level >= PassTrack.Levels;
        public int XpInLevel => IsMax ? PassTrack.XpPerLevel : Xp % PassTrack.XpPerLevel;

        /// <summary>Démarre la saison au premier appel ; une saison finie repart de zéro (nouvelle saison).</summary>
        public void EnsureSeason()
        {
            var m = _c.Meta;
            var start = ParseDay(m.seasonStart);
            if (start == null)
            {
                m.seasonStart = _c.Today;
                return;
            }
            if ((_c.LocalNow.Date - start.Value.Date).TotalDays >= PassTrack.SeasonDays)
            {
                m.seasonStart = _c.Today;
                m.passXp = 0;
                m.passPremium = false;
                m.passFreeClaimed.Clear();
                m.passPremiumClaimed.Clear();
                _c.Touch(MetaArea.Pass);
            }
        }

        public TimeSpan TimeLeft()
        {
            EnsureSeason();
            var start = ParseDay(_c.Meta.seasonStart) ?? _c.LocalNow.Date;
            var end = start.Date.AddDays(PassTrack.SeasonDays);
            var left = end - _c.LocalNow;
            return left < TimeSpan.Zero ? TimeSpan.Zero : left;
        }

        static DateTime? ParseDay(string s)
            => DateTime.TryParseExact(s, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d) ? d : (DateTime?)null;

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            EnsureSeason();
            _c.Meta.passXp = Math.Min(PassTrack.Levels * PassTrack.XpPerLevel, _c.Meta.passXp + amount);
            _c.Touch(MetaArea.Pass);
        }

        public IReadOnlyList<PassLevelView> Levels()
        {
            EnsureSeason();
            var list = new List<PassLevelView>(PassTrack.Levels);
            for (int n = 1; n <= PassTrack.Levels; n++)
                list.Add(new PassLevelView(n, PassTrack.Free(n), PassTrack.Premium(n), n <= Level,
                                           _c.Meta.passFreeClaimed.Contains(n), _c.Meta.passPremiumClaimed.Contains(n), PremiumOwned));
            return list;
        }

        public int ClaimableCount()
        {
            int count = 0;
            foreach (var l in Levels()) { if (l.FreeClaimable) count++; if (l.PremiumClaimable) count++; }
            return count;
        }

        public Result<Reward> Claim(int level, bool premium)
        {
            if (level < 1 || level > PassTrack.Levels) return Result<Reward>.Fail(MetaErrors.Unknown);
            if (level > Level) return Result<Reward>.Fail(MetaErrors.LevelLocked);
            if (premium && !PremiumOwned) return Result<Reward>.Fail(MetaErrors.PremiumRequired);
            var claimed = premium ? _c.Meta.passPremiumClaimed : _c.Meta.passFreeClaimed;
            if (claimed.Contains(level)) return Result<Reward>.Fail(MetaErrors.AlreadyClaimed);
            claimed.Add(level);
            var reward = premium ? PassTrack.Premium(level) : PassTrack.Free(level);
            _c.Grant(reward);
            _c.Touch(MetaArea.Pass);
            return Result<Reward>.Ok(reward);
        }

        /// <summary>Récupère toutes les récompenses disponibles d'un coup ; renvoie leur somme.</summary>
        public Result<Reward> ClaimAll()
        {
            var total = new Reward();
            bool any = false;
            foreach (var l in Levels())
            {
                if (l.FreeClaimable) { var r = Claim(l.Level, false); if (r.IsSuccess) { total += r.Value; any = true; } }
                if (l.PremiumClaimable) { var r = Claim(l.Level, true); if (r.IsSuccess) { total += r.Value; any = true; } }
            }
            return any ? Result<Reward>.Ok(total) : Result<Reward>.Fail(MetaErrors.NotReady);
        }

        public Result BuyPremium()
        {
            if (PremiumOwned) return Result.Fail(MetaErrors.AlreadyDone);
            if (!_c.Session.Wallet.TrySpendGems(PassTrack.PremiumCost)) return Result.Fail(MetaErrors.NotEnoughGems);
            _c.Meta.passPremium = true;
            _c.PublishWallet();
            _c.Touch(MetaArea.Pass);
            return Result.Ok();
        }
    }
}
