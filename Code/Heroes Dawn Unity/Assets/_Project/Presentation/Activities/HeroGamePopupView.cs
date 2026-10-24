#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Zone de jeu tactile : renvoie la position du toucher en coordonnées normalisées (0..1, y vers le bas).</summary>
    public sealed class HeroFieldTap : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public System.Action<Vector2>? Pressed;
        public System.Action<Vector2>? Dragged;

        public void OnPointerDown(PointerEventData e) => Pressed?.Invoke(Normalize(e));
        public void OnDrag(PointerEventData e) => Dragged?.Invoke(Normalize(e));

        Vector2 Normalize(PointerEventData e)
        {
            var rt = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out var local)) return new Vector2(-1f, -1f);
            var r = rt.rect;
            return new Vector2((local.x - r.xMin) / r.width, 1f - (local.y - r.yMin) / r.height);
        }
    }

    /// <summary>
    /// Mini-jeux 2D de héros (prefab Popups/HeroGamePopup) : « Rempart » (toucher les projectiles avant qu'ils frappent la ville) et
    /// « Course des toits » (esquiver les obstacles, sauver les civils sur trois couloirs). Les éléments de jeu sont de simples images
    /// créées à la volée dans la zone de jeu ; la logique vit dans le domaine (<see cref="DeflectGame"/>, <see cref="RooftopRunGame"/>).
    /// </summary>
    public sealed class HeroGamePopupView : MonoBehaviour, IHeroGamePopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text scoreText = null!;
        [SerializeField] TMP_Text timeText = null!;
        [SerializeField] TMP_Text livesText = null!;
        [SerializeField] TMP_Text overlay = null!;
        [SerializeField] RectTransform field = null!;
        [SerializeField] Button leftButton = null!;
        [SerializeField] Button rightButton = null!;
        [SerializeField] UiButton abandon = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;

        readonly Dictionary<int, RectTransform> _views = new Dictionary<int, RectTransform>();
        readonly List<int> _stale = new List<int>();
        RectTransform? _hero;
        HeroFieldTap? _tap;
        HeroGameRequest? _request;
        UniTaskCompletionSource<ActivityResult>? _done;
        bool _started, _abandoned;

        public bool Cancelable => false;
        public void RequestCancel() { }

        public Button LeftButton => leftButton;
        public Button RightButton => rightButton;
        public UiButton AbandonButton => abandon;
        public RectTransform Field => field;
        public bool Started => _started;
        public int ItemViews => _views.Count;

        void Awake()
        {
            _tap = field.gameObject.GetComponent<HeroFieldTap>() ?? field.gameObject.AddComponent<HeroFieldTap>();
            _tap.Pressed = OnPressed;
            _tap.Dragged = OnDragged;
            leftButton.onClick.AddListener(() => _request?.Run?.MoveLeft());
            rightButton.onClick.AddListener(() => _request?.Run?.MoveRight());
            abandon.Button.onClick.AddListener(() => _abandoned = true);
        }

        public async UniTask<ActivityResult> PlayAsync(HeroGameRequest r, CancellationToken ct)
        {
            _request = r;
            _started = false; _abandoned = false;
            foreach (var v in _views.Values) if (v != null) Destroy(v.gameObject);
            _views.Clear();
            title.text = r.Title;
            abandon.SetLabel(r.Abandon);
            bool run = r.Kind == HeroGameKind.RooftopRun;
            leftButton.gameObject.SetActive(run);
            rightButton.gameObject.SetActive(run);
            BuildHero(r);
            overlay.gameObject.SetActive(true);
            overlay.text = r.Instructions + "\n\n<size=70%><color=#FFCF33>" + r.Start + "</color></size>";
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
            _done = new UniTaskCompletionSource<ActivityResult>();
            ct.Register(() => _done.TrySetCanceled());
            Refresh();
            return await _done.Task;
        }

        void BuildHero(HeroGameRequest r)
        {
            if (_hero != null) Destroy(_hero.gameObject);
            _hero = null;
            if (r.Kind != HeroGameKind.RooftopRun) return;
            var go = new GameObject("Hero", typeof(RectTransform), typeof(Image));
            _hero = (RectTransform)go.transform;
            _hero.SetParent(field, false);
            _hero.sizeDelta = new Vector2(120, 120);
            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.sprite = r.HeroArt != null ? r.HeroArt : Sprites.Circle;
            img.preserveAspect = true;
            if (r.HeroArt == null) img.color = Theme.Primary;
        }

        void OnPressed(Vector2 p)
        {
            if (_request == null || _done == null) return;
            if (!_started) { _started = true; overlay.gameObject.SetActive(false); return; }
            if (_request.Deflect != null)
            {
                var result = _request.Deflect.Tap(p.x, p.y);
                var pos = ToField(p.x, p.y);
                if (result == DeflectTap.Deflected) Burst(pos, Theme.Primary, 7);
                else if (result == DeflectTap.Civilian) { Burst(pos, Theme.Danger, 9); Shake(); }
            }
            else _request.Run?.MoveToward(p.x);
        }

        void OnDragged(Vector2 p)
        {
            if (_started && _request?.Run != null) _request.Run.MoveToward(p.x);
        }

        void Update()
        {
            if (_request == null || _done == null || _done.Task.Status != UniTaskStatus.Pending) return;
            var d = _request.Deflect; var run = _request.Run;
            if (_abandoned) { d?.Abandon(); run?.Abandon(); }
            if (_started && !Finished(d, run))
            {
                float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                string before = d?.LastEvent ?? run?.LastEvent ?? "none";
                d?.Step(dt); run?.Step(dt);
                string ev = d?.LastEvent ?? run?.LastEvent ?? "none";
                if (ev == "leak" || ev == "hit") Shake();
                if (ev == "rescue" && _hero != null) Burst(_hero.anchoredPosition, Theme.Success, 8);
                _ = before;
            }
            Draw();
            Refresh();
            if (_started && Finished(d, run)) Finish();
        }

        static bool Finished(DeflectGame? d, RooftopRunGame? run)
            => d != null ? d.Status != GameStatus.Running : run != null && run.Status != GameStatus.Running;

        void Finish()
        {
            var d = _request!.Deflect; var run = _request.Run;
            var result = d != null ? d.Result() : run!.Result();
            overlay.gameObject.SetActive(true);
            overlay.text = result.Success ? _request.WonText : _request.LostText;
            _done!.TrySetResult(result);
        }

        void Refresh()
        {
            if (_request == null) return;
            var d = _request.Deflect; var run = _request.Run;
            int score = d?.Score ?? run?.Rescued ?? 0, target = d?.Target ?? run?.Target ?? 0;
            float time = d?.TimeLeft ?? run?.TimeLeft ?? 0f;
            int lives = d?.Lives ?? run?.Lives ?? 0;
            scoreText.text = string.Format(_request.ScoreFormat, score, target);
            timeText.text = string.Format(_request.TimeFormat, Mathf.Max(0, Mathf.CeilToInt(time)));
            livesText.text = new string('♥', Mathf.Max(0, lives));
        }

        Vector2 ToField(float x, float y)
        {
            var r = field.rect;
            return new Vector2(r.xMin + x * r.width, r.yMax - y * r.height);
        }

        void Draw()
        {
            var r = field.rect;
            _stale.Clear();
            foreach (var k in _views.Keys) _stale.Add(k);
            if (_request?.Deflect != null)
                foreach (var it in _request.Deflect.Items)
                {
                    _stale.Remove(it.Id);
                    var rt = ItemView(it.Id, it.Civilian ? Theme.Info : Theme.Danger, it.Civilian ? 96f : 84f, it.Civilian ? Sprites.Circle : Sprites.Burst);
                    rt.anchoredPosition = new Vector2(r.xMin + it.X * r.width, r.yMax - it.Y * r.height);
                    rt.localRotation = Quaternion.Euler(0, 0, it.Civilian ? 0f : it.Y * 720f);
                }
            else if (_request?.Run != null)
            {
                foreach (var it in _request.Run.Items)
                {
                    _stale.Remove(it.Id);
                    var rt = ItemView(it.Id, it.Civilian ? Theme.Success : Theme.Danger, 92f, it.Civilian ? Sprites.Circle : Sprites.Rounded);
                    rt.anchoredPosition = new Vector2(LaneX(it.Lane), r.yMax - it.Y * r.height);
                }
                if (_hero != null) _hero.anchoredPosition = Vector2.Lerp(_hero.anchoredPosition, new Vector2(LaneX(_request.Run.Lane), r.yMax - RooftopRunGame.PlayerY * r.height), 0.35f);
            }
            foreach (var k in _stale)
            {
                if (_views.TryGetValue(k, out var rt) && rt != null) Destroy(rt.gameObject);
                _views.Remove(k);
            }
        }

        float LaneX(int lane)
        {
            var r = field.rect;
            return r.xMin + (lane + 0.5f) / RooftopRunGame.Lanes * r.width;
        }

        RectTransform ItemView(int id, Color color, float size, Sprite? sprite)
        {
            if (_views.TryGetValue(id, out var existing) && existing != null) return existing;
            var go = new GameObject("Item" + id, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(field, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            _views[id] = rt;
            return rt;
        }

        void Burst(Vector2 anchoredInField, Color color, int count)
        {
            var go = new GameObject("Fx", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(field, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredInField;
            rt.sizeDelta = new Vector2(10, 10);
            Fx.Sparks(rt, color, count, 120f, 14f);
            Destroy(go, 1.2f);
        }

        void Shake() => Fx.Shake(window, 10f);
    }
}
