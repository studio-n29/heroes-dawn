#nullable enable
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Screens
{
    /// <summary>Écran titre : logo, connexion invité simulée, effacement de la sauvegarde.</summary>
    public sealed class LoginScreen : UiScreen
    {
        UiButton _play = null!;
        Text _status = null!;
        RectTransform _logo = null!;
        RectTransform _spinner = null!;
        bool _busy;
        CancellationTokenSource? _cts;

        protected override void Build()
        {
            Backdrop(Theme.WithAlpha(Theme.Perception, 0.35f), Theme.SurfaceDark);
            ArtBackdrop(Sprites.BackgroundLandscape, 1f, 0f);
            // Dégradé sombre en bas pour la lisibilité des boutons.
            var shade = Ui.Img(Rect, "BottomShade", new Color(0, 0, 0, 0.85f), Sprites.VerticalGradient);
            var srt = shade.rectTransform;
            srt.anchorMin = new Vector2(0, 0);
            srt.anchorMax = new Vector2(1, 0);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(0, 520);
            srt.anchoredPosition = new Vector2(0, 260);
            srt.localRotation = Quaternion.Euler(0, 0, 180); // opaque en bas, transparent en haut

            var titleSprite = Sprites.Title;
            if (titleSprite != null)
            {
                var logo = Ui.Img(Rect, "Title", Color.white, titleSprite);
                logo.preserveAspect = true;
                _logo = logo.rectTransform.At(0.5f, 1f, 0, -20, 1000, 580);
            }
            else
            {
                var title = Ui.Txt(Rect, Loc.T("game.title"), 128, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Title");
                title.Inked(5f);
                _logo = title.rectTransform.At(0.5f, 1f, 0, -120, 1400, 200);
            }

            var tagline = Ui.Txt(Rect, Loc.T("game.tagline"), 36, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Tagline");
            tagline.rectTransform.At(0.5f, 0f, 0, 250, 1400, 50);
            tagline.Inked(2f);

            _play = Ui.Btn(Rect, Loc.T("login.play"), Theme.Primary, Theme.TextDark, Play, 52);
            _play.Rect.At(0.5f, 0f, 0, 120, 560, 110);
            var pulse = _play.gameObject.AddComponent<Pulse>();
            pulse.Amplitude = 0.04f;
            pulse.Speed = 3.5f;

            _status = Ui.Txt(Rect, "", 30, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Body, "Status");
            _status.rectTransform.At(0.5f, 0f, 0, 60, 1200, 44);

            var spin = Ui.Img(Rect, "Spinner", Theme.Primary, Sprites.Ring);
            spin.type = Image.Type.Filled;
            spin.fillMethod = Image.FillMethod.Radial360;
            spin.fillAmount = 0.75f;
            _spinner = spin.rectTransform.At(0.5f, 0f, 0, 150, 70, 70);
            _spinner.gameObject.SetActive(false);

            var studio = Ui.Txt(Rect, Loc.T("game.studio"), 30, Theme.Primary, TextAnchor.LowerLeft, FontKind.Title, "Studio");
            studio.rectTransform.At(0f, 0f, 40, 24, 400, 50);
            var build = Ui.Txt(Rect, Loc.T("game.build"), 22, Theme.TextMuted, TextAnchor.LowerRight, FontKind.Regular, "Build");
            build.rectTransform.At(1f, 0f, -40, 70, 600, 40);
        }

        public override void OnShow()
        {
            SetBusy(false, "");
            if (_logo != null) Tween.PopIn(_logo, 0.5f);
        }

        public override void OnHide()
        {
            _cts?.Cancel();
        }

        void Update()
        {
            if (_spinner != null && _spinner.gameObject.activeSelf) _spinner.Rotate(0, 0, -360f * Time.unscaledDeltaTime);
        }

        void SetBusy(bool busy, string status)
        {
            _busy = busy;
            _play.Interactable = !busy;
            _play.gameObject.SetActive(!busy);
            _spinner.gameObject.SetActive(busy);
            _status.text = status;
        }

        /// <summary>Intention « Jouer » : connexion invité (ignorée si une connexion est déjà en cours).</summary>
        public void Play()
        {
            if (_busy) return;
            PlayAsync().Forget();
        }

        async UniTaskVoid PlayAsync()
        {
            SetBusy(true, Loc.T("login.connecting"));
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            try
            {
                var result = await Ctx.S.SignIn.ExecuteAsync(_cts.Token);
                if (this == null) return;
                if (result.IsFailure)
                {
                    SetBusy(false, Loc.Error(result.Error));
                    Ctx.Error(result.Error);
                    return;
                }
                var report = result.Value;
                var name = Ctx.S.Session.Account?.DisplayName ?? "";
                Ctx.Toast(Loc.F(report.NewProfile ? "login.welcome_new" : "login.welcome_back", name), ToastKind.Info);
                if (report.FatigueRecovered > 0) Ctx.Toast(Loc.F("login.recovered", report.FatigueRecovered), ToastKind.Reward);
                Ctx.Nav.Show(Ctx.Hub);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (this != null) SetBusy(false, Loc.T("login.fail_network"));
            }
        }
    }
}
