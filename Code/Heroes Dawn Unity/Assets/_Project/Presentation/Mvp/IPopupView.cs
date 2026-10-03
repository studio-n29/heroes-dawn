namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Vue de modale. Une modale annulable répond à Retour comme à son bouton « Annuler ».</summary>
    public interface IPopupView
    {
        bool Cancelable { get; }

        /// <summary>Retour sur cette modale (appelé seulement si <see cref="Cancelable"/>).</summary>
        void RequestCancel();
    }
}
