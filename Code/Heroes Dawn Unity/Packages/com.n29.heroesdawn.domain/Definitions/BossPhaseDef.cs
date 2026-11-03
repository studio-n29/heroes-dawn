namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Palier d'un boss : quand ses PV passent sous <see cref="ThresholdPercent"/> % de son maximum, il entre dans la phase suivante
    /// et gagne (ou perd) ces pourcentages de force, de défense et de vitesse, de façon permanente.
    /// </summary>
    public sealed class BossPhaseDef
    {
        /// <summary>Seuil en % des PV maximum (1 à 99) ; la phase s'ouvre dès que les PV tombent à ce niveau ou en dessous.</summary>
        public int ThresholdPercent { get; }
        /// <summary>Clé de localisation du nom de la phase (ex. « Fureur du caïman »).</summary>
        public string NameKey { get; }
        public int AtkPercent { get; }
        public int DefPercent { get; }
        public int SpdPercent { get; }

        public BossPhaseDef(int thresholdPercent, string nameKey, int atkPercent, int defPercent, int spdPercent)
        {
            ThresholdPercent = thresholdPercent; NameKey = nameKey; AtkPercent = atkPercent; DefPercent = defPercent; SpdPercent = spdPercent;
        }
    }
}
