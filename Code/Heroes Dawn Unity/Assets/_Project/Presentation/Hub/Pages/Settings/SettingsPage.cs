#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Features.Tutorial;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    public enum SettingKind { Stepper, Toggle, Button, Info }

    /// <summary>Ligne de réglage : curseur à pas (− valeur +), interrupteur, bouton d'action ou simple information.</summary>
    public struct SettingRowData
    {
        public string Label;
        public string Hint;
        public SettingKind Kind;
        public string Value;
        public bool On;
        public bool Danger;

        public static SettingRowData Info(string label, string value = "", string hint = "") => new SettingRowData { Label = label, Value = value, Hint = hint, Kind = SettingKind.Info };
    }

    public struct SettingsPageData
    {
        public string[] Categories;
        public int Active;
        public string CategoryTitle;
        public SettingRowData[] Rows;
        public bool CanReset;
        public string ResetLabel;
    }

    public interface ISettingsPageView : IView
    {
        event Action<int> CategorySelected;
        /// <summary>Ligne puis action : 0 = bouton / interrupteur / « − », 1 = « + ».</summary>
        event Action<int, int> RowAction;
        event Action ResetCategoryClicked;
        void Show(SettingsPageData data);
    }

    /// <summary>
    /// Paramètres (M80-M85) : huit catégories à gauche, réglages à droite, appliqués immédiatement.
    /// Audio, Graphismes, Accessibilité, Langue, Notifications, Contrôles, Compte & confidentialité, Support & aide.
    /// </summary>
    public sealed class SettingsPagePresenter : PresenterBase<ISettingsPageView>, IPageTitle
    {
        static readonly SettingsCategory[] Categories =
        {
            SettingsCategory.Audio, SettingsCategory.Graphics, SettingsCategory.Accessibility, SettingsCategory.Language,
            SettingsCategory.Notifications, SettingsCategory.Controls, SettingsCategory.Account, SettingsCategory.Support,
        };

        readonly ISessionReadModel _session;
        readonly ISignOutUseCase _signOut;
        readonly IResetProgressUseCase _reset;
        readonly IOpenShopUseCase _openShop;
        readonly PlayerSettings _settings;
        readonly TutorialGuide _tutorial;
        readonly HubNavigator _nav;
        readonly IModalService _modals;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly IPublisher<NavigateRequested> _navigate;
        readonly ISubscriber<MetaChanged> _changed;
        readonly PageAction _action;
        readonly List<Action<int>> _handlers = new List<Action<int>>();
        IDisposable? _sub;
        int _active;

        public SettingsPagePresenter(ISettingsPageView view, ISessionReadModel session, ISignOutUseCase signOut, IResetProgressUseCase reset,
                                     IOpenShopUseCase openShop, PlayerSettings settings, TutorialGuide tutorial, HubNavigator nav,
                                     IModalService modals, IToastService toasts, ILocalization loc, IPublisher<NavigateRequested> navigate,
                                     ISubscriber<MetaChanged> changed)
            : base(view, null)
        {
            _session = session; _signOut = signOut; _reset = reset; _openShop = openShop; _settings = settings; _tutorial = tutorial; _nav = nav;
            _modals = modals; _toasts = toasts; _loc = loc; _navigate = navigate; _changed = changed;
            _action = new PageAction(toasts, loc);
        }

        public string Title => _loc.T("page.settings");

        protected override void OnStart()
        {
            View.CategorySelected += SelectCategory;
            View.RowAction += OnRow;
            View.ResetCategoryClicked += ResetCategory;
        }

        protected override void OnShown()
        {
            _sub?.Dispose();
            _sub = _changed.Subscribe(m => { if (m.Area == MetaArea.Settings) Refresh(); });
            Refresh();
        }

        protected override void OnHidden()
        {
            _sub?.Dispose();
            _sub = null;
        }

        protected override void OnDispose()
        {
            _sub?.Dispose();
            View.CategorySelected -= SelectCategory;
            View.RowAction -= OnRow;
            View.ResetCategoryClicked -= ResetCategory;
        }

        void SelectCategory(int index)
        {
            if (index < 0 || index >= Categories.Length) return;
            _active = index;
            Refresh();
        }

        void OnRow(int row, int action)
        {
            if (row >= 0 && row < _handlers.Count) _handlers[row]?.Invoke(action);
        }

        void ResetCategory()
        {
            _settings.Reset(Categories[_active]);
            _toasts.Show(_loc.T("settings.reset_category.done"), ToastKind.Info);
        }

        static string Key(SettingsCategory c) => "settings.cat." + c;

        void Refresh()
        {
            if (Disposed) return;
            _handlers.Clear();
            var rows = new List<SettingRowData>();
            Build(Categories[_active], rows);
            var titles = new string[Categories.Length];
            for (int i = 0; i < titles.Length; i++) titles[i] = _loc.T(Key(Categories[i]));
            var cat = Categories[_active];
            View.Show(new SettingsPageData
            {
                Categories = titles,
                Active = _active,
                CategoryTitle = _loc.T(Key(cat)),
                Rows = rows.ToArray(),
                CanReset = cat != SettingsCategory.Account && cat != SettingsCategory.Support && cat != SettingsCategory.Language,
                ResetLabel = _loc.T("settings.reset_category"),
            });
        }

        // ------------------------------------------------------------------ Lignes

        void Add(SettingRowData row, Action<int>? handler = null)
        {
            rowsRef!.Add(row);
            _handlers.Add(handler ?? (_ => { }));
        }

        List<SettingRowData>? rowsRef;

        void Build(SettingsCategory cat, List<SettingRowData> rows)
        {
            rowsRef = rows;
            var s = _settings.Current;
            switch (cat)
            {
                case SettingsCategory.Audio:
                    Percent("settings.master", () => s.master, v => s.master = v);
                    Percent("settings.music", () => s.music, v => s.music = v);
                    Percent("settings.sfx", () => s.sfx, v => s.sfx = v);
                    Percent("settings.voice", () => s.voice, v => s.voice = v);
                    Toggle("settings.mute_bg", () => s.muteInBackground, v => s.muteInBackground = v);
                    break;
                case SettingsCategory.Graphics:
                    Choice("settings.quality", () => s.quality, v => s.quality = v, 4, "settings.quality.");
                    Toggle("settings.fps60", () => s.fps60, v => s.fps60 = v);
                    Toggle("settings.battery", () => s.batterySaver, v => s.batterySaver = v, "settings.battery.hint");
                    Toggle("settings.full_fx", () => s.fullCombatEffects, v => s.fullCombatEffects = v);
                    break;
                case SettingsCategory.Accessibility:
                    Toggle("settings.flashes", () => s.reduceFlashes, v => s.reduceFlashes = v);
                    Toggle("settings.shakes", () => s.reduceShakes, v => s.reduceShakes = v);
                    Toggle("settings.extended_time", () => s.extendedMiniGameTime, v => s.extendedMiniGameTime = v, "settings.extended_time.hint");
                    Toggle("settings.subtitles", () => s.subtitles, v => s.subtitles = v);
                    break;
                case SettingsCategory.Language:
                    Add(SettingRowData.Info(_loc.T("settings.lang.texts"), _loc.T("settings.lang.fr")));
                    Add(SettingRowData.Info(_loc.T("settings.lang.voice"), _loc.T("settings.lang.en")));
                    Add(SettingRowData.Info(_loc.T("settings.lang.more"), _loc.T("wip.soon")));
                    break;
                case SettingsCategory.Notifications:
                    Toggle("settings.notif.all", () => s.notifications, v => s.notifications = v, "settings.notif.hint");
                    Toggle("settings.notif.training", () => s.notifTraining, v => s.notifTraining = v);
                    Toggle("settings.notif.expedition", () => s.notifExpedition, v => s.notifExpedition = v);
                    Toggle("settings.notif.lab", () => s.notifLab, v => s.notifLab = v);
                    Toggle("settings.notif.recovered", () => s.notifRecovered, v => s.notifRecovered = v);
                    Toggle("settings.notif.shop", () => s.notifShop, v => s.notifShop = v);
                    Toggle("settings.notif.objectives", () => s.notifObjectives, v => s.notifObjectives = v);
                    Toggle("settings.notif.season", () => s.notifSeason, v => s.notifSeason = v);
                    Toggle("settings.notif.quiet", () => s.quietHours, v => s.quietHours = v);
                    break;
                case SettingsCategory.Controls:
                    Toggle("settings.show_keys", () => s.showKeys, v => s.showKeys = v);
                    foreach (var k in new[] { "roll", "bonus", "end", "skills", "auto", "speed", "pause", "back" })
                        Add(SettingRowData.Info(_loc.T("settings.key." + k), _loc.T("settings.key." + k + ".v")));
                    break;
                case SettingsCategory.Account: BuildAccount(s); break;
                case SettingsCategory.Support: BuildSupport(); break;
            }
        }

        void Percent(string key, Func<int> get, Action<int> set)
            => Add(new SettingRowData { Label = _loc.T(key), Kind = SettingKind.Stepper, Value = get() + " %" }, a =>
            {
                int v = Mathf.Clamp(get() + (a == 1 ? 10 : -10), 0, 100);
                _settings.Update(_ => set(v));
            });

        void Choice(string key, Func<int> get, Action<int> set, int count, string valuePrefix)
            => Add(new SettingRowData { Label = _loc.T(key), Kind = SettingKind.Stepper, Value = _loc.T(valuePrefix + get()) }, a =>
            {
                int v = ((get() + (a == 1 ? 1 : -1)) % count + count) % count;
                _settings.Update(_ => set(v));
            });

        void Toggle(string key, Func<bool> get, Action<bool> set, string? hint = null)
            => Add(new SettingRowData { Label = _loc.T(key), Kind = SettingKind.Toggle, On = get(), Value = get() ? _loc.T("settings.on") : _loc.T("settings.off"), Hint = hint != null ? _loc.T(hint) : "" },
                   _ => _settings.Update(_ => set(!get())));

        void BuildAccount(SettingsSave s)
        {
            var a = _session.Account;
            Add(SettingRowData.Info(_loc.T("settings.account.player"), a != null ? _loc.F("settings.player", a.DisplayName, a.PlayerId) : "—"));
            Add(SettingRowData.Info(_loc.T("settings.account.linked"), _loc.T("settings.account.guest"), _loc.T("settings.account.link_hint")));
            Toggle("settings.consent", () => s.consentStats, v => s.consentStats = v);
            Add(new SettingRowData { Label = _loc.T("settings.export"), Kind = SettingKind.Button, Value = _loc.T("settings.export.btn"), Hint = _loc.T("settings.export.hint") },
                _ => _toasts.Show(_loc.T("settings.export.done"), ToastKind.Info));
            Add(new SettingRowData { Label = _loc.T("settings.logout"), Kind = SettingKind.Button, Value = _loc.T("settings.logout") }, _ => Logout());
            Add(new SettingRowData { Label = _loc.T("settings.reset"), Kind = SettingKind.Button, Value = _loc.T("settings.reset"), Hint = _loc.T("settings.reset.hint"), Danger = true }, _ => Reset());
        }

        void BuildSupport()
        {
            var a = _session.Account;
            Add(new SettingRowData { Label = _loc.T("settings.tutorial"), Kind = SettingKind.Button, Value = _loc.T("settings.tutorial.btn"), Hint = _loc.T("settings.tutorial.hint") }, _ => ReplayTutorial());
            Add(SettingRowData.Info(_loc.T("settings.player_id"), a != null ? a.PlayerId : "—"));
            Add(new SettingRowData { Label = _loc.T("settings.copy_id"), Kind = SettingKind.Button, Value = _loc.T("settings.copy_id.btn") }, _ =>
            {
                if (a != null) GUIUtility.systemCopyBuffer = a.PlayerId;
                _toasts.Show(_loc.T("settings.copy_id.done"), ToastKind.Info);
            });
            Add(new SettingRowData { Label = _loc.T("settings.contact"), Kind = SettingKind.Button, Value = _loc.T("settings.contact.btn"), Hint = _loc.T("settings.contact.hint") },
                _ => _toasts.Show(_loc.T("settings.contact.done"), ToastKind.Info));
            Add(SettingRowData.Info(_loc.T("settings.version"), _loc.T("game.build")));
            Add(SettingRowData.Info(_loc.T("settings.credits"), _loc.T("settings.credits.v")));
        }

        void ReplayTutorial()
        {
            _tutorial.Restart();
            _nav.SelectTab(HubPage.Qg);
            _toasts.Show(_loc.T("settings.tutorial.started"), ToastKind.Info);
        }

        void Logout() => _action.Run(ct => _signOut.ExecuteAsync(ct), Lifetime, () => _navigate.Publish(new NavigateRequested(GameState.Login)));

        void Reset()
        {
            if (_action.IsBusy) return;
            ResetAsync().Forget();
        }

        async UniTaskVoid ResetAsync()
        {
            bool ok;
            try
            {
                ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T("login.reset.title"), _loc.T("login.reset.body"),
                                                                _loc.T("ui.confirm"), _loc.T("ui.cancel"), Tone.Danger), Lifetime);
            }
            catch (OperationCanceledException) { return; }
            // Nouvelle partie : boutique neuve tout de suite (sinon « collection complète » jusqu'au prochain tirage).
            if (ok && !Disposed)
                _action.Run(ct => _reset.ExecuteAsync(ct), Lifetime, () =>
                {
                    _openShop.ExecuteAsync(Lifetime).Forget();
                    _toasts.Show(_loc.T("settings.reset.done"), ToastKind.Info);
                    Refresh();
                });
        }
    }
}
