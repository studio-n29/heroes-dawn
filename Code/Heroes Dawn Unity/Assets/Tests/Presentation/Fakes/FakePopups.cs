#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Popups;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Popups de case de test : répondent immédiatement par la réponse préréglée.</summary>
    public sealed class FakeRestPopup : FakePopup, IRestPopupView
    {
        public bool Answer = true;
        public RestPopupData? Data;
        public UniTask<bool> ChooseAsync(RestPopupData d, CancellationToken ct) { Data = d; return UniTask.FromResult(Answer); }
    }

    public sealed class FakeNarrativePopup : FakePopup, INarrativePopupView
    {
        public NarrativeChoice Answer = NarrativeChoice.Research;
        public NarrativePopupData? Data;
        public UniTask<NarrativeChoice> ChooseAsync(NarrativePopupData d, CancellationToken ct) { Data = d; return UniTask.FromResult(Answer); }
    }

    public sealed class FakeMiniGamePopup : FakePopup, IMiniGamePopupView
    {
        public bool Answer = true;
        public UniTask<bool> PlayAsync(MiniGamePopupData d, CancellationToken ct) => UniTask.FromResult(Answer);
    }

    public sealed class FakeZoneCompletePopup : FakePopup, IZoneCompletePopupView
    {
        public ZoneCompleteData? Data;
        public UniTask ShowAsync(ZoneCompleteData d, CancellationToken ct) { Data = d; return UniTask.CompletedTask; }
    }

    /// <summary>Résolutions de case de test : enregistrent leur argument, réussissent ou échouent sur demande.</summary>
    public sealed class FakeCaseResolutions : IResolveRestUseCase, IResolveNarrativeUseCase, IResolveMiniGameUseCase
    {
        public readonly List<object> Calls = new List<object>();
        public bool Fail;
        public readonly IReadOnlyList<BoardEvent> Events = new BoardEvent[] { new FogRevealedEvent(new[] { new CaseId("n05") }) };

        UniTask<Result<IReadOnlyList<BoardEvent>>> Answer(object arg)
        {
            Calls.Add(arg);
            return UniTask.FromResult(Fail ? Result<IReadOnlyList<BoardEvent>>.Fail(new DomainError("test", "errors.network"))
                                           : Result<IReadOnlyList<BoardEvent>>.Ok(Events));
        }

        UniTask<Result<IReadOnlyList<BoardEvent>>> IResolveRestUseCase.ExecuteAsync(bool take, CancellationToken ct) => Answer(take);
        UniTask<Result<IReadOnlyList<BoardEvent>>> IResolveNarrativeUseCase.ExecuteAsync(NarrativeChoice c, CancellationToken ct) => Answer(c);
        UniTask<Result<IReadOnlyList<BoardEvent>>> IResolveMiniGameUseCase.ExecuteAsync(bool success, CancellationToken ct) => Answer(success);
    }
}
