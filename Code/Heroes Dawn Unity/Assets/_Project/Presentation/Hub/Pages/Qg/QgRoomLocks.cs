#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Session;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Déblocage des pièces du QG : Grimoire et Inventaire après le tutoriel ; Registre, Labo et Opérations après les chapitres 1.1, 1.2 et 1.3.
    /// Le réglage de test « tout débloquer » ouvre tout.
    /// </summary>
    public static class QgRoomLocks
    {
        /// <summary>Raison du verrouillage : « tutorial » (tutoriel non terminé) ou l'identifiant de la zone à valider ; nul = accessible.</summary>
        public static string? Reason(HubPage page, ISessionReadModel s, bool tutorialDone, bool unlockAll)
        {
            if (unlockAll) return null;
            switch (page)
            {
                case HubPage.Grimoire:
                case HubPage.Inventory:
                    return tutorialDone ? null : "tutorial";
                case HubPage.Agents: return Need(s, "zone_ny_1_1");
                case HubPage.Lab: return Need(s, "zone_ny_1_2");
                case HubPage.Operations: return Need(s, "zone_ny_1_3");
                default: return null;
            }
        }

        static string? Need(ISessionReadModel s, string zone) => s.CompletedZones.Any(z => z == zone) ? null : zone;
    }
}
