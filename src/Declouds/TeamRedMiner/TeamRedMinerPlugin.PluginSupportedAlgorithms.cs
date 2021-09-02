using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = DCL.DecloudPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace TeamRedDecloud
{
    public partial class TeamRedDecloudPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 2.5,
            AlgorithmFees = new Dictionary<AlgorithmType, double>
            {
                { AlgorithmType.DaggerHashimoto, 1.0 },
                { AlgorithmType.KAWPOW, 2.0 }

            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.Lyra2REv3),
                        new SAS(AlgorithmType.GrinCuckatoo31),
                        new SAS(AlgorithmType.DaggerHashimoto),
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 }
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.Lyra2REv3, "lyra2rev3" },
                { AlgorithmType.GrinCuckatoo31, "cuckatoo31_grin" },
                { AlgorithmType.DaggerHashimoto, "ethash" },
                { AlgorithmType.KAWPOW, "kawpow" }
            }
        };
    }
}
