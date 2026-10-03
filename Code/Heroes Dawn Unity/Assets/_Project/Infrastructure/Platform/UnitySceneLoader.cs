#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace N29.HeroesDawn.Infrastructure.Platform
{
    /// <summary>
    /// Chargement des scènes de jeu en additif. Masquer une scène désactive ses objets racines visuels (Canvas),
    /// ce qui met l'écran en pause ; son scope VContainer (objet sans Canvas) reste actif.
    /// </summary>
    public sealed class UnitySceneLoader : ISceneLoader
    {
        public async UniTask LoadAsync(string scene, CancellationToken ct)
        {
            var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            if (op == null) throw new InvalidOperationException("Scène introuvable dans les Build Settings : " + scene);
            await op.ToUniTask(cancellationToken: ct);
        }

        public async UniTask UnloadAsync(string scene, CancellationToken ct)
        {
            var s = SceneManager.GetSceneByName(scene);
            if (!s.isLoaded) return;
            var op = SceneManager.UnloadSceneAsync(s);
            if (op == null) throw new InvalidOperationException("Déchargement impossible : " + scene);
            await op.ToUniTask(cancellationToken: ct);
        }

        public void SetVisible(string scene, bool visible)
        {
            var s = SceneManager.GetSceneByName(scene);
            if (!s.isLoaded) throw new InvalidOperationException("Scène non chargée : " + scene);
            foreach (var go in s.GetRootGameObjects())
                if (go.GetComponent<Canvas>() != null) go.SetActive(visible);
        }
    }
}
