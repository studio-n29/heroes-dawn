#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Page QG (UI Toolkit) : bandeau de quête principale, tuiles de pièces illustrées (cadenas sur les pièces verrouillées),
    /// activités du jour en liste à cocher avec leurs icônes d'alerte, carte de la prochaine mission, MISSION.
    /// </summary>
    public sealed class QgPageView : HdView, IQgPageView
    {
        const int MaxRooms = 5;

        readonly HubPage[] _pages = new HubPage[MaxRooms];
        readonly UnityEngine.UIElements.Button[] _rooms = new UnityEngine.UIElements.Button[MaxRooms];
        readonly HdButtonRef[] _roomRefs = new HdButtonRef[MaxRooms];
        readonly System.Collections.Generic.List<HdTutorialAnchor> _anchors = new System.Collections.Generic.List<HdTutorialAnchor>();
        Label _bannerTitle = null!, _bannerLine = null!, _dailyCount = null!, _dailyBody = null!, _dailyBadge = null!, _shopBadge = null!, _hint = null!, _sub = null!;
        UnityEngine.UIElements.Button _mission = null!, _banner = null!;
        VisualElement _dailyBadgeBox = null!, _shopBadgeBox = null!, _secondaryDot = null!, _roomsRow = null!, _dailyList = null!;
        /// <summary>Séparateur entre le libellé d'une activité et son compteur (tiré du gabarit de texte « qg.activity.line »).</summary>
        string _activitySeparator = " · ";
        string _activityDone = "";
        QgPageData? _last;

        public event Action<HubPage>? RoomClicked;
        public event Action? MissionClicked;
        public event Action? DailyClicked;
        public event Action? SecondaryClicked;
        public event Action? ShopAlertClicked;
        public event Action? QuestsClicked;
        public event Action<HubPage>? SubTabClicked;

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
            _secondaryDot = Q<VisualElement>("secondaryDot"); _roomsRow = Q<VisualElement>("rooms"); _dailyList = Q<VisualElement>("dailyList");
            MissionButton = new HdButtonRef(_mission, this); QuestsButton = new HdButtonRef(_banner, this);

            Q<UnityEngine.UIElements.Button>("tabHq").text = Loc.T("qg.tab.hq");
            Q<UnityEngine.UIElements.Button>("tabHq").SetEnabled(false);
            var tabTournament = Q<UnityEngine.UIElements.Button>("tabTournament");
            tabTournament.text = Loc.T("qg.tab.tournaments");
            tabTournament.clicked += () => SubTabClicked?.Invoke(HubPage.Tournament);
            var tabLair = Q<UnityEngine.UIElements.Button>("tabLair");
            tabLair.text = Loc.T("qg.tab.lairs");
            tabLair.clicked += () => SubTabClicked?.Invoke(HubPage.Lair);
            Q<Label>("roomsTitle").text = Loc.T("qg.rooms");
            Q<Label>("dailyTitle").text = Loc.T("qg.daily");
            Q<Label>("shopAlertLabel").text = Loc.T("nav.shop");
            Q<Label>("secondaryLabel").text = Loc.T("qg.daily");
            _mission.text = Loc.T("qg.mission");
            // Le gabarit « libellé · compteur » donne le séparateur : la liste à cocher sépare les deux sans supposer la langue.
            string probe = Loc.F("qg.activity.line", "\u0001", "\u0002");
            int labelAt = probe.IndexOf('\u0001'), countAt = probe.IndexOf('\u0002');
            if (labelAt >= 0 && countAt > labelAt + 1) _activitySeparator = probe.Substring(labelAt + 1, countAt - labelAt - 1);
            _activityDone = Loc.T("quests.state.done");
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
                room.EnableInClassList("hd-room--locked", r.Locked);
                var art = Passive("hd-room__art");
                if (r.Art != null) art.style.backgroundImage = new StyleBackground(r.Art);
                room.Add(art);
                room.Add(Passive("hd-room__shade"));
                // Pièce verrouillée : cadenas sur l'illustration atténuée ; la condition reste lisible sous le titre.
                room.Add(Passive("hd-room__lock"));
                var caption = Passive("hd-room__caption");
                var icon = Passive("hd-room__icon");
                icon.AddToClassList(RoomIcon(r.Page));
                caption.Add(icon);
                var text = Passive("hd-room__text");
                var title = new Label(r.Title) { pickingMode = PickingMode.Ignore };
                title.AddToClassList("hd-room__title");
                text.Add(title);
                var info = new Label(r.Info) { pickingMode = PickingMode.Ignore };
                info.AddToClassList("hd-room__info");
                text.Add(info);
                caption.Add(text);
                room.Add(caption);
                room.Add(Passive("hd-room__frame"));
                if (r.Badge > 0)
                {
                    var badge = Passive("hd-badge");
                    badge.Add(new Label(r.Badge.ToString()) { pickingMode = PickingMode.Ignore });
                    room.Add(badge);
                }
                room.SetEnabled(!r.Locked);
            }
            ShowActivities(d.DailyLines);
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

        /// <summary>
        /// Activités du jour en liste à cocher : case, libellé, compteur à droite (« 0 / 2 ») ; une activité terminée montre la case cochée.
        /// Une ligne sans compteur (aucune activité pour l'instant) reste un texte simple sous la liste.
        /// </summary>
        void ShowActivities(string[]? lines)
        {
            _dailyList.Clear();
            string plain = "";
            if (lines != null)
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    int cut = line.LastIndexOf(_activitySeparator, StringComparison.Ordinal);
                    if (cut <= 0)
                    {
                        plain = plain.Length == 0 ? line : plain + "\n" + line;
                        continue;
                    }
                    string count = line.Substring(cut + _activitySeparator.Length);
                    bool done = count == _activityDone;
                    var row = Passive("hd-qg__act");
                    row.EnableInClassList("hd-qg__act--done", done);
                    row.Add(Passive("hd-qg__act-box"));
                    var label = new Label(line.Substring(0, cut)) { pickingMode = PickingMode.Ignore };
                    label.AddToClassList("hd-qg__act-label");
                    row.Add(label);
                    if (!done)
                    {
                        var counter = new Label(count) { pickingMode = PickingMode.Ignore };
                        counter.AddToClassList("hd-qg__act-count");
                        row.Add(counter);
                    }
                    _dailyList.Add(row);
                }
            _dailyList.style.display = _dailyList.childCount > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _dailyBody.text = plain;
            _dailyBody.style.display = plain.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        static VisualElement Passive(string ussClass)
        {
            var e = new VisualElement { pickingMode = PickingMode.Ignore };
            e.AddToClassList(ussClass);
            return e;
        }

        /// <summary>Icône du kit (classe USS) de chaque pièce.</summary>
        static string RoomIcon(HubPage page)
        {
            switch (page)
            {
                case HubPage.Lab: return "hd-room__icon--workshop";
                case HubPage.Operations: return "hd-room__icon--operations";
                case HubPage.Grimoire: return "hd-room__icon--grimoire";
                case HubPage.Inventory: return "hd-room__icon--inventory";
                case HubPage.Agents: return "hd-room__icon--register";
                default: return "hd-room__icon--default";
            }
        }
    }
}
