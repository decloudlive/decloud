using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCLCore.Mining.MiningStats;
using DCLCore.Switching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace DCLCore.Mining
{
    // TODO this should ba a namespace with separate files
    // MiningStats are the new APIData rates groups...
    // keep this here for now
    public static class MiningDataStats
    {
        private static object _lock = new object();

        // key is joined device DecloudUUID-UUIDs, sorted uuids
        private static Dictionary<string, ApiData> _apiDataGroups = new Dictionary<string, ApiData>();

        // key is joined device DecloudUUID-UUIDs, sorted uuids
        private static Dictionary<string, DecloudMiningStats> _DecloudMiningStats = new Dictionary<string, DecloudMiningStats>();

        // key is device UUID 
        private static Dictionary<string, DeviceMiningStats> _devicesMiningStats = new Dictionary<string, DeviceMiningStats>();

        private static readonly ObservableCollection<DeviceMiningStats> _devMiningStats = new ObservableCollection<DeviceMiningStats>();

        public static INotifyCollectionChanged DevicesMiningStats
        {
            get
            {
                lock (_devMiningStats)
                {
                    return _devMiningStats;
                }
            }
        }

        public static void RemoveGroup(IEnumerable<string> deviceUUIDs, string DecloudUUID)
        {
            var sortedDeviceUUIDs = deviceUUIDs.OrderBy(uuid => uuid).ToList();
            var uuidsKeys = string.Join(",", sortedDeviceUUIDs);
            var removeKey = $"{DecloudUUID}-{uuidsKeys}";
            // remove groups
            lock (_lock)
            {
                _apiDataGroups.Remove(removeKey);
                _DecloudMiningStats.Remove(removeKey);
                foreach (var devUUID in deviceUUIDs)
                {
                    _devicesMiningStats.Remove(devUUID);
                }
            }
            lock (_devMiningStats)
            {
                var toRemove = _devMiningStats.Where(devStat => deviceUUIDs.Contains(devStat.DeviceUUID)).ToArray();
                foreach (var remove in toRemove)
                {
                    _devMiningStats.Remove(remove);
                }
            }
        }

        public static void UpdateGroup(ApiData apiData, string DecloudUUID, string DecloudName)
        {

            if (apiData == null) return;
            if (apiData.AlgorithmSpeedsPerDevice == null || apiData.AlgorithmSpeedsPerDevice.Count == 0) return;
            if (apiData.PowerUsagePerDevice == null || apiData.PowerUsagePerDevice.Count == 0) return;

            var sortedDeviceUUIDs = apiData.AlgorithmSpeedsPerDevice
                .Select(speedInfo => speedInfo.Key)
                .OrderBy(uuid => uuid)
                .ToArray();
            var uuidsKeys = string.Join(",", sortedDeviceUUIDs);
            var groupKey = $"{DecloudUUID}-{uuidsKeys}";

            // update groups
            lock (_lock)
            {
                // check what keys to remove
                var removeKeys = _apiDataGroups.Keys
                    .Where(checkKey => checkKey.StartsWith(DecloudUUID) == false) // DecloudUUIDDiffers
                    .Where(checkKey => sortedDeviceUUIDs.Any(uuid => checkKey.Contains(uuid))) // deviceInKey
                    .ToArray();
                foreach (var removeKey in removeKeys)
                {
                    _apiDataGroups.Remove(removeKey);
                    _DecloudMiningStats.Remove(removeKey);
                }
                // add / update data
                _apiDataGroups[groupKey] = apiData;

                var payingRates = NHSmaData.CurrentPayingRatesSnapshot();
                // update stats
                UpdateDecloudMiningStats(apiData, DecloudUUID, DecloudName, groupKey, payingRates);
                // update device stats
                foreach (var deviceUuid in sortedDeviceUUIDs)
                {
                    UpdateDeviceMiningStats(apiData, DecloudUUID, DecloudName, groupKey, deviceUuid, payingRates);
                }

                // TODO notify change
            }
        }

        private static void UpdateDecloudMiningStats(ApiData apiData, string DecloudUUID, string DecloudName, string groupKey, Dictionary<AlgorithmType, double> payingRates)
        {
            DecloudMiningStats stat;
            if (_DecloudMiningStats.TryGetValue(groupKey, out stat) == false)
            {
                // create if it doesn't exist
                stat = new DecloudMiningStats { GroupKey = groupKey, DecloudName = DecloudName };
                _DecloudMiningStats[groupKey] = stat;
            }

            var deviceUUIDs = apiData.AlgorithmSpeedsPerDevice
                .Select(speedInfo => speedInfo.Key)
                .ToArray();

            stat.Clear();
            // add device UUIDs
            foreach (var deviceUuid in deviceUUIDs) stat.DeviceUUIDs.Add(deviceUuid);

            // update stat
            stat.PowerUsageAPI = (double)apiData.PowerUsageTotal / 1000d;
            foreach (var speedInfo in apiData.AlgorithmSpeedsTotal())
            {
                stat.Speeds.Add(speedInfo);
                if (payingRates.TryGetValue(speedInfo.type, out var paying) == false) continue;
                var payingRate = paying * speedInfo.speed * 0.000000001;
                stat.Rates.Add((speedInfo.type, payingRate));
            }
            var relevantDevices = AvailableDevices.Devices.Where(dev => deviceUUIDs.Contains(dev.Uuid)).ToArray();
            double powerUsageFromDevice = relevantDevices.Select(dev => dev.PowerUsage).Sum();
            double powerUsageFromAlgorithmSettings = relevantDevices
                .Select(dev => (dev, speeds: apiData.AlgorithmSpeedsPerDevice[dev.Uuid]))
                .Select(p => (p.dev, algoIds: p.speeds.Select(info => info.type).ToArray()))
                .Select(p => p.dev.GetAlgorithm(DecloudUUID, p.algoIds))
                .Where(algo => algo != null)
                .Select(algo => algo.PowerUsage)
                .Sum();
            stat.PowerUsageDeviceReading = powerUsageFromDevice;
            stat.PowerUsageAlgorithmSetting = powerUsageFromAlgorithmSettings;
        }

        private static void UpdateDeviceMiningStats(ApiData apiData, string DecloudUUID, string DecloudName, string groupKey, string deviceUuid, Dictionary<AlgorithmType, double> payingRates)
        {
            DeviceMiningStats stat;
            if (_devicesMiningStats.TryGetValue(deviceUuid, out stat) == false)
            {
                // create if it doesn't exist
                stat = new DeviceMiningStats { DeviceUUID = deviceUuid };
                _devicesMiningStats[deviceUuid] = stat;
            }
            stat.Clear();
            // re-set Decloud name and group key
            stat.DecloudName = DecloudName;
            stat.GroupKey = groupKey;

            // update stat
            var deviceSpeedsInfo = apiData.AlgorithmSpeedsPerDevice
                .Where(pair => pair.Key == deviceUuid)
                .Select(pair => pair.Value)
                .FirstOrDefault();

            if (deviceSpeedsInfo != null)
            {
                foreach (var speedInfo in deviceSpeedsInfo)
                {
                    stat.Speeds.Add((speedInfo.type, speedInfo.speed));
                    if (payingRates.TryGetValue(speedInfo.type, out var paying) == false) continue;
                    var payingRate = paying * speedInfo.speed * 0.000000001;
                    stat.Rates.Add((speedInfo.type, payingRate));
                }
            }

            var devicePowerUsageApi = (double)apiData.PowerUsagePerDevice
                .Where(pair => pair.Key == deviceUuid)
                .Select(pair => pair.Value)
                .FirstOrDefault();

            stat.PowerUsageAPI = devicePowerUsageApi / 1000d;

            // TODO Globals
            var dev = AvailableDevices.GetDeviceWithUuid(deviceUuid);
            if (dev != null)
            {
                stat.PowerUsageDeviceReading = dev.PowerUsage;
                var algo = dev.GetAlgorithm(DecloudUUID, apiData.AlgorithmSpeedsPerDevice[dev.Uuid].Select(info => info.type).ToArray());
                stat.PowerUsageAlgorithmSetting = algo == null ? 0d : algo.PowerUsage;
            }

            lock (_devMiningStats)
            {
                var index = _devMiningStats.IndexOf(stat);
                if (index < 0) _devMiningStats.Add(stat);
                else _devMiningStats[index] = stat;
            }

            //// TODO enable StabilityAnalyzer
            //// TODO not the best place but here is where we get our per device speeds
            //var algorithmName = string.Join("+", stat.Speeds.Select(speedPair => Enum.GetName(typeof(AlgorithmType), speedPair.type)));
            //var algorithmStringID = $"{algorithmName}_{DecloudUUID}";
            //var speedID = $"{deviceUuid}-{algorithmStringID}";
            //var primarySpeed = stat.Speeds.Count > 0 ? stat.Speeds[0].speed : 0d;
            //var secondarySpeed = stat.Speeds.Count > 1 ? stat.Speeds[1].speed : 0d;
            //var miningSpeed = new BenchmarkingAnalyzer.MiningSpeed
            //{
            //    PrimarySpeed = primarySpeed,
            //    SecondarySpeed = secondarySpeed,
            //    Timestamp = DateTime.Now
            //};
            //BenchmarkingAnalyzer.UpdateMiningSpeeds(speedID, miningSpeed);
        }

        public static void ClearApiDataGroups()
        {
            lock (_lock)
            {
                _apiDataGroups.Clear();
                _DecloudMiningStats.Clear();
                _devicesMiningStats.Clear();
                // TODO notify change
            }

            lock (_devMiningStats)
            {
                _devMiningStats.Clear();
            }
        }

        public static List<(AlgorithmType type, double speed)> GetSpeedForDevice(string deviceUuid)
        {
            var ret = new List<(AlgorithmType type, double speed)>();
            lock (_lock)
            {
                if (_devicesMiningStats.TryGetValue(deviceUuid, out var stat))
                {
                    foreach (var speedInfo in stat.Speeds)
                    {
                        ret.Add(speedInfo);
                    }
                }
            }
            return ret;
        }

        // GetProfit() deduct power cost
        public static double GetProfit(double kwhPriceInBtc)
        {
            double totalProfit = 0;
            lock (_lock)
            {
                foreach (var DecloudtatPair in _DecloudMiningStats)
                {
                    var Decloudtat = DecloudtatPair.Value;
                    // add to total
                    totalProfit += Decloudtat.TotalPayingRateDeductPowerCost(kwhPriceInBtc);
                }
            }
            return totalProfit;
        }

        // For Production
        public static List<DecloudMiningStats> GetDecloudMiningStats()
        {
            var ret = new List<DecloudMiningStats>();
            lock (_lock)
            {
                foreach (var DecloudtatPair in _DecloudMiningStats)
                {
                    var key = DecloudtatPair.Key;
                    var Decloudtat = DecloudtatPair.Value;

                    ret.Add(Decloudtat.DeepCopy());
                }
            }
            return ret;
        }

        public static List<DeviceMiningStats> GetDevicesMiningStats()
        {
            var ret = new List<DeviceMiningStats>();
            lock (_lock)
            {
                foreach (var deviceStatPair in _devicesMiningStats)
                {
                    var key = deviceStatPair.Key;
                    var deviceStat = deviceStatPair.Value;

                    ret.Add(deviceStat.DeepCopy());
                }
            }
            return ret;
        }
    }
}
