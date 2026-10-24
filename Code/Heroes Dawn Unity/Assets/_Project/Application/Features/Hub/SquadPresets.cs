#nullable enable
using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    /// <summary>
    /// Compositions d'équipe enregistrées : trois emplacements qui mémorisent l'escouade du moment et la remettent en un geste.
    /// Un agent qui n'est plus disponible (parti en opération) est simplement ignoré au chargement.
    /// </summary>
    public sealed class SquadPresets
    {
        public const int Slots = 3;

        readonly GameSession _s;
        readonly SaveScheduler _save;
        readonly IPublisher<RosterChanged> _roster;

        public SquadPresets(GameSession session, SaveScheduler save, IPublisher<RosterChanged> roster)
        {
            _s = session; _save = save; _roster = roster;
        }

        List<SquadPresetSave> Store
        {
            get
            {
                var list = _s.Meta.squadPresets;
                while (list.Count < Slots) list.Add(new SquadPresetSave());
                return list;
            }
        }

        /// <summary>Agents enregistrés à cet emplacement (vide : emplacement libre).</summary>
        public IReadOnlyList<string> Members(int slot) => slot < 0 || slot >= Slots ? new List<string>() : Store[slot].members;

        public bool IsSaved(int slot) => Members(slot).Count > 0;

        /// <summary>Enregistre l'escouade actuelle à cet emplacement (écrase l'ancienne composition).</summary>
        public Result Save(int slot)
        {
            if (slot < 0 || slot >= Slots) return Result.Fail(HubErrors.PresetEmpty);
            var squad = _s.Squad.Select(m => m.Id.Value).ToList();
            if (squad.Count == 0) return Result.Fail(HubErrors.SquadEmpty);
            Store[slot].members = squad;
            _save.RequestSave();
            return Result.Ok();
        }

        /// <summary>Remplace l'escouade par la composition enregistrée (hors partie en cours).</summary>
        public Result Load(int slot)
        {
            if (slot < 0 || slot >= Slots || !IsSaved(slot)) return Result.Fail(HubErrors.PresetEmpty);
            if (_s.Run != null) return Result.Fail(HubErrors.InMission);
            var wanted = new List<N29.HeroesDawn.Application.Session.TeamMember>();
            foreach (var id in Store[slot].members)
            {
                var m = _s.Roster.FirstOrDefault(x => x.Id.Value == id);
                if (m == null || _s.Meta.expeditions.Exists(e => e.members.Contains(id))) continue;
                if (wanted.Count < MetaRules.SquadSize) wanted.Add(m);
            }
            if (wanted.Count == 0) return Result.Fail(HubErrors.PresetUnavailable);
            foreach (var m in _s.Roster) m.InSquad = wanted.Contains(m);
            _roster.PublishRoster();
            _save.RequestSave();
            return Result.Ok();
        }
    }
}
