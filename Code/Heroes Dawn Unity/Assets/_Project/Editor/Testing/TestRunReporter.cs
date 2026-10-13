#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Testing
{
    /// <summary>
    /// Écrit un rapport HTML à la fin de chaque exécution du Test Runner (éditeur ou batchmode -runTests) :
    /// Code/docs/test-reports/&lt;date&gt;_&lt;plateforme&gt;.html et latest-&lt;plateforme&gt;.html.
    /// </summary>
    [InitializeOnLoad]
    public sealed class TestRunReporter : ICallbacks
    {
        const string StartKey = "N29.HeroesDawn.TestRunStartedTicks";

        static TestRunReporter()
        {
            ScriptableObject.CreateInstance<TestRunnerApi>().RegisterCallbacks(new TestRunReporter());
        }

        public static string ReportsDirectory =>
            Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../../docs/test-reports"));

        public void RunStarted(ITestAdaptor testsToRun) => SessionState.SetString(StartKey, DateTime.Now.Ticks.ToString());

        public void RunFinished(ITestResultAdaptor result)
        {
            try
            {
                var entries = new List<TestReportEntry>();
                Collect(result, entries);
                var started = long.TryParse(SessionState.GetString(StartKey, ""), out var ticks) ? new DateTime(ticks) : DateTime.Now;
                var (branch, commit) = GitInfo.Read(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../../..")));
                var run = new TestRunInfo
                {
                    Platform = PlatformOf(result), StartedLocal = started, DurationSeconds = result.Duration,
                    Branch = branch, Commit = commit,
                };
                var html = TestReportWriter.Render(run, entries);
                Directory.CreateDirectory(ReportsDirectory);
                File.WriteAllText(Path.Combine(ReportsDirectory, $"{started:yyyy-MM-dd_HH-mm-ss}_{run.Platform}.html"), html);
                File.WriteAllText(Path.Combine(ReportsDirectory, $"latest-{run.Platform}.html"), html);
                Debug.Log($"[Heroes Dawn] Rapport de tests {run.Platform} : {Path.Combine(ReportsDirectory, $"latest-{run.Platform}.html")}");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Heroes Dawn] Rapport de tests impossible : " + e.Message);
            }
        }

        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }

        static void Collect(ITestResultAdaptor r, List<TestReportEntry> into)
        {
            if (r.HasChildren)
            {
                foreach (var c in r.Children) Collect(c, into);
                return;
            }
            if (r.Test.IsSuite) return;
            var stack = (r.StackTrace ?? "").Trim();
            int nl = stack.IndexOf('\n');
            into.Add(new TestReportEntry
            {
                Fixture = FixtureOf(r.Test.FullName),
                Name = r.Test.Name,
                // Cas d'un [TestCase] : la description est portée par la méthode (nœud parent).
                Description = !string.IsNullOrWhiteSpace(r.Test.Description) ? r.Test.Description : r.Test.Parent?.Description ?? "",
                Outcome = r.TestStatus == TestStatus.Passed ? TestOutcome.Passed
                        : r.TestStatus == TestStatus.Failed ? TestOutcome.Failed : TestOutcome.Skipped,
                DurationSeconds = r.Duration,
                Message = r.Message ?? "",
                StackFirstLine = nl < 0 ? stack : stack.Substring(0, nl),
            });
        }

        /// <summary>« Ns.Classe.Methode(args) » → « Ns.Classe ».</summary>
        static string FixtureOf(string fullName)
        {
            int paren = fullName.IndexOf('(');
            var name = paren < 0 ? fullName : fullName.Substring(0, paren);
            int dot = name.LastIndexOf('.');
            return dot < 0 ? name : name.Substring(0, dot);
        }

        static string PlatformOf(ITestResultAdaptor result)
        {
            var mode = result.Test.TestMode;
            return mode == TestMode.PlayMode ? "PlayMode" : "EditMode";
        }
    }
}
