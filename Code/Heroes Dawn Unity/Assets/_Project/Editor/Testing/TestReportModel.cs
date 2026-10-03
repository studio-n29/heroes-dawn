#nullable enable
using System;

namespace N29.HeroesDawn.EditorTools.Testing
{
    public enum TestOutcome { Passed, Failed, Skipped }

    /// <summary>Une ligne du rapport : un test exécuté.</summary>
    public sealed class TestReportEntry
    {
        public string Fixture { get; set; } = "";
        public string Name { get; set; } = "";
        /// <summary>« Vérifie que … — parce que … » (attribut [Description] du test).</summary>
        public string Description { get; set; } = "";
        public TestOutcome Outcome { get; set; }
        public double DurationSeconds { get; set; }
        public string Message { get; set; } = "";
        public string StackFirstLine { get; set; } = "";
    }

    /// <summary>Contexte d'une exécution : plateforme, date, durée, version du code.</summary>
    public sealed class TestRunInfo
    {
        public string Platform { get; set; } = "";
        public DateTime StartedLocal { get; set; }
        public double DurationSeconds { get; set; }
        public string Branch { get; set; } = "?";
        public string Commit { get; set; } = "?";
    }
}
