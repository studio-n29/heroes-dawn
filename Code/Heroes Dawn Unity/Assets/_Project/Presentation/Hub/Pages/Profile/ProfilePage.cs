#nullable enable
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Écran Profil (planche 12.2) : bandeau (avatar, pseudo, tag, bloc Compte), grille « Mes stats » de six tuiles, liste d'amis,
    /// liens vers le registre, les archives et le tournoi. La tuile Succès ouvre la liste complète des succès à la place de la grille.
    /// </summary>
    public sealed class ProfilePagePresenter : PresenterBase<IProfilePageView>, IPageTitle
    {
        /// <summary>Nombre d'amis affiché au dénominateur du titre (« Amis (3 / 50) »).</summary>
        public const int MaxFriends = LocalFriendsPort.MaxFriends;

        readonly PlayerProfile _profile;
        readonly IFriendsPort _friends;
        readonly HubNavigator _nav;
        readonly ISessionReadModel _session;
        readonly ILocalization _loc;
        readonly IToastService _toasts;
        readonly ISubscriber<MetaChanged> _meta;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<RosterChanged> _roster;
        readonly IModalService _modals;
        readonly AccountLink? _account;
        readonly IAccountFlow? _accountFlow;
        readonly LairBook? _lairs;
        IDisposable? _subs;
        bool _achievementsOpen;

        public ProfilePagePresenter(IProfilePageView view, PlayerProfile profile, IFriendsPort friends, HubNavigator nav, ISessionReadModel session,
                                    ILocalization loc, IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                    ISubscriber<RosterChanged> roster, IClock clock, IModalService modals,
                                    AccountLink? account = null, IAccountFlow? accountFlow = null, LairBook? lairs = null)
            : base(view, null)
        {
            _profile = profile; _friends = friends; _nav = nav; _session = session; _loc = loc; _toasts = toasts;
            _meta = meta; _wallet = wallet; _roster = roster; _modals = modals;
            _account = account; _accountFlow = accountFlow; _lairs = lairs;
        }

        public string Title => _loc.T("page.profile");

        bool IsConnected => _account?.State == AccountState.Connected;

        protected override void OnStart()
        {
            View.TagClicked += CopyTag;
            View.AvatarClicked += CycleAvatar;
            View.AccountClicked += OnAccount;
            View.TileClicked += OnTile;
            View.AddFriendClicked += OnAddFriend;
            View.FriendRemoveClicked += OnFriendRemove;
            View.FriendCopyClicked += OnFriendCopy;
            View.LinkClicked += OnLink;
            View.BackClicked += OnBack;
        }

        protected override void OnShown()
        {
            _subs?.Dispose();
            var bag = DisposableBag.CreateBuilder();
            _meta.Subscribe(_ => Refresh()).AddTo(bag);
            _wallet.Subscribe(_ => Refresh()).AddTo(bag);
            _roster.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
            Refresh();
        }

        protected override void OnHidden() { _subs?.Dispose(); _subs = null; }

        protected override void OnDispose()
        {
            _subs?.Dispose();
            View.TagClicked -= CopyTag;
            View.AvatarClicked -= CycleAvatar;
            View.AccountClicked -= OnAccount;
            View.TileClicked -= OnTile;
            View.AddFriendClicked -= OnAddFriend;
            View.FriendRemoveClicked -= OnFriendRemove;
            View.FriendCopyClicked -= OnFriendCopy;
            View.LinkClicked -= OnLink;
            View.BackClicked -= OnBack;
        }

        void Refresh()
        {
            if (Disposed) return;
            View.Show(Build());
        }

        ProfileScreenData Build()
        {
            bool connected = IsConnected;
            var s = _profile.Stats();
            var list = _profile.Achievements().List();
            int unlocked = list.Count(a => a.Unlocked);
            int lair = _lairs == null ? 0 : Lairs.All.Sum(l => _lairs.DefeatedCount(l));
            var friends = _friends.Friends();
            var data = new ProfileScreenData
            {
                HeroId = _profile.HeroId,
                Pseudo = _profile.Pseudo,
                TagLine = connected ? _profile.Tag : _loc.T("profile.tag_guest"),
                TagCopyable = connected,
                RankLine = _loc.F("profile.header_line", _loc.T(_profile.RankKey), _session.Route.HeroName, _profile.PassLevel),
                Tiles = new[]
                {
                    new ProfileTileData(ProfileTiles.Level, _loc.T("profile.tile.level"), s.TopAgentLevel.ToString(), false),
                    new ProfileTileData(ProfileTiles.Agents, _loc.T("profile.tile.agents"), s.Agents.ToString(), true),
                    new ProfileTileData(ProfileTiles.Operations, _loc.T("profile.tile.operations"), s.CombatsWon.ToString(), false),
                    new ProfileTileData(ProfileTiles.Missions, _loc.T("profile.tile.missions"), s.ZonesCleared.ToString(), false),
                    new ProfileTileData(ProfileTiles.Achievements, _loc.T("profile.tile.achievements"), unlocked.ToString(), true),
                    new ProfileTileData(ProfileTiles.Lair, _loc.T("profile.tile.lair"), lair.ToString(), true),
                },
                FriendsTitle = _loc.F("profile.friends_title", friends.Count, MaxFriends),
                FriendsEmpty = _loc.T("profile.friends_empty"),
                AddLabel = _loc.T("profile.add_short"),
                Friends = friends.Select(f => new ProfileFriendData(f.Name, f.Tag, _loc.T("profile.friend_status_pending"), false)).ToArray(),
                CanAddFriend = connected,
                AddReason = connected ? "" : _loc.T("profile.tag_guest_reason"),
                LinkLabels = new[] { _loc.T("profile.registry"), _loc.T("profile.archives"), _loc.T("profile.tournament") },
                BackLabel = _loc.T("ach.back"),
            };
            if (_account != null && _accountFlow != null) FillAccount(ref data);
            if (_achievementsOpen)
            {
                data.AchievementsTitle = _loc.T("ach.title");
                data.AchievementsLine = _loc.F("ach.line", unlocked, list.Count);
                data.Achievements = list.Select(a => new ProfileAchievementData(
                    _loc.T(a.Def.NameKey), _loc.T(a.Def.DescKey),
                    a.Unlocked ? a.UnlockedDay : _loc.F("ach.progress", a.Progress, a.Def.Target),
                    a.Unlocked, a.Def.Target == 0 ? 0f : Mathf.Clamp01(a.Progress / (float)a.Def.Target))).ToArray();
            }
            return data;
        }

        /// <summary>Bloc « Compte » : invité (Lier mon compte) ou connecté (adresse, Synchroniser, Se déconnecter).</summary>
        void FillAccount(ref ProfileScreenData d)
        {
            d.AccountVisible = true;
            var cloud = _account!.Cloud;
            if (cloud == null)
            {
                d.AccountLabel = _loc.T("profile.account.label_guest");
                d.AccountA = _loc.T("profile.account.link");
                d.AccountB = "";
                d.AccountBVisible = false;
                return;
            }
            d.AccountLabel = _loc.F("profile.account.label", string.IsNullOrEmpty(cloud.Email) ? cloud.DisplayName : cloud.Email);
            d.AccountA = _loc.T("profile.account.sync");
            d.AccountB = _loc.T("profile.account.signout");
            d.AccountBVisible = true;
        }

        void OnAccount(int button)
        {
            if (_account == null || _accountFlow == null) return;
            if (_account.Cloud == null) RunAccountAsync(() => _accountFlow.RunAsync(Lifetime)).Forget();
            else if (button == 0) RunAccountAsync(() => _accountFlow.SyncNowAsync(Lifetime)).Forget();
            else RunAccountAsync(() => _accountFlow.SignOutAsync(Lifetime)).Forget();
        }

        async UniTaskVoid RunAccountAsync(Func<UniTask> action)
        {
            try { await action(); }
            catch (OperationCanceledException) { return; }
            if (!Disposed) Refresh();
        }

        void CopyTag()
        {
            if (!IsConnected) return; // hors ligne : pas de tag à copier
            GUIUtility.systemCopyBuffer = _profile.Tag;
            _toasts.Show(_loc.T("profile.copied"), ToastKind.Reward);
        }

        void CycleAvatar() { _profile.CycleAvatar(); Refresh(); }

        void OnTile(int index)
        {
            var tiles = Build().Tiles;
            if (index < 0 || index >= tiles.Length) return;
            switch (tiles[index].Id)
            {
                case ProfileTiles.Agents: _nav.Open(HubPage.Agents); break;
                case ProfileTiles.Lair: _nav.Open(HubPage.Lair); break;
                case ProfileTiles.Achievements: _achievementsOpen = true; Refresh(); break;
            }
        }

        void OnBack() { _achievementsOpen = false; Refresh(); }

        void OnLink(int index)
        {
            switch (index)
            {
                case 0: _nav.Open(HubPage.Agents); break;
                case 1: _nav.Open(HubPage.Archives); break;
                case 2: _nav.Open(HubPage.Tournament); break;
            }
        }

        void OnFriendRemove(int index)
        {
            var friends = _friends.Friends();
            if (index < 0 || index >= friends.Count) return;
            _friends.Remove(friends[index].Tag);
            _toasts.Show(_loc.T("profile.friend_removed"), ToastKind.Reward);
            Refresh();
        }

        void OnFriendCopy(int index)
        {
            var friends = _friends.Friends();
            if (index < 0 || index >= friends.Count) return;
            GUIUtility.systemCopyBuffer = friends[index].Tag;
            _toasts.Show(_loc.T("profile.copied"), ToastKind.Reward);
        }

        void OnAddFriend() { if (IsConnected) AddFriendAsync().Forget(); }

        /// <summary>Ouvre la saisie du tag ; un tag valide s'ajoute à la liste, une erreur s'affiche, annuler ne fait rien.</summary>
        async UniTaskVoid AddFriendAsync()
        {
            var view = await _modals.OpenAsync<IFriendAddPopupView>(Lifetime);
            string? tag;
            try { tag = await view.AskAsync(_loc.T("profile.add_title"), _loc.T("profile.add_hint"), _loc.T("profile.add_ok"), _loc.T("ui.cancel"), Lifetime); }
            finally { _modals.Close(view); }
            if (tag == null) return;
            var r = _friends.Add(tag);
            if (r.IsFailure) _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
            else _toasts.Show(_loc.T("profile.friend_added"), ToastKind.Reward);
            Refresh();
        }
    }
}
