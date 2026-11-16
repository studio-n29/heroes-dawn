#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Authoring
{
    /// <summary>Fiche du catalogue hors de la grammaire du plan P1 : l'identifiant de la carte, la cellule en cause et la raison du rejet.</summary>
    public sealed class CatalogImportException : Exception
    {
        public string CardId { get; }
        public string Cell { get; }
        public string Reason { get; }

        public CatalogImportException(string cardId, string cell, string reason)
            : base($"{cardId} [{cell}] : {reason}")
        {
            CardId = cardId; Cell = cell; Reason = reason;
        }
    }

    /// <summary>Ligne d'un tableau du catalogue : les 16 colonnes, telles que lues (texte nettoyé, ★ et coût en entiers).</summary>
    public sealed class CatalogRow
    {
        public string Id { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Name { get; set; } = "";
        public int Stars { get; set; }
        public string Famille { get; set; } = "";
        public string Nature { get; set; } = "";
        public string Archetype { get; set; } = "";
        public string Role { get; set; } = "";
        public string Cible { get; set; } = "";
        public int Cost { get; set; }
        public string Phase { get; set; } = "";
        public string Base { get; set; } = "";
        public string Niv3 { get; set; } = "";
        public string Niv6 { get; set; } = "";
        public string Niv10 { get; set; } = "";
        public string Idee { get; set; } = "";
    }

    /// <summary>
    /// Importeur des fiches Markdown (docs/catalogue/cartes-*.md) : lit le tableau, puis convertit une ligne (phases P1, P2, P3) en <see cref="CardRecord"/>
    /// selon la grammaire du plan P1 étendue par celle du plan P2/P3 (réactions, Consomme, Propage, échelle d'archétype). Analyse à la main (scanneur et descente récursive) : tout texte non compris lève
    /// <see cref="CatalogImportException"/>, rien n'est ignoré ni importé à moitié.
    /// </summary>
    public static class CatalogImporter
    {
        static readonly string[] Columns = { "Id", "Nom", "Name", "★", "Famille", "Nature", "Archétype", "Rôle", "Cible", "Coût", "Phase", "Base", "Niv.3", "Niv.6", "Niv.10", "Idée" };

        // ---------------------------------------------------------------- Lecture du tableau

        /// <summary>Lignes du tableau du catalogue (toutes phases confondues). Le texte hors tableau est ignoré.</summary>
        public static IReadOnlyList<CatalogRow> ReadTable(string markdown)
        {
            if (markdown == null) throw new ArgumentNullException(nameof(markdown));
            var lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            int i = 0;
            while (i < lines.Length && !(IsTableLine(lines[i]) && IsHeader(SplitCells(lines[i])))) i++;
            if (i >= lines.Length) throw new CatalogImportException("", "en-tête", "tableau du catalogue introuvable (en-tête « | " + string.Join(" | ", Columns) + " | » attendu)");
            i++;
            if (i >= lines.Length || !IsTableLine(lines[i]) || !SplitCells(lines[i]).All(IsSeparatorCell))
                throw new CatalogImportException("", "en-tête", "ligne de séparation « |---|… » attendue sous l'en-tête");
            i++;
            var rows = new List<CatalogRow>();
            for (; i < lines.Length && IsTableLine(lines[i]); i++) rows.Add(ReadRow(SplitCells(lines[i])));
            for (; i < lines.Length; i++)
            {
                if (!IsTableLine(lines[i])) continue;
                var cells = SplitCells(lines[i]);
                throw new CatalogImportException(cells.Count > 0 ? cells[0] : "", "ligne", "ligne de tableau après la fin du tableau du catalogue (un seul tableau par fichier, sans ligne vide ni texte au milieu)");
            }
            return rows;
        }

        static bool IsTableLine(string line) => line.TrimStart().StartsWith("|", StringComparison.Ordinal);

        static bool IsHeader(List<string> cells) => cells.Count == Columns.Length && cells.Select((c, k) => c == Columns[k]).All(x => x);

        static bool IsSeparatorCell(string c) => c.Length > 0 && c.All(ch => ch == '-' || ch == ':');

        static string Clean(string s) => s.Normalize(NormalizationForm.FormC).Trim();

        static List<string> SplitCells(string line)
        {
            var t = Clean(line);
            if (t.StartsWith("|", StringComparison.Ordinal)) t = t.Substring(1);
            if (t.EndsWith("|", StringComparison.Ordinal)) t = t.Substring(0, t.Length - 1);
            return t.Split('|').Select(Clean).ToList();
        }

        static CatalogRow ReadRow(List<string> c)
        {
            var id = c.Count > 0 ? c[0] : "";
            if (c.Count != Columns.Length) throw new CatalogImportException(id, "ligne", $"{c.Count} colonnes au lieu de {Columns.Length}");
            if (id.Length == 0) throw new CatalogImportException(id, "Id", "identifiant vide");
            if (!int.TryParse(c[3], out var stars)) throw new CatalogImportException(id, "★", $"entier attendu, « {c[3]} » lu");
            if (!int.TryParse(c[9], out var cost)) throw new CatalogImportException(id, "Coût", $"entier attendu, « {c[9]} » lu");
            if (c[10] != "P1" && c[10] != "P2" && c[10] != "P3") throw new CatalogImportException(id, "Phase", $"P1, P2 ou P3 attendu, « {c[10]} » lu");
            return new CatalogRow
            {
                Id = id, Nom = c[1], Name = c[2], Stars = stars, Famille = c[4], Nature = c[5], Archetype = c[6], Role = c[7], Cible = c[8],
                Cost = cost, Phase = c[10], Base = c[11], Niv3 = c[12], Niv6 = c[13], Niv10 = c[14], Idee = c[15],
            };
        }

        // ---------------------------------------------------------------- Conversion d'une fiche

        /// <summary>Carte de la fiche (phases P1, P2 et P3 : la phase n'est qu'une étiquette, la grammaire est la même ; tout texte hors grammaire lève <see cref="CatalogImportException"/>).</summary>
        public static CardRecord ToRecord(CatalogRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            var id = row.Id;
            if (id.Length == 0) throw new CatalogImportException(id, "Id", "identifiant vide");
            if (row.Stars < 1 || row.Stars > 5) throw new CatalogImportException(id, "★", $"1 à 5 attendu, {row.Stars} lu");
            if (row.Cost < 1) throw new CatalogImportException(id, "Coût", $"coût positif attendu, {row.Cost} lu");

            var card = new CardRecord
            {
                id = id,
                family = Lookup(id, "Famille", row.Famille, Families),
                stars = row.Stars,
                nature = Lookup(id, "Nature", row.Nature, Natures),
                role = Lookup(id, "Rôle", row.Role, Roles),
            };
            card.archetypes.AddRange(ParseArchetypes(id, row.Archetype));
            card.skill.id = id;
            card.skill.slot = SkillSlot.A3;
            card.skill.cost = row.Cost;
            card.skill.cooldown = row.Cost / 10;
            ParseTarget(id, row.Cible, card.skill);
            card.skill.effects.AddRange(new CellParser(id, "Base").Parse(row.Base));
            card.tiers.Add(new TierRecord { level = 3, effects = new CellParser(id, "Niv.3").Parse(row.Niv3).ToList() });
            card.tiers.Add(new TierRecord { level = 6, effects = new CellParser(id, "Niv.6").Parse(row.Niv6).ToList() });
            card.tiers.Add(new TierRecord { level = 10, effects = new CellParser(id, "Niv.10").Parse(row.Niv10).ToList() });
            return card;
        }

        static readonly Dictionary<string, Family> Families = new Dictionary<string, Family>
            { ["Matière"] = Family.Matter, ["Élémentaire"] = Family.Elemental, ["Physique/Bio"] = Family.PhysicalBio, ["Perception"] = Family.Perception };

        static readonly Dictionary<string, CardNature> Natures = new Dictionary<string, CardNature>
            { ["Offensive"] = CardNature.Offensive, ["Contrôle"] = CardNature.Control, ["Défensive"] = CardNature.Defensive, ["Soutien"] = CardNature.Support };

        static readonly Dictionary<string, CardRole> Roles = new Dictionary<string, CardRole>
            { ["Amorce"] = CardRole.Primer, ["Gain"] = CardRole.Payoff, ["Pont"] = CardRole.Bridge, ["Base"] = CardRole.Basic };

        static readonly Dictionary<string, Archetype> Archetypes = new Dictionary<string, Archetype>
        {
            ["Brûlure"] = Archetype.Burn, ["Entrave"] = Archetype.Restrain, ["Traque"] = Archetype.Hunt,
            ["Tempo"] = Archetype.Tempo, ["Rempart"] = Archetype.Bulwark, ["Renouveau"] = Archetype.Renewal,
        };

        static readonly Dictionary<string, StatusType> Statuses = Enum.GetValues(typeof(StatusType)).Cast<StatusType>()
            .Where(s => s != StatusType.None).ToDictionary(s => s.ToString(), s => s);

        static T Lookup<T>(string id, string cell, string text, Dictionary<string, T> map)
        {
            if (map.TryGetValue(Clean(text), out var v)) return v;
            throw new CatalogImportException(id, cell, $"valeur inconnue « {text} » (attendu : {string.Join(", ", map.Keys)})");
        }

        static IEnumerable<Archetype> ParseArchetypes(string id, string text)
        {
            var t = Clean(text);
            if (t == "—" || t == "–") return Array.Empty<Archetype>();
            var result = new List<Archetype>();
            foreach (var part in t.Split('+'))
            {
                var name = Clean(part);
                if (name.Length == 0) throw new CatalogImportException(id, "Archétype", $"nom vide dans « {text} »");
                var a = Lookup(id, "Archétype", name, Archetypes);
                if (result.Contains(a)) throw new CatalogImportException(id, "Archétype", $"archétype répété dans « {text} »");
                result.Add(a);
            }
            if (result.Count > 2) throw new CatalogImportException(id, "Archétype", $"au plus deux archétypes, « {text} » lu");
            return result;
        }

        static void ParseTarget(string id, string text, SkillRecord skill)
        {
            CatalogImportException Fail(string reason) => new CatalogImportException(id, "Cible", reason + $" (« {text} »)");
            var t = Clean(text);
            var colon = t.IndexOf(':');
            var head = colon < 0 ? t : Clean(t.Substring(0, colon));
            var tail = colon < 0 ? null : Clean(t.Substring(colon + 1));
            switch (head)
            {
                case "1E": skill.target = TargetRule.SingleEnemy; break;
                case "ZE": skill.target = TargetRule.AllEnemies; break;
                case "1A": skill.target = TargetRule.SingleAlly; break;
                case "ZA": skill.target = TargetRule.AllAllies; break;
                case "Soi": skill.target = TargetRule.Self; break;
                case "2E":
                    if (tail != "aléatoires") throw Fail("« 2E » exige « 2E:aléatoires »");
                    skill.target = TargetRule.AllEnemies; skill.pick = TargetPick.TwoRandom;
                    return;
                default: throw Fail("cible inconnue");
            }
            if (tail == null) return;
            if (head != "1E" && head != "1A") throw Fail("un ciblage fin n'existe que pour 1E, 1A et 2E");
            var enemy = head == "1E";
            switch (tail)
            {
                case "faible": skill.pick = TargetPick.LowestHp; return;
                case "prochain": skill.pick = TargetPick.NextToAct; return;
                case "fort" when enemy: skill.pick = TargetPick.HighestHp; return;
                case "aléatoire" when enemy: skill.pick = TargetPick.Random; return;
            }
            if (Statuses.TryGetValue(tail, out var status)) { skill.pick = TargetPick.StatusHolder; skill.pickStatus = status; return; }
            throw Fail("ciblage fin inconnu");
        }

        // ---------------------------------------------------------------- Scanneur

        sealed class Scanner
        {
            readonly string _s;
            readonly Func<string, CatalogImportException> _fail;
            int _p;

            public Scanner(string text, Func<string, CatalogImportException> fail) { _s = text; _fail = fail; }

            public CatalogImportException Fail(string reason) => _fail(reason);

            void Ws() { while (_p < _s.Length && char.IsWhiteSpace(_s[_p])) _p++; }

            public int Mark() => _p;
            public void Reset(int mark) => _p = mark;

            public bool AtEnd { get { Ws(); return _p >= _s.Length; } }

            public char Peek() { Ws(); return _p < _s.Length ? _s[_p] : '\0'; }

            public bool Try(char c) { if (Peek() != c) return false; _p++; return true; }

            public bool TryStr(string t)
            {
                Ws();
                if (string.CompareOrdinal(_s, _p, t, 0, t.Length) != 0) return false;
                _p += t.Length;
                return true;
            }

            public void Expect(char c, string what)
            {
                if (!Try(c)) throw _fail($"« {what} » attendu{Near()}");
            }

            public string? PeekWord()
            {
                Ws();
                int q = _p;
                while (q < _s.Length && char.IsLetter(_s[q])) q++;
                return q == _p ? null : _s.Substring(_p, q - _p);
            }

            public string Word(string what)
            {
                var w = PeekWord();
                if (w == null) throw _fail($"{what} attendu{Near()}");
                _p += w.Length;
                if (_p < _s.Length && char.IsDigit(_s[_p])) throw _fail($"espace manquant après « {w} »");
                return w;
            }

            public void ExpectWord(string word)
            {
                if (PeekWord() != word) throw _fail($"« {word} » attendu{Near()}");
                _p += word.Length;
            }

            public int Int(string what)
            {
                Ws();
                int q = _p;
                while (q < _s.Length && _s[q] >= '0' && _s[q] <= '9') q++;
                if (q == _p || q - _p > 6) throw _fail($"{what} attendu{Near()}");
                var v = int.Parse(_s.Substring(_p, q - _p));
                _p = q;
                return v;
            }

            /// <summary>Le reste du texte (sans espaces aux bords) ; le scanneur est alors en fin de texte.</summary>
            public string Rest() { Ws(); var rest = _s.Substring(_p).Trim(); _p = _s.Length; return rest; }

            public void End()
            {
                if (!AtEnd) throw _fail($"texte inattendu{Near()}");
            }

            string Near() => _p >= _s.Length ? " en fin de texte" : $" avant « {_s.Substring(_p)} »";
        }

        // ---------------------------------------------------------------- Cellules d'effets

        sealed class Item
        {
            public EffectRecord? Host;
            public readonly List<EffectRecord> Extras = new List<EffectRecord>();
            public readonly List<string> Brackets = new List<string>();
        }

        sealed class CellParser
        {
            static readonly HashSet<EffectType> ValueTypes = new HashSet<EffectType>
                { EffectType.Damage, EffectType.Heal, EffectType.Shield, EffectType.Advance, EffectType.PushBack, EffectType.GaugeDelta };

            static readonly Dictionary<string, EffectType> BonusTypes = new Dictionary<string, EffectType>
            {
                ["Dmg"] = EffectType.Damage, ["Heal"] = EffectType.Heal, ["Shield"] = EffectType.Shield,
                ["Adv"] = EffectType.Advance, ["Push"] = EffectType.PushBack, ["Gauge"] = EffectType.GaugeDelta,
            };

            readonly string _id;
            readonly string _cell;

            public CellParser(string cardId, string cell) { _id = cardId; _cell = cell; }

            CatalogImportException Fail(string reason) => new CatalogImportException(_id, _cell, reason);

            Scanner Scan(string text) => new Scanner(text, Fail);

            public IReadOnlyList<EffectRecord> Parse(string text)
            {
                var clean = Clean(text);
                if (clean.Length == 0) throw Fail("cellule vide");
                var items = Split(clean).Select(ParseItem).ToList();
                for (int i = 0; i < items.Count; i++)
                    foreach (var bracket in items[i].Brackets) ApplyBracket(items, i, bracket);
                var result = new List<EffectRecord>();
                foreach (var item in items)
                {
                    if (item.Host != null) result.Add(item.Host);
                    result.AddRange(item.Extras);
                }
                return result;
            }

            // Éléments séparés par « , » ou « ; » hors crochets et parenthèses.
            List<string> Split(string text)
            {
                var parts = new List<string>();
                int square = 0, round = 0, start = 0;
                for (int k = 0; k < text.Length; k++)
                {
                    switch (text[k])
                    {
                        case '[': if (square > 0) throw Fail("crochets imbriqués"); square++; break;
                        case ']': if (square == 0) throw Fail("« ] » sans « [ »"); square--; break;
                        case '(': round++; break;
                        case ')': if (round == 0) throw Fail("« ) » sans « ( »"); round--; break;
                        case ',':
                        case ';':
                            if (square == 0 && round == 0) { parts.Add(text.Substring(start, k - start)); start = k + 1; }
                            break;
                    }
                }
                if (square != 0) throw Fail("crochet non refermé");
                if (round != 0) throw Fail("parenthèse non refermée");
                parts.Add(text.Substring(start));
                foreach (var p in parts) if (Clean(p).Length == 0) throw Fail("élément vide (séparateur en trop)");
                return parts.Select(Clean).ToList();
            }

            Item ParseItem(string text)
            {
                var item = new Item();
                int open = text.IndexOf('[');
                var effectText = open < 0 ? text : text.Substring(0, open);
                if (Clean(effectText).Length > 0)
                {
                    var s = Scan(effectText);
                    item.Host = ParseEffect(s);
                    s.End();
                }
                int p = open;
                while (p >= 0 && p < text.Length)
                {
                    if (text[p] != '[') throw Fail($"texte inattendu après un crochet : « {text.Substring(p)} »");
                    int close = text.IndexOf(']', p);
                    item.Brackets.Add(text.Substring(p + 1, close - p - 1));
                    p = close + 1;
                    while (p < text.Length && char.IsWhiteSpace(text[p])) p++;
                }
                return item;
            }

            EffectRecord ParseEffect(Scanner s)
            {
                var word = s.Word("nom d'effet");
                switch (word)
                {
                    case "Dmg": return Make(s, EffectType.Damage, s.Int("valeur"), 1);
                    case "Heal": return Make(s, EffectType.Heal, s.Int("valeur"), 1);
                    case "Shield": return Make(s, EffectType.Shield, s.Int("valeur"), 1);
                    case "Push": return Make(s, EffectType.PushBack, s.Int("valeur"), 1);
                    case "Adv": return Make(s, EffectType.Advance, s.Int("valeur"), 1);
                    case "Steal": return Make(s, EffectType.Lifesteal, s.Int("valeur"), 1);
                    case "Purge": return Make(s, EffectType.Purge, s.Int("valeur"), 0);
                    case "Bounce": return Make(s, EffectType.Bounce, s.Int("valeur"), 1);
                    case "Exec": return Make(s, EffectType.Execute, s.Int("valeur"), 1);
                    case "Consomme": return ParseConsume(s);
                    case "Propage": return new EffectRecord { type = EffectType.Spread, status = ParseStatus(s), chance = 100 };
                    case "Gauge":
                    {
                        var sign = s.Peek();
                        if (sign != '+' && sign != '-') throw s.Fail("« Gauge » exige un signe : « Gauge +N » ou « Gauge -N »");
                        s.Try(sign);
                        var n = s.Int("valeur");
                        return Make(s, EffectType.GaugeDelta, sign == '-' ? -n : n, 1, signed: true);
                    }
                    case "St":
                    {
                        var status = ParseStatus(s);
                        var turns = s.Int("durée");
                        s.ExpectWord("t");
                        if (turns < 1) throw s.Fail("durée d'au moins 1 tour");
                        var chance = 100;
                        if (s.Try('('))
                        {
                            chance = s.Int("chance");
                            s.Expect('%', "%");
                            s.Expect(')', ")");
                            if (chance < 1 || chance > 100) throw s.Fail($"chance de 1 à 100 % attendue, {chance} lu");
                        }
                        return new EffectRecord { type = EffectType.ApplyStatus, status = status, duration = turns, chance = chance };
                    }
                    default: throw s.Fail($"effet inconnu « {word} »");
                }
            }

            static EffectRecord Make(Scanner s, EffectType type, int value, int min, bool signed = false)
            {
                if ((signed ? Math.Abs(value) : value) < min) throw s.Fail($"valeur d'au moins {min} attendue, {value} lu");
                return NewEffect(type, value);
            }

            static EffectRecord NewEffect(EffectType type, int value)
            {
                var e = new EffectRecord { type = type, value = value };
                if (type == EffectType.Shield) { e.status = StatusType.Shield; e.duration = 2; }
                return e;
            }

            static StatusType ParseStatus(Scanner s)
            {
                var name = s.Word("nom de statut");
                if (Statuses.TryGetValue(name, out var status)) return status;
                throw s.Fail($"statut inconnu « {name} »");
            }

            // « Consomme <Statut> : Dmg|Heal N par tour restant (max M) » (M de 1 à 10) : Value = N, Duration = M.
            static EffectRecord ParseConsume(Scanner s)
            {
                var status = ParseStatus(s);
                s.Expect(':', ":");
                var kind = s.Word("« Dmg » ou « Heal »");
                EffectType type;
                if (kind == "Dmg") type = EffectType.ConsumeDamage;
                else if (kind == "Heal") type = EffectType.ConsumeHeal;
                else throw s.Fail($"« Dmg » ou « Heal » attendu après « Consomme {status} : », « {kind} » lu");
                var value = s.Int("valeur");
                if (value < 1) throw s.Fail($"valeur d'au moins 1 attendue, {value} lu");
                s.ExpectWord("par"); s.ExpectWord("tour"); s.ExpectWord("restant");
                s.Expect('(', "(max M)");
                s.ExpectWord("max");
                var max = s.Int("max");
                s.Expect(')', ")");
                if (max < 1 || max > 10) throw s.Fail($"max de 1 à 10 tours attendu pour Consomme, {max} lu");
                return new EffectRecord { type = type, value = value, status = status, duration = max, chance = 100 };
            }

            static readonly HashSet<EffectType> ReactionEffects = new HashSet<EffectType>
            {
                EffectType.Damage, EffectType.Heal, EffectType.Shield, EffectType.ApplyStatus,
                EffectType.PushBack, EffectType.Advance, EffectType.GaugeDelta, EffectType.Purge,
            };

            // « Réaction Nt : quand <événement> → <effet> [sur <cible>] » : un effet ArmReaction à la place du crochet.
            void ApplyReaction(List<Item> items, int index, Scanner s)
            {
                s.ExpectWord("Réaction");
                var turns = s.Int("durée");
                s.ExpectWord("t");
                if (turns < 1 || turns > 5) throw s.Fail($"durée de réaction de 1 à 5 tours attendue, {turns} lu");
                s.Expect(':', ":");
                s.ExpectWord("quand");
                var (trigger, status) = ParseEvent(s);
                if (!s.Try('→')) throw s.Fail("« → » attendu entre l'événement et l'effet de la réaction");
                var nested = ParseEffect(s);
                if (!ReactionEffects.Contains(nested.type))
                    throw s.Fail($"{nested.type} ne peut pas être l'effet d'une réaction (seuls Dmg, Heal, Shield, St, Push, Adv, Gauge et Purge le peuvent)");

                var onTrigger = trigger == ReactionTrigger.EnemyReceivesStatus || trigger == ReactionTrigger.AllyReceivesStatus;
                var target = TargetRule.Self;
                var pick = TargetPick.Default;
                if (s.PeekWord() == "sur")
                {
                    s.ExpectWord("sur");
                    var name = s.Rest();
                    onTrigger = false;
                    switch (name)
                    {
                        case "Soi": break;
                        case "ZE": target = TargetRule.AllEnemies; break;
                        case "ZA": target = TargetRule.AllAllies; break;
                        case "1E:aléatoire": target = TargetRule.SingleEnemy; pick = TargetPick.Random; break;
                        case "1E:faible": target = TargetRule.SingleEnemy; pick = TargetPick.LowestHp; break;
                        case "1A:prochain": target = TargetRule.SingleAlly; pick = TargetPick.NextToAct; break;
                        case "1A:faible": target = TargetRule.SingleAlly; pick = TargetPick.LowestHp; break;
                        case "déclencheur": onTrigger = true; break;
                        default: throw s.Fail($"cible de réaction inconnue « {name} » (attendu : Soi, ZE, ZA, 1E:aléatoire, 1E:faible, 1A:prochain, 1A:faible, déclencheur)");
                    }
                }
                s.End();

                items[index].Extras.Add(new EffectRecord
                {
                    type = EffectType.ArmReaction, chance = 100,
                    hasReaction = true, reactionTrigger = trigger, reactionStatus = status, reactionTurns = turns,
                    reactionEffectType = nested.type, reactionEffectValue = nested.value, reactionEffectStatus = nested.status,
                    reactionEffectDuration = nested.duration, reactionEffectChance = nested.chance,
                    reactionTarget = target, reactionPick = pick, reactionPickStatus = StatusType.None, reactionOnTriggerUnit = onTrigger,
                });
            }

            // Les six événements : « un ennemi|allié reçoit <Statut> », « un ennemi|allié est vaincu », « le porteur est touché », « le porteur inflige un critique ».
            static (ReactionTrigger, StatusType) ParseEvent(Scanner s)
            {
                var first = s.Word("« un » ou « le »");
                if (first == "un")
                {
                    var who = s.Word("« ennemi » ou « allié »");
                    if (who != "ennemi" && who != "allié") throw s.Fail($"« ennemi » ou « allié » attendu, « {who} » lu");
                    var enemy = who == "ennemi";
                    var verb = s.Word("« reçoit » ou « est »");
                    if (verb == "reçoit")
                        return (enemy ? ReactionTrigger.EnemyReceivesStatus : ReactionTrigger.AllyReceivesStatus, ParseStatus(s));
                    if (verb == "est")
                    {
                        s.ExpectWord("vaincu");
                        return (enemy ? ReactionTrigger.EnemyDefeated : ReactionTrigger.AllyDefeated, StatusType.None);
                    }
                    throw s.Fail($"« reçoit » ou « est » attendu après « un {who} », « {verb} » lu");
                }
                if (first == "le")
                {
                    s.ExpectWord("porteur");
                    var verb = s.Word("« est » ou « inflige »");
                    if (verb == "est") { s.ExpectWord("touché"); return (ReactionTrigger.CarrierHit, StatusType.None); }
                    if (verb == "inflige") { s.ExpectWord("un"); s.ExpectWord("critique"); return (ReactionTrigger.CarrierCrit, StatusType.None); }
                    throw s.Fail($"« est » ou « inflige » attendu après « le porteur », « {verb} » lu");
                }
                throw s.Fail($"événement de réaction inconnu (« un ennemi… », « un allié… » ou « le porteur… » attendu, « {first} » lu)");
            }

            // ------------------------------------------------------------ Crochets

            struct Cond
            {
                public ConditionKind Kind; public StatusType Status; public int Value;
            }

            static bool TryGe(Scanner s) => s.Try('≥') || s.TryStr(">=");
            static bool TryLe(Scanner s) => s.Try('≤') || s.TryStr("<=");

            static int Percent(Scanner s)
            {
                var n = s.Int("pourcentage");
                s.Expect('%', "%");
                s.ExpectWord("PV");
                if (n < 1 || n > 100) throw s.Fail($"pourcentage de PV de 1 à 100 attendu, {n} lu");
                return n;
            }

            static Cond ParseCondition(Scanner s)
            {
                if (TryGe(s)) { var n = Count(s, "ennemis", "vivants"); return new Cond { Kind = ConditionKind.EnemiesAliveAtLeast, Value = n }; }
                if (TryLe(s)) { var n = Count(s, "ennemis", "vivants"); return new Cond { Kind = ConditionKind.EnemiesAliveAtMost, Value = n }; }
                var who = s.Word("« cible », « lanceur » ou « un allié »");
                switch (who)
                {
                    case "cible":
                        if (s.Try('<')) return new Cond { Kind = ConditionKind.TargetHpBelow, Value = Percent(s) };
                        if (s.Try('>')) return new Cond { Kind = ConditionKind.TargetHpAbove, Value = Percent(s) };
                        var verb = s.Word("« a » ou « sans »");
                        if (verb == "sans") return new Cond { Kind = ConditionKind.TargetLacksStatus, Status = ParseStatus(s) };
                        if (verb != "a") throw s.Fail($"« a » ou « sans » attendu, « {verb} » lu");
                        if (TryGe(s)) return new Cond { Kind = ConditionKind.TargetStatusCountAtLeast, Value = Count(s, "statuts") };
                        if (s.PeekWord() == "annoncé")
                        {
                            s.ExpectWord("annoncé"); s.ExpectWord("son"); s.ExpectWord("action");
                            return new Cond { Kind = ConditionKind.TargetPlannedAction };
                        }
                        return new Cond { Kind = ConditionKind.TargetHasStatus, Status = ParseStatus(s) };
                    case "lanceur":
                        if (s.Try('<')) return new Cond { Kind = ConditionKind.CasterHpBelow, Value = Percent(s) };
                        s.ExpectWord("a");
                        return new Cond { Kind = ConditionKind.CasterHasStatus, Status = ParseStatus(s) };
                    case "un":
                        s.ExpectWord("allié"); s.ExpectWord("a");
                        return new Cond { Kind = ConditionKind.AnyAllyHasStatus, Status = ParseStatus(s) };
                    default:
                        throw s.Fail($"condition inconnue (« {who} » lu)");
                }
            }

            static int Count(Scanner s, params string[] words)
            {
                var n = s.Int("nombre");
                if (n < 1) throw s.Fail("nombre d'au moins 1 attendu");
                foreach (var w in words) s.ExpectWord(w);
                return n;
            }

            void ApplyBracket(List<Item> items, int index, string text)
            {
                var s = Scan(text);
                if (s.Try('+')) { ApplyScale(items, index, s); return; }
                if (s.PeekWord() == "Réaction") { ApplyReaction(items, index, s); return; }
                if (s.PeekWord() != "si") throw Fail($"crochet incompris « [{text.Trim()}] » : « si <condition> : … », « +N par … » ou « Réaction Nt : quand … → … » attendu");
                s.ExpectWord("si");
                var cond = ParseCondition(s);
                s.Expect(':', ":");

                if (s.Try('+'))
                {
                    var bonus = s.Int("bonus");
                    s.End();
                    if (bonus < 1) throw Fail("bonus d'au moins 1 attendu");
                    AddMod(Host(items, index), NewMod(bonus, cond));
                    return;
                }

                var mark = s.Mark();
                var word = s.PeekWord();
                if (word != null && BonusTypes.TryGetValue(word, out var type))
                {
                    s.Word("type");
                    if (s.Peek() == '+')
                    {
                        s.Try('+');
                        var bonus = s.Int("bonus");
                        s.End();
                        TypedBonus(items, index, type, bonus, cond);
                        return;
                    }
                    s.Reset(mark);
                }

                var extra = ParseEffect(s);
                s.End();
                if (!ValueTypes.Contains(extra.type) && extra.type != EffectType.ApplyStatus)
                    throw Fail($"{extra.type} ne peut pas porter de condition (seuls Dmg, Heal, Shield, Adv, Push, Gauge et St le peuvent)");
                SetCondition(extra, cond);
                items[index].Extras.Add(extra);
            }

            void ApplyScale(List<Item> items, int index, Scanner s)
            {
                var bonus = s.Int("bonus");
                if (bonus < 1) throw s.Fail("bonus d'au moins 1 attendu");
                s.ExpectWord("par");
                ScaleKind per;
                Archetype? archetype = null;
                var word = s.PeekWord();
                if (word == "statut") { s.ExpectWord("statut"); s.ExpectWord("de"); s.ExpectWord("la"); s.ExpectWord("cible"); per = ScaleKind.TargetStatusCount; }
                else if (word == "ennemi") { s.ExpectWord("ennemi"); s.ExpectWord("vivant"); per = ScaleKind.EnemiesAlive; }
                else if (word == "allié") { s.ExpectWord("allié"); s.ExpectWord("vivant"); per = ScaleKind.AlliesAlive; }
                else if (word == "carte")
                {
                    s.ExpectWord("carte");
                    var name = s.Word("nom d'archétype");
                    if (!Archetypes.TryGetValue(name, out var found)) throw s.Fail($"archétype inconnu « {name} » (attendu : {string.Join(", ", Archetypes.Keys)})");
                    s.ExpectWord("équipée"); s.ExpectWord("dans");
                    if (!s.TryStr("l'escouade")) throw s.Fail("« l'escouade » attendu après « dans »");
                    archetype = found; per = ScaleKind.EquippedArchetypeCards;
                }
                else
                {
                    var tenth = s.Int("compte");
                    if (tenth != 10) throw s.Fail($"seul « 10 % de PV manquants » est un compte admis, « {tenth} % » lu");
                    s.Expect('%', "%");
                    s.ExpectWord("de"); s.ExpectWord("PV"); s.ExpectWord("manquants");
                    if (s.PeekWord() == "du") { s.ExpectWord("du"); s.ExpectWord("lanceur"); per = ScaleKind.CasterMissingHpTenths; }
                    else { s.ExpectWord("de"); s.ExpectWord("la"); s.ExpectWord("cible"); per = ScaleKind.TargetMissingHpTenths; }
                }
                s.Expect('(', "(max M)");
                s.ExpectWord("max");
                var max = s.Int("max");
                s.Expect(')', ")");
                s.End();
                if (max > 10)
                    throw s.Fail($"max > 10 ({max}) : « max » est un nombre d'unités comptées, pas un plafond de total ; mettre {max / bonus} (= {max} / {bonus}) si le total voulu est {max}");
                if (max < 1) throw s.Fail($"max d'au moins 1 attendu, {max} lu");
                AddMod(Host(items, index), new ModRecord { bonus = bonus, hasScale = true, per = per, maxCount = max, hasArchetype = archetype.HasValue, perArchetype = archetype ?? default });
            }

            // « <Type> +N » : bonus de l'effet de ce type (celui qui précède, sinon l'unique de la cellule) ; sans effet de ce type, effet supplémentaire conditionnel.
            void TypedBonus(List<Item> items, int index, EffectType type, int bonus, Cond cond)
            {
                if (bonus < 1) throw Fail("bonus d'au moins 1 attendu");
                var preceding = Preceding(items, index);
                EffectRecord? host = null;
                if (preceding != null && preceding.type == type) host = preceding;
                else
                {
                    var same = items.Where(i => i.Host != null && i.Host.type == type).Select(i => i.Host!).ToList();
                    if (same.Count == 1) host = same[0];
                    else if (same.Count > 1) throw Fail($"ambigu : {same.Count} effets {type} dans la cellule et aucun ne précède le crochet");
                }
                if (host != null) { AddMod(host, NewMod(bonus, cond)); return; }
                var extra = NewEffect(type, bonus);
                SetCondition(extra, cond);
                items[index].Extras.Add(extra);
            }

            static EffectRecord? Preceding(List<Item> items, int index)
            {
                for (int k = index; k >= 0; k--) if (items[k].Host != null) return items[k].Host;
                return null;
            }

            // Hôte d'un « +N » : l'effet qui précède ; s'il ne peut pas porter de bonus, erreur ; sans effet précédent, le premier effet porteur de la cellule.
            EffectRecord Host(List<Item> items, int index)
            {
                var preceding = Preceding(items, index);
                if (preceding != null)
                {
                    if (ValueTypes.Contains(preceding.type)) return preceding;
                    throw Fail($"« +N » placé après un effet {preceding.type} qui ne peut pas porter de bonus (écrire « <Type> +N » ou déplacer le crochet)");
                }
                var first = items.Select(i => i.Host).FirstOrDefault(h => h != null && ValueTypes.Contains(h.type));
                if (first != null) return first;
                throw Fail("aucun effet Dmg, Heal, Shield, Adv, Push ou Gauge dans la cellule pour porter le bonus");
            }

            void AddMod(EffectRecord host, ModRecord mod)
            {
                if (!ValueTypes.Contains(host.type)) throw Fail($"{host.type} ne peut pas porter de modificateur");
                host.mods.Add(mod);
            }

            static ModRecord NewMod(int bonus, Cond c)
                => new ModRecord { bonus = bonus, hasCondition = true, condKind = c.Kind, condStatus = c.Status, condValue = c.Value };

            static void SetCondition(EffectRecord e, Cond c)
            {
                e.hasCondition = true; e.condKind = c.Kind; e.condStatus = c.Status; e.condValue = c.Value;
            }
        }
    }
}
