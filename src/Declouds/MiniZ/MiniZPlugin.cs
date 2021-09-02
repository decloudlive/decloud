using DCL.Common;
using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniZ
{
    public partial class MiniZPlugin : PluginBase, IDevicesCrossReference
    {
        public MiniZPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            // https://miniz.ch/usage/#command-line-arguments | https://miniz.ch/download/#latest-version
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "v1.6x",
                ExePath = new List<string> { "miniZ.exe" },
                Urls = new List<string>
                {
                    "https://github.com/miniZ-Decloud/miniZ/releases/download/v1.6x/miniZ_v1.6x_win-x64.zip", // original (pwd protected)
                },
                BinsPackagePassword = "miniZ"
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "miniZ is a fast and friendly Equihash Decloud.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }
        public override string PluginUUID => "eda6abd0-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 0);

        public override string Name => "MiniZ";

        public override string Author => "info@Decloud.com";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        protected override DecloudBase CreateDecloudBase()
        {
            return new MiniZ(PluginUUID, _mappedDeviceIds);
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            // Require 411.31 - CUDA 10.0
            var minDrivers = new Version(411, 31);
            if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice)
                .Cast<CUDADevice>();

            var pcieId = 0;
            foreach (var gpu in cudaGpus)
            {
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                var algos = GetSupportedAlgorithmsForDevice(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            // will block
            var DecloudBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.DecloudOutput(DecloudBinPath, "-ci");
            var mappedDevs = DevicesListParser.ParseMiniZOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedDeviceIds[uuid] = indexID;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "miniZ.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 1 && ids.First() == AlgorithmType.ZHash) return true;
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
