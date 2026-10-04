#nullable enable
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Barre de ressources du hub (widget Widgets/HubWallet) : Crédits, PR, Gemmes (WIP) et indicateur de sauvegarde.</summary>
    public sealed class HubWalletView : MonoBehaviour, IWalletView
    {
        [SerializeField] TMP_Text credits = null!;
        [SerializeField] TMP_Text pr = null!;
        [SerializeField] TMP_Text gems = null!;
        [SerializeField] TMP_Text saving = null!;
        int _lastCredits = -1;
        int _lastPr = -1;

        public void Show(WalletData d)
        {
            credits.text = d.Credits.ToString("N0");
            pr.text = d.Pr.ToString("N0");
            gems.text = d.Gems.ToString("N0");
            if (_lastCredits >= 0 && d.Credits != _lastCredits) Tween.Punch(credits.transform.parent, 0.12f);
            if (_lastPr >= 0 && d.Pr != _lastPr) Tween.Punch(pr.transform.parent, 0.12f);
            _lastCredits = d.Credits;
            _lastPr = d.Pr;
        }

        public void SetSaving(bool on) => saving.text = on ? Loc.T("ui.saving") : "";
    }
}
