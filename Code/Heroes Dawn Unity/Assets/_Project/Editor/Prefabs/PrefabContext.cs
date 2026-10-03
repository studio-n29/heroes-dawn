#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs
{
    /// <summary>Outils offerts aux recettes : prefabs déjà générés (imbrication), branchement des champs sérialisés.</summary>
    public sealed class PrefabContext
    {
        public string Root { get; }

        public PrefabContext(string root) => Root = root;

        /// <summary>Prefab déjà généré sous la racine (une recette d'ordre inférieur l'a produit).</summary>
        public GameObject Asset(string relativePath)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + relativePath);
            if (go == null) throw new InvalidOperationException("Prefab requis non généré : " + relativePath);
            return go;
        }

        /// <summary>Instance imbriquée d'un prefab déjà généré (une retouche du widget se propage).</summary>
        public GameObject Nest(string relativePath, Transform parent, string? name = null)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(Asset(relativePath), parent);
            if (name != null) go.name = name;
            return go;
        }

        /// <summary>Instance imbriquée, renvoyée par son composant principal.</summary>
        public T Nest<T>(string relativePath, Transform parent, string? name = null) where T : Component
            => Nest(relativePath, parent, name).GetComponent<T>();

        /// <summary>Branche un champ sérialisé (privé compris) d'un composant.</summary>
        public void Wire(Component target, string field, UnityEngine.Object? value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field) ?? throw new InvalidOperationException($"Champ sérialisé introuvable : {target.GetType().Name}.{field}");
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Branche un tableau sérialisé de références.</summary>
        public void WireArray(Component target, string field, IReadOnlyList<UnityEngine.Object> values)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field) ?? throw new InvalidOperationException($"Champ sérialisé introuvable : {target.GetType().Name}.{field}");
            p.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Racine d'interface plein cadre (RectTransform étiré, couche UI).</summary>
        public static RectTransform NewRoot(string name, float width = 0, float height = 0)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = 5 };
            var rt = (RectTransform)go.transform;
            if (width > 0 && height > 0) rt.sizeDelta = new Vector2(width, height);
            else rt.Stretch();
            return rt;
        }
    }
}
