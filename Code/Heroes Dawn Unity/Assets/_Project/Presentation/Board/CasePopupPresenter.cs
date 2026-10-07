#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Audio;
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
        readonly IPlanActivityUseCase _plan;
        readonly IResolveActivityUseCase _activity;
        readonly ISessionReadModel _session;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly IAudioService _audio;

        public CasePopupPresenter(IModalService modals, IResolveRestUseCase rest, IResolveNarrativeUseCase narrative, IPlanActivityUseCase plan, IResolveActivityUseCase activity,
                                  ISessionReadModel session, IToastService toasts, ILocalization loc, IAudioService? audio = null)
        {
            _modals = modals; _rest = rest; _narrative = narrative; _plan = plan; _activity = activity; _session = session; _toasts = toasts; _loc = loc;
            _audio = audio ?? NullAudioService.Instance;
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

        /// <summary>Nombre maximal d'activités enchaînées sur une même case (un échec peut lancer une activité liée).</summary>
        const int MaxChain = 4;

        /// <summary>
        /// Case Mini-jeu : prépare l'activité de la case (framework d'activités), la fait jouer (réflexe historique ou fenêtre d'activité),
        /// la résout (gains, malus, suite) et enchaîne tant qu'un échec lance une autre activité.
        /// </summary>
        async UniTask<IReadOnlyList<BoardEvent>> MiniGameAsync(CaseId at, CancellationToken ct)
        {
            var all = new List<BoardEvent>();
            for (int step = 0; step < MaxChain; step++)
            {
                var planned = _plan.Execute();
                if (planned.IsFailure)
                {
                    _toasts.Show(_loc.Error(planned.Error!), ToastKind.Error);
                    break;
                }
                var plan = planned.Value;
                var result = plan.Def.Kind == ActivityKinds.Reflex ? await PlayReflexAsync(at, ct) : await PlayActivityAsync(plan, ct);
                _audio.Play(result.Success ? SfxId.MiniGameHit : SfxId.MiniGameMiss);
                ToastOutcome(ActivityRules.Resolve(plan, result));
                var events = Events(await _activity.ExecuteAsync(result, ct));
                all.AddRange(events);
                var resolved = events.OfType<ActivityResolvedEvent>().FirstOrDefault();
                if (resolved == null || resolved.Outcome.FollowUp == null || resolved.Outcome.FollowUp.Type != FollowUpType.Activity) break;
            }
            return all;
        }

        async UniTask<ActivityResult> PlayReflexAsync(CaseId at, CancellationToken ct)
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
                return new ActivityResult(success);
            }
            finally { _modals.Close(view); }
        }

        async UniTask<ActivityResult> PlayActivityAsync(ActivityPlan plan, CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IActivityPopupView>(ct);
            try { return await view.PlayAsync(ActivityData(plan), ct); }
            finally { _modals.Close(view); }
        }

        /// <summary>Contenu de la fenêtre d'activité : description, décor, patient, réglages et résumé du niveau généré.</summary>
        ActivityPopupData ActivityData(ActivityPlan plan)
        {
            var def = plan.Def;
            var lines = new List<string>
            {
                _loc.F("activity.env", _loc.T("env.biome." + plan.Environment.Biome), _loc.T("env.weather." + plan.Environment.Weather)),
            };
            if (plan.Patient.HasValue && plan.Level.PayloadAs<PotionRecipe>() is PotionRecipe recipe)
                lines.Add(_loc.F("activity.patient", _loc.MemberName(_session.Content.Member(plan.Patient.Value)), _loc.T("potion.ailment." + recipe.Ailment)));
            lines.Add(_loc.F("activity.level", plan.Level.Primitives.Count, (plan.Seed % 100000).ToString()));
            lines.Add(_loc.T("activity.params"));
            foreach (var kv in plan.Parameters.Values.OrderBy(k => k.Key, StringComparer.Ordinal))
                lines.Add("  " + _loc.Or("activity.param." + kv.Key, "activity.params").TrimEnd(':') + " : " + kv.Value);
            int need = plan.Parameters.Get("success_fish");
            return new ActivityPopupData
            {
                Title = _loc.T(def.NameKey).ToUpperInvariant(),
                Body = _loc.T(def.DescriptionKey),
                Lines = lines.ToArray(),
                Wip = _loc.T("activity.wip"),
                WinLabel = _loc.T("activity.simulate_win"),
                FailLabel = _loc.T("activity.simulate_fail"),
                WinScore = need,
                FailScore = need / 2,
            };
        }

        void ToastOutcome(ActivityOutcome o)
        {
            if (o.Result.Success) _toasts.Show(_loc.F("mg.success", o.Credits, o.Pr), ToastKind.Reward);
            else
            {
                _toasts.Show(_loc.T("mg.fail"), ToastKind.Warning);
                if (o.Credits > 0) _toasts.Show(_loc.F("mg.success", o.Credits, o.Pr), ToastKind.Reward); // pièces au prorata (pêche)
            }
            if (o.Malus != null)
                _toasts.Show(_loc.F("activity.malus_applied", _loc.MemberName(_session.Content.Member(o.Malus.Member)), -o.Malus.StatPercent,
                                    _loc.Or(o.Malus.ReasonKey, "activity.params")), ToastKind.Warning);
            if (o.FollowUp != null && o.FollowUp.Type == FollowUpType.Combat) _toasts.Show(_loc.T("activity.followup_combat"), ToastKind.Warning);
            else if (o.FollowUp != null && o.FollowUp.Type == FollowUpType.Activity) _toasts.Show(_loc.T("activity.followup_activity"), ToastKind.Info);
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
