#nullable enable
namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Une quête principale (zone de la campagne) : 0 verrouillée, 1 en cours, 2 terminée.</summary>
    public struct MainQuestRowData
    {
        public string Title;
        public string Detail;
        public string State;
        public int Status;
    }

    /// <summary>Une quête secondaire : progression, récompense et bouton « Réclamer ».</summary>
    public struct SideQuestRowData
    {
        public string Id;
        public string Title;
        public string Description;
        public string ProgressText;
        public float Progress01;
        public string RewardText;
        public bool Claimable;
        public bool Claimed;
        public string ClaimLabel;
    }

    public struct QuestLogData
    {
        public string Title;
        public string MainTitle;
        public string SideTitle;
        public string SideEmpty;
        /// <summary>Titre de la section « Activités du jour ».</summary>
        public string ActivityTitle;
        public string ActivityEmpty;
        /// <summary>Activités du jour (même présentation qu'une quête secondaire ; identifiant act:n).</summary>
        public SideQuestRowData[] Activities;
        public string Close;
        public MainQuestRowData[] Main;
        public SideQuestRowData[] Side;
    }
}
