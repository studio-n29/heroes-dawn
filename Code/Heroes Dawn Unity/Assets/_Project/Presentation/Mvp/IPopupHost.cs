namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Crée et détruit les vues de modales (prefabs du catalogue, sous la couche des modales).</summary>
    public interface IPopupHost
    {
        TView Spawn<TView>() where TView : class, IPopupView;
        void Despawn(IPopupView view);
    }
}
