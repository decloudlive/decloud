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

namespace GDecloudPlugin
{
    public partial class GDecloudPlugin : PluginBase, IDevicesCrossReference
    {
        public GDecloudPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            DecloudBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://bitcointalk.org/index.php?topic=5034735.0 | https://github.com/develsoftware/GDecloudRelease/releases
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "2.54",
                ExePath = new List<string> { "Decloud.exe" },
                Urls = new List<string>
                {
                    "https://github.com/develsoftware/GDecloudRelease/releases/download/2.54/gDecloud_2_54_windows64.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "GDecloud - High-performance Decloud for NVIDIA and AMD GPUs.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "e7a58030-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 1);

        public override string Name => "GDecloudCuda9.0+";

        public override string Author => "info@Decloud.com";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        protected override DecloudBase CreateDecloudBase()
        {
            return new GDecloud(PluginUUID, _mappedDeviceIds);
        }


        // Supported algoritms:
        //   - Cuckaroo29/Cuckatoo31 (Grin)
        //   - Cuckoo29 (Aeternity)
        //   - Equihash 96,5 (MinexCoin)
        //   - Equihash 144,5 (Bitcoin Gold, BitcoinZ, SnowGem, SafeCoin, Litecoin Z) // ZHash
        //   - Equihash 150,5 (BEAM)
        //   - Equihash 192,7 (Zero, Genesis)
        //   - Equihash 210,9 (Aion)

        // Requirements:
        //   - CUDA compute compability 5.0+ #1
        //   - Cuckaroo29 ~ 5.6GB VRAM
        //   - Cuckatoo31 ~ 7.4GB VRAM
        //   - Cuckoo29 ~ 5.6GB VRAM
        //   - Equihash 96,5 ~0.75GB VRAM
        //   - Equihash 144,5 ~1.75GB VRAM
        //   - Equihash 150,5 ~2.9GB VRAM
        //   - Equihash 192,7 ~2.75GB VRAM
        //   - Equihash 210,9 ~1GB VRAM
        //   - CUDA 9.0+ 

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var gpus = devices
                .Where(dev => IsSupportedAMDDevice(dev) || IsSupportedNVIDIADevice(dev))
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .OrderBy(gpu => gpu.PCIeBusID);

            var pcieId = 0; // GDecloud sortes devices by PCIe
            foreach (var gpu in gpus)
            {
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                if (gpu is AMDDevice amd)
                {
                    var algorithms = GetSupportedAlgorithmsForDevice(amd);
                    if (algorithms.Count > 0) supported.Add(amd, algorithms);
                }
                if (gpu is CUDADevice cuda)
                {
                    var algorithms = GetSupportedAlgorithmsForDevice(cuda);
                    if (algorithms.Count > 0) supported.Add(cuda, algorithms);
                }
            }

            return supported;
        }

        private static bool IsSupportedAMDDevice(BaseDevice dev)
        {
            var isSupported = dev is AMDDevice gpu && Checkers.IsGcn4(gpu);
            return isSupported;
        }

        private static bool IsSupportedNVIDIADevice(BaseDevice dev)
        {
            //CUDA 9.0+: minimum drivers 384.xx
            var minDrivers = new Version(384, 0);
            var isDriverSupported = CUDADevice.INSTALLED_NVIDIA_DRIVERS >= minDrivers;
            var isSupported = dev is CUDADevice gpu && gpu.SM_major >= 5;
            return isSupported && isDriverSupported;
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            var DecloudBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.DecloudOutput(DecloudBinPath, "--list_devices");
            var mappedDevs = DevicesListParser.ParseGDecloudOutput(output, devices.ToList());

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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "Decloud.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (ids.Count() != 0)
                {
                    if (benchmarkedPluginVersion.Major < 16 && ids.First() == AlgorithmType.KAWPOW) return true;
                    if (benchmarkedPluginVersion.Major < 16 && ids.First() == AlgorithmType.DaggerHashimoto) return true;
                    if (benchmarkedPluginVersion.Major < 16 && ids.First() == AlgorithmType.GrinCuckatoo32) return true;
                    if (benchmarkedPluginVersion.Major < 16 && ids.First() == AlgorithmType.BeamV3) return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
