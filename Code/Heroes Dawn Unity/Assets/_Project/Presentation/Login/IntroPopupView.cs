#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Vignette d'un avatar de la galerie (UI Toolkit) : son numéro et un cadre qui s'allume quand il est choisi.</summary>
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
            var label = new Label(number.ToString()) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("hd-intropop__avatarnum");
            Root.Add(label);
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
    /// Première connexion (fenêtre plein écran) : la cinématique de lancement (vidéos des 46 plans et voix off, jouées par <see cref="CinematicVideoPlayer"/>,
    /// surface vidéo uGUI posée sous la fenêtre), puis la création du personnage joueur en UI Toolkit : un nom saisi et un avatar de la galerie.
    /// Un toucher passe au plan suivant ; « Passer » termine la cinématique.
    /// </summary>
    public sealed class IntroPopupView : HdPopup, IIntroPopupView
    {
        public enum Stage { None, Cinematic, Character }

        [SerializeField] CinematicVideoPlayer player = null!;

        readonly List<AvatarTile> _avatars = new List<AvatarTile>();
        UnityEngine.UIElements.Button _tap = null!, _skip = null!, _confirm = null!;
        VisualElement _cine = null!, _char = null!, _avatarRow = null!, _modal = null!;
        Label _title = null!, _hint = null!;
        TextField _name = null!;
        string? _selectedAvatar;
        int _maxNameLength;
        Stage _stage;
        string _skipLabel = "";
        ChooseCharacterData? _chooseData;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        protected override VisualElement DefaultFocus => _stage == Stage.Character ? (VisualElement)_name : _tap;

        public Stage Current => _stage;
        public HdButtonRef TapButton { get; private set; } = null!;
        public HdButtonRef SkipButton { get; private set; } = null!;
        public HdButtonRef CharacterConfirmButton { get; private set; } = null!;
        public HdFieldRef NameField { get; private set; } = null!;
        public IReadOnlyList<AvatarTile> Avatars => _avatars;
        public CinematicVideoPlayer Player => player;

        protected override void Bind()
        {
            _modal = Q<VisualElement>("modal");
            _tap = Q<UnityEngine.UIElements.Button>("tap"); _skip = Q<UnityEngine.UIElements.Button>("skip"); _confirm = Q<UnityEngine.UIElements.Button>("confirm");
            _cine = Q<VisualElement>("cinematicLayer"); _char = Q<VisualElement>("characterLayer"); _avatarRow = Q<VisualElement>("avatars");
            _title = Q<Label>("title"); _hint = Q<Label>("nameHint"); _name = Q<TextField>("nameField");
            TapButton = new HdButtonRef(_tap, this); SkipButton = new HdButtonRef(_skip, this);
            CharacterConfirmButton = new HdButtonRef(_confirm, this); NameField = new HdFieldRef(_name);
            _tap.clicked += () => player.SkipShot();
            _skip.clicked += () => player.SkipAll();
            _name.RegisterValueChangedCallback(_ => Refresh());
            ApplyStage();
            if (_skipLabel.Length > 0) _skip.text = _skipLabel;
            if (_chooseData.HasValue) FillCharacter(_chooseData.Value);
        }

        void SetStage(Stage s)
        {
            _stage = s;
            if (_cine != null) ApplyStage();
        }

        void ApplyStage()
        {
            _cine.style.display = _stage == Stage.Cinematic ? DisplayStyle.Flex : DisplayStyle.None;
            _char.style.display = _stage == Stage.Character ? DisplayStyle.Flex : DisplayStyle.None;
            // Pendant la cinématique, la vidéo (uGUI, dessous) doit se voir : le fond est transparent ; il ne se peint que pour la création du personnage.
            _modal.style.backgroundColor = _stage == Stage.Character ? (StyleColor)Theme.Navy : new StyleColor(Color.clear);
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

        // ------------------------------------------------------------------ Personnage

        void FillCharacter(ChooseCharacterData d)
        {
            _title.text = d.Title; _hint.text = d.NameHint;
            _name.maxLength = d.MaxNameLength; _maxNameLength = d.MaxNameLength;
            _name.value = "";
            _confirm.text = d.Confirm;
            _avatars.Clear(); _avatarRow.Clear();
            foreach (var id in d.Avatars)
            {
                var tile = new AvatarTile(id, _avatars.Count + 1);
                tile.Clicked += SelectAvatar;
                _avatars.Add(tile); _avatarRow.Add(tile.Root);
            }
            _selectedAvatar = null;
            if (d.Avatars.Count > 0) SelectAvatar(d.Avatars[0]);
        }

        public async UniTask<CharacterChoice> ChooseCharacterAsync(ChooseCharacterData d, CancellationToken ct)
        {
            _chooseData = d;
            await UniTask.WaitUntil(() => _cine != null, cancellationToken: ct);
            FillCharacter(d);
            SetStage(Stage.Character);
            HudAnim.PopIn(_char, 0.3f);
            DefaultFocus.schedule.Execute(() => DefaultFocus.Focus()).StartingIn(60);

            var done = new UniTaskCompletionSource<CharacterChoice>();
            void Confirm() { if (TryChoice(out var choice)) done.TrySetResult(choice); }
            _confirm.clicked += Confirm;
            Refresh();
            try { return await done.Task.AttachExternalCancellation(ct); }
            finally { _confirm.clicked -= Confirm; }
        }

        /// <summary>Choisit l'avatar de la galerie (clic sur sa vignette).</summary>
        public void SelectAvatar(string id)
        {
            AvatarTile? chosen = null;
            foreach (var a in _avatars) if (a.Id == id) chosen = a;
            if (chosen == null) return;
            _selectedAvatar = id;
            foreach (var a in _avatars) a.SetSelected(a == chosen);
            Refresh();
        }

        /// <summary>Le nom est saisi (1 à la longueur maximale, espaces rognés) et un avatar est choisi.</summary>
        bool TryChoice(out CharacterChoice choice)
        {
            string name = (_name?.value ?? "").Trim();
            choice = new CharacterChoice(name, _selectedAvatar ?? "");
            return name.Length > 0 && name.Length <= _maxNameLength && _selectedAvatar != null;
        }

        void Refresh()
        {
            if (_confirm != null) _confirm.SetEnabled(TryChoice(out _));
        }
    }
}
