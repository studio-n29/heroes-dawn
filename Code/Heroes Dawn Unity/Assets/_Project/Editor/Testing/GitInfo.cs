#nullable enable
using System.IO;

namespace N29.HeroesDawn.EditorTools.Testing
{
    /// <summary>Branche et commit courants, lus dans .git sans lancer git.</summary>
    public static class GitInfo
    {
        public static (string Branch, string Commit) Read(string repoRoot)
        {
            try
            {
                var head = Path.Combine(repoRoot, ".git", "HEAD");
                if (!File.Exists(head)) return ("?", "?");
                var content = File.ReadAllText(head).Trim();
                if (!content.StartsWith("ref: ")) return ("(détaché)", Short(content));
                var reference = content.Substring(5).Trim();
                var branch = reference.StartsWith("refs/heads/") ? reference.Substring(11) : reference;
                var refFile = Path.Combine(repoRoot, ".git", reference.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(refFile)) return (branch, Short(File.ReadAllText(refFile).Trim()));
                var packed = Path.Combine(repoRoot, ".git", "packed-refs");
                if (File.Exists(packed))
                    foreach (var line in File.ReadAllLines(packed))
                        if (line.EndsWith(" " + reference)) return (branch, Short(line.Substring(0, line.IndexOf(' '))));
                return (branch, "?");
            }
            catch (IOException) { return ("?", "?"); }
        }

        static string Short(string sha) => sha.Length > 7 ? sha.Substring(0, 7) : sha;
    }
}
