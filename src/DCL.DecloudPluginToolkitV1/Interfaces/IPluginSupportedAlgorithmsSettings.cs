using DCL.Common.Algorithm;
using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using System.Collections.Generic;

namespace DCL.DecloudPluginToolkitV1.Interfaces
{
    public interface IPluginSupportedAlgorithmsSettings
    {
        PluginSupportedAlgorithmsSettings PluginSupportedAlgorithmsSettings { get; }

        bool UnsafeLimits();

        Dictionary<DeviceType, List<AlgorithmType>> SupportedDevicesAlgorithmsDict();

        List<Algorithm> GetSupportedAlgorithmsForDeviceType(DeviceType deviceType);

        string AlgorithmName(params AlgorithmType[] algorithmTypes);
        double DevFee(params AlgorithmType[] algorithmTypes);
    }
}
