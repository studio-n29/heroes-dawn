#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Session;

namespace N29.HeroesDawn.Application.Features.Meta
{
    public enum ArchiveCategory { Lore, Hero, Zone, Enemy }

    public readonly struct ArchiveEntry
    {
        public readonly string Id;
        public readonly ArchiveCategory Category;
        public readonly string TitleKey, BodyKey;
        public readonly bool Unlocked;
        public readonly bool Read;

        public ArchiveEntry(string id, ArchiveCategory category, string titleKey, string bodyKey, bool unlocked, bool read)
        { Id = id; Category = category; TitleKey = titleKey; BodyKey = bodyKey; Unlocked = unlocked; Read = read; }

        public bool Unread => Unlocked && !Read;
    }

    /// <summary>
    /// Archives : fiches de lore débloquées par la progression (héros recrutés, zones validées, ennemis vaincus, histoire).
    /// Une fiche non lue porte une pastille « N ».
    /// </summary>
    public sealed class ArchiveBook
    {
        readonly MetaContext _c;

        public ArchiveBook(MetaContext context) { _c = context; }

        public IReadOnlyList<ArchiveEntry> Entries()
        {
            var s = _c.Session;
            var read = _c.Meta.archivesRead;
            bool Done(string zone) => s.CompletedZones.Contains(zone);
            var list = new List<ArchiveEntry>();
            void Add(string id, ArchiveCategory cat, string title, string body, bool unlocked)
                => list.Add(new ArchiveEntry(id, cat, title, body, unlocked, read.Contains(id)));

            Add("lore.fracture", ArchiveCategory.Lore, "archives.lore.fracture.title", "archives.lore.fracture.body", true);
            Add("lore.section", ArchiveCategory.Lore, "archives.lore.section.title", "archives.lore.section.body", true);
            Add("lore.rayner", ArchiveCategory.Lore, "archives.lore.rayner.title", "archives.lore.rayner.body", true);
            Add("lore.apex", ArchiveCategory.Lore, "archives.lore.apex.title", "archives.lore.apex.body", Done("zone_ny_1_1"));
            foreach (var m in s.Content.Members)
                Add("hero." + m.Id.Value, ArchiveCategory.Hero, m.NameKey, "archives.hero." + m.Id.Value, s.Roster.Any(r => r.Id == m.Id));
            foreach (var z in s.Content.Zones.Where(x => !x.IsTutorial))
            {
                bool done = Done(z.Id.Value);
                Add("zone." + z.Id.Value, ArchiveCategory.Zone, z.NameKey, "archives.zone." + z.Id.Value, done);
                foreach (var enc in z.Encounters)
                    foreach (var e in enc.Enemies)
                    {
                        string id = "enemy." + e.Value;
                        if (list.Any(x => x.Id == id))
                        {
                            if (done)
                            {
                                int i = list.FindIndex(x => x.Id == id);
                                if (!list[i].Unlocked) list[i] = new ArchiveEntry(id, ArchiveCategory.Enemy, list[i].TitleKey, list[i].BodyKey, true, read.Contains(id));
                            }
                            continue;
                        }
                        var def = s.Content.Enemy(e);
                        Add(id, ArchiveCategory.Enemy, def.NameKey, "archives.enemy." + e.Value, done);
                    }
            }
            return list;
        }

        public int UnreadCount() => Entries().Count(e => e.Unread);

        public void MarkRead(string id)
        {
            var read = _c.Meta.archivesRead;
            if (read.Contains(id)) return;
            read.Add(id);
            _c.Touch(MetaArea.Archives);
        }
    }
}
