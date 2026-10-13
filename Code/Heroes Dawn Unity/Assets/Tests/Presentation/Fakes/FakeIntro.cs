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
        UniTaskCompletionSource<string>? _pseudo;
        UniTaskCompletionSource<string>? _campaign;
        Func<string, string?>? _validate;

        public IReadOnlyList<IntroSlideData>? Slides { get; private set; }
        public PseudoPromptData? Prompt { get; private set; }
        public ChooseCampaignData? Campaigns { get; private set; }
        public string? LastError { get; private set; }

        public UniTask PlayCinematicAsync(IReadOnlyList<IntroSlideData> slides, IntroCinematicTexts texts, CancellationToken ct)
        {
            Slides = slides;
            _cinematic = new UniTaskCompletionSource();
            return _cinematic.Task;
        }

        public UniTask<string> AskPseudoAsync(PseudoPromptData data, Func<string, string?> validate, CancellationToken ct)
        {
            Prompt = data;
            _validate = validate;
            _pseudo = new UniTaskCompletionSource<string>();
            return _pseudo.Task;
        }

        public UniTask<string> ChooseCampaignAsync(ChooseCampaignData data, CancellationToken ct)
        {
            Campaigns = data;
            _campaign = new UniTaskCompletionSource<string>();
            return _campaign.Task;
        }

        public void FinishCinematic() => _cinematic?.TrySetResult();

        /// <summary>Le joueur valide ce pseudo : refusé (texte d'erreur dans <see cref="LastError"/>) ou accepté.</summary>
        public void SubmitPseudo(string text)
        {
            LastError = _validate!(text);
            if (LastError == null) _pseudo!.TrySetResult(text);
        }

        public void Choose(string id) => _campaign?.TrySetResult(id);
    }
}
