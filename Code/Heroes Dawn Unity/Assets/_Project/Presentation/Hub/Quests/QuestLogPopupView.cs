#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Suivi des quêtes (popup prefab) : quêtes principales de la campagne, puis quêtes secondaires à réclamer. Retour = Fermer.</summary>
    public sealed class QuestLogPopupView : MonoBehaviour, IQuestLogPopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text mainTitle = null!;
        [SerializeField] TMP_Text sideTitle = null!;
        [SerializeField] TMP_Text sideEmpty = null!;
        [SerializeField] RectTransform mainList = null!;
        [SerializeField] RectTransform sideList = null!;
        [SerializeField] QuestMainRowView mainRowPrefab = null!;
        [SerializeField] QuestSideRowView sideRowPrefab = null!;
        [SerializeField] UiButton close = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;

        readonly List<QuestMainRowView> _main = new List<QuestMainRowView>();
        readonly List<QuestSideRowView> _side = new List<QuestSideRowView>();
        UniTaskCompletionSource? _done;

        public bool Cancelable => true;
        public event Action<string>? ClaimClicked;
        public IReadOnlyList<QuestMainRowView> MainRows => _main;
        public IReadOnlyList<QuestSideRowView> SideRows => _side;
        public UiButton CloseButton => close;

        void Awake() => close.Button.onClick.AddListener(() => _done?.TrySetResult());

        public UniTask ShowAsync(QuestLogData d, CancellationToken ct)
        {
            Refresh(d);
            close.SetLabel(d.Close);
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }

        public void Refresh(QuestLogData d)
        {
            title.text = d.Title;
            mainTitle.text = d.MainTitle;
            sideTitle.text = d.SideTitle;
            mainList.Clear();
            _main.Clear();
            foreach (var m in d.Main)
            {
                var row = Instantiate(mainRowPrefab, mainList);
                row.Show(m);
                _main.Add(row);
            }
            sideList.Clear();
            _side.Clear();
            foreach (var s in d.Side)
            {
                var row = Instantiate(sideRowPrefab, sideList);
                row.Show(s);
                row.Claimed += id => ClaimClicked?.Invoke(id);
                _side.Add(row);
            }
            sideEmpty.text = d.SideEmpty;
            sideEmpty.gameObject.SetActive(d.Side.Length == 0);
        }

        public void RequestCancel() => _done?.TrySetResult();
    }
}
