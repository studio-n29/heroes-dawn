#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Bandeau d'un combattant à l'entrée en combat (UI Toolkit) : portrait, nom, ligne de détail, filet de la couleur de sa famille ; le côté droit est en miroir.</summary>
    public sealed class CombatIntroBanner
    {
        public VisualElement Root { get; }
        public string Name { get; }
        public string Sub { get; }

        public CombatIntroBanner(CombatIntroFighter fighter, bool left)
        {
            Name = fighter.Name; Sub = fighter.Sub;
            Root = new VisualElement();
            Root.AddToClassList("hd-intro__banner");
            Root.EnableInClassList("hd-intro__banner--right", !left);
            var accent = new VisualElement(); accent.AddToClassList("hd-intro__accent");
            accent.style.backgroundColor = Theme.FamilyColor(fighter.Portrait.Family);
            var portrait = new VisualElement(); portrait.AddToClassList("hd-intro__portrait");
            var initial = new Label(); initial.AddToClassList("hd-intro__initial"); portrait.Add(initial);
            HdPortrait.Fill(portrait, initial, fighter.Portrait);
            var text = new VisualElement(); text.AddToClassList("hd-intro__text");
            var name = new Label(fighter.Name); name.AddToClassList("hd-intro__name");
            var sub = new Label(fighter.Sub); sub.AddToClassList("hd-intro__sub");
            text.Add(name); text.Add(sub);
            Root.Add(accent); Root.Add(portrait); Root.Add(text);
        }
    }

    /// <summary>
    /// Entrée en combat (fenêtre UI Toolkit) : les agents du joueur entrent par la gauche, les ennemis par la droite, un bandeau chacun.
    /// Un toucher ou « Passer » ferme l'introduction ; une introduction déjà vue est jouée sans glissement et plus courte.
    /// </summary>
    public sealed class CombatIntroPopupView : HdPopup, ICombatIntroView
    {
        /// <summary>Durées en secondes : glissement d'un bandeau, décalage entre deux bandeaux, temps de lecture (première fois, répétition).</summary>
        const float Slide = 0.3f, Stagger = 0.18f, HoldFirst = 1.5f, HoldRepeat = 0.5f, SlideDistance = 900f;

        readonly List<CombatIntroBanner> _banners = new List<CombatIntroBanner>();
        UnityEngine.UIElements.Button _tap = null!, _skip = null!;
        Label _allyTitle = null!, _enemyTitle = null!;
        VisualElement _allies = null!, _enemies = null!;
        CombatIntroData? _last;
        bool _closed;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        protected override VisualElement DefaultFocus => _tap;

        public HdButtonRef TapButton { get; private set; } = null!;
        public HdButtonRef SkipButton { get; private set; } = null!;
        /// <summary>Bandeaux affichés, alliés d'abord (tests).</summary>
        public IReadOnlyList<CombatIntroBanner> Banners => _banners;
        public string AllyTitle => _allyTitle != null ? _allyTitle.text : "";
        public string EnemyTitle => _enemyTitle != null ? _enemyTitle.text : "";

        protected override void Bind()
        {
            _tap = Q<UnityEngine.UIElements.Button>("tap"); _skip = Q<UnityEngine.UIElements.Button>("skip");
            _allyTitle = Q<Label>("allyTitle"); _enemyTitle = Q<Label>("enemyTitle");
            _allies = Q<VisualElement>("allies"); _enemies = Q<VisualElement>("enemies");
            TapButton = new HdButtonRef(_tap, this); SkipButton = new HdButtonRef(_skip, this);
            _skip.text = Loc.T("combat.intro.skip");
            _tap.clicked += () => _closed = true;
            _skip.clicked += () => _closed = true;
            if (_last != null) Fill(_last);
        }

        void Fill(CombatIntroData data)
        {
            _allyTitle.text = data.AllyTitle; _enemyTitle.text = data.EnemyTitle;
            _allies.Clear(); _enemies.Clear(); _banners.Clear();
            foreach (var f in data.Allies) { var b = new CombatIntroBanner(f, true); _banners.Add(b); _allies.Add(b.Root); }
            foreach (var f in data.Enemies) { var b = new CombatIntroBanner(f, false); _banners.Add(b); _enemies.Add(b.Root); }
        }

        public async UniTask PlayAsync(CombatIntroData data, CancellationToken ct)
        {
            _last = data; _closed = false;
            await UniTask.WaitUntil(() => _allies != null, cancellationToken: ct); // le document se monte au premier affichage
            Fill(data);
            bool animate = !data.Repeat && !MotionSettings.Reduced;
            float t = 0f;
            float total = animate ? Stagger * _banners.Count + Slide + HoldFirst : data.Repeat ? HoldRepeat : HoldFirst * 0.5f;
            while (!_closed && t < total)
            {
                for (int i = 0; i < _banners.Count; i++)
                {
                    float k = animate ? Mathf.Clamp01((t - Stagger * i) / Slide) : 1f;
                    float eased = 1f - (1f - k) * (1f - k);
                    bool left = i < data.Allies.Count;
                    HudAnim.SetTranslate(_banners[i].Root, (1f - eased) * SlideDistance * (left ? -1f : 1f), 0f);
                }
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                t += Time.unscaledDeltaTime;
            }
        }
    }
}
