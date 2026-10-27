#nullable enable
using UnityEngine;
using UnityEngine.EventSystems;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Zone de jeu tactile : transforme toucher, glissé et relâchement en <see cref="MiniGameInput"/> (une lecture par image avec <see cref="Poll"/>).
    /// Le joystick est flottant : son origine est le point où le doigt se pose, sa portée <see cref="StickRadius"/> (en fraction de la largeur).
    /// Un doigt qui sort de la zone, ou une zone désactivée, émet un relâchement : jamais d'appui fantôme.
    /// </summary>
    public sealed class MiniGameField : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerExitHandler
    {
        public const float StickRadius = 0.12f;

        bool _held, _pressed, _released;
        Vector2 _pos, _origin;

        public bool Held => _held;
        /// <summary>Origine du joystick, normalisée dans la zone (0..1, y vers le bas).</summary>
        public Vector2 StickOrigin => _origin;
        public Vector2 Position => _pos;

        public void OnPointerDown(PointerEventData e) => PressAt(Normalize(e));
        public void OnDrag(PointerEventData e) { if (_held) _pos = Normalize(e); }
        public void OnPointerUp(PointerEventData e) => ReleaseNow();
        public void OnPointerExit(PointerEventData e) { if (_held) ReleaseNow(); }

        void OnDisable()
        {
            // Une zone désactivée en plein appui : le relâchement est émis pour le prochain lecteur, jamais perdu.
            if (_held) { _held = false; _released = true; }
            _pressed = false;
        }

        /// <summary>Pose le doigt en <paramref name="pos"/> (0..1) ; utilisé par les événements et par les tests.</summary>
        public void PressAt(Vector2 pos)
        {
            if (_held) ReleaseNow();
            _pos = _origin = pos;
            _held = true;
            _pressed = true;
        }

        public void DragTo(Vector2 pos) { if (_held) _pos = pos; }

        public void ReleaseNow()
        {
            if (!_held) return;
            _held = false;
            _released = true;
        }

        /// <summary>Remet à zéro l'état (nouvelle partie).</summary>
        public void ResetState() { _held = false; _pressed = false; _released = false; }

        /// <summary>L'entrée de l'image courante ; les drapeaux « Down » et « Up » sont consommés. Stick : x vers la droite, y vers le bas.</summary>
        public MiniGameInput Poll()
        {
            var rect = ((RectTransform)transform).rect;
            var stick = Vector2.zero;
            if (_held && rect.width > 1f)
            {
                var delta = new Vector2((_pos.x - _origin.x) * rect.width, (_pos.y - _origin.y) * rect.height) / (StickRadius * rect.width);
                stick = Vector2.ClampMagnitude(delta, 1f);
            }
            var input = new MiniGameInput { Down = _pressed, Up = _released, Held = _held, Pos = _pos, Stick = stick };
            _pressed = false;
            _released = false;
            return input;
        }

        Vector2 Normalize(PointerEventData e)
        {
            var rt = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out var local)) return _pos;
            var r = rt.rect;
            return new Vector2(Mathf.Clamp01((local.x - r.xMin) / r.width), Mathf.Clamp01(1f - (local.y - r.yMin) / r.height));
        }
    }
}
