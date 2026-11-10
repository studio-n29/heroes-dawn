#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>
    /// Base des vues UI Toolkit : un <see cref="UIDocument"/> sur le même objet, signalement Shown/Hidden comme <see cref="ViewBase"/>,
    /// classe USS du mode d'entrée sur la racine, zones sûres. Les vues concrètes retrouvent leurs éléments dans <see cref="Bind"/>.
    /// L'objet porte aussi un RectTransform : les couches existantes (écrans, modales) le placent comme un prefab uGUI.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public abstract class HdView : MonoBehaviour, IView
    {
        /// <summary>Marge de la zone sûre des consoles (surbalayage des téléviseurs), en fraction de la taille.</summary>
        public const float ConsoleMargin = 0.05f;

        UIDocument? _doc;

        public event Action? Shown;
        public event Action? Hidden;
        public bool IsVisible { get; private set; }

        public UIDocument Document => _doc != null ? _doc : (_doc = GetComponent<UIDocument>());

        /// <summary>Racine du document (le conteneur du modèle UXML).</summary>
        public VisualElement Root => Document.rootVisualElement;

        protected virtual void OnEnable()
        {
            var root = Root;
            // Hors des zones peintes, les clics traversent vers le jeu en dessous (carte, terrains de mini-jeux).
            root.pickingMode = PickingMode.Ignore;
            foreach (var template in root.Query<TemplateContainer>().ToList())
            {
                // Le conteneur du modèle remplit le panneau (point de passage commun : les écrans plein cadre en dépendent).
                template.pickingMode = PickingMode.Ignore;
                template.style.flexGrow = 1;
                template.style.width = Length.Percent(100);
                template.style.height = Length.Percent(100);
            }
            HdInput.Changed += ApplyMode;
            ApplyMode(HdInput.Mode);
            Bind();
            ApplySafeArea();
            IsVisible = true;
            Shown?.Invoke();
        }

        protected virtual void OnDisable()
        {
            HdInput.Changed -= ApplyMode;
            IsVisible = false;
            Hidden?.Invoke();
        }

        /// <summary>Retrouve les éléments du modèle et branche leurs événements (appelé à chaque activation : le document est recréé).</summary>
        protected abstract void Bind();

        /// <summary>Élément nommé du modèle ; absent, c'est une erreur de modèle (nom changé sans la vue).</summary>
        protected T Q<T>(string name) where T : VisualElement
            => Root.Q<T>(name) ?? throw new InvalidOperationException($"Élément « {name} » ({typeof(T).Name}) absent de {GetType().Name}");

        /// <summary>Élément nommé du modèle, ou null quand le modèle ne le prévoit pas.</summary>
        protected T? TryQ<T>(string name) where T : VisualElement => Root.Q<T>(name);

        void ApplyMode(HdInputMode mode)
        {
            var root = Root;
            root.EnableInClassList(HdInput.ClassOf(HdInputMode.KeyboardMouse), mode == HdInputMode.KeyboardMouse);
            root.EnableInClassList(HdInput.ClassOf(HdInputMode.Touch), mode == HdInputMode.Touch);
            root.EnableInClassList(HdInput.ClassOf(HdInputMode.Gamepad), mode == HdInputMode.Gamepad);
        }

        /// <summary>Applique les zones sûres (encoche, barres système, surbalayage console) en marge de l'élément « safe », s'il existe.</summary>
        protected void ApplySafeArea()
        {
            var safe = Root.Q<VisualElement>("safe");
            if (safe == null) return;
            float width = Mathf.Max(1, Screen.width), height = Mathf.Max(1, Screen.height);
            var area = Screen.safeArea;
            float left = area.xMin / width, right = (width - area.xMax) / width;
            float top = (height - area.yMax) / height, bottom = area.yMin / height;
            if (UnityEngine.Application.isConsolePlatform)
            {
                left = Mathf.Max(left, ConsoleMargin); right = Mathf.Max(right, ConsoleMargin);
                top = Mathf.Max(top, ConsoleMargin); bottom = Mathf.Max(bottom, ConsoleMargin);
            }
            safe.style.paddingLeft = Length.Percent(left * 100f);
            safe.style.paddingRight = Length.Percent(right * 100f);
            safe.style.paddingTop = Length.Percent(top * 100f);
            safe.style.paddingBottom = Length.Percent(bottom * 100f);
        }
    }
}
