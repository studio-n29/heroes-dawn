using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Migration SQL de la table hd_profiles (lot I) : sécurité par ligne, limite de taille, aucun contact avec les tables d'un autre outil.</summary>
    public sealed class HdProfilesMigrationTests
    {
        static string Sql => File.ReadAllText(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "_Project/Backend/Supabase/001_hd_profiles.sql")));

        [Test, Description("Vérifie que la migration active la sécurité par ligne, crée exactement trois politiques (lecture, insertion, mise à jour) toutes limitées à auth.uid() = user_id, et aucune politique de suppression — parce qu'un joueur ne doit voir et écrire que sa ligne")]
        public void Migration_HasRowLevelSecurity_AndOnlyOwnerPolicies()
        {
            var sql = Sql;
            Assert.That(sql, Does.Contain("enable row level security"));
            var policies = Regex.Matches(sql, "create policy", RegexOptions.IgnoreCase);
            Assert.That(policies.Count, Is.EqualTo(3));
            Assert.That(Regex.Matches(sql, @"auth\.uid\(\) = user_id").Count, Is.GreaterThanOrEqualTo(4), "select, insert, update (using et with check)");
            Assert.That(sql, Does.Not.Contain("for delete"));
            Assert.That(sql, Does.Not.Contain("to anon"), "aucun accès sans connexion");
            Assert.That(sql, Does.Not.Contain("to public"));
        }

        [Test, Description("Vérifie que la migration limite la taille du profil à 512 Ko (524288 octets) par une contrainte — parce qu'une ligne énorme ne doit pas être acceptée")]
        public void Migration_LimitsTheProfileSize()
        {
            Assert.That(Sql, Does.Contain("524288"));
            Assert.That(Sql, Does.Contain("pg_column_size(profile)"));
        }

        [Test, Description("Vérifie que la migration ne touche aucune table de l'autre outil (hq_vault) et que toute table créée a le préfixe hd_ — parce que ces tables appartiennent à un autre projet")]
        public void Migration_NeverTouchesTheHqVaultTables_AndUsesTheHdPrefix()
        {
            var sql = Sql;
            Assert.That(sql, Does.Not.Contain("hq_vault"));
            foreach (Match m in Regex.Matches(sql, @"create table (?:if not exists )?public\.(\w+)", RegexOptions.IgnoreCase))
                Assert.That(m.Groups[1].Value, Does.StartWith("hd_"));
        }

        [Test, Description("Vérifie que la table est rattachée aux utilisateurs avec suppression en cascade, que la date de dernière écriture est posée par un déclencheur côté serveur (pas par le client) et que la fonction fixe son search_path — parce que le client ne doit pas pouvoir truquer la date de conflit")]
        public void Migration_Cascades_AndTheServerSetsUpdatedAt()
        {
            var sql = Sql;
            Assert.That(sql, Does.Contain("references auth.users (id) on delete cascade"));
            Assert.That(sql, Does.Contain("create trigger hd_profiles_touch"));
            Assert.That(sql, Does.Contain("new.updated_at := now()"));
            Assert.That(sql, Does.Contain("set search_path = ''"));
        }
    }
}
