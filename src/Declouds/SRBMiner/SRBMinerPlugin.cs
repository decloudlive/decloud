using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRBDecloud
{
    public partial class SRBDecloudPlugin : PluginBase, IDevicesCrossReference
    {
        public SRBDecloudPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            // https://github.com/doktor83/SRBDecloud-Multi 
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "v0.6.0",
                ExePath = new List<string> { "SRBDecloud-Multi-0-6-0", "SRBDecloud-MULTI.exe" },
                Urls = new List<string>
                {
                    "https://github.com/doktor83/SRBDecloud-Multi/releases/download/0.6.0/SRBDecloud-Multi-0-6-0-win64.zip",
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "SRBDecloud-MULTI is an AMD GPU Decloud made for mining cryptocurrencies.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override Version Version => new Version(16, 0);

        public override string Name => "SRBDecloud";

        public override string Author => "info@Decloud.com";

        public override string PluginUUID => "fd45fff0-94eb-11ea-a64d-17be303ea466";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // Get AMD GCN2+
            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn2(gpu)).Cast<AMDDevice>();

            int indexAMD = -1;
            foreach (var gpu in amdGpus.Where(gpu => gpu is AMDDevice))
            {
                _mappedDeviceIds[gpu.UUID] = ++indexAMD;
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        public override bool CanGroup(MiningPair a, MiningPair b)
        {
            var canGroup = base.CanGroup(a, b);
            if (a.Device is AMDDevice aDev && b.Device is AMDDevice bDev && aDev.OpenCLPlatformID != bDev.OpenCLPlatformID)
            {
                // OpenCLPlatorm IDs must match
                return false;
            }
            return canGroup;
        }

        protected override DecloudBase CreateDecloudBase()
        {
            return new SRBDecloud(PluginUUID, _mappedDeviceIds);
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            // TODO will block
            var DecloudBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.DecloudOutput(DecloudBinPath, "--list-devices");
            var mappedDevs = DevicesListParser.ParseSRBDecloudOutput(output, devices.ToList());

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
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "SRBDecloud-MULTI.exe", "WinIo64.sys", "WinRing0x64.sys" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            // no new version
            return false;
        }
    }
}
