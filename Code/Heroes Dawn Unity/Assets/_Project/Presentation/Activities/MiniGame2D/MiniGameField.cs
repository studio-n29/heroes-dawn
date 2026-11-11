#nullable enable
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Zone de jeu tactile (UI Toolkit) : transforme toucher, glissé et relâchement en <see cref="MiniGameInput"/> (une lecture par image avec <see cref="Poll"/>).
    /// Le joystick est flottant : son origine est le point où le doigt se pose, sa portée <see cref="StickRadius"/> (en fraction de la largeur).
    /// Un doigt qui sort de la zone émet un relâchement : jamais d'appui fantôme.
    /// </summary>
    public sealed class MiniGameField
    {
        public const float StickRadius = 0.12f;

        readonly VisualElement _element;
        bool _held, _pressed, _released;
        Vector2 _pos, _origin;

        public MiniGameField(VisualElement element)
        {
            _element = element;
            element.RegisterCallback<PointerDownEvent>(e => { element.CapturePointer(e.pointerId); PressAt(Normalize(e.position)); });
            element.RegisterCallback<PointerMoveEvent>(e => { if (_held) _pos = Normalize(e.position); });
            element.RegisterCallback<PointerUpEvent>(e => { element.ReleasePointer(e.pointerId); ReleaseNow(); });
            element.RegisterCallback<PointerCancelEvent>(_ => ReleaseNow());
            element.RegisterCallback<DetachFromPanelEvent>(_ => OnDisable());
        }

        public bool Held => _held;
        /// <summary>Origine du joystick, normalisée dans la zone (0..1, y vers le bas).</summary>
        public Vector2 StickOrigin => _origin;
        public Vector2 Position => _pos;

        /// <summary>Le doigt sort de la zone (tests PlayMode : mêmes effets que l'événement).</summary>
        public void PointerLeft() { if (_held) ReleaseNow(); }

        void OnDisable()
        {
            // Une zone retirée en plein appui : le relâchement est émis pour le prochain lecteur, jamais perdu.
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
            var size = _element.layout.size;
            if (size.x <= 1f) size = new Vector2(1208f, 604f);
            var stick = Vector2.zero;
            if (_held)
            {
                var delta = new Vector2((_pos.x - _origin.x) * size.x, (_pos.y - _origin.y) * size.y) / (StickRadius * size.x);
                stick = Vector2.ClampMagnitude(delta, 1f);
            }
            var input = new MiniGameInput { Down = _pressed, Up = _released, Held = _held, Pos = _pos, Stick = stick };
            _pressed = false;
            _released = false;
            return input;
        }

        Vector2 Normalize(Vector2 panelPos)
        {
            var r = _element.worldBound;
            if (r.width <= 1f || r.height <= 1f) return _pos;
            return new Vector2(Mathf.Clamp01((panelPos.x - r.xMin) / r.width), Mathf.Clamp01((panelPos.y - r.yMin) / r.height));
        }
    }
}
