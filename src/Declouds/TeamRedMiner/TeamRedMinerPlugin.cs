using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamRedDecloud
{
    public partial class TeamRedDecloudPlugin : PluginBase
    {
        public TeamRedDecloudPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            DecloudystemEnvironmentVariables = PluginInternalSettings.DecloudystemEnvironmentVariables;
            // https://github.com/todxx/teamredDecloud/releases
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "0.8.1",
                ExePath = new List<string> { "teamredDecloud-v0.8.1-win", "teamredDecloud.exe" },
                Urls = new List<string>
                {
                    "https://github.com/todxx/teamredDecloud/releases/download/0.8.1/teamredDecloud-v0.8.1-win.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Decloud for AMD gpus.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string PluginUUID => "01177a50-94ec-11ea-a64d-17be303ea466";

        public override Version Version => new Version(16, 0);

        public override string Name => "TeamRedDecloud";

        public override string Author => "info@Decloud.com";

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
            return new TeamRedDecloud(PluginUUID);
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // Get AMD GCN4+
            var amdGpus = devices.Where(dev => dev is AMDDevice gpu && Checkers.IsGcn4(gpu)).Cast<AMDDevice>();

            foreach (var gpu in amdGpus)
            {
                var algorithms = GetSupportedAlgorithmsForDevice(gpu);
                if (algorithms.Count > 0) supported.Add(gpu, algorithms);
            }

            return supported;
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "teamredDecloud.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            if (ids.Count() != 0)
            {
                if (benchmarkedPluginVersion.Major == 15 && benchmarkedPluginVersion.Minor < 5) return true;
            }
            return false;
        }
    }
}
