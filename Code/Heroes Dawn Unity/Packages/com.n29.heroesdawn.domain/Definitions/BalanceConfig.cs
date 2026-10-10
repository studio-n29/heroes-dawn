namespace N29.HeroesDawn.Definitions
{
    /// <summary>Paramètres d'équilibrage. Valeurs par défaut = tableur 05_Equilibrage.xlsx.</summary>
    public sealed class BalanceConfig
    {
        public int TickMs { get; set; } = 50;
        public int AtbGaugePoints { get; set; } = 1000;
        public int AtbFactorPerSecond { get; set; } = 4;
        /// <summary>Vitesse de la frise en % (100 = vitesse d'origine) : plus bas, plus de temps de réflexion.</summary>
        public int TimelineSpeedPercent { get; set; } = 70;
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
        public int ComboCooldownActions { get; set; } = 3;

        // ---- Frise chronologique (Timeline) : positions en % de la jauge, 100 = point d action
        /// <summary>Position après une compétence légère (A1).</summary>
        public int TimelineLightResetPercent { get; set; } = 30;
        /// <summary>Position après une compétence intermédiaire (A2).</summary>
        public int TimelineMediumResetPercent { get; set; } = 15;
        /// <summary>Position après une compétence lourde ou ultime (A3).</summary>
        public int TimelineHeavyResetPercent { get; set; } = 0;
        /// <summary>Position après un combo : négative = temps de canalisation à rattraper.</summary>
        public int TimelineComboResetPercent { get; set; } = -15;
        /// <summary>Début de la zone de préparation (80-100 %) : l action est annoncée, un critique l annule.</summary>
        public int TimelinePrepZonePercent { get; set; } = 80;
        /// <summary>Début de la zone de préparation vue grâce à la Perception (précognition) : une unité Perception vivante révèle l action ennemie plus tôt.</summary>
        public int TimelinePrecognitionPercent { get; set; } = 50;
        /// <summary>Position où une unité interrompue (critique en zone de préparation) est renvoyée.</summary>
        public int TimelineInterruptResetPercent { get; set; } = 40;
        public int PushBackBindPercent { get; set; } = 25;
        public int PushBackShockPercent { get; set; } = 15;
        public int PushBackFreezePercent { get; set; } = 20;
        /// <summary>Brûlure : % des PV max perdus pour une frise complète parcourue (dégâts à chaque pas).</summary>
        public int BurnPercentPerTimeline { get; set; } = 8;

        // ---- Combo à proximité (gratuit)
        /// <summary>Quand un allié est à 0, un partenaire de combo valide doit avoir au moins cette position (%) pour lancer le combo.</summary>
        public int ComboSyncWindowPercent { get; set; } = 60;
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
