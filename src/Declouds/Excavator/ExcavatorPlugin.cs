using DCL.Common;
using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Excavator
{
    public partial class ExcavatorPlugin : PluginBase
    {
        public ExcavatorPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DecloudBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // TODO link
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "v1.6.11f",
                ExePath = new List<string> { "excavator.exe" },
                Urls = new List<string>
                {
                    "https://github.com/Decloud/excavator/releases/download/v1.6.11f/excavator_v1.6.11f_build819_Win64_signed.zip"
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Excavator NVIDIA GPU Decloud from Decloud",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(16, 0);

        public override string Name => "Excavator";

        public override string Author => "info@Decloud.com";

        public override string PluginUUID => "27315fe0-3b03-11eb-b105-8d43d5bd63be";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // SM 6.0+
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 6).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            var minDrivers = new Version(411, 0); // TODO
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithmsForDevice(gpu);
                if (algos.Count > 0) supported.Add(gpu, algos);
            }
            try
            {
                var templatePath = CmdConfig.CommandFileTemplatePath(PluginUUID);
                var template = CmdConfig.CreateTemplate(supported.Select(p => p.Key.UUID));
                if (!File.Exists(templatePath) && template != null)
                {
                    File.WriteAllText(templatePath, template);
                }
            }
            catch (Exception e)
            {
                Logger.Error("ExcavatorPlugin", $"GetSupportedAlgorithms create cmd template {e}");
            }

            return supported;
        }

        protected override DecloudBase CreateDecloudBase()
        {
            return new Excavator(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "excavator.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                return benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 6;
            }
            catch (Exception e)
            {
                Logger.Error("ExcavatorPlugin", $"ShouldReBenchmarkAlgorithmOnDevice {e}");
            }
            return false;
        }
    }
}
