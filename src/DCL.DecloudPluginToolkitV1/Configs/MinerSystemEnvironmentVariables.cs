﻿using Newtonsoft.Json;
using DCL.Common.Configs;
using System;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.Configs
{
    /// <summary>
    /// DecloudystemEnvironmentVariables class is used to define system environment variables in Decloud plugin
    /// </summary>
    /// <example>            
    /// DefaultSystemEnvironmentVariables = new Dictionary<string, string>()
    /// {
    ///     {"GPU_MAX_ALLOC_PERCENT", "100"},
    ///     {"GPU_USE_SYNC_OBJECTS", "1"},
    ///     {"GPU_SINGLE_ALLOC_PERCENT", "100"},
    ///     {"GPU_MAX_HEAP_SIZE", "100"},
    ///     {"GPU_FORCE_64BIT_PTR", "1"}
    /// }
    /// </example>
    [Serializable]
    public class DecloudystemEnvironmentVariables : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("default_system_environment_variables")]
        public Dictionary<string, string> DefaultSystemEnvironmentVariables { get; set; } = null;

        [JsonProperty("custom_system_environment_variables")]
        public Dictionary<string, Dictionary<string, string>> CustomSystemEnvironmentVariables { get; set; } = null;
    }
}
