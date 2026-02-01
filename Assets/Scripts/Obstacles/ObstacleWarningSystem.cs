using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EscapeTrainRun.Core;
using EscapeTrainRun.Player;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// Displays warning indicators for upcoming obstacles.
    /// </summary>
    public class ObstacleWarningSystem : MonoBehaviour
    {
        public static ObstacleWarningSystem Instance { get; private set; }

        [Header("Warning Settings")]
        [SerializeField] private float warningDistance = 40f;
        [SerializeField] private float criticalDistance = 15f;
        [SerializeField] private bool enableWarnings = true;

        [Header("UI References")]
        [SerializeField] private RectTransform warningContainer;
        [SerializeField] private GameObject warningIndicatorPrefab;

        [Header("Warning Icons")]
        [SerializeField] private Sprite jumpWarningIcon;
        [SerializeField] private Sprite slideWarningIcon;
        [SerializeField] private Sprite dodgeWarningIcon;
        [SerializeField] private Sprite dangerWarningIcon;

        [Header("Colors")]
        [SerializeField] private Color normalWarningColor = Color.yellow;
        [SerializeField] private Color criticalWarningColor = Color.red;

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseScale = 1.2f;

        // Active warnings
        private Dictionary<Obstacle, WarningIndicator> activeWarnings = new Dictionary<Obstacle, WarningIndicator>();
        private Queue<WarningIndicator> indicatorPool = new Queue<WarningIndicator>();

        // Player reference
        private Transform playerTransform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePool();
        }

        private void Start()
        {
            FindPlayer();
        }

        private void InitializePool()
        {
            if (warningIndicatorPrefab == null || warningContainer == null) return;

            for (int i = 0; i < 10; i++)
            {
                CreatePooledIndicator();
            }
        }

        private void CreatePooledIndicator()
        {
            var instance = Instantiate(warningIndicatorPrefab, warningContainer);
            var indicator = instance.GetComponent<WarningIndicator>();
            if (indicator == null)
            {
                indicator = instance.AddComponent<WarningIndicator>();
            }
            instance.SetActive(false);
            indicatorPool.Enqueue(indicator);
        }

        private void FindPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            playerTransform = player?.transform;
        }

        private void Update()
        {
            if (!enableWarnings || playerTransform == null) return;

            UpdateWarnings();
        }

        private void UpdateWarnings()
        {
            // Get all active obstacles from manager
            if (ObstacleManager.Instance == null) return;

            // Check each obstacle's distance
            var obstacles = FindObjectsOfType<Obstacle>();

            foreach (var obstacle in obstacles)
            {
                if (!obstacle.IsActive) continue;

                float distance = obstacle.transform.position.z - playerTransform.position.z;

                if (distance > 0 && distance <= warningDistance)
                {
                    // Should show warning
                    if (!activeWarnings.ContainsKey(obstacle))
                    {
                        ShowWarning(obstacle);
                    }
                    else
                    {
                        UpdateWarning(obstacle, distance);
                    }
                }
                else if (activeWarnings.ContainsKey(obstacle))
                {
                    // Should hide warning
                    HideWarning(obstacle);
                }
            }
        }

        #region Public API

        /// <summary>
        /// Shows a warning for the specified obstacle.
        /// </summary>
        public void ShowWarning(Obstacle obstacle)
        {
            if (!enableWarnings || activeWarnings.ContainsKey(obstacle)) return;

            WarningIndicator indicator = GetIndicatorFromPool();
            if (indicator == null) return;

            indicator.Setup(
                GetWarningIcon(obstacle.AvoidanceMethod),
                normalWarningColor,
                GetLanePosition(obstacle.Lane)
            );
            indicator.Show();

            activeWarnings[obstacle] = indicator;
            obstacle.ShowWarning();
        }

        /// <summary>
        /// Hides the warning for the specified obstacle.
        /// </summary>
        public void HideWarning(Obstacle obstacle)
        {
            if (!activeWarnings.TryGetValue(obstacle, out var indicator)) return;

            indicator.Hide();
            ReturnIndicatorToPool(indicator);
            activeWarnings.Remove(obstacle);
            obstacle.HideWarning();
        }

        /// <summary>
        /// Clears all warnings.
        /// </summary>
        public void ClearAllWarnings()
        {
            foreach (var kvp in activeWarnings)
            {
                kvp.Value.Hide();
                ReturnIndicatorToPool(kvp.Value);
                kvp.Key.HideWarning();
            }
            activeWarnings.Clear();
        }

        /// <summary>
        /// Enables or disables the warning system.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            enableWarnings = enabled;
            if (!enabled)
            {
                ClearAllWarnings();
            }
        }

        #endregion

        #region Helpers

        private void UpdateWarning(Obstacle obstacle, float distance)
        {
            if (!activeWarnings.TryGetValue(obstacle, out var indicator)) return;

            // Update urgency based on distance
            float urgency = 1f - (distance / warningDistance);
            bool isCritical = distance <= criticalDistance;

            Color color = isCritical ? criticalWarningColor : normalWarningColor;
            indicator.UpdateUrgency(urgency, color, isCritical);
        }

        private Sprite GetWarningIcon(ObstacleAction avoidance)
        {
            return avoidance switch
            {
                ObstacleAction.Jump => jumpWarningIcon,
                ObstacleAction.Slide => slideWarningIcon,
                ObstacleAction.ChangeLane => dodgeWarningIcon,
                ObstacleAction.Any => dangerWarningIcon,
                _ => dangerWarningIcon
            };
        }

        private Vector2 GetLanePosition(int lane)
        {
            // Map lane to screen position
            float screenWidth = Screen.width;
            float laneWidth = screenWidth / 3f;
            float x = (lane - 1) * laneWidth;

            return new Vector2(x, 0);
        }

        private WarningIndicator GetIndicatorFromPool()
        {
            if (indicatorPool.Count == 0)
            {
                CreatePooledIndicator();
            }

            if (indicatorPool.Count > 0)
            {
                return indicatorPool.Dequeue();
            }

            return null;
        }

        private void ReturnIndicatorToPool(WarningIndicator indicator)
        {
            indicator.gameObject.SetActive(false);
            indicatorPool.Enqueue(indicator);
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    /// <summary>
    /// Individual warning indicator UI element.
    /// </summary>
    public class WarningIndicator : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private RectTransform rectTransform;
        private float baseScale = 1f;
        private bool isPulsing;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (iconImage == null)
            {
                iconImage = GetComponentInChildren<Image>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Update()
        {
            if (isPulsing)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.2f;
                transform.localScale = Vector3.one * baseScale * pulse;
            }
        }

        public void Setup(Sprite icon, Color color, Vector2 position)
        {
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.color = color;
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        public void Hide()
        {
            isPulsing = false;
            gameObject.SetActive(false);
        }

        public void UpdateUrgency(float urgency, Color color, bool pulse)
        {
            if (iconImage != null)
            {
                iconImage.color = color;
            }

            baseScale = 1f + urgency * 0.3f;
            isPulsing = pulse;

            if (!isPulsing)
            {
                transform.localScale = Vector3.one * baseScale;
            }
        }
    }
}
