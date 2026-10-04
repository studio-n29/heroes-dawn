using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using N29.HeroesDawn.EditorTools.Prefabs;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Garde-fous sur les prefabs du jeu (générés puis retouchés à la main).</summary>
    public sealed class PrefabStructureTests
    {
        static IEnumerable<(string Path, GameObject Root)> Prefabs()
            => AssetDatabase.FindAssets("t:Prefab", new[] { PrefabGenerator.DefaultRoot })
                            .Select(AssetDatabase.GUIDToAssetPath)
                            .Select(p => (p, AssetDatabase.LoadAssetAtPath<GameObject>(p)));

        [Test, Description("Vérifie que chaque référence sérialisée des vues du jeu est branchée dans les prefabs — parce qu'une retouche manuelle peut casser un lien, et une vue sans sa référence plante à l'exécution (spec §9.2)")]
        public void EveryViewSerializedReferenceIsWired()
        {
            var missing = new List<string>();
            foreach (var (path, root) in Prefabs())
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) { missing.Add(path + " : script manquant"); continue; }
                var type = mb.GetType();
                if (type.Namespace == null || !type.Namespace.StartsWith("N29.HeroesDawn.Presentation")) continue;
                for (var t = type; t != null && t != typeof(MonoBehaviour); t = t.BaseType)
                foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (f.DeclaringType?.Namespace == null || !f.DeclaringType.Namespace.StartsWith("N29.HeroesDawn")) continue; // champs hérités d Unity (Graphic.m_Material) : hors périmètre
                    if (f.GetCustomAttribute<SerializeField>() == null && !f.IsPublic) continue;
                    if (f.GetCustomAttribute<OptionalRefAttribute>() != null) continue;
                    var value = f.GetValue(mb);
                    if (typeof(Object).IsAssignableFrom(f.FieldType) && (value as Object) == null)
                        missing.Add($"{path} : {type.Name}.{f.Name}");
                    else if (value is Object[] arr && arr.Any(o => o == null))
                        missing.Add($"{path} : {type.Name}.{f.Name}[]");
                }
            }
            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Test, Description("Vérifie qu'aucun prefab n'utilise l'ancien texte uGUI — parce que tout le texte passe en TextMeshPro (spec §5)")]
        public void NoLegacyTextComponent()
        {
            var legacy = Prefabs().Where(p => p.Root.GetComponentsInChildren<UnityEngine.UI.Text>(true).Length > 0).Select(p => p.Path).ToList();
            Assert.That(legacy, Is.Empty, string.Join("\n", legacy));
        }

        [Test, Description("Vérifie qu'aucun texte des prefabs ne descend sous 20 px (taille ou taille minimale d'ajustement) — parce que les maquettes imposent 20 px minimum en 1080p (maquettes §3)")]
        public void NoTmpTextUnder20px()
        {
            var small = new List<string>();
            foreach (var (path, root) in Prefabs())
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                float min = t.enableAutoSizing ? t.fontSizeMin : t.fontSize;
                if (min < 20f) small.Add($"{path} : {t.name} ({min})");
            }
            Assert.That(small, Is.Empty, string.Join("\n", small));
        }

        [Test, Description("Vérifie que le cercle de chargement de la connexion tourne autour de son centre (pivot 0,5 / 0,5) — parce qu'un pivot en bas le faisait tourner autour d'un point du bas au lieu de sur lui-même (retour du test sur téléphone)")]
        public void LoginSpinner_PivotIsCentered()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabGenerator.DefaultRoot + "/Screens/Login.prefab");
            var view = prefab.GetComponent<N29.HeroesDawn.Presentation.Login.LoginView>();
            var spinner = (RectTransform)new SerializedObject(view).FindProperty("spinner").objectReferenceValue;
            Assert.That(spinner.pivot, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        }

        [Test, Description("Vérifie que chaque clé de texte statique des prefabs existe dans la localisation — parce qu'une clé inconnue s'afficherait « [clé] » à l'écran")]
        public void EveryLocalizedTextKeyExists()
        {
            var unknown = new List<string>();
            foreach (var (path, root) in Prefabs())
            foreach (var l in root.GetComponentsInChildren<LocalizedText>(true))
                if (string.IsNullOrEmpty(l.Key) || !Loc.Has(l.Key)) unknown.Add($"{path} : {l.name} « {l.Key} »");
            Assert.That(unknown, Is.Empty, string.Join("\n", unknown));
        }
    }
}
