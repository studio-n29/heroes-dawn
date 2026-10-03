using System.Linq;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class SceneGeneratorTests
    {
        const string Dir = "Assets/_Project/Scenes/";

        static T WithScene<T>(string name, System.Func<Scene, T> read)
        {
            var scene = EditorSceneManager.OpenScene(Dir + name + ".unity", OpenSceneMode.Additive);
            try { return read(scene); }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        static TC Find<TC>(Scene s) where TC : Component
            => s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TC>(true)).FirstOrDefault();

        [Test, Description("Vérifie que les Build Settings listent Boot, Hub, Board, Combat dans cet ordre — parce que Boot doit être la scène de démarrage (index 0) et les autres chargeables par nom")]
        public void BuildSettings_ListBootHubBoardCombat_InOrder()
        {
            var names = EditorBuildSettings.scenes.Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path)).ToArray();
            Assert.That(names, Is.EqualTo(new[] { "Boot", "Hub", "Board", "Combat" }));
            Assert.That(EditorBuildSettings.scenes.All(s => s.enabled));
        }

        [Test, Description("Vérifie que les scopes du QG et du plateau ont la racine pour parent et que le combat a le plateau pour parent — parce que le combat hérite des services du plateau (doc 07)")]
        public void HubAndBoardScopes_HaveRootParent_CombatScope_HasBoardParent()
        {
            Assert.That(WithScene("Hub", s => Find<HubLifetimeScope>(s).parentReference.TypeName), Is.EqualTo(typeof(RootLifetimeScope).FullName));
            Assert.That(WithScene("Board", s => Find<BoardLifetimeScope>(s).parentReference.TypeName), Is.EqualTo(typeof(RootLifetimeScope).FullName));
            Assert.That(WithScene("Combat", s => Find<CombatLifetimeScope>(s).parentReference.TypeName), Is.EqualTo(typeof(BoardLifetimeScope).FullName));
        }

        [Test, Description("Vérifie l'ordre d'affichage des canvas de scène (QG 10, plateau 10, combat 20) et que chaque scope vise sa racine d'écran — parce que le combat doit couvrir le plateau resté chargé")]
        public void SceneCanvases_SortingOrder_Hub10_Board10_Combat20()
        {
            Assert.That(WithScene("Hub", s => Find<Canvas>(s).sortingOrder), Is.EqualTo(10));
            Assert.That(WithScene("Board", s => Find<Canvas>(s).sortingOrder), Is.EqualTo(10));
            Assert.That(WithScene("Combat", s => Find<Canvas>(s).sortingOrder), Is.EqualTo(20));
            Assert.That(WithScene("Hub", s => Find<HubLifetimeScope>(s).ScreenRoot != null), Is.True);
        }

        [Test, Description("Vérifie que la scène temporaire du Test Runner est reconnue et pas les scènes du jeu — parce que Play démarre par Boot sauf pendant les tests, sinon les tests PlayMode ne démarrent jamais (Review Focus 4)")]
        public void BootSceneStarter_SkipsOnlyTestRunnerScene()
        {
            Assert.That(N29.HeroesDawn.EditorTools.Scenes.BootSceneStarter.IsTestRunnerScene("InitTestScene638952"), Is.True);
            Assert.That(N29.HeroesDawn.EditorTools.Scenes.BootSceneStarter.IsTestRunnerScene("Hub"), Is.False);
            Assert.That(N29.HeroesDawn.EditorTools.Scenes.BootSceneStarter.IsTestRunnerScene("Boot"), Is.False);
        }

        [Test, Description("Vérifie que Boot contient le point d'entrée, le scope racine, l'interface commune et une caméra — parce que c'est la seule scène chargée au lancement")]
        public void BootScene_HasGameBootstrapRootScopeAndUiRoot()
        {
            Assert.That(WithScene("Boot", s => Find<GameBootstrap>(s) != null && Find<RootLifetimeScope>(s) != null), Is.True);
            Assert.That(WithScene("Boot", s => Find<UiRoot>(s) != null), Is.True);
            Assert.That(WithScene("Boot", s => Find<Camera>(s) != null), Is.True);
        }
    }
}
