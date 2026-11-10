#nullable enable
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Messages éphémères en haut de l'écran (UI Toolkit) : au plus quatre, chacun disparaît seul.</summary>
    public sealed class ToastsView : HdView, IToastService
    {
        const int Max = 4;
        const long LifetimeMs = 3200;
        VisualElement _stack = null!;

        protected override void Bind()
        {
            _stack = Q<VisualElement>("stack");
            _stack.pickingMode = PickingMode.Ignore;
            var safe = TryQ<VisualElement>("safe");
            if (safe != null) safe.pickingMode = PickingMode.Ignore;
        }

        public int Count => _stack != null ? _stack.childCount : 0;

        /// <summary>Textes des messages affichés (tests).</summary>
        public System.Collections.Generic.List<string> Texts()
        {
            var texts = new System.Collections.Generic.List<string>();
            if (_stack == null) return texts;
            foreach (var item in _stack.Children()) { var l = item.Q<Label>(); if (l != null) texts.Add(l.text); }
            return texts;
        }

        public void Show(string text, ToastKind kind)
        {
            if (string.IsNullOrEmpty(text) || _stack == null) return;
            while (_stack.childCount >= Max) _stack.RemoveAt(0);
            var item = new VisualElement { pickingMode = PickingMode.Ignore };
            item.AddToClassList("hd-toast");
            item.AddToClassList(kind == ToastKind.Reward ? "hd-toast--reward" : kind == ToastKind.Warning ? "hd-toast--warning" : kind == ToastKind.Error ? "hd-toast--error" : "hd-toast--info");
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("hd-toast__text");
            item.Add(label);
            _stack.Add(item);
            item.schedule.Execute(() => item.AddToClassList("hd-toast--in")).StartingIn(20);
            item.schedule.Execute(() => item.RemoveFromClassList("hd-toast--in")).StartingIn(LifetimeMs - 250);
            item.schedule.Execute(() => item.RemoveFromHierarchy()).StartingIn(LifetimeMs);
        }
    }
}
