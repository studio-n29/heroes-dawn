#nullable enable
using System.Collections.Generic;
using System.IO;
using N29.HeroesDawn.Presentation.Characters3D;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Characters3D
{
    /// <summary>
    /// Masque des zones d'un personnage : rouge = vêtements (teinte A), vert = équipement sombre et chaussures (teinte B), noir = peau, mains, visage et fond (jamais teintés).
    /// Les textures de Meshy sont presque monochromes : les zones viennent donc de la géométrie (os dominant de chaque triangle, projeté dans l'atlas UV),
    /// complétées par la luminosité pour l'équipement sombre.
    /// </summary>
    public static class CharacterMaskBuilder
    {
        const byte None = 0, Keep = 1, Gear = 2, Body = 3;

        public static string TexturePath(string name) => CharacterImportDiscovery.Root + "/" + name + "/Textures/Material_0_baseColor.png";
        public static string MaskPath(string name) => CharacterImportDiscovery.Root + "/" + name + "/Textures/mask.png";

        /// <summary>Charge la texture de base (lisible, valeurs brutes) ; null si elle n'existe pas.</summary>
        public static Texture2D? LoadBase(string name)
        {
            string path = TexturePath(name);
            if (!File.Exists(path)) return null;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, true) { hideFlags = HideFlags.HideAndDontSave };
            return tex.LoadImage(File.ReadAllBytes(path)) ? tex : null;
        }

        static byte CategoryOf(string bone, EnemyPalettes.MaskRules r)
        {
            string b = bone.ToLowerInvariant();
            if (b.Contains("hand") || b.Contains("finger") || b.Contains("thumb") || b.Contains("pinky")) return r.keepHands ? Keep : Body;
            if (b.Contains("head") || b.Contains("neck") || b.Contains("jaw") || b.Contains("eye")) return r.keepHead ? Keep : Body;
            if (b.Contains("foot") || b.Contains("toe")) return r.feetAsGear ? Gear : Body;
            return Body;
        }

        /// <summary>Carte des zones (une valeur par pixel de l'atlas) rasterisée depuis le maillage.</summary>
        static byte[] Regions(string name, EnemyPalettes.MaskRules r, int w, int h)
        {
            var data = new byte[w * h];
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterImportDiscovery.Root + "/" + name + "/character.fbx");
            var smr = go != null ? go.GetComponentInChildren<SkinnedMeshRenderer>() : null;
            if (smr == null || smr.sharedMesh == null)
            {
                for (int i = 0; i < data.Length; i++) data[i] = Body;
                return data;
            }
            var mesh = smr.sharedMesh;
            var uv = mesh.uv; var tris = mesh.triangles; var weights = mesh.boneWeights; var bones = smr.bones;
            var cat = new byte[uv.Length];
            for (int i = 0; i < cat.Length; i++)
            {
                int bi = i < weights.Length ? weights[i].boneIndex0 : 0;
                cat[i] = bi >= 0 && bi < bones.Length && bones[bi] != null ? CategoryOf(bones[bi].name, r) : Body;
            }
            for (int t = 0; t + 2 < tris.Length; t += 3)
            {
                int a = tris[t], b = tris[t + 1], c = tris[t + 2];
                byte k = cat[a] == cat[b] || cat[a] == cat[c] ? cat[a] : cat[b] == cat[c] ? cat[b] : cat[a];
                var pa = new Vector2(uv[a].x * w, uv[a].y * h);
                var pb = new Vector2(uv[b].x * w, uv[b].y * h);
                var pc = new Vector2(uv[c].x * w, uv[c].y * h);
                int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(pa.x, Mathf.Min(pb.x, pc.x))) - 1);
                int x1 = Mathf.Min(w - 1, Mathf.CeilToInt(Mathf.Max(pa.x, Mathf.Max(pb.x, pc.x))) + 1);
                int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(pa.y, Mathf.Min(pb.y, pc.y))) - 1);
                int y1 = Mathf.Min(h - 1, Mathf.CeilToInt(Mathf.Max(pa.y, Mathf.Max(pb.y, pc.y))) + 1);
                float den = (pb.y - pc.y) * (pa.x - pc.x) + (pc.x - pb.x) * (pa.y - pc.y);
                if (Mathf.Abs(den) < 1e-6f) continue;
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float px = x + 0.5f, py = y + 0.5f;
                        float l1 = ((pb.y - pc.y) * (px - pc.x) + (pc.x - pb.x) * (py - pc.y)) / den;
                        float l2 = ((pc.y - pa.y) * (px - pc.x) + (pa.x - pc.x) * (py - pc.y)) / den;
                        float l3 = 1f - l1 - l2;
                        const float eps = -0.04f; // marge : évite les fentes entre triangles
                        if (l1 < eps || l2 < eps || l3 < eps) continue;
                        int idx = y * w + x;
                        // La peau l'emporte sur le reste en cas de recouvrement au bord d'un îlot.
                        if (data[idx] == None || data[idx] == Body || k == Keep) data[idx] = k;
                    }
            }
            return data;
        }

        /// <summary>Calcule le masque (en mémoire) d'après les règles ; <paramref name="downscale"/> réduit la taille (aperçu rapide dans l'outil).</summary>
        public static Texture2D Compute(string name, Texture2D source, EnemyPalettes.MaskRules rules, int downscale = 1)
        {
            int w = Mathf.Max(1, source.width / downscale), h = Mathf.Max(1, source.height / downscale);
            var src = source.GetPixels32();
            var region = rules.separateSkin ? Regions(name, rules, w, h) : null;
            var mask = new Texture2D(w, h, TextureFormat.RGBA32, true, true) { hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    var c = src[(y * downscale) * source.width + x * downscale];
                    Color.RGBToHSV(c, out _, out _, out float val);
                    Color32 o;
                    if (val < 0.04f) o = new Color32(0, 0, 0, 255);               // fond de l'atlas
                    else if (region == null) o = new Color32(255, 0, 0, 255);     // modèle sans peau : tout est teinté
                    else
                    {
                        byte k = region[y * w + x];
                        o = k == Keep || k == None ? new Color32(0, 0, 0, 255)
                          : k == Gear || val < rules.darkBelow ? new Color32(0, 255, 0, 255)
                          : new Color32(255, 0, 0, 255);
                    }
                    px[y * w + x] = o;
                }
            mask.SetPixels32(px);
            mask.Apply(true);
            return mask;
        }

        /// <summary>Écrit le masque dans le projet (Textures/mask.png) et le règle comme texture de données (sans sRGB).</summary>
        public static Texture2D? Write(string name, EnemyPalettes.MaskRules rules)
        {
            var baseTex = LoadBase(name);
            if (baseTex == null) return null;
            var mask = Compute(name, baseTex, rules);
            File.WriteAllBytes(MaskPath(name), mask.EncodeToPNG());
            Object.DestroyImmediate(mask);
            Object.DestroyImmediate(baseTex);
            AssetDatabase.ImportAsset(MaskPath(name), ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(MaskPath(name));
            imp.sRGBTexture = false;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = true;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(MaskPath(name));
        }
    }
}
