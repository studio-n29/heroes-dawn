namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Effets sonores. Valeurs sérialisées (AudioCatalog, boutons des prefabs) : ne jamais renuméroter, ajouter à la fin.
    /// Le dossier <c>Audio/Sfx/&lt;nom&gt;/</c> contient les variantes de l'effet (une tirée au hasard).
    /// </summary>
    public enum SfxId
    {
        None = 0,

        // Interface
        UiClick = 1,
        UiBack = 2,
        UiConfirm = 3,
        UiError = 4,
        PopupOpen = 5,
        PopupClose = 6,
        ToastInfo = 7,
        ToastReward = 8,
        ToastWarning = 9,
        LoginStart = 10,

        // Plateau
        DiceShake = 20,
        DiceThrow = 21,
        PawnMove = 22,
        CaseEvent = 23,
        CombatAlert = 24,
        ZoneComplete = 25,
        MiniGameHit = 26,
        MiniGameMiss = 27,

        // QG
        Recruit = 40,
        Train = 41,
        Heal = 42,

        // Combat
        SkillCast = 60,
        Hit = 61,
        CritHit = 62,
        HealFx = 63,
        Shield = 64,
        Miss = 65,
        Status = 66,
        KnockOut = 67,
        Combo = 68,
        Victory = 69,
        Defeat = 70,
        AllyReady = 71,
    }
}
