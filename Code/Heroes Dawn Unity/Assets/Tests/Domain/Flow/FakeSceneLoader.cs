#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Tests.Domain.Flow
{
    /// <summary>Chargeur de scènes factice : enregistre les appels (« load:Hub », « hide:Board »…) dans un journal partagé.</summary>
    public sealed class FakeSceneLoader : ISceneLoader
    {
        readonly List<string> _calls;
        public string? FailOnLoad;
        /// <summary>Si renseigné, le prochain LoadAsync attend que ce signal soit déclenché.</summary>
        public UniTaskCompletionSource? Gate;

        public FakeSceneLoader(List<string> calls) { _calls = calls; }

        public async UniTask LoadAsync(string scene, CancellationToken ct)
        {
            _calls.Add("load:" + scene);
            if (Gate != null)
            {
                var g = Gate;
                Gate = null;
                await g.Task;
            }
            if (FailOnLoad == scene)
            {
                FailOnLoad = null;
                throw new InvalidOperationException("Chargement impossible : " + scene);
            }
        }

        public UniTask UnloadAsync(string scene, CancellationToken ct)
        {
            _calls.Add("unload:" + scene);
            return UniTask.CompletedTask;
        }

        public void SetVisible(string scene, bool visible) => _calls.Add((visible ? "show:" : "hide:") + scene);
    }

    public sealed class FakeScreens : IScreenVisibility
    {
        readonly List<string> _calls;
        public FakeScreens(List<string> calls) { _calls = calls; }
        public void SetLoginVisible(bool visible) => _calls.Add(visible ? "login:on" : "login:off");
    }
}
