#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Narration
{
    /// <summary>
    /// Boîte de dialogue de narration (prefab) : avatar du locuteur, plaque de nom, bulle de bande dessinée avec sa queue, texte qui s'écrit.
    /// Un toucher achève la réplique, puis passe à la suivante ; « Passer » ferme tout le dialogue.
    /// </summary>
    public sealed class DialogueBoxView : MonoBehaviour, IDialogueBoxView
    {
        [SerializeField] CanvasGroup group = null!;
        [SerializeField] Button tapArea = null!;
        [SerializeField] RectTransform avatarRoot = null!;
        [SerializeField] PortraitView portrait = null!;
        [SerializeField] Image namePlate = null!;
        [SerializeField] TMP_Text nameText = null!;
        [SerializeField] RectTransform bubble = null!;
        [SerializeField] RectTransform tail = null!;
        [SerializeField] TMP_Text body = null!;
        [SerializeField] TMP_Text hint = null!;
        [SerializeField] Button skip = null!;
        [SerializeField] TMP_Text progress = null!;
        [SerializeField] UiButton next = null!;
        [SerializeField] UiButton auto = null!;

        const float CharsPerSecond = 55f;
        const float AvatarWidth = 350f;
        const float Margin = 30f;
        /// <summary>Pause avant la réplique suivante en mode automatique : fixe, plus un peu par caractère lu.</summary>
        const float AutoBaseSeconds = 1.2f, AutoPerChar = 0.04f;

        /// <summary>Avance automatique choisie par le joueur : elle reste d'un dialogue à l'autre pendant la session.</summary>
        public static bool AutoAdvance { get; set; }

        public bool Cancelable => false;
        public void RequestCancel() { }

        /// <summary>Réplique affichée (tests).</summary>
        public string Body => body.text;
        public string Speaker => nameText.text;
        public bool Typing { get; private set; }
        public Button TapButton => tapArea;
        public Button SkipButton => skip;
        public Button NextButton => next.Button;
        public Button AutoButton => auto.Button;
        /// <summary>« 2 / 5 » : la réplique affichée sur le total (tests).</summary>
        public string Progress => progress.text;

        public event System.Action<DialogueBoxLine>? LineShown;

        public async UniTask PlayAsync(IReadOnlyList<DialogueBoxLine> lines, CancellationToken ct)
        {
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            bool skipped = false;
            auto.Button.onClick.RemoveAllListeners();
            auto.Button.onClick.AddListener(() => { AutoAdvance = !AutoAdvance; RefreshAuto(); });
            RefreshAuto();
            int index = 0;
            foreach (var line in lines)
            {
                if (skipped) break;
                index++;
                progress.text = index + " / " + lines.Count;
                Show(line);
                LineShown?.Invoke(line);
                Tween.PopIn(bubble, 0.2f);
                Tween.PopIn(avatarRoot, 0.3f);
                var advance = new UniTaskCompletionSource();
                bool finishTyping = false;
                tapArea.onClick.RemoveAllListeners();
                void Advance() { if (Typing) finishTyping = true; else advance.TrySetResult(); }
                tapArea.onClick.AddListener(Advance);
                next.Button.onClick.RemoveAllListeners();
                next.Button.onClick.AddListener(Advance);
                skip.onClick.RemoveAllListeners();
                skip.onClick.AddListener(() => { skipped = true; advance.TrySetResult(); });

                hint.gameObject.SetActive(false);
                body.text = line.Text;
                body.ForceMeshUpdate();
                int total = body.textInfo.characterCount;
                body.maxVisibleCharacters = 0;
                Typing = !MotionSettings.Reduced;
                float shown = 0f;
                while (Typing && !finishTyping && !skipped && body.maxVisibleCharacters < total)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                    shown += CharsPerSecond * Time.unscaledDeltaTime;
                    body.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(shown));
                }
                Typing = false;
                body.maxVisibleCharacters = int.MaxValue;
                hint.gameObject.SetActive(true);
                if (!skipped) await WaitForAdvanceAsync(advance, line.Text.Length, ct);
            }
            tapArea.onClick.RemoveAllListeners();
            next.Button.onClick.RemoveAllListeners();
            skip.onClick.RemoveAllListeners();
            auto.Button.onClick.RemoveAllListeners();
        }

        /// <summary>Attend le toucher du joueur ; en mode automatique, la réplique suivante vient d'elle-même après un temps de lecture.</summary>
        async UniTask WaitForAdvanceAsync(UniTaskCompletionSource advance, int chars, CancellationToken ct)
        {
            float waited = 0f, limit = AutoBaseSeconds + AutoPerChar * chars;
            while (advance.Task.Status == UniTaskStatus.Pending)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                if (!AutoAdvance) { waited = 0f; continue; }
                waited += Time.unscaledDeltaTime;
                if (waited >= limit) return;
            }
        }

        static void PlaceButton(UiButton button, float anchorX, float x)
        {
            var rt = (RectTransform)button.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(anchorX, 0f);
            rt.pivot = new Vector2(anchorX, 0f);
            rt.anchoredPosition = new Vector2(x, 16f);
        }

        void RefreshAuto()
        {
            auto.SetColors(AutoAdvance ? Theme.Primary : Theme.Surface3, AutoAdvance ? Theme.TextDark : Theme.Text);
        }

        void Show(DialogueBoxLine line)
        {
            portrait.Show(line.Portrait, cutout: true);
            nameText.text = line.Speaker;
            namePlate.color = Theme.FamilyColor(line.Portrait.Family);
            // L'avatar est du côté du locuteur ; la bulle occupe le reste et sa queue pointe vers lui.
            float x = line.Left ? 0f : 1f;
            avatarRoot.anchorMin = avatarRoot.anchorMax = new Vector2(x, 0.5f);
            avatarRoot.pivot = new Vector2(x, 0.5f);
            avatarRoot.anchoredPosition = new Vector2(line.Left ? Margin : -Margin, 20f);
            bubble.offsetMin = new Vector2(line.Left ? AvatarWidth + Margin : Margin, bubble.offsetMin.y);
            bubble.offsetMax = new Vector2(line.Left ? -Margin : -(AvatarWidth + Margin), bubble.offsetMax.y);
            // Les boutons restent du côté du texte, jamais sous l'avatar.
            float bx = line.Left ? 1f : 0f, side = line.Left ? -1f : 1f;
            PlaceButton(next, bx, side * 30f);
            PlaceButton(auto, bx, side * 290f);
            tail.anchorMin = tail.anchorMax = new Vector2(x, 0.5f);
            tail.anchoredPosition = new Vector2(line.Left ? AvatarWidth + Margin - 6f : -(AvatarWidth + Margin - 6f), 20f);
        }
    }
}
