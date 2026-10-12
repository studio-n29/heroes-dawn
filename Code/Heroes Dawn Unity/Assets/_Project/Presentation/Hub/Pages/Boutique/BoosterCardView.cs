#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Carte tirée d'un booster (widget prefab) : dos au départ, puis retournement avec un éclat à la couleur de la rareté.</summary>
    public sealed class BoosterCardView : MonoBehaviour
    {
        [SerializeField] Image frame = null!;
        [SerializeField] GameObject faceRoot = null!;
        [SerializeField] GameObject back = null!;
        [SerializeField] TMP_Text glyph = null!;
        [SerializeField] TMP_Text cardName = null!;
        [SerializeField] TMP_Text rarity = null!;
        [SerializeField] GameObject newBadge = null!;
        [SerializeField] TMP_Text newText = null!;

        BoosterCardData _data;

        public bool Revealed { get; private set; }
        public int RarityRank => _data.RarityRank;
        public bool IsNew => _data.IsNew;
        public string Name => _data.Name;
        public bool FaceVisible => faceRoot.activeSelf;

        public void Setup(BoosterCardData d, string newLabel)
        {
            _data = d;
            Revealed = false;
            frame.color = Theme.FrostEdge;
            glyph.text = d.Glyph;
            cardName.text = d.Name;
            rarity.text = d.RarityName;
            rarity.color = Color.Lerp(d.RarityColor, Theme.Navy, 0.35f);
            newText.text = newLabel;
            faceRoot.SetActive(false);
            back.SetActive(true);
            newBadge.SetActive(false);
        }

        /// <summary>Retourne la carte ; <paramref name="animate"/> faux : tout de suite, sans éclat.</summary>
        public void Reveal(bool animate)
        {
            if (Revealed) return;
            Revealed = true;
            if (!animate || MotionSettings.Reduced || !UnityEngine.Application.isPlaying) { ShowFace(); return; }
            var rt = (RectTransform)transform;
            Tween.Run(0.12f, k => { if (rt != null) rt.localScale = new Vector3(1f - k, 1f, 1f); }, () =>
            {
                if (rt == null) return;
                ShowFace();
                Tween.Run(0.16f, k => { if (rt != null) rt.localScale = new Vector3(k, 1f + 0.08f * Mathf.Sin(k * Mathf.PI), 1f); },
                          () => { if (rt != null) { rt.localScale = Vector3.one; Burst(); } }, rt, Tween.EaseOutBack);
            }, rt, Tween.Linear);
        }

        void ShowFace()
        {
            back.SetActive(false);
            faceRoot.SetActive(true);
            frame.color = _data.RarityColor;
            newBadge.SetActive(_data.IsNew);
        }

        void Burst()
        {
            Fx.Sparks((RectTransform)transform, _data.RarityColor, 6 + 5 * _data.RarityRank, 110f + 30f * _data.RarityRank, 14f + 2f * _data.RarityRank);
            if (_data.RarityRank >= 2) Tween.Punch(transform, 0.1f, 0.3f);
        }
    }
}
