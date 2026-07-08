using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    /// <summary>
    /// Simple fail-safe manager that tracks errors and implements thresholds for session closure and session shutdown
    /// </summary>
    [AddComponentMenu("")]
    public class FusionFailSafeManager : Singleton<FusionFailSafeManager>
    {
        [Serializable]
        public class ErrorEntry
        {
            public DateTime timestamp;
            public string message;
                        public ErrorEntry(string message)
            {
                this.timestamp = DateTime.Now;
                this.message = message;
            }
        }

        // Events
        public static event Action<string> OnErrorLogged;
        public static event Action OnSessionMarkedClosed;
        public static event Action OnSessionShutdownInitiated;

        // Properties
        public static int ErrorCount => Instance ? Instance._errorHistory.Count : 0;
        public static bool IsSessionClosed => Instance && Instance._isSessionClosed;
        public static bool IsSessionShuttingDown => Instance && Instance._isSessionShuttingDown;

        private readonly List<ErrorEntry> _errorHistory = new();
        private bool _isSessionClosed = false;
        private bool _isSessionShuttingDown = false;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnSubsystemsInit()
        {
            Instance.WakeUp();
        }

        private void Awake()
        {
            // Subscribe to application log events to catch exceptions
            Application.logMessageReceived += OnLogMessageReceived;
            NetworkManager.EventGameStarted += ResetFailSafeState;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            NetworkManager.EventGameStarted -= ResetFailSafeState;
        }

        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            var settings = FusionRepository.Get.FailSafe;
            if (!settings.Enabled) return;

            // Only work when Fusion is connected in a session
            if (!NetworkManager.IsConnected) return;

            // Only track errors and exceptions
            if (type is LogType.Error or LogType.Exception)
            {
                LogError(logString);
            }
        }

        /// <summary>
        /// Logs an error and checks if thresholds are exceeded
        /// </summary>
        public static void LogError(string message)
        {
            var settings = FusionRepository.Get.FailSafe;
            if (!settings.Enabled) return;

            // Only work when Fusion is connected in a session
            if (!NetworkManager.IsConnected) return;

            var errorEntry = new ErrorEntry(message);
            Instance._errorHistory.Add(errorEntry);

            // Clean old errors if time window is set
            if (settings.ErrorTimeWindow > 0)
            {
                Instance.CleanOldErrors();
            }

            OnErrorLogged?.Invoke(message);
            // Debug.LogWarning($"[FusionFailSafe] Error logged ({Instance._errorHistory.Count} total): {message}");

            Instance.CheckThresholds();
        }

        private void CleanOldErrors()
        {
            var settings = FusionRepository.Get.FailSafe;            var cutoffTime = DateTime.Now.AddSeconds(-settings.ErrorTimeWindow);
            _errorHistory.RemoveAll(error => error.timestamp < cutoffTime);
        }

        private void CheckThresholds()
        {
            var settings = FusionRepository.Get.FailSafe;
            var errorCount = _errorHistory.Count;

            // Debug.Log($"[FusionFailSafe] Checking thresholds: {errorCount} errors, close threshold: {settings.CloseErrorThreshold}, shutdown threshold: {settings.ShutdownErrorThreshold}");

            // Check session shutdown threshold
            if (errorCount >= settings.ShutdownErrorThreshold && !_isSessionShuttingDown)
            {
                InitiateSessionShutdown();
                return;
            }

            // Check session close threshold
            if (errorCount >= settings.CloseErrorThreshold && !_isSessionClosed)
            {
                MarkSessionAsClosed();
            }
        }

        private void MarkSessionAsClosed()
        {
            if (_isSessionClosed) return;

            _isSessionClosed = true;
            Debug.LogWarning("[FusionFailSafe] Session marked as closed due to error threshold");
            
            OnSessionMarkedClosed?.Invoke();

            // Prevent new players from joining
            if (NetworkManager.Runner && (NetworkManager.Runner.IsServer || NetworkManager.Runner.IsSharedModeMasterClient))
            {
                NetworkManager.Runner.SessionInfo.IsOpen = false;
            }
        }

        public async void InitiateSessionShutdown()
        {
            if (_isSessionShuttingDown) return;

            _isSessionShuttingDown = true;
            Debug.LogWarning("[FusionFailSafe] Initiating session shutdown due to error threshold");
            
            OnSessionShutdownInitiated?.Invoke();

            // Graceful shutdown
            try
            {
                await NetworkManager.DisconnectAsync();            }
            catch (Exception ex)
            {
                Debug.LogError($"[FusionFailSafe] Error during shutdown: {ex.Message}");
            }
            
            ResetFailSafeState();
        }

        /// <summary>
        /// Manually reset the fail-safe state
        /// </summary>
        public static void ResetFailSafeState()
        {
            if (!Instance) return;

            Instance._errorHistory.Clear();
            Instance._isSessionClosed = false;
            Instance._isSessionShuttingDown = false;

            // Debug.Log("[FusionFailSafe] Fail-safe state reset");
        }
    }
}