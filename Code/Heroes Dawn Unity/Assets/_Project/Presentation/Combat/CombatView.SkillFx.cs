#nullable enable
using N29.HeroesDawn.Presentation.Combat.Hud;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Animation d'une compétence jouée dans le terrain central : titre, projectile vers chaque cible, éclat propre à sa famille.</summary>
    public sealed partial class CombatView
    {
        /// <inheritdoc />
        public float PlaySkillFx(SkillFxData d, float seconds)
        {
            if (!AnimationsAllowed || seconds <= 0f) return 0f;
            var color = SkillFx.ColorOf(d.Kind);
            hud.ShowArenaTitle(d.Title, color, seconds);
            if (!hud.TryCard(d.ActorId, out var actor)) return seconds;
            var layer = hud.FxLayer;
            var from = layer.WorldToLocal(actor.Root.worldBound.center);
            bool toAllies = d.Kind == SkillFxKind.Heal || d.Kind == SkillFxKind.Shield || d.Kind == SkillFxKind.Haste;
            int shown = 0;
            foreach (var id in d.TargetIds)
            {
                if (shown++ >= 4 || !hud.TryCard(id, out var target)) continue;
                var targetRoot = target.Root;
                var to = layer.WorldToLocal(targetRoot.worldBound.center);
                if (toAllies || id == d.ActorId) HudAnim.After(layer, 0.12f, () => SkillFxHud.Burst(layer, targetRoot, d.Kind, color));
                else SkillFxHud.Projectile(layer, from, to, d.Kind, color, () => SkillFxHud.Burst(layer, targetRoot, d.Kind, color));
            }
            return seconds;
        }
    }
}
