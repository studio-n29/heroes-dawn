#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>La potion magique (fenêtre UI Toolkit) : le livre, les ingrédients à toucher, la quantité, les étapes déjà ajoutées, « Préparer ».</summary>
    public sealed class PotionPopupView : HdPopup, IPotionPopupView
    {
        readonly List<UnityEngine.UIElements.Button> _ingredients = new List<UnityEngine.UIElements.Button>();
        Label _title = null!, _intro = null!, _book = null!, _qty = null!, _steps = null!, _attempts = null!, _feedback = null!;
        VisualElement _row = null!;
        UnityEngine.UIElements.Button _minus = null!, _plus = null!, _undo = null!, _clear = null!, _brew = null!, _quit = null!;
        PotionRequest? _r;
        int _quantity = 1;
        UniTaskCompletionSource<ActivityResult>? _done;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        protected override VisualElement DefaultFocus => _ingredients.Count > 0 ? _ingredients[0] : _brew;

        public int IngredientButtons => _ingredients.Count;
        public HdButtonRef IngredientButton(int i) => new HdButtonRef(_ingredients[i], this);
        public HdButtonRef BrewButton { get; private set; } = null!;
        public HdButtonRef QuantityPlus { get; private set; } = null!;
        public HdButtonRef QuantityMinus { get; private set; } = null!;
        public HdButtonRef QuitButton { get; private set; } = null!;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _intro = Q<Label>("intro"); _book = Q<Label>("book"); _qty = Q<Label>("qty");
            _steps = Q<Label>("steps"); _attempts = Q<Label>("attempts"); _feedback = Q<Label>("feedback");
            _steps.enableRichText = true; _feedback.enableRichText = true;
            _row = Q<VisualElement>("ingredients");
            _minus = Q<UnityEngine.UIElements.Button>("minus"); _plus = Q<UnityEngine.UIElements.Button>("plus");
            _undo = Q<UnityEngine.UIElements.Button>("undo"); _clear = Q<UnityEngine.UIElements.Button>("clear");
            _brew = Q<UnityEngine.UIElements.Button>("brew"); _quit = Q<UnityEngine.UIElements.Button>("quit");
            BrewButton = new HdButtonRef(_brew, this); QuantityPlus = new HdButtonRef(_plus, this);
            QuantityMinus = new HdButtonRef(_minus, this); QuitButton = new HdButtonRef(_quit, this);
            _undo.text = Loc.T("stage.potion.undo"); _clear.text = Loc.T("stage.potion.clear");
            _brew.text = Loc.T("stage.potion.brew"); _quit.text = Loc.T("stage.quit");
            _minus.clicked += () => SetQuantity(_quantity - 1);
            _plus.clicked += () => SetQuantity(_quantity + 1);
            _undo.clicked += () => { if (_r != null) { _r.Game.RemoveLast(); Refresh(); } };
            _clear.clicked += () => { if (_r != null) { _r.Game.Clear(); Refresh(); } };
            _brew.clicked += () => Brew().Forget();
            _quit.clicked += () => _done?.TrySetResult(ActivityResult.Lost());
            if (_r != null) Fill(_r);
        }

        void Fill(PotionRequest r)
        {
            _title.text = r.Title; _intro.text = r.Intro; _book.text = r.Book;
            _feedback.text = "";
            _row.Clear(); _ingredients.Clear();
            for (int i = 0; i < r.IngredientNames.Count; i++)
            {
                int index = i;
                var b = new UnityEngine.UIElements.Button { text = r.IngredientNames[i] };
                b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--primary"); b.AddToClassList("hd-potion__ingredient");
                if (i < r.IngredientColors.Count) b.style.unityBackgroundImageTintColor = r.IngredientColors[i];
                b.clicked += () => Add(index);
                _row.Add(b); _ingredients.Add(b);
            }
            Refresh();
        }

        public async UniTask<ActivityResult> PlayAsync(PotionRequest r, CancellationToken ct)
        {
            _r = r; _quantity = 1;
            _done = new UniTaskCompletionSource<ActivityResult>();
            ct.Register(() => _done.TrySetCanceled());
            if (_title != null) Fill(r);
            return await _done.Task;
        }

        void SetQuantity(int q)
        {
            if (_r == null) return;
            _quantity = Mathf.Clamp(q, 1, _r.Game.Recipe.MaxQuantity);
            Refresh();
        }

        void Add(int ingredient)
        {
            if (_r == null || !_r.Game.Add(ingredient, _quantity)) return;
            _feedback.text = "";
            Refresh();
        }

        async UniTaskVoid Brew()
        {
            var g = _r!.Game;
            if (!g.Brew()) { _feedback.text = _r.Loc.T("stage.potion.incomplete"); return; }
            var sb = new StringBuilder();
            if (g.Status == GameStatus.Won) sb.Append("<color=#6BE88A>").Append(_r.Loc.T("stage.potion.success")).Append("</color>");
            else
            {
                var line = new StringBuilder();
                for (int i = 0; i < g.LastAttempt.Count; i++)
                    line.Append(i + 1).Append(". ").Append(_r.IngredientNames[g.LastAttempt[i].Ingredient].Split('\n')[0]).Append(" x").Append(g.LastAttempt[i].Quantity)
                        .Append("  ").Append(Mark(g.LastFeedback[i])).Append('\n');
                sb.Append(line);
                if (g.Status == GameStatus.Lost) sb.Append("<color=#FF6B6B>").Append(_r.Loc.T("stage.potion.fail")).Append("</color>");
            }
            _feedback.text = sb.ToString();
            Refresh();
            if (g.Status != GameStatus.Running)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1.8), ignoreTimeScale: true);
                _done?.TrySetResult(g.Result());
            }
        }

        string Mark(StepFeedback f)
        {
            switch (f)
            {
                case StepFeedback.Correct: return "<color=#6BE88A>" + _r!.Loc.T("stage.potion.fb.correct") + "</color>";
                case StepFeedback.TooMuch: return "<color=#FFB84D>" + _r!.Loc.T("stage.potion.fb.toomuch") + "</color>";
                case StepFeedback.TooLittle: return "<color=#FFB84D>" + _r!.Loc.T("stage.potion.fb.toolittle") + "</color>";
                case StepFeedback.WrongPlace: return "<color=#7FC8FF>" + _r!.Loc.T("stage.potion.fb.wrongplace") + "</color>";
                default: return "<color=#FF6B6B>" + _r!.Loc.T("stage.potion.fb.wrong") + "</color>";
            }
        }

        void Refresh()
        {
            var g = _r!.Game;
            _qty.text = _r.Loc.F("stage.potion.quantity", _quantity);
            _attempts.text = _r.Loc.F("stage.potion.attempts", g.AttemptsLeft);
            var sb = new StringBuilder();
            for (int i = 0; i < g.StepsNeeded; i++)
            {
                sb.Append(i + 1).Append(". ");
                if (i < g.Current.Count) sb.Append(_r.IngredientNames[g.Current[i].Ingredient].Split('\n')[0]).Append(" x").Append(g.Current[i].Quantity);
                else sb.Append("<color=#888888>...</color>");
                sb.Append('\n');
            }
            _steps.text = sb.ToString();
            _brew.SetEnabled(g.Status == GameStatus.Running);
        }
    }
}
