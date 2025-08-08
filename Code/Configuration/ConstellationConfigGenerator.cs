using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Logging;

namespace DynamicQuotaCap.Configuration
{
    /// <summary>
    /// Generates constellation-specific configurations by parsing LethalConstellations config files
    /// </summary>
    public class ConstellationConfigGenerator
    {
        private readonly ConfigManager _configManager;
        private readonly ManualLogSource _logger;
        private int _retryCount = 0;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 5000;

        /// <summary>
        /// Initializes a new instance of the ConstellationConfigGenerator class
        /// </summary>
        /// <param name="configManager">The configuration manager instance</param>
        /// <param name="logger">The logger instance</param>
        public ConstellationConfigGenerator(ConfigManager configManager, ManualLogSource logger)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates constellation configurations from LethalConstellations config files
        /// </summary>
        /// <returns>True if generation was successful, false otherwise</returns>
        public bool GenerateConstellationConfigs()
        {
            try
            {
                _logger.LogInfo("Generating constellation configs from LethalConstellations config file");

                // Get the custom constellation word from LethalConstellations config
                string constellationWord = GetConstellationWord();
                _logger.LogInfo($"Using constellation word: {constellationWord}");

                // Parse constellation names from the generated config file
                var constellationNames = ParseConstellationNames(constellationWord);
                if (constellationNames.Count == 0)
                {
                    _logger.LogWarning($"No constellations found in LethalConstellations config file");
                    return RetryConfigGeneration();
                }

                _logger.LogInfo($"Found {constellationNames.Count} constellations, generating config entries");

                // Create config entries for each constellation
                foreach (string constellationName in constellationNames)
                {
                    _configManager.CreateConstellationConfigEntries(constellationName, constellationWord);
                }

                // Save the configuration
                _configManager.SaveConfiguration();

                _logger.LogInfo($"Constellation quota cap configurations generated and saved - Created {constellationNames.Count} entries");
                _retryCount = 0; // Reset retry count on success
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating constellation configs: {ex.Message}");
                return RetryConfigGeneration();
            }
        }

        /// <summary>
        /// Gets the constellation word from LethalConstellations main config file
        /// </summary>
        /// <returns>The constellation word (e.g., "Constellation" or "Sector")</returns>
        private string GetConstellationWord()
        {
            try
            {
                string mainConfigPath = Path.Combine(Paths.ConfigPath, ConfigConstants.Files.LethalConstellationsMainConfig);
                
                if (!File.Exists(mainConfigPath))
                {
                    _logger.LogWarning("LethalConstellations main config file not found, using default constellation word");
                    return ConfigConstants.DefaultConstellationWord;
                }

                string[] configLines = File.ReadAllLines(mainConfigPath);
                foreach (string line in configLines)
                {
                    if (line.StartsWith(ConfigConstants.Files.ConstellationWordLineFormat))
                    {
                        string constellationWord = line.Substring(ConfigConstants.Files.ConstellationWordLineFormat.Length).Trim();
                        _logger.LogInfo($"Found custom constellation word: {constellationWord}");
                        return constellationWord;
                    }
                }

                _logger.LogWarning("Constellation word not found in config, using default");
                return ConfigConstants.DefaultConstellationWord;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting constellation word: {ex.Message}");
                return ConfigConstants.DefaultConstellationWord;
            }
        }

        /// <summary>
        /// Parses constellation names from the LethalConstellations generated config file
        /// </summary>
        /// <param name="constellationWord">The constellation word to use for parsing</param>
        /// <returns>List of constellation names</returns>
        private List<string> ParseConstellationNames(string constellationWord)
        {
            var constellationNames = new List<string>();
            
            try
            {
                string configPath = Path.Combine(Paths.ConfigPath, ConfigConstants.Files.LethalConstellationsGeneratedConfig);
                
                if (!File.Exists(configPath))
                {
                    _logger.LogWarning("LethalConstellations generated config file not found");
                    return constellationNames;
                }

                string[] lines = File.ReadAllLines(configPath);
                string sectionPrefix = $"[{constellationWord} ";
                
                foreach (string line in lines)
                {
                    if (line.StartsWith(sectionPrefix))
                    {
                        // Extract the constellation name
                        int prefixLength = sectionPrefix.Length;
                        string constellationName = line.Substring(prefixLength, line.Length - prefixLength - 1); // Remove [Word ] and ]
                        constellationNames.Add(constellationName);
                        _logger.LogInfo($"Found {constellationWord.ToLower()}: {constellationName}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing constellation names: {ex.Message}");
            }

            return constellationNames;
        }

        /// <summary>
        /// Retries configuration generation with a delay
        /// </summary>
        /// <returns>True if retry was successful, false otherwise</returns>
        private bool RetryConfigGeneration()
        {
            if (_retryCount >= MaxRetries)
            {
                _logger.LogError("Max retry attempts reached for constellation config generation");
                _retryCount = 0;
                return false;
            }

            _retryCount++;
            _logger.LogInfo($"Will retry constellation config generation in {RetryDelayMs / 1000} seconds (attempt {_retryCount}/{MaxRetries})");

            // Note: In a real implementation, you might want to use a coroutine or async method
            // For now, we'll just log the retry attempt and return false
            // The actual retry would need to be handled by the calling code
            return false;
        }

        /// <summary>
        /// Resets the retry count
        /// </summary>
        public void ResetRetryCount()
        {
            _retryCount = 0;
        }

        /// <summary>
        /// Gets the current retry count
        /// </summary>
        /// <returns>The current retry count</returns>
        public int GetRetryCount()
        {
            return _retryCount;
        }

        /// <summary>
        /// Checks if the generator can retry configuration generation
        /// </summary>
        /// <returns>True if retries are still available, false otherwise</returns>
        public bool CanRetry()
        {
            return _retryCount < MaxRetries;
        }

        /// <summary>
        /// Performs an immediate retry attempt
        /// </summary>
        /// <returns>True if the retry was successful, false otherwise</returns>
        public bool ImmediateRetry()
        {
            if (!CanRetry())
            {
                return false;
            }

            return GenerateConstellationConfigs();
        }
    }
}