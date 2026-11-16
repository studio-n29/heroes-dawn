#nullable enable
using System;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Importeur des fiches Markdown : une ligne de tableau P1 doit donner exactement la carte décrite, et toute notation hors grammaire doit être rejetée.</summary>
    public sealed class CatalogImporterTests
    {
        const string Header = "| Id | Nom | Name | ★ | Famille | Nature | Archétype | Rôle | Cible | Coût | Phase | Base | Niv.3 | Niv.6 | Niv.10 | Idée |";
        const string Separator = "|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|";

        static string Table(string rowLine) => "# Titre\n\nTexte.\n\n" + Header + "\n" + Separator + "\n" + rowLine + "\n\nSuite.\n";

        static string RowLine(string baseCell, string n3 = "St Burn 1t", string n6 = "Steal 15", string n10 = "Exec 30", string cible = "1E",
            string famille = "Matière", string nature = "Offensive", string archetype = "Brûlure", string role = "Amorce", int cost = 50, string phase = "P1", int stars = 2)
            => $"| card_test | Essai | Test | {stars} | {famille} | {nature} | {archetype} | {role} | {cible} | {cost} | {phase} | {baseCell} | {n3} | {n6} | {n10} | Une idée. |";

        static CardRecord Convert(string baseCell, string n3 = "St Burn 1t", string n6 = "Steal 15", string n10 = "Exec 30", string cible = "1E",
            string famille = "Matière", string nature = "Offensive", string archetype = "Brûlure", string role = "Amorce", int cost = 50, string phase = "P1", int stars = 2)
        {
            var rows = CatalogImporter.ReadTable(Table(RowLine(baseCell, n3, n6, n10, cible, famille, nature, archetype, role, cost, phase, stars)));
            Assert.That(rows.Count, Is.EqualTo(1));
            return CatalogImporter.ToRecord(rows[0]);
        }

        static string Cond(bool has, ConditionKind k, StatusType s, int v) => has ? $"{k}/{s}/{v}" : "-";

        static string Mod(ModRecord m) => $"{Cond(m.hasCondition, m.condKind, m.condStatus, m.condValue)}|{m.bonus}|{(m.hasScale ? m.per + "x" + m.maxCount : "-")}";

        static string Fx(EffectRecord e)
            => $"{e.type}:{e.value}:{e.status}:{e.duration}:{e.chance}:{Cond(e.hasCondition, e.condKind, e.condStatus, e.condValue)}:[{string.Join(";", e.mods.Select(Mod))}]";

        static string Fxs(System.Collections.Generic.IEnumerable<EffectRecord> e) => string.Join(" / ", e.Select(Fx));

        static CatalogImportException Rejects(string baseCell, string n3 = "St Burn 1t")
        {
            var ex = Assert.Throws<CatalogImportException>(() => Convert(baseCell, n3));
            Assert.That(ex!.CardId, Is.EqualTo("card_test"));
            return ex;
        }

        // --- Effets simples ---

        [Test, Description("Vérifie qu'une base « Dmg 105, St Burn 2t » donne deux effets dans l'ordre, sans condition ni modificateur — parce que c'est la forme la plus courante des fiches")]
        public void SimpleBase_GivesDamageThenStatus()
        {
            var c = Convert("Dmg 105, St Burn 2t");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo("Damage:105:None:0:100:-:[] / ApplyStatus:0:Burn:2:100:-:[]"));
        }

        [Test, Description("Vérifie qu'un statut suivi de « (35 %) » porte cette chance — parce que « St Shock 1t (35 %) » veut dire durée 1 tour avec 35 % de chance")]
        public void Status_WithChance_KeepsDurationAndChance()
        {
            var c = Convert("Dmg 70, St Shock 1t (35 %)");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo("Damage:70:None:0:100:-:[] / ApplyStatus:0:Shock:1:35:-:[]"));
        }

        [TestCase("Heal 30", "Heal:30:None:0:100:-:[]")]
        [TestCase("Shield 18", "Shield:18:Shield:2:100:-:[]")]
        [TestCase("Push 20", "PushBack:20:None:0:100:-:[]")]
        [TestCase("Adv 15", "Advance:15:None:0:100:-:[]")]
        [TestCase("Steal 15", "Lifesteal:15:None:0:100:-:[]")]
        [TestCase("Purge 1", "Purge:1:None:0:100:-:[]")]
        [TestCase("Bounce 40", "Bounce:40:None:0:100:-:[]")]
        [TestCase("Exec 30", "Execute:30:None:0:100:-:[]")]
        [TestCase("Gauge +120", "GaugeDelta:120:None:0:100:-:[]")]
        [Description("Vérifie que chaque nom d'effet de la grammaire donne son type, sa valeur et son statut de bouclier — parce que la table de correspondance doit être complète")]
        public void EveryEffectName_MapsToItsEffectType(string cell, string expected)
        {
            Assert.That(Fxs(Convert(cell).skill.effects), Is.EqualTo(expected));
        }

        [Test, Description("Vérifie que « Gauge -60 » donne la valeur -60 (points sur 1000, signe conservé) — parce que « Gauge -N » ralentit la cible")]
        public void NegativeGauge_KeepsItsSign()
        {
            Assert.That(Fxs(Convert("Gauge -60").skill.effects), Is.EqualTo("GaugeDelta:-60:None:0:100:-:[]"));
        }

        [Test, Description("Vérifie que « ; » sépare les éléments comme « , » — parce que les deux séparateurs sont admis par la grammaire")]
        public void SemicolonSeparator_IsAccepted()
        {
            var c = Convert("Dmg 100 ; St Burn 2t ; Push 10");
            Assert.That(c.skill.effects.Select(e => e.type), Is.EqualTo(new[] { EffectType.Damage, EffectType.ApplyStatus, EffectType.PushBack }));
        }

        // --- Crochets ---

        [Test, Description("Vérifie que « Dmg 100 [si cible a Burn : +40] » ajoute un modificateur conditionnel de 40 sur le Dmg — parce qu'un « +N » seul est un bonus de l'effet qui précède")]
        public void BareBonus_AfterAnEffect_IsAConditionalMod()
        {
            var c = Convert("Dmg 100 [si cible a Burn : +40]");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo("Damage:100:None:0:100:-:[TargetHasStatus/Burn/0|40|-]"));
        }

        [Test, Description("Vérifie que « Dmg 100, [si cible < 50% PV : Dmg +40] » donne un bonus de valeur sur le Dmg précédent — parce qu'un effet de même type existe déjà dans la cellule")]
        public void TypedBonus_WithAnEffectOfThatType_IsAMod()
        {
            var c = Convert("Dmg 100, [si cible < 50% PV : Dmg +40]");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo("Damage:100:None:0:100:-:[TargetHpBelow/None/50|40|-]"));
        }

        [Test, Description("Vérifie que « [si cible a Vulnerable : Dmg +30] » seul dans un palier donne un effet Dmg 30 conditionnel — parce qu'aucun Dmg n'existe dans la cellule à renforcer")]
        public void TypedBonus_WithoutAnEffectOfThatType_IsAnExtraConditionalEffect()
        {
            var c = Convert("Dmg 100", n3: "[si cible a Vulnerable : Dmg +30]");
            Assert.That(c.tiers[0].level, Is.EqualTo(3));
            Assert.That(Fxs(c.tiers[0].effects), Is.EqualTo("Damage:30:None:0:100:TargetHasStatus/Vulnerable/0:[]"));
        }

        [Test, Description("Vérifie qu'un bonus typé vise l'effet du même type même quand un autre effet le précède — parce que la grammaire cherche le type dans toute la cellule")]
        public void TypedBonus_FindsTheEffectOfItsTypeEvenIfNotPreceding()
        {
            var c = Convert("Dmg 100, Heal 20, Shield 10 [si cible a Burn : Heal +5]");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo(
                "Damage:100:None:0:100:-:[] / Heal:20:None:0:100:-:[TargetHasStatus/Burn/0|5|-] / Shield:10:Shield:2:100:-:[]"));
        }

        [Test, Description("Vérifie qu'un bonus typé est refusé quand deux effets de ce type existent sans que l'un précède — parce que le choix serait une supposition")]
        public void TypedBonus_AmbiguousBetweenTwoEffects_IsRejected()
        {
            Rejects("Dmg 50, Heal 20, Dmg 60, Shield 10 [si cible a Burn : Dmg +5]");
        }

        [Test, Description("Vérifie qu'un bonus « +N » placé après un effet qui ne peut pas le porter (St, Steal, Purge, Bounce, Exec) est rejeté — parce que « sinon » ne vaut que sans effet précédent et qu'un déplacement silencieux vers un autre effet serait une supposition")]
        public void BareBonus_AfterAnEffectThatCannotCarryIt_IsRejected()
        {
            Rejects("Dmg 100, St Burn 2t [si cible a Burn : +30]");
            Rejects("Dmg 110, Steal 15, [si lanceur < 50% PV : +25]");
            Rejects("Dmg 110, Purge 1 [si cible a Burn : +25]");
            Rejects("Dmg 110, Bounce 40 [+10 par ennemi vivant (max 2)]");
            Rejects("Dmg 110, Exec 30 [si cible a Burn : +5]");
        }

        [Test, Description("Vérifie qu'un bonus « +0 » est rejeté — parce qu'un bonus nul ne change rien et trahit une faute de frappe")]
        public void ZeroBonus_IsRejected()
        {
            Rejects("Dmg 100 [si cible a Burn : +0]");
            Rejects("Dmg 100 [si cible a Burn : Dmg +0]");
        }

        [Test, Description("Vérifie que « Gauge +N » est un bonus typé quand un Gauge existe, et que « Shield +N » sans Shield donne un effet Shield conditionnel — parce que ces types suivent la même règle que Dmg")]
        public void TypedBonus_GaugeAndShield_FollowTheSameRule()
        {
            Assert.That(Fxs(Convert("Gauge -60 [si cible a Burn : Gauge +20]").skill.effects), Is.EqualTo("GaugeDelta:-60:None:0:100:-:[TargetHasStatus/Burn/0|20|-]"));
            Assert.That(Fxs(Convert("Dmg 100 [si cible a Burn : Shield +12]").skill.effects), Is.EqualTo(
                "Damage:100:None:0:100:-:[] / Shield:12:Shield:2:100:TargetHasStatus/Burn/0:[]"));
        }

        [Test, Description("Vérifie que les formes ASCII « >= » et « <= » valent « ≥ » et « ≤ » — parce qu'une fiche saisie au clavier doit être lue pareil")]
        public void AsciiComparisons_AreAccepted()
        {
            var a = Convert("Dmg 100 [si >= 3 ennemis vivants : +10] [si <= 2 ennemis vivants : +5] [si cible a >= 2 statuts : +7]").skill.effects[0].mods;
            Assert.That(string.Join(";", a.Select(Mod)), Is.EqualTo("EnemiesAliveAtLeast/None/3|10|-;EnemiesAliveAtMost/None/2|5|-;TargetStatusCountAtLeast/None/2|7|-"));
        }

        [Test, Description("Vérifie qu'un compte d'échelle inconnu et « PV manquants » sans « de » sont rejetés — parce que seuls les cinq comptes du plan existent")]
        public void UnknownScaleCount_IsRejected()
        {
            Rejects("Dmg 100 [+10 par pierre (max 3)]");
            Rejects("Dmg 100 [+10 par 10 % PV manquants de la cible (max 3)]");
            Rejects("Dmg 100 [+10 par 20 % de PV manquants de la cible (max 3)]");
            Rejects("Dmg 100 [+10 par statut de la cible]");
        }

        [Test, Description("Vérifie qu'un crochet seul placé avant l'effet vise le premier effet porteur de la cellule — parce qu'aucun effet ne le précède")]
        public void BareBonus_BeforeAnyEffect_TargetsTheFirstValueEffect()
        {
            var c = Convert("[si cible a Burn : +40], Dmg 100");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo("Damage:100:None:0:100:-:[TargetHasStatus/Burn/0|40|-]"));
        }

        [Test, Description("Vérifie qu'un bonus « +N » est refusé quand la cellule n'a aucun effet de valeur — parce qu'il n'aurait rien à renforcer")]
        public void BareBonus_WithNoValueEffect_IsRejected()
        {
            Rejects("St Burn 2t [si cible a Burn : +30]");
        }

        [Test, Description("Vérifie que « Shield 18, St Taunt 1t [si ≥ 3 ennemis vivants : St Taunt 2t] » ajoute un St Taunt 2t conditionnel — parce qu'un effet complet dans un crochet est un effet supplémentaire")]
        public void FullEffectInBracket_IsAnExtraConditionalEffect()
        {
            var c = Convert("Shield 18, St Taunt 1t [si ≥ 3 ennemis vivants : St Taunt 2t]");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo(
                "Shield:18:Shield:2:100:-:[] / ApplyStatus:0:Taunt:1:100:-:[] / ApplyStatus:0:Taunt:2:100:EnemiesAliveAtLeast/None/3:[]"));
        }

        [Test, Description("Vérifie que « Dmg 95 [+15 par statut de la cible (max 3)] » donne un modificateur à échelle plafonné à 3 unités — parce que le max compte des unités")]
        public void ScaleBracket_GivesAScaledMod()
        {
            var c = Convert("Dmg 95 [+15 par statut de la cible (max 3)]");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo("Damage:95:None:0:100:-:[-|15|TargetStatusCountx3]"));
        }

        [TestCase("+10 par ennemi vivant (max 4)", "EnemiesAlivex4")]
        [TestCase("+10 par allié vivant (max 2)", "AlliesAlivex2")]
        [TestCase("+8 par 10 % de PV manquants de la cible (max 5)", "TargetMissingHpTenthsx5")]
        [TestCase("+8 par 10 % de PV manquants du lanceur (max 5)", "CasterMissingHpTenthsx5")]
        [Description("Vérifie que chaque compte d'échelle de la grammaire est reconnu avec son plafond — parce que les cinq comptes sont admis par le plan")]
        public void EveryScaleCount_IsRecognised(string bracket, string expectedScale)
        {
            var c = Convert($"Dmg 100 [{bracket}]");
            Assert.That(c.skill.effects[0].mods.Single().hasScale, Is.True);
            Assert.That(Mod(c.skill.effects[0].mods[0]).EndsWith("|" + expectedScale), Is.True, Mod(c.skill.effects[0].mods[0]));
        }

        [Test, Description("Vérifie qu'un « max 40 » avec un bonus de +10 est rejeté avec « max > 10 » — parce que max est un nombre d'unités et non un plafond de total")]
        public void ScaleMax_AboveTen_IsRejected()
        {
            var ex = Rejects("Dmg 100 [+10 par statut de la cible (max 40)]");
            Assert.That(ex.Reason, Does.Contain("max > 10"));
            Assert.That(ex.Cell, Is.Not.Empty);
        }

        [Test, Description("Vérifie qu'un « max 0 » est rejeté — parce qu'une échelle exige au moins une unité comptée")]
        public void ScaleMax_Zero_IsRejected()
        {
            Rejects("Dmg 100 [+10 par statut de la cible (max 0)]");
        }

        [Test, Description("Vérifie qu'une cellule à deux crochets garde les deux modificateurs dans l'ordre — parce qu'un effet peut porter plusieurs crochets")]
        public void TwoBrackets_OnOneEffect_KeepBothModsInOrder()
        {
            var c = Convert("Dmg 100 [si cible a Burn : +40] [+15 par statut de la cible (max 3)]");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo("Damage:100:None:0:100:-:[TargetHasStatus/Burn/0|40|- ; -|15|TargetStatusCountx3]".Replace(" ; ", ";")));
        }

        [Test, Description("Vérifie qu'un bonus et un effet supplémentaire dans deux crochets du même élément sont tous deux gardés — parce qu'aucun crochet ne doit être perdu")]
        public void TwoBrackets_ModThenExtraEffect_AreBothKept()
        {
            var c = Convert("Dmg 100 [si cible a Burn : +40] [si cible a Freeze : St Mark 1t]");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo(
                "Damage:100:None:0:100:-:[TargetHasStatus/Burn/0|40|-] / ApplyStatus:0:Mark:1:100:TargetHasStatus/Freeze/0:[]"));
        }

        // --- Conditions ---

        [TestCase("cible a Burn", ConditionKind.TargetHasStatus, StatusType.Burn, 0)]
        [TestCase("cible sans Shock", ConditionKind.TargetLacksStatus, StatusType.Shock, 0)]
        [TestCase("cible < 50% PV", ConditionKind.TargetHpBelow, StatusType.None, 50)]
        [TestCase("cible > 70% PV", ConditionKind.TargetHpAbove, StatusType.None, 70)]
        [TestCase("lanceur a Haste", ConditionKind.CasterHasStatus, StatusType.Haste, 0)]
        [TestCase("lanceur < 40% PV", ConditionKind.CasterHpBelow, StatusType.None, 40)]
        [TestCase("un allié a Regen", ConditionKind.AnyAllyHasStatus, StatusType.Regen, 0)]
        [TestCase("cible a ≥ 2 statuts", ConditionKind.TargetStatusCountAtLeast, StatusType.None, 2)]
        [TestCase("cible a annoncé son action", ConditionKind.TargetPlannedAction, StatusType.None, 0)]
        [TestCase("≥ 3 ennemis vivants", ConditionKind.EnemiesAliveAtLeast, StatusType.None, 3)]
        [TestCase("≤ 1 ennemis vivants", ConditionKind.EnemiesAliveAtMost, StatusType.None, 1)]
        [Description("Vérifie que chacune des onze conditions de la grammaire est reconnue avec son statut et son seuil — parce qu'une condition mal lue changerait le sens de la carte")]
        public void EveryCondition_IsRecognised(string condition, ConditionKind kind, StatusType status, int value)
        {
            var c = Convert($"Dmg 100 [si {condition} : +10]");
            var m = c.skill.effects[0].mods.Single();
            Assert.That(Cond(m.hasCondition, m.condKind, m.condStatus, m.condValue), Is.EqualTo($"{kind}/{status}/{value}"));
        }

        [Test, Description("Vérifie que les espaces insécables, les espaces multiples et « 50 % » avec espace sont acceptés — parce que les fiches utilisent « < 50% PV » et « ≥ 2 statuts » avec des caractères Unicode et des espacements variés")]
        public void OddWhitespace_IsTolerated()
        {
            var c = Convert("St\u00A0 Burn\u00A02t ,\tDmg\u00A0100  [si\u00A0cible <\u00A050 % PV :\u202F+40]  [ si cible a ≥\u00A02\u00A0statuts : Dmg +10 ]");
            Assert.That(Fxs(c.skill.effects), Is.EqualTo(
                "ApplyStatus:0:Burn:2:100:-:[] / Damage:100:None:0:100:-:[TargetHpBelow/None/50|40|-;TargetStatusCountAtLeast/None/2|10|-]"));
        }

        [Test, Description("Vérifie qu'une condition inconnue est rejetée — parce que ne jamais ignorer un crochet incompris est la règle de l'importeur")]
        public void UnknownCondition_IsRejected()
        {
            var ex = Rejects("Dmg 100 [si la lune est pleine : +40]");
            Assert.That(ex.Reason, Is.Not.Empty);
        }

        [Test, Description("Vérifie qu'un statut inconnu dans une condition est rejeté — parce que les noms de statut sont ceux de l'énumération")]
        public void UnknownStatusInCondition_IsRejected()
        {
            Rejects("Dmg 100 [si cible a Poison : +40]");
        }

        [Test, Description("Vérifie qu'un crochet sans « : » ou jamais refermé est rejeté — parce qu'une syntaxe cassée ne doit rien importer")]
        public void MalformedBrackets_AreRejected()
        {
            Rejects("Dmg 100 [si cible a Burn +40]");
            Rejects("Dmg 100 [si cible a Burn : +40");
            Rejects("Dmg 100 si cible a Burn : +40]");
            Rejects("Dmg 100 [si cible a Burn : +40] truc");
        }

        [Test, Description("Vérifie que la forme « +N Dmg » et un corps à deux éléments sont rejetés — parce qu'ils sortent de la grammaire et ne doivent pas être devinés")]
        public void BodyOutsideTheGrammar_IsRejected()
        {
            Rejects("Dmg 100 [si cible a Burn : +40 Dmg]");
            Rejects("Dmg 100 [si cible a Burn : Purge 1, +Heal 10]");
            Rejects("Dmg 100 [si cible a Burn : Heal +10 ; Purge 1]");
        }

        [Test, Description("Vérifie qu'une condition ou un modificateur sur Steal, Purge, Bounce ou Exec est rejeté — parce que la grammaire les réserve à Dmg, Heal, Shield, Adv, Push, Gauge et St")]
        public void ConditionOnUnsupportedEffect_IsRejected()
        {
            Rejects("Dmg 100 [si cible a Burn : Purge 1]");
            Rejects("Dmg 100 [si cible a Burn : Steal 10]");
            Rejects("Dmg 100 [si cible a Burn : Exec 30]");
            Rejects("Dmg 100 [si cible a Burn : Bounce 30]");
            Rejects("Dmg 100 [si cible a Burn : Steal +10]");
        }

        // --- Effets inconnus et garbage ---

        [Test, Description("Vérifie qu'un effet inconnu lève CatalogImportException avec l'id de la carte et la cellule — parce qu'aucun effet ne doit être perdu en silence")]
        public void UnknownEffect_IsRejected_WithCardIdAndCell()
        {
            var ex = Assert.Throws<CatalogImportException>(() => Convert("Dmg 100, Zap 5"));
            Assert.That(ex!.CardId, Is.EqualTo("card_test"));
            Assert.That(ex.Cell, Is.EqualTo("Base"));
            Assert.That(ex.Message, Does.Contain("card_test"));
            var tier = Assert.Throws<CatalogImportException>(() => Convert("Dmg 100", n6: "Zap 5"));
            Assert.That(tier!.Cell, Is.EqualTo("Niv.6"));
        }

        [TestCase("Dmg")]
        [TestCase("Dmg abc")]
        [TestCase("Dmg 100 foo")]
        [TestCase("Dmg -5")]
        [TestCase("Dmg 0")]
        [TestCase("Dmg 100,")]
        [TestCase(",Dmg 100")]
        [TestCase("St Poison 2t")]
        [TestCase("St Burn 2")]
        [TestCase("St Burn")]
        [TestCase("St Burn 2t (35)")]
        [TestCase("St Burn 2t (0 %)")]
        [TestCase("St Burn 2t (101 %)")]
        [TestCase("St None 2t")]
        [TestCase("Gauge 60")]
        [TestCase("Dmg100")]
        [TestCase("")]
        [Description("Vérifie que les effets mal formés sont rejetés — parce que l'importeur ne doit jamais accepter à moitié un texte qu'il ne comprend pas")]
        public void MalformedEffects_AreRejected(string cell)
        {
            Rejects(cell);
        }

        // --- Cibles ---

        [TestCase("1E", TargetRule.SingleEnemy, TargetPick.Default, StatusType.None)]
        [TestCase("ZE", TargetRule.AllEnemies, TargetPick.Default, StatusType.None)]
        [TestCase("1A", TargetRule.SingleAlly, TargetPick.Default, StatusType.None)]
        [TestCase("ZA", TargetRule.AllAllies, TargetPick.Default, StatusType.None)]
        [TestCase("Soi", TargetRule.Self, TargetPick.Default, StatusType.None)]
        [TestCase("1E:faible", TargetRule.SingleEnemy, TargetPick.LowestHp, StatusType.None)]
        [TestCase("1E:fort", TargetRule.SingleEnemy, TargetPick.HighestHp, StatusType.None)]
        [TestCase("1E:prochain", TargetRule.SingleEnemy, TargetPick.NextToAct, StatusType.None)]
        [TestCase("1E:aléatoire", TargetRule.SingleEnemy, TargetPick.Random, StatusType.None)]
        [TestCase("1E:Burn", TargetRule.SingleEnemy, TargetPick.StatusHolder, StatusType.Burn)]
        [TestCase("1A:faible", TargetRule.SingleAlly, TargetPick.LowestHp, StatusType.None)]
        [TestCase("1A:prochain", TargetRule.SingleAlly, TargetPick.NextToAct, StatusType.None)]
        [TestCase("1A:Regen", TargetRule.SingleAlly, TargetPick.StatusHolder, StatusType.Regen)]
        [TestCase("2E:aléatoires", TargetRule.AllEnemies, TargetPick.TwoRandom, StatusType.None)]
        [Description("Vérifie que chaque notation de cible donne sa règle et son ciblage fin — parce que la colonne Cible est la seule source du ciblage")]
        public void Target_MapsToRuleAndPick(string cible, TargetRule rule, TargetPick pick, StatusType status)
        {
            var s = Convert("Dmg 100", cible: cible).skill;
            Assert.That(s.target, Is.EqualTo(rule));
            Assert.That(s.pick, Is.EqualTo(pick));
            Assert.That(s.pickStatus, Is.EqualTo(status));
        }

        [TestCase("1A:fort")]
        [TestCase("1A:aléatoire")]
        [TestCase("ZE:faible")]
        [TestCase("Soi:faible")]
        [TestCase("2E:faible")]
        [TestCase("3E")]
        [TestCase("2E")]
        [TestCase("1E:Poison")]
        [TestCase("1E:")]
        [TestCase("")]
        [Description("Vérifie que les cibles hors grammaire sont rejetées — parce que 1A:fort ou ZE:faible n'existent pas")]
        public void UnknownTarget_IsRejected(string cible)
        {
            var ex = Assert.Throws<CatalogImportException>(() => Convert("Dmg 100", cible: cible));
            Assert.That(ex!.Cell, Is.EqualTo("Cible"));
        }

        // --- Colonnes ---

        [TestCase("Matière", Family.Matter)]
        [TestCase("Élémentaire", Family.Elemental)]
        [TestCase("Physique/Bio", Family.PhysicalBio)]
        [TestCase("Perception", Family.Perception)]
        [Description("Vérifie que chaque famille française donne la bonne énumération — parce que la fiche est en français et le code en anglais")]
        public void Family_IsMapped(string text, Family expected)
        {
            Assert.That(Convert("Dmg 100", famille: text).family, Is.EqualTo(expected));
        }

        [TestCase("Offensive", CardNature.Offensive)]
        [TestCase("Contrôle", CardNature.Control)]
        [TestCase("Défensive", CardNature.Defensive)]
        [TestCase("Soutien", CardNature.Support)]
        [Description("Vérifie que chaque nature française donne la bonne énumération — parce que Contrôle et Soutien ne portent pas le nom de l'énumération")]
        public void Nature_IsMapped(string text, CardNature expected)
        {
            Assert.That(Convert("Dmg 100", nature: text).nature, Is.EqualTo(expected));
        }

        [TestCase("Amorce", CardRole.Primer)]
        [TestCase("Gain", CardRole.Payoff)]
        [TestCase("Pont", CardRole.Bridge)]
        [TestCase("Base", CardRole.Basic)]
        [Description("Vérifie que chaque rôle français donne la bonne énumération — parce que Gain devient Payoff et Base devient Basic")]
        public void Role_IsMapped(string text, CardRole expected)
        {
            Assert.That(Convert("Dmg 100", role: text).role, Is.EqualTo(expected));
        }

        [Test, Description("Vérifie que « Tempo + Brûlure » donne deux archétypes dans cet ordre, « — » aucun, et chaque nom son archétype — parce que l'ordre des archétypes est celui de la fiche")]
        public void Archetypes_AreMapped_InOrder()
        {
            Assert.That(Convert("Dmg 100", archetype: "Tempo + Brûlure").archetypes, Is.EqualTo(new[] { Archetype.Tempo, Archetype.Burn }));
            Assert.That(Convert("Dmg 100", archetype: "—").archetypes, Is.Empty);
            var all = new[] { ("Brûlure", Archetype.Burn), ("Entrave", Archetype.Restrain), ("Traque", Archetype.Hunt), ("Tempo", Archetype.Tempo), ("Rempart", Archetype.Bulwark), ("Renouveau", Archetype.Renewal) };
            foreach (var (text, expected) in all) Assert.That(Convert("Dmg 100", archetype: text).archetypes, Is.EqualTo(new[] { expected }), text);
        }

        [TestCase("Magie")]
        [TestCase("Brûlure + Brûlure")]
        [TestCase("Brûlure + Tempo + Traque")]
        [TestCase("Brûlure +")]
        [Description("Vérifie qu'un archétype inconnu, doublé ou au nombre de trois est rejeté — parce qu'une carte a au plus deux archétypes distincts")]
        public void UnknownArchetype_IsRejected(string text)
        {
            var ex = Assert.Throws<CatalogImportException>(() => Convert("Dmg 100", archetype: text));
            Assert.That(ex!.Cell, Is.EqualTo("Archétype"));
        }

        [Test, Description("Vérifie qu'une famille, une nature ou un rôle inconnu est rejeté — parce qu'aucune colonne ne doit être devinée")]
        public void UnknownFamilyNatureRole_AreRejected()
        {
            Assert.Throws<CatalogImportException>(() => Convert("Dmg 100", famille: "Eau"));
            Assert.Throws<CatalogImportException>(() => Convert("Dmg 100", nature: "Magique"));
            Assert.Throws<CatalogImportException>(() => Convert("Dmg 100", role: "Chef"));
        }

        [Test, Description("Vérifie qu'un coût de 50 donne un coût de compétence de 50, une recharge de 5, l'emplacement A3 et l'identifiant de la carte — parce que la recharge vaut le coût divisé par 10")]
        public void Cost_GivesCostAndCooldown_AndSlotAndIds()
        {
            var c = Convert("Dmg 100", cost: 50, stars: 3);
            Assert.That(c.id, Is.EqualTo("card_test"));
            Assert.That(c.skill.id, Is.EqualTo("card_test"));
            Assert.That(c.stars, Is.EqualTo(3));
            Assert.That(c.skill.cost, Is.EqualTo(50));
            Assert.That(c.skill.cooldown, Is.EqualTo(5));
            Assert.That(c.skill.slot, Is.EqualTo(SkillSlot.A3));
            Assert.That(Convert("Dmg 100", cost: 60).skill.cooldown, Is.EqualTo(6));
            Assert.That(Convert("Dmg 100", cost: 70).skill.cooldown, Is.EqualTo(7));
        }

        [Test, Description("Vérifie que les trois cellules de palier donnent trois paliers 3, 6 et 10 avec leurs effets — parce que chaque carte a exactement trois paliers")]
        public void Tiers_AreLevels3_6_10()
        {
            var c = Convert("Dmg 100", n3: "St Burn 2t", n6: "Steal 15", n10: "Exec 30, Bounce 40");
            Assert.That(c.tiers.Select(t => t.level), Is.EqualTo(new[] { 3, 6, 10 }));
            Assert.That(Fxs(c.tiers[0].effects), Is.EqualTo("ApplyStatus:0:Burn:2:100:-:[]"));
            Assert.That(Fxs(c.tiers[1].effects), Is.EqualTo("Lifesteal:15:None:0:100:-:[]"));
            Assert.That(Fxs(c.tiers[2].effects), Is.EqualTo("Execute:30:None:0:100:-:[] / Bounce:40:None:0:100:-:[]"));
        }

        [Test, Description("Vérifie qu'un palier vide est rejeté — parce que chaque carte doit avoir trois paliers renseignés")]
        public void EmptyTier_IsRejected()
        {
            var ex = Assert.Throws<CatalogImportException>(() => Convert("Dmg 100", n6: " "));
            Assert.That(ex!.Cell, Is.EqualTo("Niv.6"));
        }

        // --- Phases ---

        [TestCase("P2")]
        [TestCase("P3")]
        [Description("Vérifie que les phases P2 et P3 sont lues mais non converties — parce que leurs briques n'existent pas encore dans le moteur")]
        public void PhaseP2P3_IsReadButNotConverted(string phase)
        {
            var rows = CatalogImporter.ReadTable(Table(RowLine("Dmg 100 [Réaction 2 t : quand un ennemi est vaincu → Heal 10]", phase: phase)));
            Assert.That(rows.Count, Is.EqualTo(1));
            Assert.That(rows[0].Phase, Is.EqualTo(phase));
            Assert.Throws<NotSupportedException>(() => CatalogImporter.ToRecord(rows[0]));
        }

        [Test, Description("Vérifie qu'une phase inconnue est rejetée à la lecture — parce que seules P1, P2 et P3 existent")]
        public void UnknownPhase_IsRejected()
        {
            Assert.Throws<CatalogImportException>(() => CatalogImporter.ReadTable(Table(RowLine("Dmg 100", phase: "P9"))));
        }

        // --- Lecture du tableau ---

        [Test, Description("Vérifie que ReadTable lit les 16 colonnes d'une ligne — parce que CatalogRow porte toutes les colonnes de la fiche")]
        public void ReadTable_ReadsAllSixteenColumns()
        {
            var r = CatalogImporter.ReadTable(Table(RowLine("Dmg 100", "St Burn 1t", "Steal 15", "Exec 30", cible: "1E:faible", famille: "Perception", nature: "Contrôle", archetype: "Tempo + Brûlure", role: "Pont", cost: 60, stars: 4)))[0];
            Assert.That(r.Id, Is.EqualTo("card_test"));
            Assert.That(r.Nom, Is.EqualTo("Essai"));
            Assert.That(r.Name, Is.EqualTo("Test"));
            Assert.That(r.Stars, Is.EqualTo(4));
            Assert.That(r.Famille, Is.EqualTo("Perception"));
            Assert.That(r.Nature, Is.EqualTo("Contrôle"));
            Assert.That(r.Archetype, Is.EqualTo("Tempo + Brûlure"));
            Assert.That(r.Role, Is.EqualTo("Pont"));
            Assert.That(r.Cible, Is.EqualTo("1E:faible"));
            Assert.That(r.Cost, Is.EqualTo(60));
            Assert.That(r.Phase, Is.EqualTo("P1"));
            Assert.That(r.Base, Is.EqualTo("Dmg 100"));
            Assert.That(r.Niv3, Is.EqualTo("St Burn 1t"));
            Assert.That(r.Niv6, Is.EqualTo("Steal 15"));
            Assert.That(r.Niv10, Is.EqualTo("Exec 30"));
            Assert.That(r.Idee, Is.EqualTo("Une idée."));
        }

        [Test, Description("Vérifie que ReadTable lit plusieurs lignes dans l'ordre et s'arrête à la fin du tableau — parce que le texte autour du tableau n'est pas une fiche")]
        public void ReadTable_ReadsSeveralRows_AndStopsAtTheEndOfTheTable()
        {
            var md = Table(RowLine("Dmg 100").Replace("card_test", "card_a") + "\n" + RowLine("Dmg 110").Replace("card_test", "card_b"));
            Assert.That(CatalogImporter.ReadTable(md).Select(r => r.Id), Is.EqualTo(new[] { "card_a", "card_b" }));
        }

        [Test, Description("Vérifie qu'un en-tête différent, une ligne à 15 colonnes ou une étoile illisible lèvent CatalogImportException — parce qu'un tableau mal formé ne doit pas être lu à moitié")]
        public void ReadTable_RejectsBadHeaderBadWidthAndBadNumbers()
        {
            Assert.Throws<CatalogImportException>(() => CatalogImporter.ReadTable("| Id | Nom |\n|---|---|\n| a | b |\n"));
            Assert.Throws<CatalogImportException>(() => CatalogImporter.ReadTable(Table("| card_x | Nom | Name | 1 | Matière |")));
            Assert.Throws<CatalogImportException>(() => CatalogImporter.ReadTable(Table(RowLine("Dmg 100").Replace("| 2 |", "| deux |"))));
            Assert.Throws<CatalogImportException>(() => CatalogImporter.ReadTable(Table(RowLine("Dmg 100").Replace("| 50 |", "| cinquante |"))));
            Assert.Throws<CatalogImportException>(() => CatalogImporter.ReadTable("Pas de tableau."));
        }

        // --- Fiches réelles ---

        static string CatalogueFolder => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "docs", "catalogue"));

        [Test, Description("Vérifie que les 48 fiches de cartes-existantes.md se convertissent toutes et redonnent les effets, paliers, cibles et étiquettes des cartes du code — parce que l'importeur ne doit rien changer au comportement des cartes existantes")]
        public void ExistingCardsSheet_ConvertsToTheSameEffectsAsTheCode()
        {
            var rows = CatalogImporter.ReadTable(File.ReadAllText(Path.Combine(CatalogueFolder, "cartes-existantes.md")));
            Assert.That(rows.Count, Is.EqualTo(48));
            var legacy = PrototypeCollection.LegacyCards().ToDictionary(c => c.Id.Value);
            Assert.That(rows.Select(r => r.Id).Distinct().Count(), Is.EqualTo(48), "identifiants en double dans la fiche");
            Assert.That(rows.Select(r => r.Id).OrderBy(x => x, StringComparer.Ordinal), Is.EqualTo(legacy.Keys.OrderBy(x => x, StringComparer.Ordinal)));
            foreach (var row in rows)
            {
                var rec = CatalogImporter.ToRecord(row);
                var card = legacy[row.Id];
                Assert.That(rec.family, Is.EqualTo(card.Family), row.Id);
                Assert.That(rec.stars, Is.EqualTo(card.Stars), row.Id);
                Assert.That(rec.nature, Is.EqualTo(card.Nature), row.Id);
                Assert.That(rec.archetypes, Is.EqualTo(card.Archetypes.ToList()), row.Id);
                Assert.That(rec.role, Is.EqualTo(card.Role), row.Id);
                Assert.That(rec.skill.target, Is.EqualTo(card.Skill.Target), row.Id);
                Assert.That(rec.skill.pick, Is.EqualTo(card.Skill.Pick), row.Id);
                Assert.That(rec.skill.pickStatus, Is.EqualTo(card.Skill.PickStatus), row.Id);
                Assert.That(rec.skill.slot, Is.EqualTo(card.Skill.Slot), row.Id);
                Assert.That(rec.skill.id, Is.EqualTo(card.Skill.Id.Value), row.Id);
                Assert.That(rec.id, Is.EqualTo(card.Id.Value), row.Id);
                Assert.That(rec.skill.cost, Is.EqualTo(card.Skill.Cost), row.Id);
                Assert.That(rec.skill.cooldown, Is.EqualTo(card.Skill.Cooldown), row.Id);
                Assert.That(Fxs(rec.skill.effects), Is.EqualTo(string.Join(" / ", card.Skill.Effects.Select(FxOf))), row.Id);
                Assert.That(rec.tiers.Count, Is.EqualTo(card.Tiers.Count), row.Id);
                for (int i = 0; i < card.Tiers.Count; i++)
                {
                    Assert.That(rec.tiers[i].level, Is.EqualTo(card.Tiers[i].Level), row.Id);
                    Assert.That(Fxs(rec.tiers[i].effects), Is.EqualTo(FxOf(card.Tiers[i].Effects.Single())), row.Id);
                }
            }
        }

        static string FxOf(EffectDef e) => $"{e.Type}:{e.Value}:{e.Status}:{e.Duration}:{e.ChancePercent}:-:[]";

        [Test, Description("Vérifie que ReadTable rejette un second en-tête ou une ligne de carte placée après la fin du tableau — parce qu'une ligne ignorée en silence serait une fiche perdue")]
        public void ReadTable_RejectsRowsAfterTheTableEnded()
        {
            Assert.Throws<CatalogImportException>(() => CatalogImporter.ReadTable(Table(RowLine("Dmg 100")) + "\n" + RowLine("Dmg 100").Replace("card_test", "card_late")));
            Assert.Throws<CatalogImportException>(() => CatalogImporter.ReadTable(Table(RowLine("Dmg 100")) + "\n" + Header + "\n" + Separator + "\n"));
        }

        [Test, Description("Vérifie que TOUTE ligne de phase P1 de toutes les fiches du catalogue s'importe sans exception et que les lignes P2/P3 sont ignorées (NotSupportedException) — parce que les fiches doivent respecter la grammaire et qu'un rejet est une fiche à corriger")]
        public void EveryP1Row_Imports()
        {
            var rejected = new System.Collections.Generic.List<string>();
            int imported = 0, skipped = 0;
            foreach (var file in Directory.GetFiles(CatalogueFolder, "cartes-*.md"))
            {
                foreach (var row in CatalogImporter.ReadTable(File.ReadAllText(file)))
                {
                    if (row.Phase != "P1") { Assert.Throws<NotSupportedException>(() => CatalogImporter.ToRecord(row), row.Id); skipped++; continue; }
                    try { CatalogImporter.ToRecord(row); imported++; }
                    catch (CatalogImportException e) { rejected.Add(e.CardId + " : " + e.Message); }
                }
            }
            TestContext.Out.WriteLine($"P1 importées : {imported} ; P2/P3 ignorées : {skipped} ; rejetées : {rejected.Count}");
            Assert.That(rejected, Is.Empty, "fiches P1 hors grammaire :\n" + string.Join("\n", rejected));
            Assert.That(imported, Is.EqualTo(196), "lignes P1 importées");
            Assert.That(skipped, Is.EqualTo(54), "lignes P2/P3 ignorées");
        }
    }
}
