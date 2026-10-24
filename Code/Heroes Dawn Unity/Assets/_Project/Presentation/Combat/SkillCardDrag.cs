#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>
    /// Glisser-déposer d'une carte de compétence : la carte quitte l'éventail, suit le doigt (agrandie) et, au lâcher, signale la position
    /// de l'écran ; la vue décide de la cible. Quel que soit le résultat, la carte revient à sa place dans l'éventail.
    /// </summary>
    public sealed class SkillCardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] RectTransform card = null!;
        [SerializeField] CanvasGroup group = null!;

        Transform? _parent;
        int _sibling;
        Vector2 _anchorMin, _anchorMax, _pivot, _anchored;
        Vector3 _euler, _scale;
        bool _dragging;

        /// <summary>Faux tant que la carte n'est pas lançable : on ne la prend pas.</summary>
        public bool CanDrag { get; set; }
        public bool IsDragging => _dragging;
        public event Action? Began;
        public event Action<Vector2>? Ended;

        public void OnBeginDrag(PointerEventData e)
        {
            if (!CanDrag || _dragging) return;
            _dragging = true;
            _parent = card.parent;
            _sibling = card.GetSiblingIndex();
            _anchorMin = card.anchorMin; _anchorMax = card.anchorMax; _pivot = card.pivot; _anchored = card.anchoredPosition;
            _euler = card.localEulerAngles; _scale = card.localScale;
            var canvas = GetComponentInParent<Canvas>(true);
            var layer = canvas != null ? canvas.rootCanvas.transform : card.root;
            card.SetParent(layer, true);
            card.SetAsLastSibling();
            card.pivot = new Vector2(0.5f, 0.5f);
            card.localEulerAngles = new Vector3(0f, 0f, 4f);
            card.localScale = _scale * 1.15f;
            group.blocksRaycasts = false; // le pointeur « voit » ce qui est sous la carte
            Follow(e);
            Began?.Invoke();
        }

        public void OnDrag(PointerEventData e)
        {
            if (_dragging) Follow(e);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!_dragging) return;
            _dragging = false;
            var at = e.position;
            Restore(animate: true);
            Ended?.Invoke(at);
        }

        void Follow(PointerEventData e)
        {
            var rt = card.parent as RectTransform;
            if (rt != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, e.position, e.pressEventCamera, out var world)) card.position = world;
        }

        /// <summary>
        /// Remet la carte dans l'Ã©ventail ; avec <paramref name="animate"/>, elle glisse de l'endroit oÃ¹ elle a Ã©tÃ© lÃ¢chÃ©e jusqu'Ã  sa place
        /// (en retrouvant sa taille et son inclinaison) au lieu de sauter.
        /// </summary>
        void Restore(bool animate = false)
        {
            var fromWorld = card.position;
            var fromScale = card.localScale;
            var fromEuler = card.localEulerAngles;
            if (_parent != null) card.SetParent(_parent, false);
            card.SetSiblingIndex(_sibling);
            card.anchorMin = _anchorMin; card.anchorMax = _anchorMax; card.pivot = _pivot; card.anchoredPosition = _anchored;
            card.localEulerAngles = _euler; card.localScale = _scale;
            group.blocksRaycasts = true;
            if (animate && UnityEngine.Application.isPlaying && !Feedback.MotionSettings.Reduced) Glide(fromWorld, fromScale, fromEuler);
        }

        void Glide(Vector3 fromWorld, Vector3 fromScale, Vector3 fromEuler)
        {
            var home = card.anchoredPosition;
            card.position = fromWorld;
            var start = card.anchoredPosition;
            float z0 = fromEuler.z > 180f ? fromEuler.z - 360f : fromEuler.z, z1 = _euler.z > 180f ? _euler.z - 360f : _euler.z;
            var target = card;
            var scale = _scale; var homeEuler = _euler;
            Common.Tween.Run(0.22f, k =>
            {
                if (target == null) return;
                target.anchoredPosition = Vector2.LerpUnclamped(start, home, k);
                target.localScale = Vector3.LerpUnclamped(fromScale, scale, k);
                target.localEulerAngles = new Vector3(homeEuler.x, homeEuler.y, Mathf.LerpUnclamped(z0, z1, k));
            }, () =>
            {
                if (target == null) return;
                target.anchoredPosition = home; target.localScale = scale; target.localEulerAngles = homeEuler;
            }, target, Common.Tween.EaseOutBack);
        }

        void OnDisable()
        {
            if (_dragging) { _dragging = false; Restore(); }
        }
    }
}
