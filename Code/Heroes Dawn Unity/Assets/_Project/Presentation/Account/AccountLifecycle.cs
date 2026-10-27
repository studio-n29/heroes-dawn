#nullable enable
using System;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Account
{
    /// <summary>
    /// Vide la synchronisation cloud quand l'application passe en arrière-plan (le système peut la tuer ensuite) et à la fermeture :
    /// la dernière progression part sans attendre l'intervalle de 20 s.
    /// </summary>
    public sealed class AccountLifecycle : IInitializable, IDisposable
    {
        readonly CloudSync _sync;

        public AccountLifecycle(CloudSync sync) { _sync = sync; }

        public void Initialize()
        {
            UnityEngine.Application.focusChanged += OnFocusChanged;
            UnityEngine.Application.quitting += OnQuit;
        }

        public void Dispose()
        {
            UnityEngine.Application.focusChanged -= OnFocusChanged;
            UnityEngine.Application.quitting -= OnQuit;
        }

        public void OnFocusChanged(bool hasFocus)
        {
            if (!hasFocus) _sync.FlushAsync(default).Forget();
        }

        public void OnQuit() => _sync.FlushAsync(default).Forget();
    }
}
