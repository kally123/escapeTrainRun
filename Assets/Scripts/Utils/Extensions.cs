using UnityEngine;
using System.Collections.Generic;

namespace EscapeTrainRun.Utils
{
    /// <summary>
    /// Extension methods for common Unity operations.
    /// Improves code readability and reduces boilerplate.
    /// </summary>
    public static class Extensions
    {
        #region Transform Extensions

        /// <summary>
        /// Resets the transform to default local position, rotation, and scale.
        /// </summary>
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Resets the transform to default world position and rotation.
        /// </summary>
        public static void ResetWorld(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Sets only the X position of the transform.
        /// </summary>
        public static void SetX(this Transform transform, float x)
        {
            Vector3 pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }

        /// <summary>
        /// Sets only the Y position of the transform.
        /// </summary>
        public static void SetY(this Transform transform, float y)
        {
            Vector3 pos = transform.position;
            pos.y = y;
            transform.position = pos;
        }

        /// <summary>
        /// Sets only the Z position of the transform.
        /// </summary>
        public static void SetZ(this Transform transform, float z)
        {
            Vector3 pos = transform.position;
            pos.z = z;
            transform.position = pos;
        }

        /// <summary>
        /// Sets the local X position.
        /// </summary>
        public static void SetLocalX(this Transform transform, float x)
        {
            Vector3 pos = transform.localPosition;
            pos.x = x;
            transform.localPosition = pos;
        }

        /// <summary>
        /// Sets the local Y position.
        /// </summary>
        public static void SetLocalY(this Transform transform, float y)
        {
            Vector3 pos = transform.localPosition;
            pos.y = y;
            transform.localPosition = pos;
        }

        /// <summary>
        /// Sets the local Z position.
        /// </summary>
        public static void SetLocalZ(this Transform transform, float z)
        {
            Vector3 pos = transform.localPosition;
            pos.z = z;
            transform.localPosition = pos;
        }

        /// <summary>
        /// Destroys all children of the transform.
        /// </summary>
        public static void DestroyAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        #endregion

        #region Vector Extensions

        /// <summary>
        /// Returns a copy with a modified X value.
        /// </summary>
        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        /// <summary>
        /// Returns a copy with a modified Y value.
        /// </summary>
        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        /// <summary>
        /// Returns a copy with a modified Z value.
        /// </summary>
        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        /// Converts to Vector2 (XY).
        /// </summary>
        public static Vector2 ToVector2XY(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        /// Converts to Vector2 (XZ).
        /// </summary>
        public static Vector2 ToVector2XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        /// <summary>
        /// Returns a flat version (Y = 0).
        /// </summary>
        public static Vector3 Flat(this Vector3 v)
        {
            return new Vector3(v.x, 0, v.z);
        }

        #endregion

        #region GameObject Extensions

        /// <summary>
        /// Sets the layer for this GameObject and all children.
        /// </summary>
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        /// <summary>
        /// Gets or adds a component to the GameObject.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Checks if the GameObject has a specific component.
        /// </summary>
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }

        #endregion

        #region Collection Extensions

        /// <summary>
        /// Gets a random element from the list.
        /// </summary>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default;
            }
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Gets a random element from the array.
        /// </summary>
        public static T GetRandom<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return default;
            }
            return array[Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Shuffles the list in place.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Checks if the collection is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        #endregion

        #region Color Extensions

        /// <summary>
        /// Returns a copy with modified alpha.
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        #endregion

        #region String Extensions

        /// <summary>
        /// Checks if the string is null or whitespace.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Truncates the string to a maximum length.
        /// </summary>
        public static string Truncate(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
            {
                return str;
            }
            return str.Substring(0, maxLength) + "...";
        }

        #endregion

        #region Float Extensions

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }

        /// <summary>
        /// Checks if the value is approximately equal to another.
        /// </summary>
        public static bool Approximately(this float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        #endregion
    }
}
