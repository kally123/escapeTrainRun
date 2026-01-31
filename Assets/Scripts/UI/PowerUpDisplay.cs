using UnityEngine;
using UnityEngine.UI;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Collectibles
{
    /// <summary>
    /// UI panel that displays all active power-ups.
    /// </summary>
    public class PowerUpDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform indicatorContainer;
        [SerializeField] private GameObject indicatorPrefab;

        [Header("Layout")]
        [SerializeField] private float indicatorSpacing = 10f;
        [SerializeField] private bool horizontalLayout = true;

        private PowerUpManager powerUpManager;

        private void Start()
        {
            powerUpManager = PowerUpManager.Instance;

            if (powerUpManager != null)
            {
                powerUpManager.OnPowerUpTimeUpdated += OnPowerUpTimeUpdated;
            }
        }

        private void OnDestroy()
        {
            if (powerUpManager != null)
            {
                powerUpManager.OnPowerUpTimeUpdated -= OnPowerUpTimeUpdated;
            }
        }

        private void OnPowerUpTimeUpdated(PowerUpType type, float normalizedTime)
        {
            // Update specific indicator if needed
            // This could be used for additional UI effects
        }

        /// <summary>
        /// Shows a notification when a power-up is collected.
        /// </summary>
        public void ShowPowerUpNotification(PowerUpType type)
        {
            // Could show a popup notification
            Debug.Log($"[PowerUpDisplay] Power-up collected: {type}");
        }

        /// <summary>
        /// Gets the name of a power-up for display.
        /// </summary>
        public static string GetPowerUpName(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.Magnet => "Coin Magnet",
                PowerUpType.Shield => "Shield",
                PowerUpType.SpeedBoost => "Speed Boost",
                PowerUpType.StarPower => "Star Power",
                PowerUpType.Multiplier => "2x Multiplier",
                PowerUpType.MysteryBox => "Mystery Box",
                _ => "Power-Up"
            };
        }

        /// <summary>
        /// Gets the description of a power-up for display.
        /// </summary>
        public static string GetPowerUpDescription(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.Magnet => "Attracts nearby coins!",
                PowerUpType.Shield => "Blocks one obstacle hit!",
                PowerUpType.SpeedBoost => "Super speed + invincibility!",
                PowerUpType.StarPower => "Fly above everything!",
                PowerUpType.Multiplier => "Double all score gains!",
                PowerUpType.MysteryBox => "Random power-up!",
                _ => ""
            };
        }
    }
}
