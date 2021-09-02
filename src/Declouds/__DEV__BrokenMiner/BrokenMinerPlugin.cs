using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCL.DecloudPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;

namespace BrokenDecloud
{
    public class BrokenDecloudPlugin : IDecloudPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2, IDecloudBinsSource
    {

        Version IDecloudPlugin.Version => GetValueOrErrorSettings.GetValueOrError("Version", new Version(16, 0));

        string IDecloudPlugin.Name => GetValueOrErrorSettings.GetValueOrError("Name", "Broken Plugin");

        string IDecloudPlugin.Author => GetValueOrErrorSettings.GetValueOrError("Author", "John Doe");

        string IDecloudPlugin.PluginUUID => GetValueOrErrorSettings.GetValueOrError("PluginUUID", "BrokenDecloudPluginUUID");

        bool IDecloudPlugin.CanGroup(MiningPair a, MiningPair b) => GetValueOrErrorSettings.GetValueOrError("CanGroup", false);

        IEnumerable<string> IBinaryPackageMissingFilesChecker.CheckBinaryPackageMissingFiles() =>
            GetValueOrErrorSettings.GetValueOrError("CheckBinaryPackageMissingFiles", new List<string> { "text_file_acting_as_exe.txt" });

        IDecloud IDecloudPlugin.CreateDecloud() => GetValueOrErrorSettings.GetValueOrError("CreateDecloud", new BrokenDecloud());

        TimeSpan IGetApiMaxTimeoutV2.GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs) => GetValueOrErrorSettings.GetValueOrError("GetApiMaxTimeout", new TimeSpan(1, 10, 5));
        bool IGetApiMaxTimeoutV2.IsGetApiMaxTimeoutEnabled => GetValueOrErrorSettings.GetValueOrError("IsGetApiMaxTimeoutEnabled", true);

        Dictionary<BaseDevice, IReadOnlyList<Algorithm>> IDecloudPlugin.GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
            // this will break the default loader
            ////// Fake device 
            //var gpu = new BaseDevice(DeviceType.NVIDIA, "FAKE-d97bdb7c-4155-9124-31b7-4743e16d3ac0", "GTX 1070 Ti", 0);
            //supported.Add(gpu, new List<Algorithm>() { new Algorithm("BrokenDecloudPluginUUID", AlgorithmType.ZHash), new Algorithm("BrokenDecloudPluginUUID", AlgorithmType.DaggerHashimoto) });
            // we support all devices
            foreach (var dev in devices)
            {
                supported.Add(dev, new List<Algorithm>() { new Algorithm("BrokenDecloudPluginUUID", AlgorithmType.ZHash) });
            }

            return GetValueOrErrorSettings.GetValueOrError("GetSupportedAlgorithms", supported);
        }

        void IInitInternals.InitInternals() => GetValueOrErrorSettings.SetError("InitInternals");

        bool IReBenchmarkChecker.ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids) =>
            GetValueOrErrorSettings.GetValueOrError("ShouldReBenchmarkAlgorithmOnDevice", false);

        IEnumerable<string> IDecloudBinsSource.GetDecloudBinsUrlsForPlugin()
        {
            return GetValueOrErrorSettings.GetValueOrError("GetDecloudBinsUrlsForPlugin", new List<string> { "https://github.com/Decloud/DecloudDownloads/releases/download/v1.0/BrokenDecloudPlugin.zip" });
        }
    }
}
