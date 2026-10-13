using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Textes localisés pour les présentateurs (port fin autour de <c>Loc</c>).</summary>
    public interface ILocalization
    {
        string T(string key);
        string F(string key, params object[] args);
        bool Has(string key);
        string Or(string key, string fallbackKey);
        string Error(DomainError e);
        string MemberName(MemberDef d);
        string EnemyName(EnemyDef d);
        string SkillName(SkillDef s);
        string SynergyName(SynergyDef s);
        /// <summary>« +6 % Attaque » : ce que la synergie donne à toute l équipe.</summary>
        string SynergyEffect(SynergyDef s);
        string StatusName(StatusType s);
        string FamilyName(Family f);
        string RoleName(Role r);
        string RarityName(Rarity r);
        string RankName(CombatRank r);
        string DescribeSkill(SkillDef s);
    }
}
