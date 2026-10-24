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

    public sealed class FakeHeroGamePopup : FakePopup, IHeroGamePopupView
    {
        public ActivityResult Result = ActivityResult.Won();
        public HeroGameRequest? Request;
        public UniTask<ActivityResult> PlayAsync(HeroGameRequest request, CancellationToken ct) { Request = request; return UniTask.FromResult(Result); }
    }

    public sealed class FakePotionPopup : FakePopup, IPotionPopupView
    {
        public ActivityResult Result = ActivityResult.Won();
        public PotionRequest? Request;
        public UniTask<ActivityResult> PlayAsync(PotionRequest request, CancellationToken ct) { Request = request; return UniTask.FromResult(Result); }
    }

    /// <summary>Boîte de dialogue de narration de test : enregistre les répliques reçues.</summary>
    public sealed class FakeDialogueBox : FakePopup, N29.HeroesDawn.Presentation.Narration.IDialogueBoxView
    {
        public readonly System.Collections.Generic.List<N29.HeroesDawn.Presentation.Narration.DialogueBoxLine> Played = new System.Collections.Generic.List<N29.HeroesDawn.Presentation.Narration.DialogueBoxLine>();
        public UniTask PlayAsync(System.Collections.Generic.IReadOnlyList<N29.HeroesDawn.Presentation.Narration.DialogueBoxLine> lines, CancellationToken ct) { Played.AddRange(lines); return UniTask.CompletedTask; }
    }

    public sealed class FakeDialoguePopup : FakePopup, IDialoguePopupView
    {
        public ActivityResult Result = ActivityResult.Won();
        public DialogueRequest? Request;
        public UniTask<ActivityResult> PlayAsync(DialogueRequest request, CancellationToken ct) { Request = request; return UniTask.FromResult(Result); }
    }
}
