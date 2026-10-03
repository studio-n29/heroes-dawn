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

    public sealed class ModalLayer : MonoBehaviour
    {
        readonly List<ModalWindow> _open = new List<ModalWindow>();
        RectTransform _rt;

        public bool AnyOpen => _open.Count > 0;

        public static ModalLayer Create(RectTransform parent)
        {
            var rt = Ui.Rect(parent, "Modals").Stretch();
            var m = rt.gameObject.AddComponent<ModalLayer>();
            m._rt = rt;
            return m;
        }

        public ModalWindow Open(string title, float width, float height, Color? titleColor = null)
        {
            var root = Ui.Rect(_rt, "Modal").Stretch();
            var dim = Ui.Img(root, "Dim", Theme.Overlay, null, true);
            dim.rectTransform.Stretch();
            var fill = Ui.Panel(root, "Window", Theme.Surface, 6f, 12f, true);
            var window = fill.Frame().At(0.5f, 0.5f, 0, 0, width, height);

            // Bandeau de titre jaune, façon récitatif.
            var band = Ui.Panel(window, "TitleBand", titleColor ?? Theme.Primary, 4f, 0f, false);
            band.Frame().At(0.5f, 1f, 0, 30, Mathf.Min(width - 80, 760), 84);
            var titleText = Ui.Txt(band.transform, title, 50, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Title");
            titleText.rectTransform.Stretch(12, 0, 12, 0);

            var content = Ui.Rect(window, "Content").Stretch(36, 30, 36, 80);
            var cg = root.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            Tween.Fade(cg, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);

            var mw = new ModalWindow(this, root, window, content, titleText);
            _open.Add(mw);
            return mw;
        }

        internal void Remove(ModalWindow w)
        {
            _open.Remove(w);
            if (w.Root != null) Destroy(w.Root.gameObject);
        }

        public void CloseAll()
        {
            for (int i = _open.Count - 1; i >= 0; i--) _open[i].Close();
        }

        /// <summary>Boîte de confirmation standard.</summary>
        public ModalWindow Confirm(string title, string body, string okLabel, Action onOk, string cancelLabel = null, Action onCancel = null,
                                   Color? okColor = null)
        {
            var w = Open(title, 980, 480);
            var text = Ui.Txt(w.Content, body, 34, Theme.Text, TextAnchor.MiddleCenter);
            text.rectTransform.Stretch(0, 120, 0, 10);
            var row = Ui.Rect(w.Content, "Buttons").BottomBand(96, 0);
            row.HLayout(40, 0, TextAnchor.MiddleCenter);
            if (cancelLabel != null)
            {
                var cancel = Ui.Btn(row, cancelLabel, Theme.Surface3, Theme.Text, () => { w.Close(); onCancel?.Invoke(); }, 36);
                cancel.Rect.Pref(300, 90);
            }
            var ok = Ui.Btn(row, okLabel, okColor ?? Theme.Primary, Theme.TextDark, () => { w.Close(); onOk?.Invoke(); }, 40);
            ok.Rect.Pref(340, 90);
            return w;
        }
    }
}
