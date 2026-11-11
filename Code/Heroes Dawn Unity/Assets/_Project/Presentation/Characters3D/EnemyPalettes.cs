#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Characters3D
{
    /// <summary>
    /// Palettes de couleurs des ennemis 3D : chaque sorte d'ennemi a plusieurs variantes de couleurs (réglées dans l'outil « Couleurs des ennemis »).
    /// Une variante a deux teintes : les vêtements et l'équipement sombre ; la peau n'est jamais teintée. Les zones viennent d'un masque tiré de la texture.
    /// </summary>
    [CreateAssetMenu(menuName = "Heroes Dawn/Palettes des ennemis 3D", fileName = "EnemyPalettes")]
    public sealed class EnemyPalettes : ScriptableObject
    {
        public const string ResourcePath = "Characters3D/EnemyPalettes";
        /// <summary>Version des palettes d'origine ; une palette plus ancienne est remplacée à la génération (les masques et les deux teintes sont arrivés en 2).</summary>
        public const int CurrentVersion = 3;

        [Serializable]
        public sealed class Variant
        {
            public string name = "Variante";
            [Tooltip("Vêtements (la zone claire de la texture).")]
            public Color tint = Color.white;
            [Tooltip("Équipement sombre : gants, chaussures, sangles, accessoires.")]
            public Color tintB = Color.white;
        }

        /// <summary>Règles qui séparent la texture en zones : peau (jamais teintée), équipement sombre, vêtements clairs.</summary>
        [Serializable]
        public sealed class MaskRules
        {
            [Tooltip("Faux : tout le modèle est teinté (créature de pierre, sans peau).")]
            public bool separateSkin = true;
            [Range(0f, 0.4f), Tooltip("Saturation à partir de laquelle une couleur chaude est de la peau.")]
            public float skinSaturation = 0.09f;
            [Range(0f, 0.2f), Tooltip("Teinte maximale (0 = rouge, 0,1 = orange) de la peau.")]
            public float skinHueMax = 0.11f;
            [Range(0f, 1f), Tooltip("Luminosité sous laquelle un pixel est de l'équipement sombre.")]
            public float darkBelow = 0.38f;
            [Tooltip("Mains, doigts et poignets : jamais teintés (peau ou gants).")]
            public bool keepHands = true;
            [Tooltip("Tête et cou : jamais teints (peau du visage). Décoché : casque ou masque teinté.")]
            public bool keepHead = false;
            [Tooltip("Pieds : teinte d équipement (chaussures) au lieu des vêtements.")]
            public bool feetAsGear = true;
        }

        [Serializable]
        public sealed class Kind
        {
            /// <summary>Identifiant du modèle (« enemy_soldier »…), qui est aussi le nom de son prefab.</summary>
            public string id = "";
            public string displayName = "";
            [Tooltip("Échelle du modèle dans l'arène (1 = taille d'origine).")]
            public float scale = 1f;
            public MaskRules mask = new MaskRules();
            public Variant[] variants = Array.Empty<Variant>();
        }

        public int version;
        public Kind[] kinds = Array.Empty<Kind>();

        static EnemyPalettes? _cached;

        public static EnemyPalettes? Load() => _cached != null ? _cached : (_cached = Resources.Load<EnemyPalettes>(ResourcePath));

        public Kind? Find(string id)
        {
            foreach (var k in kinds) if (k.id == id) return k;
            return null;
        }

        /// <summary>Variante <paramref name="variant"/> (prise modulo le nombre de variantes) ; null si la sorte n'existe pas.</summary>
        public Variant? Pick(string id, int variant)
        {
            var k = Find(id);
            if (k == null || k.variants.Length == 0) return null;
            int i = ((variant % k.variants.Length) + k.variants.Length) % k.variants.Length;
            return k.variants[i];
        }

        public Color Tint(string id, int variant) => Pick(id, variant)?.tint ?? Color.white;

        public float Scale(string id) => Find(id)?.scale ?? 1f;

        /// <summary>Palettes d'origine : quatre variantes par sorte d'ennemi.</summary>
        public static Kind[] Defaults() => new[]
        {
            Make("enemy_soldier", "Soldat", 1f, true, false,
                 ("Kaki", C(0.62f, 0.72f, 0.45f), C(0.45f, 0.5f, 0.3f)), ("Gris bleu", C(0.55f, 0.65f, 0.82f), C(0.35f, 0.4f, 0.55f)),
                 ("Désert", C(0.9f, 0.8f, 0.55f), C(0.65f, 0.5f, 0.3f)), ("Sang", C(0.9f, 0.4f, 0.4f), C(0.55f, 0.2f, 0.2f))),
            Make("enemy_gangster", "Gangster", 1f, true, true,
                 ("Rouge", C(0.95f, 0.35f, 0.35f), C(0.3f, 0.3f, 0.35f)), ("Bleu", C(0.4f, 0.55f, 0.95f), C(0.95f, 0.85f, 0.3f)),
                 ("Violet", C(0.7f, 0.45f, 0.95f), C(0.25f, 0.25f, 0.3f)), ("Orange", C(1f, 0.65f, 0.3f), C(0.3f, 0.5f, 0.8f))),
            Make("enemy_creature", "Créature", 1.25f, false, false,
                 ("Braise", C(0.98f, 0.55f, 0.2f), C(0.98f, 0.55f, 0.2f)), ("Nuit", C(0.65f, 0.4f, 0.95f), C(0.65f, 0.4f, 0.95f)),
                 ("Acide", C(0.55f, 0.95f, 0.4f), C(0.55f, 0.95f, 0.4f)), ("Rouille", C(0.88f, 0.32f, 0.3f), C(0.88f, 0.32f, 0.3f))),
        };

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        static Kind Make(string id, string name, float scale, bool separateSkin, bool keepHead, params (string n, Color a, Color b)[] v)
        {
            var variants = new Variant[v.Length];
            for (int i = 0; i < v.Length; i++) variants[i] = new Variant { name = v[i].n, tint = v[i].a, tintB = v[i].b };
            return new Kind { id = id, displayName = name, scale = scale, mask = new MaskRules { separateSkin = separateSkin, keepHead = keepHead }, variants = variants };
        }
    }

    /// <summary>Accès aux modèles 3D des personnages (prefabs générés par l'outil « Personnages 3D »).</summary>
    public static class Character3DLibrary
    {
        public const string Folder = "Characters3D/";

        /// <summary>Prefab du personnage (« agent_d », « enemy_soldier »…) ; null s'il n'existe pas.</summary>
        public static GameObject? Prefab(string id) => Resources.Load<GameObject>(Folder + id);
    }
}
