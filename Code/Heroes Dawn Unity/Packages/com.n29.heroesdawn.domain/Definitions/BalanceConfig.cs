namespace N29.HeroesDawn.Definitions
{
    /// <summary>Paramètres d'équilibrage. Valeurs par défaut = tableur 05_Equilibrage.xlsx.</summary>
    public sealed class BalanceConfig
    {
        public int TickMs { get; set; } = 50;
        public int AtbGaugePoints { get; set; } = 1000;
        public int AtbFactorPerSecond { get; set; } = 4;
        public int DefK { get; set; } = 300;
        public int CritBasePercent { get; set; } = 15;
        public int CritMultiplierPercent { get; set; } = 150;
        public int VariancePercent { get; set; } = 5;
        public int VulnerableBonusPercent { get; set; } = 25;
        public int FatiguePerWin { get; set; } = 12;
        public int FatiguePerLossExtra { get; set; } = 15;
        public int FatigueMalusThreshold { get; set; } = 60;
        public int FatigueMalusMaxPercent { get; set; } = 30;
        public int DieFaces { get; set; } = 6;
        public int PmPerBonusDie { get; set; } = 6;
        public int MaxBonusDice { get; set; } = 2;
        public int FogRadius { get; set; } = 2;
        public int StatGrowthPermillePerLevel { get; set; } = 40;
        public int RankEasyLevelOffset { get; set; } = -3;
        public int RankEliteLevelOffset { get; set; } = 3;
        public int RankEasyStatPercent { get; set; } = 80;
        public int RankEliteStatPercent { get; set; } = 130;
        /// <summary>Chance en % qu un evenement narratif resolu fasse trouver un objet.</summary>
        public int EventItemChancePercent { get; set; } = 60;
        /// <summary>Chance en % qu un mini-jeu reussi donne un booster.</summary>
        public int ActivityBoosterChancePercent { get; set; } = 50;

        // ---- Frise chronologique par points : 0 = depart, TimelinePoints = point d action
        /// <summary>Nombre de points entre le depart (point 0) et le point d action.</summary>
        public int TimelinePoints { get; set; } = 10;
        /// <summary>Point de reprise apres une competence legere (emplacement 1).</summary>
        public int TimelineResetLightPoint { get; set; } = 3;
        /// <summary>Point de reprise apres une competence intermediaire (emplacement 2).</summary>
        public int TimelineResetMediumPoint { get; set; } = 2;
        /// <summary>Point de reprise apres une competence lourde (emplacement 3, carte 4).</summary>
        public int TimelineResetHeavyPoint { get; set; } = 0;
        /// <summary>Premier point de la zone de preparation : l action ennemie est annoncee, un critique l annule.</summary>
        public int TimelinePrepZonePoint { get; set; } = 8;
        /// <summary>Premier point de la zone de preparation vue grace a la Perception (precognition).</summary>
        public int TimelinePrecognitionPoint { get; set; } = 5;
        /// <summary>Point ou une unite interrompue (critique en zone de preparation) est renvoyee.</summary>
        public int TimelineInterruptResetPoint { get; set; } = 4;
        /// <summary>Les unites demarrent sur un point tire entre 0 et cette valeur.</summary>
        public int TimelineStartPointMax { get; set; } = 2;
        /// <summary>Duree d un pas de frise (un point), en millisecondes.</summary>
        public int TimelineStepMs { get; set; } = 350;
        /// <summary>Temps maximum du joueur pour choisir au point d action.</summary>
        public int TimelineTurnTimeoutMs { get; set; } = 30000;
        /// <summary>Delai de reflexion de l IA (presentation) : borne basse et haute.</summary>
        public int TimelineAiDelayMinMs { get; set; } = 300;
        public int TimelineAiDelayMaxMs { get; set; } = 700;
        public int PushBackBindPercent { get; set; } = 25;
        public int PushBackShockPercent { get; set; } = 15;
        public int PushBackFreezePercent { get; set; } = 20;
        /// <summary>Brûlure : % des PV max perdus pour une frise complète parcourue (dégâts à chaque pas).</summary>
        public int BurnPercentPerTimeline { get; set; } = 8;

        public int ResourceCreditsMin { get; set; } = 150;
        public int ResourceCreditsMax { get; set; } = 400;
        public int ResourcePrMin { get; set; } = 5;
        public int ResourcePrMax { get; set; } = 15;
        public int RestFatigueReduction { get; set; } = 30;
        public int CreditsEasy { get; set; } = 200;
        public int CreditsNormal { get; set; } = 300;
        public int CreditsElite { get; set; } = 500;
        public int ZoneValidationCredits { get; set; } = 1500;
        public int ZoneValidationPr { get; set; } = 40;
        public int MiniGameCredits { get; set; } = 200;
        public int MiniGamePr { get; set; } = 5;
        // Boutique éphémère de recrutement
        public int ShopResetHour { get; set; } = 9;
        public int ShopWeightAgent { get; set; } = 70;
        public int ShopWeightElite { get; set; } = 25;
        public int ShopWeightLegend { get; set; } = 5;
        public int ShopRerollCost1 { get; set; } = 100;
        public int ShopRerollCost2 { get; set; } = 200;
        public int ShopRerollCost3 { get; set; } = 400;
        public int ShopRerollCost4 { get; set; } = 800;
    }
}
