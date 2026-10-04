#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Pastille d'une unité sur la frise (widget prefab) : couleur de famille, initiale, anneau quand elle est prête, annonce de l'action.</summary>
    public sealed class TimelineMarkerView : MonoBehaviour
    {
        [SerializeField] Image disc = null!;
        [SerializeField] Image ring = null!;
        [SerializeField] TMP_Text initial = null!;
        [SerializeField] TMP_Text plan = null!;
        [SerializeField] CanvasGroup group = null!;

        public RectTransform Rect => (RectTransform)transform;

        public void Show(TimelineMarkerData d)
        {
            disc.color = d.Ally ? Theme.FamilyColor(d.Family) : Color.Lerp(Theme.HpEnemy, Color.black, 0.15f);
            initial.text = d.Initial;
            ring.gameObject.SetActive(d.Ready || d.InPrep);
            ring.color = d.Ready ? Color.white : d.Ally ? Theme.Primary : Theme.Danger;
            bool planned = d.PlannedSlot >= 0;
            plan.gameObject.SetActive(planned);
            if (planned)
            {
                plan.text = d.PlannedSlot == 2 ? "A3 !!" : "A" + (d.PlannedSlot + 1) + " !";
                plan.color = d.PlannedSlot == 2 ? Theme.Danger : Theme.Primary;
            }
            group.alpha = !d.Alive ? 0f : d.Channelling ? 0.55f : 1f;
            transform.localScale = Vector3.one * (d.Boss ? 1.2f : 1f);
        }
    }
}
