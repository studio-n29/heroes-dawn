#nullable enable
namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Vue d'ensemble de la méta-progression pour le QG et les pastilles de la barre de navigation.</summary>
    public sealed class MetaOverview
    {
        public PassProgress Pass { get; }
        public DailyMissions Daily { get; }
        public SecondaryObjectives Secondary { get; }
        public Expeditions Operations { get; }
        public ArchiveBook Archives { get; }
        public Boutique Boutique { get; }
        public QuestLog Quests { get; }

        public MetaOverview(PassProgress pass, DailyMissions daily, SecondaryObjectives secondary, Expeditions operations,
                            ArchiveBook archives, Boutique boutique, QuestLog quests)
        {
            Pass = pass; Daily = daily; Secondary = secondary; Operations = operations; Archives = archives; Boutique = boutique; Quests = quests;
        }

        /// <summary>Récompenses à récupérer dans le pass, les missions du jour ou le cadeau du jour (réclamable dans le Pass).</summary>
        public int PassBadge => Pass.ClaimableCount() + Daily.ClaimableCount() + (Boutique.DailyGiftAvailable ? 1 : 0);

        public int OperationsBadge => Operations.ReadyCount();
        /// <summary>Pas de pastille tant que l'onglet Atelier n'est pas refait (lot UI).</summary>
        public int LabBadge => 0;
        public int ArchivesBadge => Archives.UnreadCount();
        public int QuestsBadge => Quests.ClaimableCount();
    }
}
