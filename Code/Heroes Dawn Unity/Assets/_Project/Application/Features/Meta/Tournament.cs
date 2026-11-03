#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Un adversaire du tournoi du jour : nom et puissance, tirés de la graine du joueur et du jour.</summary>
    public readonly struct TournamentOpponent
    {
        public readonly string Name;
        public readonly int Power;
        public TournamentOpponent(string name, int power) { Name = name; Power = power; }
    }

    /// <summary>Un match joué : manche, adversaire, puissances en présence et issue.</summary>
    public readonly struct TournamentMatch
    {
        public readonly int Round;
        public readonly TournamentOpponent Opponent;
        public readonly int PlayerPower;
        public readonly bool Won;
        public TournamentMatch(int round, TournamentOpponent opponent, int playerPower, bool won) { Round = round; Opponent = opponent; PlayerPower = playerPower; Won = won; }
    }

    /// <summary>Une ligne du classement : rang, nom, points et si c'est le joueur.</summary>
    public readonly struct LeaderboardEntry
    {
        public readonly int Rank;
        public readonly string Name;
        public readonly int Rating;
        public readonly bool IsPlayer;
        public LeaderboardEntry(int rank, string name, int rating, bool isPlayer) { Rank = rank; Name = name; Rating = rating; IsPlayer = isPlayer; }
    }

    /// <summary>
    /// Tournoi JcJ simulé côté client (0.15.0) : un tournoi par jour, trois manches (quarts, demi, finale) contre des adversaires générés localement
    /// à partir du joueur et du jour, issue tirée sur les puissances des équipes, classement fait de joueurs factices. Aucun échange réseau :
    /// le matchmaking, le classement réel et les saisons arrivent avec le serveur en 0.16.0.
    /// </summary>
    public sealed class Tournament
    {
        public const int Rounds = 3;
        public const int StartRating = 1000;
        public const int WinPoints = 25, LossPoints = 15, MinRating = 100;
        public const int BoardSize = 10;

        static readonly string[] Names =
        {
            "Vortex", "Kestrel", "Mirage", "Nocturne", "Basalte", "Cyclone", "Ombre", "Tempo", "Sirocco", "Quartz", "Lynx", "Orage",
            "Pivot", "Zéphyr", "Corail", "Atlas", "Braise", "Cobalt", "Dune", "Éclat", "Fjord", "Givre", "Halo", "Iris",
        };

        readonly MetaContext _c;

        public Tournament(MetaContext context) { _c = context; }

        TournamentSave Save => _c.Meta.tournament;

        string PlayerKey => _c.Session.Account?.PlayerId ?? "local";

        /// <summary>Repart d'un tournoi neuf quand le jour a changé (les points se gardent).</summary>
        void Ensure()
        {
            var t = Save;
            if (t.day == _c.Today) return;
            t.day = _c.Today; t.round = 0; t.eliminated = false; t.claimed = false;
        }

        ulong Seed(string scope, string day)
        {
            var h = new Fnv1a64();
            h.Add(PlayerKey.Length); h.Add(PlayerKey); h.Add(scope.Length); h.Add(scope); h.Add(day.Length); h.Add(day);
            return h.Value;
        }

        /// <summary>Puissance de l'escouade : 10 par niveau, moins la moitié de la fatigue, au moins 1 par agent.</summary>
        public int PlayerPower()
            => _c.Session.Squad.Sum(m => Math.Max(1, m.Level * 10 - m.Fatigue / 2));

        public int Rating => Save.rating;
        public int Round { get { Ensure(); return Save.round; } }
        public bool Eliminated { get { Ensure(); return Save.eliminated; } }
        public bool Finished { get { Ensure(); return Save.eliminated || Save.round >= Rounds; } }
        public bool Claimed { get { Ensure(); return Save.claimed; } }
        public bool Champion { get { Ensure(); return Save.round >= Rounds; } }

        /// <summary>Les trois adversaires du jour : plus forts à chaque manche (de 80 à 115 % de la puissance du joueur environ).</summary>
        public IReadOnlyList<TournamentOpponent> Opponents()
        {
            Ensure();
            var rng = new Pcg32(Seed("opponents", Save.day));
            int basePower = Math.Max(10, PlayerPower());
            var list = new List<TournamentOpponent>();
            var used = new HashSet<string>();
            for (int i = 0; i < Rounds; i++)
            {
                string name;
                do name = Names[rng.Range(0, Names.Length)]; while (!used.Add(name));
                int pct = 80 + 10 * i + rng.Range(0, 16);
                list.Add(new TournamentOpponent(name, Math.Max(1, basePower * pct / 100)));
            }
            return list;
        }

        /// <summary>Chances de gagner (0,1 à 0,9) selon l'écart de puissance.</summary>
        public static float WinChance(int player, int opponent)
        {
            int top = Math.Max(1, Math.Max(player, opponent));
            return Math.Min(0.9f, Math.Max(0.1f, 0.5f + (player - opponent) / (2f * top)));
        }

        public bool CanPlay => !Finished && _c.Session.Squad.Count > 0;

        /// <summary>Joue la manche suivante : l'issue est tirée de la graine du jour et de la manche (le même jour donne les mêmes tirages).</summary>
        public Result<TournamentMatch> PlayNext()
        {
            Ensure();
            if (Finished) return Result<TournamentMatch>.Fail(MetaErrors.AlreadyDone);
            if (_c.Session.Squad.Count == 0) return Result<TournamentMatch>.Fail(MetaErrors.NoMembers);
            int round = Save.round;
            var opponent = Opponents()[round];
            int power = PlayerPower();
            var rng = new Pcg32(Seed("match" + round, Save.day));
            bool won = rng.Range(0, 1000) < (int)(WinChance(power, opponent.Power) * 1000f);
            if (won) { Save.round++; Save.rating += WinPoints; }
            else { Save.eliminated = true; Save.rating = Math.Max(MinRating, Save.rating - LossPoints); }
            _c.Touch(MetaArea.Profile);
            return Result<TournamentMatch>.Ok(new TournamentMatch(round, opponent, power, won));
        }

        /// <summary>Récompense d'un tournoi fini selon le nombre de manches gagnées (0 à 3).</summary>
        public static Reward RewardFor(int wins) => wins switch
        {
            0 => new Reward(credits: 100),
            1 => new Reward(credits: 250, pr: 5),
            2 => new Reward(credits: 500, pr: 10, passXp: 20),
            _ => new Reward(credits: 1000, pr: 20, gems: 10, passXp: 40),
        };

        public Result<Reward> Claim()
        {
            Ensure();
            if (!Finished) return Result<Reward>.Fail(MetaErrors.NotReady);
            if (Save.claimed) return Result<Reward>.Fail(MetaErrors.AlreadyClaimed);
            var reward = RewardFor(Save.round);
            Save.claimed = true;
            _c.Grant(reward);
            _c.Meta.SetCounter(_c.Meta.stats, "tournaments", _c.Meta.GetCounter(_c.Meta.stats, "tournaments") + 1);
            _c.Touch(MetaArea.Profile);
            return Result<Reward>.Ok(reward);
        }

        /// <summary>Classement : neuf joueurs factices (stables pour ce joueur) et le joueur, du plus haut au plus bas.</summary>
        public IReadOnlyList<LeaderboardEntry> Leaderboard()
        {
            var rng = new Pcg32(Seed("board", ""));
            var all = new List<(string name, int rating, bool player)>();
            var used = new HashSet<string>();
            for (int i = 0; i < BoardSize - 1; i++)
            {
                string name;
                do name = Names[rng.Range(0, Names.Length)]; while (!used.Add(name));
                all.Add((name, 700 + rng.Range(0, 900), false));
            }
            all.Add(("", Save.rating, true));
            var sorted = all.OrderByDescending(e => e.rating).ThenBy(e => e.player ? 1 : 0).ToList();
            return sorted.Select((e, i) => new LeaderboardEntry(i + 1, e.name, e.rating, e.player)).ToList();
        }

        public int PlayerRank => Leaderboard().First(e => e.IsPlayer).Rank;
    }
}
