#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Écran Profil (UI Toolkit, planche 12.2) : agent détouré, bandeau avatar / pseudo / tag / Compte, grille de six tuiles, amis, liens,
    /// et liste des succès en recouvrement. Mise en page proportionnelle ; la classe « narrow » réduit les marges sous le format 3:2.
    /// </summary>
    public sealed class ProfilePageView : HdView, IProfilePageView
    {
        /// <summary>Rapport largeur/hauteur en dessous duquel l'écran passe en mise en page étroite.</summary>
        const float NarrowAspect = 1.5f;

        VisualElement _page = null!, _hero = null!, _tiles = null!, _account = null!, _achievements = null!;
        ScrollView _friendRows = null!, _achRows = null!;
        Label _pseudo = null!, _tagText = null!, _rank = null!, _avatarInitial = null!, _accountLabel = null!, _statsTitle = null!, _friendsTitle = null!, _friendsEmpty = null!, _achTitle = null!, _achLine = null!;
        UnityEngine.UIElements.Button _avatar = null!, _tag = null!, _accountA = null!, _accountB = null!, _addFriend = null!, _achBack = null!;
        readonly UnityEngine.UIElements.Button[] _links = new UnityEngine.UIElements.Button[3];
        ProfileScreenData? _last;

        public event Action? TagClicked;
        public event Action? AvatarClicked;
        public event Action<int>? AccountClicked;
        public event Action<int>? TileClicked;
        public event Action? AddFriendClicked;
        public event Action<int>? FriendRemoveClicked;
        public event Action<int>? FriendCopyClicked;
        public event Action<int>? LinkClicked;
        public event Action? BackClicked;

        public HdButtonRef AddFriendButton { get; private set; } = null!;
        public HdButtonRef AccountButton { get; private set; } = null!;

        protected override void Bind()
        {
            _page = Q<VisualElement>("page"); _hero = Q<VisualElement>("hero"); _tiles = Q<VisualElement>("tiles");
            _account = Q<VisualElement>("account"); _achievements = Q<VisualElement>("achievements");
            _friendRows = Q<ScrollView>("friendRows"); _achRows = Q<ScrollView>("achRows");
            _pseudo = Q<Label>("pseudo"); _tagText = Q<Label>("tagText"); _rank = Q<Label>("rank"); _avatarInitial = Q<Label>("avatarInitial");
            _accountLabel = Q<Label>("accountLabel"); _statsTitle = Q<Label>("statsTitle"); _friendsTitle = Q<Label>("friendsTitle");
            _friendsEmpty = Q<Label>("friendsEmpty"); _achTitle = Q<Label>("achTitle"); _achLine = Q<Label>("achLine");
            _avatar = Q<UnityEngine.UIElements.Button>("avatar"); _tag = Q<UnityEngine.UIElements.Button>("tag");
            _accountA = Q<UnityEngine.UIElements.Button>("accountA"); _accountB = Q<UnityEngine.UIElements.Button>("accountB");
            _addFriend = Q<UnityEngine.UIElements.Button>("addFriend"); _achBack = Q<UnityEngine.UIElements.Button>("achBack");
            AddFriendButton = new HdButtonRef(_addFriend, this); AccountButton = new HdButtonRef(_accountA, this);

            _avatar.clicked += () => AvatarClicked?.Invoke();
            _tag.clicked += () => TagClicked?.Invoke();
            _accountA.clicked += () => AccountClicked?.Invoke(0);
            _accountB.clicked += () => AccountClicked?.Invoke(1);
            _addFriend.clicked += () => AddFriendClicked?.Invoke();
            _achBack.clicked += () => BackClicked?.Invoke();
            for (int i = 0; i < _links.Length; i++)
            {
                int index = i;
                _links[i] = Q<UnityEngine.UIElements.Button>("link" + i);
                _links[i].clicked += () => LinkClicked?.Invoke(index);
            }

            Root.RegisterCallback<GeometryChangedEvent>(_ => ApplyAspect());
            ApplyAspect();
            _statsTitle.text = Loc.T("profile.stats_title");
            if (_last.HasValue) Show(_last.Value);
        }

        /// <summary>Format étroit (4:3, fenêtre réduite) : l'agent recule, le vide à gauche se réduit.</summary>
        void ApplyAspect()
        {
            var r = Root.contentRect;
            if (r.height <= 0f) return;
            _page.EnableInClassList("hd-profile--narrow", r.width / r.height < NarrowAspect);
        }

        public void Show(ProfileScreenData d)
        {
            _last = d;
            if (_pseudo == null) return;
            _pseudo.text = d.Pseudo;
            _tagText.text = d.TagLine;
            _tag.SetEnabled(d.TagCopyable);
            _rank.text = d.RankLine;
            _avatarInitial.text = string.IsNullOrEmpty(d.Pseudo) ? "?" : d.Pseudo.Substring(0, 1).ToUpperInvariant();
            var head = AgentArt.For(d.HeroId);
            _avatar.style.backgroundImage = head != null ? new StyleBackground(Background.FromSprite(head)) : new StyleBackground(StyleKeyword.None);
            _avatarInitial.style.display = head != null ? DisplayStyle.None : DisplayStyle.Flex;
            var cutout = AgentArt.Cutout(d.HeroId);
            _hero.style.backgroundImage = cutout != null ? new StyleBackground(Background.FromSprite(cutout)) : new StyleBackground(StyleKeyword.None);

            _account.style.display = d.AccountVisible ? DisplayStyle.Flex : DisplayStyle.None;
            _accountLabel.text = d.AccountLabel ?? "";
            _accountA.text = d.AccountA ?? "";
            _accountB.text = d.AccountB ?? "";
            _accountB.style.display = d.AccountBVisible ? DisplayStyle.Flex : DisplayStyle.None;

            for (int i = 0; i < _links.Length; i++) _links[i].text = d.LinkLabels != null && i < d.LinkLabels.Length ? d.LinkLabels[i] : "";

            bool achievements = d.Achievements != null;
            _achievements.style.display = achievements ? DisplayStyle.Flex : DisplayStyle.None;
            if (achievements) ShowAchievements(d);
            ShowTiles(d.Tiles);
            ShowFriends(d);
        }

        void ShowTiles(ProfileTileData[] tiles)
        {
            _tiles.Clear();
            for (int i = 0; i < tiles.Length; i++)
            {
                int index = i;
                var data = tiles[i];
                var tile = new UnityEngine.UIElements.Button();
                tile.AddToClassList("hd-tile");
                tile.AddToClassList("hd-tile--" + data.Id);
                if (data.Tappable) tile.AddToClassList("hd-tile--tap");
                tile.name = "tile-" + data.Id;
                tile.SetEnabled(data.Tappable);
                var icon = new VisualElement { pickingMode = PickingMode.Ignore };
                icon.AddToClassList("hd-tile__icon");
                tile.Add(icon);
                tile.Add(Text(data.Label, "hd-tile__label"));
                tile.Add(Text(data.Value, "hd-tile__value"));
                tile.clicked += () => TileClicked?.Invoke(index);
                _tiles.Add(tile);
            }
        }

        void ShowFriends(ProfileScreenData d)
        {
            _friendsTitle.text = d.FriendsTitle;
            _friendRows.Clear();
            for (int i = 0; i < d.Friends.Length; i++)
            {
                int index = i;
                var f = d.Friends[i];
                var row = new VisualElement();
                row.AddToClassList("hd-friend");
                var portrait = new VisualElement { pickingMode = PickingMode.Ignore };
                portrait.AddToClassList("hd-friend__portrait");
                portrait.Add(Text(CollectionText.Tag(f.Name), "hd-friend__initial"));
                row.Add(portrait);
                var text = new VisualElement { pickingMode = PickingMode.Ignore };
                text.AddToClassList("hd-friend__text");
                text.Add(Text(f.Name, "hd-friend__name"));
                text.Add(Text(f.Tag, "hd-friend__tag"));
                row.Add(text);
                var status = new VisualElement { pickingMode = PickingMode.Ignore };
                status.AddToClassList("hd-friend__status");
                var dot = new VisualElement();
                dot.AddToClassList("hd-friend__dot");
                dot.EnableInClassList("hd-friend__dot--online", f.Online);
                status.Add(dot);
                status.Add(Text(f.Status, "hd-friend__status-text"));
                row.Add(status);
                row.Add(Small(Loc.T("profile.friend_copy"), "hd-btn--secondary", () => FriendCopyClicked?.Invoke(index)));
                row.Add(Small(Loc.T("profile.friend_remove"), "hd-btn--danger", () => FriendRemoveClicked?.Invoke(index)));
                _friendRows.Add(row);
            }
            bool empty = d.Friends.Length == 0;
            _friendsEmpty.text = d.FriendsEmpty;
            _friendsEmpty.style.display = empty ? DisplayStyle.Flex : DisplayStyle.None;
            _friendRows.style.display = empty ? DisplayStyle.None : DisplayStyle.Flex;
            _addFriend.text = d.AddLabel;
            _addFriend.SetEnabled(d.CanAddFriend);
            _addFriend.tooltip = d.AddReason ?? "";
        }

        void ShowAchievements(ProfileScreenData d)
        {
            _achTitle.text = d.AchievementsTitle;
            _achLine.text = d.AchievementsLine;
            _achBack.text = d.BackLabel;
            _achRows.Clear();
            foreach (var a in d.Achievements!)
            {
                var row = new VisualElement();
                row.AddToClassList("hd-ach");
                row.EnableInClassList("hd-ach--locked", !a.Unlocked);
                var mark = new VisualElement { pickingMode = PickingMode.Ignore };
                mark.AddToClassList("hd-ach__mark");
                mark.EnableInClassList("hd-ach__mark--on", a.Unlocked);
                row.Add(mark);
                var text = new VisualElement { pickingMode = PickingMode.Ignore };
                text.AddToClassList("hd-ach__text");
                text.Add(Text(a.Title, "hd-ach__name"));
                text.Add(Text(a.Description, "hd-ach__desc"));
                row.Add(text);
                row.Add(Text(a.Progress, "hd-ach__progress"));
                _achRows.Add(row);
            }
        }

        static Label Text(string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            return l;
        }

        static UnityEngine.UIElements.Button Small(string text, string style, Action click)
        {
            var b = new UnityEngine.UIElements.Button(click) { text = text };
            b.AddToClassList("hd-btn"); b.AddToClassList(style); b.AddToClassList("hd-btn--small"); b.AddToClassList("hd-friend__btn");
            return b;
        }
    }
}
