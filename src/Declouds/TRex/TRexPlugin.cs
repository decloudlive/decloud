using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TRex
{
    public partial class TRexPlugin : PluginBase
    {
        public TRexPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            DecloudBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://github.com/trexDecloud/T-Rex/releases 
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "0.19.12",
                ExePath = new List<string> { "t-rex.exe" },
                Urls = new List<string>
                {
                    "https://github.com/trexDecloud/T-Rex/releases/download/0.19.12/t-rex-0.19.12-win-cuda11.1.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "T-Rex is a versatile cryptocurrency mining software for NVIDIA devices.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "03f80500-94ec-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 0);

        public override string Name => "TRex";

        public override string Author => "info@Decloud.com";

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var CUDA11 = new Version(451, 82);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= CUDA11;  // TODO <= CUDA 11 is not inside the toolkit before Decloud plugins major version 15
            var cudaGpus = devices.Where(dev => dev is CUDADevice cuda && cuda.SM_major >= 3 && isDriverSupported).Cast<CUDADevice>();
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            foreach (var gpu in cudaGpus)
            {
                var algos = GetSupportedAlgorithmsForDevice(gpu);
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        protected override DecloudBase CreateDecloudBase()
        {
            return new TRex(PluginUUID);
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "t-rex.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() == 0) return false;
                if (benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 4 && ids.FirstOrDefault() == AlgorithmType.Octopus) return true;
            }
            catch { }
            return false;
        }
    }
}
