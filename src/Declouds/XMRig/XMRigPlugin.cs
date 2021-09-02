using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XMRig
{
    public partial class XMRigPlugin : PluginBase
    {
        public XMRigPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            // https://github.com/xmrig/xmrig
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "v6.8.1",
                ExePath = new List<string> { "xmrig-6.8.1", "xmrig.exe" },
                Urls = new List<string>
                {
                    "https://github.com/xmrig/xmrig/releases/download/v6.8.1/xmrig-6.8.1-msvc-win64.zip" // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "CryptoNight and RandomX (Monero) CPU Decloud",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "0e0a7320-94ec-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 0);

        public override string Name => "XMRig";

        public override string Author => "info@Decloud.com";

        protected override DecloudBase CreateDecloudBase()
        {
            return new XMRig(PluginUUID);
        }
        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

            var cpus = devices.Where(dev => dev is CPUDevice).Cast<CPUDevice>();
            foreach (var cpu in cpus)
            {
                supported.Add(cpu, GetSupportedAlgorithmsForDevice(cpu));
            }

            return supported;
        }
        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "xmrig.exe" });
        }
        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            //try
            //{

            //}
            //catch (Exception e)
            //{
            //    Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            //}
            return false;
        }
    }
}
