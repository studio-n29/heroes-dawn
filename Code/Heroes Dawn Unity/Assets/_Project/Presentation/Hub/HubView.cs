#nullable enable
using System;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>QG (prefab Screens/Hub) : cartes dans une grille défilante, salle des opérations à droite.</summary>
    public sealed class HubView : ViewBase, IHubView
    {
        [SerializeField] TMP_Text player = null!;
        [SerializeField] TMP_Text hint = null!;
        [SerializeField] TMP_Text emptyRecruits = null!;
        [SerializeField] TMP_Text zoneStatus = null!;
        [SerializeField] TMP_Text squadTitle = null!;
        [SerializeField] TMP_Text deployHint = null!;
        [SerializeField] UiButton tabTeam = null!;
        [SerializeField] UiButton tabRecruit = null!;
        [SerializeField] UiButton logout = null!;
        [SerializeField] UiButton reset = null!;
        [SerializeField] UiButton heal = null!;
        [SerializeField] UiButton deploy = null!;
        [SerializeField] GameObject shopBar = null!;
        [SerializeField] TMP_Text shopCountdown = null!;
        [SerializeField] UiButton reroll = null!;
        [SerializeField] RectTransform grid = null!;
        [SerializeField] ScrollRect scroll = null!;
        [SerializeField] RectTransform objectives = null!;
        [SerializeField] RectTransform squadRow = null!;
        [SerializeField] WalletView wallet = null!;
        [SerializeField] TeamCardView teamCardPrefab = null!;
        [SerializeField] RecruitCardView recruitCardPrefab = null!;
        [SerializeField] SquadSlotView squadSlotPrefab = null!;
        [SerializeField] TextLineView objectiveLinePrefab = null!;

        public event Action<HubTab>? TabClicked;
        public event Action? LogoutClicked;
        public event Action? ResetClicked;
        public event Action? HealClicked;
        public event Action? DeployClicked;
        public event Action<MemberId>? SquadToggleClicked;
        public event Action<MemberId>? TrainClicked;
        public event Action<int>? RecruitClicked;
        public event Action? RerollClicked;

        public IWalletView Wallet => wallet;

        /// <summary>Bouton Déployer (tests PlayMode : clic réel).</summary>
        public Button DeployButton => deploy.Button;
        public Button RecruitTabButton => tabRecruit.Button;
        public Button RerollButton => reroll.Button;

        void Awake()
        {
            tabTeam.Button.onClick.AddListener(() => TabClicked?.Invoke(HubTab.Team));
            tabRecruit.Button.onClick.AddListener(() => TabClicked?.Invoke(HubTab.Recruit));
            logout.Button.onClick.AddListener(() => LogoutClicked?.Invoke());
            reset.Button.onClick.AddListener(() => ResetClicked?.Invoke());
            heal.Button.onClick.AddListener(() => HealClicked?.Invoke());
            deploy.Button.onClick.AddListener(() => DeployClicked?.Invoke());
            reroll.Button.onClick.AddListener(() => RerollClicked?.Invoke());
        }

        public void Show(HubData d)
        {
            player.text = d.PlayerLine;
            hint.text = d.Hint;
            bool team = d.Tab == HubTab.Team;
            tabTeam.SetColors(team ? Theme.Primary : Theme.Surface3, team ? Theme.TextDark : Theme.Text);
            tabRecruit.SetColors(!team ? Theme.Primary : Theme.Surface3, !team ? Theme.TextDark : Theme.Text);

            grid.Clear();
            if (team)
            {
                foreach (var c in d.Team)
                {
                    var card = Instantiate(teamCardPrefab, grid);
                    var id = c.Card.Id;
                    card.SquadClicked += () => SquadToggleClicked?.Invoke(id);
                    card.TrainClicked += () => TrainClicked?.Invoke(id);
                    card.Show(c);
                }
            }
            else
            {
                foreach (var c in d.Recruits)
                {
                    var card = Instantiate(recruitCardPrefab, grid);
                    int slot = c.Slot;
                    card.RecruitClicked += () => RecruitClicked?.Invoke(slot);
                    card.Show(c);
                }
            }
            emptyRecruits.text = d.EmptyRecruitText;
            emptyRecruits.gameObject.SetActive(!team && d.Recruits.Length == 0);
            if (shopBar.activeSelf == team) shopBar.SetActive(!team);
            shopCountdown.text = d.ShopCountdown;
            reroll.SetLabel(d.RerollLabel);
            reroll.Interactable = d.RerollEnabled;

            objectives.Clear();
            foreach (var o in d.ZoneObjectives) Instantiate(objectiveLinePrefab, objectives).Show(o, Theme.Text);
            zoneStatus.text = d.ZoneStatus;
            zoneStatus.color = d.ZoneDone ? Theme.Success : Theme.TextMuted;

            squadTitle.text = d.SquadTitle;
            squadRow.Clear();
            foreach (var s in d.Squad) Instantiate(squadSlotPrefab, squadRow).Show(s);

            heal.SetLabel(d.HealLabel);
            heal.Interactable = d.HealEnabled;
            deploy.Interactable = d.DeployEnabled;
            deployHint.text = d.DeployHint;
        }

        public void ScrollToTop() => scroll.verticalNormalizedPosition = 1f;
    }
}
