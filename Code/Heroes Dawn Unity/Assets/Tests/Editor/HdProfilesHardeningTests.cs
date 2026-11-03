using System.IO;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Migration de durcissement de hd_profiles (lot L5 de la 0.15.0) : droits réduits, contraintes de forme, déclencheur non appelable.</summary>
    public sealed class HdProfilesHardeningTests
    {
        static string Sql => File.ReadAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "_Project/Backend/Supabase/002_hd_profiles_durcissement.sql")));

        [Test, Description("Vérifie que la migration retire tous les droits aux rôles anonyme et connecté, puis n'accorde que lecture, insertion et mise à jour — parce qu'un client ne doit pas supprimer une sauvegarde")]
        public void Hardening_ReducesGrants_ToSelectInsertUpdate()
        {
            var sql = Sql;
            Assert.That(sql, Does.Contain("revoke all on public.hd_profiles from anon, authenticated"));
            Assert.That(sql, Does.Contain("grant select, insert, update on public.hd_profiles to authenticated"));
            Assert.That(sql, Does.Not.Contain("delete"));
        }

        [Test, Description("Vérifie que le profil doit être un objet JSON et que la version de schéma est positive — parce qu'une sauvegarde mal formée ne doit pas être acceptée")]
        public void Hardening_ChecksTheShapeOfTheProfile()
        {
            Assert.That(Sql, Does.Contain("jsonb_typeof(profile) = 'object'"));
            Assert.That(Sql, Does.Contain("schema_version >= 1"));
        }

        [Test, Description("Vérifie que la fonction de déclencheur n'est pas appelable par l'API et que la sécurité par ligne s'applique aussi au propriétaire — parce que la fonction ne doit servir qu'au déclencheur")]
        public void Hardening_LocksTheTriggerAndForcesRls()
        {
            Assert.That(Sql, Does.Contain("revoke execute on function public.hd_profiles_touch() from public, anon, authenticated"));
            Assert.That(Sql, Does.Contain("force row level security"));
        }
    }
}
