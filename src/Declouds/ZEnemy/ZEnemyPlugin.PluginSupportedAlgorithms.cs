﻿using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using System.Collections.Generic;
using SAS = DCL.DecloudPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace ZEnemy
{
    public partial class ZEnemyPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 1.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 }
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.KAWPOW, "kawpow" }
            }
        };
    }
}
