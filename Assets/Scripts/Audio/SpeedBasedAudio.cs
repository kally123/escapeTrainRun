using UnityEngine;
using EscapeTrainRun.Core;

namespace EscapeTrainRun.Audio
{
    /// <summary>
    /// Plays sounds based on player speed and movement.
    /// Adjusts pitch and volume dynamically.
    /// </summary>
    public class SpeedBasedAudio : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip movementSound;

        [Header("Speed Settings")]
        [SerializeField] private float minSpeed = 5f;
        [SerializeField] private float maxSpeed = 30f;
        [SerializeField] private float minPitch = 0.8f;
        [SerializeField] private float maxPitch = 1.5f;
        [SerializeField] private float minVolume = 0.3f;
        [SerializeField] private float maxVolume = 1f;

        [Header("Smoothing")]
        [SerializeField] private float pitchSmoothSpeed = 5f;
        [SerializeField] private float volumeSmoothSpeed = 5f;

        // State
        private float targetPitch = 1f;
        private float targetVolume = 0.5f;
        private float currentSpeed;

        private void Start()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource != null && movementSound != null)
            {
                audioSource.clip = movementSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        private void Update()
        {
            if (audioSource == null) return;

            // Smooth pitch and volume
            audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, pitchSmoothSpeed * Time.deltaTime);
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, volumeSmoothSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Updates audio based on current speed.
        /// </summary>
        public void UpdateSpeed(float speed)
        {
            currentSpeed = speed;

            // Normalize speed to 0-1 range
            float normalizedSpeed = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

            // Calculate target pitch and volume
            targetPitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);
            targetVolume = Mathf.Lerp(minVolume, maxVolume, normalizedSpeed);

            // Apply global volume
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                targetVolume *= audio.SFXVolume * audio.MasterVolume;
            }
        }

        /// <summary>
        /// Pauses the movement sound.
        /// </summary>
        public void Pause()
        {
            if (audioSource != null)
            {
                audioSource.Pause();
            }
        }

        /// <summary>
        /// Resumes the movement sound.
        /// </summary>
        public void Resume()
        {
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }

        /// <summary>
        /// Stops the movement sound.
        /// </summary>
        public void Stop()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }
}
