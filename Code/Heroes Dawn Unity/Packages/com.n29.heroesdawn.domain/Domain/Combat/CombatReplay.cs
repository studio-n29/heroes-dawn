using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>Rejoue un combat à partir de la graine et du journal (préfiguration de la vérification serveur).</summary>
    public static class CombatReplay
    {
        public static CombatResult Run(ContentCatalog c, CombatSetup setup, IReadOnlyList<CombatInput> inputs, int maxTicks = 20000)
        {
            var sim = new CombatSimulation(c, setup);
            int next = 0;
            while (!sim.IsOver && sim.Tick < maxTicks)
            {
                // Une commande journalisée au tick T a été appliquée pendant le tick T : on la soumet juste avant.
                while (next < inputs.Count && inputs[next].Tick == sim.Tick + 1) sim.Submit(inputs[next++].Command);
                sim.Step(1);
            }
            return sim.Result();
        }
    }
}
