using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace WildRig
{
    internal static class PluginInternalSettings
    {
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
                /// strategy of feeding videocards with job (default: 0)
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_strategy",
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    LongName = "--strategy=",
                    DefaultValue = "0"
                },
                /// <summary>
                /// amount of threads per OpenCL device
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_threads",
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    LongName = "--opencl-threads=",
                },
                /// <summary>
                /// list of launch config, intensity and worksize
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_launch",
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    LongName = "--opencl-launch=",
                },
                /// <summary>
                /// affine GPU threads to a CPU
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_affinity",
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    LongName = "--opencl-affinity=",
                },
                /// <summary>
                /// log all output to a file
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_log",
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ShortName = "-l",
                    LongName = "--log-file=",
                },
                /// <summary>
                /// print hashrate report every N seconds
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_printTime",
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    LongName = "--print-time=",
                },
                /// <summary>
                /// print hashrate for each videocard
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_printFull",
                    Type = DecloudOptionType.OptionIsParameter,
                    LongName = "--print-full",
                },
                /// <summary>
                /// print additional statistics
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_printStatistics",
                    Type = DecloudOptionType.OptionIsParameter,
                    LongName = "--print-statistics",
                },
                /// <summary>
                /// print debug information
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_printDebug",
                    Type = DecloudOptionType.OptionIsParameter,
                    LongName = "--print-debug",
                },
                /// <summary>
                /// print power consumption per GPU chip
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_printPower",
                    Type = DecloudOptionType.OptionIsParameter,
                    LongName = "--print-power",
                },
                /// <summary>
                /// donate level, default 2% (2 minutes in 100 minutes)
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_fee",
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    LongName = "--donate-level=",
                    DefaultValue = "2",
                },
            },
            TemperatureOptions = new List<DecloudOption>
            {
                /// <summary>
                /// set temperature at which gpu will stop mining(default: 85)
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_tempLimit",
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    LongName = "--gpu-temp-limit=",
                    DefaultValue = "85",
                },
                /// <summary>
                /// set temperature at which gpu will resume mining(default: 60)
                /// </summary>
                new DecloudOption
                {
                    ID = "wildrig_tempResume",
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    LongName = "--gpu-temp-resume=",
                    DefaultValue = "60",
                },
            }
        };
    }
}
