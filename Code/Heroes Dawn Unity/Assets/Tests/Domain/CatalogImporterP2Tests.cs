#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Importeur, grammaire des phases P2 et P3 : réactions, Consomme, Propage et échelle d'archétype.</summary>
    public sealed class CatalogImporterP2Tests
    {
        const string Header = "| Id | Nom | Name | ★ | Famille | Nature | Archétype | Rôle | Cible | Coût | Phase | Base | Niv.3 | Niv.6 | Niv.10 | Idée |";
        const string Separator = "|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|";

        static string Line(string baseCell, string n3, string n6, string n10, string phase, string cible = "1E")
            => $"| card_test | Essai | Test | 2 | Matière | Offensive | Brûlure | Amorce | {cible} | 50 | {phase} | {baseCell} | {n3} | {n6} | {n10} | Une idée. |";

        static CardRecord Convert(string baseCell, string n3 = "St Burn 1t", string n6 = "Steal 15", string n10 = "Exec 30", string phase = "P2", string cible = "1E")
        {
            var rows = CatalogImporter.ReadTable("# Titre\n\n" + Header + "\n" + Separator + "\n" + Line(baseCell, n3, n6, n10, phase, cible) + "\n");
            return CatalogImporter.ToRecord(rows.Single());
        }

        static CatalogImportException Rejects(string baseCell, string n3 = "St Burn 1t")
        {
            var ex = Assert.Throws<CatalogImportException>(() => Convert(baseCell, n3), baseCell);
            Assert.That(ex!.CardId, Is.EqualTo("card_test"));
            return ex;
        }

        /// <summary>Réaction à plat, lisible d'un coup d'oeil : déclencheur/statut/tours/effet imbriqué/cible/ciblage/statut de ciblage/déclencheur visé.</summary>
        static string Rx(EffectRecord e)
            => !e.hasReaction ? "-"
             : $"{e.reactionTrigger}/{e.reactionStatus}/{e.reactionTurns}/{e.reactionEffectType}:{e.reactionEffectValue}:{e.reactionEffectStatus}:{e.reactionEffectDuration}:{e.reactionEffectChance}"
               + $"/{e.reactionTarget}/{e.reactionPick}/{e.reactionPickStatus}/{(e.reactionOnTriggerUnit ? "T" : "-")}";

        static string Fx(EffectRecord e) => $"{e.type}:{e.value}:{e.status}:{e.duration}:{e.chance}:{(e.hasCondition ? "C" : "-")}:{e.mods.Count}:{Rx(e)}";

        // --- Réactions : un cas par événement ---

        [TestCase("un ennemi reçoit Burn → Dmg 45", "EnemyReceivesStatus/Burn/2/Damage:45:None:0:100/Self/Default/None/T")]
        [TestCase("un allié reçoit Haste → Adv 20", "AllyReceivesStatus/Haste/2/Advance:20:None:0:100/Self/Default/None/T")]
        [TestCase("un ennemi est vaincu → Dmg 50", "EnemyDefeated/None/2/Damage:50:None:0:100/Self/Default/None/-")]
        [TestCase("un allié est vaincu → Heal 12", "AllyDefeated/None/2/Heal:12:None:0:100/Self/Default/None/-")]
        [TestCase("le porteur est touché → Shield 15", "CarrierHit/None/2/Shield:15:Shield:2:100/Self/Default/None/-")]
        [TestCase("le porteur inflige un critique → St Burn 2t", "CarrierCrit/None/2/ApplyStatus:0:Burn:2:100/Self/Default/None/-")]
        [Description("Vérifie que chacun des six événements donne le bon déclencheur et, sans cible nommée, vise l'unité déclencheuse pour les deux « reçoit un statut » et le porteur pour les quatre autres — parce que c'est la règle d'import décidée pour une réaction sans « sur <cible> »")]
        public void EachEvent_ImportsWithItsTriggerAndDefaultTarget(string reaction, string expected)
        {
            var e = Convert($"Dmg 105, [Réaction 2t : quand {reaction}]").skill.effects[1];
            Assert.That(e.type, Is.EqualTo(EffectType.ArmReaction));
            Assert.That(e.chance, Is.EqualTo(100));
            Assert.That(Rx(e), Is.EqualTo(expected));
        }

        // --- Réactions : cible nommée ---

        [TestCase("Soi", "Self/Default/None/-")]
        [TestCase("ZE", "AllEnemies/Default/None/-")]
        [TestCase("ZA", "AllAllies/Default/None/-")]
        [TestCase("1E:aléatoire", "SingleEnemy/Random/None/-")]
        [TestCase("1E:faible", "SingleEnemy/LowestHp/None/-")]
        [TestCase("1A:prochain", "SingleAlly/NextToAct/None/-")]
        [TestCase("1A:faible", "SingleAlly/LowestHp/None/-")]
        [TestCase("déclencheur", "Self/Default/None/T")]
        [Description("Vérifie que chaque cible nommée de « sur <cible> » donne la règle, le ciblage fin ou le drapeau « déclencheur » prévus — parce qu'une réaction vise comme une compétence lancée par le porteur")]
        public void NamedTarget_MapsToTargetRulePickOrTheTriggerUnit(string target, string expected)
        {
            var e = Convert($"Dmg 105, [Réaction 3t : quand un ennemi est vaincu → Dmg 50 sur {target}]").skill.effects[1];
            Assert.That(Rx(e), Is.EqualTo($"EnemyDefeated/None/3/Damage:50:None:0:100/{expected}"));
        }

        [Test, Description("Vérifie qu'une cible nommée l'emporte sur la cible par défaut d'un événement « reçoit un statut » — parce que « sur ZE » ne doit pas rester sur l'unité déclencheuse")]
        public void NamedTarget_OverridesTheTriggerUnitDefault()
        {
            var e = Convert("Dmg 100, St Shock 1t, [Réaction 3t : quand un ennemi reçoit Shock → St Shock 1t sur ZE]").skill.effects[2];
            Assert.That(Rx(e), Is.EqualTo("EnemyReceivesStatus/Shock/3/ApplyStatus:0:Shock:1:100/AllEnemies/Default/None/-"));
        }

        // --- Réactions : effet imbriqué ---

        [TestCase("Dmg 70", "Damage:70:None:0:100")]
        [TestCase("Heal 15", "Heal:15:None:0:100")]
        [TestCase("Shield 20", "Shield:20:Shield:2:100")]
        [TestCase("St Mark 2t", "ApplyStatus:0:Mark:2:100")]
        [TestCase("St Freeze 1t (30 %)", "ApplyStatus:0:Freeze:1:30")]
        [TestCase("Push 15", "PushBack:15:None:0:100")]
        [TestCase("Adv 20", "Advance:20:None:0:100")]
        [TestCase("Gauge -100", "GaugeDelta:-100:None:0:100")]
        [TestCase("Gauge +50", "GaugeDelta:50:None:0:100")]
        [TestCase("Purge 1", "Purge:1:None:0:100")]
        [Description("Vérifie que chacun des huit types d'effet admis dans une réaction se range dans les champs plats de l'effet imbriqué, avec sa valeur fixe — parce que JsonUtility ne sait pas écrire un enregistrement dans lui-même")]
        public void NestedEffect_FillsTheFlatFields(string effect, string expected)
        {
            var e = Convert($"Dmg 105, [Réaction 2t : quand un ennemi est vaincu → {effect}]").skill.effects[1];
            Assert.That(Rx(e), Does.Contain("/" + expected + "/"));
            Assert.That(e.mods, Is.Empty);
            Assert.That(e.hasCondition, Is.False);
        }

        // --- Réactions : place dans la liste ---

        [Test, Description("Vérifie que la réaction prend la place de son crochet dans la liste d'effets de la base — parce que l'ordre des effets d'une cellule est celui de la fiche")]
        public void Reaction_KeepsThePositionOfItsBracket_InTheBase()
        {
            var c = Convert("Dmg 105, [Réaction 2t : quand un ennemi est vaincu → Dmg 70], St Burn 1t");
            Assert.That(c.skill.effects.Select(e => e.type), Is.EqualTo(new[] { EffectType.Damage, EffectType.ArmReaction, EffectType.ApplyStatus }));
            Assert.That(Fx(c.skill.effects[0]), Is.EqualTo("Damage:105:None:0:100:-:0:-"));
        }

        [Test, Description("Vérifie qu'un crochet de réaction collé à son effet (sans virgule) vient juste après lui — parce que les crochets d'un élément s'ajoutent derrière l'effet")]
        public void Reaction_AttachedToAnEffect_FollowsIt()
        {
            var c = Convert("Dmg 105 [Réaction 2t : quand un ennemi est vaincu → Dmg 70], St Burn 1t");
            Assert.That(c.skill.effects.Select(e => e.type), Is.EqualTo(new[] { EffectType.Damage, EffectType.ArmReaction, EffectType.ApplyStatus }));
        }

        [Test, Description("Vérifie que « ; » sépare une réaction comme « , » — parce que les fiches écrivent aussi « Shield 15 ; [Réaction … ] »")]
        public void Reaction_AfterASemicolon_IsAnEffect()
        {
            var c = Convert("Shield 15 ; [Réaction 3t : quand le porteur est touché → Heal 15]", cible: "Soi");
            Assert.That(c.skill.effects.Select(e => e.type), Is.EqualTo(new[] { EffectType.Shield, EffectType.ArmReaction }));
        }

        [Test, Description("Vérifie qu'une réaction seule dans une cellule de palier est un effet de palier — parce que « card_rebirth » arme sa réaction au niveau 6")]
        public void Reaction_InATierCell_IsATierEffect()
        {
            var c = Convert("Heal 30", n6: "[Réaction 4t : quand un allié est vaincu → Heal 40 sur 1A:faible]");
            var tier = c.tiers.Single(t => t.level == 6);
            Assert.That(tier.effects.Count, Is.EqualTo(1));
            Assert.That(Rx(tier.effects[0]), Is.EqualTo("AllyDefeated/None/4/Heal:40:None:0:100/SingleAlly/LowestHp/None/-"));
            Assert.That(tier.effects[0].type, Is.EqualTo(EffectType.ArmReaction));
        }

        [Test, Description("Vérifie qu'une réaction seule dans la base (sans autre effet) est acceptée — parce que « card_launch_ramp » n'a pas d'autre effet")]
        public void Reaction_AloneInTheBase_IsAccepted()
        {
            var c = Convert("[Réaction 2t : quand un ennemi est vaincu → Adv 15 sur ZA]", cible: "Soi");
            Assert.That(c.skill.effects.Count, Is.EqualTo(1));
            Assert.That(c.skill.effects[0].type, Is.EqualTo(EffectType.ArmReaction));
        }

        [Test, Description("Vérifie qu'une réaction n'enlève pas son bonus à un Dmg qui la précède avec un autre crochet — parce qu'un crochet de réaction n'est ni un bonus ni une condition")]
        public void Reaction_DoesNotDisturbOtherBrackets()
        {
            var c = Convert("Dmg 110 [si cible a Burn : +40], [Réaction 2t : quand le porteur inflige un critique → Dmg 70 sur 1E:aléatoire]");
            Assert.That(c.skill.effects.Select(e => e.type), Is.EqualTo(new[] { EffectType.Damage, EffectType.ArmReaction }));
            Assert.That(c.skill.effects[0].mods.Single().bonus, Is.EqualTo(40));
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        [Description("Vérifie que les durées de 1 à 5 tours sont acceptées — parce que la grammaire borne N de 1 à 5")]
        public void ReactionTurns_AreInRange(int turns)
        {
            var e = Convert($"Dmg 105, [Réaction {turns}t : quand un ennemi est vaincu → Dmg 70]").skill.effects[1];
            Assert.That(e.reactionTurns, Is.EqualTo(turns));
        }

        // --- Consomme ---

        [Test, Description("Vérifie que « Consomme Burn : Dmg 30 par tour restant (max 4) » donne un ConsumeDamage de statut Burn, valeur 30 et durée (maximum de tours) 4 — parce que Duration porte le « max » du plan")]
        public void ConsumeDamage_ImportsStatusValueAndMax()
        {
            var c = Convert("Dmg 90, Consomme Burn : Dmg 30 par tour restant (max 4)");
            Assert.That(Fx(c.skill.effects[1]), Is.EqualTo("ConsumeDamage:30:Burn:4:100:-:0:-"));
        }

        [Test, Description("Vérifie que « Consomme Regen : Heal 18 par tour restant (max 4) » donne un ConsumeHeal — parce que Heal sépare le soin de l'attaque")]
        public void ConsumeHeal_ImportsStatusValueAndMax()
        {
            var c = Convert("Consomme Regen : Heal 18 par tour restant (max 4)", cible: "1A:Regen");
            Assert.That(Fx(c.skill.effects.Single()), Is.EqualTo("ConsumeHeal:18:Regen:4:100:-:0:-"));
        }

        [Test, Description("Vérifie que « Consomme Mark : Dmg 70 par tour restant (max 1) » est accepté avec un autre Dmg dans la même cellule — parce que « card_mark_harvest » écrit ainsi")]
        public void ConsumeMark_WithAMaxOfOne_IsAccepted()
        {
            var c = Convert("Consomme Mark : Dmg 70 par tour restant (max 1), Dmg 100", cible: "1E:Mark");
            Assert.That(c.skill.effects.Select(Fx), Is.EqualTo(new[] { "ConsumeDamage:70:Mark:1:100:-:0:-", "Damage:100:None:0:100:-:0:-" }));
        }

        [TestCase(1)]
        [TestCase(10)]
        [Description("Vérifie que les bornes 1 et 10 de « max » d'un Consomme sont acceptées — parce que M va de 1 à 10")]
        public void ConsumeMax_BoundsAreAccepted(int max)
        {
            Assert.That(Convert($"Consomme Burn : Dmg 30 par tour restant (max {max})").skill.effects.Single().duration, Is.EqualTo(max));
        }

        // --- Propage ---

        [Test, Description("Vérifie que « Propage Burn » donne un effet Spread de statut Burn, sans valeur — parce que seul le statut compte")]
        public void Spread_ImportsTheStatus()
        {
            var c = Convert("Dmg 130, Propage Burn", cible: "1E:Burn");
            Assert.That(Fx(c.skill.effects[1]), Is.EqualTo("Spread:0:Burn:0:100:-:0:-"));
        }

        [Test, Description("Vérifie que « Propage Mark, Dmg 100 » garde l'ordre de la fiche — parce que Propage peut précéder les dégâts")]
        public void Spread_BeforeDamage_KeepsTheOrder()
        {
            var c = Convert("Propage Mark, Dmg 100", cible: "1E:Mark");
            Assert.That(c.skill.effects.Select(e => e.type), Is.EqualTo(new[] { EffectType.Spread, EffectType.Damage }));
        }

        // --- Échelle d'archétype ---

        [TestCase("Brûlure", Archetype.Burn)]
        [TestCase("Entrave", Archetype.Restrain)]
        [TestCase("Traque", Archetype.Hunt)]
        [TestCase("Tempo", Archetype.Tempo)]
        [TestCase("Rempart", Archetype.Bulwark)]
        [TestCase("Renouveau", Archetype.Renewal)]
        [Description("Vérifie que chacun des six archétypes donne un modificateur à échelle EquippedArchetypeCards avec son archétype, le bonus et le plafond de cartes — parce que les 14 cartes P3 n'utilisent que cette échelle")]
        public void ArchetypeScale_MapsEachArchetype(string name, Archetype expected)
        {
            var m = Convert($"Dmg 60 [+10 par carte {name} équipée dans l'escouade (max 4)]", phase: "P3").skill.effects.Single().mods.Single();
            Assert.That(m.hasScale, Is.True);
            Assert.That(m.per, Is.EqualTo(ScaleKind.EquippedArchetypeCards));
            Assert.That(m.hasArchetype, Is.True);
            Assert.That(m.perArchetype, Is.EqualTo(expected));
            Assert.That(m.bonus, Is.EqualTo(10));
            Assert.That(m.maxCount, Is.EqualTo(4));
            Assert.That(m.hasCondition, Is.False);
        }

        [Test, Description("Vérifie que « Dmg 95, [+10 par carte Traque … (max 5)] » (crochet séparé par une virgule) renforce le Dmg qui précède — parce que l'échelle d'archétype se rattache comme les échelles P1")]
        public void ArchetypeScale_AfterAComma_BindsToThePrecedingEffect()
        {
            var c = Convert("Dmg 95, [+10 par carte Traque équipée dans l'escouade (max 5)]", phase: "P3");
            Assert.That(c.skill.effects.Count, Is.EqualTo(1));
            Assert.That(c.skill.effects[0].mods.Single().perArchetype, Is.EqualTo(Archetype.Hunt));
        }

        [Test, Description("Vérifie qu'une échelle d'archétype s'ajoute à côté d'une condition sur le même effet, dans l'ordre de la fiche — parce que « card_alpha_hunter » cumule échelle et condition")]
        public void ArchetypeScale_NextToACondition_KeepsBothMods()
        {
            var c = Convert("Dmg 130, [+10 par carte Traque équipée dans l'escouade (max 5)], [si cible a Mark : +25]", phase: "P3");
            var mods = c.skill.effects.Single().mods;
            Assert.That(mods.Count, Is.EqualTo(2));
            Assert.That(mods[0].hasScale, Is.True);
            Assert.That(mods[1].hasCondition, Is.True);
            Assert.That(mods[1].hasArchetype, Is.False);
        }

        [Test, Description("Vérifie qu'un bonus à échelle de P1 ne porte pas d'archétype — parce que « hasArchetype » est faux hors de EquippedArchetypeCards")]
        public void P1Scale_HasNoArchetype()
        {
            var m = Convert("Dmg 95 [+15 par statut de la cible (max 3)]").skill.effects.Single().mods.Single();
            Assert.That(m.hasArchetype, Is.False);
            Assert.That(m.per, Is.EqualTo(ScaleKind.TargetStatusCount));
        }

        [Test, Description("Vérifie que « Gauge +90 [+10 par carte Tempo … (max 4)] » et « Shield 16 [+3 par carte Rempart … (max 3)] » se rattachent à leur effet — parce que Gauge et Shield portent des modificateurs")]
        public void ArchetypeScale_OnGaugeAndShield()
        {
            var g = Convert("Gauge +90 [+10 par carte Tempo équipée dans l'escouade (max 4)]", cible: "ZA", phase: "P3").skill.effects.Single();
            Assert.That(g.type, Is.EqualTo(EffectType.GaugeDelta));
            Assert.That(g.mods.Single().perArchetype, Is.EqualTo(Archetype.Tempo));
            var s = Convert("Shield 16 [+3 par carte Rempart équipée dans l'escouade (max 3)]", cible: "ZA", phase: "P3").skill.effects.Single();
            Assert.That(s.mods.Single().maxCount, Is.EqualTo(3));
        }

        [Test, Description("Vérifie que l'échelle d'archétype se lit aussi dans une cellule de palier — parce que les crochets des paliers suivent la même grammaire que la base")]
        public void ArchetypeScale_InATierCell()
        {
            var c = Convert("Dmg 100", n3: "Dmg 20 [+5 par carte Brûlure équipée dans l'escouade (max 2)]");
            Assert.That(c.tiers[0].effects.Single().mods.Single().perArchetype, Is.EqualTo(Archetype.Burn));
        }

        // --- Rejets ---

        [TestCase("[Réaction 2t : quand un ennemi est vaincu →]")]
        [TestCase("[Réaction 2t : quand un ennemi est vaincu]")]
        [TestCase("[Réaction 2t : quand un ennemi est vaincu → sur ZE]")]
        [Description("Vérifie qu'une réaction sans effet est rejetée — parce qu'une réaction sans effet n'a aucun sens et ne doit pas être importée à moitié")]
        public void Reaction_WithoutAnEffect_IsRejected(string cell) => Rejects("Dmg 100, " + cell);

        [TestCase("[Réaction 2t : quand un ennemi est vaincu → Dmg 10, Heal 5]")]
        [TestCase("[Réaction 2t : quand un ennemi est vaincu → Dmg 10 ; Heal 5]")]
        [TestCase("[Réaction 2t : quand un ennemi est vaincu → Dmg 10 Heal 5]")]
        [Description("Vérifie qu'une réaction à deux effets est rejetée — parce que la grammaire n'autorise qu'un seul effet par réaction")]
        public void Reaction_WithTwoEffects_IsRejected(string cell) => Rejects("Dmg 100, " + cell);

        [TestCase("[Réaction 2t : quand un ennemi saute → Dmg 10]")]
        [TestCase("[Réaction 2t : quand le porteur est soigné → Dmg 10]")]
        [TestCase("[Réaction 2t : quand un ennemi reçoit → Dmg 10]")]
        [TestCase("[Réaction 2t : quand un ennemi reçoit Poison → Dmg 10]")]
        [TestCase("[Réaction 2t : quand un ennemi reçoit None → Dmg 10]")]
        [TestCase("[Réaction 2t : quand un allié reçoit → Dmg 10]")]
        [TestCase("[Réaction 2t : quand un ennemi est vaincu avec Burn → Dmg 10]")]
        [TestCase("[Réaction 2t : lorsque un ennemi est vaincu → Dmg 10]")]
        [TestCase("[Réaction 2t : un ennemi est vaincu → Dmg 10]")]
        [TestCase("[Réaction 2t : quand le porteur est touché par un ennemi → Dmg 10]")]
        [Description("Vérifie qu'un événement inconnu, un statut inconnu ou manquant, ou une forme voisine des six événements est rejeté — parce que seules les six formes du plan existent")]
        public void Reaction_UnknownEvent_IsRejected(string cell) => Rejects("Dmg 100, " + cell);

        [TestCase("[Réaction 0t : quand un ennemi est vaincu → Dmg 10]")]
        [TestCase("[Réaction 6t : quand un ennemi est vaincu → Dmg 10]")]
        [TestCase("[Réaction t : quand un ennemi est vaincu → Dmg 10]")]
        [TestCase("[Réaction 2 : quand un ennemi est vaincu → Dmg 10]")]
        [TestCase("[Réaction : quand un ennemi est vaincu → Dmg 10]")]
        [TestCase("[Réaction 2t quand un ennemi est vaincu → Dmg 10]")]
        [Description("Vérifie qu'une durée hors de 1 à 5 tours ou une forme sans « Nt » est rejetée — parce que N est borné de 1 à 5")]
        public void Reaction_BadTurns_IsRejected(string cell) => Rejects("Dmg 100, " + cell);

        [TestCase("sur 3E")]
        [TestCase("sur 1E")]
        [TestCase("sur 1E:fort")]
        [TestCase("sur 1A")]
        [TestCase("sur ZE:faible")]
        [TestCase("sur Soi:faible")]
        [TestCase("sur 2E:aléatoires")]
        [TestCase("sur 1E:Burn")]
        [TestCase("sur")]
        [TestCase("sur ZE ZA")]
        [TestCase("sur Déclencheur")]
        [Description("Vérifie qu'une cible nommée hors de la liste fermée (huit cibles) est rejetée — parce que la réaction ne comprend que Soi, ZE, ZA, 1E:aléatoire, 1E:faible, 1A:prochain, 1A:faible et déclencheur")]
        public void Reaction_UnknownTarget_IsRejected(string target) => Rejects("Dmg 100, [Réaction 2t : quand un ennemi est vaincu → Dmg 10 " + target + "]");

        [TestCase("Steal 10")]
        [TestCase("Bounce 40")]
        [TestCase("Exec 30")]
        [TestCase("Consomme Burn : Dmg 10 par tour restant (max 2)")]
        [TestCase("Propage Burn")]
        [TestCase("Boum 5")]
        [Description("Vérifie que Steal, Bounce, Exec, Consomme, Propage et un effet inconnu sont rejetés dans une réaction — parce que l'effet d'une réaction se limite à Dmg, Heal, Shield, St, Push, Adv, Gauge et Purge")]
        public void Reaction_ForbiddenNestedEffect_IsRejected(string effect) => Rejects("Dmg 100, [Réaction 2t : quand un ennemi est vaincu → " + effect + "]");

        [TestCase("Dmg 10 [si cible a Burn : +5]")]
        [TestCase("[si cible a Burn : Dmg 10]")]
        [TestCase("Dmg 10 [+5 par ennemi vivant (max 2)]")]
        [TestCase("Dmg 10 [Réaction 2t : quand un ennemi est vaincu → Dmg 5]")]
        [TestCase("Dmg 0")]
        [TestCase("Dmg -5")]
        [TestCase("St Poison 2t")]
        [Description("Vérifie qu'un effet de réaction avec crochet, condition, modificateur, réaction imbriquée, valeur nulle ou statut inconnu est rejeté — parce que l'effet d'une réaction est de valeur fixe, sans condition")]
        public void Reaction_NestedEffectWithExtras_IsRejected(string effect) => Rejects("Dmg 100, [Réaction 2t : quand un ennemi est vaincu → " + effect + "]");

        [Test, Description("Vérifie que deux crochets de réaction dans la même cellule sont tous deux gardés, dans l'ordre — parce qu'aucun crochet ne doit se perdre")]
        public void TwoReactions_AreBothKept()
        {
            var c = Convert("Dmg 100, [Réaction 2t : quand un ennemi est vaincu → Dmg 10], [Réaction 3t : quand un allié est vaincu → Heal 20 sur ZA]");
            Assert.That(c.skill.effects.Select(e => e.reactionTrigger), Is.EqualTo(new[] { ReactionTrigger.EnemyReceivesStatus, ReactionTrigger.EnemyDefeated, ReactionTrigger.AllyDefeated }));
            Assert.That(c.skill.effects.Select(e => e.type), Is.EqualTo(new[] { EffectType.Damage, EffectType.ArmReaction, EffectType.ArmReaction }));
        }

        [TestCase("Consomme Burn : Dmg 30 par tour restant (max 11)")]
        [TestCase("Consomme Burn : Dmg 30 par tour restant (max 0)")]
        [TestCase("Consomme Burn : Dmg 30 par tour restant (max 40)")]
        [TestCase("Consomme Burn : Dmg 30 par tour restant")]
        [TestCase("Consomme Burn : Dmg 30 (max 4)")]
        [TestCase("Consomme Burn : Dmg 30 par tour (max 4)")]
        [TestCase("Consomme Burn : Dmg 30 par tours restants (max 4)")]
        [TestCase("Consomme Burn Dmg 30 par tour restant (max 4)")]
        [TestCase("Consomme Burn : Dmg 0 par tour restant (max 4)")]
        [TestCase("Consomme Burn : Steal 30 par tour restant (max 4)")]
        [TestCase("Consomme Burn : Shield 30 par tour restant (max 4)")]
        [TestCase("Consomme Burn : Dmg par tour restant (max 4)")]
        [TestCase("Consomme Poison : Dmg 30 par tour restant (max 4)")]
        [TestCase("Consomme None : Dmg 30 par tour restant (max 4)")]
        [TestCase("Consomme : Dmg 30 par tour restant (max 4)")]
        [TestCase("Consomme Burn : Dmg 30 par tour restant (max 4) foo")]
        [TestCase("Consomme Burn : Dmg 30 par tour restant (4)")]
        [Description("Vérifie qu'un Consomme avec max hors de 1 à 10, sans « par tour restant », sans max, avec un autre verbe que Dmg/Heal, un statut inconnu ou une valeur nulle est rejeté — parce que la grammaire de Consomme est fermée")]
        public void Consume_BadForms_AreRejected(string cell) => Rejects(cell);

        [TestCase("Propage")]
        [TestCase("Propage Poison")]
        [TestCase("Propage None")]
        [TestCase("Propage Burn 2t")]
        [TestCase("Propage Burn Mark")]
        [Description("Vérifie qu'un Propage sans statut, avec un statut inconnu ou avec un reste de texte est rejeté — parce que Propage prend exactement un statut")]
        public void Spread_BadForms_AreRejected(string cell) => Rejects("Dmg 100, " + cell);

        [TestCase("Dmg 60 [+10 par carte Magie équipée dans l'escouade (max 4)]")]
        [TestCase("Dmg 60 [+10 par carte équipée dans l'escouade (max 4)]")]
        [TestCase("Dmg 60 [+10 par carte Brûlure équipée dans l'escouade (max 11)]")]
        [TestCase("Dmg 60 [+10 par carte Brûlure équipée dans l'escouade (max 40)]")]
        [TestCase("Dmg 60 [+10 par carte Brûlure équipée dans l'escouade (max 0)]")]
        [TestCase("Dmg 60 [+10 par carte Brûlure équipée dans l'escouade]")]
        [TestCase("Dmg 60 [+10 par carte Brûlure équipée (max 4)]")]
        [TestCase("Dmg 60 [+10 par carte Brûlure dans l'escouade (max 4)]")]
        [TestCase("Dmg 60 [+10 par carte Brûlure équipée dans l'escouade (max 4) foo]")]
        [TestCase("Dmg 60 [+0 par carte Brûlure équipée dans l'escouade (max 4)]")]
        [TestCase("Dmg 60 [+10 par cartes Brûlure équipée dans l'escouade (max 4)]")]
        [TestCase("Dmg 60 [+10 par carte Brûlure + Tempo équipée dans l'escouade (max 4)]")]
        [Description("Vérifie qu'un archétype inconnu ou absent, un max hors de 1 à 10, un morceau de phrase manquant ou un bonus nul est rejeté — parce que la forme « +N par carte <Archétype> équipée dans l'escouade (max M) » est fermée")]
        public void ArchetypeScale_BadForms_AreRejected(string cell) => Rejects(cell);

        [Test, Description("Vérifie qu'un max de 40 avec un bonus de +10 est rejeté avec un message qui parle de « max > 10 » — parce que max compte des cartes, pas un plafond de total")]
        public void ArchetypeScale_MaxAboveTen_ExplainsTheRule()
        {
            var ex = Rejects("Dmg 60 [+10 par carte Brûlure équipée dans l'escouade (max 40)]");
            Assert.That(ex.Reason, Does.Contain("max > 10"));
        }

        [Test, Description("Vérifie qu'une échelle d'archétype après un effet qui ne porte pas de bonus (St) est rejetée comme les échelles de P1 — parce que le rattachement est le même")]
        public void ArchetypeScale_AfterAStatus_IsRejected()
            => Rejects("St Burn 1t, [+10 par carte Brûlure équipée dans l'escouade (max 4)]");

        [Test, Description("Vérifie qu'une échelle d'archétype placée après une réaction se rattache au dernier effet porteur de la cellule (le Dmg), pas à l'effet imbriqué de la réaction — parce que la réaction est un effet supplémentaire et non un hôte")]
        public void ArchetypeScale_AfterAReaction_BindsToTheCarrierEffect()
        {
            var c = Convert("Dmg 100, [Réaction 2t : quand un ennemi est vaincu → Dmg 10], [+10 par carte Brûlure équipée dans l'escouade (max 4)]");
            Assert.That(c.skill.effects[0].mods.Single().perArchetype, Is.EqualTo(Archetype.Burn));
            Assert.That(c.skill.effects[1].mods, Is.Empty);
        }

        [Test, Description("Vérifie qu'un Consomme ou un Propage ne peut pas porter de condition ni de bonus — parce que la grammaire ne prévoit ni l'un ni l'autre")]
        public void ConsumeAndSpread_CannotCarryConditionsOrBonuses()
        {
            Rejects("Consomme Burn : Dmg 30 par tour restant (max 4) [+10 par carte Brûlure équipée dans l'escouade (max 4)]");
            Rejects("Consomme Burn : Dmg 30 par tour restant (max 4) [si cible a Burn : +10]");
            Rejects("Dmg 100, [si cible a Burn : Propage Burn]");
            Rejects("Dmg 100, [si cible a Burn : Consomme Burn : Dmg 30 par tour restant (max 4)]");
            Rejects("Dmg 100, [si cible a Burn : Réaction 2t : quand un ennemi est vaincu → Dmg 10]");
        }

        // --- Fiches réelles ---

        static string CatalogueFolder => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "docs", "catalogue"));

        static List<CatalogRow> AllRows()
            => Directory.GetFiles(CatalogueFolder, "cartes-*.md").OrderBy(f => f, System.StringComparer.Ordinal)
                .SelectMany(f => CatalogImporter.ReadTable(File.ReadAllText(f))).ToList();

        static bool HasBrick(CardRecord c)
        {
            bool Brick(EffectRecord e) => e.type == EffectType.ArmReaction || e.type == EffectType.ConsumeDamage || e.type == EffectType.ConsumeHeal
                                          || e.type == EffectType.Spread || e.mods.Any(m => m.hasArchetype);
            return c.skill.effects.Any(Brick) || c.tiers.Any(t => t.effects.Any(Brick));
        }

        [Test, Description("Vérifie que les 54 fiches de phase P2 ou P3 s'importent sans rejet, qu'aucune n'est vide de brique P2/P3 et qu'aucune fiche P1 n'en contient — parce que la phase de la fiche doit dire exactement quelles briques elle utilise")]
        public void EveryP2P3Row_Imports()
        {
            var rejected = new List<string>();
            int imported = 0;
            foreach (var row in AllRows().Where(r => r.Phase != "P1"))
            {
                try
                {
                    var rec = CatalogImporter.ToRecord(row);
                    Assert.That(HasBrick(rec), Is.True, row.Id + " : aucune brique P2/P3 reconnue");
                    imported++;
                }
                catch (CatalogImportException e) { rejected.Add(e.CardId + " : " + e.Message); }
            }
            Assert.That(rejected, Is.Empty, "fiches P2/P3 hors grammaire :\n" + string.Join("\n", rejected));
            Assert.That(imported, Is.EqualTo(54));
            foreach (var row in AllRows().Where(r => r.Phase == "P1"))
                Assert.That(HasBrick(CatalogImporter.ToRecord(row)), Is.False, row.Id + " : une fiche P1 utilise une brique P2/P3");
        }

        [Test, Description("Vérifie que les 250 fiches du catalogue s'importent sans rejet (196 de phase P1, 54 de P2 ou P3), que le catalogue obtenu passe Validate et qu'il survit au JSON sans perte — parce que l'importeur et le chargeur doivent s'accorder sur toutes les cartes")]
        public void EveryRow_Imports()
        {
            var rows = AllRows();
            var rejected = new List<string>();
            var records = new List<CardRecord>();
            foreach (var row in rows)
            {
                try { records.Add(CatalogImporter.ToRecord(row)); }
                catch (CatalogImportException e) { rejected.Add(e.CardId + " : " + e.Message); }
            }
            Assert.That(rejected, Is.Empty, "fiches hors grammaire :\n" + string.Join("\n", rejected));
            Assert.That(records.Count, Is.EqualTo(250));
            Assert.That(rows.Count(r => r.Phase == "P1"), Is.EqualTo(196));
            Assert.That(rows.Count(r => r.Phase != "P1"), Is.EqualTo(54));
            Assert.That(records.Select(r => r.id).Distinct().Count(), Is.EqualTo(250), "identifiants en double");

            var defs = records.Select(ContentDataConverter.ToDef).ToList();
            Assert.That(CardDataLibrary.Validate(defs), Is.Empty);
            var json = CardDataLibrary.ToJson(defs);
            Assert.That(CardDataLibrary.ToJson(CardDataLibrary.FromJson(json)), Is.EqualTo(json), "le JSON perd ou change un champ");
        }

        [Test, Description("Vérifie que les cartes P2/P3 couvrent chacun des six déclencheurs au moins une fois — parce qu'un écart signalerait une cellule lue autrement que prévu")]
        public void P2P3Cards_CoverEveryTrigger()
        {
            var reactions = new List<EffectRecord>();
            foreach (var row in AllRows().Where(r => r.Phase != "P1"))
            {
                var rec = CatalogImporter.ToRecord(row);
                reactions.AddRange(rec.skill.effects.Concat(rec.tiers.SelectMany(t => t.effects)).Where(e => e.type == EffectType.ArmReaction));
            }
            foreach (ReactionTrigger t in System.Enum.GetValues(typeof(ReactionTrigger)))
                Assert.That(reactions.Any(r => r.reactionTrigger == t), Is.True, t.ToString());
            TestContext.Out.WriteLine("réactions armées dans P2/P3 : " + reactions.Count);
        }
    }
}
