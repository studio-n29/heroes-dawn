#nullable enable
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    public static class ShopErrors
    {
        public static readonly DomainError Expired = new DomainError("shop.expired", "errors.shop.expired");
        public static readonly DomainError UnknownSlot = new DomainError("shop.slot", "errors.shop.slot");
        public static readonly DomainError AlreadyRecruited = new DomainError("shop.recruited", "errors.shop.recruited");
    }
}
