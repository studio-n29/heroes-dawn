#nullable enable
#if HD_FPS_OVERLAY
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Compteur d'images par seconde des builds de test (option de build <c>-fpsOverlay</c>, symbole <c>HD_FPS_OVERLAY</c>) : fonctionne dans un
    /// build de production (pas de build de développement, donc des performances réelles). Un seul texte, mis à jour deux fois par seconde,
    /// chaînes préparées une fois : aucune allocation par image. Affiche la moyenne et la pire image de la demi-seconde écoulée.
    /// Absent du jeu quand le symbole n'est pas défini.
    /// </summary>
    public sealed class FpsOverlay : MonoBehaviour
    {
        const float Window = 0.5f;
        const int MaxShown = 240;
        static readonly string[] Numbers = BuildNumbers();

        Text _text = null!;
        int _frames;
        float _elapsed, _worst;
        int _lastFps = -1, _lastWorst = -1;

        static string[] BuildNumbers()
        {
            var numbers = new string[MaxShown + 1];
            for (int i = 0; i <= MaxShown; i++) numbers[i] = i.ToString();
            return numbers;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var go = new GameObject("FpsOverlay");
            DontDestroyOnLoad(go);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;

            var label = new GameObject("Label");
            label.transform.SetParent(go.transform, false);
            var text = label.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 30;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.UpperCenter;
            text.color = new Color(0.45f, 1f, 0.45f, 0.95f);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var shadow = label.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(2f, -2f);
            var rect = text.rectTransform;
            // Au centre du haut de l'écran : à gauche se trouvent le titre et le retour, à droite le portefeuille et les boutons du combat.
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -6f);
            rect.sizeDelta = new Vector2(400f, 40f);

            go.AddComponent<FpsOverlay>()._text = text;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _frames++;
            _elapsed += dt;
            if (dt > _worst) _worst = dt;
            if (_elapsed < Window) return;

            int fps = Mathf.Min(MaxShown, Mathf.RoundToInt(_frames / _elapsed));
            int worst = Mathf.Min(MaxShown, Mathf.RoundToInt(_worst * 1000f));
            if (fps != _lastFps || worst != _lastWorst)
            {
                _lastFps = fps;
                _lastWorst = worst;
                _text.text = string.Concat(Numbers[fps], " FPS  ", Numbers[worst], " ms");
            }
            _frames = 0;
            _elapsed = 0f;
            _worst = 0f;
        }
    }
}
#endif
