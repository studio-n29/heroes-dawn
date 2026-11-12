#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page QG (UI Toolkit) : pièces illustrées, activités du jour, icônes d'alerte, quête principale, MISSION.</summary>
    public sealed class QgPageView : HdView, IQgPageView
    {
        const int MaxRooms = 5;

        readonly HubPage[] _pages = new HubPage[MaxRooms];
        readonly UnityEngine.UIElements.Button[] _rooms = new UnityEngine.UIElements.Button[MaxRooms];
        readonly HdButtonRef[] _roomRefs = new HdButtonRef[MaxRooms];
        readonly System.Collections.Generic.List<HdTutorialAnchor> _anchors = new System.Collections.Generic.List<HdTutorialAnchor>();
        Label _bannerTitle = null!, _bannerLine = null!, _dailyCount = null!, _dailyBody = null!, _dailyBadge = null!, _shopBadge = null!, _hint = null!, _sub = null!;
        UnityEngine.UIElements.Button _mission = null!, _banner = null!;
        VisualElement _dailyBadgeBox = null!, _shopBadgeBox = null!, _secondaryDot = null!, _roomsRow = null!;
        QgPageData? _last;

        public event Action<HubPage>? RoomClicked;
        public event Action? MissionClicked;
        public event Action? DailyClicked;
        public event Action? SecondaryClicked;
        public event Action? ShopAlertClicked;
        public event Action? QuestsClicked;

        /// <summary>Bouton MISSION (tests PlayMode : clic réel).</summary>
        public HdButtonRef MissionButton { get; private set; } = null!;
        public HdButtonRef QuestsButton { get; private set; } = null!;

        /// <summary>Bouton d'une pièce (tests PlayMode), selon le dernier affichage.</summary>
        public HdButtonRef? RoomButton(HubPage page)
        {
            for (int i = 0; i < MaxRooms; i++) if (_roomRefs[i] != null && _pages[i] == page && _rooms[i].resolvedStyle.display != DisplayStyle.None) return _roomRefs[i];
            return null;
        }

        protected override void Bind()
        {
            _bannerTitle = Q<Label>("bannerTitle"); _bannerLine = Q<Label>("bannerLine");
            _dailyCount = Q<Label>("dailyCount"); _dailyBody = Q<Label>("dailyBody"); _dailyBadge = Q<Label>("dailyBadge");
            _shopBadge = Q<Label>("shopAlertBadge"); _hint = Q<Label>("missionHint"); _sub = Q<Label>("missionSub");
            _mission = Q<UnityEngine.UIElements.Button>("mission"); _banner = Q<UnityEngine.UIElements.Button>("banner");
            _dailyBadgeBox = Q<VisualElement>("dailyBadgeBox"); _shopBadgeBox = Q<VisualElement>("shopAlertBadgeBox");
            _secondaryDot = Q<VisualElement>("secondaryDot"); _roomsRow = Q<VisualElement>("rooms");
            MissionButton = new HdButtonRef(_mission, this); QuestsButton = new HdButtonRef(_banner, this);

            Q<Label>("roomsTitle").text = Loc.T("qg.rooms");
            Q<Label>("dailyTitle").text = Loc.T("qg.daily");
            Q<Label>("shopAlertLabel").text = Loc.T("nav.shop");
            Q<Label>("secondaryLabel").text = Loc.T("qg.daily");
            _mission.text = Loc.T("qg.mission");
            var nextArt = TacticalArt.Load("combat-red-hook");
            if (nextArt != null) Q<VisualElement>("nextArt").style.backgroundImage = new StyleBackground(nextArt);

            _mission.clicked += () => MissionClicked?.Invoke();
            _banner.clicked += () => QuestsClicked?.Invoke();
            Q<UnityEngine.UIElements.Button>("daily").clicked += () => DailyClicked?.Invoke();
            Q<UnityEngine.UIElements.Button>("secondary").clicked += () => SecondaryClicked?.Invoke();
            Q<UnityEngine.UIElements.Button>("shopAlert").clicked += () => ShopAlertClicked?.Invoke();

            _roomsRow.Clear();
            for (int i = 0; i < MaxRooms; i++)
            {
                int index = i;
                var room = new UnityEngine.UIElements.Button();
                room.AddToClassList("hd-room");
                room.clicked += () => RoomClicked?.Invoke(_pages[index]);
                _rooms[i] = room;
                _roomRefs[i] = new HdButtonRef(room, this);
                _roomsRow.Add(room);
            }

            foreach (var old in _anchors) if (old != null) Destroy(old.gameObject);
            _anchors.Clear();
            _anchors.Add(HdTutorialAnchor.Attach(this, Q<VisualElement>("page"), TutorialTarget.QgPage));
            _anchors.Add(HdTutorialAnchor.Attach(this, _mission, TutorialTarget.QgMission));
            if (_last.HasValue) Show(_last.Value);   // le document est recréé à chaque activation : on réaffiche la dernière donnée
        }

        public void Show(QgPageData d)
        {
            _last = d;
            for (int i = 0; i < MaxRooms; i++)
            {
                var room = _rooms[i];
                bool used = i < d.Rooms.Length;
                room.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used) continue;
                var r = d.Rooms[i];
                _pages[i] = r.Page;
                room.Clear();
                var art = new VisualElement { pickingMode = PickingMode.Ignore };
                art.AddToClassList("hd-room__art");
                if (r.Art != null) art.style.backgroundImage = new StyleBackground(r.Art);
                room.Add(art);
                var state = new Label(r.State) { pickingMode = PickingMode.Ignore };
                state.AddToClassList("hd-room__state");
                room.Add(state);
                var title = new Label(r.Title) { pickingMode = PickingMode.Ignore };
                title.AddToClassList("hd-room__title");
                room.Add(title);
                var info = new Label(r.Info) { pickingMode = PickingMode.Ignore };
                info.AddToClassList("hd-room__info");
                room.Add(info);
                if (r.Badge > 0)
                {
                    var badge = new VisualElement { pickingMode = PickingMode.Ignore };
                    badge.AddToClassList("hd-badge");
                    badge.Add(new Label(r.Badge.ToString()) { pickingMode = PickingMode.Ignore });
                    room.Add(badge);
                }
                room.SetEnabled(!r.Locked);
            }
            _dailyBody.text = string.Join("\n", d.DailyLines);
            _dailyCount.text = d.DailyBanner;
            _dailyBadge.text = d.DailyBadge;
            _dailyBadgeBox.style.display = string.IsNullOrEmpty(d.DailyBadge) ? DisplayStyle.None : DisplayStyle.Flex;
            _secondaryDot.style.display = d.SecondaryActive ? DisplayStyle.Flex : DisplayStyle.None;
            _shopBadge.text = d.ShopAlertBadge;
            _shopBadgeBox.style.display = string.IsNullOrEmpty(d.ShopAlertBadge) ? DisplayStyle.None : DisplayStyle.Flex;
            _mission.text = d.MissionTitle;
            _mission.SetEnabled(d.MissionEnabled);
            _sub.text = d.MissionSub;
            _hint.text = d.MissionHint;
            _hint.style.display = string.IsNullOrEmpty(d.MissionHint) ? DisplayStyle.None : DisplayStyle.Flex;
            _bannerTitle.text = d.MainTitle;
            _bannerLine.text = d.MissionSub;
        }
    }
}
