#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Un article de la rotation de la semaine.</summary>
    public readonly struct CosmeticOffer
    {
        public readonly CosmeticDef Def;
        public readonly bool Owned;
        public CosmeticOffer(CosmeticDef def, bool owned) { Def = def; Owned = owned; }
    }

    /// <summary>
    /// Boutique de cosmétiques (Gemmes) : six articles de source Boutique par semaine ISO, tirés de façon déterministe, sans les cosmétiques déjà possédés.
    /// Les cosmétiques de pass, de tournoi, de succès et d'événement ne sont jamais en vente.
    /// </summary>
    public sealed class CosmeticShop
    {
        public const int WeeklyCount = 6;
        public const int MaxPrice = 1000;

        readonly MetaContext _c;
        readonly IReadOnlyList<CosmeticDef> _catalog;

        public CosmeticShop(MetaContext context, IReadOnlyList<CosmeticDef> catalog) { _c = context; _catalog = catalog; }

        public IReadOnlyList<CosmeticDef> Catalog => _catalog;

        public CosmeticDef? Find(string id) => _catalog.FirstOrDefault(d => d.Id == id);

        /// <summary>Clé de la semaine ISO du jour (« 2026-W40 »).</summary>
        public static string WeekKey(DateTime day) => ISOWeek.GetYear(day) + "-W" + ISOWeek.GetWeekOfYear(day).ToString("00", CultureInfo.InvariantCulture);

        /// <summary>Les articles de la semaine : ordre mélangé par la semaine, puis les six premiers non possédés (le même profil voit toujours la même rotation).</summary>
        public IReadOnlyList<CosmeticOffer> Weekly(DateTime day)
        {
            var pool = _catalog.Where(d => d.Source == CosmeticSource.Shop && d.Price > 0 && d.Price <= MaxPrice).OrderBy(d => d.Id, StringComparer.Ordinal).ToList();
            var rng = new Pcg32(Fnv("cosmetics:" + WeekKey(day)));
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            return pool.Where(d => !CosmeticOps.Owns(_c.Meta, d.Id)).Take(WeeklyCount).Select(d => new CosmeticOffer(d, false)).ToList();
        }

        /// <summary>Les articles de la semaine en cours (selon l'horloge du jeu).</summary>
        public IReadOnlyList<CosmeticOffer> WeeklyNow() => Weekly(_c.LocalNow);

        /// <summary>Achète un article de la rotation de la semaine ; refusé (rien n'est dépensé) s'il est déjà possédé, hors rotation ou si les Gemmes manquent.</summary>
        public Result Buy(string cosmeticId)
        {
            var def = Find(cosmeticId);
            if (def == null) return Result.Fail(MetaErrors.Unknown);
            if (CosmeticOps.Owns(_c.Meta, cosmeticId)) return Result.Fail(MetaErrors.AlreadyDone);
            if (!Weekly(_c.LocalNow).Any(o => o.Def.Id == cosmeticId)) return Result.Fail(MetaErrors.Unknown);
            if (!_c.Session.Wallet.TrySpendGems(def.Price)) return Result.Fail(MetaErrors.NotEnoughGems);
            CosmeticOps.Unlock(_c.Meta, cosmeticId);
            _c.PublishWallet();
            _c.Touch(MetaArea.Boutique);
            return Result.Ok();
        }

        static ulong Fnv(string s)
        {
            ulong h = 14695981039346656037UL;
            foreach (char ch in s) { h ^= ch; h *= 1099511628211UL; }
            return h;
        }
    }
}
