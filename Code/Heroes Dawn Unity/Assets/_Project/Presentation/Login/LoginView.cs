#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Écran titre (prefab Screens/Login) : illustration, logo, Jouer, état de connexion.</summary>
    public sealed class LoginView : ViewBase, ILoginView
    {
        [SerializeField] UiButton play = null!;
        [SerializeField] TMP_Text status = null!;
        [SerializeField] RectTransform spinner = null!;
        [SerializeField] RectTransform logo = null!;

        public event Action? PlayClicked;

        /// <summary>Bouton Jouer (tests PlayMode : clic réel).</summary>
        public Button PlayButton => play.Button;

        void Awake() => play.Button.onClick.AddListener(() => PlayClicked?.Invoke());

        void Update()
        {
            if (spinner.gameObject.activeSelf) spinner.Rotate(0, 0, -360f * Time.unscaledDeltaTime);
        }

        public void SetBusy(bool busy, string text)
        {
            play.Interactable = !busy;
            play.gameObject.SetActive(!busy);
            spinner.gameObject.SetActive(busy);
            status.text = text;
        }

        public void PlayIntro() => Tween.PopIn(logo, 0.5f);
    }
}
