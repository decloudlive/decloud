using Newtonsoft.Json;
using DCL.Common.Configs;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.Configs
{
    public class BenchmarkLineExceptions : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        [JsonProperty("benchmark_line_message_exceptions")]
        public Dictionary<string, string> BenchmarkLineMessageExceptions = new Dictionary<string, string> { };
    }
}
