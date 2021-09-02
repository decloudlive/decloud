using Newtonsoft.Json;
using DCL.Common.Configs;
using System;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.Configs
{
    [Serializable]
    public class DecloudCustomActionSettings : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        public class DecloudCustomActionEntry
        {
            [JsonProperty("start")]
            public string StartExePath { get; set; }
            [JsonProperty("wait_start_exec")]
            public bool StartExePathWaitExec { get; set; }
            [JsonProperty("stop")]
            public string StopExePath { get; set; }
            [JsonProperty("wait_stop_exec")]
            public bool StopExePathWaitExec { get; set; }
        }


        [JsonProperty("algorithm_custom_actions")]
        public Dictionary<string, DecloudCustomActionEntry> AlgorithmCustomActions { get; set; } = null;
    }
}
