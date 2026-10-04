#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Wip;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class CounterRollTests
    {
        [Test, Description("Vérifie que le compteur finit exactement sur la nouvelle valeur — parce que le solde affiché doit être juste à la fin de l'animation")]
        public void At_EndsOnTarget() => Assert.That(CounterRoll.At(800, 350, 1f), Is.EqualTo(350));

        [Test, Description("Vérifie que le compteur part de l'ancienne valeur — parce qu'un saut au début casserait l'effet de roulement")]
        public void At_StartsOnFrom() => Assert.That(CounterRoll.At(800, 350, 0f), Is.EqualTo(800));

        [Test, Description("Vérifie que le compteur avance toujours dans le même sens — parce qu'un compteur qui recule puis avance se lit comme une erreur")]
        public void At_Monotonic()
        {
            int prev = CounterRoll.At(100, 1600, 0f);
            for (int i = 1; i <= 20; i++)
            {
                int v = CounterRoll.At(100, 1600, i / 20f);
                Assert.That(v, Is.GreaterThanOrEqualTo(prev));
                prev = v;
            }
        }

        [Test, Description("Vérifie que la durée du roulement reste entre 0,25 s et 0,8 s selon l'écart — parce qu'un feedback ne doit jamais faire attendre (spec §6)")]
        public void DurationFor_ClampedBetween025And08()
        {
            Assert.That(CounterRoll.DurationFor(100, 101), Is.EqualTo(0.25f).Within(1e-4));
            Assert.That(CounterRoll.DurationFor(0, 1_000_000), Is.EqualTo(0.8f).Within(1e-4));
            float mid = CounterRoll.DurationFor(800, 350);
            Assert.That(mid, Is.InRange(0.25f, 0.8f));
        }
    }

    public sealed class StaggerTests
    {
        [Test, Description("Vérifie que chaque carte entre 40 ms après la précédente — parce que la cascade guide l'œil (spec §6)")]
        public void DelayFor_IncreasesByStep()
        {
            Assert.That(Stagger.DelayFor(0), Is.EqualTo(0f));
            Assert.That(Stagger.DelayFor(3), Is.EqualTo(0.12f).Within(1e-5));
        }

        [Test, Description("Vérifie que le décalage plafonne — parce qu'une longue liste ne doit pas faire attendre ses dernières cartes")]
        public void DelayFor_CappedAtMax() => Assert.That(Stagger.DelayFor(50), Is.EqualTo(Stagger.DelayFor(12)));
    }

    public sealed class MotionSettingsTests
    {
        [TearDown]
        public void TearDown() => MotionSettings.Reduced = false;

        [Test, Description("Vérifie que les mouvements réduits sont désactivés par défaut — parce que les feedbacks sont actifs tant que les Paramètres ne les coupent pas (contraintes)")]
        public void Reduced_DefaultsToFalse()
        {
            MotionSettings.ResetForTests();
            Assert.That(MotionSettings.Reduced, Is.False);
        }

        [Test, Description("Vérifie qu'avec les mouvements réduits la secousse et l'éclat ne jouent pas — parce que ce réglage d'accessibilité coupe secousses, flashs et éclats (spec §6)")]
        public void ShakeAndBurst_SkippedWhenReduced()
        {
            MotionSettings.Reduced = true;
            var go = new GameObject("x", typeof(RectTransform));
            try
            {
                Assert.That(Fx.Shake((RectTransform)go.transform), Is.False);
                Assert.That(Fx.Burst(go.transform), Is.False);
                Assert.That(go.transform.localScale, Is.EqualTo(Vector3.one));
            }
            finally { Object.DestroyImmediate(go); }
        }
    }

    public sealed class UiButtonFeedbackTests
    {
        GameObject _root = null!;
        UiTapRelay _relay = null!;
        UiButton _button = null!;
        string? _reason;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform));
            _relay = _root.AddComponent<UiTapRelay>();
            _relay.ReasonTapped += r => _reason = r;
            _reason = null;
            var go = new GameObject("Button", typeof(RectTransform));
            go.transform.SetParent(_root.transform, false);
            var img = go.AddComponent<Image>();
            var b = go.AddComponent<Button>();
            var text = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(go.transform, false);
            _button = go.AddComponent<UiButton>();
            _button.Init(b, img, text, new Color(0.2f, 0.2f, 0.2f), Color.white);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_root);

        [Test, Description("Vérifie qu'un toucher sur un bouton grisé envoie sa raison au relais (toast) — parce qu'un bouton désactivé doit dire pourquoi (spec §6)")]
        public void DisabledTap_RaisesReasonOnRelay()
        {
            _button.Interactable = false;
            _button.DisabledReason = "Il manque 400 CR";
            _button.OnPointerClick(null!);
            Assert.That(_reason, Is.EqualTo("Il manque 400 CR"));
        }

        [Test, Description("Vérifie qu'un bouton grisé sans raison ne lance aucun toast — parce qu'un toast vide n'aide personne")]
        public void DisabledTap_WithoutReason_OnlyShakes()
        {
            _button.Interactable = false;
            _button.OnPointerClick(null!);
            Assert.That(_reason, Is.Null);
        }

        [Test, Description("Vérifie que le survol grossit et éclaircit le bouton, et que la sortie le rétablit — parce que le survol à la souris annonce ce qui est cliquable (spec §6)")]
        public void Hover_ScalesAndExitRestores()
        {
            var bg = _button.Fill.color;
            _button.OnPointerEnter(null!);
            Assert.That(_button.transform.localScale.x, Is.EqualTo(1.04f).Within(1e-4));
            Assert.That(_button.Fill.color.r, Is.GreaterThan(bg.r));
            _button.OnPointerExit(null!);
            Assert.That(_button.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(_button.Fill.color, Is.EqualTo(bg));
        }

        [Test, Description("Vérifie que l'appui ne bloque pas le clic et qu'un bouton actif ne lance aucune raison — parce que les feedbacks ne sont jamais bloquants (spec §6)")]
        public void Press_DoesNotBlockClick()
        {
            int clicks = 0;
            _button.Button.onClick.AddListener(() => clicks++);
            _button.DisabledReason = "raison";
            _button.OnPointerDown(null!);
            _button.Button.onClick.Invoke();
            _button.OnPointerClick(null!);
            _button.OnPointerUp(null!);
            Assert.That(clicks, Is.EqualTo(1));
            Assert.That(_reason, Is.Null);
        }
    }
}
