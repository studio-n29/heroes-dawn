using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Illustrations des agents (Resources/Illustrations/Agents) et des cryptides (Resources/Illustrations/Cryptides).</summary>
    public sealed class AgentIllustrationTests
    {
        static string DataPath(string relative) => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, relative));

        static string[] AgentIds()
            => Regex.Matches(File.ReadAllText(DataPath("_Project/Resources/Content/agents.json")), "\"id\":\\s*\"([a-z0-9_]+)\"").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToArray();

        [Test, Description("Vérifie que chaque agent d'agents.json a sa propre illustration 16:9 (1280 × 720) dans Resources/Illustrations/Agents et que AgentArt.For rend cette illustration, pas une image de remplacement — parce que chaque agent doit avoir son visage")]
        public void EveryAgent_HasItsOwnIllustration()
        {
            var ids = AgentIds();
            Assert.That(ids.Length, Is.EqualTo(32), "32 agents jouables (30 agents de zone + Agent D et Agent M)");
            foreach (var id in ids)
            {
                var own = Resources.Load<Sprite>("Illustrations/Agents/" + id);
                Assert.That(own, Is.Not.Null, id);
                Assert.That((own!.rect.width, own.rect.height), Is.EqualTo((1280f, 720f)), id);
                Assert.That(AgentArt.For(id), Is.SameAs(own), id + " : AgentArt.For rend l'illustration de l'agent");
            }
        }

        [Test, Description("Vérifie qu'aucune illustration d'agent n'est orpheline (un fichier sans agent) — parce qu'une image sans agent est un oubli ou une faute de nom")]
        public void NoOrphanAgentIllustration()
        {
            var ids = AgentIds();
            var files = Directory.GetFiles(DataPath("_Project/Resources/Illustrations/Agents"), "*.jpg").Select(Path.GetFileNameWithoutExtension).ToArray();
            Assert.That(files, Is.EquivalentTo(ids));
        }

        [Test, Description("Vérifie que les 9 illustrations de cryptides illustrés (S1 à S7, S9, S10 ; le Jersey Devil S8 reste à dessiner) sont chargeables en sprites 1280 × 720 — parce qu'elles serviront aux ennemis de Salem")]
        public void CryptideIllustrations_AreImportedAsSprites()
        {
            foreach (var id in new[] { "cryptide_s1_kraken", "cryptide_s2_mante", "cryptide_s3_caprin", "cryptide_s4_phalene", "cryptide_s5_famine",
                                       "cryptide_s6_pied_grand", "cryptide_s7_lupin", "cryptide_s9_pierre", "cryptide_s10_suceur" })
            {
                var sprite = Resources.Load<Sprite>("Illustrations/Cryptides/" + id);
                Assert.That(sprite, Is.Not.Null, id);
                Assert.That((sprite!.rect.width, sprite.rect.height), Is.EqualTo((1280f, 720f)), id);
            }
        }

        [Test, Description("Vérifie que les sources d'art (03_Art_UI_UX/Illustrations/Agents) contiennent un fichier par agent — parce que ce dossier garde les originaux en pleine résolution")]
        public void ArtSources_HaveOneFilePerAgent()
        {
            string dir = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../../../03_Art_UI_UX/Illustrations/Agents"));
            var files = Directory.GetFiles(dir, "*.jpg").Select(Path.GetFileNameWithoutExtension).ToArray();
            Assert.That(files, Is.SupersetOf(AgentIds()));
        }

        [Test, Description("Vérifie qu'un agent inconnu ou un personnage non jouable garde l'illustration de remplacement stable (pas de null) — parce que les dialogues utilisent aussi AgentArt pour des personnages comme Rayner")]
        public void UnknownCharacter_KeepsAStableFallback()
        {
            Assert.That(AgentArt.For("rayner"), Is.Not.Null);
            Assert.That(AgentArt.For("quelqu_un_d_inconnu"), Is.Not.Null);
            Assert.That(AgentArt.For("quelqu_un_d_inconnu"), Is.SameAs(AgentArt.For("quelqu_un_d_inconnu")));
            Assert.That(AgentArt.For(""), Is.Null);
        }
    }
}
