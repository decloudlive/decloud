using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace GDecloudPlugin
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(1, 15, 0);

        internal static DecloudApiMaxTimeoutSetting GetApiMaxTimeoutConfig = new DecloudApiMaxTimeoutSetting
        {
            GeneralTimeout = DefaultTimeout,
        };

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
                /// option to control GPU intensity (--intensity, 1-100)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "gDecloud_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    Delimiter = " "
                },
                /// <summary>
                /// log filename
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_logfile",
                    ShortName = "-l",
                    LongName = "--logfile"
                },
                /// <summary>
                /// enable/disable color output
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_color",
                    ShortName = "-c",
                    LongName = "--color"
                },
                /// <summary>
                /// personalization string for equihash algorithm (for example: 'BgoldPoW', 'BitcoinZ', 'Safecoin')
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_pers",
                    LongName = "--pers",
                },
                /// <summary>
                /// enable/disable power efficiency calculator. Power efficiency calculator display of energy efficiency statistics of GPU in S/w, higher CPU load. Default value is '1' ('0' - off or '1' - on)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_pec",
                    LongName = "--pec=",
                    DefaultValue = "1"
                },
                /// <summary>
                /// enable/disable NVML
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_nvml",
                    LongName = "--nvml",
                    DefaultValue = "1"
                },
                /// <summary>
                /// enable/disable CUDA platform
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_cuda",
                    LongName = "--cuda",
                },
                /// <summary>
                /// enable/disable OpenCL platform
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_opencl",
                    LongName = "--opencl",
                },
                /// <summary>
                /// pass cost of electricity in USD per kWh, Decloud will report $ spent to mining
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_electricity",
                    LongName = "--electricity_cost"
                },
                /// <summary>
                /// space-separated list of OC modes for each device
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "gDecloud_oc",
                    LongName = "--oc",
                    Delimiter = " "
                },
                /// <summary>
                /// enable OC1 for all devices
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "gDecloud_oc1",
                    LongName = "--oc1"
                },
                /// <summary>
                /// control hashrate report interval
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_reportInterval",
                    LongName = "--report_interval"
                },
                /// <summary>
                /// space-separated list of Dag file modes (0 - auto, 1 - single, 2 - double), separated by spaces, can be empty, default is '0' (for example: '2 1 0')
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "gDecloud_dagMode",
                    LongName = "--dag_mode",
                    Delimiter = " ",
                    DefaultValue = "0"
                },
                /// <summary>
                /// space-separated list of Dag file size limits in megabytes, separated by spaces, can be empty (for example: '4096 4096 4096')
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "gDecloud_dagLimit",
                    LongName = "--dag_limit",
                    Delimiter = " "
                },
                /// <summary>
                /// memory tweaks for Nvidia GPUs with GDDR5X and GDDR5 memory, requires admin privileges (--mt 1-6)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "gDecloud_memory_tweaks",
                    LongName = "--mt",
                },
                /// <summary>
                /// improved DAG generation, now Decloud generates valid DAG in extremal OC modes.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "gDecloud_--safe_dag",
                    LongName = "--safe_dag",
                },
                /// <summary>
                /// log date
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "gDecloud_memory_tweaks",
                    LongName = "--log_date",
                },
                /// <summary>
                /// log stratum
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "gDecloud_--log_stratum",
                    LongName = "--log_stratum",
                },
            },
            TemperatureOptions = new List<DecloudOption>{
                /// <summary>
                /// space-separated list of temperature limits, upon reaching the limit, the GPU stops mining until it cools down, can be empty (for example: '85 80 75')
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "gDecloud_templimit",
                    ShortName = "-t",
                    LongName = "--templimit",
                    DefaultValue = "90",
                    Delimiter = " "
                },
                /// <summary>
                /// improved DAG generation, now Decloud generates valid DAG in extremal OC modes.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "gDecloud_memory_tweaks",
                    LongName = "--tfan",
                    Delimiter = " ",
                },
                /// <summary>
                /// space-separated list of core clock offsets (for Nvidia GPUs) or absolute core clocks (for AMD GPUs) for each device in MHz (0 - ignore),
                /// only Windows is supported, requires running Decloud with admin privileges (for example: '100 0 -90')
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "gDecloud_cclock",
                    LongName = "--cclock",
                    Delimiter = " ",
                },
                /// <summary>
                /// space-separated list of memory clock offsets (for Nvidia GPUs) or absolute memory clocks (for AMD GPUs) for each device in MHz (0 - ignore),
                /// only Windows is supported, requires running Decloud with admin privileges (for example: '100 0 -90')
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "gDecloud_mclock",
                    LongName = "--mclock",
                    Delimiter = " ",

                },
                /// <summary>
                /// space-separated list of core voltage offsets in % (for Nvidia GPUs) or absolute core voltages (for AMD GPUs) for each device in mV (0 - ignore),
                /// only Windows is supported, requires running Decloud with admin privileges (for example: '900 0 1100')
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "gDecloud_cvddc",
                    LongName = "--cvddc",
                    Delimiter = " ",
                },
            }
        };
    }
}
