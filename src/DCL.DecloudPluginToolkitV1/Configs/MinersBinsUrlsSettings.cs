using Newtonsoft.Json;
using DCL.Common.Configs;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.Configs
{
    public class DecloudBinsUrlsSettings : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("bin_version")]
        public string BinVersion { get; set; } = null;

        // ExePath this should be enough for both cwd and bin path
        [JsonProperty("bin_path")]
        public List<string> ExePath { get; set; } = null;

        [JsonProperty("bins_urls")]
        public List<string> Urls { get; set; } = null;

        [JsonProperty("bins_package_pwd")]
        public string BinsPackagePassword { get; set; } = null;
    }
}
