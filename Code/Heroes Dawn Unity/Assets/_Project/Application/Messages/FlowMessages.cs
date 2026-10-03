#nullable enable
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Flow;

namespace N29.HeroesDawn.Application.Messages
{
    /// <summary>Un écran demande à aller vers un état (seule façon de naviguer).</summary>
    public readonly struct NavigateRequested
    {
        public readonly GameState Target;
        public NavigateRequested(GameState target) { Target = target; }
    }

    /// <summary>Le joueur a fermé les résultats du combat (« Continuer »).</summary>
    public readonly struct CombatResultsClosed
    {
        public readonly CombatReport Report;
        public CombatResultsClosed(CombatReport report) { Report = report; }
    }

    /// <summary>Le plateau est de nouveau affiché après un combat ; il applique le rapport.</summary>
    public readonly struct ReturnedToBoard
    {
        public readonly CombatReport Report;
        public ReturnedToBoard(CombatReport report) { Report = report; }
    }

    /// <summary>Une transition vient de réussir.</summary>
    public readonly struct GameStateChanged
    {
        public readonly GameState From;
        public readonly GameState To;
        public GameStateChanged(GameState from, GameState to) { From = from; To = to; }
    }
}
