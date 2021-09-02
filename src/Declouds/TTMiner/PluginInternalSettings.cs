using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace TTDecloud
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 3, 0);

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
                /// Comma or space separated list of intensities that should be used mining.
			    /// First value for first GPU and so on. A single value sets the same intensity to all GPUs. A value of -1 uses the default intensity of the Decloud.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "ttDecloud_intensity",
                    ShortName = "-i",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// intensity grid. Same as intensity (-i) just defines the size for the grid directly.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "ttDecloud_intensity_grid",
                    ShortName = "-ig",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                /// <summary>
                /// intensity grid-size. This will give you more and finer control about the gridsize.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "ttDecloud_grid_size",
                    ShortName = "-gs",
                    DefaultValue = "-1",
                    Delimiter = ","
                },
                
                /// <summary>
                /// Reports the current hashrate every 90 seconds to the pool
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "ttDecloud_rate",
                    ShortName = "-RH",
                    LongName = "-rate"
                },
                /// <summary>
                /// This option set the process priority for TT-Decloud to a different level:
                /// 1 low
                /// 2 below normal
                /// 3 normal
                /// 4 above normal
                /// 5 high
                /// Default: -PP 3
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "ttDecloud_processPriority",
                    ShortName = "-PP",
                    DefaultValue = "3"
                },
                /// <summary>
                /// Performance-Report GPU-name
                /// Prints the name/model in the performance report
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "ttDecloud_perfRepGpuName",
                    ShortName = "-PRGN",
                },
                /// <summary>
                /// Performance-Report Hash-Rate Interval
                /// Performance-Report & information after INT multiple of one minute. Minimum value for INT to
                /// 1 which creates a hashrate interval of a minute. Higher Intervals gives you a more stable
                /// hashrate. If the interval is too high the displayed average of your hashrate will change
                /// very slowly. The default of 2 will give you an average of 2 minutes.
                /// Default: -PRHRI 2
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "ttDecloud_perfRepHashRateInterval",
                    ShortName = "-PRHRI",
                    DefaultValue = "2"
                },
                /// <summary>
                /// Performance-Report & information after INT multiple of 5 seconds
                /// Set INT to 0 to disable output after a fixed timeframe
                /// sample -RPT 24 shows the performance report after 24 * 5 sec = 2 minutes
                /// Default: -PRT 3
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "ttDecloud_perfRepInfoTime",
                    ShortName = "-PRT",
                    DefaultValue = "3"
                },
                /// <summary>
                /// Performance-Report & information after a INT shares found
                /// Set INT to 0 to disable output after a fixed number of shares
                /// sample - RPS 10 shows the performance report after 10 shares were found
                /// Default: -PRS 0
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "ttDecloud_perfRepInfoShares",
                    ShortName = "-PRS",
                    DefaultValue = "0"
                },
                /// <summary>
                /// Enable logging of the pool communication. TT-Decloud creates the pool-logfile in the folder 'Logs'.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "ttDecloud_logPool",
                    ShortName = "-logpool",
                },
                /// <summary>
                /// Enable logging of screen output and additional information, the file is created in the folder 'Logs'.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "ttDecloud_log",
                    ShortName = "-log",
                },
            }
        };
    }
}
