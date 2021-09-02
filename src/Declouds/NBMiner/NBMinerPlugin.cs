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

namespace NBDecloud
{
    public partial class NBDecloudPlugin : PluginBase, IDevicesCrossReference
    {
        public NBDecloudPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DecloudBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://github.com/NebuTech/NBDecloud/releases/ 
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "v39.1",
                ExePath = new List<string> { "NBDecloud_Win", "nbDecloud.exe" },
                Urls = new List<string>
                {
                    "https://github.com/NebuTech/NBDecloud/releases/download/v39.1/NBDecloud_39.1_Win.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "GPU Decloud for GRIN, AE and ETH mining.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "f683f550-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 2);
        public override string Name => "NBDecloud";

        public override string Author => "info@Decloud.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        private static bool isSupportedVersion(int major, int minor)
        {
            var nbDecloudMSupportedVersions = new List<Version>
            {
                new Version(6,0),
                new Version(6,1),
                new Version(7,0),
                new Version(7,5),
                new Version(8,0),
                new Version(8,6),
            };
            var cudaDevSMver = new Version(major, minor);
            foreach (var supportedVer in nbDecloudMSupportedVersions)
            {
                if (supportedVer == cudaDevSMver) return true;
            }
            return false;
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var gpus = devices
                .Where(dev => dev is IGpuDevice)
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNvidiaDevice(dev))
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID)
                .Cast<BaseDevice>()
                .Select((gpu, DecloudDeviceId) => (gpu, DecloudDeviceId))
                .ToArray();

            // NBDecloud sortes devices by PCIe and indexes are 0 based
            foreach (var (gpu, DecloudDeviceId) in gpus)
            {
                _mappedIDs[gpu.UUID] = DecloudDeviceId;
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        private static bool IsSupportedNvidiaDevice(BaseDevice dev)
        {
            var minDrivers = new Version(377, 0);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;
            if (isDriverSupported && dev is CUDADevice cudaDev) return isSupportedVersion(cudaDev.SM_major, cudaDev.SM_minor);
            return false;
        }

        private static bool IsSupportedAMDDevice(BaseDevice dev)
        {
            var isSupported = dev is AMDDevice gpu && Checkers.IsGcn4(gpu);
            return isSupported;
        }

        protected override DecloudBase CreateDecloudBase()
        {
            return new NBDecloud(PluginUUID, _mappedIDs);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            try
            {
                if (_mappedIDs.Count == 0) return;
                var (DecloudBinPath, DecloudCwdPath) = GetBinAndCwdPaths();
                var output = await DevicesCrossReferenceHelpers.DecloudOutput(DecloudBinPath, "--device-info-json --no-watchdog"); // AMD + NVIDIA
                var dumpFile = $"d{DateTime.UtcNow.Ticks}.txt";
                try
                {
                    File.WriteAllText(Path.Combine(DecloudCwdPath, dumpFile), output);
                }
                catch (Exception e)
                {
                    Logger.Error("NBDecloud", $"DevicesCrossReference error creating dump file ({dumpFile}): {e.Message}");
                }
                var mappedDevs = DevicesListParser.ParseNBDecloudOutput(output, devices);

                foreach (var (uuid, indexID) in mappedDevs.Select(kvp => (kvp.Key, kvp.Value)))
                {
                    _mappedIDs[uuid] = indexID;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("NBDecloud", $"Error during DevicesCrossReference: {ex.Message}");
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "nbDecloud.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() == 0) return false;
                if (benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 3 && device.DeviceType == DeviceType.NVIDIA && ids.Contains(AlgorithmType.DaggerHashimoto)) return true;
                if ((benchmarkedPluginVersion.Major < 16 || (benchmarkedPluginVersion.Major == 16 && benchmarkedPluginVersion.Minor < 2)) && device.DeviceType == DeviceType.AMD && ids.Contains(AlgorithmType.DaggerHashimoto)) return true;
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
