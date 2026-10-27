#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    public static class FriendErrors
    {
        public static readonly DomainError Invalid = new DomainError("friends.invalid", "errors.friends_invalid");
        public static readonly DomainError Self = new DomainError("friends.self", "errors.friends_self");
        public static readonly DomainError Duplicate = new DomainError("friends.duplicate", "errors.friends_duplicate");
        public static readonly DomainError Full = new DomainError("friends.full", "errors.friends_full");
    }

    /// <summary>
    /// Amis simulés en local : une liste de tags dans le profil. Sans réseau, on ne peut pas vérifier qu'un ami existe :
    /// tous restent « en attente de connexion en ligne » jusqu'au branchement du compte.
    /// </summary>
    public sealed class LocalFriendsPort : IFriendsPort
    {
        public const int MaxFriends = 50;

        readonly MetaContext _c;

        public LocalFriendsPort(MetaContext context) { _c = context; }

        List<string> Tags => _c.Meta.friends;

        public IReadOnlyList<FriendEntry> Friends()
        {
            var list = new List<FriendEntry>();
            foreach (var tag in Tags)
                if (PlayerTag.TryParse(tag, out var name, out _)) list.Add(new FriendEntry(tag, name));
            return list;
        }

        public Result Add(string tag)
        {
            if (!PlayerTag.TryParse(tag, out var name, out int code)) return Result.Fail(FriendErrors.Invalid);
            string normalized = PlayerTag.Format(name, code);
            string own = PlayerTag.Of(_c.Session.Account?.PlayerId ?? "local", _c.Meta.onboarding.pseudo);
            if (Same(normalized, own)) return Result.Fail(FriendErrors.Self);
            if (Tags.Any(t => Same(t, normalized))) return Result.Fail(FriendErrors.Duplicate);
            if (Tags.Count >= MaxFriends) return Result.Fail(FriendErrors.Full);
            Tags.Add(normalized);
            _c.Touch(MetaArea.Profile);
            return Result.Ok();
        }

        public Result Remove(string tag)
        {
            int removed = Tags.RemoveAll(t => Same(t, tag.Trim()));
            if (removed > 0) _c.Touch(MetaArea.Profile);
            return Result.Ok();
        }

        static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
