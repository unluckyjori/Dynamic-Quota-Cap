namespace DynamicQuotaCap.Patches
{
    /// <summary>
    /// Constants and configuration values for Harmony patches
    /// </summary>
    public static class PatchConstants
    {
        /// <summary>
        /// The name of the method to patch in TimeOfDay class
        /// </summary>
        public const string TimeOfDaySetNewProfitQuotaMethod = "SetNewProfitQuota";

        /// <summary>
        /// The full name of the TimeOfDay class
        /// </summary>
        public const string TimeOfDayClassName = "TimeOfDay";

        /// <summary>
        /// Harmony patch ID for the TimeOfDay patch
        /// </summary>
        public const string TimeOfDayPatchId = "com.example.dynamicquotacap.timedofday.patch";

        /// <summary>
        /// Description for the TimeOfDay patch
        /// </summary>
        public const string TimeOfDayPatchDescription = "Applies quota cap logic to profit quota setting";

        /// <summary>
        /// Default values for patch behavior
        /// </summary>
        public static class Defaults
        {
            /// <summary>
            /// Default delay before retrying a failed patch (in milliseconds)
            /// </summary>
            public const int PatchRetryDelayMs = 1000;

            /// <summary>
            /// Maximum number of patch retry attempts
            /// </summary>
            public const int MaxPatchRetries = 3;

            /// <summary>
            /// Default timeout for patch operations (in milliseconds)
            /// </summary>
            public const int PatchTimeoutMs = 5000;
        }

        /// <summary>
        /// Error messages for patch operations
        /// </summary>
        public static class ErrorMessages
        {
            /// <summary>
            /// Error message when TimeOfDay class is not found
            /// </summary>
            public const string TimeOfDayClassNotFound = "TimeOfDay class not found for patching";

            /// <summary>
            /// Error message when SetNewProfitQuota method is not found
            /// </summary>
            public const string SetNewProfitQuotaMethodNotFound = "SetNewProfitQuota method not found in TimeOfDay class";

            /// <summary>
            /// Error message when patch application fails
            /// </summary>
            public const string PatchApplicationFailed = "Failed to apply TimeOfDay patch";

            /// <summary>
            /// Error message when patch is already applied
            /// </summary>
            public const string PatchAlreadyApplied = "TimeOfDay patch is already applied";

            /// <summary>
            /// Error message when quota cap service is not available
            /// </summary>
            public const string QuotaCapServiceNotAvailable = "QuotaCapService is not available for patch execution";
        }

        /// <summary>
        /// Log categories for patch operations
        /// </summary>
        public static class LogCategories
        {
            /// <summary>
            /// Log category for patch application
            /// </summary>
            public const string PatchApplication = "PatchApplication";

            /// <summary>
            /// Log category for patch execution
            /// </summary>
            public const string PatchExecution = "PatchExecution";

            /// <summary>
            /// Log category for patch errors
            /// </summary>
            public const string PatchError = "PatchError";

            /// <summary>
            /// Log category for quota cap operations
            /// </summary>
            public const string QuotaCap = "QuotaCap";
        }

        /// <summary>
        /// Parameter names for the SetNewProfitQuota method
        /// </summary>
        public static class Parameters
        {
            /// <summary>
            /// Name of the profit quota parameter in SetNewProfitQuota method
            /// </summary>
            public const string ProfitQuota = "profitQuota";

            /// <summary>
            /// Name of the profit quota field reference in TimeOfDay class
            /// </summary>
            public const string ProfitQuotaField = "___profitQuota";
        }
    }
}