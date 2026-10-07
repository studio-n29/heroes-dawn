namespace N29.HeroesDawn.Definitions
{
    /// <summary>Type de décor : change les couleurs et la répartition des éléments des niveaux d'activité (et le fond de carte, sous-projet E).</summary>
    public enum Biome { City, Harbor, Industrial, Forest, Snow, Desert }

    /// <summary>Météo / moment : teinte les couleurs plates des niveaux.</summary>
    public enum Weather { Day, Night, Snow, Rain, Fog }

    /// <summary>Environnement d'un plateau (zone) : les activités lancées depuis ses cases s'y adaptent.</summary>
    public readonly struct ZoneEnvironment
    {
        public Biome Biome { get; }
        public Weather Weather { get; }
        public ZoneEnvironment(Biome biome, Weather weather) { Biome = biome; Weather = weather; }
        public static ZoneEnvironment Default => new ZoneEnvironment(Biome.City, Weather.Day);
        public override string ToString() => Biome + "/" + Weather;
    }
}
