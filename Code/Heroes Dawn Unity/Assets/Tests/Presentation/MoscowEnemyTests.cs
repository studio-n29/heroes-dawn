#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Ennemis du chapitre Sergueiv (Moscou) : garde, contrebandier, homme de main, le Passeur.</summary>
    public sealed class MoscowEnemyTests
    {
        static readonly string[] Ids = { "mos_guard", "mos_smuggler", "mos_enforcer", "mos_passeur" };
        static readonly ContentCatalog C = PrototypeContent.Build();

        [Test, Description("Vérifie que les quatre ennemis de Moscou existent et que seul le Passeur est un boss — parce que le chapitre n'a qu'un boss et un seul élite")]
        public void AllFourExist_AndOnlyThePasseurIsBoss()
        {
            foreach (var id in Ids) Assert.That(C.Enemies.Any(e => e.Id.Value == id), Is.True, id);
            Assert.That(PrototypeMoscowEnemies.All().Where(e => e.IsBoss).Select(e => e.Id.Value), Is.EqualTo(new[] { "mos_passeur" }));
            Assert.That(PrototypeMoscowEnemies.All().Select(e => e.Id.Value), Is.EquivalentTo(Ids));
        }

        [Test, Description("Vérifie que le Passeur a deux phases aux seuils 66 % puis 33 % et le style d'IA Contrôle — parce que c'est le même mécanisme que Crocodile et le Titan")]
        public void Passeur_HasTwoPhases_At66And33()
        {
            var p = C.Enemy(new EnemyId("mos_passeur"));
            Assert.That(p.Phases.Select(x => x.ThresholdPercent), Is.EqualTo(new[] { 66, 33 }));
            Assert.That(p.Style, Is.EqualTo(AiStyle.Control));
            Assert.That(p.BaseStats.Hp, Is.EqualTo(5000));
        }

        [Test, Description("Vérifie que les statistiques de départ du §5 de la spec sont respectées — parce que l'équilibrage part de ces valeurs")]
        public void StartingStats_FollowTheSpec()
        {
            StatBlock S(string id) => C.Enemy(new EnemyId(id)).BaseStats;
            Assert.That(S("mos_guard").Hp, Is.EqualTo(1500));
            Assert.That(S("mos_smuggler").Hp, Is.EqualTo(900));
            Assert.That(S("mos_enforcer").Hp, Is.EqualTo(2800));
            Assert.That(C.Enemy(new EnemyId("mos_guard")).Position, Is.EqualTo(Position.FrontLine));
            Assert.That(C.Enemy(new EnemyId("mos_smuggler")).Position, Is.EqualTo(Position.Ranged));
        }

        [Test, Description("Vérifie que chaque ennemi de Moscou a son nom, ses compétences (phases comprises), sa description de bestiaire en français ET en anglais — parce qu'une clé manquante s'afficherait telle quelle")]
        public void EveryMoscowEnemy_HasNameAndSkillTexts_InFrAndEn()
        {
            var keys = new List<string>();
            foreach (var id in Ids)
            {
                var e = C.Enemy(new EnemyId(id));
                keys.Add("enemies." + id + ".name");
                keys.Add("archives.enemy." + id);
                keys.AddRange(e.Skills.Select(s => "skills." + s.Id.Value));
                foreach (var ph in e.Phases)
                {
                    keys.Add(ph.NameKey);
                    if (ph.Skills != null) keys.AddRange(ph.Skills.Select(s => "skills." + s.Id.Value));
                }
            }
            keys.Add("enemies.mos_passeur.phase2");
            keys.Add("enemies.mos_passeur.phase3");
            var en = new HashSet<string>(Loc.EnglishKeys());
            var missingFr = keys.Distinct().Where(k => !Loc.Has(k)).ToList();
            var missingEn = keys.Distinct().Where(k => !en.Contains(k)).ToList();
            Assert.That(missingFr, Is.Empty, "FR : " + string.Join(", ", missingFr));
            Assert.That(missingEn, Is.Empty, "EN : " + string.Join(", ", missingEn));
        }
    }
}
