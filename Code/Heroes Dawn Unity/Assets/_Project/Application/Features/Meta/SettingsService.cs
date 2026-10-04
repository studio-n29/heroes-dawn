#nullable enable
using System;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Application.Features.Meta
{
    public enum SettingsCategory { Audio, Graphics, Accessibility, Language, Notifications, Controls, Account, Support }

    /// <summary>Réglages du joueur (stockés dans le profil) ; chaque changement est annoncé pour être appliqué tout de suite.</summary>
    public sealed class SettingsService
    {
        readonly MetaContext _c;

        public SettingsService(MetaContext context) { _c = context; }

        public SettingsSave Current => _c.Meta.settings;

        public void Update(Action<SettingsSave> change)
        {
            change(_c.Meta.settings);
            _c.Touch(MetaArea.Settings);
        }

        /// <summary>Remet une catégorie à ses valeurs d'origine.</summary>
        public void Reset(SettingsCategory category)
        {
            var d = new SettingsSave();
            Update(s =>
            {
                switch (category)
                {
                    case SettingsCategory.Audio:
                        s.master = d.master; s.music = d.music; s.sfx = d.sfx; s.voice = d.voice; s.muteInBackground = d.muteInBackground; break;
                    case SettingsCategory.Graphics:
                        s.quality = d.quality; s.fps60 = d.fps60; s.batterySaver = d.batterySaver; s.fullCombatEffects = d.fullCombatEffects; break;
                    case SettingsCategory.Accessibility:
                        s.reduceFlashes = d.reduceFlashes; s.reduceShakes = d.reduceShakes; s.extendedMiniGameTime = d.extendedMiniGameTime;
                        s.subtitles = d.subtitles; s.textSize = d.textSize; break;
                    case SettingsCategory.Notifications:
                        s.notifications = d.notifications; s.notifTraining = d.notifTraining; s.notifExpedition = d.notifExpedition;
                        s.notifLab = d.notifLab; s.notifRecovered = d.notifRecovered; s.notifShop = d.notifShop;
                        s.notifObjectives = d.notifObjectives; s.notifSeason = d.notifSeason; s.quietHours = d.quietHours; break;
                    case SettingsCategory.Controls:
                        s.showKeys = d.showKeys; break;
                    case SettingsCategory.Language:
                        s.language = d.language; break;
                }
            });
        }
    }
}
