#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Noms des agents (les 22 anciens agents ne servent plus qu aux tests, les 30 agents de The Dawn sont dans LocDawnAgents), compétences, déblocages, textes du registre.</summary>
    public static partial class Loc
    {
        /// <summary>Agent (identifiant, nom de code, rôle) : les noms de compétence se déduisent du rôle.</summary>
        static (string Id, string Name, string Role)[] ExtraAgents() => new[]
        {
            ("bastion", "Bastion", "Tank"), ("prism", "Prism", "Damage"), ("warden", "Warden", "Control"), ("cinder", "Cinder", "Damage"),
            ("volt", "Volt", "Control"), ("tide", "Tide", "Support"), ("sinew", "Sinew", "Damage"), ("marrow", "Marrow", "Support"),
            ("radar", "Radar", "Precision"), ("lens", "Lens", "Control"), ("vanguard", "Vanguard", "Tank"), ("oracle", "Oracle", "Support"),
            ("meteor", "Meteor", "Damage"), ("monolith", "Monolith", "Tank"), ("forge", "Forge", "Damage"), ("tempest", "Tempest", "Control"),
            ("mend", "Mend", "Support"), ("mirage", "Mirage", "Precision"), ("lone_wolf", "Lone Wolf", "Damage"), ("hermit", "Hermit", "Precision"),
            ("phoenix", "Phoenix", "Support"), ("aegis", "Aegis", "Tank"),
        };

        static Dictionary<string, string[]> KitNames() => new Dictionary<string, string[]>
        {
            ["Tank"] = new[] { "Poing", "Garde", "Rempart" },
            ["Damage"] = new[] { "Tir", "Salve", "Coup fatal" },
            ["Control"] = new[] { "Entrave", "Verrou", "Onde" },
            ["Support"] = new[] { "Appui", "Soin", "Aura" },
            ["Precision"] = new[] { "Visée", "Failles", "Rafale" },
        };

        static partial void ExtendAgents(Dictionary<string, string> d)
        {
            var kits = KitNames();
            foreach (var (id, name, role) in ExtraAgents())
            {
                d["members." + id + ".name"] = name;
                var kit = kits[role];
                for (int i = 0; i < 3; i++) d["skills." + id + "_a" + (i + 1)] = kit[i] + " · " + name;
            }

            AddDawnAgents(d, kits);

            d["page.agents"] = "Registre des agents";
            d["agents.title"] = "REGISTRE DES AGENTS";
            d["agents.line"] = "{0} agent(s) sur {1} · {2} à débloquer";
            d["agents.open"] = "REGISTRE";
            d["agents.owned"] = "DANS L'ÉQUIPE";
            d["agents.how.Shop"] = "Boutique de recrutement (rotation quotidienne).";
            d["agents.how.Shop_locked"] = "Boutique de recrutement, après la zone {0}.";
            d["agents.how.Arc"] = "Arc narratif : valide la zone {0}.";
            d["agents.how.Event"] = "Événement du week-end : valide la zone {0} le samedi ou le dimanche.";
            d["agents.how.Event_today"] = "Événement en cours aujourd'hui : valide la zone {0} !";
            d["agents.how.Feat.SoloClear"] = "Prouesse : valide la zone {0} avec un seul agent au combat.";
            d["agents.how.Feat.NoDefeat"] = "Prouesse : valide la zone {0} sans aucune défaite.";
            d["agents.how.Hero"] = "Personnage joueur.";
            d["agents.kind.Hero"] = "PERSONNAGE";
            d["agents.joined"] = "{0} rejoint ton équipe !";
            d["agents.joined_rewards"] = " · {0} rejoint l'équipe";
            d["board.agent_joined"] = "Nouvel agent : {0} !";
            d["agents.kind.Shop"] = "BOUTIQUE";
            d["agents.kind.Arc"] = "ARC";
            d["agents.kind.Event"] = "ÉVÉNEMENT";
            d["agents.kind.Feat"] = "PROUESSE";
        }
    }
}
