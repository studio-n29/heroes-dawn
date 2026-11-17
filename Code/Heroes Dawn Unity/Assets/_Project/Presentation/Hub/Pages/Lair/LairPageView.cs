#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Écran Repaire (UI Toolkit, planche 12.4) : liste « Ennemis découverts » à gauche, grand portrait du boss choisi à droite avec sa fiche
    /// (progression, lore, récompense) et LANCER L'OPÉRATION. Sous le format 3:2 la fiche occupe toute la largeur.
    /// </summary>
    public sealed class LairPageView : HdView, ILairPageView
    {
        const float NarrowAspect = 1.5f;

        VisualElement _page = null!, _hero = null!, _fill = null!;
        ScrollView _rows = null!;
        Label _listTitle = null!, _name = null!, _sub = null!, _progressTitle = null!, _progressText = null!, _loreTitle = null!, _lore = null!, _reward = null!, _squad = null!;
        UnityEngine.UIElements.Button _enter = null!;
        LairScreenData? _last;

        public event Action<int>? BossSelected;
        public event Action? EnterClicked;

        public HdButtonRef EnterButton { get; private set; } = null!;

        protected override void Bind()
        {
            _page = Q<VisualElement>("page"); _hero = Q<VisualElement>("hero"); _fill = Q<VisualElement>("progressFill");
            _rows = Q<ScrollView>("bossRows");
            _listTitle = Q<Label>("listTitle"); _name = Q<Label>("bossName"); _sub = Q<Label>("bossSub");
            _progressTitle = Q<Label>("progressTitle"); _progressText = Q<Label>("progressText"); _loreTitle = Q<Label>("loreTitle");
            _lore = Q<Label>("lore"); _reward = Q<Label>("reward"); _squad = Q<Label>("squad");
            _enter = Q<UnityEngine.UIElements.Button>("enter");
            EnterButton = new HdButtonRef(_enter, this);
            _enter.clicked += () => EnterClicked?.Invoke();
            Root.RegisterCallback<GeometryChangedEvent>(_ => ApplyAspect());
            ApplyAspect();
            if (_last.HasValue) Show(_last.Value);
        }

        void ApplyAspect()
        {
            var r = Root.contentRect;
            if (r.height <= 0f) return;
            _page.EnableInClassList("hd-lair--narrow", r.width / r.height < NarrowAspect);
        }

        public void Show(LairScreenData d)
        {
            _last = d;
            if (_name == null) return;
            _listTitle.text = d.ListTitle;
            _name.text = d.BossName;
            _sub.text = d.BossSubtitle;
            _progressTitle.text = d.ProgressTitle;
            _progressText.text = d.ProgressText;
            _fill.style.width = Length.Percent(Mathf.Clamp01(d.Progress) * 100f);
            _loreTitle.text = d.LoreTitle;
            _lore.text = d.Lore;
            _reward.text = d.RewardLine;
            _reward.style.display = string.IsNullOrEmpty(d.RewardLine) ? DisplayStyle.None : DisplayStyle.Flex;
            _squad.text = d.SquadLine;
            _squad.style.display = string.IsNullOrEmpty(d.SquadLine) ? DisplayStyle.None : DisplayStyle.Flex;
            _enter.text = d.EnterLabel;
            _enter.SetEnabled(d.CanEnter);
            _enter.tooltip = d.EnterReason;

            var art = BossArt(d.BossEnemyId);
            _hero.style.backgroundImage = art != null ? new StyleBackground(Background.FromSprite(art)) : new StyleBackground(StyleKeyword.None);

            _rows.Clear();
            for (int i = 0; i < d.Bosses.Length; i++)
            {
                int index = i;
                var b = d.Bosses[i];
                var row = new UnityEngine.UIElements.Button();
                row.AddToClassList("hd-boss");
                row.EnableInClassList("hd-boss--selected", b.Selected);
                row.EnableInClassList("hd-boss--locked", b.Locked);
                var portrait = new VisualElement { pickingMode = PickingMode.Ignore };
                portrait.AddToClassList("hd-boss__portrait");
                var thumb = BossArt(b.EnemyId);
                if (thumb != null) portrait.style.backgroundImage = new StyleBackground(Background.FromSprite(thumb));
                row.Add(portrait);
                var text = new VisualElement { pickingMode = PickingMode.Ignore };
                text.AddToClassList("hd-boss__text");
                text.Add(Text(b.Name, "hd-boss__name"));
                text.Add(Text(b.Level, "hd-boss__level"));
                var bar = new VisualElement { pickingMode = PickingMode.Ignore };
                bar.AddToClassList("hd-bar"); bar.AddToClassList("hd-boss__bar");
                var fill = new VisualElement { pickingMode = PickingMode.Ignore };
                fill.AddToClassList("hd-bar__fill");
                fill.style.width = Length.Percent(b.Defeated ? 100f : 0f);
                bar.Add(fill);
                text.Add(bar);
                row.Add(text);
                row.clicked += () => BossSelected?.Invoke(index);
                _rows.Add(row);
            }
        }

        /// <summary>Portrait du boss (Resources/Illustrations/Enemies/<id>) ; null tant que l'illustration n'est pas livrée.</summary>
        static Sprite? BossArt(string enemyId) => string.IsNullOrEmpty(enemyId) ? null : Resources.Load<Sprite>("Illustrations/Enemies/" + enemyId);

        static Label Text(string text, string cls)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList(cls);
            return l;
        }
    }
}
