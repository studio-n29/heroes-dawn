#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>
    /// Les 250 cartes du catalogue chargé en combat : chacune se lance en Auto, chacun de ses paliers change le combat dans au moins un scénario, et aucune ne fait
    /// planter un combat complet. (Le texte généré de chaque carte est vérifié dans la présentation : <c>AllCardsTextTests</c>.)
    /// </summary>
    public sealed class AllCardsTests
    {
        const int MaxTicks = 3000;
        static readonly int[] TierLevels = { 3, 6, 10 };

        static IEnumerable<EffectDef> AllEffects(SkillCardDef card) => card.Skill.Effects.Concat(card.Tiers.SelectMany(t => t.Effects));

        static bool AllySide(TargetRule t) => t == TargetRule.SingleAlly || t == TargetRule.AllAllies || t == TargetRule.Self;

        /// <summary>
        /// Cartes jouées avec une seconde alliée (« givre ») : cible alliée ou « Soi », ou carte qui lit ses alliés (réaction à un allié, réaction sur un allié,
        /// condition sur un allié, échelle par allié vivant). Une seule alliée ne leur donnerait personne à viser ou à lire.
        /// </summary>
        internal static bool NeedsAlly(SkillCardDef card)
        {
            if (AllySide(card.Skill.Target)) return true;
            foreach (var e in AllEffects(card))
            {
                if (e.Reaction != null && (e.Reaction.Trigger == ReactionTrigger.AllyDefeated || e.Reaction.Trigger == ReactionTrigger.AllyReceivesStatus
                                           || (!e.Reaction.OnTriggerUnit && AllySide(e.Reaction.Target) && e.Reaction.Target != TargetRule.Self))) return true;
                if (e.When != null && e.When.Kind == ConditionKind.AnyAllyHasStatus) return true;
                if (e.Mods.Any(m => m.Per == ScaleKind.AlliesAlive || m.When?.Kind == ConditionKind.AnyAllyHasStatus)) return true;
            }
            return false;
        }

        /// <summary>
        /// Combat de référence d'une carte, comme les garde-fous (<see cref="CombatGoldenTests.Card"/>, <see cref="WaveGoldenTests.TwoAllyCard"/>) : « verre » équipée de
        /// la carte au niveau donné (énergie pleine), « givre » en plus si <see cref="NeedsAlly"/>, contre deux brutes, Auto. <paramref name="situation"/> fixe les brutes et ajuste le départ
        /// (scénario dédié). Le catalogue est passé pour pouvoir y remplacer la carte par une variante.
        /// </summary>
        static CombatSimulation Run(ContentCatalog catalog, SkillCardDef card, int level, ulong seed, Situation? situation = null, int maxTicks = MaxTicks)
        {
            var at = situation ?? Situations[0];
            var allies = new List<AllyEntry> { new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { card.Id }, new[] { level }) };
            if (NeedsAlly(card)) allies.Add(new AllyEntry(new MemberId("givre"), 5, 0));
            var setup = new CombatSetup(seed, CombatRank.Normal, allies.ToArray(),
                                        Enumerable.Range(0, at.Enemies).Select(_ => new EnemyEntry(new EnemyId("brute"), at.EnemyLevel)).ToArray(), true);
            var sim = new CombatSimulation(catalog, setup);
            sim.Unit(0).Energy = sim.Unit(0).MaxEnergy;
            at.Prepare?.Invoke(sim);
            for (int i = 0; i < maxTicks && !sim.IsOver; i++) sim.Step(1);
            return sim;
        }

        /// <summary>Le catalogue où la carte de même identifiant est remplacée (à sa place) par <paramref name="variant"/>.</summary>
        static ContentCatalog With(ContentCatalog catalog, SkillCardDef variant)
        {
            var col = catalog.Collection;
            var cards = col.Cards.Select(c => c.Id.Value == variant.Id.Value ? variant : c).ToList();
            return catalog.WithCollection(new CollectionDef(col.Items, cards, col.Boosters));
        }

        /// <summary>La carte privée de son palier <paramref name="tier"/> : un palier qui n'est pas inerte change l'empreinte d'au moins un combat par rapport à elle.</summary>
        static SkillCardDef Without(SkillCardDef card, CardTier tier)
            => new SkillCardDef(card.Id, card.NameKey, card.Family, card.Stars, card.Skill, card.Nature, card.Tiers.Where(t => t != tier).ToList(), card.Archetypes, card.Role);

        static void Give(CombatUnit u, StatusType status, int turns = 3) => u.Statuses.Add(new StatusInstance(status, turns, 0));

        /// <summary>Départ d'un scénario : nombre et niveau des brutes, ajustement de l'état initial.</summary>
        sealed class Situation
        {
            public readonly string Name; public readonly int Enemies; public readonly int EnemyLevel; public readonly Action<CombatSimulation>? Prepare;
            public Situation(string name, int enemies = 2, int enemyLevel = 5, Action<CombatSimulation>? prepare = null) { Name = name; Enemies = enemies; EnemyLevel = enemyLevel; Prepare = prepare; }
        }

        static IEnumerable<CombatUnit> OnSide(CombatSimulation s, Side side) => s.Units.Where(u => u.Side == side);

        /// <summary>
        /// Le scénario générique (celui des garde-fous), puis des scénarios dédiés communs à toutes les cartes : chacun crée une situation que le générique ne crée
        /// jamais ou trop tard — alliés sous 40 % de PV, alliés affligés (pour Purge, avec Regen pour les cartes qui la lisent), ennemis affligés (Brûlure, Entrave,
        /// Marque, Provocation), ennemis renforcés (Provocation puis Haste : une purge d'un seul bonus retire Haste et laisse la Provocation lisible), trois ennemis, brutes robustes (niveau 20) pour que l'exécution ait une cible sous le seuil qui survive au coup.
        /// </summary>
        static readonly Situation[] Situations =
        {
            new Situation("générique"),
            new Situation("alliés blessés", prepare: s => { foreach (var u in OnSide(s, Side.Ally)) u.Hp = u.MaxHp * 30 / 100; }),
            new Situation("alliés affligés", prepare: s => { foreach (var u in OnSide(s, Side.Ally)) { Give(u, StatusType.Regen); Give(u, StatusType.Burn); Give(u, StatusType.Corrosion); Give(u, StatusType.Vulnerable); } }),
            new Situation("ennemis affligés", prepare: s => { foreach (var u in OnSide(s, Side.Enemy)) { Give(u, StatusType.Burn); Give(u, StatusType.Bind); Give(u, StatusType.Mark); Give(u, StatusType.Taunt); } }),
            new Situation("ennemis renforcés", prepare: s => { foreach (var u in OnSide(s, Side.Enemy)) { Give(u, StatusType.Taunt); Give(u, StatusType.Haste); } }),
            new Situation("trois ennemis", enemies: 3),
            new Situation("brutes robustes", enemyLevel: 20),
            new Situation("brutes robustes blessées", enemyLevel: 20, prepare: s => { foreach (var u in OnSide(s, Side.Enemy)) u.Hp = u.MaxHp * 28 / 100; }),
        };

        static CombatSimulation[] Scenarios(ContentCatalog catalog, SkillCardDef card, int level)
            => Situations.Select(s => Run(catalog, card, level, 7, s)).ToArray();

        /// <summary>
        /// Paliers inertes dans tous les scénarios ci-dessus, faute de pouvoir exprimer la situation qu'ils demandent, avec la raison (clé « id:niveau »).
        /// Toute autre carte dont un palier ne change rien est une faute de fiche, corrigée dans la fiche (journal corrections-fiches.md).
        /// </summary>
        internal static readonly Dictionary<string, string> InertTierExceptions = new Dictionary<string, string>
        {
        };

        // ------------------------------------------------------------------ Tests

        [Test, Description("Vérifie que chacune des 250 cartes, équipée au niveau 1 (graine 7, énergie pleine, seconde alliée pour les cartes alliées), est lancée au moins une fois en combat Auto — parce qu'une carte que l'IA ne lance jamais ne se jouerait pas")]
        public void EveryCard_IsCastInAuto()
        {
            var catalog = TestContent.Build();
            Assert.That(catalog.Collection.Cards.Count, Is.EqualTo(250));
            var never = new List<string>();
            foreach (var card in catalog.Collection.Cards)
                if (!Run(catalog, card, 1, 7).Log.OfType<SkillUsedEvent>().Any(e => e.Skill == card.Skill.Id)) never.Add(card.Id.Value);
            Assert.That(never, Is.Empty, "cartes jamais lancées : " + string.Join(", ", never));
        }

        [Test, Description("Vérifie que chaque palier (3, 6, 10) de chacune des 250 cartes change l'empreinte d'au moins un combat aux niveaux 3, 6 ou 10 (scénario générique ou dédié) par rapport à la même carte privée de ce palier, hors exceptions listées avec leur raison — parce qu'un palier qui ne change jamais rien est une faute de fiche")]
        public void EveryCardTier_ChangesBehaviour_ForAtLeastOneScenario()
        {
            var catalog = TestContent.Build();
            var inert = new List<string>();
            var stale = new List<string>();
            foreach (var card in catalog.Collection.Cards)
            {
                var full = TierLevels.ToDictionary(l => l, l => Scenarios(catalog, card, l).Select(CombatDigest.Of).ToArray());
                foreach (var tier in card.Tiers)
                {
                    var variant = With(catalog, Without(card, tier));
                    bool changes = TierLevels.Where(l => l >= tier.Level)
                                             .Any(l => !Scenarios(variant, card, l).Select(CombatDigest.Of).SequenceEqual(full[l]));
                    string key = card.Id.Value + ":" + tier.Level;
                    bool listed = InertTierExceptions.ContainsKey(key);
                    if (!changes && !listed) inert.Add(key);
                    if (changes && listed) stale.Add(key);
                }
            }
            Assert.That(inert, Is.Empty, "paliers inertes : " + string.Join(", ", inert));
            Assert.That(stale, Is.Empty, "exceptions devenues inutiles (le palier agit) : " + string.Join(", ", stale));
        }

        [Test, Description("Vérifie que chacune des 250 cartes au niveau 10 (tous paliers), dans chaque scénario, mène un combat Auto complet jusqu'à sa fin sans exception — parce qu'aucune carte ne doit bloquer ni faire planter un combat")]
        public void NoCard_CrashesACombat()
        {
            var catalog = TestContent.Build();
            var failures = new List<string>();
            foreach (var card in catalog.Collection.Cards)
                foreach (var s in Situations)
                {
                    try
                    {
                        var sim = Run(catalog, card, 10, 7, s, 40000);
                        if (!sim.IsOver) failures.Add($"{card.Id.Value} ({s.Name}) : combat non terminé");
                    }
                    catch (Exception e) { failures.Add($"{card.Id.Value} ({s.Name}) : {e.GetType().Name} {e.Message}"); }
                }
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }
    }
}
