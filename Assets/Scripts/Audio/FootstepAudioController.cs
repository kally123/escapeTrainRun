using UnityEngine;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Audio
{
    /// <summary>
    /// Handles footstep sounds based on player movement and surface type.
    /// </summary>
    public class FootstepAudioController : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource footstepSource;

        [Header("Footstep Sounds")]
        [SerializeField] private AudioClip[] defaultFootsteps;
        [SerializeField] private AudioClip[] metalFootsteps;
        [SerializeField] private AudioClip[] woodFootsteps;
        [SerializeField] private AudioClip[] concreteFootsteps;
        [SerializeField] private AudioClip[] grassFootsteps;

        [Header("Settings")]
        [SerializeField] private float baseStepInterval = 0.3f;
        [SerializeField] private float minStepInterval = 0.15f;
        [SerializeField] private float baseVolume = 0.5f;
        [SerializeField] private float pitchVariation = 0.1f;

        [Header("Speed Scaling")]
        [SerializeField] private float minSpeed = 5f;
        [SerializeField] private float maxSpeed = 25f;

        // State
        private SurfaceType currentSurface = SurfaceType.Default;
        private float stepTimer;
        private float currentInterval;
        private bool isMoving;

        private void Start()
        {
            if (footstepSource == null)
            {
                footstepSource = GetComponent<AudioSource>();
            }

            currentInterval = baseStepInterval;
        }

        private void Update()
        {
            if (!isMoving) return;

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = currentInterval;
            }
        }

        /// <summary>
        /// Updates footstep timing based on player speed.
        /// </summary>
        public void UpdateSpeed(float speed)
        {
            isMoving = speed > 0.1f;

            if (isMoving)
            {
                float normalizedSpeed = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
                currentInterval = Mathf.Lerp(baseStepInterval, minStepInterval, normalizedSpeed);
            }
        }

        /// <summary>
        /// Sets the surface type for footstep sounds.
        /// </summary>
        public void SetSurface(SurfaceType surface)
        {
            currentSurface = surface;
        }

        /// <summary>
        /// Plays a footstep sound.
        /// </summary>
        public void PlayFootstep()
        {
            AudioClip[] clips = GetClipsForSurface(currentSurface);
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;

            float volume = baseVolume;
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                volume *= audio.SFXVolume * audio.MasterVolume;
            }

            if (footstepSource != null)
            {
                footstepSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                footstepSource.PlayOneShot(clip, volume);
            }
        }

        /// <summary>
        /// Plays a landing sound.
        /// </summary>
        public void PlayLanding()
        {
            AudioClip[] clips = GetClipsForSurface(currentSurface);
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;

            float volume = baseVolume * 1.5f; // Louder for landing
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                volume *= audio.SFXVolume * audio.MasterVolume;
            }

            if (footstepSource != null)
            {
                footstepSource.pitch = 0.9f; // Lower pitch for impact
                footstepSource.PlayOneShot(clip, volume);
            }
        }

        private AudioClip[] GetClipsForSurface(SurfaceType surface)
        {
            return surface switch
            {
                SurfaceType.Metal => metalFootsteps?.Length > 0 ? metalFootsteps : defaultFootsteps,
                SurfaceType.Wood => woodFootsteps?.Length > 0 ? woodFootsteps : defaultFootsteps,
                SurfaceType.Concrete => concreteFootsteps?.Length > 0 ? concreteFootsteps : defaultFootsteps,
                SurfaceType.Grass => grassFootsteps?.Length > 0 ? grassFootsteps : defaultFootsteps,
                _ => defaultFootsteps
            };
        }

        /// <summary>
        /// Stops footstep sounds.
        /// </summary>
        public void StopFootsteps()
        {
            isMoving = false;
        }

        /// <summary>
        /// Starts footstep sounds.
        /// </summary>
        public void StartFootsteps()
        {
            isMoving = true;
            stepTimer = 0f;
        }
    }

    /// <summary>
    /// Surface types for footstep sounds.
    /// </summary>
    public enum SurfaceType
    {
        Default,
        Metal,
        Wood,
        Concrete,
        Grass
    }
}
