using System;
using System.Linq;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Garde-fous de l'architecture (règles 1-2 de 08_Regles_Conventions, spec du sous-projet 1).</summary>
    public sealed class DependencyGuardTests
    {
        [TestCase(typeof(N29.HeroesDawn.Core.Result))]
        [TestCase(typeof(N29.HeroesDawn.Definitions.BalanceConfig))]
        [TestCase(typeof(N29.HeroesDawn.Domain.Board.ZoneRun))]
        [TestCase(typeof(N29.HeroesDawn.Domain.Combat.CombatSimulation))]
        [Description("Vérifie que le code du domaine ne référence ni VContainer, ni MessagePipe, ni UniTask — parce que le domaine reste du C# pur, testable et indépendant des frameworks (règle 2)")]
        public void SharedAssemblies_DoNotReferenceFrameworkPackages(Type marker)
        {
            var refs = marker.Assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
            Assert.That(refs.Where(r => r.StartsWith("VContainer") || r.StartsWith("MessagePipe") || r.StartsWith("UniTask")), Is.Empty);
        }

        [Test, Description("Vérifie qu'il ne reste aucune classe « *Service » dans la couche Application — parce que le sous-projet 1 a remplacé les services par des use cases et ils ne doivent pas revenir")]
        public void NoServiceClassesRemain()
        {
            var app = typeof(N29.HeroesDawn.Application.Session.GameSession).Assembly;
            Assert.That(app.GetTypes().Where(t => t.Name.EndsWith("Service")).Select(t => t.FullName), Is.Empty);
        }
    }
}
