using BepInEx;
using BepInEx.Logging;
using DynamicQuotaCap.Configuration;

namespace DynamicQuotaCap.Services
{
    /// <summary>
    /// Enhanced logging utilities for the Dynamic Quota Cap plugin
    /// </summary>
    public class LoggingService
    {
        private readonly ManualLogSource _logger;
        private readonly ConfigManager _configManager;

        /// <summary>
        /// Initializes a new instance of the LoggingService class
        /// </summary>
        /// <param name="logger">The BepInEx logger instance</param>
        /// <param name="configManager">The configuration manager instance</param>
        public LoggingService(ManualLogSource logger, ConfigManager configManager)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _configManager = configManager ?? throw new System.ArgumentNullException(nameof(configManager));
        }

        /// <summary>
        /// Logs a debug message if debug mode is enabled
        /// </summary>
        /// <param name="message">The message to log</param>
        public void LogDebug(string message)
        {
            if (_configManager?.EnableDebug?.Value == true)
            {
                _logger.LogInfo($"[DEBUG] {message}");
            }
        }

        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="message">The message to log</param>
        public void LogInfo(string message)
        {
            _logger.LogInfo(message);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        public void LogError(string message)
        {
            _logger.LogError(message);
        }

        /// <summary>
        /// Logs an error message with exception details
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The exception to include in the log</param>
        public void LogError(string message, System.Exception exception)
        {
            _logger.LogError($"{message}: {exception.Message}");
            if (exception.InnerException != null)
            {
                _logger.LogError($"Inner exception: {exception.InnerException.Message}");
            }
        }

        /// <summary>
        /// Logs a message with structured data
        /// </summary>
        /// <param name="level">The log level</param>
        /// <param name="category">The log category</param>
        /// <param name="message">The message to log</param>
        /// <param name="data">Additional data to include in the log</param>
        public void LogStructured(LogLevel level, string category, string message, object data = null)
        {
            string structuredMessage = $"[{category}] {message}";
            
            if (data != null)
            {
                structuredMessage += $" | Data: {data}";
            }

            switch (level)
            {
                case LogLevel.Debug:
                    LogDebug(structuredMessage);
                    break;
                case LogLevel.Info:
                    LogInfo(structuredMessage);
                    break;
                case LogLevel.Warning:
                    LogWarning(structuredMessage);
                    break;
                case LogLevel.Error:
                    LogError(structuredMessage);
                    break;
                case LogLevel.Fatal:
                    _logger.LogFatal(structuredMessage);
                    break;
                default:
                    LogInfo(structuredMessage);
                    break;
            }
        }

        /// <summary>
        /// Logs the start of a method or operation
        /// </summary>
        /// <param name="methodName">The name of the method or operation</param>
        /// <param name="parameters">Optional parameters to log</param>
        public void LogMethodStart(string methodName, object parameters = null)
        {
            string message = $"Starting {methodName}";
            if (parameters != null)
            {
                message += $" with parameters: {parameters}";
            }
            LogDebug(message);
        }

        /// <summary>
        /// Logs the end of a method or operation
        /// </summary>
        /// <param name="methodName">The name of the method or operation</param>
        /// <param name="result">Optional result to log</param>
        public void LogMethodEnd(string methodName, object result = null)
        {
            string message = $"Finished {methodName}";
            if (result != null)
            {
                message += $" with result: {result}";
            }
            LogDebug(message);
        }

        /// <summary>
        /// Logs quota cap application details
        /// </summary>
        /// <param name="constellation">The current constellation</param>
        /// <param name="originalQuota">The original quota value</param>
        /// <param name="newQuota">The new quota value after capping</param>
        /// <param name="capValue">The cap value that was applied</param>
        /// <param name="capEnabled">Whether the cap was enabled</param>
        public void LogQuotaCapApplication(string constellation, int originalQuota, int newQuota, int capValue, bool capEnabled)
        {
            if (!capEnabled)
            {
                LogDebug($"Quota cap disabled for constellation '{constellation}'. Original quota: {originalQuota}");
                return;
            }

            if (capValue == Configuration.ConfigConstants.DisabledQuotaCapValue)
            {
                LogDebug($"Quota cap set to disabled (-1) for constellation '{constellation}'. Original quota: {originalQuota}");
                return;
            }

            if (originalQuota > capValue)
            {
                LogInfo($"Quota capped for constellation '{constellation}': {originalQuota} -> {newQuota} (cap: {capValue})");
            }
            else
            {
                LogDebug($"Quota within cap for constellation '{constellation}': {originalQuota} (cap: {capValue})");
            }
        }

        /// <summary>
        /// Logs configuration generation details
        /// </summary>
        /// <param name="constellationCount">The number of constellations processed</param>
        /// <param name="success">Whether the generation was successful</param>
        /// <param name="errorMessage">Optional error message if generation failed</param>
        public void LogConfigGeneration(int constellationCount, bool success, string errorMessage = null)
        {
            if (success)
            {
                LogInfo($"Configuration generation completed successfully for {constellationCount} constellations");
            }
            else
            {
                LogError($"Configuration generation failed for {constellationCount} constellations");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    LogError($"Error details: {errorMessage}");
                }
            }
        }

        /// <summary>
        /// Logs patch application status
        /// </summary>
        /// <param name="patchName">The name of the patch</param>
        /// <param name="success">Whether the patch was applied successfully</param>
        /// <param name="errorMessage">Optional error message if patch failed</param>
        public void LogPatchApplication(string patchName, bool success, string errorMessage = null)
        {
            if (success)
            {
                LogInfo($"Patch '{patchName}' applied successfully");
            }
            else
            {
                LogError($"Patch '{patchName}' failed to apply");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    LogError($"Patch error details: {errorMessage}");
                }
            }
        }
    }

    /// <summary>
    /// Log levels for structured logging
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }
}