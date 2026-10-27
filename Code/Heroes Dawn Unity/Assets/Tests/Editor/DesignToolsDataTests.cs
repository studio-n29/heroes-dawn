#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.EditorTools.Design;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Outils de design : les fiches JSON reproduisent le contenu du code, se valident et remplacent le contenu au chargement.</summary>
    public sealed class DesignToolsDataTests
    {
        static readonly ContentCatalog Code = PrototypeContent.Build();

        static (ItemsFile Items, SkillsFile Skills, AgentsFile Agents) FromCode()
        {
            var items = new ItemsFile();
            foreach (var i in Code.Collection.Items) items.items.Add(ContentDataConverter.FromDef(i, i.Id.Value));
            var skills = new SkillsFile();
            var agents = new AgentsFile();
            foreach (var m in Code.Members)
            {
                foreach (var s in m.Skills) skills.skills.Add(ContentDataConverter.FromDef(s, s.Id.Value));
                agents.agents.Add(ContentDataConverter.FromDef(m, m.Id.Value));
            }
            return (items, skills, agents);
        }

        [Test, Description("Vérifie que les fiches tirées du code reviennent identiques après conversion (agents, statistiques, compétences, effets, objets, déblocages) — parce que remplacer le code par les fichiers ne doit rien changer au jeu")]
        public void CodeContent_RoundTripsThroughTheRecords()
        {
            var (items, skills, agents) = FromCode();
            var lib = ContentDataLibrary.FromFiles(items, skills, agents);
            Assert.That(lib.Skipped, Is.Empty);
            Assert.That(lib.Members!.Count, Is.EqualTo(Code.Members.Count));
            Assert.That(lib.Items!.Count, Is.EqualTo(Code.Collection.Items.Count));
            for (int i = 0; i < Code.Members.Count; i++)
            {
                var a = Code.Members[i]; var b = lib.Members[i];
                Assert.That(b.Id, Is.EqualTo(a.Id));
                Assert.That((b.Family, b.Role, b.Rarity), Is.EqualTo((a.Family, a.Role, a.Rarity)));
                Assert.That((b.BaseStats.Hp, b.BaseStats.Atk, b.BaseStats.Def, b.BaseStats.Spd), Is.EqualTo((a.BaseStats.Hp, a.BaseStats.Atk, a.BaseStats.Def, a.BaseStats.Spd)));
                Assert.That(b.UnlockZone, Is.EqualTo(a.UnlockZone), a.Id.Value);
                Assert.That(b.Unlock?.Kind, Is.EqualTo(a.Unlock?.Kind), a.Id.Value);
                Assert.That(b.Unlock?.Zone, Is.EqualTo(a.Unlock?.Zone), a.Id.Value);
                Assert.That(b.Unlock?.Feat, Is.EqualTo(a.Unlock?.Feat), a.Id.Value);
                Assert.That(b.Unlock?.WeekdayMask, Is.EqualTo(a.Unlock?.WeekdayMask), a.Id.Value);
                Assert.That(b.Skills.Count, Is.EqualTo(a.Skills.Count));
                for (int s = 0; s < a.Skills.Count; s++)
                {
                    var x = a.Skills[s]; var y = b.Skills[s];
                    Assert.That((y.Id, y.Slot, y.Cooldown, y.Target), Is.EqualTo((x.Id, x.Slot, x.Cooldown, x.Target)));
                    Assert.That(y.Effects.Select(e => (e.Type, e.Value, e.Status, e.Duration, e.ChancePercent)),
                                Is.EqualTo(x.Effects.Select(e => (e.Type, e.Value, e.Status, e.Duration, e.ChancePercent))), x.Id.Value);
                }
            }
            foreach (var i in Code.Collection.Items)
            {
                var j = lib.Items.Single(x => x.Id == i.Id);
                Assert.That((j.Rarity, j.HpPercent, j.AtkPercent, j.DefPercent, j.SpdPercent, j.Perk, j.PerkValue, j.Price),
                            Is.EqualTo((i.Rarity, i.HpPercent, i.AtkPercent, i.DefPercent, i.SpdPercent, i.Perk, i.PerkValue, i.Price)), i.Id.Value);
            }
        }

        [Test, Description("Vérifie que le contenu du code passe toutes les validations des outils — parce qu'un fichier initial invalide bloquerait l'enregistrement dès l'ouverture")]
        public void CodeContent_IsValid()
        {
            var (items, skills, agents) = FromCode();
            Assert.That(ContentDataValidator.ValidateAll(items, skills, agents, DesignStore.ZoneIds()), Is.Empty);
        }

        [Test, Description("Vérifie que les fichiers de design livrés avec le projet (Resources/Content) se chargent sans rien ignorer, avec les 30 agents et les 10 objets — parce que ce sont eux que le jeu lit au démarrage")]
        public void ShippedFiles_LoadCleanly()
        {
            var lib = ContentDataLibrary.Load();
            Assert.That(lib.Skipped, Is.Empty);
            Assert.That(lib.Members, Is.Not.Null);
            Assert.That(lib.Members!.Count, Is.EqualTo(32));
            Assert.That(lib.Items!.Count, Is.EqualTo(10));
            Assert.That(lib.Texts["members.darius_brooks.name"], Is.EqualTo("Darius Brooks"));
        }

        [Test, Description("Vérifie qu'un objet retiré du fichier disparaît du jeu et qu'un objet ajouté apparaît, avec son nom — parce que le fichier remplace la liste du code en entier")]
        public void Catalog_FollowsTheItemsFile()
        {
            var (items, skills, agents) = FromCode();
            items.items.RemoveAll(i => i.id == "kevlar_vest");
            items.items.Add(new ItemRecord { id = "neon_visor", name = "Visière néon", rarity = ItemRarity.Rare, atk = 5, price = 700 });
            var lib = ContentDataLibrary.FromFiles(items, skills, agents);
            var catalog = Code.WithItems(lib.Items!);
            Assert.That(catalog.Collection.Items.Any(i => i.Id.Value == "kevlar_vest"), Is.False);
            Assert.That(catalog.Collection.Item(new Core.ItemId("neon_visor")).AtkPercent, Is.EqualTo(5));
            Assert.That(lib.Texts["items.neon_visor.name"], Is.EqualTo("Visière néon"));
            Assert.That(catalog.Collection.Cards.Count, Is.EqualTo(Code.Collection.Cards.Count), "les cartes ne bougent pas");
        }

        [Test, Description("Vérifie qu'un nouvel agent avec ses trois compétences entre dans le catalogue et qu'un agent supprimé en sort — parce que l'outil des agents ajoute et supprime des agents complets")]
        public void Catalog_FollowsTheAgentsFile()
        {
            var (items, skills, agents) = FromCode();
            agents.agents.RemoveAll(a => a.id == "giulia_rossi");
            for (int i = 1; i <= 2; i++)
                skills.skills.Add(new SkillRecord { id = "nova_a" + i, name = "Nova " + i, slot = (SkillSlot)(i - 1), target = TargetRule.SingleEnemy, effects = { new EffectRecord { type = EffectType.Damage, value = 100 } } });
            agents.agents.Add(new AgentRecord { id = "nova", name = "Nova", family = Family.Elemental, role = Role.Damage, rarity = Rarity.Elite, skills = new List<string> { "nova_a1", "nova_a2" }, hp = 1200, atk = 150, def = 90, spd = 110 });
            var lib = ContentDataLibrary.FromFiles(items, skills, agents);
            var catalog = Code.WithMembers(lib.Members!);
            Assert.That(catalog.Members.Any(m => m.Id.Value == "giulia_rossi"), Is.False);
            var nova = catalog.Member(new Core.MemberId("nova"));
            Assert.That(nova.Skills.Count, Is.EqualTo(2));
            Assert.That(nova.BaseStats.Atk, Is.EqualTo(150));
        }

        [Test, Description("Vérifie que les erreurs de saisie sont détectées : identifiant invalide ou en double, compétence introuvable ou partagée, effet sans durée, objet sans valeur d'effet — parce qu'on ne doit pas pouvoir enregistrer un contenu cassé")]
        public void Validator_CatchesMistakes()
        {
            var (items, skills, agents) = FromCode();
            Assert.That(ContentDataValidator.Validate(new ItemRecord { id = "Mauvais Id", name = "x" }), Is.Not.Empty);
            Assert.That(ContentDataValidator.Validate(new ItemRecord { id = "ok", name = "x", perk = ItemPerk.CritChance }), Is.Not.Empty, "effet sans valeur");
            Assert.That(ContentDataValidator.Validate(new SkillRecord { id = "ok", name = "x", effects = { new EffectRecord { type = EffectType.ApplyStatus, status = StatusType.Burn } } }), Is.Not.Empty, "statut sans durée");
            Assert.That(ContentDataValidator.Validate(new SkillRecord { id = "ok", name = "x" }), Is.Not.Empty, "aucun effet");
            agents.agents.Add(new AgentRecord { id = "darius_brooks", name = "Doublon", skills = new List<string> { "darius_brooks_a1", "darius_brooks_a2", "introuvable" } });
            agents.agents.Add(new AgentRecord { id = "clone", name = "Clone", skills = new List<string> { "darius_brooks_a1", "darius_brooks_a2", "darius_brooks_a3" } });
            var all = ContentDataValidator.ValidateAll(items, skills, agents);
            Assert.That(all.Any(m => m.Contains("« darius_brooks » en double")), Is.True);
            Assert.That(all.Any(m => m.Contains("introuvable")), Is.True);
            Assert.That(all.Any(m => m.Contains("plusieurs agents")), Is.True);
        }

        [Test, Description("Vérifie qu'un agent invalide du fichier est ignoré au chargement et signalé, sans empêcher les autres de se charger — parce que le jeu ne doit pas planter sur une fiche cassée")]
        public void Library_SkipsABrokenAgent()
        {
            var (items, skills, agents) = FromCode();
            agents.agents.Add(new AgentRecord { id = "casse", name = "Cassé", skills = new List<string> { "x", "y", "z" } });
            var lib = ContentDataLibrary.FromFiles(items, skills, agents);
            Assert.That(lib.Skipped.Single(), Does.Contain("casse"));
            Assert.That(lib.Members!.Count, Is.EqualTo(32));
        }

        [Test, Description("Vérifie qu'un texte modifié dans l'outil de narration remplace le texte d'origine du jeu et que « revenir à l'original » le rétablit — parce que le tutoriel et les astuces se corrigent sans toucher au code")]
        public void TextsFile_OverridesTheGameTexts()
        {
            const string key = "tutorial.step.Move.title";
            string original = N29.HeroesDawn.Presentation.Common.Loc.T(key);
            var (items, skills, agents) = FromCode();
            var texts = new TextsFile();
            texts.texts.Add(new TextRecord { key = key, text = "SE DÉPLACER" });
            var lib = ContentDataLibrary.FromFiles(items, skills, agents, texts);
            Assert.That(lib.Texts[key], Is.EqualTo("SE DÉPLACER"));
            try
            {
                N29.HeroesDawn.Presentation.Common.Loc.Override(new Dictionary<string, string> { [key] = lib.Texts[key] });
                Assert.That(N29.HeroesDawn.Presentation.Common.Loc.T(key), Is.EqualTo("SE DÉPLACER"));
            }
            finally { N29.HeroesDawn.Presentation.Common.Loc.Override(new Dictionary<string, string> { [key] = original }); }
            Assert.That(N29.HeroesDawn.Presentation.Common.Loc.T(key), Is.EqualTo(original));
        }

        [Test, Description("Vérifie que le fichier de textes livré avec le projet existe et se charge, vide au départ — parce que l'outil de narration y enregistre ses modifications")]
        public void ShippedTextsFile_Loads()
        {
            var asset = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Content/texts");
            Assert.That(asset, Is.Not.Null);
            Assert.That(UnityEngine.JsonUtility.FromJson<TextsFile>(asset!.text).texts, Is.Empty);
        }

        // ------------------------------------------------------------------ Catégories (lot B 0.14.0)

        static readonly Dictionary<string, CardNature> SpecNatures = new Dictionary<string, CardNature>
        {
            ["card_prism_burst"] = CardNature.Offensive, ["card_shackle"] = CardNature.Offensive, ["card_ember"] = CardNature.Offensive,
            ["card_storm"] = CardNature.Offensive, ["card_inferno"] = CardNature.Offensive, ["card_bulwark_charge"] = CardNature.Offensive,
            ["card_mark"] = CardNature.Offensive, ["card_glass_wall"] = CardNature.Defensive, ["card_second_wind"] = CardNature.Support,
            ["card_regen_field"] = CardNature.Support, ["card_precog"] = CardNature.Support, ["card_time_rift"] = CardNature.Support,
            // Lot C2 : 36 nouvelles cartes
            ["card_glass_shards"] = CardNature.Offensive, ["card_concrete_rampart"] = CardNature.Defensive, ["card_rivet_armor"] = CardNature.Defensive,
            ["card_healing_molt"] = CardNature.Support, ["card_reflecting_prism"] = CardNature.Defensive, ["card_support_cables"] = CardNature.Support,
            ["card_borrowed_mask"] = CardNature.Support, ["card_living_forge"] = CardNature.Support, ["card_monolith"] = CardNature.Offensive,
            ["card_frost_wall"] = CardNature.Defensive, ["card_steam_shield"] = CardNature.Defensive, ["card_tonic_current"] = CardNature.Support,
            ["card_soothing_mist"] = CardNature.Support, ["card_neon_halo"] = CardNature.Defensive, ["card_spark_rush"] = CardNature.Support,
            ["card_power_flux"] = CardNature.Support, ["card_ice_carapace"] = CardNature.Defensive, ["card_chain_lightning"] = CardNature.Offensive,
            ["card_ram_strike"] = CardNature.Offensive, ["card_steel_skin"] = CardNature.Defensive, ["card_deep_anchor"] = CardNature.Defensive,
            ["card_fresh_blood"] = CardNature.Support, ["card_bloodletting"] = CardNature.Offensive, ["card_challenge"] = CardNature.Defensive,
            ["card_shared_pulse"] = CardNature.Support, ["card_living_bastion"] = CardNature.Defensive, ["card_telluric_fist"] = CardNature.Offensive,
            ["card_precise_shot"] = CardNature.Offensive, ["card_side_step"] = CardNature.Defensive, ["card_camouflage"] = CardNature.Defensive,
            ["card_guiding_echo"] = CardNature.Support, ["card_guard_break"] = CardNature.Offensive, ["card_premonition"] = CardNature.Defensive,
            ["card_shared_watch"] = CardNature.Support, ["card_silent_takedown"] = CardNature.Offensive, ["card_time_veil"] = CardNature.Defensive,
        };

        static readonly Dictionary<string, ItemType> SpecTypes = new Dictionary<string, ItemType>
        {
            ["steel_knuckles"] = ItemType.Weapon, ["titan_gauntlets"] = ItemType.Weapon, ["apex_module"] = ItemType.Weapon,
            ["kevlar_vest"] = ItemType.Armor, ["riot_shield"] = ItemType.Armor, ["reactive_plating"] = ItemType.Armor, ["phoenix_core"] = ItemType.Armor,
            ["running_shoes"] = ItemType.Tool, ["adrenaline_patch"] = ItemType.Tool, ["time_shard"] = ItemType.Tool,
            ["power_gauntlet"] = ItemType.Implant, ["arcane_focus"] = ItemType.Implant,
        };

        [Test, Description("Vérifie que les 12 cartes et les 12 objets du code ont la nature et le type fixés par la spec du lot B — parce que ces catégories pilotent les filtres et la règle d'équipement")]
        public void CodeCatalog_HasTheSpecCategories()
        {
            var col = PrototypeCollection.Build();
            Assert.That(col.Cards.ToDictionary(c => c.Id.Value, c => c.Nature), Is.EquivalentTo(SpecNatures));
            Assert.That(col.Items.ToDictionary(i => i.Id.Value, i => i.Type), Is.EquivalentTo(SpecTypes));
        }

        [Test, Description("Vérifie que chaque objet de items.json porte un type saisi, égal à celui de la spec — parce qu'un type manquant serait déduit en silence et pourrait changer l'équipement autorisé")]
        public void ItemsJson_EveryItemHasAnExplicitType()
        {
            var asset = UnityEngine.Resources.Load<UnityEngine.TextAsset>(ContentDataLibrary.ResourcesFolder + "/" + ContentDataLibrary.ItemsName);
            var file = UnityEngine.JsonUtility.FromJson<ItemsFile>(asset.text);
            Assert.That(file.items, Is.Not.Empty);
            foreach (var r in file.items)
            {
                Assert.That(System.Enum.IsDefined(typeof(ItemType), r.type), Is.True, r.id + " sans type");
                Assert.That(r.type, Is.EqualTo(SpecTypes[r.id]), r.id);
            }
        }

        [Test, Description("Vérifie qu'une fiche d'objet sans champ type reçoit le type déduit (ici Armure), pas la première valeur de l'enum — parce que JsonUtility remet un champ absent à sa valeur par défaut")]
        public void ItemRecord_WithoutType_GetsTheDeducedType()
        {
            var file = UnityEngine.JsonUtility.FromJson<ItemsFile>("{\"items\":[{\"id\":\"x\",\"def\":10}]}");
            Assert.That(ContentDataConverter.ToDef(file.items[0]).Type, Is.EqualTo(ItemType.Armor));
        }

        [Test, Description("Vérifie qu'un objet repasse par sa fiche sans perdre son type — parce que l'outil Objets réécrit items.json depuis les définitions")]
        public void ItemRecord_RoundTrip_KeepsTheType()
        {
            var tool = new ItemDef(new Core.ItemId("t"), "t", ItemRarity.Common, atk: 20, type: ItemType.Tool);
            Assert.That(ContentDataConverter.ToDef(ContentDataConverter.FromDef(tool, "T")).Type, Is.EqualTo(ItemType.Tool));
        }
    }
}
