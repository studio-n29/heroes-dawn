#nullable enable
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace N29.HeroesDawn.EditorTools.Testing
{
    /// <summary>Rend une exécution de tests en page HTML autonome (CSS en ligne, thèmes clair et sombre).</summary>
    public static class TestReportWriter
    {
        public static string Render(TestRunInfo run, IReadOnlyList<TestReportEntry> entries)
        {
            var inv = CultureInfo.InvariantCulture;
            int passed = entries.Count(e => e.Outcome == TestOutcome.Passed);
            int failed = entries.Count(e => e.Outcome == TestOutcome.Failed);
            int skipped = entries.Count(e => e.Outcome == TestOutcome.Skipped);
            bool ok = failed == 0;

            var sb = new StringBuilder();
            sb.Append("<!doctype html><html lang=\"fr\"><head><meta charset=\"utf-8\">")
              .Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">")
              .Append("<title>Tests ").Append(E(run.Platform)).Append("</title><style>").Append(Css).Append("</style></head><body>");

            sb.Append("<header><h1>Heroes Dawn — tests ").Append(E(run.Platform)).Append("</h1>")
              .Append("<p class=\"meta\">").Append(run.StartedLocal.ToString("dd/MM/yyyy HH:mm:ss", inv))
              .Append(" · ").Append(run.DurationSeconds.ToString("0.0", inv)).Append(" s · branche <code>").Append(E(run.Branch))
              .Append("</code> · commit <code>").Append(E(run.Commit)).Append("</code></p>")
              .Append("<p class=\"summary ").Append(ok ? "ok" : "ko").Append("\">")
              .Append(entries.Count).Append(" tests : ")
              .Append(passed).Append(passed > 1 ? " réussis" : " réussi").Append(" · ")
              .Append(failed).Append(failed > 1 ? " échoués" : " échoué").Append(" · ")
              .Append(skipped).Append(skipped > 1 ? " ignorés" : " ignoré").Append("</p></header><main>");

            foreach (var group in entries.GroupBy(e => e.Fixture).OrderBy(g => g.Key))
            {
                int gFailed = group.Count(e => e.Outcome == TestOutcome.Failed);
                sb.Append("<section><h2>").Append(E(ShortName(group.Key))).Append(" <span class=\"count\">")
                  .Append(group.Count() - gFailed).Append('/').Append(group.Count()).Append("</span></h2>")
                  .Append("<table><thead><tr><th>Résultat</th><th>Test</th><th>Ce qu'il vérifie et pourquoi</th><th>Durée</th></tr></thead><tbody>");
                foreach (var e in group.OrderBy(x => x.Name))
                {
                    string mark = e.Outcome == TestOutcome.Passed ? "✔" : e.Outcome == TestOutcome.Failed ? "✘" : "–";
                    string cls = e.Outcome == TestOutcome.Passed ? "pass" : e.Outcome == TestOutcome.Failed ? "fail" : "skip";
                    sb.Append("<tr class=\"").Append(cls).Append("\"><td class=\"mark\">").Append(mark).Append("</td><td><code>")
                      .Append(E(e.Name)).Append("</code></td><td>")
                      .Append(string.IsNullOrWhiteSpace(e.Description) ? "<em>sans description</em>" : E(e.Description));
                    if (e.Outcome != TestOutcome.Passed && !string.IsNullOrWhiteSpace(e.Message))
                    {
                        sb.Append("<div class=\"why\">").Append(E(e.Message.Trim()));
                        if (!string.IsNullOrWhiteSpace(e.StackFirstLine)) sb.Append("<br><code>").Append(E(e.StackFirstLine.Trim())).Append("</code>");
                        sb.Append("</div>");
                    }
                    sb.Append("</td><td class=\"dur\">").Append((e.DurationSeconds * 1000).ToString("0", inv)).Append(" ms</td></tr>");
                }
                sb.Append("</tbody></table></section>");
            }
            sb.Append("</main></body></html>");
            return sb.ToString();
        }

        static string ShortName(string fixture)
        {
            int i = fixture.LastIndexOf('.');
            return i < 0 ? fixture : fixture.Substring(i + 1);
        }

        static string E(string? s) => (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

        const string Css =
            ":root{--bg:#f7f7f9;--fg:#1d2230;--muted:#5b6475;--card:#fff;--line:#e3e6ec;--ok:#1f8a4c;--ko:#c62f35;--skip:#8a6d1f}" +
            "@media (prefers-color-scheme: dark){:root{--bg:#14171f;--fg:#e8eaf0;--muted:#9aa3b5;--card:#1d222d;--line:#2c3342;--ok:#4cc38a;--ko:#ff6b6b;--skip:#e0b84c}}" +
            "body{margin:0;padding:24px 16px;background:var(--bg);color:var(--fg);font:15px/1.5 system-ui,-apple-system,Segoe UI,sans-serif}" +
            "header,main{max-width:1100px;margin:0 auto}h1{font-size:22px;margin:0 0 4px}.meta{color:var(--muted);margin:0 0 12px}" +
            ".summary{font-weight:600;padding:10px 14px;border-radius:8px;background:var(--card);border:1px solid var(--line)}" +
            ".summary.ok{border-left:5px solid var(--ok)}.summary.ko{border-left:5px solid var(--ko)}" +
            "section{margin-top:22px}h2{font-size:17px;margin:0 0 8px}.count{color:var(--muted);font-weight:400}" +
            "table{width:100%;border-collapse:collapse;background:var(--card);border:1px solid var(--line);border-radius:8px;overflow:hidden}" +
            "th,td{text-align:left;padding:8px 10px;border-top:1px solid var(--line);vertical-align:top}th{border-top:0;color:var(--muted);font-weight:600}" +
            ".mark{width:28px;font-weight:700}.pass .mark{color:var(--ok)}.fail .mark{color:var(--ko)}.skip .mark{color:var(--skip)}" +
            ".dur{width:70px;color:var(--muted);white-space:nowrap}.why{margin-top:6px;color:var(--ko)}code{font-size:13px}" +
            "@media (max-width:640px){th:nth-child(4),td:nth-child(4){display:none}}";
    }
}
