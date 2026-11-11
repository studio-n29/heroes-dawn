#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Characters3D
{
    /// <summary>
    /// Un personnage 3D de la scène des activités (héros ou ennemi) : joue ses animations (repos, marche, course, attaque, sort, coup reçu, mort)
    /// et se colore : deux teintes (vêtements, équipement) appliquées par zones, la peau restant intacte, et des éclats sur tout le corps.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Character3DView : MonoBehaviour
    {
        public const string Idle = "Idle", Walk = "Walk", Run = "Run", Attack = "Attack", Cast = "Cast", Hit = "Hit", Death = "Death";
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int TintBId = Shader.PropertyToID("_TintB");
        static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
        static readonly int MaskMap = Shader.PropertyToID("_MaskMap");

        [SerializeField] Animator animator = null!;
        [SerializeField] Renderer[] renderers = System.Array.Empty<Renderer>();
        [Tooltip("Vitesse (m/s) au-delà de laquelle le personnage marche, puis court.")]
        [SerializeField] float walkFrom = 0.25f, runFrom = 3.6f;

        MaterialPropertyBlock? _block;
        string _state = "";
        float _actionLeft;
        float _flashLeft;
        Color _tint = Color.white, _tintB = Color.white, _flash = Color.clear;
        bool _dead;

        public bool Dead => _dead;
        /// <summary>État d'animation joué en ce moment (tests).</summary>
        public string State => _state;
        public Color Tint => _tint;
        public Color TintB => _tintB;

        public void Setup(Animator a, Renderer[] r) { animator = a; renderers = r; }

        void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<Renderer>();
            if (animator != null) animator.applyRootMotion = false;
            Play(Idle, 0f);
        }

        /// <summary>Vitesse de déplacement actuelle : repos, marche ou course (sans effet pendant une action ou après la mort).</summary>
        public void SetMoveSpeed(float metersPerSecond)
        {
            if (_dead || _actionLeft > 0f) return;
            Play(metersPerSecond >= runFrom ? Run : metersPerSecond >= walkFrom ? Walk : Idle, 0.15f);
        }

        /// <summary>Joue une action (attaque, sort, coup reçu) pendant <paramref name="seconds"/>, puis revient au déplacement.</summary>
        public void PlayAction(string state, float seconds)
        {
            if (_dead) return;
            _actionLeft = seconds;
            Play(state, 0.08f, restart: true);
        }

        public void PlayAttack() => PlayAction(Attack, 0.8f);
        public void PlayCast() => PlayAction(HasState(Cast) ? Cast : Attack, 1.0f);
        public void PlayHit() => PlayAction(Hit, 0.45f);

        public void PlayDeath()
        {
            if (_dead) return;
            _dead = true; _actionLeft = 0f;
            Play(Death, 0.1f, restart: true);
        }

        /// <summary>Remet le personnage debout et au repos (réutilisation d'un même objet).</summary>
        public void Revive()
        {
            _dead = false; _actionLeft = 0f;
            Play(Idle, 0f, restart: true);
        }

        bool HasState(string state) => animator != null && animator.HasState(0, Animator.StringToHash(state));

        void Play(string state, float fade, bool restart = false)
        {
            if (animator == null || (!restart && _state == state)) return;
            _state = state;
            if (!HasState(state)) return;
            if (fade <= 0f) animator.Play(state, 0, 0f);
            else animator.CrossFadeInFixedTime(state, fade, 0, 0f);
        }

        /// <summary>Couleurs du personnage : vêtements et équipement sombre (la peau n'est pas teintée). Sans zones dans la texture, ces teintes sont sans effet.</summary>
        public void SetTint(Color clothes, Color gear)
        {
            _tint = clothes; _tintB = gear;
            Apply();
        }

        public void SetTint(Color tint) => SetTint(tint, tint);

        /// <summary>Éclat de couleur sur tout le corps pendant <paramref name="seconds"/> (coup reçu, charge annoncée), puis retour aux couleurs.</summary>
        public void Flash(Color color, float seconds)
        {
            _flash = new Color(color.r, color.g, color.b, 0.85f); _flashLeft = seconds;
            Apply();
        }

        /// <summary>Masque de zones propre à ce personnage (aperçu de l'outil de couleurs) ; null : celui de son matériau.</summary>
        public Texture? MaskOverride { get; set; }

        void Apply()
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;
                _block ??= new MaterialPropertyBlock();
                r.GetPropertyBlock(_block);
                _block.SetColor(BaseColor, _tint);
                _block.SetColor(TintBId, _tintB);
                _block.SetColor(FlashColor, _flashLeft > 0f ? _flash : Color.clear);
                if (MaskOverride != null) _block.SetTexture(MaskMap, MaskOverride);
                r.SetPropertyBlock(_block);
            }
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (_flashLeft > 0f && (_flashLeft -= dt) <= 0f) { _flashLeft = 0f; Apply(); }
            if (_actionLeft > 0f && (_actionLeft -= dt) <= 0f && !_dead) { _actionLeft = 0f; Play(Idle, 0.15f, restart: true); }
        }
    }
}
