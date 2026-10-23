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

        [Test, Description("Vérifie que le découpage compte 46 plans numérotés S01 à S46, sans doublon ni trou — parce que le document de cinématique les numérote ainsi et que le montage s'y réfère")]
        public void Shots_AreNumberedS01ToS46()
        {
            Assert.That(Shots.Count, Is.EqualTo(46));
            for (int i = 0; i < Shots.Count; i++) Assert.That(Shots[i].Id, Is.EqualTo("S" + (i + 1).ToString("D2")));
        }

        [Test, Description("Vérifie que chaque illustration citée par un plan existe dans Resources/Cinematic/Art — parce qu'un plan sans image jouerait un écran noir")]
        public void EveryArtKey_Resolves()
        {
            var keys = Shots.SelectMany(s => new[] { s.Art }.Concat(s.Parts ?? new string[0])).Where(k => k != null).Distinct();
            foreach (var key in keys) Assert.That(Resources.Load<Sprite>("Cinematic/Art/" + key), Is.Not.Null, "illustration manquante : " + key);
        }

        [Test, Description("Vérifie que chaque plan parlé a sa voix off enregistrée et son sous-titre, et chaque effet sonore son fichier — parce que la cinématique se cale sur la voix et ne doit rien laisser muet par erreur")]
        public void EveryVoiceAndSfx_Exists()
        {
            foreach (var shot in Shots)
            {
                if (shot.Voice)
                {
                    Assert.That(Resources.Load<AudioClip>("Cinematic/Voice/vo_" + shot.Id), Is.Not.Null, "voix manquante : " + shot.Id);
                    Assert.That(Loc.Has("cinematic.vo." + shot.Id), Is.True, "sous-titre manquant : " + shot.Id);
                }
                if (shot.Sfx != null) Assert.That(Resources.Load<AudioClip>("Cinematic/Audio/" + shot.Sfx), Is.Not.Null, "son manquant : " + shot.Sfx);
            }
            foreach (var bed in new[] { "mus_cold", "mus_war", "mus_piano" }) Assert.That(Resources.Load<AudioClip>("Cinematic/Audio/" + bed), Is.Not.Null, bed);
        }

        [Test, Description("Vérifie que les plans muets du document (S04, S40, S46) n'ont pas de voix et que la liste des voix correspond aux plans parlés — parce que le silence est un choix de montage, pas un oubli")]
        public void SilentShots_AreTheDocumentedOnes()
        {
            var silent = Shots.Where(s => !s.Voice).Select(s => s.Id);
            Assert.That(silent, Is.EqualTo(new[] { "S04", "S40", "S46" }));
            Assert.That(Loc.CinematicVoiceIds, Is.EquivalentTo(Shots.Where(s => s.Voice).Select(s => s.Id)));
        }

        [Test, Description("Vérifie que la durée minimale cumulée des plans (hors voix) approche les 4 minutes du document — parce que la cinématique doit rester un récap d'environ quatre minutes")]
        public void MinimumDuration_IsAboutFourMinutes()
        {
            float total = Shots.Sum(s => s.Seconds);
            Assert.That(total, Is.InRange(220f, 270f));
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

        [Test, Description("Vérifie que les titres d'époque du montage (1973, 1981, URSS, 1991, 2026) sont posés sur les bons plans — parce que le document les place au début de chaque séquence")]
        public void EraTitles_AreOnTheDocumentedShots()
        {
            var titled = Shots.Where(s => s.Title != null).Select(s => s.Id + ":" + s.Title);
            Assert.That(titled, Is.EqualTo(new[] { "S02:1973", "S05:1981", "S11:URSS", "S12:1991", "S15:2026" }));
        }
    }
}
