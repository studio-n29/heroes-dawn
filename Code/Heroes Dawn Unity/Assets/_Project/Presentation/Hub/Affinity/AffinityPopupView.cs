#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Affinités d'un agent (popup) : son lien et le bonus de leur duo avec chaque agent de l'effectif ; se termine à la fermeture.</summary>
    public interface IAffinityPopupView : IPopupView
    {
        UniTask ShowAsync(AffinityPopupData data, CancellationToken ct);
    }

    /// <summary>
    /// Fenêtre d'affinités (UI Toolkit, planche 5.4) : graphe en étoile, l'agent au centre et ses affinités sur une ellipse autour de lui,
    /// chaque nœud relié au centre par un lien dont le trait s'épaissit avec le niveau de lien (3 / 8 / 15 / 25 victoires) ; une fiche à droite
    /// donne le détail du nœud choisi (focus manette ou toucher). Au-delà de six affinités, ou si les nœuds ne tiennent pas dans la place
    /// disponible, la fenêtre reprend la liste défilante d'origine. Retour = Fermer.
    /// </summary>
    public sealed class AffinityPopupView : HdPopup, IAffinityPopupView
    {
        /// <summary>Nombre maximal de nœuds de l'étoile ; au-delà, liste défilante.</summary>
        const int MaxStar = 6;
        /// <summary>Rapport hauteur / largeur du nœud (256 × 320) et largeur minimale lisible d'un nœud, en pixels.</summary>
        const float NodeAspect = 1.25f, MinNodeWidth = 124f, Gap = 10f;

        sealed class NodeEl
        {
            public VisualElement Root = null!;
            public VisualElement Link = null!;
        }

        readonly List<AffinityRowData> _rows = new List<AffinityRowData>();
        readonly List<NodeEl> _nodes = new List<NodeEl>();
        Label _title = null!, _subtitle = null!, _empty = null!;
        Label _detailName = null!, _detailLink = null!, _detailBonus = null!, _detailPair = null!;
        ScrollView _list = null!;
        VisualElement _starBox = null!, _star = null!;
        VisualElement? _center;
        UnityEngine.UIElements.Button _close = null!;
        UniTaskCompletionSource? _done;
        AffinityPopupData? _last;
        Vector2 _starSize;
        int _selected = -1;
        bool _starMode;

        public override bool Cancelable => true;
        public IReadOnlyList<AffinityRowData> Rows => _rows;
        public HdButtonRef CloseButton { get; private set; } = null!;
        /// <summary>L'étoile est affichée (sinon la liste défilante).</summary>
        public bool StarMode => _starMode;
        /// <summary>Bouton du nœud i de l'étoile (tests PlayMode).</summary>
        public HdButtonRef NodeButton(int i) => new HdButtonRef((UnityEngine.UIElements.Button)_nodes[i].Root, this);
        protected override VisualElement DefaultFocus => _close;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _subtitle = Q<Label>("subtitle"); _empty = Q<Label>("empty");
            _list = Q<ScrollView>("list"); _close = Q<UnityEngine.UIElements.Button>("close");
            _starBox = Q<VisualElement>("starBox"); _star = Q<VisualElement>("star");
            _detailName = Q<Label>("detailName"); _detailLink = Q<Label>("detailLink");
            _detailBonus = Q<Label>("detailBonus"); _detailPair = Q<Label>("detailPair");
            CloseButton = new HdButtonRef(_close, this);
            _close.clicked += () => _done?.TrySetResult();
            _star.RegisterCallback<GeometryChangedEvent>(e =>
            {
                var size = new Vector2(e.newRect.width, e.newRect.height);
                if ((size - _starSize).sqrMagnitude < 1f) return;
                _starSize = size;
                if (_starMode) LayoutStar();
            });
            _nodes.Clear(); _center = null; _selected = -1; _starMode = false; _starSize = Vector2.zero;
            _starBox.style.display = DisplayStyle.None;
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill(AffinityPopupData d)
        {
            _title.text = d.Title; _subtitle.text = d.Subtitle; _close.text = d.Close;
            _list.Clear(); _rows.Clear(); _star.Clear(); _nodes.Clear(); _center = null; _selected = -1;
            foreach (var r in d.Rows)
            {
                _rows.Add(r);
                _list.Add(NewListRow(r));
            }
            _empty.text = d.Empty;
            _empty.style.display = d.Rows.Length == 0 ? DisplayStyle.Flex : DisplayStyle.None;

            bool star = d.Rows.Length > 0 && d.Rows.Length <= MaxStar;
            if (star) BuildStar(d);
            SetMode(star);
            if (star)
            {
                Select(0);
                _starSize = Vector2.zero;
                // La place de l'étoile est connue après la première mise en page (le reste vient de GeometryChangedEvent).
                _star.schedule.Execute(() =>
                {
                    _starSize = new Vector2(_star.layout.width, _star.layout.height);
                    if (_starMode) LayoutStar();
                });
            }
        }

        void SetMode(bool star)
        {
            _starMode = star;
            _starBox.style.display = star ? DisplayStyle.Flex : DisplayStyle.None;
            _list.style.display = !star && _rows.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // ------------------------------------------------------------------ Liste défilante (repli)

        static VisualElement NewListRow(AffinityRowData r)
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList("hd-poprow");
            var art = new VisualElement { pickingMode = PickingMode.Ignore };
            art.AddToClassList("hd-poprow__art");
            // Portrait cadré sur le visage, bordé à la couleur de la famille, icône de famille en coin.
            art.AddToClassList("hd-aff__face");
            HdTeamUi.SetFamily(art, r.Portrait.Family, "hd-aff__face--");
            HdTeamUi.Head(art, r.Portrait.Art);
            var family = HdTeamUi.FamilyIcon(r.Portrait.Family);
            family.AddToClassList("hd-aff__family");
            art.Add(family);
            row.AddToClassList("hd-aff__row");
            var text = new VisualElement { pickingMode = PickingMode.Ignore };
            text.AddToClassList("hd-poprow__text");
            var name = new Label(r.Name) { pickingMode = PickingMode.Ignore }; name.AddToClassList("hd-poprow__name");
            var link = new Label(r.Link + "  " + r.Pair) { pickingMode = PickingMode.Ignore }; link.AddToClassList("hd-pop__hint");
            text.Add(name); text.Add(link);
            var bonus = new Label(r.Bonus) { pickingMode = PickingMode.Ignore }; bonus.AddToClassList("hd-poprow__label");
            bonus.style.color = r.HasBonus ? Theme.Success : Theme.TextMuted;
            row.Add(art); row.Add(text); row.Add(bonus);
            return row;
        }

        // ------------------------------------------------------------------ Étoile

        /// <summary>Nœud du graphe : cadre <c>Affinity/node</c> (9 tranches, bas protégé), portrait dans le carré intérieur, nom et famille dessous.</summary>
        static VisualElement NewNode(PortraitData portrait, string name, string family, bool button)
        {
            VisualElement root;
            if (button)
            {
                var b = new UnityEngine.UIElements.Button();
                b.RemoveFromClassList("unity-button");
                root = b;
            }
            else root = new VisualElement { pickingMode = PickingMode.Ignore };
            root.AddToClassList("hd-aff__node");
            var face = new VisualElement { pickingMode = PickingMode.Ignore };
            face.AddToClassList("hd-aff__nodeface");
            HdTeamUi.Head(face, portrait.Art);
            var fam = HdTeamUi.FamilyIcon(portrait.Family);
            fam.AddToClassList("hd-aff__nodefam");
            face.Add(fam);
            var text = new VisualElement { pickingMode = PickingMode.Ignore };
            text.AddToClassList("hd-aff__nodetext");
            var n = new Label(name) { pickingMode = PickingMode.Ignore }; n.AddToClassList("hd-aff__nodename");
            var f = new Label(family) { pickingMode = PickingMode.Ignore }; f.AddToClassList("hd-aff__nodefamily");
            text.Add(n); text.Add(f);
            root.Add(face); root.Add(text);
            return root;
        }

        /// <summary>Classe USS du lien selon le niveau : 3, 8, 15 et 25 victoires (niveaux 2 à 5) ; le niveau 1 reprend le fil le plus fin, atténué.</summary>
        static string LinkClass(int level) => level >= 5 ? "hd-aff__link--25" : level == 4 ? "hd-aff__link--15" : level == 3 ? "hd-aff__link--8" : "hd-aff__link--3";

        void BuildStar(AffinityPopupData d)
        {
            // Liens d'abord (sous les nœuds), puis le centre, puis les affinités.
            var links = new VisualElement[d.Rows.Length];
            for (int i = 0; i < d.Rows.Length; i++)
            {
                var link = new VisualElement { pickingMode = PickingMode.Ignore };
                link.AddToClassList("hd-aff__link");
                link.AddToClassList(LinkClass(d.Rows[i].Level));
                link.EnableInClassList("hd-aff__link--faint", d.Rows[i].Level <= 1);
                _star.Add(link);
                links[i] = link;
            }
            _center = NewNode(d.Center, d.CenterName ?? "", d.CenterFamily ?? "", false);
            _center.AddToClassList("hd-aff__node--center");
            _star.Add(_center);
            for (int i = 0; i < d.Rows.Length; i++)
            {
                int k = i;
                var r = d.Rows[i];
                var node = (UnityEngine.UIElements.Button)NewNode(r.Portrait, r.Name, r.FamilyText ?? "", true);
                node.clicked += () => Select(k);
                node.RegisterCallback<FocusInEvent>(_ => Select(k));
                node.tooltip = r.Name;
                _star.Add(node);
                _nodes.Add(new NodeEl { Root = node, Link = links[i] });
            }
        }

        void Select(int index)
        {
            if (index < 0 || index >= _rows.Count || index == _selected) return;
            _selected = index;
            for (int i = 0; i < _nodes.Count; i++) _nodes[i].Root.EnableInClassList("hd-aff__node--sel", i == index);
            var r = _rows[index];
            _detailName.text = r.Name;
            _detailLink.text = r.Link;
            _detailBonus.text = r.Bonus;
            _detailBonus.style.color = r.HasBonus ? Theme.Success : Theme.TextMuted;
            _detailPair.text = r.Pair;
        }

        /// <summary>Taille de départ des nœuds pour la place donnée (largeur d'un nœud), avant réduction.</summary>
        static float StartNodeWidth(float w, float h) => Mathf.Min(w * 0.2f, h * 0.46f / NodeAspect);

        /// <summary>Centres des nœuds par rapport au centre de l'étoile, ou false si deux nœuds (ou un nœud et le centre) se recouvrent.</summary>
        static bool Place(int n, float w, float h, float nw, Vector2[] centers)
        {
            float nh = nw * NodeAspect;
            float ry = (h - nh) * 0.5f, rx = (w - nw) * 0.5f;
            if (rx <= 0f || ry <= 0f) return false;
            // Nombre pair : aucun nœud à la verticale du centre ; un seul nœud : à droite.
            float start = n == 1 ? 0f : n % 2 == 0 ? -90f + 180f / n : -90f;
            for (int i = 0; i < n; i++)
            {
                float a = (start + 360f * i / n) * Mathf.Deg2Rad;
                centers[i] = new Vector2(rx * Mathf.Cos(a), ry * Mathf.Sin(a));
            }
            for (int i = 0; i < n; i++)
            {
                if (Overlap(centers[i], nw, nh, Vector2.zero, nw, nh)) return false;
                for (int j = i + 1; j < n; j++) if (Overlap(centers[i], nw, nh, centers[j], nw, nh)) return false;
            }
            return true;
        }

        static bool Overlap(Vector2 a, float aw, float ah, Vector2 b, float bw, float bh)
            => Mathf.Abs(a.x - b.x) < (aw + bw) * 0.5f + Gap && Mathf.Abs(a.y - b.y) < (ah + bh) * 0.5f + Gap;

        /// <summary>Pose les nœuds sur l'ellipse et chaque lien entre le centre et son nœud ; si rien ne tient, bascule sur la liste.</summary>
        void LayoutStar()
        {
            int n = _nodes.Count;
            float w = _starSize.x, h = _starSize.y;
            if (n == 0 || _center == null || !(w >= 10f) || !(h >= 10f)) return;
            var centers = new Vector2[n];
            float nw = StartNodeWidth(w, h);
            while (nw >= MinNodeWidth && !Place(n, w, h, nw, centers)) nw *= 0.94f;
            if (nw < MinNodeWidth) { SetMode(false); return; }
            SetMode(true);
            float nh = nw * NodeAspect;
            float cx = w * 0.5f, cy = h * 0.5f;
            PlaceAt(_center, cx, cy, nw, nh);
            for (int i = 0; i < n; i++)
            {
                PlaceAt(_nodes[i].Root, cx + centers[i].x, cy + centers[i].y, nw, nh);
                // Le lien (256 × 128) garde son rapport : sa largeur est la distance centre-nœud, pivoté autour de son milieu.
                float d = centers[i].magnitude;
                var link = _nodes[i].Link;
                link.style.width = d; link.style.height = d * 0.5f;
                link.style.left = cx + centers[i].x * 0.5f - d * 0.5f;
                link.style.top = cy + centers[i].y * 0.5f - d * 0.25f;
                float deg = Mathf.Atan2(centers[i].y, centers[i].x) * Mathf.Rad2Deg;
                link.style.rotate = new StyleRotate(new Rotate(new Angle(deg, AngleUnit.Degree)));
            }
        }

        static void PlaceAt(VisualElement e, float cx, float cy, float w, float h)
        {
            e.style.width = w; e.style.height = h;
            e.style.left = cx - w * 0.5f; e.style.top = cy - h * 0.5f;
        }

        public UniTask ShowAsync(AffinityPopupData d, CancellationToken ct)
        {
            _last = d;
            if (_title != null) Fill(d);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }

        public override void RequestCancel() => _done?.TrySetResult();
    }
}
