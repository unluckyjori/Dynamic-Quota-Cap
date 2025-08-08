using System;
using HarmonyLib;
using DynamicQuotaCap.Services;
using DynamicQuotaCap.Configuration;

namespace DynamicQuotaCap.Patches
{
    /// <summary>
    /// Harmony patch for modifying TimeOfDay.SetNewProfitQuota method to apply quota caps
    /// </summary>
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        private static QuotaCapService _quotaCapService;
        private static LoggingService _loggingService;

        /// <summary>
        /// Initializes the patch with required services
        /// </summary>
        /// <param name="quotaCapService">The quota cap service instance</param>
        /// <param name="loggingService">The logging service instance</param>
        public static void Initialize(QuotaCapService quotaCapService, LoggingService loggingService)
        {
            _quotaCapService = quotaCapService ?? throw new ArgumentNullException(nameof(quotaCapService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            
            _loggingService.LogInfo("TimeOfDayPatch initialized with services");
        }

        /// <summary>
        /// Postfix patch for TimeOfDay.SetNewProfitQuota method
        /// </summary>
        /// <param name="___profitQuota">Reference to the profit quota field (passed by ref)</param>
        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPostfix]
        private static void SetNewProfitQuotaPostfix(ref int ___profitQuota)
        {
            try
            {
                // Check if services are available
                if (_quotaCapService == null || _loggingService == null)
                {
                    Console.WriteLine($"[DynamicQuotaCap] {PatchConstants.ErrorMessages.QuotaCapServiceNotAvailable}");
                    return;
                }

                _loggingService.LogMethodStart(nameof(SetNewProfitQuotaPostfix), new { originalQuota = ___profitQuota });

                // Get the current constellation from LethalConstellations
                string currentConstellation = LethalConstellations.PluginCore.Collections.CurrentConstellation;
                
                _loggingService.LogDebug($"Applying quota cap logic - Current constellation: '{currentConstellation}', New quota: {___profitQuota}");

                // Apply the quota cap
                int cappedQuota = _quotaCapService.CalculateQuotaCap(currentConstellation, ___profitQuota);
                
                // Update the quota if it was capped
                if (cappedQuota != ___profitQuota)
                {
                    _loggingService.LogInfo($"Quota modified by patch: {___profitQuota} -> {cappedQuota}");
                    ___profitQuota = cappedQuota;
                }

                _loggingService.LogMethodEnd(nameof(SetNewProfitQuotaPostfix), ___profitQuota);
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking the game
                string errorMessage = $"Exception in TimeOfDayPatch: {ex.Message}";
                Console.WriteLine($"[DynamicQuotaCap] {errorMessage}");
                
                _loggingService?.LogError(errorMessage, ex);
                
                // Try to log inner exception if present
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[DynamicQuotaCap] Inner exception: {ex.InnerException.Message}");
                    _loggingService?.LogError($"Inner exception in TimeOfDayPatch: {ex.InnerException.Message}", ex.InnerException);
                }
            }
        }

        /// <summary>
        /// Gets the current patch status
        /// </summary>
        /// <returns>True if patch is properly initialized, false otherwise</returns>
        public static bool IsPatchInitialized()
        {
            return _quotaCapService != null && _loggingService != null;
        }

        /// <summary>
        /// Gets the current constellation name for debugging purposes
        /// </summary>
        /// <returns>The current constellation name or "Unknown" if not available</returns>
        public static string GetCurrentConstellationForDebug()
        {
            try
            {
                return LethalConstellations.PluginCore.Collections.CurrentConstellation ?? "Unknown";
            }
            catch
            {
                return "Error";
            }
        }

        /// <summary>
        /// Validates that the patch can be applied
        /// </summary>
        /// <returns>True if patch can be applied, false otherwise</returns>
        public static bool ValidatePatchPrerequisites()
        {
            try
            {
                // Check if TimeOfDay class exists
                var timeOfDayType = typeof(TimeOfDay);
                if (timeOfDayType == null)
                {
                    Console.WriteLine($"[DynamicQuotaCap] {PatchConstants.ErrorMessages.TimeOfDayClassNotFound}");
                    return false;
                }

                // Check if SetNewProfitQuota method exists
                var setNewProfitQuotaMethod = timeOfDayType.GetMethod("SetNewProfitQuota");
                if (setNewProfitQuotaMethod == null)
                {
                    Console.WriteLine($"[DynamicQuotaCap] {PatchConstants.ErrorMessages.SetNewProfitQuotaMethodNotFound}");
                    return false;
                }

                // Check if LethalConstellations is available
                var collectionsType = typeof(LethalConstellations.PluginCore.Collections);
                if (collectionsType == null)
                {
                    Console.WriteLine("[DynamicQuotaCap] LethalConstellations.Collections not found");
                    return false;
                }

                // Check if CurrentConstellation property exists
                var currentConstellationProperty = collectionsType.GetProperty("CurrentConstellation");
                if (currentConstellationProperty == null)
                {
                    Console.WriteLine("[DynamicQuotaCap] CurrentConstellation property not found in LethalConstellations.Collections");
                    return false;
                }

                _loggingService?.LogInfo("TimeOfDay patch prerequisites validated successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DynamicQuotaCap] Error validating patch prerequisites: {ex.Message}");
                _loggingService?.LogError($"Error validating patch prerequisites: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Applies the patch manually (for testing purposes)
        /// </summary>
        /// <param name="harmony">The Harmony instance</param>
        /// <returns>True if patch was applied successfully, false otherwise</returns>
        public static bool ApplyPatchManually(Harmony harmony)
        {
            try
            {
                if (!ValidatePatchPrerequisites())
                {
                    return false;
                }

                if (!IsPatchInitialized())
                {
                    Console.WriteLine($"[DynamicQuotaCap] Cannot apply patch: services not initialized");
                    return false;
                }

                // The patch is applied automatically via Harmony attributes, but this method
                // can be used for manual application or testing
                _loggingService?.LogPatchApplication(PatchConstants.TimeOfDayPatchId, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DynamicQuotaCap] {PatchConstants.ErrorMessages.PatchApplicationFailed}: {ex.Message}");
                _loggingService?.LogError($"{PatchConstants.ErrorMessages.PatchApplicationFailed}: {ex.Message}", ex);
                return false;
            }
        }
    }
}