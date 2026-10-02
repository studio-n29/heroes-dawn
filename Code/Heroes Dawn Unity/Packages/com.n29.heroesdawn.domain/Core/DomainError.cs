namespace N29.HeroesDawn.Core
{
    /// <summary>Échec métier attendu (code + clé de localisation).</summary>
    public sealed class DomainError
    {
        public string Code { get; }
        public string LocKey { get; }
        public DomainError(string code, string locKey) { Code = code; LocKey = locKey; }
        public override string ToString() => Code;
    }
}
