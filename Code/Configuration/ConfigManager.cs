using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace DynamicQuotaCap.Configuration
{
    /// <summary>
    /// Centralized configuration management for the Dynamic Quota Cap plugin
    /// </summary>
    public class ConfigManager
    {
        private readonly ConfigFile _configFile;
        private readonly ManualLogSource _logger;
        private bool _saveOnConfigSet = true;

        /// <summary>
        /// Dictionary to store cap values for each constellation
        /// </summary>
        public Dictionary<string, ConfigEntry<int>> ConstellationCaps { get; } = new Dictionary<string, ConfigEntry<int>>();

        /// <summary>
        /// Dictionary to store enable status for each constellation
        /// </summary>
        public Dictionary<string, ConfigEntry<bool>> ConstellationCapEnabled { get; } = new Dictionary<string, ConfigEntry<bool>>();

        /// <summary>
        /// Debug mode configuration entry
        /// </summary>
        public ConfigEntry<bool> EnableDebug { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ConfigManager class
        /// </summary>
        /// <param name="configFile">The BepInEx configuration file</param>
        /// <param name="logger">The logger instance</param>
        public ConfigManager(ConfigFile configFile, ManualLogSource logger)
        {
            _configFile = configFile ?? throw new ArgumentNullException(nameof(configFile));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads all configuration entries
        /// </summary>
        public void LoadConfiguration()
        {
            try
            {
                _logger.LogInfo("Loading configuration...");

                // Load general settings
                LoadGeneralSettings();

                _logger.LogInfo("Configuration loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading configuration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves the configuration to disk
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                _configFile.Save();
                _logger.LogInfo("Configuration saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving configuration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reloads the configuration from disk
        /// </summary>
        public void ReloadConfiguration()
        {
            try
            {
                _configFile.Reload();
                _logger.LogInfo("Configuration reloaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reloading configuration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets or sets whether to automatically save configuration when values change
        /// </summary>
        public bool SaveOnConfigSet
        {
            get => _saveOnConfigSet;
            set
            {
                _saveOnConfigSet = value;
                _configFile.SaveOnConfigSet = value;
            }
        }

        /// <summary>
        /// Gets a configuration entry of the specified type
        /// </summary>
        /// <typeparam name="T">The type of the configuration value</typeparam>
        /// <param name="section">The configuration section</param>
        /// <param name="key">The configuration key</param>
        /// <param name="defaultValue">The default value</param>
        /// <param name="description">The description of the configuration entry</param>
        /// <returns>The configuration entry</returns>
        public ConfigEntry<T> GetConfigEntry<T>(string section, string key, T defaultValue, string description)
        {
            try
            {
                return _configFile.Bind(section, key, defaultValue, description);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting config entry '{section}.{key}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validates the current configuration
        /// </summary>
        /// <returns>True if configuration is valid, false otherwise</returns>
        public bool ValidateConfig()
        {
            try
            {
                _logger.LogInfo("Validating configuration...");

                // Validate debug setting
                if (EnableDebug == null)
                {
                    _logger.LogError("Debug configuration entry is null");
                    return false;
                }

                // Validate constellation caps
                foreach (var capEntry in ConstellationCaps)
                {
                    if (capEntry.Value == null)
                    {
                        _logger.LogError($"Constellation cap entry for '{capEntry.Key}' is null");
                        return false;
                    }

                    if (capEntry.Value.Value < ConfigConstants.DisabledQuotaCapValue)
                    {
                        _logger.LogWarning($"Constellation cap value for '{capEntry.Key}' is invalid: {capEntry.Value.Value}");
                    }
                }

                // Validate constellation enabled states
                foreach (var enabledEntry in ConstellationCapEnabled)
                {
                    if (enabledEntry.Value == null)
                    {
                        _logger.LogError($"Constellation enabled entry for '{enabledEntry.Key}' is null");
                        return false;
                    }
                }

                _logger.LogInfo("Configuration validation completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates configuration entries for a constellation
        /// </summary>
        /// <param name="constellationName">The name of the constellation</param>
        /// <param name="constellationWord">The constellation word (e.g., "constellation" or "sector")</param>
        public void CreateConstellationConfigEntries(string constellationName, string constellationWord)
        {
            try
            {
                if (string.IsNullOrEmpty(constellationName))
                {
                    _logger.LogError("Constellation name cannot be null or empty");
                    return;
                }

                if (string.IsNullOrEmpty(constellationWord))
                {
                    constellationWord = ConfigConstants.DefaultConstellationWord;
                }

                // Create config entry for cap value
                var capKey = string.Format(ConfigConstants.Keys.ConstellationCapFormat, constellationName);
                var capDescription = string.Format(ConfigConstants.Descriptions.ConstellationCap, constellationName, constellationWord.ToLower());
                
                ConstellationCaps[constellationName] = GetConfigEntry<int>(
                    ConfigConstants.Sections.QuotaCaps,
                    capKey,
                    ConfigConstants.DefaultQuotaCap,
                    capDescription
                );

                // Create config entry for enabling/disabling the cap
                var enabledKey = string.Format(ConfigConstants.Keys.ConstellationEnabledFormat, constellationName);
                var enabledDescription = string.Format(ConfigConstants.Descriptions.ConstellationEnabled, constellationName, constellationWord.ToLower());
                
                ConstellationCapEnabled[constellationName] = GetConfigEntry<bool>(
                    ConfigConstants.Sections.QuotaCapToggles,
                    enabledKey,
                    true,
                    enabledDescription
                );

                _logger.LogInfo($"Created configuration entries for constellation: {constellationName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating constellation config entries for '{constellationName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Loads general configuration settings
        /// </summary>
        private void LoadGeneralSettings()
        {
            EnableDebug = GetConfigEntry<bool>(
                ConfigConstants.Sections.General,
                ConfigConstants.Keys.EnableDebug,
                ConfigConstants.DefaultDebugEnabled,
                ConfigConstants.Descriptions.EnableDebug
            );
        }
    }
}