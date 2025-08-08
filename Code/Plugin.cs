using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using DynamicQuotaCap.Configuration;
using DynamicQuotaCap.Services;
using DynamicQuotaCap.Patches;

namespace DynamicQuotaCap
{
    [BepInPlugin("com.example.dynamicquotacap", "Dynamic Quota Cap", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony;
        private ConfigManager _configManager;
        private LoggingService _loggingService;
        private QuotaCapService _quotaCapService;
        private ConstellationConfigGenerator _constellationConfigGenerator;

        private void Awake()
        {
            // Initialize logging first
            _configManager = new ConfigManager(Config, Logger);
            _loggingService = new LoggingService(Logger, _configManager);
            
            _loggingService.LogInfo("Initializing Dynamic Quota Cap plugin");

            try
            {
                // Load configuration
                _configManager.LoadConfiguration();

                // Initialize services
                _quotaCapService = new QuotaCapService(_configManager, _loggingService);
                _constellationConfigGenerator = new ConstellationConfigGenerator(_configManager, Logger);

                // Initialize patches with services
                TimeOfDayPatch.Initialize(_quotaCapService, _loggingService);

                // Validate patch prerequisites
                if (!TimeOfDayPatch.ValidatePatchPrerequisites())
                {
                    _loggingService.LogError("Failed to validate patch prerequisites. Some functionality may not work.");
                }

                // Apply Harmony patches
                _harmony = new Harmony("com.example.dynamicquotacap");
                _harmony.PatchAll();
                _loggingService.LogInfo("Harmony patches applied");

                // Generate constellation configs from LethalConstellations config file
                GenerateConstellationConfigs();

                _loggingService.LogInfo("Dynamic Quota Cap plugin initialized successfully");
            }
            catch (System.Exception ex)
            {
                _loggingService.LogError($"Failed to initialize plugin: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates constellation configurations using the config generator
        /// </summary>
        private void GenerateConstellationConfigs()
        {
            try
            {
                _loggingService.LogMethodStart(nameof(GenerateConstellationConfigs));
                
                bool success = _constellationConfigGenerator.GenerateConstellationConfigs();
                
                if (success)
                {
                    _loggingService.LogConfigGeneration(
                        _configManager.ConstellationCaps.Count, 
                        true
                    );
                }
                else
                {
                    _loggingService.LogConfigGeneration(
                        _configManager.ConstellationCaps.Count, 
                        false, 
                        "Initial generation failed, retry may be attempted"
                    );
                    
                    // Try immediate retry if possible
                    if (_constellationConfigGenerator.CanRetry())
                    {
                        _loggingService.LogInfo("Attempting immediate retry for constellation config generation");
                        success = _constellationConfigGenerator.ImmediateRetry();
                        
                        if (success)
                        {
                            _loggingService.LogConfigGeneration(
                                _configManager.ConstellationCaps.Count, 
                                true
                            );
                        }
                        else
                        {
                            _loggingService.LogError("Failed to generate constellation configs after retry");
                        }
                    }
                }
                
                _loggingService.LogMethodEnd(nameof(GenerateConstellationConfigs), success);
            }
            catch (System.Exception ex)
            {
                _loggingService.LogError($"Error in GenerateConstellationConfigs: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Public method for manual configuration generation trigger
        /// </summary>
        public void GenerateConfigs()
        {
            GenerateConstellationConfigs();
        }

        /// <summary>
        /// Gets the current configuration manager (for debugging/external access)
        /// </summary>
        /// <returns>The configuration manager instance</returns>
        public ConfigManager GetConfigManager()
        {
            return _configManager;
        }

        /// <summary>
        /// Gets the current quota cap service (for debugging/external access)
        /// </summary>
        /// <returns>The quota cap service instance</returns>
        public QuotaCapService GetQuotaCapService()
        {
            return _quotaCapService;
        }

        /// <summary>
        /// Gets the current logging service (for debugging/external access)
        /// </summary>
        /// <returns>The logging service instance</returns>
        public LoggingService GetLoggingService()
        {
            return _loggingService;
        }

        /// <summary>
        /// Gets the current constellation config generator (for debugging/external access)
        /// </summary>
        /// <returns>The constellation config generator instance</returns>
        public ConstellationConfigGenerator GetConstellationConfigGenerator()
        {
            return _constellationConfigGenerator;
        }

        /// <summary>
        /// Cleanup method called when the plugin is destroyed
        /// </summary>
        private void OnDestroy()
        {
            try
            {
                _loggingService?.LogInfo("Dynamic Quota Cap plugin shutting down");
                
                // Cleanup Harmony patches
                _harmony?.UnpatchSelf();
                
                // Save configuration one last time
                _configManager?.SaveConfiguration();
                
                _loggingService?.LogInfo("Dynamic Quota Cap plugin shutdown complete");
            }
            catch (System.Exception ex)
            {
                // Don't use logging service here as it might be disposed
                Logger?.LogError($"Error during plugin shutdown: {ex.Message}");
            }
        }
    }
}