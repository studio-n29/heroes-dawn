using System.Collections.Generic;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Anciens fichiers d'agents (sans position ni rang) — bloc A de la 1.0.0.</summary>
    public sealed class AgentJsonMigrationTests
    {
        [Test, Description("Vérifie qu'un agent JSON sans champ position ni rang se charge en Première ligne, rang 1, sans exception — parce que les anciens fichiers et les agents de test n'ont pas ces champs")]
        public void Un_agent_json_sans_position_ni_rang_se_charge_en_Premiere_ligne_rang_1()
        {
            var record = JsonUtility.FromJson<AgentRecord>("{\"id\":\"ancien\",\"name\":\"Ancien\",\"family\":1,\"hp\":900,\"atk\":90,\"def\":80,\"spd\":100,\"skills\":[]}");
            var def = ContentDataConverter.ToDef(record, new Dictionary<string, SkillDef>());
            Assert.That(def.Position, Is.EqualTo(Position.FrontLine));
            Assert.That(def.Rank, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un rang saisi hors bornes (0 ou 9) est ramené dans 1..5 — parce que la fiche est éditée à la main")]
        public void Un_rang_hors_bornes_est_ramene_dans_1_5()
        {
            Assert.That(ContentDataConverter.ToDef(new AgentRecord { id = "a", rank = 0 }, new Dictionary<string, SkillDef>()).Rank, Is.EqualTo(1));
            Assert.That(ContentDataConverter.ToDef(new AgentRecord { id = "b", rank = 9 }, new Dictionary<string, SkillDef>()).Rank, Is.EqualTo(5));
        }
    }
}
