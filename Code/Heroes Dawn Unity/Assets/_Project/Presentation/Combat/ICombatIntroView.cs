#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Un combattant de l'entrée en combat : nom, ligne de détail (rôle, niveau) et portrait.</summary>
    public readonly struct CombatIntroFighter
    {
        public string Name { get; }
        public string Sub { get; }
        public PortraitData Portrait { get; }

        public CombatIntroFighter(string name, string sub, PortraitData portrait)
        {
            Name = name; Sub = sub; Portrait = portrait;
        }
    }

    /// <summary>Ce que montre l'entrée en combat : les agents du joueur à gauche, les ennemis à droite ; <see cref="Repeat"/> abrège une introduction déjà vue.</summary>
    public sealed class CombatIntroData
    {
        public string AllyTitle { get; set; } = "";
        public string EnemyTitle { get; set; } = "";
        public IReadOnlyList<CombatIntroFighter> Allies { get; set; } = System.Array.Empty<CombatIntroFighter>();
        public IReadOnlyList<CombatIntroFighter> Enemies { get; set; } = System.Array.Empty<CombatIntroFighter>();
        public bool Repeat { get; set; }
    }

    /// <summary>Entrée en combat : un bandeau par combattant, puis le combat. Un toucher ou « Passer » abrège l'introduction.</summary>
    public interface ICombatIntroView : IPopupView
    {
        /// <summary>Joue l'introduction ; rend la main à sa fin ou dès que le joueur la passe.</summary>
        UniTask PlayAsync(CombatIntroData data, CancellationToken ct);
    }
}
