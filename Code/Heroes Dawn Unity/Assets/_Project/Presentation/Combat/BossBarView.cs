#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Barre de vie élargie du boss (au centre, sous la frise) : nom, phase, PV, repères des seuils ; un éclat annonce chaque changement de phase.</summary>
    public sealed class BossBarView : MonoBehaviour
    {
        [SerializeField] TMP_Text nameText = null!;
        [SerializeField] TMP_Text phaseText = null!;
        [SerializeField] UiBar bar = null!;
        [SerializeField] RectTransform marksRoot = null!;
        [SerializeField] CanvasGroup flash = null!;

        readonly List<Image> _marks = new List<Image>();
        bool _first = true;

        public string Name => nameText.text;
        public string Phase => phaseText.text;
        /// <summary>Nombre de repères de seuil affichés sur la barre (tests).</summary>
        public int MarkCount => _marks.Count;

        public void Show(BossBarData d, bool phaseChanged)
        {
            nameText.text = d.Name;
            phaseText.text = d.Phase;
            phaseText.gameObject.SetActive(d.Phase.Length > 0);
            bar.Set(d.Hp01, instant: _first);
            bar.SetLabel(d.HpText);
            _first = false;
            ShowMarks(d.Marks);
            if (!phaseChanged) return;
            // Télégraphie : la barre claque, le texte de phase apparaît, un éclat blanc s'éteint.
            Tween.Punch(transform, 0.04f, 0.5f);
            Tween.PopIn(phaseText.rectTransform, 0.4f);
            flash.alpha = 0.85f;
            Tween.Fade(flash, 0f, 0.7f);
        }

        void ShowMarks(IReadOnlyList<float> marks)
        {
            while (_marks.Count < marks.Count)
            {
                var go = new GameObject("Mark" + _marks.Count, typeof(RectTransform), typeof(Image)) { layer = marksRoot.gameObject.layer };
                go.transform.SetParent(marksRoot, false);
                var m = go.GetComponent<Image>();
                m.color = Theme.Primary;
                m.raycastTarget = false;
                _marks.Add(m);
            }
            for (int i = 0; i < _marks.Count; i++)
            {
                bool on = i < marks.Count;
                if (_marks[i].gameObject.activeSelf != on) _marks[i].gameObject.SetActive(on);
                if (!on) continue;
                var rt = _marks[i].rectTransform;
                rt.anchorMin = new Vector2(marks[i], 0f);
                rt.anchorMax = new Vector2(marks[i], 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(5f, 8f);
                rt.anchoredPosition = Vector2.zero;
            }
        }
    }
}
