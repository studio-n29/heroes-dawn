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
        /// <summary>Ticks de temps actif : n'avance pas pendant l'attente d'une action (pause globale).</summary>
        public int ActiveTick { get; private set; }
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
            var families = new List<Family>();
            foreach (var a in setup.Allies) families.Add(c.Member(a.Member).Family);
            var activeSynergies = SynergyRules.Active(families, c.Synergies);
            var synergy = SynergyRules.Total(System.Linq.Enumerable.Select(activeSynergies,
                s => (s.Synergy, setup.LinkLevel(setup.Allies[s.A].Member, setup.Allies[s.B].Member))));
            foreach (var a in setup.Allies) _units.Add(UnitFactory.Ally(id++, c.Member(a.Member), a, _b, c.Collection, synergy));
            var roles = new List<Role>();
            foreach (var a in setup.Allies) roles.Add(c.Member(a.Member).Role);
            ApplyTeamEffects(TeamEffects.Active(families, roles));
            SpawnWave(0);
            foreach (var u in _units)
            {
                u.Point = _rng.Range(0, _b.TimelineStartPointMax + 1); // depart entre le point 0 et le point 2
                if (u.StartPointBonus > 0) u.Point = System.Math.Min(u.ActionPoint - 1, u.Point + u.StartPointBonus);
            }
        }

        public CombatUnit Unit(int id) => _units[id];

        /// <summary>Effets uniques de la composition d équipe, appliqués à chaque allié au début du combat.</summary>
        void ApplyTeamEffects(IReadOnlyList<TeamEffectKind> effects)
        {
            foreach (var u in _units)
            {
                if (u.Side != Side.Ally) continue;
                foreach (var e in effects)
                    switch (e)
                    {
                        case TeamEffectKind.Quartet: u.CritBonusPercent += TeamEffects.QuartetCritPercent; break;
                        case TeamEffectKind.Trinity: u.EnergyRegenBonus += TeamEffects.TrinityEnergyRegen; break;
                        case TeamEffectKind.DoubleFront: u.Statuses.Add(new StatusInstance(StatusType.Shield, 2, u.MaxHp * TeamEffects.DoubleFrontShieldPercent / 100)); break;
                        case TeamEffectKind.Spark: u.Energy = System.Math.Min(u.MaxEnergy, u.Energy + TeamEffects.SparkStartEnergy); break;
                    }
            }
        }


        /// <summary>Points franchis par une unite a ce pas : 1 pour tous ; Hate : 2 un pas sur trois ; Gel : un pas sur deux seulement.</summary>
        int PointsThisStep(CombatUnit u)
        {
            if (u.Has(StatusType.Freeze) && Tick % 2 == 1) return 0;
            if (u.Has(StatusType.Haste) && Tick % 3 == 0) return 2;
            return 1;
        }

        public IReadOnlyList<CombatEvent> Step(int ticks)
        {
            var output = new List<CombatEvent>();
            for (int i = 0; i < ticks && !IsOver; i++) StepOne(output);
            return output;
        }

        /// <summary>Vrai tant qu'un allié a atteint 0 et attend l'ordre du joueur : toute la frise est figée.</summary>
        public bool IsAwaitingAction
        {
            get
            {
                if (Auto) return false;
                foreach (var u in _units) if (u.Side == Side.Ally && u.IsReady) return true;
                return false;
            }
        }

        void StepOne(List<CombatEvent> output)
        {
            Tick++;
            if (IsAwaitingAction)
            {
                // Pause d'action : ni frise, ni recharges ; seules les commandes sont traitees.
                ProcessCommands(output);
                ActReadyUnits(output);
                CheckEnd(output);
                return;
            }
            ActiveTick++;
            for (int i = 0; i < _units.Count; i++)
            {
                var u = _units[i];
                if (!u.IsAlive || u.IsReady) continue;
                u.Energy = System.Math.Min(u.MaxEnergy, u.Energy + _b.EnergyRegenPerTick + u.EnergyRegenBonus); // l énergie se régénère à chaque pas
                int moves = PointsThisStep(u);
                for (int m = 0; m < moves && u.IsAlive && !u.IsReady; m++)
                {
                    u.Point++;
                    BurnStep(u, output); // la brulure mord a chaque point franchi
                }
                if (!u.IsAlive) continue;
                if (!u.IsReady) PlanIfNeeded(u, output);
                else
                {
                    output.Add(new GaugeFullEvent(Tick, u.Id));
                    BeginTurn(u, output);
                }
            }
            output.Add(new TimelineAdvancedEvent(Tick));
            ResolveEncounters(output);
            ProcessCommands(output);
            ActReadyUnits(output);
            CheckEnd(output);
        }

        /// <summary>Traite les ordres en attente sans avancer la frise (fenetre d interruption, pause d action).</summary>
        public IReadOnlyList<CombatEvent> ApplyCommands()
        {
            var output = new List<CombatEvent>();
            if (IsOver) return output;
            ProcessCommands(output);
            ActReadyUnits(output);
            CheckEnd(output);
            return output;
        }

        /// <summary>Aide de test : fait avancer une seule unite de <paramref name="points"/> points sans declencher de tour.</summary>
        public void StepSingleUnitForTests(CombatUnit u, int points)
        {
            for (int i = 0; i < points; i++) if (u.Point < u.ActionPoint) u.Point++;
        }
    }
}
