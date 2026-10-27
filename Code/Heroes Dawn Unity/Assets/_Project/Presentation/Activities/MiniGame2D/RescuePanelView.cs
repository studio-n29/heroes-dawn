#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Interface d'« Ingénierie de secours » (partie du prefab Popups/MiniGame2DPopup, masquée hors de ce jeu) : le manuel, la réserve de pièces,
    /// les emplacements de l'outil, Annuler et Valider. Le pilote ne fait que la remplir.
    /// </summary>
    public sealed class RescuePanelView : MonoBehaviour
    {
        [SerializeField] TMP_Text manual = null!;
        [SerializeField] TMP_Text status = null!;
        [SerializeField] UiButton[] poolButtons = null!;
        [SerializeField] Image[] slotBoxes = null!;
        [SerializeField] TMP_Text[] slotTexts = null!;
        [SerializeField] UiButton undo = null!;
        [SerializeField] UiButton submit = null!;

        public TMP_Text Manual => manual;
        public TMP_Text Status => status;
        public int PoolCapacity => poolButtons.Length;
        public int SlotCapacity => slotBoxes.Length;
        public UiButton PoolButton(int i) => poolButtons[i];
        public Image SlotBox(int i) => slotBoxes[i];
        public TMP_Text SlotText(int i) => slotTexts[i];
        public UiButton Undo => undo;
        public UiButton Submit => submit;
    }

    /// <summary>Pilote qui a besoin d'un panneau du prefab : la fenêtre le lui remet avant le début de la partie.</summary>
    public interface IRescuePanelUser
    {
        void Bind(RescuePanelView panel);
    }
}
