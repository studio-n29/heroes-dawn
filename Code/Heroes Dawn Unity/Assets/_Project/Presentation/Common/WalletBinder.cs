#nullable enable
using System;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Tient une barre de ressources à jour (portefeuille, sauvegarde) ; utilisé par les présentateurs du QG et du plateau.</summary>
    public sealed class WalletBinder : IDisposable
    {
        readonly IDisposable _subs;

        public WalletBinder(IWalletView view, ISessionReadModel session, ISubscriber<WalletChanged> wallet, ISubscriber<SaveStateChanged> saving)
        {
            view.Show(new WalletData(session.Wallet.Credits, session.Wallet.Pr));
            var bag = DisposableBag.CreateBuilder();
            wallet.Subscribe(w => view.Show(new WalletData(w.Credits, w.Pr))).AddTo(bag);
            saving.Subscribe(s => view.SetSaving(s.Saving)).AddTo(bag);
            _subs = bag.Build();
        }

        public void Dispose() => _subs.Dispose();
    }
}
