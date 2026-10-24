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

        const float CharsPerSecond = 55f;
        const float AvatarWidth = 350f;
        const float Margin = 30f;

        public bool Cancelable => false;
        public void RequestCancel() { }

        /// <summary>Réplique affichée (tests).</summary>
        public string Body => body.text;
        public string Speaker => nameText.text;
        public bool Typing { get; private set; }
        public Button TapButton => tapArea;
        public Button SkipButton => skip;

        public async UniTask PlayAsync(IReadOnlyList<DialogueBoxLine> lines, CancellationToken ct)
        {
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            bool skipped = false;
            foreach (var line in lines)
            {
                if (skipped) break;
                Show(line);
                Tween.PopIn(bubble, 0.2f);
                Tween.PopIn(avatarRoot, 0.3f);
                var advance = new UniTaskCompletionSource();
                bool finishTyping = false;
                tapArea.onClick.RemoveAllListeners();
                tapArea.onClick.AddListener(() => { if (Typing) finishTyping = true; else advance.TrySetResult(); });
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
                if (!skipped) await advance.Task.AttachExternalCancellation(ct);
            }
            tapArea.onClick.RemoveAllListeners();
            skip.onClick.RemoveAllListeners();
        }

        void Show(DialogueBoxLine line)
        {
            portrait.Show(line.Portrait);
            nameText.text = line.Speaker;
            namePlate.color = Theme.FamilyColor(line.Portrait.Family);
            // L'avatar est du côté du locuteur ; la bulle occupe le reste et sa queue pointe vers lui.
            float x = line.Left ? 0f : 1f;
            avatarRoot.anchorMin = avatarRoot.anchorMax = new Vector2(x, 0.5f);
            avatarRoot.pivot = new Vector2(x, 0.5f);
            avatarRoot.anchoredPosition = new Vector2(line.Left ? Margin : -Margin, 20f);
            bubble.offsetMin = new Vector2(line.Left ? AvatarWidth + Margin : Margin, bubble.offsetMin.y);
            bubble.offsetMax = new Vector2(line.Left ? -Margin : -(AvatarWidth + Margin), bubble.offsetMax.y);
            tail.anchorMin = tail.anchorMax = new Vector2(x, 0.5f);
            tail.anchoredPosition = new Vector2(line.Left ? AvatarWidth + Margin - 6f : -(AvatarWidth + Margin - 6f), 20f);
        }
    }
}
