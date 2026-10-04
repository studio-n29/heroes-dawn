#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>
    /// Confirmation d'achat (M31, popup prefab) : héros, combo phare, solde avant / coût / après ; si le solde ne suffit pas,
    /// manque en rouge, piste pour gagner des PR et bouton d'achat désactivé. Retour = Annuler.
    /// </summary>
    public sealed class RecruitConfirmPopupView : MonoBehaviour, IRecruitConfirmPopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text memberName = null!;
        [SerializeField] TMP_Text rarity = null!;
        [SerializeField] TMP_Text role = null!;
        [SerializeField] TMP_Text combo = null!;
        [SerializeField] TMP_Text before = null!;
        [SerializeField] TMP_Text cost = null!;
        [SerializeField] TMP_Text after = null!;
        [SerializeField] TMP_Text missing = null!;
        [SerializeField] TMP_Text howToEarn = null!;
        [SerializeField] UiButton ok = null!;
        [SerializeField] UiButton cancel = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;
        UniTaskCompletionSource<bool>? _answer;

        public bool Cancelable => true;
        public UiButton OkButton => ok;
        public UiButton CancelButton => cancel;

        void Awake()
        {
            ok.Button.onClick.AddListener(() => _answer?.TrySetResult(true));
            cancel.Button.onClick.AddListener(() => _answer?.TrySetResult(false));
        }

        public UniTask<bool> AskAsync(RecruitConfirmData d, CancellationToken ct)
        {
            title.text = d.Title;
            portrait.Show(d.Card.Portrait);
            memberName.text = d.Card.Name;
            rarity.text = d.Card.RarityText;
            rarity.color = Theme.RarityColor(d.Card.Rarity);
            role.text = d.Card.RoleLine;
            combo.text = d.Combo;
            before.text = d.Before;
            cost.text = d.Cost;
            after.text = d.CanAfford ? d.After : "";
            missing.text = d.Missing;
            howToEarn.text = d.HowToEarn;
            ok.SetLabel(d.Ok);
            ok.Interactable = d.CanAfford;
            cancel.SetLabel(d.Cancel);
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
            _answer = new UniTaskCompletionSource<bool>();
            ct.Register(() => _answer.TrySetCanceled());
            return _answer.Task;
        }

        public void RequestCancel() => _answer?.TrySetResult(false);
    }
}
