using DCL.Common;
using DCL.Common.Algorithm;
using DCL.Common.Configs;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using DCL.DecloudPluginToolkitV1.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using InternalConfigsCommon = DCL.Common.Configs.InternalConfigs;

namespace DCL.DecloudPluginToolkitV1
{
    // TODO add documentation
    public abstract class PluginBase : IDecloudPlugin, IInitInternals, IBinaryPackageMissingFilesChecker, IReBenchmarkChecker, IGetApiMaxTimeoutV2, IDecloudBinsSource, IBinAndCwdPathsGettter, IGetDecloudBinaryVersion, IGetPluginMetaInfo, IPluginSupportedAlgorithmsSettings, IGetDecloudOptionsPackage, IGetBinsPackagePassword
    {
        public static bool IS_CALLED_FROM_PACKER { get; set; } = false;
        protected abstract DecloudBase CreateDecloudBase();

        #region IDecloudPlugin
        public abstract Version Version { get; }
        public abstract string Name { get; }
        public abstract string Author { get; }
        public abstract string PluginUUID { get; }

        public virtual bool CanGroup(MiningPair a, MiningPair b)
        {
            var checkELPCompatibility = DecloudOptionsPackage?.GroupMiningPairsOnlyWithCompatibleOptions ?? false;
            var isSameAlgoType = DecloudToolkit.IsSameAlgorithmType(a.Algorithm, b.Algorithm);
            if (isSameAlgoType && checkELPCompatibility)
            {
                var ignoreDefaults = DecloudOptionsPackage.IgnoreDefaultValueOptions;
                var areGeneralOptionsCompatible = ExtraLaunchParametersParser.CheckIfCanGroup(a, b, DecloudOptionsPackage.GeneralOptions, ignoreDefaults);
                var areTemperatureOptionsCompatible = ExtraLaunchParametersParser.CheckIfCanGroup(a, b, DecloudOptionsPackage.TemperatureOptions, ignoreDefaults);
                return areGeneralOptionsCompatible && areTemperatureOptionsCompatible;
            }

            return isSameAlgoType;
        }


        public virtual IDecloud CreateDecloud()
        {
            var Decloud = CreateDecloudBase();
            Decloud.BinAndCwdPathsGettter = this; // set the paths interface
            Decloud.PluginSupportedAlgorithms = this; // dev fee, algo names
            // set internal settings
            if (DecloudOptionsPackage != null) Decloud.DecloudOptionsPackage = DecloudOptionsPackage;
            if (DecloudystemEnvironmentVariables != null) Decloud.DecloudystemEnvironmentVariables = DecloudystemEnvironmentVariables;
            if (DecloudReservedApiPorts != null) Decloud.DecloudReservedApiPorts = DecloudReservedApiPorts;
            if (DecloudBenchmarkTimeSettings != null) Decloud.DecloudBenchmarkTimeSettings = DecloudBenchmarkTimeSettings;
            if (DecloudCustomActionSettings != null) Decloud.DecloudCustomActionSettings = DecloudCustomActionSettings;
            return Decloud;
        }

        #endregion IDecloudPlugin

        public abstract Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices);

        protected PluginMetaInfo PluginMetaInfo { get; set; } = null;

        #region IInitInternals
        public virtual void InitInternals()
        {
            (DecloudystemEnvironmentVariables, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "DecloudystemEnvironmentVariables.json"), DecloudystemEnvironmentVariables);
            (DecloudOptionsPackage, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "DecloudOptionsPackage.json"), DecloudOptionsPackage);
            (DecloudReservedApiPorts, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "DecloudReservedPorts.json"), DecloudReservedApiPorts);
            (GetApiMaxTimeoutConfig, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "DecloudApiMaxTimeoutSetting.json"), GetApiMaxTimeoutConfig);
            (DecloudBenchmarkTimeSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "DecloudBenchmarkTimeSettings.json"), DecloudBenchmarkTimeSettings);
            (DecloudBinsUrlsSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "DecloudBinsUrlsSettings.json"), DecloudBinsUrlsSettings);
            (PluginSupportedAlgorithmsSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "PluginSupportedAlgorithmsSettings.json"), PluginSupportedAlgorithmsSettings);
            (DecloudCustomActionSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "DecloudCustomActionSettings.json"), DecloudCustomActionSettings);
        }

        // internal settings
        protected DecloudOptionsPackage DecloudOptionsPackage { get; set; } = new DecloudOptionsPackage { };
        protected DecloudystemEnvironmentVariables DecloudystemEnvironmentVariables { get; set; } = new DecloudystemEnvironmentVariables { };
        protected DecloudReservedPorts DecloudReservedApiPorts { get; set; } = new DecloudReservedPorts { };
        protected DecloudApiMaxTimeoutSetting GetApiMaxTimeoutConfig { get; set; } = new DecloudApiMaxTimeoutSetting { GeneralTimeout = new TimeSpan(0, 5, 0) };
        protected DecloudBenchmarkTimeSettings DecloudBenchmarkTimeSettings { get; set; } = new DecloudBenchmarkTimeSettings { };

        protected DecloudBinsUrlsSettings DecloudBinsUrlsSettings { get; set; } = new DecloudBinsUrlsSettings { };

        protected DecloudCustomActionSettings DecloudCustomActionSettings { get; set; } = new DecloudCustomActionSettings { };

        public PluginSupportedAlgorithmsSettings PluginSupportedAlgorithmsSettings { get; set; } = new PluginSupportedAlgorithmsSettings();

        // we must define this for every Decloud plugin
        protected abstract PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings { get; }

        protected void InitInsideConstuctorPluginSupportedAlgorithmsSettings()
        {
            PluginSupportedAlgorithmsSettings = DefaultPluginSupportedAlgorithmsSettings;
            if (IS_CALLED_FROM_PACKER) return;
            (PluginSupportedAlgorithmsSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "PluginSupportedAlgorithmsSettings.json"), DefaultPluginSupportedAlgorithmsSettings);
        }

        #endregion IInitInternals

        #region IReBenchmarkChecker
        public abstract bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids);
        #endregion IReBenchmarkChecker

        #region IGetApiMaxTimeoutV2
        public virtual bool IsGetApiMaxTimeoutEnabled => DecloudApiMaxTimeoutSetting.ParseIsEnabled(true, GetApiMaxTimeoutConfig);


        protected TimeSpan DefaultTimeout { get; set; } = new TimeSpan(0, 5, 0);

        public virtual TimeSpan GetApiMaxTimeout(IEnumerable<MiningPair> miningPairs)
        {
            return DecloudApiMaxTimeoutSetting.ParseMaxTimeout(DefaultTimeout, GetApiMaxTimeoutConfig, miningPairs);
        }
        #endregion IGetApiMaxTimeoutV2

        #region IBinaryPackageMissingFilesChecker
        public abstract IEnumerable<string> CheckBinaryPackageMissingFiles();
        #endregion IBinaryPackageMissingFilesChecker

        #region IDecloudBinsSource
        public virtual IEnumerable<string> GetDecloudBinsUrlsForPlugin()
        {
            if (DecloudBinsUrlsSettings == null || DecloudBinsUrlsSettings.Urls == null) return Enumerable.Empty<string>();
            return DecloudBinsUrlsSettings.Urls;
        }
        #endregion IDecloudBinsSource

        #region IBinAndCwdPathsGettter
        public virtual (string binPath, string cwdPath) GetBinAndCwdPaths()
        {
            if (DecloudBinsUrlsSettings == null || DecloudBinsUrlsSettings.ExePath == null || DecloudBinsUrlsSettings.ExePath.Count == 0)
            {
                throw new Exception("Unable to return cwd and exe paths DecloudBinsUrlsSettings == null || DecloudBinsUrlsSettings.Path == null || DecloudBinsUrlsSettings.Path.Count == 0");
            }
            var paths = new List<string> { Paths.DecloudPluginsPath(PluginUUID, "bins", $"{Version.Major}.{Version.Minor}" ) };
            paths.AddRange(DecloudBinsUrlsSettings.ExePath);
            var binCwd = Path.Combine(paths.GetRange(0, paths.Count - 1).ToArray());
            var binPath = Path.Combine(paths.ToArray());
            return (binPath, binCwd);
        }
        #endregion IBinAndCwdPathsGettter

        #region IGetDecloudBinaryVersion
        public string GetDecloudBinaryVersion()
        {
            if (DecloudBinsUrlsSettings == null || DecloudBinsUrlsSettings.BinVersion == null)
            {
                // return this or throw???
                return "N/A";
            }
            return DecloudBinsUrlsSettings.BinVersion;
        }
        #endregion IGetDecloudBinaryVersion

        #region IGetPluginMetaInfo
        public PluginMetaInfo GetPluginMetaInfo()
        {
            return PluginMetaInfo;
        }
        #endregion IGetPluginMetaInfo

        #region IGetDecloudOptionsPackage
        DecloudOptionsPackage IGetDecloudOptionsPackage.GetDecloudOptionsPackage() => DecloudOptionsPackage;
        #endregion IGetDecloudOptionsPackage
        #region IPluginSupportedAlgorithmsSettings
        public virtual bool UnsafeLimits()
        {
            return PluginSupportedAlgorithmsSettings.EnableUnsafeRAMLimits;
        }

        public virtual Dictionary<DeviceType, List<AlgorithmType>> SupportedDevicesAlgorithmsDict()
        {
            DeviceType[] deviceTypes = new DeviceType[] { DeviceType.CPU, DeviceType.AMD, DeviceType.NVIDIA };
            var ret = new Dictionary<DeviceType, List<AlgorithmType>> { };
            foreach (var deviceType in deviceTypes)
            {
                var algos = GetSupportedAlgorithmsForDeviceType(deviceType);
                if (algos.Count == 0) continue;
                ret[deviceType] = new HashSet<AlgorithmType>(algos.SelectMany(a => a.IDs)).ToList();
            }
            return ret;
        }

        public virtual List<Algorithm> GetSupportedAlgorithmsForDeviceType(DeviceType deviceType)
        {
            if (PluginSupportedAlgorithmsSettings.Algorithms?.ContainsKey(deviceType) ?? false)
            {
                var sass = PluginSupportedAlgorithmsSettings.Algorithms[deviceType];
                return sass.Select(sas => sas.ToAlgorithm(PluginUUID)).ToList();
            }
            return new List<Algorithm>(); // return empty
        }

        public virtual string AlgorithmName(params AlgorithmType[] algorithmTypes)
        {
            if (algorithmTypes.Length == 1)
            {
                var id = algorithmTypes[0];
                if (PluginSupportedAlgorithmsSettings.AlgorithmNames != null && PluginSupportedAlgorithmsSettings.AlgorithmNames.ContainsKey(id))
                {
                    return PluginSupportedAlgorithmsSettings.AlgorithmNames[id];
                }
            }
            return "";
        }

        public virtual double DevFee(params AlgorithmType[] algorithmTypes)
        {
            if (algorithmTypes.Length == 1)
            {
                var id = algorithmTypes[0];
                if (PluginSupportedAlgorithmsSettings.AlgorithmFees?.ContainsKey(id) ?? false)
                {
                    return PluginSupportedAlgorithmsSettings.AlgorithmFees[id];
                }
            }
            return PluginSupportedAlgorithmsSettings.DefaultFee;
        }
        #endregion IPluginSupportedAlgorithmsSettings

        protected Dictionary<AlgorithmType, ulong> GetCustomMinimumMemoryPerAlgorithm(DeviceType deviceType)
        {
            var ret = new Dictionary<AlgorithmType, ulong>();
            if (PluginSupportedAlgorithmsSettings.Algorithms?.ContainsKey(deviceType) ?? false)
            {
                var sass = PluginSupportedAlgorithmsSettings.Algorithms[deviceType];
                var customRAMLimits = sass.Where(sas => sas.NonDefaultRAMLimit.HasValue);
                foreach (var el in customRAMLimits)
                {
                    ret[el.IDs.First()] = el.NonDefaultRAMLimit.Value;
                }
            }
            return ret;
        }

        public IReadOnlyList<Algorithm> GetSupportedAlgorithmsForDevice(BaseDevice dev)
        {
            var deviceType = dev.DeviceType;
            var algorithms = GetSupportedAlgorithmsForDeviceType(deviceType);
            var gpu = dev as IGpuDevice;
            if (UnsafeLimits() || gpu == null) return algorithms;
            // GPU RAM filtering
            var ramLimits = GetCustomMinimumMemoryPerAlgorithm(deviceType);
            var filteredAlgorithms = Filters.FilterInsufficientRamAlgorithmsListCustom(gpu.GpuRam, algorithms, ramLimits);
            return filteredAlgorithms;
        }

        public virtual string BinsPackagePassword
        {
            get
            {
                try
                {
                    return DecloudBinsUrlsSettings?.BinsPackagePassword ?? null;
                }
                catch { }
                return null;
            }
        }
    }
}
