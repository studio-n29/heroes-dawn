#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>La potion magique (prefab) : le livre, les ingrédients à toucher, la quantité, les étapes déjà ajoutées, « Préparer ».</summary>
    public sealed class PotionPopupView : PopupViewBase, IPotionPopupView
    {
        [SerializeField] TMP_Text intro = null!;
        [SerializeField] TMP_Text book = null!;
        [SerializeField] RectTransform ingredientRow = null!;
        [SerializeField] UiButton ingredientPrefab = null!;
        [SerializeField] UiButton qtyMinus = null!;
        [SerializeField] UiButton qtyPlus = null!;
        [SerializeField] TMP_Text qtyText = null!;
        [SerializeField] TMP_Text steps = null!;
        [SerializeField] TMP_Text attempts = null!;
        [SerializeField] TMP_Text feedback = null!;
        [SerializeField] UiButton undoButton = null!;
        [SerializeField] UiButton clearButton = null!;
        [SerializeField] UiButton brewButton = null!;
        [SerializeField] UiButton quitButton = null!;

        PotionRequest? _r;
        int _quantity = 1;
        UniTaskCompletionSource<ActivityResult>? _done;

        public int IngredientButtons => ingredientRow.childCount;
        public UnityEngine.UI.Button IngredientButton(int i) => ingredientRow.GetChild(i).GetComponent<UiButton>().Button;
        public UnityEngine.UI.Button BrewButton => brewButton.Button;
        public UnityEngine.UI.Button QuantityPlus => qtyPlus.Button;
        public UnityEngine.UI.Button QuantityMinus => qtyMinus.Button;
        public UnityEngine.UI.Button QuitButton => quitButton.Button;

        public async UniTask<ActivityResult> PlayAsync(PotionRequest r, CancellationToken ct)
        {
            Open(r.Title);
            _r = r;
            _quantity = 1;
            _done = new UniTaskCompletionSource<ActivityResult>();
            ct.Register(() => _done.TrySetCanceled());
            intro.text = r.Intro;
            book.text = r.Book;
            feedback.text = "";
            ingredientRow.Clear();
            for (int i = 0; i < r.IngredientNames.Count; i++)
            {
                int index = i;
                var b = Instantiate(ingredientPrefab, ingredientRow);
                b.SetLabel(r.IngredientNames[i]);
                b.SetColors(i < r.IngredientColors.Count ? r.IngredientColors[i] : Theme.Primary, Theme.TextDark);
                b.Button.onClick.AddListener(() => Add(index));
            }
            qtyMinus.Button.onClick.RemoveAllListeners(); qtyMinus.Button.onClick.AddListener(() => SetQuantity(_quantity - 1));
            qtyPlus.Button.onClick.RemoveAllListeners(); qtyPlus.Button.onClick.AddListener(() => SetQuantity(_quantity + 1));
            undoButton.Button.onClick.RemoveAllListeners(); undoButton.Button.onClick.AddListener(() => { r.Game.RemoveLast(); Refresh(); });
            clearButton.Button.onClick.RemoveAllListeners(); clearButton.Button.onClick.AddListener(() => { r.Game.Clear(); Refresh(); });
            brewButton.Button.onClick.RemoveAllListeners(); brewButton.Button.onClick.AddListener(() => Brew().Forget());
            quitButton.Button.onClick.RemoveAllListeners(); quitButton.Button.onClick.AddListener(() => _done.TrySetResult(ActivityResult.Lost()));
            Refresh();
            return await _done.Task;
        }

        void SetQuantity(int q)
        {
            _quantity = Mathf.Clamp(q, 1, _r!.Game.Recipe.MaxQuantity);
            Refresh();
        }

        void Add(int ingredient)
        {
            if (_r == null || !_r.Game.Add(ingredient, _quantity)) return;
            feedback.text = "";
            Refresh();
        }

        async UniTaskVoid Brew()
        {
            var g = _r!.Game;
            if (!g.Brew()) { feedback.text = _r.Loc.T("stage.potion.incomplete"); return; }
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
            feedback.text = sb.ToString();
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
            qtyText.text = _r.Loc.F("stage.potion.quantity", _quantity);
            attempts.text = _r.Loc.F("stage.potion.attempts", g.AttemptsLeft);
            var sb = new StringBuilder();
            for (int i = 0; i < g.StepsNeeded; i++)
            {
                sb.Append(i + 1).Append(". ");
                if (i < g.Current.Count) sb.Append(_r.IngredientNames[g.Current[i].Ingredient].Split('\n')[0]).Append(" x").Append(g.Current[i].Quantity);
                else sb.Append("<color=#888888>...</color>");
                sb.Append('\n');
            }
            steps.text = sb.ToString();
            brewButton.Interactable = g.Status == GameStatus.Running;
        }
    }
}
