using N29.HeroesDawn.Application.Messages;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Messages éphémères ; le texte est déjà localisé.</summary>
    public interface IToastService
    {
        void Show(string text, ToastKind kind);
    }
}
