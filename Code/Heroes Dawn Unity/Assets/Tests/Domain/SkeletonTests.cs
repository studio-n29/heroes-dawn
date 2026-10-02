using N29.HeroesDawn.Core;
using NUnit.Framework;

public class SkeletonTests
{
    [Test]
    public void Skeleton_Compiles() => Assert.That(RulesVersion.Value, Is.Not.Empty);
}
