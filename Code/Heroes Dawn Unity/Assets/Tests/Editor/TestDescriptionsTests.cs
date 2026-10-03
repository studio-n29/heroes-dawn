using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class TestDescriptionsTests
    {
        [Test, Description("Vérifie que chaque méthode de test du projet porte une description non vide — parce que le rapport HTML doit dire ce que chaque test prouve et pourquoi")]
        public void EveryTestHasADescription()
        {
            var missing = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith("N29.HeroesDawn.Tests"))
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(m => m.GetCustomAttributes().Any(x => x is TestAttribute || x is UnityTestAttribute || x is TestCaseAttribute))
                .Where(m => string.IsNullOrWhiteSpace(m.GetCustomAttribute<DescriptionAttribute>()?.Properties.Get("Description") as string))
                .Select(m => m.DeclaringType!.FullName + "." + m.Name)
                .OrderBy(n => n)
                .ToList();
            Assert.That(missing, Is.Empty, "Tests sans description :\n" + string.Join("\n", missing));
        }
    }
}
