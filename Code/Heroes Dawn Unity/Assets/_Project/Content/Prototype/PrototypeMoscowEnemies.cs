using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Ennemis du chapitre Sergueiv (Moscou) : garde et contrebandier (base), homme de main (élite), le Passeur (boss).
    /// Écriture identique à <see cref="PrototypeZones.Enemies"/> (mêmes helpers). Valeurs de départ : spec 2026-11-13 §5, réglées par l'équilibrage (tâche 8).
    /// </summary>
    public static class PrototypeMoscowEnemies
    {
        static EffectDef Dmg(int pct) => PrototypeZones.Dmg(pct);
        static EffectDef St(StatusType s, int turns, int chance = 100) => PrototypeZones.St(s, turns, chance);
        static EffectDef Shield(int pct, int turns = 2) => PrototypeZones.Shield(pct, turns);
        static SkillDef S(string id, SkillSlot slot, int cd, TargetRule t, params EffectDef[] fx) => PrototypeZones.S(id, slot, cd, t, fx);

        public static IEnumerable<EnemyDef> All()
        {
            // Base : garde (ligne de front, bouclier + provocation) et contrebandier (tireur, marque).
            yield return PrototypeZones.E("mos_guard", Position.FrontLine, Family.Matter, new StatBlock(1500, 115, 90, 100), false,
                S("mos_guard_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(100)),
                S("mos_guard_a2", SkillSlot.A2, 4, TargetRule.Self, Shield(15), St(StatusType.Taunt, 1)));
            yield return PrototypeZones.E("mos_smuggler", Position.Ranged, Family.Perception, new StatBlock(900, 135, 60, 110), false,
                S("mos_smuggler_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(105)),
                S("mos_smuggler_a2", SkillSlot.A2, 4, TargetRule.SingleEnemy, Dmg(160), St(StatusType.Mark, 2)));
            // Élite : homme de main.
            yield return PrototypeZones.E("mos_enforcer", Position.Attacker, Family.PhysicalBio, new StatBlock(2800, 562, 100, 100), false, // ATQ 125 x4,5 : réglage de la tâche 8 (élites à 60-85 % au niveau recommandé), rapport 2026-11-13_equilibrage-moscou
                S("mos_enforcer_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(110)),
                S("mos_enforcer_a2", SkillSlot.A2, 3, TargetRule.AllEnemies, Dmg(80), St(StatusType.Shock, 1, 30)),
                S("mos_enforcer_a3", SkillSlot.A3, 5, TargetRule.Self, Shield(20)));
            // Boss : le Passeur. Phase 1 « Marchandage » ; sous 66 % « Les clients arrivent » (A2 remplacée par un coup unique) ; sous 33 % « Tout doit partir ».
            yield return PrototypeZones.Styled(PrototypeZones.Phased(PrototypeZones.E("mos_passeur", Position.Attacker, Family.Matter, new StatBlock(5000, 830, 120, 100), true, // ATQ 405 x2,05 : réglage de la tâche 8 (équipe de départ + objets, 30 à 70 % selon le niveau, moyenne 51 %), rapport 2026-11-13_equilibrage-moscou
                S("mos_passeur_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                S("mos_passeur_a2", SkillSlot.A2, 3, TargetRule.Self, Shield(25), St(StatusType.Taunt, 1)),
                S("mos_passeur_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90), St(StatusType.Corrosion, 2, 40)),
                S("mos_passeur_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50))),
                PrototypeZones.Phase(66, "enemies.mos_passeur.phase2", 10, 0, 5,
                    new[]
                    {
                        S("mos_passeur_a1", SkillSlot.A1, 0, TargetRule.SingleEnemy, Dmg(120), St(StatusType.Mark, 2)),
                        S("mos_passeur_p2_a2", SkillSlot.A2, 3, TargetRule.SingleEnemy, Dmg(150), St(StatusType.Corrosion, 2)),
                        S("mos_passeur_a3", SkillSlot.A3, 5, TargetRule.AllEnemies, Dmg(90), St(StatusType.Corrosion, 2, 40)),
                        S("mos_passeur_a4", SkillSlot.A4, 4, TargetRule.SingleEnemy, Dmg(75), St(StatusType.Bind, 1, 50)),
                    }, AiStyle.Aggressive, lines: new[] { new NarrationLineDef("passeur", "narration.mos_1_4.boss.phase1.0") }),
                PrototypeZones.Phase(33, "enemies.mos_passeur.phase3", 25, -10, 10, lines: new[] { new NarrationLineDef("passeur", "narration.mos_1_4.boss.phase2.0") })), AiStyle.Control);
        }
    }
}
