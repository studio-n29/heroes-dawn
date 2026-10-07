#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
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

    public sealed class FakeActivityPopup : FakePopup, IActivityPopupView
    {
        public bool Win = true;
        public ActivityPopupData? Data;
        public UniTask<ActivityResult> PlayAsync(ActivityPopupData d, CancellationToken ct)
        {
            Data = d;
            return UniTask.FromResult(Win ? ActivityResult.Won(d.WinScore) : ActivityResult.Lost(d.FailScore));
        }
    }

    /// <summary>Planification de test : rend les activités demandées dans l'ordre (la dernière se répète), générées comme en jeu.</summary>
    public sealed class FakePlanActivity : IPlanActivityUseCase
    {
        readonly ContentCatalog _content;
        readonly List<string> _ids = new List<string>();
        public readonly List<ActivityPlan> Plans = new List<ActivityPlan>();
        public IReadOnlyList<MemberId> Squad = new[] { new MemberId("ancre"), new MemberId("givre") };

        public FakePlanActivity(ContentCatalog content, params string[] activityIds)
        {
            _content = content;
            _ids.AddRange(activityIds.Length == 0 ? new[] { ActivityIds.Reflex.Value } : activityIds);
        }

        public Result<ActivityPlan> Execute()
        {
            string id = _ids[System.Math.Min(Plans.Count, _ids.Count - 1)];
            var plan = ActivityPlanner.Plan(ActivityRegistry.Default, _content.Activity(new ActivityId(id)), new ZoneEnvironment(Biome.Harbor, Weather.Night),
                                            1UL, new CaseId("n07"), Plans.Count + 1, Squad);
            Plans.Add(plan);
            return Result<ActivityPlan>.Ok(plan);
        }
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
    public sealed class FakeCaseResolutions : IResolveRestUseCase, IResolveNarrativeUseCase, IResolveActivityUseCase
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
        /// <summary>Événements rendus pour une activité (par défaut : <see cref="Events"/>).</summary>
        public System.Func<ActivityResult, IReadOnlyList<BoardEvent>>? ActivityEvents;

        public UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(ActivityResult result, CancellationToken ct = default)
        {
            Calls.Add(result);
            if (Fail) return UniTask.FromResult(Result<IReadOnlyList<BoardEvent>>.Fail(new DomainError("test", "errors.network")));
            return UniTask.FromResult(Result<IReadOnlyList<BoardEvent>>.Ok(ActivityEvents != null ? ActivityEvents(result) : Events));
        }
    }
}
