using Newtonsoft.Json;
using DCL.Common;
using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCL.DecloudPluginLoader;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Interfaces;
using DCL.DecloudDownloader;
using DCLCore.ApplicationState;
using DCLCore.Configs;
using DCLCore.Notifications;
using DCLCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DCLCore.Mining.Plugins
{
    public static class DecloudPluginsManager
    {
        private static readonly List<IDecloudPlugin> _integratedPlugins;
        static DecloudPluginsManager()
        {
            // This is just a list of Decloud that are intergated in the DCL client. usefull when debuging
            _integratedPlugins = new List<IDecloudPlugin>
            {
                // __DEV__*          
#if INTEGRATE_BrokenDecloud_PLUGIN
                new BrokenDecloud.BrokenDecloudPlugin(),
#endif
#if INTEGRATE_ExamplePlugin_PLUGIN
                new Example.ExamplePlugin(),
#endif
#if INTEGRATE_FakePlugin_PLUGIN
                new FakePlugin.FakePlugin(),
#endif

// real Decloud
#if INTEGRATE_GDecloud_PLUGIN
                new GDecloudPlugin.GDecloudPlugin(),
#endif
#if INTEGRATE_NBDecloud_PLUGIN
                new NBDecloud.NBDecloudPlugin(),
#endif
#if INTEGRATE_Phoenix_PLUGIN
                new Phoenix.PhoenixPlugin(),
#endif
#if INTEGRATE_TeamRedDecloud_PLUGIN
                new TeamRedDecloud.TeamRedDecloudPlugin(),
#endif
#if INTEGRATE_TRex_PLUGIN
                new TRex.TRexPlugin(),
#endif
#if INTEGRATE_TTDecloud_PLUGIN
                new TTDecloud.TTDecloudPlugin(),
#endif
#if INTEGRATE_NanoDecloud_PLUGIN
                new NanoDecloud.NanoDecloudPlugin(),
#endif
#if INTEGRATE_WildRig_PLUGIN
                new WildRig.WildRigPlugin(),
#endif
#if INTEGRATE_CryptoDredge_PLUGIN
                new CryptoDredge.CryptoDredgePlugin(),
#endif
#if INTEGRATE_ZEnemy_PLUGIN
                new ZEnemy.ZEnemyPlugin(),
#endif
#if INTEGRATE_LolDecloud_PLUGIN
                new LolDecloud.LolDecloudPlugin(),
#endif
#if INTEGRATE_SRBDecloud_PLUGIN
                new SRBDecloud.SRBDecloudPlugin(),
#endif
#if INTEGRATE_XMRig_PLUGIN
                new XMRig.XMRigPlugin(),
#endif
#if INTEGRATE_MiniZ_PLUGIN
                new MiniZ.MiniZPlugin(),
#endif

#if INTEGRATE_ALL_PLUGINS
                new GDecloudPlugin.GDecloudPlugin(),
                new NBDecloud.NBDecloudPlugin(),
                new Phoenix.PhoenixPlugin(),
                new TeamRedDecloud.TeamRedDecloudPlugin(),
                new TRex.TRexPlugin(),
                new TTDecloud.TTDecloudPlugin(),
                new NanoDecloud.NanoDecloudPlugin(),
                new WildRig.WildRigPlugin(),
                new CryptoDredge.CryptoDredgePlugin(),
                new ZEnemy.ZEnemyPlugin(),
                new LolDecloud.LolDecloudPlugin(),
                new SRBDecloud.SRBDecloudPlugin(),
                new MiniZ.MiniZPlugin(),
#endif


                // service plugin
                EthlargementIntegratedPlugin.Instance,

                // plugin dependencies
                VC_REDIST_x64_2015_2019_DEPENDENCY_PLUGIN.Instance
            };

            (_initOnlinePlugins, OnlinePlugins) = ReadCachedOnlinePlugins();
        }

        private static readonly bool _initOnlinePlugins;

        // API data
        private static List<PluginPackageInfo> OnlinePlugins { get; set; }
        private static Dictionary<string, PluginPackageInfoCR> PluginsPackagesInfosCRs { get; set; } = new Dictionary<string, PluginPackageInfoCR>();

        //public static PluginPackageInfoCR GetPluginPackageInfoCR(string pluginUUID)
        //{
        //    if (PluginsPackagesInfosCRs.ContainsKey(pluginUUID)) return PluginsPackagesInfosCRs[pluginUUID];
        //    return null;
        //}

        public static IEnumerable<PluginPackageInfoCR> RankedPlugins
        {
            get
            {
                return PluginsPackagesInfosCRs
                    .Select(kvp => kvp.Value)
                    .OrderByDescending(info => info.HasNewerVersion)
                    .ThenByDescending(info => info.OnlineSupportedDeviceCount)
                    .ThenBy(info => info.PluginName);
            }
        }

        #region Update Decloud plugin dlls
        public static void CheckAndDeleteUnsupportedPlugins()
        {
            try
            {
                foreach (var obsolete in Checkers.ObsoleteDecloudPlugins)
                {
                    try
                    {
                        var obsoletePath = Paths.DecloudPluginsPath(obsolete);
                        if (Directory.Exists(obsoletePath))
                        {
                            Directory.Delete(obsoletePath, true);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("CheckAndDeleteUnsupportedPlugins", e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("CheckAndDeleteUnsupportedPlugins", e.Message);
            }
        }

        #endregion Update Decloud plugin dlls

        public static async Task LoadAndInitDecloudPlugins()
        {
            try
            {
                var filteredIntegratedPlugins = _integratedPlugins
                    .Where(p => BlacklistedPlugins.IsNotBlacklisted(p.PluginUUID))
                    .Select(PluginContainer.Create);

                // TODO ADD STEP AND MESSAGE
                EthlargementIntegratedPlugin.Instance.InitAndCheckSupportedDevices(AvailableDevices.Devices.Select(dev => dev.BaseDevice));
                CheckAndDeleteUnsupportedPlugins();

                // load dll's and create plugin containers
                var externalLoadedPlugins = DecloudPluginHost.LoadPlugins(Paths.DecloudPluginsPath(new string[] { }))
                    .Where(BlacklistedPlugins.IsNotBlacklisted)
                    .Where(DecloudPluginHost.DecloudPlugin.ContainsKey)
                    .Select(pluginUUID => DecloudPluginHost.DecloudPlugin[pluginUUID])
                    .Select(PluginContainer.Create);

                var loadedPlugins = new List<PluginContainer>();
                loadedPlugins.AddRange(filteredIntegratedPlugins);
                loadedPlugins.AddRange(externalLoadedPlugins);
                // init all containers
                foreach (var plugin in loadedPlugins)
                {
                    if (!plugin.IsInitialized)
                    {
                        plugin.InitPluginContainer();
                    }

                    if (plugin.Enabled)
                    {
                        plugin.AddAlgorithmsToDevices();
                    }
                    else
                    {
                        plugin.RemoveAlgorithmsFromDevices();
                    }
                }
                // cross reference local and online list
                var success = await GetOnlineDecloudPlugins();
                if (success) CrossReferenceInstalledWithOnline();
                CheckAccepted3rdPartyPlugins();
                Logger.Info("DecloudPluginsManager", "Finished initialization of Decloud.");
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginsManager", $"Initialization of Decloud error {e}.");
            }
        }

        public static Task RunninLoops { get; private set; } = null;

        public static void StartLoops(CancellationToken stop)
        {
            RunninLoops = Task.Run(() =>
            {
                var loop1 = PluginsUpdaterLoop(stop);
                var loop2 = PluginInstaller.RestartDevicesStateLoop(stop);
                return Task.WhenAll(loop1, loop2);
            });
        }

        private static async Task PluginsUpdaterLoop(CancellationToken stop)
        {
            try
            {
                var checkWaitTime = TimeSpan.FromMilliseconds(50);
                Func<bool> isActive = () => !stop.IsCancellationRequested;


                // TODO set this interval somwhere
                // periodically update the plugin list
                var getOnlineDecloudPluginsElapsedTimeChecker = new ElapsedTimeChecker(DecloudPluginsUpdaterSettings.CheckPluginsInterval, false);


                // TODO for now every minute check
                // TODO debug only we should check plugin updates after we update the Decloud plugin API
                //var pluginsUpdateElapsedTimeChecker = new ElapsedTimeChecker(TimeSpan.FromSeconds(30), false);

                Logger.Debug("DecloudPluginsManager", $"STARTING MAIN LOOP");
                while (isActive())
                {
                    try
                    {
                        if (isActive()) await TaskHelpers.TryDelay(checkWaitTime, stop);

                        var execAutoUpdate = false;
                        if (getOnlineDecloudPluginsElapsedTimeChecker.CheckAndMarkElapsedTime())
                        {
                            Logger.Debug("DecloudPluginsManager", $"Checking for plugin updates");
                            var success = await GetOnlineDecloudPlugins();
                            if (success) CrossReferenceInstalledWithOnline();
                            var logValue = success ? "SUCCESS" : "FAIL";
                            Logger.Debug("DecloudPluginsManager", $"Checking for plugin updates returned {logValue}");
                            execAutoUpdate = success && UpdateSettings.Instance.AutoUpdateDecloudPlugins;
                        }

                        if (isActive() && execAutoUpdate)
                        {
                            Logger.Debug("DecloudPluginsManager", $"Checking plugins to Install/Update");
                            var pluginsThatCanAutoUpdate = PluginsPackagesInfosCRs.Values
                                .Where(p => p.Installed)
                                .Where(p => p.IsAutoUpdateEnabled)
                                .Where(p => p.HasNewerVersion)
                                .Where(p => p.CompatibleNHPluginVersion)
                                .Where(p => p.Supported)
                                .Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID))
                                .Where(p => DecloudPluginInstallTasks.ContainsKey(p.PluginUUID) == false) // skip if update is already in progress
                                .Select(p => p.PluginUUID)
                                .ToArray();
                            foreach (var pluginUUID in pluginsThatCanAutoUpdate)
                            {
                                Logger.Debug("DecloudPluginsManager", $"Main loop Install/Update {pluginUUID}");
                                _ = _DecloudPluginInstallTasksProgress.TryGetValue(pluginUUID, out var progress);
                                _ = DownloadAndInstall(pluginUUID, progress, stop);
                            }
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        Logger.Info("DecloudPluginsManager", $"Main Loop TaskCanceledException {e.Message}");
                        return;
                    }
                    catch (Exception e)
                    {
                        Logger.Error("DecloudPluginsManager", $"Main Loop Tick Exception {e.Message}");
                    }
                }
            }
            finally
            {
                Logger.Debug("DecloudPluginsManager", $"EXITING MAIN LOOP");
                // cleanup
                foreach (var installTask in DecloudPluginInstallTasks.Values)
                {
                    installTask.TryCancelInstall();
                }
            }
        }

        private static class PluginInstaller
        {
            private static readonly TrivialChannel<IPluginInstallerCommand> Channel = new TrivialChannel<IPluginInstallerCommand>();
            private interface IPluginInstallerCommand { string PluginUUID { get; set; } };
            private class RemoveCommand : IPluginInstallerCommand { public string PluginUUID { get; set; } }
            private class RemovedCommand : IPluginInstallerCommand { public string PluginUUID { get; set; } public bool Success { get; set; } }
            private class InstallCommand : IPluginInstallerCommand { public string PluginUUID { get; set; } }
            private class InstalledCommand : IPluginInstallerCommand { public string PluginUUID { get; set; } public bool Success { get; set; } }

            private static bool IsRemovalCommand(IPluginInstallerCommand c) => c is RemoveCommand || c is RemovedCommand;
            private static bool IsInstallationCommand(IPluginInstallerCommand c) => c is InstallCommand || c is InstalledCommand;

            public static void RemovePlugin(string pluginUUID)
            {
                Channel.Enqueue(new RemoveCommand { PluginUUID = pluginUUID });
            }

            public static void RemovedPluginStatus(string pluginUUID, bool success)
            {
                Channel.Enqueue(new RemovedCommand { PluginUUID = pluginUUID, Success = success });
            }

            public static void InstallPlugin(string pluginUUID)
            {
                Channel.Enqueue(new InstallCommand { PluginUUID = pluginUUID });
            }

            public static void InstalledPluginStatus(string pluginUUID, bool success)
            {
                Channel.Enqueue(new InstalledCommand { PluginUUID = pluginUUID, Success = success });
            }

            public static async Task RestartDevicesStateLoop(CancellationToken stop)
            {
                var lastCommandTime = DateTime.UtcNow;
                Func<bool> checkCommandsForRestart = () => (DateTime.UtcNow - lastCommandTime).TotalSeconds >= 0.5;
                var pairedCommands = new Dictionary<string, List<IPluginInstallerCommand>>();
                var pluginsToDelete = new List<string>();
                try
                {
                    var checkWaitTime = TimeSpan.FromMilliseconds(50);
                    Func<bool> isActive = () => !stop.IsCancellationRequested;
                    Logger.Info("PluginInstaller", "Starting RestartDevicesStateLoop");
                    while (isActive())
                    {
                        if (isActive()) await TaskHelpers.TryDelay(checkWaitTime, stop);

                        // TODO check last command time and after a delay execute device restart
                        if (pairedCommands.Any() && checkCommandsForRestart())
                        {
                            var partitionedCommands = pairedCommands.Keys
                                .ToArray()
                                .Select(pluginUUID => (pluginUUID, commands: pairedCommands[pluginUUID]))
                                .Select(p => (p.pluginUUID, removal: p.commands.Where(IsRemovalCommand), installation: p.commands.Where(IsInstallationCommand)))
                                .ToArray();

                            var currentPluginsToDelete = partitionedCommands.Where(p => p.removal.Count() > 1)
                                .Where(p => p.installation.Count() == 0)
                                .Select(p => p.pluginUUID)
                                .ToArray();
                            pluginsToDelete.AddRange(currentPluginsToDelete);

                            foreach (var (pluginUUID, removal, installation) in partitionedCommands)
                            {
                                if (removal.Any() && installation.Any())
                                {
                                    Logger.Error("PluginInstaller", $"Plugin {pluginUUID} has installation and removal commands at same time!!!");
                                    pairedCommands.Remove(pluginUUID);
                                }
                                else if (removal.Count() > 1 || installation.Count() > 1)
                                {
                                    pairedCommands.Remove(pluginUUID);
                                }
                            }
                            // when we have no commands pending restart devices
                            if (!pairedCommands.Any())
                            {
                                await ApplicationStateManager.RestartDevicesState();
                                var deletePluginTasks = pluginsToDelete
                                    .Distinct()
                                    .Select(DelayedPluginDelete)
                                    .ToArray();
                                pluginsToDelete.Clear();
                                _ = Task.WhenAll(deletePluginTasks); // TODO await or leave
                            }
                        }

                        // command handling
                        var (command, hasTimedout, exceptionString) = await Channel.ReadAsync(checkWaitTime, stop);
                        if (command == null)
                        {
                            if (exceptionString != null) Logger.Error("PluginInstaller", $"Channel.ReadAsync error: {exceptionString}");
                            continue;
                        }
                        // handle commands
                        if (!pairedCommands.ContainsKey(command.PluginUUID)) pairedCommands[command.PluginUUID] = new List<IPluginInstallerCommand>() { };
                        pairedCommands[command.PluginUUID].Add(command);
                        lastCommandTime = DateTime.UtcNow;
                    }
                }
                catch (TaskCanceledException e)
                {
                    Logger.Debug("PluginInstaller", $"RestartDevicesStateLoop TaskCanceledException: {e.Message}");
                }
                finally
                {
                    Logger.Info("PluginInstaller", "Exiting RestartDevicesStateLoop run cleanup");
                    // cleanup
                }
            }
        }

        public static async Task DevicesCrossReferenceIDsWithDecloudIndexes(IStartupLoader loader)
        {
            // get devices
            var baseDevices = AvailableDevices.Devices.Select(dev => dev.BaseDevice);
            var checkPlugins = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                .Where(p => p.HasDevicesCrossReference())
                //.Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID)) // WARNING We still want to mine with these 
                .ToArray();

            if (checkPlugins.Length > 0 && loader != null)
            {
                loader.SecondaryVisible = true;
                loader.SecondaryTitle = Translations.Tr("Devices Cross Reference");
                loader?.SecondaryProgress?.Report((Translations.Tr("Pending"), 0));
            }
            var pluginDoneCount = 0d;
            foreach (var plugin in checkPlugins)
            {
                try
                {
                    Logger.Info("DecloudPluginsManager", $"Cross Reference {plugin.Name}_{plugin.Version}_{plugin.PluginUUID}");
                    loader?.SecondaryProgress?.Report((Translations.Tr("Cross Reference {0}", plugin.Name), (int)((pluginDoneCount / checkPlugins.Length) * 100)));
                    await plugin.DevicesCrossReference(baseDevices);
                    pluginDoneCount += 1;
                    loader?.SecondaryProgress?.Report((Translations.Tr("Cross Reference {0}", plugin.Name), (int)((pluginDoneCount / checkPlugins.Length) * 100)));
                }
                catch (Exception e)
                {
                    Logger.Error("DecloudPluginsManager", $"DevicesCrossReferenceIDsWithDecloudIndexes error: {e.Message}");
                }
            }
            if (loader != null)
            {
                loader.SecondaryVisible = false;
            }
        }

        public static async Task DownloadMissingDecloudBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            var pluginsWithMissingPackageFiles = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                .Where(p => p.HasMisingBinaryPackageFiles())
                .Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID))
                .Select(p => (p, p.GetDecloudBinsUrls().ToList()))
                .Where<(PluginContainer p, List<string> urls)>(pair => pair.urls.Any())
                .ToArray();

            foreach (var (plugin, urls) in pluginsWithMissingPackageFiles)
            {
                Logger.Info("DecloudPluginsManager", $"Downloading missing files for {plugin.PluginUUID}-{plugin.Name}");
                var downloadProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Downloading {0} %", $"{plugin.Name} {perc}"), perc)));
                var unzipProgress = new Progress<int>(perc => progress?.Report((Translations.Tr("Extracting {0} %", $"{plugin.Name} {perc}"), perc)));
                await DownloadInternalBins(plugin, urls, downloadProgress, unzipProgress, stop);
                // check if we have missing files after the download 
                if (plugin.HasMisingBinaryPackageFiles()) AvailableNotifications.CreateMissingDecloudBinsInfo(plugin.Name);
            }
        }

        private static string PluginInstallProgressStateToString(PluginInstallProgressState state, string pluginName, int progressPerc)
        {
            switch (state)
            {
                case PluginInstallProgressState.DownloadingDecloud:
                    return Translations.Tr("Downloading Decloud: {0}%", $"{pluginName} {progressPerc}");
                case PluginInstallProgressState.DownloadingPlugin:
                    return Translations.Tr("Downloading Plugin: {0}%", $"{pluginName} {progressPerc}");
                case PluginInstallProgressState.ExtractingDecloud:
                    return Translations.Tr("Extracting Decloud: {0}%", $"{pluginName} {progressPerc}");
                case PluginInstallProgressState.ExtractingPlugin:
                    return Translations.Tr("Extracting Plugin: {0}%", $"{pluginName} {progressPerc}");
                default:
                    return Translations.Tr("Pending Install") + $" {pluginName}";
            }
        }


        public static async Task UpdateDecloudBins(IProgress<(string loadMessageText, int prog)> progress, CancellationToken stop)
        {
            Func<PluginContainer, bool> hasUpdate = (p) =>
            {
                return PluginsPackagesInfosCRs.TryGetValue(p.PluginUUID, out var pcr) && pcr.HasNewerVersion;
            };
            var pluginsToUpdate = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                .Where(hasUpdate)
                .Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID))
                .ToArray();

            foreach (var plugin in pluginsToUpdate)
            {
                var wrappedProgress = new Progress<Tuple<PluginInstallProgressState, int>>(status =>
                {
                    var (state, progressPerc) = status;
                    string statusText = PluginInstallProgressStateToString(state, plugin.Name, progressPerc);
                    progress?.Report((statusText, progressPerc));
                });
                await DownloadAndInstall(plugin.PluginUUID, wrappedProgress, stop);
            }
        }

        internal static bool CanFallbackAndMineWithPlugin(IDecloudPlugin plugin)
        {
            try
            {
                // call to check if it throws and exception
                var uuid = plugin.PluginUUID;
                var version = plugin.Version;
                // finally we must guarantee that we don't have any missing Decloud files
                return plugin is IBinaryPackageMissingFilesChecker impl && impl.CheckBinaryPackageMissingFiles().Count() == 0;
            }
            catch (Exception)
            {
                // for what ever reason skip this plugin
                return false;
            }
        }

        public static List<PluginPackageInfoCR> EulaConfirm { get; private set; } = new List<PluginPackageInfoCR>();

        private static void CheckAccepted3rdPartyPlugins()
        {
            var nonAcceptedlugins = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                .Where(p => !AcceptedPlugins.IsAccepted(p.PluginUUID))
                .ToArray();
            var nonAcceptedluginsUUIDs = nonAcceptedlugins
                .Select(p => p.PluginUUID)
                .ToArray();
            EulaConfirm = RankedPlugins.Where(pcr => nonAcceptedluginsUUIDs.Contains(pcr.PluginUUID)).ToList();
            EulaConfirm.ForEach(el =>
            {
                Logger.Info("DecloudPluginsManager", $"Plugin is not accepted {el.PluginUUID}-{el.PluginName}. Skipping...");
                el.IsUserActionRequired = true;
            });
            var nonAcceptedPluginsWithMissingBinaries = nonAcceptedlugins
                .Where(p => p.HasMisingBinaryPackageFiles())
                .Where(p => Directory.Exists(Paths.DecloudPluginsPath(p.PluginUUID, "dlls")))
                .ToArray();
            foreach (var p in nonAcceptedPluginsWithMissingBinaries)
            {
                try
                {
                    var oldPlugins = Directory.GetFiles(Paths.DecloudPluginsPath(p.PluginUUID, "dlls"), "*.dll", SearchOption.AllDirectories)
                        .SelectMany(DecloudPluginHost.LoadPluginsFromDllFile)
                        .Where(CanFallbackAndMineWithPlugin)
                        .Where(plugin => plugin.PluginUUID == p.PluginUUID)
                        .OrderByDescending(plugin => plugin.Version)
                        .ToList();
                    if (oldPlugins.Any()) PluginContainer.RemovePluginContainer(p);
                    foreach (var fallbackPlugin in oldPlugins)
                    {
                        // init all containers
                        var plugin = PluginContainer.Create(fallbackPlugin);
                        if (!plugin.IsInitialized)
                        {
                            plugin.InitPluginContainer();
                        }

                        if (plugin.Enabled)
                        {
                            plugin.AddAlgorithmsToDevices();
                            break; // we are good stop fallback plugins init
                        }
                        else
                        {
                            plugin.RemoveAlgorithmsFromDevices();
                            PluginContainer.RemovePluginContainer(plugin);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("DecloudPluginsManager", $"Error setting fallback plugin '{p.PluginUUID}': {e.Message}");
                }
            }
        }

        public static bool HasMissingDecloud()
        {
            var anyPluginWithMissingPackageFiles = PluginContainer.PluginContainers
                .Where(p => p.Enabled)
                .Where(p => AcceptedPlugins.IsAccepted(p.PluginUUID))
                .Any(p => p.HasMisingBinaryPackageFiles());
            return anyPluginWithMissingPackageFiles;
        }

        private static async Task DelayedPluginDelete(string pluginUUID)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var deletePath = Paths.DecloudPluginsPath(pluginUUID);
            var start = DateTime.UtcNow;
            while (true)
            {
                var elapsedAfterStart = DateTime.UtcNow - start;
                if (elapsedAfterStart.TotalSeconds > 15) return;
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                try
                {
                    if (Directory.Exists(deletePath))
                    {
                        Directory.Delete(deletePath, true);
                        return;
                    }
                }
                catch { }
            }
        }

        public static async Task RemovePlugin(string pluginUUID, bool crossReferenceInstalledWithOnline = true)
        {
            BlacklistedPlugins.AddToBlacklist(pluginUUID);
            try
            {
                PluginInstaller.RemovePlugin(pluginUUID);
                if (EthlargementIntegratedPlugin.Instance.PluginUUID == pluginUUID) EthlargementIntegratedPlugin.Instance.Remove();

                AcceptedPlugins.Remove(pluginUUID);
                DecloudPluginHost.DecloudPlugin.Remove(pluginUUID);
                var oldPlugins = PluginContainer.PluginContainers.Where(p => p.PluginUUID == pluginUUID).ToArray();
                foreach (var old in oldPlugins) PluginContainer.RemovePluginContainer(old);

                // remove from cross ref dict
                if (PluginsPackagesInfosCRs.ContainsKey(pluginUUID))
                {
                    PluginsPackagesInfosCRs[pluginUUID].LocalInfo = null;
                    // we might not have any online reference so remove it in this case
                    if (PluginsPackagesInfosCRs[pluginUUID].OnlineInfo == null)
                    {
                        PluginsPackagesInfosCRs.Remove(pluginUUID);
                    }
                }

                if (crossReferenceInstalledWithOnline) CrossReferenceInstalledWithOnline();
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginsManager", $"Error occured while removing {pluginUUID} plugin: {e.Message}");
            }
            finally
            {
                PluginInstaller.RemovedPluginStatus(pluginUUID, true);
                await Task.CompletedTask;
            }
        }

        private static void CrossReferenceInstalledWithOnlineEthlargementIntegratedPlugin()
        {
            if (!EthlargementIntegratedPlugin.Instance.SystemContainsSupportedDevices) return;
            var uuid = EthlargementIntegratedPlugin.Instance.PluginUUID;
            if (PluginsPackagesInfosCRs.ContainsKey(uuid) == false)
            {
                PluginsPackagesInfosCRs[uuid] = new PluginPackageInfoCR(uuid);
            }
            if (EthlargementIntegratedPlugin.Instance.IsInstalled)
            {
                var localPluginInfo = new PluginPackageInfo
                {
                    PluginAuthor = EthlargementIntegratedPlugin.Instance.Author,
                    PluginName = EthlargementIntegratedPlugin.Instance.Name,
                    PluginUUID = uuid,
                    PluginVersion = EthlargementIntegratedPlugin.Instance.Version,
                    // other stuff is not inside the plugin
                };
                PluginsPackagesInfosCRs[uuid].LocalInfo = localPluginInfo;
            }
            var online = new PluginPackageInfo
            {
                PluginAuthor = EthlargementIntegratedPlugin.Instance.Author,
                PluginName = EthlargementIntegratedPlugin.Instance.Name,
                PluginUUID = uuid,
                PluginVersion = EthlargementIntegratedPlugin.Instance.Version,
                DecloudPackageURL = EthlargementIntegratedPlugin.Instance.GetDecloudBinsUrlsForPlugin().FirstOrDefault(),
                PackagePassword = null,
                PluginDescription = "ETHlargement increases DaggerHashimoto hashrate for NVIDIA 1080, 1080 Ti and Titan Xp GPUs.",
                PluginPackageURL = "N/A",
                SupportedDevicesAlgorithms = new Dictionary<string, List<string>> {
                        { DeviceType.NVIDIA.ToString(), new List<string> { AlgorithmType.DaggerHashimoto.ToString() } }
                    }
            };
            PluginsPackagesInfosCRs[uuid].OnlineInfo = online;
            if (online.SupportedDevicesAlgorithms != null)
            {
                var supportedDevices = online.SupportedDevicesAlgorithms
                    .Where(kvp => kvp.Value.Count > 0)
                    .Select(kvp => kvp.Key);
                var devRank = AvailableDevices.Devices
                    .Where(d => supportedDevices.Contains(d.DeviceType.ToString()))
                    .Count();
                PluginsPackagesInfosCRs[uuid].OnlineSupportedDeviceCount = devRank;
            }
        }

        public static void CrossReferenceInstalledWithOnline()
        {
            // EthlargementIntegratedPlugin special case
            CrossReferenceInstalledWithOnlineEthlargementIntegratedPlugin();

            // first go over the installed plugins
            var installedPlugins = PluginContainer.PluginContainers
                //.Where(p => p.Enabled) // we can have installed plugins that are obsolete
                .Where(p => !_integratedPlugins.Any(integrated => integrated.PluginUUID == p.PluginUUID)) // ignore integrated
                .ToArray();
            foreach (var installed in installedPlugins)
            {
                var uuid = installed.PluginUUID;
                var localPluginInfo = new PluginPackageInfo
                {
                    PluginAuthor = installed.Author,
                    PluginName = installed.Name,
                    PluginUUID = uuid,
                    PluginVersion = installed.Version,
                    // other stuff is not inside the plugin
                };
                if (PluginsPackagesInfosCRs.ContainsKey(uuid) == false)
                {
                    PluginsPackagesInfosCRs[uuid] = new PluginPackageInfoCR(uuid);
                }
                PluginsPackagesInfosCRs[uuid].LocalInfo = localPluginInfo;
            }

            // get online list and check what we have and what is online
            if (OnlinePlugins == null) return;

            foreach (var online in OnlinePlugins)
            {
                var uuid = online.PluginUUID;
                if (PluginsPackagesInfosCRs.ContainsKey(uuid) == false)
                {
                    PluginsPackagesInfosCRs[uuid] = new PluginPackageInfoCR(uuid);
                }
                PluginsPackagesInfosCRs[uuid].OnlineInfo = online;
                if (online.SupportedDevicesAlgorithms != null)
                {
                    var supportedDevices = online.SupportedDevicesAlgorithms
                        .Where(kvp => kvp.Value.Count > 0)
                        .Select(kvp => kvp.Key);
                    var devRank = AvailableDevices.Devices
                        .Where(d => supportedDevices.Contains(d.DeviceType.ToString()))
                        .Count();
                    PluginsPackagesInfosCRs[uuid].OnlineSupportedDeviceCount = devRank;
                }
            }

            DecloudPluginsManagerState.Instance.RankedPlugins = RankedPlugins.ToList();
        }


        private class NoKeepAlivesWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    ((HttpWebRequest)request).KeepAlive = false;
                }

                return request;
            }
        }


        private static async Task<bool> GetOnlineDecloudPlugins()
        {
            try
            {
                using (var client = new NoKeepAlivesWebClient())
                {
                    string s = await client.DownloadStringTaskAsync(Links.PluginsJsonApiUrl);
                    //// local fake string
                    //string s = Properties.Resources.pluginJSON;
                    var onlinePlugins = JsonConvert.DeserializeObject<List<PluginPackageInfo>>(s, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        Culture = CultureInfo.InvariantCulture
                    });
                    OnlinePlugins = onlinePlugins;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginsManager", $"Error occured while getting online Decloud plugins: {e.Message}");
            }
            return _initOnlinePlugins;
        }

        private static (bool, List<PluginPackageInfo>) ReadCachedOnlinePlugins()
        {
            try
            {
                var cachedPluginsInfoPath = Paths.RootPath("plugins_packages", "update.json");
                if (!File.Exists(cachedPluginsInfoPath)) return (false, null);
                string s = File.ReadAllText(cachedPluginsInfoPath);

                var onlinePlugins = JsonConvert.DeserializeObject<List<PluginPackageInfo>>(s, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Culture = CultureInfo.InvariantCulture
                });
                return (true, onlinePlugins);
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginsManager", $"Error occured while reading cached online Decloud plugins: {e.Message}");
            }
            return (false, null);
        }

        public static PluginContainer GetPluginWithUuid(string pluginUuid)
        {
            var ret = PluginContainer.PluginContainers.FirstOrDefault(p => p.PluginUUID == pluginUuid);
            return ret;
        }

        #region DownloadingInstalling

        public static async Task DownloadInternalBins(PluginContainer pluginContainer, List<string> urls, IProgress<int> downloadProgress, IProgress<int> unzipProgress, CancellationToken stop)
        {
            if (EthlargementIntegratedPlugin.Instance.PluginUUID == pluginContainer.PluginUUID)
            {
                await DownloadEthlargementInternalBins(urls, downloadProgress, stop);
                return;
            }
            var pluginUUID = pluginContainer.PluginUUID;
            var ver = pluginContainer.Version;
            var installingPluginBinsPath = Paths.DecloudPluginsPath(pluginUUID, "bins", $"{ver.Major}.{ver.Minor}");
            try
            {
                if (Directory.Exists(installingPluginBinsPath)) Directory.Delete(installingPluginBinsPath, true);
                Directory.CreateDirectory(installingPluginBinsPath);
                var installedBins = false;
                foreach (var url in urls)
                {
                    var downloadDecloudBinsResult = await DecloudDownloadManager.DownloadFileAsync(url, installingPluginBinsPath, "Decloud_bins", downloadProgress, stop);
                    var binsPackageDownloaded = downloadDecloudBinsResult.downloadedFilePath;
                    var downloadDecloudBinsOK = downloadDecloudBinsResult.success;
                    if (!downloadDecloudBinsOK || stop.IsCancellationRequested) return;
                    // unzip 
                    var binsUnzipPath = installingPluginBinsPath;
                    var unzipDecloudBinsOK = await ArchiveHelpers.ExtractFileAsync(pluginContainer.GetBinsPackagePassword(), binsPackageDownloaded, binsUnzipPath, unzipProgress, stop);
                    if (stop.IsCancellationRequested) return;
                    if (unzipDecloudBinsOK)
                    {
                        installedBins = true;
                        File.Delete(binsPackageDownloaded);
                        break;
                    }
                }
                if (!installedBins)
                {
                    Logger.Error("DecloudPluginsManager", $"Decloud bins of {pluginUUID} not installed");
                }

                //clear old bins
                clearOldPluginBins(Paths.DecloudPluginsPath(pluginUUID, "bins"));
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginsManager", $"Installation of {pluginUUID} failed: ${e.Message}");
            }
        }

        private static async Task<bool> DownloadEthlargementInternalBins(List<string> urls, IProgress<int> downloadProgress, CancellationToken stop)
        {
            var pluginUUID = EthlargementIntegratedPlugin.Instance.PluginUUID;
            var ver = EthlargementIntegratedPlugin.Instance.Version;
            var installingPluginBinsPath = Paths.DecloudPluginsPath(pluginUUID, "bins", $"{ver.Major}.{ver.Minor}");
            try
            {
                if (Directory.Exists(installingPluginBinsPath)) Directory.Delete(installingPluginBinsPath, true);
                Directory.CreateDirectory(installingPluginBinsPath);
                var installedBins = false;
                foreach (var url in urls)
                {
                    var downloadDecloudBinsResult = await DecloudDownloadManager.DownloadFileAsync(url, installingPluginBinsPath, EthlargementIntegratedPlugin.BinName.Replace(".exe", ""), downloadProgress, stop);
                    var binsPackageDownloaded = downloadDecloudBinsResult.downloadedFilePath;
                    var downloadDecloudBinsOK = downloadDecloudBinsResult.success;
                    installedBins = true;
                    if (!downloadDecloudBinsOK || stop.IsCancellationRequested) return false;
                    if (stop.IsCancellationRequested) return false;

                    break;
                }
                if (!installedBins)
                {
                    Logger.Error("DownloadEthlargementInternalBins", $"Decloud bins of {pluginUUID} not installed");
                }

                //clear old bins
                clearOldPluginBins(Paths.DecloudPluginsPath(pluginUUID, "bins"));
                CrossReferenceInstalledWithOnlineEthlargementIntegratedPlugin();
                return installedBins;
            }
            catch (Exception e)
            {
                Logger.Error("DownloadEthlargementInternalBins", $"Installation of {pluginUUID} failed: ${e.Message}");
                return false;
            }
        }

        private static ConcurrentDictionary<string, DecloudPluginInstallTask> DecloudPluginInstallTasks = new ConcurrentDictionary<string, DecloudPluginInstallTask>();
        // with WPF we have only one Progress 
        private static ConcurrentDictionary<string, IProgress<Tuple<PluginInstallProgressState, int>>> _DecloudPluginInstallTasksProgress = new ConcurrentDictionary<string, IProgress<Tuple<PluginInstallProgressState, int>>>();

        public static void InstallAddProgress(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            if (DecloudPluginInstallTasks.TryGetValue(pluginUUID, out var installTask))
            {
                installTask.AddProgress(progress);
            }
            // 
            if (_DecloudPluginInstallTasksProgress.ContainsKey(pluginUUID) == false)
            {
                _DecloudPluginInstallTasksProgress.TryAdd(pluginUUID, progress);
            }
        }

        public static void InstallRemoveProgress(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress)
        {
            if (DecloudPluginInstallTasks.TryGetValue(pluginUUID, out var installTask))
            {
                installTask.RemoveProgress(progress);
            }
            if (_DecloudPluginInstallTasksProgress.TryRemove(pluginUUID, out var _) == false)
            {
                // log error
            }
        }

        public static async Task DownloadAndInstall(string pluginUUID, IProgress<Tuple<PluginInstallProgressState, int>> progress, CancellationToken stop)
        {
            if (DecloudPluginInstallTasks.ContainsKey(pluginUUID)) return;

            var addSuccess = false;
            var installSuccess = false;
            using (var DecloudInstall = new DecloudPluginInstallTask())
            using (var tcs = CancellationTokenSource.CreateLinkedTokenSource(stop, DecloudInstall.CancelInstallToken))
            {
                try
                {
                    PluginInstaller.InstallPlugin(pluginUUID);
                    var pluginPackageInfo = PluginsPackagesInfosCRs[pluginUUID];
                    addSuccess = DecloudPluginInstallTasks.TryAdd(pluginUUID, DecloudInstall);
                    if (progress != null)
                    {
                        progress?.Report(Tuple.Create(PluginInstallProgressState.Pending, 0));
                        DecloudInstall.AddProgress(progress);
                    }
                    var installResult = PluginInstallProgressState.Canceled;
                    if (pluginPackageInfo.PluginUUID == EthlargementIntegratedPlugin.Instance.PluginUUID)
                    {
                        installResult = await DownloadAndInstallEthlargementIntegratedPlugin(pluginPackageInfo, DecloudInstall, tcs.Token);
                    }
                    else
                    {
                        installResult = await DownloadAndInstall(pluginPackageInfo, DecloudInstall, tcs.Token);
                    }
                    installSuccess = installResult == PluginInstallProgressState.Success;
                }
                finally
                {
                    PluginInstaller.InstalledPluginStatus(pluginUUID, installSuccess);
                    DecloudPluginInstallTasks.TryRemove(pluginUUID, out var _);
                    if (addSuccess) BlacklistedPlugins.RemoveFromBlacklist(pluginUUID);


                    AvailableNotifications.CreatePluginUpdateInfo(PluginsPackagesInfosCRs[pluginUUID].PluginName, installSuccess);
                }
            }
        }

        internal static async Task<PluginInstallProgressState> DownloadAndInstall(PluginPackageInfoCR plugin, IProgress<Tuple<PluginInstallProgressState, int>> progress, CancellationToken stop)
        {
            var downloadPluginProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingPlugin, perc)));
            var zipProgressPluginChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingPlugin, perc)));
            var downloadDecloudProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingDecloud, perc)));
            var zipProgressDecloudChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingDecloud, perc)));

            var finalState = PluginInstallProgressState.Pending;
            try
            {
                var versionStr = $"{plugin.OnlineInfo.PluginVersion.Major}.{plugin.OnlineInfo.PluginVersion.Minor}";
                var pluginRootPath = Paths.DecloudPluginsPath(plugin.PluginUUID);
                var installDllPath = Path.Combine(pluginRootPath, "dlls", versionStr);
                var installBinsPath = Path.Combine(pluginRootPath, "bins", versionStr);

                if (Directory.Exists(installDllPath)) Directory.Delete(installDllPath, true);
                Directory.CreateDirectory(installDllPath);
                if (Directory.Exists(installBinsPath)) Directory.Delete(installBinsPath, true);
                Directory.CreateDirectory(installBinsPath);

                //clear old bins
                clearOldPluginBins(Path.Combine(pluginRootPath, "bins"));

                // download plugin dll
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingDownloadingPlugin, 0));
                var downloadPluginResult = await DecloudDownloadManager.DownloadFileAsync(plugin.PluginPackageURL, installDllPath, "plugin", downloadPluginProgressChangedEventHandler, stop);
                var pluginPackageDownloaded = downloadPluginResult.downloadedFilePath;
                var downloadPluginOK = downloadPluginResult.success;
                if (!downloadPluginOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedDownloadingPlugin;
                    return finalState;
                }
                // unzip 
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingExtractingPlugin, 0));
                var unzipPluginOK = await ArchiveHelpers.ExtractFileAsync(null, pluginPackageDownloaded, installDllPath, zipProgressPluginChangedEventHandler, stop);
                if (!unzipPluginOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedExtractingPlugin;
                    return finalState;
                }
                File.Delete(pluginPackageDownloaded);

                // download plugin binary
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingDownloadingDecloud, 0));
                var downloadDecloudBinsResult = await DecloudDownloadManager.DownloadFileAsync(plugin.DecloudPackageURL, installBinsPath, "Decloud_bins", downloadDecloudProgressChangedEventHandler, stop);
                var binsPackageDownloaded = downloadDecloudBinsResult.downloadedFilePath;
                var downloadDecloudBinsOK = downloadDecloudBinsResult.success;
                if (!downloadDecloudBinsOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedDownloadingDecloud;
                    return finalState;
                }
                // unzip 
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingExtractingDecloud, 0));
                var unzipDecloudBinsOK = await ArchiveHelpers.ExtractFileAsync(plugin.BinsPackagePassword, binsPackageDownloaded, installBinsPath, zipProgressDecloudChangedEventHandler, stop);
                if (!unzipDecloudBinsOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedExtractingDecloud;
                    return finalState;
                }
                File.Delete(binsPackageDownloaded);

                // TODO from here on add the failed plugin load state and success state
                var loadedPlugins = DecloudPluginHost.LoadPlugin(installDllPath);
                if (loadedPlugins.Count() == 0)
                {
                    //downloadAndInstallUpdate($"Loaded ZERO PLUGINS");
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedPluginLoad;
                    Directory.Delete(installDllPath, true);
                    return finalState;
                }

                //downloadAndInstallUpdate("Checking old plugin");
                //downloadAndInstallUpdate($"Loaded {loadedPlugins} PLUGIN");
                // add or update plugins
                foreach (var pluginUUID in loadedPlugins)
                {
                    var newExternalPlugin = DecloudPluginHost.DecloudPlugin[pluginUUID];
                    // remove old
                    var oldPlugins = PluginContainer.PluginContainers.Where(p => p.PluginUUID == pluginUUID).ToArray();
                    foreach (var old in oldPlugins)
                    {
                        PluginContainer.RemovePluginContainer(old);
                    }
                    var newPlugin = PluginContainer.Create(newExternalPlugin);
                    // TODO/TESTING scope for our fake plugins
                    try
                    {
                        var newPluginDllSettings = Directory.GetFiles(installDllPath, "*.json");
                        foreach (var jsonFile in newPluginDllSettings)
                        {
                            var name = Path.GetFileName(jsonFile);
                            var installJSONFilePath = Path.Combine(pluginRootPath, name);
                            File.Copy(jsonFile, installJSONFilePath, true);
                        }
                    }
                    catch { }
                    var success = newPlugin.InitPluginContainer();
                    // TODO after add or remove plugins we should clean up the device settings
                    if (success)
                    {
                        var oldInstalledDlls = Directory.GetFiles(pluginRootPath, "*.dll");
                        foreach (var oldDll in oldInstalledDlls)
                        {
                            File.Delete(oldDll);
                        }
                        var newDllPath = Directory.GetFiles(installDllPath).FirstOrDefault();
                        var name = Path.GetFileNameWithoutExtension(newDllPath);
                        var newVerStr = $"{newPlugin.Version.Major}.{newPlugin.Version.Minor}";
                        var installedDllPath = Path.Combine(pluginRootPath, $"{name}.dll");
                        File.Copy(newDllPath, installedDllPath);

                        newPlugin.AddAlgorithmsToDevices();
                        await newPlugin.DevicesCrossReference(AvailableDevices.Devices.Select(d => d.BaseDevice));
                        finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.Success;
                    }
                    else
                    {
                        finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedPluginInit;
                        // TODO mark that this plugin wasn't loaded
                        Logger.Error("DecloudPluginsManager", $"DownloadAndInstall unable to init and install {pluginUUID}");
                    }
                }
                // cross reference local and online list
                CrossReferenceInstalledWithOnline();
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginsManager", $"Installation of {plugin.PluginName}_{plugin.PluginVersion}_{plugin.PluginUUID} failed: {e.Message}");
                //downloadAndInstallUpdate();
                finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedUnknown;
            }
            finally
            {
                progress?.Report(Tuple.Create(finalState, 0));
            }
            return finalState;
        }

        internal static async Task<PluginInstallProgressState> DownloadAndInstallEthlargementIntegratedPlugin(PluginPackageInfoCR plugin, IProgress<Tuple<PluginInstallProgressState, int>> progress, CancellationToken stop)
        {
            var downloadPluginProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingPlugin, perc)));
            var zipProgressPluginChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingPlugin, perc)));
            var downloadDecloudProgressChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.DownloadingDecloud, perc)));
            var zipProgressDecloudChangedEventHandler = new Progress<int>(perc => progress?.Report(Tuple.Create(PluginInstallProgressState.ExtractingDecloud, perc)));

            var finalState = PluginInstallProgressState.Pending;
            try
            {
                var versionStr = $"{plugin.OnlineInfo.PluginVersion.Major}.{plugin.OnlineInfo.PluginVersion.Minor}";
                var pluginRootPath = Paths.DecloudPluginsPath(plugin.PluginUUID);
                var installDllPath = Path.Combine(pluginRootPath, "dlls", versionStr);
                var installBinsPath = Path.Combine(pluginRootPath, "bins", versionStr);

                if (Directory.Exists(installDllPath)) Directory.Delete(installDllPath, true);
                Directory.CreateDirectory(installDllPath);
                if (Directory.Exists(installBinsPath)) Directory.Delete(installBinsPath, true);
                Directory.CreateDirectory(installBinsPath);

                //clear old bins
                clearOldPluginBins(Path.Combine(pluginRootPath, "bins"));

                // download plugin binary
                progress?.Report(Tuple.Create(PluginInstallProgressState.PendingDownloadingDecloud, 0));
                var urls = EthlargementIntegratedPlugin.Instance.GetDecloudBinsUrlsForPlugin().ToList();
                var downloadDecloudBinsOK = await DownloadEthlargementInternalBins(urls, downloadDecloudProgressChangedEventHandler, stop);
                if (!downloadDecloudBinsOK || stop.IsCancellationRequested)
                {
                    finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedDownloadingDecloud;
                    return finalState;
                }
                EthlargementIntegratedPlugin.Instance.InitAndCheckSupportedDevices(AvailableDevices.Devices.Select(dev => dev.BaseDevice));
                finalState = PluginInstallProgressState.Success;
                // cross reference local and online list
                CrossReferenceInstalledWithOnline();
            }
            catch (Exception e)
            {
                Logger.Error("DecloudPluginsManager", $"Installation of {plugin.PluginName}_{plugin.PluginVersion}_{plugin.PluginUUID} failed: {e.Message}");
                //downloadAndInstallUpdate();
                finalState = stop.IsCancellationRequested ? PluginInstallProgressState.Canceled : PluginInstallProgressState.FailedUnknown;
            }
            finally
            {
                progress?.Report(Tuple.Create(finalState, 0));
            }
            return finalState;
        }


        private static void clearOldPluginBins(string pluginBinsPath)
        {
            try
            {
                //keep only 3 versions
                var numOfKeepVersions = 3;
                var installedVersions = new DirectoryInfo(pluginBinsPath).GetDirectories("*", SearchOption.AllDirectories).ToList();
                if (installedVersions.Count() > numOfKeepVersions)
                {

                    //parse versions
                    var versionDic = new Dictionary<Version, string>();
                    foreach (var dir in installedVersions)
                    {
                        var dirName = dir.Name;
                        var version = Version.Parse(dirName);
                        versionDic.Add(version, dir.FullName);
                    }

                    //get old versions
                    var dirsToDelete = new List<string>();
                    int counter = 0;
                    foreach (var nek in versionDic.OrderByDescending(key => key.Value))
                    {
                        counter++;
                        if (counter > numOfKeepVersions)
                        {
                            dirsToDelete.Add(nek.Value);
                        }
                    }

                    foreach (var path in dirsToDelete)
                    {
                        Directory.Delete(path, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DecloudPluginsManager", $"Clearing of old plugin bins failed: {ex.Message}");
            }
        }
        #endregion DownloadingInstalling
    }
}
