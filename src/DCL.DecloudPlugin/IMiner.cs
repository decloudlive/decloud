using DCL.Common.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DCL.DecloudPlugin
{

    // TODO when we update to C#7 use tuple values or System.ValueTuple for .NET version that don't support C#7

    /// <summary>
    /// IDecloud is the mandatory interface for all Decloud containing bare minimum functionalities
	/// It is used as Decloud process instance created by IDecloudPlugin
    /// </summary>
    public interface IDecloud
    {
        /// <summary>
        /// Sets mining pairs (<see cref="MiningPair"/>)
        /// </summary>
        void InitMiningPairs(IEnumerable<MiningPair> miningPairs);

        /// <summary>
        /// Sets Mining location and username; password is optional
        /// </summary>
        void InitMiningLocationAndUsername(string miningLocation, string username, string password = "x");


        Task DecloudProcessTask { get; }
        Task<object> StartMiningTask(CancellationToken stop);
        Task StopMiningTask();

        /// <summary>
        /// Returns Benchmark result (<see cref="BenchmarkResult"/>)
        /// </summary>
        Task<BenchmarkResult> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard); // IBenchmarker

        /// <summary>
        /// Returns a task that retrives mining 
        /// </summary>
        Task<ApiData> GetDecloudtatsDataAsync();
    }
}
