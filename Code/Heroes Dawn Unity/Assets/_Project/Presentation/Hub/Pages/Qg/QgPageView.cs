#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page QG (prefab Pages/Qg) : pièces illustrées, activités du jour, icônes d'alerte, quête principale, MISSION.</summary>
    public sealed class QgPageView : ViewBase, IQgPageView
    {
        [SerializeField] RoomCardView[] rooms = Array.Empty<RoomCardView>();
        [SerializeField] Button dailyCard = null!;
        [SerializeField] TMP_Text dailyBody = null!;
        [SerializeField] TMP_Text dailyBadge = null!;
        [SerializeField] Button secondaryCard = null!;
        [SerializeField] Button shopAlertButton = null!;
        [SerializeField] TMP_Text shopAlertBadge = null!;
        [SerializeField] GameObject secondaryDot = null!;
        [SerializeField] TMP_Text filSpeaker = null!;
        [SerializeField] TMP_Text filText = null!;
        [SerializeField] Button filCard = null!;
        [SerializeField] TMP_Text questsBadge = null!;
        [SerializeField] UiButton mission = null!;
        [SerializeField] TMP_Text missionSub = null!;
        [SerializeField] TMP_Text missionHint = null!;

        readonly HubPage[] _pages = new HubPage[8];
        bool _cascaded;

        public event Action<HubPage>? RoomClicked;
        public event Action? MissionClicked;
        public event Action? DailyClicked;
        public event Action? SecondaryClicked;
        public event Action? ShopAlertClicked;
        public event Action? QuestsClicked;

        /// <summary>Bouton MISSION (tests PlayMode : clic réel).</summary>
        public Button MissionButton => mission.Button;

        /// <summary>Carte « suivi des quêtes » (tests PlayMode : clic réel).</summary>
        public Button QuestsCard => filCard;

        /// <summary>Bouton d'une pièce (tests PlayMode), selon le dernier affichage.</summary>
        public Button? RoomButton(HubPage page)
        {
            for (int i = 0; i < rooms.Length; i++) if (_pages[i] == page) return rooms[i].Button.Button;
            return null;
        }

        void Awake()
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                int index = i;
                rooms[i].Clicked += () => RoomClicked?.Invoke(_pages[index]);
            }
            mission.Button.onClick.AddListener(() => MissionClicked?.Invoke());
            dailyCard.onClick.AddListener(() => DailyClicked?.Invoke());
            secondaryCard.onClick.AddListener(() => SecondaryClicked?.Invoke());
            shopAlertButton.onClick.AddListener(() => ShopAlertClicked?.Invoke());
            filCard.onClick.AddListener(() => QuestsClicked?.Invoke());
        }

        public void Show(QgPageData d)
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                bool used = i < d.Rooms.Length;
                if (rooms[i].gameObject.activeSelf != used) rooms[i].gameObject.SetActive(used);
                if (!used) continue;
                _pages[i] = d.Rooms[i].Page;
                rooms[i].Show(d.Rooms[i]);
                if (!_cascaded) Fx.PopInAfter(rooms[i].transform, Stagger.DelayFor(i));
            }
            dailyBody.text = string.Join("\n", d.DailyLines);
            dailyBadge.text = d.DailyBadge;
            dailyBadge.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(d.DailyBadge));
            secondaryDot.SetActive(d.SecondaryActive);
            shopAlertBadge.text = d.ShopAlertBadge;
            shopAlertBadge.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(d.ShopAlertBadge));
            filSpeaker.text = d.FilSpeaker;
            filText.text = d.FilText;
            questsBadge.text = d.QuestsBadge;
            questsBadge.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(d.QuestsBadge));
            mission.SetLabel(d.MissionTitle);
            mission.Interactable = d.MissionEnabled;
            mission.DisabledReason = d.MissionHint;
            missionSub.text = d.MissionSub;
            missionHint.text = d.MissionHint;
            _cascaded = true;
        }
    }
}
