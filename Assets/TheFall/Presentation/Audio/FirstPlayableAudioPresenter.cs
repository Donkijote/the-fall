using System.Collections.Generic;
using TheFall.Presentation.Animation;
using UnityEngine;

namespace TheFall.Presentation.Audio
{
    /// <summary>
    /// Plays short, non-looping prototype effects for already-resolved presentation beats.
    /// There is no delayed scheduling: stopping the presenter cancels every active cue immediately.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class FirstPlayableAudioPresenter : MonoBehaviour
    {
        private readonly Dictionary<PrototypeAudioCueKind, AudioClip> _clips =
            new Dictionary<PrototypeAudioCueKind, AudioClip>();
        private readonly List<PrototypeAudioCueKind> _cueHistory =
            new List<PrototypeAudioCueKind>();
        private AudioSource _effectsSource;
        private PrototypeAudioCueKind? _activeCue;

        public IReadOnlyList<PrototypeAudioCueKind> CueHistory => _cueHistory;

        public bool MasterEnabled { get; private set; } = true;

        public bool EffectsEnabled { get; private set; } = true;

        public bool MusicEnabled { get; private set; }

        public bool FastForward { get; private set; }

        public bool EffectsAudible => MasterEnabled && EffectsEnabled;

        public int PlayedCueCount { get; private set; }

        public int StopCount { get; private set; }

        public PrototypeAudioCueKind? ActiveCue => _activeCue;

        public bool IsPlaying => _effectsSource != null && _effectsSource.isPlaying;

        private void OnEnable()
        {
            if (UnityEngine.Application.isPlaying)
            {
                EnsureInitialized();
            }
        }

        private void Update()
        {
            if (_activeCue.HasValue && (_effectsSource == null || !_effectsSource.isPlaying))
            {
                _activeCue = null;
            }
        }

        private void OnDisable()
        {
            StopAll();
            DestroyGeneratedClips();
        }

        public void BeginSession()
        {
            StopAll();
            _cueHistory.Clear();
            PlayedCueCount = 0;
        }

        public void Present(ResolvedAnimationStep step)
        {
            if (step == null || !PrototypeAudioCueLibrary.TryResolve(step.Kind, out var cueKind))
            {
                return;
            }

            _cueHistory.Add(cueKind);
            if (!EffectsAudible)
            {
                return;
            }

            EnsureInitialized();
            _effectsSource.Stop();
            _effectsSource.clip = _clips[cueKind];
            _effectsSource.pitch = FastForward ? 1.12f : 1f;
            _effectsSource.Play();
            _activeCue = cueKind;
            PlayedCueCount++;
        }

        public void SetMasterEnabled(bool enabled)
        {
            MasterEnabled = enabled;
            if (!EffectsAudible)
            {
                StopAll();
            }
        }

        public void SetEffectsEnabled(bool enabled)
        {
            EffectsEnabled = enabled;
            if (!EffectsAudible)
            {
                StopAll();
            }
        }

        public void SetMusicEnabled(bool enabled)
        {
            MusicEnabled = enabled;
        }

        public void SetFastForward(bool enabled)
        {
            if (FastForward == enabled)
            {
                return;
            }

            FastForward = enabled;
            StopAll();
        }

        public void StopAll()
        {
            if (_effectsSource != null)
            {
                _effectsSource.Stop();
                _effectsSource.clip = null;
            }

            _activeCue = null;
            StopCount++;
        }

        private void EnsureInitialized()
        {
            _effectsSource ??= GetComponent<AudioSource>();
            _effectsSource.playOnAwake = false;
            _effectsSource.loop = false;
            _effectsSource.spatialBlend = 0f;
            _effectsSource.volume = 1f;

            if (_clips.Count > 0)
            {
                return;
            }

            foreach (var definition in PrototypeAudioCueLibrary.Definitions)
            {
                _clips.Add(definition.Kind, PrototypeAudioCueLibrary.CreateClip(definition.Kind));
            }
        }

        private void DestroyGeneratedClips()
        {
            foreach (var clip in _clips.Values)
            {
                if (clip != null)
                {
                    Destroy(clip);
                }
            }

            _clips.Clear();
        }
    }
}
