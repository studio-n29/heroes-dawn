#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>
    /// Simulation ATB déterministe (02_Combat). Avance par ticks fixes ; produit des CombatEvent que la présentation joue.
    /// Tour d'une unité : sa jauge se remplit → début de tour (brûlure, régénération, durées, recharges) →
    /// action (IA ou commande du joueur) → fin de tour (jauge à 0).
    /// </summary>
    public sealed partial class CombatSimulation
    {
        readonly ContentCatalog _c;
        readonly BalanceConfig _b;
        readonly IRandom _rng;
        readonly List<CombatUnit> _units = new List<CombatUnit>();

        public CombatSetup Setup { get; }
        public int Tick { get; private set; }
        public IReadOnlyList<CombatUnit> Units => _units;
        public bool IsOver => Victory.HasValue;
        public bool? Victory { get; private set; }
        public bool Auto { get; private set; }

        public CombatSimulation(ContentCatalog c, CombatSetup setup)
        {
            _c = c;
            _b = c.Balance;
            Setup = setup;
            Auto = setup.Auto;
            _rng = new Pcg32(setup.Seed).Fork(3);
            int id = 0;
            foreach (var a in setup.Allies) _units.Add(UnitFactory.Ally(id++, c.Member(a.Member), a, _b));
            foreach (var e in setup.Enemies) _units.Add(UnitFactory.Enemy(id++, c.Enemy(e.Enemy), e, setup.Rank, _b));
            foreach (var u in _units) u.Gauge = u.GaugeMax * _rng.Range(0, 21) / 100; // départ entre 0 et 20 %
            InitTimeline();
        }

        public CombatUnit Unit(int id) => _units[id];

        /// <summary>Milli-points ajoutés à la jauge à chaque tick.</summary>
        public long FillPerTick(CombatUnit u)
        {
            long fill = (long)u.Spd * _b.AtbFactorPerSecond * _b.TickMs;
            if (u.Has(StatusType.Haste)) fill = fill * 130 / 100;
            if (u.Has(StatusType.Freeze)) fill = fill * 50 / 100;
            return fill;
        }

        public IReadOnlyList<CombatEvent> Step(int ticks)
        {
            var output = new List<CombatEvent>();
            for (int i = 0; i < ticks && !IsOver; i++) StepOne(output);
            return output;
        }

        void StepOne(List<CombatEvent> output)
        {
            Tick++;
            RegenTimeBank();
            for (int i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                if (!u.IsAlive || u.IsReady) continue;
                long fill = FillPerTick(u);
                u.Gauge += fill;
                BurnStep(u, fill, output); // la brûlure mord à chaque pas sur la frise
                if (!u.IsAlive) continue;
                if (u.Gauge < u.GaugeMax) PlanIfNeeded(u, output);
                if (u.Gauge >= u.GaugeMax)
                {
                    u.Gauge = u.GaugeMax;
                    output.Add(new GaugeFullEvent(Tick, u.Id));
                    BeginTurn(u, output);
                }
            }
            ProcessCommands(output);
            ActReadyUnits(output);
            CheckEnd(output);
        }

        /// <summary>Aide de test : remplit la jauge d'une seule unité sans déclencher de tour.</summary>
        public void StepSingleUnitForTests(CombatUnit u, int ticks)
        {
            for (int i = 0; i < ticks; i++)
            {
                u.Gauge += FillPerTick(u);
                if (u.Gauge > u.GaugeMax) u.Gauge = u.GaugeMax;
            }
        }
    }
}
