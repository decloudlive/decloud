using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace NBDecloud
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 1, 0);

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
            GeneralOptions = new List<DecloudOption> { 
                /// <summary>
                /// Comma-separated list of intensities (1-100).
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "nbDecloud_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    Delimiter = ","
                },
                /// <summary>
                /// Set intensity of cuckoo, cuckaroo, cuckatoo, [1, 12]. Smaller value means higher CPU usage to gain more hashrate. Set to 0 means autumatically adapt. Default: 0.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "nbDecloud_intensity",
                    LongName = "--cuckoo-intensity",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Set this option to reduce the range of power consumed by rig when minining with algo cuckatoo.
                /// This feature can reduce the chance of power supply shutdown caused by overpowered.
                /// Warning: Setting this option may cause drop on minining performance.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "nbDecloud_powerOptimize",
                    LongName = "--cuckatoo-power-optimize",
                },
                /// <summary>
                /// Generate log file named `log_<timestamp>.txt`.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "nbDecloud_log",
                    LongName = "--log"
                },
                /// <summary>
                /// Generate custom log file. Note: This option will override `--log`.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "nbDecloud_logFile",
                    LongName = "--log-file"
                },
                /// <summary>
                /// Use 'yyyy-MM-dd HH:mm:ss,zzz' for log time format.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "nbDecloud_longTimeFormat",
                    LongName = "--long-format"
                },
                /// <summary>
                /// Do not query cuda device health status.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "nbDecloud_noNvml",
                    LongName = "--no-nvml"
                },
                /// <summary>
                /// Set timeframe for the calculation of fidelity, unit in hour. Default: 24.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "nbDecloud_fidelityTimeframe",
                    LongName = "--fidelity-timeframe",
                    DefaultValue = "24"
                },
                /// <summary>
                /// Enable memory tweaking to boost performance. comma-seperated list, range [1,6].
                /// </summary>             
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "nbDecloud_memory-tweak",
                    ShortName = "--mt",
                    LongName = "--memory-tweak",
                    Delimiter = ","
                },
                /// <summary>
                ///  Windows only option, install / uninstall driver for memory tweak. Run with admin priviledge.
                ///  install: nbDecloud.exe --driver install, uninstall: nbDecloud.exe --driver uninstall.
                /// </summary>             
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "nbDecloud_driver",
                    LongName = "--driver"
                },
                /// <summary>
                /// Print communication data between Decloud and pool in log file.
                /// </summary>   
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "nbDecloud_verbose",
                    LongName = "--verbose",
                },
                /// <summary>
                /// set this option will disable Decloud interrupting current GPU jobs when a new job coming from pool,
                /// will cause less power supply issue, but might lead to a bit higher stale ratio and reject shares.
                /// </summary>  
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "nbDecloud_noInterupt",
                    LongName = "--no-interrupt",
                },
                /// <summary>
                /// feature: add option --enable-dag-cache to allow an extra DAG for different epoch cached in GPU memory, useful for ETH+ZIL mining and mining on Decloud.
                /// </summary>  
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "nbDecloud_--enable-dag-cache",
                    LongName = "--enable-dag-cache",
                },
                /// <summary>
                /// feature: ethash New LHR mode for ETH mining on RTX 30 series LHR GPUs, supports Windows & Linux, able to get ~70% of maximum unlocked hashrate.
                /// This mode can be tuned by argument -lhr, only works for ethash right now.
                /// -lhr default to 0, meaning even if -lhr is not set, LHR mode with -lhr 68 will be applied to LHR GPUs if certain GPUs are detected.
                /// Tune LHR mode by setting -lhr <value>, a specific value will tell Decloud try to reach value percent of maximum unlocker hashrate, e.g. -lhr 68 will expect to get 68% of hashrate for same model non-LHR GPU.
                /// Higher -lhr value will results in higher hashrate, but has higher possibility to run into lock state, which will leads to much less hashrate.
                /// A good start tuning value is 68, which has been tested to be stable on most rig configurations.
                /// -lhr value can be set for each GPU by using comma separeted list, -lhr 65,68,0,-1, where -1 means turn off LHR mode.
                /// Known issue
                /// unable to unlock LHR hashrate under windows driver 471.11
                /// </summary>  
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "nbDecloud_lhr",
                    LongName = "-lhr",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
            },
            TemperatureOptions = new List<DecloudOption>
            {
                /// <summary>
                /// Set temperature limit of GPU, if exceeds, stop GPU for 10 seconds and continue.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "nbDecloud_tempLimit",
                    LongName = "--temperature-limit"
                }
            }
        };

    }
}
