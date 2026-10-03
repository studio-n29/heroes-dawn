#nullable enable
using MessagePipe;
using N29.HeroesDawn.Application.Session;

namespace N29.HeroesDawn.Application.Messages
{
    public enum ToastKind { Info, Reward, Warning, Error }

    /// <summary>Le portefeuille a changé (valeurs après mise à jour).</summary>
    public readonly struct WalletChanged
    {
        public readonly int Credits;
        public readonly int Pr;
        public WalletChanged(int credits, int pr) { Credits = credits; Pr = pr; }
    }

    /// <summary>L'effectif a changé (niveau, fatigue, escouade, recrue).</summary>
    public readonly struct RosterChanged { }

    /// <summary>La boutique éphémère a changé (nouveau jour, achat, renouvellement).</summary>
    public readonly struct ShopChanged { }

    /// <summary>Message court à afficher au joueur.</summary>
    public readonly struct ToastRequested
    {
        public readonly string Text;
        public readonly ToastKind Kind;
        public ToastRequested(string text, ToastKind kind) { Text = text; Kind = kind; }
    }

    /// <summary>Une sauvegarde commence (true) ou se termine (false).</summary>
    public readonly struct SaveStateChanged
    {
        public readonly bool Saving;
        public SaveStateChanged(bool saving) { Saving = saving; }
    }

    public static class SessionPublishing
    {
        /// <summary>Publie l'état courant du portefeuille de la session.</summary>
        public static void PublishWallet(this IPublisher<WalletChanged> publisher, GameSession session)
            => publisher.Publish(new WalletChanged(session.Wallet.Credits, session.Wallet.Pr));

        public static void PublishRoster(this IPublisher<RosterChanged> publisher) => publisher.Publish(default);
    }
}
