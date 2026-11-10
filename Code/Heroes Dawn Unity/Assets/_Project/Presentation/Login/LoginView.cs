#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Écran titre (UI Toolkit, planche 1.1) : panorama, héroïne, logotype, Jouer, Se connecter, état de connexion.</summary>
    public sealed class LoginView : HdView, ILoginView
    {
        Button _play = null!;
        Button _account = null!;
        Label _status = null!;
        VisualElement _spinner = null!;
        VisualElement _lockup = null!;
        IVisualElementScheduledItem? _spin;
        float _angle;

        public event Action? PlayClicked;
        public event Action? AccountClicked;

        /// <summary>Bouton Jouer (tests PlayMode : appui réel par la validation de la manette).</summary>
        public HdButtonRef PlayButton { get; private set; } = null!;

        /// <summary>Bouton « Se connecter » (tests PlayMode).</summary>
        public HdButtonRef AccountButton { get; private set; } = null!;
        public string AccountLabel => _account.text;

        protected override void Bind()
        {
            _play = Q<Button>("play");
            _account = Q<Button>("account");
            _status = Q<Label>("status");
            _spinner = Q<VisualElement>("spinner");
            _lockup = Q<VisualElement>("lockup");
            PlayButton = new HdButtonRef(_play, this);
            AccountButton = new HdButtonRef(_account, this);
            _play.text = Loc.T("login.play");
            _account.text = Loc.T("login.account");
            Q<Label>("studio").text = Loc.T("game.studio");
            _play.clicked += () => PlayClicked?.Invoke();
            _account.clicked += () => AccountClicked?.Invoke();
            _play.Focus();
        }

        public void SetAccountLabel(string label) => _account.text = label;

        public void SetBusy(bool busy, string text)
        {
            _play.SetEnabled(!busy);
            _play.style.display = busy ? DisplayStyle.None : DisplayStyle.Flex;
            _spinner.style.display = busy ? DisplayStyle.Flex : DisplayStyle.None;
            _status.text = text;
            _spin?.Pause();
            if (!busy) return;
            // Le disque tourne tant que la connexion est en cours.
            _spin = _spinner.schedule.Execute(() =>
            {
                _angle = (_angle + 6f) % 360f;
                _spinner.style.rotate = new Rotate(new Angle(_angle, AngleUnit.Degree));
            }).Every(16);
        }

        public void PlayIntro()
        {
            _lockup.RemoveFromClassList("login__lockup--in");
            _lockup.schedule.Execute(() => _lockup.AddToClassList("login__lockup--in")).StartingIn(30);
        }
    }
}
