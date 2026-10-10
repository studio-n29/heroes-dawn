#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Domain.Activities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Scène 3D d'une activité : construit le niveau généré (primitives de couleur plate selon l'environnement) loin de la scène du jeu,
    /// le filme avec une caméra dédiée vers une texture affichée dans l'interface. Le niveau est en centimètres, la scène en mètres.
    /// </summary>
    public sealed class ActivityStage : IDisposable
    {
        /// <summary>Position du monde de l'activité : très loin au-dessus de la scène du jeu (hors du champ des caméras du jeu).</summary>
        public static readonly Vector3 Origin = new Vector3(0f, 6000f, 0f);

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int Color0 = Shader.PropertyToID("_Color");

        readonly GameObject _root;
        readonly Material _material;
        readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        readonly List<GameObject> _owned = new List<GameObject>();

        public Camera Camera { get; }
        public RenderTexture Texture { get; }
        public Transform Root => _root.transform;
        public ActivityLevel Level { get; }
        /// <summary>Éléments du niveau par rôle (sol, totem, poisson, mur…), pour que les jeux les masquent ou les animent.</summary>
        public IReadOnlyDictionary<string, List<GameObject>> ByRole => _byRole;
        readonly Dictionary<string, List<GameObject>> _byRole = new Dictionary<string, List<GameObject>>();

        public ActivityStage(ActivityLevel level, int width = 1440, int height = 720)
        {
            Level = level;
            _root = new GameObject("ActivityStage") { hideFlags = HideFlags.DontSave };
            _root.transform.position = Origin;
            _material = Resources.Load<Material>("Activities/FlatUnlit") ?? new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
            Texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { name = "ActivityStageTexture", antiAliasing = 2 };
            var camGo = new GameObject("StageCamera");
            camGo.transform.SetParent(_root.transform, false);
            Camera = camGo.AddComponent<Camera>();
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = ToColor(level.Palette.Sky);
            Camera.targetTexture = Texture;
            Camera.nearClipPlane = 0.3f;
            Camera.farClipPlane = 250f;
            Camera.fieldOfView = 45f;
            Camera.depth = -50f;
            Build();
        }

        public static Color ToColor(int rgb) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, 1f);

        void Build()
        {
            foreach (var p in Level.Primitives)
            {
                var go = Spawn(p.Shape, ToColor(p.Color), new Vector3(p.X, p.Y, p.Z) / 100f, new Vector3(p.SizeX, p.SizeY, p.SizeZ) / 100f, true);
                if (!_byRole.TryGetValue(p.Role, out var list)) _byRole[p.Role] = list = new List<GameObject>();
                list.Add(go);
            }
        }

        /// <summary>Crée une primitive plate dans le monde de l'activité (position et taille en mètres ; le Plan fait 10 m de côté par unité d'échelle).</summary>
        public GameObject Spawn(PrimitiveShape shape, Color color, Vector3 position, Vector3 size, bool levelSpace = false)
        {
            var type = shape == PrimitiveShape.Sphere ? PrimitiveType.Sphere : shape == PrimitiveShape.Cylinder ? PrimitiveType.Cylinder
                     : shape == PrimitiveShape.Capsule ? PrimitiveType.Capsule : shape == PrimitiveShape.Plane ? PrimitiveType.Plane : PrimitiveType.Cube;
            var go = GameObject.CreatePrimitive(type);
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            go.transform.SetParent(_root.transform, false);
            go.transform.localPosition = position;
            go.transform.localScale = shape == PrimitiveShape.Plane ? new Vector3(size.x / 10f, 1f, size.z / 10f)
                                    : shape == PrimitiveShape.Cylinder || shape == PrimitiveShape.Capsule ? new Vector3(size.x, size.y / 2f, size.z) : size;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = _material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            SetColor(go, color);
            _owned.Add(go);
            return go;
        }

        public void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            _block.Clear();
            _block.SetColor(BaseColor, color);
            _block.SetColor(Color0, color);
            r.SetPropertyBlock(_block);
        }

        /// <summary>Caméra du dessus penchée : se place à <paramref name="back"/> mètres derrière et <paramref name="height"/> au-dessus de la cible, et la regarde.</summary>
        public void Frame(Vector3 target, float height, float back)
        {
            var t = Camera.transform;
            t.localPosition = target + new Vector3(0f, height, -back);
            t.LookAt(Root.TransformPoint(target));
        }

        public static Vector3 ToWorld(float x, float z, float y = 0f) => new Vector3(x, y, z);

        public void Dispose()
        {
            if (Texture != null) { Texture.Release(); Object.Destroy(Texture); }
            if (_root != null) Object.Destroy(_root);
        }
    }
}
