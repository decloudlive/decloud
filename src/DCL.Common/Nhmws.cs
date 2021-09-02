
using DCL.Common.Enums;

namespace DCL.Common
{
    public static class DCLws
    {
        // SMA Socket

        internal static string BuildTagDCLSocketAddress()
        {
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNET) return "wss://DCLws-test.Decloud.com/v3/DCLl";
            if (BuildOptions.BUILD_TAG == BuildTag.TESTNETDEV) return "wss://DCLws-test-dev.Decloud.com/v3/DCLl";
            //BuildTag.PRODUCTION
            return "wss://DCLws.Decloud.com/v3/DCLl";
        }

        public static string DCLSocketAddress
        {
            get
            {
                if (BuildOptions.CUSTOM_ENDPOINTS_ENABLED) return StratumServiceHelpers.DCLSocketAddress;
                return BuildTagDCLSocketAddress();
            }
        }
    }
}
