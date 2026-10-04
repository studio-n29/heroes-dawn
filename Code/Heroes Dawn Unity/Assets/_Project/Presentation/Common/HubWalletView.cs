#nullable enable
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Barre de ressources du hub (widget Widgets/HubWallet) : Crédits, PR, Gemmes (WIP) et indicateur de sauvegarde.
    /// Un changement fait rouler le compteur, « punche » la pastille et affiche l'écart flottant (+150 / −450).
    /// </summary>
    public sealed class HubWalletView : MonoBehaviour, IWalletView
    {
        [SerializeField] TMP_Text credits = null!;
        [SerializeField] TMP_Text pr = null!;
        [SerializeField] TMP_Text gems = null!;
        [SerializeField] TMP_Text saving = null!;
        [SerializeField, OptionalRef] FloatDelta? creditsDelta;
        [SerializeField, OptionalRef] FloatDelta? prDelta;
        int _lastCredits = -1;
        int _lastPr = -1;
        Coroutine? _rollCredits;
        Coroutine? _rollPr;

        /// <summary>Nombre d'affichages (diagnostic des abonnements, lu par les tests PlayMode).</summary>
        public int Shows { get; private set; }

        public void Show(WalletData d)
        {
            Shows++;
            gems.text = d.Gems.ToString("N0");
            Change(credits, creditsDelta, _lastCredits, d.Credits, ref _rollCredits);
            Change(pr, prDelta, _lastPr, d.Pr, ref _rollPr);
            _lastCredits = d.Credits;
            _lastPr = d.Pr;
        }

        void Change(TMP_Text text, FloatDelta? delta, int from, int to, ref Coroutine? roll)
        {
            Tween.Stop(roll);
            roll = null;
            if (from < 0 || from == to || !UnityEngine.Application.isPlaying)
            {
                text.text = to.ToString("N0");
                return;
            }
            Tween.Punch(text.transform.parent, 0.12f);
            delta?.Play(to - from);
            roll = Tween.Run(CounterRoll.DurationFor(from, to), k => { if (text != null) text.text = CounterRoll.At(from, to, k).ToString("N0"); },
                             null, text, Tween.Linear);
        }

        public void SetSaving(bool on) => saving.text = on ? Loc.T("ui.saving") : "";
    }
}
