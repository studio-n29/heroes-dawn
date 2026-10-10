#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Popups;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Fenêtres de jeu de test : enregistrent la demande reçue et rendent le résultat préréglé.</summary>
    public sealed class FakeStagePopup : FakePopup, IStageGameView
    {
        public ActivityResult Result = ActivityResult.Won();
        public StageRequest? Request;
        public UniTask<ActivityResult> PlayAsync(StageRequest request, CancellationToken ct) { Request = request; return UniTask.FromResult(Result); }
    }

    public sealed class FakePotionPopup : FakePopup, IPotionPopupView
    {
        public ActivityResult Result = ActivityResult.Won();
        public PotionRequest? Request;
        public UniTask<ActivityResult> PlayAsync(PotionRequest request, CancellationToken ct) { Request = request; return UniTask.FromResult(Result); }
    }

    public sealed class FakeDialoguePopup : FakePopup, IDialoguePopupView
    {
        public ActivityResult Result = ActivityResult.Won();
        public DialogueRequest? Request;
        public UniTask<ActivityResult> PlayAsync(DialogueRequest request, CancellationToken ct) { Request = request; return UniTask.FromResult(Result); }
    }
}
