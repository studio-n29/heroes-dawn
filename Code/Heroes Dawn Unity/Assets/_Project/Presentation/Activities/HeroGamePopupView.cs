#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Mini-jeux 2D de héros (fenêtre UI Toolkit) : « Rempart » (toucher les projectiles avant qu'ils frappent la ville) et
    /// « Course des toits » (esquiver les obstacles, sauver les civils sur trois couloirs). Les éléments de jeu sont de simples images
    /// créées à la volée dans la zone de jeu ; la logique vit dans le domaine (<see cref="DeflectGame"/>, <see cref="RooftopRunGame"/>).
    /// </summary>
    public sealed class HeroGamePopupView : HdPopup, IHeroGamePopupView
    {
        readonly Dictionary<int, FieldSprite> _views = new Dictionary<int, FieldSprite>();
        readonly List<int> _stale = new List<int>();
        Label _title = null!, _score = null!, _time = null!, _lives = null!, _overlay = null!;
        VisualElement _field = null!, _briefing = null!, _window = null!;
        UnityEngine.UIElements.Button _left = null!, _right = null!, _abandon = null!;
        FieldSurface? _surface;
        FieldSprite? _hero;
        HeroGameRequest? _request;
        UniTaskCompletionSource<ActivityResult>? _done;
        bool _started, _abandoned;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        protected override VisualElement DefaultFocus => _abandon;

        public HdButtonRef LeftButton { get; private set; } = null!;
        public HdButtonRef RightButton { get; private set; } = null!;
        public HdButtonRef AbandonButton { get; private set; } = null!;
        public FieldSurface Field => _surface!;
        public bool Started => _started;
        public int ItemViews => _views.Count;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _score = Q<Label>("score"); _time = Q<Label>("time"); _lives = Q<Label>("lives"); _overlay = Q<Label>("overlay");
            _field = Q<VisualElement>("field"); _briefing = Q<VisualElement>("briefing"); _window = Q<VisualElement>("window");
            _left = Q<UnityEngine.UIElements.Button>("left"); _right = Q<UnityEngine.UIElements.Button>("right"); _abandon = Q<UnityEngine.UIElements.Button>("abandon");
            LeftButton = new HdButtonRef(_left, this); RightButton = new HdButtonRef(_right, this); AbandonButton = new HdButtonRef(_abandon, this);
            _overlay.enableRichText = true; _briefing.pickingMode = PickingMode.Ignore; _overlay.pickingMode = PickingMode.Ignore;
            _surface = new FieldSurface(_field);
            _field.RegisterCallback<PointerDownEvent>(e => OnPressed(Normalize(e.position)));
            _field.RegisterCallback<PointerMoveEvent>(e => { if (e.pressedButtons != 0) OnDragged(Normalize(e.position)); });
            _left.clicked += () => _request?.Run?.MoveLeft();
            _right.clicked += () => _request?.Run?.MoveRight();
            _abandon.clicked += () => _abandoned = true;
            if (_request != null) Present(_request);
        }

        Vector2 Normalize(Vector2 panelPos)
        {
            var r = _field.worldBound;
            if (r.width <= 1f || r.height <= 1f) return new Vector2(-1f, -1f);
            return new Vector2((panelPos.x - r.xMin) / r.width, (panelPos.y - r.yMin) / r.height);
        }

        /// <summary>Toucher simulé en coordonnées normalisées de la zone (tests PlayMode).</summary>
        public void PressAtField(Vector2 normalized) => OnPressed(normalized);

        public async UniTask<ActivityResult> PlayAsync(HeroGameRequest r, CancellationToken ct)
        {
            _request = r;
            _started = false; _abandoned = false;
            _done = new UniTaskCompletionSource<ActivityResult>();
            ct.Register(() => _done.TrySetCanceled());
            if (_surface == null) await UniTask.WaitUntil(() => _surface != null, cancellationToken: ct);
            Present(r);
            return await _done.Task;
        }

        void Present(HeroGameRequest r)
        {
            _surface!.Clear(); _views.Clear(); _hero = null;
            _title.text = r.Title;
            _abandon.text = r.Abandon;
            bool run = r.Kind == HeroGameKind.RooftopRun;
            _left.style.display = run ? DisplayStyle.Flex : DisplayStyle.None;
            _right.style.display = run ? DisplayStyle.Flex : DisplayStyle.None;
            if (run)
            {
                _hero = _surface.Add("Hero", r.HeroArt != null ? r.HeroArt : Sprites.Circle, r.HeroArt == null ? Theme.Primary : Color.white, new Vector2(120, 120));
            }
            _briefing.style.display = DisplayStyle.Flex; _briefing.BringToFront();
            _overlay.text = r.Instructions + "\n\n<size=70%><color=#FFCF33>" + r.Start + "</color></size>";
            Refresh();
        }

        void OnPressed(Vector2 p)
        {
            if (_request == null || _done == null) return;
            if (!_started) { _started = true; _briefing.style.display = DisplayStyle.None; return; }
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
            if (_request == null || _done == null || _done.Task.Status != UniTaskStatus.Pending || _surface == null) return;
            var d = _request.Deflect; var run = _request.Run;
            if (_abandoned) { d?.Abandon(); run?.Abandon(); }
            if (_started && !Finished(d, run))
            {
                float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                d?.Step(dt); run?.Step(dt);
                string ev = d?.LastEvent ?? run?.LastEvent ?? "none";
                if (ev == "leak" || ev == "hit") Shake();
                if (ev == "rescue" && _hero != null) Burst(_hero.Position, Theme.Success, 8);
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
            _briefing.style.display = DisplayStyle.Flex; _briefing.BringToFront();
            _overlay.text = result.Success ? _request.WonText : _request.LostText;
            _done!.TrySetResult(result);
        }

        void Refresh()
        {
            if (_request == null) return;
            var d = _request.Deflect; var run = _request.Run;
            int score = d?.Score ?? run?.Rescued ?? 0, target = d?.Target ?? run?.Target ?? 0;
            float time = d?.TimeLeft ?? run?.TimeLeft ?? 0f;
            int lives = d?.Lives ?? run?.Lives ?? 0;
            _score.text = string.Format(_request.ScoreFormat, score, target);
            _time.text = string.Format(_request.TimeFormat, Mathf.Max(0, Mathf.CeilToInt(time)));
            _lives.text = new string('♥', Mathf.Max(0, lives));
        }

        Vector2 ToField(float x, float y)
        {
            var r = _surface!.rect;
            return new Vector2(r.xMin + x * r.width, r.yMax - y * r.height);
        }

        void Draw()
        {
            var r = _surface!.rect;
            _stale.Clear();
            foreach (var k in _views.Keys) _stale.Add(k);
            if (_request?.Deflect != null)
                foreach (var it in _request.Deflect.Items)
                {
                    _stale.Remove(it.Id);
                    var v = ItemView(it.Id, it.Civilian ? Theme.Info : Theme.Danger, it.Civilian ? 96f : 84f, it.Civilian ? Sprites.Circle : Sprites.Burst);
                    v.rectTransform.anchoredPosition = new Vector2(r.xMin + it.X * r.width, r.yMax - it.Y * r.height);
                    v.rectTransform.localRotation = Quaternion.Euler(0, 0, it.Civilian ? 0f : it.Y * 720f);
                }
            else if (_request?.Run != null)
            {
                foreach (var it in _request.Run.Items)
                {
                    _stale.Remove(it.Id);
                    var v = ItemView(it.Id, it.Civilian ? Theme.Success : Theme.Danger, 92f, it.Civilian ? Sprites.Circle : Sprites.Rounded);
                    v.rectTransform.anchoredPosition = new Vector2(LaneX(it.Lane), r.yMax - it.Y * r.height);
                }
                if (_hero != null)
                {
                    // Le héros se tient sur sa case : ses pieds sur la ligne, le reste au-dessus.
                    var target = new Vector2(LaneX(_request.Run.Lane), r.yMax - RooftopRunGame.PlayerY * r.height);
                    _hero.rectTransform.anchoredPosition = Vector2.Lerp(_hero.Position, target, 0.35f);
                }
            }
            foreach (var k in _stale)
            {
                if (_views.TryGetValue(k, out var v)) v.Element.RemoveFromHierarchy();
                _views.Remove(k);
            }
            _briefing.BringToFront();
        }

        float LaneX(int lane)
        {
            var r = _surface!.rect;
            return r.xMin + (lane + 0.5f) / RooftopRunGame.Lanes * r.width;
        }

        FieldSprite ItemView(int id, Color color, float size, Sprite? sprite)
        {
            if (_views.TryGetValue(id, out var existing)) return existing;
            var v = _surface!.Add("Item" + id, sprite, color, new Vector2(size, size));
            _views[id] = v;
            return v;
        }

        void Burst(Vector2 anchoredInField, Color color, int count)
        {
            var s = _surface!.Size;
            HudAnim.Sparks(_field, new Vector2(s.x * 0.5f + anchoredInField.x, s.y * 0.5f - anchoredInField.y), color, count, 120f, 14f);
        }

        void Shake() => HudAnim.Shake(_window, 10f, 0.25f);
    }
}
