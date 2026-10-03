#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>
    /// Cases interactives du plateau (repos, événement, mini-jeu) et écran de zone sécurisée : ouvre la popup,
    /// attend le choix (obligatoire : non annulable), résout la case, ferme, puis rend les événements au plateau.
    /// </summary>
    public sealed class CasePopupPresenter
    {
        readonly IModalService _modals;
        readonly IResolveRestUseCase _rest;
        readonly IResolveNarrativeUseCase _narrative;
        readonly IResolveMiniGameUseCase _miniGame;
        readonly ISessionReadModel _session;
        readonly IToastService _toasts;
        readonly ILocalization _loc;

        public CasePopupPresenter(IModalService modals, IResolveRestUseCase rest, IResolveNarrativeUseCase narrative, IResolveMiniGameUseCase miniGame,
                                  ISessionReadModel session, IToastService toasts, ILocalization loc)
        {
            _modals = modals; _rest = rest; _narrative = narrative; _miniGame = miniGame; _session = session; _toasts = toasts; _loc = loc;
        }

        /// <summary>Joue la case ; liste vide si elle n'est pas interactive ou si sa résolution échoue.</summary>
        public UniTask<IReadOnlyList<BoardEvent>> RunAsync(CaseOutcome outcome, CancellationToken ct)
        {
            switch (outcome.Type)
            {
                case CaseType.Rest: return RestAsync(ct);
                case CaseType.Narrative: return NarrativeAsync(outcome.Case, ct);
                case CaseType.MiniGame: return MiniGameAsync(outcome.Case, ct);
                default: return UniTask.FromResult<IReadOnlyList<BoardEvent>>(Array.Empty<BoardEvent>());
            }
        }

        async UniTask<IReadOnlyList<BoardEvent>> RestAsync(CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IRestPopupView>(ct);
            try
            {
                bool take = await view.ChooseAsync(new RestPopupData
                {
                    Title = _loc.T("rest.title"), Body = _loc.T("rest.body"), Skip = _loc.T("rest.skip"), Take = _loc.T("rest.take"),
                }, ct);
                return Events(await _rest.ExecuteAsync(take, ct));
            }
            finally { _modals.Close(view); }
        }

        async UniTask<IReadOnlyList<BoardEvent>> NarrativeAsync(CaseId at, CancellationToken ct)
        {
            string key = "ev." + at.Value;
            var view = await _modals.OpenAsync<INarrativePopupView>(ct);
            try
            {
                var choice = await view.ChooseAsync(new NarrativePopupData
                {
                    Title = _loc.Or(key + ".title", "ev.default.title"),
                    Body = _loc.Or(key + ".body", "ev.default.body"),
                    Choices = new[]
                    {
                        new NarrativeChoiceData { Choice = NarrativeChoice.Credits, Label = _loc.F("ev.choice.credits", ResolveNarrativeUseCase.Credits), Tone = Tone.Credits },
                        new NarrativeChoiceData { Choice = NarrativeChoice.Research, Label = _loc.F("ev.choice.research", ResolveNarrativeUseCase.Pr), Tone = Tone.Pr },
                        new NarrativeChoiceData { Choice = NarrativeChoice.Morale, Label = _loc.F("ev.choice.morale", ResolveNarrativeUseCase.Morale), Tone = Tone.Success },
                    },
                }, ct);
                return Events(await _narrative.ExecuteAsync(choice, ct));
            }
            finally { _modals.Close(view); }
        }

        async UniTask<IReadOnlyList<BoardEvent>> MiniGameAsync(CaseId at, CancellationToken ct)
        {
            string key = "mg." + at.Value;
            var view = await _modals.OpenAsync<IMiniGamePopupView>(ct);
            try
            {
                bool success = await view.PlayAsync(new MiniGamePopupData
                {
                    Title = _loc.Or(key + ".title", "mg.default.title"),
                    Body = _loc.Or(key + ".body", "mg.default.body"),
                    Rules = _loc.T("mg.rules"),
                    Stop = _loc.T("mg.stop"),
                    ScoreFormat = _loc.T("mg.score"),
                }, ct);
                var b = _session.Content.Balance;
                if (success) _toasts.Show(_loc.F("mg.success", b.MiniGameCredits, b.MiniGamePr), ToastKind.Reward);
                else _toasts.Show(_loc.T("mg.fail"), ToastKind.Warning);
                return Events(await _miniGame.ExecuteAsync(success, ct));
            }
            finally { _modals.Close(view); }
        }

        /// <summary>Écran « zone sécurisée » : récompenses, puis attend « Vers le QG ».</summary>
        public async UniTask ShowZoneCompleteAsync(ZoneRewards? rewards, CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IZoneCompletePopupView>(ct);
            try
            {
                string text = rewards == null ? ""
                            : rewards.FirstClear ? _loc.F("board.zone_rewards", rewards.Credits, rewards.Pr)
                            : _loc.F("board.zone_rewards.replay", rewards.Credits);
                await view.ShowAsync(new ZoneCompleteData { Title = _loc.T("board.zone_complete"), Rewards = text, ToHq = _loc.T("board.to_hq") }, ct);
            }
            finally { _modals.Close(view); }
        }

        IReadOnlyList<BoardEvent> Events(Result<IReadOnlyList<BoardEvent>> r)
        {
            if (r.IsSuccess) return r.Value;
            _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
            return Array.Empty<BoardEvent>();
        }
    }
}
