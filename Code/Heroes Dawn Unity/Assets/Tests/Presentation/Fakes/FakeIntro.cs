#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Login;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Première connexion de test : garde ce que le flux montre et laisse le test jouer chaque étape.</summary>
    public sealed class FakeIntroPopup : FakePopup, IIntroPopupView
    {
        UniTaskCompletionSource? _cinematic;
        UniTaskCompletionSource<string>? _campaign;

        public System.Action<float>? MainTheme { get; private set; }
        public ChooseCampaignData? Campaigns { get; private set; }

        public UniTask PlayCinematicAsync(IntroCinematicTexts texts, System.Action<float>? mainTheme, CancellationToken ct)
        {
            MainTheme = mainTheme;
            _cinematic = new UniTaskCompletionSource();
            return _cinematic.Task;
        }

        public UniTask<string> ChooseCampaignAsync(ChooseCampaignData data, CancellationToken ct)
        {
            Campaigns = data;
            _campaign = new UniTaskCompletionSource<string>();
            return _campaign.Task;
        }

        public void FinishCinematic() => _cinematic?.TrySetResult();

        public void Choose(string id) => _campaign?.TrySetResult(id);
    }
}
