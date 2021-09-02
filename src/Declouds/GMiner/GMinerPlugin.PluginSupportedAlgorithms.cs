using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = DCL.DecloudPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace GDecloudPlugin
{
    public partial class GDecloudPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            // fee fixed
            DefaultFee = 2.0,
            AlgorithmFees = new Dictionary<AlgorithmType, double>
            {
                { AlgorithmType.KAWPOW, 1.0 },
            },
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.ZHash),
                        new SAS(AlgorithmType.GrinCuckatoo31),
                        new SAS(AlgorithmType.CuckooCycle) {Enabled = false }, //~5% of invalid nonce shares,
                        new SAS(AlgorithmType.GrinCuckatoo32),
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.BeamV3),
                        new SAS(AlgorithmType.DaggerHashimoto)
                    }
                },
                {
                    DeviceType.AMD,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.CuckooCycle) {Enabled = false }, //~5% of invalid nonce shares
                        new SAS(AlgorithmType.DaggerHashimoto)
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.ZHash, "144_5" },
                { AlgorithmType.GrinCuckatoo31, "grin31" },
                { AlgorithmType.CuckooCycle, "cuckoo29" },
                { AlgorithmType.GrinCuckatoo32, "cuckatoo32" },
                { AlgorithmType.KAWPOW, "kawpow" },
                { AlgorithmType.BeamV3, "beamhashIII" },
                { AlgorithmType.DaggerHashimoto, "ethash" }
            }
        };
    }
}
