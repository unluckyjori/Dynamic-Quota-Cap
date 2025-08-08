using System;
using DynamicQuotaCap.Configuration;

namespace DynamicQuotaCap.Services
{
    /// <summary>
    /// Business logic for quota capping functionality
    /// </summary>
    public class QuotaCapService
    {
        private readonly ConfigManager _configManager;
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Initializes a new instance of the QuotaCapService class
        /// </summary>
        /// <param name="configManager">The configuration manager instance</param>
        /// <param name="loggingService">The logging service instance</param>
        public QuotaCapService(ConfigManager configManager, LoggingService loggingService)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        /// <summary>
        /// Calculates the quota cap for a given constellation
        /// </summary>
        /// <param name="constellation">The constellation name</param>
        /// <param name="currentQuota">The current quota value</param>
        /// <returns>The capped quota value</returns>
        public int CalculateQuotaCap(string constellation, int currentQuota)
        {
            _loggingService.LogMethodStart(nameof(CalculateQuotaCap), new { constellation, currentQuota });

            try
            {
                // If we're not in any constellation, return the original quota
                if (string.IsNullOrEmpty(constellation))
                {
                    _loggingService.LogDebug("Not in any constellation, skipping quota cap");
                    _loggingService.LogMethodEnd(nameof(CalculateQuotaCap), currentQuota);
                    return currentQuota;
                }

                // Check if configs have been generated
                if (_configManager.ConstellationCaps.Count == 0)
                {
                    _loggingService.LogDebug("No constellation configs generated yet, skipping quota cap");
                    _loggingService.LogMethodEnd(nameof(CalculateQuotaCap), currentQuota);
                    return currentQuota;
                }

                // Get the cap settings for this constellation
                int cap = GetConstellationCap(constellation);
                bool enabled = IsQuotaCapEnabled(constellation);

                _loggingService.LogDebug($"Constellation '{constellation}' - Cap: {cap}, Enabled: {enabled}");

                // Apply the cap logic
                int result = ApplyQuotaCapLogic(currentQuota, cap, enabled, constellation);
                
                _loggingService.LogQuotaCapApplication(constellation, currentQuota, result, cap, enabled);
                _loggingService.LogMethodEnd(nameof(CalculateQuotaCap), result);
                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error calculating quota cap for constellation '{constellation}'", ex);
                _loggingService.LogMethodEnd(nameof(CalculateQuotaCap), currentQuota);
                return currentQuota; // Return original quota on error
            }
        }

        /// <summary>
        /// Gets the quota cap value for a specific constellation
        /// </summary>
        /// <param name="constellation">The constellation name</param>
        /// <returns>The quota cap value</returns>
        public int GetConstellationCap(string constellation)
        {
            try
            {
                if (_configManager.ConstellationCaps.TryGetValue(constellation, out var capEntry))
                {
                    return capEntry.Value;
                }

                _loggingService.LogDebug($"No specific config found for constellation '{constellation}', using default cap");
                return GetDefaultQuotaCap();
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error getting constellation cap for '{constellation}'", ex);
                return GetDefaultQuotaCap();
            }
        }

        /// <summary>
        /// Checks if quota cap is enabled for a specific constellation
        /// </summary>
        /// <param name="constellation">The constellation name</param>
        /// <returns>True if quota cap is enabled, false otherwise</returns>
        public bool IsQuotaCapEnabled(string constellation)
        {
            try
            {
                if (_configManager.ConstellationCapEnabled.TryGetValue(constellation, out var enabledEntry))
                {
                    return enabledEntry.Value;
                }

                _loggingService.LogDebug($"No enabled config found for constellation '{constellation}', defaulting to enabled");
                return true; // Default to enabled if no specific config
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error checking if quota cap is enabled for constellation '{constellation}'", ex);
                return true; // Default to enabled on error
            }
        }

        /// <summary>
        /// Gets the default quota cap value
        /// </summary>
        /// <returns>The default quota cap value</returns>
        public int GetDefaultQuotaCap()
        {
            return ConfigConstants.DefaultQuotaCap;
        }

        /// <summary>
        /// Validates a quota value
        /// </summary>
        /// <param name="quota">The quota value to validate</param>
        /// <returns>True if the quota value is valid, false otherwise</returns>
        public bool ValidateQuotaValue(int quota)
        {
            try
            {
                // Quota should be positive or zero (unless it's the disabled value)
                if (quota == ConfigConstants.DisabledQuotaCapValue)
                {
                    return true; // -1 is valid for disabled caps
                }

                if (quota < 0)
                {
                    _loggingService.LogWarning($"Invalid quota value detected: {quota}. Quota should be non-negative.");
                    return false;
                }

                // Check for reasonable upper bounds (e.g., prevent extremely high values)
                const int maxReasonableQuota = 1000000; // 1 million
                if (quota > maxReasonableQuota)
                {
                    _loggingService.LogWarning($"Quota value {quota} seems unusually high. Consider reviewing configuration.");
                    return true; // Still valid, but warn
                }

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error validating quota value: {quota}", ex);
                return false;
            }
        }

        /// <summary>
        /// Applies the quota cap logic to determine the final quota value
        /// </summary>
        /// <param name="currentQuota">The current quota value</param>
        /// <param name="cap">The cap value to apply</param>
        /// <param name="enabled">Whether the cap is enabled</param>
        /// <param name="constellation">The constellation name for logging</param>
        /// <returns>The final quota value after applying cap logic</returns>
        private int ApplyQuotaCapLogic(int currentQuota, int cap, bool enabled, string constellation)
        {
            // If the cap is disabled, do nothing
            if (!enabled)
            {
                _loggingService.LogDebug($"Quota cap disabled for constellation '{constellation}'");
                return currentQuota;
            }

            // If the cap is disabled (-1), do nothing
            if (cap == ConfigConstants.DisabledQuotaCapValue)
            {
                _loggingService.LogDebug($"Quota cap disabled (-1) for constellation '{constellation}'");
                return currentQuota;
            }

            // Validate the cap value
            if (!ValidateQuotaValue(cap))
            {
                _loggingService.LogWarning($"Invalid cap value {cap} for constellation '{constellation}', using default");
                cap = GetDefaultQuotaCap();
            }

            // Apply the cap - if quota is above cap, set it to cap
            if (currentQuota > cap)
            {
                _loggingService.LogDebug($"Quota ({currentQuota}) exceeds cap ({cap}) for constellation '{constellation}', applying cap");
                return cap;
            }

            // Quota is within cap, return as-is
            _loggingService.LogDebug($"Quota ({currentQuota}) is within cap ({cap}) for constellation '{constellation}'");
            return currentQuota;
        }

        /// <summary>
        /// Gets the current quota cap status for all constellations
        /// </summary>
        /// <returns>A dictionary containing constellation names and their cap status</returns>
        public System.Collections.Generic.Dictionary<string, (int Cap, bool Enabled)> GetQuotaCapStatus()
        {
            var status = new System.Collections.Generic.Dictionary<string, (int Cap, bool Enabled)>();

            try
            {
                foreach (var constellation in _configManager.ConstellationCaps.Keys)
                {
                    int cap = GetConstellationCap(constellation);
                    bool enabled = IsQuotaCapEnabled(constellation);
                    status[constellation] = (cap, enabled);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error getting quota cap status", ex);
            }

            return status;
        }

        /// <summary>
        /// Resets all quota caps to their default values
        /// </summary>
        /// <returns>True if reset was successful, false otherwise</returns>
        public bool ResetAllQuotaCapsToDefaults()
        {
            try
            {
                _loggingService.LogInfo("Resetting all quota caps to default values");

                foreach (var constellation in _configManager.ConstellationCaps.Keys)
                {
                    if (_configManager.ConstellationCaps.TryGetValue(constellation, out var capEntry))
                    {
                        capEntry.Value = ConfigConstants.DefaultQuotaCap;
                        _loggingService.LogDebug($"Reset cap for constellation '{constellation}' to {ConfigConstants.DefaultQuotaCap}");
                    }

                    if (_configManager.ConstellationCapEnabled.TryGetValue(constellation, out var enabledEntry))
                    {
                        enabledEntry.Value = true;
                        _loggingService.LogDebug($"Reset enabled state for constellation '{constellation}' to true");
                    }
                }

                _configManager.SaveConfiguration();
                _loggingService.LogInfo("All quota caps reset to default values successfully");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error resetting quota caps to defaults", ex);
                return false;
            }
        }
    }
}