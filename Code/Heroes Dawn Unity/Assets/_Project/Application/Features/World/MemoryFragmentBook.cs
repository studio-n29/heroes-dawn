#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.World
{
    /// <summary>Fragments de souvenirs : débloqués en validant la zone qui les donne, marqués lus à la première lecture (enregistrés dans le profil).</summary>
    public sealed class MemoryFragmentBook
    {
        readonly MetaContext _c;

        public MemoryFragmentBook(MetaContext context) { _c = context; }

        public bool Unlocked(MemoryFragmentDef fragment) => _c.Session.CompletedZones.Contains(fragment.UnlockZone);

        public bool Read(string fragmentId) => _c.Meta.fragmentsRead.Contains(fragmentId);

        public void MarkRead(string fragmentId)
        {
            if (Read(fragmentId)) return;
            _c.Meta.fragmentsRead.Add(fragmentId);
            _c.Touch(MetaArea.Profile);
        }

        public IReadOnlyList<MemoryFragmentDef> OfChapter(string chapterId) => MemoryFragments.OfChapter(chapterId);

        public int UnlockedCount(string chapterId) => OfChapter(chapterId).Count(Unlocked);

        /// <summary>Fragments débloqués et pas encore lus (pastille « nouveau »).</summary>
        public int NewCount(string chapterId) => OfChapter(chapterId).Count(f => Unlocked(f) && !Read(f.Id));
    }
}
