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

        [Test, Description("Vérifie que la version courte garde 12 plans du découpage, dans l ordre du document, de S01 à S46 — parce que la cinématique doit condenser l histoire sans la réordonner")]
        public void Shots_AreTheCondensedCut()
        {
            Assert.That(Shots.Select(s => s.Id), Is.EqualTo(new[] { "S01", "S02", "S10", "S11", "S15", "S20", "S24", "S33", "S37", "S41", "S44", "S46" }));
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
            Assert.That(silent, Is.EqualTo(new[] { "S46" }));
            Assert.That(Loc.CinematicVoiceIds, Is.EquivalentTo(Shots.Where(s => s.Voice).Select(s => s.Id)));
        }

        [Test, Description("Vérifie que la durée réelle (voix comprise) reste sous la minute — parce que la cinématique est un condensé de l histoire, pas un film")]
        public void TotalDuration_StaysUnderAMinute()
        {
            float total = Shots.Sum(s => Mathf.Max(s.Seconds, s.Voice ? 0.8f + Resources.Load<AudioClip>("Cinematic/Voice/vo_" + s.Id).length : 0f));
            Assert.That(total, Is.InRange(40f, 57f));
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
