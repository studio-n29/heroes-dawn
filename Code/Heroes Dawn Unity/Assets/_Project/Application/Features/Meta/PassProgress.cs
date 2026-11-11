#nullable enable
using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Barème du pass saisonnier : 50 paliers de 1 300 points, 42 jours, deux pistes (gratuite, premium). Le premium est un achat en argent réel (produit <see cref="PremiumProduct"/>).</summary>
    public static class PassTrack
    {
        public const int Levels = 50;
        public const int XpPerLevel = 1300;
        public const int SeasonDays = 42;
        public const int MaxXp = Levels * XpPerLevel;
        /// <summary>Identifiant du produit à déclarer dans les stores (consommable : valable pour la saison en cours).</summary>
        public const string PremiumProduct = "pass_premium";

        /// <summary>Lundi 5 janvier 2026 : les saisons s'enchaînent de 42 jours à partir de ce jour (la saison 0 commence ici).</summary>
        public static readonly DateTime Epoch = new DateTime(2026, 1, 5);

        public static int LevelOf(int xp) => Math.Min(Levels, Math.Max(0, xp) / XpPerLevel);

        /// <summary>Numéro de la saison qui contient ce jour (calendrier local).</summary>
        public static int SeasonIndex(DateTime day)
        {
            int days = (int)Math.Floor((day.Date - Epoch).TotalDays);
            return days >= 0 ? days / SeasonDays : -((-days + SeasonDays - 1) / SeasonDays);
        }

        public static DateTime SeasonStart(int index) => Epoch.AddDays((long)index * SeasonDays);
        public static DateTime SeasonEnd(int index) => SeasonStart(index + 1);
    }

    /// <summary>Points de pass donnés par les nouvelles sources (spec §2) ; les valeurs sont à relire par le design.</summary>
    public static class PassXp
    {
        public const int Lair = 150, DuelWon = 30, DuelLost = 10, Craft = 20;
        public static int Duel(bool won) => won ? DuelWon : DuelLost;
    }

    /// <summary>Un niveau du pass tel que l'interface l'affiche.</summary>
    public readonly struct PassLevelView
    {
        public readonly int Level;
        public readonly Reward Free, Premium;
        public readonly bool Reached, FreeClaimed, PremiumClaimed, PremiumOwned;
        /// <summary>Le détail complet des lots (Composants, catalyseurs, accélérateurs, cosmétiques) en plus des Crédits, PR et Gemmes de <see cref="Free"/> et <see cref="Premium"/>.</summary>
        public readonly PassRewardDef FreeDef, PremiumDef;

        public PassLevelView(int level, Reward free, Reward premium, bool reached, bool freeClaimed, bool premiumClaimed, bool premiumOwned,
                             PassRewardDef? freeDef = null, PassRewardDef? premiumDef = null)
        {
            Level = level; Free = free; Premium = premium; Reached = reached; FreeClaimed = freeClaimed; PremiumClaimed = premiumClaimed;
            PremiumOwned = premiumOwned; FreeDef = freeDef ?? PassRewardDef.Empty; PremiumDef = premiumDef ?? PassRewardDef.Empty;
        }

        public bool FreeClaimable => Reached && !FreeClaimed;
        public bool PremiumClaimable => Reached && PremiumOwned && !PremiumClaimed;
    }

    /// <summary>
    /// Pass saisonnier : les saisons de 42 jours s'enchaînent depuis une date fixe (<see cref="PassTrack.Epoch"/>), les récompenses viennent de <c>pass_rewards.json</c>,
    /// la piste premium s'achète en argent réel pour la saison en cours.
    /// </summary>
    public sealed class PassProgress
    {
        readonly MetaContext _c;
        readonly PassRewardTable _rewards;
        readonly IPurchaseGateway _payments;

        public PassProgress(MetaContext context, PassRewardTable rewards, IPurchaseGateway payments)
        {
            _c = context; _rewards = rewards; _payments = payments;
        }

        public int Xp => _c.Meta.passXp;
        public bool PremiumOwned => _c.Meta.passPremium;
        public int Level => PassTrack.LevelOf(Xp);
        public bool IsMax => Level >= PassTrack.Levels;
        public int XpInLevel => IsMax ? PassTrack.XpPerLevel : Xp % PassTrack.XpPerLevel;

        /// <summary>Numéro de la saison en cours.</summary>
        public int SeasonIndex => PassTrack.SeasonIndex(_c.LocalNow);

        public PassRewardDef Free(int level) => _rewards.FreeAt(level);
        public PassRewardDef Premium(int level) => _rewards.PremiumAt(level);

        /// <summary>Une nouvelle saison (ou le premier lancement) remet les points, le premium et les paliers réclamés à zéro.</summary>
        public void EnsureSeason()
        {
            var m = _c.Meta;
            int index = SeasonIndex;
            if (m.passSeason == index) return;
            bool blank = m.passXp == 0 && !m.passPremium && m.passFreeClaimed.Count == 0 && m.passPremiumClaimed.Count == 0;
            m.passSeason = index;
            m.seasonStart = PassTrack.SeasonStart(index).ToString("yyyy-MM-dd");
            m.passXp = 0;
            m.passPremium = false;
            m.passFreeClaimed.Clear();
            m.passPremiumClaimed.Clear();
            if (!blank) _c.Touch(MetaArea.Pass);
        }

        public TimeSpan TimeLeft()
        {
            EnsureSeason();
            var left = PassTrack.SeasonEnd(SeasonIndex) - _c.LocalNow;
            return left < TimeSpan.Zero ? TimeSpan.Zero : left;
        }

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            EnsureSeason();
            _c.Meta.passXp = Math.Min(PassTrack.MaxXp, _c.Meta.passXp + amount);
            _c.Touch(MetaArea.Pass);
        }

        public IReadOnlyList<PassLevelView> Levels()
        {
            EnsureSeason();
            var list = new List<PassLevelView>(PassTrack.Levels);
            for (int n = 1; n <= PassTrack.Levels; n++)
            {
                var free = Free(n);
                var premium = Premium(n);
                list.Add(new PassLevelView(n, ToReward(free), ToReward(premium), n <= Level,
                                           _c.Meta.passFreeClaimed.Contains(n), _c.Meta.passPremiumClaimed.Contains(n), PremiumOwned, free, premium));
            }
            return list;
        }

        public int ClaimableCount()
        {
            int count = 0;
            foreach (var l in Levels()) { if (l.FreeClaimable) count++; if (l.PremiumClaimable) count++; }
            return count;
        }

        static Reward ToReward(PassRewardDef d) => new Reward(credits: d.Credits, pr: d.Pr, gems: d.Gems);

        public Result<Reward> Claim(int level, bool premium)
        {
            EnsureSeason();
            if (level < 1 || level > PassTrack.Levels) return Result<Reward>.Fail(MetaErrors.Unknown);
            if (level > Level) return Result<Reward>.Fail(MetaErrors.LevelLocked);
            if (premium && !PremiumOwned) return Result<Reward>.Fail(MetaErrors.PremiumRequired);
            var claimed = premium ? _c.Meta.passPremiumClaimed : _c.Meta.passFreeClaimed;
            if (claimed.Contains(level)) return Result<Reward>.Fail(MetaErrors.AlreadyClaimed);
            claimed.Add(level);
            var def = premium ? Premium(level) : Free(level);
            var reward = ToReward(def);
            _c.Grant(reward);
            foreach (var g in def.Components) CraftingOps.AddComponents(_c.Session, g.Kind, g.Grade, g.Count);
            foreach (var g in def.Catalysts) CraftingOps.AddCatalyst(_c.Session, g.Kind, g.Count);
            _c.Meta.speedUpTokens += def.SpeedUpMinutes;
            foreach (var id in def.Cosmetics) CosmeticOps.Unlock(_c.Meta, id);
            _c.Touch(MetaArea.Pass);
            return Result<Reward>.Ok(reward);
        }

        /// <summary>Récupère toutes les récompenses disponibles d'un coup ; renvoie leur somme (Crédits, PR et Gemmes).</summary>
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

        /// <summary>
        /// Achat du pass premium de la saison (argent réel) : refusé sans appeler le store s'il est déjà acquis. Si la saison change pendant le paiement, le joueur reçoit le premium
        /// de la saison qui commence (il a payé : on ne le prive pas).
        /// </summary>
        public async UniTask<Result> BuyPremiumAsync(CancellationToken ct)
        {
            EnsureSeason();
            if (PremiumOwned) return Result.Fail(MetaErrors.AlreadyDone);
            var paid = await _payments.PurchaseAsync(PassTrack.PremiumProduct, ct);
            ct.ThrowIfCancellationRequested();
            if (paid.IsFailure) return Result.Fail(paid.Error!);
            var ledger = _payments as IPendingPurchaseSource;
            GrantPremium(ledger?.TransactionOf(PassTrack.PremiumProduct));
            ledger?.Confirm(PassTrack.PremiumProduct);
            return Result.Ok();
        }

        /// <summary>Active le premium de la saison en cours (achat confirmé ou transaction reprise) ; faux s'il l'était déjà.</summary>
        public bool GrantPremium(string? transactionId = null)
        {
            if (!PurchaseLedger.TryRecord(_c.Meta, transactionId)) return false;
            EnsureSeason();
            if (PremiumOwned) return false;
            _c.Meta.passPremium = true;
            _c.Touch(MetaArea.Pass);
            return true;
        }
    }
}
