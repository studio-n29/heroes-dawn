#nullable enable
namespace N29.HeroesDawn.Infrastructure.Supabase
{
    public enum AccountBackendKind { Simulated, Supabase }

    /// <summary>Choix du fournisseur de compte et de sauvegarde cloud : Supabase quand la configuration est valide, sinon les simulateurs en mémoire.</summary>
    public static class AccountBackend
    {
        public static AccountBackendKind Choose(SupabaseConfig? config) => config != null && config.IsValid ? AccountBackendKind.Supabase : AccountBackendKind.Simulated;
    }
}
