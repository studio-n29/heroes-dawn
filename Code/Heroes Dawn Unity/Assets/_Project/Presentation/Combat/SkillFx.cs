#nullable enable
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Famille d'animation d'une compétence (feu, givre, foudre…), déduite de ses effets.</summary>
    public enum SkillFxKind { Impact, Fire, Ice, Lightning, Acid, Chains, Mark, Heal, Shield, Haste, Time }

    /// <summary>Compétence jouée, à animer dans le terrain central du combat.</summary>
    public struct SkillFxData
    {
        public int ActorId;
        public int[] TargetIds;
        public SkillFxKind Kind;
        /// <summary>Nom de la compétence, affiché dans le terrain.</summary>
        public string Title;
    }

    public static class SkillFx
    {
        /// <summary>
        /// Famille d'animation. Compétence offensive : le premier statut élémentaire (Brûlure, Gel, Choc, Corrosion, Entrave), puis Marque ou
        /// Vulnérable, sinon impact. Compétence sur un allié : Soin, Bouclier, Hâte ou Régénération.
        /// </summary>
        public static SkillFxKind KindOf(SkillDef skill)
        {
            bool onAllies = skill.Target == TargetRule.Self || skill.Target == TargetRule.SingleAlly || skill.Target == TargetRule.AllAllies;
            if (onAllies)
            {
                foreach (var e in skill.Effects)
                {
                    if (e.Type == EffectType.Heal) return SkillFxKind.Heal;
                    if (e.Type == EffectType.Shield) return SkillFxKind.Shield;
                }
                foreach (var e in skill.Effects)
                {
                    if (e.Type == EffectType.Advance) return SkillFxKind.Haste;
                    if (e.Type != EffectType.ApplyStatus) continue;
                    if (e.Status == StatusType.Regen) return SkillFxKind.Heal;
                    if (e.Status == StatusType.Haste) return SkillFxKind.Haste;
                }
                return SkillFxKind.Shield;
            }
            foreach (var e in skill.Effects)
            {
                if (e.Type != EffectType.ApplyStatus) continue;
                switch (e.Status)
                {
                    case StatusType.Burn: return SkillFxKind.Fire;
                    case StatusType.Freeze: return SkillFxKind.Ice;
                    case StatusType.Shock: return SkillFxKind.Lightning;
                    case StatusType.Corrosion: return SkillFxKind.Acid;
                    case StatusType.Bind: return SkillFxKind.Chains;
                }
            }
            foreach (var e in skill.Effects)
            {
                if (e.Type == EffectType.ApplyStatus && (e.Status == StatusType.Mark || e.Status == StatusType.Vulnerable)) return SkillFxKind.Mark;
                if (e.Type == EffectType.GaugeDelta) return SkillFxKind.Time;
            }
            return SkillFxKind.Impact;
        }

        public static Color ColorOf(SkillFxKind kind)
        {
            switch (kind)
            {
                case SkillFxKind.Fire: return new Color(1f, 0.55f, 0.16f);
                case SkillFxKind.Ice: return new Color(0.55f, 0.86f, 1f);
                case SkillFxKind.Lightning: return new Color(1f, 0.9f, 0.3f);
                case SkillFxKind.Acid: return new Color(0.47f, 0.9f, 0.35f);
                case SkillFxKind.Chains: return new Color(0.75f, 0.75f, 0.85f);
                case SkillFxKind.Mark: return new Color(0.94f, 0.31f, 0.31f);
                case SkillFxKind.Heal: return new Color(0.35f, 0.86f, 0.5f);
                case SkillFxKind.Shield: return new Color(0.4f, 0.67f, 1f);
                case SkillFxKind.Haste: return new Color(1f, 0.86f, 0.47f);
                case SkillFxKind.Time: return new Color(0.7f, 0.55f, 1f);
                default: return new Color(1f, 0.95f, 0.8f);
            }
        }
    }
}
