#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
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
        [SerializeField] UiButton reroll = null!;

        readonly int[] _slots = new int[8];

        public event Action<int>? CardTapped;
        public event Action<int>? RecruitClicked;
        public event Action? RerollClicked;

        public Button RerollButton => reroll.Button;
        public ShopCardView Card(int index) => cards[index];

        void Awake()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                int index = i;
                cards[i].Tapped += () => CardTapped?.Invoke(_slots[index]);
                cards[i].RecruitClicked += () => RecruitClicked?.Invoke(_slots[index]);
            }
            reroll.Button.onClick.AddListener(() => RerollClicked?.Invoke());
        }

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
                cards[i].Show(d.Cards[i]);
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
