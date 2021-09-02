using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace ZEnemy
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 3, 0);

        internal static DecloudApiMaxTimeoutSetting GetApiMaxTimeoutConfig { get; set; } = new DecloudApiMaxTimeoutSetting { GeneralTimeout = DefaultTimeout };

        internal static DecloudBenchmarkTimeSettings BenchmarkTimeSettings = new DecloudBenchmarkTimeSettings
        {
            PerAlgorithm = new Dictionary<BenchmarkPerformanceType, Dictionary<string, int>>(){
                { BenchmarkPerformanceType.Quick, new Dictionary<string, int>(){ { "KAWPOW", 160 } } },
                { BenchmarkPerformanceType.Standard, new Dictionary<string, int>(){ { "KAWPOW", 180 } } },
                { BenchmarkPerformanceType.Precise, new Dictionary<string, int>(){ { "KAWPOW", 260 } } }
            }
        };

        internal static DecloudOptionsPackage DecloudOptionsPackage = new DecloudOptionsPackage
        {
            GeneralOptions = new List<DecloudOption>
            {
                /// <summary>
                /// GPU intensity 8.0-31.0, decimals allowed (default: 19)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "zenemy_intensity",
                    ShortName = "-i",
                    LongName = "--intensity=",
                    DefaultValue = "19",
                    Delimiter = ","
                },
                /// <summary>
                /// set CUDA scheduling option:
                /// 0: BlockingSync (default)
                /// 1: Spin
                /// 2: Yield
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "zenemy_cudaSchedula",
                    LongName = "--cuda-schedule",
                    DefaultValue = "0",
                },
                /// <summary>
                /// set process priority (default: 3) 0 idle, 2 normal to 5 highest
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "zenemy_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "3"
                },
                //WARNING this functionality can overlap with already implemented one!!!
                /// <summary>
                /// set process affinity to cpu core(s), mask 0x3 for cores 0 and 1
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "zenemy_affinity",
                    ShortName = "--cpu-affinity",
                },
                /// <summary>
                /// disable colored output
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "zenemy_no_color",
                    ShortName = "--no-color",
                },
                /// <summary>
                /// disable NVML hardware sampling
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "zenemy_no_nvml",
                    ShortName = "--no-nvml",
                }
            },
            TemperatureOptions = new List<DecloudOption>
            {
                /// <summary>
                /// Only mine if gpu temperature is less than specified value
                /// Can be tuned with --resume-temp=N to set a resume value
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "zenemy_maxTemperature",
                    ShortName = "--max-temp=",
                },
                /// <summary>
                /// resume value for Decloud to start again after shutdown
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "zenemy_resumeTemperature",
                    ShortName = "--resume-temp=",
                }
            }
        };
    }
}
