using System;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Référence sérialisée qui peut légitimement rester vide (ignorée par le garde-fou des prefabs).</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OptionalRefAttribute : Attribute { }
}
