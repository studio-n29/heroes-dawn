using System.IO;
using System.Linq;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class CinematicVideoSequenceTests
    {
        [Test, Description("Vérifie que le récit développé restitue les événements manquants sans imposer l'ancien plafond de trois minutes")]
        public void Narrative_HasVariableDurationsAndLoadingMargin()
        {
            Assert.That(CinematicVideoSequence.Steps.Count, Is.EqualTo(31));
            Assert.That(CinematicVideoSequence.DurationSeconds, Is.EqualTo(250f));
            foreach (var id in new[] { "S15", "S28", "S29", "S30", "S31", "S35", "S36" })
                Assert.That(CinematicVideoSequence.Steps.Select(s => s.Id), Does.Contain(id));
            Assert.That(CinematicVideoSequence.Steps.Select(s => s.Video), Does.Contain("s15"));
            Assert.That(CinematicVideoSequence.Steps.Select(s => s.Seconds).Distinct().Count(), Is.GreaterThan(2));
        }

        [Test, Description("Vérifie la lisibilité des répliques et leur ordre, indépendamment des changements d'image")]
        public void Cues_AllowReadingAndPreserveTheNarratorsVoice()
        {
            foreach (var step in CinematicVideoSequence.Steps)
            {
                Assert.That(step.Seconds, Is.GreaterThanOrEqualTo(5f), step.Id);
                Assert.That(step.Cues.Count, Is.GreaterThan(0), step.Id);
                Assert.That(step.Cues[0].At, Is.Zero, step.Id);
                for (int i = 0; i < step.Cues.Count; i++)
                {
                    var cue = step.Cues[i];
                    float until = i + 1 < step.Cues.Count ? step.Cues[i + 1].At : step.Seconds;
                    Assert.That(until - cue.At, Is.GreaterThanOrEqualTo(1.5f), step.Id);
                    Assert.That(cue.Text.Length / (until - cue.At), Is.LessThanOrEqualTo(20f), step.Id);
                    Assert.That(step.CaptionAt(cue.At), Is.EqualTo(cue.Text));
                }
                Assert.That(step.Voice, Is.Null);
            }
            Assert.That(CinematicVideoSequence.Steps.Single(s => s.Id == "S07").Caption, Does.Contain("Moi, Brie Rayner"));
            Assert.That(CinematicVideoSequence.Steps.Last().Cues.Last().Text, Does.Contain("l'Aube des Héros"));
        }

        [Test, Description("Vérifie que les phrases peuvent continuer sur plusieurs images sans retour systématique au noir")]
        public void Transitions_LinkMemoriesAndReserveBlackForChapters()
        {
            var steps = CinematicVideoSequence.Steps;
            Assert.That(steps.Count(s => s.FadeToBlack), Is.EqualTo(5));
            Assert.That(steps.Single(s => s.Id == "S38").Cues.Last().Text,
                Is.EqualTo(steps.Single(s => s.Id == "S39").Cues.First().Text));
            Assert.That(steps.Last().FadeToBlack, Is.True);
            Assert.That(steps.Select(s => s.Id).Distinct().Count(), Is.EqualTo(steps.Count));
        }

        [TestCase(5d, 8f), TestCase(6d, 10f), TestCase(10d, 5f)]
        [Description("Vérifie que toute la vidéo tient dans le plan, fondus compris, sans accélération ni coupure")]
        public void Playback_FitsTheCompleteVideo(double videoSeconds, float requestedSeconds)
        {
            float duration = CinematicVideoSequence.ShotDurationForVideo(videoSeconds, requestedSeconds);
            float speed = CinematicVideoSequence.PlaybackSpeedForDuration(videoSeconds, duration);
            Assert.That(speed, Is.GreaterThan(0f).And.LessThanOrEqualTo(1f));
            Assert.That(videoSeconds / speed, Is.EqualTo(duration).Within(0.001f));
            Assert.That(duration, Is.GreaterThanOrEqualTo(requestedSeconds));
        }

        [Test, Description("Vérifie que chaque vidéo du nouveau montage existe avec la casse utilisée sur mobile")]
        public void EverySelectedVideo_Exists()
        {
            foreach (var step in CinematicVideoSequence.Steps)
            {
                var folder = Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets/Cinematic/Video");
                Assert.That(Directory.GetFiles(folder).Select(Path.GetFileName), Does.Contain(step.Video + ".mp4"));
            }
        }
    }
}
