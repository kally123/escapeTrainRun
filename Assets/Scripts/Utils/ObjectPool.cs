using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeTrainRun.Utils
{
    /// <summary>
    /// Generic object pool to minimize garbage collection.
    /// Following performance guidelines for memory efficiency.
    /// </summary>
    /// <typeparam name="T">The type of objects to pool.</typeparam>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> pool;
        private readonly Func<T> createFunc;
        private readonly Action<T> actionOnGet;
        private readonly Action<T> actionOnRelease;
        private readonly Action<T> actionOnDestroy;
        private readonly int maxSize;

        /// <summary>
        /// Total number of objects created by this pool.
        /// </summary>
        public int CountAll { get; private set; }

        /// <summary>
        /// Number of objects currently in use.
        /// </summary>
        public int CountActive => CountAll - pool.Count;

        /// <summary>
        /// Number of objects available in the pool.
        /// </summary>
        public int CountInactive => pool.Count;

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="createFunc">Function to create new instances.</param>
        /// <param name="actionOnGet">Called when an object is retrieved from the pool.</param>
        /// <param name="actionOnRelease">Called when an object is returned to the pool.</param>
        /// <param name="actionOnDestroy">Called when an object is destroyed (pool overflow).</param>
        /// <param name="maxSize">Maximum pool size.</param>
        public ObjectPool(
            Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            int maxSize = 100)
        {
            this.pool = new Stack<T>();
            this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            this.actionOnGet = actionOnGet;
            this.actionOnRelease = actionOnRelease;
            this.actionOnDestroy = actionOnDestroy;
            this.maxSize = maxSize;
        }

        /// <summary>
        /// Gets an object from the pool or creates a new one if empty.
        /// </summary>
        public T Get()
        {
            T item;
            if (pool.Count > 0)
            {
                item = pool.Pop();
            }
            else
            {
                item = createFunc();
                CountAll++;
            }

            actionOnGet?.Invoke(item);
            return item;
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        public void Release(T item)
        {
            if (item == null) return;

            actionOnRelease?.Invoke(item);

            if (pool.Count < maxSize)
            {
                pool.Push(item);
            }
            else
            {
                // Pool is full, destroy the item
                actionOnDestroy?.Invoke(item);
                CountAll--;
            }
        }

        /// <summary>
        /// Clears all objects from the pool.
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                T item = pool.Pop();
                actionOnDestroy?.Invoke(item);
            }
            CountAll = 0;
        }

        /// <summary>
        /// Pre-warms the pool with a specified number of objects.
        /// </summary>
        public void Prewarm(int count)
        {
            count = Mathf.Min(count, maxSize);
            for (int i = 0; i < count; i++)
            {
                T item = createFunc();
                CountAll++;
                actionOnRelease?.Invoke(item);
                pool.Push(item);
            }
        }
    }

    /// <summary>
    /// Unity GameObject-specific object pool.
    /// Optimized for pooling prefab instances.
    /// </summary>
    public class GameObjectPool
    {
        private readonly Stack<GameObject> pool;
        private readonly GameObject prefab;
        private readonly Transform parent;
        private readonly int maxSize;

        public int CountAll { get; private set; }
        public int CountActive => CountAll - pool.Count;
        public int CountInactive => pool.Count;

        /// <summary>
        /// Creates a new GameObject pool.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="parent">Parent transform for pooled objects.</param>
        /// <param name="initialSize">Initial pool size (pre-warm).</param>
        /// <param name="maxSize">Maximum pool size.</param>
        public GameObjectPool(GameObject prefab, Transform parent = null, int initialSize = 0, int maxSize = 100)
        {
            this.pool = new Stack<GameObject>();
            this.prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            this.parent = parent;
            this.maxSize = maxSize;

            // Pre-warm pool
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = CreateInstance();
                obj.SetActive(false);
                pool.Push(obj);
            }
        }

        private GameObject CreateInstance()
        {
            GameObject obj = UnityEngine.Object.Instantiate(prefab, parent);
            CountAll++;
            return obj;
        }

        /// <summary>
        /// Gets a GameObject from the pool.
        /// </summary>
        public GameObject Get()
        {
            GameObject obj;
            if (pool.Count > 0)
            {
                obj = pool.Pop();
            }
            else
            {
                obj = CreateInstance();
            }

            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Gets a GameObject from the pool at a specific position and rotation.
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj = Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        /// <summary>
        /// Returns a GameObject to the pool.
        /// </summary>
        public void Release(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(parent);

            if (pool.Count < maxSize)
            {
                pool.Push(obj);
            }
            else
            {
                UnityEngine.Object.Destroy(obj);
                CountAll--;
            }
        }

        /// <summary>
        /// Clears all objects from the pool.
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                GameObject obj = pool.Pop();
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj);
                }
            }
            CountAll = 0;
        }
    }
}
