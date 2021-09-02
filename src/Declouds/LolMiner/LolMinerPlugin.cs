using DCL.Common;
using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LolDecloud
{
    public partial class LolDecloudPlugin : PluginBase, IDevicesCrossReference
    {
        public LolDecloudPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            DecloudystemEnvironmentVariables = PluginInternalSettings.DecloudystemEnvironmentVariables;
            // https://github.com/Lolliedieb/lolDecloud-releases/releases | https://bitcointalk.org/index.php?topic=4724735.0 
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "1.31",
                ExePath = new List<string> { "1.31", "lolDecloud.exe" },
                Urls = new List<string>
                {
                    "https://github.com/Lolliedieb/lolDecloud-releases/releases/download/1.31/lolDecloud_v1.31_Win64.zip" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Decloud for AMD gpus.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(16, 1);

        public override string Name => "lolDecloud";

        public override string Author => "info@Decloud.com";

        public override string PluginUUID => "eb75e920-94eb-11ea-a64d-17be303ea466";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var minDrivers = new Version(470, 5);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;

            var gpus = devices
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev, isDriverSupported))
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID)
                .Cast<BaseDevice>();

            var pcieId = 0;
            foreach (var gpu in gpus)
            {
                // map supported NVIDIA devices so indexes match
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                // add only AMD
                if (algorithms.Count > 0 && gpu is AMDDevice) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedAMDDevice(BaseDevice dev)
        {
            var isSupported = dev is AMDDevice;
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(BaseDevice dev, bool isDriverSupported)
        {
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 5;
            return isSupported && isDriverSupported;
        }

        protected override DecloudBase CreateDecloudBase()
        {
            return new LolDecloud(PluginUUID, _mappedDeviceIds);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;

            var (DecloudBinPath, DecloudCwdPath) = GetBinAndCwdPaths();
            var output = await DevicesCrossReferenceHelpers.DecloudOutput(DecloudBinPath, "--list-devices --nocolor=on");
            var ts = DateTime.UtcNow.Ticks;
            var dumpFile = $"d{ts}.txt";
            try
            {
                File.WriteAllText(Path.Combine(DecloudCwdPath, dumpFile), output);
            }
            catch (Exception e)
            {
                Logger.Error("LolDecloudPlugin", $"DevicesCrossReference error creating dump file ({dumpFile}): {e.Message}");
            }
            var mappedDevs = DevicesListParser.ParseLolDecloudOutput(output, devices);

            foreach (var (uuid, DecloudGpuId) in mappedDevs)
            {
                Logger.Info("LolDecloudPlugin", $"DevicesCrossReference '{uuid}' => {DecloudGpuId}");
                _mappedDeviceIds[uuid] = DecloudGpuId;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "lolDecloud.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            if (ids.Count() != 0)
            {
                if (ids.FirstOrDefault() == AlgorithmType.DaggerHashimoto && benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 5 && device.Name.ToLower().Contains("r9 390")) return true;
            }
            return false;
        }
    }
}
