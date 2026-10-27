#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities
{
    /// <summary>Ce dont un type d'activité a besoin pour générer un niveau : l'activité configurée, l'environnement, la graine.</summary>
    public sealed class ActivityContext
    {
        public ActivityDef Def { get; }
        /// <summary>Paramètres complets (valeurs par défaut du type + réglages de l'activité, bornés).</summary>
        public ActivityParameters Parameters { get; }
        public ZoneEnvironment Environment { get; }
        public ulong Seed { get; }

        public ActivityContext(ActivityDef def, ActivityParameters parameters, ZoneEnvironment environment, ulong seed)
        {
            Def = def; Parameters = parameters; Environment = environment; Seed = seed;
        }

        public int Param(string key) => Parameters.Get(key);
    }

    /// <summary>
    /// Un type d'activité (pêche, survie, arène, potion, dialogue, réflexe…) : son schéma de paramètres et son générateur de niveau.
    /// Pour ajouter une activité à venir : implémenter cette interface et l'enregistrer dans <see cref="ActivityRegistry"/>.
    /// </summary>
    public interface IActivityKind
    {
        string Id { get; }
        IReadOnlyList<ParameterDef> Schema { get; }
        ActivityLevel Generate(ActivityContext context);
    }

    /// <summary>Registre des types d'activité ; <see cref="Default"/> contient ceux du prototype.</summary>
    public sealed class ActivityRegistry
    {
        readonly Dictionary<string, IActivityKind> _kinds = new Dictionary<string, IActivityKind>();

        public IEnumerable<IActivityKind> Kinds => _kinds.Values;

        public ActivityRegistry Register(IActivityKind kind)
        {
            _kinds[kind.Id] = kind;
            return this;
        }

        public bool TryGet(string id, out IActivityKind kind) => _kinds.TryGetValue(id, out kind!);

        public IActivityKind Get(string id)
        {
            if (!_kinds.TryGetValue(id, out var kind)) throw new InvalidOperationException("Type d'activité inconnu : " + id);
            return kind;
        }

        /// <summary>Paramètres complets d'une activité : chaque clé du schéma est présente, la valeur de l'activité (ou le défaut) bornée.</summary>
        public ActivityParameters Resolve(ActivityDef def)
        {
            var kind = Get(def.Kind);
            var values = new Dictionary<string, int>();
            foreach (var p in kind.Schema) values[p.Key] = p.Clamp(def.Parameters.Get(p.Key, p.Default));
            return new ActivityParameters(values);
        }

        static ActivityRegistry? _default;

        /// <summary>Types du prototype : réflexe (historique), potion, survie en neige, arène de combat, dialogue, et les deux mini-jeux 2D de héros (rempart, course des toits).</summary>
        public static ActivityRegistry Default => _default ??= new ActivityRegistry()
            .Register(new ReflexKind())
            .Register(new PotionKind())
            .Register(new SnowSurvivalKind())
            .Register(new CombatArenaKind())
            .Register(new DeflectKind())
            .Register(new RooftopRunKind())
            .Register(new DialogueKind())
            .Register(new RhythmKind())
            .Register(new FreeThrowKind())
            .Register(new CratePickKind())
            .Register(new DnaAnalysisKind())
            .Register(new GearsKind());
    }

    /// <summary>Aides de génération : directions entières (pas de trigonométrie : même niveau sur toutes les plateformes).</summary>
    static class Gen
    {
        /// <summary>16 directions unitaires ×1000.</summary>
        public static readonly (int X, int Z)[] Dir16 =
        {
            (1000, 0), (924, 383), (707, 707), (383, 924), (0, 1000), (-383, 924), (-707, 707), (-924, 383),
            (-1000, 0), (-924, -383), (-707, -707), (-383, -924), (0, -1000), (383, -924), (707, -707), (924, -383),
        };

        public static (int X, int Z) Around(IRandom rng, int minRadius, int maxRadius)
        {
            var d = Dir16[rng.Range(0, 16)];
            int r = rng.Range(minRadius, maxRadius + 1);
            return (d.X * r / 1000, d.Z * r / 1000);
        }

        public static void Shuffle<T>(IRandom rng, IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
