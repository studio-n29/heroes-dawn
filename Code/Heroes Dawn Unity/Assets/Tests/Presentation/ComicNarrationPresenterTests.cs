#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Narration;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Onomatopées de bande dessinée et boîte de dialogue de narration (plan 2026-10-10, sous-projets H et I).</summary>
    public sealed class ComicNarrationPresenterTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();
        static readonly LocLocalization L = new LocLocalization();

        [Test, Description("Vérifie que les cases d'action ont une onomatopée et que les cases sans action n'en ont pas — parce que seules les entrées en combat, événement, défi et repos se jouent en bande dessinée")]
        public void Cues_CoverTheActionCases()
        {
            Assert.That(ComicCues.ForCase(CaseType.Combat), Is.EqualTo(ComicCue.Combat));
            Assert.That(ComicCues.ForCase(CaseType.Narrative), Is.EqualTo(ComicCue.Event));
            Assert.That(ComicCues.ForCase(CaseType.MiniGame), Is.EqualTo(ComicCue.Minigame));
            Assert.That(ComicCues.ForCase(CaseType.Rest), Is.EqualTo(ComicCue.Rest));
            foreach (var t in new[] { CaseType.Entry, CaseType.Empty, CaseType.Resource }) Assert.That(ComicCues.ForCase(t), Is.Null, t.ToString());
        }

        [Test, Description("Vérifie que chaque onomatopée a un texte français en majuscules avec point d'exclamation et une inclinaison non nulle — parce que l'étoile doit toujours avoir l'air d'une bulle de bande dessinée")]
        public void Cues_HaveATextAndAPose()
        {
            foreach (ComicCue cue in System.Enum.GetValues(typeof(ComicCue)))
            {
                string key = ComicCues.TextKey(cue);
                Assert.That(L.Has(key), Is.True, key);
                string text = L.T(key);
                Assert.That(text, Does.EndWith("!"), key);
                Assert.That(text, Is.EqualTo(text.ToUpperInvariant()), key);
                Assert.That(ComicCues.TiltOf(cue), Is.Not.EqualTo(0f));
            }
        }

        [Test, Description("Vérifie que les répliques d'un agent prennent son nom de code, son portrait et le côté gauche, et celles d'un autre personnage le côté droit — parce que la bulle doit montrer qui parle et d'où")]
        public void Lines_ResolveSpeakers()
        {
            var agent = C.Members.First();
            var line = NarrationLines.Build(new NarrationLineDef(agent.Id.Value, "narration.ny_1_1.n23.after.1"), C, L);
            Assert.That(line.Speaker, Is.EqualTo(L.MemberName(agent)));
            Assert.That(line.Left, Is.True);
            Assert.That(line.Text, Does.Contain("grue"));

            var npc = NarrationLines.Build(new NarrationLineDef("brick", "narration.ny_1_1.n23.before.0"), C, L);
            Assert.That(npc.Speaker, Is.EqualTo("Brick"));
            Assert.That(npc.Left, Is.False);
            var again = NarrationLines.Build(new NarrationLineDef("brick", "narration.ny_1_1.n23.before.0"), C, L);
            Assert.That(again.Portrait.Family, Is.EqualTo(npc.Portrait.Family), "la couleur d'un personnage est stable");

            var unknown = NarrationLines.Build(new NarrationLineDef("dockmaster2", "narration.absent.key"), C, L);
            Assert.That(unknown.Speaker, Is.EqualTo("Dockmaster2"), "repli lisible sans traduction");
            Assert.That(unknown.Text, Is.EqualTo("key"), "repli sur la fin de la clé");
        }

        [Test, Description("Vérifie que le texte d'une réplique saisi dans l'éditeur (clé surchargée) remplace le repli — parce que le level designer écrit ses dialogues sans toucher au code")]
        public void Lines_UseOverriddenTexts()
        {
            Loc.Override(new Dictionary<string, string> { ["narration.test.k"] = "Salut, le quai !" });
                var line = NarrationLines.Build(new NarrationLineDef("brick", "narration.test.k"), C, L);
                Assert.That(line.Text, Is.EqualTo("Salut, le quai !"));

        }
    }
}
