using System;
using System.IO;
using N29.HeroesDawn.EditorTools.Testing;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class TestReportWriterTests
    {
        static TestRunInfo Run() => new TestRunInfo
        {
            Platform = "EditMode", StartedLocal = new DateTime(2026, 10, 3, 9, 30, 0), DurationSeconds = 1.5,
            Branch = "architecture/scenes-flux", Commit = "abc1234",
        };

        static TestReportEntry Entry(string fixture, string name, string description, TestOutcome outcome, string message)
            => new TestReportEntry
            {
                Fixture = fixture, Name = name, Description = description, Outcome = outcome,
                DurationSeconds = 0.01, Message = message, StackFirstLine = outcome == TestOutcome.Failed ? "at Foo.Bar()" : "",
            };

        [Test, Description("Vérifie que le rapport affiche le nom, la description, le résultat et le message d'échec de chaque test — parce que l'utilisateur veut savoir ce qui marche et pourquoi")]
        public void Render_ShowsNameDescriptionOutcomeAndFailure()
        {
            var html = TestReportWriter.Render(Run(), new[]
            {
                Entry("A", "Ok_Test", "Vérifie X — parce que Y", TestOutcome.Passed, ""),
                Entry("A", "Ko_Test", "Vérifie Z — parce que W", TestOutcome.Failed, "Expected 2 but was 3"),
            });
            StringAssert.Contains("Ok_Test", html);
            StringAssert.Contains("Vérifie X — parce que Y", html);
            StringAssert.Contains("Expected 2 but was 3", html);
            StringAssert.Contains("1 réussi", html);
            StringAssert.Contains("1 échoué", html);
            StringAssert.Contains("architecture/scenes-flux", html);
        }

        [Test, Description("Vérifie que le texte des tests est échappé en HTML — parce qu'un message d'échec contenant des chevrons casserait la page du rapport")]
        public void Render_EscapesHtml()
        {
            var html = TestReportWriter.Render(Run(), new[] { Entry("A", "T", "<b>x</b>", TestOutcome.Failed, "a < b & c") });
            StringAssert.Contains("&lt;b&gt;x&lt;/b&gt;", html);
            StringAssert.Contains("a &lt; b &amp; c", html);
            StringAssert.DoesNotContain("<b>x</b>", html);
        }

        [Test, Description("Vérifie que GitInfo lit la branche et le commit dans .git sans lancer git — parce que tout doit tourner dans Unity")]
        public void GitInfo_ReadsHeadAndRef()
        {
            var root = Path.Combine(Path.GetTempPath(), "hd_gitinfo_" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(Path.Combine(root, ".git", "refs", "heads"));
                File.WriteAllText(Path.Combine(root, ".git", "HEAD"), "ref: refs/heads/demo\n");
                File.WriteAllText(Path.Combine(root, ".git", "refs", "heads", "demo"), "abc1234def\n");
                var (branch, commit) = GitInfo.Read(root);
                Assert.That(branch, Is.EqualTo("demo"));
                Assert.That(commit, Is.EqualTo("abc1234"));
                Assert.That(GitInfo.Read(Path.Combine(root, "absent")), Is.EqualTo(("?", "?")));
            }
            finally { Directory.Delete(root, true); }
        }
    }
}
