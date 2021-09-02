using Newtonsoft.Json;
using DCL.Common.Configs;
using System;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.Configs
{
    /// <summary>
    /// DecloudReservedPorts class is used to reserve specific ports for each algorithm
    /// </summary>
    /// <jsonSerializationExample>
    /// {
    ///     "use_user_settings": "true",
    ///     "algorithm_reserved_ports": {
    ///         "Beam": [4001, 4002],
    ///         "CuckooCycle": [4005, 4010]
    ///      }
    /// }
    /// </jsonSerializationExample>
    [Serializable]
    public class DecloudReservedPorts : IInternalSetting
    {
        [JsonProperty("use_user_settings")]
        public bool UseUserSettings { get; set; } = false;

        /// <summary>
        /// AlgorithmReservedPorts is a Dictionary with AlgorithmName for key and list of ports for value
        /// </summary>
        [JsonProperty("algorithm_reserved_ports")]
        public Dictionary<string, List<int>> AlgorithmReservedPorts { get; set; } = null;
    }
}
