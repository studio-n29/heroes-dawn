#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    public readonly struct SecondaryView
    {
        public readonly string ZoneId;
        public readonly TimeSpan Left;
        public SecondaryView(string zoneId, TimeSpan left) { ZoneId = zoneId; Left = left; }
    }

    /// <summary>
    /// Objectifs secondaires « en temps réel » (simulés en local) : une zone déjà validée est reprise par les pillards,
    /// la rejouer et la valider avant l'échéance rapporte une récompense.
    /// </summary>
    public sealed class SecondaryObjectives
    {
        public static readonly TimeSpan Duration = TimeSpan.FromHours(3);
        public static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(10);
        public static readonly Reward Prize = new Reward(credits: 400, gems: 5, passXp: 25);

        readonly MetaContext _c;
        readonly PassProgress _pass;

        public SecondaryObjectives(MetaContext context, PassProgress pass) { _c = context; _pass = pass; }

        public SecondaryView? Active
        {
            get
            {
                Refresh();
                var list = _c.Meta.secondary;
                if (list.Count == 0) return null;
                var left = new DateTime(list[0].expiresUtcTicks, DateTimeKind.Utc) - _c.UtcNow;
                return new SecondaryView(list[0].zoneId, left);
            }
        }

        /// <summary>Retire l'objectif expiré, en crée un nouveau si une zone est validée et le délai de repos écoulé.</summary>
        public void Refresh()
        {
            Expire();
            var m = _c.Meta;
            var now = _c.UtcNow;
            if (m.secondary.Count > 0 || m.secondaryNextUtcTicks > now.Ticks) return;
            var done = new List<string>(_c.Session.CompletedZones);
            if (done.Count == 0) return;
            done.Sort(StringComparer.Ordinal);
            var h = new Fnv1a64();
            h.Add(now.Ticks / TimeSpan.TicksPerMinute);
            var zone = done[new Pcg32(h.Value).Range(0, done.Count)];
            m.secondary.Add(new SecondarySave { zoneId = zone, expiresUtcTicks = (now + Duration).Ticks });
            _c.Touch(MetaArea.Objectives);
        }

        void Expire()
        {
            var m = _c.Meta;
            var now = _c.UtcNow;
            if (m.secondary.Count > 0 && m.secondary[0].expiresUtcTicks <= now.Ticks)
            {
                m.secondary.Clear();
                m.secondaryNextUtcTicks = (now + Cooldown).Ticks;
                _c.Touch(MetaArea.Objectives);
            }
        }

        /// <summary>Une zone vient d être validée (1re fois ou rejeu) : si c'est celle de l'objectif, il est rempli.</summary>
        public Reward? OnZoneValidated(string zoneId)
        {
            Expire();
            var m = _c.Meta;
            if (m.secondary.Count == 0)
            {
                // Première zone validée : le premier objectif secondaire arrive dans quelques minutes.
                if (m.secondaryNextUtcTicks == 0) { m.secondaryNextUtcTicks = (_c.UtcNow + TimeSpan.FromMinutes(2)).Ticks; _c.Touch(MetaArea.Objectives); }
                return null;
            }
            if (m.secondary[0].zoneId != zoneId) return null;
            m.secondary.Clear();
            m.secondaryNextUtcTicks = (_c.UtcNow + Cooldown).Ticks;
            _c.Grant(Prize);
            _pass.AddXp(Prize.PassXp);
            _c.Touch(MetaArea.Objectives);
            return Prize;
        }
    }
}
