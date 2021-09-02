using Newtonsoft.Json;
using DCL.Common;
using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.Interfaces;
using DCLCore.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using InternalConfigsCommon = DCL.Common.Configs.InternalConfigs;

namespace DCLCore.Mining.Plugins
{
    public class EthlargementIntegratedPlugin : NotifyChangedBase, IDecloudPlugin, IInitInternals, IBackgroundService, IBinaryPackageMissingFilesChecker, IDecloudBinsSource, IGetPluginMetaInfo
    {
        #region IDecloudPlugin
        public Version Version => new Version(15, 0);
        public string Name => "Ethlargement";

        public string Author => "info@Decloud.com";

        public string PluginUUID => "Ethlargement";

        #region IDecloudPlugin stubs
        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices) => new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
        public IDecloud CreateDecloud() => null;
        public bool CanGroup(MiningPair a, MiningPair b) => false;
        #endregion IDecloudPlugin stubs
        #endregion IDecloudPlugin

        private EthlargementIntegratedPlugin() { }
        public static EthlargementIntegratedPlugin Instance { get; } = new EthlargementIntegratedPlugin();

        private static readonly HashSet<string> _activeDeviceUUIDs = new HashSet<string>();


        public bool ServiceEnabled { get; set; } = false;

        private static bool ShouldRun => _activeDeviceUUIDs.Any();

        private bool _systemContainsSupportedDevices = false;
        public bool SystemContainsSupportedDevices => _systemContainsSupportedDevices;

        public bool SystemContainsSupportedDevicesNotSystemElevated => SystemContainsSupportedDevices && !Helpers.IsElevated;

        // used inside XAML settings form
        public bool IsSystemElevated => Helpers.IsElevated;

        public bool IsInstalled
        {
            get
            {
                try
                {
                    // TODO if we relly on installed package with just checking the executable existance we will most likely fail somewhere along the line
                    return File.Exists(_ethlargementBinPath);
                }
                catch (Exception e)
                {
                    Logger.Error("ETHLARGEMENT", $"Ethlargement IsInstalled: {e.Message}");
                    return false;
                }
            }
        }

        public void Remove()
        {
            Stop();
            System.Threading.Thread.Sleep(500);
            try
            {
                File.Delete(_ethlargementBinPath);
            }
            catch (Exception e)
            {
                Logger.Error("ETHLARGEMENT", $"Ethlargement Remove: {e.Message}");
            }
        }

        private bool CanStartMiningPair(MiningPair pair)
        {
            try
            {
                if (!IsSupportedDeviceName(pair.Device.Name)) return false;
                if (!pair.Algorithm.IDs.Any(IsSupportedAlgorithm)) return false;
                if (!IsSupportedDecloudPluginUUID(pair.Algorithm.DecloudID)) return false;
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("ETHLARGEMENT", $"CanStartMiningPair error: {e.Message}");
                return false;
            }
        }

        private bool HasStartRequirements()
        {
            // we can start only on elevated systems with supported GPUs if the plugin is installed
            if (!Helpers.IsElevated) return false;
            if (!_systemContainsSupportedDevices) return false;
            if (!IsInstalled) return false;
            if (!ServiceEnabled) return false;

            return true;
        }

        private static object _startStopLock = new object();

        public void Start(IEnumerable<MiningPair> miningPairs)
        {
            lock (_startStopLock)
            {
                if (HasStartRequirements())
                {
                    // check if any mining pair is supported and set as active
                    var startDeviceUUIDs = miningPairs
                        .Where(CanStartMiningPair)
                        .Select(pair => pair.Device.UUID)
                        .ToArray();
                    foreach (var deviceUUID in startDeviceUUIDs) _activeDeviceUUIDs.Add(deviceUUID);
                    if (ShouldRun) StartEthlargementProcess();
                }
                else
                {
                    // Not sure about this but close stop in any other case
                    StopEthlargementProcess();
                }
            }
        }

        public void Stop(IEnumerable<MiningPair> miningPairs = null)
        {
            lock (_startStopLock)
            {
                // update _activeDeviceUUIDs 
                if (miningPairs == null)
                {
                    // stop all
                    _activeDeviceUUIDs.Clear();
                }
                else
                {
                    // check what mining pairs to stop
                    var devicesToStop = miningPairs
                        .Select(pair => pair.Device.UUID)
                        .Where(_activeDeviceUUIDs.Contains)
                        .ToArray();
                    foreach (var deviceUUID in devicesToStop) _activeDeviceUUIDs.Remove(deviceUUID);
                }

                // check if we should stop
                if (!ShouldRun)
                {
                    StopEthlargementProcess();
                }
            }
        }


        private string EthlargementOldBinPath()
        {
            var pluginRoot = Paths.DecloudPluginsPath(PluginUUID);
            var pluginRootBins = Path.Combine(pluginRoot, "bins", $"{11}.{1}");
            var binPath = Path.Combine(pluginRootBins, "OhGodAnETHlargementPill-r2.exe");
            return binPath;
        }

        public static readonly string BinName = "ETHlargementPill-r2.exe";

        public virtual string EthlargementBinPath()
        {
            return Paths.DecloudPluginsPath(PluginUUID, "bins", $"{Version.Major}.{Version.Minor}", BinName);
        }

        public virtual string EthlargementCwdPath()
        {
            return Paths.DecloudPluginsPath(PluginUUID, "bins", $"{Version.Major}.{Version.Minor}");
        }


        #region Ethlargement Process

        private static string _ethlargementBinPath = "";
        private static string _ethlargementCwdPath = "";

        private static Process _ethlargementProcess = null;

        private static bool IsEthlargementProcessRunning()
        {
            try
            {
                if (_ethlargementProcess == null) return false;
                return Process.GetProcessById(_ethlargementProcess.Id) != null;
            }
            catch
            {
                return false;
            }
        }

        private async static void ExitEvent(object sender, EventArgs e)
        {
            _ethlargementProcess = null;
            await Task.Delay(1000);
            // TODO add delay and check if it is running
            // lock and check
            if (ShouldRun)
            {
                StartEthlargementProcess();
            }
        }

        private static void StartEthlargementProcess()
        {
            if (IsEthlargementProcessRunning() == true) return;

            _ethlargementProcess = DecloudToolkit.CreateMiningProcess(_ethlargementBinPath, _ethlargementCwdPath, "", null);
            _ethlargementProcess.Exited += ExitEvent;

            try
            {
                if (_ethlargementProcess.Start())
                {
                    Logger.Info("ETHLARGEMENT", "Starting ethlargement...");
                    //Helpers.ConsolePrint("ETHLARGEMENT", "Starting ethlargement...");
                }
                else
                {
                    Logger.Info("ETHLARGEMENT", "Couldn't start ethlargement");
                    //Helpers.ConsolePrint("ETHLARGEMENT", "Couldn't start ethlargement");
                }
            }
            catch (Exception e)
            {
                Logger.Error("ETHLARGEMENT", $"Ethlargement wasn't able to start: {e.Message}");
            }
        }

        private static void StopEthlargementProcess()
        {
            if (IsEthlargementProcessRunning() == false) return;
            try
            {
                _ethlargementProcess.Exited -= ExitEvent;
                _ethlargementProcess.CloseMainWindow();
                if (!_ethlargementProcess.WaitForExit(10 * 1000))
                {
                    _ethlargementProcess.Kill();
                }

                _ethlargementProcess.Close();
                _ethlargementProcess = null;
            }
            catch (Exception e)
            {
                Logger.Error("ETHLARGEMENT", $"Ethlargement wasn't able to stop: {e.Message}");
            }
        }

        #endregion Ethlargement Process

        #region Internal settings

        public virtual void InitInternals() { } // STUB

        public void InitAndCheckSupportedDevices(IEnumerable<BaseDevice> devices)
        {
            // set ethlargement path
            _ethlargementBinPath = EthlargementBinPath();
            _ethlargementCwdPath = EthlargementCwdPath();

            // copy EthLargement binary 
            try
            {
                var oldPath = EthlargementOldBinPath();
                if (File.Exists(oldPath) && !File.Exists(_ethlargementBinPath))
                {
                    if (!Directory.Exists(_ethlargementCwdPath)) Directory.CreateDirectory(_ethlargementCwdPath);
                    File.Copy(oldPath, _ethlargementBinPath);
                }
            }
            catch
            { }

            // internals
            (DecloudBinsUrlsSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "DecloudBinsUrlsSettings.json"), DecloudBinsUrlsSettings);
            (_ethlargementSettings, _) = InternalConfigsCommon.GetDefaultOrFileSettings(Paths.DecloudPluginsPath(PluginUUID, "internals", "EthlargementSettings.json"), _ethlargementSettings);

            // Filter out supported ones
            _systemContainsSupportedDevices = devices.Any(dev => IsSupportedDeviceName(dev.Name));
            OnPropertyChanged(nameof(SystemContainsSupportedDevices));
            OnPropertyChanged(nameof(SystemContainsSupportedDevicesNotSystemElevated));
        }

        public class SupportedDevicesSettings : DCL.Common.Configs.IInternalSetting
        {
            [JsonProperty("use_user_settings")]
            public bool UseUserSettings { get; set; } = false;

            [JsonProperty("supported_device_names")]
            public List<string> SupportedDeviceNames { get; set; } = new List<string> { "1080", "1080 Ti", "Titan Xp", "TITAN Xp" };

            [JsonProperty("supported_algorithms")]
            public List<AlgorithmType> SupportedAlgorithms { get; set; } = new List<AlgorithmType> { AlgorithmType.DaggerHashimoto };

            [JsonProperty("ignore_Decloud_uuids")]
            public List<string> IgnoreDecloudPluginUUIDs { get; set; } = new List<string> { };
            // "f683f550-94eb-11ea-a64d-17be303ea466" NBDecloud
        }

        protected SupportedDevicesSettings _ethlargementSettings = new SupportedDevicesSettings { };

        protected bool IsSupportedAlgorithm(AlgorithmType algorithmType)
        {
            try
            {
                return _ethlargementSettings?.SupportedAlgorithms?.Contains(algorithmType) ?? false;
            }
            catch
            {
                return false;
            }
        }

        protected bool IsSupportedDeviceName(string deviceName)
        {
            try
            {
                var deviceNameLowered = deviceName.ToLower();
                var supportedPartsLowered = _ethlargementSettings?.SupportedDeviceNames.Select(name => name.ToLower());
                return supportedPartsLowered.Any(namePart => deviceNameLowered.Contains(namePart));
            }
            catch
            {
                return false;
            }
        }

        protected bool IsSupportedDecloudPluginUUID(string pluginUUID)
        {
            try
            {
                var ignore = _ethlargementSettings?.IgnoreDecloudPluginUUIDs?.Contains(pluginUUID) ?? false;
                return !ignore;
            }
            catch
            {
                return false;
            }
        }



        protected DecloudBinsUrlsSettings DecloudBinsUrlsSettings { get; set; } = new DecloudBinsUrlsSettings
        {
            Urls = new List<string> { "https://github.com/Virosa/ETHlargementPill/raw/master/ETHlargementPill-r2.exe" }
        };
        #endregion Internal settings

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            if (SystemContainsSupportedDevices)
            {
                return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles("", new List<string> { EthlargementBinPath() });
            }
            return Enumerable.Empty<string>();
        }

        #region IDecloudBinsSource
        public IEnumerable<string> GetDecloudBinsUrlsForPlugin()
        {
            if (DecloudBinsUrlsSettings == null || DecloudBinsUrlsSettings.Urls == null) return Enumerable.Empty<string>();
            return DecloudBinsUrlsSettings.Urls;
        }

        PluginMetaInfo IGetPluginMetaInfo.GetPluginMetaInfo()
        {
            return new PluginMetaInfo
            {
                PluginDescription = "ETHlargement increases DaggerHashimoto hashrate for NVIDIA 1080, 1080 Ti and Titan Xp GPUs.",
                SupportedDevicesAlgorithms = new Dictionary<DeviceType, List<AlgorithmType>> { { DeviceType.NVIDIA, new List<AlgorithmType> { AlgorithmType.DaggerHashimoto } } }
            };
        }

        #endregion IDecloudBinsSource
    }
}
