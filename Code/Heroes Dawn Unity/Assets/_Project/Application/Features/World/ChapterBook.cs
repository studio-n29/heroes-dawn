#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.World
{
    /// <summary>Chapitres dont l'ouverture a déjà été montrée (enregistrés dans le profil) et nombre de chapitres terminés.</summary>
    public sealed class ChapterBook
    {
        readonly MetaContext _c;

        public ChapterBook(MetaContext context) { _c = context; }

        public bool Seen(string chapterId) => _c.Meta.chaptersSeen.Contains(chapterId);

        public void MarkSeen(string chapterId)
        {
            if (Seen(chapterId)) return;
            _c.Meta.chaptersSeen.Add(chapterId);
            _c.Touch(MetaArea.Profile);
        }

        public int DoneCount() => Chapters.All.Count(c => ChapterProgress.StatusOf(_c.Session, c) == ChapterStatus.Done);
    }
}
