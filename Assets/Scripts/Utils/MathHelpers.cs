using UnityEngine;

namespace EscapeTrainRun.Utils
{
    /// <summary>
    /// Math helper functions for common game calculations.
    /// </summary>
    public static class MathHelpers
    {
        #region Easing Functions

        /// <summary>
        /// Smoothstep easing - smooth acceleration and deceleration.
        /// </summary>
        public static float Smoothstep(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Smootherstep easing - smoother version of smoothstep.
        /// </summary>
        public static float Smootherstep(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * t * (t * (6f * t - 15f) + 10f);
        }

        /// <summary>
        /// Ease in quadratic.
        /// </summary>
        public static float EaseInQuad(float t)
        {
            return t * t;
        }

        /// <summary>
        /// Ease out quadratic.
        /// </summary>
        public static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        /// <summary>
        /// Ease in-out quadratic.
        /// </summary>
        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        /// <summary>
        /// Ease out bounce.
        /// </summary>
        public static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2f / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }

        /// <summary>
        /// Ease out elastic.
        /// </summary>
        public static float EaseOutElastic(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;

            if (t <= 0) return 0;
            if (t >= 1) return 1;

            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }

        #endregion

        #region Jump Curves

        /// <summary>
        /// Calculates a parabolic jump height for a given normalized time.
        /// </summary>
        /// <param name="t">Normalized time (0-1).</param>
        /// <param name="maxHeight">Maximum jump height.</param>
        /// <returns>Height at time t.</returns>
        public static float ParabolicJump(float t, float maxHeight)
        {
            // 4 * h * t * (1 - t) gives a parabola peaking at h when t = 0.5
            return 4f * maxHeight * t * (1f - t);
        }

        /// <summary>
        /// Calculates a sine-based jump curve (smoother than parabolic).
        /// </summary>
        public static float SineJump(float t, float maxHeight)
        {
            return Mathf.Sin(t * Mathf.PI) * maxHeight;
        }

        #endregion

        #region Lane Calculations

        /// <summary>
        /// Converts a lane index to world X position.
        /// </summary>
        /// <param name="lane">Lane index (0 = left, 1 = center, 2 = right).</param>
        /// <param name="laneWidth">Width of each lane.</param>
        /// <returns>World X position.</returns>
        public static float LaneToWorldX(int lane, float laneWidth = Constants.LaneWidth)
        {
            return (lane - 1) * laneWidth;
        }

        /// <summary>
        /// Converts a world X position to the nearest lane index.
        /// </summary>
        /// <param name="worldX">World X position.</param>
        /// <param name="laneWidth">Width of each lane.</param>
        /// <returns>Lane index (0-2).</returns>
        public static int WorldXToLane(float worldX, float laneWidth = Constants.LaneWidth)
        {
            int lane = Mathf.RoundToInt(worldX / laneWidth) + 1;
            return Mathf.Clamp(lane, 0, 2);
        }

        #endregion

        #region Distance & Collision

        /// <summary>
        /// Calculates squared distance (faster than Distance when comparing).
        /// </summary>
        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            return (a - b).sqrMagnitude;
        }

        /// <summary>
        /// Checks if two positions are within a given distance.
        /// </summary>
        public static bool WithinDistance(Vector3 a, Vector3 b, float distance)
        {
            return SqrDistance(a, b) <= distance * distance;
        }

        /// <summary>
        /// Calculates the horizontal distance (ignoring Y).
        /// </summary>
        public static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        #endregion

        #region Random Helpers

        /// <summary>
        /// Returns true with the given probability (0-1).
        /// </summary>
        public static bool RandomChance(float probability)
        {
            return Random.value < probability;
        }

        /// <summary>
        /// Returns a random value between -1 and 1.
        /// </summary>
        public static float RandomSigned()
        {
            return Random.Range(-1f, 1f);
        }

        /// <summary>
        /// Returns either -1 or 1 randomly.
        /// </summary>
        public static int RandomSign()
        {
            return Random.value < 0.5f ? -1 : 1;
        }

        /// <summary>
        /// Returns a random point within a circle.
        /// </summary>
        public static Vector2 RandomPointInCircle(float radius)
        {
            return Random.insideUnitCircle * radius;
        }

        /// <summary>
        /// Returns a random point on a circle's edge.
        /// </summary>
        public static Vector2 RandomPointOnCircle(float radius)
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        #endregion

        #region Score Formatting

        /// <summary>
        /// Formats a score with thousand separators.
        /// </summary>
        public static string FormatScore(int score)
        {
            return score.ToString("N0");
        }

        /// <summary>
        /// Formats distance in meters or kilometers.
        /// </summary>
        public static string FormatDistance(float meters)
        {
            if (meters < 1000)
            {
                return $"{meters:F0}m";
            }
            else
            {
                return $"{meters / 1000f:F2}km";
            }
        }

        /// <summary>
        /// Formats time as mm:ss.
        /// </summary>
        public static string FormatTime(float seconds)
        {
            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{mins:D2}:{secs:D2}";
        }

        #endregion

        #region Clamping

        /// <summary>
        /// Clamps a lane index to valid range (0-2).
        /// </summary>
        public static int ClampLane(int lane)
        {
            return Mathf.Clamp(lane, 0, Constants.LaneCount - 1);
        }

        /// <summary>
        /// Wraps a value to stay within a range (like modulo but handles negatives).
        /// </summary>
        public static float Wrap(float value, float min, float max)
        {
            float range = max - min;
            while (value < min) value += range;
            while (value >= max) value -= range;
            return value;
        }

        /// <summary>
        /// Wraps an integer value.
        /// </summary>
        public static int Wrap(int value, int min, int max)
        {
            int range = max - min;
            while (value < min) value += range;
            while (value >= max) value -= range;
            return value;
        }

        #endregion
    }
}
