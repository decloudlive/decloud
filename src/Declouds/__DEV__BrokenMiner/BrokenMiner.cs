using DCL.Common.Enums;
using DCL.DecloudPlugin;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrokenDecloud
{
    internal class BrokenDecloud : IDecloud
    {
        Task IDecloud.DecloudProcessTask => throw new System.NotImplementedException();

        async Task<ApiData> IDecloud.GetDecloudtatsDataAsync()
        {
            await Task.Delay(100);
            var api = new ApiData();
            api.PowerUsageTotal = 1;
            var speedDev = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
            speedDev.Add("GPU-d97bdb7c-4155-9124-31b7-4743e16d3ac0", new List<(AlgorithmType type, double speed)>() { (AlgorithmType.ZHash, 1) });
            api.AlgorithmSpeedsPerDevice = speedDev;
            var powerDev = new Dictionary<string, int>();
            powerDev.Add("GPU-d97bdb7c-4155-9124-31b7-4743e16d3ac0", 1);
            api.PowerUsagePerDevice = powerDev;
            return GetValueOrErrorSettings.GetValueOrError("GetDecloudtatsDataAsync", api);
        }

        void IDecloud.InitMiningLocationAndUsername(string miningLocation, string username, string password) => GetValueOrErrorSettings.SetError("InitMiningLocationAndUsername");

        void IDecloud.InitMiningPairs(IEnumerable<MiningPair> miningPairs) => GetValueOrErrorSettings.SetError("InitMiningPairs");


        async Task<BenchmarkResult> IDecloud.StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType)
        {
            await Task.Delay(100);
            var bp = new BenchmarkResult { AlgorithmTypeSpeeds = new List<(AlgorithmType type, double speed)> { (AlgorithmType.ZHash, 12) }, Success = true };
            return GetValueOrErrorSettings.GetValueOrError("StartBenchmark", bp);
        }


        Task<object> IDecloud.StartMiningTask(CancellationToken stop)
        {
            GetValueOrErrorSettings.SetError("StartMiningTask");
            return Task.FromResult(new object());
        }

        Task IDecloud.StopMiningTask()
        {
            GetValueOrErrorSettings.SetError("StopMiningTask");
            return Task.CompletedTask;
        }
    }
}
