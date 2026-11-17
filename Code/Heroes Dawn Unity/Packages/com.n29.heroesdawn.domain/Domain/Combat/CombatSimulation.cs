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
        readonly List<CombatEvent> _log = new List<CombatEvent>();

        /// <summary>Tous les événements produits depuis le début du combat, dans l'ordre (statistiques du profil : ennemis battus, favoris).</summary>
        public IReadOnlyList<CombatEvent> Log => _log;
        public CombatSetup Setup { get; }
        public int Tick { get; private set; }
        /// <summary>Ticks de temps actif : n'avance pas pendant l'attente d'une action (pause globale).</summary>
        public int ActiveTick { get; private set; }
        public IReadOnlyList<CombatUnit> Units => _units;
        public bool IsOver => Victory.HasValue;
        public bool? Victory { get; private set; }
        public bool Auto { get; private set; }

        public CombatSimulation(ContentCatalog c, CombatSetup setup) : this(c, setup, null) { }

        /// <summary>Constructeur de la prévisualisation : <paramref name="wrapRng"/> enveloppe le générateur du combat (même flux par défaut) pour changer de source d'aléa après le rejeu.</summary>
        internal CombatSimulation(ContentCatalog c, CombatSetup setup, System.Func<IRandom, IRandom>? wrapRng)
        {
            _c = c;
            _b = c.Balance;
            Setup = setup;
            Auto = setup.Auto;
            IRandom stream = new Pcg32(setup.Seed).Fork(3);
            _rng = wrapRng == null ? stream : wrapRng(stream);
            BuildTeam(setup.Allies, setup.Links, Side.Ally);
            if (setup.OpponentTeam.Count > 0) BuildTeam(setup.OpponentTeam, setup.OpponentLinks, Side.Enemy); // duel de tournoi : le camp adverse est une équipe de joueur
            else SpawnWave(0);
            foreach (var u in _units)
            {
                u.Point = _rng.Range(0, _b.TimelineStartPointMax + 1); // depart entre le point 0 et le point 2
                if (u.StartPointBonus > 0) u.Point = System.Math.Min(u.ActionPoint - 1, u.Point + u.StartPointBonus);
            }
        }

        public CombatUnit Unit(int id) => _units[id];

        /// <summary>Fait entrer une équipe d'agents dans un camp : synergies, bonus de paire et effets d'équipe de cette équipe seule.</summary>
        void BuildTeam(IReadOnlyList<AllyEntry> team, IReadOnlyList<LinkEntry> links, Side side)
        {
            var families = new List<Family>();
            foreach (var a in team) families.Add(_c.Member(a.Member).Family);
            var activeSynergies = SynergyRules.Active(families, _c.Synergies);
            var synergy = SynergyRules.Total(System.Linq.Enumerable.Select(activeSynergies,
                s => (s.Synergy, CombatSetup.LinkLevelIn(links, team[s.A].Member, team[s.B].Member))));
            for (int i = 0; i < team.Count; i++)
            {
                var def = _c.Member(team[i].Member);
                var pair = PairBonusOf(_c, team, links, i);
                _units.Add(side == Side.Ally
                    ? UnitFactory.Ally(_units.Count, def, team[i], _b, _c.Collection, synergy, pair)
                    : UnitFactory.Opponent(_units.Count, team[i], def, _c.Collection, _b, synergy, pair));
            }
            var roles = new List<Position>();
            foreach (var a in team) roles.Add(_c.Member(a.Member).Position);
            ApplyTeamEffects(TeamEffects.Active(families, roles), side);
        }

        /// <summary>Bonus de paire d'un allié : ceux de chacune de ses paires dans l'escouade (niveau de lien 2 et plus), plafonnés.</summary>
        static PairBonusTotal PairBonusOf(ContentCatalog c, IReadOnlyList<AllyEntry> team, IReadOnlyList<LinkEntry> links, int index)
        {
            var me = c.Member(team[index].Member);
            var bonuses = new List<PairBonus>();
            for (int j = 0; j < team.Count; j++)
            {
                if (j == index) continue;
                var other = c.Member(team[j].Member);
                bonuses.AddRange(PairBonusRules.Of(me.Position, other.Position, CombatSetup.LinkLevelIn(links, me.Id, other.Id)));
            }
            return PairBonusRules.Total(bonuses);
        }

        /// <summary>Effets uniques de la composition d équipe, appliqués à chaque allié au début du combat.</summary>
        void ApplyTeamEffects(IReadOnlyList<TeamEffectKind> effects, Side side)
        {
            foreach (var u in _units)
            {
                if (u.Side != side) continue;
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
            for (int i = 0; i < ticks && !IsOver; i++)
            {
                int from = output.Count;
                StepOne(output);
                ProcessReactions(output, from); // réactions sur les seuls événements de ce pas
            }
            _log.AddRange(output);
            return output;
        }

        /// <summary>Vrai tant qu'un allié a atteint 0 et attend l'ordre du joueur : toute la frise est figée.</summary>
        public bool IsAwaitingAction => AwaitingSide.HasValue;

        /// <summary>Le camp dont une unité est prête et attend une commande (l'allié d'abord ; le camp adverse seulement s'il est mené de l'extérieur) ; nul sinon.</summary>
        public Side? AwaitingSide
        {
            get
            {
                if (Auto) return null;
                foreach (var u in _units) if (u.Side == Side.Ally && u.IsReady) return Side.Ally;
                if (Setup.EnemyControl == SideControl.External)
                    foreach (var u in _units) if (u.Side == Side.Enemy && u.IsReady) return Side.Enemy;
                return null;
            }
        }

        /// <summary>Un joueur (et non l'IA) mène ce camp.</summary>
        bool IsHumanSide(Side s) => s == Side.Ally || Setup.EnemyControl == SideControl.External;

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
            ProcessReactions(output, 0); // commandes appliquées hors frise (Submit(cmd, from), fenêtre d'interruption)
            _log.AddRange(output);
            return output;
        }

        /// <summary>Aide de test : fait avancer une seule unite de <paramref name="points"/> points sans declencher de tour.</summary>
        public void StepSingleUnitForTests(CombatUnit u, int points)
        {
            for (int i = 0; i < points; i++) if (u.Point < u.ActionPoint) u.Point++;
        }
    }
}
