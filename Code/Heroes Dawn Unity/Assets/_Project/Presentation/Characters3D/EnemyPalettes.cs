#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Characters3D
{
    /// <summary>Palettes de couleurs des ennemis 3D : chaque sorte d'ennemi a plusieurs variantes de teinte (réglées dans l'outil « Couleurs des ennemis »).</summary>
    [CreateAssetMenu(menuName = "Heroes Dawn/Palettes des ennemis 3D", fileName = "EnemyPalettes")]
    public sealed class EnemyPalettes : ScriptableObject
    {
        public const string ResourcePath = "Characters3D/EnemyPalettes";

        [Serializable]
        public sealed class Variant
        {
            public string name = "Variante";
            public Color tint = Color.white;
        }

        [Serializable]
        public sealed class Kind
        {
            /// <summary>Identifiant du modèle (« enemy_soldier »…), qui est aussi le nom de son prefab.</summary>
            public string id = "";
            public string displayName = "";
            [Tooltip("Échelle du modèle dans l'arène (1 = taille d'origine).")]
            public float scale = 1f;
            public Variant[] variants = Array.Empty<Variant>();
        }

        public Kind[] kinds = Array.Empty<Kind>();

        static EnemyPalettes? _cached;

        public static EnemyPalettes? Load() => _cached != null ? _cached : (_cached = Resources.Load<EnemyPalettes>(ResourcePath));

        public Kind? Find(string id)
        {
            foreach (var k in kinds) if (k.id == id) return k;
            return null;
        }

        /// <summary>Teinte de la variante <paramref name="variant"/> (prise modulo le nombre de variantes) ; blanc si la sorte n'existe pas.</summary>
        public Color Tint(string id, int variant)
        {
            var k = Find(id);
            if (k == null || k.variants.Length == 0) return Color.white;
            int i = ((variant % k.variants.Length) + k.variants.Length) % k.variants.Length;
            return k.variants[i].tint;
        }

        public float Scale(string id) => Find(id)?.scale ?? 1f;

        /// <summary>Palettes d'origine : quatre variantes par sorte d'ennemi.</summary>
        public static Kind[] Defaults() => new[]
        {
            Make("enemy_soldier", "Soldat", 1f, ("Kaki", 0.62f, 0.72f, 0.45f), ("Gris bleu", 0.55f, 0.65f, 0.82f), ("Désert", 0.9f, 0.8f, 0.55f), ("Sang", 0.9f, 0.4f, 0.4f)),
            Make("enemy_gangster", "Gangster", 1f, ("Rouge", 0.95f, 0.35f, 0.35f), ("Bleu", 0.4f, 0.55f, 0.95f), ("Violet", 0.7f, 0.45f, 0.95f), ("Orange", 1f, 0.65f, 0.3f)),
            Make("enemy_creature", "Créature", 1.25f, ("Braise", 0.98f, 0.55f, 0.2f), ("Nuit", 0.65f, 0.4f, 0.95f), ("Acide", 0.55f, 0.95f, 0.4f), ("Rouille", 0.88f, 0.32f, 0.3f)),
        };

        static Kind Make(string id, string name, float scale, params (string n, float r, float g, float b)[] v)
        {
            var variants = new Variant[v.Length];
            for (int i = 0; i < v.Length; i++) variants[i] = new Variant { name = v[i].n, tint = new Color(v[i].r, v[i].g, v[i].b, 1f) };
            return new Kind { id = id, displayName = name, scale = scale, variants = variants };
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
