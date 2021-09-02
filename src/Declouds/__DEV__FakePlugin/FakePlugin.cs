using Newtonsoft.Json;
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

namespace FakePlugin
{
    /// <summary>
    /// Plugin class inherits IDecloudPlugin interface for registering plugin
    /// </summary>
    public partial class FakePlugin : PluginBase
    {
        //public override string PluginUUID => "b82f4e50-8002-11eb-9bca-b75efa9f41af"; //plugin 1
        public override string PluginUUID => "4ecb5de0-8003-11eb-9bca-b75efa9f41af"; //plugin 2
        public override string Name => GetPluginName();
        public override Version Version => GetPluginVersion();

        internal static testSettingsJson DEFAULT_SETTINGS = new testSettingsJson
        {
            name = "FakePlugin2",
            exitTimeWaitSeconds = 5,
            version = new Version(16, 0),
        };

        private testSettingsJson GetTestSettings()
        {
            try
            {
                var path = Paths.DecloudPluginsPath(PluginUUID, "testSettings.json");
                string text = File.ReadAllText(path);
                var settingsObject = JsonConvert.DeserializeObject<testSettingsJson>(text);
                if (settingsObject != null) return settingsObject;
            }
            catch { }
            return DEFAULT_SETTINGS;
        }


        private string GetPluginName()
        {
            var settingsObject = GetTestSettings();
            return settingsObject.name;
        }

        private Version GetPluginVersion()
        {
            var settingsObject = GetTestSettings();
            return settingsObject.version;
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = devices.Select(device => (device, algorithms: GetSupportedAlgorithmsForDevice(device)))
                .Where(p => p.algorithms.Any())
                .ToDictionary(p => p.device, p => p.algorithms);
            return supported;
        }

        public FakePlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            DecloudOptionsPackage = PluginInternalSettings.DecloudOptionsPackage;
            GetApiMaxTimeoutConfig = PluginInternalSettings.GetApiMaxTimeoutConfig;
            DefaultTimeout = PluginInternalSettings.DefaultTimeout;
            DecloudBinsUrlsSettings = new DecloudBinsUrlsSettings
            {
                BinVersion = "1.83",
                ExePath = new List<string> { "DemoDecloud.exe" },
                Urls = new List<string>
                {
                    "https://github.com/Decloud/DCL_DecloudPluginsDownloads/releases/download/binVer/DemoDecloud.zip", // original
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "Fake Decloud - High-performance Decloud for NVIDIA and AMD GPUs.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }

        public override string Author => "info@Decloud.com";

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            return new List<string>() { };
        }
        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            var settingsObject = GetTestSettings();
            var rebenchAlgos = settingsObject.rebenchmarkAlgorithms;
            if (rebenchAlgos == null) return false;
            var isReBenchVersion = benchmarkedPluginVersion.Major == Version.Major && benchmarkedPluginVersion.Minor < Version.Minor;
            var first = ids.FirstOrDefault();
            var isInList = rebenchAlgos.Contains(first);
            return isReBenchVersion && isInList;
        }
        protected override DecloudBase CreateDecloudBase()
        {
            return new FakeDecloud(PluginUUID);
        }
    }
}
