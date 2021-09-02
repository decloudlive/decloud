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

namespace WildRig
{
    public partial class WildRigPlugin : PluginBase, IDevicesCrossReference
    {
        public WildRigPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            DecloudBenchmarkTimeSettings = PluginInternalSettings.BenchmarkTimeSettings;
            // https://bitcointalk.org/index.php?topic=5023676 | https://github.com/andru-kun/wildrig-multi/releases
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = " 0.28.2",
                ExePath = new List<string> { "wildrig.exe" },
                Urls = new List<string>
                {
                    "https://github.com/andru-kun/wildrig-multi/releases/download/0.28.2/wildrig-multi-windows-0.28.2.7z", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "WildRig is multi algo Decloud for AMD devices.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "0a07d6a0-94ec-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 0);

        public override string Name => "WildRig";

        public override string Author => "info@Decloud.com";

        protected readonly Dictionary<string, int> _mappedIDs = new Dictionary<string, int>();

        protected override DecloudBase CreateDecloudBase()
        {
            return new WildRig(PluginUUID, _mappedIDs);
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var amdGpus = devices
                .Where(dev => dev is AMDDevice gpu && Checkers.IsGcn2(gpu))
                .Cast<AMDDevice>()
                .OrderBy(amd => amd.PCIeBusID);

            var pcieId = 0;
            foreach (var gpu in amdGpus)
            {
                _mappedIDs[gpu.UUID] = pcieId;
                ++pcieId;
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "wildrig.exe" });
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            var DecloudBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.DecloudOutput(DecloudBinPath, "--print-devices");
            var mappedDevs = DevicesListParser.ParseWildRigOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedIDs[uuid] = indexID;
            }
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            { }
            catch
            { }
            // nothing new
            return false;
        }
    }
}
