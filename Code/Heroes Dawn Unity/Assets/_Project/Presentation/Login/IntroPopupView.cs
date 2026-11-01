#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Première connexion (popup plein écran) : la cinématique de lancement (vidéos des 46 plans et voix off, joués par <see cref="CinematicVideoPlayer"/>),
    /// puis la création du personnage joueur : un nom saisi et un avatar de la galerie.
    /// Un toucher passe au plan suivant ; « Passer » termine la cinématique.
    /// </summary>
    public sealed class IntroPopupView : MonoBehaviour, IIntroPopupView
    {
        public enum Stage { None, Cinematic, Character }

        [SerializeField] CanvasGroup group = null!;
        [SerializeField] RectTransform cinematic = null!;
        [SerializeField] CinematicVideoPlayer player = null!;
        [SerializeField] Button tapArea = null!;
        [SerializeField] UiButton skip = null!;
        [SerializeField] RectTransform characterStage = null!;
        [SerializeField] TMP_Text characterTitle = null!;
        [SerializeField] TMP_InputField nameField = null!;
        [SerializeField] TMP_Text nameHint = null!;
        [SerializeField] RectTransform avatarRow = null!;
        [SerializeField] AvatarTileView avatarPrefab = null!;
        [SerializeField] UiButton characterConfirm = null!;

        readonly List<AvatarTileView> _avatars = new List<AvatarTileView>();
        string? _selectedAvatar;
        int _maxNameLength;

        public bool Cancelable => false;
        public void RequestCancel() { }

        public Stage Current { get; private set; } = Stage.None;
        public Button TapButton => tapArea;
        public Button SkipButton => skip.Button;
        public Button CharacterConfirmButton => characterConfirm.Button;
        public TMP_InputField NameField => nameField;
        public IReadOnlyList<AvatarTileView> Avatars => _avatars;
        public CinematicVideoPlayer Player => player;

        static bool Animated => !MotionSettings.Reduced && UnityEngine.Application.isPlaying;

        void Awake()
        {
            // Keep a comfortable touch target, but reduce the visual weight of Skip.
            skip.Rect.anchorMin = skip.Rect.anchorMax = Vector2.one;
            skip.Rect.pivot = Vector2.one;
            skip.Rect.sizeDelta = new Vector2(170f, 64f);
            skip.Label.fontSize = 24f;
            skip.Label.color = new Color(1f, 1f, 1f, 0.75f);
            skip.Fill.color = new Color(0f, 0f, 0f, 0.25f);
            tapArea.onClick.AddListener(() => player.SkipShot());
            skip.Button.onClick.AddListener(() => player.SkipAll());
        }

        void LateUpdate()
        {
            // Convert the physical safe-area corner to the popup's canvas coordinates.
            var parent = (RectTransform)skip.Rect.parent;
            var canvas = GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Screen.safeArea.max, camera, out var corner))
                skip.Rect.anchoredPosition = corner - parent.rect.max - new Vector2(20f, 16f);
        }

        void SetStage(Stage s)
        {
            Current = s;
            cinematic.gameObject.SetActive(s == Stage.Cinematic);
            characterStage.gameObject.SetActive(s == Stage.Character);
            if (group.alpha < 1f) Tween.Fade(group, 1f, Animated ? 0.2f : 0f);
        }

        // ------------------------------------------------------------------ Cinématique

        public async UniTask PlayCinematicAsync(IntroCinematicTexts texts, Action<float>? mainTheme, CancellationToken ct)
        {
            skip.SetLabel(texts.Skip);
            SetStage(Stage.Cinematic);
            await player.PlayAsync(mainTheme, ct);
        }

        // ------------------------------------------------------------------ Personnage

        public async UniTask<CharacterChoice> ChooseCharacterAsync(ChooseCharacterData d, CancellationToken ct)
        {
            characterTitle.text = d.Title;
            nameHint.text = d.NameHint;
            nameField.characterLimit = d.MaxNameLength;
            _maxNameLength = d.MaxNameLength;
            nameField.text = "";
            characterConfirm.SetLabel(d.Confirm);
            foreach (var old in _avatars) Destroy(old.gameObject);
            _avatars.Clear();
            avatarRow.Clear();
            foreach (var id in d.Avatars)
            {
                var tile = Instantiate(avatarPrefab, avatarRow);
                tile.Setup(id, _avatars.Count + 1);
                tile.Clicked += SelectAvatar;
                _avatars.Add(tile);
            }
            _selectedAvatar = null;
            if (d.Avatars.Count > 0) SelectAvatar(d.Avatars[0]);
            SetStage(Stage.Character);
            if (Animated) Tween.PopIn(characterStage, 0.3f);

            var done = new UniTaskCompletionSource<CharacterChoice>();
            void Confirm() { if (TryChoice(out var choice)) done.TrySetResult(choice); }
            void Changed(string _) => Refresh();
            characterConfirm.Button.onClick.AddListener(Confirm);
            nameField.onValueChanged.AddListener(Changed);
            Refresh();
            try { return await done.Task.AttachExternalCancellation(ct); }
            finally
            {
                characterConfirm.Button.onClick.RemoveListener(Confirm);
                nameField.onValueChanged.RemoveListener(Changed);
            }
        }

        /// <summary>Choisit l'avatar de la galerie (clic sur sa vignette).</summary>
        public void SelectAvatar(string id)
        {
            AvatarTileView? chosen = null;
            foreach (var a in _avatars) if (a.Id == id) chosen = a;
            if (chosen == null) return;
            _selectedAvatar = id;
            foreach (var a in _avatars) a.SetSelected(a == chosen);
            Refresh();
        }

        /// <summary>Le nom est saisi (1 à la longueur maximale, espaces rognés) et un avatar est choisi.</summary>
        bool TryChoice(out CharacterChoice choice)
        {
            string name = nameField.text.Trim();
            choice = new CharacterChoice(name, _selectedAvatar ?? "");
            return name.Length > 0 && name.Length <= _maxNameLength && _selectedAvatar != null;
        }

        void Refresh() => characterConfirm.Interactable = TryChoice(out _);
    }
}
