#nullable enable
using System;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Carte de la grille Équipe (widget prefab) : toucher la carte ouvre la fiche, le bouton gère l'escouade.</summary>
    public sealed class MemberTileView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Button tap = null!;
        [SerializeField] Image fill = null!;
        [SerializeField] Image strip = null!;
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] TMP_Text memberName = null!;
        [SerializeField] TMP_Text level = null!;
        [SerializeField] TMP_Text rarity = null!;
        [SerializeField] TMP_Text status = null!;
        [SerializeField] UiBar fatigue = null!;
        [SerializeField] UiButton squad = null!;

        Color _fill;
        Family _family;

        public event Action? Tapped;
        public event Action? SquadClicked;

        public Button TapButton => tap;
        public Button SquadButton => squad.Button;

        void Awake()
        {
            tap.onClick.AddListener(() => Tapped?.Invoke());
            squad.Button.onClick.AddListener(() => SquadClicked?.Invoke());
        }

        /// <summary>Survol : la carte se soulève, teintée à la couleur de sa famille.</summary>
        public void OnPointerEnter(PointerEventData e)
        {
            transform.localScale = new Vector3(1.04f, 1.04f, 1f);
            fill.color = Color.Lerp(_fill, Theme.FamilyColor(_family), 0.2f);
        }

        public void OnPointerExit(PointerEventData e)
        {
            transform.localScale = Vector3.one;
            fill.color = _fill;
        }

        public void Show(MemberTileData d)
        {
            strip.color = Theme.FamilyColor(d.Card.Family);
            portrait.Show(d.Card.Portrait);
            memberName.text = d.Card.Name;
            level.text = d.Card.LevelText;
            rarity.text = d.Card.RarityText;
            rarity.color = Theme.RarityColor(d.Card.Rarity);
            status.text = d.Status;
            status.color = d.Exhausted ? Theme.Danger : Theme.Primary;
            fatigue.SetColor(Theme.FatigueColor(d.Fatigue));
            fatigue.Set(d.Fatigue01, true);
            fatigue.SetLabel(d.FatigueText);
            squad.SetLabel(d.SquadLabel);
            squad.SetColors(d.InSquad ? Theme.Surface3 : Theme.Primary, d.InSquad ? Theme.Text : Theme.TextDark);
            fill.color = d.Exhausted ? Color.Lerp(Theme.Surface, Theme.Danger, 0.25f) : d.InSquad ? Color.Lerp(Theme.Surface, Theme.Primary, 0.12f) : Theme.Surface;
            _fill = fill.color;
            _family = d.Card.Family;
        }
    }
}
