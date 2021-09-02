using DCL.Common;
using DCL.Common.Enums;
using DCL.DecloudPlugin;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.CCDecloudCommon;
using System.Linq;
using System.Threading.Tasks;

namespace ZEnemy
{
    public class ZEnemy : DecloudBase
    {
        private string _devices;

        private int _apiPort;

        public ZEnemy(string uuid) : base(uuid)
        { }

        protected virtual string AlgorithmName(AlgorithmType algorithmType) => PluginSupportedAlgorithms.AlgorithmName(algorithmType);

        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);

        public override Task<ApiData> GetDecloudtatsDataAsync()
        {
            return CCDecloudAPIHelpers.GetDecloudtatsDataAsync(_apiPort, _algorithmType, _miningPairs, _logGroup, DevFee);
        }

        protected override void Init()
        {
            _devices = string.Join(",", _miningPairs.Select(p => p.Device.ID));
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var urlWithPort = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, DCLConectionType.STRATUM_TCP);

            var algo = AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url={urlWithPort} --user {_username} --api-bind={_apiPort} --devices {_devices} {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
