#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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


        public static int RecruitCost(Rarity r) => r == Rarity.Legend ? 1200 : r == Rarity.Elite ? 450 : 150;
        public static int TrainCost(int level) => 100 * level;
    }

    /// <summary>Copie locale du portefeuille (l'autorité sera UGS Economy au M2).</summary>
    public sealed class Wallet : IWalletReadModel
    {
        public int Credits { get; private set; }
        public int Pr { get; private set; }
        public int Gems { get; private set; }
        public void Add(int credits, int pr) { Credits += credits; Pr += pr; }
        public void AddGems(int gems) { Gems = Math.Max(0, Gems + gems); }
        public void SetGems(int gems) { Gems = Math.Max(0, gems); }
        public bool TrySpendGems(int gems)
        {
            if (gems < 0 || Gems < gems) return false;
            Gems -= gems;
            return true;
        }
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
        /// <summary>Objets equipes (au plus 2).</summary>
        public List<ItemId> Items { get; } = new List<ItemId>();
        /// <summary>Cartes equipees : emplacement 3 puis 4 (null = pas de carte).</summary>
        public CardId?[] Cards { get; } = new CardId?[2];
        public bool Exhausted => Fatigue >= 100;
        IReadOnlyList<ItemId> ITeamMemberReadModel.Items => Items;
        IReadOnlyList<CardId?> ITeamMemberReadModel.Cards => Cards;
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
        ZoneRun? _run;
        /// <summary>Plateau en cours ; en changer remet a zero les agents engages au combat (prouesse de compagnon).</summary>
        public ZoneRun? Run
        {
            get => _run;
            set { if (!ReferenceEquals(_run, value)) RunEngaged.Clear(); _run = value; }
        }
        /// <summary>Agents engages dans au moins un combat du plateau en cours.</summary>
        public HashSet<string> RunEngaged { get; } = new HashSet<string>();
        public ZoneDef? RunZone { get; set; }
        public PlayerAccount? Account { get; set; }
        public bool AutoCombat { get; set; }
        public ShopState Shop { get; } = new ShopState();
        /// <summary>Méta-progression (pass, missions, opérations…) : modifiée uniquement par les services de Features/Meta.</summary>
        public MetaSave Meta { get; private set; } = new MetaSave();

        public int OwnedItemCount { get { int n = 0; foreach (var i in Meta.collection.items) n += i.value; return n; } }
        public int OwnedCardKinds { get { int n = 0; foreach (var c in Meta.collection.cards) if (c.value > 0) n++; return n; } }

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

        /// <summary>Prix d entraînement avec la réduction du labo.</summary>
        public int TrainCostFor(int level) => Math.Max(1, MetaRules.TrainCost(level) * Features.Meta.LabBonuses.TrainCostPercent(Meta) / 100);

        public int SquadAverageLevel()
        {
            var squad = Squad;
            if (squad.Count == 0) return 1;
            int sum = 0;
            foreach (var m in squad) sum += m.Level;
            return Math.Max(1, sum / squad.Count);
        }

        /// <summary>Parcours du personnage choisi à la première connexion ; Agent D tant qu aucun choix n est enregistré.</summary>
        public StoryRoute Route => StoryRoutes.For(Meta.onboarding.hero);

        public int LinkLevel(MemberId a, MemberId b) => Features.Combat.TeamLinks.Level(Meta, a.Value, b.Value);

        /// <summary>Remplace l effectif par ces agents, tous en escouade (choix du personnage, avant toute progression).</summary>
        public void StartWithTeam(System.Collections.Generic.IEnumerable<string> ids)
        {
            Roster.Clear();
            foreach (var id in ids)
            {
                var def = FindDef(new MemberId(id));
                if (def != null && !Owns(def.Id)) Roster.Add(new TeamMember(def, MetaRules.StarterLevel) { InSquad = true });
            }
        }

        public void ResetToNewProfile()
        {
            Roster.Clear();
            CompletedZones.Clear();
            Run = null;
            RunZone = null;
            AutoCombat = false;
            Shop.Clear();
            Meta = new MetaSave();
            Wallet.SetGems(Meta.gems);
            Wallet.Set(MetaRules.StartCredits, MetaRules.StartPr);
            foreach (var id in Content.StarterTeam)
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
            Meta = data.meta ?? new MetaSave();
            Wallet.SetGems(Meta.gems);
            Roster.Clear();
            if (data.team != null)
            {
                foreach (var s in data.team)
                {
                    var def = FindDef(new MemberId(s.id ?? ""));
                    if (def == null || Owns(def.Id)) continue;
                    var member = new TeamMember(def, Math.Max(1, Math.Min(MetaRules.MaxLevel, s.level)))
                    {
                        Fatigue = Math.Max(0, Math.Min(100, s.fatigue)),
                        InSquad = s.inSquad,
                    };
                    LoadEquipment(member, s);
                    Roster.Add(member);
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
            Features.Zone.AgentUnlocks.CatchUpArcs(this);
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
            {
                var save = new MemberSave { id = m.Id.Value, level = m.Level, fatigue = m.Fatigue, inSquad = m.InSquad };
                foreach (var item in m.Items) save.items.Add(item.Value);
                foreach (var card in m.Cards) save.cards.Add(card.HasValue ? card.Value.Value : "");
                p.team.Add(save);
            }
            Meta.gems = Wallet.Gems;
            p.meta = Meta;
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

        /// <summary>
        /// Objets et cartes equipes sauvegardes ; un objet ou une carte inconnu du jeu, en trop ou d'une autre categorie est ignore,
        /// comme un objet d'un type deja porte (il reste dans l'inventaire).
        /// </summary>
        void LoadEquipment(TeamMember member, MemberSave save)
        {
            var collection = Content.Collection;
            if (save.items != null)
                foreach (var id in save.items)
                {
                    if (member.Items.Count >= CollectionRules.MaxItems) break;
                    if (string.IsNullOrEmpty(id) || !collection.HasItem(new ItemId(id))) continue;
                    if (!CollectionRules.CanEquip(collection.Item(new ItemId(id)), member.Items.Select(collection.Item))) continue;
                    member.Items.Add(new ItemId(id));
                }
            if (save.cards != null)
                for (int i = 0; i < save.cards.Count && i < member.Cards.Length; i++)
                {
                    var id = save.cards[i];
                    if (string.IsNullOrEmpty(id) || !collection.HasCard(new CardId(id))) continue;
                    if (!CollectionRules.CanEquip(collection.Card(new CardId(id)), member.Def)) continue;
                    member.Cards[i] = new CardId(id);
                }
            if (member.Cards[0] == null && member.Cards[1] != null) { member.Cards[0] = member.Cards[1]; member.Cards[1] = null; }
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
