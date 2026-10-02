#nullable enable
using System;
using System.Text;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Carte d'une unité en combat : portrait, PV, jauge ATB, statuts, états (prête, sélectionnée, ciblable, K.O.).</summary>
    public sealed class UnitCard : MonoBehaviour
    {
        public int UnitId { get; private set; }
        public RectTransform Rect => (RectTransform)transform;
        Image _fill = null!;
        Image _frame = null!;
        Image _select = null!;
        Image _target = null!;
        UiBar _hp = null!;
        UiBar _gauge = null!;
        Text _statuses = null!;
        Text _ko = null!;
        Pulse _readyPulse = null!;
        CanvasGroup _group = null!;
        Color _baseFill;
        bool _isAlly;

        public event Action<int>? Tapped;

        public static UnitCard Create(Transform parent, CombatUnit u, string name, Family family, bool ally, float w, float h)
        {
            var fill = Ui.Panel(parent, (ally ? "Ally_" : "Enemy_") + u.Id, ally ? Theme.Surface : Theme.Hex(0x2A1C22), 4f, 7f, true);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(w, h);
            var card = frame.gameObject.AddComponent<UnitCard>();
            card.UnitId = u.Id;
            card._fill = fill;
            card._baseFill = fill.color;
            card._frame = frame.GetComponent<Image>();
            card._isAlly = ally;
            card._group = frame.gameObject.AddComponent<CanvasGroup>();

            card._target = Ui.Img(frame, "TargetRing", Theme.Danger, Sprites.Rounded);
            card._target.rectTransform.Stretch(-10, -10, -10, -10);
            card._target.transform.SetAsFirstSibling();
            card._target.gameObject.AddComponent<Pulse>().Amplitude = 0.03f;
            card._target.gameObject.SetActive(false);

            card._select = Ui.Img(frame, "SelectRing", Theme.Primary, Sprites.Rounded);
            card._select.rectTransform.Stretch(-8, -8, -8, -8);
            card._select.transform.SetAsFirstSibling();
            card._select.gameObject.SetActive(false);

            var strip = Ui.Img(fill.transform, "Family", Theme.FamilyColor(family), Sprites.RoundedSmall);
            strip.rectTransform.TopBand(10, 0);

            float ps = Mathf.Min(h - 50, 96);
            var portrait = Portrait.Create(fill.transform, name, family, ps, u.IsBoss);
            portrait.rectTransform.At(0f, 1f, 12, -20, ps, ps);

            float x = ps + 26;
            float barW = w - x - 18;
            var nameText = Ui.Txt(fill.transform, name.ToUpperInvariant(), 32, u.IsBoss ? Theme.Primary : Color.white, TextAnchor.UpperLeft, FontKind.Title, "Name");
            nameText.rectTransform.At(0f, 1f, x, -14, barW, 38);
            nameText.Inked(2f);

            card._hp = Ui.Bar(fill.transform, "Hp", ally ? Theme.Hp : Theme.HpEnemy, Theme.SurfaceDark, true, 18);
            ((RectTransform)card._hp.transform).At(0f, 1f, x, -56, barW, 28);
            card._gauge = Ui.Bar(fill.transform, "Gauge", Theme.Gauge, Theme.SurfaceDark);
            card._gauge.Smooth = false;
            ((RectTransform)card._gauge.transform).At(0f, 1f, x, -90, barW, 16);

            card._statuses = Ui.Txt(fill.transform, "", 18, Theme.Shield, TextAnchor.UpperLeft, FontKind.Body, "Statuses");
            card._statuses.rectTransform.At(0f, 1f, x, -112, barW, 50);

            card._ko = Ui.Txt(fill.transform, "K.O.", 64, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Title, "KO");
            card._ko.rectTransform.Stretch();
            card._ko.Inked(3f);
            card._ko.gameObject.SetActive(false);

            card._readyPulse = frame.gameObject.AddComponent<Pulse>();
            card._readyPulse.Amplitude = 0.035f;
            card._readyPulse.Speed = 7f;
            card._readyPulse.enabled = false;

            card._frame.MakeClickable(() => card.Tapped?.Invoke(card.UnitId));
            card.Refresh(u, false, false, false, true);
            return card;
        }

        public void Refresh(CombatUnit u, bool readyHighlight, bool selected, bool targetable, bool instant = false)
        {
            _hp.Set(u.MaxHp == 0 ? 0 : u.Hp / (float)u.MaxHp, instant);
            _hp.SetLabel(u.Hp + " / " + u.MaxHp);
            _gauge.Set(u.GaugeMax == 0 ? 0 : (float)((double)u.Gauge / u.GaugeMax), true);
            _gauge.SetColor(u.IsReady ? Color.white : Theme.Gauge);

            var sb = new StringBuilder();
            foreach (var s in u.Statuses)
            {
                if (sb.Length > 0) sb.Append("  ");
                sb.Append(Loc.StatusName(s.Type));
                if (s.Type == StatusType.Shield) sb.Append(' ').Append(s.Value);
                else if (s.Type != StatusType.Mark) sb.Append(' ').Append(s.TurnsLeft);
            }
            _statuses.text = sb.ToString();

            bool alive = u.IsAlive;
            _ko.gameObject.SetActive(!alive);
            _group.alpha = alive ? 1f : 0.45f;
            _fill.color = readyHighlight && alive ? Color.Lerp(_baseFill, Theme.Primary, 0.22f) : _baseFill;
            _readyPulse.enabled = readyHighlight && alive;
            _select.gameObject.SetActive(selected && alive);
            _target.gameObject.SetActive(targetable && alive);
            _target.color = _isAlly ? Theme.Success : Theme.Danger;
        }

        /// <summary>Nombre flottant (dégâts, soins, statut…) qui monte et s'efface.</summary>
        public void Float(string text, Color color, int size = 44)
        {
            var t = Ui.Txt(transform, text, size, color, TextAnchor.MiddleCenter, FontKind.Title, "Float");
            var rt = t.rectTransform.At(0.5f, 0.5f, UnityEngine.Random.Range(-60f, 60f), 10, 360, 70);
            t.Inked(2.5f);
            var cg = t.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            Vector2 start = rt.anchoredPosition;
            Tween.PopIn(rt, 0.18f);
            Tween.Run(0.9f, k =>
            {
                if (rt == null) return;
                rt.anchoredPosition = start + new Vector2(0, 90f * k);
                cg.alpha = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
            }, () => { if (rt != null) Destroy(rt.gameObject); }, rt, Tween.Linear);
        }

        public void Hit() => Tween.Shake(Rect, 12f, 0.25f);
        public void Act() => Tween.Punch(Rect, 0.08f, 0.25f);
    }
}
