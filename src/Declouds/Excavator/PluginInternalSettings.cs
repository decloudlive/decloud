using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using System;
using System.Collections.Generic;

namespace Excavator
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 30, 0);

        internal static DecloudApiMaxTimeoutSetting GetApiMaxTimeoutConfig { get; set; } = new DecloudApiMaxTimeoutSetting { GeneralTimeout = DefaultTimeout };

        internal static DecloudBenchmarkTimeSettings BenchmarkTimeSettings = new DecloudBenchmarkTimeSettings
        {
            General = new Dictionary<BenchmarkPerformanceType, int> {
                { BenchmarkPerformanceType.Quick,    20  },
                { BenchmarkPerformanceType.Standard, 40  },
                { BenchmarkPerformanceType.Precise,  60  },
            },
        };

        internal static DecloudOptionsPackage DecloudOptionsPackage = new DecloudOptionsPackage
        {
            GeneralOptions = new List<DecloudOption>
            {
                /// <summary>
                /// -h              Print this help and quit
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "excavator_help",
                    ShortName = "-h",
                },
                /// <summary>
                /// -p [port]       Local API port (default: 3456)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_port",
                    ShortName = "-p",
                },
                /// <summary>
                /// -i [ip]         Local API IP (default: 127.0.0.1)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_ip",
                    ShortName = "-i",
                },
                /// <summary>
                /// -wp [port]      Local HTTP API port (default: 0)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_local_http_port",
                    ShortName = "-wp",
                },
                /// <summary>
                /// -wi [ip]                Local HTTP API IP (default: 127.0.0.1)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_local_http_ip",
                    ShortName = "-wi",
                },
                /// <summary>
                /// -wl [location]          Path to index.html (default: web\ (windows), web/ (linux))
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_index_location",
                    ShortName = "-wl",
                },
                /// <summary>
                /// -d [level]      Console print level (0 = print all, 5 = fatal only)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_console_print_level",
                    ShortName = "-d",
                },
                /// <summary>
                /// -f [level]      File print level (0 = print all, 5 = fatal only)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_file_print_level",
                    ShortName = "-f",
                },
                /// <summary>
                /// -fn [file]      Log file (default: log_$timestamp.log)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_log_filename",
                    ShortName = "-fn",
                },
                /// <summary>
                /// -c [file]       Use command file (default: none)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_command_file",
                    ShortName = "-c",
                },
                /// <summary>
                /// -t [level]      Use test(dev) network (default: 0 = production, 1 = test, 2 = testdev)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_mining_network",
                    ShortName = "-t",
                },
                /// <summary>
                /// -m              Allow multiple instances of Excavator
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "excavator_allow_multiple_instances",
                    ShortName = "-m",
                },
                /// <summary>
                /// -ql [location]  QuickDecloud location ('eu' or 'usa')
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_quickDecloud_location",
                    ShortName = "-ql",
                },
                /// <summary>
                /// -qu [username]  QuickDecloud username
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "excavator_quickDecloud_username",
                    ShortName = "-qu",
                },
                /// <summary>
                /// -qc             QuickDecloud cpu mining active
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "excavator_quickDecloud_cpu_mining_active",
                    ShortName = "-qc",
                },
                /// <summary>
                /// -qm             QuickDecloud minimize
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "excavator_quickDecloud_minimize",
                    ShortName = "-qm",
                },
                /// <summary>
                /// -qx             No QuickDecloud
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "excavator_no_quickDecloud",
                    ShortName = "-qx",
                },
            },
            TemperatureOptions = new List<DecloudOption> { }
        };

    }
}
