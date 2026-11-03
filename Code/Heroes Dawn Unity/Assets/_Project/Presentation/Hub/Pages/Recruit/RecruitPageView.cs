#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Page Recruter (prefab Pages/Recruit) : règle, compte à rebours, 3 cartes, progression, Renouveler.</summary>
    public sealed class RecruitPageView : ViewBase, IRecruitPageView
    {
        [SerializeField] TMP_Text rule = null!;
        [SerializeField] TMP_Text countdown = null!;
        [SerializeField] ShopCardView[] cards = Array.Empty<ShopCardView>();
        [SerializeField] TMP_Text empty = null!;
        [SerializeField] TMP_Text progress = null!;
        [SerializeField] UiButton[] scraps = Array.Empty<UiButton>();
        [SerializeField] UiButton reroll = null!;
        [SerializeField] UiButton registry = null!;
        [SerializeField] ShopTabsView shopTabs = null!;

        readonly int[] _slots = new int[8];
        readonly Core.MemberId[] _ids = new Core.MemberId[8];
        readonly bool[] _shown = new bool[8];
        readonly int[] _scrapSlots = new int[6];

        public event Action<int>? CardTapped;
        public event Action<int>? RecruitClicked;
        public event Action? RerollClicked;
        public event Action<int>? ScrapClicked;
        public event Action? RegistryClicked;
        public event Action<int>? ShopTabClicked;

        public Button RerollButton => reroll.Button;
        public ShopCardView Card(int index) => cards[index];
        /// <summary>Bandeau d'onglets de la boutique (tests PlayMode).</summary>
        public ShopTabsView ShopTabs => shopTabs;

        void Awake()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                int index = i;
                cards[i].Tapped += () => CardTapped?.Invoke(_slots[index]);
                cards[i].RecruitClicked += () => RecruitClicked?.Invoke(_slots[index]);
            }
            for (int i = 0; i < scraps.Length; i++)
            {
                int index = i;
                scraps[i].Button.onClick.AddListener(() => ScrapClicked?.Invoke(_scrapSlots[index]));
            }
            reroll.Button.onClick.AddListener(() => RerollClicked?.Invoke());
            registry.Button.onClick.AddListener(() => RegistryClicked?.Invoke());
            shopTabs.TabClicked += i => ShopTabClicked?.Invoke(i);
        }

        public void ShowShopTab(int active) => shopTabs.Show(active);

        public void Show(RecruitPageData d)
        {
            rule.text = d.Rule;
            countdown.text = d.Countdown;
            for (int i = 0; i < cards.Length; i++)
            {
                bool used = i < d.Cards.Length;
                if (cards[i].gameObject.activeSelf != used) cards[i].gameObject.SetActive(used);
                if (!used) continue;
                _slots[i] = d.Cards[i].Slot;
                // Feedbacks : cascade à la première ouverture, retournement quand l'offre change (renouvellement, 9h00).
                var id = d.Cards[i].Card.Id;
                bool changed = _shown[i] && !_ids[i].Equals(id);
                if (changed) cards[i].ResetState();
                cards[i].Show(d.Cards[i]);
                if (!_shown[i]) Fx.PopInAfter(cards[i].transform, Stagger.DelayFor(i));
                else if (changed) Fx.FlipIn(cards[i].transform, Stagger.DelayFor(i, 0.08f));
                _ids[i] = id;
                _shown[i] = true;
            }
            for (int i = 0; i < scraps.Length; i++)
            {
                bool has = i < d.Scraps.Length;
                if (scraps[i].gameObject.activeSelf != has) scraps[i].gameObject.SetActive(has);
                if (!has) continue;
                _scrapSlots[i] = d.Scraps[i].Slot;
                scraps[i].SetLabel(d.Scraps[i].Text);
                scraps[i].Interactable = d.Scraps[i].Enabled;
            }
            empty.text = d.EmptyText;
            bool none = d.Cards.Length == 0;
            if (empty.gameObject.activeSelf != none) empty.gameObject.SetActive(none);
            progress.text = d.Progress;
            reroll.SetLabel(d.RerollLabel);
            reroll.Interactable = d.RerollEnabled;
        }
    }
}
