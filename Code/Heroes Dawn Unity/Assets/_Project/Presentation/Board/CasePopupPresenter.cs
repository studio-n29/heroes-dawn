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
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Activities;
using UnityEngine;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Narration;
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

        /// <summary>Joue l'activité avec sa fenêtre : potion, dialogue, survie, pêche ou arène ; un type sans jeu dédié garde la fenêtre générique.</summary>
        async UniTask<ActivityResult> PlayActivityAsync(ActivityPlan plan, CancellationToken ct)
        {
            switch (plan.Def.Kind)
            {
                case ActivityKinds.Potion when plan.Level.PayloadAs<PotionRecipe>() != null: return await PlayPotionAsync(plan, ct);
                case ActivityKinds.Dialogue when plan.Level.PayloadAs<DialogueScript>() != null: return await PlayDialogueAsync(plan, ct);
                case ActivityKinds.Deflect:
                case ActivityKinds.RooftopRun:
                    return await PlayHeroGameAsync(plan, ct);
                case ActivityKinds.SnowSurvival:
                case ActivityKinds.CombatArena:
                    return await PlayStageAsync(plan, ct);
            }
            var view = await _modals.OpenAsync<IActivityPopupView>(ct);
            try { return await view.PlayAsync(ActivityData(plan), ct); }
            finally { _modals.Close(view); }
        }

        static readonly Dictionary<PotionTag, Color> TagColors = new Dictionary<PotionTag, Color>
        {
            [PotionTag.Heat] = new Color(1f, 0.55f, 0.3f), [PotionTag.Cold] = new Color(0.55f, 0.85f, 1f), [PotionTag.Calm] = new Color(0.78f, 0.7f, 1f),
            [PotionTag.Vigor] = new Color(0.55f, 0.9f, 0.5f), [PotionTag.Antidote] = new Color(0.4f, 0.85f, 0.8f), [PotionTag.Binding] = new Color(0.95f, 0.75f, 0.35f),
            [PotionTag.Light] = new Color(1f, 0.95f, 0.5f), [PotionTag.Earth] = new Color(0.75f, 0.6f, 0.45f),
        };

        async UniTask<ActivityResult> PlayPotionAsync(ActivityPlan plan, CancellationToken ct)
        {
            var request = PotionRequestFor(plan);
            var view = await _modals.OpenAsync<IPotionPopupView>(ct);
            try { return await view.PlayAsync(request, ct); }
            finally { _modals.Close(view); }
        }

        /// <summary>Potion : la partie, les ingrédients (nom + propriété), le livre d'indices et la présentation du patient.</summary>
        public PotionRequest PotionRequestFor(ActivityPlan plan)
        {
            var recipe = plan.Level.PayloadAs<PotionRecipe>()!;
            string Name(int i) => _loc.T("potion.name." + recipe.Ingredients[i].Tag);
            var sb = new System.Text.StringBuilder();
            sb.Append("<color=#FFD24D>").Append(_loc.T("potion.book.effects")).Append("</color>\n");
            foreach (var h in recipe.Hints.Where(h => h.Kind == "effect")) sb.Append(_loc.F("potion.hint.effect", Name(h.A), _loc.T("potion.tag." + (PotionTag)h.B))).Append('\n');
            sb.Append("\n<color=#FFD24D>").Append(_loc.T("potion.book.recipe")).Append("</color>\n");
            foreach (var h in recipe.Hints.Where(h => h.Kind == "order").OrderBy(h => h.B)) sb.Append(_loc.F("potion.hint.order", Name(h.A), h.B)).Append('\n');
            foreach (var h in recipe.Hints.Where(h => h.Kind == "quantity")) sb.Append(_loc.F("potion.hint.quantity", Name(h.A), h.B)).Append('\n');
            string ailment = _loc.T("potion.ailment." + recipe.Ailment);
            return new PotionRequest
            {
                Title = _loc.T(plan.Def.NameKey).ToUpperInvariant(),
                Intro = plan.Patient.HasValue ? _loc.F("potion.intro", _loc.MemberName(_session.Content.Member(plan.Patient.Value)), ailment) : _loc.F("potion.intro_nobody", ailment),
                Book = sb.ToString(),
                Game = new PotionGame(recipe),
                IngredientNames = recipe.Ingredients.Select(i => _loc.T("potion.ingredient." + i.Tag)).ToList(),
                IngredientColors = recipe.Ingredients.Select(i => TagColors[i.Tag]).ToList(),
                Loc = _loc,
            };
        }

        async UniTask<ActivityResult> PlayDialogueAsync(ActivityPlan plan, CancellationToken ct)
        {
            var request = DialogueRequestFor(plan);
            var view = await _modals.OpenAsync<IDialoguePopupView>(ct);
            try { return await view.PlayAsync(request, ct); }
            finally { _modals.Close(view); }
        }

        public DialogueRequest DialogueRequestFor(ActivityPlan plan)
            => new DialogueRequest
            {
                Title = _loc.T(plan.Def.NameKey).ToUpperInvariant(),
                Game = new DialogueGame(plan.Level.PayloadAs<DialogueScript>()!),
                Text = k => _loc.Has(k) ? _loc.T(k) : k.Substring(k.LastIndexOf('.') + 1),
                Loc = _loc,
            };

        async UniTask<ActivityResult> PlayHeroGameAsync(ActivityPlan plan, CancellationToken ct)
        {
            var request = HeroGameRequestFor(plan);
            var view = await _modals.OpenAsync<IHeroGamePopupView>(ct);
            try { return await view.PlayAsync(request, ct); }
            finally { _modals.Close(view); }
        }

        /// <summary>Mini-jeu 2D de héros : « Rempart » ou « Course des toits », avec ses consignes et l'illustration du héros.</summary>
        public HeroGameRequest HeroGameRequestFor(ActivityPlan plan)
        {
            bool deflect = plan.Def.Kind == ActivityKinds.Deflect;
            string key = deflect ? "hero.deflect" : "hero.run";
            var r = new HeroGameRequest
            {
                Kind = deflect ? HeroGameKind.Deflect : HeroGameKind.RooftopRun,
                Title = _loc.T(plan.Def.NameKey).ToUpperInvariant(),
                Instructions = _loc.T(key + ".instructions"),
                Start = _loc.T("hero.start"),
                Abandon = _loc.T("hero.abandon"),
                ScoreFormat = _loc.T(key + ".score"),
                TimeFormat = _loc.T("hero.time"),
                WonText = _loc.T("hero.won"),
                LostText = _loc.T("hero.lost"),
                HeroArt = N29.HeroesDawn.Presentation.Common.AgentArt.For(_session.Route.HeroId),
            };
            ulong seed = plan.Level.Seed;
            if (deflect) r.Deflect = new DeflectGame(seed, plan.Parameters);
            else r.Run = new RooftopRunGame(seed, plan.Parameters);
            return r;
        }

        async UniTask<ActivityResult> PlayStageAsync(ActivityPlan plan, CancellationToken ct)
        {
            var request = StageRequestFor(plan);
            var view = await _modals.OpenAsync<IStageGameView>(ct);
            try { return await view.PlayAsync(request, ct); }
            finally { _modals.Close(view); }
        }

        /// <summary>Activité 3D : la partie (ou, pour l'arène, la fabrique selon l'agent choisi dans l'escouade), les textes et les consignes.</summary>
        public StageRequest StageRequestFor(ActivityPlan plan)
        {
            var r = new StageRequest { Title = _loc.T(plan.Def.NameKey).ToUpperInvariant(), Level = plan.Level, Loc = _loc };
            switch (plan.Def.Kind)
            {
                case ActivityKinds.SnowSurvival:
                    r.Kind = StageKind.Survival; r.Instructions = _loc.T("stage.instr.survival");
                    r.Survival = new SnowSurvivalGame(plan.Level, plan.Parameters);
                    break;
                default:
                    r.Kind = StageKind.Arena; r.Instructions = _loc.T("stage.instr.arena") + "\n\n" + _loc.T("stage.choose_agent");
                    var members = _session.Squad.Where(m => !m.Exhausted).ToList();
                    if (members.Count == 0) members = _session.Squad.ToList();
                    var agents = members.Select(m => ArenaAgent.From(m.Def, m.Level, _session.Content.Balance)).ToList();
                    r.Agents = members.Select((m, i) => new AgentChoice { Name = _loc.MemberName(m.Def).ToUpperInvariant(), Detail = _loc.T("stage.skill." + agents[i].Primary.Id + ".detail") }).ToList();
                    r.ArenaFactory = i => new ArenaGame(plan.Level, plan.Parameters, agents[Math.Max(0, Math.Min(i, agents.Count - 1))]);
                    break;
            }
            return r;
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

        /// <summary>Dialogue de narration d'une case (avant son action ou après sa résolution) dans la boîte de dialogue ; rend la main à la fin ou si le joueur le passe.</summary>
        public async UniTask PlayNarrationAsync(NarrationEvent e, CancellationToken ct)
        {
            var lines = NarrationLines.Build(e, _session.Content, _loc);
            if (lines.Count == 0) return;
            var view = await _modals.OpenAsync<IDialogueBoxView>(ct);
            try { await view.PlayAsync(lines, ct); }
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
                if (rewards != null && rewards.Boosters > 0) text += _loc.F("board.zone_booster", rewards.Boosters);
                if (rewards != null && rewards.NewAgents.Count > 0)
                    text += _loc.F("agents.joined_rewards", string.Join(", ", rewards.NewAgents.Select(id => _loc.MemberName(_session.Content.Member(id)))));
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
