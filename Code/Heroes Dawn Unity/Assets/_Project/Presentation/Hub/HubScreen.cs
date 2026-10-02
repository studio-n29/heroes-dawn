#nullable enable
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>QG : effectif (escouade, entraînement), recrutement, salle des opérations, infirmerie.</summary>
    public sealed class HubScreen : UiScreen
    {
        System.IDisposable? _subs;

        void OnDestroy() => _subs?.Dispose();
        enum Tab { Team, Recruit }

        Tab _tab = Tab.Team;
        RectTransform _grid = null!;
        ScrollRect _scroll = null!;
        UiButton _tabTeam = null!;
        UiButton _tabRecruit = null!;
        Text _player = null!;
        Text _hint = null!;
        Text _zoneStatus = null!;
        Text _squadTitle = null!;
        RectTransform _squadRow = null!;
        UiButton _deploy = null!;
        UiButton _heal = null!;
        Text _deployHint = null!;
        RectTransform _objectives = null!;

        protected override void Build()
        {
            Backdrop(Theme.WithAlpha(Theme.Matter, 0.18f), Theme.Background);
            ArtBackdrop(Sprites.BackgroundLandscape, 1f, 0.74f);

            // ---- Barre du haut
            var top = Ui.Panel(Rect, "TopBar", Theme.SurfaceDark, 0f, 0f, true).Frame().TopBand(110);
            var logo = Sprites.Logo;
            if (logo != null)
            {
                var l = Ui.Img(top, "Logo", Color.white, logo);
                l.preserveAspect = true;
                l.rectTransform.At(0f, 0.5f, 14, -4, 130, 130);
            }
            var title = Ui.Txt(top, Loc.T("hub.title"), 56, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 0.5f, 150, 12, 700, 70);
            title.Inked(3f);
            _player = Ui.Txt(top, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Player");
            _player.rectTransform.At(0f, 0.5f, 152, -32, 700, 34);

            var wallet = WalletView.Create(top, Ctx.S.Session, Ctx.S.Wallet, Ctx.S.Saving);
            ((RectTransform)wallet.transform).At(1f, 0.5f, -330, 0, 700, 70);
            var logout = Ui.Btn(top, Loc.T("hub.logout"), Theme.Surface3, Theme.Text, OnLogout, 22, FontKind.Body);
            logout.Rect.At(1f, 0.5f, -20, 22, 290, 46);
            var reset = Ui.Btn(top, Loc.T("login.reset"), Theme.Surface2, Theme.TextMuted, OnReset, 18, FontKind.Body);
            reset.Rect.At(1f, 0.5f, -20, -28, 290, 40);

            // ---- Colonne gauche : onglets + grille
            var left = Ui.Rect(Rect, "Left");
            left.anchorMin = new Vector2(0, 0);
            left.anchorMax = new Vector2(0, 1);
            left.pivot = new Vector2(0, 0.5f);
            left.offsetMin = new Vector2(30, 30);
            left.offsetMax = new Vector2(1250, -130);

            _tabTeam = Ui.Btn(left, Loc.T("hub.tab.team"), Theme.Primary, Theme.TextDark, () => SetTab(Tab.Team), 36);
            _tabTeam.Rect.At(0f, 1f, 0, 0, 300, 76);
            _tabRecruit = Ui.Btn(left, Loc.T("hub.tab.recruit"), Theme.Surface3, Theme.Text, () => SetTab(Tab.Recruit), 36);
            _tabRecruit.Rect.At(0f, 1f, 320, 0, 340, 76);
            _hint = Ui.Txt(left, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Hint");
            _hint.rectTransform.At(0f, 1f, 690, -10, 530, 60);

            var gridArea = Ui.Rect(left, "GridArea").Stretch(0, 0, 0, 96);
            _grid = Ui.VScroll(gridArea, out _scroll);
            var grid = _grid.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(390, 380);
            grid.spacing = new Vector2(20, 22);
            grid.padding = new RectOffset(4, 4, 6, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            // ---- Colonne droite : opérations
            var ops = Ui.Panel(Rect, "Ops", Theme.Surface, 5f, 10f, true);
            var opsFrame = ops.Frame();
            opsFrame.anchorMin = new Vector2(1, 0);
            opsFrame.anchorMax = new Vector2(1, 1);
            opsFrame.pivot = new Vector2(1, 0.5f);
            opsFrame.offsetMin = new Vector2(-640, 30);
            opsFrame.offsetMax = new Vector2(-30, -140);
            var opsArt = Sprites.BackgroundPortrait;
            if (opsArt != null)
            {
                var holder = Ui.Rect(ops.transform, "OpsArt").Stretch();
                holder.gameObject.AddComponent<RectMask2D>();
                var art = Ui.Img(holder, "Art", new Color(1f, 1f, 1f, 0.3f), opsArt);
                var fit = art.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = opsArt.rect.width / opsArt.rect.height;
            }

            var opsBand = Ui.Panel(opsFrame, "OpsBand", Theme.Primary, 4f, 0f, false);
            opsBand.Frame().At(0.5f, 1f, 0, 26, 520, 70);
            Ui.Txt(opsBand.transform, Loc.T("hub.ops"), 40, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title).rectTransform.Stretch();

            var zoneCode = Ui.Txt(ops.transform, Loc.T("zones.ny_1_1.code"), 26, Theme.Primary, TextAnchor.UpperLeft, FontKind.Title);
            zoneCode.rectTransform.At(0f, 1f, 30, -64, 540, 34);
            var zoneName = Ui.Txt(ops.transform, Loc.T("zones.ny_1_1.name"), 46, Theme.Text, TextAnchor.UpperLeft, FontKind.Title);
            zoneName.rectTransform.At(0f, 1f, 30, -96, 560, 56);
            zoneName.Inked(2f);
            var zoneDesc = Ui.Txt(ops.transform, Loc.T("zones.ny_1_1.desc"), 24, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Regular);
            zoneDesc.rectTransform.At(0f, 1f, 30, -156, 550, 90);
            _objectives = Ui.Rect(ops.transform, "Objectives").At(0f, 1f, 30, -250, 550, 110);
            _objectives.VLayout(4, 0, TextAnchor.UpperLeft);
            _zoneStatus = Ui.Txt(ops.transform, "", 24, Theme.Success, TextAnchor.UpperLeft, FontKind.Body);
            _zoneStatus.rectTransform.At(0f, 1f, 30, -366, 550, 34);

            _squadTitle = Ui.Txt(ops.transform, "", 30, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title);
            _squadTitle.rectTransform.At(0f, 1f, 30, -410, 550, 40);
            _squadRow = Ui.Rect(ops.transform, "Squad").At(0f, 1f, 30, -455, 550, 150);
            _squadRow.HLayout(12, 0, TextAnchor.UpperLeft);

            _heal = Ui.Btn(ops.transform, "", Theme.Success, Theme.TextDark, OnHeal, 28);
            _heal.Rect.At(0.5f, 0f, 0, 150, 540, 70);
            _deployHint = Ui.Txt(ops.transform, "", 22, Theme.Fatigue, TextAnchor.MiddleCenter, FontKind.Body);
            _deployHint.rectTransform.At(0.5f, 0f, 0, 118, 540, 30);
            _deploy = Ui.Btn(ops.transform, Loc.T("hub.deploy"), Theme.Danger, Color.white, OnDeploy, 54);
            _deploy.Rect.At(0.5f, 0f, 0, 20, 540, 96);
            _deploy.Label.Inked(2f);
        }

        public override void OnShow()
        {
            var bag = DisposableBag.CreateBuilder();
            Ctx.S.Roster.Subscribe(_ => Refresh()).AddTo(bag);
            Ctx.S.Wallet.Subscribe(_ => Refresh()).AddTo(bag);
            _subs = bag.Build();
            Refresh();
        }

        public override void OnHide()
        {
            _subs?.Dispose();
            _subs = null;
        }

        void SetTab(Tab t)
        {
            _tab = t;
            Refresh();
            _scroll.verticalNormalizedPosition = 1f;
        }

        void Refresh()
        {
            var s = Ctx.S.Session;
            _player.text = s.Account != null ? s.Account.DisplayName + " · " + s.Account.PlayerId : "";
            _tabTeam.SetColors(_tab == Tab.Team ? Theme.Primary : Theme.Surface3, _tab == Tab.Team ? Theme.TextDark : Theme.Text);
            _tabRecruit.SetColors(_tab == Tab.Recruit ? Theme.Primary : Theme.Surface3, _tab == Tab.Recruit ? Theme.TextDark : Theme.Text);

            _grid.Clear();
            if (_tab == Tab.Team)
            {
                _hint.text = Loc.F("hub.squad", s.Squad.Count);
                foreach (var m in s.Roster) TeamCard(m);
            }
            else
            {
                _hint.text = Loc.T("hub.recruit.hint");
                var list = Ctx.S.Hub.AvailableRecruits();
                if (list.Count == 0)
                {
                    var t = Ui.Txt(_grid, Loc.T("hub.recruit.empty"), 30, Theme.Text, TextAnchor.MiddleCenter);
                    t.Pref(1180, 200);
                }
                foreach (var d in list) RecruitCard(d);
            }
            RefreshOps();
        }

        // ------------------------------------------------------------------ Cartes

        RectTransform CardBase(MemberDef d, int level, out Image fill)
        {
            fill = Ui.Panel(_grid, "Card_" + d.Id.Value, Theme.Surface, 4f, 7f, false);
            var frame = fill.Frame();
            var strip = Ui.Img(fill.transform, "Family", Theme.FamilyColor(d.Family), Sprites.Rounded);
            strip.rectTransform.TopBand(12, 0);
            var name = Loc.MemberName(d);
            var portrait = Portrait.Create(fill.transform, name, d.Family, 96);
            portrait.rectTransform.At(0f, 1f, 16, -24, 96, 96);
            var nameText = Ui.Txt(fill.transform, name.ToUpperInvariant(), 40, Color.white, TextAnchor.UpperLeft, FontKind.Title, "Name");
            nameText.rectTransform.At(0f, 1f, 124, -22, 250, 48);
            nameText.Inked(2f);
            var rarity = Ui.Txt(fill.transform, Loc.RarityName(d.Rarity), 22, Theme.RarityColor(d.Rarity), TextAnchor.UpperLeft, FontKind.Body);
            rarity.rectTransform.At(0f, 1f, 126, -70, 250, 28);
            var role = Ui.Txt(fill.transform, Loc.RoleName(d.Role) + " · " + Loc.FamilyName(d.Family), 20, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Regular);
            role.rectTransform.At(0f, 1f, 126, -96, 250, 26);
            var lvl = Ui.Txt(fill.transform, Loc.F("ui.level", level), 30, Theme.Primary, TextAnchor.UpperRight, FontKind.Title, "Level");
            lvl.rectTransform.At(1f, 1f, -14, -22, 110, 36);

            var b = Ctx.S.Session.Content.Balance;
            string stats = Loc.F("hub.stats", UnitFactory.ScaleStat(d.BaseStats.Hp, level, b), UnitFactory.ScaleStat(d.BaseStats.Atk, level, b),
                                 UnitFactory.ScaleStat(d.BaseStats.Def, level, b), d.BaseStats.Spd);
            var st = Ui.Txt(fill.transform, stats, 20, Theme.Text, TextAnchor.UpperLeft, FontKind.Regular, "Stats");
            st.rectTransform.At(0f, 1f, 16, -130, 360, 26);
            return frame;
        }

        void TeamCard(ITeamMemberReadModel m)
        {
            var frame = CardBase(m.Def, m.Level, out var fill);
            var skills = SkillsLine(m.Def);
            var sk = Ui.Txt(fill.transform, skills, 19, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Regular, "Skills");
            sk.rectTransform.At(0f, 1f, 16, -160, 360, 50);

            var bar = Ui.Bar(fill.transform, "Fatigue", Theme.FatigueColor(m.Fatigue), Theme.SurfaceDark, true, 18);
            ((RectTransform)bar.transform).At(0f, 1f, 16, -214, 356, 28);
            bar.Set(m.Fatigue / 100f, true);
            bar.SetLabel(Loc.F("ui.fatigue", m.Fatigue));

            bool inSquad = m.InSquad;
            var squadBtn = Ui.Btn(fill.transform, inSquad ? Loc.T("hub.squad.in") : Loc.T("hub.squad.add"),
                                  inSquad ? Theme.Primary : Theme.Surface3, inSquad ? Theme.TextDark : Theme.Text,
                                  () => Report(Ctx.S.Hub.ToggleSquad(m.Id)), 24);
            squadBtn.Rect.At(0f, 0f, 14, 74, 356, 52);

            bool max = m.Level >= MetaRules.MaxLevel;
            int cost = MetaRules.TrainCost(m.Level);
            var train = Ui.Btn(fill.transform, max ? Loc.T("hub.train.max") : Loc.F("hub.train", cost + " CR"),
                               Theme.Info, Theme.TextDark, () => OnTrain(m), 24);
            train.Rect.At(0f, 0f, 14, 14, 356, 52);
            train.Interactable = !max && Ctx.S.Session.Wallet.Credits >= cost;
            if (m.Exhausted) fill.color = Color.Lerp(Theme.Surface, Theme.Danger, 0.25f);
        }

        void RecruitCard(MemberDef d)
        {
            CardBase(d, MetaRules.RecruitLevel, out var fill);
            var sk = Ui.Txt(fill.transform, SkillsLine(d), 19, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Regular, "Skills");
            sk.rectTransform.At(0f, 1f, 16, -160, 360, 80);
            int cost = MetaRules.RecruitCost(d.Rarity);
            var btn = Ui.Btn(fill.transform, Loc.F("hub.recruit", cost), Theme.Pr, Theme.TextDark, () => OnRecruit(d), 28);
            btn.Rect.At(0f, 0f, 14, 16, 356, 62);
            btn.Interactable = Ctx.S.Session.Wallet.Pr >= cost;
        }

        static string SkillsLine(MemberDef d)
        {
            var names = new List<string>();
            foreach (var s in d.Skills) names.Add(Loc.SkillName(s));
            return string.Join(" · ", names);
        }

        // ------------------------------------------------------------------ Opérations

        void RefreshOps()
        {
            var s = Ctx.S.Session;
            _objectives.Clear();
            ZoneDef? zone = null;
            foreach (var z in s.Content.Zones) if (z.Id == Ctx.S.FirstZone) zone = z;
            if (zone != null)
            {
                foreach (var o in zone.Objectives)
                {
                    var t = Ui.Txt(_objectives, "•  " + Loc.T(o.NameKey), 24, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body);
                    t.Pref(540, 32);
                }
            }
            bool done = s.CompletedZones.Contains(Ctx.S.FirstZone.Value);
            _zoneStatus.text = done ? Loc.T("hub.zone.done") : Loc.T("hub.zone.new");
            _zoneStatus.color = done ? Theme.Success : Theme.TextMuted;

            var squad = s.Squad;
            _squadTitle.text = Loc.F("hub.squad", squad.Count);
            _squadRow.Clear();
            foreach (var m in squad)
            {
                var cell = Ui.Rect(_squadRow, "Member");
                cell.Pref(125, 150);
                var name = Loc.MemberName(m.Def);
                var p = Portrait.Create(cell, name, m.Def.Family, 90);
                p.rectTransform.At(0.5f, 1f, 0, 0, 90, 90);
                var n = Ui.Txt(cell, name, 22, Color.white, TextAnchor.MiddleCenter, FontKind.Body);
                n.rectTransform.At(0.5f, 1f, 0, -92, 125, 26);
                var bar = Ui.Bar(cell, "Fatigue", Theme.FatigueColor(m.Fatigue), Theme.SurfaceDark);
                ((RectTransform)bar.transform).At(0.5f, 1f, 0, -122, 110, 16);
                bar.Set(m.Fatigue / 100f, true);
            }

            bool anyFatigue = false;
            foreach (var m in s.Roster) if (m.Fatigue > 0) anyFatigue = true;
            _heal.SetLabel(Loc.F("hub.heal", MetaRules.InfirmaryCost + " CR"));
            _heal.Interactable = anyFatigue && s.Wallet.Credits >= MetaRules.InfirmaryCost;

            bool exhausted = s.SquadExhausted;
            _deploy.Interactable = !exhausted;
            _deployHint.text = exhausted ? Loc.T("hub.deploy.exhausted") : "";
        }

        // ------------------------------------------------------------------ Actions

        void Report(N29.HeroesDawn.Core.Result r)
        {
            if (r.IsFailure) Ctx.Error(r.Error);
        }

        void OnTrain(ITeamMemberReadModel m)
        {
            var r = Ctx.S.Hub.Train(m.Id);
            if (r.IsFailure) { Ctx.Error(r.Error); return; }
            Ctx.Toast(Loc.F("hub.trained", Loc.MemberName(m.Def), m.Level), ToastKind.Reward);
        }

        void OnRecruit(MemberDef d)
        {
            var r = Ctx.S.Hub.Recruit(d.Id);
            if (r.IsFailure) { Ctx.Error(r.Error); return; }
            Ctx.Toast(Loc.F("hub.recruited", Loc.MemberName(d)), ToastKind.Reward);
        }

        void OnHeal()
        {
            var r = Ctx.S.Hub.Heal();
            if (r.IsFailure) { Ctx.Error(r.Error); return; }
            Ctx.Toast(Loc.T("hub.healed"), ToastKind.Reward);
        }

        bool _deploying;

        void OnDeploy()
        {
            if (_deploying) return;
            DeployAsync().Forget();
        }

        async UniTaskVoid DeployAsync()
        {
            _deploying = true;
            Core.Result r;
            try { r = await Ctx.S.StartZone.ExecuteAsync(Ctx.S.FirstZone, this.GetCancellationTokenOnDestroy()); }
            finally { _deploying = false; }
            if (r.IsFailure) { Ctx.Error(r.Error); return; }
            Ctx.Nav.Show(Ctx.Board);
        }

        void OnLogout()
        {
            Ctx.S.Auth.SignOut();
            Ctx.Nav.Show(Ctx.Login);
        }

        void OnReset()
        {
            Ctx.Modal.Confirm(Loc.T("login.reset.title"), Loc.T("login.reset.body"), Loc.T("ui.confirm"),
                              () => Ctx.S.Auth.ResetProgress(), Loc.T("ui.cancel"), null, Theme.Danger);
        }
    }
}
