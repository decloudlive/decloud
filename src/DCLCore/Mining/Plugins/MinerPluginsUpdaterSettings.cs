using Newtonsoft.Json;
using DCL.Common;
using DCL.Common.Configs;
using System;

namespace DCLCore.Mining.Plugins
{
    internal static class DecloudPluginsUpdaterSettings
    {
        private class SupportedAlgorithmsFilterSettingsFile : IInternalSetting
        {
            [JsonProperty("use_user_settings")]
            public bool UseUserSettings { get; set; } = false;

            [JsonProperty("check_plugins_interval")]
            public TimeSpan CheckPluginsInterval = TimeSpan.FromMinutes(30);
        }

        static readonly SupportedAlgorithmsFilterSettingsFile _settings;

        static DecloudPluginsUpdaterSettings()
        {
            (_settings, _) = InternalConfigs.GetDefaultOrFileSettings(Paths.InternalsPath("DecloudPluginsUpdaterSettings.json"), new SupportedAlgorithmsFilterSettingsFile());
        }

        public static TimeSpan CheckPluginsInterval => _settings.CheckPluginsInterval;
    }
}
