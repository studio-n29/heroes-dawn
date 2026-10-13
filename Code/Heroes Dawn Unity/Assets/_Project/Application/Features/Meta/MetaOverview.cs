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
        public ResearchLab Lab { get; }
        public ArchiveBook Archives { get; }
        public Boutique Boutique { get; }
        public QuestLog Quests { get; }

        public MetaOverview(PassProgress pass, DailyMissions daily, SecondaryObjectives secondary, Expeditions operations,
                            ResearchLab lab, ArchiveBook archives, Boutique boutique, QuestLog quests)
        {
            Pass = pass; Daily = daily; Secondary = secondary; Operations = operations; Lab = lab; Archives = archives; Boutique = boutique; Quests = quests;
        }

        /// <summary>Récompenses à récupérer dans le pass ou les missions du jour.</summary>
        public int PassBadge => Pass.ClaimableCount() + Daily.ClaimableCount();

        /// <summary>1 si le cadeau du jour n'est pas pris.</summary>
        public int ShopBadge => Boutique.DailyGiftAvailable ? 1 : 0;

        public int OperationsBadge => Operations.ReadyCount();
        public int LabBadge => Lab.IsReady ? 1 : 0;
        public int ArchivesBadge => Archives.UnreadCount();
        public int QuestsBadge => Quests.ClaimableCount();
    }
}
