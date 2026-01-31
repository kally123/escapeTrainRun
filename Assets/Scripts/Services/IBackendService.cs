using System;
using System.Threading.Tasks;

namespace EscapeTrainRun.Services
{
    /// <summary>
    /// Base interface for all backend services.
    /// Provides common initialization and status patterns.
    /// </summary>
    public interface IBackendService
    {
        /// <summary>Service name for logging and debugging.</summary>
        string ServiceName { get; }

        /// <summary>Whether the service is currently connected and operational.</summary>
        bool IsConnected { get; }

        /// <summary>Whether the service is in offline/fallback mode.</summary>
        bool IsOfflineMode { get; }

        /// <summary>Initialize the service and establish connections.</summary>
        Task InitializeAsync();

        /// <summary>Gracefully shutdown the service.</summary>
        Task ShutdownAsync();
    }

    /// <summary>
    /// Result wrapper for backend operations.
    /// Provides success/failure status with optional data.
    /// </summary>
    /// <typeparam name="T">Type of data returned on success.</typeparam>
    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public T Data { get; private set; }
        public string ErrorMessage { get; private set; }
        public ServiceErrorCode ErrorCode { get; private set; }

        private ServiceResult() { }

        public static ServiceResult<T> Succeeded(T data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data,
                ErrorMessage = null,
                ErrorCode = ServiceErrorCode.None
            };
        }

        public static ServiceResult<T> Failed(string message, ServiceErrorCode code = ServiceErrorCode.Unknown)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Data = default,
                ErrorMessage = message,
                ErrorCode = code
            };
        }
    }

    /// <summary>
    /// Standard error codes for service operations.
    /// </summary>
    public enum ServiceErrorCode
    {
        None = 0,
        Unknown = 1,
        NetworkError = 100,
        Timeout = 101,
        ConnectionLost = 102,
        AuthenticationFailed = 200,
        AuthorizationDenied = 201,
        InvalidRequest = 300,
        NotFound = 301,
        Conflict = 302,
        RateLimited = 400,
        ServerError = 500,
        ServiceUnavailable = 503
    }

    /// <summary>
    /// Event data for service status changes.
    /// </summary>
    public class ServiceStatusChangedEventArgs : EventArgs
    {
        public string ServiceName { get; set; }
        public bool IsConnected { get; set; }
        public string StatusMessage { get; set; }
    }
}
