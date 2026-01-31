using UnityEngine;
using System;
using EscapeTrainRun.Environment;

namespace EscapeTrainRun.Obstacles
{
    /// <summary>
    /// ScriptableObject containing obstacle prefabs for a specific theme.
    /// </summary>
    [CreateAssetMenu(fileName = "ObstaclePrefabSet", menuName = "EscapeTrainRun/Obstacles/Prefab Set")]
    public class ObstaclePrefabSet : ScriptableObject
    {
        [Header("Theme")]
        public string themeName;
        public ThemeType theme;

        [Header("Static Obstacles")]
        public GameObject[] staticObstacles;

        [Header("Low Obstacles (Jump Over)")]
        public GameObject[] lowObstacles;

        [Header("High Obstacles (Slide Under)")]
        public GameObject[] highObstacles;

        [Header("Moving Obstacles")]
        public GameObject[] movingObstacles;

        [Header("Wide Obstacles")]
        public GameObject[] wideObstacles;

        [Header("Combined Obstacles")]
        public GameObject[] combinedObstacles;

        [Header("Flying Obstacles")]
        public GameObject[] flyingObstacles;

        [Header("Destructible Obstacles")]
        public GameObject[] destructibleObstacles;

        [Header("Hazard Obstacles")]
        public GameObject[] hazardObstacles;

        [Header("Boss Obstacles")]
        public GameObject[] bossObstacles;

        /// <summary>
        /// Gets a random prefab for the specified obstacle type.
        /// </summary>
        public GameObject GetPrefab(ObstacleType type)
        {
            var array = GetPrefabArray(type);
            if (array == null || array.Length == 0) return null;

            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Gets all prefabs for the specified obstacle type.
        /// </summary>
        public GameObject[] GetPrefabs(ObstacleType type)
        {
            return GetPrefabArray(type);
        }

        /// <summary>
        /// Gets the count of prefabs for a type.
        /// </summary>
        public int GetPrefabCount(ObstacleType type)
        {
            var array = GetPrefabArray(type);
            return array?.Length ?? 0;
        }

        private GameObject[] GetPrefabArray(ObstacleType type)
        {
            return type switch
            {
                ObstacleType.Static => staticObstacles,
                ObstacleType.Low => lowObstacles,
                ObstacleType.High => highObstacles,
                ObstacleType.Moving => movingObstacles,
                ObstacleType.Wide => wideObstacles,
                ObstacleType.Combined => combinedObstacles,
                ObstacleType.Flying => flyingObstacles,
                ObstacleType.Destructible => destructibleObstacles,
                ObstacleType.Hazard => hazardObstacles,
                ObstacleType.Boss => bossObstacles,
                _ => staticObstacles
            };
        }
    }
}
