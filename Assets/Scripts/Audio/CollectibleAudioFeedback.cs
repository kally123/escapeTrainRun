using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Audio
{
    /// <summary>
    /// Provides audio feedback for collectibles.
    /// Handles coin, power-up, and special item sounds with 3D positioning.
    /// </summary>
    public class CollectibleAudioFeedback : MonoBehaviour
    {
        [Header("Coin Sounds")]
        [SerializeField] private AudioClip[] coinSounds;
        [SerializeField] private AudioClip coinComboSound;
        [SerializeField] private AudioClip superCoinSound;

        [Header("Power-Up Sounds")]
        [SerializeField] private AudioClip powerUpAppearSound;
        [SerializeField] private AudioClip powerUpCollectSound;
        [SerializeField] private AudioClip mysteryBoxSound;

        [Header("Combo Settings")]
        [SerializeField] private float comboPitchIncrease = 0.05f;
        [SerializeField] private float maxComboPitch = 1.5f;
        [SerializeField] private float comboResetTime = 1f;
        [SerializeField] private int comboSoundThreshold = 5;

        [Header("3D Audio Settings")]
        [SerializeField] private bool use3DAudio = true;
        [SerializeField] private float maxAudioDistance = 30f;

        // State
        private int coinCombo;
        private float comboPitch = 1f;
        private float comboTimer;

        private void OnEnable()
        {
            GameEvents.OnCoinsCollected += OnCoinsCollected;
        }

        private void OnDisable()
        {
            GameEvents.OnCoinsCollected -= OnCoinsCollected;
        }

        private void Update()
        {
            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }

        private void OnCoinsCollected(int amount)
        {
            coinCombo += amount;
            comboTimer = comboResetTime;

            // Increase pitch with combo
            comboPitch = Mathf.Min(1f + (coinCombo * comboPitchIncrease), maxComboPitch);

            // Play combo sound at threshold
            if (coinCombo >= comboSoundThreshold && coinCombo % comboSoundThreshold == 0)
            {
                PlayComboSound();
            }
        }

        private void ResetCombo()
        {
            coinCombo = 0;
            comboPitch = 1f;
        }

        /// <summary>
        /// Plays a coin collection sound at the specified position.
        /// </summary>
        public void PlayCoinSound(Vector3 position, int coinValue = 1)
        {
            if (coinSounds == null || coinSounds.Length == 0) return;

            AudioClip clip = coinValue > 1 ? superCoinSound : coinSounds[Random.Range(0, coinSounds.Length)];
            if (clip == null) return;

            if (use3DAudio && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySpatialSFX(clip, position, 1f);
            }
            else
            {
                PlayWithPitch(clip, comboPitch);
            }
        }

        /// <summary>
        /// Plays a power-up appear sound.
        /// </summary>
        public void PlayPowerUpAppear(Vector3 position)
        {
            if (powerUpAppearSound == null) return;

            if (use3DAudio && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySpatialSFX(powerUpAppearSound, position, 0.7f);
            }
            else if (ServiceLocator.TryGet<AudioManager>(out var audio2))
            {
                audio2.PlaySFX(powerUpAppearSound);
            }
        }

        /// <summary>
        /// Plays a power-up collection sound.
        /// </summary>
        public void PlayPowerUpCollect(Vector3 position)
        {
            if (powerUpCollectSound == null) return;

            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(powerUpCollectSound);
            }
        }

        /// <summary>
        /// Plays the mystery box sound.
        /// </summary>
        public void PlayMysteryBox(Vector3 position)
        {
            if (mysteryBoxSound == null) return;

            if (use3DAudio && ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySpatialSFX(mysteryBoxSound, position);
            }
            else if (ServiceLocator.TryGet<AudioManager>(out var audio2))
            {
                audio2.PlaySFX(mysteryBoxSound);
            }
        }

        private void PlayComboSound()
        {
            if (coinComboSound == null) return;

            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFX(coinComboSound);
            }
        }

        private void PlayWithPitch(AudioClip clip, float pitch)
        {
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlaySFXWithPitch(clip, pitch);
            }
        }

        /// <summary>
        /// Gets the current combo count.
        /// </summary>
        public int GetComboCount() => coinCombo;

        /// <summary>
        /// Gets the current combo pitch.
        /// </summary>
        public float GetComboPitch() => comboPitch;
    }
}
