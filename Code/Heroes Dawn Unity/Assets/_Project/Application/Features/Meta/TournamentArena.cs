#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Meta
{
    public static class TournamentErrors
    {
        public static readonly DomainError NotActive = new DomainError("tournament.inactive", "errors.tournament_inactive");
        public static readonly DomainError NeedsAccount = new DomainError("tournament.account", "errors.tournament_account");
        public static readonly DomainError DailyCap = new DomainError("tournament.cap", "errors.tournament_cap");
        public static readonly DomainError Network = new DomainError("tournament.network", "errors.tournament_network");
        public static readonly DomainError ClaimExpired = new DomainError("tournament.claim_expired", "errors.tournament_claim_expired");
        public static readonly DomainError NotFinished = new DomainError("tournament.not_finished", "errors.tournament_not_finished");
        public static readonly DomainError NotEarned = new DomainError("tournament.not_earned", "errors.tournament_not_earned");
    }

    /// <summary>Ce que la page Tournoi affiche : le tournoi actif (ou le prochain), les dates de l'édition, les points, le palier et les duels restants.</summary>
    public sealed class TournamentView
    {
        public TournamentDef Def = new TournamentDef();
        /// <summary>Faux entre deux tournois : <see cref="Def"/> est alors le prochain.</summary>
        public bool IsActive;
        public DateTime Start, End;
        /// <summary>« christmas:2026 » : identifiant de l'édition, celui envoyé au serveur.</summary>
        public string EditionKey = "";
        public int Points;
        public int Tier;
        public int DuelsLeft;
        public bool SignedIn;
        /// <summary>Paliers de points atteints, pas encore réclamés et dans le délai.</summary>
        public IReadOnlyList<int> ClaimableTiers = Array.Empty<int>();
        /// <summary>Une édition passée dont les récompenses sont encore réclamables (clé), sinon vide.</summary>
        public string PreviousEditionKey = "";
    }

    /// <summary>Un duel engagé : le combat à jouer et, en ligne, le lien avec l'adversaire.</summary>
    public sealed class DuelSession
    {
        public string MatchId = "";
        public string EditionKey = "";
        public DuelMode Mode;
        public CombatSetup Setup = null!;
        /// <summary>Vrai pour l'hôte (identifiant de joueur le plus petit) : il occupe le camp <see cref="Side.Ally"/> ; l'invité est <see cref="Side.Enemy"/>.</summary>
        public bool IAmHost = true;
        public DefenseSnapshot Opponent = new DefenseSnapshot();
        public bool OpponentGenerated;
        /// <summary>Lien temps réel (duel en ligne seulement).</summary>
        public OnlineDuelLink? Link;
        public bool Reported;

        /// <summary>Le camp du joueur sur cet appareil.</summary>
        public Side MySide => IAmHost ? Side.Ally : Side.Enemy;
    }

    public sealed class DuelReport
    {
        public DuelMode Mode;
        public DuelOutcome Outcome;
        public int PointsDelta;
        public int TotalPoints;
        public bool Counted;
        public int TierBefore, TierAfter;
    }

    public enum DuelLinkKind { Command, PeerLeft, PeerSilent, Desync }

    public sealed class DuelLinkEvent
    {
        public DuelLinkKind Kind;
        public int Tick;
        public CombatCommand? Command;
    }

    /// <summary>
    /// Échanges d'un duel en ligne : on n'envoie que les commandes ; tous les 20 ticks chaque appareil envoie l'empreinte de son état et compare avec celle de l'autre
    /// (différence : <see cref="Desynced"/>, le duel est annulé sans points). Un adversaire muet <see cref="ArenaRules.SilenceSeconds"/> secondes est considéré comme parti.
    /// </summary>
    public sealed class OnlineDuelLink
    {
        readonly IDuelChannel _channel;
        readonly Dictionary<int, ulong> _local = new Dictionary<int, ulong>();
        readonly Dictionary<int, ulong> _remote = new Dictionary<int, ulong>();

        public bool Desynced { get; private set; }
        public int DesyncTick { get; private set; } = -1;

        public OnlineDuelLink(IDuelChannel channel) { _channel = channel; }

        public UniTask SendCommandAsync(int tick, CombatCommand command, CancellationToken ct) => _channel.SendAsync(DuelMessage.OfCommand(tick, command), ct);

        /// <summary>Envoie l'empreinte locale d'un tick de contrôle ; compare tout de suite si celle de l'autre est déjà là.</summary>
        public async UniTask SendHashAsync(int tick, ulong hash, CancellationToken ct)
        {
            _local[tick] = hash;
            await _channel.SendAsync(DuelMessage.OfHash(tick, hash), ct);
            if (_remote.TryGetValue(tick, out var theirs)) Compare(tick, hash, theirs);
        }

        void Compare(int tick, ulong mine, ulong theirs)
        {
            if (mine == theirs || Desynced) return;
            Desynced = true; DesyncTick = tick;
        }

        /// <summary>Attend le prochain événement de l'adversaire (commande, départ, silence ou désynchronisation).</summary>
        public async UniTask<DuelLinkEvent> NextAsync(CancellationToken ct)
        {
            while (true)
            {
                if (Desynced) return new DuelLinkEvent { Kind = DuelLinkKind.Desync, Tick = DesyncTick };
                var m = await _channel.ReceiveAsync(TimeSpan.FromSeconds(ArenaRules.SilenceSeconds), ct);
                if (m == null) return new DuelLinkEvent { Kind = DuelLinkKind.PeerSilent };
                switch (m.Type)
                {
                    case DuelMessageType.Leave: return new DuelLinkEvent { Kind = DuelLinkKind.PeerLeft };
                    case DuelMessageType.Command:
                        if (m.Command == null) continue;
                        return new DuelLinkEvent { Kind = DuelLinkKind.Command, Tick = m.Tick, Command = m.Command };
                    case DuelMessageType.Hash:
                        _remote[m.Tick] = m.Hash;
                        if (_local.TryGetValue(m.Tick, out var mine)) Compare(m.Tick, mine, m.Hash);
                        continue;
                }
            }
        }

        public UniTask LeaveAsync(CancellationToken ct) => _channel.LeaveAsync(ct);
        public UniTask SendLeaveAsync(CancellationToken ct) => _channel.SendAsync(DuelMessage.Leave(), ct);
    }

    /// <summary>
    /// Tournois JcJ (bloc D de la 1.0.0) : duels hors ligne (capture d'un autre joueur, IA) et en ligne (file d'attente temps réel, repli hors ligne après 15 s),
    /// points et plafond quotidien, classement et récompenses par paliers. Remplace le tournoi simulé de la 0.15.0.
    /// </summary>
    public sealed class TournamentArena
    {
        readonly MetaContext _c;
        readonly IArenaGateway _gateway;
        readonly IDuelChannel _channel;
        readonly IReadOnlyList<TournamentDef> _calendar;
        readonly ArenaRewardTable _rewards;
        bool _uploadedThisSession;

        public TournamentArena(MetaContext context, IArenaGateway gateway, IDuelChannel channel, IReadOnlyList<TournamentDef> calendar, ArenaRewardTable rewards)
        {
            _c = context; _gateway = gateway; _channel = channel; _calendar = calendar; _rewards = rewards;
        }

        ArenaSave Save => _c.Meta.arena;
        GameSession Session => _c.Session;

        /// <summary>Un compte connecté et confirmé : le tournoi en demande un.</summary>
        public bool SignedIn => Session.Account != null && !Session.Account.NeedsConfirmation && Session.Account.PlayerId.Length > 0;

        static string KeyOf(TournamentDef def, DateTime start) => def.Id + ":" + start.Year;

        // ---- Calendrier et état
        public TournamentView Current()
        {
            var today = _c.LocalNow.Date;
            var active = TournamentCalendar.ActiveOn(_calendar, today);
            var def = active ?? (_calendar.Count > 0 ? TournamentCalendar.Next(_calendar, today) : new TournamentDef());
            var (start, end) = _calendar.Count > 0 ? TournamentCalendar.Window(def, today) : (today, today);
            var view = new TournamentView { Def = def, IsActive = active != null, Start = start, End = end, EditionKey = KeyOf(def, start), SignedIn = SignedIn };
            if (view.IsActive) Rotate(view.EditionKey);
            bool mine = Save.tournamentId == view.EditionKey;
            view.Points = mine ? Save.points : 0;
            view.Tier = ArenaRules.TierReached(view.Points);
            view.DuelsLeft = view.IsActive ? ArenaRules.DailyDuelCap - DuelsToday(view.EditionKey) : 0;
            if (view.IsActive) view.ClaimableTiers = ClaimableTierList(view.EditionKey, view.Points, Save.claimedTiers);
            if (!view.IsActive || Save.previousId.Length > 0) view.PreviousEditionKey = ReadyToClaim(Save.previousId) ? Save.previousId : "";
            return view;
        }

        /// <summary>Compteur du jour pour l'édition (remis à zéro au changement de jour).</summary>
        int DuelsToday(string editionKey)
            => Save.tournamentId == editionKey && Save.duelDay == _c.Today ? Save.duelsToday : 0;

        /// <summary>Quand une nouvelle édition devient active, l'ancienne passe en « précédente » (réclamable 7 jours après sa fin) et les compteurs repartent à zéro.</summary>
        void Rotate(string activeKey)
        {
            if (Save.tournamentId == activeKey) return;
            if (Save.tournamentId.Length > 0 && Save.points > 0)
            {
                Save.previousId = Save.tournamentId; Save.previousPoints = Save.points;
                Save.previousClaimedTiers = new List<int>(Save.claimedTiers);
            }
            Save.tournamentId = activeKey;
            Save.points = 0; Save.duelsToday = 0; Save.duelDay = ""; Save.claimedTiers = new List<int>();
            _c.Touch(MetaArea.Profile);
        }

        TournamentDef? DefOf(string editionKey, out DateTime start, out DateTime end)
        {
            start = end = default;
            int cut = editionKey.LastIndexOf(':');
            if (cut <= 0 || !int.TryParse(editionKey.Substring(cut + 1), out int year)) return null;
            var def = _calendar.FirstOrDefault(t => t.Id == editionKey.Substring(0, cut));
            if (def == null) return null;
            var (s, e) = TournamentCalendar.Window(def, new DateTime(year, def.Start.Month, Math.Min(def.Start.Day, DateTime.DaysInMonth(year, def.Start.Month))));
            start = s; end = e;
            return def;
        }

        bool ReadyToClaim(string editionKey)
        {
            if (editionKey.Length == 0 || DefOf(editionKey, out _, out var end) == null) return false;
            return _c.LocalNow.Date <= end.AddDays(ArenaRules.ClaimDays);
        }

        IReadOnlyList<int> ClaimableTierList(string editionKey, int points, List<int> claimed)
        {
            var list = new List<int>();
            if (!ReadyToClaim(editionKey)) return list;
            for (int t = 1; t <= ArenaRules.TierReached(points); t++) if (!claimed.Contains(t)) list.Add(t);
            return list;
        }

        // ---- Capture de défense
        public async UniTask UploadDefenseIfNeededAsync(CancellationToken ct)
        {
            if (!SignedIn || Session.Squad.Count == 0) return;
            var snap = ArenaSnapshots.Capture(Session, _c.UtcNow);
            string hash = HashOf(snap);
            if (_uploadedThisSession && hash == Save.lastDefenseHash) return;
            var sent = await _gateway.UploadDefenseAsync(snap, ct);
            if (sent.IsFailure) return;
            _uploadedThisSession = true;
            Save.lastDefenseHash = hash;
            Save.lastDefenseUploadUtcTicks = _c.UtcNow.Ticks;
            _c.Touch(MetaArea.Profile);
        }

        static string HashOf(DefenseSnapshot s)
        {
            var h = new Fnv1a64();
            foreach (var t in s.Team)
            {
                h.Add(t.member.Length); h.Add(t.member); h.Add(t.level);
                foreach (var i in t.items) { h.Add(i.Length); h.Add(i); }
                for (int k = 0; k < t.cards.Count; k++) { h.Add(t.cards[k].Length); h.Add(t.cards[k]); h.Add(k < t.cardLevels.Count ? t.cardLevels[k] : 1); }
            }
            foreach (var l in s.Links) { h.Add(l.a); h.Add(l.b); h.Add(l.level); }
            return h.Value.ToString("x16");
        }

        // ---- Un duel
        public async UniTask<Result<DuelSession>> StartDuelAsync(DuelMode preferred, CancellationToken ct)
        {
            var view = Current();
            if (!view.IsActive) return Result<DuelSession>.Fail(TournamentErrors.NotActive);
            if (!SignedIn) return Result<DuelSession>.Fail(TournamentErrors.NeedsAccount);
            if (Session.Squad.Count == 0) return Result<DuelSession>.Fail(MetaErrors.NoMembers);
            if (!ArenaRules.CanStartDuel(DuelsToday(view.EditionKey))) return Result<DuelSession>.Fail(TournamentErrors.DailyCap);

            await UploadDefenseIfNeededAsync(ct);
            var me = ArenaSnapshots.Capture(Session, _c.UtcNow);
            if (me.Team.Count == 0) return Result<DuelSession>.Fail(MetaErrors.NoMembers);

            if (preferred == DuelMode.Online)
            {
                DuelTicket? found;
                try { found = await _channel.JoinQueueAsync(view.EditionKey, me, TimeSpan.FromSeconds(ArenaRules.QueueSeconds), ct); }
                catch (OperationCanceledException) { throw; }
                catch (Exception) { return Result<DuelSession>.Fail(TournamentErrors.Network); }   // connexion perdue en file : retour au tournoi, sans pénalité
                if (found != null) return Result<DuelSession>.Ok(BuildOnline(view, me, found));
                try { await _channel.LeaveAsync(ct); } catch (Exception) { /* la file se referme seule */ }
            }

            var offline = await _gateway.FindOfflineOpponentAsync(view.EditionKey, me.Rating, ct);
            if (offline.IsFailure) return Result<DuelSession>.Fail(TournamentErrors.Network);
            return Result<DuelSession>.Ok(BuildOffline(view, me, offline.Value));
        }

        DuelSession BuildOffline(TournamentView view, DefenseSnapshot me, DuelTicket ticket)
        {
            var mine = ArenaSnapshots.ToTeam(me, Session.Content);
            var theirs = ArenaSnapshots.ToTeam(ticket.Opponent, Session.Content);
            if (theirs.Count == 0)
            {
                ticket.Opponent = ArenaSnapshots.Fallback(Session.Content, me.Rating, ticket.Seed);
                ticket.Generated = true;
                theirs = ArenaSnapshots.ToTeam(ticket.Opponent, Session.Content);
            }
            var setup = new CombatSetup(ticket.Seed, CombatRank.Elite, mine, new EnemyEntry[0], Session.AutoCombat)
                .WithLinks(ArenaSnapshots.ToLinks(me, mine))
                .WithOpponentTeam(theirs, SideControl.Ai, ArenaSnapshots.ToLinks(ticket.Opponent, theirs));
            return new DuelSession { MatchId = ticket.MatchId, EditionKey = view.EditionKey, Mode = DuelMode.Offline, Setup = setup, IAmHost = true, Opponent = ticket.Opponent, OpponentGenerated = ticket.Generated };
        }

        DuelSession BuildOnline(TournamentView view, DefenseSnapshot me, DuelTicket ticket)
        {
            // L'hôte est l'identifiant de joueur le plus petit : il occupe le camp Ally des deux côtés, l'invité le camp Enemy.
            bool host = string.CompareOrdinal(me.PlayerId, ticket.Opponent.PlayerId) < 0;
            var hostSnap = host ? me : ticket.Opponent;
            var guestSnap = host ? ticket.Opponent : me;
            var hostTeam = ArenaSnapshots.ToTeam(hostSnap, Session.Content);
            var guestTeam = ArenaSnapshots.ToTeam(guestSnap, Session.Content);
            var setup = new CombatSetup(ticket.Seed, CombatRank.Elite, hostTeam, new EnemyEntry[0], false)
                .WithLinks(ArenaSnapshots.ToLinks(hostSnap, hostTeam))
                .WithOpponentTeam(guestTeam, SideControl.External, ArenaSnapshots.ToLinks(guestSnap, guestTeam));
            return new DuelSession
            {
                MatchId = ticket.MatchId, EditionKey = view.EditionKey, Mode = DuelMode.Online, Setup = setup, IAmHost = host, Opponent = ticket.Opponent,
                Link = new OnlineDuelLink(_channel),
            };
        }

        /// <summary>
        /// Clôt un duel : verse l'issue au serveur (qui applique points et plafond), met à jour le score local et le compteur du jour.
        /// Un échec réseau laisse le duel non reporté : on peut réessayer avec la même session.
        /// </summary>
        public async UniTask<Result<DuelReport>> FinishDuelAsync(DuelSession s, DuelOutcome outcome, CancellationToken ct)
        {
            if (s.Reported) return Result<DuelReport>.Fail(MetaErrors.AlreadyDone);
            if (s.Link != null)
            {
                try
                {
                    if (outcome == DuelOutcome.Quit) await s.Link.SendLeaveAsync(ct);
                    await s.Link.LeaveAsync(ct);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception) { /* le canal est déjà fermé : l'issue compte quand même */ }
            }
            Rotate(s.EditionKey);
            int before = Save.tournamentId == s.EditionKey ? Save.points : 0;
            bool counts = ArenaRules.CountsAsDuel(s.Mode, outcome);
            int delta = ArenaRules.PointsDelta(s.Mode, outcome, before);
            var report = new DuelReport { Mode = s.Mode, Outcome = outcome, TotalPoints = before, Counted = counts, TierBefore = ArenaRules.TierReached(before), TierAfter = ArenaRules.TierReached(before) };
            if (counts || delta != 0)
            {
                var sent = await _gateway.SubmitDuelAsync(s.EditionKey, s.MatchId, s.Mode, outcome, ct);
                if (sent.IsFailure) return Result<DuelReport>.Fail(sent.Error!);
                int total = Math.Max(0, sent.Value);
                if (Save.tournamentId == s.EditionKey)
                {
                    Save.points = total;
                    if (counts)
                    {
                        if (Save.duelDay != _c.Today) { Save.duelDay = _c.Today; Save.duelsToday = 0; }
                        Save.duelsToday++;
                    }
                }
                report.TotalPoints = total; report.PointsDelta = total - before; report.TierAfter = ArenaRules.TierReached(total);
                _c.Touch(MetaArea.Profile);
            }
            s.Reported = true;
            return Result<DuelReport>.Ok(report);
        }

        // ---- Classement
        public UniTask<Result<IReadOnlyList<ScoreRow>>> LeaderboardAsync(CancellationToken ct, int top = 100)
            => _gateway.LeaderboardAsync(Current().EditionKey, top, ct);

        public UniTask<Result<Standing>> StandingAsync(CancellationToken ct) => _gateway.StandingAsync(Current().EditionKey, ct);

        // ---- Récompenses
        /// <summary>
        /// Réclame un palier : 1 à 5 (points), 6 (top 10 %) ou 7 (top 3, après la fin). Refusé deux fois, au-delà de 7 jours après la fin, ou si le palier n'est pas atteint.
        /// <c>editionKey</c> vide : l'édition en cours, sinon la précédente si elle est encore dans le délai.
        /// </summary>
        public async UniTask<Result> ClaimTierAsync(int tier, CancellationToken ct, string editionKey = "")
        {
            var view = Current();
            if (!SignedIn) return Result.Fail(TournamentErrors.NeedsAccount);
            string key = editionKey.Length > 0 ? editionKey : view.IsActive ? view.EditionKey : Save.tournamentId;
            bool isCurrent = key == Save.tournamentId;
            bool isPrevious = key == Save.previousId && key.Length > 0;
            if (!isCurrent && !isPrevious) return Result.Fail(TournamentErrors.ClaimExpired);
            var def = DefOf(key, out _, out var end);
            if (def == null) return Result.Fail(MetaErrors.Unknown);
            var reward = _rewards.ForTier(tier);
            if (reward == null) return Result.Fail(MetaErrors.Unknown);
            var claimed = isCurrent ? Save.claimedTiers : Save.previousClaimedTiers;
            if (claimed.Contains(tier)) return Result.Fail(MetaErrors.AlreadyClaimed);
            if (_c.LocalNow.Date > end.AddDays(ArenaRules.ClaimDays)) return Result.Fail(TournamentErrors.ClaimExpired);
            int points = isCurrent ? Save.points : Save.previousPoints;

            if (tier <= ArenaRules.TierPoints.Count)
            {
                if (points < ArenaRules.TierPoints[tier - 1]) return Result.Fail(TournamentErrors.NotEarned);
            }
            else
            {
                if (_c.LocalNow.Date <= end) return Result.Fail(TournamentErrors.NotFinished);
                var standing = await _gateway.StandingAsync(key, ct);
                if (standing.IsFailure) return Result.Fail(TournamentErrors.Network);
                var st = standing.Value;
                bool earned = st.Rank > 0 && (tier == ArenaRewardTable.TierTop3 ? st.Rank <= 3 : st.Rank <= Math.Max(1, (int)Math.Ceiling(st.Total * 0.10)));
                if (!earned) return Result.Fail(TournamentErrors.NotEarned);
            }

            var sent = await _gateway.ClaimAsync(key, tier, end, ct);
            if (sent.IsFailure) return sent;
            claimed.Add(tier);
            Grant(reward, def.RewardsId);
            _c.Touch(MetaArea.Profile);
            return Result.Ok();
        }

        void Grant(ArenaRewardDef r, string rewardsId)
        {
            _c.Grant(new Reward(credits: r.Credits));
            foreach (var g in r.Components) CraftingOps.AddComponents(Session, g.Kind, g.Grade, g.Count);
            foreach (var g in r.Catalysts) CraftingOps.AddCatalyst(Session, g.Kind, g.Count);
            if (r.RecipeId != null && Recipes.Find(r.RecipeId) != null) CraftingOps.Learn(Session, r.RecipeId);
            foreach (var c in r.Cosmetics)
            {
                string id = c.Replace("{r}", rewardsId);
                if (!Save.cosmetics.Contains(id)) Save.cosmetics.Add(id);
            }
            _c.PublishWallet();
        }
    }
}
