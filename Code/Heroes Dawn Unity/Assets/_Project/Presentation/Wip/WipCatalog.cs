#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Wip
{
    /// <summary>Liste unique des fonctions en WIP (retirer une entrée quand la fonction existe).</summary>
    public static class WipCatalog
    {
        public static readonly WipFeature SecondaryObjectives = new WipFeature("secondary_objectives");
        public static readonly WipFeature SettingsNotifications = new WipFeature("settings_notifications");
        public static readonly WipFeature SettingsControls = new WipFeature("settings_controls");

        public static readonly IReadOnlyList<WipFeature> All = new[]
        {
            SecondaryObjectives, SettingsNotifications, SettingsControls,
        };

        public static WipFeature Get(string id)
        {
            foreach (var f in All) if (f.Id == id) return f;
            throw new ArgumentException("Fonction WIP inconnue : " + id, nameof(id));
        }
    }
}
