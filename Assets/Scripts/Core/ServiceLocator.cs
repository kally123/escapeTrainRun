using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeTrainRun.Core
{
    /// <summary>
    /// Central service locator for dependency management.
    /// Following microservice single responsibility principle.
    /// Provides loose coupling between game systems.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private static bool isQuitting = false;

        /// <summary>
        /// Registers a service instance for the specified type.
        /// </summary>
        /// <typeparam name="T">The service interface or type.</typeparam>
        /// <param name="service">The service instance to register.</param>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"[ServiceLocator] Cannot register null service for type {typeof(T).Name}");
                return;
            }

            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered. Replacing with new instance.");
            }
            services[type] = service;
            Debug.Log($"[ServiceLocator] Registered service: {type.Name}");
        }

        /// <summary>
        /// Gets a registered service by type.
        /// </summary>
        /// <typeparam name="T">The service type to retrieve.</typeparam>
        /// <returns>The registered service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when service is not registered.</exception>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            if (isQuitting)
            {
                return null;
            }

            throw new InvalidOperationException(
                $"[ServiceLocator] Service {type.Name} not registered. " +
                "Make sure it's registered before accessing.");
        }

        /// <summary>
        /// Tries to get a registered service by type.
        /// </summary>
        /// <typeparam name="T">The service type to retrieve.</typeparam>
        /// <param name="service">The output service instance.</param>
        /// <returns>True if the service was found, false otherwise.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        /// <typeparam name="T">The service type to check.</typeparam>
        /// <returns>True if the service is registered.</returns>
        public static bool IsRegistered<T>() where T : class
        {
            return services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Unregisters a service by type.
        /// </summary>
        /// <typeparam name="T">The service type to unregister.</typeparam>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            if (services.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered service: {type.Name}");
            }
        }

        /// <summary>
        /// Clears all registered services.
        /// Called when transitioning between major game states.
        /// </summary>
        public static void Clear()
        {
            services.Clear();
            Debug.Log("[ServiceLocator] All services cleared");
        }

        /// <summary>
        /// Called when the application is quitting to prevent errors.
        /// </summary>
        public static void OnApplicationQuit()
        {
            isQuitting = true;
            Clear();
        }

        /// <summary>
        /// Gets the count of registered services.
        /// </summary>
        public static int ServiceCount => services.Count;
    }
}
