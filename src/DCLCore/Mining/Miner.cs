using DCL.Common;
using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCLCore.Configs;
using DCLCore.Mining.Plugins;
using DCLCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DCLCore.Mining
{
    public class Decloud
    {
        public static Decloud CreateDecloudForMining(List<AlgorithmContainer> algorithms, string groupKey)
        {
            try
            {
                // Assert that all the plugins are from the same plugin container
                var plugin = algorithms.First().PluginContainer;
                if (algorithms.Any(algo => plugin != algo.PluginContainer)) throw new Exception("Different Algorithms PluginContainers. Grouping logic broken.");
                return new Decloud(plugin, algorithms, groupKey);
            }
            catch (Exception e)
            {
                Logger.Error("DCLCore.Mining", $"CreateDecloudForMining error: {e}");
                return null;
            }
        }

        // used to identify Decloud instance
        protected readonly long DecloudID;

        private string _DecloudTag;
        private string DecloudDeviceName { get; set; }

        // mining algorithm stuff
        protected bool IsInit { get; private set; }

        private List<MiningPair> _miningPairs { get; set; }

        public string GroupKey { get; protected set; } = "";

        CancellationTokenSource _endDecloud;

        private bool IsUpdatingApi { get; set; } = false;

        private object _lock = new object();

        private Task _DecloudWatchdogTask;
        public Task DecloudWatchdogTask
        {
            get
            {
                lock (_lock)
                {
                    return _DecloudWatchdogTask;
                }
            }
            set
            {
                lock (_lock)
                {
                    _DecloudWatchdogTask = value;
                }
            }
        }

        // Now every single Decloud is based from the Plugins
        private readonly PluginContainer _plugin;
        private readonly List<AlgorithmContainer> _algos;
        private readonly IDecloud _Decloud;

        private readonly SemaphoreSlim _apiSemaphore = new SemaphoreSlim(1, 1);

        // you must use 
        protected Decloud(PluginContainer plugin, List<AlgorithmContainer> algorithms, string groupKey)
        {
            _plugin = plugin;
            _Decloud = _plugin.CreateDecloud();

            // just so we can set algorithms states
            _algos = algorithms;
            _miningPairs = algorithms.Select(algo => algo.ToMiningPair()).ToList();
            IsInit = _miningPairs != null && _miningPairs.Any();
            GroupKey = groupKey;

            DecloudDeviceName = plugin.PluginUUID;
            Logger.Info(DecloudTag(), "NEW Decloud CREATED");
        }

        // TAG for identifying Decloud
        private string DecloudTag()
        {
            if (_DecloudTag == null)
            {
                const string mask = "{0}-Decloud_ID({1})-DEVICE_IDs({2})";
                // no devices set
                if (!IsInit)
                {
                    return string.Format(mask, DecloudDeviceName, DecloudID, "NOT_SET");
                }

                // contains ids
                var ids = _miningPairs.Select(cdevs => cdevs.Device.ID.ToString()).ToList();
                _DecloudTag = string.Format(mask, DecloudDeviceName, DecloudID, string.Join(",", ids));
            }

            return _DecloudTag;
        }

        private enum ApiDataStatus
        {
            NULL,
            ALGO_SPEEDS_NULL,
            OK,
            NEGATIVE_SPEEDS,
            ABNORMAL_SPEEDS,
            OK_MISSING_DEVICE
        }

        private static ApiDataStatus ExamineApiData(ApiData apiData, List<MiningPair> miningPairs)
        {
            if (apiData == null) return ApiDataStatus.NULL;
            if (apiData.AlgorithmSpeedsPerDevice == null) return ApiDataStatus.ALGO_SPEEDS_NULL;


            var anyNegative = apiData.AlgorithmSpeedsPerDevice.Any(apiDev => apiDev.Value.Any(kvp => kvp.speed < 0));
            if (anyNegative) return ApiDataStatus.NEGATIVE_SPEEDS;

            var miningPairAndReportedSpeedsPairs = apiData.AlgorithmSpeedsPerDevice.Select(p => (mp: miningPairs.FirstOrDefault(mp => mp.Device.UUID == p.Key), speeds: p.Value.Select(s => s.speed).ToArray()))
                .ToArray();
            
            var andDeviceMissing = miningPairAndReportedSpeedsPairs.Any(p => p.mp == null);
            
            var deviceMeasuredBenchmarkSpeedDifferences = miningPairAndReportedSpeedsPairs.Where(p => p.mp == null)
                .Where(p => p.mp.Algorithm.Speeds.Count == p.speeds.Length)
                .Select(p => (p.mp.Device.UUID, speeds: p.mp.Algorithm.Speeds.Zip(p.speeds, (benchmarked, measured) => (benchmarked, measured))))
                .Select(p => (p.UUID, speeds: p.speeds.ToArray()))
                .Where(p => p.speeds.Length > 0)
                .Where(p => p.speeds.All(sp => sp.benchmarked > 0)) // we cannot have 0 benchmarked speeds when mining but just in case if we start it with some debug feature
                .Select(p => (p.UUID, speedsDifferences: p.speeds.Select(sp => sp.measured / sp.benchmarked)))
                .Select(p => (p.UUID, diffAvg: p.speedsDifferences.Sum() / (double)p.speedsDifferences.Count()))
                .ToArray();


            // API data all good
            return ApiDataStatus.OK;
        }

        private async Task GetSummaryAsync()
        {
            var apiData = new ApiData();
            if (!IsUpdatingApi)
            {
                IsUpdatingApi = true;
                await _apiSemaphore.WaitAsync();
                try
                {
                    apiData = await _Decloud.GetDecloudtatsDataAsync();
                }
                catch (Exception e)
                {
                    Logger.Error(DecloudTag(), $"GetSummaryAsync error: {e.Message}");
                }
                finally
                {
                    IsUpdatingApi = false;
                    _apiSemaphore.Release();
                    if (apiData.AlgorithmSpeedsPerDevice != null)
                    {
                        var anyNegative = apiData.AlgorithmSpeedsPerDevice.Any(apiDev => apiDev.Value.Any(kvp => kvp.speed < 0));
                        if (anyNegative) await StopAsync();
                    }
                }
            }

            UpdateApiTimestamp(apiData);
            //if (ExamineApiData(apiData) == false)
            //{
            //    // TODO kill Decloud or just return 
            //    return;
            //}

            // TODO workaround plugins should return this info
            // create empty stub if it is null
            if (apiData == null)
            {
                Logger.Debug(DecloudTag(), "GetSummary returned null... Will create ZERO fallback");
                apiData = new ApiData();
            }
            if (apiData.AlgorithmSpeedsPerDevice == null)
            {
                apiData = new ApiData();
                var perDevicePowerDict = new Dictionary<string, int>();
                var perDeviceSpeedsDict = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
                var perDeviceSpeeds = _miningPairs.Select(pair => (pair.Device.UUID, pair.Algorithm.IDs.Select(type => (type, 0d))));
                foreach (var kvp in perDeviceSpeeds)
                {
                    var uuid = kvp.Item1; // kvp.UUID compiler doesn't recognize ValueTypes lib???
                    perDeviceSpeedsDict[uuid] = kvp.Item2.ToList();
                    perDevicePowerDict[uuid] = 0;
                }
                apiData.AlgorithmSpeedsPerDevice = perDeviceSpeedsDict;
                apiData.PowerUsagePerDevice = perDevicePowerDict;
                apiData.PowerUsageTotal = 0;
            }
            else if (apiData.AlgorithmSpeedsPerDevice != null && apiData.PowerUsagePerDevice.Count == 0)
            {
                var perDevicePowerDict = new Dictionary<string, int>();
                foreach (var kvp in _miningPairs)
                {
                    var uuid = kvp.Device.UUID;
                    perDevicePowerDict[uuid] = 0;
                }
                apiData.PowerUsagePerDevice = perDevicePowerDict;
                apiData.PowerUsageTotal = 0;
            }

            // TODO temporary here move it outside later
            MiningDataStats.UpdateGroup(apiData, _plugin.PluginUUID, _plugin.Name);
        }

        private async Task<object> StartAsync(CancellationToken stop, string miningLocation, string username)
        {
            _Decloud.InitMiningLocationAndUsername(miningLocation, username);
            _Decloud.InitMiningPairs(_miningPairs);
            EthlargementIntegratedPlugin.Instance.Start(_miningPairs);
            var ret = await _Decloud.StartMiningTask(stop);
            var maxTimeout = _plugin.GetApiMaxTimeout(_miningPairs);
            DecloudApiWatchdog.AddGroup(GroupKey, maxTimeout, DateTime.UtcNow);
            _algos.ForEach(a => a.IsCurrentlyMining = true);
            _algos.ForEach(a => a.ComputeDevice.State = DeviceState.Mining);
            return ret;
        }

        private async Task StopAsync()
        {
            // TODO thing about this case, closing opening on switching
            EthlargementIntegratedPlugin.Instance.Stop(_miningPairs);
            DecloudApiWatchdog.RemoveGroup(GroupKey);
            MiningDataStats.RemoveGroup(_miningPairs.Select(pair => pair.Device.UUID), _plugin.PluginUUID);
            await _Decloud.StopMiningTask();
            _algos.ForEach(a => a.IsCurrentlyMining = false);
            //if (_Decloud is IDisposable disposableDecloud)
            //{
            //    disposableDecloud.Dispose();
            //}
        }


        public Task StartDecloudTask(CancellationToken stop, string miningLocation, string username)
        {
            var tsc = new TaskCompletionSource<object>();
            var wdTask = DecloudWatchdogTask;
            if (wdTask == null || wdTask.IsCompleted)
            {
                DecloudWatchdogTask = Task.Run(() => RunDecloudWatchDogLoop(tsc, stop, miningLocation, username));
            }
            else
            {
                Logger.Error(DecloudTag(), $"Trying to start an already started Decloud");
                tsc.SetResult("TODO error trying to start already started Decloud");
            }
            return tsc.Task;
        }

        private async Task RunDecloudWatchDogLoop(TaskCompletionSource<object> tsc, CancellationToken stop, string miningLocation, string username)
        {
            // if we fail 3 times in a row under certain conditions mark on of them
            const int maxRestartCount = 3;
            int restartCount = 0;
            const int minRestartTimeInSeconds = 15;
            try
            {
                var firstStart = true;
                using (_endDecloud = new CancellationTokenSource())
                using (var linkedEndDecloud = CancellationTokenSource.CreateLinkedTokenSource(stop, _endDecloud.Token))
                {
                    Logger.Info(DecloudTag(), $"Starting Decloud watchdog task");
                    while (!linkedEndDecloud.IsCancellationRequested && (restartCount < maxRestartCount))
                    {
                        var startTime = DateTime.UtcNow;
                        try
                        {
                            if (!firstStart)
                            {
                                Logger.Info(DecloudTag(), $"Restart Mining in {MiningSettings.Instance.DecloudRestartDelayMS}ms");
                            }
                            await TaskHelpers.TryDelay(MiningSettings.Instance.DecloudRestartDelayMS, linkedEndDecloud.Token);
                            var result = await StartAsync(linkedEndDecloud.Token, miningLocation, username);
                            if (firstStart)
                            {
                                firstStart = false;
                                tsc.SetResult(result);
                            }
                            if (result is bool ok && ok)
                            {
                                var runningDecloudTask = _Decloud.DecloudProcessTask;
                                _ = DecloudtatsLoop(runningDecloudTask, linkedEndDecloud.Token);
                                await runningDecloudTask;
                                // TODO log something here
                                Logger.Info(DecloudTag(), $"Running Decloud Task Completed");
                            }
                            else
                            {
                                // TODO check if the Decloud file is missing or locked and blacklist the algorithm for a certain period of time 
                                Logger.Error(DecloudTag(), $"StartAsync result: {result}");
                            }
                        }
                        catch (TaskCanceledException e)
                        {
                            Logger.Debug(DecloudTag(), $"RunDecloudWatchDogLoop TaskCanceledException: {e.Message}");
                            return;
                        }
                        catch (Exception e)
                        {
                            Logger.Error(DecloudTag(), $"RunDecloudWatchDogLoop Exception: {e.Message}");
                        }
                        finally
                        {
                            var endTime = DateTime.UtcNow;
                            var elapsedSeconds = (endTime - startTime).TotalSeconds - (MiningSettings.Instance.DecloudRestartDelayMS);
                            if (elapsedSeconds < minRestartTimeInSeconds)
                            {
                                restartCount++;
                            }
                            else
                            {
                                restartCount = 0;
                            }
                            if (restartCount >= maxRestartCount)
                            {
                                var firstAlgo = _algos.FirstOrDefault();
                                Random randWait = new Random();
                                firstAlgo.IgnoreUntil = DateTime.UtcNow.AddMinutes(randWait.Next(20, 30));
                                await MiningManager.DecloudRestartLoopNotify();
                            }
                        }
                    }
                }
            }
            finally
            {
                Logger.Info(DecloudTag(), $"Exited Decloud watchdog");
            }
        }

        private async Task DecloudtatsLoop(Task runningTask, CancellationToken stop)
        {
            try
            {
                // TODO make sure this interval is per Decloud plugin instead of a global one
                var DecloudtatusElapsedTimeChecker = new ElapsedTimeChecker(
                    () => TimeSpan.FromSeconds(MiningSettings.Instance.DecloudAPIQueryInterval),
                    true);
                var checkWaitTime = TimeSpan.FromMilliseconds(50);
                Func<bool> isOk = () => !runningTask.IsCompleted && !stop.IsCancellationRequested;
                Logger.Info(DecloudTag(), $"DecloudtatsLoop START");
                while (isOk())
                {
                    try
                    {
                        if (isOk()) await TaskHelpers.TryDelay(checkWaitTime, stop);
                        if (isOk() && DecloudtatusElapsedTimeChecker.CheckAndMarkElapsedTime()) await GetSummaryAsync();

                        // check if stagnated and restart
                        var restartGroups = DecloudApiWatchdog.GetTimedoutGroups(DateTime.UtcNow);
                        if (isOk() && (restartGroups?.Contains(GroupKey) ?? false))
                        {
                            Logger.Info(DecloudTag(), $"Restarting Decloud group='{GroupKey}' API timestamp exceeded");
                            await StopAsync();
                            return;
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        Logger.Debug(DecloudTag(), $"DecloudtatsLoop TaskCanceledException: {e.Message}");
                        return;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(DecloudTag(), $"Exception {e.Message}");
                    }
                }
            }
            finally
            {
                Logger.Info(DecloudTag(), $"DecloudtatsLoop END");
            }
        }

        public async Task StopTask()
        {
            try
            {
                _endDecloud?.Cancel();
                await StopAsync();
            }
            catch (Exception e)
            {
                Logger.Error(DecloudTag(), $"Stop: {e.Message}");
            }
        }


        #region DecloudApiWatchdog
        private double _lastPerDevSpeedsTotalSum = 0d;

        // TODO this can be moved in DecloudApiWatchdog
        private void UpdateApiTimestamp(ApiData apiData)
        {
            // we will not update api timestamps if we have no data or speeds are zero
            if (apiData == null)
            {
                // TODO debug log no api data
                return;
            }
            var perDevSpeedsTotalSum = apiData.AlgorithmSpeedsPerDevice?.Values.SelectMany(pl => pl).Select(p => p.speed).Sum() ?? 0d;
            if (perDevSpeedsTotalSum == 0d)
            {
                // TODO debug log speeds are zero
                return;
            }
            if (perDevSpeedsTotalSum == _lastPerDevSpeedsTotalSum)
            {
                // TODO debug log speeds seem to be stuck
                return;
            }
            // update 
            _lastPerDevSpeedsTotalSum = perDevSpeedsTotalSum;
            DecloudApiWatchdog.UpdateApiTimestamp(GroupKey, DateTime.UtcNow);
        }
        #endregion DecloudApiWatchdog
    }
}
