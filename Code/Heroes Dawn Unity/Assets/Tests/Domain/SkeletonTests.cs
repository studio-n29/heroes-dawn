using N29.HeroesDawn.Core;
using NUnit.Framework;

public class SkeletonTests
{
    [Test, Description("Vérifie que le code partagé compile et expose une version des règles — parce que c'est le test de fumée du projet")]
    public void Skeleton_Compiles() => Assert.That(RulesVersion.Value, Is.Not.Empty);
}
