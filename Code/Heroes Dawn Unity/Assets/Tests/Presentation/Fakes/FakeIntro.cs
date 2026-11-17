#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Presentation.Login;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Première connexion de test : garde ce que le flux montre et laisse le test jouer chaque étape.</summary>
    public sealed class FakeIntroPopup : FakePopup, IIntroPopupView
    {
        UniTaskCompletionSource? _cinematic;
        UniTaskCompletionSource? _rayner;
        UniTaskCompletionSource<AgentForm>? _form;
        UniTaskCompletionSource<string>? _start;

        public System.Action<float>? MainTheme { get; private set; }

        public CinematicKind? Kind { get; private set; }
        public UniTask PlayCinematicAsync(IntroCinematicTexts texts, System.Action<float>? mainTheme, CancellationToken ct, CinematicKind kind = CinematicKind.Launch, System.Action<string>? voiceCue = null)
        {
            MainTheme = mainTheme; Kind = kind;
            _cinematic = new UniTaskCompletionSource();
            return _cinematic.Task;
        }

        /// <summary>Les scènes de Rayner montrées, dans l'ordre.</summary>
        public List<RaynerSceneData> RaynerScenes { get; } = new List<RaynerSceneData>();
        /// <summary>Les fiches présentées au joueur (une de plus à chaque refus du domaine).</summary>
        public List<AgentFormData> Forms { get; } = new List<AgentFormData>();
        public StartChoiceData? Start { get; private set; }
        /// <summary>Ordre dans lequel les étapes ont été demandées à la vue (« rayner », « form », « start »).</summary>
        public List<string> Steps { get; } = new List<string>();

        public UniTask ShowRaynerAsync(RaynerSceneData scene, CancellationToken ct)
        {
            Steps.Add("rayner"); RaynerScenes.Add(scene);
            _rayner = new UniTaskCompletionSource();
            return _rayner.Task;
        }

        public UniTask<AgentForm> FillFormAsync(AgentFormData data, CancellationToken ct)
        {
            Steps.Add("form"); Forms.Add(data);
            _form = new UniTaskCompletionSource<AgentForm>();
            return _form.Task;
        }

        public UniTask<string> ChooseStartAsync(StartChoiceData data, CancellationToken ct)
        {
            Steps.Add("start"); Start = data;
            _start = new UniTaskCompletionSource<string>();
            return _start.Task;
        }

        public void FinishCinematic() => _cinematic?.TrySetResult();
        public void FinishRayner() => _rayner?.TrySetResult();
        public void Submit(AgentForm form) => _form?.TrySetResult(form);
        public void PickStart(string routeId) => _start?.TrySetResult(routeId);
    }
}
