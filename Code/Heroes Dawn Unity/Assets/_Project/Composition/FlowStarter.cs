#nullable enable
using N29.HeroesDawn.Application.Flow;
using VContainer.Unity;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Lance la machine d'états (Boot → Login). Enregistré après les présentateurs de la racine : VContainer démarre
    /// les points d'entrée dans l'ordre d'enregistrement, donc la connexion n'est affichée qu'une fois son présentateur abonné.
    /// </summary>
    public sealed class FlowStarter : IStartable, System.IDisposable
    {
        readonly GameFlowMachine _flow;
        readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        readonly IModalService _modals;
        readonly ILocalization _loc;
        readonly IAudioService _audio;
        public FlowStarter(GameFlowMachine flow, IModalService modals, ILocalization loc, IAudioService audio)
        { _flow = flow; _modals = modals; _loc = loc; _audio = audio; }

        public void Start() => StartAsync(_lifetime.Token).Forget();
        public void Dispose() { _lifetime.Cancel(); _lifetime.Dispose(); }

        public async UniTask StartAsync(CancellationToken ct)
        {
            // Boot runs once per application launch, before the Login state is entered.
            while (!UnityEngine.Rendering.SplashScreen.isFinished)
                await UniTask.Yield(ct);
            var view = await _modals.OpenAsync<IIntroPopupView>(ct);
            try
            {
                _audio.PlayMusic(MusicId.Cinematic, 0.5f);
                await view.PlayCinematicAsync(new IntroCinematicTexts { Skip = _loc.T("intro.skip") },
                    level =>
                    {
                        _audio.Duck(DuckLayer.Modal, level, level > 0.5f ? 2.5f : 0.4f);
                    }, ct);
            }
            finally
            {
                _audio.Duck(DuckLayer.Modal, 1f, 0.4f);
                _modals.Close(view);
                _audio.PlayMusic(MusicId.MainTheme, 1f);
            }
            ct.ThrowIfCancellationRequested();
            _flow.Start();
        }
    }
}
