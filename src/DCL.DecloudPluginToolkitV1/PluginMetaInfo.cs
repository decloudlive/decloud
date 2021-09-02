using DCL.Common.Enums;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1
{
    public class PluginMetaInfo
    {
        public string PluginDescription { get; set; }
        public Dictionary<DeviceType, List<AlgorithmType>> SupportedDevicesAlgorithms { get; set; }
    }
}
