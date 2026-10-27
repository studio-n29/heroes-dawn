using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Localisation du jeu : délègue à <see cref="Loc"/> et <see cref="SkillText"/>.</summary>
    public sealed class LocLocalization : ILocalization
    {
        public string T(string key) => Loc.T(key);
        public string F(string key, params object[] args) => Loc.F(key, args);
        public bool Has(string key) => Loc.Has(key);
        public string Or(string key, string fallbackKey) => Loc.Or(key, fallbackKey);
        public string Error(DomainError e) => Loc.Error(e);
        public string MemberName(MemberDef d) => Loc.MemberName(d);
        public string EnemyName(EnemyDef d) => Loc.EnemyName(d);
        public string SkillName(SkillDef s) => Loc.SkillName(s);
        public string SynergyName(SynergyDef s) => Loc.SynergyName(s);
        public string SynergyEffect(SynergyDef s) => Loc.SynergyEffect(s);
        public string SynergyEffect(SynergyDef s, int linkLevel) => Loc.SynergyEffect(s, linkLevel);
        public string TeamEffect(TeamEffectKind kind) => Loc.TeamEffect(kind);
        public string PairTheme(Role a, Role b) => Loc.PairTheme(a, b);
        public string PairBonusText(PairBonus bonus) => Loc.PairBonusText(bonus);
        public string StatusName(StatusType s) => Loc.StatusName(s);
        public string FamilyName(Family f) => Loc.FamilyName(f);
        public string RoleName(Role r) => Loc.RoleName(r);
        public string RarityName(Rarity r) => Loc.RarityName(r);
        public string RankName(CombatRank r) => Loc.RankName(r);
        public string DescribeSkill(SkillDef s) => SkillText.Describe(s);
        public string DescribeSkill(SkillDef s, int atk, int maxHp) => SkillText.Describe(s, new SkillOwnerStats(atk, maxHp));
    }
}
