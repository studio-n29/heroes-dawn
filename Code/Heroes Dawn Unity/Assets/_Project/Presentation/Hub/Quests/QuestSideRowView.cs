#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ligne d'une quête secondaire (widget prefab) : titre, description, barre de progression, récompense, bouton.</summary>
    public sealed class QuestSideRowView : MonoBehaviour
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text description = null!;
        [SerializeField] UiBar bar = null!;
        [SerializeField] TMP_Text reward = null!;
        [SerializeField] UiButton claim = null!;

        public string Id { get; private set; } = "";
        public SideQuestRowData Data { get; private set; }
        public event Action<string>? Claimed;
        public Button ClaimButton => claim.Button;

        void Awake() => claim.Button.onClick.AddListener(() => Claimed?.Invoke(Id));

        public void Show(SideQuestRowData d)
        {
            Data = d;
            Id = d.Id;
            title.text = d.Title;
            description.text = d.Description;
            bar.Set(d.Progress01, instant: true);
            bar.SetLabel(d.ProgressText);
            reward.text = d.RewardText;
            claim.SetLabel(d.ClaimLabel);
            claim.Interactable = d.Claimable;
            claim.gameObject.SetActive(!d.Claimed);
            bar.SetColor(d.Claimed ? Theme.Success : Theme.Primary);
        }
    }
}
