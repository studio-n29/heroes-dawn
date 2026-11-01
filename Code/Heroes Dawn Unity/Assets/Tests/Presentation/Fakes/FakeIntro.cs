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
        UniTaskCompletionSource<CharacterChoice>? _character;

        public System.Action<float>? MainTheme { get; private set; }
        public ChooseCharacterData? Character { get; private set; }

        public UniTask PlayCinematicAsync(IntroCinematicTexts texts, System.Action<float>? mainTheme, CancellationToken ct)
        {
            MainTheme = mainTheme;
            _cinematic = new UniTaskCompletionSource();
            return _cinematic.Task;
        }

        public UniTask<CharacterChoice> ChooseCharacterAsync(ChooseCharacterData data, CancellationToken ct)
        {
            Character = data;
            _character = new UniTaskCompletionSource<CharacterChoice>();
            return _character.Task;
        }

        public void FinishCinematic() => _cinematic?.TrySetResult();

        public void Choose(string name, string avatar) => _character?.TrySetResult(new CharacterChoice(name, avatar));
    }
}
