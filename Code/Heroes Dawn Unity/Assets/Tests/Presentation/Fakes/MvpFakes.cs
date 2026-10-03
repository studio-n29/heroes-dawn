#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Popup de test : enregistre les demandes d'annulation.</summary>
    public class FakePopup : IPopupView
    {
        public bool Cancelable { get; set; }
        public int CancelRequests { get; private set; }
        public virtual void RequestCancel() => CancelRequests++;
    }

    /// <summary>Confirmation de test : répond par <see cref="Answer"/> ; Retour = non.</summary>
    public sealed class FakeConfirmPopup : FakePopup, IConfirmPopupView
    {
        UniTaskCompletionSource<bool>? _tcs;
        public ConfirmData? Asked { get; private set; }
        public FakeConfirmPopup() { Cancelable = true; }

        public UniTask<bool> AskAsync(ConfirmData d, CancellationToken ct)
        {
            Asked = d;
            _tcs = new UniTaskCompletionSource<bool>();
            return _tcs.Task;
        }

        public void Answer(bool ok) => _tcs?.TrySetResult(ok);
        public override void RequestCancel() { base.RequestCancel(); Answer(false); }
    }

    /// <summary>Hôte de popups de test : crée un double par type demandé (fabriques enregistrables), garde la trace.</summary>
    public sealed class FakeModalHost : IPopupHost
    {
        readonly Dictionary<Type, Func<IPopupView>> _factories = new Dictionary<Type, Func<IPopupView>>();
        public readonly List<IPopupView> Spawned = new List<IPopupView>();
        public readonly List<IPopupView> Despawned = new List<IPopupView>();

        public FakeModalHost()
        {
            Register<IConfirmPopupView>(() => new FakeConfirmPopup());
            Register<FakePopup>(() => new FakePopup());
        }

        public void Register<TView>(Func<IPopupView> factory) => _factories[typeof(TView)] = factory;

        public TView Spawn<TView>() where TView : class, IPopupView
        {
            var v = (TView)_factories[typeof(TView)]();
            Spawned.Add(v);
            return v;
        }

        public void Despawn(IPopupView v) => Despawned.Add(v);
    }

    public sealed class FakeBackInput : IBackInput
    {
        public bool Pressed;
        public bool BackPressedThisFrame => Pressed;
    }

    public sealed class FakeFrameClock : IFrameClock
    {
        public float Dt = 0.1f;
        public float UnscaledDeltaTime => Dt;
    }

    public sealed class FakeToasts : IToastService
    {
        public readonly List<(string Text, ToastKind Kind)> Shown = new List<(string, ToastKind)>();
        public void Show(string text, ToastKind kind) => Shown.Add((text, kind));
    }

    public sealed class FakeWalletView : IWalletView
    {
        public WalletData Last;
        public int Shows;
        public bool Saving;
        public void Show(WalletData d) { Last = d; Shows++; }
        public void SetSaving(bool saving) => Saving = saving;
    }

    /// <summary>Vue minimale : affichage et masquage pilotés par le test.</summary>
    public class FakeView : IView
    {
        public event Action? Shown;
        public event Action? Hidden;
        public bool IsVisible { get; private set; }
        public void Show() { IsVisible = true; Shown?.Invoke(); }
        public void Hide() { IsVisible = false; Hidden?.Invoke(); }
    }
}
