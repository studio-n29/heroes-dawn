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
        /// <summary>Effet passif d une synergie amplifié par le niveau de lien (voir LinkRules).</summary>
        string SynergyEffect(SynergyDef s, int linkLevel);
        /// <summary>Nom et effet d un effet unique de composition d équipe.</summary>
        string TeamEffect(TeamEffectKind kind);
        /// <summary>Nom du thème d'une paire d'agents, d'après le couple de leurs rôles (« Rempart et lame »).</summary>
        string PairTheme(Role a, Role b);
        /// <summary>Un bonus de paire en une ligne (« PV +4 % », « Bouclier d'ouverture 8 % »).</summary>
        string PairBonusText(PairBonus bonus);
        string StatusName(StatusType s);
        string FamilyName(Family f);
        string RoleName(Role r);
        string RarityName(Rarity r);
        string RankName(CombatRank r);
        string DescribeSkill(SkillDef s);
        /// <summary>Description avec les montants bruts (dégâts, soins, boucliers) pour un agent de ces statistiques.</summary>
        string DescribeSkill(SkillDef s, int atk, int maxHp);
    }
}
