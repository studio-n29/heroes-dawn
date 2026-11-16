#nullable enable
using System;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Textes générés des conditions, modificateurs et ciblage fin (plan P1, tâche 6).</summary>
    public sealed class SkillTextConditionTests
    {
        static EffectDef Dmg(int v = 100, Condition? when = null, params ValueMod[] mods) => new EffectDef(EffectType.Damage, v, when: when, mods: mods);
        static string[] Lines(params EffectDef[] fx) => SkillText.Effects(fx).ToArray();
        static SkillDef Skill(TargetPick pick, StatusType pickStatus = StatusType.None)
            => new SkillDef(new SkillId("x"), SkillSlot.A1, 0, TargetRule.SingleEnemy, new[] { Dmg() }, pick: pick, pickStatus: pickStatus);

        [Test, Description("Vérifie qu'un effet sans condition ni modificateur garde exactement le texte d'avant (cartes Braise et Mur de verre) — parce que le texte existant ne doit pas bouger")]
        public void PlainEffects_KeepExactTextOfExistingCards()
        {
            var col = PrototypeCollection.Build();
            var ember = col.Card(new CardId("card_ember"));
            var wall = col.Card(new CardId("card_glass_wall"));
            int threshold = new BalanceConfig().ExecuteThresholdPercent;
            Assert.That(SkillText.Describe(ember.Skill), Is.EqualTo("1 ennemi · Dégâts 140 %"));
            Assert.That(ember.Tiers.Select(CollectionText.TierEffects),
                Is.EqualTo(new[] { "Brûlure 2 t", "Rebond 50 %", "Exécution +50 % sous " + threshold + " % PV" }));
            Assert.That(SkillText.Describe(wall.Skill), Is.EqualTo("Toute l'équipe · Bouclier 15 % PV"));
            Assert.That(wall.Tiers.Select(CollectionText.TierEffects), Is.EqualTo(new[] { "Purge (1)", "Ancrage 1 t", "Régén. 2 t" }));
        }

        [Test, Description("Vérifie le texte exact d'un modificateur fixe sous condition de statut — parce que le joueur doit lire « +40 % si la cible subit Brûlure »")]
        public void FlatMod_TargetHasStatus_ExactText()
        {
            var lines = Lines(Dmg(100, null, new ValueMod(40, new Condition(ConditionKind.TargetHasStatus, StatusType.Burn))));
            Assert.That(lines, Is.EqualTo(new[] { "Dégâts 100 %", "+40 % si la cible subit Brûlure" }));
        }

        [Test, Description("Vérifie le texte exact d'une condition de PV sur un effet conditionnel — parce que l'effet doit être préfixé « Si … : »")]
        public void ConditionalEffect_HpBelow_ExactText()
        {
            var lines = Lines(Dmg(80, new Condition(ConditionKind.TargetHpBelow, StatusType.None, 50)));
            Assert.That(lines, Is.EqualTo(new[] { "Si la cible est sous 50 % PV : Dégâts 80 %" }));
        }

        [Test, Description("Vérifie le texte exact d'un modificateur à échelle plafonné — parce que le joueur doit voir le bonus par unité et le maximum")]
        public void ScaledMod_ExactText()
        {
            var lines = Lines(Dmg(100, null, new ValueMod(15, null, ScaleKind.TargetStatusCount, 3)));
            Assert.That(lines, Is.EqualTo(new[] { "Dégâts 100 %", "+15 % par statut sur la cible (max 3)" }));
        }

        [TestCase(ConditionKind.TargetHasStatus, "la cible subit Brûlure"), TestCase(ConditionKind.TargetLacksStatus, "la cible ne subit pas Brûlure"),
         TestCase(ConditionKind.TargetHpBelow, "la cible est sous 2 % PV"), TestCase(ConditionKind.TargetHpAbove, "la cible est au-dessus de 2 % PV"),
         TestCase(ConditionKind.CasterHasStatus, "le lanceur a Brûlure"), TestCase(ConditionKind.CasterHpBelow, "le lanceur est sous 2 % PV"),
         TestCase(ConditionKind.AnyAllyHasStatus, "un allié a Brûlure"), TestCase(ConditionKind.TargetStatusCountAtLeast, "la cible a 2 statut(s) ou plus"),
         TestCase(ConditionKind.TargetPlannedAction, "la cible prépare une action"), TestCase(ConditionKind.EnemiesAliveAtLeast, "2 ennemi(s) ou plus en vie"),
         TestCase(ConditionKind.EnemiesAliveAtMost, "2 ennemi(s) ou moins en vie")]
        [Description("Vérifie la phrase française exacte de chaque ConditionKind (statut Brûlure, seuil 2) — parce qu'un {0}/{1} inversé ou un accord faux serait affiché au joueur")]
        public void EveryConditionKind_ExactSentence(ConditionKind kind, string expected)
        {
            Assert.That(Lines(Dmg(100, new Condition(kind, StatusType.Burn, 2))), Is.EqualTo(new[] { "Si " + expected + " : Dégâts 100 %" }));
            Assert.That(Enum.GetValues(typeof(ConditionKind)).Length, Is.EqualTo(11), "un ConditionKind a été ajouté : compléter la table");
        }

        [TestCase(ScaleKind.TargetStatusCount, "statut sur la cible"), TestCase(ScaleKind.EnemiesAlive, "ennemi en vie"), TestCase(ScaleKind.AlliesAlive, "allié en vie"),
         TestCase(ScaleKind.TargetMissingHpTenths, "10 % de PV manquants de la cible"), TestCase(ScaleKind.CasterMissingHpTenths, "10 % de PV manquants du lanceur")]
        [Description("Vérifie la phrase française exacte de chaque ScaleKind (bonus 10, max 2) — parce qu'une grandeur mal nommée tromperait le joueur sur le calcul")]
        public void EveryScaleKind_ExactSentence(ScaleKind kind, string expected)
        {
            Assert.That(Lines(Dmg(100, null, new ValueMod(10, null, kind, 2))), Is.EqualTo(new[] { "Dégâts 100 %", "+10 % par " + expected + " (max 2)" }));
            Assert.That(Enum.GetValues(typeof(ScaleKind)).Length, Is.EqualTo(6), "un ScaleKind a été ajouté : compléter la table");
        }

        [Test, Description("Vérifie qu'un modificateur sans condition ni échelle s'affiche seul avec son signe — parce que fx.mod_always doit produire « +25 % »")]
        public void ModAlways_ExactText()
            => Assert.That(Lines(Dmg(100, null, new ValueMod(25))), Is.EqualTo(new[] { "Dégâts 100 %", "+25 %" }));

        [Test, Description("Vérifie que le bonus d'une jauge est converti en % comme la ligne de base (points sur 1000 divisés par 10, division entière) — parce que +40 points valent +4 %")]
        public void GaugeMod_DividesByTen_LikeBaseLine()
        {
            var g = new EffectDef(EffectType.GaugeDelta, 80, mods: new[] { new ValueMod(10, null, ScaleKind.AlliesAlive, 4), new ValueMod(40, new Condition(ConditionKind.TargetHasStatus, StatusType.Burn)), new ValueMod(5) });
            Assert.That(Lines(g), Is.EqualTo(new[] { "Jauge +8 %", "+1 % par allié en vie (max 4)", "+4 % si la cible subit Brûlure", "0 %" }));
        }

        [Test, Description("Vérifie que le bonus d'une jauge négative (ralentissement) garde le signe de la base — parce que le moteur AUGMENTE le ralentissement avec un bonus positif et que « +4 % » se lirait comme un ralentissement plus faible")]
        public void GaugeMod_OnASlowdown_KeepsTheSignOfTheBase()
        {
            var slow = new EffectDef(EffectType.GaugeDelta, -80, mods: new[] { new ValueMod(10, null, ScaleKind.AlliesAlive, 4), new ValueMod(40, new Condition(ConditionKind.TargetHasStatus, StatusType.Burn)), new ValueMod(5) });
            Assert.That(Lines(slow), Is.EqualTo(new[] { "Jauge -8 %", "-1 % par allié en vie (max 4)", "-4 % si la cible subit Brûlure", "0 %" }));
            var speed = new EffectDef(EffectType.GaugeDelta, 80, mods: new[] { new ValueMod(40, new Condition(ConditionKind.TargetHasStatus, StatusType.Burn)) });
            Assert.That(Lines(speed), Is.EqualTo(new[] { "Jauge +8 %", "+4 % si la cible subit Brûlure" }));
        }

        [Test, Description("Vérifie que le texte du ciblage « PV les plus hauts » ne parle plus du « plus vaillant » — parce que ce mot subjectif ne dit pas qu'il s'agit du pourcentage de PV le plus haut")]
        public void HighestHpPickText_IsClear()
        {
            var text = Loc.T("pick.HighestHp");
            Assert.That(text, Does.Not.Contain("vaillant"));
            Assert.That(text, Does.Contain("PV"), "le texte dit sur quoi porte le choix");
        }

        [Test, Description("Vérifie qu'avec les statistiques de l'agent les bonus sont convertis en montants bruts comme la ligne de base (dégâts, soin, bouclier), échelle comprise, et restent en % sans elles — parce que « +40 % » à côté de « Dégâts 196 » serait ambigu")]
        public void OwnerMode_ConvertsModsToRaw_AndPercentOtherwise()
        {
            var owner = new SkillOwnerStats(atk: 196, maxHp: 200);
            var burn = new Condition(ConditionKind.TargetHasStatus, StatusType.Burn);
            var dmg = Dmg(100, null, new ValueMod(40, burn), new ValueMod(10, null, ScaleKind.TargetStatusCount, 3));
            Assert.That(SkillText.Effects(new[] { dmg }, owner), Is.EqualTo(new[] { "Dégâts 196", "+78 de dégâts si la cible subit Brûlure", "+19 de dégâts par statut sur la cible (max 3)" }));
            Assert.That(Lines(dmg), Is.EqualTo(new[] { "Dégâts 100 %", "+40 % si la cible subit Brûlure", "+10 % par statut sur la cible (max 3)" }));
            var heal = new EffectDef(EffectType.Heal, 20, mods: new[] { new ValueMod(10) });
            var shield = new EffectDef(EffectType.Shield, 20, mods: new[] { new ValueMod(10) });
            Assert.That(SkillText.Effects(new[] { heal, shield }, owner), Is.EqualTo(new[] { "Soin 40 PV", "+20 PV de soin", "Bouclier 40", "+20 de bouclier" }));
        }

        [Test, Description("Vérifie que Describe mentionne le ciblage fin, avec le statut cherché pour StatusHolder, et rien pour le ciblage par défaut — parce que le joueur doit savoir qui sera visé")]
        public void Describe_MentionsFinePick()
        {
            Assert.That(SkillText.Describe(Skill(TargetPick.LowestHp)), Is.EqualTo("1 ennemi · Cible : le plus faible en PV · Dégâts 100 %"));
            Assert.That(SkillText.Describe(Skill(TargetPick.StatusHolder, StatusType.Burn)), Does.Contain("Cible : porteur de Brûlure"));
            Assert.That(SkillText.Describe(Skill(TargetPick.Default)), Is.EqualTo("1 ennemi · Dégâts 100 %"));
        }

        [Test, Description("Vérifie que la ligne d'un palier de carte montre l'effet conditionnel — parce que le grimoire passe par TierEffects")]
        public void TierLine_ShowsConditionalEffect()
        {
            var tier = new CardTier(3, Dmg(60, new Condition(ConditionKind.TargetHasStatus, StatusType.Burn)));
            Assert.That(CollectionText.TierEffects(tier), Is.EqualTo("Si la cible subit Brûlure : Dégâts 60 %"));
        }

        [Test, Description("Vérifie qu'une clé de texte existe pour chaque ConditionKind, ScaleKind et TargetPick (hors Default) — parce qu'une valeur ajoutée plus tard sans texte afficherait une clé brute")]
        public void AllKeysExist_ForEveryKind()
        {
            foreach (ConditionKind k in Enum.GetValues(typeof(ConditionKind))) Assert.That(Loc.Has("cond." + k), "cond." + k);
            foreach (ScaleKind k in Enum.GetValues(typeof(ScaleKind))) Assert.That(Loc.Has("scale." + k), "scale." + k);
            foreach (TargetPick k in Enum.GetValues(typeof(TargetPick)).Cast<TargetPick>().Where(p => p != TargetPick.Default)) Assert.That(Loc.Has("pick." + k), "pick." + k);
            Assert.That(Loc.Has("fx.mod_flat") && Loc.Has("fx.mod_always") && Loc.Has("fx.mod_scaled") && Loc.Has("fx.conditional"));
        }
    }
}
