#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Règles méta du prototype (QG, recrutement, entraînement). Valeurs issues du GDD §7-9 et du tableur.</summary>
    public static class MetaRules
    {
        public const int StartCredits = 800;
        public const int StartPr = 160;
        public const int StarterLevel = 5;
        public const int RecruitLevel = 5;
        public const int MaxLevel = 50;
        public const int SquadSize = 4;
        public const int InfirmaryCost = 150;
        public const int InfirmaryReduction = 30;
        public const int BossCredits = 1000;
        public const int ZoneReplayCredits = 300;
        /// <summary>Récupération passive de fatigue (points par minute réelle, hors mission).</summary>
        public const int FatigueRecoveryPerMinute = 1;

        public static readonly string[] StarterTeam = { "ancre", "givre", "echo", "seve" };

        public static int RecruitCost(Rarity r) => r == Rarity.Legend ? 1200 : r == Rarity.Elite ? 450 : 150;
        public static int TrainCost(int level) => 100 * level;
    }

    /// <summary>Copie locale du portefeuille (l'autorité sera UGS Economy au M2).</summary>
    public sealed class Wallet : IWalletReadModel
    {
        public int Credits { get; private set; }
        public int Pr { get; private set; }
        public void Add(int credits, int pr) { Credits += credits; Pr += pr; }
        public void Set(int credits, int pr) { Credits = credits; Pr = pr; }
        public bool TrySpend(int credits, int pr)
        {
            if (Credits < credits || Pr < pr) return false;
            Credits -= credits;
            Pr -= pr;
            return true;
        }
    }

    public sealed class TeamMember : ITeamMemberReadModel
    {
        public MemberId Id { get; }
        public MemberDef Def { get; }
        public int Level { get; set; }
        public int Fatigue { get; set; }
        public bool InSquad { get; set; }
        public bool Exhausted => Fatigue >= 100;
        public TeamMember(MemberDef def, int level) { Def = def; Id = def.Id; Level = level; }

        public void AddFatigue(int amount) => Fatigue = Math.Max(0, Math.Min(100, Fatigue + amount));
    }

    /// <summary>État du joueur pour la session. Lu par la présentation, modifié uniquement par les services.</summary>
    public sealed class GameSession : ISessionReadModel
    {
        public ContentCatalog Content { get; }
        public Wallet Wallet { get; } = new Wallet();
        public List<TeamMember> Roster { get; } = new List<TeamMember>();
        public HashSet<string> CompletedZones { get; } = new HashSet<string>();
        public ZoneRun? Run { get; set; }
        public ZoneDef? RunZone { get; set; }
        public PlayerAccount? Account { get; set; }
        public bool AutoCombat { get; set; }
        public ShopState Shop { get; } = new ShopState();

        public GameSession(ContentCatalog content)
        {
            Content = content;
            ResetToNewProfile();
        }

        public IReadOnlyList<TeamMember> Squad
        {
            get
            {
                var list = new List<TeamMember>();
                foreach (var m in Roster) if (m.InSquad) list.Add(m);
                return list;
            }
        }

        /// <summary>Membres de l'escouade encore capables de combattre.</summary>
        public IReadOnlyList<TeamMember> Fighters
        {
            get
            {
                var list = new List<TeamMember>();
                foreach (var m in Roster) if (m.InSquad && !m.Exhausted) list.Add(m);
                return list;
            }
        }

        public bool SquadExhausted => Fighters.Count == 0;

        // ---- ISessionReadModel
        IWalletReadModel ISessionReadModel.Wallet => Wallet;
        IReadOnlyList<ITeamMemberReadModel> ISessionReadModel.Roster => Roster;
        IReadOnlyList<ITeamMemberReadModel> ISessionReadModel.Squad => Squad;
        IReadOnlyCollection<string> ISessionReadModel.CompletedZones => CompletedZones;
        IShopReadModel ISessionReadModel.Shop => Shop;

        public TeamMember? Find(MemberId id)
        {
            foreach (var m in Roster) if (m.Id == id) return m;
            return null;
        }

        public bool Owns(MemberId id) => Find(id) != null;

        public int SquadAverageLevel()
        {
            var squad = Squad;
            if (squad.Count == 0) return 1;
            int sum = 0;
            foreach (var m in squad) sum += m.Level;
            return Math.Max(1, sum / squad.Count);
        }

        public void ResetToNewProfile()
        {
            Roster.Clear();
            CompletedZones.Clear();
            Run = null;
            RunZone = null;
            AutoCombat = false;
            Shop.Clear();
            Wallet.Set(MetaRules.StartCredits, MetaRules.StartPr);
            foreach (var id in MetaRules.StarterTeam)
            {
                var def = FindDef(new MemberId(id));
                if (def != null) Roster.Add(new TeamMember(def, MetaRules.StarterLevel) { InSquad = true });
            }
        }

        MemberDef? FindDef(MemberId id)
        {
            foreach (var m in Content.Members) if (m.Id == id) return m;
            return null;
        }

        public void ApplyProfile(ProfileData data)
        {
            Wallet.Set(Math.Max(0, data.credits), Math.Max(0, data.pr));
            CompletedZones.Clear();
            if (data.completedZones != null) foreach (var z in data.completedZones) CompletedZones.Add(z);
            AutoCombat = data.autoCombat;
            Roster.Clear();
            if (data.team != null)
            {
                foreach (var s in data.team)
                {
                    var def = FindDef(new MemberId(s.id ?? ""));
                    if (def == null || Owns(def.Id)) continue;
                    Roster.Add(new TeamMember(def, Math.Max(1, Math.Min(MetaRules.MaxLevel, s.level)))
                    {
                        Fatigue = Math.Max(0, Math.Min(100, s.fatigue)),
                        InSquad = s.inSquad,
                    });
                }
            }
            if (Roster.Count == 0)
            {
                ResetToNewProfile();
                return;
            }
            ApplyShop(data.shop);
            // Escouade cohérente : 1 à 4 membres.
            int inSquad = 0;
            foreach (var m in Roster)
            {
                if (m.InSquad && inSquad >= MetaRules.SquadSize) m.InSquad = false;
                if (m.InSquad) inSquad++;
            }
            if (inSquad == 0)
                for (int i = 0; i < Roster.Count && i < MetaRules.SquadSize; i++) Roster[i].InSquad = true;
        }

        public ProfileData ToProfile(DateTime utcNow)
        {
            var p = new ProfileData
            {
                credits = Wallet.Credits,
                pr = Wallet.Pr,
                lastSeenUtcTicks = utcNow.Ticks,
                autoCombat = AutoCombat,
            };
            foreach (var z in CompletedZones) p.completedZones.Add(z);
            p.completedZones.Sort(StringComparer.Ordinal);
            foreach (var m in Roster)
                p.team.Add(new MemberSave { id = m.Id.Value, level = m.Level, fatigue = m.Fatigue, inSquad = m.InSquad });
            p.shop.day = Shop.Day;
            p.shop.draw = Shop.Draw;
            p.shop.seenDay = Shop.SeenDay;
            foreach (var o in Shop.Offers)
            {
                p.shop.offers.Add(o.Id.Value);
                p.shop.recruited.Add(o.Recruited);
            }
            return p;
        }

        /// <summary>Restaure la boutique sauvegardée ; une offre dont la recrue n'existe plus est ignorée.</summary>
        void ApplyShop(ShopSave? save)
        {
            if (save == null || string.IsNullOrEmpty(save.day)) { Shop.Clear(); return; }
            var offers = new List<ShopOffer>();
            for (int i = 0; i < (save.offers?.Count ?? 0); i++)
            {
                var def = FindDef(new MemberId(save.offers![i] ?? ""));
                if (def == null) continue;
                bool recruited = save.recruited != null && i < save.recruited.Count && save.recruited[i];
                offers.Add(new ShopOffer(def.Id, recruited));
            }
            Shop.Replace(save.day, Math.Max(0, save.draw), offers);
            Shop.SetSeenDay(save.seenDay ?? "");
        }
    }
}
