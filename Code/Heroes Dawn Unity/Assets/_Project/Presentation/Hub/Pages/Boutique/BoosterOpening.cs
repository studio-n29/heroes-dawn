#nullable enable
using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ouvre un booster de la réserve et le montre dans la popup animée (boutique et grimoire).</summary>
    public static class BoosterOpening
    {
        static int RarityRank(ItemRarity r) => r switch { ItemRarity.Legendary => 3, ItemRarity.Epic => 2, ItemRarity.Rare => 1, _ => 0 };

        public static BoosterOpeningData Build(CollectionDef defs, System.Collections.Generic.IReadOnlyList<OpenedCard> cards, ILocalization loc, SkillOwnerStats? stats = null, int remainingBoosters = 0)
        {
            return new BoosterOpeningData
            {
                Title = loc.T("booster.title"),
                TapHint = loc.T("booster.tap"),
                SkipLabel = loc.T("booster.skip"),
                ContinueLabel = loc.T("ui.continue"),
                NewLabel = loc.T("grimoire.new_card").ToUpperInvariant(),
                DuplicateLabel = loc.T("booster.duplicate").ToUpperInvariant(),
                FlipHint = loc.T("booster.flip"),
                RecapTitle = loc.T("booster.summary"),
                RecapBestLabel = loc.T("booster.best"),
                OpenAnotherLabel = loc.T("booster.again"),
                RemainingBoosters = remainingBoosters,
                RecapLine = (n, d) => loc.F("booster.recap", n, d),
                ReceivedLine = n => loc.F("booster.received", n),
                Cards = cards.Select(c =>
                {
                    var def = defs.Card(c.Id);
                    var name = CollectionText.CardName(loc, def);
                    return new BoosterCardData
                    {
                        Name = name,
                        RarityName = CollectionText.RarityName(loc, CollectionText.StarsRarity(def.Stars)),
                        RarityColor = CollectionText.RarityColor(CollectionText.StarsRarity(def.Stars)),
                        RarityRank = RarityRank(CollectionText.StarsRarity(def.Stars)),
                        Glyph = CollectionText.Tag(name),
                        Art = AgentArt.Card(c.Id.Value),
                        Description = stats.HasValue ? loc.DescribeSkill(def.Skill, stats.Value.Atk, stats.Value.MaxHp) : loc.DescribeSkill(def.Skill),
                        IsNew = c.IsNew,
                    };
                }).ToArray(),
            };
        }

        /// <summary>
        /// Ouvre un booster ; faux (et erreur affichée) s'il n'y en a pas en réserve. Si le joueur demande « Ouvrir un autre booster »,
        /// en ouvre un autre du même type tant qu'il en reste.
        /// </summary>
        public static async UniTask<bool> OpenAndShowAsync(CollectionBook book, CollectionDef defs, IModalService modals, ILocalization loc,
                                                           IToastService toasts, CancellationToken ct, SkillOwnerStats? stats = null, string boosterId = BoosterDef.StandardId,
                                                           IAudioService? audio = null)
        {
            bool first = true;
            while (true)
            {
                var result = book.OpenBoosterDetailed(boosterId);
                if (result.IsFailure) { if (first) toasts.Show(loc.Error(result.Error!), ToastKind.Error); return !first; }
                first = false;
                var data = Build(defs, result.Value, loc, stats, book.BoosterStock(boosterId));
                if (audio != null) data.PlaySfx = id => audio.Play(id);
                var view = await modals.OpenAsync<IBoosterOpeningPopupView>(ct);
                bool again = false;
                try { again = await view.ShowAsync(data, ct); }
                catch (OperationCanceledException) { }
                finally { modals.Close(view); }
                if (!again || ct.IsCancellationRequested) return true;
            }
        }
    }
}
