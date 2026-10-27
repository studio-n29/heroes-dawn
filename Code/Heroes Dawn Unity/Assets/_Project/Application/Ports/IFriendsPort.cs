#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Ports
{
    /// <summary>Un ami : son tag complet (« Mara#0007 ») et son nom.</summary>
    public readonly struct FriendEntry
    {
        public string Tag { get; }
        public string Name { get; }
        public FriendEntry(string tag, string name) { Tag = tag; Name = name; }
    }

    /// <summary>Liste d'amis (M1 : simulée en local, enregistrée dans le profil ; le compte en ligne la remplacera).</summary>
    public interface IFriendsPort
    {
        IReadOnlyList<FriendEntry> Friends();
        Result Add(string tag);
        Result Remove(string tag);
    }
}
