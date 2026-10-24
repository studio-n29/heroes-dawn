#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using UnityEngine;
using UnityEngine.Networking;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Envoie les réponses au formulaire Google Forms configuré (POST sur formResponse, sans compte). Échec silencieux : le rapport local reste.</summary>
    public sealed class PlaytestFormSender
    {
        readonly PlaytestFormConfig _config;

        public PlaytestFormSender()
        {
            var asset = Resources.Load<TextAsset>("Playtest/PlaytestForm");
            _config = PlaytestFormConfig.Parse(asset != null ? asset.text : "");
        }

        public bool Enabled => _config.Enabled;

        /// <summary>Renvoie vrai si Google a accepté l'envoi.</summary>
        public async UniTask<bool> SendAsync(PlaytestAnswers answers, string version, CancellationToken ct)
        {
            if (!_config.Enabled) return false;
            var form = new WWWForm();
            foreach (var f in _config.Fields(answers, version)) form.AddField(f.Key, f.Value);
            try
            {
                using var req = UnityWebRequest.Post(_config.Url, form);
                req.timeout = 15;
                await req.SendWebRequest().WithCancellation(ct);
                return req.result == UnityWebRequest.Result.Success;
            }
            catch (System.OperationCanceledException) { throw; }
            catch (System.Exception) { return false; }
        }
    }
}
