#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public interface IProfilePageView : IListPageView { }

    /// <summary>
    /// Profil du joueur : en-tête (tag personnel, pseudo, héros, niveau de pass), accès au registre des agents et aux archives,
    /// statistiques de jeu, puis la liste d'amis. Les actions des rangées sont mémorisées à chaque rendu (une par rangée).
    /// </summary>
    public sealed class ProfilePagePresenter : ListPagePresenter<IProfilePageView>
    {
        readonly PlayerProfile _profile;
        readonly IFriendsPort _friends;
        readonly HubNavigator _nav;
        readonly ISessionReadModel _session;
        readonly IModalService _modals;
        readonly List<Action?> _actions = new List<Action?>();
        readonly List<Action?> _actionsB = new List<Action?>();
        readonly AccountLink? _account;
        readonly IAccountFlow? _accountFlow;

        public ProfilePagePresenter(IProfilePageView view, PlayerProfile profile, IFriendsPort friends, HubNavigator nav, ISessionReadModel session,
                                    ILocalization loc, IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                    ISubscriber<RosterChanged> roster, IClock clock, IModalService modals,
                                    AccountLink? account = null, IAccountFlow? accountFlow = null)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _profile = profile; _friends = friends; _nav = nav; _session = session; _modals = modals;
            _account = account; _accountFlow = accountFlow;
        }

        public override string Title => Loc.T("page.profile");

        /// <summary>Vrai quand la liste complète des succès est ouverte (depuis le dernier succès).</summary>
        bool _achievementsOpen;

        protected override void Render()
        {
            if (_achievementsOpen) { RenderAchievements(); return; }
            // Le tag (qui permet d'être trouvé) n'existe qu'avec un compte : en hors ligne, il est remplacé par une invitation.
            bool connected = IsConnected;
            View.ShowHeader(new ListHeaderData
            {
                Title = connected ? _profile.Tag : Loc.T("profile.tag_guest"),
                Line = Loc.F("profile.header_line", _profile.Pseudo, _session.Route.HeroName, _profile.PassLevel),
                Progress = -1f,
                A = RowButtonData.Of(Loc.T("profile.copy"), connected, Loc.T("profile.tag_guest_reason"), 0),
                B = RowButtonData.Of(Loc.T("profile.add"), connected, Loc.T("profile.tag_guest_reason"), 1),
            });

            _actions.Clear();
            _actionsB.Clear();
            var rows = new List<InfoRowData>();
            void Add(InfoRowData row, Action? action, Action? actionB = null) { rows.Add(row); _actions.Add(action); _actionsB.Add(actionB); }

            if (_account != null && _accountFlow != null) AddAccountRow(Add);

            Add(NavRow("profile.registry", "profile.registry.sub", "R"), () => _nav.Open(HubPage.Agents));
            Add(NavRow("profile.archives", "profile.archives.sub", "A"), () => _nav.Open(HubPage.Archives));

            // Dernier succès débloqué : un appui ouvre la liste complète.
            var last = _profile.Achievements().Last();
            var lastRow = InfoRowData.Basic(Loc.T("profile.last_achievement"),
                last.HasValue ? Loc.T(last.Value.Def.NameKey) + " · " + last.Value.UnlockedDay : Loc.T("profile.no_achievement"));
            lastRow.Tag = "A";
            lastRow.TagColor = Theme.Primary;
            lastRow.A = RowButtonData.Of(Loc.T("profile.open"), true, "", 0);
            Add(lastRow, () => { _achievementsOpen = true; Refresh(); });

            var s = _profile.Stats();
            var values = new[] { s.CombatsWon, s.CombatsLost, s.ZonesCleared, s.Agents, s.TopAgentLevel, s.BestLinkWins, s.CraftedItems, s.CreditsEarned };
            var keys = new[] { "combatsWon", "combatsLost", "zones", "agents", "topLevel", "bestLink", "crafted", "credits" };
            for (int i = 0; i < keys.Length; i++)
            {
                var r = InfoRowData.Basic(Loc.T("profile.stat." + keys[i]));
                r.Tag = values[i].ToString();
                r.TagColor = Theme.Info;
                Add(r, null);
            }

            var section = InfoRowData.Basic(Loc.T("profile.friends"));
            section.Dim = true;
            Add(section, null);

            var friends = _friends.Friends();
            if (friends.Count == 0)
            {
                var none = InfoRowData.Basic(Loc.T("profile.friends_empty"), Loc.T("profile.friends_empty_sub"));
                none.Dim = true;
                Add(none, null);
            }
            foreach (var f in friends)
            {
                var friend = f;
                var r = InfoRowData.Basic(friend.Name, Loc.F("profile.friend_pending", friend.Tag));
                r.Tag = CollectionText.Tag(friend.Name);
                r.TagColor = Theme.Perception;
                r.A = RowButtonData.Of(Loc.T("profile.friend_remove"), true, "", 3);
                Add(r, () => { _friends.Remove(friend.Tag); Toasts.Show(Loc.T("profile.friend_removed"), ToastKind.Reward); });
            }
            View.ShowRows(rows.ToArray());
        }

        /// <summary>Bandeau « Compte » : invité (Lier mon compte) ou connecté (e-mail, fournisseur, dernière synchronisation, Synchroniser, Se déconnecter).</summary>
        void AddAccountRow(Action<InfoRowData, Action?, Action?> add)
        {
            var cloud = _account!.Cloud;
            if (cloud == null)
            {
                var guest = InfoRowData.Basic(Loc.T("profile.account.guest"), Loc.T("profile.account.guest_sub"));
                guest.Tag = "C";
                guest.TagColor = Theme.Info;
                guest.A = RowButtonData.Of(Loc.T("profile.account.link"), true, "", 0);
                add(guest, () => RunAccountAsync(() => _accountFlow!.RunAsync(Lifetime)).Forget(), null);
                return;
            }
            string provider = Loc.T("account.provider." + cloud.Provider.ToString().ToLowerInvariant());
            var last = _account.LastSyncUtc;
            string when = last.HasValue ? last.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : Loc.T("profile.account.never");
            var row = InfoRowData.Basic(Loc.F("profile.account.connected", string.IsNullOrEmpty(cloud.Email) ? cloud.DisplayName : cloud.Email), Loc.F("profile.account.connected_sub", provider, when));
            row.Tag = "C";
            row.TagColor = Theme.Success;
            row.A = RowButtonData.Of(Loc.T("profile.account.sync"), true, "", 0);
            row.B = RowButtonData.Of(Loc.T("profile.account.signout"), true, "", 1);
            add(row, () => RunAccountAsync(() => _accountFlow!.SyncNowAsync(Lifetime)).Forget(), () => RunAccountAsync(() => _accountFlow!.SignOutAsync(Lifetime)).Forget());
        }

        async UniTaskVoid RunAccountAsync(Func<UniTask> action)
        {
            try { await action(); }
            catch (OperationCanceledException) { return; }
            if (!Disposed) Render();
        }

        InfoRowData NavRow(string titleKey, string subKey, string tag)
        {
            var r = InfoRowData.Basic(Loc.T(titleKey), Loc.T(subKey));
            r.Tag = tag;
            r.TagColor = Theme.Primary;
            r.A = RowButtonData.Of(Loc.T("profile.open"), true, "", 0);
            return r;
        }

        protected override void OnHeader(int index)
        {
            if (_achievementsOpen) { _achievementsOpen = false; Refresh(); return; }
            if (!IsConnected) return; // hors ligne : pas de tag à copier, pas d'ami à ajouter
            if (index == 0) CopyTag();
            else AddFriendAsync().Forget();
        }

        /// <summary>Compte créé ou lié : seul cas où le joueur a un tag partageable.</summary>
        bool IsConnected => _account?.State == AccountState.Connected;

        /// <summary>Liste complète des succès : débloqué (avec son jour) ou verrouillé (avec la progression).</summary>
        void RenderAchievements()
        {
            var list = _profile.Achievements().List();
            int done = list.Count(a => a.Unlocked);
            View.ShowHeader(new ListHeaderData
            {
                Title = Loc.T("ach.title"),
                Line = Loc.F("ach.line", done, list.Count),
                Progress = list.Count == 0 ? -1f : done / (float)list.Count,
                ProgressLabel = done + " / " + list.Count,
                A = RowButtonData.Of(Loc.T("ach.back"), true, "", 0),
            });
            _actions.Clear();
            _actionsB.Clear();
            var rows = new List<InfoRowData>();
            foreach (var a in list)
            {
                var r = InfoRowData.Basic(Loc.T(a.Def.NameKey), Loc.T(a.Def.DescKey));
                r.Tag = a.Unlocked ? "OK" : "--";
                r.TagColor = a.Unlocked ? Theme.Success : Theme.Surface3;
                r.Badge = a.Unlocked ? a.UnlockedDay : Loc.F("ach.progress", a.Progress, a.Def.Target);
                r.Progress = a.Def.Target == 0 ? -1f : a.Progress / (float)a.Def.Target;
                r.ProgressLabel = a.Progress + " / " + a.Def.Target;
                r.Dim = !a.Unlocked;
                rows.Add(r);
                _actions.Add(null);
                _actionsB.Add(null);
            }
            View.ShowRows(rows.ToArray());
        }

        /// <summary>Ouvre la saisie du tag ; un tag valide s'ajoute à la liste, une erreur s'affiche, annuler ne fait rien.</summary>
        async UniTaskVoid AddFriendAsync()
        {
            var view = await _modals.OpenAsync<IFriendAddPopupView>(Lifetime);
            string? tag;
            try { tag = await view.AskAsync(Loc.T("profile.add_title"), Loc.T("profile.add_hint"), Loc.T("profile.add_ok"), Loc.T("ui.cancel"), Lifetime); }
            finally { _modals.Close(view); }
            if (tag == null) return;
            Report(_friends.Add(tag), Loc.T("profile.friend_added"));
        }

        void CopyTag()
        {
            GUIUtility.systemCopyBuffer = _profile.Tag;
            Toasts.Show(Loc.T("profile.copied"), ToastKind.Reward);
        }

        protected override void OnRowButton(int row, int button)
        {
            var list = button == 1 ? _actionsB : _actions;
            if (row >= 0 && row < list.Count) list[row]?.Invoke();
        }
    }
}
