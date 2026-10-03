#nullable enable
using N29.HeroesDawn.Application.Flow;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Lance la machine d'états (Boot → Login). Enregistré après les présentateurs de la racine : VContainer démarre
    /// les points d'entrée dans l'ordre d'enregistrement, donc la connexion n'est affichée qu'une fois son présentateur abonné.
    /// </summary>
    public sealed class FlowStarter : IStartable
    {
        readonly GameFlowMachine _flow;
        public FlowStarter(GameFlowMachine flow) => _flow = flow;
        public void Start() => _flow.Start();
    }
}
