#nullable enable
using System;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Pastilles Crédits / PR, mises à jour sur l'événement WalletChanged.</summary>
    public sealed class WalletView : MonoBehaviour
    {
        ISessionReadModel _session = null!;
        ISubscriber<WalletChanged> _walletEvents = null!;
        ISubscriber<SaveStateChanged> _savingEvents = null!;
        IDisposable? _subs;
        TMP_Text _credits = null!;
        TMP_Text _pr = null!;
        TMP_Text _saving = null!;
        int _lastCredits = -1;
        int _lastPr = -1;

        public static WalletView Create(RectTransform parent, ISessionReadModel session, ISubscriber<WalletChanged> wallet, ISubscriber<SaveStateChanged> saving)
        {
            var rt = Ui.Rect(parent, "Wallet");
            var v = rt.gameObject.AddComponent<WalletView>();
            v._session = session;
            v._walletEvents = wallet;
            v._savingEvents = saving;
            rt.HLayout(16, 0, TextAnchor.MiddleRight);
            v._saving = Ui.Txt(rt, "", 22, Theme.TextMuted, TextAnchor.MiddleRight, FontKind.Regular, "Saving");
            v._saving.Pref(170, 60);
            v._credits = Pill(rt, Theme.Credits, "CR");
            v._pr = Pill(rt, Theme.Pr, "PR");
            return v;
        }

        static TMP_Text Pill(RectTransform parent, Color accent, string tag)
        {
            var fill = Ui.Panel(parent, "Pill" + tag, Theme.SurfaceDark, 3f, 4f, false);
            fill.Frame().Pref(230, 62);
            var badge = Ui.Img(fill.transform, "Badge", accent, Sprites.Circle);
            badge.rectTransform.At(0f, 0.5f, 6, 0, 46, 46);
            var tagText = Ui.Txt(badge.transform, tag, 20, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title);
            tagText.rectTransform.Stretch();
            var value = Ui.Txt(fill.transform, "0", 34, Color.white, TextAnchor.MiddleRight, FontKind.Title, "Value");
            value.rectTransform.Stretch(60, 0, 16, 0);
            return value;
        }

        void OnEnable()
        {
            if (_walletEvents == null) return;
            var bag = DisposableBag.CreateBuilder();
            _walletEvents.Subscribe(_ => Refresh()).AddTo(bag);
            _savingEvents.Subscribe(m => OnSaving(m.Saving)).AddTo(bag);
            _subs = bag.Build();
            Refresh();
        }

        void OnDisable()
        {
            _subs?.Dispose();
            _subs = null;
        }

        void OnSaving(bool saving) { if (_saving != null) _saving.text = saving ? Loc.T("ui.saving") : ""; }

        public void Refresh()
        {
            if (_session == null) return;
            int c = _session.Wallet.Credits, p = _session.Wallet.Pr;
            _credits.text = c.ToString("N0");
            _pr.text = p.ToString("N0");
            if (_lastCredits >= 0 && c != _lastCredits) Tween.Punch(_credits.transform.parent, 0.12f);
            if (_lastPr >= 0 && p != _lastPr) Tween.Punch(_pr.transform.parent, 0.12f);
            _lastCredits = c;
            _lastPr = p;
        }
    }

    /// <summary>Petit portrait générique : disque à la couleur de famille + initiale (en attendant les illustrations).</summary>
    public static class Portrait
    {
        public static Image Create(Transform parent, string name, Family family, float size, bool boss = false)
        {
            var ring = Ui.Img(parent, "Portrait", Theme.Ink, Sprites.Circle);
            ring.rectTransform.sizeDelta = new Vector2(size, size);
            var disc = Ui.Img(ring.transform, "Disc", Theme.FamilyColor(family), Sprites.Circle);
            disc.rectTransform.Stretch(4, 4, 4, 4);
            var shade = Ui.Img(disc.transform, "Shade", new Color(0, 0, 0, 0.25f), Sprites.VerticalGradient);
            shade.rectTransform.Stretch();
            shade.rectTransform.localScale = new Vector3(1, -1, 1);
            string initial = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();
            var t = Ui.Txt(disc.transform, initial, Mathf.RoundToInt(size * 0.5f), Color.white, TextAnchor.MiddleCenter, FontKind.Title);
            t.rectTransform.Stretch();
            t.Inked(2f);
            if (boss)
            {
                var crown = Ui.Img(ring.transform, "Boss", Theme.Primary, Sprites.Ring);
                crown.rectTransform.Stretch(-6, -6, -6, -6);
            }
            return ring;
        }
    }
}
