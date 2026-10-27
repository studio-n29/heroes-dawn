#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>
    /// Habillage de <see cref="SnowSurvivalGame"/> aux conventions des mini-jeux 2D (<see cref="IMiniGame"/>) : la direction du joueur est fixée par
    /// <see cref="SetMove"/> puis appliquée à chaque <see cref="Step"/>, et l'abandon est géré ici. Les règles restent celles de la survie en neige.
    /// </summary>
    public sealed class SnowSurvivalMiniGame : IMiniGame
    {
        Vec2f _move = Vec2f.Zero;
        bool _abandoned;

        public SnowSurvivalGame Inner { get; }

        public SnowSurvivalMiniGame(ActivityLevel level, ActivityParameters p) => Inner = new SnowSurvivalGame(level, p);

        public GameStatus Status => _abandoned ? GameStatus.Lost : Inner.Status;
        public int Score => Inner.Status == GameStatus.Won && !_abandoned ? 1 : 0;
        public int Target => 1;
        public float TimeLeft => Inner.TimeLeft;
        public int Lives => -1;

        public void SetMove(Vec2f move) => _move = move.Clamped(1f);

        public void Step(float dt)
        {
            if (_abandoned) return;
            Inner.Step(dt, _move);
        }

        public void Abandon() { if (Status == GameStatus.Running) _abandoned = true; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
    }
}
