#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ligne d'une quête principale (widget prefab) : pastille d'état, titre, détail.</summary>
    public sealed class QuestMainRowView : MonoBehaviour
    {
        [SerializeField] Image accent = null!;
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text detail = null!;
        [SerializeField] TMP_Text state = null!;

        public int Status { get; private set; }
        public string Title => title.text;

        public void Show(MainQuestRowData d)
        {
            Status = d.Status;
            title.text = d.Title;
            detail.text = d.Detail;
            state.text = d.State;
            var c = d.Status == 2 ? Theme.Success : d.Status == 1 ? Theme.Primary : Theme.TextMuted;
            accent.color = c;
            state.color = c;
            title.color = d.Status == 0 ? Theme.TextMuted : Theme.Navy;
        }
    }
}
