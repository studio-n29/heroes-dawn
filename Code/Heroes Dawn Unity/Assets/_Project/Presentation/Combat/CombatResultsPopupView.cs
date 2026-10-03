#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Résultats du combat (popup prefab) : récompense, fatigue, vérification, Continuer. Retour = Continuer.</summary>
    public sealed class CombatResultsPopupView : PopupViewBase, ICombatResultsPopupView
    {
        [SerializeField] Image titleBand = null!;
        [SerializeField] TMP_Text reward = null!;
        [SerializeField] TMP_Text fatigueTitle = null!;
        [SerializeField] RectTransform fatigueList = null!;
        [SerializeField] FatigueRowView fatigueRowPrefab = null!;
        [SerializeField] TMP_Text verify = null!;
        [SerializeField] TMP_Text toHq = null!;
        [SerializeField] UiButton continueButton = null!;
        UniTaskCompletionSource? _done;

        public override bool Cancelable => true;
        public override void RequestCancel() => _done?.TrySetResult();

        /// <summary>Bouton Continuer (tests PlayMode : clic réel).</summary>
        public Button ContinueButton => continueButton.Button;

        void Awake() => continueButton.Button.onClick.AddListener(() => _done?.TrySetResult());

        public UniTask ShowAsync(CombatResultsData d, CancellationToken ct)
        {
            Open(d.Title);
            titleBand.color = d.Victory ? Theme.Primary : Theme.Danger;
            reward.text = d.Reward;
            fatigueTitle.text = d.FatigueTitle;
            fatigueList.Clear();
            foreach (var f in d.Fatigue) Instantiate(fatigueRowPrefab, fatigueList).Show(f);
            verify.text = d.Verify;
            verify.color = Theme.TextMuted;
            toHq.text = d.ToHq;
            toHq.gameObject.SetActive(!string.IsNullOrEmpty(d.ToHq));
            continueButton.SetLabel(d.Continue);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }

        public void SetVerification(string text, Tone tone)
        {
            verify.text = text;
            verify.color = ToneColors.Of(tone);
        }
    }
}
