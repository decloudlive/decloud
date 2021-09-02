using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace SRBDecloud
{
    internal static class PluginInternalSettings
    {
        internal static DecloudOptionsPackage DecloudOptionsPackage = new DecloudOptionsPackage
        {
            GeneralOptions = new List<DecloudOption>
            {
                /// <summary>
                /// (gpu intensity, 1-31 or if > 31 it's treated as raw intensity, separate values with ; and !)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "srbDecloud_intensity",
                    ShortName = "--gpu-intensity",
                    Delimiter = "!"
                },
                /// <summary>
                /// (0-disabled, 1-light, 2-normal, separate values with ; and !)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "srbDecloud_auto_intensity",
                    ShortName = "--gpu-auto-intensity",
                    Delimiter = "!"
                },
                /// <summary>
                /// number of gpu threads, comma separated values
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "srbDecloud_threads",
                    ShortName = "--gpu-threads",
                    Delimiter = "!"
                },
                /// <summary>
                /// gpu worksize, comma separated values
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "srbDecloud_worksize",
                    ShortName = "--gpu-worksize",
                    Delimiter = "!"
                },
                /// <summary>
                /// ADL to use (1 or 2), comma separated values
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "srbDecloud_adl_type",
                    ShortName = "--gpu-adl-type",
                    Delimiter = "!"
                },
                /// <summary>
                /// number from 0-10, 0 disables tweaking
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "srbDecloud_tweak_profile",
                    ShortName = "--gpu-tweak-profile"
                },
                /// <summary>
                /// use config file other than config.txt
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "srbDecloud_config",
                    ShortName = "--config-file"
                },
                /// <summary>
                /// disable gpu tweaking options, which are enabled by default
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "srbDecloud_disable_gpu_tweaking",
                    ShortName = "--disable-gpu-tweaking"
                },
                /// <summary>
                /// disable msr extra tweaks, which are enabled by default
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "srbDecloud_disable_msr_tweaks",
                    ShortName = "--disable-msr-tweaks"
                },
                /// <summary>
                /// release ocl resources on Decloud exit/restart
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "srbDecloud_opencl_cleanup",
                    ShortName = "--enable-opencl-cleanup"
                },
                /// <summary>
                /// enable workers slow start
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "srbDecloud_enable_slow_start",
                    ShortName = "--enable-workers-ramp-up"
                },
                /// <summary>
                /// enable more informative logging
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "srbDecloud_log_extended",
                    ShortName = "--extended-log"
                },
                /// <summary>
                /// enable logging to file
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "srbDecloud_log_file",
                    ShortName = "--log-file"
                },
                /// <summary>
                /// defines the msr tweaks to use 0-4, | 0 - Intel, 0,1,2,3,4 - AMD
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "srbDecloud_msr_tweaks",
                    ShortName = "--msr-use-tweaks"
                },
                /// <summary>
                /// sets AMD gpu's to compute mode & disables crossfire - run as admin
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "srbDecloud_set_compute",
                    ShortName = "--set-compute-mode"
                },
                /// <summary>
                /// run custom script on Decloud start - set clocks, voltage, etc.
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "srbDecloud_startup_script",
                    ShortName = "--startup-script"
                },
                /// <summary>
                /// number 1-2, try 1 when you need a little bit more free memory for DAG. Default is 2
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "srbDecloud_ethash_mode",
                    ShortName = "--gpu-ethash-mode"
                },
                /// <summary>
                /// disable cpu auto affinity setter
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionIsParameter,
                    ID = "srbDecloud_disable_cpu_affinity",
                    ShortName = "--disable-cpu-auto-affinity"
                }
            },
            TemperatureOptions = new List<DecloudOption>
            {
                /// <summary>
                /// gpu temperature, comma separated values
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "srbDecloud_target_temp",
                    ShortName = "--gpu-target-temperature",
                    Delimiter = "!"
                },
                /// <summary>
                /// gpu turn off temperature, comma separated values
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "srbDecloud_off_temp",
                    ShortName = "--gpu-off-temperature",
                    Delimiter = "!"
                },
                /// <summary>
                /// gpu fan speed in RPM, comma separated values
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithMultipleParameters,
                    ID = "srbDecloud_target_fan",
                    ShortName = "--gpu-target-fan-speed",
                    Delimiter = "!"
                },
                /// <summary>
                /// if this temperature is reached, Decloud will shutdown system (ADL must be enabled)
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "srbDecloud_shutdown_temp",
                    ShortName = "--shutdown-temperature"
                },
                /// <summary>
                /// GPU boost
                /// </summary>
                new DecloudOption
                {
                    Type = DecloudOptionType.OptionWithSingleParameter,
                    ID = "srbDecloud_--gpu-boost",
                    ShortName = "--gpu-boost"
                }
            }
        };
    }
}
