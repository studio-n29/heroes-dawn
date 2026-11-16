#nullable enable
using System;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Textes générés des réactions armées, de Consomme, de Propage et de l'échelle d'archétype (plan P2-P4, tâche 4).</summary>
    public sealed class SkillTextReactionTests
    {
        static string[] Lines(params EffectDef[] fx) => SkillText.Effects(fx).ToArray();

        static EffectDef Arm(ReactionTrigger trigger, EffectDef effect, StatusType status = StatusType.None, int turns = 3,
                             TargetRule target = TargetRule.Self, TargetPick pick = TargetPick.Default, StatusType pickStatus = StatusType.None, bool onTrigger = false)
            => new EffectDef(EffectType.ArmReaction, 0, reaction: new ReactionDef(trigger, status, turns, effect, target, pick, pickStatus, onTrigger));

        static EffectDef Dmg(int v) => new EffectDef(EffectType.Damage, v);

        // ------------------------------------------------------------------ Réactions : événements

        const string Riposte = "Riposte du lanceur ";

        [Test, Description("Vérifie la phrase exacte d'une réaction sur « un ennemi reçoit un statut » avec cible nommée (un ennemi au hasard) — parce que le joueur doit lire qui riposte, la durée en tours du lanceur, l'événement, l'effet et la cible")]
        public void Reaction_EnemyReceivesStatus_NamedRandomEnemy()
            => Assert.That(Lines(Arm(ReactionTrigger.EnemyReceivesStatus, Dmg(70), StatusType.Shock, 3, TargetRule.SingleEnemy, TargetPick.Random)),
                           Is.EqualTo(new[] { "Riposte du lanceur (3 de ses tours) : quand un ennemi reçoit Choc → Dégâts 70 % à un ennemi au hasard" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « un allié reçoit un statut » visant l'unité déclencheuse : « (lanceur compris) » et « à cet allié » — parce que l'allié qui reçoit peut être le lanceur et que la cible s'écrit toujours")]
        public void Reaction_AllyReceivesStatus_OnTheReceiver()
            => Assert.That(Lines(Arm(ReactionTrigger.AllyReceivesStatus, new EffectDef(EffectType.Heal, 10), StatusType.Burn, 2, onTrigger: true)),
                           Is.EqualTo(new[] { "Riposte du lanceur (2 de ses tours) : quand un allié (lanceur compris) reçoit Brûlure → Soin 10 % PV à cet allié" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « un ennemi est vaincu » sur le lanceur, durée d'un seul tour — parce que la cible « au lanceur » s'écrit aussi quand c'est le défaut")]
        public void Reaction_EnemyDefeated_OnTheCaster_OneTurn()
            => Assert.That(Lines(Arm(ReactionTrigger.EnemyDefeated, new EffectDef(EffectType.Shield, 20), StatusType.None, 1)),
                           Is.EqualTo(new[] { "Riposte du lanceur (1 de ses tours) : quand un ennemi est vaincu → Bouclier 20 % PV au lanceur" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « un autre allié est vaincu » qui fait avancer toute l'équipe (élan sans « la cible ») — parce que le moteur exclut le lanceur de ce déclencheur et que l'élan d'une réaction se lit « Avance … sur la frise »")]
        public void Reaction_AllyDefeated_WholeTeam()
            => Assert.That(Lines(Arm(ReactionTrigger.AllyDefeated, new EffectDef(EffectType.Advance, 15), StatusType.None, 4, TargetRule.AllAllies)),
                           Is.EqualTo(new[] { "Riposte du lanceur (4 de ses tours) : quand un autre allié est vaincu → Avance 15 % sur la frise à toute l'équipe" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « le lanceur est frappé » visant l'attaquant — parce que la cible déclencheur de cet événement est celui qui a frappé")]
        public void Reaction_CarrierHit_OnTheAttacker()
            => Assert.That(Lines(Arm(ReactionTrigger.CarrierHit, new EffectDef(EffectType.ApplyStatus, 0, StatusType.Burn, 2), StatusType.None, 5, onTrigger: true)),
                           Is.EqualTo(new[] { "Riposte du lanceur (5 de ses tours) : quand le lanceur est frappé → Brûlure 2 t à l'attaquant" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « le lanceur réussit un coup critique » visant tous les ennemis — parce que la zone ennemie s'écrit « à tous les ennemis »")]
        public void Reaction_CarrierCrit_AllEnemies()
            => Assert.That(Lines(Arm(ReactionTrigger.CarrierCrit, Dmg(50), StatusType.None, 3, TargetRule.AllEnemies)),
                           Is.EqualTo(new[] { "Riposte du lanceur (3 de ses tours) : quand le lanceur réussit un coup critique → Dégâts 50 % à tous les ennemis" }));

        [TestCase(ReactionTrigger.EnemyReceivesStatus, "quand un ennemi reçoit Brûlure → Dégâts 40 % à cet ennemi")]
        [TestCase(ReactionTrigger.AllyReceivesStatus, "quand un allié (lanceur compris) reçoit Brûlure → Dégâts 40 % à cet allié")]
        [TestCase(ReactionTrigger.EnemyDefeated, "quand un ennemi est vaincu → Dégâts 40 % à l'ennemi vaincu")]
        [TestCase(ReactionTrigger.AllyDefeated, "quand un autre allié est vaincu → Dégâts 40 % à l'allié vaincu")]
        [TestCase(ReactionTrigger.CarrierHit, "quand le lanceur est frappé → Dégâts 40 % à l'attaquant")]
        [TestCase(ReactionTrigger.CarrierCrit, "quand le lanceur réussit un coup critique → Dégâts 40 % à l'ennemi touché")]
        [Description("Vérifie la cible « déclencheur » écrite selon l'événement (cet ennemi, cet allié, l'attaquant, l'ennemi touché…) — parce que l'unité concernée change avec l'événement")]
        public void Reaction_TriggerUnit_DependsOnTheEvent(ReactionTrigger trigger, string expected)
        {
            var status = trigger == ReactionTrigger.EnemyReceivesStatus || trigger == ReactionTrigger.AllyReceivesStatus ? StatusType.Burn : StatusType.None;
            Assert.That(Lines(Arm(trigger, Dmg(40), status, 2, onTrigger: true)), Is.EqualTo(new[] { Riposte + "(2 de ses tours) : " + expected }));
        }

        // ------------------------------------------------------------------ Réactions : cibles nommées

        [Test, Description("Vérifie la cible nommée « au lanceur » sur un événement de statut — parce que la forme « Soi » de la fiche doit se lire")]
        public void Reaction_NamedSelf_OnAStatusEvent()
            => Assert.That(Lines(Arm(ReactionTrigger.EnemyReceivesStatus, Dmg(30), StatusType.Freeze, 2, TargetRule.Self)),
                           Is.EqualTo(new[] { "Riposte du lanceur (2 de ses tours) : quand un ennemi reçoit Gel → Dégâts 30 % au lanceur" }));

        [Test, Description("Vérifie que la cible « au lanceur » s'écrit aussi hors événements de statut — parce que la cible d'une réaction s'écrit toujours")]
        public void Reaction_SelfOnAnOtherEvent_IsWritten()
            => Assert.That(Lines(Arm(ReactionTrigger.CarrierHit, new EffectDef(EffectType.Heal, 5), StatusType.None, 2, TargetRule.Self)),
                           Is.EqualTo(new[] { "Riposte du lanceur (2 de ses tours) : quand le lanceur est frappé → Soin 5 % PV au lanceur" }));

        [TestCase(TargetRule.SingleEnemy, TargetPick.Default, "à un ennemi"), TestCase(TargetRule.SingleEnemy, TargetPick.Random, "à un ennemi au hasard"),
         TestCase(TargetRule.SingleEnemy, TargetPick.TwoRandom, "à deux ennemis au hasard"), TestCase(TargetRule.SingleEnemy, TargetPick.LowestHp, "à l'ennemi le plus faible"),
         TestCase(TargetRule.SingleEnemy, TargetPick.HighestHp, "à l'ennemi le plus en forme"), TestCase(TargetRule.SingleEnemy, TargetPick.NextToAct, "au prochain ennemi à agir"),
         TestCase(TargetRule.SingleEnemy, TargetPick.StatusHolder, "à un ennemi porteur de Brûlure"),
         TestCase(TargetRule.SingleAlly, TargetPick.Default, "à un allié"), TestCase(TargetRule.SingleAlly, TargetPick.Random, "à un allié au hasard"),
         TestCase(TargetRule.SingleAlly, TargetPick.TwoRandom, "à deux alliés au hasard"), TestCase(TargetRule.SingleAlly, TargetPick.LowestHp, "à l'allié le plus faible"),
         TestCase(TargetRule.SingleAlly, TargetPick.HighestHp, "à l'allié le plus en forme"), TestCase(TargetRule.SingleAlly, TargetPick.NextToAct, "au prochain allié à agir"),
         TestCase(TargetRule.SingleAlly, TargetPick.StatusHolder, "à un allié porteur de Brûlure"),
         TestCase(TargetRule.AllEnemies, TargetPick.Random, "à tous les ennemis"), TestCase(TargetRule.AllEnemies, TargetPick.TwoRandom, "à deux ennemis au hasard"),
         TestCase(TargetRule.AllAllies, TargetPick.LowestHp, "à toute l'équipe"), TestCase(TargetRule.Self, TargetPick.Random, "au lanceur")]
        [Description("Vérifie la cible nommée de chaque forme (règle et choix) : un ou deux au hasard, le plus faible, le plus en forme, le prochain à agir, porteur d'un statut, zones, lanceur — parce que la fiche nomme la cible par règle et choix")]
        public void Reaction_NamedTargetForms(TargetRule rule, TargetPick pick, string expected)
            => Assert.That(Lines(Arm(ReactionTrigger.CarrierCrit, Dmg(40), StatusType.None, 3, rule, pick, StatusType.Burn)),
                           Is.EqualTo(new[] { "Riposte du lanceur (3 de ses tours) : quand le lanceur réussit un coup critique → Dégâts 40 % " + expected }));

        [Test, Description("Vérifie qu'une compétence qui arme une réaction se décrit en entier (cible de la carte, dégâts, réaction) — parce que la fiche d'une carte passe par Describe")]
        public void Describe_ASkillThatArmsAReaction()
        {
            var skill = new SkillDef(new SkillId("x"), SkillSlot.A3, 0, TargetRule.SingleEnemy,
                new[] { Dmg(100), Arm(ReactionTrigger.EnemyDefeated, new EffectDef(EffectType.Heal, 10), StatusType.None, 2) });
            Assert.That(SkillText.Describe(skill), Is.EqualTo("1 ennemi · Dégâts 100 % · Riposte du lanceur (2 de ses tours) : quand un ennemi est vaincu → Soin 10 % PV au lanceur"));
        }

        const string SoloLine = "Riposte du lanceur (2 de ses tours, seul le lanceur riposte) : quand le lanceur est frappé → Dégâts 60 % à l'attaquant";

        [TestCase(TargetRule.AllAllies, "Toute l'équipe"), TestCase(TargetRule.SingleAlly, "1 allié")]
        [Description("Vérifie qu'une carte à cible ZA ou 1A précise « seul le lanceur riposte » — parce que la carte touche des alliés mais seule la réaction du lanceur est armée")]
        public void Reaction_OnAnAllyCard_SaysOnlyTheCasterRipostes(TargetRule cardTarget, string targetText)
        {
            var skill = new SkillDef(new SkillId("x"), SkillSlot.A3, 0, cardTarget,
                new[] { new EffectDef(EffectType.Shield, 10), Arm(ReactionTrigger.CarrierHit, Dmg(60), StatusType.None, 2, onTrigger: true) });
            Assert.That(SkillText.Describe(skill), Is.EqualTo(targetText + " · Bouclier 10 % PV · " + SoloLine));
            var arm = skill.Effects[1];
            Assert.That(SkillText.Effects(new[] { arm }, null, cardTarget), Is.EqualTo(new[] { SoloLine }), "lignes d'effets avec la cible de la carte");
            Assert.That(N29.HeroesDawn.Presentation.Hub.CollectionText.TierEffects(new CardTier(3, arm), cardTarget), Is.EqualTo(SoloLine), "ligne de palier du grimoire");
            Assert.That(SkillText.Effects(new[] { arm }, null, TargetRule.SingleEnemy).Single(), Does.Not.Contain("seul le lanceur"), "carte qui vise un ennemi : sans la précision");
        }

        [Test, Description("Vérifie qu'en mode propriétaire (montants bruts) l'effet d'une réaction reste en pourcentage — parce que la valeur d'une réaction n'est jamais convertie en montant brut")]
        public void Reaction_OwnerMode_StaysInPercent()
        {
            var owner = new SkillOwnerStats(atk: 196, maxHp: 200);
            var fx = Arm(ReactionTrigger.EnemyReceivesStatus, Dmg(70), StatusType.Shock, 3, TargetRule.SingleEnemy, TargetPick.Random);
            Assert.That(SkillText.Effects(new[] { fx }, owner), Is.EqualTo(Lines(fx)));
        }

        [Test, Description("Vérifie que le recul d'une réaction s'écrit sans « la cible » (la cible suit en fin de phrase) — parce que la phrase de réaction nomme déjà la cible")]
        public void Reaction_PushBack_WithoutTheTargetWord()
            => Assert.That(Lines(Arm(ReactionTrigger.CarrierHit, new EffectDef(EffectType.PushBack, 20), StatusType.None, 2, onTrigger: true)),
                           Is.EqualTo(new[] { "Riposte du lanceur (2 de ses tours) : quand le lanceur est frappé → Recule de 20 % sur la frise à l'attaquant" }));

        // ------------------------------------------------------------------ Marque

        [Test, Description("Vérifie que le statut Marque s'écrit sans durée (« Marque », « Marque (50 %) »), alors que les autres statuts gardent leur durée — parce que la Marque dure jusqu'au prochain coup reçu")]
        public void MarkStatus_HasNoDuration()
        {
            Assert.That(Lines(new EffectDef(EffectType.ApplyStatus, 0, StatusType.Mark, 2)), Is.EqualTo(new[] { "Marque" }));
            Assert.That(Lines(new EffectDef(EffectType.ApplyStatus, 0, StatusType.Mark, 2, 50)), Is.EqualTo(new[] { "Marque (50 %)" }));
            Assert.That(Lines(new EffectDef(EffectType.ApplyStatus, 0, StatusType.Burn, 2)), Is.EqualTo(new[] { "Brûlure 2 t" }));
        }

        // ------------------------------------------------------------------ Consomme et Propage

        [Test, Description("Vérifie la phrase exacte de Consomme (dégâts par tour restant, plafond de tours) — parce que le joueur doit voir que le statut est retiré, la valeur par tour qu'il lui restait et le maximum")]
        public void ConsumeDamage_ExactText()
            => Assert.That(Lines(new EffectDef(EffectType.ConsumeDamage, 40, StatusType.Burn, 3)), Is.EqualTo(new[] { "Retire Brûlure de la cible : Dégâts 40 % par tour qu'il lui restait (au plus 3 tours)" }));

        [Test, Description("Vérifie la phrase exacte de Consomme en soin (statut Régén., max 5) — parce que le soin s'écrit en % de PV comme les autres soins")]
        public void ConsumeHeal_ExactText()
            => Assert.That(Lines(new EffectDef(EffectType.ConsumeHeal, 30, StatusType.Regen, 5)), Is.EqualTo(new[] { "Retire Régén. de la cible : Soin 30 % PV par tour qu'il lui restait (au plus 5 tours)" }));

        [Test, Description("Vérifie la phrase de Consomme Marque (max 1) : « au plus 1 tour » au singulier — parce que la forme à un seul tour garde la même tournure")]
        public void ConsumeDamage_Mark_MaxOne()
            => Assert.That(Lines(new EffectDef(EffectType.ConsumeDamage, 150, StatusType.Mark, 1)), Is.EqualTo(new[] { "Retire Marque de la cible : Dégâts 150 % par tour qu'il lui restait (au plus 1 tour)" }));

        [Test, Description("Vérifie la phrase exacte de Propage — parce que le joueur doit lire que le statut est copié avec ses tours restants sur tous les autres ennemis")]
        public void Spread_ExactText()
            => Assert.That(Lines(new EffectDef(EffectType.Spread, 0, StatusType.Burn)), Is.EqualTo(new[] { "Copie Brûlure de la cible (mêmes tours restants) sur tous les autres ennemis" }));

        [Test, Description("Vérifie qu'en mode propriétaire les textes de Consomme et de Propage restent en pourcentage — parce que ces valeurs sont par tour restant et ne se convertissent pas en montant brut")]
        public void ConsumeAndSpread_OwnerMode_StayInPercent()
        {
            var owner = new SkillOwnerStats(atk: 196, maxHp: 200);
            var fx = new[] { new EffectDef(EffectType.ConsumeDamage, 40, StatusType.Burn, 3), new EffectDef(EffectType.ConsumeHeal, 30, StatusType.Regen, 5), new EffectDef(EffectType.Spread, 0, StatusType.Burn) };
            Assert.That(SkillText.Effects(fx, owner), Is.EqualTo(new[]
            {
                "Retire Brûlure de la cible : Dégâts 40 % par tour qu'il lui restait (au plus 3 tours)", "Retire Régén. de la cible : Soin 30 % PV par tour qu'il lui restait (au plus 5 tours)",
                "Copie Brûlure de la cible (mêmes tours restants) sur tous les autres ennemis",
            }));
        }

        // ------------------------------------------------------------------ Échelle d'archétype

        [TestCase(Archetype.Burn, "Brûlure"), TestCase(Archetype.Restrain, "Entrave"), TestCase(Archetype.Hunt, "Traque"),
         TestCase(Archetype.Tempo, "Tempo"), TestCase(Archetype.Bulwark, "Rempart"), TestCase(Archetype.Renewal, "Renouveau")]
        [Description("Vérifie la phrase exacte de l'échelle d'archétype pour chaque archétype (« carte d'archétype Brûlure », carte lancée comprise, au plus M cartes) — parce que le joueur doit distinguer l'archétype Brûlure du statut Brûlure et savoir que la carte se compte")]
        public void ArchetypeScale_ExactSentence_PerArchetype(Archetype a, string name)
        {
            var fx = new EffectDef(EffectType.Damage, 100, mods: new[] { new ValueMod(10, null, ScaleKind.EquippedArchetypeCards, 4, a) });
            Assert.That(Lines(fx), Is.EqualTo(new[] { "Dégâts 100 %", "+10 % par carte d'archétype " + name + " équipée dans l'escouade, celle-ci comprise (au plus 4 cartes)" }));
            Assert.That(Enum.GetValues(typeof(Archetype)).Length, Is.EqualTo(6), "un Archetype a été ajouté : compléter la table");
        }

        [Test, Description("Vérifie que l'échelle d'archétype se convertit en montant brut comme les autres échelles en mode propriétaire, et qu'au plus une carte s'écrit au singulier — parce que « +19 de dégâts par carte » doit suivre « Dégâts 196 »")]
        public void ArchetypeScale_OwnerMode_ConvertsToRaw()
        {
            var fx = new EffectDef(EffectType.Damage, 100, mods: new[] { new ValueMod(10, null, ScaleKind.EquippedArchetypeCards, 3, Archetype.Bulwark) });
            Assert.That(SkillText.Effects(new[] { fx }, new SkillOwnerStats(196, 200)),
                        Is.EqualTo(new[] { "Dégâts 196", "+19 de dégâts par carte d'archétype Rempart équipée dans l'escouade, celle-ci comprise (au plus 3 cartes)" }));
            var one = new EffectDef(EffectType.Damage, 100, mods: new[] { new ValueMod(10, null, ScaleKind.EquippedArchetypeCards, 1, Archetype.Tempo) });
            Assert.That(Lines(one)[1], Is.EqualTo("+10 % par carte d'archétype Tempo équipée dans l'escouade, celle-ci comprise (au plus 1 carte)"));
        }

        // ------------------------------------------------------------------ Textes existants et clés

        [Test, Description("Vérifie qu'un effet sans réaction, Consomme, Propage ni échelle d'archétype garde exactement son ancien texte (cartes Braise et Mur de verre) — parce que le texte existant ne doit pas bouger")]
        public void PlainEffects_KeepExactTextOfExistingCards()
        {
            var col = PrototypeCollection.Build();
            var ember = col.Card(new CardId("card_ember"));
            var wall = col.Card(new CardId("card_glass_wall"));
            Assert.That(SkillText.Describe(ember.Skill), Is.EqualTo("1 ennemi · Dégâts 140 %"));
            Assert.That(SkillText.Describe(wall.Skill), Is.EqualTo("Toute l'équipe · Bouclier 15 % PV"));
            Assert.That(Lines(new EffectDef(EffectType.Damage, 100, mods: new[] { new ValueMod(15, null, ScaleKind.TargetStatusCount, 3) })), Is.EqualTo(new[] { "Dégâts 100 %", "+15 % par statut sur la cible (max 3)" }));
            Assert.That(Lines(new EffectDef(EffectType.Advance, 15)), Is.EqualTo(new[] { "Propulse la cible de 15 % vers l'action" }), "l'élan hors réaction garde son texte");
        }

        [Test, Description("Vérifie qu'une clé de texte existe pour chaque ReactionTrigger (événement et cible déclencheur), chaque cible de réaction (règle et choix), Archetype et ScaleKind, et pour les gabarits de réaction, Consomme, Propage et échelle d'archétype — parce qu'une valeur ajoutée plus tard sans texte afficherait une clé brute")]
        public void AllKeysExist_ForEveryNewKind()
        {
            foreach (ReactionTrigger k in Enum.GetValues(typeof(ReactionTrigger)))
            {
                Assert.That(Loc.Has("reaction.trigger." + k), "reaction.trigger." + k);
                Assert.That(Loc.Has("reaction.trigger_unit." + k), "reaction.trigger_unit." + k);
            }
            foreach (TargetRule k in Enum.GetValues(typeof(TargetRule)))
            {
                if (k == TargetRule.SingleEnemy || k == TargetRule.SingleAlly)
                    foreach (TargetPick p in Enum.GetValues(typeof(TargetPick))) Assert.That(Loc.Has("reaction.to." + k + "." + p), "reaction.to." + k + "." + p);
                else Assert.That(Loc.Has("reaction.to." + k), "reaction.to." + k);
            }
            foreach (Archetype k in Enum.GetValues(typeof(Archetype))) Assert.That(Loc.Has("archetype." + k), "archetype." + k);
            foreach (ScaleKind k in Enum.GetValues(typeof(ScaleKind))) Assert.That(Loc.Has("scale." + k), "scale." + k);
            foreach (var key in new[] { "reaction.line", "reaction.line_solo", "reaction.to.AllEnemies.TwoRandom", "fx.reaction_advance", "fx.reaction_pushback",
                                        "fx.consume_damage", "fx.consume_damage_one", "fx.consume_heal", "fx.consume_heal_one", "fx.spread",
                                        "fx.mod_archetype", "fx.mod_archetype_one", "fx.status_untimed", "fx.status_untimed_chance" })
                Assert.That(Loc.Has(key), key);
            Assert.That(Enum.GetValues(typeof(ReactionTrigger)).Length, Is.EqualTo(6), "un ReactionTrigger a été ajouté : compléter la table des phrases");
            Assert.That(Enum.GetValues(typeof(TargetRule)).Length, Is.EqualTo(5), "une TargetRule a été ajoutée : compléter les cibles de réaction");
        }
    }
}
