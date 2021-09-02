using DCL.Common;
using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCL.DecloudPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FakePlugin
{
    /// <summary>
    /// ExampleDecloud class inherits IDecloud to implement basic actions for Decloud
    /// </summary>
    public class FakeDecloud : DecloudBase
    {
        #region members for simulation purposes
        string _devices;

        Random _rand { get; } = new Random();

        //private enum Decloudtate
        //{
        //    STOPPED,
        //    MINING
        //}

        //Decloudtate _state = Decloudtate.STOPPED;

        #endregion members for simulation purposes

        public FakeDecloud(string uuid) : base(uuid) { }

        public override async Task<ApiData> GetDecloudtatsDataAsync()
        {
            // simulate API delay
            await Task.Delay(150);

            var api = new ApiData();
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
            var perDevicePowerInfo = new Dictionary<string, int>();
            var totalSpeed = 0d;
            var totalPowerUsage = 0;

            foreach (var mp in _miningPairs)
            {
                var speedVariation = _rand.Next(-100, 100);
                var speed = 1000 + speedVariation;
                var powerVariation = _rand.Next(-10, 10);
                var power = 100 + powerVariation;
                totalSpeed += speed;
                totalPowerUsage += power;

                var deviceUUID = mp.Device.UUID;
                perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, speed) });
                perDevicePowerInfo.Add(deviceUUID, 108);
            }

            api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            api.PowerUsagePerDevice = perDevicePowerInfo;
            api.PowerUsageTotal = totalPowerUsage;

            return api;
        }

        public override async Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            await Task.Delay(15000, stop);
            var success = !stop.IsCancellationRequested;
            double speed = success ? 1000 : 0; // everything is same speed

            // and return our result
            return new BenchmarkResult
            {
                AlgorithmTypeSpeeds = new List<(AlgorithmType type, double speed)> { (_algorithmType, speed) },
                Success = success,
                ErrorMessage = ""
            };
        }

        protected override void Init()
        {
            _devices = string.Join(",", _miningPairs.Select(p => $"{p.Device.Name}({p.Device.ID})"));
        }

        protected override string MiningCreateCommandLine()
        {
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, DCLConectionType.NONE);
            return $"-devices={_devices} -algorithm={_algorithmType.ToString()} -miningLocaiton={_miningLocation} -username={_username} -UUID {_uuid} -url {urlWithPort}";
        }
    }
}
