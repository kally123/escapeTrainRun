using UnityEngine;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Audio
{
    /// <summary>
    /// Plays a sound effect at a 3D position in the world.
    /// Attach to collectibles, obstacles, or other world objects.
    /// </summary>
    public class SpatialAudioSource : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private float volume = 1f;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private bool playOnStart;
        [SerializeField] private bool loop;

        [Header("Random Pitch")]
        [SerializeField] private bool randomizePitch;
        [SerializeField] private float minPitch = 0.9f;
        [SerializeField] private float maxPitch = 1.1f;

        private AudioSource audioSource;

        private void Awake()
        {
            SetupAudioSource();
        }

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        private void SetupAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.spatialBlend = 1f; // Full 3D
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.playOnAwake = false;
            audioSource.loop = loop;
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        public void Play()
        {
            if (audioSource == null || audioClip == null) return;

            if (randomizePitch)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }

            // Apply global volume
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audioSource.volume = volume * audio.SFXVolume * audio.MasterVolume;
            }

            audioSource.Play();
        }

        /// <summary>
        /// Plays a one-shot audio clip.
        /// </summary>
        public void PlayOneShot()
        {
            if (audioClip == null) return;

            float finalVolume = volume;
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                finalVolume *= audio.SFXVolume * audio.MasterVolume;
            }

            if (randomizePitch && audioSource != null)
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }

            audioSource?.PlayOneShot(audioClip, finalVolume);
        }

        /// <summary>
        /// Plays a specific clip at this position.
        /// </summary>
        public void PlayClip(AudioClip clip)
        {
            if (clip == null) return;

            float finalVolume = volume;
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                finalVolume *= audio.SFXVolume * audio.MasterVolume;
            }

            audioSource?.PlayOneShot(clip, finalVolume);
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public void Stop()
        {
            audioSource?.Stop();
        }

        /// <summary>
        /// Sets the audio clip.
        /// </summary>
        public void SetClip(AudioClip clip)
        {
            audioClip = clip;
            if (audioSource != null)
            {
                audioSource.clip = clip;
            }
        }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
        }
    }
}
