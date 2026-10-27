#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Presentation.Account;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Fenêtre Compte de test : enregistre les affichages et laisse le test simuler les clics.</summary>
    public sealed class FakeAccountPopup : FakePopup, IAccountPopupView
    {
        public event Action<AccountMode>? ModeChanged;
        public event Action<AccountForm>? SubmitClicked;
        public event Action<AuthProvider>? ProviderClicked;
        public event Action<string>? ForgotClicked;
        public event Action? CloseClicked;

        public AccountPopupData? Data;
        public AccountMode Mode;
        public readonly List<bool> BusyStates = new List<bool>();
        public readonly List<(string Text, bool IsError)> Messages = new List<(string, bool)>();

        public void Show(AccountPopupData data) { Data = data; Mode = data.Mode; }
        public void SetMode(AccountMode mode, string title, string submit) => Mode = mode;
        public void SetBusy(bool busy, string text) => BusyStates.Add(busy);
        public void ShowMessage(string text, bool isError) => Messages.Add((text, isError));
        public override void RequestCancel() => CloseClicked?.Invoke();

        public void ClickMode(AccountMode m) => ModeChanged?.Invoke(m);
        public void ClickSubmit(string email, string password, string name = "") => SubmitClicked?.Invoke(new AccountForm { Email = email, Password = password, DisplayName = name });
        public void ClickProvider(AuthProvider p) => ProviderClicked?.Invoke(p);
        public void ClickForgot(string email) => ForgotClicked?.Invoke(email);
        public void ClickClose() => CloseClicked?.Invoke();
        public (string Text, bool IsError) LastMessage => Messages[Messages.Count - 1];
    }

    /// <summary>Confirmation qui répond tout de suite par <see cref="Yes"/>.</summary>
    public sealed class FakeAutoConfirmPopup : FakePopup, N29.HeroesDawn.Presentation.Mvp.IConfirmPopupView
    {
        public bool Yes;
        public N29.HeroesDawn.Presentation.Mvp.ConfirmData? Asked;
        public UniTask<bool> AskAsync(N29.HeroesDawn.Presentation.Mvp.ConfirmData data, CancellationToken ct) { Asked = data; return UniTask.FromResult(Yes); }
    }

    /// <summary>Choix de progression de test : rend la réponse préparée (nul = Retour).</summary>
    public sealed class FakeProgressChoicePopup : FakePopup, IProgressChoicePopupView
    {
        public readonly Queue<ChoiceKind?> Answers = new Queue<ChoiceKind?>();
        public readonly List<ProgressChoiceData> Asked = new List<ProgressChoiceData>();

        public UniTask<ChoiceKind?> AskAsync(ProgressChoiceData data, CancellationToken ct)
        {
            Asked.Add(data);
            return UniTask.FromResult(Answers.Count > 0 ? Answers.Dequeue() : null);
        }
    }
}
