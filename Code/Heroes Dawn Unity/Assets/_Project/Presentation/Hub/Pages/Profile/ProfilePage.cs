#nullable enable
using System;
using System.Collections.Generic;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
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
        readonly List<Action?> _actions = new List<Action?>();

        public ProfilePagePresenter(IProfilePageView view, PlayerProfile profile, IFriendsPort friends, HubNavigator nav, ISessionReadModel session,
                                    ILocalization loc, IToastService toasts, ISubscriber<MetaChanged> meta, ISubscriber<WalletChanged> wallet,
                                    ISubscriber<RosterChanged> roster, IClock clock)
            : base(view, loc, toasts, meta, wallet, roster, clock)
        {
            _profile = profile; _friends = friends; _nav = nav; _session = session;
        }

        public override string Title => Loc.T("page.profile");

        protected override void Render()
        {
            View.ShowHeader(new ListHeaderData
            {
                Title = _profile.Tag,
                Line = Loc.F("profile.header_line", _profile.Pseudo, _session.Route.HeroName, _profile.PassLevel),
                Progress = -1f,
                A = RowButtonData.Of(Loc.T("profile.copy"), true, "", 0),
                B = RowButtonData.Of(Loc.T("profile.add"), true, "", 1),
            });

            _actions.Clear();
            var rows = new List<InfoRowData>();
            void Add(InfoRowData row, Action? action) { rows.Add(row); _actions.Add(action); }

            Add(NavRow("profile.registry", "profile.registry.sub", "R"), () => _nav.Open(HubPage.Agents));
            Add(NavRow("profile.archives", "profile.archives.sub", "A"), () => _nav.Open(HubPage.Archives));

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
            if (index == 0) CopyTag();
            else AddFriendRequested?.Invoke();
        }

        /// <summary>Le bouton « Ajouter un ami » a été touché (la saisie est ouverte par l'étape suivante du lot).</summary>
        public event Action? AddFriendRequested;

        void CopyTag()
        {
            GUIUtility.systemCopyBuffer = _profile.Tag;
            Toasts.Show(Loc.T("profile.copied"), ToastKind.Reward);
        }

        protected override void OnRowButton(int row, int button)
        {
            if (row >= 0 && row < _actions.Count) _actions[row]?.Invoke();
        }
    }
}
