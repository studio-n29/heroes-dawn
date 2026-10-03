using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Fenêtre modale : voile sombre bloquant + case de BD centrée avec titre.</summary>
    public sealed class ModalWindow
    {
        public RectTransform Root { get; }
        public RectTransform Window { get; }
        /// <summary>Zone de contenu sous le titre.</summary>
        public RectTransform Content { get; }
        public TMP_Text Title { get; }
        readonly ModalLayer _layer;
        public bool IsOpen { get; private set; } = true;
        public event Action Closed;

        internal ModalWindow(ModalLayer layer, RectTransform root, RectTransform window, RectTransform content, TMP_Text title)
        {
            _layer = layer; Root = root; Window = window; Content = content; Title = title;
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            _layer.Remove(this);
            Closed?.Invoke();
        }
    }

}
