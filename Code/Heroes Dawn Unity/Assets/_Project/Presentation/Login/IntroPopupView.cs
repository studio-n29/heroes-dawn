#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Vignette d'un avatar de la galerie (UI Toolkit) : son image ronde (ou son numéro) et un cadre qui s'allume quand il est choisi.</summary>
    public sealed class AvatarTile
    {
        public VisualElement Root { get; } = new VisualElement();
        public string Id { get; }
        public bool Selected { get; private set; }
        public event Action<string>? Clicked;

        public AvatarTile(string id, int number)
        {
            Id = id;
            Root.name = "Avatar_" + id;
            Root.AddToClassList("hd-intropop__avatar");
            var art = PlayerAvatarArt.For(id);
            if (art != null)
            {
                // Image de la galerie sous masque rond (USS) ; sans image, repli sur le numéro.
                var face = new VisualElement { pickingMode = PickingMode.Ignore };
                face.AddToClassList("hd-avatar__face");
                face.style.backgroundImage = new StyleBackground(art);
                Root.Add(face);
            }
            else
            {
                var label = new Label(number.ToString()) { pickingMode = PickingMode.Ignore };
                label.AddToClassList("hd-intropop__avatarnum");
                Root.Add(label);
            }
            Root.RegisterCallback<ClickEvent>(_ => Clicked?.Invoke(Id));
            SetSelected(false);
        }

        public void SetSelected(bool on)
        {
            Selected = on;
            Root.EnableInClassList("hd-intropop__avatar--on", on);
            HudAnim.SetScale(Root, on ? 1.08f : 1f);
        }

        /// <summary>Clic simulé (tests PlayMode).</summary>
        public void Click() => Clicked?.Invoke(Id);
    }

    /// <summary>
    /// Première connexion (fenêtre plein écran) : la cinématique de lancement (vidéos et voix off, jouées par <see cref="CinematicVideoPlayer"/>,
    /// surface vidéo uGUI posée sous la fenêtre), puis, en UI Toolkit : Rayner en Full Art qui présente The Dawn, la fiche d'identification
    /// (nom, prénom, âge, avatar, pouvoir choisi dans un tableau groupé par famille), la conclusion de Rayner et le choix de la ville de départ.
    /// Un toucher passe au plan (cinématique) ou à la réplique (Rayner) suivant ; « Passer » termine la cinématique.
    /// </summary>
    public sealed class IntroPopupView : HdPopup, IIntroPopupView
    {
        public enum Stage { None, Cinematic, Rayner, Form, Start }

        [SerializeField] CinematicVideoPlayer player = null!;

        readonly List<AvatarTile> _avatars = new List<AvatarTile>();
        readonly List<PowerItem> _powerItems = new List<PowerItem>();
        readonly List<StartItem> _startItems = new List<StartItem>();
        UnityEngine.UIElements.Button _tap = null!, _skip = null!, _raynerTap = null!, _formConfirm = null!, _power = null!;
        VisualElement _cine = null!, _rayner = null!, _raynerArt = null!, _raynerBox = null!, _form = null!, _start = null!;
        VisualElement _avatarRow = null!, _modal = null!, _powerSheet = null!, _powerTable = null!, _startCards = null!;
        Label _raynerName = null!, _raynerText = null!, _raynerHint = null!, _formTitle = null!, _formError = null!, _startTitle = null!;
        Label _lastNameLabel = null!, _firstNameLabel = null!, _ageLabel = null!, _avatarLabel = null!, _powerLabel = null!;
        TextField _lastName = null!, _firstName = null!, _age = null!;
        string? _selectedAvatar, _selectedPower;
        int _maxNameLength = 20, _minAge = AgentForm.MinAge, _maxAge = AgentForm.MaxAge;
        Stage _stage;
        string _skipLabel = "";
        AgentFormData? _formData;
        Action? _raynerAdvance;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        protected override VisualElement DefaultFocus => _stage == Stage.Form ? (VisualElement)_lastName : _stage == Stage.Rayner ? _raynerTap : _tap;

        public Stage Current => _stage;
        public HdButtonRef TapButton { get; private set; } = null!;
        public HdButtonRef SkipButton { get; private set; } = null!;
        public HdButtonRef RaynerTapButton { get; private set; } = null!;
        public HdButtonRef FormConfirmButton { get; private set; } = null!;
        public HdButtonRef PowerButton { get; private set; } = null!;
        public HdFieldRef LastNameField { get; private set; } = null!;
        public HdFieldRef FirstNameField { get; private set; } = null!;
        public HdFieldRef AgeField { get; private set; } = null!;
        public IReadOnlyList<AvatarTile> Avatars => _avatars;
        public CinematicVideoPlayer Player => player;
        /// <summary>Texte de la réplique de Rayner affichée.</summary>
        public string RaynerLine => _raynerText?.text ?? "";
        public int PowerCount => _powerItems.Count;
        public bool PowerSheetOpen => _powerSheet != null && _powerSheet.ClassListContains("hd-intropop__powersheet--open");
        public IReadOnlyList<(string RouteId, bool Available)> StartChoices => _startItems.ConvertAll(s => (s.RouteId, s.Available));

        protected override void Bind()
        {
            _modal = Q<VisualElement>("modal");
            _tap = Q<UnityEngine.UIElements.Button>("tap"); _skip = Q<UnityEngine.UIElements.Button>("skip");
            _cine = Q<VisualElement>("cinematicLayer");
            _rayner = Q<VisualElement>("raynerLayer"); _raynerArt = Q<VisualElement>("raynerArt"); _raynerBox = _rayner.Q<VisualElement>(className: "hd-intropop__rayner-box");
            _raynerTap = Q<UnityEngine.UIElements.Button>("raynerTap");
            _raynerName = Q<Label>("raynerName"); _raynerText = Q<Label>("raynerText"); _raynerHint = Q<Label>("raynerHint");
            _form = Q<VisualElement>("formLayer"); _start = Q<VisualElement>("startLayer");
            _formTitle = Q<Label>("formTitle"); _formError = Q<Label>("formError");
            _lastNameLabel = Q<Label>("lastNameLabel"); _firstNameLabel = Q<Label>("firstNameLabel"); _ageLabel = Q<Label>("ageLabel");
            _avatarLabel = Q<Label>("avatarLabel"); _powerLabel = Q<Label>("powerLabel");
            _lastName = Q<TextField>("lastNameField"); _firstName = Q<TextField>("firstNameField"); _age = Q<TextField>("ageField");
            _power = Q<UnityEngine.UIElements.Button>("powerButton"); _powerSheet = Q<VisualElement>("powerSheet"); _powerTable = Q<VisualElement>("powerTable");
            _avatarRow = Q<VisualElement>("avatars"); _formConfirm = Q<UnityEngine.UIElements.Button>("formConfirm");
            _startTitle = Q<Label>("startTitle"); _startCards = Q<VisualElement>("startCards");

            TapButton = new HdButtonRef(_tap, this); SkipButton = new HdButtonRef(_skip, this);
            RaynerTapButton = new HdButtonRef(_raynerTap, this); FormConfirmButton = new HdButtonRef(_formConfirm, this); PowerButton = new HdButtonRef(_power, this);
            LastNameField = new HdFieldRef(_lastName); FirstNameField = new HdFieldRef(_firstName); AgeField = new HdFieldRef(_age);
            _tap.clicked += () => player.SkipShot();
            _skip.clicked += () => player.SkipAll();
            _power.clicked += TogglePowerSheet;
            _lastName.RegisterValueChangedCallback(_ => RefreshForm());
            _firstName.RegisterValueChangedCallback(_ => RefreshForm());
            _age.RegisterValueChangedCallback(e =>
            {
                // L'âge ne prend que des chiffres.
                string digits = DigitsOnly(e.newValue);
                if (digits != e.newValue) _age.SetValueWithoutNotify(digits);
                RefreshForm();
            });
            ApplyStage();
            if (_skipLabel.Length > 0) _skip.text = _skipLabel;
            if (_formData.HasValue) FillForm(_formData.Value);
        }

        static string DigitsOnly(string s)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in s ?? "") if (c >= '0' && c <= '9') sb.Append(c);
            return sb.ToString();
        }

        void SetStage(Stage s)
        {
            _stage = s;
            if (_cine != null) ApplyStage();
        }

        void ApplyStage()
        {
            _cine.style.display = _stage == Stage.Cinematic ? DisplayStyle.Flex : DisplayStyle.None;
            _rayner.style.display = _stage == Stage.Rayner ? DisplayStyle.Flex : DisplayStyle.None;
            _form.style.display = _stage == Stage.Form ? DisplayStyle.Flex : DisplayStyle.None;
            _start.style.display = _stage == Stage.Start ? DisplayStyle.Flex : DisplayStyle.None;
            // Pendant la cinématique, la vidéo (uGUI, dessous) doit se voir : le fond est transparent ; il ne se peint que pour la suite.
            _modal.style.backgroundColor = _stage == Stage.Cinematic || _stage == Stage.None ? new StyleColor(Color.clear) : (StyleColor)Theme.Navy;
        }

        // ------------------------------------------------------------------ Cinématique

        public async UniTask PlayCinematicAsync(IntroCinematicTexts texts, Action<float>? mainTheme, CancellationToken ct, CinematicKind kind = CinematicKind.Launch, Action<string>? voiceCue = null)
        {
            _skipLabel = texts.Skip;
            await UniTask.WaitUntil(() => _cine != null, cancellationToken: ct); // le document se monte au premier affichage
            _skip.text = texts.Skip;
            SetStage(Stage.Cinematic);
            await player.PlayAsync(mainTheme, ct, kind == CinematicKind.Full ? CinematicVideoSequence.Steps : CinematicVideoSequence.Launch, voiceCue);
        }

        // ------------------------------------------------------------------ Rayner en Full Art

        /// <summary>Joue les répliques de Rayner (portrait plein écran) ; un toucher passe à la suivante, le dernier termine la scène.</summary>
        public async UniTask ShowRaynerAsync(RaynerSceneData scene, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => _cine != null, cancellationToken: ct);
            _raynerArt.style.backgroundImage = new StyleBackground(RaynerArt(scene.PortraitId));
            _raynerName.text = scene.Speaker;
            _raynerHint.text = scene.Continue;
            SetStage(Stage.Rayner);
            HudAnim.PopIn(_raynerBox, 0.3f);

            var lines = scene.Lines;
            int index = 0;
            var done = new UniTaskCompletionSource();
            void Show() => _raynerText.text = lines.Count > 0 ? lines[index] : "";
            void Next()
            {
                if (index + 1 < lines.Count) { index++; Show(); }
                else done.TrySetResult();
            }
            _raynerAdvance = Next;
            Show();
            if (lines.Count == 0) done.TrySetResult();
            _raynerTap.clicked += Next;
            try { await done.Task.AttachExternalCancellation(ct); }
            finally { _raynerTap.clicked -= Next; _raynerAdvance = null; }
        }

        /// <summary>Passe à la réplique suivante de Rayner (le toucher du joueur ; sert aussi aux tests PlayMode).</summary>
        public void AdvanceRayner() => _raynerAdvance?.Invoke();

        /// <summary>Le Full Art de Rayner : sa grande illustration de la cinématique, à défaut son illustration d'agent.</summary>
        static Sprite? RaynerArt(string id)
        {
            var big = Resources.Load<Sprite>("Cinematic/Art/" + id);
            return big != null ? big : AgentArt.For(id);
        }

        // ------------------------------------------------------------------ Fiche d'identification

        public async UniTask<AgentForm> FillFormAsync(AgentFormData d, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => _cine != null, cancellationToken: ct);
            _formData = d;
            FillForm(d);
            SetStage(Stage.Form);
            HudAnim.PopIn(_form, 0.3f);
            DefaultFocus.schedule.Execute(() => DefaultFocus.Focus()).StartingIn(60);

            var done = new UniTaskCompletionSource<AgentForm>();
            void Confirm() { if (TryForm(out var form)) done.TrySetResult(form); }
            _formConfirm.clicked += Confirm;
            RefreshForm();
            try { return await done.Task.AttachExternalCancellation(ct); }
            finally { _formConfirm.clicked -= Confirm; }
        }

        void FillForm(AgentFormData d)
        {
            _formTitle.text = d.Title;
            _lastNameLabel.text = d.LastNameLabel; _firstNameLabel.text = d.FirstNameLabel; _ageLabel.text = d.AgeLabel;
            _avatarLabel.text = d.AvatarLabel; _powerLabel.text = d.PowerLabel;
            _formError.text = d.Error ?? "";
            _lastName.maxLength = d.MaxNameLength; _firstName.maxLength = d.MaxNameLength;
            _maxNameLength = d.MaxNameLength; _minAge = d.MinAge; _maxAge = d.MaxAge;
            _lastName.value = ""; _firstName.value = ""; _age.value = "";
            _formConfirm.text = d.Confirm;

            _avatars.Clear(); _avatarRow.Clear();
            foreach (var id in d.Avatars)
            {
                var tile = new AvatarTile(id, _avatars.Count + 1);
                tile.Clicked += SelectAvatar;
                _avatars.Add(tile); _avatarRow.Add(tile.Root);
            }
            _selectedAvatar = null;
            if (d.Avatars.Count > 0) SelectAvatar(d.Avatars[0]);

            BuildPowerTable(d);
            _selectedPower = null;
            ClosePowerSheet();
            UpdatePowerButton();
        }

        /// <summary>Le tableau des pouvoirs : une colonne par famille (en-tête coloré), un bouton par pouvoir.</summary>
        void BuildPowerTable(AgentFormData d)
        {
            _powerTable.Clear(); _powerItems.Clear();
            foreach (var column in d.Powers)
            {
                var col = new VisualElement();
                col.AddToClassList("hd-intropop__powercol");
                var head = new Label(column.Label) { pickingMode = PickingMode.Ignore };
                head.AddToClassList("hd-intropop__powerhead");
                head.AddToClassList("hd-intropop__powerhead--" + column.FamilyId);
                col.Add(head);
                foreach (var option in column.Powers)
                {
                    var id = option.Id;
                    var item = new UnityEngine.UIElements.Button { text = option.Label, name = "Power_" + id };
                    item.AddToClassList("hd-intropop__poweritem");
                    item.clicked += () => SelectPower(id);
                    col.Add(item);
                    _powerItems.Add(new PowerItem(id, option.Label, column.Label, item));
                }
                _powerTable.Add(col);
            }
        }

        /// <summary>Choisit un pouvoir dans le tableau (clic sur son bouton) et referme le tableau.</summary>
        public void SelectPower(string id)
        {
            bool known = false;
            foreach (var p in _powerItems) if (p.Id == id) known = true;
            if (!known) return;
            _selectedPower = id;
            foreach (var p in _powerItems) p.Button.EnableInClassList("hd-intropop__poweritem--on", p.Id == id);
            ClosePowerSheet();
            UpdatePowerButton();
            RefreshForm();
        }

        void TogglePowerSheet() => _powerSheet.EnableInClassList("hd-intropop__powersheet--open", !PowerSheetOpen);

        void ClosePowerSheet() => _powerSheet.RemoveFromClassList("hd-intropop__powersheet--open");

        void UpdatePowerButton()
        {
            string? label = null;
            foreach (var p in _powerItems) if (p.Id == _selectedPower) label = p.Family + " · " + p.Label;
            _power.text = label ?? (_formData?.PowerPlaceholder ?? "");
        }

        /// <summary>Choisit l'avatar de la galerie (clic sur sa vignette).</summary>
        public void SelectAvatar(string id)
        {
            AvatarTile? chosen = null;
            foreach (var a in _avatars) if (a.Id == id) chosen = a;
            if (chosen == null) return;
            _selectedAvatar = id;
            foreach (var a in _avatars) a.SetSelected(a == chosen);
            RefreshForm();
        }

        /// <summary>Nom et prénom saisis (1 à la longueur maximale, espaces rognés), âge dans les bornes, avatar et pouvoir choisis.</summary>
        bool TryForm(out AgentForm form)
        {
            string last = (_lastName?.value ?? "").Trim(), first = (_firstName?.value ?? "").Trim();
            int.TryParse(_age?.value ?? "", out int age);
            form = new AgentForm(last, first, age, _selectedAvatar ?? "", _selectedPower ?? "");
            return last.Length > 0 && last.Length <= _maxNameLength && first.Length > 0 && first.Length <= _maxNameLength
                   && age >= _minAge && age <= _maxAge && _selectedAvatar != null && _selectedPower != null;
        }

        void RefreshForm()
        {
            if (_formConfirm != null) _formConfirm.SetEnabled(TryForm(out _));
        }

        // ------------------------------------------------------------------ Choix de la ville de départ

        public async UniTask<string> ChooseStartAsync(StartChoiceData d, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => _cine != null, cancellationToken: ct);
            _startTitle.text = d.Title;
            _startCards.Clear(); _startItems.Clear();
            var done = new UniTaskCompletionSource<string>();
            foreach (var o in d.Options)
            {
                var card = new UnityEngine.UIElements.Button { name = "Start_" + o.RouteId };
                card.AddToClassList("hd-intropop__startcard");
                card.Add(Line(o.City, "hd-intropop__startcity"));
                card.Add(Line(o.Agent, "hd-intropop__startagent"));
                card.Add(Line(o.Description, "hd-intropop__startdesc"));
                string route = o.RouteId;
                if (o.Available) card.clicked += () => done.TrySetResult(route);
                else
                {
                    card.AddToClassList("hd-intropop__startcard--off");
                    card.SetEnabled(false);
                    card.Add(Line(o.UnavailableLabel, "hd-intropop__startsoon"));
                }
                _startCards.Add(card);
                _startItems.Add(new StartItem(route, o.Available, () => done.TrySetResult(route)));
            }
            SetStage(Stage.Start);
            HudAnim.PopIn(_start, 0.3f);
            return await done.Task.AttachExternalCancellation(ct);
        }

        static Label Line(string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            return l;
        }

        /// <summary>Choisit une ville de départ (le clic sur sa carte) ; sans effet si son parcours est fermé (tests PlayMode).</summary>
        public void PickStart(string routeId)
        {
            foreach (var s in _startItems) if (s.RouteId == routeId && s.Available) s.Pick();
        }

        sealed class PowerItem
        {
            public readonly string Id, Label, Family;
            public readonly UnityEngine.UIElements.Button Button;
            public PowerItem(string id, string label, string family, UnityEngine.UIElements.Button button) { Id = id; Label = label; Family = family; Button = button; }
        }

        sealed class StartItem
        {
            public readonly string RouteId;
            public readonly bool Available;
            public readonly Action Pick;
            public StartItem(string routeId, bool available, Action pick) { RouteId = routeId; Available = available; Pick = pick; }
        }
    }
}
