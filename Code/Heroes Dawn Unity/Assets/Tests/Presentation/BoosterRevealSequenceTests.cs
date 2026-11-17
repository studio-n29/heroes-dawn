#nullable enable
using NUnit.Framework;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class BoosterRevealSequenceTests
    {
        static BoosterRevealSequence Seq(params (int rank, bool isNew)[] cards)
        {
            var list = new RevealCard[cards.Length];
            for (int i = 0; i < cards.Length; i++) list[i] = new RevealCard(cards[i].rank, cards[i].isNew);
            return new BoosterRevealSequence(list);
        }

        static BoosterRevealSequence Three() => Seq((0, true), (1, false), (3, true));

        [Test, Description("Vérifie que le paquet attend un toucher avant de s'ouvrir — parce que le joueur doit déclencher l'ouverture lui-même")]
        public void Starts_Sealed_AndFirstTapStartsOpening()
        {
            var s = Three();
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Sealed));
            Assert.That(s.Tap().Kind, Is.EqualTo(SeqActionKind.StartOpening));
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Opening));
        }

        [Test, Description("Vérifie qu'un toucher pendant l'animation d'ouverture est ignoré — parce que les cartes n'existent pas encore")]
        public void Tap_WhileOpening_DoesNothing()
        {
            var s = Three(); s.Tap();
            Assert.That(s.Tap().Kind, Is.EqualTo(SeqActionKind.None));
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Opening));
        }

        [Test, Description("Vérifie que la fin de l'animation d'ouverture lance la révélation, et qu'elle n'a d'effet qu'en phase d'ouverture")]
        public void OpeningFinished_StartsRevealing_OnlyFromOpening()
        {
            var s = Three();
            s.OpeningFinished();
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Sealed), "sans toucher préalable, rien ne bouge");
            s.Tap(); s.OpeningFinished();
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Revealing));
            Assert.That(s.NextIndex, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'un toucher retourne une carte, dans l'ordre — parce que c'est le principe d'une révélation carte par carte")]
        public void Tap_RevealsCardsOneByOneInOrder()
        {
            var s = Three(); s.Tap(); s.OpeningFinished();
            var a = s.Tap();
            Assert.That(a.Kind, Is.EqualTo(SeqActionKind.Reveal));
            Assert.That(a.Index, Is.EqualTo(0));
            Assert.That(s.IsRevealed(0), Is.True);
            Assert.That(s.IsRevealed(1), Is.False);
            Assert.That(s.NextIndex, Is.EqualTo(1));
            Assert.That(s.Tap().Index, Is.EqualTo(1));
            Assert.That(s.RevealedCount, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que la dernière carte retournée passe en phase « tout révélé » — parce que le bouton Continuer doit alors apparaître")]
        public void LastReveal_MovesToAllRevealed()
        {
            var s = Three(); s.Tap(); s.OpeningFinished(); s.Tap(); s.Tap();
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Revealing));
            var last = s.Tap();
            Assert.That(last.Index, Is.EqualTo(2));
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.AllRevealed));
            Assert.That(s.NextIndex, Is.EqualTo(-1));
        }

        [Test, Description("Vérifie qu'un toucher une fois tout révélé ouvre le récapitulatif, puis qu'on y reste")]
        public void Tap_AfterAllRevealed_GoesToRecap_AndStaysThere()
        {
            var s = Three(); s.Tap(); s.OpeningFinished(); s.Tap(); s.Tap(); s.Tap();
            Assert.That(s.Tap().Kind, Is.EqualTo(SeqActionKind.ToRecap));
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Recap));
            Assert.That(s.Tap().Kind, Is.EqualTo(SeqActionKind.None));
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Recap));
        }

        [Test, Description("Vérifie que Passer retourne tout et va au récapitulatif depuis n'importe quelle phase — parce que le joueur doit pouvoir sauter l'animation")]
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void Skip_RevealsAll_AndGoesToRecap(int tapsBefore)
        {
            var s = Three();
            if (tapsBefore >= 1) s.Tap();
            if (tapsBefore >= 2) { s.OpeningFinished(); s.Tap(); }
            s.Skip();
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Recap));
            Assert.That(s.RevealedCount, Is.EqualTo(3));
            Assert.That(s.NextIndex, Is.EqualTo(-1));
        }

        [Test, Description("Vérifie que Passer est sans effet une fois le récapitulatif affiché ou la séquence fermée")]
        public void Skip_IsIdempotent()
        {
            var s = Three(); s.Skip();
            s.Skip();
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Recap));
            s.Close(false);
            s.Skip();
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Closed));
        }

        [Test, Description("Vérifie que la fermeture mémorise si le joueur demande un autre booster — parce que la fenêtre doit le renvoyer à l'appelant")]
        public void Close_RemembersOpenAnother()
        {
            var a = Three(); a.Skip(); a.Close(true);
            Assert.That(a.Phase, Is.EqualTo(RevealPhase.Closed));
            Assert.That(a.OpenAnother, Is.True);
            var b = Three(); b.Skip(); b.Close(false);
            Assert.That(b.OpenAnother, Is.False);
        }

        [Test, Description("Vérifie que Retour (fermeture) est possible à tout moment et que toutes les cartes comptent comme reçues — parce que le tirage est déjà enregistré")]
        public void Close_FromAnyPhase_IsAllowed()
        {
            var s = Three(); s.Tap();
            s.Close(false);
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.Closed));
            Assert.That(s.RevealedCount, Is.EqualTo(3));
        }

        [Test, Description("Vérifie le bilan : nouvelles, doublons — parce que le récapitulatif annonce « X nouvelles · Y doublons »")]
        public void Summary_CountsNewAndDuplicates()
        {
            var s = Seq((0, true), (0, false), (1, false), (2, true));
            Assert.That(s.NewCount, Is.EqualTo(2));
            Assert.That(s.DuplicateCount, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que la meilleure carte est la plus rare, puis la nouvelle, puis la première — parce que le récapitulatif la met en avant")]
        public void BestIndex_PrefersRarityThenNewThenFirst()
        {
            Assert.That(Seq((0, true), (2, false), (1, true)).BestIndex, Is.EqualTo(1));
            Assert.That(Seq((2, false), (2, true), (1, true)).BestIndex, Is.EqualTo(1), "à rareté égale, la nouvelle");
            Assert.That(Seq((1, true), (1, true)).BestIndex, Is.EqualTo(0), "à égalité, la première");
            Assert.That(Seq().BestIndex, Is.EqualTo(-1));
        }

        [Test, Description("Vérifie qu'un booster vide ne bloque pas : les phases s'enchaînent sans carte")]
        public void EmptyBooster_StillCompletes()
        {
            var s = Seq(); s.Tap(); s.OpeningFinished();
            Assert.That(s.Phase, Is.EqualTo(RevealPhase.AllRevealed));
            Assert.That(s.Tap().Kind, Is.EqualTo(SeqActionKind.ToRecap));
        }

        [Test, Description("Vérifie que la mise en scène croît avec la rareté, avec une pause dramatique seulement pour la légendaire — parce que les rares doivent se sentir plus marquants")]
        public void Styles_GrowWithRarity()
        {
            var c = RevealStyles.For(0); var r = RevealStyles.For(1); var e = RevealStyles.For(2); var l = RevealStyles.For(3);
            Assert.That(r.Sparks, Is.GreaterThan(c.Sparks));
            Assert.That(e.Sparks, Is.GreaterThan(r.Sparks));
            Assert.That(l.Sparks, Is.GreaterThan(e.Sparks));
            Assert.That(c.PauseSeconds, Is.EqualTo(0f));
            Assert.That(r.PauseSeconds, Is.EqualTo(0f));
            Assert.That(l.PauseSeconds, Is.GreaterThan(0f));
            Assert.That(c.ScreenFlash, Is.False);
            Assert.That(l.ScreenFlash, Is.True);
            Assert.That(c.Rays, Is.False);
            Assert.That(r.Rays, Is.True);
        }

        [Test, Description("Vérifie que les rangs hors bornes sont ramenés dans 0 à 3 — parce qu'une donnée invalide ne doit pas planter la vue")]
        public void Styles_ClampRank()
        {
            Assert.That(RevealStyles.For(-4).Sparks, Is.EqualTo(RevealStyles.For(0).Sparks));
            Assert.That(RevealStyles.For(9).Sparks, Is.EqualTo(RevealStyles.For(3).Sparks));
        }
    }
}
