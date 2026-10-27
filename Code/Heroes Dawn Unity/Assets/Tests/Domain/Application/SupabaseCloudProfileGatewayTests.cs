using System;
using System.Linq;
using System.Threading;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Infrastructure.Supabase;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Client de sauvegarde cloud (PostgREST) avec un faux client HTTP : requêtes construites, conflits, erreurs.</summary>
    public sealed class SupabaseCloudProfileGatewayTests
    {
        const string Url = "https://projet.supabase.co";
        static readonly CancellationToken Ct = CancellationToken.None;

        FakeSupabaseHttp _http = null!;
        MemoryTokenStore _tokens = null!;
        SupabaseCloudProfileGateway _cloud = null!;

        [SetUp]
        public void SetUp()
        {
            _http = new FakeSupabaseHttp();
            _tokens = new MemoryTokenStore();
            _tokens.Save(new SupabaseSession { AccessToken = "JETON", RefreshToken = "R", ExpiresAtUnix = 4102444800, UserId = "11111111-2222-3333-4444-555555555555", Email = "moi@exemple.fr", DisplayName = "Moi" });
            var config = new SupabaseConfig(Url, "cle-anon", "heroesdawn://auth");
            var accounts = new SupabaseAccountGateway(config, _http, _tokens, new FixedClock());
            _cloud = new SupabaseCloudProfileGateway(config, _http, accounts);
        }

        static ProfileData Profile(int credits = 42) => new ProfileData { credits = credits, schemaVersion = 4 };

        static string Row(int credits, string updatedAt = "2026-10-28T12:00:00.123456+00:00")
            => "{\"profile\":" + JsonUtility.ToJson(Profile(credits)) + ",\"schema_version\":4,\"updated_at\":\"" + updatedAt + "\"}";

        [Test, Description("Vérifie que la lecture appelle GET /rest/v1/hd_profiles avec la clé publique et le jeton du joueur, et lit le profil et la date de dernière écriture — parce que c'est le contrat avec PostgREST")]
        public void Get_BuildsTheRequest_AndParsesTheRow()
        {
            _http.Enqueue(200, "[" + Row(77) + "]");
            var r = _cloud.GetAsync(Ct).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(r.Value!.Profile.credits, Is.EqualTo(77));
            Assert.That(r.Value.UpdatedAt, Is.EqualTo("2026-10-28T12:00:00.123456+00:00"));
            var req = _http.Requests.Single();
            Assert.That((req.Method, req.Url), Is.EqualTo(("GET", Url + "/rest/v1/hd_profiles?select=profile,schema_version,updated_at&limit=1")));
            Assert.That(req.Headers["apikey"], Is.EqualTo("cle-anon"));
            Assert.That(req.Headers["Authorization"], Is.EqualTo("Bearer JETON"));
        }

        [Test, Description("Vérifie qu'une réponse sans ligne rend « pas de profil en ligne » (succès, valeur nulle) — parce qu'un nouveau compte n'a pas encore de sauvegarde")]
        public void Get_EmptyRows_ReturnsNull()
        {
            _http.Enqueue(200, "[]");
            var r = _cloud.GetAsync(Ct).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(r.Value, Is.Null);
        }

        [Test, Description("Vérifie qu'un corps mal formé ou vide, un 5xx ou une panne réseau rendent « sauvegarde en ligne indisponible » sans exception — parce que le serveur peut mal répondre")]
        public void Get_MalformedBody_5xx_AndNetwork_ReturnCloudError()
        {
            foreach (var body in new[] { "pas du json", "", "[{\"profile\":", "{}" })
            {
                _http.Enqueue(200, body);
                var r = _cloud.GetAsync(Ct).GetAwaiter().GetResult();
                Assert.That(r.IsFailure, Is.True, "corps : " + body);
                Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_cloud"), "corps : " + body);
            }
            _http.Enqueue(503, "oups");
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_cloud"));
            _http.EnqueueNetworkError();
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_cloud"));
        }

        [Test, Description("Vérifie que la première écriture appelle POST avec l'identifiant du joueur, le profil et la version de schéma, rend la nouvelle date, et que 409 (ligne déjà là) est un conflit — parce que la première écriture ne doit jamais écraser")]
        public void Upsert_FirstWrite_Posts_AndAConflictOn409()
        {
            _http.Enqueue(201, "[" + Row(42, "2026-10-28T13:00:00+00:00") + "]");
            var ok = _cloud.UpsertAsync(Profile(), null, Ct).GetAwaiter().GetResult().Value;
            Assert.That((ok.Status, ok.UpdatedAt), Is.EqualTo((CloudWriteStatus.Ok, "2026-10-28T13:00:00+00:00")));
            var req = _http.Requests.Single();
            Assert.That((req.Method, req.Url), Is.EqualTo(("POST", Url + "/rest/v1/hd_profiles")));
            Assert.That(req.Body, Does.Contain("\"user_id\":\"11111111-2222-3333-4444-555555555555\"").And.Contain("\"profile\":{").And.Contain("\"schema_version\":4"));
            Assert.That(req.Headers["Prefer"], Is.EqualTo("return=representation"));

            _http.Enqueue(409, "{\"code\":\"23505\"}");
            Assert.That(_cloud.UpsertAsync(Profile(), null, Ct).GetAwaiter().GetResult().Value.Status, Is.EqualTo(CloudWriteStatus.Conflict));
        }

        [Test, Description("Vérifie que l'écriture conditionnelle appelle PATCH filtré sur le joueur ET sur la date attendue (encodée, le « + » du fuseau devient %2B), et rend la nouvelle date — parce que c'est ce filtre qui empêche d'écraser une version plus récente")]
        public void Upsert_Patch_UsesTheExpectedUpdatedAtFilter()
        {
            _http.Enqueue(200, "[" + Row(50, "2026-10-28T14:00:00+00:00") + "]");
            var r = _cloud.UpsertAsync(Profile(50), "2026-10-28T12:00:00.123456+00:00", Ct).GetAwaiter().GetResult().Value;
            Assert.That((r.Status, r.UpdatedAt), Is.EqualTo((CloudWriteStatus.Ok, "2026-10-28T14:00:00+00:00")));
            var req = _http.Requests.Single();
            Assert.That(req.Method, Is.EqualTo("PATCH"));
            Assert.That(req.Url, Is.EqualTo(Url + "/rest/v1/hd_profiles?user_id=eq.11111111-2222-3333-4444-555555555555&updated_at=eq.2026-10-28T12%3A00%3A00.123456%2B00%3A00"));
            Assert.That(req.Body, Does.Not.Contain("user_id"), "la mise à jour ne réécrit pas l'identifiant");
        }

        [Test, Description("Vérifie qu'une mise à jour qui ne touche aucune ligne (tableau vide : la date a changé entre-temps) est un conflit, et qu'aucune erreur n'est levée — parce que le jeu doit alors proposer le choix, sans rien perdre")]
        public void Upsert_EmptyArray_IsAConflict()
        {
            _http.Enqueue(200, "[]");
            var r = _cloud.UpsertAsync(Profile(), "2026-10-28T12:00:00+00:00", Ct).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(r.Value.Status, Is.EqualTo(CloudWriteStatus.Conflict));
        }

        [Test, Description("Vérifie que sans session le client rend « session expirée » sans aucune requête, et qu'un 401 du serveur rend aussi « session expirée » — parce qu'il faut alors se reconnecter")]
        public void NoSession_And401_ReturnSessionError()
        {
            _http.Enqueue(401, "{\"message\":\"JWT expired\"}");
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_session"));
            _tokens.Clear();
            int before = _http.Requests.Count;
            Assert.That(_cloud.GetAsync(Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_session"));
            Assert.That(_cloud.UpsertAsync(Profile(), null, Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_session"));
            Assert.That(_http.Requests.Count, Is.EqualTo(before));
        }

        [Test, Description("Vérifie qu'un profil de plus de 512 Ko est refusé avant tout envoi — parce que la base refuserait de toute façon cette ligne et qu'il vaut mieux ne rien envoyer")]
        public void OversizedProfile_IsRejectedWithoutARequest()
        {
            var big = Profile();
            for (int i = 0; i < 30000; i++) big.completedZones.Add("zone_tres_longue_pour_gonfler_le_profil_" + i);
            var r = _cloud.UpsertAsync(big, null, Ct).GetAwaiter().GetResult();
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_cloud"));
            Assert.That(_http.Requests, Is.Empty);
        }

        [Test, Description("Vérifie que les erreurs réseau ou 5xx à l'écriture rendent « sauvegarde en ligne indisponible » et jamais un faux succès — parce qu'un échec réseau ne doit pas être pris pour une sauvegarde")]
        public void Upsert_NetworkAnd5xx_AreErrors_NotSuccess()
        {
            _http.EnqueueNetworkError();
            Assert.That(_cloud.UpsertAsync(Profile(), "x", Ct).GetAwaiter().GetResult().IsFailure, Is.True);
            _http.Enqueue(500, "{}");
            Assert.That(_cloud.UpsertAsync(Profile(), null, Ct).GetAwaiter().GetResult().IsFailure, Is.True);
            _http.Enqueue(200, "oups");
            Assert.That(_cloud.UpsertAsync(Profile(), "x", Ct).GetAwaiter().GetResult().IsFailure, Is.True, "réponse illisible : ni succès ni conflit présumé");
        }
    }
}
