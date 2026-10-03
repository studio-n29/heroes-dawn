using System;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class BackendLayoutTests
    {
        const string Root = "Assets/_Project/Backend";

        [Test, Description("Vérifie que la clé Remote Config « balance » contient un équilibrage lisible — parce que le serveur doit lire les mêmes valeurs que le client")]
        public void RemoteConfig_BalanceEntry_IsValidBalanceJson()
        {
            var rc = File.ReadAllText(Root + "/RemoteConfig/balance.rc");
            var start = rc.IndexOf("\"balance\": {", StringComparison.Ordinal) + "\"balance\": ".Length;
            var obj = rc.Substring(start, rc.IndexOf('}', start) - start + 1);
            Assert.That(BalanceJson.Deserialize(obj).IsSuccess);
        }

        [Test, Description("Vérifie que chaque script Cloud Code exporte son point d'entrée et ses paramètres — parce que la fenêtre Deployment refuse sinon de les déployer")]
        public void CloudCode_EveryScript_ExportsParamsAndHandler()
        {
            var files = Directory.GetFiles(Root + "/CloudCode", "*.js");
            Assert.That(files, Is.Not.Empty);
            foreach (var f in files)
            {
                var js = File.ReadAllText(f);
                StringAssert.Contains("module.exports", js, f);
                StringAssert.Contains("module.exports.params", js, f);
            }
        }

        [Test, Description("Vérifie que balance.rc est importé par l'importeur Remote Config — parce que sinon la fenêtre Deployment ne le liste pas")] // la fenêtre Deployment ne liste que les .rc importés par le ScriptedImporter de Remote Config
        public void RemoteConfig_File_UsesScriptedImporter()
        {
            StringAssert.Contains("ScriptedImporter:", File.ReadAllText(Root + "/RemoteConfig/balance.rc.meta"));
        }

        [Test, Description("Vérifie qu'aucun projet .NET n'existe hors du projet Unity — parce que tout doit se passer dans Unity (décision du 2026-10-02)")] // garde-fou « tout dans Unity »
        public void NoDotnetProjectOutsideUnity()
        {
            var code = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            Assert.That(Directory.GetFiles(code, "*.csproj", SearchOption.AllDirectories)
                .Where(p => !p.Contains("Heroes Dawn Unity")), Is.Empty);
        }
    }
}
