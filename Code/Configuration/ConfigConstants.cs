using BepInEx.Configuration;

namespace DynamicQuotaCap.Configuration
{
    /// <summary>
    /// Configuration constants and default values for the Dynamic Quota Cap plugin
    /// </summary>
    public static class ConfigConstants
    {
        /// <summary>
        /// Default value for debug mode enabled
        /// </summary>
        public const bool DefaultDebugEnabled = true;
        
        /// <summary>
        /// Default quota cap value when no specific constellation config exists
        /// </summary>
        public const int DefaultQuotaCap = 4000;
        
        /// <summary>
        /// Value used to disable quota cap (-1 means no cap)
        /// </summary>
        public const int DisabledQuotaCapValue = -1;
        
        /// <summary>
        /// Default constellation word used in LethalConstellations config
        /// </summary>
        public const string DefaultConstellationWord = "Constellation";
        
        /// <summary>
        /// Configuration section names
        /// </summary>
        public static class Sections
        {
            /// <summary>
            /// General configuration section
            /// </summary>
            public const string General = "General";
            
            /// <summary>
            /// Quota caps configuration section
            /// </summary>
            public const string QuotaCaps = "Quota Caps";
            
            /// <summary>
            /// Quota cap toggles configuration section
            /// </summary>
            public const string QuotaCapToggles = "Quota Cap Toggles";
        }
        
        /// <summary>
        /// Configuration key names
        /// </summary>
        public static class Keys
        {
            /// <summary>
            /// Key for debug mode setting
            /// </summary>
            public const string EnableDebug = "EnableDebug";
            
            /// <summary>
            /// Key format for constellation cap values
            /// </summary>
            public const string ConstellationCapFormat = "{0}_Cap";
            
            /// <summary>
            /// Key format for constellation cap enabled state
            /// </summary>
            public const string ConstellationEnabledFormat = "{0}_Enabled";
        }
        
        /// <summary>
        /// Configuration descriptions
        /// </summary>
        public static class Descriptions
        {
            /// <summary>
            /// Description for debug mode setting
            /// </summary>
            public const string EnableDebug = "Toggle the debug stuff";
            
            /// <summary>
            /// Description format for constellation cap values
            /// </summary>
            public const string ConstellationCap = "Quota cap for {0} {1}";
            
            /// <summary>
            /// Description format for constellation cap enabled state
            /// </summary>
            public const string ConstellationEnabled = "Enable quota cap for {0} {1}";
        }
        
        /// <summary>
        /// File names and paths
        /// </summary>
        public static class Files
        {
            /// <summary>
            /// LethalConstellations main config file name
            /// </summary>
            public const string LethalConstellationsMainConfig = "com.github.darmuh.LethalConstellations.cfg";
            
            /// <summary>
            /// LethalConstellations generated config file name
            /// </summary>
            public const string LethalConstellationsGeneratedConfig = "LethalConstellations_Generated.cfg";
            
            /// <summary>
            /// Config file line format for constellation word extraction
            /// </summary>
            public const string ConstellationWordLineFormat = "ConstellationWord = ";
        }
    }
}