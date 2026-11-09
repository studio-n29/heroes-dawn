#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Textes anglais (L1 de la 0.15.0). Couverture : coque du QG, connexion, boutique, réglages (langue, audio, graphismes, accessibilité).
    /// Une clé absente de ce dictionnaire reprend le texte français : les autres écrans se traduisent par lots.
    /// </summary>
    public static partial class Loc
    {
        static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            // --- Général ---
            ["booster.next"] = "NEXT",
            ["lab.mode.craft"] = "CRAFTING",
            ["lab.mode.research"] = "RESEARCH",
            ["lab.mode.research_hint"] = "Choose a research project. Its cost and duration are shown before starting. Only one project can run at a time.",
            ["booster.summary"] = "BOOSTER SUMMARY",
            ["booster.received"] = "{0} cards added to the Grimoire",
            ["shop.daily_pass"] = "GIFT · PASS",
            ["game.tagline"] = "New York still holds. For how long?",
            ["ui.ok"] = "OK", ["ui.cancel"] = "Cancel", ["ui.confirm"] = "Confirm", ["ui.continue"] = "Continue",
            ["ui.close"] = "Close", ["ui.back"] = "Back", ["ui.credits"] = "Credits",
            ["ui.level"] = "Lv. {0}", ["ui.fatigue"] = "Fatigue {0} %", ["ui.saving"] = "Saving…",

            // --- Connexion ---
            ["login.play"] = "PLAY AS GUEST",
            ["login.connecting"] = "Connecting to N29 servers…",
            ["login.loading"] = "Loading profile…",
            ["login.reset"] = "Erase local save",
            ["login.reset.title"] = "Erase save?",
            ["login.reset.body"] = "Your local progress will be deleted. The guest profile will start from scratch.",
            ["login.welcome_new"] = "Welcome, {0}! Your squad is waiting at HQ.",
            ["login.welcome_back"] = "Welcome back, {0}!",
            ["login.fail_network"] = "Connection impossible (simulated network)",

            // --- QG ---
            ["hub.title"] = "HQ — RED HOOK",
            ["hub.tab.team"] = "TEAM",
            ["hub.tab.recruit"] = "SHOP",
            ["hub.ops"] = "OPERATIONS ROOM",
            ["hub.squad.in"] = "IN SQUAD",
            ["hub.squad.add"] = "ADD",
            ["hub.deploy"] = "DEPLOY",
            ["hub.logout"] = "Log out",
            ["hub.quit.title"] = "Quit the game?",
            ["hub.quit.body"] = "Your progress is saved.",
            ["hub.quit.ok"] = "Quit",

            // --- Coque du hub ---
            ["nav.qg"] = "HQ", ["nav.team"] = "TEAM", ["nav.recruit"] = "RECRUIT", ["nav.pass"] = "PASS", ["nav.shop"] = "SHOP",
            ["nav.back"] = "BACK", ["nav.settings"] = "SETTINGS",
            ["page.qg"] = "HQ · New York Section",
            ["page.team"] = "Team · {0} members", ["page.team_one"] = "Team · 1 member",
            ["page.recruit"] = "Recruitment",
            ["page.pass"] = "Pass · Season 1", ["page.shop"] = "Shop",
            ["page.operations"] = "Operations room", ["page.lab"] = "Crafting workshop", ["page.archives"] = "Archives",
            ["page.settings"] = "Settings", ["page.member_sheet"] = "Member sheet",

            // --- Boutique éphémère ---
            ["shop.title"] = "Ephemeral shop · 3 heroes a day",
            ["shop.countdown.h"] = "New heroes in {0} h {1:00}",
            ["shop.countdown.m"] = "New heroes in {0} min",
            ["shop.reroll.free"] = "REROLL · FREE",
            ["shop.reroll.cost"] = "REROLL · {0} CR",
            ["shop.reroll.confirm.title"] = "Reroll the shop?",
            ["shop.reroll.confirm.body"] = "3 new heroes for {0} Credits.",
            ["shop.recruited"] = "RECRUITED",
            ["shop.empty"] = "New York collection complete!",
            ["shop.cat.gems"] = "GEMS", ["shop.cat.gems_sub"] = "Gem packs for purchase",
            ["shop.cat.exchange"] = "EXCHANGES", ["shop.cat.exchange_sub"] = "Turn gems into credits or PR",
            ["power.tier"] = "POWER {0} / 5",
            ["qg.state.new"] = "NEW", ["qg.state.open"] = "OPEN",
            ["qg.main.line"] = "{0} · {1} / {2} zones cleared",
            ["grimoire.search"] = "Search a card",
            ["onboarding.name.invalid"] = "The name must be 1 to 20 characters long.",
            ["profile.tag_guest"] = "Create an account to get your tag", ["profile.tag_guest_reason"] = "Available after creating an account",
            ["profile.last_achievement"] = "Last achievement", ["profile.no_achievement"] = "No achievement yet",
            ["ach.title"] = "ACHIEVEMENTS", ["ach.line"] = "{0} / {1} unlocked", ["ach.back"] = "BACK", ["ach.progress"] = "{0} / {1}",
            ["ach.first_win.name"] = "First win", ["ach.first_win.desc"] = "Win a combat.",
            ["ach.wins_10.name"] = "Veteran", ["ach.wins_10.desc"] = "Win 10 combats.",
            ["ach.wins_50.name"] = "Street legend", ["ach.wins_50.desc"] = "Win 50 combats.",
            ["ach.zone_1.name"] = "First district", ["ach.zone_1.desc"] = "Clear a zone.",
            ["ach.zone_4.name"] = "Brooklyn tour", ["ach.zone_4.desc"] = "Clear 4 zones.",
            ["ach.agents_5.name"] = "Small circle", ["ach.agents_5.desc"] = "Add 5 agents to your team.",
            ["ach.agents_15.name"] = "Big network", ["ach.agents_15.desc"] = "Add 15 agents to your team.",
            ["ach.craft_1.name"] = "First tinkering", ["ach.craft_1.desc"] = "Craft an item.",
            ["ach.friend_1.name"] = "First ally", ["ach.friend_1.desc"] = "Add a friend.",
            ["lab.side.none"] = "Pick a recipe", ["lab.state.ok"] = "Crafting possible.", ["lab.state.missing"] = "Not enough components.",
            ["lab.provenance"] = "Scraps: daily shop lots and combat loot.", ["lab.have"] = "Owned: {0}", ["lab.missing"] = "Missing {0}",
            ["qg.daily.banner"] = "{0} activity(ies) today",["qg.state.locked"] = "LOCKED", ["qg.lock.tutorial"] = "Finish the tutorial", ["qg.lock.zone"] = "Clear chapter {0}",
            ["settings.unlock_all"] = "Unlock everything (test)", ["settings.unlock_all.hint"] = "Opens every QG room, without tutorial or chapter.",
            ["inventory.sort.rarity"] = "Rarity", ["inventory.sort.name"] = "Name", ["inventory.sort.count"] = "Quantity",
            ["shop.scrap_lot"] = "{0} × {1}", ["shop.scrap_sold"] = "BOUGHT",
            ["shop.scrap.confirm.title"] = "Buy this lot?", ["shop.scrap.confirm.body"] = "{0} Credits will be spent.",
            ["scrap.Weapon"] = "Weapon scraps", ["scrap.Armor"] = "Armor scraps", ["scrap.Tool"] = "Tool scraps",
            ["scrap.Implant"] = "Implant scraps", ["scrap.Core"] = "Cores",

            // --- Paramètres ---
            ["settings.cat.Audio"] = "AUDIO",
            ["settings.cat.Graphics"] = "GRAPHICS",
            ["settings.cat.Accessibility"] = "ACCESSIBILITY",
            ["settings.cat.Language"] = "LANGUAGE",
            ["settings.cat.Notifications"] = "NOTIFICATIONS",
            ["settings.cat.Controls"] = "CONTROLS",
            ["settings.cat.Account"] = "ACCOUNT",
            ["settings.cat.Support"] = "SUPPORT & HELP",
            ["settings.reset_category"] = "RESET",
            ["settings.on"] = "ON",
            ["settings.off"] = "OFF",
            ["settings.master"] = "Master volume",
            ["settings.music"] = "Music",
            ["settings.sfx"] = "Effects",
            ["settings.voice"] = "Voice",
            ["settings.mute_bg"] = "Mute in background",
            ["settings.quality"] = "Graphics quality",
            ["settings.quality.0"] = "AUTO",
            ["settings.quality.1"] = "LOW",
            ["settings.quality.2"] = "MEDIUM",
            ["settings.quality.3"] = "HIGH",
            ["settings.fps60"] = "60 frames per second",
            ["settings.battery"] = "Battery saver",
            ["settings.full_fx"] = "Full combat effects",
            ["settings.flashes"] = "Reduce flashes",
            ["settings.shakes"] = "Reduce shakes",
            ["settings.subtitles"] = "Subtitles",
            ["settings.lang.texts"] = "Text language",
            ["settings.lang.fr"] = "Français",
            ["settings.lang.voice"] = "Voice language",
            ["settings.lang.en"] = "English",
            ["settings.lang.more"] = "English, Spanish, Portuguese, German",

            // --- Dialogues et entrée en combat (L7) ---
            ["narration.skip"] = "SKIP",
            ["narration.next"] = "▼ Tap to continue",
            ["narration.next_btn"] = "NEXT",
            ["narration.auto"] = "AUTO",
            ["combat.boss.phase"] = "PHASE {0}/{1}",
            ["combat.boss.phase_banner"] = "PHASE {0}!",
            ["combat.boss.phase_log"] = "phase {0}/{1}: {2}",
            ["combat.intro.skip"] = "SKIP",
            ["combat.intro.allies"] = "YOUR SQUAD",
            ["combat.intro.enemies"] = "ENEMIES",
        };

        /// <summary>Clés traduites en anglais (contrôlées par les tests de langue).</summary>
        public static IEnumerable<string> EnglishKeys() => En.Keys;
    }
}
