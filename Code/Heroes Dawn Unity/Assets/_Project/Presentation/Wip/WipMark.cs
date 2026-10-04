#nullable enable
using UnityEngine;
using UnityEngine.EventSystems;

namespace N29.HeroesDawn.Presentation.Wip
{
    /// <summary>
    /// Bandeau BIENTÔT posé sur un élément pas encore développé (widget Widgets/WipMark.prefab, étiré sur l'élément) :
    /// voile l'élément, capte le toucher et le remonte au relais de la coque.
    /// </summary>
    public sealed class WipMark : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] string featureId = "";

        public string FeatureId { get => featureId; set => featureId = value; }

        public void OnPointerClick(PointerEventData eventData) => Tap();

        public void Tap() => GetComponentInParent<UiTapRelay>(true)?.RaiseWip(featureId);
    }
}
