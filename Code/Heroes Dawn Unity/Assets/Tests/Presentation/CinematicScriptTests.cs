using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class CinematicScriptTests
    {
        static IReadOnlyList<CinematicShot> Shots => CinematicScript.Shots;

        [Test, Description("Vérifie que la version courte garde 11 plans du découpage, dans l ordre du document, de S01 à S46 — parce que la cinématique doit condenser l histoire sans la réordonner")]
        public void Shots_AreTheCondensedCut()
        {
            Assert.That(Shots.Select(s => s.Id), Is.EqualTo(new[] { "S01", "S02", "S10", "S11", "S15", "S24", "S33", "S37", "S41", "S44", "S46" }));
        }

        [Test, Description("Vérifie que chaque illustration citée par un plan existe dans Resources/Cinematic/Art — parce qu'un plan sans image jouerait un écran noir")]
        public void EveryArtKey_Resolves()
        {
            var keys = Shots.SelectMany(s => new[] { s.Art }.Concat(s.Parts ?? new string[0])).Where(k => k != null).Distinct();
            foreach (var key in keys) Assert.That(Resources.Load<Sprite>("Cinematic/Art/" + key), Is.Not.Null, "illustration manquante : " + key);
        }

        [Test, Description("Vérifie que chaque plan a sa légende (la voix off est facultative) et chaque effet sonore son fichier, et chaque effet sonore son fichier — parce que la cinématique se cale sur la voix et ne doit rien laisser muet par erreur")]
        public void EveryVoiceAndSfx_Exists()
        {
            foreach (var shot in Shots)
            {
                if (shot.Voice) Assert.That(Loc.T("cinematic.vo." + shot.Id), Is.Not.Empty.And.Not.StartWith("cinematic.vo"), "sous-titre manquant : " + shot.Id);
                if (shot.Sfx != null) Assert.That(Resources.Load<AudioClip>("Cinematic/Audio/" + shot.Sfx), Is.Not.Null, "son manquant : " + shot.Sfx);
            }
            foreach (var bed in new[] { "mus_cold", "mus_war", "mus_piano" }) Assert.That(Resources.Load<AudioClip>("Cinematic/Audio/" + bed), Is.Not.Null, bed);
        }

        [Test, Description("Vérifie que tous les plans ont une légende (aucun plan muet) et que la liste des plans à légende correspond au script — parce que la narration du PDF couvre toute la cinématique, voix ou non")]
        public void NoShotIsSilent()
        {
            Assert.That(Shots.Where(s => !s.Voice), Is.Empty);
            Assert.That(Loc.CinematicVoiceIds, Is.EquivalentTo(Shots.Select(s => s.Id)));
        }

        [Test, Description("Vérifie qu'aucun fichier de voix off ne reste dans les ressources (les anciennes voix disaient l'ancien texte) — parce que la cinématique doit marcher sans voix, et jouer une voix dès qu'un vo_<plan> est déposé")]
        public void NoVoiceFilesRemain()
        {
            foreach (var shot in Shots) Assert.That(Resources.Load<AudioClip>("Cinematic/Voice/vo_" + shot.Id), Is.Null, shot.Id);
        }

        [Test, Description("Vérifie que la durée totale au temps de lecture (sans voix) reste entre 60 et 170 secondes — parce que la narration du PDF est longue mais la cinématique reste un condensé")]
        public void TotalDuration_FollowsTheReadingTime()
        {
            float total = Shots.Sum(s => CinematicTiming.ShotSeconds(s.Seconds, 0f, Loc.T("cinematic.vo." + s.Id)));
            Assert.That(total, Is.InRange(60f, 170f));
        }

        [Test, Description("Vérifie que chaque légende fait de 3 à 45 mots — parce qu'une légende trop longue déborde de l'écran")]
        public void NoCaptionIsEmptyOrTooLong()
        {
            foreach (var s in Shots)
            {
                int words = CinematicTiming.Words(Loc.T("cinematic.vo." + s.Id));
                Assert.That(words, Is.InRange(3, 45), s.Id);
            }
        }

        [Test, Description("Vérifie que les poses de caméra restent dans l'image (échelle ≥ 1, point regardé entre 0 et 1) et que les lueurs sont placées dans l'image — parce qu'un cadrage hors image montrerait du noir")]
        public void CameraPoses_StayInsideTheImage()
        {
            foreach (var s in Shots)
            {
                foreach (var p in new[] { s.From, s.To })
                {
                    Assert.That(p.Scale, Is.InRange(1f, 5f), s.Id);
                    Assert.That(p.Focus.x, Is.InRange(0f, 1f), s.Id);
                    Assert.That(p.Focus.y, Is.InRange(0f, 1f), s.Id);
                }
                if (s.Glows != null) foreach (var g in s.Glows) { Assert.That(g.Pos.x, Is.InRange(0f, 1f), s.Id); Assert.That(g.Pos.y, Is.InRange(0f, 1f), s.Id); }
            }
        }

        [Test, Description("Vérifie que les titres d époque du montage (1973, URSS, 2026) sont posés sur les bons plans — parce que le document les place au début de chaque séquence")]
        public void EraTitles_AreOnTheDocumentedShots()
        {
            var titled = Shots.Where(s => s.Title != null).Select(s => s.Id + ":" + s.Title);
            Assert.That(titled, Is.EqualTo(new[] { "S02:1973", "S11:URSS", "S15:2026" }));
        }
    }
}
