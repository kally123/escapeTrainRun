using UnityEngine;
using EscapeTrainRun.Core;
using EscapeTrainRun.Events;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// Detects near misses with obstacles for bonus points.
    /// </summary>
    public class NearMissDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float nearMissDistance = 0.8f;
        [SerializeField] private float perfectMissDistance = 0.3f;
        [SerializeField] private float detectionCooldown = 0.5f;

        [Header("Scoring")]
        [SerializeField] private int nearMissPoints = 25;
        [SerializeField] private int perfectMissPoints = 50;

        [Header("Combo")]
        [SerializeField] private int comboMultiplierMax = 5;
        [SerializeField] private float comboResetTime = 2f;

        [Header("Effects")]
        [SerializeField] private GameObject nearMissVFX;
        [SerializeField] private GameObject perfectMissVFX;

        // State
        private int currentCombo;
        private float lastNearMissTime;
        private float lastComboTime;

        public int CurrentCombo => currentCombo;

        private void Update()
        {
            // Reset combo if too much time has passed
            if (currentCombo > 0 && Time.time - lastComboTime > comboResetTime)
            {
                currentCombo = 0;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckNearMiss(other);
        }

        private void OnTriggerExit(Collider other)
        {
            // Could also detect on exit for more accurate measurement
        }

        private void CheckNearMiss(Collider other)
        {
            // Check if this is an obstacle
            var obstacle = other.GetComponent<Obstacle>();
            if (obstacle == null) return;

            // Check cooldown
            if (Time.time - lastNearMissTime < detectionCooldown) return;

            // Calculate distance to obstacle
            float distance = GetClosestDistance(other);

            if (distance <= perfectMissDistance)
            {
                OnPerfectMiss(obstacle);
            }
            else if (distance <= nearMissDistance)
            {
                OnNearMiss(obstacle);
            }
        }

        private float GetClosestDistance(Collider obstacle)
        {
            // Get closest point on obstacle to player
            Vector3 playerPos = transform.position;
            Vector3 closestPoint = obstacle.ClosestPoint(playerPos);

            // Only consider horizontal distance for lane-based near misses
            Vector3 horizontalPlayer = new Vector3(playerPos.x, 0, 0);
            Vector3 horizontalObstacle = new Vector3(closestPoint.x, 0, 0);

            return Vector3.Distance(horizontalPlayer, horizontalObstacle);
        }

        private void OnNearMiss(Obstacle obstacle)
        {
            lastNearMissTime = Time.time;
            lastComboTime = Time.time;
            currentCombo = Mathf.Min(currentCombo + 1, comboMultiplierMax);

            int points = nearMissPoints * currentCombo;
            AwardPoints(points);

            // Raise event
            GameEvents.RaiseNearMiss();

            // Visual feedback
            SpawnEffect(nearMissVFX, obstacle.transform.position);

            // Audio feedback
            PlayNearMissSound(false);

            Debug.Log($"Near Miss! Combo: {currentCombo}x, Points: {points}");
        }

        private void OnPerfectMiss(Obstacle obstacle)
        {
            lastNearMissTime = Time.time;
            lastComboTime = Time.time;
            currentCombo = Mathf.Min(currentCombo + 2, comboMultiplierMax);

            int points = perfectMissPoints * currentCombo;
            AwardPoints(points);

            // Raise event
            GameEvents.RaiseNearMiss();

            // Visual feedback
            SpawnEffect(perfectMissVFX, obstacle.transform.position);

            // Audio feedback
            PlayNearMissSound(true);

            Debug.Log($"PERFECT Miss! Combo: {currentCombo}x, Points: {points}");
        }

        private void AwardPoints(int points)
        {
            GameEvents.RaiseScoreChanged(points);
        }

        private void SpawnEffect(GameObject effectPrefab, Vector3 position)
        {
            if (effectPrefab != null)
            {
                var effect = Instantiate(effectPrefab, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        private void PlayNearMissSound(bool isPerfect)
        {
            if (ServiceLocator.TryGet<AudioManager>(out var audio))
            {
                audio.PlayNearMiss();
            }
        }

        /// <summary>
        /// Resets the near miss combo.
        /// </summary>
        public void ResetCombo()
        {
            currentCombo = 0;
        }
    }
}
