#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat.Hud
{
    /// <summary>Fiche d'une unité en combat (UI Toolkit) : portrait carré cadré sur le visage, insigne de famille (alliés) ou losange (ennemis), nom, PV, jauge d'énergie, statuts, états (prête, sélectionnée, ciblable, K.O.).</summary>
    public sealed class UnitCardHud
    {
        readonly VisualElement _root = new VisualElement(), _portrait = new VisualElement(), _selectRing = new VisualElement(), _targetRing = new VisualElement(), _flash = new VisualElement(), _arrow = new VisualElement();
        readonly VisualElement _hpFill = new VisualElement(), _gaugeFill = new VisualElement(), _floats = new VisualElement(), _badge = new VisualElement();
        readonly Label _initial = new Label(), _name = new Label(), _sub = new Label(), _hpText = new Label(), _statuses = new Label(), _ko = new Label(), _badgeText = new Label();
        readonly VisualElement _chips = new VisualElement(), _position = new VisualElement(), _elite = new VisualElement();
        readonly List<StatusChip> _chipPool = new List<StatusChip>();
        bool _ally, _targetable, _pulseOn;
        string? _foeKey, _posKey;
        int _phase = -1, _chipSignature = int.MinValue;
        Color _baseFill;
        IVisualElementScheduledItem? _pulse;

        public VisualElement Root => _root;
        public int UnitId { get; private set; }
        /// <summary>Illustration de l'agent (null : silhouette provisoire), reprise en grand au centre du terrain quand il agit.</summary>
        public Sprite? PortraitArt { get; private set; }
        public event Action<int>? Tapped;

        public UnitCardHud()
        {
            _root.AddToClassList("hd-unit");
            _selectRing.AddToClassList("hd-unit__select"); _targetRing.AddToClassList("hd-unit__target");
            _selectRing.pickingMode = PickingMode.Ignore; _targetRing.pickingMode = PickingMode.Ignore;
            _selectRing.style.display = DisplayStyle.None; _targetRing.style.display = DisplayStyle.None;
            // Flèche de ciblage : posée à côté de la fiche, elle pointe vers elle (visible seulement pendant le ciblage).
            _arrow.AddToClassList("hd-unit__arrow"); _arrow.pickingMode = PickingMode.Ignore; _arrow.style.display = DisplayStyle.None;
            _portrait.AddToClassList("hd-unit__portrait");
            _initial.AddToClassList("hd-unit__initial");
            _portrait.Add(_initial);
            _portrait.pickingMode = PickingMode.Ignore;
            _badge.AddToClassList("hd-unit__badge"); _badge.pickingMode = PickingMode.Ignore;
            _badgeText.AddToClassList("hd-unit__badge-text"); _badgeText.pickingMode = PickingMode.Ignore;
            _badge.Add(_badgeText);
            _name.AddToClassList("hd-unit__name"); _sub.AddToClassList("hd-unit__sub");
            var hp = new VisualElement(); hp.AddToClassList("hd-unit__bar"); _hpFill.AddToClassList("hd-unit__hp"); _hpText.AddToClassList("hd-unit__bartext");
            hp.Add(_hpFill); hp.Add(_hpText);
            var gauge = new VisualElement(); gauge.AddToClassList("hd-unit__gauge"); _gaugeFill.AddToClassList("hd-unit__gaugefill"); gauge.Add(_gaugeFill);
            _statuses.AddToClassList("hd-unit__statuses");
            // Icônes d'états sous les barres, position (alliés) et cadre élite (ennemis) : images posées par classes USS.
            _chips.AddToClassList("hd-unit__chips"); _chips.pickingMode = PickingMode.Ignore; _chips.style.display = DisplayStyle.None;
            _position.AddToClassList("hd-unit__pos"); _position.pickingMode = PickingMode.Ignore; _position.style.display = DisplayStyle.None;
            _elite.AddToClassList("hd-unit__elite"); _elite.pickingMode = PickingMode.Ignore; _elite.style.display = DisplayStyle.None;
            _ko.AddToClassList("hd-unit__ko"); _ko.text = "K.O."; _ko.style.display = DisplayStyle.None;
            _flash.AddToClassList("hd-unit__flash"); _flash.pickingMode = PickingMode.Ignore;
            _floats.AddToClassList("hd-unit__floats"); _floats.pickingMode = PickingMode.Ignore;
            foreach (var e in new VisualElement[] { _targetRing, _selectRing, _portrait, _elite, _position, _badge, _name, _sub, hp, gauge, _chips, _statuses, _flash, _ko, _floats, _arrow }) _root.Add(e);
            _root.RegisterCallback<ClickEvent>(_ => Tapped?.Invoke(UnitId));
        }

        static void Border(VisualElement e, Color c)
            => e.style.borderTopColor = e.style.borderBottomColor = e.style.borderLeftColor = e.style.borderRightColor = c;

        public void Setup(UnitCardData d)
        {
            UnitId = d.Id; _ally = d.Ally;
            _root.name = (d.Ally ? "Ally_" : "Enemy_") + d.Id;
            _baseFill = Theme.WithAlpha(d.Ally ? Theme.Frost : Color.Lerp(Theme.SurfaceDark, Theme.Danger, 0.22f), 0.92f);
            _root.style.backgroundColor = _baseFill;
            _root.EnableInClassList("hd-unit--enemy", !d.Ally);
            _arrow.EnableInClassList("hd-unit__arrow--ally", d.Ally);
            _root.EnableInClassList("hd-unit--boss", d.Boss);
            _root.EnableInClassList("hd-unit--matter", d.Family == Family.Matter);
            _root.EnableInClassList("hd-unit--elemental", d.Family == Family.Elemental);
            _root.EnableInClassList("hd-unit--bio", d.Family == Family.PhysicalBio);
            _root.EnableInClassList("hd-unit--perception", d.Family == Family.Perception);
            _sub.text = d.Sub ?? "";
            var family = Theme.FamilyColor(d.Family);
            Border(_portrait, d.Ally ? family : Theme.Danger);
            PortraitArt = d.Portrait.Art;
            // Portrait cadré sur le visage (agents) ou illustration de l'ennemi ; à défaut l'illustration reçue, sinon une silhouette (USS).
            _foeKey = d.Ally ? null : HudPortraits.FoeKey(d.Name, d.DefId); _phase = 0;
            bool framed = d.Ally ? HudPortraits.ApplyAgent(_portrait, PortraitArt, d.Name) : HudPortraits.ApplyEnemy(_portrait, d.Name, d.DefId, 0);
            bool empty = !framed && PortraitArt == null;
            if (!framed) _portrait.style.backgroundImage = PortraitArt != null ? new StyleBackground(Background.FromSprite(PortraitArt)) : new StyleBackground(StyleKeyword.Null);
            _portrait.EnableInClassList("hd-unit__portrait--empty", empty);
            _elite.style.display = !d.Ally && d.Elite ? DisplayStyle.Flex : DisplayStyle.None;
            SetPosition(d);
            _portrait.style.backgroundColor = Color.Lerp(d.Ally ? family : Theme.Danger, Theme.Navy, d.Ally ? 0.55f : 0.7f);
            _initial.text = empty && d.Ally ? d.Portrait.Initial : "";
            _badgeText.text = d.Ally ? "" : d.Portrait.Initial;
            _name.text = d.Name;
            _name.style.color = d.Boss ? Theme.Primary : Theme.Text;
            _hpFill.style.backgroundColor = d.Ally ? Theme.Hp : Theme.HpEnemy;
            _gaugeFill.style.backgroundColor = Theme.Info;
        }

        /// <summary>Icône de position (alliés) teintée par la famille.</summary>
        void SetPosition(UnitCardData d)
        {
            string? key = null;
            if (d.Ally)
                switch (d.Position)
                {
                    case Definitions.Position.FrontLine: key = "frontline"; break;
                    case Definitions.Position.Attacker: key = "attacker"; break;
                    case Definitions.Position.Controller: key = "control"; break;
                    case Definitions.Position.Support: key = "support"; break;
                    case Definitions.Position.Ranged: key = "ranged"; break;
                }
            if (_posKey != null) _position.RemoveFromClassList("hd-pos--" + _posKey);
            _posKey = key;
            _position.style.display = key == null ? DisplayStyle.None : DisplayStyle.Flex;
            if (key == null) return;
            _position.AddToClassList("hd-pos--" + key);
            _position.style.unityBackgroundImageTintColor = Theme.FamilyColor(d.Family);
        }

        /// <summary>Le boss change de phase : son portrait suit (seulement pour les ennemis qui ont une illustration par phase).</summary>
        void SetPhase(int phase)
        {
            if (phase == _phase || !HudPortraits.HasPhases(_foeKey)) return;
            _phase = phase;
            HudPortraits.ApplyEnemy(_portrait, null, _foeKey, phase);
        }

        /// <summary>Identifiant du fichier d'icône d'un statut (dossier Status/) ; null : pas d'icône, la fiche garde le texte.</summary>
        public static string? StatusKey(StatusType type)
        {
            switch (type)
            {
                case StatusType.Burn: return "burn";
                case StatusType.Freeze: return "freeze";
                case StatusType.Shock: return "shock";
                case StatusType.Corrosion: return "corrosion";
                case StatusType.Blind: return "blind";
                case StatusType.Vulnerable: return "vulnerable";
                case StatusType.Bind: return "snare";
                case StatusType.Mark: return "mark";
                case StatusType.Taunt: return "taunt";
                case StatusType.Shield: return "shield";
                case StatusType.Regen: return "regeneration";
                case StatusType.Haste: return "haste";
                case StatusType.Evade: return "dodge";
                case StatusType.Anchor: return "anchor";
                default: return null;
            }
        }

        sealed class StatusChip
        {
            public readonly VisualElement Root = new VisualElement(), Icon = new VisualElement();
            public readonly Label Count = new Label();
            public string? Key;
        }

        const int MaxChips = 8;

        /// <summary>
        /// Statuts en icônes (durée ou cumul en petit chiffre). Reconstruit seulement quand la liste change ; les pastilles sont réutilisées.
        /// Un statut sans icône, ou l'absence de données, garde le texte.
        /// </summary>
        void SetChips(StatusChipData[]? chips, string? text)
        {
            bool allIcons = chips != null;
            if (chips != null) foreach (var c in chips) if (StatusKey(c.Type) == null) { allIcons = false; break; }
            if (!allIcons)
            {
                _chips.style.display = DisplayStyle.None; _chipSignature = int.MinValue;
                _statuses.style.display = DisplayStyle.Flex;
                _statuses.text = text ?? "";
                return;
            }
            _statuses.style.display = DisplayStyle.None;
            int sig = 17;
            foreach (var c in chips!) sig = sig * 31 + (int)c.Type * 100003 + c.Count;
            if (sig == _chipSignature) return;
            _chipSignature = sig;
            _chips.style.display = chips.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            while (_chipPool.Count < Math.Min(chips.Length, MaxChips))
            {
                var chip = new StatusChip();
                chip.Root.AddToClassList("hd-chip"); chip.Icon.AddToClassList("hd-chip__icon"); chip.Count.AddToClassList("hd-chip__count");
                chip.Root.pickingMode = PickingMode.Ignore; chip.Icon.pickingMode = PickingMode.Ignore; chip.Count.pickingMode = PickingMode.Ignore;
                chip.Root.Add(chip.Icon); chip.Root.Add(chip.Count);
                _chips.Add(chip.Root);
                _chipPool.Add(chip);
            }
            for (int i = 0; i < _chipPool.Count; i++)
            {
                var chip = _chipPool[i];
                if (i >= chips.Length) { chip.Root.style.display = DisplayStyle.None; continue; }
                string key = StatusKey(chips[i].Type)!;
                if (chip.Key != key)
                {
                    if (chip.Key != null) chip.Icon.RemoveFromClassList("hd-status--" + chip.Key);
                    chip.Icon.AddToClassList("hd-status--" + key);
                    chip.Key = key;
                }
                chip.Count.text = chips[i].Count > 0 ? chips[i].Count.ToString() : "";
                chip.Count.style.display = chips[i].Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                chip.Root.style.display = DisplayStyle.Flex;
            }
        }

        /// <param name="dimmed">Ciblage en cours et cette unité n'est pas ciblable : la fiche est assombrie.</param>
        public void Refresh(UnitStateData s, bool dimmed = false)
        {
            _hpFill.style.width = Length.Percent(Mathf.Clamp01(s.Hp01) * 100f);
            _hpText.text = s.HpText;
            _gaugeFill.style.width = Length.Percent(Mathf.Clamp01(s.Energy01) * 100f);
            SetPhase(s.Phase);
            SetChips(s.Chips, s.Statuses);
            _ko.style.display = s.Alive ? DisplayStyle.None : DisplayStyle.Flex;
            _root.style.opacity = !s.Alive ? 0.45f : dimmed ? 0.55f : 1f;
            _root.style.backgroundColor = s.Highlight && s.Alive ? Color.Lerp(_baseFill, Theme.Primary, 0.22f) : _baseFill;
            _root.EnableInClassList("hd-unit--ready", s.Highlight && s.Alive);
            _selectRing.style.display = s.Selected && s.Alive ? DisplayStyle.Flex : DisplayStyle.None;
            SetTargetable(s.Targetable && s.Alive);
        }

        /// <summary>Marqueurs de ciblage : cadre laiton et flèche dorée qui oscillent (bascule de classes USS à intervalle fixe, les transitions USS font l'animation) ; retirés dès que le ciblage prend fin.</summary>
        void SetTargetable(bool on)
        {
            if (on == _targetable) return;
            _targetable = on;
            var show = on ? DisplayStyle.Flex : DisplayStyle.None;
            _targetRing.style.display = show; _arrow.style.display = show;
            _pulseOn = false;
            _targetRing.RemoveFromClassList("hd-unit__target--pulse"); _arrow.RemoveFromClassList("hd-unit__arrow--out");
            if (!on) { _pulse?.Pause(); return; }
            if (!HudAnim.Allowed) return;
            _pulse ??= _root.schedule.Execute(PulseStep).Every(600);
            _pulse.Resume();
        }

        void PulseStep()
        {
            _pulseOn = !_pulseOn;
            _targetRing.EnableInClassList("hd-unit__target--pulse", _pulseOn);
            _arrow.EnableInClassList("hd-unit__arrow--out", _pulseOn);
        }

        // ---- Aperçu de la carte tenue : barre de PV fantôme, delta, badge « variable », statuts, K.O. et action annulée
        VisualElement? _pvGhost;
        Label? _pvDelta, _pvVariable, _pvChips, _pvKo;
        /// <summary>Dernier aperçu reçu (tests PlayMode et lecture de l'état affiché) ; null : aucun.</summary>
        public UnitPreviewData? Preview { get; private set; }

        void EnsurePreviewElements()
        {
            if (_pvGhost != null) return;
            _pvGhost = new VisualElement { pickingMode = PickingMode.Ignore }; _pvGhost.AddToClassList("hd-unit__pvghost");
            _hpFill.parent.Insert(1, _pvGhost); // au-dessus du remplissage, sous le texte de la barre
            _pvDelta = new Label { pickingMode = PickingMode.Ignore }; _pvDelta.AddToClassList("hd-unit__pvdelta");
            _pvVariable = new Label { pickingMode = PickingMode.Ignore }; _pvVariable.AddToClassList("hd-unit__pvvar");
            _pvChips = new Label { pickingMode = PickingMode.Ignore, enableRichText = true }; _pvChips.AddToClassList("hd-unit__pvchips");
            _pvKo = new Label { pickingMode = PickingMode.Ignore }; _pvKo.AddToClassList("hd-unit__pvko");
            foreach (var e in new VisualElement[] { _pvDelta, _pvVariable, _pvChips, _pvKo }) _root.Add(e);
        }

        static string Rich(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);

        /// <summary>Montre (ou efface, avec null) ce que la carte tenue changerait sur cette unité ; ne touche pas à l'état réel affiché.</summary>
        public void ShowPreview(UnitPreviewData? d)
        {
            Preview = d;
            if (d == null && _pvGhost == null) return;
            EnsurePreviewElements();
            var show = d.HasValue;
            var p = d.GetValueOrDefault();
            // Barre fantôme : la part de PV perdue (rouge) ou gagnée (vert), calée sur la barre réelle.
            bool loss = p.HpAfter01 < p.HpBefore01, gain = p.HpAfter01 > p.HpBefore01;
            _pvGhost!.style.display = show && (loss || gain) ? DisplayStyle.Flex : DisplayStyle.None;
            if (show)
            {
                float lo = Mathf.Min(p.HpBefore01, p.HpAfter01), hi = Mathf.Max(p.HpBefore01, p.HpAfter01);
                _pvGhost.style.left = Length.Percent(lo * 100f);
                _pvGhost.style.width = Length.Percent((hi - lo) * 100f);
                _pvGhost.style.backgroundColor = gain ? Theme.WithAlpha(Theme.Success, 0.75f) : Theme.WithAlpha(Theme.Danger, 0.8f);
            }
            _pvDelta!.text = show ? p.HpDeltaText ?? "" : "";
            _pvDelta.style.display = show && !string.IsNullOrEmpty(p.HpDeltaText) ? DisplayStyle.Flex : DisplayStyle.None;
            _pvDelta.style.color = gain ? Theme.Success : Theme.Primary;
            _pvVariable!.text = show ? p.VariableLabel ?? "" : "";
            _pvVariable.style.display = show && p.Variable ? DisplayStyle.Flex : DisplayStyle.None;
            _pvKo!.text = !show || p.Defeat == PreviewDefeat.None ? "" : p.Defeat == PreviewDefeat.Sure ? "K.O.!" : "K.O. ?";
            _pvKo.style.display = show && p.Defeat != PreviewDefeat.None ? DisplayStyle.Flex : DisplayStyle.None;
            var sb = new System.Text.StringBuilder();
            if (show)
            {
                if (!string.IsNullOrEmpty(p.ShieldText)) sb.Append(p.ShieldText).Append("  ");
                if (p.Added != null) foreach (var c in p.Added) sb.Append("<color=").Append(Rich(Theme.Success)).Append(">+").Append(c.Label).Append(c.Possible ? " ?" : "").Append("</color>  ");
                if (p.Removed != null) foreach (var c in p.Removed) sb.Append("<color=").Append(Rich(Theme.Danger)).Append(">−").Append(c.Label).Append(c.Possible ? " ?" : "").Append("</color>  ");
                if (!string.IsNullOrEmpty(p.PlanText)) sb.Append(p.PlanText);
            }
            _pvChips!.text = sb.ToString().TrimEnd();
            _pvChips.style.display = sb.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            // Pendant l'aperçu, la ligne de statuts réels cède la place aux changements prévus.
            _statuses.style.display = sb.Length > 0 ? DisplayStyle.None : DisplayStyle.Flex;
            _root.EnableInClassList("hd-unit--preview", show);
        }

        public void Float(string text, Color color, int size)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList("hd-unit__float");
            l.style.color = color; l.style.fontSize = size;
            float x = UnityEngine.Random.Range(-60f, 60f);
            _floats.Add(l);
            l.style.marginLeft = x;
            HudAnim.PopIn(l, 0.18f);
            HudAnim.Run(l, 0.9f, k =>
            {
                l.style.translate = new Translate(0f, -90f * k);
                l.style.opacity = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
            }, () => l.RemoveFromHierarchy());
        }

        public void Hit(VisualElement fxLayer)
        {
            HudAnim.Shake(_root, 12f, 0.25f);
            HudAnim.Flash(_flash);
            HudAnim.Sparks(fxLayer, _root, _ally ? Theme.Danger : Theme.Primary, 7, 90f, 14f);
        }

        public void Act()
        {
            HudAnim.Punch(_root, 0.08f, 0.25f);
            if (!HudAnim.Allowed) return;
            // Élan : la carte se penche vers son adversaire (alliés à gauche, ennemis au centre) puis revient.
            float lean = _ally ? -7f : 7f;
            HudAnim.Run(_root, 0.3f, k => HudAnim.SetRotate(_root, lean * Mathf.Sin(k * Mathf.PI)), () => HudAnim.SetRotate(_root, 0f));
        }

        public void FlashCard() => HudAnim.Flash(_flash);

        /// <summary>Centre de la carte en coordonnées de l'écran (y vers le haut), pour les dépôts et les tests.</summary>
        public Vector2 ScreenCenter() => HudSpace.PanelToScreen(_root, _root.worldBound.center);
        public bool ContainsScreenPoint(Vector2 screen) => _root.worldBound.Contains(HudSpace.ScreenToPanel(_root, screen));
    }
}
