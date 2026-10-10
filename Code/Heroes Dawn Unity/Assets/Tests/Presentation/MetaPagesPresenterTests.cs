using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Vue de liste factice : un seul double pour Pass, Boutique, Opérations, Labo et Archives.</summary>
    public sealed class FakeListView : FakeView, IPassPageView, IBoutiquePageView, IOperationsPageView, ILabPageView, IArchivesPageView, ILoadoutPageView, IGrimoirePageView
    {
        public event Action<int>? HeaderClicked;
        public event Action<int, int>? RowButtonClicked;
        public event Action<int>? RowSelected;
        public event Action<int, int>? SideButtonClicked;
        public event Action<int>? SideRowSelected;
        public ListHeaderData Header;
        public InfoRowData[] Rows = Array.Empty<InfoRowData>();
        public ListSideData Side;
        public void ShowHeader(ListHeaderData d) => Header = d;
        public void ShowRows(InfoRowData[] r) => Rows = r;
        public void ShowSide(ListSideData d) => Side = d;
        public void ClickHeader(int i) => HeaderClicked?.Invoke(i);
        public void ClickRow(int row, int button = 0) => RowButtonClicked?.Invoke(row, button);
        public void SelectRow(int row) => RowSelected?.Invoke(row);
        public void ClickSide(int row, int button = 0) => SideButtonClicked?.Invoke(row, button);
    }

    public sealed class MetaPagesPresenterTests
    {
        TestApp _app = null!;
        FakeListView _view = null!;
        FakeToasts _toasts = null!;
        FakeModalHost _host = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeListView();
            _toasts = new FakeToasts();
            _host = new FakeModalHost();
        }

        T Start<T>(T p) where T : IDisposable
        {
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        PassPagePresenter Pass() => Start(new PassPagePresenter(_view, _app.Pass, _app.Daily, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                                               _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time));

        [Test, Description("Vérifie que le pass affiche 30 niveaux, que la récompense gratuite d'un niveau atteint se prend d'un toucher et passe à « PRIS » — parce que c'est le geste central du pass (M40)")]
        public void Pass_ListsThirtyLevels_ClaimFlipsToTaken()
        {
            _app.Pass.AddXp(100);
            Pass();
            Assert.That(_view.Rows.Length, Is.EqualTo(30));
            Assert.That(_view.Rows[0].A.Enabled, Is.True);
            Assert.That(_view.Rows[1].A.Enabled, Is.False, "niveau 2 pas atteint");
            int credits = _app.Session.Wallet.Credits;
            _view.ClickRow(0, 0);
            Assert.That(_app.Session.Wallet.Credits, Is.GreaterThan(credits));
            Assert.That(_view.Rows[0].A.Label, Is.EqualTo(_loc.T("pass.taken")));
            Assert.That(_toasts.Shown.Last().Kind, Is.EqualTo(ToastKind.Reward));
        }

        [Test, Description("Vérifie que « Tout récupérer » prend d'un coup toutes les récompenses disponibles — parce qu'on ne veut pas toucher trente fois")]
        public void Pass_ClaimAll_TakesEverything()
        {
            _app.Pass.AddXp(500);
            Pass();
            _view.ClickHeader(1);
            Assert.That(_app.Pass.ClaimableCount(), Is.Zero);
        }

        [Test, Description("Vérifie que la colonne de droite du pass montre les trois missions du jour et que leur bouton ne s'allume qu'une fois terminée — parce que les missions nourrissent le pass")]
        public void Pass_SideShowsDailyMissions_ClaimWhenDone()
        {
            Pass();
            Assert.That(_view.Side.Rows.Length, Is.EqualTo(3));
            Assert.That(_view.Side.Rows[0].A.Enabled, Is.False);
            var m = _app.Daily.Today()[0];
            _app.Daily.Record(m.Template.Kind, m.Template.Target);
            Assert.That(_view.Side.Rows[0].A.Enabled, Is.True, "la page se rafraîchit toute seule");
            _view.ClickSide(0);
            Assert.That(_view.Side.Rows[0].A.Label, Is.EqualTo(_loc.T("pass.taken")));
        }

        [Test, Description("Vérifie que l'échange de Gemmes contre des Crédits est grisé sans Gemmes, avec la raison, puis se fait quand on en a — parce qu'un bouton grisé doit dire pourquoi")]
        public void Boutique_Exchange_NeedsGems()
        {
            _app.Session.Wallet.SetGems(0);
            var p = Start(new BoutiquePagePresenter(_view, _app.Boutique, _app.Session, new ModalService(_host), _loc, _toasts,
                                                    _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time, _app.Collection, new HubNavigator()));
            int row = BoutiqueRow("ex_credits");
            Assert.That(_view.Rows[row].A.Enabled, Is.False);
            Assert.That(_view.Rows[row].A.Reason, Does.Contain("20"));
            _app.Session.Wallet.SetGems(25);
            _app.MetaCtx.PublishWallet();
            int credits = _app.Session.Wallet.Credits;
            _view.ClickRow(row);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits + 1000));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(5));
            p.Dispose();
        }

        static int BoutiqueRow(string exchangeId) => Boutique.Packs.Count + Boutique.Exchanges.ToList().FindIndex(e => e.Id == exchangeId);

        [Test, Description("Vérifie que l'achat d'un paquet de Gemmes passe par une confirmation qui précise que le paiement est simulé, puis ajoute les Gemmes — parce qu'aucun argent réel ne doit être suggéré")]
        public void Boutique_PackPurchase_ConfirmsThenGrantsGems()
        {
            var p = Start(new BoutiquePagePresenter(_view, _app.Boutique, _app.Session, new ModalService(_host), _loc, _toasts,
                                                    _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time, _app.Collection, new HubNavigator()));
            int gems = _app.Session.Wallet.Gems;
            _view.ClickRow(0);
            var popup = (FakeConfirmPopup)_host.Spawned.Single();
            Assert.That(popup.Asked!.Value.Body, Does.Contain("simulé"));
            popup.Answer(true);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems + 100));
            p.Dispose();
        }

        [Test, Description("Vérifie que le cadeau du jour se prend une fois par jour — parce qu'il donne une raison de revenir")]
        public void Boutique_DailyGift_OncePerDay()
        {
            var p = Start(new BoutiquePagePresenter(_view, _app.Boutique, _app.Session, new ModalService(_host), _loc, _toasts,
                                                    _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time, _app.Collection, new HubNavigator()));
            Assert.That(_view.Header.A.Enabled, Is.True);
            _view.ClickHeader(0);
            Assert.That(_view.Header.A.Enabled, Is.False);
            _app.Time.LocalNow = _app.Time.LocalNow.AddDays(1);
            _app.MetaCtx.Touch(MetaArea.Boutique);
            Assert.That(_view.Header.A.Enabled, Is.True);
            p.Dispose();
        }

        [Test, Description("Vérifie que la page Opérations propose les trois durées, grise celles qui manquent de membres libres et lance l'expédition choisie — parce que l'envoi est le geste principal de la salle")]
        public void Operations_EmptySlot_StartsChosenExpedition()
        {
            _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(new MemberId("etincelle")), 8));
            var p = Start(new OperationsPagePresenter(_view, _app.Operations, _app.Session, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                                      _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time));
            Assert.That(_view.Rows.Length, Is.EqualTo(3));
            Assert.That(_view.Rows[0].A.Enabled, Is.True, "patrouille : 1 membre");
            Assert.That(_view.Rows[0].B.Enabled, Is.False, "reconnaissance : 2 membres");
            _view.ClickRow(0, 0);
            Assert.That(_app.Operations.SlotViews()[0].Running, Is.True);
            Assert.That(_view.Rows[0].A.Label, Does.Contain(_loc.F("ops.speed", 3).Split(' ')[0]));
            p.Dispose();
        }

        [Test, Description("Vérifie que lancer une recherche l'affiche en cours avec sa barre, et qu'une seconde est refusée avec la raison — parce que le labo ne fait qu'une recherche à la fois")]
        public void Lab_StartShowsProgress_SecondRefused()
        {
            _app.Session.Wallet.Set(10_000, 0);
            var p = Start(new LabPagePresenter(_view, _app.Lab, _app.Session, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                               _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time));
            _view.ClickRow(0);
            Assert.That(_view.Rows[0].Progress, Is.GreaterThanOrEqualTo(0f));
            Assert.That(_view.Rows[1].A.Enabled, Is.False);
            Assert.That(_view.Rows[1].A.Reason, Is.EqualTo(_loc.T("lab.busy")));
            p.Dispose();
        }

        [Test, Description("Vérifie que lire une fiche des archives retire son badge « NOUVEAU » et affiche son texte à droite — parce que la pastille doit refléter ce qu'il reste à lire")]
        public void Archives_SelectingEntry_ShowsTextAndMarksRead()
        {
            var p = Start(new ArchivesPagePresenter(_view, _app.Archives, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                                    _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time));
            int unread = _app.Archives.UnreadCount();
            Assert.That(unread, Is.GreaterThan(0));
            _view.SelectRow(1);
            Assert.That(_app.Archives.UnreadCount(), Is.EqualTo(unread - 1));
            Assert.That(_view.Side.Text, Is.EqualTo(_loc.T("archives.lore.section.body")));
            Assert.That(_view.Rows.Any(r => r.Dim), Is.True, "les fiches verrouillées sont estompées");
            p.Dispose();
        }
    }

    public sealed class CityMapPresenterTests
    {
        sealed class FakeMapView : FakeView, ICityMapPageView
        {
            public event Action<string>? ZoneSelected;
            public event Action? DeployClicked;
            public event Action<string>? LockedTapped;
            public CityMapPageData Last;
            public void Show(CityMapPageData d) => Last = d;
            public void Select(string z) => ZoneSelected?.Invoke(z);
            public void Deploy() => DeployClicked?.Invoke();
            public void TapLocked(string l) => LockedTapped?.Invoke(l);
        }

        TestApp _app = null!;
        FakeMapView _view = null!;
        FakeToasts _toasts = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeMapView();
            _toasts = new FakeToasts();
        }

        CityMapPagePresenter Make()
        {
            var mission = new MissionLauncher(_app.Zone.Start, PrototypeContent.Zone11, _app.Publisher<NavigateRequested>(), _toasts, _loc);
            var p = new CityMapPagePresenter(_view, _app.Session, mission, _toasts, _loc, _app.Subscriber<RosterChanged>());
            _view.Show();
            p.Start();
            return p;
        }

        [Test, Description("Vérifie que la carte montre les cinq quartiers, que seul Brooklyn est ouvert et que les autres sont marqués verrouillés — parce que les actes suivants ne sont pas encore jouables")]
        public void Map_FiveBoroughs_OnlyBrooklynOpen()
        {
            Make();
            Assert.That(_view.Last.Boroughs.Length, Is.EqualTo(5));
            Assert.That(_view.Last.Boroughs.Where(b => !b.Locked).Select(b => b.Id), Is.EqualTo(new[] { "brooklyn" }));
            Assert.That(_view.Last.Points.Count(p => p.ZoneId != null), Is.EqualTo(3), "trois zones jouables");
            Assert.That(_view.Last.Points.Count(p => p.ZoneId == null), Is.GreaterThan(0), "points à venir dans les quartiers verrouillés");
        }

        [Test, Description("Vérifie que la zone proposée est la première non validée et que DÉPLOYER la démarre — parce que MISSION → carte → Déployer remplace le lancement direct")]
        public void Deploy_StartsSelectedZone()
        {
            Make();
            Assert.That(_view.Last.Points.First(p => p.Selected).ZoneId, Is.EqualTo("zone_ny_1_1"));
            _view.Select("zone_ny_1_2");
            _view.Deploy();
            Assert.That(_app.Session.RunZone!.Id.Value, Is.EqualTo("zone_ny_1_2"));
            Assert.That(_app.Received<N29.HeroesDawn.Application.Messages.NavigateRequested>().Count(n => n.Target == N29.HeroesDawn.Application.Flow.GameState.Board), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que la zone 1.3 est verrouillée tant que 1.1 et 1.2 ne sont pas validées, avec la raison, puis s'ouvre — parce que le boss de l'acte suit les deux autres zones")]
        public void BossZone_LockedUntilOthersDone()
        {
            Make();
            _view.Select("zone_ny_1_3");
            Assert.That(_view.Last.Detail.ButtonEnabled, Is.False);
            Assert.That(_view.Last.Detail.Reason, Does.Contain("Red Hook"));
            _view.Deploy();
            Assert.That(_app.Session.Run, Is.Null);
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            _app.Session.CompletedZones.Add("zone_ny_1_2");
            _view.Select("zone_ny_1_3");
            Assert.That(_view.Last.Detail.ButtonEnabled, Is.True);
        }

        [Test, Description("Vérifie que toucher un quartier ou un point verrouillé affiche un toast explicatif sans rien lancer — parce que le grisé doit expliquer pourquoi")]
        public void LockedTap_ShowsToast()
        {
            Make();
            _view.TapLocked("ACTE 2");
            Assert.That(_toasts.Shown.Single().Text, Does.Contain("ACTE 2"));
            Assert.That(_app.Session.Run, Is.Null);
        }

        [Test, Description("Vérifie qu'une zone déjà validée propose REJOUER avec la récompense réduite — parce que rejouer rapporte moins")]
        public void DoneZone_OffersReplay()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            Make();
            _view.Select("zone_ny_1_1");
            Assert.That(_view.Last.Detail.ButtonLabel, Is.EqualTo(_loc.T("map.replay")));
        }
    }

    public sealed class ZoneProgressTests
    {
        [Test, Description("Vérifie l'ordre libre dans l'acte : 1.1 et 1.2 sont ouvertes d'emblée, 1.3 attend les deux — parce que les maquettes (M60) laissent l'ordre libre à l'intérieur d'un acte")]
        public void ActOne_FreeOrder_BossLast()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            var z = app.Session.Content.Zones;
            Assert.That(ZoneProgress.StatusOf(app.Session, z.First(x => x.Code == "1.1")), Is.EqualTo(ZoneStatus.Available));
            Assert.That(ZoneProgress.StatusOf(app.Session, z.First(x => x.Code == "1.2")), Is.EqualTo(ZoneStatus.Available));
            Assert.That(ZoneProgress.StatusOf(app.Session, z.First(x => x.Code == "1.3")), Is.EqualTo(ZoneStatus.Locked));
            Assert.That(ZoneProgress.Recommended(app.Session).Code, Is.EqualTo("1.1"));
        }

        [Test, Description("Vérifie que démarrer une zone verrouillée échoue avec une erreur métier — parce que l'interface ne doit pas être la seule garde")]
        public void StartLockedZone_Fails()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            var r = app.Zone.Start.ExecuteAsync(PrototypeZones.Zone13).GetAwaiter().GetResult();
            Assert.That(r.Error?.Code, Is.EqualTo("zone.locked"));
        }

        [Test, Description("Vérifie que les zones 1.2 et 1.3 sont jouables de bout en bout (graphe valide, objectifs atteignables, combats existants) — parce qu'une zone injouable bloquerait la progression")]
        public void NewZones_AreWellFormed()
        {
            var c = PrototypeContent.Build();
            foreach (var id in new[] { PrototypeZones.Zone12, PrototypeZones.Zone13 })
            {
                var zone = c.Zone(id);
                var graph = new N29.HeroesDawn.Domain.Board.BoardGraph(zone);
                var seen = new HashSet<CaseId> { zone.Entry };
                var queue = new Queue<CaseId>();
                queue.Enqueue(zone.Entry);
                while (queue.Count > 0)
                    foreach (var n in graph.Neighbors(queue.Dequeue())) if (seen.Add(n)) queue.Enqueue(n);
                Assert.That(seen.Count, Is.EqualTo(zone.Nodes.Count), id + " : toutes les cases sont reliées");
                foreach (var o in zone.Objectives) Assert.That(zone.Nodes.Any(n => n.Id == o.Target), id + " : cible d'objectif " + o.Id);
                foreach (var e in zone.Encounters.SelectMany(x => x.Enemies)) Assert.That(c.Enemy(e), Is.Not.Null);
                foreach (var rank in zone.Nodes.Where(n => n.Type == N29.HeroesDawn.Definitions.CaseType.Combat).Select(n => n.Rank).Distinct())
                    Assert.That(zone.Encounters.Any(x => x.Rank == rank), id + " : rencontre pour le rang " + rank);
            }
        }
    }
}
