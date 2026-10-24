#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.Tutorial;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Fiche d'un membre (M21). Membre possédé : stats, compétences, synergies de sa famille, flèches pour parcourir l'effectif
    /// (sans empiler de page), raccourcis Salon / Infirmerie, Entraîner · N CR.
    /// Héros de la boutique (<see cref="HubLocation.FromShop"/>) : même fiche, bouton Recruter · N PR par la confirmation M31 ;
    /// si l'offre disparaît (boutique renouvelée), retour à Recruter avec un toast.
    /// </summary>
    public sealed class MemberSheetPresenter : PresenterBase<IMemberSheetView>, IPageTitle
    {
        readonly ISessionReadModel _session;
        readonly ITrainMemberUseCase _train;
        readonly RecruitFlow _recruit;
        readonly HubNavigator _nav;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        readonly ISubscriber<ShopChanged> _shop;
        readonly PageAction _action;
        readonly bool _fromShop;
        readonly GameSession? _game;
        readonly CollectionBook? _book;
        readonly TutorialGuide? _tutorial;
        readonly List<Action> _pickActions = new List<Action>();
        Action? _pickRemove;
        IDisposable? _subs;
        MemberId _member;
        bool _left;

        public MemberSheetPresenter(IMemberSheetView view, HubLocation location, ISessionReadModel session, ITrainMemberUseCase train,
                                    RecruitFlow recruit, HubNavigator nav, IToastService toasts, ILocalization loc,
                                    ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, ISubscriber<ShopChanged> shop,
                                    GameSession? game = null, CollectionBook? book = null, TutorialGuide? tutorial = null)
            : base(view, null)
        {
            _session = session; _train = train; _recruit = recruit; _nav = nav; _toasts = toasts; _loc = loc;
            _wallet = wallet; _roster = roster; _shop = shop; _game = game; _book = book; _tutorial = tutorial;
            _member = location.Member ?? throw new ArgumentException("Fiche sans membre", nameof(location));
            _fromShop = location.FromShop;
            _action = new PageAction(toasts, loc);
        }

        public string Title => _loc.T("page.member_sheet");

        protected override void OnStart()
        {
            View.PreviousClicked += Previous;
            View.NextClicked += Next;
            View.PrimaryClicked += Primary;
            View.RoomClicked += OpenRoom;
            View.LoadoutClicked += OpenLoadout;
            View.SkillTileClicked += OnSkillTile;
            View.ItemTileClicked += OnItemTile;
            View.PickerChosen += OnPickerChosen;
            View.PickerRemoved += OnPickerRemoved;
            View.PickerClosed += ClosePicker;
        }

        protected override void OnShown()
        {
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _wallet.Subscribe(_ => Refresh()).AddTo(bag);
            _roster.Subscribe(_ => Refresh()).AddTo(bag);
            _shop.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
            Refresh();
            if (!_fromShop) _tutorial?.Signal(TutorialSignal.SheetOpened);
        }

        protected override void OnHidden()
        {
            _subs?.Dispose();
            _subs = null;
        }

        protected override void OnDispose()
        {
            _subs?.Dispose();
            View.PreviousClicked -= Previous;
            View.NextClicked -= Next;
            View.PrimaryClicked -= Primary;
            View.RoomClicked -= OpenRoom;
            View.LoadoutClicked -= OpenLoadout;
            View.SkillTileClicked -= OnSkillTile;
            View.ItemTileClicked -= OnItemTile;
            View.PickerChosen -= OnPickerChosen;
            View.PickerRemoved -= OnPickerRemoved;
            View.PickerClosed -= ClosePicker;
        }

        void Previous() => Step(-1);
        void Next() => Step(1);

        void Step(int delta)
        {
            if (_fromShop) return;
            var roster = _session.Roster;
            if (roster.Count == 0) return;
            int i = 0;
            for (int k = 0; k < roster.Count; k++) if (roster[k].Id == _member) { i = k; break; }
            _member = roster[((i + delta) % roster.Count + roster.Count) % roster.Count].Id;
            _nav.ReplaceCurrent(new HubLocation(HubPage.MemberSheet, _member));
            Refresh();
        }

        void OpenRoom(HubPage page) => _nav.Open(page);

        void OpenLoadout() => _nav.Open(HubPage.Loadout, _member);

        void Primary()
        {
            if (_fromShop) Recruit();
            else Train();
        }

        void Train()
        {
            var id = _member;
            _action.Run(ct => _train.ExecuteAsync(id, ct), Lifetime, () =>
            {
                var m = _session.Roster.FirstOrDefault(x => x.Id == id);
                if (m != null) _toasts.Show(_loc.F("hub.trained", _loc.MemberName(m.Def), m.Level), ToastKind.Reward);
                _tutorial?.Signal(TutorialSignal.Trained);
                Refresh();
            });
        }

        void Recruit()
        {
            int slot = Slot();
            if (slot < 0) return;
            RecruitAsync(slot).Forget();
        }

        async UniTaskVoid RecruitAsync(int slot)
        {
            var outcome = await _recruit.RunAsync(slot, Lifetime);
            if (Disposed) return;
            if (outcome == RecruitOutcome.Expired) Leave(null);
            else Refresh();
        }

        int Slot()
        {
            var offers = _session.Shop.Offers;
            for (int i = 0; i < offers.Count; i++) if (offers[i].Id == _member) return i;
            return -1;
        }

        /// <summary>
        /// Le héros de la fiche n'existe plus (offre renouvelée, membre effacé) : retour à la page précédente, une seule fois,
        /// avec le toast <paramref name="error"/> si personne ne l'a déjà affiché.
        /// </summary>
        void Leave(DomainError? error)
        {
            if (_left) return;
            _left = true;
            if (error != null) _toasts.Show(_loc.Error(error), ToastKind.Error);
            _nav.Back();
        }

        void Refresh()
        {
            if (Disposed || _left) return;
            if (_fromShop) RefreshShop();
            else RefreshOwned();
        }

        void RefreshOwned()
        {
            var m = _session.Roster.FirstOrDefault(x => x.Id == _member);
            if (m == null) { Leave(HubErrors.UnknownMember); return; }
            bool max = m.Level >= MetaRules.MaxLevel;
            int cost = _session.TrainCostFor(m.Level);
            int missing = cost - _session.Wallet.Credits;
            var d = Sheet(m.Def, m.Level, true, m);
            d.FatigueText = _loc.F("ui.fatigue", m.Fatigue);
            d.Fatigue01 = m.Fatigue / 100f;
            d.Fatigue = m.Fatigue;
            d.PrimaryLabel = max ? _loc.T("hub.train.max") : _loc.F("hub.train", cost + " CR");
            d.PrimaryEnabled = !max && missing <= 0;
            d.PrimaryReason = !max && missing > 0 ? _loc.F("hub.train.missing", missing) : "";
            d.ShowArrows = _session.Roster.Count > 1;
            d.ShowRooms = true;
            d.ShowLoadout = true;
            d.LoadoutLabel = _loc.F("sheet.loadout_counts", m.Items.Count, 2, m.Cards.Count(c => c.HasValue), 2);
            View.Show(d);
        }

        void RefreshShop()
        {
            int slot = Slot();
            if (slot < 0) { Leave(ShopErrors.Expired); return; }
            var offer = _session.Shop.Offers[slot];
            var def = _session.Content.Member(offer.Id);
            bool inTeam = offer.Recruited || _session.Roster.Any(x => x.Id == offer.Id);
            var d = Sheet(def, MetaRules.RecruitLevel, false);
            d.FatigueText = "";
            d.PrimaryLabel = inTeam ? _loc.T("recruit.in_team") : _loc.F("hub.recruit", MetaRules.RecruitCost(def.Rarity));
            d.PrimaryEnabled = !inTeam;
            d.PrimaryReason = "";
            View.Show(d);
        }

        MemberSheetData Sheet(MemberDef def, int level, bool owned, ITeamMemberReadModel? m = null)
        {
            var d = new MemberSheetData
            {
                Card = MemberCards.Card(def, level, _session.Content.Balance, _loc),
                Owned = owned,
                Skills = def.Skills.Select(s => { var st = CollectionText.StatsOf(def, level, _session.Content.Balance); return new SkillLineData { Name = _loc.SkillName(s), Description = _loc.DescribeSkill(s, st.Atk, st.MaxHp) }; }).ToArray(),
                Synergies = SquadSynergies.For(def.Family, _session.Content.Synergies)
                                    .Select(c => _loc.F("sheet.synergy_line", _loc.SynergyName(c), _loc.SynergyEffect(c), _loc.FamilyName(c.A == def.Family ? c.B : c.A)))
                                    .ToArray(),
                Equippable = owned && m != null && _game != null && _book != null,
            };
            string bioKey = "members." + def.Id.Value + ".bio";
            d.Bio = _loc.Has(bioKey) ? _loc.T(bioKey) : "";
            int max = MetaRules.MaxLevel;
            d.Level01 = max <= 1 ? 1f : Mathf.Clamp01((level - 1f) / (max - 1f));
            d.LevelBarText = _loc.F("sheet.level_bar", level, max);
            d.TiersTitle = _loc.T("sheet.tiers");
            d.Tiers = Tiers(def, level, max);
            d.SkillTiles = SkillTiles(def, m);
            d.ItemTiles = ItemTiles(m);
            d.Affinity = Affinity(def);
            d.AffinityTitle = _loc.T("sheet.affinity");
            return d;
        }

        /// <summary>Paliers de niveau : les bonus passifs que l'agent gagne en montant (atteints en clair, le prochain en Ã©vidence).</summary>
        TierLineData[] Tiers(MemberDef def, int level, int max)
        {
            var next = LevelTiers.All.Where(t => t.Level > level).Select(t => (int?)t.Level).FirstOrDefault();
            return LevelTiers.All.Select(t => new TierLineData
            {
                Level = _loc.F("sheet.tier_level", t.Level),
                Stats = _loc.F("sheet.tier_passive", t.Percent, _loc.T("synergy.stat." + t.Stat)),
                Reached = t.Level <= level,
                Current = t.Level == next,
            }).ToArray();
        }

        SheetTile[] SkillTiles(MemberDef def, ITeamMemberReadModel? m)
        {
            var tiles = new SheetTile[4];
            var family = Theme.FamilyColor(def.Family);
            for (int i = 0; i < 2; i++)
            {
                bool has = i < def.Skills.Count;
                tiles[i] = new SheetTile
                {
                    Title = has ? _loc.SkillName(def.Skills[i]) : "",
                    Sub = _loc.F("sheet.skill_fixed", i + 1),
                    Art = has ? AgentArt.Ability(def.Id.Value + i) : null,
                    Frame = family,
                    Kind = has ? SheetTileKind.Fixed : SheetTileKind.Empty,
                };
            }
            var defs = _session.Content.Collection;
            for (int slot = 0; slot < CollectionRules.CardSlots; slot++)
            {
                var card = m != null && slot < m.Cards.Count ? m.Cards[slot] : null;
                string sub = _loc.F("sheet.skill_card", slot + 3);
                if (card.HasValue && defs.HasCard(card.Value))
                {
                    var cd = defs.Card(card.Value);
                    tiles[slot + 2] = new SheetTile { Title = CollectionText.CardName(_loc, cd), Sub = sub, Art = AgentArt.Card(card.Value.Value),
                                                      Frame = CollectionText.RarityColor(cd.Rarity), Kind = SheetTileKind.Card };
                }
                else
                {
                    bool locked = m == null || (slot == 1 && (m.Cards.Count == 0 || !m.Cards[0].HasValue));
                    tiles[slot + 2] = new SheetTile { Title = locked ? _loc.T("sheet.slot_locked") : _loc.T("sheet.slot_empty"), Sub = sub,
                                                      Frame = Theme.Outline, Kind = locked ? SheetTileKind.Locked : SheetTileKind.Empty };
                }
            }
            return tiles;
        }

        SheetTile[] ItemTiles(ITeamMemberReadModel? m)
        {
            var tiles = new SheetTile[CollectionRules.MaxItems + 1];
            var defs = _session.Content.Collection;
            for (int i = 0; i < CollectionRules.MaxItems; i++)
            {
                if (m != null && i < m.Items.Count && defs.HasItem(m.Items[i]))
                {
                    var it = defs.Item(m.Items[i]);
                    tiles[i] = new SheetTile { Title = CollectionText.ItemName(_loc, it), Sub = CollectionText.ItemEffects(_loc, it), Art = AgentArt.Item(it.Id.Value), Frame = CollectionText.RarityColor(it.Rarity), Kind = SheetTileKind.Item };
                }
                else
                {
                    tiles[i] = new SheetTile { Title = m == null ? _loc.T("sheet.slot_locked") : _loc.T("sheet.slot_empty"), Sub = _loc.F("sheet.item_slot", i + 1),
                                               Frame = Theme.Outline, Kind = m == null ? SheetTileKind.Locked : SheetTileKind.Empty };
                }
            }
            tiles[CollectionRules.MaxItems] = new SheetTile { Title = _loc.T("sheet.slot_locked"), Sub = _loc.F("sheet.item_slot", CollectionRules.MaxItems + 1), Frame = Theme.Outline, Kind = SheetTileKind.Locked };
            return tiles;
        }

        /// <summary>Agents de l'effectif dont la famille forme une synergie avec celle de l'agent (les plus hauts niveaux d'abord).</summary>
        PortraitData[] Affinity(MemberDef def)
        {
            var partners = new HashSet<Family>(SquadSynergies.For(def.Family, _session.Content.Synergies).Select(c => c.A == def.Family ? c.B : c.A));
            return _session.Roster.Where(x => x.Id != _member && partners.Contains(x.Def.Family))
                           .OrderByDescending(x => x.Level)
                           .Take(3)
                           .Select(x => new PortraitData(_loc.MemberName(x.Def), x.Def.Family, art: AgentArt.For(x.Def.Id.Value)))
                           .ToArray();
        }

        // ---------------------------------------------------------------- Équipement (panneau d'inventaire)

        ITeamMemberReadModel? OwnedMember() => _fromShop ? null : _session.Roster.FirstOrDefault(x => x.Id == _member);

        void OnSkillTile(int index)
        {
            var m = OwnedMember();
            if (m == null || _game == null || _book == null) return;
            if (index < 2)
            {
                if (index < m.Def.Skills.Count) _toasts.Show(_loc.SkillName(m.Def.Skills[index]) + " · " + _loc.DescribeSkill(m.Def.Skills[index], CollectionText.StatsOf(m.Def, m.Level, _session.Content.Balance).Atk, CollectionText.StatsOf(m.Def, m.Level, _session.Content.Balance).MaxHp), ToastKind.Info);
                return;
            }
            int slot = index - 2;
            if (slot == 1 && !m.Cards[0].HasValue) { _toasts.Show(_loc.T("loadout.locked_card_sub"), ToastKind.Warning); return; }
            // Une carte Ã©quipÃ©e s'ouvre dans le grimoire (description, niveau, amÃ©lioration) ; un emplacement vide ouvre le choix des cartes.
            if (m.Cards[slot].HasValue) { _nav.FocusCard = m.Cards[slot]!.Value.Value; _nav.Open(HubPage.Grimoire); return; }
            OpenCardPicker(m, slot);
        }

        void OnItemTile(int index)
        {
            var m = OwnedMember();
            if (m == null || _game == null || _book == null) return;
            if (index >= CollectionRules.MaxItems) { _toasts.Show(_loc.T("sheet.item_locked_toast"), ToastKind.Info); return; }
            OpenItemPicker(m, index);
            _tutorial?.Signal(TutorialSignal.SlotOpened);
        }

        void OpenCardPicker(ITeamMemberReadModel m, int slot)
        {
            var defs = _session.Content.Collection;
            var options = new List<SheetTile>();
            _pickActions.Clear();
            foreach (var cd in CollectionText.CardsFor(defs, m.Def).OrderByDescending(c => c.Rarity).ThenBy(c => c.Id))
            {
                int free = CollectionOps.AvailableCards(_game!, cd.Id);
                if (free <= 0) continue;
                var id = cd.Id;
                string name = CollectionText.CardName(_loc, cd);
                options.Add(new SheetTile { Title = name, Sub = free > 1 ? _loc.F("loadout.copies", free) : CollectionText.RarityName(_loc, cd.Rarity),
                                            Art = AgentArt.Card(id.Value), Frame = CollectionText.RarityColor(cd.Rarity), Kind = SheetTileKind.Card });
                _pickActions.Add(() => Apply(_book!.EquipCard(_member, slot, id), "loadout.equipped", name));
            }
            var current = m.Cards[slot];
            _pickRemove = current.HasValue && defs.HasCard(current.Value)
                ? () => Apply(_book!.UnequipCard(_member, slot), "loadout.removed", CollectionText.CardName(_loc, defs.Card(current.Value)))
                : (Action?)null;
            View.ShowPicker(new PickerData
            {
                Title = _loc.F("sheet.picker_cards", slot + 3),
                Hint = options.Count == 0 ? _loc.T("loadout.none_cards") : _loc.T("sheet.picker_hint"),
                Options = options.Take(8).ToArray(),
                CanRemove = _pickRemove != null,
            });
        }

        void OpenItemPicker(ITeamMemberReadModel m, int slot)
        {
            var defs = _session.Content.Collection;
            var options = new List<SheetTile>();
            _pickActions.Clear();
            ItemId? currentId = slot < m.Items.Count ? m.Items[slot] : (ItemId?)null;
            foreach (var it in defs.Items.OrderByDescending(i => i.Rarity).ThenBy(i => i.Id))
            {
                int free = CollectionOps.AvailableItems(_game!, it.Id);
                if (free <= 0) continue;
                var id = it.Id;
                string name = CollectionText.ItemName(_loc, it);
                options.Add(new SheetTile { Title = name, Sub = free > 1 ? _loc.F("loadout.copies", free) : CollectionText.ItemEffects(_loc, it),
                                            Art = AgentArt.Item(id.Value), Frame = CollectionText.RarityColor(it.Rarity), Kind = SheetTileKind.Item });
                _pickActions.Add(() =>
                {
                    // Emplacement occupé : l'objet actuel est retiré avant d'équiper le nouveau (remis en place si l'échange échoue).
                    if (currentId.HasValue) _book!.UnequipItem(_member, currentId.Value);
                    var r = _book!.EquipItem(_member, id);
                    if (r.IsFailure && currentId.HasValue) _book.EquipItem(_member, currentId.Value);
                    Apply(r, "loadout.equipped", name);
                });
            }
            _pickRemove = currentId.HasValue
                ? () => Apply(_book!.UnequipItem(_member, currentId.Value), "loadout.removed", CollectionText.ItemName(_loc, defs.Item(currentId.Value)))
                : (Action?)null;
            View.ShowPicker(new PickerData
            {
                Title = _loc.F("sheet.picker_items", slot + 1),
                Hint = options.Count == 0 ? _loc.T("loadout.none_items") : _loc.T("sheet.picker_hint"),
                Options = options.Take(8).ToArray(),
                CanRemove = _pickRemove != null,
            });
        }

        void OnPickerChosen(int index)
        {
            if (index >= 0 && index < _pickActions.Count) _pickActions[index]();
        }

        void OnPickerRemoved() => _pickRemove?.Invoke();

        void ClosePicker()
        {
            _pickActions.Clear();
            _pickRemove = null;
            View.HidePicker();
        }

        void Apply(Result r, string successKey, string what)
        {
            if (r.IsFailure) _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
            else
            {
                _toasts.Show(_loc.F(successKey, what), ToastKind.Reward);
                if (successKey == "loadout.equipped") _tutorial?.Signal(TutorialSignal.ItemEquipped);
            }
            ClosePicker();
            Refresh();
        }
    }
}
