using N29.HeroesDawn.Application.Features.Tutorial;
using N29.HeroesDawn.Presentation.Tutorial;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class TutorialStepInfoTests
    {
        [Test, Description("Vérifie que l'étape « Avancer » ne pose pas de voile bloquant — parce que la fiche de la case (« S'y rendre ») doit rester utilisable sinon le tutoriel se bloque")]
        public void MoveStep_DoesNotBlockTheScreen()
        {
            Assert.That(TutorialStepInfo.Of(TutorialStep.Move).Dim, Is.False);
        }

        [Test, Description("Vérifie que l'étape de compétence en combat ne bloque pas l'écran — parce que le joueur doit pouvoir glisser une carte vers un ennemi")]
        public void CombatSkillStep_DoesNotBlockTheScreen()
        {
            Assert.That(TutorialStepInfo.Of(TutorialStep.CombatSkill).Dim, Is.False);
        }
    }
}
