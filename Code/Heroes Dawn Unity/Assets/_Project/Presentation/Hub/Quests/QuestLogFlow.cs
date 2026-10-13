#nullable enable
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Suivi des quêtes du QG : ouvre la fenêtre (quêtes principales de la campagne, quêtes secondaires) et gère la récupération
    /// des récompenses. Se termine quand le joueur ferme la fenêtre.
    /// </summary>
    public sealed class QuestLogFlow
    {
        readonly QuestLog _quests;
        readonly IModalService _modals;
        readonly ILocalization _loc;
        readonly IToastService _toasts;

        public QuestLogFlow(QuestLog quests, IModalService modals, ILocalization loc, IToastService toasts)
        {
            _quests = quests; _modals = modals; _loc = loc; _toasts = toasts;
        }

        public async UniTask OpenAsync(ISessionReadModel session, CancellationToken ct)
        {
            var view = await _modals.OpenAsync<IQuestLogPopupView>(ct);
            void Claim(string id)
            {
                var result = _quests.Claim(id);
                if (result.IsFailure) { _toasts.Show(_loc.Error(result.Error!), ToastKind.Error); return; }
                _toasts.Show(_loc.F("quests.claimed", MetaFormat.RewardText(result.Value, _loc)), ToastKind.Reward);
                view.Refresh(Build(session));
            }
            view.ClaimClicked += Claim;
            try { await view.ShowAsync(Build(session), ct); }
            finally { view.ClaimClicked -= Claim; _modals.Close(view); }
        }

        public QuestLogData Build(ISessionReadModel session)
        {
            var main = _quests.Main().Select(m => new MainQuestRowData
            {
                Title = _loc.T(m.Zone.NameKey),
                Detail = m.Status == ZoneStatus.Locked ? _loc.T("quests.locked")
                       : m.Status == ZoneStatus.Done ? _loc.T("quests.main.done")
                       : _loc.Or("qg.fil." + m.Zone.Id.Value, "qg.fil.done"),
                State = _loc.T(m.Status == ZoneStatus.Done ? "quests.state.done" : m.Status == ZoneStatus.Available ? "quests.state.active" : "quests.state.locked"),
                Status = m.Status == ZoneStatus.Done ? 2 : m.Status == ZoneStatus.Available ? 1 : 0,
            }).ToArray();
            var side = _quests.Side().Select(v => new SideQuestRowData
            {
                Id = v.Quest.Id,
                Title = _loc.T(v.Quest.TitleKey),
                Description = _loc.T(v.Quest.DescKey),
                ProgressText = v.Claimed ? _loc.T("quests.state.done") : v.Progress + " / " + v.Quest.Target,
                Progress01 = v.Quest.Target == 0 ? 1f : v.Progress / (float)v.Quest.Target,
                RewardText = MetaFormat.RewardText(v.Quest.Reward, _loc),
                Claimable = v.Claimable,
                Claimed = v.Claimed,
                ClaimLabel = _loc.T("quests.claim"),
            }).ToArray();
            return new QuestLogData
            {
                Title = _loc.T("quests.title"), MainTitle = _loc.T("quests.main"), SideTitle = _loc.T("quests.side"),
                SideEmpty = _loc.T("quests.side.empty"), Close = _loc.T("ui.close"), Main = main, Side = side,
            };
        }
    }
}
