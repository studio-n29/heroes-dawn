#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Wip
{
    /// <summary>Liste unique des fonctions en WIP (retirer une entrée quand la fonction existe).</summary>
    public static class WipCatalog
    {
        public static readonly WipFeature Pass = new WipFeature("pass");
        public static readonly WipFeature Shop = new WipFeature("shop");
        public static readonly WipFeature Operations = new WipFeature("operations");
        public static readonly WipFeature Lab = new WipFeature("lab");
        public static readonly WipFeature Archives = new WipFeature("archives");
        public static readonly WipFeature DailyMissions = new WipFeature("daily_missions");
        public static readonly WipFeature SecondaryObjectives = new WipFeature("secondary_objectives");
        public static readonly WipFeature Gems = new WipFeature("gems");
        public static readonly WipFeature MemberArt = new WipFeature("member_art");
        public static readonly WipFeature SettingsAudio = new WipFeature("settings_audio");
        public static readonly WipFeature SettingsGraphics = new WipFeature("settings_graphics");
        public static readonly WipFeature SettingsAccessibility = new WipFeature("settings_accessibility");
        public static readonly WipFeature SettingsNotifications = new WipFeature("settings_notifications");
        public static readonly WipFeature SettingsControls = new WipFeature("settings_controls");

        public static readonly IReadOnlyList<WipFeature> All = new[]
        {
            Pass, Shop, Operations, Lab, Archives, DailyMissions, SecondaryObjectives, Gems, MemberArt,
            SettingsAudio, SettingsGraphics, SettingsAccessibility, SettingsNotifications, SettingsControls,
        };

        public static WipFeature Get(string id)
        {
            foreach (var f in All) if (f.Id == id) return f;
            throw new ArgumentException("Fonction WIP inconnue : " + id, nameof(id));
        }
    }
}
