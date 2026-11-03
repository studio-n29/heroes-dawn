#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using VContainer.Unity;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>Joue les voix depuis Resources/Voices, normalise chaque clip à <see cref="VoiceLevels.TargetPeak"/> et baisse la musique pendant la réplique.</summary>
    public sealed class VoicePlayer : IVoicePlayer, ITickable
    {
        readonly AudioRig _rig;
        readonly IAudioService _audio;
        readonly PlayerSettings _settings;
        readonly Dictionary<string, AudioClip?> _clips = new Dictionary<string, AudioClip?>();
        readonly Dictionary<AudioClip, float> _gains = new Dictionary<AudioClip, float>();
        bool _active;

        public VoicePlayer(AudioRig rig, IAudioService audio, PlayerSettings settings)
        {
            _rig = rig; _audio = audio; _settings = settings;
        }

        AudioClip? Load(string lineKey)
        {
            string path = VoiceLevels.ResourcePath(Loc.Language, lineKey);
            if (!_clips.TryGetValue(path, out var clip)) _clips[path] = clip = Resources.Load<AudioClip>(path);
            return clip;
        }

        public bool Has(string lineKey) => lineKey.Length > 0 && Load(lineKey) != null;

        public void Play(string lineKey)
        {
            Stop();
            var clip = lineKey.Length == 0 ? null : Load(lineKey);
            if (clip == null) return;
            var s = _settings.Current;
            _rig.Speech.clip = clip;
            _rig.Speech.volume = Mathf.Clamp01(Mathf.Clamp01(s.master / 100f) * Mathf.Clamp01(s.voice / 100f) * GainOf(clip));
            _rig.Speech.Play();
            _audio.Duck(DuckLayer.Voice, VoiceLevels.MusicDuck, VoiceLevels.DuckAttack);
            _active = true;
        }

        public void Stop()
        {
            if (_rig.Speech.isPlaying) _rig.Speech.Stop();
            if (!_active) return;
            _active = false;
            _audio.Duck(DuckLayer.Voice, 1f, VoiceLevels.DuckRelease);
        }

        public void Tick()
        {
            if (_active && !_rig.Speech.isPlaying) Stop();
        }

        float GainOf(AudioClip clip)
        {
            if (_gains.TryGetValue(clip, out float g)) return g;
            float peak = 0f;
            try
            {
                var data = new float[clip.samples * clip.channels];
                if (clip.GetData(data, 0)) foreach (var v in data) peak = Mathf.Max(peak, Mathf.Abs(v));
            }
            catch (System.Exception) { peak = 0f; } // clip non décompressé : gain neutre
            return _gains[clip] = VoiceLevels.GainFor(peak);
        }
    }
}
