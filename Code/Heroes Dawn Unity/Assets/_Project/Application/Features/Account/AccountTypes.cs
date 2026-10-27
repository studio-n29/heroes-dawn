#nullable enable
using System;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Application.Features.Account
{
    public enum AccountState { Guest, Connected }

    /// <summary>Choix du joueur quand l'appareil et le compte ont chacun de la progression.</summary>
    public enum ChoiceKind { KeepDevice, KeepAccount }

    /// <summary>Résultat d'un rattachement : profil local envoyé, profil du compte récupéré, choix à faire (rien n'est écrit), ou connecté sans profil local encore chargé (écran titre : le rattachement a lieu après « Jouer »).</summary>
    public enum LinkStatus { Uploaded, Pulled, NeedsChoice, Connected }

    /// <summary>Les chiffres montrés dans le choix de progression : niveau le plus haut, Crédits, zones terminées, dernière partie.</summary>
    public sealed class ProfileSummary
    {
        public int Level { get; }
        public int Credits { get; }
        public int ZonesCleared { get; }
        public DateTime LastSeenUtc { get; }
        /// <summary>Profil « neuf » : aucune zone terminée et tutoriel non fini. Un profil neuf peut être remplacé sans demander.</summary>
        public bool IsFresh { get; }

        ProfileSummary(int level, int credits, int zones, DateTime lastSeen, bool fresh)
        {
            Level = level; Credits = credits; ZonesCleared = zones; LastSeenUtc = lastSeen; IsFresh = fresh;
        }

        public static ProfileSummary Of(ProfileData p)
        {
            int level = 0;
            foreach (var m in p.team) level = Math.Max(level, m.level);
            int zones = p.completedZones?.Count ?? 0;
            var seen = p.lastSeenUtcTicks > 0 && p.lastSeenUtcTicks < DateTime.MaxValue.Ticks ? new DateTime(p.lastSeenUtcTicks, DateTimeKind.Utc) : DateTime.MinValue;
            bool fresh = zones == 0 && !(p.meta?.tutorial.done ?? false);
            return new ProfileSummary(level, p.credits, zones, seen, fresh);
        }
    }

    public sealed class LinkOutcome
    {
        public LinkStatus Status { get; }
        public ProfileSummary? Local { get; }
        public ProfileSummary? Cloud { get; }
        public LinkOutcome(LinkStatus status, ProfileSummary? local = null, ProfileSummary? cloud = null) { Status = status; Local = local; Cloud = cloud; }
    }

    /// <summary>Prévenu après chaque écriture du profil local (le <see cref="Session.SaveScheduler"/> appelle cet observateur).</summary>
    public interface ILocalSaveObserver
    {
        void NotifyLocalSaved();
    }
}
