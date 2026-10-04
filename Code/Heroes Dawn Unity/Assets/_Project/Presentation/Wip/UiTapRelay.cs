#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Wip
{
    /// <summary>Relais posé sur la racine de la coque : les éléments enfants (WipMark, boutons grisés) le trouvent parmi leurs parents.</summary>
    public sealed class UiTapRelay : MonoBehaviour, IUiTapRelay
    {
        public event Action<string>? WipTapped;
        public event Action<string>? ReasonTapped;

        public void RaiseWip(string featureId) => WipTapped?.Invoke(featureId);
        public void RaiseReason(string text) => ReasonTapped?.Invoke(text);
    }
}
