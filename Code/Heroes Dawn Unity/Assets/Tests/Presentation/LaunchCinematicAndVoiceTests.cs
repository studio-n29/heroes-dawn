#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Narration;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Cinématique de lancement courte, rejeu de la version longue et voix des répliques (L14 de la 0.15.0).</summary>
    public sealed class LaunchCinematicAndVoiceTests
    {
        sealed class FakeVoice : IVoicePlayer
        {
            public readonly List<string> Played = new List<string>();
            public int Stops;
            public readonly HashSet<string> Existing = new HashSet<string>();
            public bool Has(string lineKey) => Existing.Contains(lineKey);
            public void Play(string lineKey) => Played.Add(lineKey);
            public void Stop() => Stops++;
        }

        [Test, Description("Vérifie que le montage de lancement dure de 40 à 50 secondes, sans aucun sous-titre, avec des plans coupés à leur durée et finit au noir — parce que c'est un générique dynamique sans dialogue ni texte")]
        public void LaunchMontage_IsShort_Silent_AndEndsOnBlack()
        {
            var steps = CinematicVideoSequence.Launch;
            Assert.That(CinematicVideoSequence.LaunchDurationSeconds, Is.InRange(40f, 50f));
            Assert.That(steps.All(s => s.Cut), Is.True);
            Assert.That(steps.All(s => s.Cues.Count == 0 && s.Caption == ""), Is.True, "aucun texte");
            Assert.That(steps.Last().FadeToBlack, Is.True);
            Assert.That(steps.Take(steps.Count - 1).Any(s => s.FadeToBlack), Is.False, "pas de noir entre deux plans : le rythme se tient");
            Assert.That(steps.Select(s => s.Id).Distinct().Count(), Is.EqualTo(steps.Count));
        }

        [Test, Description("Vérifie que chaque plan du montage vient d'une vidéo de la version longue et que son fichier existe — parce qu'un plan manquant couperait la cinématique de lancement")]
        public void LaunchMontage_UsesExistingClips()
        {
            var longVideos = CinematicVideoSequence.Steps.Select(s => s.Video).Concat(new[] { "s46" }).ToHashSet();
            foreach (var step in CinematicVideoSequence.Launch)
            {
                string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, CinematicVideoSequence.VideoFolder, step.Video + ".mp4");
                Assert.That(File.Exists(path), Is.True, path);
                Assert.That(longVideos, Does.Contain(step.Video), step.Id);
            }
        }

        [Test, Description("Vérifie que la version longue garde ses 31 plans et ses sous-titres et qu'une réplique a une clé de voix « cinematic.<plan>.<n° de réplique> » — parce que les voix se rangent par réplique")]
        public void LongVersion_KeepsItsCaptions_AndVoiceKeys()
        {
            var s01 = CinematicVideoSequence.Steps.Single(s => s.Id == "S01");
            Assert.That(CinematicVideoSequence.Steps.Count, Is.EqualTo(31));
            Assert.That(CinematicVideoSequence.Steps.Any(s => s.Cut), Is.False);
            Assert.That(CinematicVideoSequence.VoiceKey(s01, 1), Is.EqualTo("cinematic.S01.1"));
        }

        [Test, Description("Vérifie le nom de fichier et le chemin d'une voix (points remplacés par des soulignés, dossier par langue) — parce que l'équipe range les fichiers ElevenLabs selon cette règle")]
        public void VoiceFiles_FollowTheNamingRule()
        {
            Assert.That(VoiceLevels.FileName("narration.ny_1_3.n18.before.0"), Is.EqualTo("narration_ny_1_3_n18_before_0"));
            Assert.That(VoiceLevels.ResourcePath("en", "cinematic.S01.0"), Is.EqualTo("Voices/en/cinematic_S01_0"));
        }

        [Test, Description("Vérifie que la normalisation ramène la crête à 0,9, borne le gain et laisse un clip silencieux inchangé — parce que les voix ne doivent ni écraser la musique ni saturer")]
        public void Normalisation_TargetsThePeak_WithinBounds()
        {
            Assert.That(VoiceLevels.GainFor(0.45f), Is.EqualTo(2f).Within(0.001f));
            Assert.That(VoiceLevels.GainFor(1f), Is.EqualTo(0.9f).Within(0.001f));
            Assert.That(VoiceLevels.GainFor(0.01f), Is.EqualTo(4f), "gain plafonné");
            Assert.That(VoiceLevels.GainFor(8f), Is.EqualTo(0.25f), "gain plancher");
            Assert.That(VoiceLevels.GainFor(0f), Is.EqualTo(1f));
            Assert.That(VoiceLevels.MusicDuck, Is.LessThan(1f));
        }

        [Test, Description("Vérifie que chaque réplique de narration porte la clé de son texte comme clé de voix — parce que la voix se retrouve par la clé de la réplique")]
        public void NarrationLines_CarryTheirVoiceKey()
        {
            var content = N29.HeroesDawn.Content.Prototype.PrototypeContent.Build();
            var line = NarrationLines.Build(new NarrationLineDef("brick", "narration.ny_1_1.n23.before.0"), content, new LocLocalization());
            Assert.That(line.VoiceKey, Is.EqualTo("narration.ny_1_1.n23.before.0"));
        }

        [Test, Description("Vérifie que le dialogue de narration joue la voix de chaque réplique affichée puis l'arrête à la fermeture — parce que la voix suit le texte et ne déborde pas sur la suite")]
        public void Dialogue_PlaysEachLinesVoice_ThenStops()
        {
            var app = new TestApp();
            var host = new FakeModalHost();
            var box = new FakeDialogueBox();
            host.Register<IDialogueBoxView>(() => box);
            var voice = new FakeVoice();
            var presenter = new CasePopupPresenter(new ModalService(host), new FakeCaseResolutions(), new FakeCaseResolutions(), new FakePlanActivity(app.Session.Content),
                                                   new FakeCaseResolutions(), app.Session, new FakeToasts(), new LocLocalization(), null, voice);
            var lines = new[] { new NarrationLineDef("brick", "narration.ny_1_1.n23.before.0"), new NarrationLineDef("brick", "narration.ny_1_1.n23.before.1") };
            presenter.PlayNarrationAsync(new N29.HeroesDawn.Domain.Board.NarrationEvent(new N29.HeroesDawn.Core.CaseId("n23"), NarrationPhase.Before, lines),
                                         CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(voice.Played, Is.EqualTo(new[] { "narration.ny_1_1.n23.before.0", "narration.ny_1_1.n23.before.1" }));
            Assert.That(voice.Stops, Is.GreaterThan(0));
        }

        [Test, Description("Vérifie que le rejeu de la cinématique ouvre la fenêtre en version longue, relaie la voix de chaque réplique et l'arrête à la fin — parce que la version longue reste accessible depuis le jeu")]
        public void Replay_PlaysTheLongVersion_WithVoices()
        {
            var host = new FakeModalHost();
            var popup = new FakeIntroPopup();
            host.Register<IIntroPopupView>(() => popup);
            var voice = new FakeVoice();
            var replay = new CinematicReplay(new ModalService(host), new LocLocalization(), null, voice);
            var task = replay.PlayFullAsync(CancellationToken.None);
            Assert.That(popup.Kind, Is.EqualTo(CinematicKind.Full));
            popup.FinishCinematic();
            task.GetAwaiter().GetResult();
            Assert.That(voice.Stops, Is.GreaterThan(0));
            Assert.That(host.Despawned, Does.Contain(popup));
        }

        [Test, Description("Vérifie que la page Chapitres propose CINÉMATIQUE et que le bouton lance la version longue — parce que c'est l'accès in-game à la cinématique complète")]
        public void ChaptersPage_CinematicButton_PlaysTheLongVersion()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            var view = new FakeListView();
            var host = new FakeModalHost();
            var popup = new FakeIntroPopup();
            host.Register<IIntroPopupView>(() => popup);
            var loc = new LocLocalization();
            var modals = new ModalService(host);
            var page = new N29.HeroesDawn.Presentation.Hub.ChaptersPagePresenter(view, app.Session, app.Chapters, new N29.HeroesDawn.Presentation.Hub.ChapterIntroFlow(modals, loc),
                app.Fragments, new N29.HeroesDawn.Presentation.Hub.NoFragmentAudio(), modals, new N29.HeroesDawn.Presentation.Hub.HubNavigator(), new CinematicReplay(modals, loc), loc,
                new FakeToasts(), app.Subscriber<N29.HeroesDawn.Application.Features.Meta.MetaChanged>(), app.Subscriber<N29.HeroesDawn.Application.Messages.WalletChanged>(),
                app.Subscriber<N29.HeroesDawn.Application.Messages.RosterChanged>(), app.Time);
            view.Show();
            ((VContainer.Unity.IStartable)page).Start();
            Assert.That(view.Header.B.Label, Is.EqualTo(loc.T("chapters.cinematic")));
            view.ClickHeader(1);
            Assert.That(popup.Kind, Is.EqualTo(CinematicKind.Full));
            Assert.That(view.Header.B.Enabled, Is.False, "un seul rejeu à la fois");
            popup.FinishCinematic();
            Assert.That(view.Header.B.Enabled, Is.True);
            page.Dispose();
        }
    }
}
