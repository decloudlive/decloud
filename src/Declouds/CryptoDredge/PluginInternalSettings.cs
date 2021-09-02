using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace CryptoDredge
{
    internal static class PluginInternalSettings
    {
        internal static DecloudOptionsPackage DecloudOptionsPackage = new DecloudOptionsPackage
        {
            GeneralOptions = new List<DecloudOption>
            {
                /// <summary>
                /// Mining intensity (0 - 6). (default: 6)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "cryptodredge_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    DefaultValue = "6",
                    Delimiter = ","
                },
                /// <summary>
                /// Set process priority in the range 0 (low) to 5 (high). (default: 3)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID  = "cryptodredge_cpu_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "3"
                },
                /// <summary>
                /// Log output to file
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID  = "cryptodredge_log",
                    ShortName = "--log"
                },
                /// <summary>
                /// JSON configuration file to use
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID  = "cryptodredge_config",
                    ShortName = "-c",
                    LongName = "--config"
                }
            },
            TemperatureOptions = new List<DecloudOption>
            {
                /// <summary>
                /// GPU limit temperature, 0 disabled (default: 0)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID  = "cryptodredge_temp_limit",
                    ShortName = "--temperature-limit",
                    DefaultValue = "0"
                },
                /// <summary>
                /// GPU resume temperature, 0 disabled (default: 0)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID  = "cryptodredge_temp_start",
                    ShortName = "--temperature-start",
                    DefaultValue = "0"
                },
            }
        };
    }
}
