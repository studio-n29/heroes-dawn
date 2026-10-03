#nullable enable
using System;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Affiche en toasts les messages publiés sur le bus ; un message peut être une clé de localisation.</summary>
    public sealed class ToastRelay : IStartable, IDisposable
    {
        readonly ISubscriber<ToastRequested> _messages;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        IDisposable? _sub;

        public ToastRelay(ISubscriber<ToastRequested> messages, IToastService toasts, ILocalization loc)
        {
            _messages = messages; _toasts = toasts; _loc = loc;
        }

        public void Start() => _sub = _messages.Subscribe(t => _toasts.Show(_loc.Has(t.Text) ? _loc.T(t.Text) : t.Text, t.Kind));

        public void Dispose()
        {
            _sub?.Dispose();
            _sub = null;
        }
    }
}
