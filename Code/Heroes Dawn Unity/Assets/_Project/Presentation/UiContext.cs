#nullable enable
using N29.HeroesDawn.Application.Events;
using N29.HeroesDawn.Application.Features;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;
using AppLogger = N29.HeroesDawn.Application.Ports.ILogger;

namespace N29.HeroesDawn.Presentation
{
    /// <summary>Services applicatifs fournis par la racine de composition.</summary>
    public sealed class UiServices
    {
        public GameSession Session { get; }
        public AuthService Auth { get; }
        public HubService Hub { get; }
        public ZoneService Zones { get; }
        public CombatService Combat { get; }
        public GameEvents Events { get; }
        public AppLogger Log { get; }
        public ZoneId FirstZone { get; }

        public UiServices(GameSession session, AuthService auth, HubService hub, ZoneService zones, CombatService combat,
                          GameEvents events, AppLogger log, ZoneId firstZone)
        {
            Session = session; Auth = auth; Hub = hub; Zones = zones; Combat = combat; Events = events; Log = log; FirstZone = firstZone;
        }
    }

    /// <summary>Tout ce dont les écrans ont besoin : services, navigation, modales, messages.</summary>
    public sealed class UiContext
    {
        public UiServices S { get; }
        public Navigator Nav { get; }
        public ModalLayer Modal { get; }
        public Toasts Toasts { get; }
        public RectTransform ScreensLayer { get; }

        public LoginScreen Login { get; internal set; } = null!;
        public HubScreen Hub { get; internal set; } = null!;
        public BoardScreen Board { get; internal set; } = null!;
        public CombatScreen Combat { get; internal set; } = null!;

        public UiContext(UiServices s, Navigator nav, ModalLayer modal, Toasts toasts, RectTransform screens)
        {
            S = s; Nav = nav; Modal = modal; Toasts = toasts; ScreensLayer = screens;
        }

        public void Toast(string text, ToastKind kind = ToastKind.Info)
        {
            Color c = kind == ToastKind.Reward ? Theme.Success : kind == ToastKind.Warning ? Theme.Fatigue
                    : kind == ToastKind.Error ? Theme.Danger : Theme.Info;
            Toasts.Show(text, c);
        }

        public void Error(DomainError? e) { if (e != null) Toast(Loc.Error(e), ToastKind.Error); }
    }
}
