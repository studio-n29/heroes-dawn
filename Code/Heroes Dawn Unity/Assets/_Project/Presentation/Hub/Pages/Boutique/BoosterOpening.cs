#nullable enable
using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ouvre un booster de la réserve et le montre dans la popup animée (boutique et grimoire).</summary>
    public static class BoosterOpening
    {
        static int RarityRank(ItemRarity r) => r switch { ItemRarity.Legendary => 3, ItemRarity.Epic => 2, ItemRarity.Rare => 1, _ => 0 };

        public static BoosterOpeningData Build(CollectionDef defs, System.Collections.Generic.IReadOnlyList<OpenedCard> cards, ILocalization loc)
        {
            return new BoosterOpeningData
            {
                Title = loc.T("booster.title"),
                TapHint = loc.T("booster.tap"),
                SkipLabel = loc.T("booster.skip"),
                ContinueLabel = loc.T("ui.continue"),
                NewLabel = loc.T("grimoire.new_card").ToUpperInvariant(),
                Cards = cards.Select(c =>
                {
                    var def = defs.Card(c.Id);
                    var name = CollectionText.CardName(loc, def);
                    return new BoosterCardData
                    {
                        Name = name,
                        RarityName = CollectionText.RarityName(loc, def.Rarity),
                        RarityColor = CollectionText.RarityColor(def.Rarity),
                        RarityRank = RarityRank(def.Rarity),
                        Glyph = CollectionText.Tag(name),
                        IsNew = c.IsNew,
                    };
                }).ToArray(),
            };
        }

        /// <summary>Ouvre un booster standard ; faux (et erreur affichée) s'il n'y en a pas en réserve.</summary>
        public static async UniTask<bool> OpenAndShowAsync(CollectionBook book, CollectionDef defs, IModalService modals, ILocalization loc,
                                                           IToastService toasts, CancellationToken ct)
        {
            var result = book.OpenBoosterDetailed(BoosterDef.StandardId);
            if (result.IsFailure) { toasts.Show(loc.Error(result.Error!), ToastKind.Error); return false; }
            var view = await modals.OpenAsync<IBoosterOpeningPopupView>(ct);
            try { await view.ShowAsync(Build(defs, result.Value, loc), ct); }
            catch (OperationCanceledException) { }
            finally { modals.Close(view); }
            return true;
        }
    }
}
