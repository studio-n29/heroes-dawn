#nullable enable
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Couche des modales (prefab, au-dessus de toutes les scènes) : crée les popups du <see cref="ScreenCatalog"/> pour <see cref="ModalService"/>.</summary>
    public sealed class ModalLayer : MonoBehaviour, IPopupHost
    {
        ScreenCatalog? _catalog;

        public void Init(ScreenCatalog catalog) => _catalog = catalog;

        public TView Spawn<TView>() where TView : class, IPopupView
        {
            if (_catalog == null) throw new System.InvalidOperationException("ModalLayer non initialisée (ScreenCatalog absent).");
            var go = Instantiate(_catalog.PopupFor<TView>(), transform);
            ((RectTransform)go.transform).Stretch();
            return go.GetComponent<TView>();
        }

        public void Despawn(IPopupView view)
        {
            if (view is Component c && c != null) Destroy(c.gameObject);
        }
    }
}
