using Newtonsoft.Json;
using DCL.Common.Configs;
using System;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.ExtraLaunchParameters
{
    /// <summary>
    /// DecloudOptionsPackage combines General and Temperature options (both of type DecloudOption<see cref="DecloudOption"/>)
    /// With UseUserSettings property user can define if Decloud options should be used from local DecloudOptionsPackage.json file
    /// </summary>
    [Serializable]
    public class DecloudOptionsPackage : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("group_mining_pairs_only_with_compatible_options")]
        public bool GroupMiningPairsOnlyWithCompatibleOptions { get; set; } = true;

        [JsonProperty("ignore_default_value_options")]
        public bool IgnoreDefaultValueOptions { get; set; } = true;

        [JsonProperty("general_options")]
        public List<DecloudOption> GeneralOptions { get; set; }

        [JsonProperty("temperature_options")]
        public List<DecloudOption> TemperatureOptions { get; set; }
    }
}
